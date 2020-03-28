using System;

namespace MetaExplorer.Common.VideoPropertiesProvider
{
    public enum VideoPropertiesTechnology
    {
        //Shell32,
        MediaToolkit,
        //WinAPICodePack,
        None
    }

    public class VideoPropertiesProvider
    {
        public IVideoPropertiesProvider Provider
        {
            get;
            private set;
        }

        public VideoPropertiesProvider(VideoPropertiesTechnology technology, string ffmpegLocation)
        {
            //if (technology == VideoPropertiesTechnology.Shell32)
            //    Provider = new Shellprovider();
            if (technology == VideoPropertiesTechnology.MediaToolkit)
            {
                Provider = new MediaToolkitProvider()
                {
                    FFMpegLocation = ffmpegLocation
                };

            }
            //else if (technology == VideoPropertiesTechnology.WinAPICodePack)
            //    Provider = new WindowsApiCodepackProvider();
            else if (technology == VideoPropertiesTechnology.None)
            {
                Provider = new DefaultProvider();
            }
            else
            {
                string errorMsg = String.Format("Unsupported Technology: <{0}>", technology.ToString());
                throw new Exception(errorMsg);
            }
        }


    }
}
