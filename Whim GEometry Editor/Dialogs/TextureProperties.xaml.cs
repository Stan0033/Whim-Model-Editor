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
    /// Interaction logic for TextureProperties.xaml
    /// </summary>
    public partial class TextureProperties : Window
    {
        w3Texture t;
        bool Init = true;
        public TextureProperties(w3Texture tex)
        {
            InitializeComponent();
            t = tex;
            Check_W.IsChecked = t.Wrap_Width;
            Check_H.IsChecked = t.Wrap_Height;
            Init = false;
        }

        private void Check_WW(object sender, RoutedEventArgs e)
        {
            if (Init) return;
            t.Wrap_Width = Check_W.IsChecked == true;
            
        }

        private void Check_WH(object sender, RoutedEventArgs e)
        {
            if (Init) return;
            t.Wrap_Height = Check_H.IsChecked == true;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) { DialogResult = false; }
        }
    }
}
