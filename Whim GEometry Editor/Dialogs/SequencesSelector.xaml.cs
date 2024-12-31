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
    /// Interaction logic for SequencesSelector.xaml
    /// </summary>
    public partial class SequencesSelector : Window
    {
        public List<int> indexes = new List<int>();
        List<w3Sequence> full;
        public string NewName;
        public SequencesSelector(string title, List<w3Sequence> sequences)
        {
            Title = title;
            foreach (w3Sequence sequence in sequences)
            {
                List_Sequences.Items.Add(new ListBoxItem() { Content = sequence.Name });
            }
            full = sequences;
            InitializeComponent();
        }

        private void ok(object sender, RoutedEventArgs e)
        {
            NewName = NameInput.Text.Trim();
            if (NewName.Length == 0 || full.Any(x => x.Name.ToLower() == NewName)) { MessageBox.Show("Input a valid name"); return; }
            if (List_Sequences.SelectedItems.Count < 2) { return; }
            else
            {

                foreach (var selectedItem in List_Sequences.SelectedItems)
                {
                    int index = List_Sequences.Items.IndexOf(selectedItem);
                    indexes.Add(index);
                }
            }
        }
    }
}
