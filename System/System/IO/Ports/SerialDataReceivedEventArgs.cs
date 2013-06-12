namespace System.IO.Ports
{
    using System;

    public class SerialDataReceivedEventArgs : EventArgs
    {
        internal SerialData receiveType;

        internal SerialDataReceivedEventArgs(SerialData eventCode)
        {
            this.receiveType = eventCode;
        }

        public SerialData EventType
        {
            get
            {
                return this.receiveType;
            }
        }
    }
}

