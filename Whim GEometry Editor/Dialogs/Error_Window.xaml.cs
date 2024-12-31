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
    /// Interaction logic for Error_Window.xaml
    /// </summary>
    public partial class Error_Window : Window
    {
        string Message;
        public Error_Window(string message)
        {
            InitializeComponent();
            Message = message;
            FlowDocument d = new FlowDocument();
            d.Blocks.Add(new Paragraph(new Run(message)));
            Box.Document = d;
        }

        private void ok(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void copy(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(Message);
        }
    }
}
