using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace DevLynx.Packaging.Visualizer.Extensions
{
    internal static class QuaternionExtensions
    {
        public static Quaternion Euler(Vector3D v) => Euler(v.X, v.Y, v.Z);

        public static Quaternion Euler(double yaw, double pitch, double roll)
        {
            // Convert degrees to radians
            yaw = yaw * Math.PI / 180;
            pitch = pitch * Math.PI / 180;
            roll = roll * Math.PI / 180;

            return RadEulerRad(yaw, pitch, roll);
        }

        public static Quaternion RadEulerRad(Vector3D v) => RadEulerRad(v.X, v.Y, v.Z);

        public static Quaternion RadEulerRad(double yaw, double pitch, double roll)
        {
            yaw /= 2;
            pitch /= 2;
            roll /= 2;

            double sinRoll = Math.Sin(roll);
            double cosRoll = Math.Cos(roll);

            double sinPitch = Math.Sin(pitch);
            double cosPitch = Math.Cos(pitch);

            double sinYaw = Math.Sin(yaw);
            double cosYaw = Math.Cos(yaw);

            double qx, qy, qz, qw;

            qx = sinRoll * cosPitch * cosYaw - cosRoll * sinPitch * sinYaw;
            qy = cosRoll * sinPitch * cosYaw + sinRoll * cosPitch * sinYaw;
            qz = cosRoll * cosPitch * sinYaw - sinRoll * sinPitch * cosYaw;
            qw = cosRoll * cosPitch * cosYaw + sinRoll * sinPitch * sinYaw;

            return new Quaternion(qx, qy, qz, qw);
        }

        public static Vector3D ToEuler(this Quaternion q)
        {
            // TODO: Get the standard conversion
            double x = q.X, y = q.Y, z = q.Z, w = q.W;

            double t0, t1, t2, t3, t4;
            Vector3D v = new Vector3D();


            t0 = 2 * (w * x + y * z);
            t1 = 1 - 2 * (x * x + y * y);
            v.X = Math.Atan2(t0, t1);

            t2 = 2 * (w * y - z * x);

            if (t2 > 1) t2 = 1;
            else if (t2 < -1) t2 = -1;
            v.Y = Math.Asin(t2);

            t3 = 2 * (w * z + x * y);
            t4 = 1 - 2 * (y * y + z * z);
            v.Z = Math.Atan2(t3, t4);

            return v;
        }

        private static Quaternion Norm(this Quaternion q)
        {
            q.Normalize();
            return q;
        }

        //private static Lazy<Quaternion> X_90 = new Lazy<Quaternion>(() => Euler(0, 0, 90).Norm());
        private static Lazy<Quaternion> X_90 = new Lazy<Quaternion>(() => (new Quaternion(0, 0.7071, 0.7071, 0)).Norm());
        private static Lazy<Quaternion> Y_90 = new Lazy<Quaternion>(() => Euler(0, 90, 0).Norm());
        private static Lazy<Quaternion> Y_INV_90 = new Lazy<Quaternion>(() => Euler(0, -90, 0).Norm());
        //private static Lazy<Quaternion> Z_90 = new Lazy<Quaternion>(() => Euler(270, 0,0).Norm());

        //private static Lazy<Quaternion> Z_90 = new Lazy<Quaternion>(() => Euler(-90, 180, -180).Norm());
        private static Lazy<Quaternion> Z_90 = new Lazy<Quaternion>(() => Euler(90, 180, -180).Norm());

        //private static Lazy<Quaternion> YX_90 = new Lazy<Quaternion>(() => (Z_90.Value * Y_90.Value).Norm());
        // Euler(90, 0, 90)
        /*
         * w = 0.5
x = -0.5
y = 0.5
z = -0.5
        */
        private static Lazy<Quaternion> YX_90 = new Lazy<Quaternion>(() => (Euler(0, 0, 90) * Y_90.Value).Norm());
        private static Lazy<Quaternion> YZ_90 = new Lazy<Quaternion>(() => (new Quaternion(-.5, -.5, -.5, .5)).Norm());

        public static Quaternion FromOrientation(PackOrientation orientation)
        {
            switch (orientation)
            {
                default: return Quaternion.Identity;

                case PackOrientation.X_90: return X_90.Value;

                case PackOrientation.Y_90: return Y_INV_90.Value;

                case PackOrientation.Z_90: return Z_90.Value;

                case PackOrientation.YX_90: return YX_90.Value;

                case PackOrientation.YZ_90: return YZ_90.Value;
            }
        }
    }
}
