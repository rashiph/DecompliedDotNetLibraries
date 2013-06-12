namespace System.Web
{
    using System;

    internal class ResponseDependencyInfo
    {
        internal readonly string[] items;
        internal readonly DateTime utcDate;

        internal ResponseDependencyInfo(string[] items, DateTime utcDate)
        {
            this.items = items;
            this.utcDate = utcDate;
        }
    }
}

