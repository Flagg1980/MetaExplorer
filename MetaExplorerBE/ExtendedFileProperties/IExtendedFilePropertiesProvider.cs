using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetaExplorerBE.ExtendedFileProperties
{
    [Obsolete]
    public interface IExtendedFilePropertiesProvider
    {
        int GetBitrate(FileInfo file);

        int GetFrameHeight(FileInfo file);

        int GetFrameWidth(FileInfo file);
    }
}
