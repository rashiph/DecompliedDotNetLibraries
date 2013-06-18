namespace System.ServiceModel.Channels
{
    using System;
    using System.IO;
    using System.ServiceModel;

    internal class ClientDuplexDecoder : ClientFramingDecoder
    {
        private int envelopeBytesNeeded;
        private int envelopeSize;
        private FaultStringDecoder faultDecoder;
        private IntDecoder sizeDecoder;

        public ClientDuplexDecoder(long streamPosition) : base(streamPosition)
        {
            this.sizeDecoder = new IntDecoder();
        }

        public override int Decode(byte[] bytes, int offset, int size)
        {
            int num2;
            DecoderHelper.ValidateSize(size);
            try
            {
                int envelopeBytesNeeded;
                FramingRecordType type;
                switch (base.CurrentState)
                {
                    case ClientFramingDecoderState.ReadingUpgradeRecord:
                        type = (FramingRecordType) bytes[offset];
                        if (type != FramingRecordType.UpgradeResponse)
                        {
                            break;
                        }
                        envelopeBytesNeeded = 1;
                        base.CurrentState = ClientFramingDecoderState.UpgradeResponse;
                        goto Label_0248;

                    case ClientFramingDecoderState.UpgradeResponse:
                        envelopeBytesNeeded = 0;
                        base.CurrentState = ClientFramingDecoderState.ReadingUpgradeRecord;
                        goto Label_0248;

                    case ClientFramingDecoderState.ReadingAckRecord:
                        type = (FramingRecordType) bytes[offset];
                        if (type != FramingRecordType.Fault)
                        {
                            goto Label_00AA;
                        }
                        envelopeBytesNeeded = 1;
                        this.faultDecoder = new FaultStringDecoder();
                        base.CurrentState = ClientFramingDecoderState.ReadingFaultString;
                        goto Label_0248;

                    case ClientFramingDecoderState.Start:
                        envelopeBytesNeeded = 0;
                        base.CurrentState = ClientFramingDecoderState.ReadingEnvelopeRecord;
                        goto Label_0248;

                    case ClientFramingDecoderState.ReadingFaultString:
                        envelopeBytesNeeded = this.faultDecoder.Decode(bytes, offset, size);
                        if (this.faultDecoder.IsValueDecoded)
                        {
                            base.CurrentState = ClientFramingDecoderState.Fault;
                        }
                        goto Label_0248;

                    case ClientFramingDecoderState.Fault:
                        envelopeBytesNeeded = 0;
                        base.CurrentState = ClientFramingDecoderState.ReadingEndRecord;
                        goto Label_0248;

                    case ClientFramingDecoderState.ReadingEnvelopeRecord:
                        type = (FramingRecordType) bytes[offset];
                        if (type != FramingRecordType.End)
                        {
                            goto Label_00E4;
                        }
                        envelopeBytesNeeded = 1;
                        base.CurrentState = ClientFramingDecoderState.End;
                        goto Label_0248;

                    case ClientFramingDecoderState.ReadingEnvelopeSize:
                        envelopeBytesNeeded = this.sizeDecoder.Decode(bytes, offset, size);
                        if (this.sizeDecoder.IsValueDecoded)
                        {
                            base.CurrentState = ClientFramingDecoderState.EnvelopeStart;
                            this.envelopeSize = this.sizeDecoder.Value;
                            this.envelopeBytesNeeded = this.envelopeSize;
                        }
                        goto Label_0248;

                    case ClientFramingDecoderState.EnvelopeStart:
                        envelopeBytesNeeded = 0;
                        base.CurrentState = ClientFramingDecoderState.ReadingEnvelopeBytes;
                        goto Label_0248;

                    case ClientFramingDecoderState.ReadingEnvelopeBytes:
                        envelopeBytesNeeded = size;
                        if (envelopeBytesNeeded > this.envelopeBytesNeeded)
                        {
                            envelopeBytesNeeded = this.envelopeBytesNeeded;
                        }
                        this.envelopeBytesNeeded -= envelopeBytesNeeded;
                        if (this.envelopeBytesNeeded == 0)
                        {
                            base.CurrentState = ClientFramingDecoderState.EnvelopeEnd;
                        }
                        goto Label_0248;

                    case ClientFramingDecoderState.EnvelopeEnd:
                        envelopeBytesNeeded = 0;
                        base.CurrentState = ClientFramingDecoderState.ReadingEnvelopeRecord;
                        goto Label_0248;

                    case ClientFramingDecoderState.ReadingEndRecord:
                        base.ValidateRecordType(FramingRecordType.End, (FramingRecordType) bytes[offset]);
                        envelopeBytesNeeded = 1;
                        base.CurrentState = ClientFramingDecoderState.End;
                        goto Label_0248;

                    case ClientFramingDecoderState.End:
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(base.CreateException(new InvalidDataException(System.ServiceModel.SR.GetString("FramingAtEnd"))));

                    default:
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(base.CreateException(new InvalidDataException(System.ServiceModel.SR.GetString("InvalidDecoderStateMachine"))));
                }
                envelopeBytesNeeded = 0;
                base.CurrentState = ClientFramingDecoderState.ReadingAckRecord;
                goto Label_0248;
            Label_00AA:
                base.ValidatePreambleAck(type);
                envelopeBytesNeeded = 1;
                base.CurrentState = ClientFramingDecoderState.Start;
                goto Label_0248;
            Label_00E4:
                if (type == FramingRecordType.Fault)
                {
                    envelopeBytesNeeded = 1;
                    this.faultDecoder = new FaultStringDecoder();
                    base.CurrentState = ClientFramingDecoderState.ReadingFaultString;
                }
                else
                {
                    base.ValidateRecordType(FramingRecordType.SizedEnvelope, type);
                    envelopeBytesNeeded = 1;
                    base.CurrentState = ClientFramingDecoderState.ReadingEnvelopeSize;
                    this.sizeDecoder.Reset();
                }
            Label_0248:
                base.StreamPosition += envelopeBytesNeeded;
                num2 = envelopeBytesNeeded;
            }
            catch (InvalidDataException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(base.CreateException(exception));
            }
            return num2;
        }

        public int EnvelopeSize
        {
            get
            {
                if (base.CurrentState < ClientFramingDecoderState.EnvelopeStart)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("FramingValueNotAvailable")));
                }
                return this.envelopeSize;
            }
        }

        public override string Fault
        {
            get
            {
                if (base.CurrentState < ClientFramingDecoderState.Fault)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("FramingValueNotAvailable")));
                }
                return this.faultDecoder.Value;
            }
        }
    }
}

