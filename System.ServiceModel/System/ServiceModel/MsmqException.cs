namespace System.ServiceModel
{
    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;

    [Serializable]
    public class MsmqException : ExternalException
    {
        [NonSerialized]
        private bool? faultReceiver;
        [NonSerialized]
        private bool? faultSender;
        [NonSerialized]
        private Type outerExceptionType;

        public MsmqException()
        {
            this.faultSender = null;
            this.faultReceiver = null;
        }

        public MsmqException(string message) : base(message)
        {
            this.faultSender = null;
            this.faultReceiver = null;
        }

        protected MsmqException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            this.faultSender = null;
            this.faultReceiver = null;
        }

        public MsmqException(string message, Exception inner) : base(message, inner)
        {
            this.faultSender = null;
            this.faultReceiver = null;
        }

        public MsmqException(string message, int error) : base(message, error)
        {
            this.faultSender = null;
            this.faultReceiver = null;
        }

        private void TuneBehavior()
        {
            if (!this.faultSender.HasValue || !this.faultReceiver.HasValue)
            {
                switch (this.ErrorCode)
                {
                    case -1072824311:
                        this.faultSender = true;
                        this.faultReceiver = true;
                        this.outerExceptionType = typeof(AddressAccessDeniedException);
                        return;

                    case -1072824309:
                        this.faultSender = false;
                        this.faultReceiver = true;
                        this.outerExceptionType = typeof(EndpointNotFoundException);
                        return;

                    case -1072824300:
                        this.faultSender = false;
                        this.faultReceiver = false;
                        this.outerExceptionType = typeof(ArgumentException);
                        return;

                    case -1072824317:
                        this.faultSender = true;
                        this.faultReceiver = true;
                        this.outerExceptionType = typeof(EndpointNotFoundException);
                        return;

                    case -1072824290:
                        this.faultSender = false;
                        this.faultReceiver = false;
                        this.outerExceptionType = typeof(ArgumentException);
                        return;

                    case -1072824288:
                        this.faultSender = true;
                        this.faultReceiver = true;
                        this.outerExceptionType = typeof(ArgumentException);
                        return;

                    case -1072824293:
                        this.faultSender = false;
                        this.faultReceiver = false;
                        this.outerExceptionType = typeof(TimeoutException);
                        return;

                    case -1072824283:
                        this.faultSender = true;
                        this.faultReceiver = true;
                        this.outerExceptionType = typeof(AddressAccessDeniedException);
                        return;

                    case -1072824282:
                        this.faultSender = true;
                        this.faultReceiver = true;
                        this.outerExceptionType = typeof(CommunicationException);
                        return;

                    case -1072824281:
                        this.faultSender = true;
                        this.faultReceiver = true;
                        this.outerExceptionType = typeof(CommunicationException);
                        return;

                    case -1072824278:
                        this.faultSender = true;
                        this.faultReceiver = true;
                        this.outerExceptionType = typeof(CommunicationException);
                        return;

                    case -1072824276:
                        this.faultSender = true;
                        this.faultReceiver = true;
                        this.outerExceptionType = typeof(CommunicationException);
                        return;

                    case -1072824273:
                        this.faultSender = true;
                        this.faultReceiver = true;
                        this.outerExceptionType = typeof(CommunicationException);
                        return;

                    case -1072824271:
                        this.faultSender = true;
                        this.faultReceiver = true;
                        this.outerExceptionType = typeof(CommunicationException);
                        return;

                    case -1072824267:
                        this.faultSender = true;
                        this.faultReceiver = true;
                        this.outerExceptionType = typeof(CommunicationException);
                        return;

                    case -1072824266:
                        this.faultSender = true;
                        this.faultReceiver = true;
                        this.outerExceptionType = typeof(CommunicationException);
                        return;

                    case -1072824257:
                        this.faultSender = true;
                        this.faultReceiver = true;
                        this.outerExceptionType = typeof(CommunicationException);
                        return;

                    case -1072824255:
                        this.faultSender = true;
                        this.faultReceiver = true;
                        this.outerExceptionType = typeof(CommunicationException);
                        return;

                    case -1072824245:
                        this.faultSender = false;
                        this.faultReceiver = true;
                        this.outerExceptionType = typeof(EndpointNotFoundException);
                        return;

                    case -1072824244:
                        this.faultSender = false;
                        this.faultReceiver = true;
                        this.outerExceptionType = typeof(CommunicationException);
                        return;

                    case -1072824242:
                        this.faultSender = true;
                        this.faultReceiver = true;
                        this.outerExceptionType = typeof(CommunicationException);
                        return;

                    case -1072824240:
                        this.faultSender = true;
                        this.faultReceiver = true;
                        this.outerExceptionType = typeof(InvalidOperationException);
                        return;

                    case -1072824234:
                        this.faultSender = false;
                        this.faultReceiver = false;
                        this.outerExceptionType = typeof(InvalidOperationException);
                        return;

                    case -1072824232:
                        this.faultSender = false;
                        this.faultReceiver = true;
                        this.outerExceptionType = typeof(CommunicationException);
                        return;

                    case -1072824230:
                        this.faultSender = true;
                        this.faultReceiver = true;
                        this.outerExceptionType = typeof(EndpointNotFoundException);
                        return;

                    case -1072824215:
                        this.faultSender = false;
                        this.faultReceiver = true;
                        this.outerExceptionType = typeof(EndpointNotFoundException);
                        return;

                    case -1072824211:
                        this.faultSender = true;
                        this.faultReceiver = true;
                        this.outerExceptionType = typeof(CommunicationException);
                        return;

                    case -1072824209:
                        this.faultSender = true;
                        this.faultReceiver = true;
                        this.outerExceptionType = typeof(CommunicationException);
                        return;

                    case -1072824193:
                        this.faultSender = true;
                        this.faultReceiver = true;
                        this.outerExceptionType = typeof(CommunicationException);
                        return;

                    case -1072824192:
                        this.faultSender = true;
                        this.faultReceiver = true;
                        this.outerExceptionType = typeof(CommunicationException);
                        return;

                    case -1072824190:
                        this.faultSender = true;
                        this.faultReceiver = true;
                        this.outerExceptionType = typeof(CommunicationException);
                        return;
                }
                this.faultSender = true;
                this.faultReceiver = true;
                this.outerExceptionType = null;
            }
        }

        internal bool FaultReceiver
        {
            get
            {
                this.TuneBehavior();
                return this.faultReceiver.Value;
            }
        }

        internal bool FaultSender
        {
            get
            {
                this.TuneBehavior();
                return this.faultSender.Value;
            }
        }

        internal Exception Normalized
        {
            get
            {
                this.TuneBehavior();
                if (null != this.outerExceptionType)
                {
                    return (Activator.CreateInstance(this.outerExceptionType, new object[] { this.Message, this }) as Exception);
                }
                return this;
            }
        }
    }
}

