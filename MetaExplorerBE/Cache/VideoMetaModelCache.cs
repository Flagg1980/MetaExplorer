using Domain;
using MetaExplorer.Common;
using MetaExplorer.Domain;
using MetaExplorerBE.Converter;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MetaExplorerBE
{
    /// <summary>
    /// 
    /// </summary>
    public class VideoMetaModelCache : BaseCache<Video>
    {
        private readonly IVideoPropertiesCache myVideoPropertiesCache;
        private readonly IVideoFileCache myVideoFileCache;
        private readonly IVideoThumbnailCache myVideoThumbnailCache;
        private readonly ICriterionCache myCriterionCache;

        private readonly IEqualityComparer<CriterionInstance> myComparer = new CriterionInstanceComparer();

        #region Constructor

        /// <summary>
        /// </summary>
        public VideoMetaModelCache(IVideoFileCache videoFileCache, IVideoThumbnailCache videoThumbnailCache, IVideoPropertiesCache videoPropertiesCache, ICriterionCache criterionCache)
        {
            myVideoFileCache = videoFileCache;
            myVideoThumbnailCache = videoThumbnailCache;
            myVideoPropertiesCache = videoPropertiesCache;
            myCriterionCache = criterionCache;
        }

        #endregion

        #region Public Methods

        public override Task InitCacheAsync(IProgress<int> progress, IProgress<string> progressFile)
        {
            return Task.Factory.StartNew(() =>
            {
                this.CachedItems.Clear();
                IConverter mmConverter = new FileNameConverter(myCriterionCache.CriteriaConfig);

                progress.Report(0);

                int i = 0;
                foreach (string file in myVideoFileCache.CachedItems)
                {
                    Video video = new Video();
                    video.criteriaMapping = mmConverter.ConvertFrom(file);
                    
                    video.File = new FileInfo(file);
                    string starsString = video.criteriaMapping.FirstOrDefault(x =>
                        x.Criterion.Name.Equals("Stars", StringComparison.InvariantCultureIgnoreCase)).Name;
                    
                    video.Stars = int.Parse(starsString[0].ToString());
                    try
                    {
                        //attach thumbnail
                        string md5 = Helper.GetMD5Hash(new FileInfo(file));

                        var cachedThumb = myVideoThumbnailCache.GetByFilename(md5);

                        if (cachedThumb != null)
                        {
                            video.Thumbnail.Image = cachedThumb;
                            video.Thumbnail.Image.Freeze();
                        }
                        else
                        {
                            //TODO: create thumbnail if not yet existing
                            video.Thumbnail = null;
                        }

                        //retrieve extended file properties
                        FileInfo fi = new FileInfo(file);

                        video.Properties = myVideoPropertiesCache.CachedItems.GetValueOrDefault(md5);
                        if (video.Properties == null)
                        {
                            video.Properties = new VideoProperties();
                            Trace.TraceError($"Could not find Video properties cache entry for file <{file}> with md5 <{md5}> in video properties cache file <{myVideoPropertiesCache.Location}>.");
                        }

                        //define the captions of the thumbnails
                        video.ThumbnailCaption1 = Path.GetFileName(file);
                        video.ThumbnailCaption2 = string.Format("{0} x {1} ({2} Kbs)", video.Properties.FrameWidth, video.Properties.FrameHeight, video.Properties.BitRate.ToString("N0"));

                        //add meta model to cache
                        this.CachedItems.Add(video);
                    }
                    catch (Exception e)
                    {
                        throw new Exception(string.Format("Error while attaching thumbnail to video meta model for file <{0}>. Message: <{1}>", file, e.Message));
                    }

                    progress.Report((i * 99) / myVideoFileCache.CachedItems.Count);
                    progressFile.Report(file);
                    i++;
                }

                //sort by date
                this.ResortBy(x => x.File.LastWriteTime);

                progress.Report(100);
            });
        }

        public Task UpdateNonExistingThumbnailCacheAsync(IProgress<int> progress, IProgress<string> progressFile)
        {
            return Task.Factory.StartNew(() =>
            {
                progress.Report(0);
                progressFile.Report("");

                //get thumbnail in cache
                List<Video> noThumbnail = this.CachedItems.FindAll(x => { return x.Thumbnail == null; });

                //update progress
                int idx = 0;
                foreach (Video videoMetaModel in noThumbnail)
                {
                    var bitmapSource = this.myVideoThumbnailCache.UpdateThumbnailCache(videoMetaModel.File);

                    videoMetaModel.Thumbnail = new Thumbnail();
                    videoMetaModel.Thumbnail.Image = bitmapSource;
                    videoMetaModel.Thumbnail.Image.Freeze();

                    idx++;
                    progressFile.Report(videoMetaModel.File.FullName);
                    progress.Report((idx * 100) / noThumbnail.Count);
                }

                progress.Report(100);
                progressFile.Report("");
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="regex"></param>
        /// <returns></returns>
        public ObservableCollection<Video> GetVideoFileSelection(Video mmRef)
        {
            IEnumerable<Video> res = this.CachedItems;

            //check stars
            if (mmRef.Stars != 0)
            {
                res = res.Where((Video m) => { return m.Stars == mmRef.Stars; });
            }

            //foreach (Criterion criterion in myCriterionCache.CriteriaConfig.Criteria)
            foreach (CriterionInstance critInst in mmRef.criteriaMapping)
            {
                res = res.Where((Video video) =>
                                {
                                    return video.criteriaMapping.Contains(critInst, myComparer);
                                })
                                .ToList();
            }

            //check freetext search (last because most expensive)
            if (!string.IsNullOrEmpty(mmRef.ThumbnailCaption1))
            {
                res = res.Where((Video m) => 
                    {
                        return Path.GetFileNameWithoutExtension(m.File.FullName).IndexOf(mmRef.ThumbnailCaption1, StringComparison.CurrentCultureIgnoreCase) >= 0;
                    })
                    .ToList();
            }

            return new ObservableCollection<Video>(res);
        }

        public void ResortBy(Func<Video, object> func)
        {
            this.CachedItems = this.CachedItems.OrderByDescending(func).ToList();
        }

        #endregion
    }
}
