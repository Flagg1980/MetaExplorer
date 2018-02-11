using MetaExplorer.Common;
using MetaExplorer.Common.VideoProperties;
using MetaExplorer.Domain;
using MetaExplorerBE.MetaModels;
using System;
using System.Collections.Generic;
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
        #region Private Members

        private IVideoPropertiesProvider myExtendedPropertiesProvider;

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

            myExtendedPropertiesProvider = new VideoPropertiesProvider(VideoPropertiesTechnology.None).Provider;
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
                    Video mm = mmConverter.ConvertFrom(file);
                    try
                    {
                        //attach thumbnail
                        string md5 = Helper.GetMD5Hash(Path.GetFileName(file));

                        var cachedThumb = myVideoThumbnailCache.GetByFilename(md5);

                        if (cachedThumb != null)
                        {
                            mm.Thumbnail.Image = cachedThumb;
                            mm.Thumbnail.Image.Freeze();
                        }
                        else
                        {
                            //TODO: create thumbnail if not yet existing
                            mm.Thumbnail = null;
                        }

                        //retrieve extended file properties
                        FileInfo fi = new FileInfo(file);
                        VideoProperties vp = myExtendedPropertiesProvider.GetVideoProperties(fi);

                        mm.BitRate = vp.bitrate;
                        mm.FrameHeight = vp.frameheight;
                        mm.FrameWidth = vp.frameWidth;

                        //define the captions of the thumbnails
                        mm.ThumbnailCaption1 = Path.GetFileName(file);
                        mm.ThumbnailCaption2 = String.Format("{0} x {1} ({2} kbps)", mm.FrameWidth, mm.FrameHeight, mm.BitRate/1000);

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
        public List<Video> GetThumbnailFileSelection(Video mmRef)
        {
            List<Video> videoFiles = this.GetVideoFileSelection(mmRef);
            return videoFiles;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="regex"></param>
        /// <returns></returns>
        public List<Video> GetVideoFileSelection(Video mmRef)
        {
            List<Video> res = new List<Video>(this.CachedItems);

            //check stars
            if (mmRef.Stars != 0)
            {
                res = res.Where((Video m) => { return m.Stars == mmRef.Stars; }).ToList();
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
                    res = res.Where((Video m) =>
                                    {
                                        return m.criteriaContents[criterion.Name].Contains(mmRef.criteriaContents[criterion.Name][0], 
                                                                                           StringComparer.CurrentCultureIgnoreCase);
                                    })
                                    .ToList();
                }
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

            return res;
        }

        public void ResortBy(Func<Video, object> func)
        {
            this.CachedItems = this.CachedItems.OrderByDescending(func).ToList();
        }

        #endregion
    }
}
