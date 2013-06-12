namespace System.Web.Configuration
{
    using System;
    using System.Collections.Specialized;

    internal class BrowserTree : OrderedDictionary
    {
        internal BrowserTree() : base(StringComparer.OrdinalIgnoreCase)
        {
        }
    }
}

