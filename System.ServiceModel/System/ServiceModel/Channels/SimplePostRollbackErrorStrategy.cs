namespace System.ServiceModel.Channels
{
    using System;
    using System.Threading;

    internal class SimplePostRollbackErrorStrategy : IPostRollbackErrorStrategy
    {
        private const int Attempts = 50;
        private int attemptsLeft = 50;
        private long lookupId;
        private const int MillisecondsToSleep = 100;

        internal SimplePostRollbackErrorStrategy(long lookupId)
        {
            this.lookupId = lookupId;
        }

        public bool AnotherTryNeeded()
        {
            if (--this.attemptsLeft > 0)
            {
                if (this.attemptsLeft == 0x31)
                {
                    MsmqDiagnostics.MessageLockedUnderTheTransaction(this.lookupId);
                }
                Thread.Sleep(TimeSpan.FromMilliseconds(100.0));
                return true;
            }
            MsmqDiagnostics.MoveOrDeleteAttemptFailed(this.lookupId);
            return false;
        }
    }
}

