using DevLynx.Packaging.Visualizer.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace DevLynx.Packaging.Visualizer.Extensions
{
    internal static class MediaExtensions
    {
        public static void AddTriangle(this Int32Collection col, int p1, int p2, int p3)
        {
            col.Add(p1);
            col.Add(p2);
            col.Add(p3);
        }

        public static void AddTriangle(this MeshGeometry3D mesh, int p1, int p2, int p3)
        {
            mesh.TriangleIndices.Add(p1);
            mesh.TriangleIndices.Add(p2);
            mesh.TriangleIndices.Add(p3);
        }

        //public static void AddTriangle(this MeshGeometry3D mesh, Triangle3D t)
        //{
        //    mesh.TriangleIndices.Add(t.P1);
        //}
    }
}
