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
        public List<CriterionInstance> criteriaMapping = new List<CriterionInstance>();

        public Thumbnail Thumbnail { get; set; }

        public string ThumbnailCaption1 { get; set; }

        public string ThumbnailCaption2 { get; set; }

        public FileInfo File { get; set; }

        public int Stars { get; set; }

        public VideoProperties Properties { get; set; }

        public Video()
        {
            this.Thumbnail = new Thumbnail();
        }
    }
}
