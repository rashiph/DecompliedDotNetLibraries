namespace System.Linq.Parallel
{
    using System;
    using System.Linq;
    using System.Threading;

    internal class CancellationState
    {
        internal CancellationToken ExternalCancellationToken;
        internal CancellationTokenSource InternalCancellationTokenSource;
        internal CancellationTokenSource MergedCancellationTokenSource;
        internal const int POLL_INTERVAL = 0x3f;
        internal Shared<bool> TopLevelDisposedFlag;

        internal CancellationState(CancellationToken externalCancellationToken)
        {
            this.ExternalCancellationToken = externalCancellationToken;
            this.TopLevelDisposedFlag = new Shared<bool>(false);
        }

        internal static void ThrowIfCanceled(CancellationToken token)
        {
            if (token.IsCancellationRequested)
            {
                throw new OperationCanceledException(token);
            }
        }

        internal static void ThrowWithStandardMessageIfCanceled(CancellationToken externalCancellationToken)
        {
            if (externalCancellationToken.IsCancellationRequested)
            {
                throw new OperationCanceledException(System.Linq.SR.GetString("PLINQ_ExternalCancellationRequested"), externalCancellationToken);
            }
        }

        internal CancellationToken MergedCancellationToken
        {
            get
            {
                if (this.MergedCancellationTokenSource != null)
                {
                    return this.MergedCancellationTokenSource.Token;
                }
                return new CancellationToken(false);
            }
        }
    }
}

