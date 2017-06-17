using System;
using System.IO;
using System.Linq;
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
