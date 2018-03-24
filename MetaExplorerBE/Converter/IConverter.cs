using MetaExplorer.Domain;

namespace MetaExplorerBE.Converter
{
    interface IConverter
    {
        Video ConvertFrom(string input);

        string ConvertTo(Video input);
    }
}
