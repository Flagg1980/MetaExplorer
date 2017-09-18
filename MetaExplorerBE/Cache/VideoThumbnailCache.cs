using MetaExplorer.Common;
using System;
using System.IO;
using System.Linq;
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
                if (!Directory.Exists(myThumbnailPath))
                    Directory.CreateDirectory(myThumbnailPath);

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
        public BitmapSource UpdateThumbnailCache(FileInfo file)
        {
            FFmpegWrapper wrapper = new FFmpegWrapper(myFFmpegLocation);

            string md5 = Helper.GetMD5Hash(file.Name);

            string cacheFileLocation = Path.Combine(myThumbnailPath, md5);

            if (File.Exists(cacheFileLocation))
            {
                File.Delete(cacheFileLocation);
            }

            //getDuration
            TimeSpan duration = wrapper.GetDuration(file.FullName);
            TimeSpan position = TimeSpan.FromSeconds(120);  //todo: remove hardcoded time

            if (duration.TotalSeconds <= position.TotalSeconds)
            {
                position = TimeSpan.FromTicks(duration.Ticks / 2);
            }

            //generate Thumbnail from movie 
            wrapper.CreateJpegThumbnail(file.FullName, position, cacheFileLocation, false);

            BitmapSource bs = this.CreateReducedThumbnailImage(new Uri(cacheFileLocation), this.myThumbnailHeight, this.myThumbnailWidth);
            bs.Freeze();
            this.CachedItems.Add(new FileInfo(cacheFileLocation), bs);

            //return the created bitmap
            return bs;
        }
    }
}
