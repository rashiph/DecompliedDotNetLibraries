namespace System.Web.UI
{
    using System;

    public interface ICallbackEventHandler
    {
        string GetCallbackResult();
        void RaiseCallbackEvent(string eventArgument);
    }
}

