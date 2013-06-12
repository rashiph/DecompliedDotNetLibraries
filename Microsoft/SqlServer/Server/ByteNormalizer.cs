namespace Microsoft.SqlServer.Server
{
    using System;
    using System.IO;
    using System.Reflection;

    internal sealed class ByteNormalizer : Normalizer
    {
        internal override void DeNormalize(FieldInfo fi, object recvr, Stream s)
        {
            byte num = (byte) s.ReadByte();
            base.SetValue(fi, recvr, num);
        }

        internal override void Normalize(FieldInfo fi, object obj, Stream s)
        {
            byte num = (byte) base.GetValue(fi, obj);
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

