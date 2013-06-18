namespace System.Xml.Linq
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    public sealed class XNodeDocumentOrderComparer : IComparer, IComparer<XNode>
    {
        public int Compare(XNode x, XNode y)
        {
            return XNode.CompareDocumentOrder(x, y);
        }

        int IComparer.Compare(object x, object y)
        {
            XNode node = x as XNode;
            if ((node == null) && (x != null))
            {
                throw new ArgumentException(Res.GetString("Argument_MustBeDerivedFrom", new object[] { typeof(XNode) }), "x");
            }
            XNode node2 = y as XNode;
            if ((node2 == null) && (y != null))
            {
                throw new ArgumentException(Res.GetString("Argument_MustBeDerivedFrom", new object[] { typeof(XNode) }), "y");
            }
            return this.Compare(node, node2);
        }
    }
}

