namespace System.Runtime.Remoting.Channels.Ipc
{
    using System;

    internal class PortConnection
    {
        private IpcPort _port;
        private DateTime _socketLastUsed;

        internal PortConnection(IpcPort port)
        {
            this._port = port;
            this._socketLastUsed = DateTime.Now;
        }

        internal DateTime LastUsed
        {
            get
            {
                return this._socketLastUsed;
            }
        }

        internal IpcPort Port
        {
            get
            {
                return this._port;
            }
        }
    }
}

