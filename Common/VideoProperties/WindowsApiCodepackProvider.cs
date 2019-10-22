//using Microsoft.WindowsAPICodePack.Shell;
//using Microsoft.WindowsAPICodePack.Shell.PropertySystem;
//using Shell32;
//using System;
//using System.IO;
//using System.Text.RegularExpressions;

//namespace MetaExplorer.Common2.VideoProperties
//{
//    internal class WindowsApiCodepackProvider : IVideoPropertiesProvider
//    {
//        private string GetProperty(FileInfo file, PropertyKey key)
//        {
//            try
//            {
//                var shellFile = ShellFile.FromParsingName(file.FullName);
//                var result = shellFile.Properties.GetProperty(key).ValueAsObject.ToString();

//                return result;
//            }
//            catch (Exception e)
//            {
//                string msg = String.Format("Warning: Error getting property with index <{0}> from file <{1}>. Error message is: <{2}>", key.ToString(), file.FullName, e.Message);
//                Console.WriteLine(msg);

//                return string.Empty;
//            }
//        }

//        public VideoProperties GetVideoProperties(FileInfo file)
//        {
//            int bitRateI = this.GetNumericalProperty(file, SystemProperties.System.Video.TotalBitrate);
//            int widthI = this.GetNumericalProperty(file, SystemProperties.System.Video.FrameWidth);
//            int heightI = this.GetNumericalProperty(file, SystemProperties.System.Video.FrameHeight);

//            return new VideoProperties
//            {
//                bitrate = bitRateI,
//                frameheight = heightI,
//                frameWidth = widthI
//            };
//        }

//        private int GetNumericalProperty(FileInfo file, PropertyKey key)
//        {
//            string prop = string.Empty;

//            try
//            {
//                prop = this.GetProperty(file, key);
//                var numbersAsString = Regex.Match(prop, @"\d+").Value;
//                var numberInt = int.Parse(numbersAsString);
//                return numberInt;
//            }
//            catch (Exception e)
//            {
//                string msg = String.Format("Warning: Error transforming string property <{0}> to integer. Error message is: <{1}>", prop, e.Message);
//                Console.WriteLine(msg);

//                return -1;
//            }
//        }
//    }
//}
