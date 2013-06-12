namespace Microsoft.SqlServer.Server
{
    using System;
    using System.Data.SqlTypes;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    internal sealed class BinaryOrderedUdtNormalizer : Normalizer
    {
        internal readonly FieldInfoEx[] m_fieldsToNormalize;
        private bool m_isTopLevelUdt;
        private byte[] m_PadBuffer;
        private int m_size;
        internal readonly object NullInstance;

        internal BinaryOrderedUdtNormalizer(Type t, bool isTopLevelUdt)
        {
            base.m_skipNormalize = false;
            if (base.m_skipNormalize)
            {
                this.m_isTopLevelUdt = true;
            }
            this.m_isTopLevelUdt = true;
            FieldInfo[] fields = this.GetFields(t);
            this.m_fieldsToNormalize = new FieldInfoEx[fields.Length];
            int num2 = 0;
            foreach (FieldInfo info in fields)
            {
                int offset = Marshal.OffsetOf(info.DeclaringType, info.Name).ToInt32();
                this.m_fieldsToNormalize[num2++] = new FieldInfoEx(info, offset, Normalizer.GetNormalizer(info.FieldType));
            }
            Array.Sort<FieldInfoEx>(this.m_fieldsToNormalize);
            if (!this.m_isTopLevelUdt && typeof(INullable).IsAssignableFrom(t))
            {
                PropertyInfo property = t.GetProperty("Null", BindingFlags.Public | BindingFlags.Static);
                if ((property == null) || (property.PropertyType != t))
                {
                    FieldInfo field = t.GetField("Null", BindingFlags.Public | BindingFlags.Static);
                    if ((field == null) || (field.FieldType != t))
                    {
                        throw new Exception("could not find Null field/property in nullable type " + t);
                    }
                    this.NullInstance = field.GetValue(null);
                }
                else
                {
                    this.NullInstance = property.GetValue(null, null);
                }
                this.m_PadBuffer = new byte[this.Size - 1];
            }
        }

        internal override void DeNormalize(FieldInfo fi, object recvr, Stream s)
        {
            base.SetValue(fi, recvr, this.DeNormalizeInternal(fi.FieldType, s));
        }

        private object DeNormalizeInternal(Type t, Stream s)
        {
            object recvr = null;
            if ((!this.m_isTopLevelUdt && typeof(INullable).IsAssignableFrom(t)) && (((byte) s.ReadByte()) == 0))
            {
                recvr = this.NullInstance;
                s.Read(this.m_PadBuffer, 0, this.m_PadBuffer.Length);
                return recvr;
            }
            if (recvr == null)
            {
                recvr = Activator.CreateInstance(t);
            }
            foreach (FieldInfoEx ex in this.m_fieldsToNormalize)
            {
                ex.normalizer.DeNormalize(ex.fieldInfo, recvr, s);
            }
            return recvr;
        }

        internal object DeNormalizeTopObject(Type t, Stream s)
        {
            return this.DeNormalizeInternal(t, s);
        }

        [ReflectionPermission(SecurityAction.Assert, MemberAccess=true)]
        private FieldInfo[] GetFields(Type t)
        {
            return t.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        }

        internal override void Normalize(FieldInfo fi, object obj, Stream s)
        {
            object obj2;
            if (fi == null)
            {
                obj2 = obj;
            }
            else
            {
                obj2 = base.GetValue(fi, obj);
            }
            INullable nullable = obj2 as INullable;
            if ((nullable != null) && !this.m_isTopLevelUdt)
            {
                if (nullable.IsNull)
                {
                    s.WriteByte(0);
                    s.Write(this.m_PadBuffer, 0, this.m_PadBuffer.Length);
                    return;
                }
                s.WriteByte(1);
            }
            foreach (FieldInfoEx ex in this.m_fieldsToNormalize)
            {
                ex.normalizer.Normalize(ex.fieldInfo, obj2, s);
            }
        }

        internal void NormalizeTopObject(object udt, Stream s)
        {
            this.Normalize(null, udt, s);
        }

        internal bool IsNullable
        {
            get
            {
                return (this.NullInstance != null);
            }
        }

        internal override int Size
        {
            get
            {
                if (this.m_size == 0)
                {
                    if (this.IsNullable && !this.m_isTopLevelUdt)
                    {
                        this.m_size = 1;
                    }
                    foreach (FieldInfoEx ex in this.m_fieldsToNormalize)
                    {
                        this.m_size += ex.normalizer.Size;
                    }
                }
                return this.m_size;
            }
        }
    }
}

