using MetaExplorer.Common;
using MetaExplorer.Domain;
using Microsoft.Extensions.Configuration;
using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace MetaExplorerGUI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private IConfigurationRoot configRoot;
        private IConfigurationRoot configCrit;

        public App() : base()
        {
            this.Dispatcher.UnhandledException += OnDispatcherUnhandledException;

            // read base path for appsettings.json and critsettings.json.
            // if this bnase path exists, we know we are in staging or productive environment.
            // if not, we are in development mode.
            var basePath = ConfigurationManager.AppSettings.Get("ConfigBasePath");
            if (basePath == null || !Directory.Exists(basePath))
                basePath = Directory.GetCurrentDirectory();

            var builderRoot = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false);
            configRoot = builderRoot.Build();

            var builderCrit = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("critsettings.json", optional: false, reloadOnChange: false);
            configCrit = builderCrit.Build();
        }

        void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            string errorMessage = string.Format("An unhandled exception occurred: {0}\n, StackTrace:\n{1}", e.Exception.Message, e.Exception.StackTrace);
            Trace.TraceError(errorMessage);
            MessageBox.Show(errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        }

        [STAThread]
        private void ApplicationStart(object sender, StartupEventArgs e)
        {
            CriteriaConfig criteriaConfig = new CriteriaConfig();
            criteriaConfig.LoadFromAppSettings(configCrit);

            var mainWindow = new MainWindow(configRoot, criteriaConfig);
            mainWindow.Show();
            mainWindow.DoInitStuff();

            foreach (Criterion crit in criteriaConfig.Criteria)
            {
                mainWindow.AddDynamicCriterionButton(crit);
            }
        }
    }
}
