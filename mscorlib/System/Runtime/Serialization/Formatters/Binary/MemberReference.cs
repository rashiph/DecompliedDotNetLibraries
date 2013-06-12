namespace System.Runtime.Serialization.Formatters.Binary
{
    using System;
    using System.Diagnostics;
    using System.Security;

    internal sealed class MemberReference : IStreamable
    {
        internal int idRef;

        internal MemberReference()
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
            this.idRef = input.ReadInt32();
        }

        internal void Set(int idRef)
        {
            this.idRef = idRef;
        }

        public void Write(__BinaryWriter sout)
        {
            sout.WriteByte(9);
            sout.WriteInt32(this.idRef);
        }
    }
}

