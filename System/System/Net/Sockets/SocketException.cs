namespace System.Net.Sockets
{
    using System;
    using System.ComponentModel;
    using System.Net;
    using System.Runtime.Serialization;

    [Serializable]
    public class SocketException : Win32Exception
    {
        [NonSerialized]
        private EndPoint m_EndPoint;

        public SocketException() : base(Marshal.GetLastWin32Error())
        {
        }

        public SocketException(int errorCode) : base(errorCode)
        {
        }

        internal SocketException(EndPoint endPoint) : base(Marshal.GetLastWin32Error())
        {
            this.m_EndPoint = endPoint;
        }

        internal SocketException(SocketError socketError) : base((int) socketError)
        {
        }

        internal SocketException(int errorCode, EndPoint endPoint) : base(errorCode)
        {
            this.m_EndPoint = endPoint;
        }

        protected SocketException(SerializationInfo serializationInfo, StreamingContext streamingContext) : base(serializationInfo, streamingContext)
        {
        }

        public override int ErrorCode
        {
            get
            {
                return base.NativeErrorCode;
            }
        }

        public override string Message
        {
            get
            {
                if (this.m_EndPoint == null)
                {
                    return base.Message;
                }
                return (base.Message + " " + this.m_EndPoint.ToString());
            }
        }

        public SocketError SocketErrorCode
        {
            get
            {
                return (SocketError) base.NativeErrorCode;
            }
        }
    }
}

