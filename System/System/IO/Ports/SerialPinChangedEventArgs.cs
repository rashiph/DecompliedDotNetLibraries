namespace System.IO.Ports
{
    using System;

    public class SerialPinChangedEventArgs : EventArgs
    {
        private SerialPinChange pinChanged;

        internal SerialPinChangedEventArgs(SerialPinChange eventCode)
        {
            this.pinChanged = eventCode;
        }

        public SerialPinChange EventType
        {
            get
            {
                return this.pinChanged;
            }
        }
    }
}

