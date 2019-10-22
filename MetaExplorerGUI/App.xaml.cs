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
        //private IHost _host;
        private IConfigurationRoot configRoot;

        public App() : base()
        {
            this.Dispatcher.UnhandledException += OnDispatcherUnhandledException;
            //var bla = Directory.GetCurrentDirectory();
            //_host = new HostBuilder()
            //    .ConfigureHostConfiguration(config =>
            //        config.SetBasePath(Directory.GetCurrentDirectory())
            //              .AddJsonFile("appsettings.json", optional: false))
            //    .ConfigureAppConfiguration(config =>
            //        config.SetBasePath(Directory.GetCurrentDirectory())
            //              .AddJsonFile("appsettings.json", optional: false))
            //    .ConfigureServices((context, services) =>
            //    {
            //        //services.AddOptions();
            //        services.Configure<Settings>(context.Configuration.GetSection("section0"));
            //        services.AddSingleton<ITextService, TextService>();
            //        services.Configure<Application>(context.Configuration.GetSection("section0"));

            //        //                    services.AddSingleton<MainWindow>();
            //    })
            //    .Build();

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();
            configRoot = builder.Build();

            //_host.Start();
            
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
           // _host.Start();
            //var config = _host.Services.GetService<ITextService>();
            var config = configRoot.GetSection("section0");
            CriteriaConfig.LoadFromAppSettings(config.GetValue<string>("CriterionConfig"));

            //show the main window
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
