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
    /// Interaction logic for CMessageBox.xaml
    /// </summary>
    public partial class CMessageBox : Window
    {
        public CMessageBox( )
        {
            InitializeComponent();
             
        }
        public void Show(string message, int height = 200)
        {
           
            MultilineTextBox.Text = message;
            Height = height;
            ShowDialog();
        }
        public void Show(string message, string title, int height = 200)
        {
            
            Title = title;
            MultilineTextBox.Text = message;
            Height = height;
            ShowDialog();
           
        }

        private void copy(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(MultilineTextBox.Text);
        }

        private void ok(object sender, RoutedEventArgs e)
        {
            MultilineTextBox.Text = "";
            Hide();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
        }
    }
}
