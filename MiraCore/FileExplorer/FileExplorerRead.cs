using System.IO;

namespace MiraCore.Client.FileExplorer
{
    public class FileExplorerReadRequest : MessageSerializable
    {
        public int Handle;
        public int Count;

        public override void Deserialize(BinaryReader p_Reader)
        {
            Handle = p_Reader.ReadInt32();
            Count = p_Reader.ReadInt32();
        }

        public override byte[] Serialize()
        {
            using (var s_Writer = new BinaryWriter(new MemoryStream()))
            {
                s_Writer.Write(Handle);
                s_Writer.Write(Count);

                return ((MemoryStream)s_Writer.BaseStream).ToArray();
            }
        }
    }

    public class FileExplorerReadResponse : MessageSerializable
    {
        public short Count { get; set; }

        public byte[] Data { get; set; } = new byte[FileExplorerExtensions.c_MaxBufferLength];

        public override void Deserialize(BinaryReader p_Reader)
        {
            Count = p_Reader.ReadInt16();
            Data = p_Reader.ReadBytes(FileExplorerExtensions.c_MaxBufferLength);
        }

        public override byte[] Serialize()
        {
            using (var s_Writer = new BinaryWriter(new MemoryStream()))
            {
                s_Writer.Write(Count);
                s_Writer.Write(Data);

                return ((MemoryStream)s_Writer.BaseStream).ToArray();
            }
        }
    }
}
