using System.IO;

namespace MiraCore.Client
{
    public abstract class MessageSerializable
    {
        public abstract byte[] Serialize();

        public abstract void Deserialize(BinaryReader p_Reader);
    }
}
