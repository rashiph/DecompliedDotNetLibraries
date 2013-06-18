namespace System.Security.Cryptography.Xml
{
    using System;
    using System.Collections;
    using System.Runtime.InteropServices;
    using System.Xml;

    internal abstract class AncestralNamespaceContextManager
    {
        internal ArrayList m_ancestorStack = new ArrayList();

        protected AncestralNamespaceContextManager()
        {
        }

        internal void AddRendered(XmlAttribute attr)
        {
            this.GetCurrentScope().AddRendered(attr);
        }

        internal void AddUnrendered(XmlAttribute attr)
        {
            this.GetCurrentScope().AddUnrendered(attr);
        }

        internal void EnterElementContext()
        {
            this.m_ancestorStack.Add(new NamespaceFrame());
        }

        internal void ExitElementContext()
        {
            this.m_ancestorStack.RemoveAt(this.m_ancestorStack.Count - 1);
        }

        internal NamespaceFrame GetCurrentScope()
        {
            return this.GetScopeAt(this.m_ancestorStack.Count - 1);
        }

        internal abstract void GetNamespacesToRender(XmlElement element, SortedList attrListToRender, SortedList nsListToRender, Hashtable nsLocallyDeclared);
        protected XmlAttribute GetNearestRenderedNamespaceWithMatchingPrefix(string nsPrefix, out int depth)
        {
            XmlAttribute rendered = null;
            depth = -1;
            for (int i = this.m_ancestorStack.Count - 1; i >= 0; i--)
            {
                rendered = this.GetScopeAt(i).GetRendered(nsPrefix);
                if (rendered != null)
                {
                    depth = i;
                    return rendered;
                }
            }
            return null;
        }

        protected XmlAttribute GetNearestUnrenderedNamespaceWithMatchingPrefix(string nsPrefix, out int depth)
        {
            XmlAttribute unrendered = null;
            depth = -1;
            for (int i = this.m_ancestorStack.Count - 1; i >= 0; i--)
            {
                unrendered = this.GetScopeAt(i).GetUnrendered(nsPrefix);
                if (unrendered != null)
                {
                    depth = i;
                    return unrendered;
                }
            }
            return null;
        }

        internal NamespaceFrame GetScopeAt(int i)
        {
            return (NamespaceFrame) this.m_ancestorStack[i];
        }

        internal void LoadRenderedNamespaces(SortedList nsRenderedList)
        {
            foreach (object obj2 in nsRenderedList.GetKeyList())
            {
                this.AddRendered((XmlAttribute) obj2);
            }
        }

        internal void LoadUnrenderedNamespaces(Hashtable nsLocallyDeclared)
        {
            object[] array = new object[nsLocallyDeclared.Count];
            nsLocallyDeclared.Values.CopyTo(array, 0);
            foreach (object obj2 in array)
            {
                this.AddUnrendered((XmlAttribute) obj2);
            }
        }

        internal abstract void TrackNamespaceNode(XmlAttribute attr, SortedList nsListToRender, Hashtable nsLocallyDeclared);
        internal abstract void TrackXmlNamespaceNode(XmlAttribute attr, SortedList nsListToRender, SortedList attrListToRender, Hashtable nsLocallyDeclared);
    }
}

