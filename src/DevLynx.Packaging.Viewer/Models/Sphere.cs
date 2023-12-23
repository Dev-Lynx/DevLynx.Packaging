using DevLynx.Packaging.Visualizer.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace DevLynx.Packaging.Visualizer.Models
{
    internal readonly struct Sphere
    {
        public readonly Point3D StartPoint;
        public readonly double Radius;

        public Sphere(Point3D start, double radius)
        {
            StartPoint = start;
            Radius = radius;
        }
    }

    internal static partial class MeshExtensions
    {
        public static void ModelSphere(this Model3DGroup scene, Sphere sphere, int resolution, Material material)
        {
            Model3DGroup model = new Model3DGroup();
            scene.Children.Add(model);

            MeshGeometry3D mesh = new MeshGeometry3D();
            mesh.AddSphere(sphere, resolution);

            scene.Children.Add(new GeometryModel3D(mesh, material));
        }

        public static void AddSphere(this MeshGeometry3D mesh, Sphere sphere, int resolution = 8)
        {
            Point3D s = sphere.StartPoint;

            double radius = sphere.Radius;
            double diameter = radius * 2;

            int slices = (int)Math.Round(diameter / resolution);
            if (slices <= 0) slices = resolution;

            double phi, theta;
            double x, y, z;
            
            Point3D v0 = s.CreateOffset(0, 1, 0);
            Point3D v1 = s.CreateOffset(0, -1, 0);


            int k = 0;
            Point3D[] vertices = new Point3D[slices*slices];
            vertices[k++] = v0;

            
            for (int i = 0; i < slices - 1; i++)
            {
                phi = Math.PI * (i + 1) / slices;

                for (int j = 0; j < slices; j++)
                {
                    theta = 2 * Math.PI * j / slices;

                    x = Math.Sin(phi) * Math.Cos(theta);
                    y = Math.Cos(phi);
                    z = Math.Sin(phi) * Math.Sin(theta);

                    vertices[++k] = new Point3D(x, y, z);
                }
            }

            int i0, i1, i2, i3;

            // add top / bottom triangles
            for (int i = 0; i < slices; ++i)
            {
                i0 = i + 1;
                i1 = (i + 1) % slices + 1;
                mesh.AddTriangle(v0, vertices[i1], vertices[i0]);

                i0 = i + slices * (slices - 2) + 1; // TODO: Crosscheck math order
                i1 = (i + 1) % slices + slices * (slices - 2) + 1;

                mesh.AddTriangle(v1, vertices[i0], vertices[i1]);
            }

            int j0, j1;

            for (int j = 0; j < slices - 2; j++)
            {
                j0 = j * slices + 1;
                j1 = (j + 1) * slices + 1;

                for (int i = 0; i < slices; i++)
                {
                    i0 = j0 + i;
                    i1 = j0 + (i + 1) % slices;
                    i2 = j1 + (i + 1) % slices;
                    i3 = j1 + i;

                    mesh.AddRect(new MeshRect3D(vertices[i0], vertices[i1], vertices[i2], vertices[i3]));
                }
            }
        }
    }
}
