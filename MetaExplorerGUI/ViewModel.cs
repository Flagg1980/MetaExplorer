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
        
        private string _freeTextSearch = string.Empty;

        public VideoFileCache VideoFileCache { get; set; }
        public VideoMetaModelCache VideoMetaModelCache { get; set; }
        public CriterionCache CriterionCache { get; set; }

        private Video mmRef = null;

        #endregion

        #region Public Properties

        public Dictionary<Criterion, CriterionInstance> CurrentCriterionSelection { get; set; }

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

        public ObservableCollection<Video> CurrentFileSelection { get; private set; }

        //public ObservableCollection<CriterionInstance> CurrentCriterionInstanceList { get; private set; }

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

        public ViewModel(CriterionCache criterionCache, VideoFileCache videoFileCache, VideoMetaModelCache videoMetaModelCache)
        {
            CriterionCache = criterionCache;
            VideoFileCache = videoFileCache;
            VideoMetaModelCache = videoMetaModelCache;

            CurrentFileSelection = new ObservableCollection<Video>();

            CurrentCriterionSelection = new Dictionary<Criterion, CriterionInstance>();
            CriteriaConfig.Criteria.ForEach((Criterion crit) => 
            {
                CurrentCriterionSelection.Add(crit, null);
            });

            //CurrentCriterionInstanceList = this.CriterionCache.GetCriterionInstances("Actor");

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
            CurrentFileSelection.Clear();
            //currentFileSelection.AddRange(this.VideoMetaModelCache.GetVideoFileSelection(mmRef));
            CurrentFileSelection = this.VideoMetaModelCache.GetVideoFileSelection(mmRef);

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
            currentFileSelection.Add(this.VideoMetaModelCache.CachedItems[index]);
            this.CurrentFileSelectionCount++;
            this.CurrentFileSelectionSize += this.VideoMetaModelCache.CachedItems[index].File.Length;
        }

        public void UpdateMMref()
        { 
            this.MMRef = new Video();

            MMRef.ThumbnailCaption1 = this.FreeTextSearch;

            CriteriaConfig.Criteria.ForEach((Criterion crit) =>
            {
                this.MMRef.criteriaContents[crit.Name] = new List<string>();

                if (this.CurrentCriterionSelection[crit] != null)
                {
                    CriterionInstance critInstance = this.CurrentCriterionSelection[crit];
                    //CriterionInstance critInstance = this.CriterionCache.GetCriterionInstances(crit)[critInstanceIndex];
                    this.MMRef.criteriaContents[crit.Name].Add(critInstance.Name);
                }
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;  
        public void OnPropertyChanged(string Property = "")  
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(Property));
        }

        #endregion
    }
}
