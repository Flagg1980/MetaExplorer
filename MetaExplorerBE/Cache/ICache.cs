using System;
using System.Threading.Tasks;

namespace MetaExplorerBE
{
    public interface ICache
    {
        Task InitCacheAsync(IProgress<int> progress, IProgress<string> progressFile);
    }
}
