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
    /// Interaction logic for newModel.xaml
    /// </summary>
    public partial class NewModelWindow : Window
    {
        w3Model Model;
        public NewModelWindow(w3Model model)
        {
            InitializeComponent();
            Model = model;
        }

        private void SelectedType(object sender, SelectionChangedEventArgs e)
        {
            Uncheck(List_Sequences);
            Uncheck(List_Attachments);
            switch (List_Type.SelectedIndex)
            {
                case 0:
                    
                    break;
                 case 1:
                    Check(List_Attachments, Overhead, Head, Chest, Feet, HandLeft, HandRight);
                    Check(List_Sequences, Dissipate, Attack, DecayBone, DecayFlesh, Portrait, PortraitTalk, Walk);
                    break;
                case 2 :
                    Check(List_Attachments, Overhead, Head, Chest, Feet, HandLeft, HandRight, Sprite1, Sprite2, Sprite3, Sprite4);
                    Check(List_Sequences, Dissipate, Attack, DecayBone, DecayFlesh, Portrait, PortraitTalk, Walk);
                    break;
                case 3:
                    Check(List_Attachments, Overhead, Head, Chest, Feet, HandLeft, HandRight);
                    Check(List_Sequences, Attack, DecayBone, DecayFlesh, Portrait, PortraitTalk, Walk);
                    break;
                case 4:
                    Check(List_Attachments, Overhead, Head, Chest, Feet, HandLeft, HandRight, Sprite1, Sprite2, Sprite3, Sprite4);
                    Check(List_Sequences, Attack, DecayBone, DecayFlesh, Portrait, PortraitTalk, Walk);
                    break;
                case 5:
                    Check(List_Attachments, Overhead, Head, Chest, Feet, HandLeft, HandRight);
                    Check(List_Sequences, Attack, DecayBone, DecayFlesh, Portrait, PortraitTalk, Walk, AttackGold, AttackLumber, StandGold, StandLumber, StandWork, StandWorkLumber, WalkGold, WalkLumber);
                    break;
                case 6:
                    Check(List_Attachments, Overhead, Head, Chest, Feet, HandLeft, HandRight, Sprite1, Sprite2, Sprite3, Sprite4);
                    Check(List_Sequences, Attack, DecayBone, DecayFlesh, Portrait, PortraitTalk, Walk,AttackGold, AttackLumber, StandGold, StandLumber, StandWork, StandWorkLumber, WalkGold, WalkLumber);
                    break;
                case 7:
                    Check(List_Attachments, Overhead);
                    Check(List_Sequences, Attack, Decay, Portrait, PortraitTalk, Birth); break;
                case 8:
                    Check(List_Attachments, Overhead, Sprite1, Sprite2, Sprite3, Sprite4);
                    Check(List_Sequences, Attack, Decay, Portrait, PortraitTalk, Birth); break;
                case 9:break;
                case 10:break;
                case 11:break;
                case 12 :break;
                case 13: Check(List_Sequences, DeathAlternate);     break;
                case 14: Check(List_Sequences, Birth);     break;
                case 15: Check(List_Sequences, Birth);     break;
            }
        }

        private void Create(object sender, RoutedEventArgs e)
        {
            foreach (string s in ExtractAttachments(List_Attachments))
            {
                w3Node node = new w3Node();
                node.Data = new w3Attachment();
                node.Name = s;
                node.objectId = IDCounter.Next();
                Model.Nodes.Add(node);
            }
            int currentTime = 0;
            foreach (string s in ExtractNames(List_Sequences))
            {
                w3Sequence seq = new w3Sequence();
                seq.Name = s;
                seq.From = currentTime;
                seq.To = currentTime + 999;
                currentTime += 1000;
                Model.Sequences.Add(seq);
            }
            Model.Name = ModelNAme.Text.Trim();

            if (TierPicker.SelectedIndex > 0) {
                List<w3Sequence> tiered = new List<w3Sequence>();

                foreach (w3Sequence seq in Model.Sequences)
                {
                    if (TierPicker.SelectedIndex >= 1)
                    {
                        string name = seq.Name + " Upgrade First";
                        w3Sequence tieredSeq = new w3Sequence();
                        tieredSeq.Name = name;
                        tieredSeq.From = currentTime;
                        tieredSeq.To = currentTime + 999;
                        currentTime += 1000;
                        tiered.Add(tieredSeq);
                    }
                    if (TierPicker.SelectedIndex >= 2)
                    {
                        string name = seq.Name + " Upgrade Second";
                        w3Sequence tieredSeq = new w3Sequence();
                        tieredSeq.Name = name;
                        tieredSeq.From = currentTime;
                        tieredSeq.To = currentTime + 999;
                        currentTime += 1000;
                        tiered.Add(tieredSeq);
                    }
                    if (TierPicker.SelectedIndex >= 3)
                    {
                        string name = seq.Name + " Upgrade Third";
                        w3Sequence tieredSeq = new w3Sequence();
                        tieredSeq.Name = name;
                        tieredSeq.From = currentTime;
                        tieredSeq.To = currentTime + 999;
                        currentTime += 1000;
                        tiered.Add(tieredSeq);
                    }
                    if (TierPicker.SelectedIndex >= 4)
                    {
                        string name = seq.Name + " Upgrade Fourth";
                        w3Sequence tieredSeq = new w3Sequence();
                        tieredSeq.Name = name;
                        tieredSeq.From = currentTime;
                        tieredSeq.To = currentTime + 999;
                        currentTime += 1000;
                        tiered.Add(tieredSeq);
                    }
                   
                }
                if (Check_Morph.IsChecked == true)
                {
                    w3Sequence morph = new w3Sequence();
                    morph.Name = "Morph";
                   
                    morph.To = currentTime + 999;
                    currentTime += 1000;
                    foreach (w3Sequence seq in Model.Sequences)
                    {
                        seq.Name = seq.Name + " Alternate";

                        seq.To = currentTime + 999;
                        currentTime += 1000;
                    }
                }
                Model.Sequences.AddRange(tiered);
            }
            
            DialogResult = true;
         }
        private void Uncheck(ListBox list)
        {
             
            foreach (object item in list.Items)
            {
                CheckBox c = item as CheckBox;
                if (c.IsEnabled) {c.IsChecked = false;} 
            }
        }
        private void Check(ListBox list, params CheckBox[] checkboxes)
        {
            foreach (CheckBox check in checkboxes) check.IsChecked = true;
            
            
        }
        private List<string> ExtractAttachments(ListBox list)
        {
            List<string> nlist = new List<string>();
            foreach (object item in list.Items)
            {
                CheckBox c = item as CheckBox;
                if (c.IsChecked == true)
                {
                    string name = c.Content.ToString().Split("(")[0];
                     nlist.Add(name);
                }
            }
            return nlist;
        }
        private List<string> ExtractNames(ListBox list)
        {
            List<string> nlist = new List<string>();
            foreach (object item in list.Items)
            {
                CheckBox c = item as CheckBox;
                if (c.IsChecked == true)
                {
                    string name = c.Content.ToString().Split("(")[0];
                    nlist.Add(name);
                }
            }
            return nlist;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) { DialogResult = false; }
        }
    }
}
