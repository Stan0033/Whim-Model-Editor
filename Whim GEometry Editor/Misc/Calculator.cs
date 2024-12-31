using MdxLib.Primitives;
using System;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
namespace MDLLibs.Classes.Misc
{
    public static class UVCalculator
    {
        public static float GetNormalized(int currentPosition, int Total)
        {
             return (float)currentPosition / Total;
           
            
        }

        public static int GetActual(float value, int Total)
        {
            return (int)(value * Total);
        }
    }

    internal static class Clamper
    {
       

        public static float Degree(float value)
        {
            if (value < -360) return -360;
            if (value > 360) return 360;
            return value;
        }
        public static float Radian(float value)
        {
            if (value < -6.28318530718f) return  -6.28318530718f;
            if (value > 6.28318530718f) return 6.28318530718f;
            return value;
        }
        public static float Scaling(float value) { return value < 0 ? 0 : value; }
        public static float Attentuation(float value)
        {
            if (value < 80) return 80;
            if (value > 200) return 200;
            return value;
        }

        public static float Visibility(float value) { return value < 1 ? 0 : 1; }
        public static float Normalized (float value)
        {
            if (value < 0) return 0;
            if (value > 1) return 1;
            return value;
        }
        public static float Time(float value)
        {
            if (value < 0) return 0;
            if (value > 59) return 59;
            return value;
        }
        public static float RGB(float value)
        {
            if (value < 0) return 0;
            if (value > 255) return 255;
            return value;
        }
        public static float Percentage(float value)
        {
            if (value < 0) return 0;
            if (value > 100) return 100;
            return value;
        }
        public static float Clamp(float value, float min, float max)
        {
          
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        internal static float Positive(float v)
        {
            return (v < 0) ? 0 : v;
        }
    }
    internal static  class Calculator
    {
        public static float RadiansToDegrees(float radians)
        {
            return radians * (180f / (float)Math.PI);
        }
        public static float[] Rotation_Euler_To_Quaternion(float[] euler)
        {
            // Extract the Euler angles
            float roll = euler[0];
            float pitch = euler[1];
            float yaw = euler[2];
            // Convert degrees to radians
            roll = roll * (float)Math.PI / 180f;
            pitch = pitch * (float)Math.PI / 180f;
            yaw = yaw * (float)Math.PI / 180f;
            // Calculate trigonometric values
            float cy = (float)Math.Cos(yaw * 0.5f);
            float sy = (float)Math.Sin(yaw * 0.5f);
            float cp = (float)Math.Cos(pitch * 0.5f);
            float sp = (float)Math.Sin(pitch * 0.5f);
            float cr = (float)Math.Cos(roll * 0.5f);
            float sr = (float)Math.Sin(roll * 0.5f);
            // Calculate the quaternion
            float w = cr * cp * cy + sr * sp * sy;
            float x = sr * cp * cy - cr * sp * sy;
            float y = cr * sp * cy + sr * cp * sy;
            float z = cr * cp * sy - sr * sp * cy;
            return [x, y, z, w];
        }
        public static float[] RemoveFirstIfEligible(float[] inputArray)
        {
            // Check if the input array has 4 or 5 elements
            if (inputArray.Length == 4 || inputArray.Length == 5)
            {
                // Use LINQ to skip the first element and return the rest
                return inputArray.Skip(1).ToArray();
            }
            // If the condition is not met, return the original array
            return inputArray;
        }
         public static float[] Rotation_Quaternion_To_Euler(float[] quaternion) { 
        
            if (quaternion.Length != 4)
            {
               // MessageBox.Show($"Quaternion array must be 4 values, but is {quaternion.Length}: {string.Join(", ", quaternion)}");
                throw new Exception($"Quaternion array must be 4 values, but is {quaternion.Length}: {string.Join(", ", quaternion)}");
            }
            float x = quaternion[0];
            float y = quaternion[1];
            float z = quaternion[2];
            float w = quaternion[3];
            // Calculate Euler angles from quaternion
            float sinr_cosp = 2 * (w * x + y * z);
            float cosr_cosp = 1 - 2 * (x * x + y * y);
            float roll = (float)Math.Atan2(sinr_cosp, cosr_cosp);
            float sinp = 2 * (w * y - z * x);
            float pitch;
            if (Math.Abs(sinp) >= 1)
                pitch = (float)Math.CopySign(Math.PI / 2, sinp); // use 90 degrees if out of range
            else
                pitch = (float)Math.Asin(sinp);
            float siny_cosp = 2 * (w * z + x * y);
            float cosy_cosp = 1 - 2 * (y * y + z * z);
            float yaw = (float)Math.Atan2(siny_cosp, cosy_cosp);
            // Convert radians to degrees
            roll = roll * 180f / (float)Math.PI;
            pitch = pitch * 180f / (float)Math.PI;
            yaw = yaw * 180f / (float)Math.PI;
            return new float[] { roll, pitch, yaw };
        }

    public static bool AreArraysEqual(float[] array1, float[] array2)
        {
            // Check if both arrays are null
            if (array1 == null && array2 == null)
            {
                return true;
            }
            // Check if one of the arrays is null
            if (array1 == null || array2 == null)
            {
                return false;
            }
            // Check if the arrays have different lengths
            if (array1.Length != array2.Length)
            {
                return false;
            }
            // Compare each element in the arrays
            for (int i = 0; i < array1.Length; i++)
            {
                if (array1[i] != array2[i])
                {
                    return false;
                }
            }
            return true;
        }
       
        internal  static   string ResetNumbers(string input)
        {
            if (input.Trim().Length == 0) { return ""; }
            // Split the input string by commas
            string[] numbers = input.Split(',');
            // Define a regular expression pattern to match integers or doubles
            string pattern = @"^\s*-?\d+(\.\d+)?\s*$";
            // Loop through each number and reset it to 0 if it matches the pattern
            for (int i = 0; i < numbers.Length; i++)
            {
                if (Regex.IsMatch(numbers[i], pattern))
                {
                    numbers[i] = "0";
                }
            }
            // Join the modified numbers back into a string with commas
            string result = string.Join(", ", numbers);
            return result;
        }
        internal static double[] ConvertStringToDoubleArray(string input)
        {
            // Split the input string by commas
            string[] substrings = input.Split(',');
            // Create a double array to store the converted values
            double[] result = new double[substrings.Length];
            // Convert each substring to double and store in the result array
            for (int i = 0; i < substrings.Length; i++)
            {
                if (double.TryParse(substrings[i], out double value))
                {
                    result[i] = value;
                }
                else
                {
                    // Handle invalid input
                    throw new ArgumentException("Input contains non-numeric values.");
                }
            }
            return result;
        }
        internal static bool AreIntervalsEqual(int start1, int end1, int start2, int end2)
        {
            // Sort the intervals
            int min1 = Math.Min(start1, end1);
            int max1 = Math.Max(start1, end1);
            int min2 = Math.Min(start2, end2);
            int max2 = Math.Max(start2, end2);
            // Check if the intervals are equal
            return min1 == min2 && max1 == max2;
        }
       internal static int ConvertKeyframe(int start1, int end1, int start2, int end2, int keyframe)
        {
            // Calculate the length of each interval
            int length1 = end1 - start1;
            int length2 = end2 - start2;
            // Calculate the position of the keyframe in the second interval
            int equivalentPosition = start2 + ((keyframe - start1) * length2) / length1;
            return equivalentPosition;
        }

        internal static float SafeRotation(float angle, bool increase)
        {
            float f = angle;
            if (increase)
            {
                angle++;
                if (angle > 360) { angle = -360; }
            }
            else
            {
                angle--;
                if (angle < -360) { angle = 360; }
            }
            return f;
        }

        internal static float GetDecimalPart(float value)
        {
            return value - (float)Math.Floor(value);
        }

        internal static double GetDistanceBetweenControls(FrameworkElement control1, FrameworkElement control2)
        {
            if (control1 == null || control2 == null)
                throw new ArgumentNullException("Both controls must be non-null.");

            // Transform the top-left corner of each control to the same coordinate space (e.g., their common ancestor).
            var transform1 = control1.TransformToAncestor(GetCommonAncestor(control1, control2));
            var transform2 = control2.TransformToAncestor(GetCommonAncestor(control1, control2));

            Point position1 = transform1.Transform(new Point(0, 0));
            Point position2 = transform2.Transform(new Point(0, 0));

            // Get the bounding rectangles of the controls
            Rect rect1 = new Rect(position1, new Size(control1.ActualWidth, control1.ActualHeight));
            Rect rect2 = new Rect(position2, new Size(control2.ActualWidth, control2.ActualHeight));

            // Calculate the minimum distance between the edges of the rectangles
            double horizontalDistance = Math.Max(0, Math.Max(rect1.Left, rect2.Left) - Math.Min(rect1.Right, rect2.Right));
            double verticalDistance = Math.Max(0, Math.Max(rect1.Top, rect2.Top) - Math.Min(rect1.Bottom, rect2.Bottom));

            // Return the Euclidean distance between the closest edges
            return Math.Sqrt(horizontalDistance * horizontalDistance + verticalDistance * verticalDistance);

        }
        private static FrameworkElement GetCommonAncestor(FrameworkElement control1, FrameworkElement control2)
        {
            DependencyObject parent1 = control1;
            while (parent1 != null)
            {
                DependencyObject parent2 = control2;
                while (parent2 != null)
                {
                    if (parent1 == parent2)
                        return parent1 as FrameworkElement;
                    parent2 = VisualTreeHelper.GetParent(parent2);
                }
                parent1 = VisualTreeHelper.GetParent(parent1);
            }
            throw new InvalidOperationException("The controls do not share a common ancestor.");
        }
    }
}
