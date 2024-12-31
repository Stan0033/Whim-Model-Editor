using MDLLibs.Classes.Misc;
using System;
using System.Collections.Generic;
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
    /// Interaction logic for About_w.xaml
    /// </summary>
    public partial class About_w : Window
    {
        public About_w()
        {
            InitializeComponent();
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder
                .AppendLine($"{AppInfo.AppTitle} v{AppInfo.Version} by Stan0033 (C) 2024")
                .AppendLine($"A warcraft 3 modelling tool.")
                .AppendLine($"Built with Visual Studio 2022, C# 12.0 and .NET 8.0")
                .AppendLine($"Credits:")
                .AppendLine($"- MPQ read, BLP read by Drake53's War3Net libraries.")
                .AppendLine($"- Model rendering and visual interaction achieved with SharpGL.")
                .AppendLine($"- MDL Parser by me.")
                .AppendLine($"- MDL/MDX Conversion by Magos' MDXLib.")
                .AppendLine($"- Color Picker for WPF by Dsafa.")


                ;

            Data.Text = stringBuilder.ToString();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) { DialogResult = false; }
        }
    }
}
