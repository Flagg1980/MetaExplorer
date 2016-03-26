#region Imports

using MetaExplorerBE;
using MetaExplorerBE.Configuration;
using MetaExplorerBE.MetaModels;
using MetaExplorerGUI.Properties;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

#endregion

namespace MetaExplorerGUI
{
    public class ViewModel : INotifyPropertyChanged
    {
        #region Private Members

        private int currentFileSelectionCount = 0;
        private long currentFileSelectionSize = 0;
        
        private Dictionary<string, int> currentCriterionSelectionIndex = new Dictionary<string, int>();
        
        private string _freeTextSearch = string.Empty;

        private MetaExplorerManager me;

        private RangeObservableCollection<VideoMetaModel> currentFileSelection;

        private VideoMetaModel mmRef = null;

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

        public VideoMetaModel MMRef
        {
            get { return this.mmRef; }
            private set { this.mmRef = value; }
        }

        public RangeObservableCollection<VideoMetaModel> CurrentFileSelection
        {
            get { return this.currentFileSelection; }
        }

        public MetaExplorerManager MEManager
        {
            get { return this.me; }
            set { this.me = value;  }
        }

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
            currentFileSelection = new RangeObservableCollection<VideoMetaModel>();

            CriteriaConfig.Criteria.ForEach((Criterion x) => currentCriterionSelectionIndex[x.Name] = -1);
        }

        #endregion

        #region Public Methods

        public void UpdateCurrentSelection()
        {
            currentFileSelection.Clear();
            currentFileSelection.AddRange(me.Cache.GetThumbnailFileSelection(mmRef));
            
            this.CurrentFileSelectionCount = this.currentFileSelection.Count();

            this.CurrentFileSelectionSize = 0;
            foreach (VideoMetaModel vmm in this.currentFileSelection) { this.CurrentFileSelectionSize += vmm.FileSize; }
        }

        public void UpdateMMref()
        { 
            this.MMRef = new VideoMetaModel();

            MMRef.fileName = this.FreeTextSearch;

            CriteriaConfig.Criteria.ForEach((Criterion x) =>
            {
                this.MMRef.criteriaContents[x.Name].Clear();

                if (this.currentCriterionSelectionIndex[x.Name] >= 0)
                {
                    int critInstanceIndex = this.currentCriterionSelectionIndex[x.Name];
                    CriterionInstance critInstance = this.me.Cache.GetCriterionInstances(x)[critInstanceIndex];
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
