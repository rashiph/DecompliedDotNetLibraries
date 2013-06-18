namespace System.Xaml
{
    using System;

    internal class XamlSubreader : XamlReader, IXamlLineInfo
    {
        private int _depth;
        private bool _done;
        private bool _firstRead;
        private IXamlLineInfo _lineInfoReader;
        private XamlReader _reader;
        private bool _rootIsStartMember;

        public XamlSubreader(XamlReader reader)
        {
            this._reader = reader;
            this._lineInfoReader = reader as IXamlLineInfo;
            this._done = false;
            this._depth = 0;
            this._firstRead = true;
            this._rootIsStartMember = reader.NodeType == XamlNodeType.StartMember;
        }

        private bool LimitedRead()
        {
            if (this.IsEof)
            {
                return false;
            }
            XamlNodeType nodeType = this._reader.NodeType;
            if (this._rootIsStartMember)
            {
                switch (nodeType)
                {
                    case XamlNodeType.StartMember:
                        this._depth++;
                        break;

                    case XamlNodeType.EndMember:
                        this._depth--;
                        break;
                }
            }
            else
            {
                switch (nodeType)
                {
                    case XamlNodeType.StartObject:
                    case XamlNodeType.GetObject:
                        this._depth++;
                        break;

                    case XamlNodeType.EndObject:
                        this._depth--;
                        break;
                }
            }
            if (this._depth == 0)
            {
                this._done = true;
            }
            this._reader.Read();
            return !this.IsEof;
        }

        public override bool Read()
        {
            if (base.IsDisposed)
            {
                throw new ObjectDisposedException("XamlReader");
            }
            if (!this._firstRead)
            {
                return this.LimitedRead();
            }
            this._firstRead = false;
            return true;
        }

        public bool HasLineInfo
        {
            get
            {
                if (this._lineInfoReader == null)
                {
                    return false;
                }
                return this._lineInfoReader.HasLineInfo;
            }
        }

        private bool IsEmpty
        {
            get
            {
                if (!this._done)
                {
                    return this._firstRead;
                }
                return true;
            }
        }

        public override bool IsEof
        {
            get
            {
                if (!this.IsEmpty)
                {
                    return this._reader.IsEof;
                }
                return true;
            }
        }

        public int LineNumber
        {
            get
            {
                if (this._lineInfoReader == null)
                {
                    return 0;
                }
                return this._lineInfoReader.LineNumber;
            }
        }

        public int LinePosition
        {
            get
            {
                if (this._lineInfoReader == null)
                {
                    return 0;
                }
                return this._lineInfoReader.LinePosition;
            }
        }

        public override XamlMember Member
        {
            get
            {
                if (!this.IsEmpty)
                {
                    return this._reader.Member;
                }
                return null;
            }
        }

        public override NamespaceDeclaration Namespace
        {
            get
            {
                if (!this.IsEmpty)
                {
                    return this._reader.Namespace;
                }
                return null;
            }
        }

        public override XamlNodeType NodeType
        {
            get
            {
                if (!this.IsEmpty)
                {
                    return this._reader.NodeType;
                }
                return XamlNodeType.None;
            }
        }

        public override XamlSchemaContext SchemaContext
        {
            get
            {
                return this._reader.SchemaContext;
            }
        }

        public override XamlType Type
        {
            get
            {
                if (!this.IsEmpty)
                {
                    return this._reader.Type;
                }
                return null;
            }
        }

        public override object Value
        {
            get
            {
                if (!this.IsEmpty)
                {
                    return this._reader.Value;
                }
                return null;
            }
        }
    }
}

