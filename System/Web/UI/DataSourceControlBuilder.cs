namespace System.Web.UI
{
    using System;

    public sealed class DataSourceControlBuilder : ControlBuilder
    {
        public override bool AllowWhitespaceLiterals()
        {
            return false;
        }
    }
}

