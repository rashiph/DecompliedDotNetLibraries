namespace System.Web.UI
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Globalization;
    using System.Web;
    using System.Web.UI.HtmlControls;
    using System.Web.Util;
    using System.Xml;

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public abstract class PageTheme
    {
        private System.Web.UI.Page _page;
        private bool _styleSheetTheme;

        protected PageTheme()
        {
        }

        internal void ApplyControlSkin(Control control)
        {
            if (control == null)
            {
                throw new ArgumentNullException("control");
            }
            ControlSkin skin = null;
            string skinID = control.SkinID;
            skin = (ControlSkin) this.ControlSkins[CreateSkinKey(control.GetType(), skinID)];
            if (skin != null)
            {
                skin.ApplySkin(control);
            }
        }

        public static object CreateSkinKey(Type controlType, string skinID)
        {
            if (controlType == null)
            {
                throw new ArgumentNullException("controlType");
            }
            return new SkinKey(controlType.ToString(), skinID);
        }

        protected object Eval(string expression)
        {
            return this.Page.Eval(expression);
        }

        protected string Eval(string expression, string format)
        {
            return this.Page.Eval(expression, format);
        }

        internal void Initialize(System.Web.UI.Page page, bool styleSheetTheme)
        {
            this._page = page;
            this._styleSheetTheme = styleSheetTheme;
        }

        internal void SetStyleSheet()
        {
            if ((this.LinkedStyleSheets != null) && (this.LinkedStyleSheets.Length > 0))
            {
                if (this.Page.Header == null)
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("Page_theme_requires_page_header"));
                }
                int num = 0;
                foreach (string str in this.LinkedStyleSheets)
                {
                    HtmlLink child = new HtmlLink {
                        Href = str
                    };
                    child.Attributes["type"] = "text/css";
                    child.Attributes["rel"] = "stylesheet";
                    if (this._styleSheetTheme)
                    {
                        this.Page.Header.Controls.AddAt(num++, child);
                    }
                    else
                    {
                        this.Page.Header.Controls.Add(child);
                    }
                }
            }
        }

        public bool TestDeviceFilter(string deviceFilterName)
        {
            return this.Page.TestDeviceFilter(deviceFilterName);
        }

        protected object XPath(string xPathExpression)
        {
            return this.Page.XPath(xPathExpression);
        }

        protected string XPath(string xPathExpression, string format)
        {
            return this.Page.XPath(xPathExpression, format);
        }

        protected object XPath(string xPathExpression, IXmlNamespaceResolver resolver)
        {
            return this.Page.XPath(xPathExpression, resolver);
        }

        protected string XPath(string xPathExpression, string format, IXmlNamespaceResolver resolver)
        {
            return this.Page.XPath(xPathExpression, format, resolver);
        }

        protected IEnumerable XPathSelect(string xPathExpression)
        {
            return this.Page.XPathSelect(xPathExpression);
        }

        protected IEnumerable XPathSelect(string xPathExpression, IXmlNamespaceResolver resolver)
        {
            return this.Page.XPathSelect(xPathExpression, resolver);
        }

        protected abstract string AppRelativeTemplateSourceDirectory { get; }

        protected abstract IDictionary ControlSkins { get; }

        protected abstract string[] LinkedStyleSheets { get; }

        protected System.Web.UI.Page Page
        {
            get
            {
                return this._page;
            }
        }

        private class SkinKey
        {
            private string _skinID;
            private string _typeName;

            internal SkinKey(string typeName, string skinID)
            {
                this._typeName = typeName;
                if (string.IsNullOrEmpty(skinID))
                {
                    this._skinID = null;
                }
                else
                {
                    this._skinID = skinID.ToLower(CultureInfo.InvariantCulture);
                }
            }

            public override bool Equals(object o)
            {
                PageTheme.SkinKey key = (PageTheme.SkinKey) o;
                return ((this._typeName == key._typeName) && (this._skinID == key._skinID));
            }

            public override int GetHashCode()
            {
                if (this._skinID == null)
                {
                    return this._typeName.GetHashCode();
                }
                return HashCodeCombiner.CombineHashCodes(this._typeName.GetHashCode(), this._skinID.GetHashCode());
            }
        }
    }
}

