namespace System.Web.UI
{
    using System;
    using System.Collections;
    using System.Web;

    internal sealed class StringPropertyBuilder : ControlBuilder
    {
        private string _text;

        internal StringPropertyBuilder()
        {
        }

        internal StringPropertyBuilder(string text)
        {
            this._text = text;
        }

        public override void AppendLiteralString(string s)
        {
            if ((base.ParentBuilder != null) && base.ParentBuilder.HtmlDecodeLiterals())
            {
                s = HttpUtility.HtmlDecode(s);
            }
            this._text = s;
        }

        public override void AppendSubBuilder(ControlBuilder subBuilder)
        {
            throw new HttpException(System.Web.SR.GetString("StringPropertyBuilder_CannotHaveChildObjects", new object[] { base.TagName, (base.ParentBuilder != null) ? base.ParentBuilder.TagName : string.Empty }));
        }

        public override object BuildObject()
        {
            return this.Text;
        }

        public override void Init(TemplateParser parser, ControlBuilder parentBuilder, Type type, string tagName, string ID, IDictionary attribs)
        {
            base.Init(parser, parentBuilder, type, tagName, ID, attribs);
            base.SetControlType(typeof(string));
        }

        public string Text
        {
            get
            {
                if (this._text != null)
                {
                    return this._text;
                }
                return string.Empty;
            }
        }
    }
}

