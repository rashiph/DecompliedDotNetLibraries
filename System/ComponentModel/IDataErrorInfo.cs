namespace System.ComponentModel
{
    using System;
    using System.Reflection;

    public interface IDataErrorInfo
    {
        string Error { get; }

        string this[string columnName] { get; }
    }
}

