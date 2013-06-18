namespace System.ServiceModel.Channels
{
    using System;
    using System.IO;
    using System.ServiceModel;

    internal class ServerSingletonSizedDecoder : FramingDecoder
    {
        private string contentType;
        private ContentTypeStringDecoder contentTypeDecoder;
        private State currentState;
        private ViaStringDecoder viaDecoder;

        public ServerSingletonSizedDecoder(long streamPosition, int maxViaLength, int maxContentTypeLength) : base(streamPosition)
        {
            this.viaDecoder = new ViaStringDecoder(maxViaLength);
            this.contentTypeDecoder = new ContentTypeStringDecoder(maxContentTypeLength);
            this.currentState = State.ReadingViaRecord;
        }

        public int Decode(byte[] bytes, int offset, int size)
        {
            int num2;
            DecoderHelper.ValidateSize(size);
            try
            {
                int num;
                FramingRecordType type;
                switch (this.currentState)
                {
                    case State.ReadingViaRecord:
                        type = (FramingRecordType) bytes[offset];
                        base.ValidateRecordType(FramingRecordType.Via, type);
                        num = 1;
                        this.viaDecoder.Reset();
                        this.currentState = State.ReadingViaString;
                        goto Label_0148;

                    case State.ReadingViaString:
                        num = this.viaDecoder.Decode(bytes, offset, size);
                        if (this.viaDecoder.IsValueDecoded)
                        {
                            this.currentState = State.ReadingContentTypeRecord;
                        }
                        goto Label_0148;

                    case State.ReadingContentTypeRecord:
                        type = (FramingRecordType) bytes[offset];
                        if (type != FramingRecordType.KnownEncoding)
                        {
                            break;
                        }
                        num = 1;
                        this.currentState = State.ReadingContentTypeByte;
                        goto Label_0148;

                    case State.ReadingContentTypeString:
                        num = this.contentTypeDecoder.Decode(bytes, offset, size);
                        if (this.contentTypeDecoder.IsValueDecoded)
                        {
                            this.currentState = State.Start;
                            this.contentType = this.contentTypeDecoder.Value;
                        }
                        goto Label_0148;

                    case State.ReadingContentTypeByte:
                        this.contentType = ContentTypeStringDecoder.GetString((FramingEncodingType) bytes[offset]);
                        num = 1;
                        this.currentState = State.Start;
                        goto Label_0148;

                    case State.Start:
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(base.CreateException(new InvalidDataException(System.ServiceModel.SR.GetString("FramingAtEnd"))));

                    default:
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(base.CreateException(new InvalidDataException(System.ServiceModel.SR.GetString("InvalidDecoderStateMachine"))));
                }
                base.ValidateRecordType(FramingRecordType.ExtensibleEncoding, type);
                num = 1;
                this.contentTypeDecoder.Reset();
                this.currentState = State.ReadingContentTypeString;
            Label_0148:
                base.StreamPosition += num;
                num2 = num;
            }
            catch (InvalidDataException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(base.CreateException(exception));
            }
            return num2;
        }

        public void Reset(long streamPosition)
        {
            base.StreamPosition = streamPosition;
            this.currentState = State.ReadingViaRecord;
        }

        public string ContentType
        {
            get
            {
                if (this.currentState < State.Start)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("FramingValueNotAvailable")));
                }
                return this.contentType;
            }
        }

        public State CurrentState
        {
            get
            {
                return this.currentState;
            }
        }

        protected override string CurrentStateAsString
        {
            get
            {
                return this.currentState.ToString();
            }
        }

        public Uri Via
        {
            get
            {
                if (this.currentState < State.ReadingContentTypeRecord)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("FramingValueNotAvailable")));
                }
                return this.viaDecoder.ValueAsUri;
            }
        }

        public enum State
        {
            ReadingViaRecord,
            ReadingViaString,
            ReadingContentTypeRecord,
            ReadingContentTypeString,
            ReadingContentTypeByte,
            Start
        }
    }
}

