namespace System.Xaml
{
    using System;

    internal class ReaderMultiIndexDelegate : ReaderBaseDelegate, IXamlIndexingReader
    {
        private int _count;
        private int _idx;
        private XamlNodeIndexDelegate _indexDelegate;
        private static XamlNode s_EndOfStream = new XamlNode(XamlNode.InternalNodeType.EndOfStream);
        private static XamlNode s_StartOfStream = new XamlNode(XamlNode.InternalNodeType.StartOfStream);

        public ReaderMultiIndexDelegate(XamlSchemaContext schemaContext, XamlNodeIndexDelegate indexDelegate, int count, bool hasLineInfo) : base(schemaContext)
        {
            this._indexDelegate = indexDelegate;
            this._count = count;
            this._idx = -1;
            base._currentNode = s_StartOfStream;
            base._currentLineInfo = null;
            base._hasLineInfo = hasLineInfo;
        }

        public override bool Read()
        {
            if (base.IsDisposed)
            {
                throw new ObjectDisposedException("XamlReader");
            }
        Label_0013:
            if (this._idx < (this._count - 1))
            {
                base._currentNode = this._indexDelegate(++this._idx);
                if (this._currentNode.NodeType != XamlNodeType.None)
                {
                    return true;
                }
                if (this._currentNode.LineInfo != null)
                {
                    base._currentLineInfo = this._currentNode.LineInfo;
                    goto Label_00A3;
                }
                if (!this._currentNode.IsEof)
                {
                    goto Label_00A3;
                }
            }
            else
            {
                this._idx = this._count;
                base._currentNode = s_EndOfStream;
                base._currentLineInfo = null;
            }
            goto Label_00B3;
        Label_00A3:
            if (this._currentNode.NodeType == XamlNodeType.None)
            {
                goto Label_0013;
            }
        Label_00B3:
            return !this.IsEof;
        }

        public int Count
        {
            get
            {
                return this._count;
            }
        }

        public int CurrentIndex
        {
            get
            {
                return this._idx;
            }
            set
            {
                if ((value < -1) || (value > this._count))
                {
                    throw new IndexOutOfRangeException();
                }
                if (value == -1)
                {
                    this._idx = -1;
                    base._currentNode = s_StartOfStream;
                    base._currentLineInfo = null;
                }
                else
                {
                    this._idx = value - 1;
                    this.Read();
                }
            }
        }
    }
}

