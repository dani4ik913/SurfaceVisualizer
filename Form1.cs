using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SurfaceVisualizer
{
    public partial class Form1 : Form
    {
        private Surface _surface;
        public Form1()
        {
            InitializeComponent();
            _surface = new Surface();

            // Подпишемся на события
            this.pictureBox1.Paint += PictureBox1_Paint;
            this.trackBarX.Scroll += TrackBar_Scroll;
            this.trackBarY.Scroll += TrackBar_Scroll;
            this.trackBarZ.Scroll += TrackBar_Scroll;

            // Первоначальный расчёт
            UpdateSurface();
        }
        private void TrackBar_Scroll(object sender, EventArgs e)
        {
            UpdateSurface();
        }

        private void UpdateSurface()
        {
            // Получаем углы из слайдеров (в градусах)
            double ax = trackBarX.Value;
            double ay = trackBarY.Value;
            double az = trackBarZ.Value;

            // Задаём поворот
            _surface.Rotate(ax, ay, az);

            // Вычисляем смещение для центрирования в PictureBox
            float centerX = pictureBox1.ClientSize.Width / 2f;
            float centerY = pictureBox1.ClientSize.Height / 2f;
            _surface.Transform(centerX, centerY);

            // Перерисовать
            pictureBox1.Invalidate();
        }

        private void PictureBox1_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.Clear(Color.White);
            _surface.Draw(e.Graphics);
        }

        // При изменении размеров PictureBox – пересчитать центрирование
        private void PictureBox1_Resize(object sender, EventArgs e)
        {
            UpdateSurface();
        }
    }
}
