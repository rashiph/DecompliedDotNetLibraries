namespace System.Web.UI
{
    using System;
    using System.Collections;

    internal interface ITagNameToTypeMapper
    {
        Type GetControlType(string tagName, IDictionary attribs);
    }
}

