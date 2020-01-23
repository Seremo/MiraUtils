using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MiraCore.Client.LogViewer
{
	// Token: 0x02000027 RID: 39
	public static class LogManagerExtensions
	{
		// Token: 0x060000A1 RID: 161 RVA: 0x00006304 File Offset: 0x00004504
		public static byte GetLogChar(this MiraConnection p_Connection)
		{
			bool flag = p_Connection == null;
            byte result;
			if (flag)
			{
				result = 0x0;
			}
			else
			{
                Message s_Request = new Message(MessageHeader.MessageCategory.Log, 600774213u, true, new LogRequest().Serialize());
                p_Connection.SendMessage(s_Request);
                ValueTuple<Message, LogResponse> expr_58 = p_Connection.RecvMessage<LogResponse>(s_Request);
                bool flag3 = expr_58.Item2 == null;
                if (!flag3)
                {
                    result = expr_58.Item2.Data;
                    return result;
                }
                result = 0x0;
                return result;
            }
			return result;
		}
		// Token: 0x040000A0 RID: 160
		public const int c_MaxBufferLength = 4096;

		// Token: 0x040000A1 RID: 161
		public const int c_MaxPathLength = 1024;

		// Token: 0x040000A2 RID: 162
		public const int c_MaxNameLength = 255;

		// Token: 0x02000029 RID: 41
		public enum OpenOnlyFlags
		{
			// Token: 0x040000AC RID: 172
			O_RDONLY,
			// Token: 0x040000AD RID: 173
			O_WRONLY,
			// Token: 0x040000AE RID: 174
			O_RDWR,
			// Token: 0x040000AF RID: 175
			O_ACCMODE
		}
	}
}
