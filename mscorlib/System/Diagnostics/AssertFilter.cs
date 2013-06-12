namespace System.Diagnostics
{
    using System;

    [Serializable]
    internal abstract class AssertFilter
    {
        protected AssertFilter()
        {
        }

        public abstract AssertFilters AssertFailure(string condition, string message, StackTrace location);
    }
}

