using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace MetaExplorerGUI
{
    /// <summary>
    /// Interaction logic for Progress.xaml
    /// </summary>
    public partial class ProgressWindow : Window
    {
        public ProgressWindow()
        {
            InitializeComponent();
        }

        public static void DoWorkWithModal(string heading, Func<IProgress<int>, IProgress<string>, Task> work)
        {
            ProgressWindow splash = new ProgressWindow();
            Task task = null;
            

            splash.Loaded += (_, args) =>
            {
                splash.HeadingTextBlock.Text = heading;
                Progress<string> progressMsg = new Progress<string>(data => splash.MyProgressTextblock.Text = data);
                Progress<int> progress = new Progress<int>(data => splash.MyProgressBar.Value = data);

                Action action = new Action(async () =>
                {
                    await work(progress, progressMsg);

                    splash.Close();
                });

                task = Task.Factory.StartNew(action, 
                                            CancellationToken.None, 
                                            TaskCreationOptions.None, 
                                            TaskScheduler.FromCurrentSynchronizationContext());

            };

            splash.ShowDialog();
            task.Wait();
        }
    }
}
