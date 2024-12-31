﻿using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;
using System.Drawing;
using War3Net.IO.Mpq;
using War3Net.Drawing.Blp;
using System.Drawing.Imaging;
using MDLLibs.Classes.Misc;

namespace W3_Texture_Finder
{
    internal static class MPQHelper
    {
         

        internal static List<string> Listfile_blp_War3 = new List<string>();
        internal static List<string> Listfile_blp_War3x = new List<string>();
        internal static List<string> Listfile_blp_War3Patch = new List<string>();
        internal static List<string> Listfile_blp_War3xLocal = new List<string>();

        internal static List<string> Listfile_All = new List<string>();
        
        internal static void Initialize()
        {

            string War3PatchPath = Path.Combine(MPQPaths.local, "Paths\\War3Patch.txt");
            if (File.Exists(War3PatchPath) == false) { MessageBox.Show($"{War3PatchPath} not found"); Environment.Exit(0); }
            Listfile_blp_War3Patch = File.ReadAllLines(War3PatchPath).Where(line => line.EndsWith(".blp")).ToList();

            LoadDataBrowserLists(MPQPaths.War3, Listfile_blp_War3);
            LoadDataBrowserLists(MPQPaths.War3X, Listfile_blp_War3x);
            LoadDataBrowserLists(MPQPaths.War3xLocal, Listfile_blp_War3xLocal);
            Listfile_All.AddRange(Listfile_blp_War3);
            Listfile_All.AddRange(Listfile_blp_War3x);
            Listfile_All.AddRange(Listfile_blp_War3Patch);
            Listfile_All.AddRange(Listfile_blp_War3xLocal);

        }
        internal static bool FileExists(string path)
        {
            if (Listfile_All.Contains(path)) { return true; }
            return File.Exists(path);
        }
        internal static bool FileExists(string path, string Archive)
        {
            if (Archive == MPQPaths.War3) { return Listfile_blp_War3.Contains(path); }
            if (Archive == MPQPaths.War3X) { return Listfile_blp_War3x.Contains(path); }
            if (Archive == MPQPaths.War3Patch) { return Listfile_blp_War3Patch.Contains(path); }
            if (Archive == MPQPaths.War3xLocal) { return Listfile_blp_War3xLocal.Contains(path); }
            return false;
        }
        internal static void Export(string targetPath, string savePath)
        {
            string archive = string.Empty;
            if (FileExists(targetPath, MPQPaths.War3)) { archive = MPQPaths.War3; }
            if (FileExists(targetPath, MPQPaths.War3X)) { archive = MPQPaths.War3X; }
            if (FileExists(targetPath, MPQPaths.War3xLocal)) { archive = MPQPaths.War3xLocal; }
            if (FileExists(targetPath, MPQPaths.War3Patch)) { archive = MPQPaths.War3Patch; }
            using (MpqArchive mpqArchive = MpqArchive.Open(archive))
            {
                if (mpqArchive != null)
                {
                    // Specify the file path within the MPQ archive
                    // Check if the file exists in the archive
                    if (mpqArchive.FileExists(targetPath))
                    {
                        using (MpqStream mpqStream = mpqArchive.OpenFile(targetPath))
                        {
                            // Create a FileStream and write the contents of the MPQ stream directly to it
                            using (FileStream fileStream = new FileStream(savePath, FileMode.Create))
                            {
                                byte[] buffer = new byte[4096]; // 4KB buffer size
                                int bytesRead;
                                while ((bytesRead = mpqStream.Read(buffer, 0, buffer.Length)) > 0)
                                {
                                    fileStream.Write(buffer, 0, bytesRead);
                                }
                            }
                        }
                    }
                }
            }
        }
        internal static void ExportPNG(ImageSource imageSource, string outputPath)
        {
            // Convert ImageSource to BitmapSource
            var bitmapSource = (BitmapSource)imageSource;
            // Create a PngBitmapEncoder
            var encoder = new PngBitmapEncoder();
            // Add the BitmapSource to the encoder
            encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
            // Create a FileStream to write the image file
            using (var fileStream = new FileStream(outputPath, FileMode.Create))
            {
                // Save the image to the FileStream
                encoder.Save(fileStream);
            }
        }
        internal static ImageSource GetImageSource(string path)
        {
            string archive = string.Empty;
            if (FileExists(path, MPQPaths.War3)) { archive = MPQPaths.War3; }
            if (FileExists(path, MPQPaths.War3X)) { archive = MPQPaths.War3X; }
            if (FileExists(path, MPQPaths.War3xLocal)) { archive = MPQPaths.War3xLocal; }
            if (FileExists(path, MPQPaths.War3Patch)) { archive = MPQPaths.War3Patch; }
            if (archive.Length == 0) // Could be in local folder
            {
                if (File.Exists(path))
                {
                    using (FileStream mpqStream = File.OpenRead(path))
                    {
                        // Read from the stream
                        byte[] buffer = new byte[mpqStream.Length];
                        mpqStream.Read(buffer, 0, buffer.Length);
                        //-----------------------
                        // Save the file
                        //-----------------------
                        string outputPath = MPQPaths.temp;
                        File.WriteAllBytes(outputPath, buffer);
                        using (var fileStream = File.OpenRead(outputPath))
                        {
                            var blpFile = new BlpFile(fileStream);
                            var bitmapSource = blpFile.GetBitmapSource();
                            // Delete the temporary file
                            File.Delete(outputPath);
                            // Return the ImageSource
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Missing file");
                    return null;
                }
            }
            else
            {
                using (MpqArchive mpqArchive = MpqArchive.Open(archive))
                {
                    // Check if the archive is valid
                    if (mpqArchive != null)
                    {
                        // Check if the file exists in the archive
                        if (mpqArchive.FileExists(path))
                        {
                            // Open the file stream
                            using (MpqStream mpqStream = mpqArchive.OpenFile(path))
                            {
                                // Read from the stream
                                byte[] buffer = new byte[mpqStream.Length];
                                mpqStream.Read(buffer, 0, buffer.Length);
                                //-----------------------
                                // Save the file
                                //-----------------------
                                string outputPath = AppHelper.TemporaryBLPLocation;
                              
                                File.WriteAllBytes(outputPath, buffer);
                                using (var fileStream = File.OpenRead(outputPath))
                                {
                                    var blpFile = new BlpFile(fileStream);
                                    var bitmapSource = blpFile.GetBitmapSource();
                                    // Delete the temporary file
                                    //   File.Delete(outputPath);
                                    // Return the ImageSource
                                    return bitmapSource;
                                }
                            }
                        }
                        else
                        {
                            return null; // or throw an exception indicating file not found
                        }
                    }
                    else
                    {
                        return null; // or throw an exception indicating invalid archive
                    }
                }
            }
            return null;
        }
        static ImageSource ReplaceTransparentPixelsWithBlack(BitmapSource bitmapSource)
        {
            var formatConvertedBitmap = new FormatConvertedBitmap();
            formatConvertedBitmap.BeginInit();
            formatConvertedBitmap.Source = bitmapSource;
            formatConvertedBitmap.DestinationFormat = PixelFormats.Bgra32;
            formatConvertedBitmap.EndInit();
            var width = formatConvertedBitmap.PixelWidth;
            var height = formatConvertedBitmap.PixelHeight;
            var stride = width * 4;
            var pixels = new byte[height * stride];
            formatConvertedBitmap.CopyPixels(pixels, stride, 0);
            for (int i = 3; i < pixels.Length; i += 4)
            {
                if (pixels[i] == 0) // Check if the pixel is fully transparent
                {
                    pixels[i - 3] = 0; // Set blue channel to 0
                    pixels[i - 2] = 0; // Set green channel to 0
                    pixels[i - 1] = 0; // Set red channel to 0
                }
            }
            var blackBitmapSource = BitmapSource.Create(width, height, 96, 96, PixelFormats.Bgra32, null, pixels, stride);
            return blackBitmapSource;
        }
        //         MPQPaths.temp
        internal static Bitmap GetImage(string path)
        {
            string archive = string.Empty;
            if (FileExists(path, MPQPaths.War3)) { archive = MPQPaths.War3; }
            if (FileExists(path, MPQPaths.War3X)) { archive = MPQPaths.War3X; }
            if (FileExists(path, MPQPaths.War3xLocal)) { archive = MPQPaths.War3xLocal; }
            if (FileExists(path, MPQPaths.War3Patch)) { archive = MPQPaths.War3Patch; }
            if (archive.Length == 0) // could be in local folder
            {
                if (File.Exists(path))
                {
                    using (FileStream mpqStream = File.OpenRead(path))
                    {
                        // Read from the stream
                        byte[] buffer = new byte[mpqStream.Length];
                        mpqStream.Read(buffer, 0, buffer.Length);
                        //-----------------------
                        // save the file
                        //-----------------------
                        string outputPath = AppHelper.TemporaryBLPLocation;
                        File.WriteAllBytes(outputPath, buffer);
                        using (var fileStream = File.OpenRead(outputPath))
                        {
                            var blpFile = new BlpFile(fileStream);
                            var bitmapSource = blpFile.GetBitmapSource();
                            // Convert BitmapSource to Bitmap
                            var bitmap = new Bitmap(bitmapSource.PixelWidth, bitmapSource.PixelHeight, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
                            var data = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
                            bitmapSource.CopyPixels(Int32Rect.Empty, data.Scan0, data.Height * data.Stride, data.Stride);
                            bitmap.UnlockBits(data);
                            // Delete the temporary file
                            File.Delete(outputPath);
                            // Return the Bitmap
                            return bitmap;
                        }
                    }
                }
                else
                {
                    MessageBox.Show($"Could not find the texture at \"{path}\"");
                      return GetWhiteBitmap();
                  
                }
            }
            else
            {
                using (MpqArchive mpqArchive = MpqArchive.Open(archive))
                {
                    // Check if the archive is valid
                    if (mpqArchive != null)
                    {
                        // Specify the file path within the MPQ archive
                        // Check if the file exists in the archive
                        if (mpqArchive.FileExists(path))
                        {
                            // Open the file stream
                            using (MpqStream mpqStream = mpqArchive.OpenFile(path))
                            {
                                // Read from the stream
                                byte[] buffer = new byte[mpqStream.Length];
                                mpqStream.Read(buffer, 0, buffer.Length);
                                //-----------------------
                                // save the file
                                //-----------------------
                                string outputPath = MPQPaths.temp;
                                File.WriteAllBytes(outputPath, buffer);
                                using (var fileStream = File.OpenRead(outputPath))
                                {
                                    var blpFile = new BlpFile(fileStream);
                                    var bitmapSource = blpFile.GetBitmapSource();
                                    // Convert BitmapSource to Bitmap
                                    var bitmap = new Bitmap(bitmapSource.PixelWidth, bitmapSource.PixelHeight, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
                                    var data = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
                                    bitmapSource.CopyPixels(Int32Rect.Empty, data.Scan0, data.Height * data.Stride, data.Stride);
                                    bitmap.UnlockBits(data);
                                    // Delete the temporary file
                                    // File.Delete(outputPath);
                                    // Return the Bitmap
                                    return bitmap;
                                }
                            }
                        }
                        else
                        {
                            MessageBox.Show($"Could not find the texture at \"{path}\"");
                            return GetWhiteBitmap();

                        }
                    }
                    else
                    {
                        MessageBox.Show($"Invalid archive");
                        return GetWhiteBitmap();
                    }
                }
            }
          
        }
        private static Bitmap GetWhiteBitmap()
        {
            return  MPQHelper.GetImage("Textures\\white.blp");
        }
        private static void LoadDataBrowserLists(string Archive, List<string> list_blp)
        {
            string searched = "(listfile)";
            using (MpqArchive mpqArchive = MpqArchive.Open(Archive))
            {
                using (MpqStream mpqStream = mpqArchive.OpenFile(searched))
                {
                    // Read from the stream
                    byte[] buffer = new byte[mpqStream.Length];
                    mpqStream.Read(buffer, 0, buffer.Length);
                    //-----------------------
                    // save the file
                    //-----------------------
                    string outputPath = MPQPaths.temp;
                    File.WriteAllBytes(outputPath, buffer);
                    foreach (string item in File.ReadAllLines(outputPath))
                    {
                        if (Path.GetExtension(item) == ".blp")
                        {
                            list_blp.Add(item);
                        }

                    }
                }
            }
        }




    }
}
