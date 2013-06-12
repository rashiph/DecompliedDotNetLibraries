namespace System.Text
{
    using System;
    using System.Security;

    [Serializable]
    public sealed class EncodingInfo
    {
        private int iCodePage;
        private string strDisplayName;
        private string strEncodingName;

        internal EncodingInfo(int codePage, string name, string displayName)
        {
            this.iCodePage = codePage;
            this.strEncodingName = name;
            this.strDisplayName = displayName;
        }

        public override bool Equals(object value)
        {
            EncodingInfo info = value as EncodingInfo;
            return ((info != null) && (this.CodePage == info.CodePage));
        }

        [SecuritySafeCritical]
        public Encoding GetEncoding()
        {
            return Encoding.GetEncoding(this.iCodePage);
        }

        public override int GetHashCode()
        {
            return this.CodePage;
        }

        public int CodePage
        {
            get
            {
                return this.iCodePage;
            }
        }

        public string DisplayName
        {
            get
            {
                return this.strDisplayName;
            }
        }

        public string Name
        {
            get
            {
                return this.strEncodingName;
            }
        }
    }
}

