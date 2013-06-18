namespace System.Workflow.Activities
{
    using System;
    using System.Collections;

    internal interface IMethodResponseMessage
    {
        void SendException(System.Exception exception);
        void SendResponse(ICollection outArgs);

        System.Exception Exception { get; }

        ICollection OutArgs { get; }
    }
}

