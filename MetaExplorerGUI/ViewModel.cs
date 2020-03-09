using MetaExplorer.Common;
using MetaExplorer.Domain;
using MetaExplorerBE;
using System;
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
        
        private Dictionary<string, int> currentCriterionSelectionIndex = new Dictionary<string, int>();
        
        private string _freeTextSearch = string.Empty;

        public VideoFileCache VideoFileCache { get; set; }
        public VideoMetaModelCache VideoMetaModelCache { get; set; }
        public CriterionCache CriterionCache { get; set; }

        private ObservableCollection<Video> currentFileSelection;

        private Video mmRef = null;

        #endregion

        #region Public Properties

        public Dictionary<string, int> CurrentCriterionSelectionIndex
        {
            get { return this.currentCriterionSelectionIndex; }
        }

        public string FreeTextSearch
        {
            get { return this._freeTextSearch; }
            set { this._freeTextSearch = value; OnPropertyChanged("FreeTextSearch");  }
        }

        public Video MMRef
        {
            get { return this.mmRef; }
            private set { this.mmRef = value; }
        }

        public ObservableCollection<Video> CurrentFileSelection
        {
            get { return this.currentFileSelection; }
        }

        //public MetaExplorerManager MEManager
        //{
        //    get { return this.me; }
        //    set { this.me = value;  }
        //}

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

        public ViewModel()
        {
            currentFileSelection = new ObservableCollection<Video>();

            CriteriaConfig.Criteria.ForEach((Criterion x) => currentCriterionSelectionIndex[x.Name] = -1);
        }

        #endregion

        #region Public Methods

        //public void Resort()
        //{
        //    //sort by property
        //    //this.MEManager.VideoMetaModelCache = this.videoMetaModelCache.OrderByDescending(x => x.DateModified).ToList();
        //    this.MEManager.VideoMetaModelCache.ResortBy(x => x.DateModified);
        //}

        public void UpdateCurrentSelection()
        {
            currentFileSelection.Clear();
            //currentFileSelection.AddRange(this.VideoMetaModelCache.GetVideoFileSelection(mmRef));
            currentFileSelection = this.VideoMetaModelCache.GetVideoFileSelection(mmRef);

            this.CurrentFileSelectionCount = this.currentFileSelection.Count();

            this.CurrentFileSelectionSize = 0;
            foreach (Video vmm in this.currentFileSelection) { this.CurrentFileSelectionSize += vmm.File.Length; }
        }

        /// <summary>
        /// Adds a specific item from the VideoMetaModelCache to the current selection.
        /// </summary>
        /// <param name="index">The index of the VideoMetaModelCache to be added.</param>
        public void AddToCurrentSelection(int index)
        {
            currentFileSelection.Add(this.VideoMetaModelCache.CachedItems[index]);
            this.CurrentFileSelectionCount++;
            this.CurrentFileSelectionSize += this.VideoMetaModelCache.CachedItems[index].File.Length;
        }

        public void UpdateMMref()
        { 
            this.MMRef = new Video();

            MMRef.ThumbnailCaption1 = this.FreeTextSearch;

            CriteriaConfig.Criteria.ForEach((Criterion x) =>
            {
                this.MMRef.criteriaContents[x.Name] = new List<string>();

                if (this.currentCriterionSelectionIndex[x.Name] >= 0)
                {
                    int critInstanceIndex = this.currentCriterionSelectionIndex[x.Name];
                    CriterionInstance critInstance = this.CriterionCache.GetCriterionInstances(x)[critInstanceIndex];
                    this.MMRef.criteriaContents[x.Name].Add(critInstance.Name);
                }
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;  
        public void OnPropertyChanged(string Property = "")  
        {  
            if (PropertyChanged != null)  
            {  
                PropertyChanged(this, new PropertyChangedEventArgs(Property));  
            }  
        }

        #endregion
    }
}
