using System;
using System.ComponentModel;
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

        public static void DoWorkWithModal(string heading, Action<IProgress<string> , IProgress<int>> work)
        {
            ProgressWindow splash = new ProgressWindow();
            splash.HeadingTextBlock.Text = heading;

            splash.Loaded += (_, args) =>
            {
                BackgroundWorker worker = new BackgroundWorker();

                Progress<string> progressMsg = new Progress<string>(data => splash.MyProgressTextblock.Text = data);
                Progress<int> progress = new Progress<int>(data => splash.MyProgressBar.Value = data);

                worker.DoWork += (s, workerArgs) => work(progressMsg, progress);

                worker.RunWorkerCompleted += (s, workerArgs) =>
                {
                    //handle if an exception was thrown in the backgound worker thread
                    if (workerArgs.Error != null)
                    {
                        MessageBox.Show(String.Format("There was an error in the background worker thread: <{0}>!", workerArgs.Error.ToString()));
                    }

                    splash.Close();
                };

                worker.RunWorkerAsync();
            };

            splash.ShowDialog();
        }
    }
}
