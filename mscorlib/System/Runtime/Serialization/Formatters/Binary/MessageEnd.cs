namespace System.Runtime.Serialization.Formatters.Binary
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Security;

    internal sealed class MessageEnd : IStreamable
    {
        internal MessageEnd()
        {
        }

        public void Dump()
        {
        }

        public void Dump(Stream sout)
        {
        }

        [Conditional("_LOGGING")]
        private void DumpInternal(Stream sout)
        {
            if ((BCLDebug.CheckEnabled("BINARY") && (sout != null)) && sout.CanSeek)
            {
                long length = sout.Length;
            }
        }

        [SecurityCritical]
        public void Read(__BinaryParser input)
        {
        }

        public void Write(__BinaryWriter sout)
        {
            sout.WriteByte(11);
        }
    }
}

