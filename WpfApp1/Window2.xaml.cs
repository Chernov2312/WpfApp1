using IronPython.Hosting;
using Microsoft.Scripting.Hosting;
using ScottPlot.Interactivity;
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
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WpfApp1
{
    /// <summary>
    /// Логика взаимодействия для Window2.xaml
    /// </summary>
    public partial class Window2 : Window
    {
        public int grafics = 0;
        static Dictionary<double, double> GenerateQuadraticSplines(double[] x, double[] y, double splineStep = 0.05)
        {
            if (x == null || y == null || x.Length != y.Length || x.Length < 3)
                throw new ArgumentException("Invalid input arrays. Arrays must be of the same length and have at least 3 points.");

            // Сортируем точки по x, если они не упорядочены
            var points = x.Zip(y, (xi, yi) => new { X = xi, Y = yi })
                          .OrderBy(p => p.X)
                          .ToList();

            // Проверяем, что все x уникальны
            if (points.Select(p => p.X).Distinct().Count() != points.Count)
                throw new ArgumentException("Duplicate x-values are not allowed.");

            var splineDict = new Dictionary<double, double>();

            // Проходим по всем интервалам между точками
            for (int i = 0; i < points.Count - 1; i++)
            {
                double x0 = points[i].X;
                double y0 = points[i].Y;
                double x1 = points[i + 1].X;
                double y1 = points[i + 1].Y;

                // Для квадратичного сплайна нужно три точки. Используем текущую, следующую и среднюю.
                double xMid = (x0 + x1) / 2;
                double yMid = (y0 + y1) / 2; // Можно использовать другую интерполяцию

                // Коэффициенты квадратичного уравнения y = a*x^2 + b*x + c
                // Решаем систему уравнений для трех точек
                double[,] matrix = {
                { x0 * x0, x0, 1 },
                { xMid * xMid, xMid, 1 },
                { x1 * x1, x1, 1 }
            };
                double[] rhs = { y0, yMid, y1 };

                double[] coefficients = SolveLinearSystem(matrix, rhs);
                double a = coefficients[0];
                double b = coefficients[1];
                double c = coefficients[2];

                // Генерируем точки сплайна с заданным шагом
                for (double xi = x0; xi <= x1; xi += splineStep)
                {
                    if (xi > x1 && i < points.Count - 2) // Чтобы избежать дублирования на границах
                        break;

                    double yi = a * xi * xi + b * xi + c;
                    if (!splineDict.ContainsKey(xi)) // Избегаем дублирования ключей
                        splineDict.Add(xi, yi);
                }
            }

            return splineDict;
        }

        // Решение системы линейных уравнений методом Гаусса
        static double[] SolveLinearSystem(double[,] matrix, double[] rhs)
        {
            int n = rhs.Length;
            double[] result = new double[n];

            // Прямой ход метода Гаусса
            for (int i = 0; i < n; i++)
            {
                // Поиск ведущего элемента
                int maxRow = i;
                for (int k = i + 1; k < n; k++)
                {
                    if (Math.Abs(matrix[k, i]) > Math.Abs(matrix[maxRow, i]))
                        maxRow = k;
                }

                // Перестановка строк
                if (maxRow != i)
                {
                    for (int k = 0; k < n; k++)
                    {
                        double temp = matrix[i, k];
                        matrix[i, k] = matrix[maxRow, k];
                        matrix[maxRow, k] = temp;
                    }
                    double tempRhs = rhs[i];
                    rhs[i] = rhs[maxRow];
                    rhs[maxRow] = tempRhs;
                }

                // Приведение к верхнетреугольному виду
                for (int k = i + 1; k < n; k++)
                {
                    double factor = matrix[k, i] / matrix[i, i];
                    rhs[k] -= factor * rhs[i];
                    for (int j = i; j < n; j++)
                    {
                        matrix[k, j] -= factor * matrix[i, j];
                    }
                }
            }

            // Обратный ход метода Гаусса
            for (int i = n - 1; i >= 0; i--)
            {
                result[i] = rhs[i];
                for (int j = i + 1; j < n; j++)
                {
                    result[i] -= matrix[i, j] * result[j];
                }
                result[i] /= matrix[i, i];
            }

            return result;
        }
        public static Dictionary<double, double> lineinspline(double[] x, double[] y, double key = 0.02)
        {
            Dictionary<double, double> itog = new Dictionary<double, double>();
            double[] b = new double[y.Length];
            double[] k = new double[y.Length];
            for (int i = 0; i < y.Length - 1; i++)
            {
                k[i] = (y[i + 1] - y[i]) / (x[i + 1] - x[i]);
                b[i] = y[i] - k[i] * x[i];
            }
            for (int i = 0; i < y.Length - 1; i++)
            {
                itog.Add(x[i], y[i]);
                for (double j = x[i] + key; j <= x[i + 1]; j += key)
                {
                    itog.Add(j, j * k[i] + b[i]);
                }
            }
            return itog;
        }
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
                itog.Add(x[i], y[i]);
                for (double j = x[i] + key; j < x[i + 1]; j += key)
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
            double widthRatio = ActualWidth / 800 * 40;
            double heightRatio = ActualHeight / 600 * 20;
            button_clear.FontSize = widthRatio;
            button_clear.FontSize = heightRatio;
            create_function.FontSize = widthRatio;
            create_function.FontSize = heightRatio;
            Coordinates.FontSize = widthRatio;
            Coordinates.FontSize = heightRatio;
            Max_Min.FontSize = widthRatio;
            Max_Min.FontSize = heightRatio;
            back.FontSize = heightRatio;
            back.FontSize = heightRatio;
            extremum.FontSize = heightRatio;
            extremum.FontSize = heightRatio;
            enter_extremum.FontSize = heightRatio;
            enter_extremum.FontSize = heightRatio;
            combox.FontSize = heightRatio;
            combox.FontSize = heightRatio;

        }
        public Window2()
        {
            InitializeComponent();
            combox.Items.Add("Квадратичный сплайн");
            combox.Items.Add("Линейный сплайн");
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

        private void Button_back_to_menu(object sender, RoutedEventArgs e)
        {
            MainWindow window = new MainWindow();
            this.Close();
            window.Show();
        }
        private void Button_Click3(object sender, RoutedEventArgs e)
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
                int k = 0;
                //Dictionary<double, double> tochki = GenerateQuadraticSplines(dataX.ToArray(), dataY.ToArray());
                if (combox.Text.Equals("Линейный сплайн"))
                {
                    Dictionary<double, double> tochki = lineinspline(dataX.ToArray(), dataY.ToArray());
                    WpfPlot1.Plot.Add.Scatter(tochki.Keys.ToArray(), tochki.Values.ToArray());
                    WpfPlot1.Refresh();
                }
                else
                {
                    Dictionary<double, double> tochki = spline(dataX.ToArray(), dataY.ToArray());
                    WpfPlot1.Plot.Add.Scatter(tochki.Keys.ToArray(), tochki.Values.ToArray());
                    WpfPlot1.Refresh();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred. Please try again later.", ex.ToString(), MessageBoxButton.OK, MessageBoxImage.Error);
            }
            try
            {
                List<double> dataX2 = new List<double>();
                List<double> dataY2 = new List<double>();
                int index_max = -1;
                int index_min = -1;
                for (int i = 0; i < dataX.ToArray().Length; i++)
                {
                    if (dataX[i] >= double.Parse(extremum.Text.Split(" ")[1]) && index_max == -1)
                    {
                        index_max = i;
                    }
                }
                for (int i = dataX.ToArray().Length - 1; i >= 0; i--)
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
            catch
            {
                ///
            }
        }
    }
}