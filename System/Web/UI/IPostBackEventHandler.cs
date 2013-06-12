namespace System.Web.UI
{
    using System;

    public interface IPostBackEventHandler
    {
        void RaisePostBackEvent(string eventArgument);
    }
}

