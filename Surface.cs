using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

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
        private List<Triangle> _triangles;
        private List<Point3D> _rotatedPoints;
        private List<PointF> _screenPoints;
        private HashSet<(int, int)> _edges; // кеш уникальных рёбер

        // Цвета для flat-закраски
        public Color OutColor = Color.DarkRed;  // внешняя сторона
        public Color InColor = Color.Cyan;  // внутренняя сторона

        // Режим отрисовки: true = flat, false = каркас
        public bool IsFlatMode = false;

        private Matrix4x4 _rotationMatrix = new Matrix4x4();
        private float _offsetX, _offsetY, _scale = 100f;

        private int _angleX = 0, _angleY = 0, _angleZ = 0;

        public Surface()
        {
            RebuildModel();
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
            ReCountColor();
            
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
            _triangles = new List<Triangle>();

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

                    _triangles.Add(new Triangle(p00, p10, p01));
                    _triangles.Add(new Triangle(p10, p11, p01));
                }
            }

            // Перестроить кеш рёбер
            BuildEdgeCache();
        }

        // Строит множество уникальных рёбер на основе текущих треугольников
        private void BuildEdgeCache()
        {
            _edges = new HashSet<(int, int)>();
            if (_triangles == null) return;

            foreach (var tri in _triangles)
            {
                    AddEdgeToSet(_edges, tri.A, tri.B);
                    AddEdgeToSet(_edges, tri.B, tri.C);
                    AddEdgeToSet(_edges, tri.C, tri.A);
            }
        }

        // Вспомогательный метод для добавления ребра в HashSet с нормализацией порядка вершин
        private static void AddEdgeToSet(HashSet<(int, int)> edges, int a, int b)
        {
            if (a > b)
                (a, b) = (b, a);
            edges.Add((a, b));
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

        public void RotateX(int angle)
        {
            int delta = _angleX - angle;
            _rotationMatrix *= Matrix4x4.RotateX(delta * Math.PI / 180.0);
            ApplyTransform();
            _angleX = angle;
        }

        public void RotateY(int angle)
        {
            int delta = _angleY - angle;
            _rotationMatrix *= Matrix4x4.RotateY(delta * Math.PI / 180.0);
            ApplyTransform();
            _angleY = angle;
        }

        public void RotateZ(int angle)
        {
            int delta = _angleZ - angle;
            _rotationMatrix *= Matrix4x4.RotateZ(delta * Math.PI / 180.0);
            ApplyTransform();
            _angleZ = angle;
        }

        public void SetTransform(float offsetX, float offsetY, float scale)
        {
            _offsetX = offsetX;
            _offsetY = offsetY;
            _scale = scale;
            ApplyTransform();
        }

        public void ReCountColor()
        {
            if (_triangles == null || _rotatedPoints == null) return;

            foreach (Triangle tri in _triangles)
            {
                Point3D p0 = _rotatedPoints[tri.A];
                Point3D p1 = _rotatedPoints[tri.B];
                Point3D p2 = _rotatedPoints[tri.C];

                // Векторы двух рёбер
                double ax = p1.X - p0.X, ay = p1.Y - p0.Y, az = p1.Z - p0.Z;
                double bx = p2.X - p0.X, by = p2.Y - p0.Y, bz = p2.Z - p0.Z;

                // Векторное произведение (нормаль)
                double nx = ay * bz - az * by;
                double ny = az * bx - ax * bz;
                double nz = ax * by - ay * bx;

                double len = Math.Sqrt(nx * nx + ny * ny + nz * nz);
                if (len < 1e-12)
                {
                    tri.FillColor = Color.Black;
                    continue;
                }

                // Источник света и наблюдатель: L = (0, 0, 1)
                double cos = nz / len;

                // Внешняя или внутренняя сторона
                Color baseColor = cos >= 0 ? OutColor : InColor;
                double k = Math.Abs(cos);

                tri.FillColor = Color.FromArgb(
                    (int)Math.Min(255, baseColor.R * k),
                    (int)Math.Min(255, baseColor.G * k),
                    (int)Math.Min(255, baseColor.B * k)
                );
            }
        }

        // Новый метод Draw, рисующий только уникальные рёбра
        public void Draw(Graphics g)
        {
            if (_screenPoints == null) return;

            if (IsFlatMode)
            {
                // ----- Flat-закраска -----
                if (_triangles == null) return;

                foreach (Triangle tri in _triangles)
                {
                    PointF[] pts = new PointF[3]
                    {
                        _screenPoints[tri.A],
                        _screenPoints[tri.B],
                        _screenPoints[tri.C]
                    };

                    using (SolidBrush brush = new SolidBrush(tri.FillColor))
                    {
                        g.FillPolygon(brush, pts);
                    }
                }
            }
            else
            {
                // ----- Каркас -----
                if (_edges == null) return;

                using (Pen pen = new Pen(Color.Black, 2))
                {
                    pen.LineJoin = LineJoin.Round;

                    foreach (var (a, b) in _edges)
                    {
                        g.DrawLine(pen, _screenPoints[a], _screenPoints[b]);
                    }
                }
            }
        }
    }
}