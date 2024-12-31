using MDLLib;
using Microsoft.Win32;
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
using static test_parser_mdl.Parser_MDL;
using test_parser_mdl;
using Whim_GEometry_Editor.Misc;

namespace Whim_GEometry_Editor.Dialogs
{
    /// <summary>
    /// Interaction logic for import_animations.xaml
    /// </summary>
    public partial class import_animations : Window
    {
        w3Model model;
        private bool AndSequences = false;
        public int SequencesAdded = 0;
         

        public import_animations(w3Model m)
        {
            InitializeComponent();
            model = m;
        }
        private string GetFile()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "MDL Files (*.mdl)|*.mdl", // Filter for .mdl files
                Title = "Open MDL File" // Title for the dialog
            };

            // Show the dialog and check if a file was selected
            bool? result = openFileDialog.ShowDialog();

            if (result == true)
            {
                return openFileDialog.FileName;

            }
            return "";
        }
        private void import(object sender, RoutedEventArgs e)
        {  //-------------------------------------------


            AndSequences = check_addsequences.IsChecked == true;




            //--------------------------------------s-----
            string filename = "";
            filename = GetFile();
            if (filename == "") { return; }
            //-------------------------------------------
            w3Model TemporaryModel = new w3Model();
            List<Token> tokens = Parser_MDL.Tokenize(filename);
            List<TemporaryObject> temporaryObjects = Parser_MDL.SplitCollectObjects(tokens);
            TemporaryModel = Parser_MDL.Parse(temporaryObjects);

            TemporaryModel.FinalizeComponents();
            TemporaryModel.Optimize();
            //-------------------------------------------
            foreach (w3Node TemporaryNpde in TemporaryModel.Nodes)
            {
                if (model.Nodes.Any(x => x.Name.ToLower() == TemporaryNpde.Name.ToLower()))
                {
                    w3Node native = model.Nodes.First(x => x.Name.ToLower() == TemporaryNpde.Name.ToLower());
                    native.Translation = TemporaryNpde.Translation.Clone();
                    native.Rotation = TemporaryNpde.Rotation.Clone();
                    native.Scaling = TemporaryNpde.Scaling.Clone();
                }
            }
            if (AndSequences)
            {
                foreach (w3Sequence sequence in TemporaryModel.Sequences)
                {
                    if (model.Sequences.Any(x=>x.Name.ToLower() == sequence.Name.ToLower()) == false)
                    {
                        // sequence is not used
                        // check if sequence has overlapping interval with another
                        if (IntervalExists(sequence) == false)
                        {
                            // if not then
                            // check if is animated
                            if (SequenceAnimated(sequence))
                            {
                                model.Sequences.Add(sequence.Clone());
                                SequencesAdded++;
                            }
                            // if yes then add it to the list
                        }


                    }
                }

                DialogResult = true;
            }


        }
        private bool IntervalExists(w3Sequence givenSequence)
        {
            return model.Sequences.Any(sequence =>
                sequence.From <= givenSequence.From && sequence.To >= givenSequence.To);
        }

        private bool SequenceAnimated(w3Sequence givenSequence)
        {
            foreach (w3Node node in model.Nodes)
            {
                if (node.Translation.Keyframes.Any(x=>x.Track >= givenSequence.From && x.Track <= givenSequence.To))
                {
                    return true;
                }
                if (node.Rotation.Keyframes.Any(x => x.Track >= givenSequence.From && x.Track <= givenSequence.To))
                {
                    return true;
                }
                if (node.Scaling.Keyframes.Any(x => x.Track >= givenSequence.From && x.Track <= givenSequence.To))
                {
                    return true;
                }
            }
            return false;
        }





        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) { DialogResult = false; }
        }
    }
}
