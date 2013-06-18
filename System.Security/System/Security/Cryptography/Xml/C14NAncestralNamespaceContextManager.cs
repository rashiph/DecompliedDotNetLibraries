namespace System.Security.Cryptography.Xml
{
    using System;
    using System.Collections;
    using System.Xml;

    internal class C14NAncestralNamespaceContextManager : AncestralNamespaceContextManager
    {
        internal C14NAncestralNamespaceContextManager()
        {
        }

        internal override void GetNamespacesToRender(XmlElement element, SortedList attrListToRender, SortedList nsListToRender, Hashtable nsLocallyDeclared)
        {
            XmlAttribute a = null;
            object[] array = new object[nsLocallyDeclared.Count];
            nsLocallyDeclared.Values.CopyTo(array, 0);
            foreach (object obj2 in array)
            {
                int num;
                a = (XmlAttribute) obj2;
                XmlAttribute nearestRenderedNamespaceWithMatchingPrefix = base.GetNearestRenderedNamespaceWithMatchingPrefix(Utils.GetNamespacePrefix(a), out num);
                if (Utils.IsNonRedundantNamespaceDecl(a, nearestRenderedNamespaceWithMatchingPrefix))
                {
                    nsLocallyDeclared.Remove(Utils.GetNamespacePrefix(a));
                    if (Utils.IsXmlNamespaceNode(a))
                    {
                        attrListToRender.Add(a, null);
                    }
                    else
                    {
                        nsListToRender.Add(a, null);
                    }
                }
            }
            for (int i = base.m_ancestorStack.Count - 1; i >= 0; i--)
            {
                foreach (object obj3 in base.GetScopeAt(i).GetUnrendered().Values)
                {
                    a = (XmlAttribute) obj3;
                    if (a != null)
                    {
                        this.GetNamespaceToRender(Utils.GetNamespacePrefix(a), attrListToRender, nsListToRender, nsLocallyDeclared);
                    }
                }
            }
        }

        private void GetNamespaceToRender(string nsPrefix, SortedList attrListToRender, SortedList nsListToRender, Hashtable nsLocallyDeclared)
        {
            int num;
            foreach (object obj2 in nsListToRender.GetKeyList())
            {
                if (Utils.HasNamespacePrefix((XmlAttribute) obj2, nsPrefix))
                {
                    return;
                }
            }
            foreach (object obj3 in attrListToRender.GetKeyList())
            {
                if (((XmlAttribute) obj3).LocalName.Equals(nsPrefix))
                {
                    return;
                }
            }
            XmlAttribute a = (XmlAttribute) nsLocallyDeclared[nsPrefix];
            XmlAttribute nearestRenderedNamespaceWithMatchingPrefix = base.GetNearestRenderedNamespaceWithMatchingPrefix(nsPrefix, out num);
            if (a != null)
            {
                if (Utils.IsNonRedundantNamespaceDecl(a, nearestRenderedNamespaceWithMatchingPrefix))
                {
                    nsLocallyDeclared.Remove(nsPrefix);
                    if (Utils.IsXmlNamespaceNode(a))
                    {
                        attrListToRender.Add(a, null);
                    }
                    else
                    {
                        nsListToRender.Add(a, null);
                    }
                }
            }
            else
            {
                int num2;
                XmlAttribute nearestUnrenderedNamespaceWithMatchingPrefix = base.GetNearestUnrenderedNamespaceWithMatchingPrefix(nsPrefix, out num2);
                if (((nearestUnrenderedNamespaceWithMatchingPrefix != null) && (num2 > num)) && Utils.IsNonRedundantNamespaceDecl(nearestUnrenderedNamespaceWithMatchingPrefix, nearestRenderedNamespaceWithMatchingPrefix))
                {
                    if (Utils.IsXmlNamespaceNode(nearestUnrenderedNamespaceWithMatchingPrefix))
                    {
                        attrListToRender.Add(nearestUnrenderedNamespaceWithMatchingPrefix, null);
                    }
                    else
                    {
                        nsListToRender.Add(nearestUnrenderedNamespaceWithMatchingPrefix, null);
                    }
                }
            }
        }

        internal override void TrackNamespaceNode(XmlAttribute attr, SortedList nsListToRender, Hashtable nsLocallyDeclared)
        {
            nsLocallyDeclared.Add(Utils.GetNamespacePrefix(attr), attr);
        }

        internal override void TrackXmlNamespaceNode(XmlAttribute attr, SortedList nsListToRender, SortedList attrListToRender, Hashtable nsLocallyDeclared)
        {
            nsLocallyDeclared.Add(Utils.GetNamespacePrefix(attr), attr);
        }
    }
}

