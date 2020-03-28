using Domain;
using System.IO;

namespace MetaExplorer.Common.VideoPropertiesProvider
{
    public interface IVideoPropertiesProvider
    {
        global::Domain.VideoProperties GetVideoProperties(FileInfo file);
    }
}
