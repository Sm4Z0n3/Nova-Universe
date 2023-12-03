using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WpfDemo
{
    /// <summary>
    /// Creat.xaml 的交互逻辑
    /// </summary>
    public partial class Creat : Window
    {
        public Creat()
        {
            InitializeComponent();
            this.Background = SystemParameters.WindowGlassBrush;
        }
        MainWindow mw = new MainWindow();
        private void Num_Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            MainWindow.NumberS = (int)Num_Slider.Value;
        }

        private void Wid_Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            MainWindow.CTSWH[0] = (int)Wid_Slider.Value;
        }

        private void Hei_Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            MainWindow.CTSWH[1] = (int)Hei_Slider.Value;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            mw.Visibility = Visibility.Visible;
            mw.log.Visibility = Visibility.Visible;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            System.Environment.Exit(0);
        }
    }
}
