using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace DevLynx.Packaging.Visualizer.Models
{
    internal readonly struct Cuboid
    {
        public readonly Point3D StartPoint;

        public readonly double Width;
        public readonly double Height;
        public readonly double Depth;

        public Cuboid(Point3D startPoint, double width, double height, double depth)
        {
            StartPoint = startPoint;
            Width = width;
            Height = height;
            Depth = depth;
        }

        public Cuboid(double width, double height, double depth)
        {
            StartPoint = new Point3D(-(width / 2), -(height / 2), -(depth / 2));
            Width = width;
            Height = height;
            Depth = depth;
        }
    }

    internal static class CuboidExtensions
    {
        public static void ModelCuboid(this Model3DGroup scene, Cuboid cuboid, Material material)
        {
            Model3DGroup model = new Model3DGroup();
            scene.Children.Add(model);

            Point3D p0 = cuboid.StartPoint;
            double w = cuboid.Width;
            double h = cuboid.Height;
            double d = cuboid.Depth;

            MeshRect3D rect1 = new MeshRect3D(p0, w, h, 0);
            MeshRect3D rect2 = new MeshRect3D(p0, 0, h, d);
            MeshRect3D rect3 = new MeshRect3D(p0.AddOffset(0, h, 0), w, 0, d);

            MeshRect3D rect4 = new MeshRect3D(p0.AddOffset(0, 0, d), w, h, 0);
            MeshRect3D rect5 = new MeshRect3D(p0.AddOffset(w, 0, 0), 0, h, d);
            MeshRect3D rect6 = new MeshRect3D(p0, w, 0, d);

            model.ModelRect(rect1, material, material);
            model.ModelRect(rect2, material, material);
            model.ModelRect(rect3, material, material);

            model.ModelRect(rect4, material, material);
            model.ModelRect(rect5, material, material);
            model.ModelRect(rect6, material, material);
        }
    }
}
