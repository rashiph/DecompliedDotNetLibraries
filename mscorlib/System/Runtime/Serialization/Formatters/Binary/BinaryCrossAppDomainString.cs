namespace System.Runtime.Serialization.Formatters.Binary
{
    using System;
    using System.Diagnostics;
    using System.Security;

    internal sealed class BinaryCrossAppDomainString : IStreamable
    {
        internal int objectId;
        internal int value;

        internal BinaryCrossAppDomainString()
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
            this.value = input.ReadInt32();
        }

        public void Write(__BinaryWriter sout)
        {
            sout.WriteByte(0x13);
            sout.WriteInt32(this.objectId);
            sout.WriteInt32(this.value);
        }
    }
}

