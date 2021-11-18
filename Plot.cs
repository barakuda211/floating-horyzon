using floating_horyzon.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace floating_horyzon
{
    public static class Plot
    {
        private static double Func(double x, double y)
        {
            //double r = 12*x * x + 12*y * y;
            double r = x * x + y * y;
            return Math.Cos(r) / (r + 1);
        }
        
        public static Mesh GetMesh(double x0, double x1, double dx, double z0, double z1, double dz,double AngleX = Math.PI/4, double AngleY = Math.PI / 2, double AngleZ = Math.PI / 4, double scale = 1)
        {
            int nx = (int)((x1 - x0) / dx);
            int nz = (int)((z1 - z0) / dz);
            var vertices = new Point3D[nx * nz];
            var indices = new int[(nx - 1) * (nz - 1)][];
            for (int i = 0; i < nx; ++i)
                for (int j = 0; j < nz; ++j)
                {
                    var x = x0 + dx * i;
                    var z = z0 + dz * j;
                    vertices[i * nz + j] = new Point3D(x*scale, Func(x, z)*scale, z*scale);
                }
            for (int i = 0; i < nx - 1; ++i)
                for (int j = 0; j < nz - 1; j++)
                {
                    indices[i * (nz - 1) + j] = new int[4] {
                        i * nz + j,
                        (i + 1) * nz + j,
                        (i + 1) * nz + j + 1,
                        i * nz + j + 1
                    };
                }

			Mesh m = new Mesh(vertices, indices,nx,nz);

            m.Apply(Athens.RotateX(-AngleX));
            m.Apply(Athens.RotateZ(-AngleZ));
            m.Apply(Athens.RotateY(-AngleY));

            m.DeleteInvisible();

            m.Apply(Athens.RotateY(AngleY));
            m.Apply(Athens.RotateZ(AngleZ));
            m.Apply(Athens.RotateX(AngleX));

            return m;
        }
    }
}
