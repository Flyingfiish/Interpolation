using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ZedGraph;

namespace PolynomialInterpolation
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            DrawGraph();
        }

        private void DrawGraph()
        {
            GraphPane pane = zedGraph.GraphPane;

            pane.Title.Text = "Интерполяция";

            pane.CurveList.Clear();

            PointPairList list = new PointPairList();

            double xmin = 0.8;
            double xmax = 2.2;

            var xValues = new double[] { 1.0, 1.2, 1.4, 1.6, 1.8, 2.0 };
            var yValues = new double[] { 1.2, 2.0, 3.2, 4.2, 4.8, 6.1 };
            int n = 5;
            GetSplines(xValues, yValues, n);

            for (double x = xmin; x <= xmax; x += 0.001)
            {
                list.Add(x, Interpolate(x));
            }

            pane.AddCurve("Lagrange", list, Color.Blue, SymbolType.None);

            zedGraph.AxisChange();

            zedGraph.Invalidate();
        }

        public static void GetSplines(double[] x, double[] y, int n)
        {
            splines = new Spline[n + 1];
            for (int i = 0; i < n + 1; ++i)
            {
                splines[i].x = x[i];
                splines[i].a = y[i];
            }
            splines[1].c = splines[n].c = 0.0;

            var alpha = new double[n + 1];
            var beta = new double[n + 1];
            var h = new double[n + 1];
            for (int i = 1; i < n + 1; ++i)
            {
                h[i] = x[i] - x[i - 1];
            }
            for (int i = 2; i < n + 1; i++)
            {
                double a = h[i - 1];
                double b = 2.0 * (h[i - 1] + h[i]);
                double c = h[i];
                double d = 3.0 * ((y[i] - y[i - 1]) / h[i] - (y[i - 1] - y[i - 2]) / h[i - 1]);
                double z = (a * alpha[i - 1] + b);
                alpha[i] = -c / z;
                beta[i] = (d - a * beta[i - 1]) / z;
            }
            alpha[n] = 0;
            splines[n].c = beta[n];

            for (int i = n - 1; i > 0; --i)
            {
                splines[i].c = alpha[i] * splines[i + 1].c + beta[i];
            }

            for (int i = n; i > 0; --i)
            {
                splines[i].d = (splines[i].c - splines[i - 1].c) / (3 * h[i]);
                splines[i].b = h[i] * (2.0 * splines[i].c + splines[i - 1].c) / 3.0 + (y[i] - y[i - 1]) / h[i];
            }
        }



        public static double Interpolate(double x)
        {
            if (splines == null)
            {
                return double.NaN;
            }

            var iterator = 0;
            int n = splines.Length;

            if (x <= splines[0].x)
            {
                iterator = 1;
            }
            else if (x >= splines[n - 1].x)
            {
                iterator = n - 1;
            }
            else
            {
                int i = 0;
                int j = n - 1;
                while (i + 1 < j)
                {
                    int k = i + (j - i) / 2;
                    if (x <= splines[k].x)
                    {
                        j = k;
                    }
                    else
                    {
                        i = k;
                    }
                }
                iterator = j;
            }

            return splines[iterator].a +
            splines[iterator].b * (x - splines[iterator].x) +
            splines[iterator].c * (x - splines[iterator].x) * (x - splines[iterator].x) +
            splines[iterator].d * (x - splines[iterator].x) * (x - splines[iterator].x) * (x - splines[iterator].x);
        }

        private struct Spline
        {
            public double a, b, c, d, x;
        }
        static Spline[] splines;
    }
}
