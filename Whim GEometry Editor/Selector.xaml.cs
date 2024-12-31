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
using System.Windows.Shapes;

namespace Whim_Model_Editor
{
    /// <summary>
    /// Interaction logic for Selector.xaml
    /// </summary>
    public partial class Selector : Window
    {
        public string Selected;
        public Selector(List<string> items)
        {
            InitializeComponent();
            foreach (var item in items)
            {
                box.Items.Add(new ListBoxItem() {Content= item });
            }
        }

        private void Set(object sender, RoutedEventArgs e)
        {
            if (box.SelectedItems.Count > 0)
            {
                Selected = (box.SelectedItem as ListBoxItem).Content.ToString();
                DialogResult = true;
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) { DialogResult = false; }
        }
    }
}
