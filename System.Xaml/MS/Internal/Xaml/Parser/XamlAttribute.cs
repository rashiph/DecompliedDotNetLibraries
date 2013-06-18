namespace MS.Internal.Xaml.Parser
{
    using MS.Internal.Xaml.Context;
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Xaml;
    using System.Xaml.MS.Impl;
    using System.Xml;

    [DebuggerDisplay("{Name.ScopedName}='{Value}'  {Kind}")]
    internal class XamlAttribute
    {
        private string _xmlnsDefinitionPrefix;
        private string _xmlnsDefinitionUri;

        public XamlAttribute(XamlPropertyName propName, string val, IXmlLineInfo lineInfo)
        {
            this.Name = propName;
            this.Value = val;
            this.Kind = ScannerAttributeKind.Property;
            if (lineInfo != null)
            {
                this.LineNumber = lineInfo.LineNumber;
                this.LinePosition = lineInfo.LinePosition;
            }
            if (this.CheckIsXmlNamespaceDefinition(out this._xmlnsDefinitionPrefix, out this._xmlnsDefinitionUri))
            {
                this.Kind = ScannerAttributeKind.Namespace;
            }
        }

        internal bool CheckIsXmlNamespaceDefinition(out string definingPrefix, out string uri)
        {
            uri = string.Empty;
            definingPrefix = string.Empty;
            if (KS.Eq(this.Name.Prefix, "xmlns"))
            {
                uri = this.Value;
                definingPrefix = !this.Name.IsDotted ? this.Name.Name : (this.Name.OwnerName + "." + this.Name.Name);
                return true;
            }
            if (string.IsNullOrEmpty(this.Name.Prefix) && KS.Eq(this.Name.Name, "xmlns"))
            {
                uri = this.Value;
                definingPrefix = string.Empty;
                return true;
            }
            return false;
        }

        private XamlMember GetXamlAttributeProperty(XamlParserContext context, XamlPropertyName propName, XamlType tagType, string tagNamespace, bool tagIsRoot)
        {
            string attributeNamespace = context.GetAttributeNamespace(propName, tagNamespace);
            if (attributeNamespace == null)
            {
                if (propName.IsDotted)
                {
                    return new XamlMember(propName.Name, new XamlType(string.Empty, propName.OwnerName, null, context.SchemaContext), true);
                }
                return new XamlMember(propName.Name, tagType, false);
            }
            if (propName.IsDotted)
            {
                return context.GetDottedProperty(tagType, tagNamespace, propName, tagIsRoot);
            }
            return context.GetNoDotAttributeProperty(tagType, propName, tagNamespace, attributeNamespace, tagIsRoot);
        }

        public void Initialize(XamlParserContext context, XamlType tagType, string ownerNamespace, bool tagIsRoot)
        {
            if (this.Kind != ScannerAttributeKind.Namespace)
            {
                this.Property = this.GetXamlAttributeProperty(context, this.Name, tagType, ownerNamespace, tagIsRoot);
                if (this.Property.IsUnknown)
                {
                    this.Kind = ScannerAttributeKind.Unknown;
                }
                else if (this.Property.IsEvent)
                {
                    this.Kind = ScannerAttributeKind.Event;
                }
                else if (this.Property.IsDirective)
                {
                    if (this.Property == XamlLanguage.Space)
                    {
                        this.Kind = ScannerAttributeKind.XmlSpace;
                    }
                    else if (((this.Property == XamlLanguage.FactoryMethod) || (this.Property == XamlLanguage.Arguments)) || ((this.Property == XamlLanguage.TypeArguments) || (this.Property == XamlLanguage.Base)))
                    {
                        this.Kind = ScannerAttributeKind.CtorDirective;
                    }
                    else
                    {
                        this.Kind = ScannerAttributeKind.Directive;
                    }
                }
                else if (this.Property.IsAttachable)
                {
                    this.Kind = ScannerAttributeKind.AttachableProperty;
                }
                else if (this.Property == tagType.GetAliasedProperty(XamlLanguage.Name))
                {
                    this.Kind = ScannerAttributeKind.Name;
                }
                else
                {
                    this.Kind = ScannerAttributeKind.Property;
                }
            }
        }

        public ScannerAttributeKind Kind { get; private set; }

        public int LineNumber { get; private set; }

        public int LinePosition { get; private set; }

        public XamlPropertyName Name { get; private set; }

        public XamlMember Property { get; private set; }

        public string Value { get; private set; }

        public string XmlNsPrefixDefined
        {
            get
            {
                return this._xmlnsDefinitionPrefix;
            }
        }

        public string XmlNsUriDefined
        {
            get
            {
                return this._xmlnsDefinitionUri;
            }
        }
    }
}

