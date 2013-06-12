namespace System.Runtime.Serialization.Formatters.Binary
{
    using System;
    using System.Diagnostics;
    using System.Security;

    internal sealed class BinaryCrossAppDomainMap : IStreamable
    {
        internal int crossAppDomainArrayIndex;

        internal BinaryCrossAppDomainMap()
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
            this.crossAppDomainArrayIndex = input.ReadInt32();
        }

        public void Write(__BinaryWriter sout)
        {
            sout.WriteByte(0x12);
            sout.WriteInt32(this.crossAppDomainArrayIndex);
        }
    }
}

