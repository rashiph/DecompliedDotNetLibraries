namespace System.Net
{
    using System;
    using System.Net.Sockets;

    [Serializable]
    public abstract class EndPoint
    {
        protected EndPoint()
        {
        }

        public virtual EndPoint Create(SocketAddress socketAddress)
        {
            throw ExceptionHelper.MethodNotImplementedException;
        }

        public virtual SocketAddress Serialize()
        {
            throw ExceptionHelper.MethodNotImplementedException;
        }

        public virtual System.Net.Sockets.AddressFamily AddressFamily
        {
            get
            {
                throw ExceptionHelper.PropertyNotImplementedException;
            }
        }
    }
}

