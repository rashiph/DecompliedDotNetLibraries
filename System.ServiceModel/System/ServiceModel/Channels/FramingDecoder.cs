namespace System.ServiceModel.Channels
{
    using System;
    using System.IO;
    using System.ServiceModel;

    internal abstract class FramingDecoder
    {
        private long streamPosition;

        protected FramingDecoder()
        {
        }

        protected FramingDecoder(long streamPosition)
        {
            this.streamPosition = streamPosition;
        }

        protected Exception CreateException(InvalidDataException innerException)
        {
            return new ProtocolException(System.ServiceModel.SR.GetString("FramingError", new object[] { this.StreamPosition, this.CurrentStateAsString }), innerException);
        }

        protected Exception CreateException(InvalidDataException innerException, string framingFault)
        {
            Exception exception = this.CreateException(innerException);
            FramingEncodingString.AddFaultString(exception, framingFault);
            return exception;
        }

        private Exception CreateInvalidRecordTypeException(FramingRecordType expectedType, FramingRecordType foundType)
        {
            return new InvalidDataException(System.ServiceModel.SR.GetString("FramingRecordTypeMismatch", new object[] { expectedType.ToString(), foundType.ToString() }));
        }

        public Exception CreatePrematureEOFException()
        {
            return this.CreateException(new InvalidDataException(System.ServiceModel.SR.GetString("FramingPrematureEOF")));
        }

        protected void ValidateFramingMode(FramingMode mode)
        {
            switch (mode)
            {
                case FramingMode.Singleton:
                case FramingMode.Duplex:
                case FramingMode.Simplex:
                case FramingMode.SingletonSized:
                    return;
            }
            Exception exception = this.CreateException(new InvalidDataException(System.ServiceModel.SR.GetString("FramingModeNotSupported", new object[] { mode.ToString() })), "http://schemas.microsoft.com/ws/2006/05/framing/faults/UnsupportedMode");
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(exception);
        }

        protected void ValidateMajorVersion(int majorVersion)
        {
            if (majorVersion != 1)
            {
                Exception exception = this.CreateException(new InvalidDataException(System.ServiceModel.SR.GetString("FramingVersionNotSupported", new object[] { majorVersion })), "http://schemas.microsoft.com/ws/2006/05/framing/faults/UnsupportedVersion");
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(exception);
            }
        }

        protected void ValidatePreambleAck(FramingRecordType foundType)
        {
            if (foundType != FramingRecordType.PreambleAck)
            {
                string str;
                Exception innerException = this.CreateInvalidRecordTypeException(FramingRecordType.PreambleAck, foundType);
                if ((((byte) foundType) == 0x68) || (((byte) foundType) == 0x48))
                {
                    str = System.ServiceModel.SR.GetString("PreambleAckIncorrectMaybeHttp");
                }
                else
                {
                    str = System.ServiceModel.SR.GetString("PreambleAckIncorrect");
                }
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ProtocolException(str, innerException));
            }
        }

        protected void ValidateRecordType(FramingRecordType expectedType, FramingRecordType foundType)
        {
            if (foundType != expectedType)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(this.CreateInvalidRecordTypeException(expectedType, foundType));
            }
        }

        protected abstract string CurrentStateAsString { get; }

        public long StreamPosition
        {
            get
            {
                return this.streamPosition;
            }
            set
            {
                this.streamPosition = value;
            }
        }
    }
}

