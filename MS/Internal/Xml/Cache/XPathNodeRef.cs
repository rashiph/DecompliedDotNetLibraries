namespace MS.Internal.Xml.Cache
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct XPathNodeRef
    {
        private XPathNode[] page;
        private int idx;
        public static XPathNodeRef Null
        {
            get
            {
                return new XPathNodeRef();
            }
        }
        public XPathNodeRef(XPathNode[] page, int idx)
        {
            this.page = page;
            this.idx = idx;
        }

        public bool IsNull
        {
            get
            {
                return (this.page == null);
            }
        }
        public XPathNode[] Page
        {
            get
            {
                return this.page;
            }
        }
        public int Index
        {
            get
            {
                return this.idx;
            }
        }
        public override int GetHashCode()
        {
            return XPathNodeHelper.GetLocation(this.page, this.idx);
        }
    }
}

