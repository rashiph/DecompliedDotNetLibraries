namespace System.Xaml
{
    using System;

    internal abstract class ReaderBaseDelegate : XamlReader, IXamlLineInfo
    {
        protected LineInfo _currentLineInfo;
        protected XamlNode _currentNode;
        protected bool _hasLineInfo;
        protected XamlSchemaContext _schemaContext;

        protected ReaderBaseDelegate(XamlSchemaContext schemaContext)
        {
            if (schemaContext == null)
            {
                throw new ArgumentNullException("schemaContext");
            }
            this._schemaContext = schemaContext;
        }

        public bool HasLineInfo
        {
            get
            {
                return this._hasLineInfo;
            }
            set
            {
                this._hasLineInfo = value;
            }
        }

        public override bool IsEof
        {
            get
            {
                return this._currentNode.IsEof;
            }
        }

        public int LineNumber
        {
            get
            {
                if (this._currentLineInfo != null)
                {
                    return this._currentLineInfo.LineNumber;
                }
                return 0;
            }
        }

        public int LinePosition
        {
            get
            {
                if (this._currentLineInfo != null)
                {
                    return this._currentLineInfo.LinePosition;
                }
                return 0;
            }
        }

        public override XamlMember Member
        {
            get
            {
                return this._currentNode.Member;
            }
        }

        public override NamespaceDeclaration Namespace
        {
            get
            {
                return this._currentNode.NamespaceDeclaration;
            }
        }

        public override XamlNodeType NodeType
        {
            get
            {
                return this._currentNode.NodeType;
            }
        }

        public override XamlSchemaContext SchemaContext
        {
            get
            {
                return this._schemaContext;
            }
        }

        public override XamlType Type
        {
            get
            {
                return this._currentNode.XamlType;
            }
        }

        public override object Value
        {
            get
            {
                return this._currentNode.Value;
            }
        }
    }
}

