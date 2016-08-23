using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EngineBuilder
{
    /// <summary>
    /// Логика взаимодействия для UserStep2_4_StatorCalc.xaml
    /// </summary>
    public partial class UserStep2_4_StatorCalc : UserControl
    {
        Engine engine;
        Popup popup;
        StackPanel stackPanel;

        public UserStep2_4_StatorCalc(Engine engine)
        {
            this.engine = engine;

            popup = new Popup();
            stackPanel = new StackPanel();
            stackPanel.Children.Add(new TextBox());
            stackPanel.Children.Add(new TextBox());
            stackPanel.Children.Add(new TextBox());
            stackPanel.Children.Add(new TextBox());
            stackPanel.Children.Add(new TextBox());
            stackPanel.Children.Add(new TextBox());
            (stackPanel.Children[stackPanel.Children.Count - 1] as TextBox).TextWrapping = TextWrapping.Wrap;
            (stackPanel.Children[stackPanel.Children.Count - 1] as TextBox).MaxWidth = 300;
            popup.Child = stackPanel;

            InitializeComponent();
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            string name = ((Slider)sender).Name;
            name = new string(name.ToCharArray(), 0, name.Length - "Slider".Length);
            string engineParameter = name;
            name += "TextBox";
            TextBox textBox = (TextBox)this.FindName(name);
            textBox.Text = Convert.ToString(Math.Round(((Slider)sender).Value, 4));
            engine.SetEngineParameter(engineParameter, ((Slider)sender).Value);
        }

        private void TextBox_ValueChanged(object sender, RoutedEventArgs e)
        {
            string name = ((TextBox)sender).Name;
            name = new string(name.ToCharArray(), 0, name.Length - "TextBox".Length);
            string engineParameter = name;
            Slider slider = (Slider)this.FindName(name + "Slider");
            try
            {
                //проверка на макс мин значения
                double val = Convert.ToDouble(((TextBox)sender).Text);
                if (val < engine.GetEngineParameter(name).MinValue && !engine.unsafeMode)
                {
                    ((TextBox)sender).Text = engine.GetEngineParameter(name).MinValue.ToString();
                }
                else if (val > engine.GetEngineParameter(name).MaxValue && !engine.unsafeMode)
                {
                    ((TextBox)sender).Text = engine.GetEngineParameter(name).MaxValue.ToString();
                }
                else
                {
                    if (!engine.unsafeMode) slider.Value = val;
                    engine.SetEngineParameter(engineParameter, val);
                }
            }
            catch (Exception)
            {

            }
        }

        private void TextBox_Initialized(object sender, EventArgs e)
        {
            string name = ((TextBox)sender).Name;
            name = new string(name.ToCharArray(), 0, name.Length - "TextBox".Length);
            ((TextBox)sender).Text = Convert.ToString(Math.Round(engine.GetEngineParameter(name).Value, 4));
        }

        private void Slider_Initialized(object sender, EventArgs e)
        {
            string name = ((Slider)sender).Name;
            name = new string(name.ToCharArray(), 0, name.Length - "Slider".Length);
            ((Slider)sender).IsSnapToTickEnabled = true;
            ((Slider)sender).Value = Convert.ToDouble(engine.GetEngineParameter(name).Value);
            ((Slider)sender).TickFrequency = engine.GetEngineParameter(name).Step;
            if (!engine.unsafeMode)
            {
                ((Slider)sender).Minimum = engine.GetEngineParameter(name).MinValue;
                ((Slider)sender).Maximum = engine.GetEngineParameter(name).MaxValue;
            }
            else
            {
                ((Slider)sender).Minimum = 0;
                ((Slider)sender).Maximum = 1000000;
            }
        }

        private void TextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            popup.IsOpen = true;
            ((TextBox)(stackPanel.Children[0])).Text = "Обозначение: " + engine.GetEngineParameter(((TextBlock)sender).Text).ShotName;
            ((TextBox)(stackPanel.Children[1])).Text = "Значение = " + engine.GetEngineParameter(((TextBlock)sender).Text).Value.ToString();
            ((TextBox)(stackPanel.Children[2])).Text = "Min = " + engine.GetEngineParameter(((TextBlock)sender).Text).MinValue.ToString();
            ((TextBox)(stackPanel.Children[3])).Text = "Max = " + engine.GetEngineParameter(((TextBlock)sender).Text).MaxValue.ToString();
            ((TextBox)(stackPanel.Children[4])).Text = "Единица измерения: " + engine.GetEngineParameter(((TextBlock)sender).Text).Measure.ToString();
            ((TextBox)(stackPanel.Children[5])).Text = "Описание: " + engine.GetEngineParameter(((TextBlock)sender).Text).Help.ToString();
            popup.Placement = PlacementMode.Mouse;
        }

        private void TextBlock_MouseLeave(object sender, MouseEventArgs e)
        {
            popup.IsOpen = false;
        }
    }
}
