using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using MiraCore.Client;
using MiraCore.Client.FileExplorer;
using MiraUI.Docker;

namespace MiraUI.ViewModels
{
    public class PkgData
    {

        public string Title { get; set; }

        public BitmapImage ImageData { get; set; }
    }

    public class AppsViewModel : ToolViewModel
    {
        public const string ToolContentId = "LoggerTool";

        private ObservableCollection<PkgData> _pkgItems;
        public ObservableCollection<PkgData> PkgItems
        {
            get => _pkgItems;
            set
            {
                _pkgItems = value;
                NotifyOfPropertyChange(() => PkgItems);
            }
        }

        private readonly MiraConnection m_Connection;

        public AppsViewModel(MiraConnection connection) : base("Apps")
        {
            PkgItems = new ObservableCollection<PkgData>();
            m_Connection = connection;
            Initialize();
        }
        public override ToolLocation PreferredLocation => ToolLocation.Bottom;
        public override double PreferredHeight => base.PreferredHeight;
        public override double PreferredWidth => 650;

        public async void Initialize()
        {
            await Task.Run(async() => await GetAppsList());
        }

        public async Task GetAppsList()
        {
            if (m_Connection == null)
                return;

            var s_Dents = m_Connection.GetDents("/user/app/");
            var s_Folders = s_Dents.Where(x => (FileTypes)x.Type == FileTypes.DT_DIR).ToList();
            foreach (var l_Dent in s_Folders)
            {
                if (l_Dent.NameString == ".." || l_Dent.NameString == ".")
                    continue;
                var fd = new PkgData();
                fd.Title = l_Dent.NameString;
                if (await GetAppsImages(fd))
                    await Application.Current.Dispatcher.BeginInvoke(() => { PkgItems.Add(fd); });
            }
        }

        public async Task<bool> GetAppsImages(PkgData item)
        {
            var stream = m_Connection.DownloadFileToByteArray("/user/appmeta/" + item.Title + "/icon0.png");
            if (stream != null)
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.DecodePixelWidth = 100;
                bitmap.DecodePixelHeight = 100;
                bitmap.StreamSource = new MemoryStream(stream.ToArray());
                bitmap.EndInit();
                bitmap.Freeze();
                item.ImageData = bitmap;
                return true;
            }
            return false;
        }
    }
}
