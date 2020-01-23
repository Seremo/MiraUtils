using System.IO;

namespace MiraCore.Client.FileExplorer
{
    public class FileExplorerMkdirRequest : MessageSerializable
    {
        public int Mode;
        public ushort PathLength;
        public char[] Path;

        public override void Deserialize(BinaryReader p_Reader)
        {
            Mode = p_Reader.ReadInt32();
            PathLength = p_Reader.ReadUInt16();
            Path = new string(p_Reader.ReadChars(FileExplorerExtensions.c_MaxPathLength)).TrimEnd('\0').ToCharArray();

        }

        public override byte[] Serialize()
        {
            using (var s_Writer = new BinaryWriter(new MemoryStream()))
            {
                s_Writer.Write(Mode);
                s_Writer.Write(PathLength);
                s_Writer.Write(new string(Path).PadRight(FileExplorerExtensions.c_MaxPathLength, '\0').ToCharArray());

                return ((MemoryStream)s_Writer.BaseStream).ToArray();
            }
        }
    }
}
