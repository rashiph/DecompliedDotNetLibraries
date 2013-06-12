namespace System.IO.Compression
{
    using System;
    using System.Runtime.InteropServices;

    internal class DeflateInput
    {
        private byte[] buffer;
        private int count;
        private int startIndex;

        internal void ConsumeBytes(int n)
        {
            this.startIndex += n;
            this.count -= n;
        }

        internal InputState DumpState()
        {
            InputState state;
            state.count = this.count;
            state.startIndex = this.startIndex;
            return state;
        }

        internal void RestoreState(InputState state)
        {
            this.count = state.count;
            this.startIndex = state.startIndex;
        }

        internal byte[] Buffer
        {
            get
            {
                return this.buffer;
            }
            set
            {
                this.buffer = value;
            }
        }

        internal int Count
        {
            get
            {
                return this.count;
            }
            set
            {
                this.count = value;
            }
        }

        internal int StartIndex
        {
            get
            {
                return this.startIndex;
            }
            set
            {
                this.startIndex = value;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct InputState
        {
            internal int count;
            internal int startIndex;
        }
    }
}

