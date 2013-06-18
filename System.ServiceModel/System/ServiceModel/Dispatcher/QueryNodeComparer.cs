namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Collections.Generic;
    using System.ServiceModel;
    using System.Xml;
    using System.Xml.XPath;

    internal class QueryNodeComparer : IComparer<QueryNode>
    {
        public int Compare(QueryNode item1, QueryNode item2)
        {
            switch (item1.Node.ComparePosition(item1.Position, item2.Position))
            {
                case XmlNodeOrder.Before:
                    return -1;

                case XmlNodeOrder.After:
                    return 1;

                case XmlNodeOrder.Same:
                    return 0;
            }
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperCritical(new XPathException(System.ServiceModel.SR.GetString("QueryNotSortable")));
        }

        public bool Equals(QueryNode item1, QueryNode item2)
        {
            return (this.Compare(item1, item2) == 0);
        }

        public int GetHashCode(QueryNode item)
        {
            return item.GetHashCode();
        }
    }
}

