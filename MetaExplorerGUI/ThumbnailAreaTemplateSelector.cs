using System.Windows;
using System.Windows.Controls;

namespace MetaExplorerGUI
{
    public class ThumbnailAreaTemplateSelector : DataTemplateSelector
    {
        public DataTemplate TemplateThumbnailAreaCriterionThumbs { get; set; }
        public DataTemplate TemplateThumbnailAreaVideoThumbs { get; set; }
        
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            switch (item)
            {
                case MetaExplorer.Domain.Video _:
                    return TemplateThumbnailAreaVideoThumbs;
                case MetaExplorer.Domain.CriterionInstance _:
                    return TemplateThumbnailAreaCriterionThumbs;
            }

            throw new System.Exception("Unknown type for ThumbnailAreaTemplateSelector: " + item.GetType());
        }
    }
}
