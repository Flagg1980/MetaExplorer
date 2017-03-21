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
        private string myFileLocation;

        public VideoFileCache(string fileLocation)
        {
            myFileLocation = fileLocation;
        }

        public override Task InitCacheAsync(IProgress<int> progress, IProgress<string> progressFile)
        {
            return Task.Factory.StartNew(() =>
            {
                progress.Report(0);

                //string baseDir = this.LocationVideoFiles;

                if (!Directory.Exists(myFileLocation))
                {
                    throw new Exception(String.Format("Basedir <{0}> does not exist.", myFileLocation));
                }

                string[] files = Directory.GetFiles(myFileLocation, "*", SearchOption.AllDirectories);
                this.CachedItems = files.ToList();

                progress.Report(100);
            });
        }
    }
}
