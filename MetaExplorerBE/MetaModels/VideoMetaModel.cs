using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MetaExplorerBE.MetaModels
{
    /// <summary>
    /// Represents a video which was loaded from file system.
    /// </summary>
    //public class VideoMetaModel : INotifyPropertyChanged 
    //{
    //    #region Private Members

    //    private BitmapSource _thumbnail = null;

    //    #endregion

    //    #region Public Members

    //    public event PropertyChangedEventHandler PropertyChanged;

    //    //criteriaContents["Actors"]=("a", "n", "o")
    //    public Dictionary<string, List<string>> criteriaContents = new Dictionary<string, List<string>>();

    //    #endregion

    //    #region Public Properties

    //    public BitmapSource Thumbnail
    //    {
    //        get { return _thumbnail; }
    //        set { _thumbnail = value;  OnPropertyChanged("Thumbnail"); }
    //    }

    //    public string ThumbnailCaption1 { get; set; }

    //    public string ThumbnailCaption2 { get; set; }

    //    public DateTime DateModified { get; set; }

    //    public long FileSize { get; set; }

    //    public string FileName { get; set; }

    //    public int Stars { get; set; }

    //    public int BitRate { get; set; }

    //    public int FrameWidth { get; set; }

    //    public int FrameHeight { get; set; }

    //    #endregion

    //    #region C'tor

    //    public VideoMetaModel()
    //    {
    //        CriteriaConfig.Criteria.ForEach((Criterion crit) => this.criteriaContents[crit.Name] = new List<string>());
    //    }

    //    #endregion

    //    #region Public Methods

    //    public List<string> GetList(Criterion criterion)
    //    {
    //        return this.criteriaContents[criterion.Name];
    //    }

    //    #endregion

    //    #region Private Methods

    //    private void OnPropertyChanged(string value)
    //    {
    //        PropertyChangedEventHandler handler = PropertyChanged;
    //        if (handler != null)
    //        {
    //            handler(this, new PropertyChangedEventArgs(value));
    //        }
    //    }

    //    #endregion
    //}
}
