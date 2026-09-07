using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SurfaceVisualizer
{
    internal class Matrix4x4
    {
        private double[,] _m = new double[4, 4];

        public static Matrix4x4 operator *(Matrix4x4 a, Matrix4x4 b) => a.Multiply(b);

        public Matrix4x4()
        {
            // Инициализация единичной матрицы
            for (int i = 0; i < 4; i++)
                _m[i, i] = 1.0;
        }

        public Matrix4x4(double[,] values)
        {
            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 4; j++)
                    _m[i, j] = values[i, j];
        }

        // Умножение матрицы на вектор-строку (1x4)
        public double[] Multiply(double[] vector)
        {
            double[] result = new double[4];
            for (int i = 0; i < 4; i++)
            {
                result[i] = 0;
                for (int j = 0; j < 4; j++)
                    result[i] += vector[j] * _m[j, i]; // строка * столбец
            }
            return result;
        }

        // Умножение матриц: this * right
        public Matrix4x4 Multiply(Matrix4x4 right)
        {
            double[,] res = new double[4, 4];
            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 4; j++)
                {
                    res[i, j] = 0;
                    for (int k = 0; k < 4; k++)
                        res[i, j] += _m[i, k] * right._m[k, j];
                }
            return new Matrix4x4(res);
        }

        // Создание матрицы поворота вокруг оси X (угол в радианах)
        public static Matrix4x4 RotateX(double angle)
        {
            double c = Math.Cos(angle), s = Math.Sin(angle);
            double[,] m = {
            {1, 0, 0, 0},
            {0, c, s, 0},
            {0, -s, c, 0},
            {0, 0, 0, 1}
        };
            return new Matrix4x4(m);
        }

        public static Matrix4x4 RotateY(double angle)
        {
            double c = Math.Cos(angle), s = Math.Sin(angle);
            double[,] m = {
            {c, 0, -s, 0},
            {0, 1, 0, 0},
            {s, 0, c, 0},
            {0, 0, 0, 1}
        };
            return new Matrix4x4(m);
        }

        public static Matrix4x4 RotateZ(double angle)
        {
            double c = Math.Cos(angle), s = Math.Sin(angle);
            double[,] m = {
            {c, s, 0, 0},
            {-s, c, 0, 0},
            {0, 0, 1, 0},
            {0, 0, 0, 1}
        };
            return new Matrix4x4(m);
        }
    }
}
