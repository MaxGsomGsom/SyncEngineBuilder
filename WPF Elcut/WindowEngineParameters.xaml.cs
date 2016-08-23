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
    /// Логика взаимодействия для Window1.xaml
    /// </summary>
    public partial class WindowEngineParameters : Window
    {
        public WindowEngineParameters(Engine engine)
        {
            InitializeComponent();

            dataGrid.BeginningEdit += (s, e) => { e.Cancel = true; };

            dataGrid.Items.Clear();
            dataGrid.Columns.Add(new DataGridTextColumn() { Header = "Название", Binding = new Binding("Name")});
            dataGrid.Columns.Add(new DataGridTextColumn() { Header = "Обозначение", Binding = new Binding("ShotName") });
            dataGrid.Columns.Add(new DataGridTextColumn() { Header = "Единица измерения", Binding = new Binding("Measure") });
            dataGrid.Columns.Add(new DataGridTextColumn() { Header = "Значение", Binding = new Binding("Value") });

            for (int i = 0; i < engine.engineParameters.Count; i++)
            {
                dataGrid.Items.Add(new { Name = engine.engineParameters[i].Name, ShotName = engine.engineParameters[i].ShotName, Measure = engine.engineParameters[i].Measure, Value = engine.engineParameters[i].Value.ToString() });
            }

        }

        private void MenuItem_Copy(object sender, RoutedEventArgs e)
        {
            dataGrid.SelectAllCells();
            ApplicationCommands.Copy.Execute(null, dataGrid);
            
        }
    }


}
