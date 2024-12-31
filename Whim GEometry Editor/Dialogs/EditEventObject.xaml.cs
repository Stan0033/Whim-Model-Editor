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
using Whim_GEometry_Editor.Misc;

namespace Whim_GEometry_Editor.Dialogs
{
    /// <summary>
    /// Interaction logic for EditEventObject.xaml
    /// </summary>
    public partial class EditEventObject : Window
    {
        w3Node Node;
        Event_Object ev;
        w3Model  Model;
        public EditEventObject(w3Node node, w3Model model)
        {
            InitializeComponent();
            Node = node;
            Model = model;
            ev = node.Data as Event_Object;
             
            AddAlphabetToComboBox(ListIdentifiers);
          
            FillListsAndSelectIitem();
            FillExistingTracks();
            SetVAlueBasedOnName();
            FillAllGlobalSEquences();
            SetGlobalSequence();

        }
        private void FillAllGlobalSEquences()
        {
            ListGlobalSequences.Items.Add(new ComboBoxItem() { Content = "(None)" });
            foreach (w3Global_Sequence s in Model.Global_Sequences)
            {
                ListGlobalSequences.Items.Add(new ComboBoxItem() { Content = s.ID.ToString() });
            }
        }
        private void SetGlobalSequence()
        {
            if (Model.Global_Sequences.Any(x=>x.ID == ev.Global_sequence_ID)){
                int index = Model.Global_Sequences.FindIndex(x=>x.ID == ev.Global_sequence_ID) +1;
                ListGlobalSequences.SelectedIndex= index;
            }
            else
            {
                ListGlobalSequences.SelectedIndex = 0;
            }
        }
        private void SetVAlueBasedOnName()
        {
          if (Node.Name.Length == 8)
            {
                char identifier = Node.Name[3];
                SelectComboBoxItemByCharacter(ListIdentifiers, identifier);

            } 
        }
        public void SelectComboBoxItemByCharacter(ComboBox comboBox, char letter)
        {
            // Loop through all ComboBoxItems in the ComboBox
            foreach (var item in comboBox.Items)
            {
                ComboBoxItem comboBoxItem = item as ComboBoxItem;
                if (comboBoxItem != null && comboBoxItem.Content.ToString() == letter.ToString())
                {
                    // Set the selected item to the matching ComboBoxItem
                    comboBox.SelectedItem = comboBoxItem;
                    return;
                }
            }
        }
            public void AddAlphabetToComboBox(ComboBox comboBox)
        {
            // Clear any existing items in the ComboBox
            comboBox.Items.Clear();

            // Loop through A-Z (uppercase letters)
            for (char letter = 'a'; letter <= 'z'; letter++)
            {
                // Create a new ComboBoxItem
                ComboBoxItem item = new ComboBoxItem
                {
                    // Set the header to the letter
                    Content = letter.ToString()
                };

                // Add the item to the ComboBox
                comboBox.Items.Add(item);
            }
        }
            private void FillListsAndSelectIitem()
        {
            foreach ( var item in EventObjectHelper.Data)
            {
                List_EventCategories.Items.Add(new ListBoxItem() { Content = item.Key.ToString() });
            }
            if (Node.Name.Length == 8) {
                string category = Node.Name.Substring(0, 3);
                string type = Node.Name.Substring(4, 4);
                
                switch (category)
                {
                    case "FPT":

                        foreach (string item in EventObjectHelper.FootprintData)
                        {
                            List_EventTypes.Items.Add(new ListBoxItem() { Content = item });
                        }
                        SelectTypeFromString(List_EventTypes, type);
                        break;
                    case "SPL":
                        foreach (string item in EventObjectHelper.BloodSplats)
                        {
                            List_EventTypes.Items.Add(new ListBoxItem() { Content = item });
                        }
                        SelectTypeFromString(List_EventTypes, type);
                        break;
                    case "UBR":
                        foreach (string item in EventObjectHelper.ubersplatData)
                        {
                            List_EventTypes.Items.Add(new ListBoxItem() { Content = item });
                        }
                        SelectTypeFromString(List_EventTypes, type);
                        break;
                    case "SND":
                        foreach (string item in EventObjectHelper.soundEffects)
                        {
                            List_EventTypes.Items.Add(new ListBoxItem() { Content = item });
                        }
                        SelectTypeFromString(List_EventTypes, type);
                        break;
                    case "SPN":
                        foreach (string item in EventObjectHelper.SpawnObjects)
                        {
                            List_EventTypes.Items.Add(new ListBoxItem() { Content = item });
                        }
                        SelectTypeFromString(List_EventTypes, type);
                        break;
                }
            }
            else
            {
                foreach (string item in EventObjectHelper.FootprintData)
                {
                    List_EventTypes.Items.Add(new ListBoxItem() { Content = item });
                }
            }
           
        }
        private void SelectTypeFromString(ListBox list, string data)
        {
            for (int i = 0; i < list.Items.Count; i++)
            {
                ListBoxItem item = list.Items[i] as ListBoxItem;
                string name = item.Content.ToString();
                if (name.StartsWith(data))
                {
                    list.SelectedIndex = i;
                    list.UpdateLayout();
                    list.ScrollIntoView(list.Items[i]);
                    break;
                }
            }
        }
        string GetCategory()
        {
            return (List_EventCategories.SelectedItem as ListBoxItem).Content.ToString();
        }
        string GetData()
        {
            return (List_EventTypes.SelectedItem as ListBoxItem).Content.ToString();
        }
        private void Finalize(object sender, RoutedEventArgs e)
        {
            if (List_EventTypes.SelectedItem == null || List_EventCategories.SelectedItem == null)
            {
                MessageBox.Show("Select category and data");return;
            }
            //get identifier
            char selected = (ListIdentifiers.SelectedItem as ComboBoxItem).Content.ToString()[0];
            ev.Identifier = selected;
            // change name
            Node.Name = GetCategory() + selected + GetData();
            //get tracks
            List<int> tracks = CollectTracks();
            ev.Tracks = tracks;
           
            // change global sequence
            if (ListGlobalSequences.SelectedIndex> 0)
            {
                ev.Global_sequence_ID = Model.Global_Sequences[ListGlobalSequences.SelectedIndex - 1].ID;
            }
            DialogResult = true;
        }
        private void FillExistingTracks()
        {
            foreach (int track in ev.Tracks)
            {
                List_Tracks.Items.Add(new ListBoxItem() { Content = track.ToString() });
            }
        }
        private int GetTrack()
        {
            ListBoxItem item = List_Tracks.SelectedItem as ListBoxItem;
            return int.Parse(item.Content.ToString());
        }
        private void RefreshTracks()
        {
            List_Tracks.Items.Clear();
            ev.Tracks = ev.Tracks.OrderBy(x=>x).ToList();
            foreach (var track in ev.Tracks)
            {
                List_Tracks.Items.Add(new ListBoxItem() { Content = track });
            }
        }
        private void AddTrack(object sender, RoutedEventArgs e)
        {
            try
            {
                int track = int.Parse(Input_Track.Text);
                if (ListHasValue(track))
                {
                    MessageBox.Show("There is already a track with this value", "Invalid request"); return;
                }
                if (Model.Sequences.Any(x => track >= x.From && track <= x.To) == false)
                {
                    MessageBox.Show("This track is not part of any sequence", "Invalid request"); return;
                }
                List_Tracks.Items.Add(new ListBoxItem() { Content = track.ToString() });
                RefreshTracksInList();


            }
            catch { MessageBox.Show("Input must be an integer", "Invalid request"); }
        
        }
        private void RefreshTracksInList()
        {
            // Get the ListBox items and convert them to a list of integers
            List<int> itemValues = new List<int>();

            foreach (ListBoxItem item in List_Tracks.Items)
            {
                // Assume item.Content is an integer (or can be parsed as one)
                if (int.TryParse(item.Content.ToString(), out int value))
                {
                    itemValues.Add(value);
                }
            }

            // Sort the list of integers
            itemValues.Sort();

            // Clear the ListBox and re-add the sorted items
            List_Tracks.Items.Clear();

            foreach (int value in itemValues)
            {
                List_Tracks.Items.Add(new ListBoxItem { Content = value });
            }
        }

        private bool ListHasValue(int which)
        {
            foreach (object item in List_Tracks.Items)
            {
                ListBoxItem i = item as ListBoxItem;
                int value = int.Parse(i.Content.ToString());
                if (which == value) { return true; }

            }
            return false;
        }
        private List<int>   CollectTracks()
        {
            List<int> itemValues = new List<int>();

            foreach (ListBoxItem item in List_Tracks.Items)
            {
                // Assume item.Content is an integer (or can be parsed as one)
                if (int.TryParse(item.Content.ToString(), out int value))
                {
                    itemValues.Add(value);
                }
            }
            return itemValues;
        }

        private void DelTrack(object sender, RoutedEventArgs e)
        {
            if (List_Tracks.SelectedItem != null)
            {

                int Track = GetTrack();
               
                List_Tracks.Items.Remove(List_Tracks.SelectedItem);
            }
        }

        private void SelectedCategory(object sender, SelectionChangedEventArgs e)
        {
            List_EventTypes.Items.Clear();
            ListBoxItem selectedItem = List_EventCategories.SelectedItem as ListBoxItem;
            string name = selectedItem.Content.ToString().Substring(0,3);
            switch (name)
            {


                case "FPT":

                    foreach (string item in EventObjectHelper.FootprintData)
                    {
                        List_EventTypes.Items.Add(new ListBoxItem() { Content = item });
                    }
                     
                    break;
                case "SPL":
                    foreach (string item in EventObjectHelper.BloodSplats)
                    {
                        List_EventTypes.Items.Add(new ListBoxItem() { Content = item });
                    }
                  
                    break;
                case "UBR":
                    foreach (string item in EventObjectHelper.ubersplatData)
                    {
                        List_EventTypes.Items.Add(new ListBoxItem() { Content = item });
                    }
                   
                    break;
                case "SND":
                    foreach (string item in EventObjectHelper.soundEffects)
                    {
                        List_EventTypes.Items.Add(new ListBoxItem() { Content = item });
                    }
                    
                    break;
                case "SPN":
                    foreach (string item in EventObjectHelper.SpawnObjects)
                    {
                        List_EventTypes.Items.Add(new ListBoxItem() { Content = item });
                    }break;
                   
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) { DialogResult = false; }
        }
    }
}
