namespace System.Web.UI.HtmlControls
{
    using System;
    using System.Web.UI;

    public sealed class HtmlEmptyTagControlBuilder : ControlBuilder
    {
        public override bool HasBody()
        {
            return false;
        }
    }
}

