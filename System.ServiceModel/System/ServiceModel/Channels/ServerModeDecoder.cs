namespace System.ServiceModel.Channels
{
    using System;
    using System.IO;
    using System.ServiceModel;

    internal class ServerModeDecoder : FramingDecoder
    {
        private State currentState = State.ReadingVersionRecord;
        private int majorVersion;
        private int minorVersion;
        private FramingMode mode;

        public int Decode(byte[] bytes, int offset, int size)
        {
            int num2;
            DecoderHelper.ValidateSize(size);
            try
            {
                int num;
                switch (this.currentState)
                {
                    case State.ReadingVersionRecord:
                        base.ValidateRecordType(FramingRecordType.Version, (FramingRecordType) bytes[offset]);
                        this.currentState = State.ReadingMajorVersion;
                        num = 1;
                        break;

                    case State.ReadingMajorVersion:
                        this.majorVersion = bytes[offset];
                        base.ValidateMajorVersion(this.majorVersion);
                        this.currentState = State.ReadingMinorVersion;
                        num = 1;
                        break;

                    case State.ReadingMinorVersion:
                        this.minorVersion = bytes[offset];
                        this.currentState = State.ReadingModeRecord;
                        num = 1;
                        break;

                    case State.ReadingModeRecord:
                        base.ValidateRecordType(FramingRecordType.Mode, (FramingRecordType) bytes[offset]);
                        this.currentState = State.ReadingModeValue;
                        num = 1;
                        break;

                    case State.ReadingModeValue:
                        this.mode = (FramingMode) bytes[offset];
                        base.ValidateFramingMode(this.mode);
                        this.currentState = State.Done;
                        num = 1;
                        break;

                    default:
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(base.CreateException(new InvalidDataException(System.ServiceModel.SR.GetString("InvalidDecoderStateMachine"))));
                }
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
            base.StreamPosition = 0L;
            this.currentState = State.ReadingVersionRecord;
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

        public int MajorVersion
        {
            get
            {
                if (this.currentState != State.Done)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("FramingValueNotAvailable")));
                }
                return this.majorVersion;
            }
        }

        public int MinorVersion
        {
            get
            {
                if (this.currentState != State.Done)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("FramingValueNotAvailable")));
                }
                return this.minorVersion;
            }
        }

        public FramingMode Mode
        {
            get
            {
                if (this.currentState != State.Done)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("FramingValueNotAvailable")));
                }
                return this.mode;
            }
        }

        public enum State
        {
            ReadingVersionRecord,
            ReadingMajorVersion,
            ReadingMinorVersion,
            ReadingModeRecord,
            ReadingModeValue,
            Done
        }
    }
}

