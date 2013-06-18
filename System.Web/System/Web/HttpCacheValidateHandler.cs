namespace System.Web
{
    using System;
    using System.Runtime.CompilerServices;

    public delegate void HttpCacheValidateHandler(HttpContext context, object data, ref HttpValidationStatus validationStatus);
}

