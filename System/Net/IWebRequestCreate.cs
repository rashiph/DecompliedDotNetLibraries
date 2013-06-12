namespace System.Net
{
    using System;

    public interface IWebRequestCreate
    {
        WebRequest Create(Uri uri);
    }
}

