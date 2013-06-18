namespace System.Web.UI.WebControls
{
    using System;
    using System.Web.UI;

    internal class StyleBlockStyles
    {
        private string _selector;
        private StyleBlock _styleControl;
        private CssStyleCollection _styles = new CssStyleCollection();

        public StyleBlockStyles(string selector, StyleBlock styleControl)
        {
            this._selector = selector;
            this._styleControl = styleControl;
        }

        public StyleBlockStyles AddStyle(string styleName, string value)
        {
            this._styles.Add(styleName, value);
            return this;
        }

        public StyleBlockStyles AddStyle(HtmlTextWriterStyle styleName, string value)
        {
            this._styles.Add(styleName, value);
            return this;
        }

        public StyleBlockStyles AddStyles(CssStyleCollection styles)
        {
            if (styles != null)
            {
                foreach (string str in styles.Keys)
                {
                    this._styles.Add(str, styles[str]);
                }
            }
            return this;
        }

        public StyleBlockStyles AddStyles(Style style)
        {
            if (style != null)
            {
                this.AddStyles(style.GetStyleAttributes(this._styleControl));
            }
            return this;
        }

        public void Render(HtmlTextWriter writer)
        {
            writer.WriteLine("{0} {{ {1} }}", this._selector, this._styles.Value);
        }

        public bool Empty
        {
            get
            {
                return (this._styles.Count == 0);
            }
        }
    }
}

