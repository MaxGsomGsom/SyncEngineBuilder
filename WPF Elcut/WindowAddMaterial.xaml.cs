using EngineBuilder.Entities;
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
    /// Логика взаимодействия для WindowAddMaterial.xaml
    /// </summary>
    public partial class WindowAddMaterial : Window
    {
        List<BHPair> BHSpline = new List<BHPair>();

        public event EventHandler<Material> OnReturnMaterial;

        public WindowAddMaterial()
        {
            InitializeComponent();
            dataGrid1.ItemsSource = BHSpline;
            dataGrid1.AutoGeneratingColumn += (s, e) =>
            {
                e.Column.Width = 100;
            };
            dataGrid1.IsEnabled = false;

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Material m = new Material();
            m.muX = double.Parse(muX.Text.Replace('.',','));
            m.muY = double.Parse(muY.Text.Replace('.', ','));
            m.nonLinearBH = nonLinear.IsChecked.Value;
            m.BHSpline = BHSpline;
            m.name = mat.Text;
            m.magneticPower = double.Parse(magneticPower.Text);
            m.powerAngle = double.Parse(magneticPowerAngle.Text);
            OnReturnMaterial(this, m);
            Close();
        }

        private void nonLinear_Checked_1(object sender, RoutedEventArgs e)
        {
            if (nonLinear.IsChecked.Value == true) dataGrid1.IsEnabled = true;
            else dataGrid1.IsEnabled = false;

        }
    }

}
