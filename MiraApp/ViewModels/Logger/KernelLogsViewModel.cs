using System;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using MiraUI.Docker;

namespace MiraUI.ViewModels
{
    public class KernelLogs
    {
        public string Color { get; set; }
        public string Time { get; set; }
        public string Message { get; set; }
    }

    public class KernelLogsViewModel : ToolViewModel
    {
        public const string ToolContentId = "LoggerTool";
        private readonly Socket _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, 0);

        private ObservableCollection<KernelLogs> _logsList;
        private readonly byte[] _recieveBuffer = new byte[8192];
        private string IP = "";

        public string waitforn = "";

        public KernelLogsViewModel(string ip) : base("Kernel Logger")
        {
            ContentId = ToolContentId;
            logsList = new ObservableCollection<KernelLogs>();
            IP = ip;
            Initialize();
        }
        public override ToolLocation PreferredLocation => ToolLocation.Left;
        public override double PreferredHeight => base.PreferredHeight;
        public override double PreferredWidth => 650;
        public ObservableCollection<KernelLogs> logsList
        {
            get => _logsList;
            set
            {
                _logsList = value;
                NotifyOfPropertyChange(() => logsList);
            }
        }

        public KernelLogs TextParser(string txt)
        {
            var time = "[" + DateTime.Now.ToString("HH:mm:ss") + "] ";
            return new KernelLogs {Color = "White", Time = time, Message = txt};
        }

        public async void Initialize()
        {
            await Task.Run(async () => await OpenConnection());
        }

        public async Task OpenConnection()
        {
            try
            {
                await _clientSocket.ConnectAsync(new IPEndPoint(IPAddress.Parse(IP), 9998));
            }
            catch (SocketException ex)
            {
                logsList.Add(TextParser(ex.Message));
            }
            _clientSocket.BeginReceive(_recieveBuffer, 0, _recieveBuffer.Length, SocketFlags.None, ReceiveCallback, null);
        }

        private string IntToColor(int color)
        {
            var color_r = "White";
            switch (color)
            {
                case 30:
                    color_r = "Black";
                    break;
                case 31:
                    color_r = "Red";
                    break;
                case 32:
                    color_r = "Green";
                    break;
                case 33:
                    color_r = "Yellow";
                    break;
                case 34:
                    color_r = "Blue";
                    break;
                case 35:
                    color_r = "Magenta";
                    break;
                case 36:
                    color_r = "Cyan";
                    break;
                case 37:
                    color_r = "White";
                    break;
                case 39:
                    color_r = "Default";
                    break;
                case 90:
                    color_r = "LightGray";
                    break;
                case 91:
                    color_r = "LightRed";
                    break;
                case 92:
                    color_r = "LightGreen";
                    break;
                case 93:
                    color_r = "LightYellow";
                    break;
                case 94:
                    color_r = "LightBlue";
                    break;
                case 95:
                    color_r = "LightMagneta";
                    break;
                case 96:
                    color_r = "LightCyan";
                    break;
                case 97:
                    color_r = "LightWhite";
                    break;
            }
            return color_r;
        }

        private async Task WriteData(string text)
        {
            var time = "[" + DateTime.Now.ToString("HH:mm:ss") + "] ";
            var color = "White";
            var result = Regex.Split(text, @"([\n])");
            foreach (var res in result)
                if (res == "\n")
                {
                    if (waitforn != "")
                    {
                        var final = waitforn;
                        if (final.Contains("\x1b[") && final.Contains("m[") && final.Contains("\x1b[0m"))
                        {
                            color = IntToColor(Convert.ToInt32(final.Split('[')[1].Substring(0, 2)));
                            final = final.Remove(0, 5).Replace("[0m", "");
                        }
                        logsList.Add(new KernelLogs {Color = color, Time = time, Message = final});
                        waitforn = "";
                    }
                }
                else
                {
                    waitforn += res;
                }
        }

        private async void ReceiveCallback(IAsyncResult AR)
        {
            //Check how much bytes are recieved and call EndRecieve to finalize handshake
            var recieved = _clientSocket.EndReceive(AR);

            if (recieved <= 0)
                return;

            //Copy the recieved data into new buffer , to avoid null bytes
            var recData = new byte[recieved];
            Buffer.BlockCopy(_recieveBuffer, 0, recData, 0, recieved);

            var text = Encoding.ASCII.GetString(recData).Trim('\0');
            var addlog = text != "";
            if (addlog)
                await Application.Current.Dispatcher.BeginInvoke(async () => { await WriteData(text); });
            //Application.Current.Dispatcher.Invoke(DispatcherPriority.Send, new UpdateUiTextDelegate(WriteData), text);
            _clientSocket.BeginReceive(_recieveBuffer, 0, _recieveBuffer.Length, SocketFlags.None, ReceiveCallback, null);
        }

        public void ClearText()
        {
            logsList.Clear();
        }

        public void CopyText()
        {
            var tocopy = "";
            foreach (var tex in logsList)
                tocopy += tex.Time + " " + tex.Message + "\n";
            Clipboard.SetText(tocopy);
        }

        private delegate void UpdateUiTextDelegate(string text);
    }
}
