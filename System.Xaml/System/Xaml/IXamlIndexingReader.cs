namespace System.Xaml
{
    using System;

    public interface IXamlIndexingReader
    {
        int Count { get; }

        int CurrentIndex { get; set; }
    }
}

