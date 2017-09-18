using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace MetaExplorerBE
{
    public class ImageThumbnailCache : BaseThumbnailCache
    {
        private List<string> myThumbnailPaths;

        private List<String> mySupportedImageFormats = new List<string> { ".jpg", ".bmp", ".png" };

        #region C'tor

        public ImageThumbnailCache(List<string> thumbnailPaths, int thumbnailHeight, int thumbnailWidth)
            :base(thumbnailHeight, thumbnailWidth)
        {
            myThumbnailPaths = thumbnailPaths;
        }

        #endregion

        public override Task InitCacheAsync(IProgress<int> progress, IProgress<string> progressFile)
        {
            return Task.Factory.StartNew(() =>
            {
                progress.Report(0);
                progressFile.Report("");

                foreach (string myThumbnailPath in this.myThumbnailPaths)
                {

                    //check if directory exists
                    if (!Directory.Exists(myThumbnailPath))
                    {
                        //TODO: better logging, make user aware!
                        string errorMsg = String.Format("Directory does not exist: <{0}>.", myThumbnailPath);
                        Console.WriteLine(errorMsg);
                        continue;
                    }

                    //update progress
                    int idx = 0;
                    progressFile.Report(myThumbnailPath);

                    //read all files and throw away all files which do not have a proper extension
                    var validThumbnails = Directory.GetFiles(myThumbnailPath).ToList();
                    validThumbnails.RemoveAll(x =>
                    {
                        string extension = Path.GetExtension(x);
                        return !this.mySupportedImageFormats.Contains(extension, StringComparer.CurrentCultureIgnoreCase);
                    });

                    //create thumbnails
                    foreach (string thumbnail in validThumbnails)
                    {
                        BitmapSource bs = this.CreateReducedThumbnailImage(new Uri(thumbnail), this.myThumbnailHeight, this.myThumbnailWidth);
                        bs.Freeze();
                        this.CachedItems.Add(new FileInfo(thumbnail), bs);

                        progress.Report((idx * 100) / validThumbnails.Count);
                    }
                }

                progress.Report(100);
                progressFile.Report("");
            });
        }
    }
}
