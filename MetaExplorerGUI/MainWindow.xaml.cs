using MetaExplorer.Common;
using MetaExplorer.Domain;
using MetaExplorerBE;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace MetaExplorerGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ViewModel myViewModel;
        private bool myWindowInitiallyCompletelyRendered = false;
        private Point myDragDropStartPoint;
        private IConfiguration myConfig;

        private Dictionary<Criterion, Button> dynamicCriterionButtons = new Dictionary<Criterion, Button>();

        public MainWindow(IConfiguration config)
        {
            myConfig = config;
            InitializeComponent();
        }

        public void DoInitStuff()
        {
            int videoThumbNailHeight = Int32.Parse(FindResource("VideoThumbnailHeight").ToString());
            int videoThumbNailWidth = Int32.Parse(FindResource("VideoThumbnailWidth").ToString());
            int criterionThumbNailHeight = Int32.Parse(FindResource("CriterionThumbnailHeight").ToString());
            int criterionThumbNailWidth = Int32.Parse(FindResource("CriterionThumbnailWidth").ToString());

            //INIT cache Video Files
            var videoFileCache = new VideoFileCache(myConfig.GetValue<string>("VideoFilesBasePath"));
            ProgressWindow.DoWorkWithModal("Updating Video File Cache", videoFileCache.InitCacheAsync);

            //INIT cache video thumbnails
            var videoThumbnailCache = new VideoThumbnailCache(
                Path.Combine(myConfig.GetValue<string>("CriterionFilesBasePath"), ".cache"),
                videoThumbNailHeight,
                videoThumbNailWidth,
                myConfig.GetValue<string>("FFmpegLocation")
                );
            ProgressWindow.DoWorkWithModal("Updating Video Thumbnails", videoThumbnailCache.InitCacheAsync);

            //INIT cache criterion thumbnails
            List<string> criterionThumbPaths = CriteriaConfig.Criteria.Select(x => Path.Combine(myConfig.GetValue<string>("CriterionFilesBasePath"), x.Name)).ToList();
            var criterionThumbnailCache = new ImageThumbnailCache(
                criterionThumbPaths,
                criterionThumbNailHeight,
                criterionThumbNailWidth
                );
            ProgressWindow.DoWorkWithModal("Updating Criterion Thumbnails", criterionThumbnailCache.InitCacheAsync);

            //INIT cache video meta model
            var videoMetaModelCache = new VideoMetaModelCache(
                videoFileCache,
                videoThumbnailCache
            );
            ProgressWindow.DoWorkWithModal("Updating Video Meta Model Cache", videoMetaModelCache.InitCacheAsync);
            ProgressWindow.DoWorkWithModal("Updating Video Meta Model Cache", videoMetaModelCache.UpdateNonExistingThumbnailCacheAsync);

            //INIT cache criterion
            var criterionCache = new CriterionCache(criterionThumbnailCache, videoMetaModelCache);
            ProgressWindow.DoWorkWithModal("Updating Criterion Cache", criterionCache.InitCacheAsync);

            myViewModel = new ViewModel(criterionCache, videoFileCache, videoMetaModelCache);

            this.DataContext = myViewModel;
            myItemsControl.DataContext = myViewModel;
            myViewModel.PropertyChanged += Event_PropertyChanged;

            myViewModel.UpdateMMref();
            myViewModel.UpdateCurrentSelection();
        }

        private void UpdateCriterionButton(Criterion crit, string labelText, ImageSource image)
        {
            Button selectButton = this.dynamicCriterionButtons[crit];

            var sp = selectButton.Content as DockPanel;
            Label label = sp.FindName("SelectionLabel") as Label;
            label.Content = labelText;
            Image i = sp.FindName("SelectionImage") as Image;
            i.Source = image;
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

            Button selectButton = gb.Template.FindName("SelectButton", gb) as Button;
            selectButton.Name = crit.Name + "SelectButton";

            DockPanel sp = selectButton.Content as DockPanel;
            Label label = sp.FindName("SelectionLabel") as Label;
            label.Content = "Select " + crit.Name;
            Image image = sp.FindName("SelectionImage") as Image;

            this.dynamicCriterionButtons.Add(crit, selectButton);

            selectButton.Click += (object sender, RoutedEventArgs args) =>
            {
                //CriterionSelectionWindow w = new CriterionSelectionWindow(myViewModel.CriterionCache.GetCriterionInstances(crit));
                
                //w.ShowDialog();

                //if (w.SelectedIndex != -1)
                //{
                //    myViewModel.CurrentCriterionSelectionIndex[crit.Name] = w.SelectedIndex;
                    //CriterionInstance ci = this.myViewModel.CriterionCache.GetCriterionInstances(crit)[w.SelectedIndex];

                //    //update the label and the picture of the button
                //    label.Content = ci.Name;
                //    image.Source = ci.Thumbnail.Image;

                //    //update the MMref
                //    myViewModel.UpdateMMref();

                //    //update the current selection
                //    myViewModel.UpdateCurrentSelection();
                //}
                //UpdateCriterionButton(selectButton, "Select " + crit.Name, ci.Thumbnail.Image);

                SwitchToCriterionThumbnailView(crit);
            };

            clearButton.Click += (object sender, RoutedEventArgs args) =>
            {
                myViewModel.CurrentCriterionSelectionIndex[crit.Name] = -1;

                //update the label of the button
                //label.Content = "Select " + crit.Name;

                UpdateCriterionButton(crit, "Select " + crit.Name, null);

                //update the picture of the button
                //image.Source = null;
                //UpdateCriterionButton(selectButton, "Select " + crit.Name, null);
                //update the MMref
                myViewModel.UpdateMMref();

                //update the current selection
                myViewModel.UpdateCurrentSelection();

                SwitchToVideoThumbnailView();
            };

            this.CriterionStackPanel.Children.Add(gb);
        }

        private void SwitchToCriterionThumbnailView(Criterion crit)
        {
            myItemsControl.ItemsSource = myViewModel.CriterionCache.GetCriterionInstances(crit);

            MyRandomButton.IsEnabled = false;
        }

        private void SwitchToVideoThumbnailView()
        {
            myItemsControl.ItemsSource = myViewModel.CurrentFileSelection;

            MyRandomButton.IsEnabled = true;
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

        private void ClearFreeTextSearchButton_Click(object sender, RoutedEventArgs e)
        {
            this.myViewModel.FreeTextSearch = String.Empty;
        }

        private void ThumbnailSelectionButton_Click(object sender, RoutedEventArgs e)
        {
            var button = (sender as Button);

            // found is null if the double clicked thumbnail is a criterion
            if (button.DataContext is Video)
            {
                var video = button.DataContext as Video;
                Helper.Play(myConfig.GetValue<string>("VLCLocation"), video.File.FullName);
            }
            else if (button.DataContext is CriterionInstance)
            {
                var criterionInstance = button.DataContext as CriterionInstance;
                
                //update the label and the picture of the button
                //label.Content = criterionInstance.Name;
                //image.Source = criterionInstance.Thumbnail.Image;

                UpdateCriterionButton(criterionInstance.Criterion, criterionInstance.Name, criterionInstance.Thumbnail.Image);

                SwitchToVideoThumbnailView();

                //myViewModel.CurrentCriterionSelectionIndex[criterionInstance.Criterion.Name] = 
                myViewModel.UpdateMMref();

                myViewModel.UpdateCurrentSelection();
            }
        }

        private void PlayRandomButton_Click(object sender, RoutedEventArgs e)
        {
            //get current displayed files
            int count = this.myViewModel.CurrentFileSelection.Count;

            Random rand = new Random();
            int rnd = rand.Next(count);   //0..count-1

            Video result = this.myViewModel.CurrentFileSelection[rnd];

            //bring video into view
            Button button = GetButtonByVideoMetaModel(result);
            button.BringIntoView();

            //play the video
            Helper.Play(myConfig.GetValue<string>("VLCLocation"), result.File.FullName);

            //myItemsControl.ItemsSource = myViewModel.CurrentCriterionInstanceList;

            //ContentPresenter depobj = myItemsControl.ItemContainerGenerator.ContainerFromItem(myItemsControl) as ContentPresenter;
            //DataTemplate dataTemplate = depobj.ContentTemplate;

            //if (myItemsControl.ItemsSource == myViewModel.CurrentCriterionInstanceList)
            //    myItemsControl.ItemsSource = myViewModel.CurrentFileSelection;
            //else
            //    myItemsControl.ItemsSource = myViewModel.CurrentCriterionInstanceList;
        }

        private void ContextMenuItem_OpenInExplorer_Click(object sender, RoutedEventArgs e)
        {
            Video found = (sender as MenuItem).DataContext as Video;

            string argument = @"/select, " + found.File.FullName;
            Process.Start("explorer.exe", argument);
        }

        #region Drag & Drop

        /// <summary>
        /// Remember the first occurence of a left mouse button click. USed for Drag&Drop operation.
        /// </summary>
        private void ThumbnailButton_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Store the mouse position
            myDragDropStartPoint = e.GetPosition(null);
        }

        /// <summary>
        /// Handler for the Drag&Drop operation.
        /// </summary>
        private void ThumbnailButton_MouseMove(object sender, MouseEventArgs e)
        {
            // Get the current mouse position
            Point mousePos = e.GetPosition(null);
            Vector diff = myDragDropStartPoint - mousePos;
            Video videoMetaModel = (sender as Button).DataContext as Video;

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
        private void CriterionGroupbox_Drop(object sender, DragEventArgs e)
        {
            string expectedDragType = typeof(Video).FullName;

            if (e.Data.GetDataPresent(expectedDragType))
            {
                Video vmm = e.Data.GetData(typeof(Video)) as Video;
                GroupBox gb = sender as GroupBox;
                Label label = gb.FindName("SelectionLabel") as Label;
                Image image = gb.FindName("SelectionImage") as Image;

                this.SetGroupboxBorder(gb, false);

                string critName = gb.Header as string;
                ObservableCollection<CriterionInstance> ciList = this.myViewModel.CriterionCache.GetCriterionInstances(critName);

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
                image.Source = ci.Thumbnail.Image;

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
        private bool IsMouseInside(GroupBox control)
        {
            //System.Drawing.Point mousePos = System.Windows.Forms.Control.MousePosition;
            var mousePos = Mouse.GetPosition(control);
            Point p = new Point(0d, 0d);
            Point controlPos = control.PointToScreen(p);

            //magic diff without which it does not work for drag&drop in groupboxes
            const int DIFF = 10;

            bool inside = mousePos.X >= controlPos.X &&
                            mousePos.Y >= controlPos.Y + DIFF &&
                            mousePos.X <= controlPos.X + control.ActualWidth &&
                            mousePos.Y <= controlPos.Y + control.ActualHeight;

            return inside;
        }

        #endregion

        private Button GetButtonByVideoMetaModel(Video vmm)
        {
            ContentPresenter depobj = myItemsControl.ItemContainerGenerator.ContainerFromItem(vmm) as ContentPresenter;
            DataTemplate dataTemplate = depobj.ContentTemplateSelector.SelectTemplate(vmm, depobj);
            Button candidate = dataTemplate.FindName("MyButton", depobj) as Button;

            return candidate;
        }

        private void SortingSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //the first time this event occurs during rendering of the main window, 
            //this one we want to ignore
            if (!myWindowInitiallyCompletelyRendered)
                return;

            var selecteditem = e.AddedItems[0] as ComboBoxItem;
            string itemstr = selecteditem.Content.ToString().ToLower();

            if (itemstr.Contains("date"))
            {
                this.myViewModel.VideoMetaModelCache.ResortBy(x => x.File.LastWriteTime);
            }
            else if (itemstr.Contains("resolution"))
            {
                this.myViewModel.VideoMetaModelCache.ResortBy(x => x.FrameHeight * x.FrameWidth);
            }
            else if (itemstr.Contains("bitrate"))
            {
                this.myViewModel.VideoMetaModelCache.ResortBy(x => x.BitRate);
            }
            else
            {
                string errormsg = String.Format("Unknown content in selected sort item <{0}>", itemstr);
                throw new Exception(errormsg);
            }

            this.myViewModel.UpdateCurrentSelection();
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            myWindowInitiallyCompletelyRendered = true;
        }
    }
}
