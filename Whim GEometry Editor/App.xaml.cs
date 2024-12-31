using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Windows;
using Whim_GEometry_Editor.Dialogs;

namespace Whim_Model_Editor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            //base.OnStartup(e);

            // Retrieve command-line arguments
            string[] args = Environment.GetCommandLineArgs();

            // Check if a file path is provided as an argument
            if (args.Length > 1 )
            {
                if (File.Exists(args[1]))
                {
                    string filePath = args[1];

                    // Open the MainWindow and pass the file path to it
                    MainWindow mainWindow = new MainWindow(filePath);
                    mainWindow.Show();
                }
            }
            else
            {
                // Start without file if no file path is provided
                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();
            }
        }
     
    public App( )
        {
            // Handle unhandled exceptions in the UI thread
            this.DispatcherUnhandledException += OnDispatcherUnhandledException;

            // Handle unhandled exceptions in non-UI threads
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

            // Handle unobserved task exceptions
            TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
        }

        // For exceptions thrown in the main UI thread
        private void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            DisplayError(e.Exception);
            e.Handled = true; // Prevents the app from closing
        }

        // For exceptions thrown in non-UI threads
        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = (Exception)e.ExceptionObject;
            DisplayError(ex);
        }

        // For unobserved task exceptions
        private void OnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            DisplayError(e.Exception);
            e.SetObserved(); // Prevents the process from terminating
        }

        // Method to display error and stack trace
        private void DisplayError(Exception ex)
        {
            Error_Window ew = new Error_Window($"An unexpected error occurred:\n\n{ex.Message}\n\nStack Trace:\n{ex.StackTrace}");
            ew.ShowDialog();
            

             Environment.Exit(0);   
        }
    }
}


