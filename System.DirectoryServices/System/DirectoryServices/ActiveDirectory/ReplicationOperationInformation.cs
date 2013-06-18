namespace System.DirectoryServices.ActiveDirectory
{
    using System;

    public class ReplicationOperationInformation
    {
        internal ReplicationOperationCollection collection;
        internal ReplicationOperation currentOp;
        internal DateTime startTime;

        public ReplicationOperation CurrentOperation
        {
            get
            {
                return this.currentOp;
            }
        }

        public DateTime OperationStartTime
        {
            get
            {
                return this.startTime;
            }
        }

        public ReplicationOperationCollection PendingOperations
        {
            get
            {
                return this.collection;
            }
        }
    }
}

