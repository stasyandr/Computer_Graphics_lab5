using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Comp_Graf5
{
    public partial class Form4 : Form
    {
        private Form1 main;
        private List<PointF> points;
        private Bitmap bitmap;

        private float[,] BezierMatrix = { { -1,  3, -3,  1 },
                                          {  3, -6,  3,  0 },
                                          { -3,  3,  0,  0 },
                                          {  1,  0,  0,  0 }};

        private PointF additionalPoint;
        private int movingPointIndex;

        private float[,] multMatrices(float[,] matr1, float[,] matr2)
        {
            float[,] matr = new float[matr1.GetLength(0), matr2.GetLength(1)];

            for (int i = 0; i < matr1.GetLength(0); ++i)
            {
                for (int j = 0; j < matr2.GetLength(1); ++j)
                {
                    for (int k = 0; k < matr2.GetLength(0); k++)
                    {
                        matr[i, j] += matr1[i, k] * matr2[k, j];
                    }
                }
            }
            return matr;
        }

        public Form4(Form1 form1)
        {
            main = form1;
            InitializeComponent();
            points = new List<PointF>();
            radioButton1.Checked = true;
            bitmap = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            pictureBox1.Image = bitmap;
            movingPointIndex = -1;
            additionalPoint = new PointF();
        }
        private void Form4_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }
        private void button2_Click(object sender, EventArgs e)
        {
            points.Clear();
            bitmap = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            pictureBox1.Image = bitmap;
            radioButton1.Checked = true;
            movingPointIndex = -1;
            additionalPoint = new PointF();
        }
        private void button5_Click(object sender, EventArgs e)
        {
            this.Visible = false;
            main.Visible = true;
        }
        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            if (radioButton1.Checked)
            {
                points.Add(e.Location);
            }
            if (radioButton2.Checked)
            {
                RemovePoint(e.Location);
            }
            Redraw();
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (radioButton3.Checked)
            {
                movingPointIndex = points.FindIndex(el => (el.X > e.X - 3) && (el.X < e.X + 3) && (el.Y > e.Y - 3) && (el.Y < e.Y + 3));
            }
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            if (radioButton3.Checked)
            {
                if (movingPointIndex >= 0)
                {
                    points[movingPointIndex] = new PointF(e.X, e.Y);
                    RemovePoint(additionalPoint);
                    Redraw();
                    movingPointIndex = -1;
                }
            }
        }

        private void RemovePoint(PointF point)
        {
            int findIndex = points.FindIndex(el => (el.X > point.X - 3) && (el.X < point.X + 3) && (el.Y > point.Y - 3) && (el.Y < point.Y + 3));
            if (findIndex >= 0)
            {
                points.RemoveAt(findIndex);
                bitmap = new Bitmap(pictureBox1.Width, pictureBox1.Height);
                Redraw();
            }
        }

        private void Redraw()
        {
            bitmap = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            DrawPoints();
            DrawBezierCurve();
            pictureBox1.Image = bitmap;
        }

        private void DrawPoints()
        {
            SolidBrush solidBrush = new SolidBrush(Color.Blue);
            Graphics g = Graphics.FromImage(bitmap);
            Pen pen = new Pen(Color.Blue);
            foreach (var point in points)
            {
                if (point != additionalPoint)
                {
                    g.DrawEllipse(pen, point.X - 2, point.Y - 2, 4, 4);
                    g.FillEllipse(solidBrush, point.X - 2, point.Y - 2, 4, 4);
                }
            }
            pictureBox1.Image = bitmap;
        }

        private PointF GetNextCurvePixel(PointF p0, PointF p1, PointF p2, PointF p3, float t)
        {
            float[,] matrX = { { p0.X }, { p1.X }, { p2.X }, { p3.X } };
            float[,] matrY = { { p0.Y }, { p1.Y }, { p2.Y }, { p3.Y } };

            float[,] matrParams = { { t * t * t, t * t, t, 1 } };

            float X = multMatrices(multMatrices(matrParams, BezierMatrix), matrX)[0, 0];
            float Y = multMatrices(multMatrices(matrParams, BezierMatrix), matrY)[0, 0];

            return new PointF(X, Y);
        }

        private PointF GetMidPoint(PointF point1, PointF point2)
        {
            float x = (point1.X + point2.X) / 2;
            float y = (point1.Y + point2.Y) / 2;
            return new PointF(x, y);
        }

        private void DrawCurveFor4Points(PointF p0, PointF p1, PointF p2, PointF p3)
        {
            float t = 0.0F;
            while (t <= 1.0)
            {
                var pixel = GetNextCurvePixel(p0, p1, p2, p3, t);
                bitmap.SetPixel((int)pixel.X, (int)pixel.Y, Color.Black);
                t += 0.0001F;
            }
            pictureBox1.Image = bitmap;
        }
        private void AddOrRemovePoint()
        {
            if (additionalPoint.IsEmpty)
            {
                additionalPoint = GetMidPoint(points[points.Count - 2], points[points.Count - 1]);
                points.Add(points[points.Count - 1]);
                points[points.Count - 2] = additionalPoint;
            }
            else
            {
                RemovePoint(additionalPoint);
                additionalPoint = new PointF();
            }
        }
        private void DrawCurveForMoreThan4Points()
        {
            int count = points.Count();
            PointF point0 = points[0];
            PointF point1 = points[1];
            PointF point2 = points[2];
            PointF point3 = GetMidPoint(points[2], points[3]);
            DrawCurveFor4Points(point0, point1, point2, point3);

            var index = 3;
            while (index < count - 4)
            {
                point0 = point3;
                point1 = points[index];
                point2 = points[index + 1];
                point3 = GetMidPoint(points[index + 1], points[index + 2]);
                DrawCurveFor4Points(point0, point1, point2, point3);
                index += 2;
            }

            point0 = point3;
            point1 = points[count - 3];
            point2 = points[count - 2];
            point3 = points[count - 1];
            DrawCurveFor4Points(point0, point1, point2, point3);
        }

        private void DrawBezierCurve()
        {
            int length = points.Count();
            if (length == 4)
            {
                DrawCurveFor4Points(points[0], points[1], points[2], points[3]);
            }
            if (length > 4)
            {
                if (length % 2 != 0)
                {
                    AddOrRemovePoint();
                }
                DrawCurveForMoreThan4Points();
            }
        }
    }
}
