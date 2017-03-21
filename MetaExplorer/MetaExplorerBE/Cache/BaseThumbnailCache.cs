using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace MetaExplorerBE
{
    public abstract class BaseThumbnailCache : BaseCache<FileInfo, BitmapSource>
    {
        protected readonly int myThumbnailHeight;
        protected readonly int myThumbnailWidth;

        #region C'tor

        public BaseThumbnailCache(int thumbnailHeight, int thumbnailWidth)
        {
            myThumbnailHeight = thumbnailHeight;
            myThumbnailWidth = thumbnailWidth;
        }

        #endregion

        //public override Task InitCacheAsync(IProgress<int> progress, IProgress<string> progressFile)
        //{
        //    return Task.Factory.StartNew(() =>
        //    {
        //        progress.Report(0);
        //        progressFile.Report("");

        //        //check if directory exists
        //        if (!Directory.Exists(myThumbnailPath))
        //        {
        //            string errorMsg = String.Format("Directory does not exist: <{0}>.", myThumbnailPath);
        //            Console.WriteLine(errorMsg);
        //        }

        //        //update progress
        //        int idx = 0;
        //        progressFile.Report(myThumbnailPath);

        //        //read all files and throw away all files which do not have a proper extension
        //        var validThumbnails = Directory.GetFiles(myThumbnailPath).ToList();
        //        //validThumbnails.RemoveAll(x =>
        //        //{
        //        //    string extension = Path.GetExtension(x);
        //        //    return !this.mySupportedImageFormats.Contains(extension, StringComparer.CurrentCultureIgnoreCase);
        //        //});

        //        //create thumbnails
        //        foreach (string thumbnail in validThumbnails)
        //        {
        //            BitmapSource bs = this.CreateReducedThumbnailImage(new Uri(thumbnail), this.myThumbnailHeight, this.myThumbnailWidth);
        //            bs.Freeze();
        //            this.CachedItems.Add(new FileInfo(thumbnail), bs);

        //            progress.Report((idx * 100) / validThumbnails.Count);
        //        }
        //        //}

        //        progress.Report(100);
        //        progressFile.Report("");
        //    });
        //}

        public BitmapSource GetByFilename(string filename)
        {
            var found = CachedItems
                .Select(x => x)
                .Where(x => x.Key.Name.Equals(filename));

            if (found.Count() == 0)
                return null;

            if (found.Count() > 1)
            {
                string errorMsg = String.Format("More than one cache entry found for item: <{0}>.", filename);
                throw new Exception(errorMsg);
            }

            return found.First().Value;
        }

        protected BitmapSource CreateReducedThumbnailImage(Uri uri, int desiredHeight, int desiredWidth)
        {
            // Define a BitmapImage.
            BitmapImage bi = new BitmapImage();

            // Begin initialization.
            bi.BeginInit();

            // Set properties.
            bi.CacheOption = BitmapCacheOption.OnDemand;
            bi.CreateOptions = BitmapCreateOptions.DelayCreation;
            bi.DecodePixelHeight = desiredHeight;
            bi.DecodePixelWidth = desiredWidth;
            bi.UriSource = uri;

            // End initialization.
            bi.EndInit();
            return bi;
        }
    }
}
