namespace Microsoft.JScript
{
    using System;
    using System.Runtime.InteropServices;

    [Guid("5B807FA1-00CD-46ee-A493-FD80AC944715"), ComVisible(true)]
    public interface IRedirectOutput
    {
        void SetOutputStream(IMessageReceiver output);
    }
}

