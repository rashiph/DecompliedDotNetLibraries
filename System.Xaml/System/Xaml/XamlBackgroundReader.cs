namespace System.Xaml
{
    using System;
    using System.Threading;

    public class XamlBackgroundReader : XamlReader, IXamlLineInfo
    {
        private Exception _caughtException;
        private XamlNode _currentNode;
        private EventWaitHandle _dataReceivedEvent;
        private XamlNode[] _incoming;
        private int _inIdx;
        private XamlReader _internalReader;
        private int _lineNumber;
        private int _linePosition;
        private XamlNode[] _outgoing;
        private int _outIdx;
        private int _outValid;
        private EventWaitHandle _providerFullEvent;
        private Thread _thread;
        private XamlReader _wrappedReader;
        private bool _wrappedReaderHasLineInfo;
        private XamlWriter _writer;

        public XamlBackgroundReader(XamlReader wrappedReader)
        {
            if (wrappedReader == null)
            {
                throw new ArgumentNullException("wrappedReader");
            }
            this.Initialize(wrappedReader, 0x40);
        }

        private void Add(XamlNodeType nodeType, object data)
        {
            if (!base.IsDisposed)
            {
                if (nodeType != XamlNodeType.None)
                {
                    this.AddToBuffer(new XamlNode(nodeType, data));
                }
                else
                {
                    this.AddToBuffer(new XamlNode(XamlNode.InternalNodeType.EndOfStream));
                    this._providerFullEvent.Set();
                }
            }
        }

        private void AddLineInfo(int lineNumber, int linePosition)
        {
            if (!base.IsDisposed)
            {
                LineInfo lineInfo = new LineInfo(lineNumber, linePosition);
                XamlNode node = new XamlNode(lineInfo);
                this.AddToBuffer(node);
            }
        }

        private void AddToBuffer(XamlNode node)
        {
            this._incoming[this._inIdx] = node;
            this._inIdx++;
            if (this.IncomingFull)
            {
                this._providerFullEvent.Set();
                this._dataReceivedEvent.WaitOne();
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            this._dataReceivedEvent.Set();
            this._dataReceivedEvent.Dispose();
            ((IDisposable) this._internalReader).Dispose();
            this._providerFullEvent.Dispose();
            ((IDisposable) this._writer).Dispose();
        }

        private void Initialize(XamlReader wrappedReader, int bufferSize)
        {
            XamlNodeNextDelegate delegate4;
            this._providerFullEvent = new EventWaitHandle(false, EventResetMode.AutoReset);
            this._dataReceivedEvent = new EventWaitHandle(false, EventResetMode.AutoReset);
            this._incoming = new XamlNode[bufferSize];
            this._outgoing = new XamlNode[bufferSize];
            this._wrappedReader = wrappedReader;
            this._wrappedReaderHasLineInfo = ((IXamlLineInfo) this._wrappedReader).HasLineInfo;
            XamlNodeAddDelegate add = new XamlNodeAddDelegate(this.Add);
            XamlLineInfoAddDelegate addlineInfoDelegate = null;
            if (this._wrappedReaderHasLineInfo)
            {
                addlineInfoDelegate = new XamlLineInfoAddDelegate(this.AddLineInfo);
            }
            this._writer = new WriterDelegate(add, addlineInfoDelegate, this._wrappedReader.SchemaContext);
            if (this._wrappedReaderHasLineInfo)
            {
                delegate4 = new XamlNodeNextDelegate(this.Next_ProcessLineInfo);
            }
            else
            {
                delegate4 = new XamlNodeNextDelegate(this.Next);
            }
            this._internalReader = new ReaderDelegate(this._wrappedReader.SchemaContext, delegate4, this._wrappedReaderHasLineInfo);
            this._currentNode = new XamlNode(XamlNode.InternalNodeType.StartOfStream);
        }

        private void InterruptableTransform(XamlReader reader, XamlWriter writer, bool closeWriter)
        {
            IXamlLineInfo info = reader as IXamlLineInfo;
            IXamlLineInfoConsumer consumer = writer as IXamlLineInfoConsumer;
            bool flag = false;
            if (((info != null) && info.HasLineInfo) && ((consumer != null) && consumer.ShouldProvideLineInfo))
            {
                flag = true;
            }
            while (reader.Read())
            {
                if (base.IsDisposed)
                {
                    break;
                }
                if (flag && (info.LineNumber != 0))
                {
                    consumer.SetLineInfo(info.LineNumber, info.LinePosition);
                }
                writer.WriteNode(reader);
            }
            if (closeWriter)
            {
                writer.Close();
            }
        }

        private XamlNode Next()
        {
            if (base.IsDisposed)
            {
                throw new ObjectDisposedException("XamlBackgroundReader");
            }
            if (this.OutgoingEmpty)
            {
                if (this._currentNode.IsEof)
                {
                    return this._currentNode;
                }
                this._providerFullEvent.WaitOne();
                this.SwapBuffers();
                this._dataReceivedEvent.Set();
            }
            this._currentNode = this._outgoing[this._outIdx++];
            if (this._currentNode.IsEof && (this._thread != null))
            {
                this._thread.Join();
                if (this._caughtException != null)
                {
                    Exception exception = this._caughtException;
                    this._caughtException = null;
                    throw exception;
                }
            }
            return this._currentNode;
        }

        private XamlNode Next_ProcessLineInfo()
        {
            bool flag = false;
            while (!flag)
            {
                this.Next();
                if (this._currentNode.IsLineInfo)
                {
                    this._lineNumber = this._currentNode.LineInfo.LineNumber;
                    this._linePosition = this._currentNode.LineInfo.LinePosition;
                }
                else
                {
                    flag = true;
                }
            }
            return this._currentNode;
        }

        public override bool Read()
        {
            return this._internalReader.Read();
        }

        public void StartThread()
        {
            this.StartThread("XAML reader thread");
        }

        public void StartThread(string threadName)
        {
            if (this._thread != null)
            {
                throw new InvalidOperationException(System.Xaml.SR.Get("ThreadAlreadyStarted"));
            }
            ParameterizedThreadStart start = new ParameterizedThreadStart(this.XamlReaderThreadStart);
            this._thread = new Thread(start);
            this._thread.Name = threadName;
            this._thread.Start();
        }

        private void SwapBuffers()
        {
            XamlNode[] nodeArray = this._incoming;
            this._incoming = this._outgoing;
            this._outgoing = nodeArray;
            this._outIdx = 0;
            this._outValid = this._inIdx;
            this._inIdx = 0;
        }

        private void XamlReaderThreadStart(object none)
        {
            try
            {
                this.InterruptableTransform(this._wrappedReader, this._writer, true);
            }
            catch (Exception exception)
            {
                this._writer.Close();
                this._caughtException = exception;
            }
        }

        public bool HasLineInfo
        {
            get
            {
                return this._wrappedReaderHasLineInfo;
            }
        }

        internal bool IncomingFull
        {
            get
            {
                return (this._inIdx >= this._incoming.Length);
            }
        }

        public override bool IsEof
        {
            get
            {
                return this._internalReader.IsEof;
            }
        }

        public int LineNumber
        {
            get
            {
                return this._lineNumber;
            }
        }

        public int LinePosition
        {
            get
            {
                return this._linePosition;
            }
        }

        public override XamlMember Member
        {
            get
            {
                return this._internalReader.Member;
            }
        }

        public override NamespaceDeclaration Namespace
        {
            get
            {
                return this._internalReader.Namespace;
            }
        }

        public override XamlNodeType NodeType
        {
            get
            {
                return this._internalReader.NodeType;
            }
        }

        internal bool OutgoingEmpty
        {
            get
            {
                return (this._outIdx >= this._outValid);
            }
        }

        public override XamlSchemaContext SchemaContext
        {
            get
            {
                return this._internalReader.SchemaContext;
            }
        }

        public override XamlType Type
        {
            get
            {
                return this._internalReader.Type;
            }
        }

        public override object Value
        {
            get
            {
                return this._internalReader.Value;
            }
        }
    }
}

