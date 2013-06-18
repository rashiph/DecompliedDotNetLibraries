namespace System.ServiceModel.Channels
{
    using System;
    using System.IO;
    using System.ServiceModel;

    internal class ServerSessionDecoder : FramingDecoder
    {
        private string contentType;
        private StringDecoder contentTypeDecoder;
        private State currentState;
        private int envelopeBytesNeeded;
        private int envelopeSize;
        private IntDecoder sizeDecoder;
        private string upgrade;
        private ViaStringDecoder viaDecoder;

        public ServerSessionDecoder(long streamPosition, int maxViaLength, int maxContentTypeLength) : base(streamPosition)
        {
            this.viaDecoder = new ViaStringDecoder(maxViaLength);
            this.contentTypeDecoder = new ContentTypeStringDecoder(maxContentTypeLength);
            this.sizeDecoder = new IntDecoder();
            this.currentState = State.ReadingViaRecord;
        }

        public int Decode(byte[] bytes, int offset, int size)
        {
            int num2;
            DecoderHelper.ValidateSize(size);
            try
            {
                int envelopeBytesNeeded;
                FramingRecordType type;
                switch (this.currentState)
                {
                    case State.ReadingViaRecord:
                        type = (FramingRecordType) bytes[offset];
                        base.ValidateRecordType(FramingRecordType.Via, type);
                        envelopeBytesNeeded = 1;
                        this.viaDecoder.Reset();
                        this.currentState = State.ReadingViaString;
                        goto Label_0312;

                    case State.ReadingViaString:
                        envelopeBytesNeeded = this.viaDecoder.Decode(bytes, offset, size);
                        if (this.viaDecoder.IsValueDecoded)
                        {
                            this.currentState = State.ReadingContentTypeRecord;
                        }
                        goto Label_0312;

                    case State.ReadingContentTypeRecord:
                        type = (FramingRecordType) bytes[offset];
                        if (type != FramingRecordType.KnownEncoding)
                        {
                            break;
                        }
                        envelopeBytesNeeded = 1;
                        this.currentState = State.ReadingContentTypeByte;
                        goto Label_0312;

                    case State.ReadingContentTypeString:
                        envelopeBytesNeeded = this.contentTypeDecoder.Decode(bytes, offset, size);
                        if (this.contentTypeDecoder.IsValueDecoded)
                        {
                            this.currentState = State.PreUpgradeStart;
                            this.contentType = this.contentTypeDecoder.Value;
                        }
                        goto Label_0312;

                    case State.ReadingContentTypeByte:
                        this.contentType = ContentTypeStringDecoder.GetString((FramingEncodingType) bytes[offset]);
                        envelopeBytesNeeded = 1;
                        this.currentState = State.PreUpgradeStart;
                        goto Label_0312;

                    case State.PreUpgradeStart:
                        envelopeBytesNeeded = 0;
                        this.currentState = State.ReadingUpgradeRecord;
                        goto Label_0312;

                    case State.ReadingUpgradeRecord:
                        type = (FramingRecordType) bytes[offset];
                        if (type != FramingRecordType.UpgradeRequest)
                        {
                            goto Label_0171;
                        }
                        envelopeBytesNeeded = 1;
                        this.contentTypeDecoder.Reset();
                        this.currentState = State.ReadingUpgradeString;
                        goto Label_0312;

                    case State.ReadingUpgradeString:
                        envelopeBytesNeeded = this.contentTypeDecoder.Decode(bytes, offset, size);
                        if (this.contentTypeDecoder.IsValueDecoded)
                        {
                            this.currentState = State.UpgradeRequest;
                            this.upgrade = this.contentTypeDecoder.Value;
                        }
                        goto Label_0312;

                    case State.UpgradeRequest:
                        envelopeBytesNeeded = 0;
                        this.currentState = State.ReadingUpgradeRecord;
                        goto Label_0312;

                    case State.ReadingPreambleEndRecord:
                        type = (FramingRecordType) bytes[offset];
                        base.ValidateRecordType(FramingRecordType.PreambleEnd, type);
                        envelopeBytesNeeded = 1;
                        this.currentState = State.Start;
                        goto Label_0312;

                    case State.Start:
                        envelopeBytesNeeded = 0;
                        this.currentState = State.ReadingEndRecord;
                        goto Label_0312;

                    case State.ReadingEnvelopeRecord:
                        base.ValidateRecordType(FramingRecordType.SizedEnvelope, (FramingRecordType) bytes[offset]);
                        envelopeBytesNeeded = 1;
                        this.currentState = State.ReadingEnvelopeSize;
                        this.sizeDecoder.Reset();
                        goto Label_0312;

                    case State.ReadingEnvelopeSize:
                        envelopeBytesNeeded = this.sizeDecoder.Decode(bytes, offset, size);
                        if (this.sizeDecoder.IsValueDecoded)
                        {
                            this.currentState = State.EnvelopeStart;
                            this.envelopeSize = this.sizeDecoder.Value;
                            this.envelopeBytesNeeded = this.envelopeSize;
                        }
                        goto Label_0312;

                    case State.EnvelopeStart:
                        envelopeBytesNeeded = 0;
                        this.currentState = State.ReadingEnvelopeBytes;
                        goto Label_0312;

                    case State.ReadingEnvelopeBytes:
                        envelopeBytesNeeded = size;
                        if (envelopeBytesNeeded > this.envelopeBytesNeeded)
                        {
                            envelopeBytesNeeded = this.envelopeBytesNeeded;
                        }
                        this.envelopeBytesNeeded -= envelopeBytesNeeded;
                        if (this.envelopeBytesNeeded == 0)
                        {
                            this.currentState = State.EnvelopeEnd;
                        }
                        goto Label_0312;

                    case State.EnvelopeEnd:
                        envelopeBytesNeeded = 0;
                        this.currentState = State.ReadingEndRecord;
                        goto Label_0312;

                    case State.ReadingEndRecord:
                        type = (FramingRecordType) bytes[offset];
                        if (type != FramingRecordType.End)
                        {
                            goto Label_020C;
                        }
                        envelopeBytesNeeded = 1;
                        this.currentState = State.End;
                        goto Label_0312;

                    case State.End:
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(base.CreateException(new InvalidDataException(System.ServiceModel.SR.GetString("FramingAtEnd"))));

                    default:
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(base.CreateException(new InvalidDataException(System.ServiceModel.SR.GetString("InvalidDecoderStateMachine"))));
                }
                base.ValidateRecordType(FramingRecordType.ExtensibleEncoding, type);
                envelopeBytesNeeded = 1;
                this.contentTypeDecoder.Reset();
                this.currentState = State.ReadingContentTypeString;
                goto Label_0312;
            Label_0171:
                envelopeBytesNeeded = 0;
                this.currentState = State.ReadingPreambleEndRecord;
                goto Label_0312;
            Label_020C:
                envelopeBytesNeeded = 0;
                this.currentState = State.ReadingEnvelopeRecord;
            Label_0312:
                base.StreamPosition += envelopeBytesNeeded;
                num2 = envelopeBytesNeeded;
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
                if (this.currentState < State.PreUpgradeStart)
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

        public int EnvelopeSize
        {
            get
            {
                if (this.currentState < State.EnvelopeStart)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("FramingValueNotAvailable")));
                }
                return this.envelopeSize;
            }
        }

        public string Upgrade
        {
            get
            {
                if (this.currentState != State.UpgradeRequest)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("FramingValueNotAvailable")));
                }
                return this.upgrade;
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
            PreUpgradeStart,
            ReadingUpgradeRecord,
            ReadingUpgradeString,
            UpgradeRequest,
            ReadingPreambleEndRecord,
            Start,
            ReadingEnvelopeRecord,
            ReadingEnvelopeSize,
            EnvelopeStart,
            ReadingEnvelopeBytes,
            EnvelopeEnd,
            ReadingEndRecord,
            End
        }
    }
}

