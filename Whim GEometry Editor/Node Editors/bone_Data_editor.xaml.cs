using MDLLib;
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

namespace Whim_GEometry_Editor.Node_Editors
{
    /// <summary>
    /// Interaction logic for bone_Data_editor.xaml
    /// </summary>
    public partial class bone_Data_editor : Window
    {
        Bone bone;
        public bone_Data_editor( w3Node node, w3Model model)
        {
            InitializeComponent();
            bone = node.Data as Bone;
            ListBoxGeoset.Items.Add(new ListBoxItem() { Content = "Multiple"});
            ListBoxGeoset.SelectedItem = ListBoxGeoset.Items[0];
            foreach (w3Geoset geo in model.Geosets)
            {
                ListBoxGeoset.Items.Add(new ListBoxItem() { Content = geo.ID.ToString() });
                if (bone.Geoset_ID == geo.ID) { ListBoxGeoset.SelectedItem = ListBoxGeoset.Items[ListBoxGeoset.Items.Count-1]; }

            }
            ListBoxGeosetAnimation.Items.Add(new ListBoxItem() { Content = "None" });
            ListBoxGeosetAnimation.SelectedItem = ListBoxGeosetAnimation.Items[0];
            foreach (w3Geoset_Animation geo in model.Geoset_Animations)
            {
                ListBoxGeosetAnimation.Items.Add(new ListBoxItem() { Content = geo.ID.ToString() });
                if (bone.Geoset_Animation_ID == geo.ID) { ListBoxGeoset.SelectedItem = ListBoxGeosetAnimation.Items[ListBoxGeosetAnimation.Items.Count - 1]; }
            }

        }

        private void OnButtonClick(object sender, RoutedEventArgs e)
        {

            if (ListBoxGeoset.SelectedIndex == 0)
            {
                bone.Geoset_ID = -1;

            }
            else
            {
                bone.Geoset_ID = int.Parse((ListBoxGeoset.SelectedItem as ListBoxItem).Content.ToString());
            }
            if (ListBoxGeosetAnimation.SelectedIndex == 0)
            {
                bone.Geoset_Animation_ID = -1;

            }
            else
            {
                bone.Geoset_Animation_ID = int.Parse((ListBoxGeosetAnimation.SelectedItem as ListBoxItem).Content.ToString());
            }

            DialogResult = true;
        }
    }
}
