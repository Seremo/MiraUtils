
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Threading;
using System.Windows.Threading;
using System.Text.RegularExpressions;
using MiraUI.Docker;
using System.Windows.Media;

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
        private Socket _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, 0);
        private byte[] _recieveBuffer = new byte[1024];

        public const string ToolContentId = "LoggerTool";
        public override ToolLocation PreferredLocation => ToolLocation.Left;
        public override double PreferredHeight => base.PreferredHeight;
        public override double PreferredWidth => 650;

        private ObservableCollection<KernelLogs> _logsList;
        public ObservableCollection<KernelLogs> logsList
        {
            get { return _logsList; }
            set
            {
                _logsList = value;
                NotifyOfPropertyChange(() => logsList);
            }
        }

        public KernelLogs TextParser(string txt)
        {
            string time = "[" + DateTime.Now.ToString("HH:mm:ss") + "] ";
            return new KernelLogs {Color = "White", Time = time, Message = txt };
        }

        public KernelLogsViewModel(string ip) : base("Kernel Logger")
        {
            ContentId = ToolContentId;
            logsList = new ObservableCollection<KernelLogs>();
            try
            {
                _clientSocket.Connect(new IPEndPoint(IPAddress.Parse(ip), 9998));
            }
            catch (SocketException ex)
            {
                logsList.Add(TextParser(ex.Message));
            }
            _clientSocket.BeginReceive(_recieveBuffer, 0, _recieveBuffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), null);
        }

        private delegate void UpdateUiTextDelegate(string text);

        public string waitforn = "";

        private string IntToColor(int color)
        {
            string color_r = "White";
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

        private void WriteData(string text)
        {
            string time = "[" + DateTime.Now.ToString("HH:mm:ss") + "] ";
            string color = "White";
            var result = Regex.Split(text, @"([\n])");
            foreach (var res in result)
            {
                if (res == "\n")
                {
                    if (waitforn != "")
                    {
                        string final = waitforn;
                        if (final.Contains("\x1b[") && final.Contains("m[") && final.Contains("\x1b[0m"))
                        {
                            color = IntToColor(Convert.ToInt32(final.Split('[')[1].Substring(0, 2)));
                            final = final.Remove(0, 5).Replace("[0m", "");
                        }
                        logsList.Add(new KernelLogs { Color = color, Time = time, Message = final });
                        waitforn = "";
                    }
                } else
                {
                    waitforn += res;
                }
            }
        }

        private void ReceiveCallback(IAsyncResult AR)
        {
            //Check how much bytes are recieved and call EndRecieve to finalize handshake
            int recieved = _clientSocket.EndReceive(AR);

            if (recieved <= 0)
                return;

            //Copy the recieved data into new buffer , to avoid null bytes
            byte[] recData = new byte[recieved];
            Buffer.BlockCopy(_recieveBuffer, 0, recData, 0, recieved);

            string text = System.Text.Encoding.ASCII.GetString(recData).Trim('\0');
            bool addlog = text != "";
            if(addlog)
            {
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Send, new UpdateUiTextDelegate(WriteData), text);
            }
            _clientSocket.BeginReceive(_recieveBuffer, 0, _recieveBuffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), null);
        }

        public void ClearText()
        {
            logsList.Clear();
        }

        public void CopyText()
        {
            string tocopy = "";
            foreach(var tex in logsList)
            {
                tocopy += tex.Time + " " + tex.Message + "\n";
            }
            Clipboard.SetText(tocopy);
        }
    }
}
