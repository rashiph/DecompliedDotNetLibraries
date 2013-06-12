namespace System.Diagnostics
{
    using System;
    using System.Security;

    internal class DefaultFilter : AssertFilter
    {
        internal DefaultFilter()
        {
        }

        [SecuritySafeCritical]
        public override AssertFilters AssertFailure(string condition, string message, StackTrace location)
        {
            return (AssertFilters) Assert.ShowDefaultAssertDialog(condition, message, location.ToString());
        }
    }
}

