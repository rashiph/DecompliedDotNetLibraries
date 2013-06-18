namespace Microsoft.SqlServer.Server
{
    using System;
    using System.Data;
    using System.IO;
    using System.Reflection;
    using System.Security.Permissions;

    internal abstract class Normalizer
    {
        protected bool m_skipNormalize;

        protected Normalizer()
        {
        }

        internal abstract void DeNormalize(FieldInfo fi, object recvr, Stream s);
        protected void FlipAllBits(byte[] b)
        {
            for (int i = 0; i < b.Length; i++)
            {
                b[i] = ~b[i];
            }
        }

        internal static Normalizer GetNormalizer(Type t)
        {
            Normalizer normalizer = null;
            if (t.IsPrimitive)
            {
                if (t == typeof(byte))
                {
                    normalizer = new ByteNormalizer();
                }
                else if (t == typeof(sbyte))
                {
                    normalizer = new SByteNormalizer();
                }
                else if (t == typeof(bool))
                {
                    normalizer = new BooleanNormalizer();
                }
                else if (t == typeof(short))
                {
                    normalizer = new ShortNormalizer();
                }
                else if (t == typeof(ushort))
                {
                    normalizer = new UShortNormalizer();
                }
                else if (t == typeof(int))
                {
                    normalizer = new IntNormalizer();
                }
                else if (t == typeof(uint))
                {
                    normalizer = new UIntNormalizer();
                }
                else if (t == typeof(float))
                {
                    normalizer = new FloatNormalizer();
                }
                else if (t == typeof(double))
                {
                    normalizer = new DoubleNormalizer();
                }
                else if (t == typeof(long))
                {
                    normalizer = new LongNormalizer();
                }
                else if (t == typeof(ulong))
                {
                    normalizer = new ULongNormalizer();
                }
            }
            else if (t.IsValueType)
            {
                normalizer = new BinaryOrderedUdtNormalizer(t, false);
            }
            if (normalizer == null)
            {
                throw new Exception(Res.GetString("Sql_CanotCreateNormalizer", new object[] { t.FullName }));
            }
            normalizer.m_skipNormalize = false;
            return normalizer;
        }

        [ReflectionPermission(SecurityAction.Assert, MemberAccess=true)]
        protected object GetValue(FieldInfo fi, object obj)
        {
            return fi.GetValue(obj);
        }

        internal abstract void Normalize(FieldInfo fi, object recvr, Stream s);
        [ReflectionPermission(SecurityAction.Assert, MemberAccess=true)]
        protected void SetValue(FieldInfo fi, object recvr, object value)
        {
            fi.SetValue(recvr, value);
        }

        internal abstract int Size { get; }
    }
}

