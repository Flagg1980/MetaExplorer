using Domain;
using System;
using System.Collections.Generic;
using System.IO;

namespace MetaExplorer.Domain
{
    /// <summary>
    /// Represents a video which was loaded from file system.
    /// </summary>
    public class Video
    {
        #region Public Members

        //criteriaContents["Actors"]=("a", "n", "o")
        public Dictionary<string, List<string>> criteriaContents = new Dictionary<string, List<string>>();

        public Dictionary<Criterion, List<CriterionInstance>> criteriaMapping = new Dictionary<Criterion, List<CriterionInstance>>();

        #endregion

        #region Public Properties

        public Thumbnail Thumbnail { get; set; }

        //public DateTime DateModified { get; set; }

        public string ThumbnailCaption1 { get; set; }

        public string ThumbnailCaption2 { get; set; }

        //public long FileSize { get; set; }

        public FileInfo File { get; set; }

        public int Stars { get; set; }

        //public int BitRate { get; set; }

        //public int FrameWidth { get; set; }

        //public int FrameHeight { get; set; }

        public VideoProperties Properties { get; set; }

        #endregion

        #region C'tor

        public Video()
        {
            this.Thumbnail = new Thumbnail();
        }

        #endregion

        #region Public Methods

        public List<string> GetList(Criterion criterion)
        {
            if (!this.criteriaContents.ContainsKey(criterion.Name))
                this.criteriaContents[criterion.Name] = new List<string>();

            return this.criteriaContents[criterion.Name];
        }

        #endregion
    }
}
