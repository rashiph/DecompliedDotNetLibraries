namespace System.Xaml
{
    using System;

    internal class ReaderDelegate : ReaderBaseDelegate
    {
        private XamlNodeNextDelegate _nextDelegate;

        public ReaderDelegate(XamlSchemaContext schemaContext, XamlNodeNextDelegate next, bool hasLineInfo) : base(schemaContext)
        {
            this._nextDelegate = next;
            base._currentNode = new XamlNode(XamlNode.InternalNodeType.StartOfStream);
            base._currentLineInfo = null;
            base._hasLineInfo = hasLineInfo;
        }

        public override bool Read()
        {
            if (base.IsDisposed)
            {
                throw new ObjectDisposedException("XamlReader");
            }
            do
            {
                base._currentNode = this._nextDelegate();
                if (this._currentNode.NodeType != XamlNodeType.None)
                {
                    return true;
                }
                if (this._currentNode.IsLineInfo)
                {
                    base._currentLineInfo = this._currentNode.LineInfo;
                }
                else if (this._currentNode.IsEof)
                {
                    break;
                }
            }
            while (this._currentNode.NodeType == XamlNodeType.None);
            return !this.IsEof;
        }
    }
}

