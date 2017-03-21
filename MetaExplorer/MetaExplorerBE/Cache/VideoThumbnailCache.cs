using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace MetaExplorerBE
{
    public class VideoThumbnailCache : BaseThumbnailCache
    {
        private string myThumbnailPath;
        private string myFFmpegLocation;

        #region C'tor

        public VideoThumbnailCache(string thumbnailPath, int thumbnailHeight, int thumbnailWidth, string locationFFmpeg)
            :base(thumbnailHeight, thumbnailWidth)
        {
            myThumbnailPath = thumbnailPath;
            myFFmpegLocation = locationFFmpeg;
        }

        #endregion

        public override Task InitCacheAsync(IProgress<int> progress, IProgress<string> progressFile)
        {
            return Task.Factory.StartNew(() =>
            {
                progress.Report(0);
                progressFile.Report("");

                //check if directory exists
                if (!Directory.Exists(myThumbnailPath))
                {
                    string errorMsg = String.Format("Directory does not exist: <{0}>.", myThumbnailPath);
                    Console.WriteLine(errorMsg);
                }

                //update progress
                int idx = 0;
                progressFile.Report(myThumbnailPath);

                //read all files and throw away all files which do not have a proper extension
                var validThumbnails = Directory.GetFiles(myThumbnailPath).ToList();

                //create thumbnails
                foreach (string thumbnail in validThumbnails)
                {
                    BitmapSource bs = this.CreateReducedThumbnailImage(new Uri(thumbnail), this.myThumbnailHeight, this.myThumbnailWidth);
                    bs.Freeze();
                    this.CachedItems.Add(new FileInfo(thumbnail), bs);

                    progress.Report((idx * 100) / validThumbnails.Count);
                }

                progress.Report(100);
                progressFile.Report("");
            });
        }

        /// <summary>
        /// Updates a thumbnail cache entry for a given video file. If the cache entry already exists, it is overwritten.
        /// </summary>
        public BitmapSource UpdateThumbnailCache(string fileName)
        {
            FFmpegWrapper wrapper = new FFmpegWrapper(myFFmpegLocation);

            string cacheFile = Path.Combine(myThumbnailPath, fileName);

            if (File.Exists(cacheFile))
            {
                File.Delete(cacheFile);
            }

            //getDuration
            TimeSpan duration = wrapper.GetDuration(fileName);
            TimeSpan position = TimeSpan.FromSeconds(120);  //todo: remove hardcoded time

            if (duration.TotalSeconds <= position.TotalSeconds)
            {
                position = TimeSpan.FromTicks(duration.Ticks / 2);
            }

            //generate Thumbnail from movie 
            wrapper.CreateJpegThumbnail(fileName, position, cacheFile, false);

            BitmapSource bs = this.CreateReducedThumbnailImage(new Uri(cacheFile), this.myThumbnailHeight, this.myThumbnailWidth);
            bs.Freeze();
            this.CachedItems.Add(new FileInfo(cacheFile), bs);

            //return the created bitmap
            return this.GetByFilename(fileName);
        }
    }
}
