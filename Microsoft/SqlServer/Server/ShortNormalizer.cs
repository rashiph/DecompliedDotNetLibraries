namespace Microsoft.SqlServer.Server
{
    using System;
    using System.IO;
    using System.Reflection;

    internal sealed class ShortNormalizer : Normalizer
    {
        internal override void DeNormalize(FieldInfo fi, object recvr, Stream s)
        {
            byte[] buffer = new byte[2];
            s.Read(buffer, 0, buffer.Length);
            if (!base.m_skipNormalize)
            {
                buffer[0] = (byte) (buffer[0] ^ 0x80);
                Array.Reverse(buffer);
            }
            base.SetValue(fi, recvr, BitConverter.ToInt16(buffer, 0));
        }

        internal override void Normalize(FieldInfo fi, object obj, Stream s)
        {
            byte[] bytes = BitConverter.GetBytes((short) base.GetValue(fi, obj));
            if (!base.m_skipNormalize)
            {
                Array.Reverse(bytes);
                bytes[0] = (byte) (bytes[0] ^ 0x80);
            }
            s.Write(bytes, 0, bytes.Length);
        }

        internal override int Size
        {
            get
            {
                return 2;
            }
        }
    }
}

