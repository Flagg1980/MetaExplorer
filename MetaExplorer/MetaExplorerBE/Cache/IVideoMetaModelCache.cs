using MetaExplorerBE.MetaModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MetaExplorerBE
{
    /// <summary>
    /// 
    /// </summary>
    public interface IVideoMetaModelCache
    {
        string[] VideoFileCache { get; }

        List<VideoMetaModel> Cache { get; }

        Task UpdateVideoMetaModelCacheAsync(IProgress<int> progress, IProgress<string> progressFile);

        Task UpdateNonExistingThumbnailCacheAsync(IProgress<int> progress, IProgress<string> progressFile);

        /// <summary>
        /// Updates a thumbnail cache entry for a given video file. If the cache entry already exists, it is overwritten.
        /// </summary>
        void UpdateThumbnailCache(VideoMetaModel videoFileMetaModel);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="regex"></param>
        /// <returns></returns>
        List<VideoMetaModel> GetThumbnailFileSelection(VideoMetaModel mmRef);

        /// <summary>
        /// Reads all files from a given base directory and caches the file locations.
        /// </summary>
        Task UpdateVideoFileCacheAsync(IProgress<int> progress, IProgress<string> progressFile);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="regex"></param>
        /// <returns></returns>
        List<VideoMetaModel> GetVideoFileSelection(VideoMetaModel mmRef);
    }
}
