namespace System.Web.UI.HtmlControls
{
    using System;
    using System.Collections;
    using System.Web.UI;
    using System.Web.UI.WebControls;
    using System.Web.Util;

    public class HtmlSelectBuilder : ControlBuilder
    {
        public override bool AllowWhitespaceLiterals()
        {
            return false;
        }

        public override Type GetChildControlType(string tagName, IDictionary attribs)
        {
            if (StringUtil.EqualsIgnoreCase(tagName, "option"))
            {
                return typeof(ListItem);
            }
            return null;
        }
    }
}

