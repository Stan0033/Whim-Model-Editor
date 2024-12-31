using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
namespace MDLLibs.Classes.Misc
{
    internal static class AppIO
    {
        public static bool IsFileLargerThan10MB(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("The file does not exist.", filePath);
            }

            FileInfo fileInfo = new FileInfo(filePath);
            long fileSizeInBytes = fileInfo.Length;
            long fileSizeInMegabytes = fileSizeInBytes / 1024 / 1024; // Convert bytes to megabytes

            return fileSizeInMegabytes > 10;
        }
        public static string AppPath =  System.IO.Path.GetDirectoryName(System.Reflection.Assembly
                           .GetExecutingAssembly().GetName().CodeBase) .Replace("file:\\", "");
        public static string SettingsPath = Path.Combine(AppPath, "AppData.ini");
        public static string LanguagePath = Path.Combine(AppPath, "Languages");
        public static string GroundTexturePath = Path.Combine(AppPath, "ground_texture.jpg");
       
      
       
        
    }
}
