using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using MiraCore.Client;

namespace MiraUI.ViewModels
{
    public class InvertableBool
    {

        public InvertableBool(bool b)
        {
            Value = b;
        }

        public bool Value { get; }
        public bool Invert => !Value;

        public static implicit operator InvertableBool(bool b)
        {
            return new InvertableBool(b);
        }

        public static implicit operator bool(InvertableBool b)
        {
            return b.Value;
        }
    }
    public class ShellViewModel : Conductor<IScreen>.Collection.OneActive
    {

        private InvertableBool _isConnected = false;

        private string _ps4ip;

        public BindableCollection<Screen> _tools;

        public ShellViewModel()
        {
            Tools = new BindableCollection<Screen>();
            Initialize();
        }

        public MiraConnection Connection
        {
            get;
            set;
        }

        public string PS4IP
        {
            get => _ps4ip;
            set
            {
                _ps4ip = value;
                NotifyOfPropertyChange(() => PS4IP);
            }
        }
        public InvertableBool IsConnected
        {
            get => _isConnected;
            set
            {
                _isConnected = value;
                NotifyOfPropertyChange(() => IsConnected);
            }
        }
        public IObservableCollection<Screen> Tools
        {
            get => _tools;
            set
            {
                _tools = value as BindableCollection<Screen>;
                NotifyOfPropertyChange(() => Tools);
            }
        }

        public async Task ShowTool(ToolViewModel model)
        {
            if (Tools.Contains(model))
                model.IsVisible = true;
            else
                Tools.Add(model);
            model.IsSelected = true;
        }

        public async Task CloseTool(ToolViewModel model)
        {
            if (Tools.Contains(model))
                Tools.Remove(model);
        }

        public async void ReinitializeTools()
        {
            Tools.Clear();
            await ShowTool(new HomeViewModel());
        }

        public async Task ConnectDialog()
        {
            var metroWindow = Application.Current.MainWindow as MetroWindow;
            var ip = await metroWindow.ShowInputAsync("Connect", "Enter your PS4 IP address:");
            if (ip == null)
                return;
            var s_Connection = new MiraConnection(ip);
            if (s_Connection.Connect())
            {
                Connection = s_Connection;
                IsConnected = true;
                await ShowTool(new KernelLogsViewModel(ip));
                await ShowTool(new FileManagerViewModel(Connection));
                await ShowTool(new AppsViewModel(Connection));
            }
        }

        public async Task DisconnectDialog()
        {
            if (!IsConnected)
                return;
            var metroWindow = Application.Current.MainWindow as MetroWindow;
            var result = await metroWindow.ShowMessageAsync("Confirm disconnect", "Are you sure you want to disconnect?",
                MessageDialogStyle.AffirmativeAndNegative);
            if (result == MessageDialogResult.Negative)
                return;
            Connection.Disconnect();
            IsConnected = false;
            Connection = null;
            ReinitializeTools();
        }

        public async Task ClearCacheDialog()
        {
            if (!IsConnected)
                return;
            var metroWindow = Application.Current.MainWindow as MetroWindow;
            var result = await metroWindow.ShowMessageAsync("Confirm delete cache", "Are you sure you want to delete local cache?",
                MessageDialogStyle.AffirmativeAndNegative);
            if (result == MessageDialogResult.Negative)
                return;

            DirectoryInfo di = new DirectoryInfo(Path.GetTempPath() + "\\MiraUtils\\");
            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }
            foreach (DirectoryInfo dir in di.GetDirectories())
            {
                dir.Delete(true);
            }
        }

        public async void Initialize()
        {
            await ShowTool(new HomeViewModel());
        }
    }
}
