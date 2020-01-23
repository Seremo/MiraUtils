using System;
using System.IO;
using System.Text;

namespace MiraCore.Client.LogViewer
{
    // Token: 0x02000011 RID: 17
    public class LogRequest : MessageSerializable
    {
        // Token: 0x0600005C RID: 92 RVA: 0x00004CD8 File Offset: 0x00002ED8
        public LogRequest()
        {
            this.Length = 1;
        }

        // Token: 0x0600005D RID: 93 RVA: 0x00004D64 File Offset: 0x00002F64
        public override void Deserialize(BinaryReader p_Reader)
        {
            this.Length = p_Reader.ReadUInt16();
        }

        // Token: 0x0600005E RID: 94 RVA: 0x00004D84 File Offset: 0x00002F84
        public override byte[] Serialize()
        {
            byte[] result;
            using (BinaryWriter s_Writer = new BinaryWriter(new MemoryStream()))
            {
                s_Writer.Write(this.Length);
                result = ((MemoryStream)s_Writer.BaseStream).ToArray();
            }
            return result;
        }

        // Token: 0x0400005E RID: 94
        public ushort Length;
    }
}
