namespace System.Runtime.Remoting.Channels
{
    using System;

    internal class CachedSocketList
    {
        private SocketCachePolicy _socketCachePolicy;
        private int _socketCount = 0;
        private TimeSpan _socketLifetime;
        private CachedSocket _socketList;

        internal CachedSocketList(TimeSpan socketLifetime, SocketCachePolicy socketCachePolicy)
        {
            this._socketLifetime = socketLifetime;
            this._socketCachePolicy = socketCachePolicy;
            this._socketList = null;
        }

        internal SocketHandler GetSocket()
        {
            if (this._socketCount != 0)
            {
                lock (this)
                {
                    if (this._socketList != null)
                    {
                        SocketHandler handler = this._socketList.Handler;
                        this._socketList = this._socketList.Next;
                        handler.RaceForControl();
                        this._socketCount--;
                        return handler;
                    }
                }
            }
            return null;
        }

        internal void ReturnSocket(SocketHandler socket)
        {
            TimeSpan span = (TimeSpan) (DateTime.UtcNow - socket.CreationTime);
            bool flag = false;
            lock (this)
            {
                if ((this._socketCachePolicy != SocketCachePolicy.AbsoluteTimeout) || (span < this._socketLifetime))
                {
                    for (CachedSocket socket2 = this._socketList; socket2 != null; socket2 = socket2.Next)
                    {
                        if (socket == socket2.Handler)
                        {
                            return;
                        }
                    }
                    this._socketList = new CachedSocket(socket, this._socketList);
                    this._socketCount++;
                }
                else
                {
                    flag = true;
                }
            }
            if (flag)
            {
                socket.Close();
            }
        }

        internal void TimeoutSockets(DateTime currentTime, TimeSpan socketLifetime)
        {
            lock (this)
            {
                CachedSocket socket = null;
                CachedSocket next = this._socketList;
                while (next != null)
                {
                    if (((this._socketCachePolicy == SocketCachePolicy.AbsoluteTimeout) && ((currentTime - next.Handler.CreationTime) > socketLifetime)) || ((currentTime - next.LastUsed) > socketLifetime))
                    {
                        next.Handler.Close();
                        if (socket == null)
                        {
                            this._socketList = next.Next;
                            next = this._socketList;
                        }
                        else
                        {
                            next = next.Next;
                            socket.Next = next;
                        }
                        this._socketCount--;
                    }
                    else
                    {
                        socket = next;
                        next = next.Next;
                    }
                }
            }
        }
    }
}

