using System.Windows;
using System.Windows.Controls;
using MiraUI.ViewModels;
using Xceed.Wpf.AvalonDock.Layout;

namespace MiraUI.Docker
{
    internal class PanesTemplateSelector : DataTemplateSelector
    {

        public DataTemplate ToolTemplate
        {
            get;
            set;
        }

        public DataTemplate DocumentTemplate
        {
            get;
            set;
        }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var itemAsLayoutContent = item as LayoutContent;

            if (item is ToolViewModel)
                return ToolTemplate;

            // if (item is FileStatsViewModel)
            //    return FileStatsViewTemplate;

            return base.SelectTemplate(item, container);
        }
    }
}
