using Dsafa.WpfColorPicker;
using MDLLib;
using MDLLibs.Classes.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
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
using Whim_GEometry_Editor;

namespace Whim_Model_Editor
{
    /// <summary>
    /// Interaction logic for geosetanims.xaml
    /// </summary>
    public partial class geosetanims : Window
    {
        w3Model model;
        w3Geoset_Animation CurrentAnim;
        public geosetanims(w3Model _model)
        {
            InitializeComponent();
            Check_AlphaStatic.IsChecked = true;
            model = _model;
            foreach (w3Geoset g in model.Geosets)
            {
                List_GAs.Items.Add(new ListBoxItem() { Content = g.ID.ToString() });
            }
            
        }
        private void clearButton(object sender, RoutedEventArgs e)
        {
            if (CurrentAnim != null)
            {
                CurrentAnim.Alpha.isStatic = true;
                CurrentAnim.Alpha.Keyframes.Clear();
                CurrentAnim.Alpha.StaticValue = [1];
             
                Check_AlphaStatic.IsChecked = true;
                staticTextBox.Text = "100";
                
            }
        }
        private void GetSelectedGA()
        {
            int id = int.Parse((List_GAs.SelectedItem as ListBoxItem).Content.ToString());

            CurrentAnim= model.Geoset_Animations.First(a => a.Geoset_ID == id);
        }
         
        public List<string> ExtractLinesFromRichTextBox(RichTextBox richTextBox)
        {
            // Create a list to store the lines
            List<string> lines = new List<string>();

            // Get the text from the RichTextBox
            var textRange = new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd);
            string text = textRange.Text.Trim();  // Trim any unnecessary whitespace

            // Split the text into lines using both carriage return and newline characters
            string[] splitLines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            // Add each line to the list
            foreach (string line in splitLines)
            {
                lines.Add(line.Trim());  // Trim each line and add it to the list
            }

            return lines;
        }

        private void AddSelectedSequence(object sender, MouseButtonEventArgs e)
        {

        }

        private void SelectedGeoset(object sender, SelectionChangedEventArgs e)
        {
            if (List_GAs.SelectedItem != null) {
                Check_AlphaStatic.IsEnabled = true;
                ColorStatic.IsEnabled = true;
                GetSelectedGA();
                if (CurrentAnim.Color.isStatic)
                {
                    float[] rgb = GetAsRGB(CurrentAnim.Color.StaticValue);
                    
                    Brush Color = Converters.IntArrayToBrush(rgb);
                    ColorButton.IsEnabled = true;
                    ColorButton.Background = Color;
                   
                    ColorStatic.IsChecked = true;
                    EditColor.IsEnabled = false;
                }
                else
                {

                    ColorButton.IsEnabled = false;
                    ColorStatic.IsChecked = false;
                    EditColor.IsEnabled = true;
                }
                if (CurrentAnim.Alpha.isStatic)
                {
                    staticTextBox.Text = (CurrentAnim.Alpha.StaticValue[0] * 100).ToString();
                    staticTextBox.IsEnabled = true;
                    Check_AlphaStatic.IsChecked = true;
                    ButtonEditAlpha.IsEnabled = false;
                }
                else
                {
                    staticTextBox.IsEnabled = false;
                    Check_AlphaStatic.IsChecked = false;
                    ButtonEditAlpha.IsEnabled = true;
                }
            }
           
            


        }

         
         private float[] GetAsRGB(float[] nums)
        {
            float one = nums[2] * 255;
            float two = nums[1] * 255;
            float three = nums[0] * 255;
            return [one,two,three];
        }

        private void SetStaticAlpha(object sender, RoutedEventArgs e)
        {
            if (CurrentAnim == null) { return; }
            CurrentAnim.Alpha.isStatic = Check_AlphaStatic.IsChecked == true;
            ButtonEditAlpha.IsEnabled = Check_AlphaStatic.IsChecked == false;
        }

        private void SetStaticColor(object sender, RoutedEventArgs e)
        {
            if (CurrentAnim == null) { return; }
            CurrentAnim.Color.isStatic = ColorStatic.IsChecked == true;
            EditColor.IsEnabled = ColorStatic.IsChecked == false;
        }

        private void EditAlphaVisibility(object sender, RoutedEventArgs e)
        {
            if (CurrentAnim == null) { return; }
            Transformation_window tw = new Transformation_window(model, CurrentAnim.Alpha, TransformationType.Alpha);
            tw.ShowDialog();
        }

        private void EditDynamicColor(object sender, RoutedEventArgs e)
        {
            if (CurrentAnim == null) { return; }
            Transformation_window tw = new Transformation_window(model, CurrentAnim.Color, TransformationType.Color);
            tw.ShowDialog();
        }

        private void SetStaticColorClick(object sender, RoutedEventArgs e)
        {
           if (CurrentAnim == null) { return; }
            var initialColor = Converters.BrushToColor(ColorButton.Background);
            var dialog = new ColorPickerDialog(initialColor);
            var result = dialog.ShowDialog();
            if (result.HasValue && result.Value)
            {
                ColorButton.Background = Converters.ColorToBrush(dialog.Color);
                
                float[] rgb = Converters.ColorToFloatArray(dialog.Color);
                
                CurrentAnim.Color.StaticValue = [rgb[2] , rgb[1] , rgb[0] ];
                
            }
            

           
            
        }

        private void SetStaticAlpha(object sender, TextChangedEventArgs e)
        {
            string text = staticTextBox.Text;
            bool parsed = int.TryParse(text, out int value);
            if (parsed)
            {
                if (value >= 0 && value <= 100)
                {
                    float converted = value / 100;
                    CurrentAnim.Alpha.StaticValue = [converted];
                }
               

            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) { DialogResult = false; }
        }
    }
}
