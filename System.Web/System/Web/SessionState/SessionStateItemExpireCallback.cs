namespace System.Web.SessionState
{
    using System;
    using System.Runtime.CompilerServices;

    public delegate void SessionStateItemExpireCallback(string id, SessionStateStoreData item);
}

