namespace System.Web.UI
{
    using System;

    public interface IAttributeAccessor
    {
        string GetAttribute(string key);
        void SetAttribute(string key, string value);
    }
}

