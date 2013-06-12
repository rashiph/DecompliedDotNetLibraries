namespace System.Web.UI
{
    using System;

    public interface ITemplate
    {
        void InstantiateIn(Control container);
    }
}

