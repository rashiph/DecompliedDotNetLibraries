namespace System.Runtime.Serialization.Formatters.Binary
{
    using System;
    using System.Diagnostics;
    using System.Security;

    internal sealed class BinaryObject : IStreamable
    {
        internal int mapId;
        internal int objectId;

        internal BinaryObject()
        {
        }

        public void Dump()
        {
        }

        [Conditional("_LOGGING")]
        private void DumpInternal()
        {
            BCLDebug.CheckEnabled("BINARY");
        }

        [SecurityCritical]
        public void Read(__BinaryParser input)
        {
            this.objectId = input.ReadInt32();
            this.mapId = input.ReadInt32();
        }

        internal void Set(int objectId, int mapId)
        {
            this.objectId = objectId;
            this.mapId = mapId;
        }

        public void Write(__BinaryWriter sout)
        {
            sout.WriteByte(1);
            sout.WriteInt32(this.objectId);
            sout.WriteInt32(this.mapId);
        }
    }
}

