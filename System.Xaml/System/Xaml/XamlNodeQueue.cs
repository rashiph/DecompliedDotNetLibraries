namespace System.Xaml
{
    using System;
    using System.Collections.Generic;

    public class XamlNodeQueue
    {
        private XamlNode _endOfStreamNode;
        private bool _hasLineInfo;
        private Queue<XamlNode> _nodeQueue;
        private ReaderDelegate _reader;
        private XamlWriter _writer;

        public XamlNodeQueue(XamlSchemaContext schemaContext)
        {
            if (schemaContext == null)
            {
                throw new ArgumentNullException("schemaContext");
            }
            this._nodeQueue = new Queue<XamlNode>();
            this._endOfStreamNode = new XamlNode(XamlNode.InternalNodeType.EndOfStream);
            this._writer = new WriterDelegate(new XamlNodeAddDelegate(this.Add), new XamlLineInfoAddDelegate(this.AddLineInfo), schemaContext);
        }

        private void Add(XamlNodeType nodeType, object data)
        {
            if (nodeType != XamlNodeType.None)
            {
                XamlNode item = new XamlNode(nodeType, data);
                this._nodeQueue.Enqueue(item);
            }
            else
            {
                this._nodeQueue.Enqueue(this._endOfStreamNode);
            }
        }

        private void AddLineInfo(int lineNumber, int linePosition)
        {
            LineInfo lineInfo = new LineInfo(lineNumber, linePosition);
            XamlNode item = new XamlNode(lineInfo);
            this._nodeQueue.Enqueue(item);
            if (!this._hasLineInfo)
            {
                this._hasLineInfo = true;
            }
            if ((this._reader != null) && !this._reader.HasLineInfo)
            {
                this._reader.HasLineInfo = true;
            }
        }

        private XamlNode Next()
        {
            if (this._nodeQueue.Count > 0)
            {
                return this._nodeQueue.Dequeue();
            }
            return this._endOfStreamNode;
        }

        public int Count
        {
            get
            {
                return this._nodeQueue.Count;
            }
        }

        public bool IsEmpty
        {
            get
            {
                return (this._nodeQueue.Count == 0);
            }
        }

        public XamlReader Reader
        {
            get
            {
                if (this._reader == null)
                {
                    this._reader = new ReaderDelegate(this._writer.SchemaContext, new XamlNodeNextDelegate(this.Next), this._hasLineInfo);
                }
                return this._reader;
            }
        }

        public XamlWriter Writer
        {
            get
            {
                return this._writer;
            }
        }
    }
}

