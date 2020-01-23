using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiraCore.Client.FileExplorer
{
    public class FileExplorerDecryptSelfRequest : MessageSerializable
    {
        public ushort PathLength;
        public byte[] Path = new byte[FileExplorerExtensions.c_MaxPathLength];

        public FileExplorerDecryptSelfRequest(string p_Path)
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

    public class FileExplorerDecryptSelfResponse : MessageSerializable
    {
        public ulong Index;
        public ulong Offset;
        public ulong Length;
        public byte[] Data = new byte[FileExplorerExtensions.c_MaxBufferLength];

        public override void Deserialize(BinaryReader p_Reader)
        {
            Index = p_Reader.ReadUInt64();
            Offset = p_Reader.ReadUInt64();
            Length = p_Reader.ReadUInt64();
            Data = p_Reader.ReadBytes(FileExplorerExtensions.c_MaxBufferLength);
        }

        public override byte[] Serialize()
        {
            using (var s_Writer = new BinaryWriter(new MemoryStream()))
            {
                s_Writer.Write(Index);
                s_Writer.Write(Offset);
                s_Writer.Write(Length);
                s_Writer.Write(Data);

                return ((MemoryStream)s_Writer.BaseStream).ToArray();
            }
        }
    }
}
