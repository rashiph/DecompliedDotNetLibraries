namespace System.ServiceModel.Channels
{
    using System;
    using System.IO;
    using System.Xml;

    internal abstract class BufferedMessageWriter
    {
        private const int expectedSizeVariance = 0x100;
        private int[] sizeHistory;
        private const int sizeHistoryCount = 4;
        private int sizeHistoryIndex;
        private BufferManagerOutputStream stream = new BufferManagerOutputStream("MaxSentMessageSizeExceeded");

        public BufferedMessageWriter()
        {
            this.InitMessagePredicter();
        }

        private void InitMessagePredicter()
        {
            this.sizeHistory = new int[4];
            for (int i = 0; i < 4; i++)
            {
                this.sizeHistory[i] = 0x100;
            }
        }

        protected virtual void OnWriteEndMessage(XmlDictionaryWriter writer)
        {
        }

        protected virtual void OnWriteStartMessage(XmlDictionaryWriter writer)
        {
        }

        private int PredictMessageSize()
        {
            int num = 0;
            for (int i = 0; i < 4; i++)
            {
                if (this.sizeHistory[i] > num)
                {
                    num = this.sizeHistory[i];
                }
            }
            return (num + 0x100);
        }

        private void RecordActualMessageSize(int size)
        {
            this.sizeHistory[this.sizeHistoryIndex] = size;
            this.sizeHistoryIndex = (this.sizeHistoryIndex + 1) % 4;
        }

        protected abstract void ReturnXmlWriter(XmlDictionaryWriter writer);
        protected abstract XmlDictionaryWriter TakeXmlWriter(Stream stream);
        public ArraySegment<byte> WriteMessage(Message message, BufferManager bufferManager, int initialOffset, int maxSizeQuota)
        {
            int num;
            ArraySegment<byte> segment;
            if (maxSizeQuota <= (0x7fffffff - initialOffset))
            {
                num = maxSizeQuota + initialOffset;
            }
            else
            {
                num = 0x7fffffff;
            }
            int initialSize = this.PredictMessageSize();
            if (initialSize > num)
            {
                initialSize = num;
            }
            else if (initialSize < initialOffset)
            {
                initialSize = initialOffset;
            }
            try
            {
                int num3;
                this.stream.Init(initialSize, maxSizeQuota, num, bufferManager);
                this.stream.Skip(initialOffset);
                XmlDictionaryWriter writer = this.TakeXmlWriter(this.stream);
                this.OnWriteStartMessage(writer);
                message.WriteMessage(writer);
                this.OnWriteEndMessage(writer);
                writer.Flush();
                this.ReturnXmlWriter(writer);
                byte[] array = this.stream.ToArray(out num3);
                this.RecordActualMessageSize(num3);
                segment = new ArraySegment<byte>(array, initialOffset, num3 - initialOffset);
            }
            finally
            {
                this.stream.Clear();
            }
            return segment;
        }
    }
}

