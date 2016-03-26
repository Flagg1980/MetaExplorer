using MetaExplorer.MetaModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MetaExplorer
{
    public class MetaExplorerManager
    {
        #region dependency objects


        public ICache Cache
        {
            get;
            private set;
        }

        #endregion

        public MetaExplorerManager(ICache cache)
        {
            this.Cache = cache;
        }

        //#endregion
    }
}
