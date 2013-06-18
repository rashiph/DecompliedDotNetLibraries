namespace System.Security.Cryptography.Xml
{
    using System;
    using System.Collections;
    using System.Xml;

    internal class ExcAncestralNamespaceContextManager : AncestralNamespaceContextManager
    {
        private Hashtable m_inclusivePrefixSet;

        internal ExcAncestralNamespaceContextManager(string inclusiveNamespacesPrefixList)
        {
            this.m_inclusivePrefixSet = Utils.TokenizePrefixListString(inclusiveNamespacesPrefixList);
        }

        private void GatherNamespaceToRender(string nsPrefix, SortedList nsListToRender, Hashtable nsLocallyDeclared)
        {
            int num;
            foreach (object obj2 in nsListToRender.GetKeyList())
            {
                if (Utils.HasNamespacePrefix((XmlAttribute) obj2, nsPrefix))
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
                    nsListToRender.Add(a, null);
                }
            }
            else
            {
                int num2;
                XmlAttribute nearestUnrenderedNamespaceWithMatchingPrefix = base.GetNearestUnrenderedNamespaceWithMatchingPrefix(nsPrefix, out num2);
                if (((nearestUnrenderedNamespaceWithMatchingPrefix != null) && (num2 > num)) && Utils.IsNonRedundantNamespaceDecl(nearestUnrenderedNamespaceWithMatchingPrefix, nearestRenderedNamespaceWithMatchingPrefix))
                {
                    nsListToRender.Add(nearestUnrenderedNamespaceWithMatchingPrefix, null);
                }
            }
        }

        internal override void GetNamespacesToRender(XmlElement element, SortedList attrListToRender, SortedList nsListToRender, Hashtable nsLocallyDeclared)
        {
            this.GatherNamespaceToRender(element.Prefix, nsListToRender, nsLocallyDeclared);
            foreach (object obj2 in attrListToRender.GetKeyList())
            {
                string prefix = ((XmlAttribute) obj2).Prefix;
                if (prefix.Length > 0)
                {
                    this.GatherNamespaceToRender(prefix, nsListToRender, nsLocallyDeclared);
                }
            }
        }

        private bool HasNonRedundantInclusivePrefix(XmlAttribute attr)
        {
            int num;
            string namespacePrefix = Utils.GetNamespacePrefix(attr);
            return (this.m_inclusivePrefixSet.ContainsKey(namespacePrefix) && Utils.IsNonRedundantNamespaceDecl(attr, base.GetNearestRenderedNamespaceWithMatchingPrefix(namespacePrefix, out num)));
        }

        internal override void TrackNamespaceNode(XmlAttribute attr, SortedList nsListToRender, Hashtable nsLocallyDeclared)
        {
            if (!Utils.IsXmlPrefixDefinitionNode(attr))
            {
                if (this.HasNonRedundantInclusivePrefix(attr))
                {
                    nsListToRender.Add(attr, null);
                }
                else
                {
                    nsLocallyDeclared.Add(Utils.GetNamespacePrefix(attr), attr);
                }
            }
        }

        internal override void TrackXmlNamespaceNode(XmlAttribute attr, SortedList nsListToRender, SortedList attrListToRender, Hashtable nsLocallyDeclared)
        {
            attrListToRender.Add(attr, null);
        }
    }
}

