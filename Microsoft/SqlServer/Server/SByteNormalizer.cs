namespace Microsoft.SqlServer.Server
{
    using System;
    using System.IO;
    using System.Reflection;

    internal sealed class SByteNormalizer : Normalizer
    {
        internal override void DeNormalize(FieldInfo fi, object recvr, Stream s)
        {
            byte num = (byte) s.ReadByte();
            if (!base.m_skipNormalize)
            {
                num = (byte) (num ^ 0x80);
            }
            sbyte num2 = (sbyte) num;
            base.SetValue(fi, recvr, num2);
        }

        internal override void Normalize(FieldInfo fi, object obj, Stream s)
        {
            sbyte num2 = (sbyte) base.GetValue(fi, obj);
            byte num = (byte) num2;
            if (!base.m_skipNormalize)
            {
                num = (byte) (num ^ 0x80);
            }
            s.WriteByte(num);
        }

        internal override int Size
        {
            get
            {
                return 1;
            }
        }
    }
}

