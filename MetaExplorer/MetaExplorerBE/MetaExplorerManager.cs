using MetaExplorerBE;
using MetaExplorerBE.MetaModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MetaExplorerBE
{
    public class MetaExplorerManager
    {
        #region dependency objects

        public VideoMetaModelCache VideoMetaModelCache
        {
            get;
            private set;
        }

        public CriterionCache CriterionCache
        {
            get;
            private set;
        }

        //new!!
        public ICache VideoFileCache
        {
            get;
            private set;
        }

        #endregion

        public MetaExplorerManager(VideoFileCache videoFileCache, VideoMetaModelCache videoMetaModelcache, CriterionCache criterionCache)
        {
            this.VideoMetaModelCache = videoMetaModelcache;
            this.CriterionCache = criterionCache;

            //new!!
            this.VideoFileCache = videoFileCache;
        }

        //#endregion
    }
}
