using MetaExplorer.Common;
using MetaExplorer.Domain;
using MetaExplorerBE;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace MetaExplorerGUI
{
    public class ViewModel : INotifyPropertyChanged
    {
        #region Private Members

        private int currentFileSelectionCount = 0;
        private long currentFileSelectionSize = 0;
        
        private string _freeTextSearch = string.Empty;

        public IVideoFileCache VideoFileCache { get; set; }
        public VideoMetaModelCache VideoMetaModelCache { get; set; }
        public ICriterionCache CriterionCache { get; set; }
        public ICollection ThumbnailViewContent { get; set; }

        private readonly ICriteriaConfig myCriteriaConfig;

        #endregion

        #region Public Properties

        //TODO: do we really need this? is it not a duplicate of this.MMRef.criteriaMapping??
        public List<CriterionInstance> CurrentCriterionFilter { get; set; }

        public string FreeTextSearch
        {
            get { return this._freeTextSearch; }
            set { this._freeTextSearch = value; OnPropertyChanged("FreeTextSearch");  }
        }

        public Video MMRef { get; private set; } = null;

        public ObservableCollection<Video> CurrentFileSelection { get; private set; }

        public int CurrentFileSelectionCount
        {
            get { return this.currentFileSelectionCount; }
            set { this.currentFileSelectionCount = value; OnPropertyChanged(); }
        }

        public long CurrentFileSelectionSize
        {
            get { return this.currentFileSelectionSize; }
            set { this.currentFileSelectionSize = value; OnPropertyChanged(); }
        }

        public string CurrentFileSelectionSizeHR
        {
            get { return String.Format("{0:0.##} GB", (float)this.currentFileSelectionSize/(float)(1024*1024*1024)); }
        }

        #endregion

        #region C'tor

        public ViewModel(ICriterionCache criterionCache, IVideoFileCache videoFileCache, VideoMetaModelCache videoMetaModelCache, ICriteriaConfig criteriaConfig)
        {
            CriterionCache = criterionCache;
            VideoFileCache = videoFileCache;
            VideoMetaModelCache = videoMetaModelCache;
            myCriteriaConfig = criteriaConfig;

            CurrentFileSelection = new ObservableCollection<Video>();

            CurrentCriterionFilter = new List<CriterionInstance>();
        }

        #endregion

        #region Public Methods

        public void UpdateCurrentSelection()
        {
            CurrentFileSelection.Clear();
            CurrentFileSelection = this.VideoMetaModelCache.GetVideoFileSelection(MMRef);

            this.CurrentFileSelectionCount = this.CurrentFileSelection.Count();

            this.CurrentFileSelectionSize = 0;
            foreach (Video vmm in this.CurrentFileSelection) 
            { 
                this.CurrentFileSelectionSize += vmm.File.Length; 
            }
        }

        /// <summary>
        /// Adds a specific item from the VideoMetaModelCache to the current selection.
        /// </summary>
        /// <param name="index">The index of the VideoMetaModelCache to be added.</param>
        public void AddToCurrentSelection(int index)
        {
            CurrentFileSelection.Add(this.VideoMetaModelCache.CachedItems[index]);
            this.CurrentFileSelectionCount++;
            this.CurrentFileSelectionSize += this.VideoMetaModelCache.CachedItems[index].File.Length;
        }

        public void UpdateMMref()
        { 
            this.MMRef = new Video();

            MMRef.ThumbnailCaption1 = this.FreeTextSearch;

            this.MMRef.criteriaMapping = this.CurrentCriterionFilter;
        }

        public event PropertyChangedEventHandler PropertyChanged;  
        public void OnPropertyChanged(string Property = "")  
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(Property));
        }

        #endregion

        public void SwitchToCriterionThumbnailView(Criterion crit)
        {
            ThumbnailViewContent = CriterionCache.GetCriterionInstances(crit);
            OnPropertyChanged();
        }

        public void SwitchToVideoThumbnailView()
        {
            ThumbnailViewContent = CurrentFileSelection;
            OnPropertyChanged();
        }
    }
}
