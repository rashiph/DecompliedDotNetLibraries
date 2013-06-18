namespace System.Xaml
{
    using System;
    using System.Collections.Generic;

    public class XamlNodeList
    {
        private bool _hasLineInfo;
        private List<XamlNode> _nodeList;
        private bool _readMode;
        private XamlWriter _writer;

        public XamlNodeList(XamlSchemaContext schemaContext)
        {
            if (schemaContext == null)
            {
                throw new ArgumentNullException("schemaContext");
            }
            this.Initialize(schemaContext, 0);
        }

        public XamlNodeList(XamlSchemaContext schemaContext, int size)
        {
            if (schemaContext == null)
            {
                throw new ArgumentNullException("schemaContext");
            }
            this.Initialize(schemaContext, size);
        }

        private void Add(XamlNodeType nodeType, object data)
        {
            if (this._readMode)
            {
                throw new XamlException(System.Xaml.SR.Get("CannotWriteClosedWriter"));
            }
            if (nodeType != XamlNodeType.None)
            {
                XamlNode item = new XamlNode(nodeType, data);
                this._nodeList.Add(item);
            }
            else
            {
                this._readMode = true;
            }
        }

        private void AddLineInfo(int lineNumber, int linePosition)
        {
            if (this._readMode)
            {
                throw new XamlException(System.Xaml.SR.Get("CannotWriteClosedWriter"));
            }
            XamlNode item = new XamlNode(new LineInfo(lineNumber, linePosition));
            this._nodeList.Add(item);
            if (!this._hasLineInfo)
            {
                this._hasLineInfo = true;
            }
        }

        public void Clear()
        {
            this._nodeList.Clear();
            this._readMode = false;
        }

        public XamlReader GetReader()
        {
            if (!this._readMode)
            {
                throw new XamlException(System.Xaml.SR.Get("CloseXamlWriterBeforeReading"));
            }
            if (this._writer.SchemaContext == null)
            {
                throw new XamlException(System.Xaml.SR.Get("SchemaContextNotInitialized"));
            }
            return new ReaderMultiIndexDelegate(this._writer.SchemaContext, new XamlNodeIndexDelegate(this.Index), this._nodeList.Count, this._hasLineInfo);
        }

        private XamlNode Index(int idx)
        {
            if (!this._readMode)
            {
                throw new XamlException(System.Xaml.SR.Get("CloseXamlWriterBeforeReading"));
            }
            return this._nodeList[idx];
        }

        private void Initialize(XamlSchemaContext schemaContext, int size)
        {
            if (size == 0)
            {
                this._nodeList = new List<XamlNode>();
            }
            else
            {
                this._nodeList = new List<XamlNode>(size);
            }
            this._writer = new WriterDelegate(new XamlNodeAddDelegate(this.Add), new XamlLineInfoAddDelegate(this.AddLineInfo), schemaContext);
        }

        public int Count
        {
            get
            {
                return this._nodeList.Count;
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

