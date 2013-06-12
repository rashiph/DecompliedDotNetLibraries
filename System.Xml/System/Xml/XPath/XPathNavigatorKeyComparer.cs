namespace System.Xml.XPath
{
    using MS.Internal.Xml.Cache;
    using System;
    using System.Collections;

    internal class XPathNavigatorKeyComparer : IEqualityComparer
    {
        bool IEqualityComparer.Equals(object obj1, object obj2)
        {
            XPathNavigator navigator = obj1 as XPathNavigator;
            XPathNavigator other = obj2 as XPathNavigator;
            return (((navigator != null) && (other != null)) && navigator.IsSamePosition(other));
        }

        int IEqualityComparer.GetHashCode(object obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }
            XPathDocumentNavigator navigator2 = obj as XPathDocumentNavigator;
            if (navigator2 != null)
            {
                return navigator2.GetPositionHashCode();
            }
            XPathNavigator navigator = obj as XPathNavigator;
            if (navigator != null)
            {
                object underlyingObject = navigator.UnderlyingObject;
                if (underlyingObject != null)
                {
                    return underlyingObject.GetHashCode();
                }
                int num = ((int) navigator.NodeType) ^ navigator.LocalName.GetHashCode();
                num ^= navigator.Prefix.GetHashCode();
                return (num ^ navigator.NamespaceURI.GetHashCode());
            }
            return obj.GetHashCode();
        }
    }
}

