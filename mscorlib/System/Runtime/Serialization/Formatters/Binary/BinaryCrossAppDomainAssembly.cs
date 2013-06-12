namespace System.Runtime.Serialization.Formatters.Binary
{
    using System;
    using System.Diagnostics;
    using System.Security;

    internal sealed class BinaryCrossAppDomainAssembly : IStreamable
    {
        internal int assemblyIndex;
        internal int assemId;

        internal BinaryCrossAppDomainAssembly()
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
            this.assemId = input.ReadInt32();
            this.assemblyIndex = input.ReadInt32();
        }

        public void Write(__BinaryWriter sout)
        {
            sout.WriteByte(20);
            sout.WriteInt32(this.assemId);
            sout.WriteInt32(this.assemblyIndex);
        }
    }
}

