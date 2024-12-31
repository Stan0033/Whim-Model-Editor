using MDLLib;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    /// Interaction logic for sequence.xaml
    /// </summary>
    public partial class sequenceWindow : Window
    {
        List<w3Sequence> sqs = new List<w3Sequence>();
        public bool Editing = false;
        public string OriginalName = "";
        public w3Sequence AcceptedSequence;
        public sequenceWindow(List<w3Sequence> list, bool hasScale = false)
        {
            InitializeComponent();
            sqs = list;
            check_rescaleK.Visibility  = hasScale ? Visibility.Visible : Visibility.Collapsed;
        }

        private void setok(object sender, RoutedEventArgs e)
        {
            string name = CapitalizeWords(text_name.Text);
            if (name.Length == 0) { MessageBox.Show("Input a name"); return; }
            if (char.IsLetter(name[0]) == false) { MessageBox.Show("The first letteer of a name must be a character"); return; }
            if (IsValidStringWithIntegerAtEnd(name) == false) { MessageBox.Show("An integer can only be at the end of the name"); return; }
            if (Editing)
            {
                if (sqs.Any(x => x.Name == name && x.Name != OriginalName))
                {
                    MessageBox.Show("There is already a sequence with this name", "Invalid request");
                    return; // Exit if the name is invalid
                }
            }
            else
            {
                if (sqs.Any(x => x.Name == name))
                {
                    MessageBox.Show("There is already a sequence with this name", "Invalid request");
                    return; // Exit if the name is invalid
                }
            }
           
            
            // Parse range's text, ensuring it is in the format "from - to"
            string[] rangeParts = text_range.Text.Split(new[] { " - " }, StringSplitOptions.RemoveEmptyEntries);

            // Ensure there are exactly 2 parts after splitting
            if (rangeParts.Length != 2)
            {
                MessageBox.Show("Invalid range format. Please use 'from - to'.", "Invalid request");
                return; // Exit if the range format is invalid
            }

            // Attempt to parse both parts into integers, checking for failure
            if (!int.TryParse(rangeParts[0], out int from) || !int.TryParse(rangeParts[1], out int to))
            {
                MessageBox.Show("Invalid range values. Please ensure both 'from' and 'to' are valid integers.", "Invalid request");
                return; // Exit if parsing fails
            }
            if (from > 999999 || to > 999999)
            {
                MessageBox.Show("'From' or 'To' are not allowed to be greater than 999,999. If you have many sequences and there are no more free interval, from the inspector check for free gaps/intervals.");
                return;
            }
            if (!RangeOK(from, to))
            {
                MessageBox.Show("That interval has conflicts with existing ones or is invalid.", "Invalid request");
                return; // Exit if the range is not okay
            }

            // Continue processing if everything is valid
            // Example: Add the new sequence to the list
            bool loop = Check_looping.IsChecked == true;
            AcceptedSequence = new w3Sequence { Name = name, From = from, To = to, Looping = loop };
             
             
            DialogResult = true;
        }
        public static bool IsValidStringWithIntegerAtEnd(string input)
        {
            bool hasDigit = false;
             foreach (char c in input)
            {
                if (char.IsDigit(c))
                {
                    hasDigit = true; break;
                }
            }
            if (hasDigit)
            {
                return System.Text.RegularExpressions.Regex.IsMatch(input, @"^.*\s\d+$");
            }else { return true; }
        }
        private bool RangeOK(int from, int to)
        {
            if (from == to) return false;
            if (from < 0 || to < 0) return false;
            if (from > to) return false;

            foreach (w3Sequence seq in sqs)
            {
                if (Editing)
                {
                    if (seq.Name == OriginalName) { continue; }
                }
                // Check if the new range overlaps with existing sequences
                if ((from >= seq.From && from <= seq.To) || (to >= seq.From && to <= seq.To) || (from <= seq.From && to >= seq.To))
                {
                    return false;
                }
            }
            return true;
        }

        public static string CapitalizeWords(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return string.Empty;
            }

            // Trim the input string
            string trimmedInput = input.Trim();

            // Capitalize the first letter of each word
            TextInfo textInfo = CultureInfo.CurrentCulture.TextInfo;
            string capitalizedOutput = textInfo.ToTitleCase(trimmedInput.ToLower());

            return capitalizedOutput;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) { DialogResult = false; }
        }
    }
}
