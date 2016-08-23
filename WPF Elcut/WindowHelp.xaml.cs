using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace EngineBuilder
{
    /// <summary>
    /// Логика взаимодействия для WindowHelp.xaml
    /// </summary>
    public partial class WindowHelp : Window
    {
        public WindowHelp()
        {
            InitializeComponent();
            Browser.Source = new Uri(Directory.GetCurrentDirectory()+"\\Help.html");
        }

        private void Window_MouseWheel_1(object sender, MouseWheelEventArgs e)
        {
            Browser.Refresh();
        }

    }
}
