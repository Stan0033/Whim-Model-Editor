using System;
using System.Collections.Generic;
using System.IO;
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
    /// Interaction logic for TextReader.xaml
    /// </summary>
    public partial class TextReaderWindow : Window
    {
        public TextReaderWindow(string file, string title)
        {
            InitializeComponent();
            LoadTextFromFile(file);
            Title = title;
        }
        private void LoadTextFromFile(string filename)
        {
            try
            {
                string[] lines = File.ReadAllLines(filename);
                foreach (var line in lines)
                {
                    // Check if the line starts and ends with square brackets
                    if (line.StartsWith("[") && line.EndsWith("]"))
                    {
                        // Create a bold text block for titles
                        var titleTextBlock = new TextBlock
                        {
                            Text = line.Trim('[', ']'),
                            FontWeight = FontWeights.Bold,
                            Margin = new Thickness(0, 5, 0, 5) // Optional margin for spacing
                        };
                        MainPanel.Children.Add(titleTextBlock);
                    }
                    else
                    {
                        // Create a normal text block for plain text
                        var textBlock = new TextBlock
                        {
                            Text = line,
                            Margin = new Thickness(0, 5, 0, 5) // Optional margin for spacing
                        };
                        MainPanel.Children.Add(textBlock);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error reading file: {ex.Message}");
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) { DialogResult = false; }
        }
    }
}
