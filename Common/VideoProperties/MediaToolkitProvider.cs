using MediaToolkit.Model;
using System;
using System.IO;

namespace MetaExplorer.Common.VideoProperties
{
    internal class MediaToolkitProvider : IVideoPropertiesProvider
    {
        MediaToolkit.Engine engine;

        public MediaToolkitProvider()
        {
            engine = new MediaToolkit.Engine();
        }

        //public int GetBitrate(FileInfo file)
        //{
        //    var inputFile = new MediaFile { Filename = file.FullName };
        //    engine.GetMetadata(inputFile);

        //    int? res = inputFile.Metadata.VideoData.BitRateKbs;

        //    return res != null ? (int)inputFile.Metadata.VideoData.BitRateKbs : -1;
        //}

        //public int GetFrameHeight(FileInfo file)
        //{
        //    var inputFile = new MediaFile { Filename = file.FullName };
        //    engine.GetMetadata(inputFile);

        //    string fs = inputFile.Metadata.VideoData.FrameSize;
        //    int xIndex = fs.IndexOf("x");
        //    string heights = fs.Substring(xIndex + 1);
        //    int heighti = Int32.Parse(heights);

        //    return heighti;
        //}

        //public int GetFrameWidth(FileInfo file)
        //{
        //    var inputFile = new MediaFile { Filename = file.FullName };
        //    engine.GetMetadata(inputFile);

        //    string fs = inputFile.Metadata.VideoData.FrameSize;

        //    int xIndex = fs.IndexOf("x");
        //    string widths = fs.Substring(0, xIndex);
        //    int widthi = Int32.Parse(widths);

        //    return widthi;
        //}

        public VideoProperties GetVideoProperties(FileInfo file)
        {
            var inputFile = new MediaFile { Filename = file.FullName };
            engine.GetMetadata(inputFile);

            //get bitrate
            int? bitrateN = inputFile.Metadata.VideoData.BitRateKbs;
            int bitratei = bitrateN != null ? (int)bitrateN : -1;

            //get video height and width
            string fs = inputFile.Metadata.VideoData.FrameSize;
            int xIndex = fs.IndexOf("x");

            string heights = fs.Substring(xIndex + 1);
            int heighti = Int32.Parse(heights);

            string widths = fs.Substring(0, xIndex);
            int widthi = Int32.Parse(widths);

            return new VideoProperties
            {
                bitrate = bitratei,
                frameheight = heighti,
                frameWidth = widthi
            };
        }
    }
}
