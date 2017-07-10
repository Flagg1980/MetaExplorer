using System;

namespace MetaExplorer.Common.VideoProperties
{
    public enum VideoPropertiesTechnology
    {
        Shell32,
        MediaToolkit
    }

    public class VideoPropertiesProvider
    {
        public IVideoPropertiesProvider Provider
        {
            get;
            private set;
        }

        public VideoPropertiesProvider(VideoPropertiesTechnology technology)
        {
            if (technology == VideoPropertiesTechnology.Shell32)
                Provider = new Shellprovider();
            else if (technology == VideoPropertiesTechnology.MediaToolkit)
                Provider = new MediaToolkitProvider();
            else
            {
                string errorMsg = String.Format("Unsupported Technology: <{0}>", technology.ToString());
                throw new Exception(errorMsg);
            }
        }


    }
}
