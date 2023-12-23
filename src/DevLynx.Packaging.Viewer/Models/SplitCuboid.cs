using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Media3D;

namespace DevLynx.Packaging.Visualizer.Models
{
    /// <summary>
    /// Replesents Half a cuboid (or the cuboid equivalent of a hemicube) with thickness
    /// </summary>
    internal readonly struct SplitCuboid
    {
        public readonly Point3D StartPoint;
        
        public readonly double Width;
        public readonly double Height;
        public readonly double Depth;

        public readonly double Thickness;
        

        public SplitCuboid(Point3D startPoint, double width, double height, double depth, double thickness)
        {
            StartPoint = startPoint;
            Width = width;
            Height = height;
            Depth = depth;
            Thickness = thickness;
        }

        public SplitCuboid(double width, double height, double depth, double thickness)
        {
            StartPoint = new Point3D(-(width / 2), -(height / 2), -(depth / 2));
            Width = width;
            Height = height;
            Depth = depth;
            Thickness = thickness;
        }
    }

    internal static class SplitCuboidExtensions
    {
        private enum CuboidPart
        {
            X, Y, Z
        }

        public static void ModelSplitCuboid(this Model3DGroup scene, SplitCuboid cuboid, Material front, Material back)
        {
            Model3DGroup model = new Model3DGroup();
            scene.Children.Add(model);

            Point3D p0 = cuboid.StartPoint;
            double w = cuboid.Width;
            double h = cuboid.Height;
            double d = cuboid.Depth;
            double th = cuboid.Thickness;

            Cuboid c1 = new Cuboid(p0, th, h, d);
            Cuboid c2 = new Cuboid(p0, w, th, d);
            Cuboid c3 = new Cuboid(p0, w, h, th);

            model.ModelSplitCuboidPart(c2, front, back, CuboidPart.X);
            model.ModelSplitCuboidPart(c1, front, back, CuboidPart.Y);
            model.ModelSplitCuboidPart(c3, front, back, CuboidPart.Z);
        }

        private static void ModelSplitCuboidPart(this Model3DGroup scene, Cuboid cuboid, Material front, Material back, CuboidPart part)
        {
            Model3DGroup model = new Model3DGroup();
            scene.Children.Add(model);

            Point3D p0 = cuboid.StartPoint;
            double w = cuboid.Width;
            double h = cuboid.Height;
            double d = cuboid.Depth;

            MeshRect3D rect1 = new MeshRect3D(p0, w, h, 0);
            MeshRect3D rect2 = new MeshRect3D(p0, 0, h, d);
            MeshRect3D rect3 = new MeshRect3D(p0.CreateOffset(0, h, 0), w, 0, d);

            MeshRect3D rect4 = new MeshRect3D(p0.CreateOffset(0, 0, d), w, h, 0);
            MeshRect3D rect5 = new MeshRect3D(p0.CreateOffset(w, 0, 0), 0, h, d);
            MeshRect3D rect6 = new MeshRect3D(p0, w, 0, d);

            model.ModelRect(rect1, back, back);
            model.ModelRect(rect2, back, back);

            if (part == CuboidPart.X)
                model.ModelRect(rect3, back, front); // X Front 2
            else
                model.ModelRect(rect3, back, back); // X Front 2

            if (part == CuboidPart.Y)
                model.ModelRect(rect5, back, front); // Y Front 2
            else model.ModelRect(rect5, back, back); // Y Front 2

            if (part == CuboidPart.Z)
                model.ModelRect(rect4, front, back); // Z Front 1
            else model.ModelRect(rect4, back, back); // Z Front 1
            model.ModelRect(rect6, back, back);
        }
    }
}
