using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace DevLynx.Packaging.Visualizer.Models
{
    internal static class MeshExtensions
    {
        public static Point3D AddOffset(this Point3D pt, double x, double y, double z)
        {
            return new Point3D(pt.X + x, pt.Y + y, pt.Z + z);
        }

        public static void AddRect(this MeshGeometry3D mesh, MeshRect3D rect)
        {
            AddTriangle(mesh, rect.P0, rect.P1, rect.P2);
            AddTriangle(mesh, rect.P0, rect.P2, rect.P3);

            mesh.TextureCoordinates.Add(new System.Windows.Point(0, 0));
            mesh.TextureCoordinates.Add(new System.Windows.Point(0, 1));
            mesh.TextureCoordinates.Add(new System.Windows.Point(1, 0));
            mesh.TextureCoordinates.Add(new System.Windows.Point(1, 1));
        }

        public static void AddTriangle(this MeshGeometry3D mesh, Point3D p0, Point3D p1, Point3D p2)
        {
            mesh.TriangleIndices.Add(mesh.AddPoint(p0));
            mesh.TriangleIndices.Add(mesh.AddPoint(p1));
            mesh.TriangleIndices.Add(mesh.AddPoint(p2));   
        }

        private static int AddPoint(this MeshGeometry3D mesh, Point3D p)
        {
            int index = mesh.Positions.IndexOf(p);

            if (index < 0)
            {
                index = mesh.Positions.Count;
                mesh.Positions.Add(p);
            }

            return index;
        }

        public static void ModelRect(this Model3DGroup scene, Point3D center, double width, double height, double depth, Material front, Material back)
        {
            MeshGeometry3D mesh = new MeshGeometry3D();
            mesh.AddRect(new MeshRect3D(center, width, height, depth));

            scene.Children.Add(new GeometryModel3D(mesh, front)
            {
                BackMaterial = back
            });
        }

        public static void ModelRect(this Model3DGroup scene, MeshRect3D rect, Material front, Material back)
        {
            MeshGeometry3D mesh = new MeshGeometry3D();
            mesh.AddRect(rect);

            scene.Children.Add(new GeometryModel3D(mesh, front)
            {
                BackMaterial = back
            });
        }



        private static Vector3D CalculateNormal(Point3D p0, Point3D p1, Point3D p2)
        {
            Vector3D v0 = new Vector3D(p1.X - p0.X, p1.Y - p0.Y, p1.Z - p0.Z);
            Vector3D v1 = new Vector3D(p2.X - p1.X, p2.Y - p1.Y, p2.Z - p1.Z);

            return Vector3D.CrossProduct(v0, v1);
        }
    }

    internal readonly struct MeshRect3D
    {
        private readonly Point3D _p0;
        private readonly Point3D _p1;
        private readonly Point3D _p2;
        private readonly Point3D _p3;

        public Point3D P0 => _p0;
        public Point3D P1 => _p1;
        public Point3D P2 => _p2;
        public Point3D P3 => _p3;

        public MeshRect3D(Point3D p0, double width, double height, double depth)
        {
            _p0 = p0;

            if (width > 0 && height > 0)
            {
                _p1 = p0.AddOffset(width, 0, 0);
                _p2 = p0.AddOffset(width, height, 0);
                _p3 = p0.AddOffset(0, height, 0);
            }
            else if (height > 0 && depth > 0)
            {
                _p1 = p0.AddOffset(0, 0, depth);
                _p2 = p0.AddOffset(0, height, depth);
                _p3 = p0.AddOffset(0, height, 0);
            }
            else if (width > 0 && depth > 0)
            {
                _p1 = p0.AddOffset(width, 0, 0);
                _p2 = p0.AddOffset(width, 0, depth);
                _p3 = p0.AddOffset(0, 0, depth);
            }

        }

        //public MeshRect3D(double size) : this (new Point3D(), size, size, size) { }
        //public MeshRect3D(Point3D centerPoint) : this (centerPoint, 1, 1, 1) { }
        //public MeshRect3D(Point3D centerPoint, double width, double height, double depth)
        //{
        //    Point3D pt = centerPoint;

        //    double x = Math.Abs(width / 2);
        //    double y = Math.Abs(height / 2);
        //    double z = Math.Abs(depth / 2);

        //    if (width > 0 && height > 0)
        //    {
        //        _p0 = pt.AddOffset(-x, -y, 0);
        //        _p1 = pt.AddOffset(x, -y, 0);
        //        _p2 = pt.AddOffset(-x, y, 0);
        //        _p3 = pt.AddOffset(x, y, 0);
        //    }
        //    else if (height > 0 && depth > 0)
        //    {
        //        _p0 = pt.AddOffset(0, -y, -z);
        //        _p1 = pt.AddOffset(0, y, -z);
        //        _p2 = pt.AddOffset(0, -y, z);
        //        _p3 = pt.AddOffset(0, y, z);
        //    }
        //    else if (width > 0 && depth > 0)
        //    {
        //        _p0 = pt.AddOffset(-x, 0, -z);
        //        _p1 = pt.AddOffset(x, 0, -z);
        //        _p2 = pt.AddOffset(-x, 0, z);
        //        _p3 = pt.AddOffset(x, 0, z);
        //    }
        //}
    }
}
