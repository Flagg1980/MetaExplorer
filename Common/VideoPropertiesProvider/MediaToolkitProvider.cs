﻿using MediaToolkit.Model;
using System;
using System.IO;
using Domain;

namespace MetaExplorer.Common.VideoPropertiesProvider
{
    internal class MediaToolkitProvider : IVideoPropertiesProvider
    {
        public string FFMpegLocation { get; set; }

        public VideoProperties GetVideoProperties(FileInfo file)
        {
            MediaToolkit.Engine engine = new MediaToolkit.Engine(FFMpegLocation);

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
                BitRate = bitratei,
                FrameHeight = heighti,
                FrameWidth = widthi
            };
        }
    }
}
