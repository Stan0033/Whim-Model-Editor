using MDLLib;
using MDLLibs.Classes.Misc;
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
    /// Interaction logic for SplitSEquenceDialog.xaml
    /// </summary>
    public partial class SplitSEquenceDialog : Window
    {
        w3Model w3Model;
        w3Sequence sequence;
        public SplitSEquenceDialog(w3Sequence whichSequence, w3Model model)
        {
            InitializeComponent();
            w3Model = model;
            sequence = whichSequence;
            Description.Text = $"Splitting sequence \"{whichSequence.Name}\":\n{whichSequence.From} - {whichSequence.To}";
        }

        private void ok(object sender, RoutedEventArgs e)
        {
            string one = Input1.Text.Trim();
            string two = Input2.Text.Trim();
            bool parsed = int.TryParse(InputTrack.Text, out int Track);
            if (parsed)
            {
                if (SequenceExists(one)) { MessageBox.Show("There is already a sequence with the first name"); return; }
                if (SequenceExists(two)) { MessageBox.Show("There is already a sequence with the second name"); return; }
                //unfinished
                if (Track < sequence.From || Track > sequence.To)
                {
                    MessageBox.Show("This track is not contained in the sequence"); return;
                } 
                if (Track == sequence.From)
                {
                    MessageBox.Show("The target track at which you split cannot be the starting track"); return;
                }
                if (Track == sequence.To)
                {
                    MessageBox.Show("The target track at which you split cannot be the ending track"); return;
                }
                if (Minimum10Frames(sequence.From, sequence.To, Track) == false)
                {
                    MessageBox.Show("Minimum 10 frames between the splitting track and either from or to", "Invalid request"); return;
                }
                one = StringHelper.CapitalizeName(one);
                two = StringHelper.CapitalizeName(two);
                w3Sequence sequence1 = new w3Sequence();    
               sequence1.From = sequence.From;
                sequence1.To = Track;
                sequence1.Name = one;
                sequence1.Looping = sequence.Looping;
                sequence1.Extent = sequence.Extent.Clone();
                w3Sequence sequence2 = new w3Sequence();
                sequence2.From = sequence1.To + 1;
                sequence2.To = sequence.To;
                sequence2.Name = two;
                sequence2.Looping = sequence.Looping;
                sequence2.Extent = sequence.Extent.Clone();
                w3Model.Sequences.Remove(sequence);
                w3Model.Sequences.Add(sequence1);
                w3Model.Sequences.Add(sequence2);

                DialogResult = true;
            }
            else
            {
                MessageBox.Show("Invalid input for track"); return;
            }
        
        }
        private bool SequenceExists(string sequenceName)
        {
            foreach (w3Sequence seq in w3Model.Sequences)
            {
                if (seq.Name.ToLower() == sequenceName.ToLower())
                {
                    return true;
                }
            }
            return false;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) { DialogResult = false; }
        }
        private bool Minimum10Frames(int from, int to, int splitat)
        {
            // Ensure there are at least 10 frames between 'splitat' and 'from'
            // or 'splitat' and 'to'
            if (Math.Abs(splitat - from) >= 10 || Math.Abs(splitat - to) >= 10)
            {
                return true;
            }

            // If neither condition is met, return false
            return false;
        }

    }
}
