namespace System.Net.NetworkInformation
{
    using System;

    public class NetworkAvailabilityEventArgs : EventArgs
    {
        private bool isAvailable;

        internal NetworkAvailabilityEventArgs(bool isAvailable)
        {
            this.isAvailable = isAvailable;
        }

        public bool IsAvailable
        {
            get
            {
                return this.isAvailable;
            }
        }
    }
}

