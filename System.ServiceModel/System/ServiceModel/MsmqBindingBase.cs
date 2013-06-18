namespace System.ServiceModel
{
    using System;
    using System.ComponentModel;
    using System.ServiceModel.Channels;

    public abstract class MsmqBindingBase : Binding, IBindingRuntimePreferences
    {
        internal MsmqBindingElementBase transport;

        protected MsmqBindingBase()
        {
        }

        [DefaultValue(null)]
        public Uri CustomDeadLetterQueue
        {
            get
            {
                return this.transport.CustomDeadLetterQueue;
            }
            set
            {
                this.transport.CustomDeadLetterQueue = value;
            }
        }

        [DefaultValue(1)]
        public System.ServiceModel.DeadLetterQueue DeadLetterQueue
        {
            get
            {
                return this.transport.DeadLetterQueue;
            }
            set
            {
                this.transport.DeadLetterQueue = value;
            }
        }

        [DefaultValue(true)]
        public bool Durable
        {
            get
            {
                return this.transport.Durable;
            }
            set
            {
                this.transport.Durable = value;
            }
        }

        [DefaultValue(true)]
        public bool ExactlyOnce
        {
            get
            {
                return this.transport.ExactlyOnce;
            }
            set
            {
                this.transport.ExactlyOnce = value;
            }
        }

        [DefaultValue((long) 0x10000L)]
        public long MaxReceivedMessageSize
        {
            get
            {
                return this.transport.MaxReceivedMessageSize;
            }
            set
            {
                this.transport.MaxReceivedMessageSize = value;
            }
        }

        [DefaultValue(2)]
        public int MaxRetryCycles
        {
            get
            {
                return this.transport.MaxRetryCycles;
            }
            set
            {
                this.transport.MaxRetryCycles = value;
            }
        }

        [DefaultValue(true)]
        public bool ReceiveContextEnabled
        {
            get
            {
                return this.transport.ReceiveContextEnabled;
            }
            set
            {
                this.transport.ReceiveContextEnabled = value;
            }
        }

        [DefaultValue(0)]
        public System.ServiceModel.ReceiveErrorHandling ReceiveErrorHandling
        {
            get
            {
                return this.transport.ReceiveErrorHandling;
            }
            set
            {
                this.transport.ReceiveErrorHandling = value;
            }
        }

        [DefaultValue(5)]
        public int ReceiveRetryCount
        {
            get
            {
                return this.transport.ReceiveRetryCount;
            }
            set
            {
                this.transport.ReceiveRetryCount = value;
            }
        }

        [DefaultValue(typeof(TimeSpan), "00:30:00")]
        public TimeSpan RetryCycleDelay
        {
            get
            {
                return this.transport.RetryCycleDelay;
            }
            set
            {
                this.transport.RetryCycleDelay = value;
            }
        }

        public override string Scheme
        {
            get
            {
                return this.transport.Scheme;
            }
        }

        bool IBindingRuntimePreferences.ReceiveSynchronously
        {
            get
            {
                return this.ExactlyOnce;
            }
        }

        [DefaultValue(typeof(TimeSpan), "1.00:00:00")]
        public TimeSpan TimeToLive
        {
            get
            {
                return this.transport.TimeToLive;
            }
            set
            {
                this.transport.TimeToLive = value;
            }
        }

        [DefaultValue(false)]
        public bool UseMsmqTracing
        {
            get
            {
                return this.transport.UseMsmqTracing;
            }
            set
            {
                this.transport.UseMsmqTracing = value;
            }
        }

        [DefaultValue(false)]
        public bool UseSourceJournal
        {
            get
            {
                return this.transport.UseSourceJournal;
            }
            set
            {
                this.transport.UseSourceJournal = value;
            }
        }

        [DefaultValue(typeof(TimeSpan), "00:05:00")]
        public TimeSpan ValidityDuration
        {
            get
            {
                return this.transport.ValidityDuration;
            }
            set
            {
                this.transport.ValidityDuration = value;
            }
        }
    }
}

