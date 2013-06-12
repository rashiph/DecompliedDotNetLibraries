namespace System.Xml
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Xml.Schema;

    internal sealed class XmlValidatingReaderImpl : XmlReader, IXmlLineInfo, IXmlNamespaceResolver
    {
        private XmlReader coreReader;
        private XmlTextReaderImpl coreReaderImpl;
        private IXmlNamespaceResolver coreReaderNSResolver;
        private ValidationEventHandling eventHandling;
        private XmlReader outerReader;
        private XmlParserContext parserContext;
        private ParsingFunction parsingFunction;
        private bool processIdentityConstraints;
        private ReadContentAsBinaryHelper readBinaryHelper;
        private XmlSchemaCollection schemaCollection;
        private System.Xml.ValidationType validationType;
        private BaseValidator validator;

        internal event System.Xml.Schema.ValidationEventHandler ValidationEventHandler
        {
            add
            {
                this.eventHandling.AddHandler(value);
            }
            remove
            {
                this.eventHandling.RemoveHandler(value);
            }
        }

        internal XmlValidatingReaderImpl(XmlReader reader)
        {
            this.parsingFunction = ParsingFunction.Init;
            this.outerReader = this;
            this.coreReader = reader;
            this.coreReaderNSResolver = reader as IXmlNamespaceResolver;
            this.coreReaderImpl = reader as XmlTextReaderImpl;
            if (this.coreReaderImpl == null)
            {
                XmlTextReader reader2 = reader as XmlTextReader;
                if (reader2 != null)
                {
                    this.coreReaderImpl = reader2.Impl;
                }
            }
            if (this.coreReaderImpl == null)
            {
                throw new ArgumentException(Res.GetString("Arg_ExpectingXmlTextReader"), "reader");
            }
            this.coreReaderImpl.EntityHandling = System.Xml.EntityHandling.ExpandEntities;
            this.coreReaderImpl.XmlValidatingReaderCompatibilityMode = true;
            this.processIdentityConstraints = true;
            this.schemaCollection = new XmlSchemaCollection(this.coreReader.NameTable);
            this.schemaCollection.XmlResolver = this.GetResolver();
            this.eventHandling = new ValidationEventHandling(this);
            this.coreReaderImpl.ValidationEventHandling = this.eventHandling;
            this.coreReaderImpl.OnDefaultAttributeUse = new XmlTextReaderImpl.OnDefaultAttributeUseDelegate(this.ValidateDefaultAttributeOnUse);
            this.validationType = System.Xml.ValidationType.Auto;
            this.SetupValidation(System.Xml.ValidationType.Auto);
        }

        internal XmlValidatingReaderImpl(Stream xmlFragment, XmlNodeType fragType, XmlParserContext context) : this(new XmlTextReader(xmlFragment, fragType, context))
        {
            if (this.coreReader.BaseURI.Length > 0)
            {
                this.validator.BaseUri = this.GetResolver().ResolveUri(null, this.coreReader.BaseURI);
            }
            if (context != null)
            {
                this.parsingFunction = ParsingFunction.ParseDtdFromContext;
                this.parserContext = context;
            }
        }

        internal XmlValidatingReaderImpl(string xmlFragment, XmlNodeType fragType, XmlParserContext context) : this(new XmlTextReader(xmlFragment, fragType, context))
        {
            if (this.coreReader.BaseURI.Length > 0)
            {
                this.validator.BaseUri = this.GetResolver().ResolveUri(null, this.coreReader.BaseURI);
            }
            if (context != null)
            {
                this.parsingFunction = ParsingFunction.ParseDtdFromContext;
                this.parserContext = context;
            }
        }

        internal XmlValidatingReaderImpl(XmlReader reader, System.Xml.Schema.ValidationEventHandler settingsEventHandler, bool processIdentityConstraints)
        {
            this.parsingFunction = ParsingFunction.Init;
            this.outerReader = this;
            this.coreReader = reader;
            this.coreReaderImpl = reader as XmlTextReaderImpl;
            if (this.coreReaderImpl == null)
            {
                XmlTextReader reader2 = reader as XmlTextReader;
                if (reader2 != null)
                {
                    this.coreReaderImpl = reader2.Impl;
                }
            }
            if (this.coreReaderImpl == null)
            {
                throw new ArgumentException(Res.GetString("Arg_ExpectingXmlTextReader"), "reader");
            }
            this.coreReaderImpl.XmlValidatingReaderCompatibilityMode = true;
            this.coreReaderNSResolver = reader as IXmlNamespaceResolver;
            this.processIdentityConstraints = processIdentityConstraints;
            this.schemaCollection = new XmlSchemaCollection(this.coreReader.NameTable);
            this.schemaCollection.XmlResolver = this.GetResolver();
            this.eventHandling = new ValidationEventHandling(this);
            if (settingsEventHandler != null)
            {
                this.eventHandling.AddHandler(settingsEventHandler);
            }
            this.coreReaderImpl.ValidationEventHandling = this.eventHandling;
            this.coreReaderImpl.OnDefaultAttributeUse = new XmlTextReaderImpl.OnDefaultAttributeUseDelegate(this.ValidateDefaultAttributeOnUse);
            this.validationType = System.Xml.ValidationType.DTD;
            this.SetupValidation(System.Xml.ValidationType.DTD);
        }

        internal bool AddDefaultAttribute(SchemaAttDef attdef)
        {
            return this.coreReaderImpl.AddDefaultAttributeNonDtd(attdef);
        }

        public override void Close()
        {
            this.coreReader.Close();
            this.parsingFunction = ParsingFunction.ReaderClosed;
        }

        internal void Close(bool closeStream)
        {
            this.coreReaderImpl.Close(closeStream);
            this.parsingFunction = ParsingFunction.ReaderClosed;
        }

        public override string GetAttribute(int i)
        {
            return this.coreReader.GetAttribute(i);
        }

        public override string GetAttribute(string name)
        {
            return this.coreReader.GetAttribute(name);
        }

        public override string GetAttribute(string localName, string namespaceURI)
        {
            return this.coreReader.GetAttribute(localName, namespaceURI);
        }

        internal IDictionary<string, string> GetNamespacesInScope(XmlNamespaceScope scope)
        {
            return this.coreReaderNSResolver.GetNamespacesInScope(scope);
        }

        private System.Xml.XmlResolver GetResolver()
        {
            return this.coreReaderImpl.GetResolver();
        }

        public bool HasLineInfo()
        {
            return true;
        }

        public override string LookupNamespace(string prefix)
        {
            return this.coreReaderImpl.LookupNamespace(prefix);
        }

        internal string LookupPrefix(string namespaceName)
        {
            return this.coreReaderNSResolver.LookupPrefix(namespaceName);
        }

        internal void MoveOffEntityReference()
        {
            if (((this.outerReader.NodeType == XmlNodeType.EntityReference) && (this.parsingFunction != ParsingFunction.ResolveEntityInternally)) && !this.outerReader.Read())
            {
                throw new InvalidOperationException(Res.GetString("Xml_InvalidOperation"));
            }
        }

        public override void MoveToAttribute(int i)
        {
            this.coreReader.MoveToAttribute(i);
            this.parsingFunction = ParsingFunction.Read;
        }

        public override bool MoveToAttribute(string name)
        {
            if (!this.coreReader.MoveToAttribute(name))
            {
                return false;
            }
            this.parsingFunction = ParsingFunction.Read;
            return true;
        }

        public override bool MoveToAttribute(string localName, string namespaceURI)
        {
            if (!this.coreReader.MoveToAttribute(localName, namespaceURI))
            {
                return false;
            }
            this.parsingFunction = ParsingFunction.Read;
            return true;
        }

        public override bool MoveToElement()
        {
            if (!this.coreReader.MoveToElement())
            {
                return false;
            }
            this.parsingFunction = ParsingFunction.Read;
            return true;
        }

        public override bool MoveToFirstAttribute()
        {
            if (!this.coreReader.MoveToFirstAttribute())
            {
                return false;
            }
            this.parsingFunction = ParsingFunction.Read;
            return true;
        }

        public override bool MoveToNextAttribute()
        {
            if (!this.coreReader.MoveToNextAttribute())
            {
                return false;
            }
            this.parsingFunction = ParsingFunction.Read;
            return true;
        }

        private void ParseDtdFromParserContext()
        {
            if ((this.parserContext.DocTypeName != null) && (this.parserContext.DocTypeName.Length != 0))
            {
                IDtdParser parser = DtdParser.Create();
                XmlTextReaderImpl.DtdParserProxy adapter = new XmlTextReaderImpl.DtdParserProxy(this.coreReaderImpl);
                IDtdInfo newDtdInfo = parser.ParseFreeFloatingDtd(this.parserContext.BaseURI, this.parserContext.DocTypeName, this.parserContext.PublicId, this.parserContext.SystemId, this.parserContext.InternalSubset, adapter);
                this.coreReaderImpl.SetDtdInfo(newDtdInfo);
                this.ValidateDtd();
            }
        }

        private void ProcessCoreReaderEvent()
        {
            XmlNodeType nodeType = this.coreReader.NodeType;
            switch (nodeType)
            {
                case XmlNodeType.EntityReference:
                    this.parsingFunction = ParsingFunction.ResolveEntityInternally;
                    break;

                case XmlNodeType.DocumentType:
                    this.ValidateDtd();
                    return;

                default:
                    if (((nodeType == XmlNodeType.Whitespace) && ((this.coreReader.Depth > 0) || (this.coreReaderImpl.FragmentType != XmlNodeType.Document))) && this.validator.PreserveWhitespace)
                    {
                        this.coreReaderImpl.ChangeCurrentNodeType(XmlNodeType.SignificantWhitespace);
                    }
                    break;
            }
            this.coreReaderImpl.InternalSchemaType = null;
            this.coreReaderImpl.InternalTypedValue = null;
            this.validator.Validate();
        }

        public override bool Read()
        {
            switch (this.parsingFunction)
            {
                case ParsingFunction.Read:
                    break;

                case ParsingFunction.Init:
                    this.parsingFunction = ParsingFunction.Read;
                    if (this.coreReader.ReadState != System.Xml.ReadState.Interactive)
                    {
                        break;
                    }
                    this.ProcessCoreReaderEvent();
                    return true;

                case ParsingFunction.ParseDtdFromContext:
                    this.parsingFunction = ParsingFunction.Read;
                    this.ParseDtdFromParserContext();
                    break;

                case ParsingFunction.ResolveEntityInternally:
                    this.parsingFunction = ParsingFunction.Read;
                    this.ResolveEntityInternally();
                    break;

                case ParsingFunction.InReadBinaryContent:
                    this.parsingFunction = ParsingFunction.Read;
                    this.readBinaryHelper.Finish();
                    break;

                case ParsingFunction.ReaderClosed:
                case ParsingFunction.Error:
                    return false;

                default:
                    return false;
            }
            if (this.coreReader.Read())
            {
                this.ProcessCoreReaderEvent();
                return true;
            }
            this.validator.CompleteValidation();
            return false;
        }

        public override bool ReadAttributeValue()
        {
            if (this.parsingFunction == ParsingFunction.InReadBinaryContent)
            {
                this.parsingFunction = ParsingFunction.Read;
                this.readBinaryHelper.Finish();
            }
            if (!this.coreReader.ReadAttributeValue())
            {
                return false;
            }
            this.parsingFunction = ParsingFunction.Read;
            return true;
        }

        public override int ReadContentAsBase64(byte[] buffer, int index, int count)
        {
            if (this.ReadState != System.Xml.ReadState.Interactive)
            {
                return 0;
            }
            if (this.parsingFunction != ParsingFunction.InReadBinaryContent)
            {
                this.readBinaryHelper = ReadContentAsBinaryHelper.CreateOrReset(this.readBinaryHelper, this.outerReader);
            }
            this.parsingFunction = ParsingFunction.Read;
            int num = this.readBinaryHelper.ReadContentAsBase64(buffer, index, count);
            this.parsingFunction = ParsingFunction.InReadBinaryContent;
            return num;
        }

        public override int ReadContentAsBinHex(byte[] buffer, int index, int count)
        {
            if (this.ReadState != System.Xml.ReadState.Interactive)
            {
                return 0;
            }
            if (this.parsingFunction != ParsingFunction.InReadBinaryContent)
            {
                this.readBinaryHelper = ReadContentAsBinaryHelper.CreateOrReset(this.readBinaryHelper, this.outerReader);
            }
            this.parsingFunction = ParsingFunction.Read;
            int num = this.readBinaryHelper.ReadContentAsBinHex(buffer, index, count);
            this.parsingFunction = ParsingFunction.InReadBinaryContent;
            return num;
        }

        public override int ReadElementContentAsBase64(byte[] buffer, int index, int count)
        {
            if (this.ReadState != System.Xml.ReadState.Interactive)
            {
                return 0;
            }
            if (this.parsingFunction != ParsingFunction.InReadBinaryContent)
            {
                this.readBinaryHelper = ReadContentAsBinaryHelper.CreateOrReset(this.readBinaryHelper, this.outerReader);
            }
            this.parsingFunction = ParsingFunction.Read;
            int num = this.readBinaryHelper.ReadElementContentAsBase64(buffer, index, count);
            this.parsingFunction = ParsingFunction.InReadBinaryContent;
            return num;
        }

        public override int ReadElementContentAsBinHex(byte[] buffer, int index, int count)
        {
            if (this.ReadState != System.Xml.ReadState.Interactive)
            {
                return 0;
            }
            if (this.parsingFunction != ParsingFunction.InReadBinaryContent)
            {
                this.readBinaryHelper = ReadContentAsBinaryHelper.CreateOrReset(this.readBinaryHelper, this.outerReader);
            }
            this.parsingFunction = ParsingFunction.Read;
            int num = this.readBinaryHelper.ReadElementContentAsBinHex(buffer, index, count);
            this.parsingFunction = ParsingFunction.InReadBinaryContent;
            return num;
        }

        public override string ReadString()
        {
            this.MoveOffEntityReference();
            return base.ReadString();
        }

        public object ReadTypedValue()
        {
            if (this.validationType == System.Xml.ValidationType.None)
            {
                return null;
            }
            switch (this.outerReader.NodeType)
            {
                case XmlNodeType.Element:
                    if (this.SchemaType != null)
                    {
                        if (((this.SchemaType is XmlSchemaDatatype) ? ((XmlSchemaDatatype) this.SchemaType) : ((XmlSchemaType) this.SchemaType).Datatype) == null)
                        {
                            return null;
                        }
                        if (this.outerReader.IsEmptyElement)
                        {
                            break;
                        }
                    Label_0087:
                        if (!this.outerReader.Read())
                        {
                            throw new InvalidOperationException(Res.GetString("Xml_InvalidOperation"));
                        }
                        switch (this.outerReader.NodeType)
                        {
                            case XmlNodeType.CDATA:
                            case XmlNodeType.Text:
                            case XmlNodeType.Whitespace:
                            case XmlNodeType.SignificantWhitespace:
                            case XmlNodeType.Comment:
                            case XmlNodeType.ProcessingInstruction:
                                goto Label_0087;
                        }
                        if (this.outerReader.NodeType != XmlNodeType.EndElement)
                        {
                            throw new XmlException("Xml_InvalidNodeType", this.outerReader.NodeType.ToString());
                        }
                        break;
                    }
                    return null;

                case XmlNodeType.Attribute:
                    return this.coreReaderImpl.InternalTypedValue;

                case XmlNodeType.EndElement:
                    return null;

                default:
                    if (this.coreReaderImpl.V1Compat)
                    {
                        return null;
                    }
                    return this.Value;
            }
            return this.coreReaderImpl.InternalTypedValue;
        }

        public override void ResolveEntity()
        {
            if (this.parsingFunction == ParsingFunction.ResolveEntityInternally)
            {
                this.parsingFunction = ParsingFunction.Read;
            }
            this.coreReader.ResolveEntity();
        }

        private void ResolveEntityInternally()
        {
            int depth = this.coreReader.Depth;
            this.outerReader.ResolveEntity();
            while (this.outerReader.Read() && (this.coreReader.Depth > depth))
            {
            }
        }

        private void SetupValidation(System.Xml.ValidationType valType)
        {
            this.validator = BaseValidator.CreateInstance(valType, this, this.schemaCollection, this.eventHandling, this.processIdentityConstraints);
            System.Xml.XmlResolver resolver = this.GetResolver();
            this.validator.XmlResolver = resolver;
            if (this.outerReader.BaseURI.Length > 0)
            {
                this.validator.BaseUri = (resolver == null) ? new Uri(this.outerReader.BaseURI, UriKind.RelativeOrAbsolute) : resolver.ResolveUri(null, this.outerReader.BaseURI);
            }
            this.coreReaderImpl.ValidationEventHandling = (this.validationType == System.Xml.ValidationType.None) ? null : this.eventHandling;
        }

        IDictionary<string, string> IXmlNamespaceResolver.GetNamespacesInScope(XmlNamespaceScope scope)
        {
            return this.GetNamespacesInScope(scope);
        }

        string IXmlNamespaceResolver.LookupNamespace(string prefix)
        {
            return this.LookupNamespace(prefix);
        }

        string IXmlNamespaceResolver.LookupPrefix(string namespaceName)
        {
            return this.LookupPrefix(namespaceName);
        }

        internal void ValidateDefaultAttributeOnUse(IDtdDefaultAttributeInfo defaultAttribute, XmlTextReaderImpl coreReader)
        {
            SchemaAttDef attdef = defaultAttribute as SchemaAttDef;
            if ((attdef != null) && !attdef.DefaultValueChecked)
            {
                SchemaInfo dtdInfo = coreReader.DtdInfo as SchemaInfo;
                if (dtdInfo != null)
                {
                    DtdValidator.CheckDefaultValue(attdef, dtdInfo, this.eventHandling, coreReader.BaseURI);
                }
            }
        }

        private void ValidateDtd()
        {
            IDtdInfo dtdInfo = this.coreReaderImpl.DtdInfo;
            if (dtdInfo != null)
            {
                switch (this.validationType)
                {
                    case System.Xml.ValidationType.None:
                    case System.Xml.ValidationType.DTD:
                        break;

                    case System.Xml.ValidationType.Auto:
                        this.SetupValidation(System.Xml.ValidationType.DTD);
                        break;

                    default:
                        return;
                }
                this.validator.DtdInfo = dtdInfo;
            }
        }

        public override int AttributeCount
        {
            get
            {
                return this.coreReader.AttributeCount;
            }
        }

        public override string BaseURI
        {
            get
            {
                return this.coreReader.BaseURI;
            }
        }

        public override bool CanReadBinaryContent
        {
            get
            {
                return true;
            }
        }

        public override bool CanResolveEntity
        {
            get
            {
                return true;
            }
        }

        public override int Depth
        {
            get
            {
                return this.coreReader.Depth;
            }
        }

        internal override IDtdInfo DtdInfo
        {
            get
            {
                return this.coreReaderImpl.DtdInfo;
            }
        }

        internal System.Text.Encoding Encoding
        {
            get
            {
                return this.coreReaderImpl.Encoding;
            }
        }

        internal System.Xml.EntityHandling EntityHandling
        {
            get
            {
                return this.coreReaderImpl.EntityHandling;
            }
            set
            {
                this.coreReaderImpl.EntityHandling = value;
            }
        }

        public override bool EOF
        {
            get
            {
                return this.coreReader.EOF;
            }
        }

        public override bool HasValue
        {
            get
            {
                return this.coreReader.HasValue;
            }
        }

        public override bool IsDefault
        {
            get
            {
                return this.coreReader.IsDefault;
            }
        }

        public override bool IsEmptyElement
        {
            get
            {
                return this.coreReader.IsEmptyElement;
            }
        }

        public int LineNumber
        {
            get
            {
                return ((IXmlLineInfo) this.coreReader).LineNumber;
            }
        }

        public int LinePosition
        {
            get
            {
                return ((IXmlLineInfo) this.coreReader).LinePosition;
            }
        }

        public override string LocalName
        {
            get
            {
                return this.coreReader.LocalName;
            }
        }

        public override string Name
        {
            get
            {
                return this.coreReader.Name;
            }
        }

        internal override XmlNamespaceManager NamespaceManager
        {
            get
            {
                return this.coreReaderImpl.NamespaceManager;
            }
        }

        internal bool Namespaces
        {
            get
            {
                return this.coreReaderImpl.Namespaces;
            }
            set
            {
                this.coreReaderImpl.Namespaces = value;
            }
        }

        public override string NamespaceURI
        {
            get
            {
                return this.coreReader.NamespaceURI;
            }
        }

        public override XmlNameTable NameTable
        {
            get
            {
                return this.coreReader.NameTable;
            }
        }

        public override XmlNodeType NodeType
        {
            get
            {
                return this.coreReader.NodeType;
            }
        }

        internal bool Normalization
        {
            get
            {
                return this.coreReaderImpl.Normalization;
            }
        }

        internal XmlReader OuterReader
        {
            get
            {
                return this.outerReader;
            }
            set
            {
                this.outerReader = value;
            }
        }

        public override string Prefix
        {
            get
            {
                return this.coreReader.Prefix;
            }
        }

        public override char QuoteChar
        {
            get
            {
                return this.coreReader.QuoteChar;
            }
        }

        internal XmlReader Reader
        {
            get
            {
                return this.coreReader;
            }
        }

        internal XmlTextReaderImpl ReaderImpl
        {
            get
            {
                return this.coreReaderImpl;
            }
        }

        public override System.Xml.ReadState ReadState
        {
            get
            {
                if (this.parsingFunction != ParsingFunction.Init)
                {
                    return this.coreReader.ReadState;
                }
                return System.Xml.ReadState.Initial;
            }
        }

        internal XmlSchemaCollection Schemas
        {
            get
            {
                return this.schemaCollection;
            }
        }

        internal object SchemaType
        {
            get
            {
                if (this.validationType == System.Xml.ValidationType.None)
                {
                    return null;
                }
                XmlSchemaType internalSchemaType = this.coreReaderImpl.InternalSchemaType as XmlSchemaType;
                if ((internalSchemaType != null) && (internalSchemaType.QualifiedName.Namespace == "http://www.w3.org/2001/XMLSchema"))
                {
                    return internalSchemaType.Datatype;
                }
                return this.coreReaderImpl.InternalSchemaType;
            }
        }

        internal object SchemaTypeObject
        {
            set
            {
                this.coreReaderImpl.InternalSchemaType = value;
            }
        }

        public override XmlReaderSettings Settings
        {
            get
            {
                XmlReaderSettings settings;
                if (this.coreReaderImpl.V1Compat)
                {
                    settings = null;
                }
                else
                {
                    settings = this.coreReader.Settings;
                }
                if (settings != null)
                {
                    settings = settings.Clone();
                }
                else
                {
                    settings = new XmlReaderSettings();
                }
                settings.ValidationType = System.Xml.ValidationType.DTD;
                if (!this.processIdentityConstraints)
                {
                    settings.ValidationFlags &= ~XmlSchemaValidationFlags.ProcessIdentityConstraints;
                }
                settings.ReadOnly = true;
                return settings;
            }
        }

        internal bool StandAlone
        {
            get
            {
                return this.coreReaderImpl.StandAlone;
            }
        }

        internal object TypedValueObject
        {
            get
            {
                return this.coreReaderImpl.InternalTypedValue;
            }
            set
            {
                this.coreReaderImpl.InternalTypedValue = value;
            }
        }

        internal System.Xml.ValidationType ValidationType
        {
            get
            {
                return this.validationType;
            }
            set
            {
                if (this.ReadState != System.Xml.ReadState.Initial)
                {
                    throw new InvalidOperationException(Res.GetString("Xml_InvalidOperation"));
                }
                this.validationType = value;
                this.SetupValidation(value);
            }
        }

        internal BaseValidator Validator
        {
            get
            {
                return this.validator;
            }
            set
            {
                this.validator = value;
            }
        }

        public override string Value
        {
            get
            {
                return this.coreReader.Value;
            }
        }

        public override string XmlLang
        {
            get
            {
                return this.coreReader.XmlLang;
            }
        }

        internal System.Xml.XmlResolver XmlResolver
        {
            set
            {
                this.coreReaderImpl.XmlResolver = value;
                this.validator.XmlResolver = value;
                this.schemaCollection.XmlResolver = value;
            }
        }

        public override System.Xml.XmlSpace XmlSpace
        {
            get
            {
                return this.coreReader.XmlSpace;
            }
        }

        private enum ParsingFunction
        {
            Read,
            Init,
            ParseDtdFromContext,
            ResolveEntityInternally,
            InReadBinaryContent,
            ReaderClosed,
            Error,
            None
        }

        internal class ValidationEventHandling : IValidationEventHandling
        {
            private ValidationEventHandler eventHandler;
            private XmlValidatingReaderImpl reader;

            internal ValidationEventHandling(XmlValidatingReaderImpl reader)
            {
                this.reader = reader;
            }

            internal void AddHandler(ValidationEventHandler handler)
            {
                this.eventHandler = (ValidationEventHandler) Delegate.Combine(this.eventHandler, handler);
            }

            internal void RemoveHandler(ValidationEventHandler handler)
            {
                this.eventHandler = (ValidationEventHandler) Delegate.Remove(this.eventHandler, handler);
            }

            void IValidationEventHandling.SendEvent(Exception exception, XmlSeverityType severity)
            {
                if (this.eventHandler != null)
                {
                    this.eventHandler(this.reader, new ValidationEventArgs((XmlSchemaException) exception, severity));
                }
                else if ((this.reader.ValidationType != ValidationType.None) && (severity == XmlSeverityType.Error))
                {
                    throw exception;
                }
            }

            object IValidationEventHandling.EventHandler
            {
                get
                {
                    return this.eventHandler;
                }
            }
        }
    }
}

