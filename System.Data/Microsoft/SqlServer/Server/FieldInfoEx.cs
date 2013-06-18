namespace Microsoft.SqlServer.Server
{
    using System;
    using System.Reflection;

    internal sealed class FieldInfoEx : IComparable
    {
        internal readonly FieldInfo fieldInfo;
        internal readonly Normalizer normalizer;
        internal readonly int offset;

        internal FieldInfoEx(FieldInfo fi, int offset, Normalizer normalizer)
        {
            this.fieldInfo = fi;
            this.offset = offset;
            this.normalizer = normalizer;
        }

        public int CompareTo(object other)
        {
            FieldInfoEx ex = other as FieldInfoEx;
            if (ex == null)
            {
                return -1;
            }
            return this.offset.CompareTo(ex.offset);
        }
    }
}

