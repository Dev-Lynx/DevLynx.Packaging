using DevLynx.Packaging.Visualizer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace DevLynx.Packaging.Visualizer.UI.Controls
{
    internal class AxisIndicator : Control
    {
        public AxisIndicator()
        {
            Width = Height = 120;
        }

        public Transform3DGroup AxisTransform
        {
            get { return (Transform3DGroup)GetValue(AxisTransformProperty); }
            set { SetValue(AxisTransformProperty, value); }
        }

        public static readonly DependencyProperty AxisTransformProperty =
            DependencyProperty.Register("AxisTransform", typeof(Transform3DGroup), typeof(AxisIndicator), new PropertyMetadata(new Transform3DGroup()));



        public override void OnApplyTemplate()
        {
            if (GetTemplateChild("PART_Viewport") is Viewport3D viewport)
            {
                Model3DGroup scene = new Model3DGroup();

                ModelVisual3D visual = new ModelVisual3D();
                viewport.Children.Add(visual);
                visual.Content = scene;

                viewport.Camera = new PerspectiveCamera()
                {
                    FarPlaneDistance = 100,
                    FieldOfView = 100,
                    LookDirection = new Vector3D(0, 0, -10),
                    NearPlaneDistance = 0,
                    Position = new Point3D(0, 0, 8),
                    UpDirection = new Vector3D(0, 1, 0)
                };
                //viewport.Loaded += HandleViewportLoaded;

                RenderAxis(scene);

                Vector3D direction = new Point3D() - new Point3D(-1, 1, 0.0);
                scene.Children.Add(new DirectionalLight(Colors.Gray, direction));
                scene.Children.Add(new AmbientLight(Colors.Gray));
            }

            base.OnApplyTemplate();
        }

        private void HandleViewportLoaded(object sender, RoutedEventArgs e)
        {
            if (sender is not Viewport3D viewport) return;

            
            if (viewport.Camera is PerspectiveCamera cam)
            {
                cam.Position = new Point3D(0, 0, -8);
            }
        }

        private void RenderAxis(Model3DGroup scene)
        {
            Model3DGroup model = new Model3DGroup();
            scene.Children.Add(model);

            Material red = new DiffuseMaterial(Brushes.Red);
            Material green = new DiffuseMaterial(Brushes.Green);
            Material blue = new DiffuseMaterial(Brushes.Blue);
            Material white = new DiffuseMaterial(Brushes.White);


            

            model.ModelCuboid(new Cuboid(new Point3D(1, 0, 0), 4, 1, 1), red);

            model.ModelCuboid(new Cuboid(new Point3D(0, 1, 0), 1, 4, 1), green);

            model.ModelCuboid(new Cuboid(new Point3D(0, 0, -4), 1, 1, 4), blue);

            model.ModelCuboid(new Cuboid(new Point3D(0, 0, 0), 1, 1, 1), white);


            BindingOperations.SetBinding(model, Model3D.TransformProperty, new Binding()
            {
                Source = this,
                Path = new PropertyPath(nameof(AxisTransform))
            });

            //model.Transform = AxisTransform;
        }
    }
}
