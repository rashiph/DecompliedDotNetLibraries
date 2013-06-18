namespace System.Web
{
    using System;
    using System.Runtime.CompilerServices;

    public delegate IAsyncResult BeginEventHandler(object sender, EventArgs e, AsyncCallback cb, object extraData);
}

