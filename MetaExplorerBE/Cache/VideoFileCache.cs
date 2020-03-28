using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetaExplorerBE
{
    /// <summary>
    /// Reads all files from a given base directory and caches the file locations.
    /// </summary>
    public class VideoFileCache : BaseCache<string>
    {
        private string myPathToVideoFiles;

        public VideoFileCache(string pathToVideoFiles)
        {
            myPathToVideoFiles = pathToVideoFiles;
        }

        public override Task InitCacheAsync(IProgress<int> progress, IProgress<string> progressFile)
        {
            return Task.Factory.StartNew(() =>
            {
                progress.Report(0);

                if (!Directory.Exists(myPathToVideoFiles))
                {
                    throw new Exception(String.Format("Basedir <{0}> does not exist.", myPathToVideoFiles));
                }

                string[] files = Directory.GetFiles(myPathToVideoFiles, "*", SearchOption.AllDirectories);
                this.CachedItems = files.ToList();

                progress.Report(100);
            });
        }
    }
}
