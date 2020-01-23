using System;
using System.IO;
using System.Text;

namespace MiraCore.Client.LogViewer
{
    // Token: 0x02000011 RID: 17
    public class LogResponse : MessageSerializable
    {
        // Token: 0x0600005F RID: 95 RVA: 0x00004DE8 File Offset: 0x00002FE8
        public override void Deserialize(BinaryReader p_Reader)
        {
            this.Data = p_Reader.ReadByte();
        }

        // Token: 0x06000060 RID: 96 RVA: 0x00004E20 File Offset: 0x00003020
        public override byte[] Serialize()
        {
            byte[] result;
            using (BinaryWriter s_Writer = new BinaryWriter(new MemoryStream()))
            {
                s_Writer.Write(this.Data);
                result = ((MemoryStream)s_Writer.BaseStream).ToArray();
            }
            return result;
        }
        // Token: 0x04000063 RID: 99
        public byte Data;
    }
}
