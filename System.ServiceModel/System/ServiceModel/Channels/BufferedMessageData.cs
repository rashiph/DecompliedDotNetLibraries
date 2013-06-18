namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime;
    using System.Xml;

    internal abstract class BufferedMessageData : IBufferedMessageData
    {
        private ArraySegment<byte> buffer;
        private System.ServiceModel.Channels.BufferManager bufferManager;
        private RecycledMessageState messageState;
        private SynchronizedPool<RecycledMessageState> messageStatePool;
        private bool multipleUsers;
        private int outstandingReaders;
        private int refCount;

        public BufferedMessageData(SynchronizedPool<RecycledMessageState> messageStatePool)
        {
            this.messageStatePool = messageStatePool;
        }

        public void Close()
        {
            if (this.multipleUsers)
            {
                lock (this.ThisLock)
                {
                    if (--this.refCount == 0)
                    {
                        this.DoClose();
                    }
                    return;
                }
            }
            this.DoClose();
        }

        private void DoClose()
        {
            this.bufferManager.ReturnBuffer(this.buffer.Array);
            if (this.outstandingReaders == 0)
            {
                this.bufferManager = null;
                this.buffer = new ArraySegment<byte>();
                this.OnClosed();
            }
        }

        public void DoReturnMessageState(RecycledMessageState messageState)
        {
            if (this.messageState == null)
            {
                this.messageState = messageState;
            }
            else
            {
                this.messageStatePool.Return(messageState);
            }
        }

        private void DoReturnXmlReader(XmlDictionaryReader reader)
        {
            this.ReturnXmlReader(reader);
            this.outstandingReaders--;
        }

        public RecycledMessageState DoTakeMessageState()
        {
            RecycledMessageState messageState = this.messageState;
            if (messageState != null)
            {
                this.messageState = null;
                return messageState;
            }
            return this.messageStatePool.Take();
        }

        private XmlDictionaryReader DoTakeXmlReader()
        {
            XmlDictionaryReader reader = this.TakeXmlReader();
            this.outstandingReaders++;
            return reader;
        }

        public void EnableMultipleUsers()
        {
            this.multipleUsers = true;
        }

        public XmlDictionaryReader GetMessageReader()
        {
            if (this.multipleUsers)
            {
                lock (this.ThisLock)
                {
                    return this.DoTakeXmlReader();
                }
            }
            return this.DoTakeXmlReader();
        }

        protected virtual void OnClosed()
        {
        }

        public void OnXmlReaderClosed(XmlDictionaryReader reader)
        {
            if (this.multipleUsers)
            {
                lock (this.ThisLock)
                {
                    this.DoReturnXmlReader(reader);
                    return;
                }
            }
            this.DoReturnXmlReader(reader);
        }

        public void Open()
        {
            lock (this.ThisLock)
            {
                this.refCount++;
            }
        }

        public void Open(ArraySegment<byte> buffer, System.ServiceModel.Channels.BufferManager bufferManager)
        {
            this.refCount = 1;
            this.bufferManager = bufferManager;
            this.buffer = buffer;
            this.multipleUsers = false;
        }

        public void ReturnMessageState(RecycledMessageState messageState)
        {
            if (this.multipleUsers)
            {
                lock (this.ThisLock)
                {
                    this.DoReturnMessageState(messageState);
                    return;
                }
            }
            this.DoReturnMessageState(messageState);
        }

        protected abstract void ReturnXmlReader(XmlDictionaryReader xmlReader);
        public RecycledMessageState TakeMessageState()
        {
            if (this.multipleUsers)
            {
                lock (this.ThisLock)
                {
                    return this.DoTakeMessageState();
                }
            }
            return this.DoTakeMessageState();
        }

        protected abstract XmlDictionaryReader TakeXmlReader();

        public ArraySegment<byte> Buffer
        {
            get
            {
                return this.buffer;
            }
        }

        public System.ServiceModel.Channels.BufferManager BufferManager
        {
            get
            {
                return this.bufferManager;
            }
        }

        public abstract System.ServiceModel.Channels.MessageEncoder MessageEncoder { get; }

        public virtual XmlDictionaryReaderQuotas Quotas
        {
            get
            {
                return XmlDictionaryReaderQuotas.Max;
            }
        }

        private object ThisLock
        {
            get
            {
                return this;
            }
        }
    }
}

