using DevLynx.Packaging.Visualizer.Extensions;
using DevLynx.Packaging.Visualizer.Models;
using DevLynx.Packaging.Visualizer.Models.Contexts;
using DevLynx.Packaging.Visualizer.Services;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using Wpf.Ui.Controls;
using UIDialog = Wpf.Ui.Controls.Dialog;
using UISnackbar = Wpf.Ui.Controls.Snackbar;

namespace DevLynx.Packaging.Visualizer.ViewModels
{
    internal class ShellViewModel : BindableBase
    {
        public ICommand ModelLoadedCommand { get; }
        public ICommand MouseDownCommand { get; }
        public ICommand ResetCommand { get; }

        public ICommand DialogLoadedCommand { get; }
        public ICommand SnackbarLoadedCommand { get; }

        public DialogContext Dialog => _appService.Context.Dialog;
        public PackageContext PContext => _packagingService.Context;

        private Point _lastPos;
        private Model3DGroup _model;
        private Model3DGroup _scene;
        private PerspectiveCamera _cam;
        private Brush _texture;

        private readonly IAppService _appService;
        private readonly IMessageService _messageService;
        private readonly IPackagingService _packagingService;

        public ShellViewModel(IAppService appService, IMessageService messageService, IPackagingService packagingService)
        {
            ModelLoadedCommand = new DelegateCommand<object>(HandleModelLoaded);
            MouseDownCommand = new DelegateCommand<object>(HandleMouseDown);
            ResetCommand = new DelegateCommand(HandleReset);
            DialogLoadedCommand = new DelegateCommand<object>(HandleDialogLoaded);
            SnackbarLoadedCommand = new DelegateCommand<object>(HandleSnackbarLoaded);

            _texture = (Brush)Application.Current.FindResource("CartonTextureBrush");

            _appService = appService;
            _messageService = messageService;
            _packagingService = packagingService;
        }

        private void HandleDialogLoaded(object obj)
        {
            if (obj is not RoutedEventArgs e) return;
            if (e.Source is not UIDialog dialog) return;            

            if (_appService is AppService ap)
                ap.RegisterDialog(dialog);
        }

        private void HandleSnackbarLoaded(object obj)
        {
            if (obj is not RoutedEventArgs e) return;
            if (e.Source is not UISnackbar snackbar) return;

            if (_messageService is UIMessageService ums)
                ums.RegisterSnackbar(snackbar);
        }

        private void HandleModelLoaded(object obj)
        {
            //if (obj is not RoutedEventArgs e) return;
            //if (e.Source is not Viewport3D viewport) return;
            //if (viewport.Children.First() is not ModelVisual3D visual) return;

            //_cam = (PerspectiveCamera)viewport.Camera;


            //viewport.MouseWheel += HandleMouseWheel;


            //visual.Content = _model = new Model3DGroup();
            //PrepareLighting(_model);


            //var scene = _scene = new Model3DGroup();
            //scene.Transform = new Transform3DGroup();

            //BuildModel(_scene);
            //_model.Children.Add(_scene);
        }

        private void HandleMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Source is not Viewport3D viewport) return;

            _cam.Position = new Point3D(_cam.Position.X, _cam.Position.Y, _cam.Position.Z - e.Delta / 250d);
        }

        private void HandleMouseDown(object obj)
        {
            if (obj is not MouseButtonEventArgs e) return;
            if (e.Source is not Viewport3D el) return;

            Point pos = e.GetPosition(el);
            _lastPos = new Point(pos.X - el.ActualWidth / 2, (el.ActualHeight / 2) - pos.Y);

            el.MouseUp += HandleDragComplete;
            el.MouseMove += HandleDrag;
            el.CaptureMouse();

            Console.WriteLine("Drag Started {0}", el);
        }

        private void HandleDrag(object sender, MouseEventArgs e)
        {
            if (sender is not Viewport3D viewport) return;

            Point pos = Mouse.GetPosition(viewport);
            Point actualPos = new Point(pos.X - viewport.ActualWidth / 2, viewport.ActualHeight / 2 - pos.Y);
            double dx = actualPos.X - _lastPos.X, dy = actualPos.Y - _lastPos.Y;

            double mouseAngle = 0;
            if (dx != 0 && dy != 0)
            {
                mouseAngle = Math.Asin(Math.Abs(dy) / Math.Sqrt(Math.Pow(dx, 2) + Math.Pow(dy, 2)));
                if (dx < 0 && dy > 0) mouseAngle += Math.PI / 2;
                else if (dx < 0 && dy < 0) mouseAngle += Math.PI;
                else if (dx > 0 && dy < 0) mouseAngle += Math.PI * 1.5;
            }
            else if (dx == 0 && dy != 0) mouseAngle = Math.Sign(dy) > 0 ? Math.PI / 2 : Math.PI * 1.5;
            else if (dx != 0 && dy == 0) mouseAngle = Math.Sign(dx) > 0 ? 0 : Math.PI;

            double axisAngle = mouseAngle + Math.PI / 2;

            Vector3D axis = new Vector3D(Math.Cos(axisAngle) * 4, Math.Sin(axisAngle) * 4, 0);

            double rotation = 0.01 * Math.Sqrt(Math.Pow(dx, 2) + Math.Pow(dy, 2));

            Transform3DGroup group = _scene.Transform as Transform3DGroup;
            QuaternionRotation3D r = new QuaternionRotation3D(new System.Windows.Media.Media3D.Quaternion(axis, rotation * 180 / Math.PI));
            group.Children.Add(new RotateTransform3D(r));

            _lastPos = actualPos;
        }

        private void HandleDragComplete(object sender, MouseButtonEventArgs e)
        {
            if (sender is not FrameworkElement el) return;

            el.MouseMove -= HandleDrag;
            el.MouseUp -= HandleDragComplete;
            el.ReleaseMouseCapture();
            

            Console.WriteLine("Drag Complete {0}", el);
        }

        private void HandleReset()
        {
            _cam.Position = new Point3D(_cam.Position.X, _cam.Position.Y, 5);
        }

        private void PrepareLighting(Model3DGroup scene)
        {
            double sceneSize = 10;
            Vector3D direction = new Point3D() - new Point3D(-sceneSize, sceneSize / 2, 0.0);
            Vector3D direction2 = new Point3D() - new Point3D(0, 0, sceneSize);

            DirectionalLight light = new DirectionalLight();
            light.Color = Colors.Gray;
            light.Direction = direction;

            DirectionalLight light2 = new DirectionalLight();
            light.Color = Colors.Gray;
            light.Direction = direction2;

            AmbientLight ambientLight = new AmbientLight();
            ambientLight.Color = Colors.Gray;

            scene.Children.Add(light);
            //scene.Children.Add(light2);
            scene.Children.Add(ambientLight);
        }

        void BuildModel(Model3DGroup root)
        {
            Model3DGroup group = new Model3DGroup();
            root.Children.Add(group);

            // new Point3D(-3, -2, -.5)
            SplitCuboid c = new SplitCuboid(6, 4, 1, .03);
            Cuboid cuboid = new Cuboid(5, 7, 2);
            
            Material material = new DiffuseMaterial(Brushes.White);
            DiffuseMaterial bMaterial = new DiffuseMaterial(_texture);
            bMaterial.Color = Color.FromRgb(0xee, 0xbf, 0x88);
            bMaterial.AmbientColor = Color.FromRgb(0xc9, 0xa2, 0x5a);

            root.ModelSplitCuboid(c, material, bMaterial);
            //root.ModelCuboid(cuboid, bMaterial);

            //MeshRect3D rect1 = new MeshRect3D(new Point3D(-.5, -.5, -.5), 1, 1.1, 0);
            //MeshRect3D rect2 = new MeshRect3D(new Point3D(-.5, -.5, -.5), 0, 1.1, 1);
            //MeshRect3D rect3 = new MeshRect3D(new Point3D(-.5, .5, -.5), 1, 0, 1);
            
            //MeshRect3D rect4 = new MeshRect3D(new Point3D(-.5, .5, .5), 1, .1, 0);
            //MeshRect3D rect5 = new MeshRect3D(new Point3D(.5, .5, -.5), 0, .1, 1);
            //MeshRect3D rect6 = new MeshRect3D(new Point3D(-.5, .6, -.5), 1, 0, 1);

            //MeshRect3D rect7 = new MeshRect3D(new Point3D(.5, -.5, -.6), 0, 1.1, .1);
            //MeshRect3D rect8 = new MeshRect3D(new Point3D(-.5, -.5, -.6), 1, 0, .1);
            //MeshRect3D rect9 = new MeshRect3D(new Point3D(-.5, -.5, -.6), 0, 1.1, .1);
            //MeshRect3D rect10 = new MeshRect3D(new Point3D(-.5, .6, -.6), 1, 0, .1);

            //MeshRect3D rect11 = new MeshRect3D(new Point3D(-.5, -.5, -.6), 1, 1.1, 0);

            //MeshRect3D rect12 = new MeshRect3D(new Point3D(-.6, -.5, -.6), .1, 0, 1.1);
            //MeshRect3D rect13 = new MeshRect3D(new Point3D(-.6, -.5, -.6), .1, 1.1, 0);
            //MeshRect3D rect14 = new MeshRect3D(new Point3D(-.6, .6, -.6), .1, 0, 1.1);
            //MeshRect3D rect15 = new MeshRect3D(new Point3D(-.6, -.5, .5), .1, 1.1, 0);
            //MeshRect3D rect16 = new MeshRect3D(new Point3D(-.6, -.5, -.6), 0, 1.1, 1.1);

            ////MeshRect3D rect8 = new MeshRect3D(new Point3D(-.5, .6, -.5), 1, 0, 1);
            ////MeshRect3D rect9 = new MeshRect3D(new Point3D(-.5, .6, -.5), 1, 0, 1);

            ////MeshRect3D rect5 = new MeshRect3D(new Point3D(0, -.6, 0), 1, 0, 1);
            ////MeshRect3D rect6 = new MeshRect3D(new Point3D(0, -.55, -.5), 1, .1, 0); ;

            ////MeshRect3D rect7 = new MeshRect3D(new Point3D(0, -.55, .5), 1, .1, 0);

            //// c9a25a
            
            //// eebf88
            //bMaterial.Color = Color.FromRgb(0xee, 0xbf, 0x88);
            //bMaterial.AmbientColor = Color.FromRgb(0xc9, 0xa2, 0x5a);

            //group.ModelRect(rect1, material, bMaterial);
            //group.ModelRect(rect2, bMaterial, material);
            //group.ModelRect(rect3, material, bMaterial);
            
            //group.ModelRect(rect4, bMaterial, material);
            //group.ModelRect(rect5, material, bMaterial);
            //group.ModelRect(rect6, material, bMaterial);

            //group.ModelRect(rect7, material, bMaterial);
            //group.ModelRect(rect8, bMaterial, material);
            //group.ModelRect(rect9, bMaterial, material);
            //group.ModelRect(rect10, material, bMaterial);
            //group.ModelRect(rect11, material, bMaterial);

            //group.ModelRect(rect12, bMaterial, material);
            //group.ModelRect(rect13, material, bMaterial);
            //group.ModelRect(rect14, material, bMaterial);
            //group.ModelRect(rect15, bMaterial, material);
            //group.ModelRect(rect16, bMaterial, material);

            //group.ModelRect(rect3, bMaterial, material);

            //group.ModelRect(rect4, bMaterial, bMaterial);
            //group.ModelRect(rect5, bMaterial, bMaterial);

            //group.ModelRect(rect6, bMaterial, bMaterial);
        }
    }
}
