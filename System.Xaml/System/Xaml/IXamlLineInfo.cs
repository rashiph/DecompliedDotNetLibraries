namespace System.Xaml
{
    using System;

    public interface IXamlLineInfo
    {
        bool HasLineInfo { get; }

        int LineNumber { get; }

        int LinePosition { get; }
    }
}

