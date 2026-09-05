using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SurfaceVisualizer
{
    internal class Surface
    {
        private List<Point3D> _points;              // исходные точки (3D)
        private List<Point3D> _rotatedPoints;       // после поворота
        private List<PointF> _screenPoints;         // после проецирования и сдвига
        private List<int[]> _triangles;             // индексы вершин (по 3)

        private Matrix4x4 _rotationMatrix = new Matrix4x4(); // текущая матрица поворота

        // Параметры поверхности (можно будет менять позже)
        private double R = 52.0, r = 52.0;
        private double uMin = 0, uMax = 2 * Math.PI;
        private double vMin = 0, vMax = Math.PI;
        private int uSegments = 20, vSegments = 20;

        // Сдвиг для центрирования на экране
        private float _offsetX, _offsetY;

        public Surface()
        {
            // При создании сразу строим модель
            BuildModel();
            // Поворот пока единичный
            Rotate(0, 0, 0);
        }

        // Построение сетки точек и треугольников по параметрическим уравнениям
        private void BuildModel()
        {
            _points = new List<Point3D>();
            _triangles = new List<int[]>();

            double du = (uMax - uMin) / uSegments;
            double dv = (vMax - vMin) / vSegments;

            // Заполняем точки (сетка размером (uSegments+1) x (vSegments+1))
            for (int i = 0; i <= uSegments; i++)
            {
                double u = uMin + i * du;
                for (int j = 0; j <= vSegments; j++)
                {
                    double v = vMin + j * dv;
                    // Параметрические уравнения из методички
                    double x = r * Math.Cos(u) * Math.Sin(v);
                    double y = r * Math.Cos(v);
                    double z = R * Math.Sin(u) * Math.Sin(v);
                    _points.Add(new Point3D(x, y, z));
                }
            }

            // Формируем треугольники (по два на каждый четырёхугольный сегмент)
            for (int i = 0; i < uSegments; i++)
            {
                for (int j = 0; j < vSegments; j++)
                {
                    int p00 = i * (vSegments + 1) + j;
                    int p01 = p00 + 1;
                    int p10 = (i + 1) * (vSegments + 1) + j;
                    int p11 = p10 + 1;

                    // Первый треугольник (p00, p01, p10)
                    _triangles.Add(new int[] { p00, p01, p10 });
                    // Второй треугольник (p11, p01, p10) - порядок обхода для согласованности нормалей (не критично для каркаса)
                    _triangles.Add(new int[] { p11, p01, p10 });
                }
            }
        }

        // Применить матрицу поворота ко всем точкам, спроецировать на XOY и сдвинуть
        public void Transform(float offsetX, float offsetY)
        {
            _offsetX = offsetX;
            _offsetY = offsetY;

            _rotatedPoints = new List<Point3D>();
            foreach (var p in _points)
            {
                var vec = p.ToArray();
                var rotated = _rotationMatrix.Multiply(vec);
                _rotatedPoints.Add(new Point3D(rotated[0], rotated[1], rotated[2]));
            }

            // Проецируем на плоскость XOY (отбрасываем Z) и сдвигаем
            _screenPoints = new List<PointF>();
            foreach (var p in _rotatedPoints)
            {
                // Масштабирование можно добавить, но пока просто сдвиг
                float sx = (float)p.X + offsetX;
                float sy = (float)p.Y + offsetY;
                _screenPoints.Add(new PointF(sx, sy));
            }
        }

        // Установить углы поворота (в градусах) и пересчитать матрицу
        public void Rotate(double angleXdeg, double angleYdeg, double angleZdeg)
        {
            // Переводим в радианы
            double radX = angleXdeg * Math.PI / 180.0;
            double radY = angleYdeg * Math.PI / 180.0;
            double radZ = angleZdeg * Math.PI / 180.0;

            // Строим матрицу поворота: сначала X, затем Y, затем Z (как в методичке)
            var mx = Matrix4x4.RotateX(radX);
            var my = Matrix4x4.RotateY(radY);
            var mz = Matrix4x4.RotateZ(radZ);
            _rotationMatrix = mx.Multiply(my).Multiply(mz);
        }

        // Отрисовка каркаса (wireframe)
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

        // Доступ к параметрам для возможного изменения (позже)
        public void SetSegments(int uSeg, int vSeg)
        {
            uSegments = uSeg; vSegments = vSeg;
            BuildModel();
        }

        public void SetRadii(double newR, double newr)
        {
            R = newR; r = newr;
            BuildModel();
        }
    }
}
