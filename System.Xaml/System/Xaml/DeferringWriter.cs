namespace System.Xaml
{
    using MS.Internal.Xaml.Context;
    using System;

    internal class DeferringWriter : XamlWriter, IXamlLineInfoConsumer
    {
        private ObjectWriterContext _context;
        private XamlNodeList _deferredList;
        private int _deferredTreeDepth;
        private XamlWriter _deferredWriter;
        private bool _handled;
        private DeferringMode _mode;

        public DeferringWriter(ObjectWriterContext context)
        {
            this._context = context;
            this._mode = DeferringMode.Off;
        }

        public void Clear()
        {
            this._handled = false;
            this._mode = DeferringMode.Off;
            this._deferredList = null;
            this._deferredTreeDepth = -1;
        }

        public XamlNodeList CollectTemplateList()
        {
            XamlNodeList list = this._deferredList;
            this._deferredList = null;
            this._mode = DeferringMode.Off;
            return list;
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if ((disposing && !base.IsDisposed) && (this._deferredWriter != null))
                {
                    this._deferredWriter.Close();
                    this._deferredWriter = null;
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        public void SetLineInfo(int lineNumber, int linePosition)
        {
            throw new NotImplementedException();
        }

        private void StartDeferredList()
        {
            this._deferredList = new XamlNodeList(this._context.SchemaContext);
            this._deferredWriter = this._deferredList.Writer;
            this._deferredTreeDepth = 0;
        }

        public override void WriteEndMember()
        {
            this._handled = false;
            switch (this._mode)
            {
                case DeferringMode.Off:
                    return;

                case DeferringMode.TemplateDeferring:
                    this._deferredWriter.WriteEndMember();
                    this._handled = true;
                    return;

                case DeferringMode.TemplateReady:
                    throw new XamlInternalException(System.Xaml.SR.Get("TemplateNotCollected", new object[] { "WriteEndMember" }));
            }
            throw new XamlInternalException(System.Xaml.SR.Get("MissingCase", new object[] { this._mode.ToString(), "WriteEndMember" }));
        }

        public override void WriteEndObject()
        {
            this._handled = false;
            switch (this._mode)
            {
                case DeferringMode.Off:
                    return;

                case DeferringMode.TemplateDeferring:
                    this._deferredWriter.WriteEndObject();
                    this._handled = true;
                    this._deferredTreeDepth--;
                    if (this._deferredTreeDepth == 0)
                    {
                        this._deferredWriter.Close();
                        this._deferredWriter = null;
                        this._mode = DeferringMode.TemplateReady;
                    }
                    return;

                case DeferringMode.TemplateReady:
                    throw new XamlInternalException(System.Xaml.SR.Get("TemplateNotCollected", new object[] { "WriteEndObject" }));
            }
            throw new XamlInternalException(System.Xaml.SR.Get("MissingCase", new object[] { this._mode.ToString(), "WriteEndObject" }));
        }

        public override void WriteGetObject()
        {
            this.WriteObject(null, true, "WriteGetObject");
        }

        public override void WriteNamespace(NamespaceDeclaration namespaceDeclaration)
        {
            switch (this._mode)
            {
                case DeferringMode.Off:
                    return;

                case DeferringMode.TemplateStarting:
                    this.StartDeferredList();
                    this._mode = DeferringMode.TemplateDeferring;
                    break;

                case DeferringMode.TemplateDeferring:
                    break;

                case DeferringMode.TemplateReady:
                    throw new XamlInternalException(System.Xaml.SR.Get("TemplateNotCollected", new object[] { "WriteNamespace" }));

                default:
                    throw new XamlInternalException(System.Xaml.SR.Get("MissingCase", new object[] { this._mode.ToString(), "WriteNamespace" }));
            }
            this._deferredWriter.WriteNamespace(namespaceDeclaration);
            this._handled = true;
        }

        private void WriteObject(XamlType xamlType, bool fromMember, string methodName)
        {
            this._handled = false;
            switch (this._mode)
            {
                case DeferringMode.Off:
                    return;

                case DeferringMode.TemplateStarting:
                    this.StartDeferredList();
                    this._mode = DeferringMode.TemplateDeferring;
                    break;

                case DeferringMode.TemplateDeferring:
                    break;

                case DeferringMode.TemplateReady:
                    throw new XamlInternalException(System.Xaml.SR.Get("TemplateNotCollected", new object[] { methodName }));

                default:
                    throw new XamlInternalException(System.Xaml.SR.Get("MissingCase", new object[] { this._mode.ToString(), methodName }));
            }
            if (fromMember)
            {
                this._deferredWriter.WriteGetObject();
            }
            else
            {
                this._deferredWriter.WriteStartObject(xamlType);
            }
            this._deferredTreeDepth++;
            this._handled = true;
        }

        public override void WriteStartMember(XamlMember property)
        {
            this._handled = false;
            switch (this._mode)
            {
                case DeferringMode.Off:
                    if (property.DeferringLoader != null)
                    {
                        this._mode = DeferringMode.TemplateStarting;
                    }
                    return;

                case DeferringMode.TemplateDeferring:
                    this._deferredWriter.WriteStartMember(property);
                    this._handled = true;
                    return;

                case DeferringMode.TemplateReady:
                    throw new XamlInternalException(System.Xaml.SR.Get("TemplateNotCollected", new object[] { "WriteMember" }));
            }
            throw new XamlInternalException(System.Xaml.SR.Get("MissingCase", new object[] { this._mode.ToString(), "WriteMember" }));
        }

        public override void WriteStartObject(XamlType xamlType)
        {
            this.WriteObject(xamlType, false, "WriteStartObject");
        }

        public override void WriteValue(object value)
        {
            this._handled = false;
            switch (this._mode)
            {
                case DeferringMode.Off:
                    return;

                case DeferringMode.TemplateStarting:
                    if (!(value is XamlNodeList))
                    {
                        this.StartDeferredList();
                        this._mode = DeferringMode.TemplateDeferring;
                        break;
                    }
                    this._deferredList = (XamlNodeList) value;
                    this._mode = DeferringMode.TemplateReady;
                    this._handled = true;
                    return;

                case DeferringMode.TemplateDeferring:
                    break;

                case DeferringMode.TemplateReady:
                    throw new XamlInternalException(System.Xaml.SR.Get("TemplateNotCollected", new object[] { "WriteValue" }));

                default:
                    throw new XamlInternalException(System.Xaml.SR.Get("MissingCase", new object[] { this._mode.ToString(), "WriteValue" }));
            }
            this._deferredWriter.WriteValue(value);
            this._handled = true;
        }

        public bool Handled
        {
            get
            {
                return this._handled;
            }
        }

        public DeferringMode Mode
        {
            get
            {
                return this._mode;
            }
        }

        public override XamlSchemaContext SchemaContext
        {
            get
            {
                return this._context.SchemaContext;
            }
        }

        public bool ShouldProvideLineInfo
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    }
}

