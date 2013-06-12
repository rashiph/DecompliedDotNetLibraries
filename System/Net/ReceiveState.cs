namespace System.Net
{
    using System;

    internal class ReceiveState
    {
        internal byte[] Buffer;
        private const int bufferSize = 0x400;
        internal CommandStream Connection;
        internal ResponseDescription Resp;
        internal int ValidThrough;

        internal ReceiveState(CommandStream connection)
        {
            this.Connection = connection;
            this.Resp = new ResponseDescription();
            this.Buffer = new byte[0x400];
            this.ValidThrough = 0;
        }
    }
}

