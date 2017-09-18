using System.IO;

namespace MetaExplorer.Common.VideoProperties
{
    public struct VideoProperties
    {
        public int bitrate;
        public int frameheight;
        public int frameWidth;
    }

    public interface IVideoPropertiesProvider
    {
        VideoProperties GetVideoProperties(FileInfo file);
    }
}
