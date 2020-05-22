using MetaExplorer.Domain;
using System.Collections.Generic;

namespace MetaExplorerBE.Converter
{
    interface IConverter
    {
        List<CriterionInstance> ConvertFrom(string input);
        string ConvertTo(List<CriterionInstance> input);
    }
}
