using ImageProcessing.ViewModels;
using ImageProcessing.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ImageProcessing
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        MainWindowViewModel viewModel = null;

        public static AppSettings appSettings = new AppSettings();

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if (!appSettings.Load())
            {
                MessageBox.Show("Could not load config file", "Error");
                App.Current.Shutdown();
                return;
            }

            var window = new MainWindow();
            viewModel = new MainWindowViewModel();
            window.DataContext = viewModel;
            window.Loaded += viewModel.OnViewLoaded;
            window.Closing += viewModel.OnWindowClosing;

            window.Show();
        }

        private void Application_DispatcherUnhandledException(object sender,
            System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            System.Windows.MessageBox.Show(e.Exception.ToString(), "Unhandled exception, shutting down");
            Application.Current.Shutdown();
        }
    }
}
