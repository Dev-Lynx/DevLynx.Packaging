using DevLynx.Packaging.Visualizer.Extensions;
using DevLynx.Packaging.Visualizer.Models;
using DevLynx.Packaging.Visualizer.Models.Contexts;
using DevLynx.Packaging.Visualizer.Services;
using ImTools;
using Prism.Commands;
using Prism.Mvvm;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Media3D;
using System.Windows.Threading;

namespace DevLynx.Packaging.Visualizer.ViewModels
{
    internal class SimViewModel : BindableBase
    {
        public ICommand LoadedCommand { get; }

        public ICommand PrevCommand { get; }
        public ICommand NextCommand { get; }


        public bool IsBusy { get; set; }
        public PackageContext Context => _packagingService.Context;

        public int CurrentIndex { get; private set; }

        [AlsoNotifyFor(nameof(CurrentIndex))]
        public bool CanPrev => CurrentIndex > 1;
        [AlsoNotifyFor(nameof(CurrentIndex))]
        public bool CanNext => CurrentIndex < Context.Iterations.Count;

        [AlsoNotifyFor(nameof(CurrentIndex))]
        public PackIteration Current => Context.Iterations.ElementAtOrDefault(CurrentIndex - 1);

        private readonly IPackagingService _packagingService;
        private readonly IAppService _appService;

        private Viewport3D _viewport => _appService.Context.Viewport;
        private Model3DGroup _packedScene => _appService.Context.PackedScene;
        private Duration _duration;
        private DoubleAnimation _animation;

        private bool _simulating;
        private int _nIteration;
        private int _nTranslate;

        public SimViewModel(IPackagingService packagingService, IAppService appService)
        {
            _packagingService = packagingService;
            _appService = appService;

            LoadedCommand = new DelegateCommand(InitSim);
            PrevCommand = new DelegateCommand(HandlePrev);
            NextCommand = new DelegateCommand(HandleNext);
        }

        private void InitSim()
        {
            _duration = new Duration(TimeSpan.FromMilliseconds(200));
            _animation = new DoubleAnimation(1, _duration, FillBehavior.HoldEnd)
            {
                EasingFunction = new CubicEase()
                {
                    EasingMode = EasingMode.EaseIn
                }
            };
            _animation.Completed += HandleAnimationComplete;

            IsBusy = true;
            Task.Run(() => _packagingService.Start())
                .ContinueWith((task) =>
                {
                    var res = Application.Current.Dispatcher.BeginInvoke(() =>
                    {
                        IsBusy = false;
                        CurrentIndex = Context.Iterations.Count;
                        RaisePropertyChanged(nameof(Current));

                        StartSimInternal();
                    });
                });
        }

        private void StartSimInternal()
        {
            try
            {
                PackIteration itr = Current;

                _simulating = true;
                _packedScene.Children.Clear();
                _nIteration = _nTranslate = 0;


                LoadIteration();
                //DispatcherTimer timer = new DispatcherTimer(DispatcherPriority.Render, App.Current.Dispatcher);
                //timer.Interval = TimeSpan.FromSeconds(.5);

                int i = 0;

                //EventHandler handler = null;
                //timer.Tick += handler = (s, e) =>
                //{
                //    if (i >= itr.Instances.Count)
                //    {
                //        timer.Stop();
                //        timer.Tick -= handler;
                //        return;
                //    }

                //    var inst = itr.Instances[i++];
                //    _packedScene.Children.Add(inst.Model);
                //};

                //timer.Start();

                //for (int i = 0; i < itr.Instances.Count; i++)
                //{
                //    var inst = itr.Instances[i];
                //    _packedScene.Children.Add(inst.Model);


                //}

                ContinueAnimation();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private void ContinueAnimation()
        {
            if (_nIteration >= Current.Instances.Count)
            {
                _simulating = false;
                _nIteration = _nTranslate = 0;
                return;
            }

            var pack = Current.Instances[_nIteration++];

            Model3D model = pack.Model;
            _packedScene.Children.Add(model);



            //TranslateTransform3D transform = (TranslateTransform3D)model.Transform;
            ScaleTransform3D transform = (ScaleTransform3D)model.Transform;


            //DoubleAnimation animation = new DoubleAnimation(0, _duration, FillBehavior.HoldEnd)
            //{
            //    EasingFunction = new CubicEase()
            //    {
            //        EasingMode = EasingMode.EaseInOut
            //    }
            //};


            



            transform.BeginAnimation(ScaleTransform3D.ScaleXProperty, _animation);
            transform.BeginAnimation(ScaleTransform3D.ScaleYProperty, _animation);
            transform.BeginAnimation(ScaleTransform3D.ScaleZProperty, _animation);
        }

        private void HandleAnimationComplete(object sender, EventArgs e)
        {
            _nTranslate++;
            if (!_simulating) return;

            if (_nTranslate % 3 == 0)
                ContinueAnimation();
        }

        private void HandlePrev()
        {
            int index = CurrentIndex - 1;

            if (index >= 1)
                CurrentIndex = index;
        }

        private void HandleNext()
        {
            int index = CurrentIndex - 1;

            if (index <= Context.Iterations.Count - 1)
                CurrentIndex = index;
        }

        private void LoadIteration()
        {
            

            PackIteration itr = Current;
            var inst = itr.Instances;

            
            Material material;
            Model3DGroup model;


            //Point3D worldStart = new Point3D(0, (_viewport.ActualHeight / 2), 0);
            Point3D worldStart = new Point3D(0, Context.Container.Height, 0);

            double chx = Context.Container.Width / 2;
            double chy = Context.Container.Height / 2;
            double chz = Context.Container.Depth / 2;

            double x, y, z;



            for (int i = 0; i < inst.Count; i++)
            {
                var pack = inst[i];
                pack.Model = model = new Model3DGroup();
                //model.Transform = new TranslateTransform3D(worldStart.X, worldStart.Y + pack.Dim.Y, worldStart.Z);
                

                x = pack.Co.X - chx;
                y = pack.Co.Y - chy;
                z = pack.Co.Z - chz;

                x += .5;
                y += .5;
                z += .5;

                model.Transform = new ScaleTransform3D(0, 0, 0, x + (pack.Dim.X / 2), y + (pack.Dim.Y / 2), z + (pack.Dim.Z / 2));


                Point3D start = new Point3D(x, y, z);


                material = new DiffuseMaterial(new SolidColorBrush(UIHelper.ParseHexColor(pack.Color)));
                model.ModelCuboid(new Cuboid(start, pack.Dim.X, pack.Dim.Y, pack.Dim.Z), material);
            }
        }
    }
}
