using EngineBuilder.Entities;
using System;
using System.Collections.Generic;
using System.IO;
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
using System.Xml.Serialization;

namespace EngineBuilder
{
    public partial class UserStep1_6_Materials : UserControl
    {
        Engine engine;
        Popup popup;
        StackPanel stackPanel;
        XmlSerializer serializer = new XmlSerializer(typeof(List<Material>));

        string filenameMaterials = "Materials.xml";

        List<Material> materials = new List<Material>();

        public UserStep1_6_Materials(Engine engine)
        {
            this.engine = engine;

            popup = new Popup();
            stackPanel = new StackPanel();
            stackPanel.Children.Add(new TextBox());
            popup.Child = stackPanel;

            InitializeComponent();

        }

        private void TextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            popup.IsOpen = true;
            ((TextBox)(stackPanel.Children[0])).Text = "Выбор напряженности магнитного поля H и соответвующей ей коэрцитивной силы B материала, а также его магнитной проницаемости Mu";

            popup.Placement = PlacementMode.Mouse;
        }

        private void TextBlock_MouseLeave(object sender, MouseEventArgs e)
        {
            popup.IsOpen = false;
        }

        private void UserControl_Loaded_1(object sender, RoutedEventArgs e)
        {

            //открываем список магнитов или создаем новый
            if (File.Exists(filenameMaterials))
            {
                FileStream file = File.OpenRead(filenameMaterials);
                materials = (List<Material>)serializer.Deserialize(file);
                file.Close();
            }
            else SaveMaterial(engine.magnetMaterial);

            magnetsComboBox.ItemsSource = materials;
            statorComboBox.ItemsSource = materials;
            rotorComboBox.ItemsSource = materials;
            bashmakComboBox.ItemsSource = materials;

            var mat = materials.Find((m) => m.name == engine.magnetMaterial.name);
            if (mat == null) magnetsComboBox.SelectedIndex = 0;
            else magnetsComboBox.SelectedItem = mat;

            var mat2 = materials.Find((m) => m.name == engine.statorMaterial.name);
            if (mat2 == null) statorComboBox.SelectedIndex = 0;
            else statorComboBox.SelectedItem = mat2;

            var mat3 = materials.Find((m) => m.name == engine.rotorMaterial.name);
            if (mat3 == null) rotorComboBox.SelectedIndex = 0;
            else rotorComboBox.SelectedItem = mat3;

            var mat4 = materials.Find((m) => m.name == engine.bashmakMaterial.name);
            if (mat4 == null) bashmakComboBox.SelectedIndex = 0;
            else bashmakComboBox.SelectedItem = mat4;
        }






        private void SaveMaterial(Material m = null)
        {
            if (m != null) materials.Add(m);
            FileStream file = File.Open(filenameMaterials, FileMode.Create);
            serializer.Serialize(file, materials);
            file.Close();

            magnetsComboBox.ItemsSource = null;
            magnetsComboBox.ItemsSource = materials;
            magnetsComboBox.SelectedItem = materials.Last();

            statorComboBox.ItemsSource = null;
            statorComboBox.ItemsSource = materials;
            statorComboBox.SelectedItem = materials.Last();

            rotorComboBox.ItemsSource = null;
            rotorComboBox.ItemsSource = materials;
            rotorComboBox.SelectedItem = materials.Last();

            bashmakComboBox.ItemsSource = null;
            bashmakComboBox.ItemsSource = materials;
            bashmakComboBox.SelectedItem = materials.Last();
        }


        private void rotorComboBox_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            engine.rotorMaterial = (Material)rotorComboBox.SelectedItem;
        }

        private void bashmakComboBox_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            engine.bashmakMaterial = (Material)bashmakComboBox.SelectedItem;
        }

        private void statorComboBox_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            engine.statorMaterial = (Material)statorComboBox.SelectedItem;
        }


        private void materialButton_Click_1(object sender, RoutedEventArgs e)
        {
            var window = new WindowAddMaterial();
            window.OnReturnMaterial += (s, m) => SaveMaterial(m);
            window.Show();
        }

        private void materialButtonDel_Click_1(object sender, RoutedEventArgs e)
        {
            materials.Remove((Material)magnetsComboBox.SelectedItem);
            SaveMaterial();
        }

        private void magnetsComboBox_Selected_1(object sender, SelectionChangedEventArgs e)
        {
            engine.magnetMaterial = (Material)magnetsComboBox.SelectedItem;
        }


    }
}
