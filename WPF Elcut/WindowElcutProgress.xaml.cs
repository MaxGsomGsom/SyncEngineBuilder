using System;
using System.Collections.Generic;
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

namespace EngineBuilder
{
    /// <summary>
    /// Логика взаимодействия для WindowElcut.xaml
    /// </summary>
    public partial class WindowElcutProgress : Window
    {
        Elcut elcut;

        public WindowElcutProgress(Engine engine)
        {
            InitializeComponent();
            elcut = new Elcut(engine);
            elcut.eventProgress += UpdateProgressBar;
            progressBar.Maximum = 50;
            progressBar.Minimum = 0;
            progressBar.Value = 0;

        }
        public void Run()
        {
            Task.Run(() =>
            elcut.Run());
        }

        private void UpdateProgressBar(string mes, bool isCaption)
        {
            Dispatcher.Invoke(() =>
            {
                progressBar.Value++;
                progressBar.UpdateLayout();

                if (isCaption)
                    textBlock.Text = mes;
                else
                    TextBoxConsole.Text += mes + '\n';
            });
        }

        private void TextBoxConsole_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
