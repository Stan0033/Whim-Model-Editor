using MDLLib;
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
    /// Interaction logic for Create_Skeleton.xaml
    /// </summary>
    public partial class Create_Skeleton : Window
    {
        w3Model CurrentModel;
        public Create_Skeleton(w3Model model)
        {
            InitializeComponent();
            CurrentModel = model;
        }

        private void ok(object sender, RoutedEventArgs e)
        {
            //unfinished
        }
    }
}
