namespace System.Net
{
    using System;

    internal class SplitWritesState
    {
        private int _Index;
        private int _LastBufferConsumed;
        private BufferOffsetSize[] _RealBuffers;
        private BufferOffsetSize[] _UserBuffers;
        private const int c_SplitEncryptedBuffersSize = 0x10000;

        internal SplitWritesState(BufferOffsetSize[] buffers)
        {
            this._UserBuffers = buffers;
            this._LastBufferConsumed = 0;
            this._Index = 0;
            this._RealBuffers = null;
        }

        internal BufferOffsetSize[] GetNextBuffers()
        {
            int index = this._Index;
            int num2 = 0;
            int size = 0;
            int num4 = this._LastBufferConsumed;
            while (this._Index < this._UserBuffers.Length)
            {
                size = this._UserBuffers[this._Index].Size - this._LastBufferConsumed;
                num2 += size;
                if (num2 > 0x10000)
                {
                    size -= num2 - 0x10000;
                    num2 = 0x10000;
                    break;
                }
                size = 0;
                this._LastBufferConsumed = 0;
                this._Index++;
            }
            if (num2 == 0)
            {
                return null;
            }
            if (((num4 == 0) && (index == 0)) && (this._Index == this._UserBuffers.Length))
            {
                return this._UserBuffers;
            }
            int num5 = (size == 0) ? (this._Index - index) : ((this._Index - index) + 1);
            if ((this._RealBuffers == null) || (this._RealBuffers.Length != num5))
            {
                this._RealBuffers = new BufferOffsetSize[num5];
            }
            int num6 = 0;
            while (index < this._Index)
            {
                this._RealBuffers[num6++] = new BufferOffsetSize(this._UserBuffers[index].Buffer, this._UserBuffers[index].Offset + num4, this._UserBuffers[index].Size - num4, false);
                num4 = 0;
                index++;
            }
            if (size != 0)
            {
                this._RealBuffers[num6] = new BufferOffsetSize(this._UserBuffers[index].Buffer, this._UserBuffers[index].Offset + this._LastBufferConsumed, size, false);
                if ((this._LastBufferConsumed += size) == this._UserBuffers[this._Index].Size)
                {
                    this._Index++;
                    this._LastBufferConsumed = 0;
                }
            }
            return this._RealBuffers;
        }

        internal bool IsDone
        {
            get
            {
                if (this._LastBufferConsumed != 0)
                {
                    return false;
                }
                for (int i = this._Index; i < this._UserBuffers.Length; i++)
                {
                    if (this._UserBuffers[i].Size != 0)
                    {
                        return false;
                    }
                }
                return true;
            }
        }
    }
}

