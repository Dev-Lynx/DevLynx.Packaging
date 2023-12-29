using DevLynx.Packaging.Visualizer.Extensions;
using DevLynx.Packaging.Visualizer.Models;
using DevLynx.Packaging.Visualizer.Models.Contexts;
using DevLynx.Packaging.Visualizer.Services;
using DevLynx.Packaging.Visualizer.UI;
using DevLynx.Packaging.Visualizer.Views;
using DryIoc;
using ImTools;
using NLog;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.DirectoryServices.ActiveDirectory;
using System.IO.Packaging;
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
        public ICommand BackCommand { get; }

        public ICommand PrevCommand { get; }
        public ICommand NextCommand { get; }

        public ICommand GoToBestCommand { get; }


        public bool IsBusy { get; set; }
        public PackageContext Context => _packagingService.Context;

        private int _currentIndex;
        public int CurrentIndex { get => _currentIndex; }

        [AlsoNotifyFor(nameof(CurrentIndex))]
        public bool CanPrev => CurrentIndex > 1;
        [AlsoNotifyFor(nameof(CurrentIndex))]
        public bool CanNext => CurrentIndex < Context.Iterations.Count;

        [AlsoNotifyFor(nameof(CurrentIndex))]
        public PackIteration Current => Context.Iterations.ElementAtOrDefault(CurrentIndex - 1);

        public Dim Rotation { get; } = new Dim(0, 0, 0);

        private readonly IPackagingService _packagingService;
        private readonly IAppService _appService;
        private readonly IRegionManager _regionManager;
        private readonly ILogger _logger;

        private Viewport3D _viewport => _appService.Context.Viewport;
        private Model3DGroup _packedScene => _appService.Context.PackedScene;
        private Duration _duration;

        private bool _awaitingRender;
        private SolidAnimation _anim;
        //private DoubleAnimation _animation;
        private readonly Debouncer _debouncer;
        private readonly Debouncer _rotationDebouncer;


        private bool _simulating;
        private int _nIteration;
        private int _nTranslate;

        public SimViewModel(IPackagingService packagingService, IAppService appService, IRegionManager regionManager, ILogger logger)
        {
            _packagingService = packagingService;
            _appService = appService;
            _regionManager = regionManager;
            _logger = logger;
            _debouncer = new Debouncer(TimeSpan.FromMilliseconds(500), DebounceTimerType.DispatcherTimer);

            LoadedCommand = new DelegateCommand(InitSim);
            PrevCommand = new DelegateCommand(HandlePrev);
            NextCommand = new DelegateCommand(HandleNext);
            BackCommand = new DelegateCommand(HandleGoBack);
            GoToBestCommand = new DelegateCommand(() =>
            {
                SetIndex(Context.BestIteration + 1);
            });


            _duration = new Duration(TimeSpan.FromMilliseconds(200));

            //_animation = new DoubleAnimation(1, _duration, FillBehavior.HoldEnd)
            //{
            //    EasingFunction = new CubicEase()
            //    {
            //        EasingMode = EasingMode.EaseIn
            //    }
            //};

            _anim = new SolidAnimation(new DoubleKeyFrameCollection
            {
                new EasingDoubleKeyFrame(0, KeyTime.FromPercent(0)),
                new EasingDoubleKeyFrame(1.1, KeyTime.FromPercent(.6)),
                new EasingDoubleKeyFrame(1, KeyTime.FromPercent(.8))
            }, _duration);

            //DoubleAnimationUsingKeyFrames xAnim = new DoubleAnimationUsingKeyFrames()
            //{
            //    Duration = _duration,
            //    BeginTime = TimeSpan.Zero,
            //    FillBehavior = FillBehavior.HoldEnd,
            //    KeyFrames =
            //    {
            //        new EasingDoubleKeyFrame(0, KeyTime.FromPercent(0)),
            //        new EasingDoubleKeyFrame(1.1, KeyTime.FromPercent(.6)),
            //        new EasingDoubleKeyFrame(1, KeyTime.FromPercent(.8))
            //    }
            //};

            //DoubleAnimationUsingKeyFrames yAnim = new DoubleAnimationUsingKeyFrames()
            //{
            //    Duration = _duration,
            //    BeginTime = TimeSpan.Zero,
            //    FillBehavior = FillBehavior.HoldEnd,
            //    KeyFrames =
            //    {
            //        new EasingDoubleKeyFrame(0, KeyTime.FromPercent(0)),
            //        new EasingDoubleKeyFrame(1.1, KeyTime.FromPercent(.6)),
            //        new EasingDoubleKeyFrame(1, KeyTime.FromPercent(.8))
            //    }
            //};

            //DoubleAnimationUsingKeyFrames zAnim = new DoubleAnimationUsingKeyFrames()
            //{
            //    Duration = _duration,
            //    BeginTime = TimeSpan.Zero,
            //    FillBehavior = FillBehavior.HoldEnd,
            //    KeyFrames =
            //    {
            //        new EasingDoubleKeyFrame(0, KeyTime.FromPercent(0)),
            //        new EasingDoubleKeyFrame(1.1, KeyTime.FromPercent(.6)),
            //        new EasingDoubleKeyFrame(1, KeyTime.FromPercent(.8))
            //    }
            //};

            //_storyboard = new Storyboard()
            //{
            //    Duration = _duration,
            //    FillBehavior = FillBehavior.HoldEnd,
            //    Children = new TimelineCollection
            //    {
            //        xAnim, yAnim, zAnim
            //    }
            //};

            //xAnim.Completed += HandleAnimationComplete;
            //yAnim.Completed += HandleAnimationComplete;
            //zAnim.Completed += HandleAnimationComplete;


            
            //Storyboard.SetTargetProperty(xAnim, new PropertyPath(nameof(ScaleTransform3D.ScaleX)));
            //Storyboard.SetTargetProperty(yAnim, new PropertyPath(nameof(ScaleTransform3D.ScaleY)));
            //Storyboard.SetTargetProperty(zAnim, new PropertyPath(nameof(ScaleTransform3D.ScaleZ)));


            //_storyboard.Completed += HandleAnimationComplete;

            _rotationDebouncer = new Debouncer(TimeSpan.FromMilliseconds(500), DebounceTimerType.DispatcherTimer);
            Rotation.Width.PropertyChanged += HandleRotationChanged;
            Rotation.Height.PropertyChanged += HandleRotationChanged;
            Rotation.Depth.PropertyChanged += HandleRotationChanged;

            Context.PropertyChanged += ContextChanged;
        }

        private void ContextChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(PackageContext.Rendering))
            {
                if (!Context.Rendering && _awaitingRender)
                {
                    _awaitingRender = false;
                    StartAnimation();
                }
            }
        }

        private void HandleRotationChanged(object sender, PropertyChangedEventArgs e)
        {
            _rotationDebouncer.Debounce(UpdateRotation);
        }

        private void UpdateRotation()
        {
            if (_packedScene.Transform is not Transform3DGroup transform) return;
            if (transform.Children[0] is not Transform3DGroup rTransform) return;
            if (rTransform.Children[0] is not RotateTransform3D rot) return;
            if (rot.Rotation is not QuaternionRotation3D qrot) return;

            Quaternion q = QuaternionExtensions.Euler(Rotation.Width, Rotation.Height, Rotation.Depth);
            q.Normalize();

            //rot.BeginAnimation(QuaternionRotation3D.QuaternionProperty, _animation);
            qrot.Quaternion = q;
        }

        private void InitSim()
        {
            IsBusy = true;
            Task.Run(() => _packagingService.Start())
                .ContinueWith((task) =>
                {
                    var res = Application.Current.Dispatcher.BeginInvoke(() =>
                    {
                        IsBusy = false;
                        SetIndex(Context.Iterations.Count);
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
                StartAnimation();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private void StartAnimation()
        {
            SolidAnimationEcho echo = null;

            if (Context.Rendering)
            {
                _awaitingRender = true;
                return;
            }
                



            for (int i = 0; i < Current.Instances.Count; i++)
            {
                PackInstance inst = Current.Instances[i];

                if (inst.Model is not Model3D model) continue;
                if (model.Transform is not Transform3DGroup group) continue;
                if (group.Children.ElementAtOrDefault(0) is not ScaleTransform3D transform) continue;

                if (echo == null)
                {
                    echo = transform.BeginAnimation(_anim, SolidAnimationKind.Scale);
                }
                else
                {
                    echo = echo.ThenBegin(transform, SolidAnimationKind.Scale);
                }
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

            Application.Current.Dispatcher.BeginInvoke(() =>
            {
                try
                {
                    var pack = Current.Instances[_nIteration++];

                    Model3D model = pack.Model;
                    //_packedScene.Children.Add(model);
                    if (model == null) return;
                    if (model.Transform is not Transform3DGroup transform) return;
                    if (transform.Children.ElementAtOrDefault(0) is not ScaleTransform3D stransform) return;




                    //TranslateTransform3D transform = (TranslateTransform3D)model.Transform;
                    //ScaleTransform3D transform = (ScaleTransform3D)model.Transform;


                    //DoubleAnimation animation = new DoubleAnimation(0, _duration, FillBehavior.HoldEnd)
                    //{
                    //    EasingFunction = new CubicEase()
                    //    {
                    //        EasingMode = EasingMode.EaseInOut
                    //    }
                    //};
                    Console.WriteLine("Begining Animation");

                    //for (int i = 0; i < _storyboard.Children.Count; i++)
                    //{
                    //    var anim = _storyboard.Children[i];

                    //    if (anim is not AnimationTimeline timeline)
                    //        continue;

                    //    DependencyProperty prop;

                    //    switch (i)
                    //    {
                    //        default:
                    //            prop = ScaleTransform3D.ScaleXProperty;
                    //            break;
                    //        case 1:
                    //            prop = ScaleTransform3D.ScaleYProperty;
                    //            break;
                    //        case 2:
                    //            prop = ScaleTransform3D.ScaleZProperty;
                    //            break;
                    //    }


                        
                        

                    //    stransform.BeginAnimation(prop, timeline);
                    //}


                    //_storyboard.Begin();
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                }
            });

            
        }

        private void HandleAnimationComplete(object sender, EventArgs e)
        {
            //Console.WriteLine(sender);
            //if (sender is not AnimationTimeline anim) return;
            //anim.Completed -= HandleAnimationComplete;

            //if (sender is not AnimationClock clock) return;
            //Console.WriteLine(clock.CurrentTime);

            _nTranslate++;
            if (!_simulating) return;

            var pack = Current.Instances.ElementAtOrDefault(_nIteration);
            if (pack != null)
            {
                Model3D model = pack.Model;
                //_packedScene.Children.Add(model);
                if (model == null) return;
                if (model.Transform is not Transform3DGroup transform) return;
                if (transform.Children.ElementAtOrDefault(0) is not ScaleTransform3D stransform) return;

                Console.WriteLine("({0:N2}, {1:N2}, {2:N2})", stransform.ScaleX, stransform.ScaleY, stransform.ScaleZ);
            }





            if (_nTranslate % 3 == 0)
                ContinueAnimation();

        }

        private void HandlePrev()
        {
            int index = CurrentIndex;

            if (index > 1)
                SetIndex(index - 1);
        }

        private void HandleNext()
        {
            int index = CurrentIndex;

            if (index < Context.Iterations.Count)
                SetIndex(index + 1);
        }

        private void HandleGoBack()
        {
            _regionManager.NavigateToView<StartView>(AppBase.MAIN_REGION);
        }

        private void SetIndex(int index)
        {
            _currentIndex = index;
            RaisePropertyChanged(nameof(CurrentIndex));
            RaisePropertyChanged(nameof(CanNext));
            RaisePropertyChanged(nameof(CanPrev));
            RaisePropertyChanged(nameof(Current));

            _debouncer.Debounce(StartSimInternal);
        }

        private void LoadIteration()
        {
            PackIteration itr = Current;
            var inst = itr.Instances;

            Context.SimContainer = itr.Container;
            //return;



            Material material;
            Model3DGroup model;


            //Point3D worldStart = new Point3D(0, (_viewport.ActualHeight / 2), 0);
            Point3D worldStart = new Point3D(0, Context.Container.Height, 0);

            

            double x, y, z;
            double th = Context.ContainerThickness;

            

            Transform3DGroup rot = new Transform3DGroup();
            //RotateTransform3D rt = new RotateTransform3D(new QuaternionRotation3D(itr.Quaternion));
            TranslateTransform3D tt = new TranslateTransform3D();
            //rot.Children.Add(rt);
            //cv = rt.Transform(cv);

            //RotateTransform3D rt = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 0, 1), -90));
            //RotateTransform3D rt2 = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(-1, 0, 0), 90));
            //Transform3DGroup rot = new Transform3DGroup();


            //rot.Children.Add(rt);
            //rot.Children.Add(rt2);



            //Vector3D cv = new Vector3D(Context.Container.Width, Context.Container.Height, Context.Container.Depth);
            Vector3D cv = new Vector3D(itr.Container.X, itr.Container.Y, itr.Container.Z);
            //cv = rot.Transform(cv);



            double chx = Math.Abs(cv.X / 2);
            double chy = Math.Abs(cv.Y / 2);
            double chz = Math.Abs(cv.Z / 2);

            Console.WriteLine("B: <{0}, {1}, {2}>\tA: {3}", chx, chy, chz, cv);
            //Console.WriteLine("QQ: {0}", itr.Quaternion);

            Transform3DGroup transform = new Transform3DGroup();
            //transform.Children.Add(rot);
            transform.Children.Add(tt);

            _packedScene.Transform = transform;

            double maxX = 0, maxY = 0, maxZ = 0;

            for (int i = 0; i < inst.Count; i++)
            {
                var pack = inst[i];
                pack.Model = model = new Model3DGroup();
                //model.Transform = new TranslateTransform3D(worldStart.X, worldStart.Y + pack.Dim.Y, worldStart.Z);

                

                x = pack.Co.X /*- chx*/;
                y = pack.Co.Y /*- chy*/;
                z = pack.Co.Z /*- chz*/;

                //if (itr.Orientation == PackOrientation.Z_90) x = x;/*x -= chz;*/
                //else

                //x -= chx;
                //y -= chy;
                //z -= chz;

                //if (itr.Orientation != PackOrientation.Z_90) x += th;
                //else x -= th;

                //x += th;
                //y += th;
                //z += th;

                Point3D center = new Point3D(x + (pack.Dim.X / 2), y + (pack.Dim.Y / 2), z + (pack.Dim.Z / 2));

                if (pack.Dim.X + pack.Co.X > maxX)
                    maxX = pack.Dim.X + pack.Co.X;
 
                if (pack.Dim.Y + pack.Co.Y > maxY)
                    maxY = pack.Dim.Y + pack.Co.Y;

                if (pack.Dim.Z + pack.Co.Z > maxZ)
                    maxZ = pack.Dim.Z + pack.Co.Z;


                //model.Transform = ;
                //Transform3DGroup transform = new Transform3DGroup();
                
                //model.Transform = transform;


                Point3D start = new Point3D(x, y, z);

                //RotateTransform3D rt = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(1, 0, 0), 90));
                
                //start = rt.Transform(start);


                material = new DiffuseMaterial(new SolidColorBrush(UIHelper.ParseHexColor(pack.Color)));
                model.ModelCuboid(new Cuboid(start, pack.Dim.X, pack.Dim.Y, pack.Dim.Z), material);

                Transform3DGroup mTransform = new Transform3DGroup();
                model.Transform = mTransform;
                mTransform.Children.Add(new ScaleTransform3D(0, 0, 0, center.X, center.Y, center.Z));




                //transform.Children.Add(rot);
                //transform.Children.Add(rt2);
                //transform.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 1, 0), 0), center));
                //transform.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 0, 1), 0), center));

                _packedScene.Children.Add(model);
            }

            //rt.CenterX = maxX / 2;
            //rt.CenterY = maxY / 2;
            //rt.CenterZ = maxZ / 2;

            tt.OffsetX = -chx +th;
            tt.OffsetY = -chy + th;
            tt.OffsetZ = -chz + th;

            //tt.OffsetX -= rt.CenterX;
            //tt.OffsetY -= rt.CenterY;
            //tt.OffsetZ -= rt.CenterZ;

            //Console.WriteLine("Center: ({0:N2}, {1:N2}, {2:N2})", rt.CenterX, rt.CenterY, rt.CenterZ);
            Console.WriteLine("Offset: ({0:N2}, {1:N2}, {2:N2})", tt.OffsetX, tt.OffsetY, tt.OffsetZ);
            Console.WriteLine("CN: ({0:N2}, {1:N2}, {2:N2})", cv.X, cv.Y, cv.Z);
        }
    }
}
