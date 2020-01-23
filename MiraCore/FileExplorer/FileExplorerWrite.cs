using System.IO;

namespace MiraCore.Client.FileExplorer
{
    public class FileExplorerWriteRequest : MessageSerializable
    {
        public int Handle;
        public int Count;
        public byte[] Data = new byte[FileExplorerExtensions.c_MaxBufferLength];

        public override void Deserialize(BinaryReader p_Reader)
        {
            Handle = p_Reader.ReadInt32();
            Count = p_Reader.ReadInt32();
            Data = p_Reader.ReadBytes(FileExplorerExtensions.c_MaxBufferLength);
        }

        public override byte[] Serialize()
        {
            using (var s_Writer = new BinaryWriter(new MemoryStream()))
            {
                s_Writer.Write(Handle);
                s_Writer.Write(Count);
                s_Writer.Write(Data);

                return ((MemoryStream)s_Writer.BaseStream).ToArray();
            }
        }
    }

    public class FileExplorerWriteResponse : MessageSerializable
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
