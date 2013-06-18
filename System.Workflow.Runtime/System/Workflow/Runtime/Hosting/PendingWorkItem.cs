namespace System.Workflow.Runtime.Hosting
{
    using System;
    using System.Data.SqlTypes;

    internal sealed class PendingWorkItem
    {
        public int Blocked;
        public string Info;
        public Guid InstanceId;
        public SqlDateTime NextTimer;
        public byte[] SerializedActivity;
        public Guid StateId;
        public int Status;
        public ItemType Type;
        public bool Unlocked;

        public enum ItemType
        {
            Instance,
            CompletedScope,
            ActivationComplete
        }
    }
}

