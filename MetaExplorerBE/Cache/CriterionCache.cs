using MetaExplorer.Common;
using MetaExplorer.Domain;
using MetaExplorerBE.Converter;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MetaExplorerBE
{
    /// <summary>
    /// This file caches criterion instances and their thumbnails.
    /// </summary>
    public class CriterionCache : BaseCache<CriterionInstance>, ICriterionCache
    {
        private readonly ImageThumbnailCache myCriterionThumbnailCache;
        private readonly IVideoFileCache myVideoFileCache;
        private readonly CriterionInstanceComparer myComparer = new CriterionInstanceComparer();

        public CriteriaConfig CriteriaConfig { get; set; }

        #region Constructor

        /// <summary>
        /// </summary>
        public CriterionCache(ImageThumbnailCache criterionThumbnailCache, CriteriaConfig criteriaConfig, IVideoFileCache videoFileCache)
        {
            myCriterionThumbnailCache = criterionThumbnailCache;
            CriteriaConfig = criteriaConfig;
            myVideoFileCache = videoFileCache;
        }

        #endregion

        //TODO consider to remove, use Cacheditems instead directly
        public List<CriterionInstance> GetCriterionInstances(Criterion criterion)
        {
            //return this.CachedItems[criterion.Name];
            return CachedItems.Where(item => item.Criterion.Name.Equals(criterion.Name, StringComparison.InvariantCultureIgnoreCase)).ToList();
        }

        //TODO consider to remove, use Cacheditems instead directly
        public List<CriterionInstance> GetCriterionInstances(string criterionName)
        {
            //return this.CachedItems[criterionName];
            return CachedItems.Where(item => item.Criterion.Name.Equals(criterionName, StringComparison.InvariantCultureIgnoreCase)).ToList();
        }

        public Criterion GetCriterionByName(string criterionName)
        {
            return CriteriaConfig.Criteria.FirstOrDefault(x => x.Name.Equals(criterionName, StringComparison.InvariantCultureIgnoreCase));
        }

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
                    InitCacheCrit(progress, progressFile);
            });
        }

        #endregion

        private void InitCacheCrit(IProgress<int> progress, IProgress<string> progressFile)
        {
            progress.Report(0);
            progressFile.Report("Updating Criterion Cache.");

            var fromThumbNails = CreateInstancesFromFileSystemThumbNails();
            var fromFileNames = CreateInstancesFromFileNames();
            CopyCounts(fromThumbNails, fromFileNames);

            var notAvailable = fromFileNames.Except(fromThumbNails, myComparer).ToList();
            MarkNotAvailable(notAvailable);
            fromThumbNails.AddRange(notAvailable);

            var redCross = fromThumbNails.Except(fromFileNames, myComparer).ToList();
            MarkRedCross(redCross);

            this.CachedItems = fromThumbNails;

            CachedItems
                .OrderBy(ci => ci.Thumbnail.Image == null)
                .ThenBy(ci => ci.Name);

            progress.Report(100);
            progressFile.Report("");
        }

        private void CopyCounts(List<CriterionInstance> fromThumbNails, List<CriterionInstance> fromFileNames)
        {
            foreach (CriterionInstance critInst in fromThumbNails)
            {
                var existing = fromFileNames.FirstOrDefault(x => myComparer.Equals(x, critInst));
                if (existing != null)
                {
                    critInst.Count = existing.Count;
                }
            }
        }

        private void MarkRedCross(IEnumerable<CriterionInstance> redCross)
        {
            foreach (CriterionInstance critInst in redCross)
            {
                critInst.Thumbnail.Image = Helper.CrossBitmapImage(critInst.Thumbnail.Image);
                critInst.Thumbnail.Image.Freeze();
            }
        }

        private void MarkNotAvailable(IEnumerable<CriterionInstance> notAvailable)
        {
            foreach (CriterionInstance critInst in notAvailable)
            {
                critInst.Thumbnail.Image = Helper.NAimage;
                critInst.Thumbnail.Image.Freeze();
            }
        }

        private List<CriterionInstance> CreateInstancesFromFileSystemThumbNails()
        {
            var result = new List<CriterionInstance>();

            foreach (var thumb in myCriterionThumbnailCache.CachedItems)
            {
                //identify matching criterion
                var criterion = CriteriaConfig.GetCriterionByName(thumb.Key.Directory.Name);

                CriterionInstance ci = new CriterionInstance();
                ci.Name = Path.GetFileNameWithoutExtension(thumb.Key.FullName);
                ci.Criterion = criterion;
                ci.Count = 0;
                ci.SumStars = 0;
                ci.Thumbnail.Image = myCriterionThumbnailCache.GetByFilename(Path.GetFileName(thumb.Key.Name));
                ci.Thumbnail.Image.Freeze();
                result.Add(ci);

                //criterion.Instances.Add(ci);
            };

            return result;
        }

        private List<CriterionInstance> CreateInstancesFromFileNames()
        {
            var result = new List<CriterionInstance>();

            IConverter mmConverter = new FileNameConverter(CriteriaConfig);

            foreach (var file in myVideoFileCache.CachedItems)
            {
                var fileCrits = mmConverter.ConvertFrom(file);
                foreach (var fileCrit in fileCrits)
                {
                    var existing = result.FirstOrDefault(x => myComparer.Equals(x, fileCrit));
                    if (existing != null)
                    {
                        existing.Count++;
                    }
                    else
                    {
                        fileCrit.Count++;
                        result.Add(fileCrit);
                    }
                }
            }

            return result;
        }
    }
}
