namespace System.Configuration.Internal
{
    using System;

    public interface IConfigErrorInfo
    {
        string Filename { get; }

        int LineNumber { get; }
    }
}

