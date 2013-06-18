namespace System.Xaml
{
    using MS.Internal.Xaml.Context;
    using System;
    using System.Runtime.CompilerServices;

    internal class XamlSavedContext
    {
        private XamlSchemaContext _context;
        private SavedContextType _savedContextType;
        private XamlContextStack<ObjectWriterFrame> _stack;

        public XamlSavedContext(SavedContextType savedContextType, ObjectWriterContext owContext, XamlContextStack<ObjectWriterFrame> stack)
        {
            this._savedContextType = savedContextType;
            this._context = owContext.SchemaContext;
            this._stack = stack;
            if (savedContextType == SavedContextType.Template)
            {
                stack.CurrentFrame.Instance = null;
            }
            this.BaseUri = owContext.BaseUri;
        }

        public Uri BaseUri { get; private set; }

        public SavedContextType SaveContextType
        {
            get
            {
                return this._savedContextType;
            }
        }

        public XamlSchemaContext SchemaContext
        {
            get
            {
                return this._context;
            }
        }

        public XamlContextStack<ObjectWriterFrame> Stack
        {
            get
            {
                return this._stack;
            }
        }
    }
}

