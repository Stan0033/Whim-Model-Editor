using MDLLib;
using MDLLibs.Classes.Misc;
using System;
using System.Collections.Generic;
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

namespace Whim_GEometry_Editor.Dialogs
{
    /// <summary>
    /// Interaction logic for MultipleSequence.xaml
    /// </summary>
    public partial class MultipleSequence : Window
    {
        List<w3Sequence> Allsequences;
        private int FirstAvailableFrom;
        private int FirstAvailableFromInitial;
        public MultipleSequence(List<w3Sequence> allsequences)
        {
            InitializeComponent();
            Allsequences = allsequences;
            FirstAvailableFrom = GetFirstAvailableFrom();
            FirstAvailableFromInitial = FirstAvailableFrom;
        }

        private int GetFirstAvailableFrom()
        {
            int start = 0;
            foreach (w3Sequence sequence in Allsequences)
            {
                start = Math.Max(start, sequence.To);
            }
            return start + 1; // Increment by 1 to get the first available integer.
        }

       

        private class WrittenSequence
        {
            public string Name;
            public int duration;
            public WrittenSequence(string n, int d)
            {
                Name = n;
                duration = d;
            }
        }
        private void ok(object sender, RoutedEventArgs e)
        {
            List<w3Sequence> sequences = new List<w3Sequence>();
            List<string> lines = Box.Text.Split('\n').Select(x => x.Trim()).ToList();
            lines.RemoveAll(x=>x.Length == 0);
            List<WrittenSequence> objects = ExtractSEquences(lines);
            if (objects.Count == 0) {return; }
            foreach (WrittenSequence writtenSequence in objects)
            {
                if (Allsequences.Any(x => x.Name.ToLower() == writtenSequence.Name.ToLower()))
                {
                    MessageBox.Show($"There is already a sequence with this name", "Invalid request");
                    sequences.Clear(); return;
                }
               if (FirstAvailableFrom + writtenSequence.duration > 999999)
                {
                    MessageBox.Show($"The would-be interval for sequence '{writtenSequence.Name}' would exceed 999,999", "Invalid request");
                    sequences.Clear(); FirstAvailableFrom = FirstAvailableFromInitial; return;
                }
               
                 w3Sequence sequence = new w3Sequence();
                sequence.From = FirstAvailableFrom;
                sequence.To = FirstAvailableFrom+writtenSequence.duration;
                FirstAvailableFrom = FirstAvailableFrom + writtenSequence.duration + 1;
                sequence.Name =  StringHelper.CapitalizeName(writtenSequence.Name);
                sequences.Add(sequence);
            }
             
            Allsequences.AddRange( sequences );
            DialogResult = true;

        }
        
        private List<WrittenSequence> ExtractSEquences(List<string> lines)
        {
            List < WrittenSequence > sequences = new List<WrittenSequence>();
            int lineNumber = 0;
            foreach (string line in lines)
            {
                string[] parts = line.Split(" - ");
                if (parts.Length == 2)
                {
                    bool hasNum = int.TryParse(parts[1], out int num);
                    if (hasNum)
                    {
                        WrittenSequence ws = new(parts[0], num);
                        sequences.Add(ws);
                    }
                   else {
                        MessageBox.Show($"Right side string at line {lineNumber} is not an integer. use 'sequence name - duration' with the space");
                        return new List<WrittenSequence>();
                    }
                    
                }
                else
                {
                    MessageBox.Show($"Incorrent format at line {lineNumber}. use 'sequence name - duration' with the spaces");
                    return new List<WrittenSequence>();
                        
                    
                }
                lineNumber++;
            }
            return sequences;
        }

        public static bool IsValidFormat(string input)
        {
            // Define a regular expression pattern to match the required format
            string pattern = @"^[a-zA-Z\s]+\s\[\d+\s-\s\d+\]$";

            // Use Regex to check if the input matches the pattern
            Regex regex = new Regex(pattern);
            return regex.IsMatch(input);
        }
         
    }
}
