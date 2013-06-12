namespace System.ComponentModel
{
    using System;

    public interface IIntellisenseBuilder
    {
        bool Show(string language, string value, ref string newValue);

        string Name { get; }
    }
}

