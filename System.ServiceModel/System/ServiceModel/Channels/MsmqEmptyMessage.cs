namespace System.ServiceModel.Channels
{
    using System;

    internal class MsmqEmptyMessage : NativeMsmqMessage
    {
        public MsmqEmptyMessage() : base(0)
        {
        }
    }
}

