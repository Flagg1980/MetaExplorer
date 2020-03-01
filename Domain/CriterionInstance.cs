using System;

namespace MetaExplorer.Domain
{
    public class CriterionInstance
    {
        //public Criterion Criterion { get; set; }

        public Thumbnail Thumbnail { get; set; }

        public string Name { get; set; }

        public int Count { get; set; }

        public int SumStars { get; set; }

        public Criterion Criterion { get; set; }

        public string Stats
        {
            get
            {
                return String.Format("Files: {0}  Average Rating: {1}", Count, String.Format("{0:0.00}", (double)SumStars / (double)Count));
            }
        }

        public CriterionInstance()
        {
            this.Thumbnail = new Thumbnail();
        }
    }
}
