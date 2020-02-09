using System;
using static MiraCore.Client.MessageHeader;

namespace MiraCore.Client.OrbisUtils
{
    public static class UtilitiesExtensions
    {
        public static bool Reboot(this MiraConnection p_Connection)
        {
            throw new NotImplementedException();
            //if (p_Connection == null)
            //    return false;

            //var s_RequestMessage = new Message(
            //   MessageCategory.File,
            //   (uint)OrbisUtilsCommands.OrbisUtils_ShutdownMira,
            //   true,
            //   new OrbisUtilsShutdownMiraRequest
            //   {
            //       RebootConsole = true,
            //       ShutdownMira = false,
            //   }.Serialize());

            //return p_Connection.SendMessage(s_RequestMessage);
        }
    }
}
