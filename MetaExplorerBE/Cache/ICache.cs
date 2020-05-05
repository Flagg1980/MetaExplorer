using Domain;
using MetaExplorer.Domain;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace MetaExplorerBE
{
    public interface ICache<T>
    {
        Task InitCacheAsync(IProgress<int> progress, IProgress<string> progressFile);

        public List<T> CachedItems { get; }
    }

    public interface ICache<T,U>
    {
        Task InitCacheAsync(IProgress<int> progress, IProgress<string> progressFile);

        Dictionary<T, U> CachedItems { get; }
    }

    public interface ICriterionCache : ICache<string, ObservableCollection<CriterionInstance>>
    {
        ObservableCollection<CriterionInstance> GetCriterionInstances(Criterion criterion);

        ObservableCollection<CriterionInstance> GetCriterionInstances(string criterionName);
    }

    public interface IVideoFileCache : ICache<string>
    { 
    }

    public interface IVideoThumbnailCache : ICache<FileInfo, BitmapSource>
    {
        BitmapSource GetByFilename(string filename);

        BitmapSource UpdateThumbnailCache(FileInfo fileInfo);
    }

    public interface IVideoPropertiesCache : ICache<string, VideoProperties>
    {
        string Location { get; }
    }
}
