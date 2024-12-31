using System.Drawing;
using System.Globalization;
using System.Windows.Interop;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Color = System.Windows.Media.Color;
using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;
using System.Windows.Controls;
using Image = System.Windows.Controls.Image;
using System.Collections.Generic;
using System;
using System.Linq;
namespace MDLLibs.Classes.Misc
{
    internal static class Converters
    {
        public static float[] SafeFloatArray(string data)
        {
            List<string> list = data.Split(',').ToList() ;
            List<float> floatArray = new List<float>();
            foreach (string dat in list)
            {
                floatArray.Add(SafeFloat(dat));
            }
                return floatArray.ToArray();

        }
        public static Brush BrushFromNormalizedRGB(float[] rgbValues)
        {
            // Ensure the input array has exactly 3 elements
            if (rgbValues == null || rgbValues.Length < 3)
            {
                throw new ArgumentException("Input array must have exactly three elements.");
            }

            // Clamp each value to the range [0, 1] and convert to byte (0-255)
            byte r = (byte)(Math.Clamp(rgbValues[0], 0f, 1f) * 255);
            byte g = (byte)(Math.Clamp(rgbValues[1], 0f, 1f) * 255);
            byte b = (byte)(Math.Clamp(rgbValues[2], 0f, 1f) * 255);

            // Create and return a SolidColorBrush
            return new SolidColorBrush(Color.FromRgb(r, g, b));
        }
        public static Brush War3ColorToBrush(float[] values)
        {
            int R = (int) values[2];
            int G = (int) values[1];
            int B = (int) values[0];
             
            return IntArrayToBrush([R,G,B]);
        }
        public static Brush IntArrayToBrush(float[] rgb)
        {
            // Ensure the input array has three elements
            if (rgb.Length != 3)
            {
                throw new ArgumentException("The RGB array must have exactly three elements.");
            }

            // Clamp values between 0 and 1 and convert to byte (0-255)
            byte r = (byte)(Math.Clamp(rgb[0], 0f, 1f) * 255);
            byte g = (byte)(Math.Clamp(rgb[1], 0f, 1f) * 255);
            byte b = (byte)(Math.Clamp(rgb[2], 0f, 1f) * 255);

            // Create a Color object
            Color color = Color.FromRgb(r, g, b);

            // Return a SolidColorBrush with the specified color
            return new SolidColorBrush(color);
        }
        public static Bitmap CopyBitmap(Bitmap original)
        {
            // Use the Clone method to create a copy of the original bitmap
            return original.Clone(new Rectangle(0, 0, original.Width, original.Height), original.PixelFormat);
        }
        public static string MultiplyBy100(List<double> vals)
        {
           List<int> result = new List<int>();
            foreach (double d in vals)
            {
                result.Add((int)(d * 100));
            }
            return string.Join(", ", result);
        }
        public static float[]  MultiplyBy100(float[] data, int m)
        {
             for (int i = 0; i<data.Length; i++)
            {
                data[i] *= m;
                data[i] = (float)Math.Truncate(data[i]);
            }
            return data;
        }
       
        public static Image ConvertBitmapToImage(Bitmap bitmap)
        {
            Image image = new Image();
            using (System.IO.MemoryStream memory = new System.IO.MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                memory.Position = 0;
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                image.Source = bitmapImage;
            }
            return image;
        }
        internal static int[] SplitStringToIntArray(string input)
        {
            input = input.Replace("}", "").Replace("{", "");
            string[] parts = input.Split(',');
            int[] result = new int[parts.Length];
            for (int i = 0; i < parts.Length; i++)
            {
                if (!int.TryParse(parts[i], out result[i]))
                {
                    // Handle invalid input
                    throw new ArgumentException("Input string contains non-integer values.");
                }
            }
            return result;
        }
        internal static string AppendDateTimeToFileName(string filePath)
        {
            // Get the directory path and file name from the full file path
            string directoryPath = System.IO.Path.GetDirectoryName(filePath);
            string fileName = System.IO.Path.GetFileNameWithoutExtension(filePath);
            string extension = System.IO.Path.GetExtension(filePath);
            // Append current datetime to the file name
            string newFileName = $"{fileName}_{DateTime.Now.ToString("yyyyMMddHHmmss")}{extension}";
            // Combine the directory path and the new file name to get the full path
            string newFilePath = System.IO.Path.Combine(directoryPath, newFileName);
            return newFilePath;
        }
        internal static List<int> GetSelectedLsitBoxITemsAsInts(ListBox lb)
        {
            List<int> result = new List<int>();
            foreach (ListBoxItem lb2 in lb.SelectedItems)
            {
                result.Add(int.Parse(lb2.Content.ToString()));
            }
            return result;
        }
        internal static void RemoveDuplicates(List<string> list)
        {
            HashSet<string> seen = new HashSet<string>();
            int i = 0;
            while (i < list.Count)
            {
                if (!seen.Add(list[i]))
                {
                    list.RemoveAt(i);
                }
                else
                {
                    i++;
                }
            }
        }
        internal static bool SafeBool(string s)
        {
            if (s.Length == 0) { return false; }
            if (s == "1") { return true; }
            if (s == "0") { return false; }
            if (s.ToLower() == "true") { return true; }
            if (s.ToLower() == "false") { return false; }
            return false;
        }
        internal static string SafeBoolAsString(string s)
        {
            if (s == "1") { return true.ToString(); }
            if (s == "0") { return false.ToString(); }
            if (s.ToLower() == "true") { return true.ToString(); }
            if (s.ToLower() == "false") { return false.ToString(); }
            return false.ToString();
        }
        internal static Bitmap ReplaceTransparentPixelsWithBlack(Bitmap bitmap)
        {
            // Create a new bitmap with the same size as the input bitmap
            Bitmap resultBitmap = new Bitmap(bitmap.Width, bitmap.Height);
            // Get the color of the transparent pixel
            // Loop through each pixel of the input bitmap
            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    // Get the color of the current pixel
                    System.Drawing.Color pixelColor = bitmap.GetPixel(x, y);
                    // Check if the pixel is transparent
                    if (pixelColor == System.Drawing.Color.Transparent)
                    {
                        // Replace transparent pixels with black
                        resultBitmap.SetPixel(x, y, System.Drawing.Color.Black);
                    }
                    else
                    {
                        // Copy non-transparent pixels as is
                        resultBitmap.SetPixel(x, y, pixelColor);
                    }
                }
            }
            return resultBitmap;
        }
        internal static ImageSource ConvertBitmapToImageSource(Bitmap bitmap)
        {
            if (bitmap == null) { MessageBox.Show("null image"); return null; }
            // Convert Bitmap to BitmapSource
            BitmapSource bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(
                bitmap.GetHbitmap(),
                nint.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
            // Release the Bitmap handle to prevent memory leaks
            bitmap.Dispose();
            return bitmapSource;
        }
        internal static Color BrushToColor(Brush brush)
        {
            SolidColorBrush solidColorBrush = brush as SolidColorBrush;
            if (solidColorBrush != null)
            {
                return solidColorBrush.Color;
            }
            // Handle other brush types if needed
            return Colors.Transparent; // Default color if brush is not SolidColorBrush
        }
        // Function to convert Color to Brush
        internal static Brush ColorToBrush(Color color)
        {
            return new SolidColorBrush(color);
        }
       
        
        internal static string ConvertAppAlphaORScalingToMDLAlpha(string input)
        {
            if (!int.TryParse(input, out int number))
            {
                // Input string is not a valid integer
                return "Invalid input";
            }
            // Divide the integer by 100
            float result = (float)number / 100;
            // Return the result as a string with two digits precision
            return result.ToString("F2");
        }
        internal static string ConvertAppVisibilityToMDL(string input)
        {
            return input.ToLower() == "true" ? "1" : input.ToLower() == "false" ? "0" : "0";
        }
        internal static Brush RGStringToBrush(string RGBCode)
        {
            if (RGBCode == null || RGBCode.Length == 0) { return Brushes.White; }
            if (string.IsNullOrEmpty(RGBCode))
                return Brushes.White;
            string[] parts = RGBCode.Split(',');
            if (parts.Length != 3)
                return Brushes.White;
            byte r, g, b;
            if (!byte.TryParse(parts[0], out r) || !byte.TryParse(parts[1], out g) || !byte.TryParse(parts[2], out b))
                return Brushes.White;
            SolidColorBrush brush = new SolidColorBrush(Color.FromRgb(r, g, b));
            return brush;
        }
        internal static bool StringToBool(string s)
        {
            if (s.ToLower() == "true") { return true; }
            if (s.ToLower() == "false") { return true; }
            if (s.ToLower() == "1") { return true; }
            if (s.ToLower() == "0") { return true; }
            return false;
        }
        public static float[] BrushToRGBArray(Brush brush)
        {
            if (brush is SolidColorBrush solidColorBrush)
            {
                Color color = solidColorBrush.Color;
                return [color.R, color.G, color.B];
            }
            else
            {
                throw new ArgumentException("The brush is not a SolidColorBrush.");
            }
        }
        public static int[] BrushToRGBArrayInt(Brush brush)
        {
            if (brush is SolidColorBrush solidColorBrush)
            {
                Color color = solidColorBrush.Color;
                return [color.R, color.G, color.B];
            }
            else
            {
                throw new ArgumentException("The brush is not a SolidColorBrush.");
            }
        }
        public static Brush RGBArrayToBrush(float[] rgb)
        {
            if (rgb.Length != 3)
            {
                throw new ArgumentException("The RGB array must contain exactly 3 elements.");
            }
            byte r = (byte)rgb[0];
            byte g = (byte)rgb[1];
            byte b = (byte)rgb[2];
            Color color = Color.FromRgb(r, g, b);
            return new SolidColorBrush(color);
        }
        public static Brush RGBArrayToBrush(int[] rgb)
        {
            if (rgb.Length != 3)
            {
                throw new ArgumentException("The RGB array must contain exactly 3 elements.");
            }
            byte r = (byte)rgb[0];
            byte g = (byte)rgb[1];
            byte b = (byte)rgb[2];
            Color color = Color.FromRgb(r, g, b);
            return new SolidColorBrush(color);
        }
        internal static string ConvertBrushToString(Brush brush)
        {
            if (brush is SolidColorBrush solidColorBrush)
            {
                var color = solidColorBrush.Color;
                return $"{color.R},{color.G},{color.B}";
            }
            else
            {
                // Handle other types of brushes if necessary
                return "Unknown";
            }
        }
        internal static int SafeInt(string input, int defa_ault)
        {
            // Trim the input string
            input = input.Trim();
            // Attempt to parse the trimmed string to an integer
            if (int.TryParse(input, out int result))
            {
                return result; // Successfully parsed, return the result
            }
            else
            {
                return defa_ault;  
            }
        }
        internal static int SafeFrame(string input)
        {
            // Trim the input string
            string formatted = input.Trim();
            if (formatted.Length == 0) { return -1; }
            // Attempt to parse the trimmed string to an integer
            if (int.TryParse(formatted, out int result))
            {
                return result; // Successfully parsed, return the result
            }
            else
            {
                return -1; // Parsing failed, return -1
            }
        }
        internal static float SafeFloat(string s, float def_ault = 0)
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            // Try to parse the string as a double
            if (float.TryParse(s,   out float result))
            {
                return result;
            }
            else
            {
                return def_ault;
            }
        }
        internal static string BoolToBinary(bool v)
        {
            return v == true ? "1" : "0";
        }
        internal static int[] GetIntsFromString(string value)
        {
            string[] prs = value.Split(',');
            List<int> ints = new List<int>();
            foreach (string s in prs)
            {
                ints.Add(Converters.SafeInt(s.Trim(), 0));
            }
            return ints.ToArray();  
        }
        
        internal static int StringToAlpha(string staticValue)
        {
            int val = SafeInt(staticValue, 0);
            int multiplied = val * 100;
            if (multiplied > 100) { return 100; }
            return multiplied;
        }
        
        internal static float SafeRotation(string text)
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            string formatted = text.Trim();
            float ff = 0;
            if (formatted.Length == 0) { return 0; }
            bool canParse = float.TryParse(formatted, NumberStyles.Float, CultureInfo.InvariantCulture, out ff);
            if (canParse) {
                if (ff<0) { return 0; }
                if (ff > 360) { return 360; }
                return ff;
            }
            return 0;
        }

        internal static float[] RGBToWarcraft3Color(float[] data)
        {
            return [data[2] / 255, data[1]/255, data[0]/255 ];
        }

        internal static float[] Warcraft3ColorToRGB(float[] data)
        {
            return [data[2] * 255, data[1] * 255, data[0] * 255];
        }

        
public static float[] ColorToFloatArray(Color color)
    {
        // Create a float array to hold the RGB values
        float[] rgb = new float[3];

        // Assign the RGB values from the Color object (normalized to [0, 1] range)
        rgb[0] = color.R / 255f; // Red
        rgb[1] = color.G / 255f; // Green
        rgb[2] = color.B / 255f; // Blue

        return rgb;
    }

        internal static float[] BrushToNormalizedRGB(Brush background)
        {
           float[] rgb = BrushToRGBArray(background);
            return [rgb[0]/255, rgb[1]/255, rgb[2]/255];
            
        }

        internal static int SafeInt(string v)
        {
            throw new NotImplementedException();
        }

        internal static System.Windows.Media.Brush BrushFromRGB(float[] staticValue)
        {
            if (staticValue == null || staticValue.Length < 3)  return Brushes.White;

            byte r = (byte)(staticValue[0] * 255);
            byte g = (byte)(staticValue[1] * 255);
            byte b = (byte)(staticValue[2] * 255);

            return new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(r, g, b));
        }

        internal static string ScalingPercentageToNormalizedString(float[] data)
        {
            float[] copy = new float[data.Length];
            Array.Copy(data, copy, data.Length);
            for (int i = 0; i < copy.Length; i++) { copy[i] /= 100; }
            return string.Join(", ", copy);
        }
    }
}
