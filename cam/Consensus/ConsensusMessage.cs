using Cam.IO;
using Cam.IO.Caching;
using System;
using System.IO;

namespace Cam.Consensus
{
    internal abstract class ConsensusMessage : ISerializable
    {



        private static ReflectionCache<byte> ReflectionCache = ReflectionCache<byte>.CreateFromEnum<ConsensusMessageType>();

        public readonly ConsensusMessageType Type;
        public byte ViewNumber;

        public int Size => sizeof(ConsensusMessageType) + sizeof(byte);

        protected ConsensusMessage(ConsensusMessageType type)
        {
            this.Type = type;
        }

        public virtual void Deserialize(BinaryReader reader)
        {
            if (Type != (ConsensusMessageType)reader.ReadByte())
                throw new FormatException();
            ViewNumber = reader.ReadByte();
        }

        public static ConsensusMessage DeserializeFrom(byte[] data)
        {
            ConsensusMessage message = ReflectionCache.CreateInstance<ConsensusMessage>(data[0]);
            if (message == null) throw new FormatException();

            using (MemoryStream ms = new MemoryStream(data, false))
            using (BinaryReader r = new BinaryReader(ms))
            {
                message.Deserialize(r);
            }
            return message;
        }

        public virtual void Serialize(BinaryWriter writer)
        {
            writer.Write((byte)Type);
            writer.Write(ViewNumber);
        }
    }
}
