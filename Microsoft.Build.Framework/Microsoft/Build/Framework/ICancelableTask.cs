namespace Microsoft.Build.Framework
{
    using System;

    public interface ICancelableTask : ITask
    {
        void Cancel();
    }
}

