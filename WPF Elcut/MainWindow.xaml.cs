using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Serialization;
using Microsoft.Win32;

namespace EngineBuilder
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        int currentStep = 0;
        int maxCountStep;
        List<UserControl> listUserStep;
        Engine engine;

        public MainWindow()
        {
            InitializeComponent();

            if (!Directory.Exists(workDirectory))
                Directory.CreateDirectory(workDirectory);

        }

        string workDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\EngineBuilder_XML\\";

        private void MenuItem_Open(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "XML (*.XML)|*.XML";
            dialog.CheckFileExists = true;
            dialog.InitialDirectory = workDirectory;

            if (dialog.ShowDialog() == true)
            {
                string path = dialog.FileName;

                using (Stream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    XmlSerializer xml = new XmlSerializer(typeof(Engine));
                    engine = (Engine)xml.Deserialize(stream);
                    stream.Dispose();
                    MessageBox.Show("Конфигурация двигателя успешно загружена.\nИсходный файл:  " + path);
                }
            }

            ResetUIPosition();
            InitializeUserStepUI();
        }

        private void MenuItem_Unsafe_Mode(object sender, RoutedEventArgs e)
        {
            if (((MenuItem)sender).IsChecked)
            {
                ((MenuItem)sender).IsChecked = false;
                engine.unsafeMode = false;
            }
            else
            {
                ((MenuItem)sender).IsChecked = true;
                engine.unsafeMode = true;
            }

            InitializeUserStepUI();

        }

        private void MenuItem_SaveAs(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "XML (*.XML)|*.XML";
            dialog.CheckPathExists = true;
            dialog.InitialDirectory = workDirectory;
            dialog.FileName = "Engine_" + DateTime.Now.ToShortDateString();
            dialog.DefaultExt = ".xml";

            if (dialog.ShowDialog() == true)
            {
                string path = dialog.FileName;
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(Engine));
                using (Stream stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    xmlSerializer.Serialize(stream, engine);
                    stream.Dispose();
                    MessageBox.Show("Конфигурация двигателя успешно сохранена.\nПуть к файлу:  " + path);
                }
            }
        }

        private void InitializeUserStepUI()
        {

            listUserStep = new List<UserControl>();
            listUserStep.Add(new UserStep1_1_MainParams(engine) { Width = viewboxMain.Width, Height = viewboxMain.Height });
            listUserStep.Add(new UserStep1_2_RotorParams(engine) { Width = viewboxMain.Width, Height = viewboxMain.Height });
            listUserStep.Add(new UserStep1_3_RotorParams(engine) { Width = viewboxMain.Width, Height = viewboxMain.Height });

            if (!engine.onlyRotor)
            {
                listUserStep.Add(new UserStep1_4_StatorParams(engine) { Width = viewboxMain.Width, Height = viewboxMain.Height });
                listUserStep.Add(new UserStep1_5_StatorParams(engine) { Width = viewboxMain.Width, Height = viewboxMain.Height });
            }

            listUserStep.Add(new UserStep1_6_Materials(engine) { Width = viewboxMain.Width, Height = viewboxMain.Height });
            if (!engine.onlyRotor)
            {
                listUserStep.Add(new UserStep1_7_RegulatorsParams(engine) { Width = viewboxMain.Width, Height = viewboxMain.Height });
            }
            /////
            listUserStep.Add(new UserStep2_1_MainCalc(engine) { Width = viewboxMain.Width, Height = viewboxMain.Height });
            listUserStep.Add(new UserStep2_2_RotorCalc(engine) { Width = viewboxMain.Width, Height = viewboxMain.Height });
            listUserStep.Add(new UserStep2_3_RotorCalc(engine) { Width = viewboxMain.Width, Height = viewboxMain.Height });

            if (!engine.onlyRotor)
            {
                listUserStep.Add(new UserStep2_4_StatorCalc(engine) { Width = viewboxMain.Width, Height = viewboxMain.Height });
                listUserStep.Add(new UserStep2_5_StatorCalc(engine) { Width = viewboxMain.Width, Height = viewboxMain.Height });
                listUserStep.Add(new UserStep2_6_StatorCalc(engine) { Width = viewboxMain.Width, Height = viewboxMain.Height });

                listUserStep.Add(new UserStep2_7_RegulatorsCalc(engine) { Width = viewboxMain.Width, Height = viewboxMain.Height });
                listUserStep.Add(new UserStep2_8_RegulatorsCalc(engine) { Width = viewboxMain.Width, Height = viewboxMain.Height });
            }

            maxCountStep = listUserStep.Count;
            viewboxMain.Child = listUserStep[0];
        }


        private void MenuItem_Build(object sender, RoutedEventArgs e)
        {
            engine.Solve();

            ResetUIPosition();
            InitializeUserStepUI();

            WindowElcutProgress we = new WindowElcutProgress(engine);
            we.Visibility = Visibility.Visible;
            we.Activate();
            we.Run();

        }

        private void MenuItem_ViewParameters(object sender, RoutedEventArgs e)
        {
            engine.Solve();

            ResetUIPosition();
            InitializeUserStepUI();

            WindowEngineParameters wep = new WindowEngineParameters(engine);
            wep.Visibility = Visibility.Visible;
            wep.Activate();
        }

        private void Button_Click_Next(object sender, RoutedEventArgs e)
        {
            if (currentStep >= 0 && currentStep < maxCountStep - 1)
            {
                engine.Solve();
                InitializeUserStepUI();
            }

            if (currentStep < maxCountStep - 1)
            {
                currentStep++;
                viewboxMain.Child = listUserStep[currentStep];
            }
            if (currentStep == maxCountStep - 1)
            {
                btn_Next.IsEnabled = false;
                btn_Build.IsEnabled = true;
            }
            else { btn_Next.IsEnabled = true; }

            if (currentStep == 0) { btn_Previous.IsEnabled = false; }
            else { btn_Previous.IsEnabled = true; }
        }

        private void Button_Click_Previous(object sender, RoutedEventArgs e)
        {
            if (currentStep > 0 && currentStep < maxCountStep - 1) 
            {
                engine.Solve();
                InitializeUserStepUI();
            }

            if (currentStep > 0)
            {
                currentStep--;
                viewboxMain.Child = listUserStep[currentStep];
            }

            if (currentStep == maxCountStep - 1) { btn_Next.IsEnabled = false; }
            else { btn_Next.IsEnabled = true; }

            if (currentStep == 0) { btn_Previous.IsEnabled = false; }
            else { btn_Previous.IsEnabled = true; }

            btn_Build.IsEnabled = false;
        }


        private void Window_Loaded_1(object sender, RoutedEventArgs e)
        {

            unsafeMenu.IsChecked = false;
            OnlyRotorMenu.IsChecked = false;

            engine = new Engine();

            engine.Initialize();
            engine.Solve();

            ResetUIPosition();
            InitializeUserStepUI();
        }

        //только ротор
        private void MenuItem_onlyRotor(object sender, RoutedEventArgs e)
        {
            if (((MenuItem)sender).IsChecked)
            {
                ((MenuItem)sender).IsChecked = false;
                engine.onlyRotor = false;
            }
            else
            {
                ((MenuItem)sender).IsChecked = true;
                engine.onlyRotor = true;
            }

            ResetUIPosition();
            InitializeUserStepUI();
        }

        void ResetUIPosition()
        {
            btn_Previous.IsEnabled = false;
            btn_Next.IsEnabled = true;
            btn_Build.IsEnabled = false;
            currentStep = 0;
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            var help = new WindowHelp();
            help.Show();
        }

    }
}
