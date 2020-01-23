using System.Windows.Controls;
using System.Windows;
using Xceed.Wpf.AvalonDock.Layout;
using MiraUI.ViewModels;

namespace MiraUI.Docker
{
    class PanesTemplateSelector : DataTemplateSelector
    {
        public PanesTemplateSelector()
        {

        }

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

        public override System.Windows.DataTemplate SelectTemplate(object item, System.Windows.DependencyObject container)
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
