using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace floating_horyzon
{
    public partial class Form1 : Form
    {
        public Mesh mesh;
        public Bitmap bmp;
        public Graphics g;
        private Camera camera;
        private double step = 0.015;

        public Form1()
        {
            InitializeComponent();
            bmp = new Bitmap(pb.Width, pb.Height);
            pb.Image = bmp;
            g = Graphics.FromImage(bmp);
            Matrix projection = Athens.PerspectiveProjection(-0.1, 0.1, -0.1, 0.1, 0.1, 10);
            //Matrix projection = Athens.OrthogonalProjection();
            camera = new Camera(new Point3D(2, 2, 2), projection);
            mesh = Plot.GetMesh(camera,-1, 1, step, -1, 1, step);
            //camera = new Camera(new Point3D(1, 0.5, 1), Math.PI / 4, -Math.Atan(1 / Math.Sqrt(3)), projection);

        }

        /// <summary>
        /// Обработка движений камеры
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="keyData"></param>
        /// <returns></returns>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            double delta = 0.15;
            switch (keyData)
            {
                //case Keys.W: camera.Position *= Athens.Translate(0.1 * camera.Forward); break;
                case Keys.Left: camera.Position *= Athens.Translate(0.1 * camera.Left); break;
                //case Keys.S: camera.Position *= Athens.Translate(0.1 * camera.Backward); break;
                case Keys.Right: camera.Position *= Athens.Translate(0.1 * camera.Right); break;
                //case Keys.Left: camera.AngleY += delta; break;
                //case Keys.Right: camera.AngleY -= delta; break;
                case Keys.Up: camera.AngleX += delta; break;
                case Keys.Down: camera.AngleX -= delta; break;
                case Keys.E:
                    camera = new Camera(new Point3D(camera.Position.X, camera.Position.Y + 0.2, camera.Position.Z),
           camera.AngleX, camera.AngleY, camera.Projection);
                    camera = new Camera(new Point3D(camera.Position.X, camera.Position.Y + 0.2, camera.Position.Z),
           camera.AngleX, camera.AngleY, camera.Projection); break;
                case Keys.Q:
                    camera = new Camera(new Point3D(camera.Position.X, camera.Position.Y - 0.2, camera.Position.Z),
           camera.AngleX, camera.AngleY, camera.Projection);
                    camera = new Camera(new Point3D(camera.Position.X, camera.Position.Y - 0.2, camera.Position.Z),
           camera.AngleX, camera.AngleY, camera.Projection); break;
            }
            mesh = Plot.GetMesh(camera ,-1, 1, step, -1, 1, step);
            DrawScene();
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DrawScene();
        }
        //Отрисовка сцены
        private void DrawScene()
        {
            g.Clear(Color.White);

            mesh.DeleteInvisible();

            //foreach (var facet in mesh.indices)
            //    for (int i = 0; i < facet.Length; ++i)
            //    {
            //        var a = mesh.points[facet[i]];
            //        var b = mesh.points[facet[(i + 1) % facet.Length]];
            //        DrawLine(a, b, Color.Black);
            //    }
            foreach (var facet in mesh.indices)
            {
                DrawLine(mesh.points[facet[0]], mesh.points[facet[1]], Color.Black);
                DrawLine(mesh.points[facet[2]], mesh.points[facet[3]], Color.Black);
            }
            pb.Refresh();
        }

        private Point3D SpaceToClip(Point3D v)
        {
            return v * camera.ViewProjection;
        }

        private Point3D NormilizeToScreen(Point3D v)
        {
            return new Point3D(
                (v.X / v.W + 1) / 2 * (Width*0.8),
                (-v.Y / v.W + 1) / 2 * (Height*0.8),
                v.Z / v.W);
        }

        private Point3D Normilize(Point3D v) => new Point3D(
                (v.X / v.W + 1) / 2 * Width,
                (-v.Y / v.W + 1) / 2 * Height,
                v.Z / v.W);

        public void DrawLine(Point3D a, Point3D b, Color col)
        {
            //a = SpaceToClip(a);
            //b = SpaceToClip(b);
            a = NormilizeToScreen(a);
            b = NormilizeToScreen(b);
            int x0 = (int)a.X;
            int y0 = (int)a.Y;
            int x1 = (int)b.X;
            int y1 = (int)b.Y;

            g.DrawLine(new Pen(col), new Point(x0, y0), new Point(x1, y1));
        }
    }
}
