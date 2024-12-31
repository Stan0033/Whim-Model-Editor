using MDLLibs.Classes.Misc;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Whim_GEometry_Editor.Misc
{
    public static class ExtensionHelper
    {
        public static void Register()
        {
           
                if (IsAdmin() == false)
                {
                    MessageBox.Show("This command requires running the app as administrator.", "No elevated access");
                    return;
                }
            else
            {
                MessageBoxResult result = MessageBox.Show($"You will be able to open MDL/MDX files by right-clicking on them. Explorer.exe will restart. Continue?", "Register extensions", MessageBoxButton.YesNo, MessageBoxImage.Question);


                if (result == MessageBoxResult.Yes)
                {

                    string exepath = Path.Combine(AppIO.AppPath, "Whim Model Editor.exe");
                    string icopath_native = Path.Combine(AppIO.AppPath, "file.ico");

                    AssociateFile(exepath, icopath_native, ".mdx");
                    AssociateFile(exepath, icopath_native, ".mdl");
                }
                
            }
               
               
            


            // Handle the result
          

        }
        internal static bool IsAdmin()
        {
            // Get the current Windows identity
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            // Create a Windows principal from the identity
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            // Check if the principal is in the administrators group
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
        internal static void AssociateFile(string appExePath, string iconPath, string fileExtension)
        {
            if (!File.Exists(iconPath)) { MessageBox.Show($"{iconPath} was not found"); return; }
            // Set the file type name
            string fileType = $"ExampleFileType_{Guid.NewGuid()}";
            // Associate the file extension with the file type
            using (RegistryKey extKey = Registry.ClassesRoot.CreateSubKey(fileExtension))
            {
                extKey.SetValue("", fileType);
            }
            // Create a subkey for the file type
            using (RegistryKey fileTypeKey = Registry.ClassesRoot.CreateSubKey(fileType))
            {
                fileTypeKey.SetValue("", "Whim! Model");
                // Associate the icon with the file type
                using (RegistryKey defaultIconKey = fileTypeKey.CreateSubKey("DefaultIcon"))
                {
                    defaultIconKey.SetValue("", iconPath);
                }
                // Create a subkey for the shell command
                using (RegistryKey shellKey = fileTypeKey.CreateSubKey("shell"))
                {
                    // Create a subkey for the open command
                    using (RegistryKey openKey = shellKey.CreateSubKey("open"))
                    {
                        // Set the command for opening the file
                        openKey.CreateSubKey("command").SetValue("", $"\"{appExePath}\" \"%1\"");
                    }
                }
            }
            // Set the program as the default application for the file extension
            using (RegistryKey extKey = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\FileExts\\" + fileExtension, true))
            {
                extKey?.DeleteSubKey("UserChoice", false);
            }
            using (RegistryKey userChoiceKey = Registry.CurrentUser.CreateSubKey($"Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\FileExts\\{fileExtension}\\UserChoice"))
            {
                userChoiceKey.SetValue("ProgId", fileType, RegistryValueKind.String);
            }
            // Restart Windows Explorer to refresh the icon cache
            RestartExplorer();
        }
        internal static void RestartExplorer()
        {
            // Find and terminate the Explorer process
            foreach (var process in Process.GetProcessesByName("explorer"))
            {
                process.Kill();
            }
        }
    }
}
