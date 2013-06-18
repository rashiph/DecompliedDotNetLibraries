namespace Microsoft.SqlServer.Server
{
    using System;
    using System.IO;
    using System.Reflection;

    internal sealed class FloatNormalizer : Normalizer
    {
        internal override void DeNormalize(FieldInfo fi, object recvr, Stream s)
        {
            byte[] buffer = new byte[4];
            s.Read(buffer, 0, buffer.Length);
            if (!base.m_skipNormalize)
            {
                if ((buffer[0] & 0x80) > 0)
                {
                    buffer[0] = (byte) (buffer[0] ^ 0x80);
                }
                else
                {
                    base.FlipAllBits(buffer);
                }
                Array.Reverse(buffer);
            }
            base.SetValue(fi, recvr, BitConverter.ToSingle(buffer, 0));
        }

        internal override void Normalize(FieldInfo fi, object obj, Stream s)
        {
            float num = (float) base.GetValue(fi, obj);
            byte[] bytes = BitConverter.GetBytes(num);
            if (!base.m_skipNormalize)
            {
                Array.Reverse(bytes);
                if ((bytes[0] & 0x80) == 0)
                {
                    bytes[0] = (byte) (bytes[0] ^ 0x80);
                }
                else if (num < 0f)
                {
                    base.FlipAllBits(bytes);
                }
            }
            s.Write(bytes, 0, bytes.Length);
        }

        internal override int Size
        {
            get
            {
                return 4;
            }
        }
    }
}

