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

namespace Whim_Model_Editor
{
    /// <summary>
    /// Interaction logic for editgs.xaml
    /// </summary>
    public partial class editgs : Window
    {
        w3Model wModel;
        public editgs(w3Model model)
        {
            InitializeComponent();
            wModel = model;
            foreach (w3Global_Sequence gs in model.Global_Sequences)
            {
                ListBoxItem i = new ListBoxItem();
                i.Content = $"{gs.ID}: Duration {gs.Duration}";
                list.Items.Add(i);
            }
        }
        int getId()
        {
            ListBoxItem i = list.SelectedItem as ListBoxItem;
            return int.Parse( i.Content.ToString().Split(":")[0]);
        }
        private void Delgs(object sender, MouseButtonEventArgs e)
        {
            if (list.SelectedItem != null)
            {
               int id = getId();
                list.Items.Remove(list.SelectedItem);
                wModel.Global_Sequences.RemoveAll(x=>x.ID == id);

            }
        }

        private void Selctedgs(object sender, SelectionChangedEventArgs e)
        {
            if (list.SelectedItem != null)
            {
                int id = getId();
                input.Text = wModel.Global_Sequences.First(x=>x.ID == id).Duration.ToString();
                

            }
        }
        bool isnum(string s)
        {
            if (s.Trim().Length == 0) { return false; }
            foreach (char c in s)
            {
                if (char.IsDigit(c) == false) { return false; }
               
            }
            return true;
        }
        private void update(object sender, RoutedEventArgs e)
        {
            if (list.SelectedItem != null)
            {
                if (input.Text.Trim().Length == 0) {return; }
                int id = getId();
                if (isnum(input.Text) == false) { MessageBox.Show("input not an integer"); return; }
                int written = int.Parse( input.Text );
                if  (written  <= 0){ MessageBox.Show("A global sequence with no duration is not allowed"); return; }
               
                string updated = $"{id}: Duration {written}";
                wModel.Global_Sequences.First(x => x.ID == id).Duration = written; ;
                ListBoxItem i = list.SelectedItem as ListBoxItem;
                i.Content = updated;
            }
        }

        private void add(object sender, RoutedEventArgs e)
        {
            if (isnum(input.Text) == false) { MessageBox.Show("input not an integer"); return; }
            int written = int.Parse(input.Text);
            if (written <= 0) { MessageBox.Show("A global sequence with no duration is not allowed"); return; }
            if (wModel.Global_Sequences.Any(x => x.Duration == written))
            { MessageBox.Show("There is already a global sequence with this duration"); return; }
            ListBoxItem i = new ListBoxItem();
            w3Global_Sequence gs = new w3Global_Sequence();
            gs.Duration = written;
            gs.ID = IDCounter.Next();
            i.Content = $"{gs.ID}: Duration {gs.Duration}";
            wModel.Global_Sequences.Add(gs);
            list.Items.Add(i);
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) { DialogResult = false; }
        }
    }
}
