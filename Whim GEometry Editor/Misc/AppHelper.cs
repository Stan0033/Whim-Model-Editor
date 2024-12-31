using System.IO;
using System.Windows;
using System.Windows.Controls;
 
using System.Text.RegularExpressions;
using System.Drawing;
using System.Text;
using System;
using Whim_Model_Editor;
using Whim_GEometry_Editor.Dialogs;


namespace MDLLibs.Classes.Misc
{
    public enum PrimitiveShape
    {
        Cube, Cyllinder, Cone, Sphere
    }
    public enum SelectionType
    {
        None,
        ClearSelect,
        AddSelect,
        Deselect
    }
    public enum PlaybackMode
    {
        None, Default,Loop,Cycle
    }
    enum TransformationCategory
    {
        CamPosition,
        CamTarget,
        CamRotation,
        GeosetAnimAlpha,
        GeosetAnimColor,
        TextureAnimTranslation,
        TextureAnimRotation,
        TextureAnimScaling,
        LayerAlpha,
        LayerTextureID,
        NodeTranslation,
        NodeRotation,
        NodeScaling,
        NodeVisibility,
        NodeAlpha,
        NodeColor,
        NodeAmbientColor,
        NodeIntensity,
        NodeAmbientIntensity,
        NodeHeightAbove,
        NodeHeightBelow,
        NodeEmissionRate,
        NodeGravity,
        NodeInitialVelocity,
        NodeLatitude,
        NodeLongitude,
        NodeSpeed,
        NodeVariation,
        NodeWidth,
        NodeLength,
        NodeTextureSlot,
        NodeLifespan,
        NodeAttenuationStart,
        NodeAttenuationEnd,
        None
    }
    public enum AntialiasingTechnique
    {
        None,            // No antialiasing applied
        MSAA_2x,        // Multisample Anti-Aliasing with 2 samples
        MSAA_4x,        // Multisample Anti-Aliasing with 4 samples
        MSAA_8x,        // Multisample Anti-Aliasing with 8 samples
        FXAA,           // Fast Approximate Anti-Aliasing
        SMAA,           // Subpixel Morphological Anti-Aliasing
        SSAA,           // Supersample Anti-Aliasing
        Blending        // Alpha blending for smooth edges
    }

  public static class AppInfo
    {
        public static string Version = "0.9";
        public static string AppTitle = "Whim! Model editor";
    }
    public static class AppHelper
    {
        public static string GetTime()
        {
            // Get the current date and time
            DateTime now = DateTime.Now;

            // Get the current time zone
            string timeZone = TimeZoneInfo.Local.StandardName;

            // Format the time as "day month year hour:minute:second TimeZone"
            string formattedTime = now.ToString($"dd MMMM yyyy HH:mm:ss {timeZone}");

            return formattedTime;
        }

        public static string appPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly
                           .GetExecutingAssembly().GetName().CodeBase).Replace("file:\\", "");

        public static string DataPath = Path.Combine(appPath, "Data");
        public static bool IndexExists<T>(List<T> list, int index)
        {
            // Check if index is within the valid range of the list
            return index >= 0 && index < list.Count;
        }

      
        internal static string TempMDLLocation = Path.Combine(appPath, "Temp\\temp.mdl");
        internal static string TemporaryBLPLocation= Path.Combine(appPath, "Temp\\temp.blp");

        internal static void ConvertMDXToMDL(string InputMdx)
        {
            //
            // The files to load from and save to.
            //
            var ToFileName = Path.Combine(appPath, "Temp\\temp.mdl");
            //
            // Creates a model.
            //
            var Model = new MdxLib.Model.CModel();
            //
            // Loads the model from a file (using the MDX format).
            //
            using (var Stream = new System.IO.FileStream(InputMdx, System.IO.FileMode.Open, System.IO.FileAccess.Read))
            {
                string name = Path.GetFileName(InputMdx);
                var ModelFormat = new MdxLib.ModelFormats.CMdx();
                ModelFormat.Load(name, Stream, Model);
            }
            //
            // Saves the model to a file (using the MDL format).
            //
            using (var Stream = new System.IO.FileStream(ToFileName, System.IO.FileMode.Create, System.IO.FileAccess.Write))
            {
                string name = Path.GetFileName(InputMdx);
                var ModelFormat = new MdxLib.ModelFormats.CMdl();
                ModelFormat.Save(name, Stream, Model);
            }
            FilterMDLGeneratedFileFromMDX(ToFileName);
         //   File.Delete(ToFileName); test - enable later
        }
        internal static void ConvertMDLToMDX(string SaveLocation)
        {
            //
            // The files to load from and save to.
            //
            var FromFileName = Path.Combine(appPath, "Temp\\temp.mdl");
            //
            // Creates a model.
            //
            var Model = new MdxLib.Model.CModel();
            //
            // Loads the model from a file (using the MDX format).
            //
            using (var Stream = new System.IO.FileStream(SaveLocation, System.IO.FileMode.Open, System.IO.FileAccess.Read))
            {
                string name = Path.GetFileName(FromFileName);
                var ModelFormat = new MdxLib.ModelFormats.CMdl();
                ModelFormat.Load(name, Stream, Model);
            }
            //
            // Saves the model to a file (using the MDL format).
            //
            using (var Stream = new System.IO.FileStream(FromFileName, System.IO.FileMode.Create, System.IO.FileAccess.Write))
            {
                string name = Path.GetFileName(FromFileName);
                var ModelFormat = new MdxLib.ModelFormats.CMdx();
                ModelFormat.Save(name, Stream, Model);
            }
        }
        static void FilterMDLGeneratedFileFromMDX(string filePath)
        {
            try
            {
                // Read the content of the file
                string content = File.ReadAllText(filePath);
                // Define the regular expression pattern to match allowed characters
                string pattern = @"[^a-zA-Z0-9{}"".,\-\/+|\\ \s\n\t]"; // Note the addition of "\\" to include backslash
                // Remove disallowed characters from the content
                string filteredContent = Regex.Replace(content, pattern, "");
                // Write the filtered content back to the file
                File.WriteAllText(filePath, filteredContent);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error while filtering mdl generated file from mdx: "+ ex.Message);
                
            }
        }
        public static void RemoveSelectedListboxItems(ListBox listBox)
        {
            // Create a list to store the selected items
            List<object> selectedItems = new List<object>(listBox.SelectedItems.Count);
            // Copy the selected items to the list
            foreach (var item in listBox.SelectedItems)
            {
                selectedItems.Add(item);
            }
            // Remove each item from the ListBox
            foreach (var item in selectedItems)
            {
                listBox.Items.Remove(item);
            }
        }

        public static bool ListContainsIndex<T>(List<T> list, int index) { return index >= 0 && index < list.Count; }
    }
}
