using MetaExplorerBE;
using MetaExplorerBE.Configuration;
using MetaExplorerBE.MetaModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;

namespace MetaExplorerGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ViewModel myViewModel;
        private MetaExplorerManager me;

        private Point dragDropStartPoint;

        Dictionary<string, GroupBox> dynamicCriterionMap = new Dictionary<string, GroupBox>();

        public ViewModel MyViewModel
        {
            get { return myViewModel; }
        }

        public MainWindow()
        {
            InitializeComponent();

        }

        public void DoInitStuff()
        {
            int videoThumbNailHeight = Int32.Parse(FindResource("VideoThumbnailHeight").ToString());
            int videoThumbNailWidth = Int32.Parse(FindResource("VideoThumbnailWidth").ToString());
            int criterionThumbNailHeight = Int32.Parse(FindResource("CriterionThumbnailHeight").ToString());
            int criterionThumbNailWidth = Int32.Parse(FindResource("CriterionThumbnailWidth").ToString());

            //init meta model manager with dependency injection
            ICache cache = new Cache(MetaExplorerGUI.Properties.Settings.Default.VideoFilesBasePath, 
                                    MetaExplorerGUI.Properties.Settings.Default.FFmpegLocation,
                                    videoThumbNailHeight,
                                    videoThumbNailWidth,
                                    criterionThumbNailHeight,
                                    criterionThumbNailWidth
            );
            me = new MetaExplorerManager(cache);

            //this.myItemsControl.ItemTemplate.

            this.ProgressAsync(me.Cache.UpdateVideoFileCacheAsync(), "Updating Video File Cache");
            this.ProgressAsync(me.Cache.UpdateVideoMetaModelCacheAsync(), "Updating Video Meta Model Cache");
            this.ProgressAsync(me.Cache.UpdateNonExistingThumbnailCacheAsync(), "Updating Thumbnails");

            foreach (Criterion crit in CriteriaConfig.Criteria)
            {
                this.ProgressAsync(me.Cache.GenerateDictAsync(crit), "Updating " + crit.Name + " Dictionary");
            }

            myViewModel = new ViewModel();
            myViewModel.MEManager = me;
            this.DataContext = myViewModel;
            myItemsControl.DataContext = myViewModel;
            myViewModel.PropertyChanged += Event_PropertyChanged;

            myViewModel.UpdateMMref();
            myViewModel.UpdateCurrentSelection();
        }

        private async void ProgressAsync(Task task, string progressHeading)
        {
            ProgressWindow.DoWorkWithModal(progressHeading, (progressMsg, progress) =>
            {
                while (me.Cache.Progress < 100)
                {
                    progress.Report(me.Cache.Progress);
                    progressMsg.Report(String.Format("Processing: <{0}>", me.Cache.ProgressFile));
                    System.Threading.Thread.Sleep(1);
                }
            });

            await task;
        }

        public void AddDynamicCriterionButton(Criterion crit)
        {
            var template = this.FindResource("CriterionTemplate") as ControlTemplate;

            GroupBox gb = new GroupBox()
            {
                Template = template,
            };
            gb.ApplyTemplate();

            Button clearButton = gb.Template.FindName("ClearButton", gb) as Button;
            clearButton.Content = "Clear";

            var groupBox = gb.Template.FindName("CriterionGroupbox", gb) as GroupBox;
            groupBox.Header = crit.Name;

            Button selectButton = gb.Template.FindName("KeywordSelectionButton", gb) as Button;

            DockPanel sp = selectButton.Content as DockPanel;
            Label label = sp.FindName("SelectionLabel") as Label;
            label.Content = "Select " + crit.Name;
            Image image = sp.FindName("SelectionImage") as Image;

            selectButton.Click += (object sender, RoutedEventArgs args) =>
            {
                CriterionSelectionWindow w = new CriterionSelectionWindow(myViewModel.MEManager.Cache.GetCriterionInstances(crit));
                w.ShowDialog();

                if (w.SelectedIndex != -1)
                {
                    myViewModel.CurrentCriterionSelectionIndex[crit.Name] = w.SelectedIndex;
                    CriterionInstance ci = this.me.Cache.GetCriterionInstances(crit)[w.SelectedIndex];

                    //update the label and the picture of the button
                    label.Content = ci.Name;
                    image.Source = ci.ImageSource;

                    //update the MMref
                    myViewModel.UpdateMMref();

                    //update the current selection
                    myViewModel.UpdateCurrentSelection();
                }
            };

            clearButton.Click += (object sender, RoutedEventArgs args) =>
            {
                myViewModel.CurrentCriterionSelectionIndex[crit.Name] = -1;

                //update the label of the button
                label.Content = "Select " + crit.Name;

                //update the picture of the button
                image.Source = null;

                //update the MMref
                myViewModel.UpdateMMref();

                //update the current selection
                myViewModel.UpdateCurrentSelection();
            };

            this.dynamicCriterionMap[crit.Name] = gb;
            this.CriterionStackPanel.Children.Add(gb);
        }

        /// <summary>
        /// Handles changes in the model, reflecting this change in the UI (view)
        /// </summary>
        private void Event_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (CriteriaConfig.Criteria.Exists((Criterion x) => e.PropertyName == x.Name) || e.PropertyName == "FreeTextSearch")
            { 
                myViewModel.UpdateMMref();
                myViewModel.UpdateCurrentSelection();
            }
        }


        //private VideoMetaModel GetVideoMetaModelFromButton(Button button)
        //{
        //    int foundIndex = -1;
        //    for (int i = 0; i < this.myViewModel.CurrentFileSelectionCount; i++)
        //    {
        //        ContentPresenter depobj = myItemsControl.ItemContainerGenerator.ContainerFromIndex(i) as ContentPresenter;
        //        DataTemplate dataTemplate = depobj.ContentTemplate;
        //        Button candidate = dataTemplate.FindName("MyButton", depobj) as Button;
        //        if (candidate == button)
        //        {
        //            foundIndex = i;
        //            break;
        //        }
        //    }

        //    if (foundIndex == -1)
        //        throw new Exception("Cound not map button to video meta model.");

        //    return this.myViewModel.CurrentFileSelection[foundIndex];
        //}

        private void ClearFreeTextSearchButton_Click(object sender, RoutedEventArgs e)
        {
            this.MyViewModel.FreeTextSearch = String.Empty;
        }

        
        private void VideoSelectionButton_Click(object sender, RoutedEventArgs e)
        {
            //VideoMetaModel found = this.GetVideoMetaModelFromButton(sender as Button);
            VideoMetaModel found = (sender as Button).DataContext as VideoMetaModel;
            Helper.Play(MetaExplorerGUI.Properties.Settings.Default.VLCLocation, found.FileName);
        }

        private void PlayRandomButton_Click(object sender, RoutedEventArgs e)
        {
            //get current displayed files
            int count = this.myViewModel.CurrentFileSelection.Count;

            Random rand = new Random();
            int rnd = rand.Next(count);   //0..count-1

            VideoMetaModel result = this.myViewModel.CurrentFileSelection[rnd];
            Helper.Play(MetaExplorerGUI.Properties.Settings.Default.VLCLocation, result.FileName);
        }

        private void ContextMenuItem_OpenInExplorer_Click(object sender, RoutedEventArgs e)
        {
            VideoMetaModel found = (sender as MenuItem).DataContext as VideoMetaModel;

            string argument = @"/select, " + found.FileName;
            Process.Start("explorer.exe", argument);
        }

        #region Drag & Drop

        /// <summary>
        /// Remember the first occurence of a left mouse button click. USed for Drag&Drop operation.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ThumbnailButton_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Store the mouse position
            dragDropStartPoint = e.GetPosition(null);
        }

        /// <summary>
        /// Handler for the Drag&Drop operation.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ThumbnailButton_MouseMove(object sender, MouseEventArgs e)
        {
            // Get the current mouse position
            Point mousePos = e.GetPosition(null);
            Vector diff = dragDropStartPoint - mousePos;
            VideoMetaModel videoMetaModel = (sender as Button).DataContext as VideoMetaModel;

            if (e.LeftButton == MouseButtonState.Pressed &&
               (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
            {
                // Get the dragged button
                Button button = sender as Button;
                
                //perform the drag
                DragDrop.DoDragDrop(button, videoMetaModel, DragDropEffects.All);
            }
        }

        /// <summary>
        /// Handler which is called once the groupbox enters the dropzone.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CriterionGroupbox_DragEnter(object sender, DragEventArgs e)
        {
            GroupBox gb = sender as GroupBox;
            if (!IsMouseInside(gb))
            {
                this.SetGroupboxBorder(gb, false);
            }
            else
            {
                this.SetGroupboxBorder(gb, true);
            }
        }

        /// <summary>
        /// Handler which is called once the groupbox leaves the dropzone.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CriterionGroupbox_DragLeave(object sender, DragEventArgs e)
        {
            GroupBox gb = sender as GroupBox;

            if (IsMouseInside(gb))
            {
                this.SetGroupboxBorder(gb, true);
            }
            else
            {
                this.SetGroupboxBorder(gb, false);
            }
        }

        /// <summary>
        /// Handler which is called once the groupbox is dropped.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CriterionGroupbox_Drop(object sender, DragEventArgs e)
        {
            string expectedDragType = typeof(VideoMetaModel).FullName;

            if (e.Data.GetDataPresent(expectedDragType))
            {
                VideoMetaModel vmm = e.Data.GetData(typeof(VideoMetaModel)) as VideoMetaModel;
                GroupBox gb = sender as GroupBox;
                Label label = gb.FindName("SelectionLabel") as Label;
                Image image = gb.FindName("SelectionImage") as Image;

                this.SetGroupboxBorder(gb, false);

                string critName = gb.Header as string;
                List<CriterionInstance> ciList = this.me.Cache.GetCriterionInstances(critName);

                //get the criterion instance
                CriterionInstance ci = null;
                int ciIndex = -1;

                //No criterion, e.g.[bla1][bla2][3Star][][] no criterion instance for criterion "keyword" is defined
                if (vmm.criteriaContents[critName].Count == 0)
                {
                    return;
                }
                //Multiple criteria, e.g.[bla1][bla2][3Star][][bla3_bla4] we have two criteria instances for criterion "keyword", namely "bla3" and "bla4"
                else if (vmm.criteriaContents[critName].Count > 1)
                {
                    //for now we just take the first criteria and neglect the others. TODO: offer a selection box??
                    string searchCritInstanceName = vmm.criteriaContents[critName][0];
                    ci = ciList.FirstOrDefault(x => String.Compare(x.Name, searchCritInstanceName, true) == 0);
                    ciIndex = ciList.IndexOf(ci);
                }
                //default case: one criteria
                else
                {
                    string searchCritInstanceName = vmm.criteriaContents[critName][0];
                    ci = ciList.FirstOrDefault(x => String.Compare(x.Name, searchCritInstanceName, true) == 0);
                    ciIndex = ciList.IndexOf(ci);
                }

                //update the label and the picture of the button
                label.Content = ci.Name;
                image.Source = ci.ImageSource;

                //important: set the current selection index such that the MMref can be set up properly
                myViewModel.CurrentCriterionSelectionIndex[critName] = ciIndex;

                //update the MMref
                myViewModel.UpdateMMref();

                //update the current selection
                myViewModel.UpdateCurrentSelection();
            }
        }

        /// <summary>
        /// Change the border of a groupbox to thick red or thin grey.
        /// </summary>
        /// <param name="groupBox"></param>
        /// <param name="dropReady"></param>
        private void SetGroupboxBorder(GroupBox groupBox, bool dropReady)
        {
            if (dropReady)
            {
                groupBox.BorderBrush = new SolidColorBrush(Colors.Red);
                groupBox.BorderThickness = new Thickness(3);
            }
            else
            {
                groupBox.BorderBrush = new SolidColorBrush(Colors.Gray);
                groupBox.BorderThickness = new Thickness(0.5);
            }
        }


        /// <summary>
        /// Checks if mouse is inside a control. 
        /// This is a replacememnt for Control.IsMOuseOver which does not work in drag/drop mode.
        /// </summary>
        /// <param name="control"></param>
        /// <returns></returns>
        private bool IsMouseInside(GroupBox control)
        {
            System.Drawing.Point mousePos = System.Windows.Forms.Control.MousePosition;
            Point p = new Point(0d, 0d);
            Point controlPos = control.PointToScreen(p);

            //magic diff without which it does not work for drag&drop in groupboxes
            const int DIFF = 10;

            bool inside =   mousePos.X >= controlPos.X && 
                            mousePos.Y >= controlPos.Y + DIFF &&
                            mousePos.X <= controlPos.X + control.ActualWidth && 
                            mousePos.Y <= controlPos.Y + control.ActualHeight;

            return inside;
        }

        #endregion
    }
}
