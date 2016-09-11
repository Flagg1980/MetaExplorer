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

        public IVideoMetaModelCache VideoMetaModelCache
        {
            get;
            private set;
        }

        public ICriterionCache CriterionCache
        {
            get;
            private set;
        }

        #endregion

        public MetaExplorerManager(IVideoMetaModelCache videoMetaModelcache, ICriterionCache criterionCache)
        {
            this.VideoMetaModelCache = videoMetaModelcache;
            this.CriterionCache = criterionCache;
        }

        //#endregion
    }
}
