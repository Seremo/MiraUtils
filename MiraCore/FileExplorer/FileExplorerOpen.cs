using System.IO;
using System.Text;

namespace MiraCore.Client.FileExplorer
{
    public class FileExplorerOpenRequest : MessageSerializable
    {
        public int Flags;
        public int Mode;
        public ushort PathLength;
        public byte[] Path = new byte[FileExplorerExtensions.c_MaxPathLength];

        public FileExplorerOpenRequest(string p_Path, int p_Flags, int p_Mode)
        {
            if (p_Path.Length > FileExplorerExtensions.c_MaxPathLength)
            {
                Path = Encoding.ASCII.GetBytes(p_Path.Substring(0, FileExplorerExtensions.c_MaxPathLength));
                PathLength = FileExplorerExtensions.c_MaxPathLength;
            }
            else
            {
                Path = Encoding.ASCII.GetBytes(p_Path.PadRight(FileExplorerExtensions.c_MaxPathLength, '\0'));
                PathLength = (ushort)p_Path.Length;
            }
            Flags = p_Flags;
            Mode = p_Mode;
        }

        public override void Deserialize(BinaryReader p_Reader)
        {
            Flags = p_Reader.ReadInt32();
            Mode = p_Reader.ReadInt32();

            PathLength = p_Reader.ReadUInt16();
            Path = p_Reader.ReadBytes(FileExplorerExtensions.c_MaxPathLength);
        }

        public override byte[] Serialize()
        {
            using (var s_Writer = new BinaryWriter(new MemoryStream()))
            {
                s_Writer.Write(Flags);
                s_Writer.Write(Mode);
                s_Writer.Write(PathLength);
                s_Writer.Write(Path);

                return ((MemoryStream)s_Writer.BaseStream).ToArray();
            }
        }
    }

    public class FileExplorerOpenResponse : MessageSerializable
    {
        public override void Deserialize(BinaryReader p_Reader)
        {
            
        }

        public override byte[] Serialize()
        {
            return new byte[0];
        }
    }
}
