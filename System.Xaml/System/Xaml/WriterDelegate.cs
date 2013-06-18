namespace System.Xaml
{
    using System;

    internal class WriterDelegate : XamlWriter, IXamlLineInfoConsumer
    {
        private XamlNodeAddDelegate _addDelegate;
        private XamlLineInfoAddDelegate _addLineInfoDelegate;
        private XamlSchemaContext _schemaContext;

        public WriterDelegate(XamlNodeAddDelegate add, XamlLineInfoAddDelegate addlineInfoDelegate, XamlSchemaContext xamlSchemaContext)
        {
            this._addDelegate = add;
            this._addLineInfoDelegate = addlineInfoDelegate;
            this._schemaContext = xamlSchemaContext;
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing && !base.IsDisposed)
                {
                    this._addDelegate(XamlNodeType.None, XamlNode.InternalNodeType.EndOfStream);
                    this._addDelegate = new XamlNodeAddDelegate(this.ThrowBecauseWriterIsClosed);
                    this._addLineInfoDelegate = (this._addLineInfoDelegate != null) ? new XamlLineInfoAddDelegate(this.ThrowBecauseWriterIsClosed2) : null;
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        public void SetLineInfo(int lineNumber, int linePosition)
        {
            this.ThrowIsDisposed();
            this._addLineInfoDelegate(lineNumber, linePosition);
        }

        private void ThrowBecauseWriterIsClosed(XamlNodeType nodeType, object data)
        {
            throw new XamlException(System.Xaml.SR.Get("WriterIsClosed"));
        }

        private void ThrowBecauseWriterIsClosed2(int lineNumber, int linePosition)
        {
            throw new XamlException(System.Xaml.SR.Get("WriterIsClosed"));
        }

        private void ThrowIsDisposed()
        {
            if (base.IsDisposed)
            {
                throw new ObjectDisposedException("XamlWriter");
            }
        }

        public override void WriteEndMember()
        {
            this.ThrowIsDisposed();
            this._addDelegate(XamlNodeType.EndMember, null);
        }

        public override void WriteEndObject()
        {
            this.ThrowIsDisposed();
            this._addDelegate(XamlNodeType.EndObject, null);
        }

        public override void WriteGetObject()
        {
            this.ThrowIsDisposed();
            this._addDelegate(XamlNodeType.GetObject, null);
        }

        public override void WriteNamespace(NamespaceDeclaration namespaceDeclaration)
        {
            this.ThrowIsDisposed();
            this._addDelegate(XamlNodeType.NamespaceDeclaration, namespaceDeclaration);
        }

        public override void WriteStartMember(XamlMember member)
        {
            this.ThrowIsDisposed();
            this._addDelegate(XamlNodeType.StartMember, member);
        }

        public override void WriteStartObject(XamlType xamlType)
        {
            this.ThrowIsDisposed();
            this._addDelegate(XamlNodeType.StartObject, xamlType);
        }

        public override void WriteValue(object value)
        {
            this.ThrowIsDisposed();
            this._addDelegate(XamlNodeType.Value, value);
        }

        public override XamlSchemaContext SchemaContext
        {
            get
            {
                return this._schemaContext;
            }
        }

        public bool ShouldProvideLineInfo
        {
            get
            {
                this.ThrowIsDisposed();
                return (this._addLineInfoDelegate != null);
            }
        }
    }
}

