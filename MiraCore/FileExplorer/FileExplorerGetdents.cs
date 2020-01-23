using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MiraCore.Client.FileExplorer
{
    public enum FileTypes : byte
    {
        DT_UNKNOWN = 0,
        DT_FIFO = 1,
        DT_CHR = 2,
        DT_DIR = 4,
        DT_BLK = 6,
        DT_REG = 8,
        DT_LNK = 10,
        DT_SOCK = 12,
        DT_WHT = 14
    }

    public class FileExplorerDent : MessageSerializable
    {
        public uint Fileno;
        public ushort Reclen;
        public byte Type;
        public byte Namelen;
        public char[] Name = new char[FileExplorerExtensions.c_MaxNameLength];

        public string NameString => new string(Name);

        public FileExplorerDent()
        {
            Fileno = 0;
            Reclen = 0;
            Type = (byte)FileTypes.DT_UNKNOWN;
            Namelen = 0;
            Name = new char[FileExplorerExtensions.c_MaxNameLength];
        }

        public FileExplorerDent(BinaryReader p_Reader)
        {
            Deserialize(p_Reader);
        }

        public override void Deserialize(BinaryReader p_Reader)
        {
            Fileno = p_Reader.ReadUInt32();
            Reclen = p_Reader.ReadUInt16();
            Type = p_Reader.ReadByte();
            Namelen = p_Reader.ReadByte();
            Name = new string(p_Reader.ReadChars(FileExplorerExtensions.c_MaxNameLength)).TrimEnd('\0').ToCharArray();
        }

        public override byte[] Serialize()
        {
            using (var s_Writer = new BinaryWriter(new MemoryStream()))
            {
                s_Writer.Write(Fileno);
                s_Writer.Write(Reclen);
                s_Writer.Write(Type);
                s_Writer.Write(Namelen);
                s_Writer.Write(Name);

                return ((MemoryStream)s_Writer.BaseStream).ToArray();
            }
        }

        public override string ToString()
        {
            return $"{(FileTypes)Type}:{new string(Name)}";
        }
    }
    public class FileExplorerGetdentsRequest : MessageSerializable
    {
        public ushort Length;
        public byte[] Path;

        public FileExplorerGetdentsRequest(string p_Path)
        {
            if (p_Path.Length > FileExplorerExtensions.c_MaxPathLength)
                Length = FileExplorerExtensions.c_MaxPathLength;
            else
                Length = (ushort)p_Path.Length;

            Path = Encoding.ASCII.GetBytes(p_Path.PadRight(FileExplorerExtensions.c_MaxPathLength, '\0'));
        }

        public override void Deserialize(BinaryReader p_Reader)
        {
            Length = p_Reader.ReadUInt16();
            Path = p_Reader.ReadBytes(FileExplorerExtensions.c_MaxPathLength);
        }

        public override byte[] Serialize()
        {
            using (var s_Writer = new BinaryWriter(new MemoryStream()))
            {
                s_Writer.Write(Length);
                s_Writer.Write(Path);

                return ((MemoryStream)s_Writer.BaseStream).ToArray();
            }
        }
    }

    public class FileExplorerGetdentsResponse : MessageSerializable
    {
        public ulong DentIndex;
        public FileExplorerDent Dent;

        public FileExplorerGetdentsResponse()
        {
            DentIndex = 0;
            Dent = new FileExplorerDent();
        }

        public override void Deserialize(BinaryReader p_Reader)
        {
            DentIndex = p_Reader.ReadUInt64();
            Dent = new FileExplorerDent(p_Reader);
        }

        public override byte[] Serialize()
        {
            using (var s_Writer = new BinaryWriter(new MemoryStream()))
            {
                s_Writer.Write(DentIndex);
                s_Writer.Write(Dent.Serialize());

                return ((MemoryStream)s_Writer.BaseStream).ToArray();
            }
        }
    }
}
