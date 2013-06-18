namespace System.Web.UI.HtmlControls
{
    using System;
    using System.Collections;
    using System.Web.UI;

    public class HtmlHeadBuilder : ControlBuilder
    {
        public override bool AllowWhitespaceLiterals()
        {
            return false;
        }

        public override Type GetChildControlType(string tagName, IDictionary attribs)
        {
            if (string.Equals(tagName, "title", StringComparison.OrdinalIgnoreCase))
            {
                return typeof(HtmlTitle);
            }
            if (string.Equals(tagName, "link", StringComparison.OrdinalIgnoreCase))
            {
                return typeof(HtmlLink);
            }
            if (string.Equals(tagName, "meta", StringComparison.OrdinalIgnoreCase))
            {
                return typeof(HtmlMeta);
            }
            return null;
        }
    }
}

