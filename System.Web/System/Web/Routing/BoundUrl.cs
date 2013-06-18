namespace System.Web.Routing
{
    using System;
    using System.Runtime.CompilerServices;

    internal class BoundUrl
    {
        public string Url { get; set; }

        public RouteValueDictionary Values { get; set; }
    }
}

