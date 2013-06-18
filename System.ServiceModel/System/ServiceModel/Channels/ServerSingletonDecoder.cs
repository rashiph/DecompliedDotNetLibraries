namespace System.ServiceModel.Channels
{
    using System;
    using System.IO;
    using System.ServiceModel;

    internal class ServerSingletonDecoder : FramingDecoder
    {
        private string contentType;
        private ContentTypeStringDecoder contentTypeDecoder;
        private State currentState;
        private string upgrade;
        private ViaStringDecoder viaDecoder;

        public ServerSingletonDecoder(long streamPosition, int maxViaLength, int maxContentTypeLength) : base(streamPosition)
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
                        goto Label_022E;

                    case State.ReadingViaString:
                        num = this.viaDecoder.Decode(bytes, offset, size);
                        if (this.viaDecoder.IsValueDecoded)
                        {
                            this.currentState = State.ReadingContentTypeRecord;
                        }
                        goto Label_022E;

                    case State.ReadingContentTypeRecord:
                        type = (FramingRecordType) bytes[offset];
                        if (type != FramingRecordType.KnownEncoding)
                        {
                            break;
                        }
                        num = 1;
                        this.currentState = State.ReadingContentTypeByte;
                        goto Label_022E;

                    case State.ReadingContentTypeString:
                        num = this.contentTypeDecoder.Decode(bytes, offset, size);
                        if (this.contentTypeDecoder.IsValueDecoded)
                        {
                            this.currentState = State.PreUpgradeStart;
                            this.contentType = this.contentTypeDecoder.Value;
                        }
                        goto Label_022E;

                    case State.ReadingContentTypeByte:
                        this.contentType = ContentTypeStringDecoder.GetString((FramingEncodingType) bytes[offset]);
                        num = 1;
                        this.currentState = State.PreUpgradeStart;
                        goto Label_022E;

                    case State.PreUpgradeStart:
                        num = 0;
                        this.currentState = State.ReadingUpgradeRecord;
                        goto Label_022E;

                    case State.ReadingUpgradeRecord:
                        type = (FramingRecordType) bytes[offset];
                        if (type != FramingRecordType.UpgradeRequest)
                        {
                            goto Label_015D;
                        }
                        num = 1;
                        this.contentTypeDecoder.Reset();
                        this.currentState = State.ReadingUpgradeString;
                        goto Label_022E;

                    case State.ReadingUpgradeString:
                        num = this.contentTypeDecoder.Decode(bytes, offset, size);
                        if (this.contentTypeDecoder.IsValueDecoded)
                        {
                            this.currentState = State.UpgradeRequest;
                            this.upgrade = this.contentTypeDecoder.Value;
                        }
                        goto Label_022E;

                    case State.UpgradeRequest:
                        num = 0;
                        this.currentState = State.ReadingUpgradeRecord;
                        goto Label_022E;

                    case State.ReadingPreambleEndRecord:
                        type = (FramingRecordType) bytes[offset];
                        base.ValidateRecordType(FramingRecordType.PreambleEnd, type);
                        num = 1;
                        this.currentState = State.Start;
                        goto Label_022E;

                    case State.Start:
                        num = 0;
                        this.currentState = State.ReadingEnvelopeRecord;
                        goto Label_022E;

                    case State.ReadingEnvelopeRecord:
                        base.ValidateRecordType(FramingRecordType.UnsizedEnvelope, (FramingRecordType) bytes[offset]);
                        num = 1;
                        this.currentState = State.EnvelopeStart;
                        goto Label_022E;

                    case State.EnvelopeStart:
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(base.CreateException(new InvalidDataException(System.ServiceModel.SR.GetString("FramingAtEnd"))));

                    default:
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(base.CreateException(new InvalidDataException(System.ServiceModel.SR.GetString("InvalidDecoderStateMachine"))));
                }
                base.ValidateRecordType(FramingRecordType.ExtensibleEncoding, type);
                num = 1;
                this.contentTypeDecoder.Reset();
                this.currentState = State.ReadingContentTypeString;
                goto Label_022E;
            Label_015D:
                num = 0;
                this.currentState = State.ReadingPreambleEndRecord;
            Label_022E:
                base.StreamPosition += num;
                num2 = num;
            }
            catch (InvalidDataException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(base.CreateException(exception));
            }
            return num2;
        }

        public void Reset()
        {
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
            EnvelopeStart,
            ReadingEnvelopeChunkSize,
            ChunkStart,
            ReadingEnvelopeChunk,
            ChunkEnd,
            End
        }
    }
}

