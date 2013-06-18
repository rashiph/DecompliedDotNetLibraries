namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;

    internal class MsmqInputMessage : NativeMsmqMessage
    {
        private NativeMsmqMessage.IntProperty abortCount;
        private NativeMsmqMessage.BufferProperty body;
        private NativeMsmqMessage.IntProperty bodyLength;
        private NativeMsmqMessage.ShortProperty cls;
        private const int initialBodySize = 0x1000;
        private const int initialCertificateSize = 0x1000;
        private const int initialSenderIdSize = 0x100;
        private NativeMsmqMessage.IntProperty lastMovedTime;
        private NativeMsmqMessage.LongProperty lookupId;
        private int maxBufferSize;
        private const int maxSize = 0x400000;
        private NativeMsmqMessage.BufferProperty messageId;
        private NativeMsmqMessage.IntProperty moveCount;
        private NativeMsmqMessage.BufferProperty senderCertificate;
        private NativeMsmqMessage.IntProperty senderCertificateLength;
        private NativeMsmqMessage.BufferProperty senderId;
        private NativeMsmqMessage.IntProperty senderIdLength;

        public MsmqInputMessage() : this(0, 0x400000)
        {
        }

        public MsmqInputMessage(int maxBufferSize) : this(0, maxBufferSize)
        {
        }

        protected MsmqInputMessage(int additionalPropertyCount, int maxBufferSize) : this(additionalPropertyCount, new SizeQuota(maxBufferSize))
        {
        }

        protected MsmqInputMessage(int additionalPropertyCount, SizeQuota bufferSizeQuota) : base(12 + additionalPropertyCount)
        {
            this.maxBufferSize = bufferSizeQuota.MaxSize;
            this.body = new NativeMsmqMessage.BufferProperty(this, 9, bufferSizeQuota.AllocIfAvailable(0x1000));
            this.bodyLength = new NativeMsmqMessage.IntProperty(this, 10);
            this.messageId = new NativeMsmqMessage.BufferProperty(this, 2, 20);
            this.lookupId = new NativeMsmqMessage.LongProperty(this, 60);
            this.cls = new NativeMsmqMessage.ShortProperty(this, 1);
            this.senderId = new NativeMsmqMessage.BufferProperty(this, 20, 0x100);
            this.senderIdLength = new NativeMsmqMessage.IntProperty(this, 0x15);
            this.senderCertificate = new NativeMsmqMessage.BufferProperty(this, 0x1c, bufferSizeQuota.AllocIfAvailable(0x1000));
            this.senderCertificateLength = new NativeMsmqMessage.IntProperty(this, 0x1d);
            if (Msmq.IsAdvancedPoisonHandlingSupported)
            {
                this.lastMovedTime = new NativeMsmqMessage.IntProperty(this, 0x4b);
                this.abortCount = new NativeMsmqMessage.IntProperty(this, 0x45);
                this.moveCount = new NativeMsmqMessage.IntProperty(this, 70);
            }
        }

        public override void GrowBuffers()
        {
            this.OnGrowBuffers(new SizeQuota(this.maxBufferSize));
        }

        protected virtual void OnGrowBuffers(SizeQuota bufferSizeQuota)
        {
            bufferSizeQuota.Alloc(this.senderIdLength.Value);
            this.senderId.EnsureBufferLength(this.senderIdLength.Value);
            bufferSizeQuota.Alloc(this.senderCertificateLength.Value);
            this.senderCertificate.EnsureBufferLength(this.senderCertificateLength.Value);
            bufferSizeQuota.Alloc(this.bodyLength.Value);
            this.body.EnsureBufferLength(this.bodyLength.Value);
        }

        public NativeMsmqMessage.IntProperty AbortCount
        {
            get
            {
                return this.abortCount;
            }
        }

        public NativeMsmqMessage.BufferProperty Body
        {
            get
            {
                return this.body;
            }
        }

        public NativeMsmqMessage.IntProperty BodyLength
        {
            get
            {
                return this.bodyLength;
            }
        }

        public NativeMsmqMessage.ShortProperty Class
        {
            get
            {
                return this.cls;
            }
        }

        public NativeMsmqMessage.IntProperty LastMovedTime
        {
            get
            {
                return this.lastMovedTime;
            }
        }

        public NativeMsmqMessage.LongProperty LookupId
        {
            get
            {
                return this.lookupId;
            }
        }

        public NativeMsmqMessage.BufferProperty MessageId
        {
            get
            {
                return this.messageId;
            }
        }

        public NativeMsmqMessage.IntProperty MoveCount
        {
            get
            {
                return this.moveCount;
            }
        }

        public NativeMsmqMessage.BufferProperty SenderCertificate
        {
            get
            {
                return this.senderCertificate;
            }
        }

        public NativeMsmqMessage.IntProperty SenderCertificateLength
        {
            get
            {
                return this.senderCertificateLength;
            }
        }

        public NativeMsmqMessage.BufferProperty SenderId
        {
            get
            {
                return this.senderId;
            }
        }

        public NativeMsmqMessage.IntProperty SenderIdLength
        {
            get
            {
                return this.senderIdLength;
            }
        }

        protected class SizeQuota
        {
            private int maxSize;
            private int remainingSize;

            public SizeQuota(int maxSize)
            {
                this.maxSize = maxSize;
                this.remainingSize = maxSize;
            }

            public void Alloc(int requiredSize)
            {
                if (requiredSize > this.remainingSize)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(MaxMessageSizeStream.CreateMaxReceivedMessageSizeExceededException((long) this.maxSize));
                }
                this.remainingSize -= requiredSize;
            }

            public int AllocIfAvailable(int desiredSize)
            {
                int num = Math.Min(desiredSize, this.remainingSize);
                this.remainingSize -= num;
                return num;
            }

            public int MaxSize
            {
                get
                {
                    return this.maxSize;
                }
            }
        }
    }
}

