using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetaExplorer.MetaModels
{
    interface IConverter
    {
        VideoMetaModel ConvertFrom(string input);

        string ConvertTo(VideoMetaModel input);
    }
}
