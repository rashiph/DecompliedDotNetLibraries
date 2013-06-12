namespace System.Web.UI
{
    using System;

    public interface IDataBindingsAccessor
    {
        DataBindingCollection DataBindings { get; }

        bool HasDataBindings { get; }
    }
}

