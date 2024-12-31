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
    /// Interaction logic for ResizeSequence.xaml
    /// </summary>
    public partial class ResizeSequence : Window
    {
        w3Model model_;
        w3Sequence seq;
        public ResizeSequence(w3Sequence original, w3Model model)
        {
            InitializeComponent();
            model_ = model;
            seq = original; 
            Initial.Text = $"{original.Name}: {original.From} - {original.To}";

           
        }

        private void Resize(object sender, RoutedEventArgs e)
        {
            bool parsed1 = int.TryParse(From.Text, out int from);
            bool parsed2 = int.TryParse(To.Text, out int to);
            if (parsed1 && parsed2)
            {
                if (NewIntervalOverlapping(from, to))
                {
                    MessageBox.Show("The new interval is overlapping with existing sequences", "Invalid request");
                    return;
                }
                else
                {
                    if (from > to) { MessageBox.Show("from cannot be greater than to"); return; }
                    if (from < 0 || to < 0 ) { MessageBox.Show("Cannot use negative values"); return; }

                    int oldFrom = seq.From;
                    int oldTo = seq.To;
                    int oldLength = oldTo - oldFrom;
                    int newLength = to - from;

                    // Set the new sequence interval
                    seq.From = from;
                    seq.To = to;

                    // Update the keyframes to account for the sequence resizing
                    model_.RefreshTransformationsList();
                    foreach (w3Transformation t in model_.Transformations)
                    {
                        foreach (w3Keyframe k in t.Keyframes)
                        {
                            // Check if the keyframe track is within the old interval
                            if (k.Track >= oldFrom && k.Track <= oldTo)
                            {
                                // Calculate the proportional position of the keyframe within the old interval
                                float proportion = (float)(k.Track - oldFrom) / oldLength;

                                // Update the keyframe's track based on the new interval
                                k.Track = (int)(from + proportion * newLength);
                            }
                        }
                    }
                    DialogResult = true;
                }
            }
            else
            {
                MessageBox.Show("Invalid input");
                return;
            }
        }

        private bool NewIntervalOverlapping(int from, int to)
        {
            bool overlapping = false;
            foreach (w3Sequence sequence in model_.Sequences)
            {
                if (sequence == seq) { continue; }

                // Check if the intervals overlap
                if (from <= sequence.To && to >= sequence.From)
                {
                    overlapping = true;
                    break; // No need to check further if an overlap is found
                }
            }
            return overlapping;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) { DialogResult = false; }
        }
    }
}
