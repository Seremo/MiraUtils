using Caliburn.Micro;
using MiraUI.Docker;
using Xceed.Wpf.AvalonDock.Layout;

namespace MiraUI.ViewModels
{
    public class HomeViewModel : ToolViewModel
    {
        public const string ToolContentId = "HomeTool";
        public override ToolLocation PreferredLocation => ToolLocation.Bottom;
        public HomeViewModel() : base("Home")
        {
            ContentId = ToolContentId;
        }
    }
}
