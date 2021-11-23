using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using static floating_horyzon.Form1;

namespace floating_horyzon
{
    public struct Point3D
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public double W { get; set; }

        public Point3D(double x, double y, double z = 0, double w = 1)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        public static Point3D operator *(double x, Point3D v)
        {
            for (int i = 0; i < 3; ++i)
                v[i] *= x;
            return v;
        }

        public static Point3D operator *(Point3D v, double x)
        {
            return x * v;
        }

        public static Point3D operator /(Point3D v, double x)
        {
            return v * (1 / x);
        }

        public static Point3D operator +(Point3D u, Point3D v)
        {
            Point3D result = new Point3D();
            for (int i = 0; i < 3; ++i)
                result[i] = u[i] * v.W + v[i] * u.W;
            result.W = u.W * v.W;
            return result;
        }

        public static Point3D operator +(double x, Point3D v)
        {
            return v + x;
        }

        public static Point3D operator +(Point3D v, double x)
        {
            for (int i = 0; i < 3; ++i)
                v[i] += x * v.W;
            return v;
        }

        public static Point3D operator -(Point3D v, double x)
        {
            return v + (-x);
        }

        public static Point3D operator -(double x, Point3D v)
        {
            return x + (-v);
        }

        public static Point3D operator -(Point3D v)
        {
            return -1 * v;
        }

        public Point3D Normalize()
        {
            var length = Modul() * W;
            if (0 == length) return new Point3D(0, 0, 0);
            var result = new Point3D(X / length, Y / length, Z / length, 1);
            var resultLength = result.Modul();
            if (0.1e6 < Math.Abs(1 - resultLength)) throw new Exception("You shouldn't see these words.");
            return result;
        }

        // Скалярное произведение векторов
        public static double DotProduct(Point3D u, Point3D v)
        {
            double result = 0;
            for (int i = 0; i < 3; ++i)
                result += u[i] * v[i];
            return result / (u.W * v.W);
        }

        public static double DotProduct4(Point3D u, Point3D v)
        {
            double result = 0;
            for (int i = 0; i < 4; ++i)
                result += u[i] * v[i];
            return result;
        }

        // Векторное произведение векторов
        public static Point3D CrossProduct(Point3D u, Point3D v)
        {
            return new Point3D(
                (u[1] * v[2] - u[2] * v[1]) / (u.W * v.W),
                (u[2] * v[0] - u[0] * v[2]) / (u.W * v.W),
                (u[0] * v[1] - u[1] * v[0]) / (u.W * v.W));
        }

        public static Point3D operator *(Point3D v, Matrix m)
        {
            var result = v;
            for (int i = 0; i < 4; ++i)
            {
                result[i] = 0;
                for (int j = 0; j < 4; ++j)
                    result[i] += v[j] * m[j, i];
            }
            return result;
        }

        public double this[int i]
        {
            get
            {
                switch (i)
                {
                    case 0: return X;
                    case 1: return Y;
                    case 2: return Z;
                    case 3: return W;
                    default: throw new IndexOutOfRangeException("Vertex has only 4 coordinates");
                }
            }
            set
            {
                switch (i)
                {
                    case 0: X = value; break;
                    case 1: Y = value; break;
                    case 2: Z = value; break;
                    case 3: W = value; break;
                    default: throw new IndexOutOfRangeException("Vertex has only 4 coordinates");
                }
            }
        }

        public override string ToString()
        {
            return string.Format("({0}, {1}, {2})", X, Y, Z);
        }

        // Модуль
        public double Modul()
        {
            return Math.Sqrt(DotProduct(this, this));
        }

        // Угол между векторами
        public static double AngleBet(Point3D u, Point3D v)
        {
            return Math.Acos(DotProduct(u, v) / (u.Modul() * v.Modul()));
        }

        public static double Dist(Point3D u, Point3D v)
        {
            return Math.Sqrt((u.X - v.X) * (u.X - v.X) +
                             (u.Y - v.Y) * (u.Y - v.Y) +
                             (u.Z - v.Z) * (u.Z - v.Z));
        }

        public static Point3D operator -(Point3D u, Point3D v)
        {
            return u + (-v);
        }
    }

    public class Mesh 
    {
        public Point3D[] points { get; set; }
		public int[][] indices { get; set; }
        private int nx, nz;

        public Mesh(Point3D[] vertices, int[][] indices,int nx, int nz)
        {
            this.points = vertices;
            this.indices = indices;
            this.nx = nx;
            this.nz = nz;
        }
        public virtual void Apply(Matrix transformation)
        {
            for (int i = 0; i < points.Length; ++i)
                points[i] *= transformation;
        }

        public void DeleteInvisible()
        {
            List<int[]> res_indices = new List<int[]>();
            List<Point3D> points = new List<Point3D>(this.points);
            for (int i = 0; i < nx - 1; ++i)
            {
                double y_max = double.MinValue;
                double y_min = double.MaxValue;
                bool first_it = true;
                for (int j = 0; j < i; ++j)
                {
                    var ind = indices[(nz - (2 + j)) * (nz - 1) + (i - j - 1)];
                    var p0 = points[ind[0]];
                    var p1 = points[ind[1]];
                    var p2 = points[ind[2]];
                    var p3 = points[ind[3]];
                    if ((p0.Y > y_max || p2.Y > y_max) &&(p1.Y > y_max || p3.Y > y_max))
                    {
                        for (int k = 0; k < 4; k++)
                        {
                            var temp = points[ind[k]];
                            if (temp.Y < y_min && !first_it)
                            {
                                points.Add(new Point3D(temp.X, y_min, temp.Z));
                                ind[k] = points.Count() - 1;
                            }
                        }
                        res_indices.Add(ind);
                        y_max = Math.Max(Math.Max(p0.Y,p1.Y),Math.Max(p2.Y,p3.Y));
                    }

                    if ((p0.Y < y_min || p2.Y < y_min) && (p1.Y < y_min || p3.Y < y_min))
                    {
                        for (int k = 0; k < 4; k++)
                        {
                            var temp = points[ind[k]];
                            if (temp.Y > y_max && !first_it)
                            {
                                points.Add(new Point3D(temp.X, y_max, temp.Z));
                                ind[k] = points.Count() - 1;
                            }
                        }
                        res_indices.Add(ind);
                        y_min = Math.Min(Math.Min(p0.Y, p1.Y), Math.Min(p2.Y, p3.Y));
                    }
                    first_it = false;
                }
            }


            for (int i = 0; i < nx - 1; ++i)
            {
                double y_max = double.MinValue;
                double y_min = double.MaxValue;
                bool first_it = true;
                for (int j = i; j >= 0; --j)
                {
                    var ind = indices[(j + 1) * nx - (i + 2)];
                    var p0 = points[ind[0]];
                    var p1 = points[ind[1]];
                    var p2 = points[ind[2]];
                    var p3 = points[ind[3]];
                    if ((p0.Y > y_max || p2.Y > y_max) && (p1.Y > y_max || p3.Y > y_max))
                    {
                        for (int k = 0; k < 4; k++)
                        {
                            var temp = points[ind[k]];
                            if (temp.Y < y_min && !first_it)
                            {
                                points.Add(new Point3D(temp.X, y_min, temp.Z));
                                ind[k] = points.Count() - 1;
                            }
                        }
                        res_indices.Add(ind);
                        y_max = Math.Max(Math.Max(p0.Y, p1.Y), Math.Max(p2.Y, p3.Y));
                    }
                    if ((p0.Y < y_min || p2.Y < y_min) && (p1.Y < y_min || p3.Y < y_min))
                    {
                        for (int k = 0; k < 4; k++)
                        {
                            var temp = points[ind[k]];
                            if (temp.Y > y_max && !first_it)
                            {
                                points.Add(new Point3D(temp.X, y_max, temp.Z));
                                ind[k] = points.Count() - 1;
                            }
                        }
                        res_indices.Add(ind);
                        y_min = Math.Min(Math.Min(p0.Y, p1.Y), Math.Min(p2.Y, p3.Y));
                    }
                    first_it = false;
                }
            }
            indices = res_indices.ToArray();
            this.points = points.ToArray();
        }
    }

    public class Camera
    {
        public Point3D Position { get; set; }
        public double AngleY { get; set; }
        public double AngleX { get; set; }
        public Matrix Projection { get; set; }

        public Point3D Forward { get { return new Point3D(0, 0, -1) * Athens.RotateX(AngleX) * Athens.RotateY(AngleY); } }
        public Point3D Left { get { return new Point3D(-1, 0, 0) * Athens.RotateX(AngleX) * Athens.RotateY(AngleY); } }
        public Point3D Up { get { return new Point3D(0, 1, 0) * Athens.RotateX(AngleX) * Athens.RotateY(AngleY); } }
        public Point3D Right { get { return -Left; } }
        public Point3D Backward { get { return -Forward; } }
        public Point3D Down { get { return -Up; } }

        public Matrix ViewProjection => Athens.Translate(-Position) * Athens.RotateY(-AngleY) * Athens.RotateX(-AngleX) * Projection;
        public Matrix DeleteProjection => Athens.Translate(-Position) * Athens.RotateY(-AngleY) * Athens.RotateX(-AngleX) * Athens.OrthogonalProjection();

        public Camera(Point3D position, double angleY, double angleX, Matrix projection)
        {
            Position = position;
            AngleY = angleY;
            AngleX = angleX;
            Projection = projection;
        }

        public Camera(Point3D position, Matrix projection)
        {
            Position = position;
            Projection = projection;
            AngleX = -Math.Atan(position.Y / Position.Z);
            AngleY = Math.Atan(position.X / Position.Z);
        }
    }
}
