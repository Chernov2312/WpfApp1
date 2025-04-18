﻿
using Microsoft.CodeAnalysis.CSharp.Scripting;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
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
using IronPython.Hosting;
using Microsoft.Scripting.Hosting;
namespace WpfApp1
{
    /// <summary>
    /// Логика взаимодействия для Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        public int grafics = 0;
        Dictionary<double, double> spline(double[] x, double[] y)
        {
            double key = 0.02;
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
        public void max_min(double[] dataX, double[] dataY)
        {
            double xmax = 0.0, ymax = 0.0;
            double xmin = 9999999, ymin = 99999;
            for (double x_dihotom = dataX[0]; x_dihotom <= dataX.ToArray()[dataX.Length - 1]; x_dihotom += 1e-1)
            {
                double y3 = find_y(x_dihotom, dataX, dataY);
                if (ymax < y3)
                {
                    xmax = x_dihotom;
                    ymax = y3;
                }
                if (ymin > y3)
                {
                    xmin = x_dihotom;
                    ymin = y3;
                }
            }
            Max_Min.Text += $"\n№{grafics}\n";
            Max_Min.Text += $"Метод дихотомии:\nmax_x={xmax}, max_y={ymax}\nx_min={xmin}, y_min={ymin}";
            double a = dataX[0];
            double b = dataX[dataX.ToArray().Length - 1];
            double eps = .001;
            double x1 = 0;
            double x2 = 0;
            double y1 = 0;
            double y2 = 0;
            double x4 = 0;
            while (true)
            {
                x1 = b - (b - a) / 1.618;
                x2 = a + (b - a) / 1.618;
                y1 = find_y(x1, dataX, dataY);
                y2 = find_y(x2, dataX, dataY);
                if (y1 >= y2)
                {
                    a = x1;
                }
                else
                {
                    b = x2;
                }
                if (Math.Abs(b - a) < eps)
                {
                    x4 = (a + b) / 2;
                    break;
                }
            }
            Max_Min.Text += "\nМетод градиентного спуска";
            Max_Min.Text += $"\nx_min={x4}, y_min={find_y(x4, dataX, dataY)}";
        }
        public Window1()
        {
            InitializeComponent();
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            
            WpfPlot1.Plot.Clear();
            WpfPlot1.Refresh();
            Max_Min.Text = "";
            Coordinates.Text = "";
        }

        private void Button_Click2(object sender, RoutedEventArgs e)
        {
            List<double> dataX = new List<double>();
            List<double> dataY = new List<double>();
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            grafics += 1;
            Coordinates.Text += $"\n№{grafics}\n";
            Coordinates.Text += "X Y";
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
                        Coordinates.Text += $"\n{Math.Round((double)x, 3)}, {Math.Round((double)y, 3)}";
                    }
                }
                Dictionary<double, double> tochki = spline(dataX.ToArray(), dataY.ToArray());
                WpfPlot1.Plot.Add.Scatter(tochki.Keys.ToArray(), tochki.Values.ToArray());
                WpfPlot1.Refresh();
                max_min(dataX.ToArray(), dataY.ToArray());
            }
            catch (Exception ex)
            {
                ///ну  тип решил не выбирать файл
            }
        }
        private void Button_Click3(object sender, RoutedEventArgs e)
        {
            List<double> dataX = new List<double>();
            List<double> dataY = new List<double>();
            grafics += 1;
            Coordinates.Text += $"\n№{grafics}\n";
            Coordinates.Text += "X Y";
            for (double i = double.Parse(predel.Text.Split(" ")[0]); i <= double.Parse(predel.Text.Split(" ")[1]); i += 0.1)
            {
                //System.Data.DataTable table = new System.Data.DataTable();
                //double a = Convert.ToDouble(table.Compute(function.Text.Split("=")[function.Text.Split("=").Length - 1].Replace("x", i.ToString()), string.Empty));
                //var a = CSharpScript.EvaluateAsync<double>(function.Text.Split("=")[function.Text.Split("=").Length - 1].Replace("x", i.ToString())).Result;
                //double a = "-2 * Math.Log(1/0.5f + Math.Sqrt(1/Math.Pow(0.5d, 2) + 1L))".Evaluate(new DotNetStandartMathContext());
                ScriptEngine engine = Python.CreateEngine();
                ScriptScope scope = engine.CreateScope();
                engine.ExecuteFile("C:\\Users\\Admin\\source\\repos\\WpfApp1\\WpfApp1\\graficstroit.py", scope);
                dynamic function_str = scope.GetVariable("function");
                dynamic result = function_str(function.Text, i);
                Coordinates.Text += $"\n{Math.Round((double)i, 3)}, {Math.Round((double)result, 3)}";
                dataX.Add(i);
                dataY.Add(result);
            }
            max_min(dataX.ToArray(), dataY.ToArray());
            WpfPlot1.Plot.Add.Scatter(dataX, dataY);
            WpfPlot1.Refresh();
        }
    }
}
