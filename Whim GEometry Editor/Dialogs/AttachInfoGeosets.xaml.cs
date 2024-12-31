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
    /// Interaction logic for AttachInfoGeosets.xaml
    /// </summary>
     
    public partial class AttachInfoGeosets : Window
    {
        Dictionary<int, List<string>> GeosetLists = new Dictionary<int, List<string>>();    
        public AttachInfoGeosets(List<w3Geoset> geosets, List<w3Node > nodes)
        {
            InitializeComponent();

            foreach (w3Geoset geoset in geosets)
            {
                int id = geoset.ID;
                List<string> data = new List<string>();
                foreach (w3Vertex v in geoset.Vertices)
                {
                    List<string> nodenames = new List<string>();
                    foreach (int nid in v.AttachedTo)
                    {
                        nodenames.Add(nodes.First(x=>x.objectId == nid).Name);
                    }
                    data.Add($"vertex {v.Id} ({v.Position.X}, {v.Position.Y}, {v.Position.Z}): {string.Join(",", nodenames)}");
                }
                GeosetLists.Add(id, data);
            }
            foreach (var item in GeosetLists)
            {
                List_Geosets.Items.Add(new ListBoxItem() {Content= item.Key });
            }

        }

        private void SelectedGeoset(object sender, SelectionChangedEventArgs e)
        {
            if (List_Geosets.SelectedItem != null) {
                int selected =int.Parse( (List_Geosets.SelectedItem as ListBoxItem).Content.ToString());
                List_Vertices.Items.Clear();
                foreach (string item in GeosetLists[selected]) 
                {
                    List_Vertices.Items.Add(new ListBoxItem() { Content = item});
                }
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) { DialogResult = false; }
        }
    }
}
