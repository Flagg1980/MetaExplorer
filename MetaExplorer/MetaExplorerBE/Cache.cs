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
    /// Types of cached objects:
    /// - Video File Cache:         A List of all video files in the video file directory
    /// - Video MetaModel Cache:    A List of all video meta model files
    /// - Thumbnail File Cache:     A list of all Thumbnails in the .thumbnail directory
    /// </summary>
    public class Cache : ICache
    {
        #region Private Members

        private string _locationFFmpeg;

        private List<String> supportedImageFormats = new List<string> { ".jpg", ".bmp", ".png" };

        public List<VideoMetaModel> videoMetaModelCache = new List<VideoMetaModel>();

        private Dictionary<string, List<CriterionInstance>> criterionInstances = new Dictionary<string, List<CriterionInstance>>();

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

        public int Progress
        {
            get;
            private set;
        }

        public string ProgressFile
        {
            get;
            private set;
        }

        public string[] VideoFileCache
        {
            get { return this.videoFileCache; }
        }

        public List<VideoMetaModel> VideoMetaModelCache
        {
            get { return this.videoMetaModelCache;  }
        }

        public List<CriterionInstance> GetCriterionInstances(Criterion criterion)
        {
            return this.criterionInstances[criterion.Name];
        }

        public List<CriterionInstance> GetCriterionInstances(string criterionName)
        {
            return this.criterionInstances[criterionName];
        }

        public int VideoThumbnailHeight { get; private set; }

        public int VideoThumbnailWidth { get; private set; }

        public int CriterionThumbnailHeight { get; private set; }

        public int CriterionThumbnailWidth { get; private set; }

        #region Constructor

        /// <summary>
        /// </summary>
        public Cache(string baseDir, string ffmpegLocation, int videoThumbnailHeight, int videoThumbnailWidth, int criterionThumbnailHeight, int criterionThumbnailWidth)
        {
            this.LocationVideoFiles = baseDir;
            this._locationFFmpeg = ffmpegLocation;

            this.VideoThumbnailHeight = videoThumbnailHeight;
            this.VideoThumbnailWidth = videoThumbnailWidth;
            this.CriterionThumbnailHeight = criterionThumbnailHeight;
            this.CriterionThumbnailWidth = criterionThumbnailWidth;

            //init list
            CriteriaConfig.Load();
            CriteriaConfig.Criteria.ForEach((Criterion x) => this.criterionInstances.Add(x.Name, new List<CriterionInstance>()));
        }

        #endregion

        #region Public Methods

        public async Task UpdateVideoMetaModelCacheAsync()
        {
            this.videoMetaModelCache.Clear();
            IConverter mmConverter = new FileNameConverter();

            this.Progress = 0;
            int i = 0;
            foreach (string file in videoFileCache)
            {
                this.Progress = (i * 99) / videoFileCache.Length;
                this.ProgressFile = file;
                
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

                        BitmapSource bi = CreateReducedThumbnailImage(uri, this.VideoThumbnailHeight, this.VideoThumbnailWidth);

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

            this.Progress = 100;
        }



        public async Task UpdateNonExistingThumbnailCacheAsync()
        {
            this.Progress = 0;
            this.ProgressFile = "";

            //get thumbnail in cache
            List<VideoMetaModel> noThumbnail = this.VideoMetaModelCache.FindAll(x => { return x.Thumbnail == null; });

            //update progress
            int idx = 0;
            foreach (VideoMetaModel videoFile in noThumbnail)
            {
                idx++;
                this.ProgressFile = videoFile.FileName;
                this.Progress = (idx * 100) / noThumbnail.Count;

                this.UpdateThumbnailCache(videoFile);

                await Task.Delay(1);
            }

            this.Progress = 100;
            this.ProgressFile = "";
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
            BitmapSource bi = CreateReducedThumbnailImage(new Uri(cacheFile, UriKind.Absolute), this.VideoThumbnailHeight, this.VideoThumbnailWidth);

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

        public async Task GenerateDictAsync(Criterion criterion)
        {
            this.Progress = 0;
            this.ProgressFile = "";

            List<CriterionInstance> currentCriterionMetaModelList = this.criterionInstances[criterion.Name];
            currentCriterionMetaModelList.Clear();

            //load all criterion instances from FS
            List<string> validCriterionInstancesOnFS = new List<string>();
            string expectedPath = Path.Combine(Directory.GetCurrentDirectory(), criterion.Name);
            if (Directory.Exists(expectedPath))
            {
                validCriterionInstancesOnFS = Directory.GetFiles(expectedPath).ToList();
                //throw away all files which do not have a proper extension
                validCriterionInstancesOnFS.RemoveAll(x =>
                {
                    string extension = Path.GetExtension(x);
                    return !this.supportedImageFormats.Contains(extension, StringComparer.CurrentCultureIgnoreCase);
                });

                //create criterion instances
                foreach (string vci in validCriterionInstancesOnFS)
                {
                    CriterionInstance cmm = new CriterionInstance();
                    cmm.Name = Path.GetFileNameWithoutExtension(vci);
                    cmm.Count = 0;
                    cmm.SumStars = 0;
                    //cmm.ImageSource = new BitmapImage(new Uri(vci));
                    cmm.ImageSource = CreateReducedThumbnailImage(new Uri(vci), this.CriterionThumbnailHeight, this.CriterionThumbnailWidth);
                    currentCriterionMetaModelList.Add(cmm);
                }

            }
            else
            { 
                //excpetion handling not possible for some reason => gui hangs if exception is thrown here
            }
            this.Progress = 0;

            //add statistic values to criterion instances
            int i = 0;
            foreach (VideoMetaModel mm in this.videoMetaModelCache)
            {
                i++;
                this.Progress = (i * 99) / this.videoMetaModelCache.Count;
                this.ProgressFile = mm.FileName;

                foreach (string critElem in mm.GetList(criterion))
                {
                    CriterionInstance existing = currentCriterionMetaModelList.Find((CriterionInstance c) => { return c.Name.Equals(critElem, StringComparison.CurrentCultureIgnoreCase); });
                    if (existing != null)
                    {
                        existing.Count++;
                        existing.SumStars += mm.Stars;
                    }
                    //find the video files which do not have a criterion instance
                    else
                    {
                        CriterionInstance unknownInstance = new CriterionInstance();
                        unknownInstance.Name = critElem;
                        unknownInstance.SumStars += mm.Stars;
                        unknownInstance.Count++;
                        unknownInstance.ImageSource = Helper.NAimage; //new BitmapImage(new Uri(Path.Combine(Directory.GetCurrentDirectory(), @"na.png"))); 
                        currentCriterionMetaModelList.Add(unknownInstance);
                    }
                }

                //speed it up!! Task.Delay() slows up everything!
                if (i % 20 == 0)
                {
                    await Task.Delay(1);
                }
            }

            //sort by name only
            List<CriterionInstance> dummyCriterionMetaModelList = currentCriterionMetaModelList.OrderBy(x => { return x.ImageSource == null; }).ThenBy(x => x.Name).ToList();
            currentCriterionMetaModelList.Clear();
            currentCriterionMetaModelList.AddRange(dummyCriterionMetaModelList);

            //foreach orphan criterion instance, mark with a red cross
            currentCriterionMetaModelList.ForEach(x =>
            {
                if (x.Count == 0)
                {
                    x.ImageSource = Helper.CrossBitmapImage(x.ImageSource);
                    x.ImageSource.Freeze();
                }
            });

            this.Progress = 100;
            this.ProgressFile = "";
        }

        /// <summary>
        /// Reads all files from a given base directory and caches the file locations.
        /// </summary>
        public async Task UpdateVideoFileCacheAsync()
        {
            this.Progress = 0;

            string baseDir = this.LocationVideoFiles;

            if (!Directory.Exists(baseDir))
            {
                throw new Exception(String.Format("Basedir <{0}> does not exist.", baseDir));
            }

            string[] files = Directory.GetFiles(baseDir, "*", SearchOption.AllDirectories);
            await Task.Delay(1);
            this.videoFileCache = files;

            this.Progress = 100;
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
