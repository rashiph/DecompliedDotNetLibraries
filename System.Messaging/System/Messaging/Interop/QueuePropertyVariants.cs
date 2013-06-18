namespace System.Messaging.Interop
{
    using System;

    internal class QueuePropertyVariants : MessagePropertyVariants
    {
        private const int MaxQueuePropertyIndex = 0x1a;

        public QueuePropertyVariants() : base(0x1a, 0x65)
        {
        }
    }
}

