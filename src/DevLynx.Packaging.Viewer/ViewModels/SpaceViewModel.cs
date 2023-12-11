using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace DevLynx.Packaging.Visualizer.ViewModels
{
    internal class SpaceViewModel : BindableBase
    {
        public ICommand LoadedCommand { get; }

        private Point _lastPos;
        private Brush _texture;

        private Viewport3D _viewport;
        private Model3DGroup _model;

        public SpaceViewModel()
        {
            _texture = (Brush)Application.Current.FindResource("CartonTextureBrush");

            LoadedCommand = new DelegateCommand<object>(HandleLoaded);
        }

        private void HandleLoaded(object obj)
        {
            if (obj is not RoutedEventArgs e) return;
            if (e.Source is not Viewport3D viewport) return;

            _viewport = viewport;
            _viewport.MouseDown += HandleMouseDown;
            _viewport.MouseWheel += HandleMouseWheel;
        }

        private void HandleMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.Source is not Viewport3D viewport) return;

            Point pos = e.GetPosition(viewport);
            _lastPos = new Point(pos.X - viewport.ActualWidth / 2, (viewport.ActualHeight / 2) - pos.Y);

            viewport.MouseMove += HandleDrag;
            viewport.MouseUp += HandleDragComplete;
            viewport.CaptureMouse();
        }

        private void HandleDrag(object sender, MouseEventArgs e)
        {
            if (e.Source is not Viewport3D viewport) return;

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

            Transform3DGroup group = _model.Transform as Transform3DGroup;
            QuaternionRotation3D r = new QuaternionRotation3D(new System.Windows.Media.Media3D.Quaternion(axis, rotation * 180 / Math.PI));
            group.Children.Add(new RotateTransform3D(r));

            _lastPos = actualPos;
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
        }
    }
}
