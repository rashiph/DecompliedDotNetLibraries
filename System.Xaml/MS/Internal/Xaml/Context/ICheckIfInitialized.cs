namespace MS.Internal.Xaml.Context
{
    using System;

    internal interface ICheckIfInitialized
    {
        bool IsFullyInitialized(object obj);
    }
}

