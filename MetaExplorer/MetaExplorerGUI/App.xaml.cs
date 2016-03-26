using MetaExplorerBE.Configuration;
using MetaExplorerBE.MetaModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace MetaExplorerGUI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App() : base()
        {
            this.Dispatcher.UnhandledException += OnDispatcherUnhandledException;
        }

        void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            string errorMessage = string.Format("An unhandled exception occurred: {0}\n, StackTrace:\n{1}", e.Exception.Message, e.Exception.StackTrace);
            MessageBox.Show(errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        }

        private void ApplicationStart(object sender, StartupEventArgs e)
        {
            //show the main window
            var mainWindow = new MainWindow();
            mainWindow.Show();

            mainWindow.DoInitStuff();

            foreach (Criterion crit in CriteriaConfig.Criteria)
            {
                mainWindow.AddDynamicCriterionButton(crit);
            }
        }
    }
}
