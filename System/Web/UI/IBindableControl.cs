namespace System.Web.UI
{
    using System;
    using System.Collections.Specialized;

    public interface IBindableControl
    {
        void ExtractValues(IOrderedDictionary dictionary);
    }
}

