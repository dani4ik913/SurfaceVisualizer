using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SurfaceVisualizer
{
    internal class Point3D
    {
            public double X { get; set; }
            public double Y { get; set; }
            public double Z { get; set; }

            public Point3D(double x, double y, double z)
            {
                X = x; Y = y; Z = z;
            }

            // Преобразование в однородный вектор-строку (для умножения на матрицу)
            public double[] ToArray() => new double[] { X, Y, Z, 1.0 };
    }
}
