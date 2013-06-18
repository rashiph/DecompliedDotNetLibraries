namespace System.ServiceModel.Channels
{
    using System;

    internal abstract class ClientFramingDecoder : FramingDecoder
    {
        private ClientFramingDecoderState currentState;

        protected ClientFramingDecoder(long streamPosition) : base(streamPosition)
        {
            this.currentState = ClientFramingDecoderState.ReadingUpgradeRecord;
        }

        public abstract int Decode(byte[] bytes, int offset, int size);

        public ClientFramingDecoderState CurrentState
        {
            get
            {
                return this.currentState;
            }
            protected set
            {
                this.currentState = value;
            }
        }

        protected override string CurrentStateAsString
        {
            get
            {
                return this.currentState.ToString();
            }
        }

        public abstract string Fault { get; }
    }
}

