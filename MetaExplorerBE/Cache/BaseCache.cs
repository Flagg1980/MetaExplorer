using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetaExplorerBE
{
    public abstract class BaseCache<T> : ICache
    {
        public List<T> CachedItems
        {
            get;
            protected set;
        }

        protected BaseCache()
        {
            this.CachedItems = new List<T>();
        }

        public abstract Task InitCacheAsync(IProgress<int> progress, IProgress<string> progressFile);
    }

    public abstract class BaseCache<T,U> : ICache
    {
        public Dictionary<T,U> CachedItems
        {
            get;
            protected set;
        }

        protected BaseCache()
        {
            this.CachedItems = new Dictionary<T,U>();
        }

        public abstract Task InitCacheAsync(IProgress<int> progress, IProgress<string> progressFile);
    }
}
