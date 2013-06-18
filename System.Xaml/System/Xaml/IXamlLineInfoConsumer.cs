namespace System.Xaml
{
    using System;

    public interface IXamlLineInfoConsumer
    {
        void SetLineInfo(int lineNumber, int linePosition);

        bool ShouldProvideLineInfo { get; }
    }
}

