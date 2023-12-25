using DevLynx.Packaging.Visualizer.Extensions;
using DevLynx.Packaging.Visualizer.Models;
using DevLynx.Packaging.Visualizer.Services;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.Intrinsics;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using Wpf.Ui.Controls;
using Quaternion = System.Windows.Media.Media3D.Quaternion;

namespace DevLynx.Packaging.Visualizer.ViewModels
{
    internal class SpaceViewModel : BindableBase, INavigationAware
    {
        public ICommand LoadedCommand { get; }
        public Transform3D ModelTransform { get; private set; }

        private readonly IPackagingService _packagingService;
        private readonly IAppService _appService;

        private Point _lastPos;
        private Brush _texture;

        private Viewport3D _viewport;
        private double _viewportMax;
        
        private Model3DGroup _model;
        private Model3DGroup _scene;

        public SpaceViewModel(IPackagingService packagingService, IAppService appService)
        {
            _texture = (Brush)Application.Current.FindResource("CartonTextureBrush");
            _packagingService = packagingService;
            _appService = appService;

            LoadedCommand = new DelegateCommand<object>(HandleLoaded);
        }

        private void HandleLoaded(object obj)
        {
            if (obj is not RoutedEventArgs e) return;
            if (e.Source is not Viewport3D viewport) return;

            var appContext = _appService.Context;

            _viewport = viewport;
            appContext.Viewport = viewport;
            _viewport.MouseDown += HandleMouseDown;
            
            if (_viewport.Parent is FrameworkElement p)
                p.MouseWheel += HandleMouseWheel;
            else _viewport.MouseWheel += HandleMouseWheel;

            ModelVisual3D visual = new ModelVisual3D();
            _viewport.Children.Add(visual);

            visual.Content = _scene = new Model3DGroup();
            
            Transform3DGroup transform = new Transform3DGroup();
            ModelTransform = transform;

            transform.Children.Add(new RotateTransform3D(new QuaternionRotation3D(Quaternion.Identity)));

            _model = new Model3DGroup()
            {
                Transform = transform
            };

            
            _scene.Children.Add(_model);

            PrepareScene();

            Model3DGroup packedScene = new Model3DGroup();
            _model.Children.Add(packedScene);

            appContext.Scene = _model;
            appContext.PackedScene = packedScene;
        }

        private void HandleMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.Source is not Viewport3D viewport) return;

            Point pos = e.GetPosition((IInputElement)viewport.Parent);
            _lastPos = pos;

            viewport.MouseMove += HandleDrag;
            viewport.MouseUp += HandleDragComplete;
            viewport.CaptureMouse();
        }

        private void HandleDrag(object sender, MouseEventArgs e)
        {
            if (e.Source is not Viewport3D viewport) return;

            Transform3DGroup group = _model.Transform as Transform3DGroup;
            RotateTransform3D rot = (group.Children[0] as RotateTransform3D);
            QuaternionRotation3D qrot = rot.Rotation as QuaternionRotation3D;

            Point pos = Mouse.GetPosition((IInputElement)viewport.Parent);

            const float sensitivity = .001f;
            double dx = pos.X - _lastPos.X, dy = pos.Y - _lastPos.Y;

            dx *= sensitivity;
            dy *= sensitivity;

            Vector3D v = new Vector3D(0, dx, dy);
            Quaternion qq = QuaternionExtensions.Euler(v) * qrot.Quaternion;

            qq.Normalize();
            qrot.Quaternion = qq;
        }

        private void HandleDragOld(object sender, MouseEventArgs e)
        {
            if (e.Source is not Viewport3D viewport) return;

            Point pos = Mouse.GetPosition((IInputElement)viewport.Parent);
            

            double dx = pos.X - _lastPos.X, dy = pos.Y - _lastPos.Y;
            _lastPos = pos;

            double mag = Math.Sqrt(Math.Pow(dx, 2) + Math.Pow(dy, 2));

            double theta = Math.Asin(dy / mag);

            

            Transform3DGroup group = _model.Transform as Transform3DGroup;
            RotateTransform3D rot = (group.Children[0] as RotateTransform3D);

            QuaternionRotation3D qrot = rot.Rotation as QuaternionRotation3D;

            



            Vector3D v1 = new Vector3D(dx, dy, 0);
            v1.Normalize();
            Vector3D v2 = QuaternionExtensions.ToEuler(qrot.Quaternion);
            Vector3D v3 = new Vector3D(0, 0, 1);





            Vector3D v = v1 + v2;
            v.Normalize();

            Vector3D v4 = Vector3D.CrossProduct(v1, v3);
            double qw = Math.Sqrt(v1.LengthSquared * v3.LengthSquared) + Vector3D.DotProduct(v1, v3);
            


            double angle = Vector3D.DotProduct(v1, v3);
            

            v4.Normalize();
            Quaternion q = new Quaternion(v4.X, v4.Y, v4.Z, qw);

            Console.WriteLine(q);

            try
            {
                v1.Normalize();
                v.Normalize();
                // qrot.Quaternion + 
                q.Normalize();
                Quaternion qq = qrot.Quaternion + q;//new Quaternion(v1, angle);
                qq.Normalize();
                qrot.Quaternion = qq;
            }
            catch { }
        }

        private void HandleDragComplete(object sender, MouseButtonEventArgs e)
        {
            if (e.Source is not Viewport3D viewport) return;

            viewport.MouseMove -= HandleDrag;
            viewport.MouseUp -= HandleDragComplete;
            viewport.ReleaseMouseCapture();
        }

        private void HandleMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (_viewport.Camera is PerspectiveCamera cam)
            {
                const float sensitivity = 0.01f;
                double z = cam.Position.Z - e.Delta * sensitivity;
                double min = _viewportMax - (_viewportMax / 3);
                double max = (_viewportMax * 10) - ((_viewportMax * 10) / 3);

                z = Math.Max(z, min);
                z = Math.Min(z, max);

                cam.Position = new Point3D(cam.Position.X, cam.Position.Y, z);
            }
            
        }

        public void OnNavigatedTo(NavigationContext nctx)
        {
            PrepareScene();
        }

        private void PrepareScene()
        {
            if (_scene == null) return;

            double viewRatio = .25;

            var con = _packagingService.Context.Container;

            double w = con.Width * viewRatio;
            double h = con.Height * viewRatio;
            double d = con.Depth * viewRatio;

            double hw = w / 2;
            double hy = h / 2;
            double hz = d / 2;

            double fw = w + con.Width;
            double fh = h + con.Height;
            double fz = d + con.Depth;

            Vector3D direction = new Point3D() - new Point3D(-w, hy, 0.0);

            _scene.Children.Add(new DirectionalLight(Colors.Gray, direction));
            _scene.Children.Add(new AmbientLight(Colors.Gray));

            double max = Math.Max(fw, fh);
            _viewportMax = max = Math.Max(max, fz);

            if (_viewport.Camera is PerspectiveCamera cam)
            {
                //cam.FieldOfView = max;
                cam.NearPlaneDistance = 0.01;
                cam.FarPlaneDistance = max * 10;
                cam.Position = new Point3D(0, 0, max);
                cam.LookDirection = new Vector3D(0, 0, -hz);
            }

            double avg = (con.Width + con.Height + con.Depth) / 3d;
            

            SplitCuboid container = new SplitCuboid(con.Width, con.Height, con.Depth, .5);
            Material material = new DiffuseMaterial(Brushes.White);
            
            DiffuseMaterial bMaterial = new DiffuseMaterial(_texture);
            bMaterial.Color = Color.FromRgb(0xee, 0xbf, 0x88);
            bMaterial.AmbientColor = Color.FromRgb(0xc9, 0xa2, 0x5a);

            _model.ModelSplitCuboid(container, material, bMaterial);

        }

        public bool IsNavigationTarget(NavigationContext navigationContext) => true;

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
        }
    }
}
