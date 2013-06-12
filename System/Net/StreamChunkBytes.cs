namespace System.Net
{
    using System;

    internal class StreamChunkBytes : IReadChunkBytes
    {
        public int BytesRead;
        public ConnectStream ChunkStream;
        private bool HavePush;
        private byte PushByte;
        public int TotalBytesRead;

        public StreamChunkBytes(ConnectStream connectStream)
        {
            this.ChunkStream = connectStream;
        }

        public int NextByte
        {
            get
            {
                if (this.HavePush)
                {
                    this.HavePush = false;
                    return this.PushByte;
                }
                return this.ChunkStream.ReadSingleByte();
            }
            set
            {
                this.PushByte = (byte) value;
                this.HavePush = true;
            }
        }
    }
}

