using MiraUI.Docker;

namespace MiraUI.ViewModels
{
    public class HomeViewModel : ToolViewModel
    {
        public const string ToolContentId = "HomeTool";
        public HomeViewModel() : base("Home")
        {
            ContentId = ToolContentId;
        }
        public override ToolLocation PreferredLocation => ToolLocation.Bottom;
    }
}
