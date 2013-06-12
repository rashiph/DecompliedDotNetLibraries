namespace System.Collections
{
    using System;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;

    [Serializable, ComVisible(true)]
    public sealed class Comparer : IComparer, ISerializable
    {
        private const string CompareInfoName = "CompareInfo";
        public static readonly Comparer Default = new Comparer(CultureInfo.CurrentCulture);
        public static readonly Comparer DefaultInvariant = new Comparer(CultureInfo.InvariantCulture);
        private CompareInfo m_compareInfo;

        private Comparer()
        {
            this.m_compareInfo = null;
        }

        public Comparer(CultureInfo culture)
        {
            if (culture == null)
            {
                throw new ArgumentNullException("culture");
            }
            this.m_compareInfo = culture.CompareInfo;
        }

        private Comparer(SerializationInfo info, StreamingContext context)
        {
            this.m_compareInfo = null;
            SerializationInfoEnumerator enumerator = info.GetEnumerator();
            while (enumerator.MoveNext())
            {
                string str;
                if (((str = enumerator.Name) != null) && (str == "CompareInfo"))
                {
                    this.m_compareInfo = (CompareInfo) info.GetValue("CompareInfo", typeof(CompareInfo));
                }
            }
        }

        public int Compare(object a, object b)
        {
            if (a == b)
            {
                return 0;
            }
            if (a == null)
            {
                return -1;
            }
            if (b == null)
            {
                return 1;
            }
            if (this.m_compareInfo != null)
            {
                string str = a as string;
                string str2 = b as string;
                if ((str != null) && (str2 != null))
                {
                    return this.m_compareInfo.Compare(str, str2);
                }
            }
            IComparable comparable = a as IComparable;
            if (comparable == null)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_ImplementIComparable"));
            }
            return comparable.CompareTo(b);
        }

        [SecurityCritical]
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            if (this.m_compareInfo != null)
            {
                info.AddValue("CompareInfo", this.m_compareInfo);
            }
        }
    }
}

