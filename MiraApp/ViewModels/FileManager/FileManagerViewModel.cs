using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Caliburn.Micro;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using MiraCore.Client;
using MiraCore.Client.FileExplorer;
using MiraUI.Docker;
using Application = System.Windows.Application;

namespace MiraUI.ViewModels
{
    public class FileManagerViewModel : ToolViewModel
    {

        public const string ToolContentId = "FileTool";

        private string _CurrentPath;

        private ObservableCollection<FileDetails> _listItems;

        private FileDetails _SelectedItem;
        private readonly MiraConnection m_Connection;
        public FileManagerViewModel(MiraConnection connection) : base("File Manager")
        {
            ContentId = ToolContentId;
            m_Connection = connection;
            listItems = new BindableCollection<FileDetails>();
            Initialize();
        }
        public ObservableCollection<FileDetails> listItems
        {
            get => _listItems;
            set
            {
                _listItems = value;
                NotifyOfPropertyChange(() => listItems);
            }
        }
        public FileDetails SelectedItem
        {
            get => _SelectedItem;
            set
            {
                _SelectedItem = value;
                NotifyOfPropertyChange(() => SelectedItem);
            }
        }
        public string CurrentPath
        {
            get => _CurrentPath;
            set
            {
                _CurrentPath = value;
                NotifyOfPropertyChange(() => CurrentPath);
            }
        }

        public override ToolLocation PreferredLocation => ToolLocation.Right;

        public async void Initialize()
        {
            await Populate(m_Connection, "/");
        }

        private async Task Populate(MiraConnection p_Connection, string p_Path)
        {
            listItems.Clear();
            CurrentPath = p_Path;
            if (p_Path != "/")
            {
                var path = p_Path.Remove(p_Path.LastIndexOf('/'), 1);
                var pathback = path.Substring(0, path.LastIndexOf('/')) + "/";
                listItems.Add(new FileDetails {FileName = "...", Path = pathback, Type = FileTypes.DT_UNKNOWN});
            }
            var s_Dents = p_Connection.GetDents(p_Path);
            var s_Folders = s_Dents.Where(x => (FileTypes)x.Type == FileTypes.DT_DIR).ToList();
            var s_Files = s_Dents.Where(x => (FileTypes)x.Type == FileTypes.DT_REG).ToList();
            var s_Others = s_Dents
                .Where(x => (FileTypes)x.Type != FileTypes.DT_DIR && (FileTypes)x.Type != FileTypes.DT_REG).ToList();
            foreach (var l_Dent in s_Folders)
            {
                if (l_Dent.NameString == ".." || l_Dent.NameString == ".")
                    continue;
                var fd = new FileDetails();
                fd.FileName = l_Dent.NameString;
                fd.Path = p_Path + l_Dent.NameString + "/";
                fd.FileImage = "pack://application:,,,/Images/folder.ico";
                fd.Type = FileTypes.DT_DIR;
                listItems.Add(fd);
            }
            foreach (var l_Dent in s_Files)
            {
                if (l_Dent.NameString == ".." || l_Dent.NameString == ".")
                    continue;
                var fd = new FileDetails();
                fd.FileName = l_Dent.NameString;
                fd.Path = p_Path + l_Dent.NameString;
                fd.FileImage = "pack://application:,,,/Images/file.ico";
                fd.Type = FileTypes.DT_REG;
                listItems.Add(fd);
            }
            foreach (var l_Dent in s_Others)
            {
                if (l_Dent.NameString == ".." || l_Dent.NameString == ".")
                    continue;
                var fd = new FileDetails();
                fd.FileName = l_Dent.NameString;
                fd.Path = p_Path + l_Dent.NameString;
                fd.FileImage = "pack://application:,,,/Images/drive.ico";
                fd.Type = FileTypes.DT_SOCK;
                listItems.Add(fd);
            }
        }

        public async void RowSelect()
        {
            if (SelectedItem == null)
                return;
            if (SelectedItem.Path.EndsWith('/'))
                await Populate(m_Connection, SelectedItem.Path);
        }

        public async void DownloadCommand()
        {
            if (SelectedItem == null)
                return;

            var s_Path = SelectedItem.Path;
            if (string.IsNullOrWhiteSpace(s_Path))
                return;

            var s_Connection = m_Connection;
            if (s_Connection == null)
                return;

            if (SelectedItem.Type == FileTypes.DT_DIR)
            {
                var s_FolderBrowserDialog = new FolderBrowserDialog
                {
                    Description = "Select folder to download to...",
                    ShowNewFolderButton = true
                };
                if (s_FolderBrowserDialog.ShowDialog() != DialogResult.OK)
                    return;
                var s_SavePath = s_FolderBrowserDialog.SelectedPath;
                RecursiveDownload(s_Connection, s_SavePath, s_Path);
            }
            else if (SelectedItem.Type == FileTypes.DT_REG || SelectedItem.Type == FileTypes.DT_CHR)
            {
                var s_SafeFileDialog = new SaveFileDialog
                {
                    Title = "Save as...",
                    FileName = SelectedItem.FileName,
                    Filter = "All Files (*.*)|*.*"
                };
                if (s_SafeFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        var s_Stream = s_Connection.DownloadFile(s_Path, s_SafeFileDialog.FileName);
                        s_Stream.Flush();
                        s_Stream.Close();
                    }
                    catch (Exception e)
                    {
                        var metroWindow = Application.Current.MainWindow as MetroWindow;
                        await metroWindow.ShowMessageAsync("Error", e.Message);
                    }
                }
            }
        }

        private async void RecursiveDownload(MiraConnection p_Connection, string p_LocalDir, string p_RemoteDir,
            bool p_Overwrite = true)
        {
            var s_DirectoryEntries = p_Connection.GetDents(p_RemoteDir);
            foreach (var l_Entry in s_DirectoryEntries)
            {
                if (l_Entry.NameString == "." || l_Entry.NameString == "..")
                    continue;

                var l_LocalPath = $"{p_LocalDir}/{new string(l_Entry.Name)}";
                var l_RemotePath = $"{p_RemoteDir}/{new string(l_Entry.Name)}";

                if ((FileTypes)l_Entry.Type == FileTypes.DT_DIR)
                {
                    if (!Directory.Exists(l_LocalPath))
                        Directory.CreateDirectory(l_LocalPath);

                    RecursiveDownload(p_Connection, l_LocalPath, l_RemotePath);
                }
                else if ((FileTypes)l_Entry.Type == FileTypes.DT_REG)
                {
                    // /mnt/sandbox/pfsmnt/CUSA03041-app0/ps4/audio/sfx/CUTSCENE_MASTERED_ONLY.rpf
                    var s_Data = p_Connection.DownloadFile(l_RemotePath, l_LocalPath);
                    if (s_Data == null)
                        continue;

                    s_Data.Flush();
                    s_Data.Close();
                }
            }
        }

        public async void DeleteCommand()
        {
            if (SelectedItem == null)
                return;

            var s_Path = SelectedItem.Path;
            var metroWindow = Application.Current.MainWindow as MetroWindow;
            var result = await metroWindow.ShowMessageAsync("Confirm delete",
                $"Are you sure you want to delete ({s_Path})?", MessageDialogStyle.AffirmativeAndNegative);
            if (result == MessageDialogResult.Negative)
                return;
            if (!m_Connection.Unlink(s_Path))
                await metroWindow.ShowMessageAsync("Error", $"Could not delete ({s_Path})!");
        }

        public async void DecryptCommand()
        {
            if (SelectedItem == null)
                return;

            var s_Path = SelectedItem.Path;

            var metroWindow = Application.Current.MainWindow as MetroWindow;
            var s_Data = m_Connection.DecryptSelf(s_Path);
            if (s_Data == null)
            {
                await metroWindow.ShowMessageAsync("Error", "Could not decrypt self");
                return;
            }

            var s_SafeFileDialog = new SaveFileDialog
            {
                Title = "Save as...",
                FileName = SelectedItem.FileName,
                Filter = "All Files (*.*)|*.*"
            };

            if (s_SafeFileDialog.ShowDialog() == DialogResult.OK)
            {
                File.WriteAllBytes(s_SafeFileDialog.FileName, s_Data);
                await metroWindow.ShowMessageAsync("Succses", "Self decrypted");
            }
        }


        public class FileDetails
        {
            public string FileName { get; set; }
            public string FileImage { get; set; }
            public string FileCreation { get; set; }
            public string Path { get; set; }
            public FileTypes Type { get; set; }
        }
    }
}
