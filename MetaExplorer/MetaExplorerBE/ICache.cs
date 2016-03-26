using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Configuration;
using System.Collections.Specialized;
using System.Windows.Media.Imaging;
using MetaExplorer.MetaModels;

namespace MetaExplorer
{
    /// <summary>
    /// Types of cached objects:
    /// - Video File Cache:         A List of all video files in the video file directory
    /// - Video MetaModel Cache:    A List of all video meta model files
    /// - Thumbnail File Cache:     A list of all Thumbnails in the .thumbnail directory
    /// </summary>
    public interface ICache
    {
        int Progress { get; }

        string ProgressFile { get; }

        string[] VideoFileCache { get; }

        List<VideoMetaModel> VideoMetaModelCache { get; }

        List<CriterionInstance> GetCriterionInstances(Criterion criterion);

        List<CriterionInstance> GetCriterionInstances(string criterionName);

        Task UpdateVideoMetaModelCacheAsync();

        Task UpdateNonExistingThumbnailCacheAsync();

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

        Task GenerateDictAsync(Criterion criterion);

        /// <summary>
        /// Reads all files from a given base directory and caches the file locations.
        /// </summary>
        Task UpdateVideoFileCacheAsync();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="regex"></param>
        /// <returns></returns>
        List<VideoMetaModel> GetVideoFileSelection(VideoMetaModel mmRef);
    }
}
