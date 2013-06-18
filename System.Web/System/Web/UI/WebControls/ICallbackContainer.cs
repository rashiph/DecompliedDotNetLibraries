namespace System.Web.UI.WebControls
{
    using System;

    public interface ICallbackContainer
    {
        string GetCallbackScript(IButtonControl buttonControl, string argument);
    }
}

