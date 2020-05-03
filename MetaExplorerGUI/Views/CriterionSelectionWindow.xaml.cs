using MetaExplorer.Domain;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace MetaExplorerGUI
{
    /// <summary>
    /// Interaction logic for CriterionSelectionWindow.xaml
    /// </summary>
    public partial class CriterionSelectionWindow : Window
    {
        public int SelectedIndex
        {
            get;
            set;
        }

        public CriterionSelectionWindow(ObservableCollection<CriterionInstance> list)
        {
            InitializeComponent();

            this.SelectedIndex = -1;
                criterionListBox.ItemsSource = list;

                //no focus means no keyboard navigation until you click in it with the mouse
                criterionListBox.Focus();
        }

        private void OnCloseCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            this.Close();
        }

        private void CriterionListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            this.SelectedIndex = criterionListBox.SelectedIndex;
            this.Close();
        }
    }
}
