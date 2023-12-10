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
        public static void ModelSplitCuboid(this Model3DGroup scene, SplitCuboid cuboid, Material front, Material back)
        {
            Model3DGroup model = new Model3DGroup();
            scene.Children.Add(model);

            Point3D p0 = cuboid.StartPoint;
            double w = cuboid.Width;
            double h = cuboid.Height;
            double d = cuboid.Depth;
            double th = cuboid.Thickness;


            MeshRect3D rect1 = new MeshRect3D(p0, w, h + th, 0);
            MeshRect3D rect2 = new MeshRect3D(p0, 0, h + th, d);
            MeshRect3D rect3 = new MeshRect3D(p0.AddOffset(0, h, 0), w, 0, d);

            MeshRect3D rect4 = new MeshRect3D(p0.AddOffset(0, h, d), w, th, 0);
            MeshRect3D rect5 = new MeshRect3D(p0.AddOffset(w, h, 0), 0, th, d);
            MeshRect3D rect6 = new MeshRect3D(p0.AddOffset(0, h + th, 0), w, 0, d);

            MeshRect3D rect7 = new MeshRect3D(p0.AddOffset(w, 0, -th), 0, h + th, th);
            MeshRect3D rect8 = new MeshRect3D(p0.AddOffset(0, 0, -th), w, 0, th);
            MeshRect3D rect9 = new MeshRect3D(p0.AddOffset(0, 0, -th), 0, h + th, th);
            MeshRect3D rect10 = new MeshRect3D(p0.AddOffset(0, h + th, -th), w, 0, th);
            MeshRect3D rect11 = new MeshRect3D(p0.AddOffset(0, 0, -th), w, h + th, 0);

            MeshRect3D rect12 = new MeshRect3D(p0.AddOffset(-th, 0, -th), th, 0, d + th);
            MeshRect3D rect13 = new MeshRect3D(p0.AddOffset(-th, 0, -th), th, h + th, 0);
            MeshRect3D rect14 = new MeshRect3D(p0.AddOffset(-th, h + th, -th), th, 0, d + th);
            MeshRect3D rect15 = new MeshRect3D(p0.AddOffset(-th, 0, d), th, h + th, 0);
            MeshRect3D rect16 = new MeshRect3D(p0.AddOffset(-th, 0, -th), 0, h + th, d + th);

            model.ModelRect(rect1, front, back);
            model.ModelRect(rect2, back, front);
            model.ModelRect(rect3, front, back);

            model.ModelRect(rect4, back, front);
            model.ModelRect(rect5, front, back);
            model.ModelRect(rect6, front, back);

            model.ModelRect(rect7, front, back);
            model.ModelRect(rect8, back, front);
            model.ModelRect(rect9, back, front);
            model.ModelRect(rect10, front, back);
            model.ModelRect(rect11, front, back);

            model.ModelRect(rect12, back, front);
            model.ModelRect(rect13, front, back);
            model.ModelRect(rect14, front, back);
            model.ModelRect(rect15, back, front);
            model.ModelRect(rect16, back, front);
        }
    }
}
