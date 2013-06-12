namespace System.Xml.Schema
{
    using System;
    using System.Collections;
    using System.Text;
    using System.Xml;

    internal class BaseValidator
    {
        private Uri baseUri;
        protected bool checkDatatype;
        protected ValidationState context;
        protected XmlQualifiedName elementName;
        private IValidationEventHandling eventHandling;
        protected bool hasSibling;
        private XmlNameTable nameTable;
        private System.Xml.PositionInfo positionInfo;
        protected XmlValidatingReaderImpl reader;
        private XmlSchemaCollection schemaCollection;
        protected System.Xml.Schema.SchemaInfo schemaInfo;
        private System.Xml.Schema.SchemaNames schemaNames;
        protected string textString;
        protected StringBuilder textValue;
        private System.Xml.XmlResolver xmlResolver;

        public BaseValidator(BaseValidator other)
        {
            this.reader = other.reader;
            this.schemaCollection = other.schemaCollection;
            this.eventHandling = other.eventHandling;
            this.nameTable = other.nameTable;
            this.schemaNames = other.schemaNames;
            this.positionInfo = other.positionInfo;
            this.xmlResolver = other.xmlResolver;
            this.baseUri = other.baseUri;
            this.elementName = other.elementName;
        }

        public BaseValidator(XmlValidatingReaderImpl reader, XmlSchemaCollection schemaCollection, IValidationEventHandling eventHandling)
        {
            this.reader = reader;
            this.schemaCollection = schemaCollection;
            this.eventHandling = eventHandling;
            this.nameTable = reader.NameTable;
            this.positionInfo = System.Xml.PositionInfo.GetPositionInfo(reader);
            this.elementName = new XmlQualifiedName();
        }

        public virtual void CompleteValidation()
        {
        }

        public static BaseValidator CreateInstance(ValidationType valType, XmlValidatingReaderImpl reader, XmlSchemaCollection schemaCollection, IValidationEventHandling eventHandling, bool processIdentityConstraints)
        {
            switch (valType)
            {
                case ValidationType.None:
                    return new BaseValidator(reader, schemaCollection, eventHandling);

                case ValidationType.Auto:
                    return new AutoValidator(reader, schemaCollection, eventHandling);

                case ValidationType.DTD:
                    return new DtdValidator(reader, eventHandling, processIdentityConstraints);

                case ValidationType.XDR:
                    return new XdrValidator(reader, schemaCollection, eventHandling);

                case ValidationType.Schema:
                    return new XsdValidator(reader, schemaCollection, eventHandling);
            }
            return null;
        }

        public virtual object FindId(string name)
        {
            return null;
        }

        protected static void ProcessEntity(System.Xml.Schema.SchemaInfo sinfo, string name, IValidationEventHandling eventHandling, string baseUriStr, int lineNumber, int linePosition)
        {
            SchemaEntity entity;
            string res = null;
            if (!sinfo.GeneralEntities.TryGetValue(new XmlQualifiedName(name), out entity))
            {
                res = "Sch_UndeclaredEntity";
            }
            else if (entity.NData.IsEmpty)
            {
                res = "Sch_UnparsedEntityRef";
            }
            if (res != null)
            {
                XmlSchemaException exception = new XmlSchemaException(res, name, baseUriStr, lineNumber, linePosition);
                if (eventHandling == null)
                {
                    throw exception;
                }
                eventHandling.SendEvent(exception, XmlSeverityType.Error);
            }
        }

        protected static void ProcessEntity(System.Xml.Schema.SchemaInfo sinfo, string name, object sender, ValidationEventHandler eventhandler, string baseUri, int lineNumber, int linePosition)
        {
            SchemaEntity entity;
            XmlSchemaException ex = null;
            if (!sinfo.GeneralEntities.TryGetValue(new XmlQualifiedName(name), out entity))
            {
                ex = new XmlSchemaException("Sch_UndeclaredEntity", name, baseUri, lineNumber, linePosition);
            }
            else if (entity.NData.IsEmpty)
            {
                ex = new XmlSchemaException("Sch_UnparsedEntityRef", name, baseUri, lineNumber, linePosition);
            }
            if (ex != null)
            {
                if (eventhandler == null)
                {
                    throw ex;
                }
                eventhandler(sender, new ValidationEventArgs(ex));
            }
        }

        private void SaveTextValue(string value)
        {
            if (this.textString.Length == 0)
            {
                this.textString = value;
            }
            else
            {
                if (!this.hasSibling)
                {
                    this.textValue.Append(this.textString);
                    this.hasSibling = true;
                }
                this.textValue.Append(value);
            }
        }

        protected void SendValidationEvent(string code)
        {
            this.SendValidationEvent(code, string.Empty);
        }

        protected void SendValidationEvent(XmlSchemaException e)
        {
            this.SendValidationEvent(e, XmlSeverityType.Error);
        }

        protected void SendValidationEvent(string code, string[] args)
        {
            this.SendValidationEvent(new XmlSchemaException(code, args, this.reader.BaseURI, this.positionInfo.LineNumber, this.positionInfo.LinePosition));
        }

        protected void SendValidationEvent(string code, string arg)
        {
            this.SendValidationEvent(new XmlSchemaException(code, arg, this.reader.BaseURI, this.positionInfo.LineNumber, this.positionInfo.LinePosition));
        }

        protected void SendValidationEvent(XmlSchemaException e, XmlSeverityType severity)
        {
            if (this.eventHandling != null)
            {
                this.eventHandling.SendEvent(e, severity);
            }
            else if (severity == XmlSeverityType.Error)
            {
                throw e;
            }
        }

        protected void SendValidationEvent(string code, string arg1, string arg2)
        {
            this.SendValidationEvent(new XmlSchemaException(code, new string[] { arg1, arg2 }, this.reader.BaseURI, this.positionInfo.LineNumber, this.positionInfo.LinePosition));
        }

        protected void SendValidationEvent(string code, string msg, XmlSeverityType severity)
        {
            this.SendValidationEvent(new XmlSchemaException(code, msg, this.reader.BaseURI, this.positionInfo.LineNumber, this.positionInfo.LinePosition), severity);
        }

        protected void SendValidationEvent(string code, string[] args, XmlSeverityType severity)
        {
            this.SendValidationEvent(new XmlSchemaException(code, args, this.reader.BaseURI, this.positionInfo.LineNumber, this.positionInfo.LinePosition), severity);
        }

        public virtual void Validate()
        {
        }

        public void ValidateText()
        {
            if (this.context.NeedValidateChildren)
            {
                if (this.context.IsNill)
                {
                    this.SendValidationEvent("Sch_ContentInNill", XmlSchemaValidator.QNameString(this.context.LocalName, this.context.Namespace));
                }
                else
                {
                    ContentValidator contentValidator = this.context.ElementDecl.ContentValidator;
                    switch (contentValidator.ContentType)
                    {
                        case XmlSchemaContentType.ElementOnly:
                        {
                            ArrayList expected = contentValidator.ExpectedElements(this.context, false);
                            if (expected == null)
                            {
                                this.SendValidationEvent("Sch_InvalidTextInElement", XmlSchemaValidator.BuildElementName(this.context.LocalName, this.context.Namespace));
                            }
                            else
                            {
                                this.SendValidationEvent("Sch_InvalidTextInElementExpecting", new string[] { XmlSchemaValidator.BuildElementName(this.context.LocalName, this.context.Namespace), XmlSchemaValidator.PrintExpectedElements(expected, false) });
                            }
                            break;
                        }
                        case XmlSchemaContentType.Empty:
                            this.SendValidationEvent("Sch_InvalidTextInEmpty", string.Empty);
                            break;
                    }
                    if (this.checkDatatype)
                    {
                        this.SaveTextValue(this.reader.Value);
                    }
                }
            }
        }

        public void ValidateWhitespace()
        {
            if (this.context.NeedValidateChildren)
            {
                XmlSchemaContentType contentType = this.context.ElementDecl.ContentValidator.ContentType;
                if (this.context.IsNill)
                {
                    this.SendValidationEvent("Sch_ContentInNill", XmlSchemaValidator.QNameString(this.context.LocalName, this.context.Namespace));
                }
                if (contentType == XmlSchemaContentType.Empty)
                {
                    this.SendValidationEvent("Sch_InvalidWhitespaceInEmpty", string.Empty);
                }
                if (this.checkDatatype)
                {
                    this.SaveTextValue(this.reader.Value);
                }
            }
        }

        public Uri BaseUri
        {
            get
            {
                return this.baseUri;
            }
            set
            {
                this.baseUri = value;
            }
        }

        public IDtdInfo DtdInfo
        {
            get
            {
                return this.schemaInfo;
            }
            set
            {
                System.Xml.Schema.SchemaInfo info = value as System.Xml.Schema.SchemaInfo;
                if (info == null)
                {
                    throw new XmlException("Xml_InternalError", string.Empty);
                }
                this.schemaInfo = info;
            }
        }

        public ValidationEventHandler EventHandler
        {
            get
            {
                return (ValidationEventHandler) this.eventHandling.EventHandler;
            }
        }

        public XmlNameTable NameTable
        {
            get
            {
                return this.nameTable;
            }
        }

        public System.Xml.PositionInfo PositionInfo
        {
            get
            {
                return this.positionInfo;
            }
        }

        public virtual bool PreserveWhitespace
        {
            get
            {
                return false;
            }
        }

        public XmlValidatingReaderImpl Reader
        {
            get
            {
                return this.reader;
            }
        }

        public XmlSchemaCollection SchemaCollection
        {
            get
            {
                return this.schemaCollection;
            }
        }

        public System.Xml.Schema.SchemaInfo SchemaInfo
        {
            get
            {
                return this.schemaInfo;
            }
            set
            {
                this.schemaInfo = value;
            }
        }

        public System.Xml.Schema.SchemaNames SchemaNames
        {
            get
            {
                if (this.schemaNames == null)
                {
                    if (this.schemaCollection != null)
                    {
                        this.schemaNames = this.schemaCollection.GetSchemaNames(this.nameTable);
                    }
                    else
                    {
                        this.schemaNames = new System.Xml.Schema.SchemaNames(this.nameTable);
                    }
                }
                return this.schemaNames;
            }
        }

        public System.Xml.XmlResolver XmlResolver
        {
            get
            {
                return this.xmlResolver;
            }
            set
            {
                this.xmlResolver = value;
            }
        }
    }
}

