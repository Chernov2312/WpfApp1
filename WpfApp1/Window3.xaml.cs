using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
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
    /// Логика взаимодействия для Window3.xaml
    /// </summary>
    public partial class Window3 : Window
    {
        List<string> urs = new List<string>();
        int urs_number = 1;
        static List<List<double>> Coefsient(string[] urs)
        {
            List<List<double>> itog = new List<List<double>>();
            string pattern = @"^\s*([+-]?\s*\d*\.?\d+\s*[a-zA-Z]+\s*)+=\s*[+-]?\s*\d+\.?\d*\s*$";
            //foreach (string equation in urs)
            //{
            //    Match match = Regex.Match(equation.Replace(" ", ""), pattern);
            //    if (match.Success)
            //    {
            //        List<double> coefs = new List<double>();
            //        int i = 0;
            //        foreach (var coef in match.Groups)
            //        {
            //            if (i == 0)
            //            {
            //                i++;
            //                continue;
            //            }
            //            coefs.Add(ParseCoefficient(coef.ToString()));
            //        }
            //        itog.Add(new List<double>(coefs));

            //    }
            //    else
            //    {
            //        Console.WriteLine("Уравнение не соответствует формату.");
            //    }

            //}
            foreach(var elem in urs)
            {
                itog.Add(ParseEquation(elem));
            }
            return itog;
        }

        //static double ParseCoefficient(string coeff)
        //{
        //    if (string.IsNullOrEmpty(coeff))
        //        return 1;
        //    if (coeff == "+")
        //        return 1;
        //    if (coeff == "-")
        //        return -1;
        //    return double.Parse(coeff);
        //}
        public static List<double> ParseEquation(string equation)
        {
            equation = equation.Replace(" ", "");
            string[] parts = equation.Split('=');
            if (parts.Length != 2) throw new FormatException("Invalid equation format");

            double constant = double.Parse(parts[1]);
            var coeffDict = new Dictionary<string, double>();

            // Обрабатываем все члены с переменными
            var matches = Regex.Matches(parts[0], @"([+-]?\d*\.?\d*)([a-zA-Z]+)");
            foreach (Match match in matches)
            {
                string coeffStr = match.Groups[1].Value;
                double coeff = coeffStr switch
                {
                    "" => 1,
                    "+" => 1,
                    "-" => -1,
                    _ => double.Parse(coeffStr)
                };

                string varName = match.Groups[2].Value;
                coeffDict[varName] = coeff;
            }

            // Сортируем переменные по алфавиту
            var variables = coeffDict.Keys.OrderBy(v => v).ToList();
            var result = variables.Select(v => coeffDict[v]).ToList();
            result.Add(constant);

            return result;
        }
        public static double[] Solve(List<List<double>> augmentedMatrix)
        {
            int rows = augmentedMatrix.ToArray().Length;
            if (rows == 0) throw new ArgumentException("Матрица не может быть пустой");

            int cols = augmentedMatrix[0].Count;
            if (cols != rows + 1) throw new ArgumentException("Неверный формат расширенной матрицы");

            // Создаем копию матрицы, чтобы не изменять исходную
            double[,] matrix = new double[rows, cols];
            for (int i = 0; i < rows; i++)
            {
                if (augmentedMatrix[i].Count != cols)
                    throw new ArgumentException("Все строки матрицы должны иметь одинаковую длину");

                for (int j = 0; j < cols; j++)
                {
                    matrix[i, j] = augmentedMatrix[i][j];
                }
            }

            // Прямой ход метода Гаусса
            for (int pivot = 0; pivot < rows; pivot++)
            {
                // Поиск строки с максимальным элементом в текущем столбце
                int maxRow = pivot;
                for (int row = pivot + 1; row < rows; row++)
                {
                    if (Math.Abs(matrix[row, pivot]) > Math.Abs(matrix[maxRow, pivot]))
                    {
                        maxRow = row;
                    }
                }

                // Перестановка строк, если необходимо
                if (maxRow != pivot)
                {
                    for (int col = 0; col < cols; col++)
                    {
                        double temp = matrix[pivot, col];
                        matrix[pivot, col] = matrix[maxRow, col];
                        matrix[maxRow, col] = temp;
                    }
                }

                // Проверка на вырожденность системы
                if (Math.Abs(matrix[pivot, pivot]) < 1e-12)
                {
                    throw new InvalidOperationException("Система не имеет единственного решения");
                }

                // Нормировка текущей строки
                double pivotValue = matrix[pivot, pivot];
                for (int col = pivot; col < cols; col++)
                {
                    matrix[pivot, col] /= pivotValue;
                }

                // Обнуление элементов ниже ведущего
                for (int row = pivot + 1; row < rows; row++)
                {
                    double factor = matrix[row, pivot];
                    for (int col = pivot; col < cols; col++)
                    {
                        matrix[row, col] -= factor * matrix[pivot, col];
                    }
                }
            }

            // Обратный ход метода Гаусса
            double[] solution = new double[rows];
            for (int row = rows - 1; row >= 0; row--)
            {
                solution[row] = matrix[row, cols - 1];
                for (int col = row + 1; col < rows; col++)
                {
                    solution[row] -= matrix[row, col] * solution[col];
                }
            }

            return solution;
        }
        public Window3()
        {
            InitializeComponent();
        }

        private void clear_button(object sender, RoutedEventArgs e)
        {
            combox.Items.Clear();
        }

        private void add_button(object sender, RoutedEventArgs e)
        {
            combox.Items.Add(uravnenie.Text);
            urs.Add(uravnenie.Text);
            uravnenie.Text = "";
        }

        private void Button_resh(object sender, RoutedEventArgs e)
        {
            List<List<double>> matrix = Coefsient(new List<string>(urs).ToArray());
            Otvet.Text += $"Номер уравнения: {urs_number}\n";
            foreach(string elem in urs)
            {
                Otvet.Text += elem + "\n";
            }
            urs.Clear();
            urs_number += 1;
            Otvet.Text += $"Ответы:\n";
            try
            {
                double[] solution = Solve(matrix);
                for (int i = 0; i < solution.Length; i++)
                {
                    Otvet.Text += $"x{i + 1} = {solution[i]}\n";
                }
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine("Ошибка: " + ex.Message);
            }
        }

        private void Button_back_to_menu(object sender, RoutedEventArgs e)
        {
            MainWindow window = new MainWindow();
            this.Close();
            window.Show();
        }
    }
}
