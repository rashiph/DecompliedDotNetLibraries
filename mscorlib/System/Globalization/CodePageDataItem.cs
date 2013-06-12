namespace System.Globalization
{
    using System;
    using System.Security;

    [Serializable]
    internal class CodePageDataItem
    {
        internal string m_bodyName;
        internal int m_dataIndex;
        internal uint m_flags;
        internal string m_headerName;
        internal int m_uiFamilyCodePage;
        internal string m_webName;

        [SecuritySafeCritical]
        internal unsafe CodePageDataItem(int dataIndex)
        {
            this.m_dataIndex = dataIndex;
            this.m_uiFamilyCodePage = EncodingTable.codePageDataPtr[dataIndex].uiFamilyCodePage;
            this.m_flags = EncodingTable.codePageDataPtr[dataIndex].flags;
        }

        public virtual string BodyName
        {
            [SecuritySafeCritical]
            get
            {
                if (this.m_bodyName == null)
                {
                    this.m_bodyName = new string(EncodingTable.codePageDataPtr[this.m_dataIndex].bodyName);
                }
                return this.m_bodyName;
            }
        }

        public virtual uint Flags
        {
            get
            {
                return this.m_flags;
            }
        }

        public virtual string HeaderName
        {
            [SecuritySafeCritical]
            get
            {
                if (this.m_headerName == null)
                {
                    this.m_headerName = new string(EncodingTable.codePageDataPtr[this.m_dataIndex].headerName);
                }
                return this.m_headerName;
            }
        }

        public virtual int UIFamilyCodePage
        {
            get
            {
                return this.m_uiFamilyCodePage;
            }
        }

        public virtual string WebName
        {
            [SecuritySafeCritical]
            get
            {
                if (this.m_webName == null)
                {
                    this.m_webName = new string(EncodingTable.codePageDataPtr[this.m_dataIndex].webName);
                }
                return this.m_webName;
            }
        }
    }
}

