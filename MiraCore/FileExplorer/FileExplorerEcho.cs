using System.IO;
using System.Text;

namespace MiraCore.Client.FileExplorer
{
    public class FileExplorerEcho : MessageSerializable
    {
        public ushort Length;
        public string Message;

        public const int c_MaxEchoLength = 256;

        public FileExplorerEcho()
        {
            Length = 0;
            Message = string.Empty;
        }

        public FileExplorerEcho(string p_Message)
        {
            if (p_Message.Length > ushort.MaxValue)
            {
                Length = 0;
                Message = string.Empty;
                return;
            }

            Message = p_Message;
            Length = (ushort)Message.Length;
        }

        public override byte[] Serialize()
        {
            if (Message.Length > ushort.MaxValue)
            {
                Length = 0;
                Message = string.Empty;
            }

            if (Length != Message.Length)
                Length = (ushort)Message.Length;

            using (var s_Writer = new BinaryWriter(new MemoryStream()))
            {
                s_Writer.Write(Length);
                s_Writer.Write(Encoding.ASCII.GetBytes(Message.PadRight(c_MaxEchoLength, '\0')));

                return ((MemoryStream)s_Writer.BaseStream).ToArray();
            }
        }

        public override void Deserialize(BinaryReader p_Reader)
        {
            // TOOD: Implement
        }
    }
}
