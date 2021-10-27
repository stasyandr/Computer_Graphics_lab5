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
    public partial class Form2 : Form
    {
        private Form1 main;
        private Graphics g;
        string name = "";        
        private Dictionary<char, string> rules = new Dictionary<char, string>();
        public Form2(Form1 form1)
        {
            main = form1;
            InitializeComponent();
            pictureBox1.Image = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            g = Graphics.FromImage(pictureBox1.Image);
            g.Clear(Color.White);
            pictureBox1.Invalidate();
        }
        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            this.Visible = false;
            main.Visible = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
                name = openFileDialog1.FileName;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            rules.Clear();
            g.Clear(Color.White);
            System.IO.StreamReader sReader = new System.IO.StreamReader(name);
            //Первая строка
            string[] first = sReader.ReadLine().Split(' ');
            string cur = first[0];
            float angle = (float)(float.Parse(first[1]) * Math.PI / 180.0);
            //Остальные строки
            while (!sReader.EndOfStream)
            {
                string s = sReader.ReadLine();
                rules.Add(s[0], s.Substring(2));
            }
            sReader.Close();
            for (int i = 0; i < numericUpDown1.Value; i++)
            {
                StringBuilder next = new StringBuilder("");
                foreach (char c in cur)
                {
                    // Если в наборе правил есть такой символ, то заменяем на его значение
                    if (rules.ContainsKey(c))
                        next.Append(rules[c]);
                    // Иначе просто добавляем символ
                    else
                        next.Append(c);
                }
                cur = next.ToString();
            }
            List<(PointF, PointF, float, Color)> points = new List<(PointF, PointF, float, Color)>();

            float length = 20;
            float current_angle = 0;
            if (first[2] == "left")
                current_angle = (float)Math.PI;
            else if (first[2] == "up")
                    current_angle = (float)(3 * Math.PI / 2.0);

            PointF curp = new PointF(0, 0);
            PointF nextp = new PointF(0, 0);

            Color color = Color.FromArgb(64, 0, 0);
            float width = name.Contains("tree") ? 10 : 2;


            points.Add((curp, nextp, width, color));

            Stack<(PointF,float)> br_stack = new Stack<(PointF, float)>();

            Random r = new Random(DateTime.Now.Millisecond);
            float d_angle = angle * 0.5f;
            float rnd = (float)(checkBox1.Checked ? r.NextDouble() : 0.5);
            foreach (char c in cur)
            {

                // Если встретили открывающую скобку, то запоминаем координаты точки и направление
                if (c == '[')
                {
                    br_stack.Push((curp, current_angle));
                }
                // Если встретили закрывающую скобку, то восстанавливаем значения
                else if (c == ']')
                {
                    (PointF, float) t = br_stack.Pop();
                    curp = t.Item1;
                    current_angle = t.Item2;
                }
                else if (c == 'F')
                {
                    float x_new = (float)(curp.X + length * Math.Cos(current_angle));
                    float y_new = (float)(curp.Y + length * Math.Sin(current_angle));
                    nextp = new PointF(x_new, y_new);
                    points.Add((curp, nextp, width, color));
                    curp = nextp;
                }
                else if (c == '-')
                {
                    rnd = (float)(checkBox1.Checked ? r.NextDouble() : 0.5);
                    current_angle -= (angle - d_angle) + 2 * d_angle * rnd;
                }
                else if (c == '+')
                {
                    rnd = (float)(checkBox1.Checked ? r.NextDouble() : 0.5);
                    current_angle += (angle - d_angle) + 2 * d_angle * rnd;
                }
                else if (c == '{')
                {
                    width--;
                    length--;
                    color = Color.FromArgb(color.R - 3, color.G + 17, color.B);
                }
                else if (c == '}')
                {
                    width++;
                    length++;
                    color = Color.FromArgb(color.R + 3, color.G - 17, color.B);
                }
            }
            float x_min = points.Min(point => Math.Min(point.Item1.X, point.Item2.X));
            float x_max = points.Max(point => Math.Max(point.Item1.X, point.Item2.X));
            float y_min = points.Min(point => Math.Min(point.Item1.Y, point.Item2.Y));
            float y_max = points.Max(point => Math.Max(point.Item1.Y, point.Item2.Y));

            PointF centerFractal = new PointF(x_min + (x_max - x_min) / 2.0f, y_min + (y_max - y_min) / 2.0f);
            PointF centerPictureBox = new PointF(pictureBox1.Width / 2.0f, pictureBox1.Height / 2.0f);
            float d = Math.Min(pictureBox1.Width / (x_max - x_min), pictureBox1.Height / (y_max - y_min));

            points = points.Select(point => scale(point, d, centerFractal, centerPictureBox)).ToList();


            Pen pen = new Pen(Color.DarkRed, 2);
            for (int i = 0; i < points.Count(); ++i)
            {
                g.DrawLine(new Pen(points[i].Item4, points[i].Item3), points[i].Item1, points[i].Item2);
                pictureBox1.Invalidate();
            }
        }
        private (PointF, PointF, float, Color) 
            scale((PointF, PointF, float, Color) points, float scale_factor, PointF centerFractal, PointF centerPictureBox)
        {
            return 
                (
                new PointF(
                centerPictureBox.X + (points.Item1.X - centerFractal.X) * scale_factor,
                centerPictureBox.Y + (points.Item1.Y - centerFractal.Y) * scale_factor),

                new PointF(
                centerPictureBox.X + (points.Item2.X - centerFractal.X) * scale_factor,
                centerPictureBox.Y + (points.Item2.Y - centerFractal.Y) * scale_factor),

                points.Item3,
                points.Item4
                );
        }
    }
}
