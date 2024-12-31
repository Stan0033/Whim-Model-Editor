 
using System.Windows;
using System.IO;
using System.Windows.Shapes;
using MdxLib.ModelFormats;
using Path = System.IO.Path;
using System.Windows.Input;

namespace Whim_GEometry_Editor.Dialogs
{
    /// <summary>
    /// Interaction logic for MDLX_Window.xaml
    /// </summary>
    public partial class MDLX_Window : Window
    {
        public MDLX_Window()
        {
            InitializeComponent();
        }

        private void DropFrame_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                foreach (string file in files)
                {
                    if (System.IO.Path.GetExtension(file).ToLower() == ".mdl")
                    {
                        var Model = new MdxLib.Model.CModel();
                        using (var Stream = new System.IO.FileStream(file, System.IO.FileMode.Open, System.IO.FileAccess.Read))
                        {
                            var ModelFormat = new MdxLib.ModelFormats.CMdx();
                            ModelFormat.Load(file, Stream, Model);
                        }
                        string ToFileName = Path.Combine(Path.GetDirectoryName(file), Path.GetFileNameWithoutExtension(file) + ".mdx");
                        using (var Stream = new System.IO.FileStream(ToFileName, System.IO.FileMode.Create, System.IO.FileAccess.Write))
                        {
                            var ModelFormat = new MdxLib.ModelFormats.CMdx();
                            ModelFormat.Save(ToFileName, Stream, Model);
                        }
                    }
                    if (System.IO.Path.GetExtension(file).ToLower() == ".mdx")
                    {
                        var Model = new MdxLib.Model.CModel();
                        using (var Stream = new System.IO.FileStream(file, System.IO.FileMode.Open, System.IO.FileAccess.Read))
                        {
                            var ModelFormat = new MdxLib.ModelFormats.CMdx();
                            ModelFormat.Load(file, Stream, Model);
                        }
                        string ToFileName = Path.Combine(Path.GetDirectoryName(file), Path.GetFileNameWithoutExtension(file) + ".mdl");
                        using (var Stream = new System.IO.FileStream(ToFileName, System.IO.FileMode.Create, System.IO.FileAccess.Write))
                        {
                            var ModelFormat = new MdxLib.ModelFormats.CMdl();
                            ModelFormat.Save(ToFileName, Stream, Model);
                        }
                    }
                }
            }
        }

        
        private void ClosingW(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Escape) { DialogResult = false; }
        }
    }
}
