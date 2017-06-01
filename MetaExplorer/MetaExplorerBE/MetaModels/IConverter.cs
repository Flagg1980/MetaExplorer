using MetaExplorer.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetaExplorerBE.MetaModels
{
    interface IConverter
    {
        Video ConvertFrom(string input);

        string ConvertTo(Video input);
    }
}
