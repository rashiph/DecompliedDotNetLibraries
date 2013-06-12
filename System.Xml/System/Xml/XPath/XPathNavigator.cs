namespace System.Xml.XPath
{
    using MS.Internal.Xml.XPath;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Xml;
    using System.Xml.Schema;

    [DebuggerDisplay("{debuggerDisplayProxy}")]
    public abstract class XPathNavigator : XPathItem, ICloneable, IXPathNavigable, IXmlNamespaceResolver
    {
        internal const int AllMask = 0x7fffffff;
        internal static readonly XPathNavigatorKeyComparer comparer = new XPathNavigatorKeyComparer();
        internal static readonly int[] ContentKindMasks = new int[] { 1, 2, 0, 0, 0x70, 0x20, 0x40, 0x80, 0x100, 0x7ffffff3 };
        internal const int NoAttrNmspMask = 0x7ffffff3;
        internal static readonly char[] NodeTypeLetter = new char[] { 'R', 'E', 'A', 'N', 'T', 'S', 'W', 'P', 'C', 'X' };
        internal const int TextMask = 0x70;
        internal static readonly char[] UniqueIdTbl = new char[] { 
            'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 
            'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z', '1', '2', '3', '4', '5', '6'
         };

        protected XPathNavigator()
        {
        }

        public virtual XmlWriter AppendChild()
        {
            throw new NotSupportedException();
        }

        public virtual void AppendChild(string newChild)
        {
            XmlReader reader = this.CreateContextReader(newChild, true);
            this.AppendChild(reader);
        }

        public virtual void AppendChild(XmlReader newChild)
        {
            if (newChild == null)
            {
                throw new ArgumentNullException("newChild");
            }
            XmlWriter writer = this.AppendChild();
            this.BuildSubtree(newChild, writer);
            writer.Close();
        }

        public virtual void AppendChild(XPathNavigator newChild)
        {
            if (newChild == null)
            {
                throw new ArgumentNullException("newChild");
            }
            if (!this.IsValidChildType(newChild.NodeType))
            {
                throw new InvalidOperationException(Res.GetString("Xpn_BadPosition"));
            }
            XmlReader reader = newChild.CreateReader();
            this.AppendChild(reader);
        }

        public virtual void AppendChildElement(string prefix, string localName, string namespaceURI, string value)
        {
            XmlWriter writer = this.AppendChild();
            writer.WriteStartElement(prefix, localName, namespaceURI);
            if (value != null)
            {
                writer.WriteString(value);
            }
            writer.WriteEndElement();
            writer.Close();
        }

        internal void BuildSubtree(XmlReader reader, XmlWriter writer)
        {
            string ns = "http://www.w3.org/2000/xmlns/";
            ReadState readState = reader.ReadState;
            if ((readState != ReadState.Initial) && (readState != ReadState.Interactive))
            {
                throw new ArgumentException(Res.GetString("Xml_InvalidOperation"), "reader");
            }
            int num = 0;
            if (readState == ReadState.Initial)
            {
                if (!reader.Read())
                {
                    return;
                }
                num++;
            }
            do
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                    {
                        writer.WriteStartElement(reader.Prefix, reader.LocalName, reader.NamespaceURI);
                        bool isEmptyElement = reader.IsEmptyElement;
                        while (reader.MoveToNextAttribute())
                        {
                            if (reader.NamespaceURI == ns)
                            {
                                if (reader.Prefix.Length == 0)
                                {
                                    writer.WriteAttributeString("", "xmlns", ns, reader.Value);
                                }
                                else
                                {
                                    writer.WriteAttributeString("xmlns", reader.LocalName, ns, reader.Value);
                                }
                            }
                            else
                            {
                                writer.WriteStartAttribute(reader.Prefix, reader.LocalName, reader.NamespaceURI);
                                writer.WriteString(reader.Value);
                                writer.WriteEndAttribute();
                            }
                        }
                        reader.MoveToElement();
                        if (isEmptyElement)
                        {
                            writer.WriteEndElement();
                        }
                        else
                        {
                            num++;
                        }
                        break;
                    }
                    case XmlNodeType.Attribute:
                        if (reader.NamespaceURI != ns)
                        {
                            writer.WriteStartAttribute(reader.Prefix, reader.LocalName, reader.NamespaceURI);
                            writer.WriteString(reader.Value);
                            writer.WriteEndAttribute();
                            break;
                        }
                        if (reader.Prefix.Length != 0)
                        {
                            writer.WriteAttributeString("xmlns", reader.LocalName, ns, reader.Value);
                            break;
                        }
                        writer.WriteAttributeString("", "xmlns", ns, reader.Value);
                        break;

                    case XmlNodeType.Text:
                    case XmlNodeType.CDATA:
                        writer.WriteString(reader.Value);
                        break;

                    case XmlNodeType.EntityReference:
                        reader.ResolveEntity();
                        break;

                    case XmlNodeType.ProcessingInstruction:
                        writer.WriteProcessingInstruction(reader.LocalName, reader.Value);
                        break;

                    case XmlNodeType.Comment:
                        writer.WriteComment(reader.Value);
                        break;

                    case XmlNodeType.Whitespace:
                    case XmlNodeType.SignificantWhitespace:
                        writer.WriteString(reader.Value);
                        break;

                    case XmlNodeType.EndElement:
                        writer.WriteFullEndElement();
                        num--;
                        break;
                }
            }
            while (reader.Read() && (num > 0));
        }

        public virtual bool CheckValidity(XmlSchemaSet schemas, ValidationEventHandler validationEventHandler)
        {
            IXmlSchemaInfo schemaInfo;
            XmlSchemaType schemaType = null;
            XmlSchemaElement schemaElement = null;
            XmlSchemaAttribute schemaAttribute = null;
            switch (this.NodeType)
            {
                case XPathNodeType.Root:
                    if (schemas == null)
                    {
                        throw new InvalidOperationException(Res.GetString("XPathDocument_MissingSchemas"));
                    }
                    schemaType = null;
                    break;

                case XPathNodeType.Element:
                    if (schemas == null)
                    {
                        throw new InvalidOperationException(Res.GetString("XPathDocument_MissingSchemas"));
                    }
                    schemaInfo = this.SchemaInfo;
                    if (schemaInfo != null)
                    {
                        schemaType = schemaInfo.SchemaType;
                        schemaElement = schemaInfo.SchemaElement;
                    }
                    if ((schemaType != null) || (schemaElement != null))
                    {
                        break;
                    }
                    throw new InvalidOperationException(Res.GetString("XPathDocument_NotEnoughSchemaInfo", (object[]) null));

                case XPathNodeType.Attribute:
                    if (schemas == null)
                    {
                        throw new InvalidOperationException(Res.GetString("XPathDocument_MissingSchemas"));
                    }
                    schemaInfo = this.SchemaInfo;
                    if (schemaInfo != null)
                    {
                        schemaType = schemaInfo.SchemaType;
                        schemaAttribute = schemaInfo.SchemaAttribute;
                    }
                    if ((schemaType == null) && (schemaAttribute == null))
                    {
                        throw new InvalidOperationException(Res.GetString("XPathDocument_NotEnoughSchemaInfo", (object[]) null));
                    }
                    break;

                default:
                    throw new InvalidOperationException(Res.GetString("XPathDocument_ValidateInvalidNodeType", (object[]) null));
            }
            XmlReader reader = this.CreateReader();
            CheckValidityHelper helper = new CheckValidityHelper(validationEventHandler, reader as XPathNavigatorReader);
            validationEventHandler = new ValidationEventHandler(helper.ValidationCallback);
            XmlReader reader2 = this.GetValidatingReader(reader, schemas, validationEventHandler, schemaType, schemaElement, schemaAttribute);
            while (reader2.Read())
            {
            }
            return helper.IsValid;
        }

        public abstract XPathNavigator Clone();
        public virtual XmlNodeOrder ComparePosition(XPathNavigator nav)
        {
            if (nav != null)
            {
                if (this.IsSamePosition(nav))
                {
                    return XmlNodeOrder.Same;
                }
                XPathNavigator navigator = this.Clone();
                XPathNavigator other = nav.Clone();
                int depth = GetDepth(navigator.Clone());
                int num2 = GetDepth(other.Clone());
                if (depth > num2)
                {
                    while (depth > num2)
                    {
                        navigator.MoveToParent();
                        depth--;
                    }
                    if (navigator.IsSamePosition(other))
                    {
                        return XmlNodeOrder.After;
                    }
                }
                if (num2 > depth)
                {
                    while (num2 > depth)
                    {
                        other.MoveToParent();
                        num2--;
                    }
                    if (navigator.IsSamePosition(other))
                    {
                        return XmlNodeOrder.Before;
                    }
                }
                XPathNavigator navigator3 = navigator.Clone();
                XPathNavigator navigator4 = other.Clone();
                while (navigator3.MoveToParent() && navigator4.MoveToParent())
                {
                    if (navigator3.IsSamePosition(navigator4))
                    {
                        bool flag1 = navigator.GetType().ToString() != "Microsoft.VisualStudio.Modeling.StoreNavigator";
                        return this.CompareSiblings(navigator, other);
                    }
                    navigator.MoveToParent();
                    other.MoveToParent();
                }
            }
            return XmlNodeOrder.Unknown;
        }

        private XmlNodeOrder CompareSiblings(XPathNavigator n1, XPathNavigator n2)
        {
            int num = 0;
            switch (n1.NodeType)
            {
                case XPathNodeType.Attribute:
                    num++;
                    break;

                case XPathNodeType.Namespace:
                    break;

                default:
                    num += 2;
                    break;
            }
            switch (n2.NodeType)
            {
                case XPathNodeType.Attribute:
                    num--;
                    if (num == 0)
                    {
                        while (n1.MoveToNextAttribute())
                        {
                            if (n1.IsSamePosition(n2))
                            {
                                return XmlNodeOrder.Before;
                            }
                        }
                    }
                    break;

                case XPathNodeType.Namespace:
                    if (num == 0)
                    {
                        while (n1.MoveToNextNamespace())
                        {
                            if (n1.IsSamePosition(n2))
                            {
                                return XmlNodeOrder.Before;
                            }
                        }
                    }
                    break;

                default:
                    num -= 2;
                    if (num == 0)
                    {
                        while (n1.MoveToNext())
                        {
                            if (n1.IsSamePosition(n2))
                            {
                                return XmlNodeOrder.Before;
                            }
                        }
                    }
                    break;
            }
            if (num >= 0)
            {
                return XmlNodeOrder.After;
            }
            return XmlNodeOrder.Before;
        }

        public virtual XPathExpression Compile(string xpath)
        {
            return XPathExpression.Compile(xpath);
        }

        private static XPathExpression CompileMatchPattern(string xpath)
        {
            bool flag;
            return new CompiledXpathExpr(new QueryBuilder().BuildPatternQuery(xpath, out flag), xpath, flag);
        }

        public virtual void CreateAttribute(string prefix, string localName, string namespaceURI, string value)
        {
            XmlWriter writer = this.CreateAttributes();
            writer.WriteStartAttribute(prefix, localName, namespaceURI);
            if (value != null)
            {
                writer.WriteString(value);
            }
            writer.WriteEndAttribute();
            writer.Close();
        }

        public virtual XmlWriter CreateAttributes()
        {
            throw new NotSupportedException();
        }

        private XmlReader CreateContextReader(string xml, bool fromCurrentNode)
        {
            if (xml == null)
            {
                throw new ArgumentNullException("xml");
            }
            XPathNavigator navigator = this.CreateNavigator();
            XmlNamespaceManager nsMgr = new XmlNamespaceManager(this.NameTable);
            if (!fromCurrentNode)
            {
                navigator.MoveToParent();
            }
            if (navigator.MoveToFirstNamespace(XPathNamespaceScope.All))
            {
                do
                {
                    nsMgr.AddNamespace(navigator.LocalName, navigator.Value);
                }
                while (navigator.MoveToNextNamespace(XPathNamespaceScope.All));
            }
            return new XmlTextReader(xml, XmlNodeType.Element, new XmlParserContext(this.NameTable, nsMgr, null, XmlSpace.Default)) { WhitespaceHandling = WhitespaceHandling.Significant };
        }

        public virtual XPathNavigator CreateNavigator()
        {
            return this.Clone();
        }

        private XmlReader CreateReader()
        {
            return XPathNavigatorReader.Create(this);
        }

        public virtual void DeleteRange(XPathNavigator lastSiblingToDelete)
        {
            throw new NotSupportedException();
        }

        public virtual void DeleteSelf()
        {
            this.DeleteRange(this);
        }

        public virtual object Evaluate(string xpath)
        {
            return this.Evaluate(XPathExpression.Compile(xpath), null);
        }

        public virtual object Evaluate(XPathExpression expr)
        {
            return this.Evaluate(expr, null);
        }

        public virtual object Evaluate(string xpath, IXmlNamespaceResolver resolver)
        {
            return this.Evaluate(XPathExpression.Compile(xpath, resolver));
        }

        public virtual object Evaluate(XPathExpression expr, XPathNodeIterator context)
        {
            CompiledXpathExpr expr2 = expr as CompiledXpathExpr;
            if (expr2 == null)
            {
                throw XPathException.Create("Xp_BadQueryObject");
            }
            Query query = Query.Clone(expr2.QueryTree);
            query.Reset();
            if (context == null)
            {
                context = new XPathSingletonIterator(this.Clone(), true);
            }
            object obj2 = query.Evaluate(context);
            if (obj2 is XPathNodeIterator)
            {
                return new XPathSelectionIterator(context.Current, query);
            }
            return obj2;
        }

        public virtual string GetAttribute(string localName, string namespaceURI)
        {
            if (!this.MoveToAttribute(localName, namespaceURI))
            {
                return "";
            }
            string str = this.Value;
            this.MoveToParent();
            return str;
        }

        internal static int GetContentKindMask(XPathNodeType type)
        {
            return ContentKindMasks[(int) type];
        }

        private static int GetDepth(XPathNavigator nav)
        {
            int num = 0;
            while (nav.MoveToParent())
            {
                num++;
            }
            return num;
        }

        internal static int GetKindMask(XPathNodeType type)
        {
            if (type == XPathNodeType.All)
            {
                return 0x7fffffff;
            }
            if (type == XPathNodeType.Text)
            {
                return 0x70;
            }
            return (((int) 1) << type);
        }

        public virtual string GetNamespace(string name)
        {
            if (!this.MoveToNamespace(name))
            {
                if (name == "xml")
                {
                    return "http://www.w3.org/XML/1998/namespace";
                }
                if (name == "xmlns")
                {
                    return "http://www.w3.org/2000/xmlns/";
                }
                return string.Empty;
            }
            string str = this.Value;
            this.MoveToParent();
            return str;
        }

        internal static XmlNamespaceManager GetNamespaces(IXmlNamespaceResolver resolver)
        {
            XmlNamespaceManager manager = new XmlNamespaceManager(new System.Xml.NameTable());
            foreach (KeyValuePair<string, string> pair in resolver.GetNamespacesInScope(XmlNamespaceScope.All))
            {
                if (pair.Key != "xmlns")
                {
                    manager.AddNamespace(pair.Key, pair.Value);
                }
            }
            return manager;
        }

        public virtual IDictionary<string, string> GetNamespacesInScope(XmlNamespaceScope scope)
        {
            XPathNodeType nodeType = this.NodeType;
            if (((nodeType != XPathNodeType.Element) && (scope != XmlNamespaceScope.Local)) || ((nodeType == XPathNodeType.Attribute) || (nodeType == XPathNodeType.Namespace)))
            {
                XPathNavigator navigator = this.Clone();
                if (navigator.MoveToParent())
                {
                    return navigator.GetNamespacesInScope(scope);
                }
            }
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            if (scope == XmlNamespaceScope.All)
            {
                dictionary["xml"] = "http://www.w3.org/XML/1998/namespace";
            }
            if (this.MoveToFirstNamespace((XPathNamespaceScope) scope))
            {
                do
                {
                    string localName = this.LocalName;
                    string str2 = this.Value;
                    if (((localName.Length != 0) || (str2.Length != 0)) || (scope == XmlNamespaceScope.Local))
                    {
                        dictionary[localName] = str2;
                    }
                }
                while (this.MoveToNextNamespace((XPathNamespaceScope) scope));
                this.MoveToParent();
            }
            return dictionary;
        }

        private XmlReader GetValidatingReader(XmlReader reader, XmlSchemaSet schemas, ValidationEventHandler validationEvent, XmlSchemaType schemaType, XmlSchemaElement schemaElement, XmlSchemaAttribute schemaAttribute)
        {
            if (schemaAttribute != null)
            {
                return schemaAttribute.Validate(reader, null, schemas, validationEvent);
            }
            if (schemaElement != null)
            {
                return schemaElement.Validate(reader, null, schemas, validationEvent);
            }
            if (schemaType != null)
            {
                return schemaType.Validate(reader, null, schemas, validationEvent);
            }
            XmlReaderSettings settings = new XmlReaderSettings {
                ConformanceLevel = ConformanceLevel.Auto,
                ValidationType = ValidationType.Schema,
                Schemas = schemas
            };
            settings.ValidationEventHandler += validationEvent;
            return XmlReader.Create(reader, settings);
        }

        public virtual XmlWriter InsertAfter()
        {
            throw new NotSupportedException();
        }

        public virtual void InsertAfter(string newSibling)
        {
            XmlReader reader = this.CreateContextReader(newSibling, false);
            this.InsertAfter(reader);
        }

        public virtual void InsertAfter(XmlReader newSibling)
        {
            if (newSibling == null)
            {
                throw new ArgumentNullException("newSibling");
            }
            XmlWriter writer = this.InsertAfter();
            this.BuildSubtree(newSibling, writer);
            writer.Close();
        }

        public virtual void InsertAfter(XPathNavigator newSibling)
        {
            if (newSibling == null)
            {
                throw new ArgumentNullException("newSibling");
            }
            if (!this.IsValidSiblingType(newSibling.NodeType))
            {
                throw new InvalidOperationException(Res.GetString("Xpn_BadPosition"));
            }
            XmlReader reader = newSibling.CreateReader();
            this.InsertAfter(reader);
        }

        public virtual XmlWriter InsertBefore()
        {
            throw new NotSupportedException();
        }

        public virtual void InsertBefore(string newSibling)
        {
            XmlReader reader = this.CreateContextReader(newSibling, false);
            this.InsertBefore(reader);
        }

        public virtual void InsertBefore(XmlReader newSibling)
        {
            if (newSibling == null)
            {
                throw new ArgumentNullException("newSibling");
            }
            XmlWriter writer = this.InsertBefore();
            this.BuildSubtree(newSibling, writer);
            writer.Close();
        }

        public virtual void InsertBefore(XPathNavigator newSibling)
        {
            if (newSibling == null)
            {
                throw new ArgumentNullException("newSibling");
            }
            if (!this.IsValidSiblingType(newSibling.NodeType))
            {
                throw new InvalidOperationException(Res.GetString("Xpn_BadPosition"));
            }
            XmlReader reader = newSibling.CreateReader();
            this.InsertBefore(reader);
        }

        public virtual void InsertElementAfter(string prefix, string localName, string namespaceURI, string value)
        {
            XmlWriter writer = this.InsertAfter();
            writer.WriteStartElement(prefix, localName, namespaceURI);
            if (value != null)
            {
                writer.WriteString(value);
            }
            writer.WriteEndElement();
            writer.Close();
        }

        public virtual void InsertElementBefore(string prefix, string localName, string namespaceURI, string value)
        {
            XmlWriter writer = this.InsertBefore();
            writer.WriteStartElement(prefix, localName, namespaceURI);
            if (value != null)
            {
                writer.WriteString(value);
            }
            writer.WriteEndElement();
            writer.Close();
        }

        public virtual bool IsDescendant(XPathNavigator nav)
        {
            if (nav != null)
            {
                nav = nav.Clone();
                while (nav.MoveToParent())
                {
                    if (nav.IsSamePosition(this))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public abstract bool IsSamePosition(XPathNavigator other);
        internal static bool IsText(XPathNodeType type)
        {
            return ((type - 4) <= XPathNodeType.Attribute);
        }

        private bool IsValidChildType(XPathNodeType type)
        {
            switch (this.NodeType)
            {
                case XPathNodeType.Root:
                    switch (type)
                    {
                        case XPathNodeType.Element:
                        case XPathNodeType.SignificantWhitespace:
                        case XPathNodeType.Whitespace:
                        case XPathNodeType.ProcessingInstruction:
                        case XPathNodeType.Comment:
                            return true;
                    }
                    break;

                case XPathNodeType.Element:
                    switch (type)
                    {
                        case XPathNodeType.Element:
                        case XPathNodeType.Text:
                        case XPathNodeType.SignificantWhitespace:
                        case XPathNodeType.Whitespace:
                        case XPathNodeType.ProcessingInstruction:
                        case XPathNodeType.Comment:
                            return true;
                    }
                    break;
            }
            return false;
        }

        private bool IsValidSiblingType(XPathNodeType type)
        {
            switch (this.NodeType)
            {
                case XPathNodeType.Element:
                case XPathNodeType.Text:
                case XPathNodeType.SignificantWhitespace:
                case XPathNodeType.Whitespace:
                case XPathNodeType.ProcessingInstruction:
                case XPathNodeType.Comment:
                    switch (type)
                    {
                        case XPathNodeType.Element:
                        case XPathNodeType.Text:
                        case XPathNodeType.SignificantWhitespace:
                        case XPathNodeType.Whitespace:
                        case XPathNodeType.ProcessingInstruction:
                        case XPathNodeType.Comment:
                            return true;
                    }
                    break;
            }
            return false;
        }

        public virtual string LookupNamespace(string prefix)
        {
            if (prefix != null)
            {
                if (this.NodeType != XPathNodeType.Element)
                {
                    XPathNavigator navigator = this.Clone();
                    if (navigator.MoveToParent())
                    {
                        return navigator.LookupNamespace(prefix);
                    }
                }
                else if (this.MoveToNamespace(prefix))
                {
                    string str = this.Value;
                    this.MoveToParent();
                    return str;
                }
                if (prefix.Length == 0)
                {
                    return string.Empty;
                }
                if (prefix == "xml")
                {
                    return "http://www.w3.org/XML/1998/namespace";
                }
                if (prefix == "xmlns")
                {
                    return "http://www.w3.org/2000/xmlns/";
                }
            }
            return null;
        }

        public virtual string LookupPrefix(string namespaceURI)
        {
            if (namespaceURI != null)
            {
                XPathNavigator navigator = this.Clone();
                if (this.NodeType != XPathNodeType.Element)
                {
                    if (navigator.MoveToParent())
                    {
                        return navigator.LookupPrefix(namespaceURI);
                    }
                }
                else if (navigator.MoveToFirstNamespace(XPathNamespaceScope.All))
                {
                    do
                    {
                        if (namespaceURI == navigator.Value)
                        {
                            return navigator.LocalName;
                        }
                    }
                    while (navigator.MoveToNextNamespace(XPathNamespaceScope.All));
                }
                if (namespaceURI == this.LookupNamespace(string.Empty))
                {
                    return string.Empty;
                }
                if (namespaceURI == "http://www.w3.org/XML/1998/namespace")
                {
                    return "xml";
                }
                if (namespaceURI == "http://www.w3.org/2000/xmlns/")
                {
                    return "xmlns";
                }
            }
            return null;
        }

        public virtual bool Matches(string xpath)
        {
            return this.Matches(CompileMatchPattern(xpath));
        }

        public virtual bool Matches(XPathExpression expr)
        {
            bool flag;
            CompiledXpathExpr expr2 = expr as CompiledXpathExpr;
            if (expr2 == null)
            {
                throw XPathException.Create("Xp_BadQueryObject");
            }
            Query query = Query.Clone(expr2.QueryTree);
            try
            {
                flag = query.MatchNode(this) != null;
            }
            catch (XPathException)
            {
                throw XPathException.Create("Xp_InvalidPattern", expr2.Expression);
            }
            return flag;
        }

        public abstract bool MoveTo(XPathNavigator other);
        public virtual bool MoveToAttribute(string localName, string namespaceURI)
        {
            if (this.MoveToFirstAttribute())
            {
                do
                {
                    if ((localName == this.LocalName) && (namespaceURI == this.NamespaceURI))
                    {
                        return true;
                    }
                }
                while (this.MoveToNextAttribute());
                this.MoveToParent();
            }
            return false;
        }

        public virtual bool MoveToChild(XPathNodeType type)
        {
            if (this.MoveToFirstChild())
            {
                int contentKindMask = GetContentKindMask(type);
                do
                {
                    if (((((int) 1) << this.NodeType) & contentKindMask) != 0)
                    {
                        return true;
                    }
                }
                while (this.MoveToNext());
                this.MoveToParent();
            }
            return false;
        }

        public virtual bool MoveToChild(string localName, string namespaceURI)
        {
            if (this.MoveToFirstChild())
            {
                do
                {
                    if (((this.NodeType == XPathNodeType.Element) && (localName == this.LocalName)) && (namespaceURI == this.NamespaceURI))
                    {
                        return true;
                    }
                }
                while (this.MoveToNext());
                this.MoveToParent();
            }
            return false;
        }

        public virtual bool MoveToFirst()
        {
            switch (this.NodeType)
            {
                case XPathNodeType.Attribute:
                case XPathNodeType.Namespace:
                    return false;
            }
            if (!this.MoveToParent())
            {
                return false;
            }
            return this.MoveToFirstChild();
        }

        public abstract bool MoveToFirstAttribute();
        public abstract bool MoveToFirstChild();
        public bool MoveToFirstNamespace()
        {
            return this.MoveToFirstNamespace(XPathNamespaceScope.All);
        }

        public abstract bool MoveToFirstNamespace(XPathNamespaceScope namespaceScope);
        public virtual bool MoveToFollowing(XPathNodeType type)
        {
            return this.MoveToFollowing(type, null);
        }

        public virtual bool MoveToFollowing(string localName, string namespaceURI)
        {
            return this.MoveToFollowing(localName, namespaceURI, null);
        }

        public virtual bool MoveToFollowing(XPathNodeType type, XPathNavigator end)
        {
            XPathNavigator other = this.Clone();
            int contentKindMask = GetContentKindMask(type);
            if (end != null)
            {
                switch (end.NodeType)
                {
                    case XPathNodeType.Attribute:
                    case XPathNodeType.Namespace:
                        end = end.Clone();
                        end.MoveToNonDescendant();
                        break;
                }
            }
            switch (this.NodeType)
            {
                case XPathNodeType.Attribute:
                case XPathNodeType.Namespace:
                    if (this.MoveToParent())
                    {
                        break;
                    }
                    return false;
            }
        Label_005C:
            if (!this.MoveToFirstChild())
            {
                do
                {
                    if (this.MoveToNext())
                    {
                        goto Label_007E;
                    }
                }
                while (this.MoveToParent());
                this.MoveTo(other);
                return false;
            }
        Label_007E:
            if ((end != null) && this.IsSamePosition(end))
            {
                this.MoveTo(other);
                return false;
            }
            if (((((int) 1) << this.NodeType) & contentKindMask) == 0)
            {
                goto Label_005C;
            }
            return true;
        }

        public virtual bool MoveToFollowing(string localName, string namespaceURI, XPathNavigator end)
        {
            XPathNavigator other = this.Clone();
            if (end != null)
            {
                switch (end.NodeType)
                {
                    case XPathNodeType.Attribute:
                    case XPathNodeType.Namespace:
                        end = end.Clone();
                        end.MoveToNonDescendant();
                        break;
                }
            }
            switch (this.NodeType)
            {
                case XPathNodeType.Attribute:
                case XPathNodeType.Namespace:
                    if (this.MoveToParent())
                    {
                        break;
                    }
                    return false;
            }
        Label_0055:
            if (!this.MoveToFirstChild())
            {
                do
                {
                    if (this.MoveToNext())
                    {
                        goto Label_0077;
                    }
                }
                while (this.MoveToParent());
                this.MoveTo(other);
                return false;
            }
        Label_0077:
            if ((end != null) && this.IsSamePosition(end))
            {
                this.MoveTo(other);
                return false;
            }
            if (((this.NodeType != XPathNodeType.Element) || (localName != this.LocalName)) || (namespaceURI != this.NamespaceURI))
            {
                goto Label_0055;
            }
            return true;
        }

        public abstract bool MoveToId(string id);
        public virtual bool MoveToNamespace(string name)
        {
            if (this.MoveToFirstNamespace(XPathNamespaceScope.All))
            {
                do
                {
                    if (name == this.LocalName)
                    {
                        return true;
                    }
                }
                while (this.MoveToNextNamespace(XPathNamespaceScope.All));
                this.MoveToParent();
            }
            return false;
        }

        public abstract bool MoveToNext();
        public virtual bool MoveToNext(XPathNodeType type)
        {
            XPathNavigator other = this.Clone();
            int contentKindMask = GetContentKindMask(type);
            while (this.MoveToNext())
            {
                if (((((int) 1) << this.NodeType) & contentKindMask) != 0)
                {
                    return true;
                }
            }
            this.MoveTo(other);
            return false;
        }

        public virtual bool MoveToNext(string localName, string namespaceURI)
        {
            XPathNavigator other = this.Clone();
            while (this.MoveToNext())
            {
                if (((this.NodeType == XPathNodeType.Element) && (localName == this.LocalName)) && (namespaceURI == this.NamespaceURI))
                {
                    return true;
                }
            }
            this.MoveTo(other);
            return false;
        }

        public abstract bool MoveToNextAttribute();
        public bool MoveToNextNamespace()
        {
            return this.MoveToNextNamespace(XPathNamespaceScope.All);
        }

        public abstract bool MoveToNextNamespace(XPathNamespaceScope namespaceScope);
        internal bool MoveToNonDescendant()
        {
            if (this.NodeType == XPathNodeType.Root)
            {
                return false;
            }
            if (!this.MoveToNext())
            {
                XPathNavigator other = this.Clone();
                if (!this.MoveToParent())
                {
                    return false;
                }
                switch (other.NodeType)
                {
                    case XPathNodeType.Attribute:
                    case XPathNodeType.Namespace:
                        if (!this.MoveToFirstChild())
                        {
                            break;
                        }
                        return true;
                }
                while (!this.MoveToNext())
                {
                    if (!this.MoveToParent())
                    {
                        this.MoveTo(other);
                        return false;
                    }
                }
            }
            return true;
        }

        public abstract bool MoveToParent();
        public abstract bool MoveToPrevious();
        internal bool MoveToPrevious(XPathNodeType type)
        {
            XPathNavigator other = this.Clone();
            int contentKindMask = GetContentKindMask(type);
            while (this.MoveToPrevious())
            {
                if (((((int) 1) << this.NodeType) & contentKindMask) != 0)
                {
                    return true;
                }
            }
            this.MoveTo(other);
            return false;
        }

        internal bool MoveToPrevious(string localName, string namespaceURI)
        {
            XPathNavigator other = this.Clone();
            localName = (localName != null) ? this.NameTable.Get(localName) : null;
            while (this.MoveToPrevious())
            {
                if (((this.NodeType == XPathNodeType.Element) && (localName == this.LocalName)) && (namespaceURI == this.NamespaceURI))
                {
                    return true;
                }
            }
            this.MoveTo(other);
            return false;
        }

        public virtual void MoveToRoot()
        {
            while (this.MoveToParent())
            {
            }
        }

        public virtual XmlWriter PrependChild()
        {
            throw new NotSupportedException();
        }

        public virtual void PrependChild(string newChild)
        {
            XmlReader reader = this.CreateContextReader(newChild, true);
            this.PrependChild(reader);
        }

        public virtual void PrependChild(XmlReader newChild)
        {
            if (newChild == null)
            {
                throw new ArgumentNullException("newChild");
            }
            XmlWriter writer = this.PrependChild();
            this.BuildSubtree(newChild, writer);
            writer.Close();
        }

        public virtual void PrependChild(XPathNavigator newChild)
        {
            if (newChild == null)
            {
                throw new ArgumentNullException("newChild");
            }
            if (!this.IsValidChildType(newChild.NodeType))
            {
                throw new InvalidOperationException(Res.GetString("Xpn_BadPosition"));
            }
            XmlReader reader = newChild.CreateReader();
            this.PrependChild(reader);
        }

        public virtual void PrependChildElement(string prefix, string localName, string namespaceURI, string value)
        {
            XmlWriter writer = this.PrependChild();
            writer.WriteStartElement(prefix, localName, namespaceURI);
            if (value != null)
            {
                writer.WriteString(value);
            }
            writer.WriteEndElement();
            writer.Close();
        }

        public virtual XmlReader ReadSubtree()
        {
            switch (this.NodeType)
            {
                case XPathNodeType.Root:
                case XPathNodeType.Element:
                    return this.CreateReader();
            }
            throw new InvalidOperationException(Res.GetString("Xpn_BadPosition"));
        }

        public virtual XmlWriter ReplaceRange(XPathNavigator lastSiblingToReplace)
        {
            throw new NotSupportedException();
        }

        public virtual void ReplaceSelf(string newNode)
        {
            XmlReader reader = this.CreateContextReader(newNode, false);
            this.ReplaceSelf(reader);
        }

        public virtual void ReplaceSelf(XmlReader newNode)
        {
            if (newNode == null)
            {
                throw new ArgumentNullException("newNode");
            }
            switch (this.NodeType)
            {
                case XPathNodeType.Root:
                case XPathNodeType.Attribute:
                case XPathNodeType.Namespace:
                    throw new InvalidOperationException(Res.GetString("Xpn_BadPosition"));
            }
            XmlWriter writer = this.ReplaceRange(this);
            this.BuildSubtree(newNode, writer);
            writer.Close();
        }

        public virtual void ReplaceSelf(XPathNavigator newNode)
        {
            if (newNode == null)
            {
                throw new ArgumentNullException("newNode");
            }
            XmlReader reader = newNode.CreateReader();
            this.ReplaceSelf(reader);
        }

        public virtual XPathNodeIterator Select(string xpath)
        {
            return this.Select(XPathExpression.Compile(xpath));
        }

        public virtual XPathNodeIterator Select(XPathExpression expr)
        {
            XPathNodeIterator iterator = this.Evaluate(expr) as XPathNodeIterator;
            if (iterator == null)
            {
                throw XPathException.Create("Xp_NodeSetExpected");
            }
            return iterator;
        }

        public virtual XPathNodeIterator Select(string xpath, IXmlNamespaceResolver resolver)
        {
            return this.Select(XPathExpression.Compile(xpath, resolver));
        }

        public virtual XPathNodeIterator SelectAncestors(XPathNodeType type, bool matchSelf)
        {
            return new XPathAncestorIterator(this.Clone(), type, matchSelf);
        }

        public virtual XPathNodeIterator SelectAncestors(string name, string namespaceURI, bool matchSelf)
        {
            return new XPathAncestorIterator(this.Clone(), name, namespaceURI, matchSelf);
        }

        public virtual XPathNodeIterator SelectChildren(XPathNodeType type)
        {
            return new XPathChildIterator(this.Clone(), type);
        }

        public virtual XPathNodeIterator SelectChildren(string name, string namespaceURI)
        {
            return new XPathChildIterator(this.Clone(), name, namespaceURI);
        }

        public virtual XPathNodeIterator SelectDescendants(XPathNodeType type, bool matchSelf)
        {
            return new XPathDescendantIterator(this.Clone(), type, matchSelf);
        }

        public virtual XPathNodeIterator SelectDescendants(string name, string namespaceURI, bool matchSelf)
        {
            return new XPathDescendantIterator(this.Clone(), name, namespaceURI, matchSelf);
        }

        public virtual XPathNavigator SelectSingleNode(string xpath)
        {
            return this.SelectSingleNode(XPathExpression.Compile(xpath));
        }

        public virtual XPathNavigator SelectSingleNode(XPathExpression expression)
        {
            XPathNodeIterator iterator = this.Select(expression);
            if (iterator.MoveNext())
            {
                return iterator.Current;
            }
            return null;
        }

        public virtual XPathNavigator SelectSingleNode(string xpath, IXmlNamespaceResolver resolver)
        {
            return this.SelectSingleNode(XPathExpression.Compile(xpath, resolver));
        }

        public virtual void SetTypedValue(object typedValue)
        {
            if (typedValue == null)
            {
                throw new ArgumentNullException("typedValue");
            }
            switch (this.NodeType)
            {
                case XPathNodeType.Element:
                case XPathNodeType.Attribute:
                {
                    string s = null;
                    IXmlSchemaInfo schemaInfo = this.SchemaInfo;
                    if (schemaInfo != null)
                    {
                        XmlSchemaType schemaType = schemaInfo.SchemaType;
                        if (schemaType != null)
                        {
                            s = schemaType.ValueConverter.ToString(typedValue, this);
                            XmlSchemaDatatype datatype = schemaType.Datatype;
                            if (datatype != null)
                            {
                                datatype.ParseValue(s, this.NameTable, this);
                            }
                        }
                    }
                    if (s == null)
                    {
                        s = XmlUntypedConverter.Untyped.ToString(typedValue, this);
                    }
                    this.SetValue(s);
                    return;
                }
            }
            throw new InvalidOperationException(Res.GetString("Xpn_BadPosition"));
        }

        public virtual void SetValue(string value)
        {
            throw new NotSupportedException();
        }

        object ICloneable.Clone()
        {
            return this.Clone();
        }

        public override string ToString()
        {
            return this.Value;
        }

        public override object ValueAs(Type returnType, IXmlNamespaceResolver nsResolver)
        {
            if (nsResolver == null)
            {
                nsResolver = this;
            }
            IXmlSchemaInfo schemaInfo = this.SchemaInfo;
            if (schemaInfo != null)
            {
                XmlSchemaType memberType;
                if (schemaInfo.Validity == XmlSchemaValidity.Valid)
                {
                    memberType = schemaInfo.MemberType;
                    if (memberType == null)
                    {
                        memberType = schemaInfo.SchemaType;
                    }
                    if (memberType != null)
                    {
                        return memberType.ValueConverter.ChangeType(this.Value, returnType, nsResolver);
                    }
                }
                else
                {
                    memberType = schemaInfo.SchemaType;
                    if (memberType != null)
                    {
                        XmlSchemaDatatype datatype = memberType.Datatype;
                        if (datatype != null)
                        {
                            return memberType.ValueConverter.ChangeType(datatype.ParseValue(this.Value, this.NameTable, nsResolver), returnType, nsResolver);
                        }
                    }
                }
            }
            return XmlUntypedConverter.Untyped.ChangeType(this.Value, returnType, nsResolver);
        }

        public virtual void WriteSubtree(XmlWriter writer)
        {
            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }
            writer.WriteNode(this, true);
        }

        public abstract string BaseURI { get; }

        public virtual bool CanEdit
        {
            get
            {
                return false;
            }
        }

        private object debuggerDisplayProxy
        {
            get
            {
                return new DebuggerDisplayProxy(this);
            }
        }

        public virtual bool HasAttributes
        {
            get
            {
                if (!this.MoveToFirstAttribute())
                {
                    return false;
                }
                this.MoveToParent();
                return true;
            }
        }

        public virtual bool HasChildren
        {
            get
            {
                if (this.MoveToFirstChild())
                {
                    this.MoveToParent();
                    return true;
                }
                return false;
            }
        }

        internal uint IndexInParent
        {
            get
            {
                XPathNavigator navigator = this.Clone();
                uint num = 0;
                switch (this.NodeType)
                {
                    case XPathNodeType.Attribute:
                        while (navigator.MoveToNextAttribute())
                        {
                            num++;
                        }
                        return num;

                    case XPathNodeType.Namespace:
                        while (navigator.MoveToNextNamespace())
                        {
                            num++;
                        }
                        return num;
                }
                while (navigator.MoveToNext())
                {
                    num++;
                }
                return num;
            }
        }

        public virtual string InnerXml
        {
            get
            {
                switch (this.NodeType)
                {
                    case XPathNodeType.Root:
                    case XPathNodeType.Element:
                    {
                        StringWriter output = new StringWriter(CultureInfo.InvariantCulture);
                        XmlWriterSettings settings = new XmlWriterSettings {
                            Indent = true,
                            OmitXmlDeclaration = true,
                            ConformanceLevel = ConformanceLevel.Auto
                        };
                        XmlWriter writer2 = XmlWriter.Create(output, settings);
                        try
                        {
                            if (this.MoveToFirstChild())
                            {
                                do
                                {
                                    writer2.WriteNode(this, true);
                                }
                                while (this.MoveToNext());
                                this.MoveToParent();
                            }
                        }
                        finally
                        {
                            writer2.Close();
                        }
                        return output.ToString();
                    }
                    case XPathNodeType.Attribute:
                    case XPathNodeType.Namespace:
                        return this.Value;
                }
                return string.Empty;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                switch (this.NodeType)
                {
                    case XPathNodeType.Root:
                    case XPathNodeType.Element:
                    {
                        XPathNavigator navigator = this.CreateNavigator();
                        while (navigator.MoveToFirstChild())
                        {
                            navigator.DeleteSelf();
                        }
                        if (value.Length != 0)
                        {
                            navigator.AppendChild(value);
                        }
                        return;
                    }
                    case XPathNodeType.Attribute:
                        this.SetValue(value);
                        return;
                }
                throw new InvalidOperationException(Res.GetString("Xpn_BadPosition"));
            }
        }

        public abstract bool IsEmptyElement { get; }

        public sealed override bool IsNode
        {
            get
            {
                return true;
            }
        }

        public abstract string LocalName { get; }

        public abstract string Name { get; }

        public abstract string NamespaceURI { get; }

        public abstract XmlNameTable NameTable { get; }

        public static IEqualityComparer NavigatorComparer
        {
            get
            {
                return comparer;
            }
        }

        public abstract XPathNodeType NodeType { get; }

        public virtual string OuterXml
        {
            get
            {
                if (this.NodeType == XPathNodeType.Attribute)
                {
                    return (this.Name + "=\"" + this.Value + "\"");
                }
                if (this.NodeType == XPathNodeType.Namespace)
                {
                    if (this.LocalName.Length == 0)
                    {
                        return ("xmlns=\"" + this.Value + "\"");
                    }
                    return ("xmlns:" + this.LocalName + "=\"" + this.Value + "\"");
                }
                StringWriter output = new StringWriter(CultureInfo.InvariantCulture);
                XmlWriterSettings settings = new XmlWriterSettings {
                    Indent = true,
                    OmitXmlDeclaration = true,
                    ConformanceLevel = ConformanceLevel.Auto
                };
                XmlWriter writer2 = XmlWriter.Create(output, settings);
                try
                {
                    writer2.WriteNode(this, true);
                }
                finally
                {
                    writer2.Close();
                }
                return output.ToString();
            }
            set
            {
                this.ReplaceSelf(value);
            }
        }

        public abstract string Prefix { get; }

        public virtual IXmlSchemaInfo SchemaInfo
        {
            get
            {
                return (this as IXmlSchemaInfo);
            }
        }

        public override object TypedValue
        {
            get
            {
                IXmlSchemaInfo schemaInfo = this.SchemaInfo;
                if (schemaInfo != null)
                {
                    XmlSchemaType memberType;
                    XmlSchemaDatatype datatype;
                    if (schemaInfo.Validity == XmlSchemaValidity.Valid)
                    {
                        memberType = schemaInfo.MemberType;
                        if (memberType == null)
                        {
                            memberType = schemaInfo.SchemaType;
                        }
                        if (memberType != null)
                        {
                            datatype = memberType.Datatype;
                            if (datatype != null)
                            {
                                return memberType.ValueConverter.ChangeType(this.Value, datatype.ValueType, this);
                            }
                        }
                    }
                    else
                    {
                        memberType = schemaInfo.SchemaType;
                        if (memberType != null)
                        {
                            datatype = memberType.Datatype;
                            if (datatype != null)
                            {
                                return memberType.ValueConverter.ChangeType(datatype.ParseValue(this.Value, this.NameTable, this), datatype.ValueType, this);
                            }
                        }
                    }
                }
                return this.Value;
            }
        }

        public virtual object UnderlyingObject
        {
            get
            {
                return null;
            }
        }

        internal virtual string UniqueId
        {
            get
            {
                XPathNavigator navigator = this.Clone();
                StringBuilder builder = new StringBuilder();
                builder.Append(NodeTypeLetter[(int) this.NodeType]);
                while (true)
                {
                    uint indexInParent = navigator.IndexInParent;
                    if (!navigator.MoveToParent())
                    {
                        return builder.ToString();
                    }
                    if (indexInParent <= 0x1f)
                    {
                        builder.Append(UniqueIdTbl[indexInParent]);
                    }
                    else
                    {
                        builder.Append('0');
                        do
                        {
                            builder.Append(UniqueIdTbl[(int) ((IntPtr) (indexInParent & 0x1f))]);
                            indexInParent = indexInParent >> 5;
                        }
                        while (indexInParent != 0);
                        builder.Append('0');
                    }
                }
            }
        }

        public override bool ValueAsBoolean
        {
            get
            {
                IXmlSchemaInfo schemaInfo = this.SchemaInfo;
                if (schemaInfo != null)
                {
                    XmlSchemaType memberType;
                    if (schemaInfo.Validity == XmlSchemaValidity.Valid)
                    {
                        memberType = schemaInfo.MemberType;
                        if (memberType == null)
                        {
                            memberType = schemaInfo.SchemaType;
                        }
                        if (memberType != null)
                        {
                            return memberType.ValueConverter.ToBoolean(this.Value);
                        }
                    }
                    else
                    {
                        memberType = schemaInfo.SchemaType;
                        if (memberType != null)
                        {
                            XmlSchemaDatatype datatype = memberType.Datatype;
                            if (datatype != null)
                            {
                                return memberType.ValueConverter.ToBoolean(datatype.ParseValue(this.Value, this.NameTable, this));
                            }
                        }
                    }
                }
                return XmlUntypedConverter.Untyped.ToBoolean(this.Value);
            }
        }

        public override DateTime ValueAsDateTime
        {
            get
            {
                IXmlSchemaInfo schemaInfo = this.SchemaInfo;
                if (schemaInfo != null)
                {
                    XmlSchemaType memberType;
                    if (schemaInfo.Validity == XmlSchemaValidity.Valid)
                    {
                        memberType = schemaInfo.MemberType;
                        if (memberType == null)
                        {
                            memberType = schemaInfo.SchemaType;
                        }
                        if (memberType != null)
                        {
                            return memberType.ValueConverter.ToDateTime(this.Value);
                        }
                    }
                    else
                    {
                        memberType = schemaInfo.SchemaType;
                        if (memberType != null)
                        {
                            XmlSchemaDatatype datatype = memberType.Datatype;
                            if (datatype != null)
                            {
                                return memberType.ValueConverter.ToDateTime(datatype.ParseValue(this.Value, this.NameTable, this));
                            }
                        }
                    }
                }
                return XmlUntypedConverter.Untyped.ToDateTime(this.Value);
            }
        }

        public override double ValueAsDouble
        {
            get
            {
                IXmlSchemaInfo schemaInfo = this.SchemaInfo;
                if (schemaInfo != null)
                {
                    XmlSchemaType memberType;
                    if (schemaInfo.Validity == XmlSchemaValidity.Valid)
                    {
                        memberType = schemaInfo.MemberType;
                        if (memberType == null)
                        {
                            memberType = schemaInfo.SchemaType;
                        }
                        if (memberType != null)
                        {
                            return memberType.ValueConverter.ToDouble(this.Value);
                        }
                    }
                    else
                    {
                        memberType = schemaInfo.SchemaType;
                        if (memberType != null)
                        {
                            XmlSchemaDatatype datatype = memberType.Datatype;
                            if (datatype != null)
                            {
                                return memberType.ValueConverter.ToDouble(datatype.ParseValue(this.Value, this.NameTable, this));
                            }
                        }
                    }
                }
                return XmlUntypedConverter.Untyped.ToDouble(this.Value);
            }
        }

        public override int ValueAsInt
        {
            get
            {
                IXmlSchemaInfo schemaInfo = this.SchemaInfo;
                if (schemaInfo != null)
                {
                    XmlSchemaType memberType;
                    if (schemaInfo.Validity == XmlSchemaValidity.Valid)
                    {
                        memberType = schemaInfo.MemberType;
                        if (memberType == null)
                        {
                            memberType = schemaInfo.SchemaType;
                        }
                        if (memberType != null)
                        {
                            return memberType.ValueConverter.ToInt32(this.Value);
                        }
                    }
                    else
                    {
                        memberType = schemaInfo.SchemaType;
                        if (memberType != null)
                        {
                            XmlSchemaDatatype datatype = memberType.Datatype;
                            if (datatype != null)
                            {
                                return memberType.ValueConverter.ToInt32(datatype.ParseValue(this.Value, this.NameTable, this));
                            }
                        }
                    }
                }
                return XmlUntypedConverter.Untyped.ToInt32(this.Value);
            }
        }

        public override long ValueAsLong
        {
            get
            {
                IXmlSchemaInfo schemaInfo = this.SchemaInfo;
                if (schemaInfo != null)
                {
                    XmlSchemaType memberType;
                    if (schemaInfo.Validity == XmlSchemaValidity.Valid)
                    {
                        memberType = schemaInfo.MemberType;
                        if (memberType == null)
                        {
                            memberType = schemaInfo.SchemaType;
                        }
                        if (memberType != null)
                        {
                            return memberType.ValueConverter.ToInt64(this.Value);
                        }
                    }
                    else
                    {
                        memberType = schemaInfo.SchemaType;
                        if (memberType != null)
                        {
                            XmlSchemaDatatype datatype = memberType.Datatype;
                            if (datatype != null)
                            {
                                return memberType.ValueConverter.ToInt64(datatype.ParseValue(this.Value, this.NameTable, this));
                            }
                        }
                    }
                }
                return XmlUntypedConverter.Untyped.ToInt64(this.Value);
            }
        }

        public override Type ValueType
        {
            get
            {
                IXmlSchemaInfo schemaInfo = this.SchemaInfo;
                if (schemaInfo != null)
                {
                    XmlSchemaType memberType;
                    XmlSchemaDatatype datatype;
                    if (schemaInfo.Validity == XmlSchemaValidity.Valid)
                    {
                        memberType = schemaInfo.MemberType;
                        if (memberType == null)
                        {
                            memberType = schemaInfo.SchemaType;
                        }
                        if (memberType != null)
                        {
                            datatype = memberType.Datatype;
                            if (datatype != null)
                            {
                                return datatype.ValueType;
                            }
                        }
                    }
                    else
                    {
                        memberType = schemaInfo.SchemaType;
                        if (memberType != null)
                        {
                            datatype = memberType.Datatype;
                            if (datatype != null)
                            {
                                return datatype.ValueType;
                            }
                        }
                    }
                }
                return typeof(string);
            }
        }

        public virtual string XmlLang
        {
            get
            {
                XPathNavigator navigator = this.Clone();
                do
                {
                    if (navigator.MoveToAttribute("lang", "http://www.w3.org/XML/1998/namespace"))
                    {
                        return navigator.Value;
                    }
                }
                while (navigator.MoveToParent());
                return string.Empty;
            }
        }

        public override XmlSchemaType XmlType
        {
            get
            {
                IXmlSchemaInfo schemaInfo = this.SchemaInfo;
                if ((schemaInfo == null) || (schemaInfo.Validity != XmlSchemaValidity.Valid))
                {
                    return null;
                }
                XmlSchemaType memberType = schemaInfo.MemberType;
                if (memberType != null)
                {
                    return memberType;
                }
                return schemaInfo.SchemaType;
            }
        }

        private class CheckValidityHelper
        {
            private bool isValid = true;
            private ValidationEventHandler nextEventHandler;
            private XPathNavigatorReader reader;

            internal CheckValidityHelper(ValidationEventHandler nextEventHandler, XPathNavigatorReader reader)
            {
                this.nextEventHandler = nextEventHandler;
                this.reader = reader;
            }

            internal void ValidationCallback(object sender, ValidationEventArgs args)
            {
                if (args.Severity == XmlSeverityType.Error)
                {
                    this.isValid = false;
                }
                XmlSchemaValidationException exception = args.Exception as XmlSchemaValidationException;
                if ((exception != null) && (this.reader != null))
                {
                    exception.SetSourceObject(this.reader.UnderlyingObject);
                }
                if (this.nextEventHandler != null)
                {
                    this.nextEventHandler(sender, args);
                }
                else if ((exception != null) && (args.Severity == XmlSeverityType.Error))
                {
                    throw exception;
                }
            }

            internal bool IsValid
            {
                get
                {
                    return this.isValid;
                }
            }
        }

        [StructLayout(LayoutKind.Sequential), DebuggerDisplay("{ToString()}")]
        internal struct DebuggerDisplayProxy
        {
            private XPathNavigator nav;
            public DebuggerDisplayProxy(XPathNavigator nav)
            {
                this.nav = nav;
            }

            public override string ToString()
            {
                string str = this.nav.NodeType.ToString();
                switch (this.nav.NodeType)
                {
                    case XPathNodeType.Element:
                    {
                        object obj2 = str;
                        return string.Concat(new object[] { obj2, ", Name=\"", this.nav.Name, '"' });
                    }
                    case XPathNodeType.Attribute:
                    case XPathNodeType.Namespace:
                    case XPathNodeType.ProcessingInstruction:
                    {
                        object obj3 = str;
                        object obj4 = string.Concat(new object[] { obj3, ", Name=\"", this.nav.Name, '"' });
                        return string.Concat(new object[] { obj4, ", Value=\"", XmlConvert.EscapeValueForDebuggerDisplay(this.nav.Value), '"' });
                    }
                    case XPathNodeType.Text:
                    case XPathNodeType.SignificantWhitespace:
                    case XPathNodeType.Whitespace:
                    case XPathNodeType.Comment:
                    {
                        object obj5 = str;
                        return string.Concat(new object[] { obj5, ", Value=\"", XmlConvert.EscapeValueForDebuggerDisplay(this.nav.Value), '"' });
                    }
                }
                return str;
            }
        }
    }
}

