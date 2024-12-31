using MDLLib;
using MDLLibs.Classes.Misc;
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
using Whim_GEometry_Editor.Misc;
using Whim_Model_Editor;

namespace Whim_GEometry_Editor
{
    /// <summary>
    /// Interaction logic for Transformation.xaml
    /// </summary>
    public partial class Transformation_window : Window
    {
        w3Model Model;
        w3Transformation transformation;
        TransformationType Type;

        int ExpectedValues = 0;
        bool allowfloat = false;
        int ValuewLowerLimit = 0;
        int ValuewUpperLimit = 0;

        public Transformation_window(
            w3Model model,
            w3Transformation tr,

            TransformationType type)
        {
            InitializeComponent();
            transformation = tr;


            Model = model;

            FillGlobalSequences();
            FillSequences();
            List_Interp.SelectedIndex = (int)transformation.Interpolation_Type;
            if (type == TransformationType.Visibility)
            {
                List_Interp.SelectedIndex = 0;
                List_Interp.IsEnabled = false;
                Combo_GS.IsEnabled = false;
            }
            Type = type;

            FillKeyframes();
            Title = "Transformation: " + type.ToString();

        }

        private void FillSequences()
        {
            foreach (w3Sequence s in Model.Sequences)
            {
                List_Sequences.Items.Add(new ListBoxItem() { Content = $"{s.Name} [{s.From} - {s.To}]" });
            }
        }
        private void FillGlobalSequences()
        {
            Combo_GS.Items.Add(new ComboBoxItem() { Content = "None" });
            int id = transformation.Global_Sequence_ID;
            int index = 0;
            foreach (w3Global_Sequence sequence in Model.Global_Sequences)
            {
                Combo_GS.Items.Add(new ComboBoxItem() { Content = sequence.ID.ToString() });
                if (sequence.ID == id) { index = Model.Global_Sequences.IndexOf(sequence); }
            }
            if (transformation.Global_Sequence_ID >= 0)
            {
                Combo_GS.SelectedIndex = index + 1;
            }
            else
            {
                Combo_GS.SelectedIndex = 0;
            }
        }
        private void SetInterpolation()
        {
            List_Interp.SelectedIndex = (int)transformation.Interpolation_Type;

        }
        public string GetRichTextBoxContent(RichTextBox richTextBox)
        {
            // Get the content from the RichTextBox's document
            TextRange textRange = new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd);

            // Return the text content as a string
            return textRange.Text;
        }

        private void ClickedOK(object sender, RoutedEventArgs e)
        {
            //unfinished

        }
        private void FillKeyframes()
        {
            transformation.Keyframes = transformation.Keyframes.OrderBy(x => x).ToList();
            List<string> values = new List<string>();


            foreach (w3Keyframe k in transformation.Keyframes)
            {
                ListBoxItem listBoxItem = new ListBoxItem();
                listBoxItem.Width = 500;
                TextBlock TrackHeader = new TextBlock();
                TrackHeader.FontWeight = FontWeights.Bold;
                TextBlock DataHeader = new TextBlock();
                TextBlock IntanHeader = new TextBlock();
                TextBlock OuttanHeader = new TextBlock();

                TrackHeader.Text = k.Track.ToString();
                TrackHeader.Margin = new Thickness(5, 0, 5, 0);
                DataHeader.Margin = new Thickness(5, 0, 5, 0);
                IntanHeader.Margin = new Thickness(5, 0, 5, 0);
                OuttanHeader.Margin = new Thickness(5, 0, 5, 0);

                DataHeader.Text = string.Join(", ", k.Data); ;
                IntanHeader.Text = string.Join(", ", k.InTan); ;
                OuttanHeader.Text = string.Join(", ", k.OutTan); ;
             


                StackPanel contentHolder = new StackPanel();
                contentHolder.Orientation = Orientation.Horizontal;
                contentHolder.Children.Add(TrackHeader);
                contentHolder.Children.Add(DataHeader);
                contentHolder.Children.Add(IntanHeader);
                contentHolder.Children.Add(OuttanHeader);
                listBoxItem.Content = contentHolder;
                Instructions.Items.Add(listBoxItem);
            }
            

        }
       
        private bool KeyframesHaveDuplicatingTracks(List<w3Keyframe> keyframes)
        {
            // Create a HashSet to store unique track values
            HashSet<int> uniqueTracks = new HashSet<int>();

            // Iterate through the list of keyframes
            foreach (var keyframe in keyframes)
            {
                // If the track value is already in the set, it means there's a duplicate
                if (!uniqueTracks.Add(keyframe.Track))
                {
                    // Return true as soon as a duplicate is found
                    return true;
                }
            }

            // If no duplicates were found, return false
            return false;
        }


        public static bool AreKeyframesInAscendingOrder(List<w3Keyframe> keyframes)
        {
            if (keyframes == null || keyframes.Count < 2)
                return true; // Empty or single keyframe list is considered ascending

            for (int i = 0; i < keyframes.Count - 1; i++)
            {
                if (keyframes[i].Track >= keyframes[i + 1].Track)
                    return false; // Found a non-ascending order
            }

            return true; // All keyframes are in ascending order
        }

        static float[] RemoveFirstElement(float[] array)
        {
            if (array.Length <= 1)
            {
                // If the array has 0 or 1 element, return an empty array
                return new float[0];
            }

            // Create a new array with length one less than the original
            float[] newArray = new float[array.Length - 1];

            // Copy the elements starting from index 1
            Array.Copy(array, 1, newArray, 0, newArray.Length);

            return newArray;
        }



        public static bool StartsWithIntegerFollowedByColon(string input)
        {
            bool metInteger = false;
            foreach (char c in input)
            {

                if (c == ':')
                {
                    if (!metInteger) { return false; } else { return true; }

                }
                if (char.IsDigit(c)) { metInteger = true; continue; }
                else { return false; }

            }
            return true;
        }
        public static float[] ExtractNumbers(string input)
        {
            // Regular expression pattern to match integers, floats, and floats with exponent
            string pattern = @"-?\d+(\.\d+)?([eE][+-]?\d+)?";

            // Find matches in the input string
            MatchCollection matches = Regex.Matches(input, pattern);

            // List to store the extracted numbers
            List<float> numbers = new List<float>();

            foreach (Match match in matches)
            {
                if (float.TryParse(match.Value, out float number))
                {
                    numbers.Add(number);
                }
            }

            // Convert the list to an array and return it
            return numbers.ToArray();
        }
        private bool ValidateInstructions(string Text, TransformationType Type, bool hasTangents)
        {
            // Split the text by lines
            string[] lines = Text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);

            int expectedValuesPerLine = (Type == TransformationType.Visibility || Type == TransformationType.Alpha ||
                                         Type == TransformationType.Int || Type == TransformationType.Float) ? 1 : 3;

            bool ExpectIntan = false;
            bool ExpectOuttan = false;
            // MessageBox.Show("lines: "+lines.Count().ToString());
            foreach (string line in lines)
            {
                if (!hasTangents)
                {
                    if (StartsWithIntegerFollowedByColon(line) == false) { MessageBox.Show("Each line must start with track number, followed by a colon", "Precaution"); return false; }

                    float[] values = ExtractNumbers(line);
                    // MessageBox.Show(string.Join(", ", values));
                    if (values.Length <= 1) { MessageBox.Show("Expected values after a track"); return false; } // no values or only track number  

                    if (values.Length - 1 != expectedValuesPerLine) { MessageBox.Show("Unexpected number of values", "Precaution"); return false; }

                    if (values[0] < 0) { MessageBox.Show("Cannot have a negative track value", "Precaution"); return false; }
                    if (Track_is_In_Sequences(values[0]) == false) { MessageBox.Show($"Track {(int)values[0]} is not part of any sequence", "Precaution"); return false; }

                }
                if (hasTangents)
                {
                    if (ExpectIntan == false && ExpectOuttan == false)
                    {
                        if (StartsWithIntegerFollowedByColon(line) == false) { MessageBox.Show("Each line must start with track number, followed by a colon", "Precaution"); return false; }

                        float[] values = ExtractNumbers(line);

                        if (values.Length <= 1) { MessageBox.Show("Expected values after a track"); return false; } // no values or only track number  

                        if (values.Length - 1 != expectedValuesPerLine) { MessageBox.Show("Unexpected number of values", "Precaution"); return false; }
                        if (values[0] < 0) { MessageBox.Show("Cannot have a negative track value", "Precaution"); return false; }
                        if (Track_is_In_Sequences(values[0]) == false) { MessageBox.Show($"Track {(int)values[0]} is not part of any sequence", "Precaution"); return false; }

                        for (int i = 1; i < values.Length; i++)
                        {
                            {
                                if (ValidateValue(values[i]) == false) { return false; }
                            }
                        }
                        ExpectIntan = true;

                    }
                    if (ExpectIntan)
                    {
                        if (line.StartsWith("InTan") == false) { MessageBox.Show($"Expected {line} to start with 'InTan'"); return false; }


                        float[] values = ExtractNumbers(line);
                        if (values.Length <= 1) { MessageBox.Show("Expected values after a track"); return false; } // no values or only track number  

                        if (values.Length - 1 != expectedValuesPerLine) { MessageBox.Show("Unexpected number of values", "Precaution"); return false; }
                        if (values[0] < 0) { MessageBox.Show("Cannot have a negative track value", "Precaution"); return false; }
                        if (Track_is_In_Sequences(values[0]) == false) { MessageBox.Show($"Track {(int)values[0]} is not part of any sequence", "Precaution"); return false; }
                        for (int i = 1; i < values.Length; i++)
                        {
                            {
                                if (ValidateValue(values[i]) == false) { return false; }
                            }
                        }
                        ExpectIntan = false;
                        ExpectOuttan = true;
                    }
                    if (ExpectOuttan)
                    {
                        if (line.StartsWith("OutTan") == false) { MessageBox.Show($"Expected {line} to start with 'OutTan'"); return false; }

                        float[] values = ExtractNumbers(line);
                        if (values.Length - 1 != expectedValuesPerLine) { return false; }
                        if (values.Length <= 1) { return false; } // no values or only track number  
                        for (int i = 1; i < values.Length; i++)
                        {
                            {
                                if (ValidateValue(values[i]) == false) { return false; }
                            }
                        }
                        ExpectIntan = false;
                        ExpectOuttan = false;
                    }
                }



            }
            return true; // All lines are valid
        }

        private bool ValidateValue(float value)
        {
            switch (Type)
            {
                case TransformationType.Float: return true;
                case TransformationType.Int: return value == Math.Floor(value);
                case TransformationType.Translation: return value >= 0;
                case TransformationType.Rotation: return value >= -360 && value <= 360;
                case TransformationType.Scaling: return value > 0;
                case TransformationType.Color: return value >= 0 && value <= 255;
                case TransformationType.Alpha: return value >= 0 && value <= 100;
                case TransformationType.Visibility: return value >= 0 && value <= 1;



            }

            return true;
        }

        private void TellHowToWriteKeyframes(object sender, RoutedEventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Each line starts with a track number, followed by a colon.");
            sb.AppendLine("then it must be followed by 1 or 3 values depending on the transformation type.");
            sb.AppendLine("Translation: 3 values, any positive integer or float");
            sb.AppendLine("Scaling: 3 values, any positive integer or float (It's a percentage)");
            sb.AppendLine("Rotation: 3 values, any integer or float between -360 and 360");
            sb.AppendLine("Integer: 1 value, must be positive integer");
            sb.AppendLine("Float: 1 value, must be positive float");
            sb.AppendLine("Color: 3 values, must be integer between 0 and 255");
            sb.AppendLine("Visibility: 1 value, must be 0 or 1");
            sb.AppendLine("Alpha: 1 value, must be between 0 and 100 (It's a percentage)");
            MessageBox.Show(sb.ToString());
        }

        private void ChangedInterpolation(object sender, SelectionChangedEventArgs e)
        {
            InputIntan.IsEnabled = List_Interp.SelectedIndex > 1;
            InputOuttan.IsEnabled = List_Interp.SelectedIndex > 1;
            transformation.Interpolation_Type = (InterpolationType)List_Interp.SelectedIndex;
            TextIntan.TextDecorations = List_Interp.SelectedIndex < 2 ? TextDecorations.Strikethrough : TextDecorations.Underline;
            TextOuttan.TextDecorations = List_Interp.SelectedIndex < 2 ? TextDecorations.Strikethrough : TextDecorations.Underline;
        }
        private bool Track_is_In_Sequences(float track)
        {
            int t = (int)track;
            return Model.Sequences.Any(x => t >= x.From && t <= x.To);
        }

        private void DelKeyframe(object sender, RoutedEventArgs e)
        {
            if (Instructions.SelectedItem != null)
            {

                int track = GetSelectedTrack();
                transformation.Keyframes.RemoveAll(x => x.Track == track);
                Instructions.Items.Remove(Instructions.SelectedItem);

            }
        }

        private int GetSelectedTrack()
        {
            ListBoxItem item = Instructions.SelectedItem as ListBoxItem;
            StackPanel s = item.Content as StackPanel;
            TextBlock b = s.Children[0] as TextBlock;
            return int.Parse(b.Text);
        }
        private w3Keyframe GetPendingInputKeyframe()
        {
            w3Keyframe k = new w3Keyframe();
            bool parsed1 = int.TryParse(InputTrack.Text, out int track);
            string[] data = InputData.Text.Trim().Split(' ');
            string[] intan = InputIntan.Text.Trim().Split(' ');
            string[] outtan = InputOuttan.Text.Trim().Split(' ');
            return k;
        }
        private void EditKeyframe_(object sender, RoutedEventArgs e)
        {
            if (Instructions.SelectedItem != null)
            {
                
                    int CurrentTrack = int.Parse(InputTrack.Text);
                    if (List_Interp.SelectedIndex < 2)
                    {
                        if (FormatCorrect(InputData.Text))
                        {

                            w3Keyframe k = new w3Keyframe();
                            float[] data = InputData.Text.Split(", ").Select(x => float.Parse(x)).ToArray();
                             w3Keyframe target = transformation.Keyframes.First(x => x.Track == CurrentTrack);
                            target.Data = data;
                          
                            RefreshKeyframesList();
                        }
                        else
                        {
                            ErrorBasedOnType(List_Interp.SelectedIndex > 1);

                        }
                    }
                    else
                    {
                        if (FormatCorrect(InputData.Text) && FormatCorrect(InputIntan.Text) && FormatCorrect(InputOuttan.Text))
                        {
                            w3Keyframe k = new w3Keyframe();
                            float[] data = InputData.Text.Split(", ").Select(x => float.Parse(x)).ToArray();
                            float[] data2 = InputIntan.Text.Split(", ").Select(x => float.Parse(x)).ToArray();
                            float[] data3 = InputOuttan.Text.Split(", ").Select(x => float.Parse(x)).ToArray();
                            k.Data = data;
                            k.InTan = data2;
                            k.OutTan = data3;
                            w3Keyframe target =
                            transformation.Keyframes.First(x => x.Track == CurrentTrack);
                            target = k;
                            transformation.Keyframes = transformation.Keyframes.OrderBy(x => x).ToList();
                            RefreshKeyframesList();
                        }
                        else
                        {
                            ErrorBasedOnType(List_Interp.SelectedIndex > 1);

                        }
                    }
                }
               
            }
       
        private bool FormatCorrect(string data)
        {
            // Trim the input data to remove leading and trailing whitespace
            string values = data.Trim();


            // Validate the values based on the TransformationType
            return ValidateValues(values);
        }
        /*
         int, id - integer
        float
        alpha integer 1 - 100
        color - from 0 to 255 3 values
        scaling from 0 to 100 3 values


         
         */
        private bool ValidateValues(string values)
        {
            string[] vals = values.Split(", ").ToArray();
            List<float> floats = new List<float>();
            bool failed = false;
            foreach (string val in vals)
            {
                bool parsed = float.TryParse(val, out float value);
                if (!parsed) { return false; }
                floats.Add(value);
            }
            if (failed) { return false; }
            switch (Type)
            {
                case TransformationType.Int:
                case TransformationType.ID:
                    if (floats.Count == 1)
                    {
                        return floats[0] == Math.Floor(floats[0]);
                    }
                    else { return false; }
                case TransformationType.Float:

                    return floats.Count == 1;


                case TransformationType.Alpha:
                    if (floats.Count == 1)
                    {
                        return floats[0] >= 0 && floats[0] <= 100;
                    }
                    else { return false; }

                case TransformationType.Color:
                    if (floats.Count == 3)
                    {
                        return
                            floats[0] >= 0 && floats[0] <= 255 &&
                            floats[1] >= 0 && floats[1] <= 255 &&
                            floats[2] >= 0 && floats[2] <= 255

                            ;
                    }
                    else { return false; }
                case TransformationType.Rotation:
                    if (floats.Count == 3)
                    {
                        return
                            floats[0] >= -360 && floats[0] <= 360 &&
                            floats[1] >= -360 && floats[1] <= 360 &&
                            floats[2] >= -360 && floats[2] <= 360

                            ;
                    }
                    else { return false; }

                case TransformationType.Translation:
                case TransformationType.Scaling:
                    return floats.Count == 3;

                case TransformationType.Visibility:
                    if (floats.Count == 1)
                    { return floats[0] == 0 || floats[0] == 1; }
                    else { return false; }

                default:
                    return false;
            }
        }

        private bool TrackCorrect()
        {
            bool parsed = int.TryParse(InputTrack.Text, out int track);
            if (parsed)
            {
                if (Model.Sequences.Any(x => x.From <= track && x.To >= track) == false)
                {
                    MessageBox.Show("Input track is not in any sequence", "Invalid input"); return false;
                }
                return !transformation.Keyframes.Any(x => x.Track == track);
            }
            else
            {


                MessageBox.Show("Input for track not valid", "Invalid input"); return false;
            }


        }
        private void AddKeyframe_(object sender, RoutedEventArgs e)
        {
            if (TrackCorrect())
            {
                int track = int.Parse(InputTrack.Text);
                if (List_Interp.SelectedIndex < 2)
                {
                    if (FormatCorrect(InputData.Text))
                    {

                        w3Keyframe k = new w3Keyframe();
                        k.Track = track;
                        k.Data = InputData.Text.Split(", ").Select(x => float.Parse(x)).ToArray();

                        transformation.Keyframes.Add(k);
                        transformation.Keyframes = transformation.Keyframes.OrderBy(x => x.Track).ToList();
                        RefreshKeyframesList();
                    }
                    else
                    {
                        ErrorBasedOnType(List_Interp.SelectedIndex > 1);

                    }
                }
                else
                {
                    if (FormatCorrect(InputData.Text) && FormatCorrect(InputIntan.Text) && FormatCorrect(InputOuttan.Text))
                    {
                        w3Keyframe k = new w3Keyframe();
                        k.Track = track;
                        float[] data = InputData.Text.Split(", ").Select(x => float.Parse(x)).ToArray();
                        float[] data2 = InputIntan.Text.Split(", ").Select(x => float.Parse(x)).ToArray();
                        float[] data3 = InputOuttan.Text.Split(", ").Select(x => float.Parse(x)).ToArray();
                        k.Data = data;
                        k.InTan = data2;
                        k.OutTan = data3;

                        transformation.Keyframes.Add(k);
                        transformation.Keyframes = transformation.Keyframes.OrderBy(x => x).ToList();
                        RefreshKeyframesList();
                    }
                    else
                    {
                        ErrorBasedOnType(List_Interp.SelectedIndex > 1);

                    }
                }
            }

        }
        private void ErrorBasedOnType(bool tangents = false)
        {
            string data = "";
            switch (Type)
            {
                case TransformationType.Int:
                case TransformationType.ID:
                    data = "int"; break;
                case TransformationType.Float:
                    data = "int/float"; break;


                case TransformationType.Alpha:

                    data = "int (0-100)"; break;
                case TransformationType.Color:
                    data = "r, g, b"; break;

                case TransformationType.Rotation:
                    data = "roll, pitch, yaw"; break;
                case TransformationType.Translation:
                    data = "int, int, int"; break;


                case TransformationType.Scaling:
                    data = "int, int, int"; break;
                case TransformationType.Visibility:
                    data = "1/0"; break;


            }
            if (tangents)
            {
                MessageBox.Show($"Incorrect input format. Expected:\n{data}", "Precaution");
            }
            else
            {
                MessageBox.Show($"Incorrect input format. Expected:\n{data}", "Precaution");
            }

        }
        private void SelectedAKeyframe(object sender, SelectionChangedEventArgs e)
        {
            if (Instructions.SelectedItem == null) { return; }
            ListBoxItem i = Instructions.SelectedItem as ListBoxItem;
            StackPanel s = i.Content as StackPanel;



            string content = i.Content as string;
            InputTrack.Text = (s.Children[0] as TextBlock).Text;
            InputData.Text = (s.Children[1] as TextBlock).Text;
            InputIntan.Text = (s.Children[2] as TextBlock).Text;
            InputOuttan.Text = (s.Children[3] as TextBlock).Text;

        }

        private void AddSequence(object sender, MouseButtonEventArgs e)
        {
            if (List_Sequences.SelectedItem != null)
            {
                string rawName = (List_Sequences.SelectedItem as ListBoxItem).Content.ToString();
                string name = rawName.Split(" [")[0];
                int from = Model.Sequences.First(x => x.Name == name).From;
                if (transformation.Keyframes.Any(x => x.Track == from))
                {
                    MessageBox.Show("There is already a track with this value", "Invalid request"); return;
                }
                w3Keyframe k = new w3Keyframe();
                k.Track = from;
                switch (Type)
                {
                    case TransformationType.Int:
                    case TransformationType.ID:
                    case TransformationType.Float:

                        k.Data = [0]; k.InTan = [0]; k.OutTan = [0];
                        break;
                    case TransformationType.Alpha:

                        k.Data = [100]; k.InTan = [100]; k.OutTan = [100];
                        break;
                    case TransformationType.Color:
                        k.Data = [255, 255, 255]; k.InTan = [255, 255, 255]; k.OutTan = [255, 255, 255];
                        break;

                    case TransformationType.Rotation:
                        k.Data = [0, 0, 0, 0]; k.InTan = [0, 0, 0, 0]; k.OutTan = [0, 0, 0, 0];

                        break;
                    case TransformationType.Translation:
                        k.Data = [0, 0, 0]; k.InTan = [0, 0, 0]; k.OutTan = [0, 0, 0];

                        break;
                    case TransformationType.Scaling:
                        k.Data = [100]; k.InTan = [100]; k.OutTan = [100];
                        break;
                    case TransformationType.Visibility:
                        k.Data = [1]; k.InTan = [1]; k.OutTan = [1];
                        break;

                }
                transformation.Keyframes.Add(k);
                transformation.Keyframes = transformation.Keyframes.OrderBy(x => x.Track).ToList(); ;
                RefreshKeyframesList();
            }

        }

        private void RefreshKeyframesList()
        {
            Instructions.Items.Clear();
            FillKeyframes();
        }

        private void ChangedGlobalSeqId(object sender, SelectionChangedEventArgs e)
        {
            if (Combo_GS.SelectedIndex == 0) { transformation.Global_Sequence_ID = -1; return; }
            if (Combo_GS.SelectedIndex > 1)
            {
                transformation.Global_Sequence_ID = Model.Global_Sequences[Combo_GS.SelectedIndex - 1].ID;
            }
        }

        private void CopyKeyframeData(object sender, RoutedEventArgs e)
        {
            if (transformation.Keyframes.Count > 0)
            {

                StringBuilder sb = new StringBuilder();

                foreach (w3Keyframe k in transformation.Keyframes)
                {
                    sb.AppendLine(k.ToFormattedCopy());
                }
                Clipboard.SetText(sb.ToString());
            }
        }

        private void PasteKeyframeData(object sender, RoutedEventArgs e)
        {
            string data = Clipboard.GetText();
            List<string> split = data.Split('\n').ToList();
            List<w3Keyframe> keys = new List<w3Keyframe>();
            string FormatError = "The format of the pasted keyframe does not match the required format of the current transformation";
            foreach (string s in split)
            {
                string[] parts = s.Split(" ");
                switch (Type)
                {
                    case TransformationType.ID:
                    case TransformationType.Int:
                    case TransformationType.Float:
                    case TransformationType.Alpha:
                    case TransformationType.Visibility:
                        if (parts.Length != 4)
                        {
                            MessageBox.Show(FormatError, "Precaution"); return;
                        }
                        else
                        {
                            bool parsedTrack = int.TryParse(parts[0], out var track);
                            if (parsedTrack)
                            {
                                w3Keyframe k = new w3Keyframe();
                                k.Track = track;
                                k.Data = [float.Parse(parts[1])];
                                k.InTan = [float.Parse(parts[2])];
                                k.OutTan = [float.Parse(parts[3])];
                            }
                            else
                            {
                                MessageBox.Show(FormatError, "Precaution"); return;

                            }
                        }
                        break;
                    case TransformationType.Translation:
                    case TransformationType.Scaling:

                    case TransformationType.Color:

                        if (parts.Length != 10)
                        {
                            MessageBox.Show("The format of the pasted keyframe does nto match the required format of the current transformation", "Precaution"); return;
                        }
                        else
                        {
                            bool parsedTrack = int.TryParse(parts[0], out var track);
                            if (parsedTrack)
                            {
                                w3Keyframe k = new w3Keyframe();
                                k.Track = track;
                                k.Data = [float.Parse(parts[1]), float.Parse(parts[2]), float.Parse(parts[3])];
                                k.InTan = [float.Parse(parts[4]), float.Parse(parts[5]), float.Parse(parts[6])];
                                k.OutTan = [float.Parse(parts[7]), float.Parse(parts[8]), float.Parse(parts[9])];
                                keys.Add(k);
                            }
                            else
                            {
                                MessageBox.Show("The format of the pasted keyframe does nto match the required format of the current transformation", "Precaution"); return;

                            }
                        }
                        break;
                    case TransformationType.Rotation:
                        if (parts.Length != 13)
                        {
                            MessageBox.Show(FormatError, "Precaution"); return;
                        }
                        else
                        {
                            bool parsedTrack = int.TryParse(parts[0], out var track);
                            if (parsedTrack)
                            {
                                w3Keyframe k = new w3Keyframe();
                                k.Track = track;
                                k.Data = [float.Parse(parts[1]), float.Parse(parts[2]), float.Parse(parts[3]), float.Parse(parts[4])];
                                k.InTan = [float.Parse(parts[5]), float.Parse(parts[6]), float.Parse(parts[7]), float.Parse(parts[8])];
                                k.OutTan = [float.Parse(parts[9]), float.Parse(parts[10]), float.Parse(parts[11]), float.Parse(parts[12])];
                                keys.Add(k);
                            }
                            else
                            {
                                MessageBox.Show(FormatError, "Precaution"); return;

                            }
                        }
                        break;
                }
            }
            transformation.Keyframes = keys;
            RefreshKeyframesList();
        }

        private void ShowModifiers(object sender, RoutedEventArgs e)
        {
            ButtonModifiers.ContextMenu.IsOpen = true;
        }

        private void reverse(object sender, RoutedEventArgs e)
        {
            transformation.Keyframes.Reverse();
            RefreshKeyframesList();
        }

        private void fillgaps(object sender, RoutedEventArgs e)
        {

        }

        private void quantize(object sender, RoutedEventArgs e)
        {
            KeyframesModifier.Quantize(transformation.Keyframes);
        }

        private void tileloop(object sender, RoutedEventArgs e)
        {

            KeyframesModifier.TileLoop(transformation.Keyframes);


        }


        private void Negatexs(object sender, RoutedEventArgs e)
        {
            foreach (var item in transformation.Keyframes)
            {
                item.Data[0] = -item.Data[0];
                item.InTan[0] = -item.InTan[0];
                item.OutTan[0] = -item.OutTan[0];
            }
        }

        private void Negateys(object sender, RoutedEventArgs e)
        {
            foreach (var item in transformation.Keyframes)
            {
                if (item.Data.Length >= 2)
                {
                    item.Data[1] = -item.Data[1];
                    item.InTan[1] = -item.InTan[1];
                    item.OutTan[1] = -item.OutTan[1];
                }

            }
        }

        private void Negatezs(object sender, RoutedEventArgs e)
        {
            foreach (var item in transformation.Keyframes)
            {
                if (item.Data.Length >= 3)
                {
                    item.Data[2] = -item.Data[2];
                    item.InTan[2] = -item.InTan[2];
                    item.OutTan[2] = -item.OutTan[2];
                }

            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) { DialogResult = false; }
        }
    }
}
 