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
    /// Interaction logic for edit_geo_vis.xaml
    /// </summary>
    public partial class edit_geo_vis : Window
    {
        w3Model model_;
        Dictionary<int, bool> visibilities = new Dictionary<int, bool>();
        List<w3Keyframe> keyframes = new List<w3Keyframe>();
        w3Geoset_Animation ga;
        public edit_geo_vis(w3Model model, w3Geoset geo)
        {
            
            
            InitializeComponent();
            model_ = model;
            Title = $"Edit visibiliies of geoset {geo.ID}";
             ga = model.Geoset_Animations.First(x => x.Geoset_ID == geo.ID);
            foreach (w3Sequence seq in model.Sequences)
            {

                ListBoxItem item = new ListBoxItem();
                CheckBox c = new CheckBox();
                c.Content = seq.Name;

                if (ga.Alpha.isStatic == true)
                {
                    if (ga.Alpha.StaticValue[0] < 1)
                    {
                        ga.Alpha.StaticValue[0] = 0;
                        c.IsChecked = false;



                        visibilities.Add(seq.From, true);
                    }
                    if (ga.Alpha.StaticValue[0] >= 1)
                    {
                        ga.Alpha.StaticValue[0] = 1;
                        c.IsChecked = true;


                    }
                }
                else
                {
                    if (ga.Alpha.Keyframes.Any(x=>x.Track == seq.From))
                    {
                        w3Keyframe k = (ga.Alpha.Keyframes.First(x => x.Track == seq.From));
                        c.IsChecked = k.Data[0] < 1 ? false : true;
                    }
                    else
                    {
                        c.IsChecked = true;
                    }
                }

                item.Content = c;
                List_Vis.Items.Add(item);
            }

        }
        private int GetSequenceFrom(string name)
        {
            return model_.Sequences.First(x=>x.Name == name).From;
        }
        private void CollectVisibilities()
        {
            
            foreach (object item in List_Vis.Items)
            {
                ListBoxItem box = item as ListBoxItem;
                CheckBox c = box.Content as CheckBox;
                string name = c.Content.ToString();
                float val = c.IsChecked == true ? 1 : 0;
                int from = GetSequenceFrom(name);
                keyframes.Add(new w3Keyframe(from, [val]));
            }
            ga.Alpha.isStatic = false;
            ga.Alpha.Keyframes = keyframes;

        }

        private void Set(object sender, RoutedEventArgs e)
        {
            CollectVisibilities();
            DialogResult = true;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) { DialogResult = false; }
        }
    }
}