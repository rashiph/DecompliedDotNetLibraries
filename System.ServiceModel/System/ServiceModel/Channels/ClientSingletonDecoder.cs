namespace System.ServiceModel.Channels
{
    using System;
    using System.IO;
    using System.ServiceModel;

    internal class ClientSingletonDecoder : ClientFramingDecoder
    {
        private FaultStringDecoder faultDecoder;

        public ClientSingletonDecoder(long streamPosition) : base(streamPosition)
        {
        }

        public override int Decode(byte[] bytes, int offset, int size)
        {
            int num2;
            DecoderHelper.ValidateSize(size);
            try
            {
                int num;
                FramingRecordType type;
                switch (base.CurrentState)
                {
                    case ClientFramingDecoderState.ReadingUpgradeRecord:
                        type = (FramingRecordType) bytes[offset];
                        if (type != FramingRecordType.UpgradeResponse)
                        {
                            break;
                        }
                        num = 1;
                        base.CurrentState = ClientFramingDecoderState.UpgradeResponse;
                        goto Label_01A4;

                    case ClientFramingDecoderState.UpgradeResponse:
                        num = 0;
                        base.CurrentState = ClientFramingDecoderState.ReadingUpgradeRecord;
                        goto Label_01A4;

                    case ClientFramingDecoderState.ReadingAckRecord:
                        type = (FramingRecordType) bytes[offset];
                        if (type != FramingRecordType.Fault)
                        {
                            goto Label_009A;
                        }
                        num = 1;
                        this.faultDecoder = new FaultStringDecoder();
                        base.CurrentState = ClientFramingDecoderState.ReadingFaultString;
                        goto Label_01A4;

                    case ClientFramingDecoderState.Start:
                        num = 0;
                        base.CurrentState = ClientFramingDecoderState.ReadingEnvelopeRecord;
                        goto Label_01A4;

                    case ClientFramingDecoderState.ReadingFault:
                        type = (FramingRecordType) bytes[offset];
                        base.ValidateRecordType(FramingRecordType.Fault, type);
                        num = 1;
                        this.faultDecoder = new FaultStringDecoder();
                        base.CurrentState = ClientFramingDecoderState.ReadingFaultString;
                        goto Label_01A4;

                    case ClientFramingDecoderState.ReadingFaultString:
                        num = this.faultDecoder.Decode(bytes, offset, size);
                        if (this.faultDecoder.IsValueDecoded)
                        {
                            base.CurrentState = ClientFramingDecoderState.Fault;
                        }
                        goto Label_01A4;

                    case ClientFramingDecoderState.Fault:
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(base.CreateException(new InvalidDataException(System.ServiceModel.SR.GetString("FramingAtEnd"))));

                    case ClientFramingDecoderState.ReadingEnvelopeRecord:
                        type = (FramingRecordType) bytes[offset];
                        if (type != FramingRecordType.End)
                        {
                            goto Label_00D4;
                        }
                        num = 1;
                        base.CurrentState = ClientFramingDecoderState.End;
                        goto Label_01A4;

                    case ClientFramingDecoderState.EnvelopeStart:
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(base.CreateException(new InvalidDataException(System.ServiceModel.SR.GetString("FramingAtEnd"))));

                    default:
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(base.CreateException(new InvalidDataException(System.ServiceModel.SR.GetString("InvalidDecoderStateMachine"))));
                }
                num = 0;
                base.CurrentState = ClientFramingDecoderState.ReadingAckRecord;
                goto Label_01A4;
            Label_009A:
                base.ValidatePreambleAck(type);
                num = 1;
                base.CurrentState = ClientFramingDecoderState.Start;
                goto Label_01A4;
            Label_00D4:
                if (type == FramingRecordType.Fault)
                {
                    num = 0;
                    base.CurrentState = ClientFramingDecoderState.ReadingFault;
                }
                else
                {
                    base.ValidateRecordType(FramingRecordType.UnsizedEnvelope, type);
                    num = 1;
                    base.CurrentState = ClientFramingDecoderState.EnvelopeStart;
                }
            Label_01A4:
                base.StreamPosition += num;
                num2 = num;
            }
            catch (InvalidDataException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(base.CreateException(exception));
            }
            return num2;
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

