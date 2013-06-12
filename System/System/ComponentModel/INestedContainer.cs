namespace System.ComponentModel
{
    using System;

    public interface INestedContainer : IContainer, IDisposable
    {
        IComponent Owner { get; }
    }
}

