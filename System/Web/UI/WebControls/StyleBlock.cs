namespace System.Web.UI.WebControls
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Web.UI;

    internal class StyleBlock : Control
    {
        private List<StyleBlockStyles> _styles = new List<StyleBlockStyles>();

        public StyleBlockStyles AddStyleDefinition(string selector)
        {
            StyleBlockStyles item = new StyleBlockStyles(selector, this);
            this._styles.Add(item);
            return item;
        }

        public StyleBlockStyles AddStyleDefinition(string selectorFormat, params object[] args)
        {
            return this.AddStyleDefinition(string.Format(CultureInfo.InvariantCulture, selectorFormat, args));
        }

        protected internal override void Render(HtmlTextWriter writer)
        {
            if (this._styles.Any<StyleBlockStyles>(s => !s.Empty))
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Type, "text/css");
                writer.RenderBeginTag(HtmlTextWriterTag.Style);
                writer.WriteLine("/* <![CDATA[ */");
                foreach (StyleBlockStyles styles in from s in this._styles
                    where !s.Empty
                    select s)
                {
                    styles.Render(writer);
                }
                writer.Write("/* ]]> */");
                writer.RenderEndTag();
            }
        }
    }
}

