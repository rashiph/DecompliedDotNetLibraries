namespace System.Web.Util
{
    using System;

    internal class CaseInsensitiveStringSet : StringSet
    {
        protected override bool CaseInsensitive
        {
            get
            {
                return true;
            }
        }
    }
}

