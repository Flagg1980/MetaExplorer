using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MetaExplorer.Domain;
using MetaExplorer.Common;
using System.Collections.ObjectModel;

namespace MetaExplorerBE
{
    /// <summary>
    /// This file caches criterion instances and their thumbnails.
    /// </summary>
    public class CriterionCache : BaseCache<string, ObservableCollection<CriterionInstance>>
    {
        public ObservableCollection<CriterionInstance> GetCriterionInstances(Criterion criterion)
        {
            return this.CachedItems[criterion.Name];
        }

        public ObservableCollection<CriterionInstance> GetCriterionInstances(string criterionName)
        {
            return this.CachedItems[criterionName];
        }

        private readonly ImageThumbnailCache myCriterionThumbnailCache;
        private readonly VideoMetaModelCache myVideoMetaModelCache;

        #region Constructor

        /// <summary>
        /// </summary>
        public CriterionCache(ImageThumbnailCache criterionThumbnailCache, VideoMetaModelCache videoMetaModelCache)
        {
            myCriterionThumbnailCache = criterionThumbnailCache;
            myVideoMetaModelCache = videoMetaModelCache;

            //init list
            CriteriaConfig.Criteria.ForEach((Criterion x) => this.CachedItems.Add(x.Name, new ObservableCollection<CriterionInstance>()));
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
        public override Task InitCacheAsync(IProgress<int> progress, IProgress<string> progressFile)
        {
            return Task.Factory.StartNew(() =>
            {
                foreach (Criterion crit in CriteriaConfig.Criteria)
                {
                    InitCacheCrit(crit, progress, progressFile);
                }
            });
        }

        #endregion

        private void InitCacheCrit(Criterion criterion, IProgress<int> progress, IProgress<string> progressFile)
        {
            progress.Report(0);
            progressFile.Report("Updating Criterion Cache <" + criterion.Name + ">");

            ObservableCollection<CriterionInstance> currentCriterionMetaModelList = this.CachedItems[criterion.Name];
            if (currentCriterionMetaModelList == null)
                currentCriterionMetaModelList = new ObservableCollection<CriterionInstance>();

            currentCriterionMetaModelList.Clear();

            // create criterion instances for all thumbnails found for this criterion
            var allRelevantCriterionInstances = this.myCriterionThumbnailCache.CachedItems
                .Select(x => x)
                .Where(x => string.Equals(x.Key.Directory.Name, criterion.Name, StringComparison.OrdinalIgnoreCase))
                .ToList();

            allRelevantCriterionInstances.ForEach(x =>
            {
                CriterionInstance cmm = new CriterionInstance();
                cmm.Name = Path.GetFileNameWithoutExtension(x.Key.FullName);
                cmm.Criterion = criterion;
                cmm.Count = 0;
                cmm.SumStars = 0;
                cmm.Thumbnail.Image = myCriterionThumbnailCache.GetByFilename(Path.GetFileName(x.Key.Name));
                cmm.Thumbnail.Image.Freeze();
                currentCriterionMetaModelList.Add(cmm);
            });

            // create criterion instances for all criteria coming from the video meta models but do not have a file system (thumbnail) representative
            int i = 0;
            //from all video meta model cached items
            this.myVideoMetaModelCache.CachedItems.ForEach(vmm =>
            {
                //look at all criterion instances for this criterion
                vmm.GetList(criterion).ForEach(crit =>
                {
                    //is this already in the criterion meta model list?
                    CriterionInstance existing = currentCriterionMetaModelList.FirstOrDefault(ci =>
                    {
                        return string.Equals(crit, ci.Name, StringComparison.OrdinalIgnoreCase);
                    });

                    if (existing == null)
                    {
                        existing = new CriterionInstance();
                        existing.Name = crit;
                        existing.Criterion = criterion;
                        existing.Thumbnail.Image = Helper.NAimage;
                        existing.Thumbnail.Image.Freeze();
                        currentCriterionMetaModelList.Add(existing);
                    }

                    //add statistic information
                    existing.SumStars += vmm.Stars;
                    existing.Count++;

                    //track progress
                    i++;
                    progress.Report((i * 99) / this.myVideoMetaModelCache.CachedItems.Count);
                    progressFile.Report(vmm.File.FullName);
                });
            });

            //sort by name only
            List<CriterionInstance> dummyCriterionMetaModelList = 
                currentCriterionMetaModelList.OrderBy(x => 
                {
                    return x.Thumbnail.Image == null;
                }).
                ThenBy(x => x.Name).ToList();

            currentCriterionMetaModelList = new ObservableCollection<CriterionInstance>(dummyCriterionMetaModelList);

            //foreach orphan criterion instance, mark with a red cross
            foreach (var x in currentCriterionMetaModelList)
            {
                if (x.Count == 0)
                {
                    x.Thumbnail.Image = Helper.CrossBitmapImage(x.Thumbnail.Image);
                    x.Thumbnail.Image.Freeze();
                }
            }

            progress.Report(100);
            progressFile.Report("");
        }
    }
}
