using System;
using System.Collections.Generic;
using System.Drawing;

namespace SurfaceVisualizer
{
    public class Surface
    {
        // Параметры поверхности (управляются через ползунки)
        private double R = 1.0;
        private double r = 1.0;
        private double uLimit = 2 * Math.PI; // umax, umin = 0
        private double vLimit = Math.PI;     // vmax, vmin = 0
        private int uSegments = 20;
        private int vSegments = 20;

        private List<Point3D> _points;
        private List<int[]> _triangles;
        private List<Point3D> _rotatedPoints;
        private List<PointF> _screenPoints;

        private Matrix4x4 _rotationMatrix = new Matrix4x4();
        private float _offsetX, _offsetY, _scale = 100f;

        public Surface()
        {
            RebuildModel();
            Rotate(0, 0, 0);
            ApplyTransform();
        }

        // Установка всех параметров поверхности (вызывается из формы)
        public void SetParameters(int newUSeg, int newVSeg, double newULimit, double newVLimit, double newR, double newRr)
        {
            uSegments = newUSeg;
            vSegments = newVSeg;
            uLimit = newULimit;
            vLimit = newVLimit;
            R = newR;
            r = newRr;
            RebuildModel();
            ApplyTransform();
        }

        // Отдельные методы (на случай, если понадобятся)
        public void SetSegments(int uSeg, int vSeg)
        {
            uSegments = uSeg;
            vSegments = vSeg;
            RebuildModel();
            ApplyTransform();
        }

        public void SetLimits(double uLim, double vLim)
        {
            uLimit = uLim;
            vLimit = vLim;
            RebuildModel();
            ApplyTransform();
        }

        public void SetRadii(double newR, double newRr)
        {
            R = newR;
            r = newRr;
            RebuildModel();
            ApplyTransform();
        }

        private void RebuildModel()
        {
            _points = new List<Point3D>();
            _triangles = new List<int[]>();

            double du = uLimit / uSegments;
            double dv = vLimit / vSegments;

            for (int i = 0; i <= uSegments; i++)
            {
                double u = i * du;
                for (int j = 0; j <= vSegments; j++)
                {
                    double v = j * dv;
                    // Параметрические уравнения из методички
                    double x = r * Math.Cos(u) * Math.Sin(v);
                    double y = r * Math.Cos(v);
                    double z = R * Math.Sin(u) * Math.Sin(v);
                    _points.Add(new Point3D(x, y, z));
                }
            }

            for (int i = 0; i < uSegments; i++)
            {
                for (int j = 0; j < vSegments; j++)
                {
                    int p00 = i * (vSegments + 1) + j;
                    int p01 = p00 + 1;
                    int p10 = (i + 1) * (vSegments + 1) + j;
                    int p11 = p10 + 1;

                    _triangles.Add(new int[] { p00, p01, p10 });
                    _triangles.Add(new int[] { p11, p01, p10 });
                }
            }
        }

        private void ApplyTransform()
        {
            if (_points == null) return;

            _rotatedPoints = new List<Point3D>();
            foreach (var p in _points)
            {
                var vec = p.ToArray();
                var rotated = _rotationMatrix.Multiply(vec);
                _rotatedPoints.Add(new Point3D(rotated[0], rotated[1], rotated[2]));
            }

            _screenPoints = new List<PointF>();
            foreach (var p in _rotatedPoints)
            {
                float sx = (float)p.X * _scale + _offsetX;
                float sy = (float)p.Y * _scale + _offsetY;
                _screenPoints.Add(new PointF(sx, sy));
            }
        }

        public void Rotate(double angleXdeg, double angleYdeg, double angleZdeg)
        {
            double radX = angleXdeg * Math.PI / 180.0;
            double radY = angleYdeg * Math.PI / 180.0;
            double radZ = angleZdeg * Math.PI / 180.0;

            var mx = Matrix4x4.RotateX(radX);
            var my = Matrix4x4.RotateY(radY);
            var mz = Matrix4x4.RotateZ(radZ);
            _rotationMatrix = mx.Multiply(my).Multiply(mz);

            ApplyTransform();
        }

        public void SetTransform(float offsetX, float offsetY, float scale)
        {
            _offsetX = offsetX;
            _offsetY = offsetY;
            _scale = scale;
            ApplyTransform();
        }

        public void Draw(Graphics g)
        {
            if (_screenPoints == null || _triangles == null) return;
            using (Pen pen = new Pen(Color.Black, 2))
            {
                foreach (var tri in _triangles)
                {
                    PointF p1 = _screenPoints[tri[0]];
                    PointF p2 = _screenPoints[tri[1]];
                    PointF p3 = _screenPoints[tri[2]];
                    g.DrawPolygon(pen, new PointF[] { p1, p2, p3 });
                }
            }
        }
    }
}