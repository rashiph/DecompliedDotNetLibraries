namespace System.ServiceModel.Channels
{
    using System;
    using System.IO;
    using System.ServiceModel;

    internal class SingletonMessageDecoder : FramingDecoder
    {
        private int chunkBytesNeeded;
        private int chunkSize;
        private State currentState;
        private IntDecoder sizeDecoder;

        public SingletonMessageDecoder(long streamPosition) : base(streamPosition)
        {
            this.sizeDecoder = new IntDecoder();
            this.currentState = State.ChunkStart;
        }

        public int Decode(byte[] bytes, int offset, int size)
        {
            int num2;
            DecoderHelper.ValidateSize(size);
            try
            {
                int chunkBytesNeeded;
                switch (this.currentState)
                {
                    case State.ReadingEnvelopeChunkSize:
                        chunkBytesNeeded = this.sizeDecoder.Decode(bytes, offset, size);
                        if (this.sizeDecoder.IsValueDecoded)
                        {
                            this.chunkSize = this.sizeDecoder.Value;
                            this.sizeDecoder.Reset();
                            if (this.chunkSize != 0)
                            {
                                break;
                            }
                            this.currentState = State.EnvelopeEnd;
                        }
                        goto Label_0136;

                    case State.ChunkStart:
                        chunkBytesNeeded = 0;
                        this.currentState = State.ReadingEnvelopeBytes;
                        goto Label_0136;

                    case State.ReadingEnvelopeBytes:
                        chunkBytesNeeded = size;
                        if (chunkBytesNeeded > this.chunkBytesNeeded)
                        {
                            chunkBytesNeeded = this.chunkBytesNeeded;
                        }
                        this.chunkBytesNeeded -= chunkBytesNeeded;
                        if (this.chunkBytesNeeded == 0)
                        {
                            this.currentState = State.ChunkEnd;
                        }
                        goto Label_0136;

                    case State.ChunkEnd:
                        chunkBytesNeeded = 0;
                        this.currentState = State.ReadingEnvelopeChunkSize;
                        goto Label_0136;

                    case State.EnvelopeEnd:
                        base.ValidateRecordType(FramingRecordType.End, (FramingRecordType) bytes[offset]);
                        chunkBytesNeeded = 1;
                        this.currentState = State.End;
                        goto Label_0136;

                    case State.End:
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(base.CreateException(new InvalidDataException(System.ServiceModel.SR.GetString("FramingAtEnd"))));

                    default:
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(base.CreateException(new InvalidDataException(System.ServiceModel.SR.GetString("InvalidDecoderStateMachine"))));
                }
                this.currentState = State.ChunkStart;
                this.chunkBytesNeeded = this.chunkSize;
            Label_0136:
                base.StreamPosition += chunkBytesNeeded;
                num2 = chunkBytesNeeded;
            }
            catch (InvalidDataException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(base.CreateException(exception));
            }
            return num2;
        }

        public void Reset()
        {
            this.currentState = State.ChunkStart;
        }

        public int ChunkSize
        {
            get
            {
                if (this.currentState < State.ChunkStart)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("FramingValueNotAvailable")));
                }
                return this.chunkSize;
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

        public enum State
        {
            ReadingEnvelopeChunkSize,
            ChunkStart,
            ReadingEnvelopeBytes,
            ChunkEnd,
            EnvelopeEnd,
            End
        }
    }
}

