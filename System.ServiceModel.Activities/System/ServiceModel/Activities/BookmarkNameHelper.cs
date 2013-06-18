namespace System.ServiceModel.Activities
{
    using System;
    using System.Globalization;
    using System.Xml.Linq;

    internal static class BookmarkNameHelper
    {
        public static string CreateBookmarkName(string operationName, XName serviceContractName)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}|{1}", new object[] { operationName, serviceContractName });
        }
    }
}

