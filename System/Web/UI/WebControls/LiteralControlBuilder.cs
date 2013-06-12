namespace System.Web.UI.WebControls
{
    using System;
    using System.Web;
    using System.Web.UI;

    public class LiteralControlBuilder : ControlBuilder
    {
        public override bool AllowWhitespaceLiterals()
        {
            return false;
        }

        public override void AppendLiteralString(string s)
        {
            if (Util.IsWhiteSpaceString(s))
            {
                base.AppendLiteralString(s);
            }
            else
            {
                base.PreprocessAttribute(string.Empty, "text", s, false);
            }
        }

        public override void AppendSubBuilder(ControlBuilder subBuilder)
        {
            throw new HttpException(System.Web.SR.GetString("Control_does_not_allow_children", new object[] { base.ControlType.ToString() }));
        }
    }
}

