namespace System.Management
{
    using System;
    using System.Threading;

    internal class WmiEventState
    {
        private ManagementEventArgs args;
        private System.Delegate d;
        private System.Threading.AutoResetEvent h;

        internal WmiEventState(System.Delegate d, ManagementEventArgs args, System.Threading.AutoResetEvent h)
        {
            this.d = d;
            this.args = args;
            this.h = h;
        }

        public ManagementEventArgs Args
        {
            get
            {
                return this.args;
            }
        }

        public System.Threading.AutoResetEvent AutoResetEvent
        {
            get
            {
                return this.h;
            }
        }

        public System.Delegate Delegate
        {
            get
            {
                return this.d;
            }
        }
    }
}

