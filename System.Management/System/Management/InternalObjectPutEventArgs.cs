namespace System.Management
{
    using System;

    internal class InternalObjectPutEventArgs : EventArgs
    {
        private ManagementPath path;

        internal InternalObjectPutEventArgs(ManagementPath path)
        {
            this.path = path.Clone();
        }

        internal ManagementPath Path
        {
            get
            {
                return this.path;
            }
        }
    }
}

