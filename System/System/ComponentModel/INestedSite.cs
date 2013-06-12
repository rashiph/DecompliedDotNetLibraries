namespace System.ComponentModel
{
    using System;

    public interface INestedSite : ISite, IServiceProvider
    {
        string FullName { get; }
    }
}

