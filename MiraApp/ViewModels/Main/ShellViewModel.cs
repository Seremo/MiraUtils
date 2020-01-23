using Caliburn.Micro;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using MiraCore.Client;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Xceed.Wpf.AvalonDock.Layout;

namespace MiraUI.ViewModels
{
    public class InvertableBool
    {
        private bool value = false;

        public bool Value { get { return value; } }
        public bool Invert { get { return !value; } }

        public InvertableBool(bool b)
        {
            value = b;
        }

        public static implicit operator InvertableBool(bool b)
        {
            return new InvertableBool(b);
        }

        public static implicit operator bool(InvertableBool b)
        {
            return b.value;
        }

    }
    public class ShellViewModel : Conductor<IScreen>.Collection.OneActive
    {
        public MiraConnection Connection
        {
            get;
            set;
        }

        private string _ps4ip;
        public string PS4IP
        {
            get { return _ps4ip; }
            set
            {
                _ps4ip = value;
                NotifyOfPropertyChange(() => PS4IP);
            }
        }

        private InvertableBool _isConnected = false;
        public InvertableBool IsConnected
        {
            get { return _isConnected; }
            set
            {
                _isConnected = value;
                NotifyOfPropertyChange(() => IsConnected);
            }
        }

        public BindableCollection<Screen> _tools;
        public IObservableCollection<Screen> Tools
        {
            get { return _tools; }
            set
            {
                _tools = value as BindableCollection<Screen>;
                NotifyOfPropertyChange(() => Tools);
            }
        }

        public void ShowTool(ToolViewModel model)
        {
            if (Tools.Contains(model))
                model.IsVisible = true;
            else
                Tools.Add(model);
            model.IsSelected = true;
        }

        public void ReinitializeTools()
        {
            Tools.Clear();
            ShowTool(new HomeViewModel());
        }

        public async Task ConnectDialog()
        {
            var metroWindow = (Application.Current.MainWindow as MetroWindow);
            string ip = await metroWindow.ShowInputAsync("Connect", "Enter your PS4 IP address:");
            if (ip != null)
            {
                MiraConnection s_Connection = new MiraConnection(ip, 9999);
                if (s_Connection.Connect())
                {
                    Connection = s_Connection;
                    IsConnected = true;
                    ShowTool(new FileManagerViewModel(Connection));
                    ShowTool(new KernelLogsViewModel(ip));
                    return;
                }
            }
            await TryCloseAsync(true);
        }

        public async Task DisconnectDialog()
        {
            if (!IsConnected)
                return;
            var metroWindow = (Application.Current.MainWindow as MetroWindow);
            var result = await metroWindow.ShowMessageAsync("Confirm disconnect", "Are you sure you want to disconnect?", MessageDialogStyle.AffirmativeAndNegative);
            if (result == MessageDialogResult.Negative)
                return;
            Connection.Disconnect();
            IsConnected = false;
            Connection = null;
            ReinitializeTools();
            await TryCloseAsync(true);
        }

        public ShellViewModel()
        {
            Tools = new BindableCollection<Screen>();
            ShowTool(new HomeViewModel());
        }

    }
}
