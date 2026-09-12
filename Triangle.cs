using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SurfaceVisualizer
{
    public class Triangle
    {
        public int A, B, C;    // индексы трёх вершин в списке _points
        public Color FillColor; // цвет заливки для flat-закраски

        public Triangle(int a, int b, int c)
        {
            A = a; B = b; C = c;
            FillColor = Color.Gray;
        }
    }
}