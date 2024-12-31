using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whim_GEometry_Editor.Misc
{
    static class BinaryConverter
    {

       public static string FloatToBinary(float value)
        {
            // Convert the float to a byte array
            byte[] bytes = BitConverter.GetBytes(value);

            // Create a string by converting each byte to a char representation
            StringBuilder sb = new StringBuilder();
            foreach (byte b in bytes)
            {
                sb.Append((char)b); // Treat each byte as a character
            }

            // Return the resulting string
            return sb.ToString();
        }

       public static float BinaryToFloat(string value)
        {
            // Create a byte array from the string's characters
            byte[] bytes = new byte[value.Length];
            for (int i = 0; i < value.Length; i++)
            {
                bytes[i] = (byte)value[i]; // Convert each char back to byte
            }

            // Convert the byte array back to a float
            return BitConverter.ToSingle(bytes, 0);
        }
    }
}
