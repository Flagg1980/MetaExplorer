using MetaExplorer.Common;
using MetaExplorer.Domain;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
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

        public App() : base()
        {
            this.Dispatcher.UnhandledException += OnDispatcherUnhandledException;

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();
            configRoot = builder.Build();         
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
            var config = configRoot.GetSection("section0");
            CriteriaConfig.LoadFromAppSettings(config.GetValue<string>("CriterionConfig"));

            var mainWindow = new MainWindow(config);
            mainWindow.Show();
            mainWindow.DoInitStuff();

            foreach (Criterion crit in CriteriaConfig.Criteria)
            {
                mainWindow.AddDynamicCriterionButton(crit);
            }
        }
    }
}
