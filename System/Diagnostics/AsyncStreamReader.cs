namespace System.Diagnostics
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Text;
    using System.Threading;

    internal class AsyncStreamReader : IDisposable
    {
        private int _maxCharsPerBuffer;
        private bool bLastCarriageReturn;
        private byte[] byteBuffer;
        private bool cancelOperation;
        private char[] charBuffer;
        private System.Text.Decoder decoder;
        internal const int DefaultBufferSize = 0x400;
        private Encoding encoding;
        private ManualResetEvent eofEvent;
        private Queue messageQueue;
        private const int MinBufferSize = 0x80;
        private Process process;
        private StringBuilder sb;
        private Stream stream;
        private UserCallBack userCallBack;

        internal AsyncStreamReader(Process process, Stream stream, UserCallBack callback, Encoding encoding) : this(process, stream, callback, encoding, 0x400)
        {
        }

        internal AsyncStreamReader(Process process, Stream stream, UserCallBack callback, Encoding encoding, int bufferSize)
        {
            this.Init(process, stream, callback, encoding, bufferSize);
            this.messageQueue = new Queue();
        }

        internal void BeginReadLine()
        {
            if (this.cancelOperation)
            {
                this.cancelOperation = false;
            }
            if (this.sb == null)
            {
                this.sb = new StringBuilder(0x400);
                this.stream.BeginRead(this.byteBuffer, 0, this.byteBuffer.Length, new AsyncCallback(this.ReadBuffer), null);
            }
            else
            {
                this.FlushMessageQueue();
            }
        }

        internal void CancelOperation()
        {
            this.cancelOperation = true;
        }

        public virtual void Close()
        {
            this.Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && (this.stream != null))
            {
                this.stream.Close();
            }
            if (this.stream != null)
            {
                this.stream = null;
                this.encoding = null;
                this.decoder = null;
                this.byteBuffer = null;
                this.charBuffer = null;
            }
            if (this.eofEvent != null)
            {
                this.eofEvent.Close();
                this.eofEvent = null;
            }
        }

        private void FlushMessageQueue()
        {
        Label_0000:
            if (this.messageQueue.Count > 0)
            {
                lock (this.messageQueue)
                {
                    if (this.messageQueue.Count > 0)
                    {
                        string data = (string) this.messageQueue.Dequeue();
                        if (!this.cancelOperation)
                        {
                            this.userCallBack(data);
                        }
                    }
                    goto Label_0000;
                }
            }
        }

        private void GetLinesFromStringBuilder()
        {
            int num = 0;
            int startIndex = 0;
            int length = this.sb.Length;
            if ((this.bLastCarriageReturn && (length > 0)) && (this.sb[0] == '\n'))
            {
                num = 1;
                startIndex = 1;
                this.bLastCarriageReturn = false;
            }
            while (num < length)
            {
                char ch = this.sb[num];
                switch (ch)
                {
                    case '\r':
                    case '\n':
                    {
                        string str = this.sb.ToString(startIndex, num - startIndex);
                        startIndex = num + 1;
                        if (((ch == '\r') && (startIndex < length)) && (this.sb[startIndex] == '\n'))
                        {
                            startIndex++;
                            num++;
                        }
                        lock (this.messageQueue)
                        {
                            this.messageQueue.Enqueue(str);
                        }
                        break;
                    }
                }
                num++;
            }
            if (this.sb[length - 1] == '\r')
            {
                this.bLastCarriageReturn = true;
            }
            if (startIndex < length)
            {
                this.sb.Remove(0, startIndex);
            }
            else
            {
                this.sb.Length = 0;
            }
            this.FlushMessageQueue();
        }

        private void Init(Process process, Stream stream, UserCallBack callback, Encoding encoding, int bufferSize)
        {
            this.process = process;
            this.stream = stream;
            this.encoding = encoding;
            this.userCallBack = callback;
            this.decoder = encoding.GetDecoder();
            if (bufferSize < 0x80)
            {
                bufferSize = 0x80;
            }
            this.byteBuffer = new byte[bufferSize];
            this._maxCharsPerBuffer = encoding.GetMaxCharCount(bufferSize);
            this.charBuffer = new char[this._maxCharsPerBuffer];
            this.cancelOperation = false;
            this.eofEvent = new ManualResetEvent(false);
            this.sb = null;
            this.bLastCarriageReturn = false;
        }

        private void ReadBuffer(IAsyncResult ar)
        {
            int num;
            try
            {
                num = this.stream.EndRead(ar);
            }
            catch (IOException)
            {
                num = 0;
            }
            catch (OperationCanceledException)
            {
                num = 0;
            }
            if (num == 0)
            {
                lock (this.messageQueue)
                {
                    if (this.sb.Length != 0)
                    {
                        this.messageQueue.Enqueue(this.sb.ToString());
                        this.sb.Length = 0;
                    }
                    this.messageQueue.Enqueue(null);
                }
                try
                {
                    this.FlushMessageQueue();
                }
                finally
                {
                    this.eofEvent.Set();
                }
            }
            else
            {
                int charCount = this.decoder.GetChars(this.byteBuffer, 0, num, this.charBuffer, 0);
                this.sb.Append(this.charBuffer, 0, charCount);
                this.GetLinesFromStringBuilder();
                this.stream.BeginRead(this.byteBuffer, 0, this.byteBuffer.Length, new AsyncCallback(this.ReadBuffer), null);
            }
        }

        void IDisposable.Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        internal void WaitUtilEOF()
        {
            if (this.eofEvent != null)
            {
                this.eofEvent.WaitOne();
                this.eofEvent.Close();
                this.eofEvent = null;
            }
        }

        public virtual Stream BaseStream
        {
            get
            {
                return this.stream;
            }
        }

        public virtual Encoding CurrentEncoding
        {
            get
            {
                return this.encoding;
            }
        }
    }
}

