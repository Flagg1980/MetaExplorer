using Domain;
using MetaExplorer.Common;
using MetaExplorer.Common.VideoPropertiesProvider;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace MetaExplorerBE
{
    public class VideoPropertiesCache : BaseCache<string, VideoProperties>, IVideoPropertiesCache
    {
        private readonly IVideoFileCache myVideoFileCache;
        private IVideoPropertiesProvider myVideoPropertiesProvider;

        public VideoPropertiesCache(string cacheLocation, string ffMpegLocation, IVideoFileCache videoFileCache)
        {
            Location = cacheLocation;
            myVideoFileCache = videoFileCache;
            myVideoPropertiesProvider = new VideoPropertiesProvider(VideoPropertiesTechnology.MediaToolkit, ffMpegLocation).Provider;
        }

        public string Location { get; }

        public override Task InitCacheAsync(IProgress<int> progress, IProgress<string> progressFile)
        {
            return Task.Factory.StartNew(() =>
            {
                 InitCache( progress, progressFile);
            });
        }

        private void InitCache(IProgress<int> progress, IProgress<string> progressFile)
        {
            progress.Report(0);
            progressFile.Report("Updating Video Properties Cache");

            ReadLocalCache();
            FillupNonExistingEntries(progress);
            WriteLocalCache();

            progress.Report(100);
            progressFile.Report("");
        }

        private void WriteLocalCache()
        {
            var jsonString = JsonConvert.SerializeObject(CachedItems, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(Location, jsonString);
        }

        private void FillupNonExistingEntries(IProgress<int> progress)
        {
            for (int i = 0; i < myVideoFileCache.CachedItems.Count; i++)
            {
                var file = myVideoFileCache.CachedItems[i];
                FileInfo fi = new FileInfo(file);
                string md5 = Helper.GetMD5Hash(fi);

                if (!this.CachedItems.ContainsKey(md5))
                {
                    VideoProperties vp = myVideoPropertiesProvider.GetVideoProperties(fi);

                    CachedItems.Add(md5, vp);
                }

                progress.Report(i * 100 / myVideoFileCache.CachedItems.Count);
            }
        }

        private void ReadLocalCache()
        {
            if (File.Exists(Location))
            {
                var jsonContent = File.ReadAllText(Location);
                CachedItems = JsonConvert.DeserializeObject<Dictionary<String, VideoProperties>>(jsonContent);
            }
            else
            {
                Trace.TraceWarning($"Video Properties Cache File not found in expected location <{Location}>.");
            }
        }
    }
}
