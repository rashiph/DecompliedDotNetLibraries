namespace System.ServiceModel
{
    using System;

    internal static class DeadLetterQueueHelper
    {
        public static bool IsDefined(DeadLetterQueue mode)
        {
            return ((mode >= DeadLetterQueue.None) && (mode <= DeadLetterQueue.Custom));
        }
    }
}

