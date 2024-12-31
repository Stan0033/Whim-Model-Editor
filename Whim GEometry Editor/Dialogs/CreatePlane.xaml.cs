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
using Whim_GEometry_Editor.Misc;

namespace Whim_GEometry_Editor.Dialogs
{
    /// <summary>
    /// Interaction logic for CreatePlane.xaml
    /// </summary>
    public partial class CreatePlane : Window
    {
        w3Model model;
        public CreatePlane(w3Model m)
        {
            InitializeComponent();
            model = m;
        }

        private void Create(object sender, RoutedEventArgs e)
        {
            bool parsedcol = int.TryParse(InputColumns.Text, out int columns);
            bool parsedrow = int.TryParse(InputRows.Text, out int rows);
            bool parsedcell = int.TryParse(InputCell.Text, out int cellsize);

            if (parsedcol && parsedrow && parsedcell)
            {
                if (columns <= 0 || rows <= 0 || cellsize <= 0)
                {
                    MessageBox.Show("Invalid input. Only integers greater than 0.", "Invalid request"); return;
                }
                else
                {
                    w3Geoset geo = ShapeGenerator.CreateGrid(columns, rows, cellsize);
                    geo.ID = IDCounter.Next();
                import_geoset ig = new import_geoset("Generated plane",model,  new List<w3Geoset>() { geo} );
                    ig.ShowDialog();
                    if (ig.DialogResult == true) {  DialogResult = true; }
                }

                }
            else
            {
                MessageBox.Show("Invalid input. Only integers greater than 0.", "Invalid request");return;
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) { DialogResult = false; }
        }
    }
}
