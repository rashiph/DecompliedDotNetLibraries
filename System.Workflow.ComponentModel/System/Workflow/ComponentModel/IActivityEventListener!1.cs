namespace System.Workflow.ComponentModel
{
    using System;

    public interface IActivityEventListener<T> where T: EventArgs
    {
        void OnEvent(object sender, T e);
    }
}

