using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using MetaExplorerBE.MetaModels;
using MetaExplorerBE.Configuration;
using MetaExplorerBE.ExtendedFileProperties;

namespace MetaExplorerBE
{
    /// <summary>
    /// 
    /// </summary>
    public class VideoMetaModelCache : BaseCache<VideoMetaModel>
    {
        #region Private Members

        private IExtendedFilePropertiesProvider myExtendedPropertiesProvider;

        #endregion

        private string BaseDir { get; set; }

        private readonly VideoFileCache myVideoFileCache;
        private readonly VideoThumbnailCache myVideoThumbnailCache;

        #region Constructor

        /// <summary>
        /// </summary>
        public VideoMetaModelCache(VideoFileCache videoFileCache, VideoThumbnailCache videoThumbnailCache)
        {
            this.myVideoFileCache = videoFileCache;
            this.myVideoThumbnailCache = videoThumbnailCache;

            myExtendedPropertiesProvider = new ExtendedFilePropertiesProvider(ExtendedFilePropertiesTechnology.Shell32).Provider;
        }

        #endregion

        #region Public Methods

        public override Task InitCacheAsync(IProgress<int> progress, IProgress<string> progressFile)
        {
            return Task.Factory.StartNew(() =>
            {
                this.CachedItems.Clear();
                IConverter mmConverter = new FileNameConverter();

                progress.Report(0);

                int i = 0;
                foreach (string file in myVideoFileCache.CachedItems)
                {
                    VideoMetaModel mm = mmConverter.ConvertFrom(file);
                    try
                    {
                        //attach thumbnail
                        string md5 = Helper.GetMD5Hash(file);

                        var cachedThumb = myVideoThumbnailCache.GetByFilename(md5);

                        if (cachedThumb != null)
                        {
                            mm.Thumbnail = cachedThumb;
                            mm.Thumbnail.Freeze();
                        }
                        else
                        {
                            //TODO: create thumbnail if not yet existing
                            mm.Thumbnail = null;
                        }

                        //retrieve extended file properties
                        FileInfo fi = new FileInfo(file);
                        mm.BitRate = myExtendedPropertiesProvider.GetBitrate(fi);
                        mm.FrameHeight = myExtendedPropertiesProvider.GetFrameHeight(fi);
                        mm.FrameWidth = myExtendedPropertiesProvider.GetFrameWidth(fi);

                        //define the captions of the thumbnails
                        mm.ThumbnailCaption1 = Path.GetFileName(file);
                        mm.ThumbnailCaption2 = String.Format("{0}x{1}(@{2})", mm.FrameWidth, mm.FrameHeight, mm.BitRate);

                        //add meta model to cache
                        this.CachedItems.Add(mm);
                    }
                    catch (Exception e)
                    {
                        throw new Exception(String.Format("Error while attaching thumbnail to video meta model for file <{0}>. Message: <{1}>", file, e.Message));
                    }

                    progress.Report((i * 99) / myVideoFileCache.CachedItems.Count);
                    progressFile.Report(file);
                    i++;
                }

                //sort by date
                this.ResortBy(x => x.DateModified);

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
                List<VideoMetaModel> noThumbnail = this.CachedItems.FindAll(x => { return x.Thumbnail == null; });

                //update progress
                int idx = 0;
                foreach (VideoMetaModel videoMetaModel in noThumbnail)
                {
                    this.myVideoThumbnailCache.UpdateThumbnailCache(Path.GetFileName(videoMetaModel.FileName));

                    idx++;
                    progressFile.Report(videoMetaModel.FileName);
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
        public List<VideoMetaModel> GetThumbnailFileSelection(VideoMetaModel mmRef)
        {
            List<VideoMetaModel> videoFiles = this.GetVideoFileSelection(mmRef);
            return videoFiles;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="regex"></param>
        /// <returns></returns>
        public List<VideoMetaModel> GetVideoFileSelection(VideoMetaModel mmRef)
        {
            List<VideoMetaModel> res = new List<VideoMetaModel>(this.CachedItems);

            //check stars
            if (mmRef.Stars != 0)
            {
                res = res.Where((VideoMetaModel m) => { return m.Stars == mmRef.Stars; }).ToList();
            }

            foreach (Criterion criterion in CriteriaConfig.Criteria)
            {
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

        public void ResortBy(Func<VideoMetaModel, object> func)
        {
            this.CachedItems = this.CachedItems.OrderByDescending(func).ToList();
        }

        #endregion
    }
}
