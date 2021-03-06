﻿using System;
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
using static MiraCore.Client.FileExplorer.FileExplorerExtensions;

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
        public override ToolLocation PreferredLocation => ToolLocation.Left;
        public override double PreferredHeight => base.PreferredHeight;
        public override double PreferredWidth => 650;

        public async void Initialize()
        {
            await Task.Run(() => GetAppsList());
        }

        public async Task GetAppsList()
        {
            if (m_Connection == null)
                return;

            var s_Dents = m_Connection.GetDents("/user/app/");
            var s_Folders = s_Dents.Where(x => (FileTypes)x.Type == FileTypes.DT_DIR).ToList();
            foreach (var l_Dent in s_Folders)
            {
                if (l_Dent.Name == ".." || l_Dent.Name == ".")
                    continue;
                var fd = new PkgData();
                fd.Title = l_Dent.Name;
                if (await GetAppsImages(fd))
                    await Application.Current.Dispatcher.BeginInvoke(() => { PkgItems.Add(fd); });
            }
        }

        public async Task<bool> GetAppsImages(PkgData item)
        {
            string CacheFolder = Path.GetTempPath() + "\\MiraUtils\\";
            string FilePath = CacheFolder + item.Title + ".png";
            if (!Directory.Exists(CacheFolder))
                Directory.CreateDirectory(CacheFolder);

            if (File.Exists(FilePath))
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.DecodePixelWidth = 100;
                bitmap.DecodePixelHeight = 100;
                bitmap.UriSource = new Uri(FilePath);
                bitmap.EndInit();
                bitmap.Freeze();
                item.ImageData = bitmap;
                return true;
            }
            else
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
                    File.WriteAllBytes(FilePath, stream.ToArray());
                    return true;
                }
                return false;
            }
        }
    }
}
