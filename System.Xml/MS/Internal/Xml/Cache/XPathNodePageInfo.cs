namespace MS.Internal.Xml.Cache
{
    using System;

    internal sealed class XPathNodePageInfo
    {
        private int nodeCount;
        private XPathNode[] pageNext;
        private int pageNum;
        private XPathNode[] pagePrev;

        public XPathNodePageInfo(XPathNode[] pagePrev, int pageNum)
        {
            this.pagePrev = pagePrev;
            this.pageNum = pageNum;
            this.nodeCount = 1;
        }

        public XPathNode[] NextPage
        {
            get
            {
                return this.pageNext;
            }
            set
            {
                this.pageNext = value;
            }
        }

        public int NodeCount
        {
            get
            {
                return this.nodeCount;
            }
            set
            {
                this.nodeCount = value;
            }
        }

        public int PageNumber
        {
            get
            {
                return this.pageNum;
            }
        }

        public XPathNode[] PreviousPage
        {
            get
            {
                return this.pagePrev;
            }
        }
    }
}

