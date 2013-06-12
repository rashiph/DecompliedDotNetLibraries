namespace System.Web.Configuration
{
    using System;

    internal class NullRuntimeConfig : RuntimeConfig
    {
        internal NullRuntimeConfig() : base(null, true)
        {
        }

        protected override object GetSectionObject(string sectionName)
        {
            return null;
        }
    }
}

