namespace System.Web.UI
{
    using System.Collections.Specialized;

    public interface IBindableTemplate : ITemplate
    {
        IOrderedDictionary ExtractValues(Control container);
    }
}

