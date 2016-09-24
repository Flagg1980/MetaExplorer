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
    /// This file caches criterion instances and their thumbnails.
    /// </summary>
    public class CriterionCache : ICriterionCache
    {
        #region Private Members

        private List<String> supportedImageFormats = new List<string> { ".jpg", ".bmp", ".png" };

        private Dictionary<string, List<CriterionInstance>> criterionInstances = new Dictionary<string, List<CriterionInstance>>();

        #endregion

        public List<CriterionInstance> GetCriterionInstances(Criterion criterion)
        {
            return this.criterionInstances[criterion.Name];
        }

        public List<CriterionInstance> GetCriterionInstances(string criterionName)
        {
            return this.criterionInstances[criterionName];
        }

        public int ThumbnailHeight { get; private set; }

        public int ThumbnailWidth { get; private set; }

        #region Constructor

        /// <summary>
        /// </summary>
        public CriterionCache(int thumbnailHeight, int thumbnailWidth)
        {
            this.ThumbnailHeight = thumbnailHeight;
            this.ThumbnailWidth = thumbnailWidth;

            //init list
            CriteriaConfig.Load();
            CriteriaConfig.Criteria.ForEach((Criterion x) => this.criterionInstances.Add(x.Name, new List<CriterionInstance>()));
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="criterion"></param>
        /// <param name="videoMetaModels">
        /// Read only. Used to identify Criterion Instances which are not available as Thumbnails, but only in the Video file names
        /// (They appear as red cross later in the application).
        /// </param>
        /// <returns></returns>
        public async Task GenerateDictAsync(Criterion criterion, List<VideoMetaModel> videoMetaModels, IProgress<int> progress, IProgress<string> progressFile)
        {
            progress.Report(0);
            progressFile.Report("");

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
                    cmm.ImageSource = CreateReducedThumbnailImage(new Uri(vci), this.ThumbnailHeight, this.ThumbnailWidth);
                    currentCriterionMetaModelList.Add(cmm);
                }
            }
            else
            {
                //excpetion handling not possible for some reason => gui hangs if exception is thrown here
                string errorMsg = "Directory <" + expectedPath + " does not exist.";
                throw new Exception(errorMsg);
            }
            progress.Report(0);

            //add statistic values to criterion instances
            int i = 0;
            foreach (VideoMetaModel mm in videoMetaModels)
            {
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

                i++;
                progress.Report((i * 99) / videoMetaModels.Count);
                progressFile.Report(mm.FileName);

                await Task.Delay(TimeSpan.FromTicks(10));
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

            progress.Report(100);
            progressFile.Report("");
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
