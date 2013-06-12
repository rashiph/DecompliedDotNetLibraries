namespace System.Runtime.Serialization.Formatters.Binary
{
    using System;
    using System.Diagnostics;
    using System.Security;

    internal sealed class BinaryObjectString : IStreamable
    {
        internal int objectId;
        internal string value;

        internal BinaryObjectString()
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
            this.value = input.ReadString();
        }

        internal void Set(int objectId, string value)
        {
            this.objectId = objectId;
            this.value = value;
        }

        public void Write(__BinaryWriter sout)
        {
            sout.WriteByte(6);
            sout.WriteInt32(this.objectId);
            sout.WriteString(this.value);
        }
    }
}

