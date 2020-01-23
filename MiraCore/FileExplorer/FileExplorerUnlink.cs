using System.IO;
using System.Text;

namespace MiraCore.Client.FileExplorer
{
    public class FileExplorerUnlinkRequest : MessageSerializable
    {
        public ushort PathLength;
        public byte[] Path;

        public FileExplorerUnlinkRequest(string p_Path)
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
        }

        public override void Deserialize(BinaryReader p_Reader)
        {
            PathLength = p_Reader.ReadUInt16();
            Path = p_Reader.ReadBytes(FileExplorerExtensions.c_MaxPathLength);
        }

        public override byte[] Serialize()
        {
            using (var s_Writer = new BinaryWriter(new MemoryStream()))
            {
                s_Writer.Write(PathLength);
                s_Writer.Write(Path);
                return ((MemoryStream)s_Writer.BaseStream).ToArray();
            }
        }
    }
}
