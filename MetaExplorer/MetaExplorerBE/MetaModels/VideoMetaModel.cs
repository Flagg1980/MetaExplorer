using MetaExplorerBE.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MetaExplorerBE.MetaModels
{
    public class VideoMetaModel : INotifyPropertyChanged 
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string fileName = null;
        
        //criteriaContents["Actors"]=("a", "n", "o")
        public Dictionary<string, List<string>> criteriaContents = new Dictionary<string, List<string>>();
        
        public int star = 0;
        private BitmapSource _thumbnail = null;
        private string _thumbnailCaption = null;
        private DateTime _dateModified;

        public BitmapSource Thumbnail
        {
            get { return _thumbnail; }
            set { _thumbnail = value;  OnPropertyChanged("Thumbnail"); }
        }

        public string ThumbnailCaption
        {
            get { return _thumbnailCaption; }
            set { _thumbnailCaption = value; }
        }

        public DateTime DateModified
        { 
            get { return _dateModified; }
            set { _dateModified = value; }
        }

        public long FileSize { get; set; }

        public VideoMetaModel()
        {
            CriteriaConfig.Criteria.ForEach((Criterion crit) => this.criteriaContents[crit.Name] = new List<string>());
        }

        public List<string> GetList(Criterion criterion)
        {
            return this.criteriaContents[criterion.Name];
        }

        private void OnPropertyChanged(string value)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(value));
            }
        }
    }
}
