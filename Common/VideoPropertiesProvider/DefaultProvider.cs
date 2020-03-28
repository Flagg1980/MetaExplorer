using System;
using System.IO;
using Domain;

namespace MetaExplorer.Common.VideoPropertiesProvider
{
    internal class DefaultProvider : IVideoPropertiesProvider
    {
        [STAThread]
        private string GetProperty(FileInfo file, int index)
        {
            return "";
        }

        public VideoProperties GetVideoProperties(FileInfo file)
        {
            return new VideoProperties
            {
                BitRate = 0,
                FrameHeight = 0,
                FrameWidth = 0
            };
        }
    }
}
