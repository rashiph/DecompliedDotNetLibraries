namespace System.IO.Ports
{
    using System;

    public class SerialErrorReceivedEventArgs : EventArgs
    {
        private SerialError errorType;

        internal SerialErrorReceivedEventArgs(SerialError eventCode)
        {
            this.errorType = eventCode;
        }

        public SerialError EventType
        {
            get
            {
                return this.errorType;
            }
        }
    }
}

