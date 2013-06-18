namespace Microsoft.SqlServer.Server
{
    using System;
    using System.IO;
    using System.Reflection;

    internal sealed class BooleanNormalizer : Normalizer
    {
        internal override void DeNormalize(FieldInfo fi, object recvr, Stream s)
        {
            byte num = (byte) s.ReadByte();
            base.SetValue(fi, recvr, num == 1);
        }

        internal override void Normalize(FieldInfo fi, object obj, Stream s)
        {
            bool flag = (bool) base.GetValue(fi, obj);
            s.WriteByte(flag ? ((byte) 1) : ((byte) 0));
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

