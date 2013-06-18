namespace System.ServiceModel.Channels
{
    using System;
    using System.Net.Security;
    using System.Security.Cryptography.X509Certificates;
    using System.ServiceModel;
    using System.ServiceModel.Security.Tokens;

    internal class MsmqOutputMessage<TChannel> : NativeMsmqMessage
    {
        private NativeMsmqMessage.IntProperty authLevel;
        private NativeMsmqMessage.BufferProperty body;
        private NativeMsmqMessage.IntProperty bodyType;
        private NativeMsmqMessage.StringProperty deadLetterQueue;
        private NativeMsmqMessage.ByteProperty delivery;
        private NativeMsmqMessage.IntProperty encryptionAlgorithm;
        private NativeMsmqMessage.IntProperty hashAlgorithm;
        private NativeMsmqMessage.ByteProperty journal;
        private NativeMsmqMessage.BufferProperty messageId;
        private NativeMsmqMessage.IntProperty privLevel;
        private NativeMsmqMessage.BufferProperty senderCert;
        private NativeMsmqMessage.IntProperty senderIdType;
        private NativeMsmqMessage.IntProperty timeToBeReceived;
        private NativeMsmqMessage.IntProperty timeToReachQueue;
        private NativeMsmqMessage.ByteProperty trace;

        public MsmqOutputMessage(MsmqChannelFactoryBase<TChannel> factory, int bodySize, EndpointAddress remoteAddress) : this(factory, bodySize, remoteAddress, 0)
        {
        }

        protected MsmqOutputMessage(MsmqChannelFactoryBase<TChannel> factory, int bodySize, EndpointAddress remoteAddress, int additionalPropertyCount) : base(15 + additionalPropertyCount)
        {
            this.body = new NativeMsmqMessage.BufferProperty(this, 9, bodySize);
            this.messageId = new NativeMsmqMessage.BufferProperty(this, 2, 20);
            this.EnsureBodyTypeProperty(0x1011);
            this.EnsureJournalProperty(2, factory.UseSourceJournal);
            this.delivery = new NativeMsmqMessage.ByteProperty(this, 5);
            if (factory.Durable)
            {
                this.delivery.Value = 1;
            }
            else
            {
                this.delivery.Value = 0;
            }
            if (factory.TimeToLive != TimeSpan.MaxValue)
            {
                int num = MsmqDuration.FromTimeSpan(factory.TimeToLive);
                this.EnsureTimeToReachQueueProperty(num);
                this.timeToBeReceived = new NativeMsmqMessage.IntProperty(this, 14, num);
            }
            switch (factory.DeadLetterQueue)
            {
                case DeadLetterQueue.None:
                    this.EnsureJournalProperty(1, false);
                    break;

                case DeadLetterQueue.System:
                    this.EnsureJournalProperty(1, true);
                    break;

                case DeadLetterQueue.Custom:
                    this.EnsureJournalProperty(1, true);
                    this.EnsureDeadLetterQueueProperty(factory.DeadLetterQueuePathName);
                    break;
            }
            if (MsmqAuthenticationMode.WindowsDomain == factory.MsmqTransportSecurity.MsmqAuthenticationMode)
            {
                this.EnsureSenderIdTypeProperty(1);
                this.authLevel = new NativeMsmqMessage.IntProperty(this, 0x18, 1);
                this.hashAlgorithm = new NativeMsmqMessage.IntProperty(this, 0x1a, MsmqSecureHashAlgorithmHelper.ToInt32(factory.MsmqTransportSecurity.MsmqSecureHashAlgorithm));
                if (ProtectionLevel.EncryptAndSign == factory.MsmqTransportSecurity.MsmqProtectionLevel)
                {
                    this.privLevel = new NativeMsmqMessage.IntProperty(this, 0x17, 3);
                    this.encryptionAlgorithm = new NativeMsmqMessage.IntProperty(this, 0x1b, MsmqEncryptionAlgorithmHelper.ToInt32(factory.MsmqTransportSecurity.MsmqEncryptionAlgorithm));
                }
            }
            else if (MsmqAuthenticationMode.Certificate == factory.MsmqTransportSecurity.MsmqAuthenticationMode)
            {
                this.authLevel = new NativeMsmqMessage.IntProperty(this, 0x18, 1);
                this.hashAlgorithm = new NativeMsmqMessage.IntProperty(this, 0x1a, MsmqSecureHashAlgorithmHelper.ToInt32(factory.MsmqTransportSecurity.MsmqSecureHashAlgorithm));
                if (ProtectionLevel.EncryptAndSign == factory.MsmqTransportSecurity.MsmqProtectionLevel)
                {
                    this.privLevel = new NativeMsmqMessage.IntProperty(this, 0x17, 3);
                    this.encryptionAlgorithm = new NativeMsmqMessage.IntProperty(this, 0x1b, MsmqEncryptionAlgorithmHelper.ToInt32(factory.MsmqTransportSecurity.MsmqEncryptionAlgorithm));
                }
                this.EnsureSenderIdTypeProperty(0);
                this.senderCert = new NativeMsmqMessage.BufferProperty(this, 0x1c);
            }
            else
            {
                this.authLevel = new NativeMsmqMessage.IntProperty(this, 0x18, 0);
                this.EnsureSenderIdTypeProperty(0);
            }
            this.trace = new NativeMsmqMessage.ByteProperty(this, 0x29, factory.UseMsmqTracing ? ((byte) 1) : ((byte) 0));
        }

        internal void ApplyCertificateIfNeeded(SecurityTokenProviderContainer certificateTokenProvider, MsmqAuthenticationMode authenticationMode, TimeSpan timeout)
        {
            if (MsmqAuthenticationMode.Certificate == authenticationMode)
            {
                if (certificateTokenProvider == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("certificateTokenProvider");
                }
                X509Certificate2 certificate = certificateTokenProvider.GetCertificate(timeout);
                if (certificate == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperCritical(new InvalidOperationException(System.ServiceModel.SR.GetString("MsmqCertificateNotFound")));
                }
                this.senderCert.SetBufferReference(certificate.GetRawCertData());
            }
        }

        protected void EnsureBodyTypeProperty(int value)
        {
            if (this.bodyType == null)
            {
                this.bodyType = new NativeMsmqMessage.IntProperty(this, 0x2a);
            }
            this.bodyType.Value = value;
        }

        protected void EnsureDeadLetterQueueProperty(string value)
        {
            if (value.Length > 0)
            {
                if (this.deadLetterQueue == null)
                {
                    this.deadLetterQueue = new NativeMsmqMessage.StringProperty(this, 0x43, value);
                }
                else
                {
                    this.deadLetterQueue.SetValue(value);
                }
            }
        }

        protected void EnsureJournalProperty(byte flag, bool isFlagSet)
        {
            if (this.journal == null)
            {
                this.journal = new NativeMsmqMessage.ByteProperty(this, 7);
            }
            if (isFlagSet)
            {
                this.journal.Value = (byte) (this.journal.Value | flag);
            }
            else
            {
                this.journal.Value = (byte) (this.journal.Value & ~flag);
            }
        }

        protected void EnsureSenderIdTypeProperty(int value)
        {
            if (this.senderIdType == null)
            {
                this.senderIdType = new NativeMsmqMessage.IntProperty(this, 0x16);
            }
            this.senderIdType.Value = value;
        }

        protected void EnsureTimeToReachQueueProperty(int value)
        {
            if (this.timeToReachQueue == null)
            {
                this.timeToReachQueue = new NativeMsmqMessage.IntProperty(this, 13);
            }
            this.timeToReachQueue.Value = value;
        }

        public NativeMsmqMessage.BufferProperty Body
        {
            get
            {
                return this.body;
            }
        }

        public NativeMsmqMessage.BufferProperty MessageId
        {
            get
            {
                return this.messageId;
            }
        }
    }
}

