namespace Microsoft.VisualBasic.Devices
{
    using System;
    using System.Runtime;

    public class NetworkAvailableEventArgs : EventArgs
    {
        private bool m_NetworkAvailable;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public NetworkAvailableEventArgs(bool networkAvailable)
        {
            this.m_NetworkAvailable = networkAvailable;
        }

        public bool IsNetworkAvailable
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.m_NetworkAvailable;
            }
        }
    }
}

