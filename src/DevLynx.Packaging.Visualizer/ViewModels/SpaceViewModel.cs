using DevLynx.Packaging.Visualizer.Extensions;
using DevLynx.Packaging.Visualizer.Models;
using DevLynx.Packaging.Visualizer.Models.Contexts;
using DevLynx.Packaging.Visualizer.Services;
using DevLynx.Packaging.Visualizer.UI;
using NLog;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.DirectoryServices;
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
        private readonly ILogger _logger;

        private Point _lastPos;
        private Brush _texture;

        private Viewport3D _viewport;
        private double _viewportMax;


        private Model3D _lModel;
        private Model3DGroup _model;

        private Model3DGroup _scene;
        private Transform3D _rootTransform;
        
        private SolidAnimation _upAnim;
        private SolidAnimation _downAnim;

        private RootContext _context => _appService.Context;
        private PackageContext _pcontext => _packagingService.Context;

        private bool _canRender;
        private bool _isFresh;

        public SpaceViewModel(IPackagingService packagingService, IAppService appService, ILogger logger)
        {
            _texture = (Brush)Application.Current.FindResource("CartonTextureBrush");

            _logger = logger;
            _appService = appService;
            _packagingService = packagingService;
            
            Duration duration = new Duration(TimeSpan.FromMilliseconds(400));
            _upAnim = new SolidAnimation(new DoubleKeyFrameCollection
            {
                new EasingDoubleKeyFrame(0, KeyTime.FromPercent(0)),
                new EasingDoubleKeyFrame(1.1, KeyTime.FromPercent(.6)),
                new EasingDoubleKeyFrame(1, KeyTime.FromPercent(.8))
            }, duration);

            _downAnim = new SolidAnimation(new DoubleKeyFrameCollection
            {
                new EasingDoubleKeyFrame(1, KeyTime.FromPercent(0)),
                new EasingDoubleKeyFrame(1.1, KeyTime.FromPercent(.2)),
                new EasingDoubleKeyFrame(0, KeyTime.FromPercent(1))
            }, duration);

            LoadedCommand = new DelegateCommand<object>(HandleLoaded);

            _packagingService.Context.PropertyChanged += HandlePackageContextChanged;
            _packagingService.PackComplete += HandlePackComplete;
        }

        private void HandlePackComplete(object sender, EventArgs e)
        {
            _isFresh = true;
            if (_viewport == null)
            {
                _canRender = true;
                return;
            }

            Application.Current.Dispatcher.BeginInvoke(StartRender);
        }

        private void HandlePackageContextChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(PackageContext.SimContainer))
            {
                StartRender();
            }
        }

        private void HandleLoaded(object obj)
        {
            if (obj is not RoutedEventArgs e) return;
            if (e.Source is not Viewport3D viewport) return;

            if (_viewport == null)
            {
                _viewport = viewport;
                _context.Viewport = viewport;
            }

            if (_canRender) StartRender();
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
            if (_model.Transform is not Transform3DGroup root) return;
            if (root.Children.ElementAtOrDefault(0) is not Transform3DGroup group) return;
            if (group.Children.ElementAtOrDefault(0) is not RotateTransform3D rot) return;
            if (rot.Rotation is not QuaternionRotation3D qrot) return;

            Point pos = Mouse.GetPosition((IInputElement)viewport.Parent);

            const float sensitivity = .001f;
            double dx = pos.X - _lastPos.X, dy = pos.Y - _lastPos.Y;

            dx *= sensitivity;
            dy *= sensitivity;

            Vector3D v = new Vector3D(0, dx, dy);
            Quaternion qq = QuaternionExtensions.RadEulerRad(v) * qrot.Quaternion;

            qq.Normalize();

            const double W_LOWER = .6;

            const double X_LOWER = .025;
            const double X_UPPER = .75;

            const double Y_LOWER = -.075;
            const double Y_UPPER = -.75;

            const double Z_LOWER = .025;
            const double Z_UPPER = .25;

            if (qq.W < W_LOWER) qq.W = W_LOWER;

            if (qq.X < X_LOWER) qq.X = X_LOWER;
            else if (qq.X > X_UPPER) qq.X = X_UPPER;

            if (qq.Y > Y_LOWER) qq.Y = Y_LOWER;
            else if (qq.Y < Y_UPPER) qq.Y = Y_UPPER;

            if (qq.Z < Z_LOWER) qq.Z = Z_LOWER;
            else if (qq.Z > Z_UPPER) qq.Z = Z_UPPER;

            qrot.Quaternion = qq;
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

                Console.WriteLine("ZZZ: {0}", z);

                cam.Position = new Point3D(cam.Position.X, cam.Position.Y, z);
            }
            
        }

        private void StartRender()
        {
            if (_model == null)
            {
                RenderScene(true);
                return;
            }

            _pcontext.Rendering = true;
            RenderScene(false);
            _isFresh = false;

            if (_model.Transform is not Transform3DGroup rootTransform)
                return;

            if (rootTransform.Children.ElementAtOrDefault(1) is not ScaleTransform3D trans)
                return;

            if (_lModel is not Model3D m1) return;
            if (m1.Transform is not Transform3DGroup g1) return;
            if (g1.Children.ElementAtOrDefault(1) is not ScaleTransform3D s1) return;

            if (_model is not Model3D m2) return;
            if (m2.Transform is not Transform3DGroup g2) return;
            if (g2.Children.ElementAtOrDefault(1) is not ScaleTransform3D s2) return;

            _logger.Warn("[Render started] {0} {1}", _lModel.GetHashCode(), _model.GetHashCode());


            var echo = s1
                .BeginAnimation(_downAnim, SolidAnimationKind.Scale)
                .ThenBegin(s2, _upAnim, SolidAnimationKind.Scale)
                .Then(() =>
                {
                    _logger.Warn("[Animation complete] {0}", s2.ScaleX);
                    _scene.Children.Remove(m1);
                    _pcontext.Rendering = false;
                });

            /*
             var echo = s2.BeginAnimation(_upAnim, SolidAnimationKind.Scale)
                //.BeginAnimation(_downAnim, SolidAnimationKind.Scale)
                //.ThenBegin(s2, _upAnim, SolidAnimationKind.Scale)
                .Then(() =>
                {
                    _logger.Warn("[Animation complete] {0}", s2.ScaleX);
                    _scene.Children.Remove(m1);
                    _pcontext.Rendering = false;
                });
             */
        }

        private void RenderScene(bool initial)
        {
            var appContext = _appService.Context;

            if (initial)
            {
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

                //Transform3DGroup rootTransform = new Transform3DGroup();
                //rootTransform.Children.Add(transform);
                //rootTransform.Children.Add(new ScaleTransform3D(0, 0, 0));

                //_rootTransform = rootTransform;

            }

            Transform3DGroup rootTransform = new Transform3DGroup();
            rootTransform.Children.Add(ModelTransform);
            rootTransform.Children.Add(new ScaleTransform3D(0, 0, 0));

            _rootTransform = rootTransform;

            _lModel = _model;

            //_scene.Children.Clear();
            _model = new Model3DGroup()
            {
                Transform = _rootTransform
            };
            _scene.Children.Add(_model);


            PrepareScene(initial);

            Model3DGroup packedScene = new Model3DGroup();
            _model.Children.Add(packedScene);

            appContext.Scene = _model;
            appContext.PackedScene = packedScene;
        }

        private void PrepareScene(bool initial)
        {
            if (_scene == null) return;

            double viewRatio = .25;

            
            var con = _packagingService.Context.SimContainer;
            if (con == Vector3.Zero)
                con = new Vector3(_packagingService.Context.Container.Width, _packagingService.Context.Container.Height, _packagingService.Context.Container.Depth);

            _logger.Warn("Container: {0}", con);

            double w = con.X * viewRatio;
            double h = con.Y * viewRatio;
            double d = con.Z * viewRatio;

            double hw = w / 2;
            double hy = h / 2;
            double hz = d / 2;

            double fw = w + con.X;
            double fh = h + con.Y;
            double fz = d + con.Z;

            Vector3D direction = new Point3D() - new Point3D(-w, hy, 0.0);

            if (initial)
            {
                _scene.Children.Add(new DirectionalLight(Colors.Gray, direction));
                _scene.Children.Add(new AmbientLight(Colors.Gray));
            }
            

            double max = Math.Max(fw, fh);
            _viewportMax = max = Math.Max(max, fz);
            
            //_viewportMax = fz;

            double avg = (con.X + con.Y + con.Z) / 3d;
            _viewportMax = avg;
            if (_viewport.Camera is PerspectiveCamera cam)
            {
                if (_isFresh)
                {
                    //cam.FieldOfView = max;
                    cam.NearPlaneDistance = 0.01;
                    //cam.FarPlaneDistance = max * 10;
                    //cam.Position = new Point3D(0, 0, max / 2);
                    cam.FarPlaneDistance = max * 10;
                    cam.Position = new Point3D(0, 0, avg * 1.4);
                    cam.LookDirection = new Vector3D(0, 0, -hz);
                }
                
            }

            double thickness = avg * .02;
            _packagingService.Context.ContainerThickness = thickness;

            SplitCuboid container = new SplitCuboid(con.X, con.Y, con.Z, thickness);
            Material material = new DiffuseMaterial(Brushes.White);
            
            DiffuseMaterial bMaterial = new DiffuseMaterial(_texture);
            bMaterial.Color = Color.FromRgb(0xee, 0xbf, 0x88);
            bMaterial.AmbientColor = Color.FromRgb(0xc9, 0xa2, 0x5a);

            _model.ModelSplitCuboid(container, material, bMaterial);

        }

        public bool IsNavigationTarget(NavigationContext navigationContext) => true;

        public void OnNavigatedTo(NavigationContext nctx)
        {
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
        }
    }
}
