namespace System.Net.NetworkInformation
{
    using System;
    using System.ComponentModel;
    using System.Net.Sockets;
    using System.Runtime.Serialization;

    [Serializable]
    public class NetworkInformationException : Win32Exception
    {
        public NetworkInformationException() : base(Marshal.GetLastWin32Error())
        {
        }

        public NetworkInformationException(int errorCode) : base(errorCode)
        {
        }

        internal NetworkInformationException(SocketError socketError) : base((int) socketError)
        {
        }

        protected NetworkInformationException(SerializationInfo serializationInfo, StreamingContext streamingContext) : base(serializationInfo, streamingContext)
        {
        }

        public override int ErrorCode
        {
            get
            {
                return base.NativeErrorCode;
            }
        }
    }
}

