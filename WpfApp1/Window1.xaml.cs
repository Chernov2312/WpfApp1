
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
using Microsoft.Scripting.Utils;
using ScottPlot;
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
        //double find_y(double x2, double[] x, double[] y)
        //{
        //    double[] b = new double[y.Length];
        //    double[] B = new double[y.Length];
        //    double[] h = new double[y.Length];
        //    double[] a = new double[y.Length];
        //    double[] c = new double[y.Length];
        //    double b0 = (y[1] - y[0]) / (x[1] - x[0]);
        //    Dictionary<double, double> itog = new Dictionary<double, double>();
        //    for (int i = 0; i < y.Length - 1; i++)
        //    {
        //        if (i > 0)
        //        {
        //            h[i] = x[i + 1] - x[i];
        //            B[i] = 2 * a[i - 1] * x[i] + b[i - 1];
        //            a[i] = 1 / (h[i] * h[i]) * (y[i + 1] - y[i]) - B[i] / h[i];
        //            b[i] = B[i] - 2 * a[i] * x[i];
        //            c[i] = y[i] - B[i] * x[i] + a[i] * x[i] * x[i];
        //        }
        //        else
        //        {
        //            h[i] = x[i + 1] - x[i];
        //            a[i] = (1 / (h[i] * h[i])) * (y[i + 1] - y[i]) - b0 / h[i];
        //            b[i] = b0 - 2 * a[i] * x[i];
        //            c[i] = y[i] - b0 * x[i] + a[i] * x[i] * x[i];
        //        }
        //    }
        //    int index = 0;
        //    for (int i = 1; i < x.Length; i++)
        //    {
        //        if (x2 <= x[i])
        //        {
        //            index = i - 1;
        //            break;
        //        }
        //    }
        //    return x2 * x2 * a[index] + x2 * b[index] + c[index];
        //}
        double find_y(double x2, double[] x, double[] y)
        {
            ScriptEngine engine = Python.CreateEngine();
            ScriptScope scope = engine.CreateScope();
            engine.ExecuteFile("C:\\Users\\Admin\\source\\repos\\WpfApp1\\WpfApp1\\graficstroit.py", scope);
            dynamic function_str = scope.GetVariable("function");
            dynamic result = function_str(function.Text, x2);
            if (result is string)
            {
                return double.NaN;
            }
            return result;
        }
        public void max_min(double[] dataX, double[] dataY)
        {
            double xmax = 0.0, ymax = 0.0;
            double xmin = 9999999, ymin = 99999;
            for (double x_dihotom = dataX[0]; x_dihotom <= dataX.ToArray()[dataX.Length - 1]; x_dihotom += 1e-1)
            {
                double y3 = find_y(x_dihotom, dataX, dataY);
                if (y3 is double.NaN)
                {
                    continue;
                }
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
            Max_Min.Text += $"Метод дихотомии:\nmax_x={Math.Round((double)xmax, 6)}, max_y={Math.Round((double)ymax, 6)}\nx_min={Math.Round((double)xmin, 6)}, y_min={Math.Round((double)ymin, 6)}";
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
                x1 = b - (b - a) / 1.5;
                x2 = a + (b - a) / 1.5;
                y1 = find_y(x1, dataX, dataY);
                y2 = find_y(x2, dataX, dataY);
                if (y2 is double.NaN || y1 is double.NaN)
                {
                    a = x1;
                    continue;
                }
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
            Max_Min.Text += $"\nx_min={Math.Round((float)x4, 6)}, y_min={Math.Round((double)find_y(x4, dataX, dataY), 6)}";
        }
        private void sized()
        {
            double widthRatio = ActualWidth / 800;
            double heightRatio = ActualHeight / 600;
            button_clear.FontSize = 40 * widthRatio;
            button_clear.FontSize = 20 * heightRatio;
            create_function.FontSize = 40 * widthRatio;
            create_function.FontSize = 20 * heightRatio;
            create_from_points.FontSize = 40 * widthRatio;
            create_from_points.FontSize = 20 * heightRatio;
            Coordinates.FontSize = 40 * widthRatio;
            Coordinates.FontSize = 20 * heightRatio;
            Max_Min.FontSize = 40 * widthRatio;
            Max_Min.FontSize = 20 * heightRatio;
            function.FontSize = 40 * widthRatio;
            function.FontSize = 20 * heightRatio;
            text_enter_function.FontSize = 40 * widthRatio;
            text_enter_function.FontSize = 20 * heightRatio;
            predel.FontSize = 40 * widthRatio;
            predel.FontSize = 20 * heightRatio;
            enter_predel.FontSize = 40 * widthRatio;
            enter_predel.FontSize = 20 * heightRatio;
            enter_extremum.FontSize = 40 * widthRatio;
            enter_extremum.FontSize = 20 * heightRatio;
            extremum.FontSize = 40 * widthRatio;
            extremum.FontSize = 20 * heightRatio;
        }
        public Window1()
        {
            InitializeComponent();
        }
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            sized();

        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            
            WpfPlot1.Plot.Clear();
            WpfPlot1.Refresh();
            grafics = 0;
            Max_Min.Text = "";
            Coordinates.Text = "";
        }

        private void Button_Click2(object sender, RoutedEventArgs e)
        {
            MainWindow window = new MainWindow();
            this.Close();
            window.Show();
        }
        private void Button_Click3(object sender, RoutedEventArgs e)
        {
            List<double> stroit_dataX = new List<double>();
            List<double> stroit_dataY = new List<double>();
            List<(List<double>, List<double>)> tupleList = new List<(List<double>, List<double>)>();
            List<double> dataX = new List<double>();
            List<double> dataY = new List<double>();
            grafics += 1;
            Coordinates.Text += $"\n№{grafics}\n";
            Coordinates.Text += "X Y";
            for (double i = double.Parse(predel.Text.Split(" ")[0]); i <= double.Parse(predel.Text.Split(" ")[1]); i += 0.04)
            {
                try
                {
                    ScriptEngine engine = Python.CreateEngine();
                    ScriptScope scope = engine.CreateScope();
                    engine.ExecuteFile("C:\\Users\\Admin\\source\\repos\\WpfApp1\\WpfApp1\\graficstroit.py", scope);
                    dynamic function_str = scope.GetVariable("function");
                    dynamic result = function_str(function.Text, i);
                    if (result is string)
                    {
                        tupleList.Add((new List<double>(stroit_dataX), new List<double>(stroit_dataY)));
                        stroit_dataX.Clear();
                        stroit_dataY.Clear();
                        continue;
                    }
                    Coordinates.Text += $"\n{Math.Round((double)i, 3)}, {Math.Round((double)result, 3)}";
                    stroit_dataX.Add(i);
                    stroit_dataY.Add(result);
                    dataX.Add(i);
                    dataY.Add(result);
                }
                catch(Exception ex)
                {
                    tupleList.Add((new List<double>(stroit_dataX), new List<double>(stroit_dataY)));
                    stroit_dataX.Clear();
                    stroit_dataY.Clear();
                }
            }
            tupleList.Add((new List<double>(stroit_dataX), new List<double>(stroit_dataY)));
            foreach (var elem in tupleList) {
                WpfPlot1.Plot.Add.Scatter(elem.Item1, elem.Item2);
            }
            WpfPlot1.Refresh();
            try
            {
                List<double> dataX2 = new List<double>();
                List<double> dataY2 = new List<double>();
                int index_max = -1;
                int index_min = -1;
                for(int i = 0; i < dataX.ToArray().Length; i++)
                {
                    if (dataX[i] >= double.Parse(extremum.Text.Split(" ")[1]) && index_max == -1)
                    {
                        index_max = i;
                    }
                }
                for(int i = dataX.ToArray().Length - 1; i >=0 ; i--)
                {
                    if (dataX[i] <= double.Parse(extremum.Text.Split(" ")[0]) && index_min == -1)
                    {
                        index_min = i;
                    }
                }
                if (index_max == -1)
                {
                    index_max = dataX.ToArray().Length - 1;
                }
                if (index_min == -1)
                {
                    index_min = dataX.ToArray().Length - 1;
                }
                for (int i = index_min; i <= index_max; i += 1)
                {
                    dataX2.Add(dataX[i]);
                    dataY2.Add(dataY[i]);
                }
                max_min(dataX2.ToArray(), dataY2.ToArray());
            }
            catch(Exception ex)
            {
                ///
            }
        }
    }
}
