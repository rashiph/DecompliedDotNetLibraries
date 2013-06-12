namespace System.Web.Util
{
    using System;

    public enum RequestValidationSource
    {
        QueryString,
        Form,
        Cookies,
        Files,
        RawUrl,
        Path,
        PathInfo,
        Headers
    }
}

