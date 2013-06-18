namespace System.Web
{
    using System;

    internal sealed class ValidationCallbackInfo
    {
        internal readonly object data;
        internal readonly HttpCacheValidateHandler handler;

        internal ValidationCallbackInfo(HttpCacheValidateHandler handler, object data)
        {
            this.handler = handler;
            this.data = data;
        }
    }
}

