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
    /// Interaction logic for DuplicateSequence.xaml
    /// </summary>
    public partial class DuplicateSequence : Window
    {
        public int GivenValue;
        public DuplicateSequence()
        {
            InitializeComponent();
        }

        private void ok(object sender, RoutedEventArgs e)
        {
            bool parsed = int.TryParse(InputTrack.Text, out int track);
            if (parsed)
            {
                GivenValue = track;
                if (InputName.Text.Trim().Length > 0)
                {
                    DialogResult = true;
                }
                else
                {
                    { return; }
                }
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) { DialogResult = false; }
        }
    }
}
