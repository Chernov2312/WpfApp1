using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Drawing;
using ScottPlot;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            video.Position = TimeSpan.FromSeconds(0);
            video.Play();
        }
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double widthRatio = ActualWidth / 800;
            double heightRatio = ActualHeight / 600;
            transition.FontSize = 50 * widthRatio;
            transition.FontSize = 25 * heightRatio;
            resh_ur.FontSize = 50 * widthRatio;
            resh_ur.FontSize = 25 * heightRatio;
            points.FontSize = 50 * widthRatio;
            points.FontSize = 25 * heightRatio;
            exit_button.FontSize = 50 * widthRatio;
            exit_button.FontSize = 25 * heightRatio;

        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Window1 window = new Window1();
            this.Close();
            window.Show();
        }
        private void Button_Click2(object sender, RoutedEventArgs e)
        {
            video.Position = TimeSpan.FromSeconds(0);
            video.Play();
        }
        private void Button_Click4(object sender, RoutedEventArgs e)
        {
            Window3 window = new Window3();
            this.Close();
            window.Show();
        }

        private void button_points(object sender, RoutedEventArgs e)
        {
            Window2 window = new Window2();
            this.Close();
            window.Show();
        }

        private void button_exit(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}