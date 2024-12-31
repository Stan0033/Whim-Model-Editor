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

namespace Whim_GEometry_Editor.Dialogs
{
    /// <summary>
    /// Interaction logic for EditGeoset_Propertoes.xaml
    /// </summary>
    public partial class EditGeoset_Propertoes : Window
    {
        w3Geoset currentGeoset;
        public EditGeoset_Propertoes(MDLLib.w3Geoset geo)
        {
            InitializeComponent();
            currentGeoset = geo;
            InputSelectionGroup.Text = geo.Selection_Group.ToString();
            Check_UnSelectable1.IsChecked = geo.Unselectable;
        }

        private void SetSelectable(object sender, RoutedEventArgs e)
        {
            currentGeoset.Unselectable = Check_UnSelectable1.IsChecked ==  true;
        }

        private void SetSelectionGroup(object sender, TextChangedEventArgs e)
        {
            bool parsed = int.TryParse(InputSelectionGroup.Text, out int value);
            if (parsed)
            {
                currentGeoset.Selection_Group = value;
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) { DialogResult = false; }
        }
    }
}
