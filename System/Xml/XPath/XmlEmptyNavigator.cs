namespace System.Xml.XPath
{
    using System;
    using System.Xml;

    internal class XmlEmptyNavigator : XPathNavigator
    {
        private static XmlEmptyNavigator singleton;

        private XmlEmptyNavigator()
        {
        }

        public override XPathNavigator Clone()
        {
            return this;
        }

        public override XmlNodeOrder ComparePosition(XPathNavigator other)
        {
            if (this != other)
            {
                return XmlNodeOrder.Unknown;
            }
            return XmlNodeOrder.Same;
        }

        public override string GetAttribute(string localName, string namespaceName)
        {
            return null;
        }

        public override string GetNamespace(string name)
        {
            return null;
        }

        public override bool IsSamePosition(XPathNavigator other)
        {
            return (this == other);
        }

        public override bool MoveTo(XPathNavigator other)
        {
            return (this == other);
        }

        public override bool MoveToAttribute(string localName, string namespaceName)
        {
            return false;
        }

        public override bool MoveToFirst()
        {
            return false;
        }

        public override bool MoveToFirstAttribute()
        {
            return false;
        }

        public override bool MoveToFirstChild()
        {
            return false;
        }

        public override bool MoveToFirstNamespace(XPathNamespaceScope scope)
        {
            return false;
        }

        public override bool MoveToId(string id)
        {
            return false;
        }

        public override bool MoveToNamespace(string prefix)
        {
            return false;
        }

        public override bool MoveToNext()
        {
            return false;
        }

        public override bool MoveToNextAttribute()
        {
            return false;
        }

        public override bool MoveToNextNamespace(XPathNamespaceScope scope)
        {
            return false;
        }

        public override bool MoveToParent()
        {
            return false;
        }

        public override bool MoveToPrevious()
        {
            return false;
        }

        public override void MoveToRoot()
        {
        }

        public override string BaseURI
        {
            get
            {
                return string.Empty;
            }
        }

        public override bool HasAttributes
        {
            get
            {
                return false;
            }
        }

        public override bool HasChildren
        {
            get
            {
                return false;
            }
        }

        public override bool IsEmptyElement
        {
            get
            {
                return false;
            }
        }

        public override string LocalName
        {
            get
            {
                return string.Empty;
            }
        }

        public override string Name
        {
            get
            {
                return string.Empty;
            }
        }

        public override string NamespaceURI
        {
            get
            {
                return string.Empty;
            }
        }

        public override XmlNameTable NameTable
        {
            get
            {
                return new System.Xml.NameTable();
            }
        }

        public override XPathNodeType NodeType
        {
            get
            {
                return XPathNodeType.All;
            }
        }

        public override string Prefix
        {
            get
            {
                return string.Empty;
            }
        }

        public static XmlEmptyNavigator Singleton
        {
            get
            {
                if (singleton == null)
                {
                    singleton = new XmlEmptyNavigator();
                }
                return singleton;
            }
        }

        public override string Value
        {
            get
            {
                return string.Empty;
            }
        }

        public override string XmlLang
        {
            get
            {
                return string.Empty;
            }
        }
    }
}

