using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using MetaExplorerBE.MetaModels;
using MetaExplorerBE.Configuration;

namespace MetaExplorerBE
{
    /// <summary>
    /// 
    /// </summary>
    public class VideoMetaModelCache : IVideoMetaModelCache
    {
        #region Private Members

        private string _locationFFmpeg;

        private List<String> supportedImageFormats = new List<string> { ".jpg", ".bmp", ".png" };

        public List<VideoMetaModel> videoMetaModelCache = new List<VideoMetaModel>();

        private string[] videoFileCache;

        #endregion

        private string BaseDir { get; set; }

        private string LocationThumbnailFiles
        {
            get
            {
                string expectedLocation = Path.Combine(Directory.GetCurrentDirectory(), ".cache");
                if (!Directory.Exists(expectedLocation))
                    Directory.CreateDirectory(expectedLocation);

                return expectedLocation;
            }
        }

        private string LocationVideoFiles
        {
            get;
            set;
        }

        public string[] VideoFileCache
        {
            get { return this.videoFileCache; }
        }

        public List<VideoMetaModel> Cache
        {
            get { return this.videoMetaModelCache;  }
        }

        public int ThumbnailHeight { get; private set; }
        public int ThumbnailWidth { get; private set; }

        #region Constructor

        /// <summary>
        /// </summary>
        public VideoMetaModelCache(string baseDir, string ffmpegLocation, int thumbnailHeight, int thumbnailWidth)
        {
            this.LocationVideoFiles = baseDir;
            this._locationFFmpeg = ffmpegLocation;

            this.ThumbnailHeight = thumbnailHeight;
            this.ThumbnailWidth = thumbnailWidth;
        }

        #endregion

        #region Public Methods

        public async Task UpdateVideoMetaModelCacheAsync(IProgress<int> progress, IProgress<string> progressFile)
        {
            this.videoMetaModelCache.Clear();
            IConverter mmConverter = new FileNameConverter();

            progress.Report(0);
            int i = 0;
            foreach (string file in videoFileCache)
            {
                progress.Report((i * 99) / videoFileCache.Length);
                progressFile.Report(file);
                
                i++;
                VideoMetaModel mm = mmConverter.ConvertFrom(file);
                try
                {
                    //attach thumbnail
                    string md5 = Helper.GetMD5Hash(file);
                    string cacheFile = Path.Combine(LocationThumbnailFiles, md5);
                    if (File.Exists(cacheFile))
                    {
                        Uri uri = new Uri(cacheFile, UriKind.Absolute);

                        //BitmapSource bi = new BitmapImage(uri);

                        BitmapSource bi = CreateReducedThumbnailImage(uri, this.ThumbnailHeight, this.ThumbnailWidth);

                        bi.Freeze(); // Must be done if databinding is done to another thread than this method thread, otherwise error message: “Must create DependencySource on same Thread as the DependencyObject”
                        mm.Thumbnail = bi;
                    }
                    else
                    {
                        mm.Thumbnail = null;
                    }

                    //add thumbnail caption
                    mm.ThumbnailCaption = Path.GetFileName(file);
                    this.videoMetaModelCache.Add(mm);
                }
                catch (Exception e)
                {
                    throw new Exception(String.Format("Error while attaching thumbnail to video meta model for file <{0}>. Message: <{1}>", file, e.Message));
                }

                if (i % 20 == 0)
                {
                    await Task.Delay(1); //for async processing
                }
            }

            //sort by date
            this.videoMetaModelCache = this.videoMetaModelCache.OrderByDescending(x => x.DateModified).ToList();

            progress.Report(100);
        }



        public async Task UpdateNonExistingThumbnailCacheAsync(IProgress<int> progress, IProgress<string> progressFile)
        {
            progress.Report(0);
            progressFile.Report("");

            //get thumbnail in cache
            List<VideoMetaModel> noThumbnail = this.Cache.FindAll(x => { return x.Thumbnail == null; });

            //update progress
            int idx = 0;
            foreach (VideoMetaModel videoFile in noThumbnail)
            {
                idx++;
                progressFile.Report(videoFile.FileName);
                progress.Report((idx * 100) / noThumbnail.Count);

                this.UpdateThumbnailCache(videoFile);

                await Task.Delay(1);
            }

            progress.Report(100);
            progressFile.Report("");
        }

        /// <summary>
        /// Updates a thumbnail cache entry for a given video file. If the cache entry already exists, it is overwritten.
        /// </summary>
        public void UpdateThumbnailCache(VideoMetaModel videoFileMetaModel)
        {
            FFmpegWrapper wrapper = new FFmpegWrapper(_locationFFmpeg);

            string md5 = Helper.GetMD5Hash(videoFileMetaModel.FileName);
            string cacheFile = Path.Combine(LocationThumbnailFiles, md5);

            if (File.Exists(cacheFile))
            {
                File.Delete(cacheFile);
            }

            //getDuration
            TimeSpan duration = wrapper.GetDuration(videoFileMetaModel.FileName);
            TimeSpan position = TimeSpan.FromSeconds(120);  //todo: remove hardcoded time

            if (duration.TotalSeconds <= position.TotalSeconds)
            {
                position = TimeSpan.FromTicks(duration.Ticks / 2);
            }

            //generate Thumbnail from movie 
            wrapper.CreateJpegThumbnail(videoFileMetaModel.FileName, position, cacheFile, false);

            //update bitmap in VideoMetaModel
            //BitmapImage bi = new BitmapImage(new Uri(cacheFile, UriKind.Absolute));
            BitmapSource bi = CreateReducedThumbnailImage(new Uri(cacheFile, UriKind.Absolute), this.ThumbnailHeight, this.ThumbnailWidth);

            bi.Freeze();              // Must be done if databinding is done to another thread than this method thread, otherwise error message: “Must create DependencySource on same Thread as the DependencyObject”
            videoFileMetaModel.Thumbnail = bi;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="regex"></param>
        /// <returns></returns>
        public List<VideoMetaModel> GetThumbnailFileSelection(VideoMetaModel mmRef)
        {
            List<VideoMetaModel> videoFiles = this.GetVideoFileSelection(mmRef);
            return videoFiles;
        }

        /// <summary>
        /// Reads all files from a given base directory and caches the file locations.
        /// </summary>
        public async Task UpdateVideoFileCacheAsync(IProgress<int> progress, IProgress<string> progressFile)
        {
            progress.Report(0);

            string baseDir = this.LocationVideoFiles;

            if (!Directory.Exists(baseDir))
            {
                throw new Exception(String.Format("Basedir <{0}> does not exist.", baseDir));
            }

            string[] files = Directory.GetFiles(baseDir, "*", SearchOption.AllDirectories);
            await Task.Delay(1);
            this.videoFileCache = files;

            progress.Report(100);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="regex"></param>
        /// <returns></returns>
        public List<VideoMetaModel> GetVideoFileSelection(VideoMetaModel mmRef)
        {
            List<VideoMetaModel> res = new List<VideoMetaModel>(this.videoMetaModelCache);

            //check stars
            if (mmRef.Stars != 0)
            {
                res = res.Where((VideoMetaModel m) => { return m.Stars == mmRef.Stars; }).ToList();
            }

            foreach (Criterion criterion in CriteriaConfig.Criteria)
            {
                //return m.actors.Contains(mmRef.actors[0], StringComparer.CurrentCultureIgnoreCase); 
                List<string> critContent = mmRef.criteriaContents[criterion.Name];

                if (critContent.Count == 0)
                {
                    //do nothing, no contriction necessary
                }
                else if (critContent.Count > 1)
                {
                    throw new Exception("Criterion count > 1 not supported.");
                }
                else
                {
                    res = res.Where((VideoMetaModel m) =>
                                    {
                                        return m.criteriaContents[criterion.Name].Contains(mmRef.criteriaContents[criterion.Name][0], StringComparer.CurrentCultureIgnoreCase);
                                    }).ToList();
                }
            }

            //check freetext search (last because most expensive
            if (mmRef.FileName != null && mmRef.FileName != string.Empty)
            {
                res = res.Where((VideoMetaModel m) => { return Path.GetFileNameWithoutExtension(m.FileName).IndexOf(mmRef.FileName, StringComparison.CurrentCultureIgnoreCase) >= 0; }).ToList();
            }

            return res;
        }

        #endregion

        #region Private Methods

        private BitmapSource CreateReducedThumbnailImage(Uri uri, int desiredHeight, int desiredWidth)
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

        #endregion
    }
}
