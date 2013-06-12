namespace System
{
    using System.Runtime.InteropServices;
    using System.Threading;

    [ComVisible(true)]
    public interface IAsyncResult
    {
        object AsyncState { get; }

        WaitHandle AsyncWaitHandle { get; }

        bool CompletedSynchronously { get; }

        bool IsCompleted { get; }
    }
}

