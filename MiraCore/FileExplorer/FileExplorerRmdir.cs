using System.IO;

namespace MiraCore.Client.FileExplorer
{
    public class FileExplorerRmdirRequest : MessageSerializable
    {
        public ushort PathLength;
        public char[] Path;

        public FileExplorerRmdirRequest()
        {
            PathLength = 0;
            Path = new char[FileExplorerExtensions.c_MaxPathLength];
        }

        public override void Deserialize(BinaryReader p_Reader)
        {
            PathLength = p_Reader.ReadUInt16();
            Path = new string(p_Reader.ReadChars(FileExplorerExtensions.c_MaxPathLength)).TrimEnd('\0').ToCharArray();
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
