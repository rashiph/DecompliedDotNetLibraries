namespace MS.Internal.Xaml.Parser
{
    using MS.Internal.Xaml.Context;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Xaml;
    using System.Xaml.Schema;

    internal class XamlPullParser
    {
        private XamlType _arrayExtensionType;
        private XamlMember _arrayTypeMember;
        private XamlParserContext _context;
        private XamlMember _itemsTypeMember;
        private XamlXmlReaderSettings _settings;
        private XamlScanner _xamlScanner;
        private readonly XamlTypeName arrayType = new XamlTypeName("http://schemas.microsoft.com/winfx/2006/xaml", "Array");

        public XamlPullParser(XamlParserContext context, XamlScanner scanner, XamlXmlReaderSettings settings)
        {
            this._context = context;
            this._xamlScanner = scanner;
            this._settings = settings;
        }

        private static bool CanAcceptString(XamlMember property)
        {
            if (property != null)
            {
                if (property.TypeConverter == BuiltInValueConverter.String)
                {
                    return true;
                }
                if (property.TypeConverter == BuiltInValueConverter.Object)
                {
                    return true;
                }
                XamlType type = property.Type;
                if (type.IsCollection)
                {
                    foreach (XamlType type2 in type.AllowedContentTypes)
                    {
                        if ((type2 == XamlLanguage.String) || (type2 == XamlLanguage.Object))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private string Logic_ApplyFinalTextTrimming(XamlText text)
        {
            ScannerNodeType peekNodeType = this._xamlScanner.PeekNodeType;
            string source = text.Text;
            if (!text.IsSpacePreserved)
            {
                switch (peekNodeType)
                {
                    case ScannerNodeType.ENDTAG:
                    case ScannerNodeType.PROPERTYELEMENT:
                    case ScannerNodeType.EMPTYPROPERTYELEMENT:
                        source = XamlText.TrimTrailingWhitespace(source);
                        break;
                }
                XamlType currentPreviousChildType = this._context.CurrentPreviousChildType;
                if ((currentPreviousChildType == null) || currentPreviousChildType.TrimSurroundingWhitespace)
                {
                    source = XamlText.TrimLeadingWhitespace(source);
                }
                if ((peekNodeType != ScannerNodeType.ELEMENT) && (peekNodeType != ScannerNodeType.EMPTYELEMENT))
                {
                    return source;
                }
                if (this._xamlScanner.PeekType.TrimSurroundingWhitespace)
                {
                    source = XamlText.TrimTrailingWhitespace(source);
                }
            }
            return source;
        }

        private XamlNode Logic_EndMember()
        {
            this._context.CurrentMember = null;
            this._context.CurrentPreviousChildType = null;
            this._context.CurrentInContainerDirective = false;
            return new XamlNode(XamlNodeType.EndMember);
        }

        private XamlNode Logic_EndObject()
        {
            XamlType currentType = this._context.CurrentType;
            this._context.PopScope();
            this._context.CurrentPreviousChildType = currentType;
            return new XamlNode(XamlNodeType.EndObject);
        }

        private XamlNode Logic_EndOfAttributes()
        {
            return new XamlNode(XamlNode.InternalNodeType.EndOfAttributes);
        }

        private bool Logic_IsDiscardableWhitespace(XamlText text)
        {
            if (!text.IsWhiteSpaceOnly)
            {
                return false;
            }
            if ((this._context.CurrentMember != null) && this._context.CurrentMember.IsUnknown)
            {
                return false;
            }
            if (this._context.CurrentInContainerDirective)
            {
                XamlType type = (this._context.CurrentMember == XamlLanguage.Items) ? this._context.CurrentType : this._context.CurrentMember.Type;
                if (type.IsWhitespaceSignificantCollection)
                {
                    return false;
                }
            }
            else
            {
                XamlMember currentMember = this._context.CurrentMember;
                if (this._xamlScanner.PeekNodeType == ScannerNodeType.ELEMENT)
                {
                    if (currentMember == null)
                    {
                        currentMember = this._context.CurrentType.ContentProperty;
                    }
                    if (((currentMember != null) && (currentMember.Type != null)) && currentMember.Type.IsWhitespaceSignificantCollection)
                    {
                        return false;
                    }
                    if ((currentMember == null) && this._context.CurrentType.IsWhitespaceSignificantCollection)
                    {
                        return false;
                    }
                }
                else if (text.IsSpacePreserved && (this._xamlScanner.PeekNodeType == ScannerNodeType.ENDTAG))
                {
                    if (currentMember != null)
                    {
                        if (this._context.CurrentPreviousChildType == null)
                        {
                            return false;
                        }
                    }
                    else if (this._context.CurrentType.ContentProperty != null)
                    {
                        currentMember = this._context.CurrentType.ContentProperty;
                        if (currentMember.Type == XamlLanguage.String)
                        {
                            return false;
                        }
                        if (currentMember.Type.IsWhitespaceSignificantCollection)
                        {
                            return false;
                        }
                    }
                    else if ((this._context.CurrentType.TypeConverter != null) && !this._context.CurrentForcedToUseConstructor)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private XamlNode Logic_LineInfo()
        {
            return new XamlNode(new LineInfo(this.LineNumber, this.LinePosition));
        }

        private XamlNode Logic_PrefixDefinition()
        {
            string prefix = this._xamlScanner.Prefix;
            string ns = this._xamlScanner.Namespace;
            return new XamlNode(XamlNodeType.NamespaceDeclaration, new NamespaceDeclaration(ns, prefix));
        }

        private XamlNode Logic_StartContentProperty(XamlMember property)
        {
            if (property == null)
            {
                property = XamlLanguage.UnknownContent;
            }
            this._context.CurrentMember = property;
            return new XamlNode(XamlNodeType.StartMember, property);
        }

        private XamlNode Logic_StartGetObjectFromMember(XamlType realType)
        {
            this._context.PushScope();
            this._context.CurrentType = realType;
            this._context.CurrentInCollectionFromMember = true;
            return new XamlNode(XamlNodeType.GetObject);
        }

        private XamlNode Logic_StartInitProperty(XamlType ownerType)
        {
            XamlDirective initialization = XamlLanguage.Initialization;
            this._context.CurrentMember = initialization;
            return new XamlNode(XamlNodeType.StartMember, initialization);
        }

        private XamlNode Logic_StartItemsProperty(XamlType collectionType)
        {
            this._context.CurrentMember = XamlLanguage.Items;
            this._context.CurrentInContainerDirective = true;
            return new XamlNode(XamlNodeType.StartMember, XamlLanguage.Items);
        }

        private XamlNode Logic_StartMember(XamlMember member)
        {
            this._context.CurrentMember = member;
            if (this._xamlScanner.IsCtorForcingMember)
            {
                this._context.CurrentForcedToUseConstructor = true;
            }
            XamlType type = member.Type;
            this._context.CurrentInContainerDirective = member.IsDirective && ((type != null) && (type.IsCollection || type.IsDictionary));
            return new XamlNode(XamlNodeType.StartMember, member);
        }

        private XamlNode Logic_StartObject(XamlType xamlType, string xamlNamespace)
        {
            this._context.PushScope();
            this._context.CurrentType = xamlType;
            this._context.CurrentTypeNamespace = xamlNamespace;
            return new XamlNode(XamlNodeType.StartObject, xamlType);
        }

        private IEnumerable<XamlNode> LogicStream_Attribute()
        {
            XamlMember propertyAttribute = this._xamlScanner.PropertyAttribute;
            XamlText propertyAttributeText = this._xamlScanner.PropertyAttributeText;
            if (this._xamlScanner.IsCtorForcingMember)
            {
                this._context.CurrentForcedToUseConstructor = true;
            }
            XamlNode iteratorVariable2 = new XamlNode(XamlNodeType.StartMember, propertyAttribute);
            yield return iteratorVariable2;
            if (propertyAttributeText.LooksLikeAMarkupExtension)
            {
                MePullParser iteratorVariable3 = new MePullParser(this._context);
                foreach (XamlNode iteratorVariable4 in iteratorVariable3.Parse(propertyAttributeText.Text, this.LineNumber, this.LinePosition))
                {
                    yield return iteratorVariable4;
                }
            }
            else
            {
                XamlNode iteratorVariable5 = new XamlNode(XamlNodeType.Value, propertyAttributeText.AttributeText);
                yield return iteratorVariable5;
            }
            yield return new XamlNode(XamlNodeType.EndMember);
        }

        private IEnumerable<XamlNode> LogicStream_CheckForStartGetCollectionFromMember()
        {
            XamlType type1 = this._context.CurrentType;
            XamlMember currentMember = this._context.CurrentMember;
            XamlType type = currentMember.Type;
            XamlType iteratorVariable2 = (this._xamlScanner.NodeType == ScannerNodeType.TEXT) ? XamlLanguage.String : this._xamlScanner.Type;
            if (type.IsArray && (this._xamlScanner.Type != this.ArrayExtensionType))
            {
                IEnumerable<NamespaceDeclaration> newNamespaces = null;
                XamlTypeName iteratorVariable4 = new XamlTypeName(type.ItemType);
                INamespacePrefixLookup prefixLookup = new NamespacePrefixLookup(out newNamespaces, new Func<string, string>(this._context.FindNamespaceByPrefix));
                string data = iteratorVariable4.ToString(prefixLookup);
                foreach (NamespaceDeclaration iteratorVariable7 in newNamespaces)
                {
                    yield return new XamlNode(XamlNodeType.NamespaceDeclaration, iteratorVariable7);
                }
                yield return this.Logic_StartObject(this.ArrayExtensionType, null);
                this._context.CurrentInImplicitArray = true;
                yield return this.Logic_StartMember(this.ArrayTypeMember);
                yield return new XamlNode(XamlNodeType.Value, data);
                yield return this.Logic_EndMember();
                yield return this.Logic_EndOfAttributes();
                yield return this.Logic_StartMember(this.ItemsTypeMember);
                XamlType currentType = this._context.CurrentType;
                currentMember = this._context.CurrentMember;
                type = currentMember.Type;
            }
            if (!currentMember.IsDirective && (type.IsCollection || type.IsDictionary))
            {
                bool iteratorVariable8 = false;
                if (currentMember.IsReadOnly || !this._context.CurrentMemberIsWriteVisible())
                {
                    iteratorVariable8 = true;
                }
                else if (((type.TypeConverter != null) && !currentMember.IsReadOnly) && (this._xamlScanner.NodeType == ScannerNodeType.TEXT))
                {
                    iteratorVariable8 = false;
                }
                else if (((iteratorVariable2 == null) || !iteratorVariable2.CanAssignTo(type)) && (iteratorVariable2 != null))
                {
                    if (!iteratorVariable2.IsMarkupExtension || this._xamlScanner.HasKeyAttribute)
                    {
                        iteratorVariable8 = true;
                    }
                    else if (iteratorVariable2 == XamlLanguage.Array)
                    {
                        iteratorVariable8 = true;
                    }
                }
                if (iteratorVariable8)
                {
                    yield return this.Logic_StartGetObjectFromMember(type);
                    yield return this.Logic_StartItemsProperty(type);
                }
            }
        }

        public IEnumerable<XamlNode> P_Element()
        {
            ScannerNodeType nodeType = this._xamlScanner.NodeType;
            switch (nodeType)
            {
                case ScannerNodeType.ELEMENT:
                {
                    IEnumerator<XamlNode> enumerator = this.P_StartElement().GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        XamlNode current = enumerator.Current;
                        yield return current;
                    }
                    foreach (XamlNode iteratorVariable3 in this.P_ElementBody())
                    {
                        yield return iteratorVariable3;
                    }
                    break;
                }
                default:
                    throw new XamlUnexpectedParseException(this._xamlScanner, nodeType, System.Xaml.SR.Get("ElementRuleException"));
                    foreach (XamlNode iteratorVariable1 in this.P_EmptyElement())
                    {
                        yield return iteratorVariable1;
                    }
                    break;
            }
        }

        public IEnumerable<XamlNode> P_ElementBody()
        {
            IEnumerator<XamlNode> iteratorVariable10;
            XamlNode iteratorVariable4;
            ScannerNodeType nodeType;
            while (this._xamlScanner.NodeType == ScannerNodeType.ATTRIBUTE)
            {
                IEnumerator<XamlNode> enumerator = this.LogicStream_Attribute().GetEnumerator();
                while (enumerator.MoveNext())
                {
                    XamlNode current = enumerator.Current;
                    yield return current;
                }
                this._xamlScanner.Read();
                if (this.ProvideLineInfo)
                {
                    yield return this.Logic_LineInfo();
                }
            }
            yield return this.Logic_EndOfAttributes();
            bool iteratorVariable1 = false;
            bool iteratorVariable2 = false;
        Label_015B:
            nodeType = this._xamlScanner.NodeType;
            switch (nodeType)
            {
                case ScannerNodeType.ELEMENT:
                case ScannerNodeType.EMPTYELEMENT:
                case ScannerNodeType.PREFIXDEFINITION:
                case ScannerNodeType.TEXT:
                    iteratorVariable2 = true;
                    do
                    {
                        foreach (XamlNode iteratorVariable5 in this.P_ElementContent())
                        {
                            yield return iteratorVariable5;
                        }
                        nodeType = this._xamlScanner.NodeType;
                    }
                    while (((nodeType == ScannerNodeType.PREFIXDEFINITION) || (nodeType == ScannerNodeType.ELEMENT)) || ((nodeType == ScannerNodeType.EMPTYELEMENT) || (nodeType == ScannerNodeType.TEXT)));
                    if ((this._context.CurrentInItemsProperty || this._context.CurrentInInitProperty) || this._context.CurrentIsUnknownContent)
                    {
                        yield return this.Logic_EndMember();
                        if (this._context.CurrentInCollectionFromMember)
                        {
                            yield return this.Logic_EndObject();
                            yield return this.Logic_EndMember();
                            this._context.CurrentInCollectionFromMember = false;
                            if (this._context.CurrentInImplicitArray)
                            {
                                this._context.CurrentInImplicitArray = false;
                                yield return this.Logic_EndObject();
                                yield return this.Logic_EndMember();
                            }
                        }
                    }
                    goto Label_0511;

                case ScannerNodeType.PROPERTYELEMENT:
                case ScannerNodeType.EMPTYPROPERTYELEMENT:
                    iteratorVariable2 = true;
                    iteratorVariable10 = this.P_PropertyElement().GetEnumerator();
                    goto Label_0201;

                case ScannerNodeType.ENDTAG:
                {
                    XamlType currentType = this._context.CurrentType;
                    bool iteratorVariable7 = currentType.TypeConverter != null;
                    bool iteratorVariable8 = currentType.IsConstructible && !currentType.ConstructionRequiresArguments;
                    if ((!iteratorVariable2 && iteratorVariable7) && !iteratorVariable8)
                    {
                        yield return this.Logic_StartInitProperty(currentType);
                        yield return new XamlNode(XamlNodeType.Value, string.Empty);
                        yield return this.Logic_EndMember();
                    }
                    iteratorVariable1 = true;
                    goto Label_0511;
                }
                default:
                    iteratorVariable1 = true;
                    goto Label_0511;
            }
        Label_01CF:
            iteratorVariable4 = iteratorVariable10.Current;
            yield return iteratorVariable4;
        Label_0201:
            if (iteratorVariable10.MoveNext())
            {
                goto Label_01CF;
            }
        Label_0511:
            if (!iteratorVariable1)
            {
                goto Label_015B;
            }
            if (this._xamlScanner.NodeType != ScannerNodeType.ENDTAG)
            {
                throw new XamlUnexpectedParseException(this._xamlScanner, this._xamlScanner.NodeType, System.Xaml.SR.Get("ElementBodyRuleException"));
            }
            yield return this.Logic_EndObject();
            this._xamlScanner.Read();
            if (this.ProvideLineInfo)
            {
                yield return this.Logic_LineInfo();
            }
        }

        public IEnumerable<XamlNode> P_ElementContent()
        {
            XamlType currentType = this._context.CurrentType;
            List<XamlNode> iteratorVariable1 = null;
            ScannerNodeType nodeType = this._xamlScanner.NodeType;
            switch (nodeType)
            {
                default:
                {
                    goto Label_0727;
                    if (nodeType != ScannerNodeType.TEXT)
                    {
                        break;
                    }
                    XamlText textContent = this._xamlScanner.TextContent;
                    if (!this.Logic_IsDiscardableWhitespace(textContent))
                    {
                        break;
                    }
                    this._xamlScanner.Read();
                    if (this.ProvideLineInfo)
                    {
                        yield return this.Logic_LineInfo();
                    }
                    goto Label_0727;
                }
            }
            while (nodeType == ScannerNodeType.PREFIXDEFINITION)
            {
                if (iteratorVariable1 == null)
                {
                    iteratorVariable1 = new List<XamlNode>();
                }
                iteratorVariable1.Add(this.Logic_PrefixDefinition());
                if (this.ProvideLineInfo)
                {
                    iteratorVariable1.Add(this.Logic_LineInfo());
                }
                this._xamlScanner.Read();
                if (this.ProvideLineInfo)
                {
                    yield return this.Logic_LineInfo();
                }
                nodeType = this._xamlScanner.NodeType;
            }
            bool iteratorVariable4 = false;
            if (!this._context.CurrentInItemsProperty)
            {
                bool iteratorVariable5 = false;
                if (nodeType == ScannerNodeType.TEXT)
                {
                    if ((currentType.ContentProperty != null) && CanAcceptString(currentType.ContentProperty))
                    {
                        iteratorVariable5 = true;
                    }
                    else if ((!this._context.CurrentForcedToUseConstructor && !this._xamlScanner.TextContent.IsEmpty) && (currentType.TypeConverter != null))
                    {
                        iteratorVariable4 = true;
                    }
                }
                if (!iteratorVariable4 && !iteratorVariable5)
                {
                    if (currentType.IsCollection || currentType.IsDictionary)
                    {
                        yield return this.Logic_StartItemsProperty(currentType);
                    }
                    else
                    {
                        iteratorVariable5 = true;
                    }
                }
                if (iteratorVariable5 && !this._context.CurrentIsUnknownContent)
                {
                    XamlMember contentProperty = currentType.ContentProperty;
                    if ((contentProperty != null) && !this._context.IsVisible(contentProperty, this._context.CurrentTypeIsRoot ? this._context.CurrentType : null))
                    {
                        contentProperty = new XamlMember(contentProperty.Name, currentType, false);
                    }
                    yield return this.Logic_StartContentProperty(contentProperty);
                    foreach (XamlNode iteratorVariable7 in this.LogicStream_CheckForStartGetCollectionFromMember())
                    {
                        yield return iteratorVariable7;
                    }
                }
            }
            if (iteratorVariable1 != null)
            {
                for (int i = 0; i < iteratorVariable1.Count; i++)
                {
                    yield return iteratorVariable1[i];
                }
            }
            if (nodeType == ScannerNodeType.TEXT)
            {
                XamlText text = this._xamlScanner.TextContent;
                string data = this.Logic_ApplyFinalTextTrimming(text);
                bool isTextXML = this._xamlScanner.IsTextXML;
                this._xamlScanner.Read();
                if (this.ProvideLineInfo)
                {
                    yield return this.Logic_LineInfo();
                }
                if (data == string.Empty)
                {
                    goto Label_0727;
                }
                if (iteratorVariable4)
                {
                    yield return this.Logic_StartInitProperty(currentType);
                }
                if (isTextXML)
                {
                    yield return this.Logic_StartObject(XamlLanguage.XData, null);
                    XamlMember member = XamlLanguage.XData.GetMember("Text");
                    yield return this.Logic_EndOfAttributes();
                    yield return this.Logic_StartMember(member);
                }
                yield return new XamlNode(XamlNodeType.Value, data);
                if (isTextXML)
                {
                    yield return this.Logic_EndMember();
                    yield return this.Logic_EndObject();
                }
            }
            else
            {
                IEnumerator<XamlNode> enumerator = this.P_Element().GetEnumerator();
                while (enumerator.MoveNext())
                {
                    XamlNode current = enumerator.Current;
                    yield return current;
                }
            }
            if (!this._context.CurrentInItemsProperty && !this._context.CurrentIsUnknownContent)
            {
                yield return this.Logic_EndMember();
            }
        Label_0727:
            yield break;
        }

        public IEnumerable<XamlNode> P_EmptyElement()
        {
            if (this._xamlScanner.NodeType != ScannerNodeType.EMPTYELEMENT)
            {
                throw new XamlUnexpectedParseException(this._xamlScanner, this._xamlScanner.NodeType, System.Xaml.SR.Get("EmptyElementRuleException"));
            }
            yield return this.Logic_StartObject(this._xamlScanner.Type, this._xamlScanner.Namespace);
            this._xamlScanner.Read();
            if (this.ProvideLineInfo)
            {
                yield return this.Logic_LineInfo();
            }
            while (this._xamlScanner.NodeType == ScannerNodeType.DIRECTIVE)
            {
                IEnumerator<XamlNode> enumerator = this.LogicStream_Attribute().GetEnumerator();
                while (enumerator.MoveNext())
                {
                    XamlNode current = enumerator.Current;
                    yield return current;
                }
                this._xamlScanner.Read();
                if (this.ProvideLineInfo)
                {
                    yield return this.Logic_LineInfo();
                }
            }
            while (this._xamlScanner.NodeType == ScannerNodeType.ATTRIBUTE)
            {
                foreach (XamlNode iteratorVariable1 in this.LogicStream_Attribute())
                {
                    yield return iteratorVariable1;
                }
                this._xamlScanner.Read();
                if (this.ProvideLineInfo)
                {
                    yield return this.Logic_LineInfo();
                }
            }
            yield return this.Logic_EndOfAttributes();
            yield return this.Logic_EndObject();
        }

        public IEnumerable<XamlNode> P_EmptyPropertyElement()
        {
            if (this._xamlScanner.NodeType != ScannerNodeType.EMPTYPROPERTYELEMENT)
            {
                throw new XamlUnexpectedParseException(this._xamlScanner, this._xamlScanner.NodeType, System.Xaml.SR.Get("EmptyPropertyElementRuleException"));
            }
            yield return this.Logic_StartMember(this._xamlScanner.PropertyElement);
            yield return this.Logic_EndMember();
            this._xamlScanner.Read();
            if (this.ProvideLineInfo)
            {
                yield return this.Logic_LineInfo();
            }
        }

        public IEnumerable<XamlNode> P_NonemptyPropertyElement()
        {
            ScannerNodeType nodeType;
            if (this._xamlScanner.NodeType != ScannerNodeType.PROPERTYELEMENT)
            {
                throw new XamlUnexpectedParseException(this._xamlScanner, this._xamlScanner.NodeType, System.Xaml.SR.Get("NonemptyPropertyElementRuleException"));
            }
            yield return this.Logic_StartMember(this._xamlScanner.PropertyElement);
            this._xamlScanner.Read();
            if (this.ProvideLineInfo)
            {
                yield return this.Logic_LineInfo();
            }
            bool iteratorVariable0 = true;
        Label_0103:
            nodeType = this._xamlScanner.NodeType;
            switch (nodeType)
            {
                case ScannerNodeType.ELEMENT:
                case ScannerNodeType.EMPTYELEMENT:
                case ScannerNodeType.PREFIXDEFINITION:
                case ScannerNodeType.TEXT:
                    break;

                default:
                    iteratorVariable0 = false;
                    goto Label_02FE;
            }
            do
            {
                foreach (XamlNode iteratorVariable2 in this.P_PropertyContent())
                {
                    yield return iteratorVariable2;
                }
                nodeType = this._xamlScanner.NodeType;
            }
            while (((nodeType == ScannerNodeType.PREFIXDEFINITION) || (nodeType == ScannerNodeType.ELEMENT)) || ((nodeType == ScannerNodeType.EMPTYELEMENT) || (nodeType == ScannerNodeType.TEXT)));
            if (this._context.CurrentInItemsProperty || this._context.CurrentInInitProperty)
            {
                yield return this.Logic_EndMember();
                if (this._context.CurrentInCollectionFromMember)
                {
                    yield return this.Logic_EndObject();
                    this._context.CurrentInCollectionFromMember = false;
                    if (this._context.CurrentInImplicitArray)
                    {
                        this._context.CurrentInImplicitArray = false;
                        yield return this.Logic_EndMember();
                        yield return this.Logic_EndObject();
                    }
                }
            }
        Label_02FE:
            if (iteratorVariable0)
            {
                goto Label_0103;
            }
            if (this._xamlScanner.NodeType != ScannerNodeType.ENDTAG)
            {
                throw new XamlUnexpectedParseException(this._xamlScanner, this._xamlScanner.NodeType, System.Xaml.SR.Get("NonemptyPropertyElementRuleException"));
            }
            yield return this.Logic_EndMember();
            this._xamlScanner.Read();
            if (this.ProvideLineInfo)
            {
                yield return this.Logic_LineInfo();
            }
        }

        public IEnumerable<XamlNode> P_PropertyContent()
        {
            ScannerNodeType nodeType = this._xamlScanner.NodeType;
            List<XamlNode> iteratorVariable1 = null;
            string data = string.Empty;
            bool isTextXML = false;
            switch (nodeType)
            {
                default:
                {
                    goto Label_04E6;
                    if (nodeType != ScannerNodeType.TEXT)
                    {
                        break;
                    }
                    XamlText textContent = this._xamlScanner.TextContent;
                    if (this.Logic_IsDiscardableWhitespace(textContent))
                    {
                        data = string.Empty;
                    }
                    else
                    {
                        data = this.Logic_ApplyFinalTextTrimming(textContent);
                    }
                    isTextXML = this._xamlScanner.IsTextXML;
                    this._xamlScanner.Read();
                    if (this.ProvideLineInfo)
                    {
                        yield return this.Logic_LineInfo();
                    }
                    if (!(data == string.Empty))
                    {
                        break;
                    }
                    goto Label_04E6;
                }
            }
            while (nodeType == ScannerNodeType.PREFIXDEFINITION)
            {
                if (iteratorVariable1 == null)
                {
                    iteratorVariable1 = new List<XamlNode>();
                }
                iteratorVariable1.Add(this.Logic_PrefixDefinition());
                if (this.ProvideLineInfo)
                {
                    iteratorVariable1.Add(this.Logic_LineInfo());
                }
                this._xamlScanner.Read();
                if (this.ProvideLineInfo)
                {
                    yield return this.Logic_LineInfo();
                }
                nodeType = this._xamlScanner.NodeType;
            }
            if ((nodeType == ScannerNodeType.TEXT) && (this._context.CurrentMember.TypeConverter != null))
            {
                yield return new XamlNode(XamlNodeType.Value, data);
            }
            else
            {
                if (!this._context.CurrentInCollectionFromMember)
                {
                    foreach (XamlNode iteratorVariable5 in this.LogicStream_CheckForStartGetCollectionFromMember())
                    {
                        yield return iteratorVariable5;
                    }
                }
                if (nodeType == ScannerNodeType.TEXT)
                {
                    if (isTextXML)
                    {
                        yield return this.Logic_StartObject(XamlLanguage.XData, null);
                        XamlMember member = XamlLanguage.XData.GetMember("Text");
                        yield return this.Logic_EndOfAttributes();
                        yield return this.Logic_StartMember(member);
                    }
                    yield return new XamlNode(XamlNodeType.Value, data);
                    if (isTextXML)
                    {
                        yield return this.Logic_EndMember();
                        yield return this.Logic_EndObject();
                    }
                }
                else
                {
                    if (iteratorVariable1 != null)
                    {
                        for (int i = 0; i < iteratorVariable1.Count; i++)
                        {
                            yield return iteratorVariable1[i];
                        }
                    }
                    foreach (XamlNode iteratorVariable8 in this.P_Element())
                    {
                        yield return iteratorVariable8;
                    }
                }
            }
        Label_04E6:
            yield break;
        }

        public IEnumerable<XamlNode> P_PropertyElement()
        {
            ScannerNodeType nodeType = this._xamlScanner.NodeType;
            switch (nodeType)
            {
                case ScannerNodeType.PROPERTYELEMENT:
                {
                    IEnumerator<XamlNode> enumerator = this.P_NonemptyPropertyElement().GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        XamlNode current = enumerator.Current;
                        yield return current;
                    }
                    break;
                }
                default:
                    throw new XamlUnexpectedParseException(this._xamlScanner, nodeType, System.Xaml.SR.Get("PropertyElementRuleException"));
                    foreach (XamlNode iteratorVariable1 in this.P_EmptyPropertyElement())
                    {
                        yield return iteratorVariable1;
                    }
                    break;
            }
        }

        public IEnumerable<XamlNode> P_StartElement()
        {
            if (this._xamlScanner.NodeType != ScannerNodeType.ELEMENT)
            {
                throw new XamlUnexpectedParseException(this._xamlScanner, this._xamlScanner.NodeType, System.Xaml.SR.Get("StartElementRuleException"));
            }
            yield return this.Logic_StartObject(this._xamlScanner.Type, this._xamlScanner.Namespace);
            this._xamlScanner.Read();
            if (this.ProvideLineInfo)
            {
                yield return this.Logic_LineInfo();
            }
            while (this._xamlScanner.NodeType == ScannerNodeType.DIRECTIVE)
            {
                IEnumerator<XamlNode> enumerator = this.LogicStream_Attribute().GetEnumerator();
                while (enumerator.MoveNext())
                {
                    XamlNode current = enumerator.Current;
                    yield return current;
                }
                this._xamlScanner.Read();
                if (this.ProvideLineInfo)
                {
                    yield return this.Logic_LineInfo();
                }
            }
        }

        public IEnumerable<XamlNode> Parse()
        {
            this._xamlScanner.Read();
            if (this.ProvideLineInfo)
            {
                yield return this.Logic_LineInfo();
            }
            for (ScannerNodeType iteratorVariable0 = this._xamlScanner.NodeType; iteratorVariable0 == ScannerNodeType.PREFIXDEFINITION; iteratorVariable0 = this._xamlScanner.NodeType)
            {
                yield return this.Logic_PrefixDefinition();
                this._xamlScanner.Read();
                if (this.ProvideLineInfo)
                {
                    yield return this.Logic_LineInfo();
                }
            }
            foreach (XamlNode iteratorVariable1 in this.P_Element())
            {
                yield return iteratorVariable1;
            }
        }

        private XamlType ArrayExtensionType
        {
            get
            {
                if (this._arrayExtensionType == null)
                {
                    this._arrayExtensionType = this._context.GetXamlType(this.arrayType);
                }
                return this._arrayExtensionType;
            }
        }

        private XamlMember ArrayTypeMember
        {
            get
            {
                if (this._arrayTypeMember == null)
                {
                    this._arrayTypeMember = this._context.GetXamlProperty(this.ArrayExtensionType, "Type", null);
                }
                return this._arrayTypeMember;
            }
        }

        private XamlMember ItemsTypeMember
        {
            get
            {
                if (this._itemsTypeMember == null)
                {
                    this._itemsTypeMember = this._context.GetXamlProperty(this.ArrayExtensionType, "Items", null);
                }
                return this._itemsTypeMember;
            }
        }

        private int LineNumber
        {
            get
            {
                return this._xamlScanner.LineNumber;
            }
        }

        private int LinePosition
        {
            get
            {
                return this._xamlScanner.LinePosition;
            }
        }

        private bool ProvideLineInfo
        {
            get
            {
                return this._settings.ProvideLineInfo;
            }
        }












    }
}

