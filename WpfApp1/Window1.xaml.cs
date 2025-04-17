
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
namespace WpfApp1
{
    /// <summary>
    /// Логика взаимодействия для Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        Dictionary<double, double> spline(double[] x, double[] y)
        {
            double key = 0.1;
            double[] b = new double[y.Length];
            double[] B = new double[y.Length];
            double[] h = new double[y.Length];
            double[] a = new double[y.Length];
            double[] c = new double[y.Length];
            double b0 = (y[1] - y[0]) / (x[1] - x[0]);
            Dictionary<double, double> itog = new Dictionary<double, double>();
            for (int i = 0; i < y.Length - 1; i++)
            {
                if (i > 0)
                {
                    h[i] = x[i + 1] - x[i];
                    B[i] = 2 * a[i - 1] * x[i] + b[i - 1];
                    a[i] = 1 / (h[i] * h[i]) * (y[i + 1] - y[i]) - B[i] / h[i];
                    b[i] = B[i] - 2 * a[i] * x[i];
                    c[i] = y[i] - B[i] * x[i] + a[i] * x[i] * x[i];
                }
                else
                {
                    h[i] = x[i + 1] - x[i];
                    a[i] = (1 / (h[i] * h[i])) * (y[i + 1] - y[i]) - b0 / h[i];
                    b[i] = b0 - 2 * a[i] * x[i];
                    c[i] = y[i] - b0 * x[i] + a[i] * x[i] * x[i];
                }
            }
            for (int i = 0; i < y.Length - 1; i++)
            {
                for (double j = x[i] + key; j <= x[i + 1]; j += key)
                {
                    itog.Add(j, j * j * a[i] + j * b[i] + c[i]);
                }
            }
            return itog;
        }
        double find_y(double x2, double[] x, double[] y)
        {
            double key = 0.1;
            double[] b = new double[y.Length];
            double[] B = new double[y.Length];
            double[] h = new double[y.Length];
            double[] a = new double[y.Length];
            double[] c = new double[y.Length];
            double b0 = (y[1] - y[0]) / (x[1] - x[0]);
            Dictionary<double, double> itog = new Dictionary<double, double>();
            for (int i = 0; i < y.Length - 1; i++)
            {
                if (i > 0)
                {
                    h[i] = x[i + 1] - x[i];
                    B[i] = 2 * a[i - 1] * x[i] + b[i - 1];
                    a[i] = 1 / (h[i] * h[i]) * (y[i + 1] - y[i]) - B[i] / h[i];
                    b[i] = B[i] - 2 * a[i] * x[i];
                    c[i] = y[i] - B[i] * x[i] + a[i] * x[i] * x[i];
                }
                else
                {
                    h[i] = x[i + 1] - x[i];
                    a[i] = (1 / (h[i] * h[i])) * (y[i + 1] - y[i]) - b0 / h[i];
                    b[i] = b0 - 2 * a[i] * x[i];
                    c[i] = y[i] - b0 * x[i] + a[i] * x[i] * x[i];
                }
            }
            int index = 0;
            for (int i = 1; i < x.Length; i++)
            {
                if (x2 <= x[i])
                {
                    index = i - 1;
                    break;
                }
            }
            return x2 * x2 * a[index] + x2 * b[index] + c[index];
        }
        public Window1()
        {
            InitializeComponent();
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            List<double> dataX = new List<double>();
            List<double> dataY = new List<double>();
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            Coordinates.Text = "X Y";
            openFileDialog.Filter = "Текстовые файлы (*.txt)|*.txt"; // "|*.txt" - так тоже можно,
                                                                     // часть до | для красоты
            openFileDialog.ShowDialog();
            try
            {
                string[] lines = File.ReadAllLines(openFileDialog.FileName);
                foreach (string line in lines)
                {
                    string[] parts = line.Split(' ');
                    if (double.TryParse(parts[0], out double x) && double.TryParse(parts[1], out double y))
                    {
                        dataX.Add(x);
                        dataY.Add(y);
                        Coordinates.Text += $"\n{x}, {y}";
                    } 


                }
                Dictionary<double, double> tochki = spline(dataX.ToArray(), dataY.ToArray());
                WpfPlot1.Plot.Clear();
                WpfPlot1.Plot.Add.Scatter(tochki.Keys.ToArray(), tochki.Values.ToArray());
                WpfPlot1.Refresh();
                double xmax = 0.0, ymax = 0.0;
                double xmin = 9999999, ymin = 99999;
                for (double x3 = dataX.ToArray()[0]; x3 <= dataX.ToArray()[dataX.ToArray().Length - 1]; x3 += 1e-1)
                {
                    double y3 = find_y(x3, dataX.ToArray(), dataY.ToArray());
                    if (ymax < y3)
                    {
                        xmax = x3;
                        ymax = y3;
                    }
                    if (ymin>y3)
                    {
                        xmin = x3;
                        ymin = y3;
                    }
                }
                Max_Min.Text = $"Метод дихотомии:\nmax_x={xmax}, max_y={ymax}\nx_min={xmin}, y_min={ymin}";
                double a = dataX[0];
                double b = dataX[dataX.ToArray().Length - 1];
                double eps = .001;
                double x1 = 0;
                double x2 = 0;
                double y1 = 0;
                double y2 = 0;
                double x4 = 0;
                while (true) {
                    x1 = b - (b - a) / 1.618;
                    x2 = a + (b - a) / 1.618;
                    y1 = find_y(x1, dataX.ToArray(), dataY.ToArray());
                    y2 = find_y(x2, dataX.ToArray(), dataY.ToArray());
                    if (y1 >= y2) {
                        a = x1;
                    }
                    else
                    {
                        b = x2; 
                    }
                     if (Math.Abs(b - a) < eps){
                        x4 = (a + b) / 2;
                        break;
                    }
                }
                Max_Min.Text += $"\nx_min={x4}, y_min={find_y(x4, dataX.ToArray(), dataY.ToArray())}";
            }
            catch(Exception ex)
            {
                ///ну  тип решил не выбирать файл
            }

        }
     }
}
