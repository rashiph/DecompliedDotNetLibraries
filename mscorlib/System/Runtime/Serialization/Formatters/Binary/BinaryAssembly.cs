namespace System.Runtime.Serialization.Formatters.Binary
{
    using System;
    using System.Diagnostics;
    using System.Security;

    internal sealed class BinaryAssembly : IStreamable
    {
        internal string assemblyString;
        internal int assemId;

        internal BinaryAssembly()
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
            this.assemblyString = input.ReadString();
        }

        internal void Set(int assemId, string assemblyString)
        {
            this.assemId = assemId;
            this.assemblyString = assemblyString;
        }

        public void Write(__BinaryWriter sout)
        {
            sout.WriteByte(12);
            sout.WriteInt32(this.assemId);
            sout.WriteString(this.assemblyString);
        }
    }
}

