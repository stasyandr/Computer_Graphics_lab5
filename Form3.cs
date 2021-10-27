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
    public partial class Form3 : Form
    {
        private class Edge
        {
            public PointF left;
            public PointF right;

            public Edge(PointF _left, PointF _right)
            {
                left = _left;
                right = _right;
            }
        }

        Bitmap bitmap;
        Graphics g;
        List<Edge> edges = new List<Edge>();
        Random rnd = new Random();
        double roughness;
        private Form1 main;
        public Form3(Form1 form1)
        {
            main = form1;
            InitializeComponent();
            bitmap = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            pictureBox1.Image = bitmap;
            g = Graphics.FromImage(bitmap);
            textBox1.Text = (pictureBox1.Height / 2).ToString();
            textBox2.Text = (pictureBox1.Height / 2).ToString();
        }
        private void Form3_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            this.Visible = false;
            main.Visible = true;
        }
        private void Make_Step(object sender, EventArgs e)
        {
            if (edges.Count == 0)
            {
                double lbound;
                double rbound;
                if (!Double.TryParse(textBox1.Text, out lbound))
                {
                    lbound = 300;
                };
                if (!Double.TryParse(textBox2.Text, out rbound))
                {
                    rbound = 300;
                };
                if (!Double.TryParse(textBox3.Text, out roughness))
                {
                    roughness = -0.8;
                }
                textBox1.Enabled = false;
                textBox2.Enabled = false;
                textBox3.Enabled = false;
                Edge first = new Edge(new PointF(0, (float)(bitmap.Height - lbound)), new PointF((float)(bitmap.Width),
                    (float)(bitmap.Height - rbound)));
                edges.Add(first);
                DrawEdges();
            }
            else
            {
                List<Edge> list_edge = new List<Edge>();
                foreach (Edge ed in edges)
                {
                    double length = Math.Sqrt(Math.Pow((ed.right.X - ed.left.X), 2) + Math.Pow((ed.right.Y - ed.left.Y), 2));
                    double new_h = (ed.left.Y + ed.right.Y) / 2 + (rnd.NextDouble() - 0.5) * length * roughness;
                    PointF mid = new PointF((ed.left.X + ed.right.X) / 2, (int)new_h);
                    list_edge.Add(new Edge(ed.left, mid));
                    list_edge.Add(new Edge(mid, ed.right));
                }
                edges = list_edge;
                DrawEdges();

            }
        }

        private void DrawEdges()
        {
            bitmap = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            pictureBox1.Image = bitmap;
            g = Graphics.FromImage(bitmap);
            foreach (Edge edge1 in edges)
            {
                drawEdge(edge1);
            }
            pictureBox1.Invalidate();
        }

        private void Clear_Click(object sender, EventArgs e)
        {
            textBox1.Enabled = true;
            textBox2.Enabled = true;
            textBox3.Enabled = true;
            edges = new List<Edge>();
            bitmap = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            pictureBox1.Image = bitmap;
            roughness = 0;
        }

        private void drawEdge(Edge edge)
        {
            g.DrawLine(Pens.Black, edge.left, edge.right);
        }
    }
}
