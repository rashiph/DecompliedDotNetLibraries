namespace System.Net
{
    using System;

    internal class ScatterGatherBuffers
    {
        private int chunkCount;
        private MemoryChunk currentChunk;
        private MemoryChunk headChunk;
        private int nextChunkLength;
        private int totalLength;

        internal ScatterGatherBuffers()
        {
            this.nextChunkLength = 0x400;
        }

        internal ScatterGatherBuffers(long totalSize)
        {
            this.nextChunkLength = 0x400;
            if (totalSize > 0L)
            {
                this.currentChunk = this.AllocateMemoryChunk((totalSize > 0x7fffffffL) ? 0x7fffffff : ((int) totalSize));
            }
        }

        private MemoryChunk AllocateMemoryChunk(int newSize)
        {
            if (newSize > this.nextChunkLength)
            {
                this.nextChunkLength = newSize;
            }
            MemoryChunk chunk = new MemoryChunk(this.nextChunkLength);
            if (this.Empty)
            {
                this.headChunk = chunk;
            }
            this.nextChunkLength *= 2;
            this.chunkCount++;
            return chunk;
        }

        internal BufferOffsetSize[] GetBuffers()
        {
            if (this.Empty)
            {
                return null;
            }
            BufferOffsetSize[] sizeArray = new BufferOffsetSize[this.chunkCount];
            int index = 0;
            for (MemoryChunk chunk = this.headChunk; chunk != null; chunk = chunk.Next)
            {
                sizeArray[index] = new BufferOffsetSize(chunk.Buffer, 0, chunk.FreeOffset, false);
                index++;
            }
            return sizeArray;
        }

        internal void Write(byte[] buffer, int offset, int count)
        {
            while (count > 0)
            {
                int num = this.Empty ? 0 : (this.currentChunk.Buffer.Length - this.currentChunk.FreeOffset);
                if (num == 0)
                {
                    MemoryChunk chunk = this.AllocateMemoryChunk(count);
                    if (this.currentChunk != null)
                    {
                        this.currentChunk.Next = chunk;
                    }
                    this.currentChunk = chunk;
                }
                int num2 = (count < num) ? count : num;
                Buffer.BlockCopy(buffer, offset, this.currentChunk.Buffer, this.currentChunk.FreeOffset, num2);
                offset += num2;
                count -= num2;
                this.totalLength += num2;
                this.currentChunk.FreeOffset += num2;
            }
        }

        private bool Empty
        {
            get
            {
                if (this.headChunk != null)
                {
                    return (this.chunkCount == 0);
                }
                return true;
            }
        }

        internal int Length
        {
            get
            {
                return this.totalLength;
            }
        }

        private class MemoryChunk
        {
            internal byte[] Buffer;
            internal int FreeOffset;
            internal ScatterGatherBuffers.MemoryChunk Next;

            internal MemoryChunk(int bufferSize)
            {
                this.Buffer = new byte[bufferSize];
            }
        }
    }
}

