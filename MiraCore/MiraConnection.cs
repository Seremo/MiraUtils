using Google.Protobuf;
using System;
using System.IO;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MiraCore.Client
{
    public class MiraConnection
    {
        private TcpClient m_Socket;
        private string m_NickName;

        private const string c_DefaultAddress = "127.0.0.1";
        private const ushort c_DefaultPort = 9999;

        /// <summary>
        /// Is the current connection active/connected
        /// </summary>
        public bool IsConnected => m_Socket?.Connected ?? false;

        /// <summary>
        /// Connection hostname or ip address
        /// </summary>
        public string Address { get; protected set; }

        /// <summary>
        /// Connection port number
        /// </summary>
        public ushort Port { get; protected set; }

        /// <summary>
        /// User settable nickname, or address:port
        /// </summary>
        public string Nickname
        {
            get { return string.IsNullOrWhiteSpace(m_NickName) ? $"{Address}:{Port}" : m_NickName; }
            set { m_NickName = value; }
        }

        /// <summary>
        /// Send and recv timeout for information, default 2s
        /// </summary>
        public int TimeoutInSeconds = 20;

        /// <summary>
        /// Maximum buffer size, this should optimally match the host
        /// </summary>
        public int MaxBufferSize = 0x10000;

        // Temporary buffer for holding our data
        private readonly byte[] m_Buffer;

        /// <summary>
        /// Creates a new MiraConnection
        /// </summary>
        /// <param name="p_Address">Hostname or IP address of target</param>
        /// <param name="p_Port">Port to connect to, default(9999)</param>
        public MiraConnection(string p_Address = c_DefaultAddress, ushort p_Port = c_DefaultPort)
        {
            Address = p_Address;
            Port = p_Port;

            // Allocate some space for our buffer
            m_Buffer = new byte[MaxBufferSize];
        }

        /// <summary>
        /// Connects to the target
        /// </summary>
        /// <returns>True on success, false otherwise</returns>
        public bool Connect()
        {
            // If we are already connected disconnect first
            if (IsConnected)
                Disconnect();

            try
            {
                // Attempt to connect to the host
                m_Socket = new TcpClient(Address, Port)
                {
                    ReceiveTimeout = 1000 * TimeoutInSeconds,
                    SendTimeout = 1000 * TimeoutInSeconds,

                    SendBufferSize = MaxBufferSize,
                    ReceiveBufferSize = MaxBufferSize
                };
            }
            catch (Exception p_Exception)
            {
                Console.WriteLine($"exception: {p_Exception.InnerException}");  
                return false;
            }

            return IsConnected;
        }

        /// <summary>
        /// Disconnects from the target
        /// </summary>
        public void Disconnect()
        {
            m_Socket?.Close();
        }

        /// <summary>
        /// Sends a message without expecting a response
        /// NOTE: If you incorrectly use this one instead of SendMessageWithResponse stuff may not work
        /// </summary>
        /// <param name="p_OutoingMessage">Outgoing message with all fields set and ready to go</param>
        /// <returns>True on success, false otherwise</returns>
        public bool SendMessage(RpcTransport p_OutoingMessage)
        {
            // Validate that we are connected
            if (!IsConnected)
                return false;

            var s_MessageData = p_OutoingMessage.ToByteArray();
            var s_Buffer = new byte[Marshal.SizeOf<ulong>() + s_MessageData.Length];

            Buffer.BlockCopy(BitConverter.GetBytes((ulong)s_MessageData.Length), 0, s_Buffer, 0, 8);
            Buffer.BlockCopy(s_MessageData, 0, s_Buffer, 8, s_MessageData.Length);

            // Write this, should call serialize which will have header + payload data
            using (var s_Writer = new BinaryWriter(m_Socket.GetStream(), Encoding.ASCII, true))
                s_Writer.Write(s_Buffer);

            return true;
        }

        public RpcTransport SendMessageWithResponse(RpcTransport p_Message)
        {
            if (!SendMessage(p_Message))
                return null;

            byte[] s_Data = null;
            using (var s_Reader = new BinaryReader(m_Socket.GetStream(), Encoding.ASCII, true))
            {
                var s_MessageSize = s_Reader.ReadUInt64();
                if (s_MessageSize > (ulong)MaxBufferSize)
                    return null;

                s_Data = s_Reader.ReadBytes((int)s_MessageSize);
            }

            if (s_Data == null)
                return null;

            return RpcTransport.Parser.ParseFrom(s_Data);
        }

        public RpcTransport RecvMessage()
        {
            byte[] s_Data = null;
            using (var s_Reader = new BinaryReader(m_Socket.GetStream(), Encoding.ASCII, true))
            {
                var s_MessageSize = s_Reader.ReadUInt64();
                if (s_MessageSize > (ulong)MaxBufferSize)
                    return null;

                s_Data = s_Reader.ReadBytes((int)s_MessageSize);
            }

            if (s_Data == null)
                return null;

            return RpcTransport.Parser.ParseFrom(s_Data);
        }
    }
}
