using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MetaExplorerBE.ExtendedFileProperties
{
    [Obsolete]
    internal class Shellprovider : IExtendedFilePropertiesProvider
    {
        const int IDX_TOTAL_BITRATE = 310;
        const int IDX_DATA_BITRATE = 305;
        const int IDX_FRAME_WIDTH = 308;
        const int IDX_FRAME_HEIGHT = 306;

        

        public int GetBitrate(FileInfo file)
        {
            return this.GetNumericalProperty(file, IDX_TOTAL_BITRATE);
        }

        public int GetFrameWidth(FileInfo file)
        {
            return this.GetNumericalProperty(file, IDX_FRAME_WIDTH);
        }

        public int GetFrameHeight(FileInfo file)
        {
            return this.GetNumericalProperty(file, IDX_FRAME_HEIGHT);
        }

        [STAThread]
        private string GetProperty(FileInfo file, int index)
        {
            try
            {

                Shell32.Shell myShell = new Shell32.Shell();
                //set the namespace to file path
                Shell32.Folder folder = myShell.NameSpace(file.DirectoryName);
                ////get ahandle to the file
                Shell32.FolderItem folderItem = folder.ParseName(file.Name);

                return folder.GetDetailsOf(folderItem, index);
            }
            catch (Exception e)
            {
                string msg = String.Format("Warning: Error getting property with index <{0}> from file <{1}>. Error message is: <{2}>", index, file.FullName, e.Message);
                Console.WriteLine(msg);

                return string.Empty;
            }
        }

        private int GetNumericalProperty(FileInfo file, int index)
        {
            string prop = string.Empty;

            try
            {
                prop = this.GetProperty(file, index);
                return int.Parse(Regex.Match(prop, @"\d+").Value);
            }
            catch (Exception e)
            {
                string msg = String.Format("Warning: Error transforming string property <{0}> to integer. Error message is: <{1}>", prop, e.Message);
                Console.WriteLine(msg);

                return -1;
            }
        }
    }
}
