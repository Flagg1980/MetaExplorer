using MetaExplorer.Common;
using MetaExplorer.Domain;
using System.Diagnostics;
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
            Trace.TraceError(errorMessage);
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
