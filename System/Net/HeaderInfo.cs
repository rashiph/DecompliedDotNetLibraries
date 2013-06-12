namespace System.Net
{
    using System;

    internal class HeaderInfo
    {
        internal readonly bool AllowMultiValues;
        internal readonly string HeaderName;
        internal readonly bool IsRequestRestricted;
        internal readonly bool IsResponseRestricted;
        internal readonly HeaderParser Parser;

        internal HeaderInfo(string name, bool requestRestricted, bool responseRestricted, bool multi, HeaderParser p)
        {
            this.HeaderName = name;
            this.IsRequestRestricted = requestRestricted;
            this.IsResponseRestricted = responseRestricted;
            this.Parser = p;
            this.AllowMultiValues = multi;
        }
    }
}

