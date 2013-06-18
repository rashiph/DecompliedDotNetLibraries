namespace System.Xml.Linq
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    public sealed class XNodeEqualityComparer : IEqualityComparer, IEqualityComparer<XNode>
    {
        public bool Equals(XNode x, XNode y)
        {
            return XNode.DeepEquals(x, y);
        }

        public int GetHashCode(XNode obj)
        {
            if (obj == null)
            {
                return 0;
            }
            return obj.GetDeepHashCode();
        }

        bool IEqualityComparer.Equals(object x, object y)
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
            return this.Equals(node, node2);
        }

        int IEqualityComparer.GetHashCode(object obj)
        {
            XNode node = obj as XNode;
            if ((node == null) && (obj != null))
            {
                throw new ArgumentException(Res.GetString("Argument_MustBeDerivedFrom", new object[] { typeof(XNode) }), "obj");
            }
            return this.GetHashCode(node);
        }
    }
}

