using System;
using System.Drawing;
using System.Windows.Forms;

namespace SurfaceVisualizer
{
    public partial class Form1 : Form
    {
        private Surface _surface;
        private double _angleX, _angleY, _angleZ;
        private float _scale = 150f; // <-- масштаб объявлен здесь

        public Form1()
        {
            InitializeComponent();
            // Устанавливаем начальные значения ползунков
            trackBarN1.Value = 20;
            trackBarN2.Value = 20;
            trackBarUlimit.Value = 360;   // 2π
            trackBarVlimit.Value = 180;   // π
            trackBarR.Value = 10;         // даёт R = 1.0 (делим на 10)
            trackBar3.Value = 10;         // даёт r = 1.0

            trackBarX.Value = 0;
            trackBarY.Value = 0;
            trackBarZ.Value = 0;
            _surface = new Surface();

            pictureBox1.Paint += PictureBox1_Paint;
            pictureBox1.Resize += PictureBox1_Resize;

            trackBarX.Scroll += (s, e) => UpdateRotation();
            trackBarY.Scroll += (s, e) => UpdateRotation();
            trackBarZ.Scroll += (s, e) => UpdateRotation();

            trackBarN1.Scroll += (s, e) => UpdateSurfaceParams();
            trackBarN2.Scroll += (s, e) => UpdateSurfaceParams();
            trackBarUlimit.Scroll += (s, e) => UpdateSurfaceParams();
            trackBarVlimit.Scroll += (s, e) => UpdateSurfaceParams();
            trackBarR.Scroll += (s, e) => UpdateSurfaceParams();
            trackBar3.Scroll += (s, e) => UpdateSurfaceParams();

            UpdateSurfaceParams();
            UpdateRotation();
        }

        private void UpdateRotation()
        {
            _angleX = trackBarX.Value;
            _angleY = trackBarY.Value;
            _angleZ = trackBarZ.Value;
            _surface.Rotate(_angleX, _angleY, _angleZ);
            pictureBox1.Invalidate();
        }

        private void UpdateSurfaceParams()
        {
            int n1 = trackBarN1.Value;
            int n2 = trackBarN2.Value;

            double uLimit = trackBarUlimit.Value * Math.PI / 180.0;
            double vLimit = trackBarVlimit.Value * Math.PI / 180.0;
            double R = trackBarR.Value / 10.0;
            double r = trackBar3.Value / 10.0;

            if (uLimit < 0.01) uLimit = 0.01;
            if (vLimit < 0.01) vLimit = 0.01;

            _surface.SetParameters(n1, n2, uLimit, vLimit, R, r);
            _surface.Rotate(_angleX, _angleY, _angleZ);

            float centerX = pictureBox1.ClientSize.Width / 2f;
            float centerY = pictureBox1.ClientSize.Height / 2f;
            _surface.SetTransform(centerX, centerY, _scale); // используется _scale

            pictureBox1.Invalidate();
        }

        private void PictureBox1_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.Clear(Color.White);
            _surface.Draw(e.Graphics);
        }

        private void PictureBox1_Resize(object sender, EventArgs e)
        {
            float centerX = pictureBox1.ClientSize.Width / 2f;
            float centerY = pictureBox1.ClientSize.Height / 2f;
            _surface.SetTransform(centerX, centerY, _scale);
            pictureBox1.Invalidate();
        }
    }
}