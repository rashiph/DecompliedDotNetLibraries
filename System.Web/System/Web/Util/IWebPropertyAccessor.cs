namespace System.Web.Util
{
    using System;

    public interface IWebPropertyAccessor
    {
        object GetProperty(object target);
        void SetProperty(object target, object value);
    }
}

