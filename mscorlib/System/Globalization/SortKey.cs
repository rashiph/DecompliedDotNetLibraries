namespace System.Globalization
{
    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;

    [Serializable, ComVisible(true)]
    public class SortKey
    {
        [OptionalField(VersionAdded=3)]
        internal string localeName;
        internal byte[] m_KeyData;
        internal string m_String;
        internal CompareOptions options;
        [OptionalField(VersionAdded=1)]
        internal int win32LCID;

        internal SortKey(string localeName, string str, CompareOptions options, byte[] keyData)
        {
            this.m_KeyData = keyData;
            this.localeName = localeName;
            this.options = options;
            this.m_String = str;
        }

        public static int Compare(SortKey sortkey1, SortKey sortkey2)
        {
            if ((sortkey1 == null) || (sortkey2 == null))
            {
                throw new ArgumentNullException((sortkey1 == null) ? "sortkey1" : "sortkey2");
            }
            byte[] keyData = sortkey1.m_KeyData;
            byte[] buffer2 = sortkey2.m_KeyData;
            if (keyData.Length == 0)
            {
                if (buffer2.Length == 0)
                {
                    return 0;
                }
                return -1;
            }
            if (buffer2.Length == 0)
            {
                return 1;
            }
            int num = (keyData.Length < buffer2.Length) ? keyData.Length : buffer2.Length;
            for (int i = 0; i < num; i++)
            {
                if (keyData[i] > buffer2[i])
                {
                    return 1;
                }
                if (keyData[i] < buffer2[i])
                {
                    return -1;
                }
            }
            return 0;
        }

        public override bool Equals(object value)
        {
            SortKey key = value as SortKey;
            return ((key != null) && (Compare(this, key) == 0));
        }

        public override int GetHashCode()
        {
            return CompareInfo.GetCompareInfo(this.localeName).GetHashCodeOfString(this.m_String, this.options);
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            if (string.IsNullOrEmpty(this.localeName) && (this.win32LCID != 0))
            {
                this.localeName = CultureInfo.GetCultureInfo(this.win32LCID).Name;
            }
        }

        [OnSerializing]
        private void OnSerializing(StreamingContext context)
        {
            if (this.win32LCID == 0)
            {
                this.win32LCID = CultureInfo.GetCultureInfo(this.localeName).LCID;
            }
        }

        public override string ToString()
        {
            return string.Concat(new object[] { "SortKey - ", this.localeName, ", ", this.options, ", ", this.m_String });
        }

        public virtual byte[] KeyData
        {
            get
            {
                return (byte[]) this.m_KeyData.Clone();
            }
        }

        public virtual string OriginalString
        {
            get
            {
                return this.m_String;
            }
        }
    }
}

