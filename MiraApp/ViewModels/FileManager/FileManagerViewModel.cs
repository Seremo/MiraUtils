using Caliburn.Micro;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using MiraCore.Client;
using MiraCore.Client.FileExplorer;
using MiraUI.Docker;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace MiraUI.ViewModels
{
    public class FileManagerViewModel : ToolViewModel
    {
        private MiraConnection m_Connection;


        public class FileDetails
        {
            public string FileName { get; set; }
            public string FileImage { get; set; }
            public string FileCreation { get; set; }
            public string Path { get; set; }
            public FileTypes Type { get; set; }
        }

        private ObservableCollection<FileDetails> _listItems;
        public ObservableCollection<FileDetails> listItems
        {
            get { return _listItems; }
            set
            {
                _listItems = value;
                NotifyOfPropertyChange(() => listItems);
            }
        }

        private FileDetails _SelectedItem;
        public FileDetails SelectedItem
        {
            get { return _SelectedItem; }
            set
            {
                _SelectedItem = value;
                NotifyOfPropertyChange(() => SelectedItem);
            }
        }

        private string _CurrentPath;
        public string CurrentPath
        {
            get { return _CurrentPath; }
            set
            {
                _CurrentPath = value;
                NotifyOfPropertyChange(() => CurrentPath);
            }
        }

        public override ToolLocation PreferredLocation => ToolLocation.Right;

        public const string ToolContentId = "FileTool";
        public FileManagerViewModel(MiraConnection connection) : base("File Manager")
        {
            ContentId = ToolContentId;
            m_Connection = connection;
            listItems = new BindableCollection<FileDetails>();
            Populate(m_Connection, "/");
        }

        private void Populate(MiraConnection p_Connection, string p_Path)
        {
            listItems.Clear();
            CurrentPath = p_Path;
            if (p_Path != "/")
            {
                string path = p_Path.Remove(p_Path.LastIndexOf('/'), 1);
                string pathback = path.Substring(0, path.LastIndexOf('/')) + "/";
                listItems.Add(new FileDetails { FileName = "...", Path = pathback, Type = FileTypes.DT_UNKNOWN });
            }
            var s_Dents = p_Connection.GetDents(p_Path);
            var s_Folders = s_Dents.Where(x => (FileTypes)x.Type == FileTypes.DT_DIR).ToList();
            var s_Files = s_Dents.Where(x => (FileTypes)x.Type == FileTypes.DT_REG).ToList();
            var s_Others = s_Dents.Where(x => (FileTypes)x.Type != FileTypes.DT_DIR && (FileTypes)x.Type != FileTypes.DT_REG).ToList();
            foreach (var l_Dent in s_Folders)
            {
                if (l_Dent.NameString == ".." || l_Dent.NameString == ".")
                    continue;
                var fd = new FileDetails();
                fd.FileName = l_Dent.NameString;
                fd.Path = p_Path + l_Dent.NameString + "/";
                fd.FileImage = $"pack://application:,,,/Images/folder.ico";
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
                fd.FileImage = $"pack://application:,,,/Images/file.ico";
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
                fd.FileImage = $"pack://application:,,,/Images/drive.ico";
                fd.Type = FileTypes.DT_SOCK;
                listItems.Add(fd);
            }
        }

        public void RowSelect()
        {
            if (SelectedItem == null)
                return;
            if(SelectedItem.Path.EndsWith('/'))
                Populate(m_Connection, SelectedItem.Path);
        }

        public void DownloadCommand()
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
                var s_FolderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog
                {
                    Description = "Select folder to download to...",
                    ShowNewFolderButton = true
                };
                if (s_FolderBrowserDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                    return;
                var s_SavePath = s_FolderBrowserDialog.SelectedPath;
                RecursiveDownload(s_Connection, s_SavePath, s_Path, true);
            }
            else if (SelectedItem.Type == FileTypes.DT_REG || SelectedItem.Type == FileTypes.DT_CHR)
            {
                var s_SafeFileDialog = new System.Windows.Forms.SaveFileDialog
                {
                    Title = "Save as...",
                    FileName = SelectedItem.FileName,
                    Filter = "All Files (*.*)|*.*",
                };
                if (s_SafeFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    var s_Stream = s_Connection.DownloadFile(s_Path, s_SafeFileDialog.FileName);
                    s_Stream.Flush();
                    s_Stream.Close();
                }
            }
        }

        private void RecursiveDownload(MiraConnection p_Connection, string p_LocalDir, string p_RemoteDir, bool p_Overwrite = true)
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
                {// /mnt/sandbox/pfsmnt/CUSA03041-app0/ps4/audio/sfx/CUTSCENE_MASTERED_ONLY.rpf
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
            MetroWindow metroWindow = Application.Current.MainWindow as MetroWindow;
            var result = await metroWindow.ShowMessageAsync("Confirm delete", $"Are you sure you want to delete ({s_Path})?", MessageDialogStyle.AffirmativeAndNegative);
            if (result == MessageDialogResult.Negative)
                return;
            if (!m_Connection.Unlink(s_Path))
            {
                await metroWindow.ShowMessageAsync("Error", $"Could not delete ({s_Path})!");
            }
        }

        public async void DecryptCommand()
        {
            if (SelectedItem == null)
                return;

            var s_Path = SelectedItem.Path;

            MetroWindow metroWindow = Application.Current.MainWindow as MetroWindow;
            var s_Data = m_Connection.DecryptSelf(s_Path);
            if (s_Data == null)
            {
                await metroWindow.ShowMessageAsync("Error", "Could not decrypt self");
                return;
            }

            var s_SafeFileDialog = new System.Windows.Forms.SaveFileDialog
            {
                Title = "Save as...",
                FileName = SelectedItem.FileName,
                Filter = "All Files (*.*)|*.*",
            };

            if (s_SafeFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                File.WriteAllBytes(s_SafeFileDialog.FileName, s_Data);
                await metroWindow.ShowMessageAsync("Succses", "Self decrypted");
            }
        }
    }
}
