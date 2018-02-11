using Shell32;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace MetaExplorer.Common.VideoProperties
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
                bitrate = 0,
                frameheight = 0,
                frameWidth = 0
            };
        }

        private int GetNumericalProperty(FileInfo file, int index)
        {
            return 0;
        }
    }
}
