using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace DevLynx.Packaging.Visualizer.Extensions
{
    internal static class VectorExtensions
    {
        public static Vector3D Orthogonal(this Vector3D v)
        {
            double x = Math.Abs(v.X);
            double y = Math.Abs(v.Y);
            double z = Math.Abs(v.Z);

            Vector3D xAxis = new Vector3D(1, 0, 0);
            Vector3D yAxis = new Vector3D(0, 1, 0);
            Vector3D zAxis = new Vector3D(0, 0, 1);

            Vector3D other = x < y ? (x < z ? xAxis : zAxis) : (y < z ? yAxis : zAxis);
            return Vector3D.CrossProduct(v, other);
        }
    }
}
