namespace System.Activities
{
    using System;

    internal interface IAsyncCodeActivity
    {
        void FinishExecution(AsyncCodeActivityContext context, IAsyncResult result);
    }
}

