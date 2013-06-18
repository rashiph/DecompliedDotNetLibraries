namespace System.Web.UI
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.IO;

    public class XhtmlTextWriter : HtmlTextWriter
    {
        private Hashtable _commonAttributes;
        private XhtmlMobileDocType _docType;
        private Hashtable _elementSpecificAttributes;
        private Hashtable _suppressCommonAttributes;

        public XhtmlTextWriter(TextWriter writer) : this(writer, "\t")
        {
        }

        public XhtmlTextWriter(TextWriter writer, string tabString) : base(writer, tabString)
        {
            this._commonAttributes = new Hashtable();
            this._elementSpecificAttributes = new Hashtable(StringComparer.CurrentCultureIgnoreCase);
            this._suppressCommonAttributes = new Hashtable(StringComparer.CurrentCultureIgnoreCase);
            this._commonAttributes.Add("class", true);
            this._commonAttributes.Add("id", true);
            this._commonAttributes.Add("title", true);
            this._commonAttributes.Add("xml:lang", true);
            this.AddRecognizedAttributes("head", new string[] { "xml:lang" });
            this._suppressCommonAttributes["head"] = true;
            this.AddRecognizedAttributes("html", new string[] { "xml:lang", "version", "xmlns" });
            this._suppressCommonAttributes["html"] = true;
            this.AddRecognizedAttributes("title", new string[] { "xml:lang" });
            this._suppressCommonAttributes["title"] = true;
            this.AddRecognizedAttributes("blockquote", new string[] { "cite" });
            this.AddRecognizedAttributes("br", new string[] { "class", "id", "title" });
            this._suppressCommonAttributes["br"] = true;
            this.AddRecognizedAttributes("pre", new string[] { "xml:space" });
            this.AddRecognizedAttributes("q", new string[] { "cite" });
            this.AddRecognizedAttributes("a", new string[] { "accesskey", "charset", "href", "hreflang", "rel", "rev", "tabindex", "type", "title" });
            this.AddRecognizedAttributes("form", new string[] { "action", "method", "enctype" });
            this.AddRecognizedAttributes("input", new string[] { "accesskey", "checked", "maxlength", "name", "size", "src", "tabindex", "type", "value", "title", "disabled" });
            this.AddRecognizedAttributes("label", new string[] { "accesskey" });
            this.AddRecognizedAttributes("label", new string[] { "for" });
            this.AddRecognizedAttributes("select", new string[] { "multiple", "name", "size", "tabindex", "disabled" });
            this.AddRecognizedAttributes("option", new string[] { "selected", "value" });
            this.AddRecognizedAttributes("textarea", new string[] { "accesskey", "cols", "name", "rows", "tabindex" });
            this.AddRecognizedAttributes("table", new string[] { "summary", "width" });
            this.AddRecognizedAttributes("td", new string[] { "abbr", "align", "axis", "colspan", "headers", "rowspan", "scope", "valign" });
            this.AddRecognizedAttributes("th", new string[] { "abbr", "align", "axis", "colspan", "headers", "rowspan", "scope", "valign" });
            this.AddRecognizedAttributes("tr", new string[] { "align", "valign" });
            this.AddRecognizedAttributes("img", new string[] { "alt", "height", "longdesc", "src", "width" });
            this.AddRecognizedAttributes("object", new string[] { "archive", "classid", "codebase", "codetype", "data", "declare", "height", "name", "standby", "tabindex", "type", "width" });
            this.AddRecognizedAttributes("param", new string[] { "id", "name", "type", "value", "valuetype" });
            this.AddRecognizedAttributes("meta", new string[] { "xml:lang", "content", "http-equiv", "name", "scheme" });
            this._suppressCommonAttributes["meta"] = true;
            this.AddRecognizedAttributes("link", new string[] { "charset", "href", "hreflang", "media", "rel", "rev", "type" });
            this.AddRecognizedAttributes("base", new string[] { "href" });
            this._suppressCommonAttributes["base"] = true;
            this.AddRecognizedAttributes("optgroup", new string[] { "disabled", "label" });
            this.AddRecognizedAttributes("ol", new string[] { "start" });
            this.AddRecognizedAttributes("li", new string[] { "value" });
            this.AddRecognizedAttributes("style", new string[] { "xml:lang", "media", "title", "type", "xml:space" });
            this._suppressCommonAttributes["style"] = true;
        }

        public virtual void AddRecognizedAttribute(string elementName, string attributeName)
        {
            this.AddRecognizedAttributes(elementName, new string[] { attributeName });
        }

        private void AddRecognizedAttributes(string elementName, params string[] attributes)
        {
            Hashtable hashtable = (Hashtable) this._elementSpecificAttributes[elementName];
            if (hashtable == null)
            {
                hashtable = new Hashtable(StringComparer.CurrentCultureIgnoreCase);
                this._elementSpecificAttributes[elementName] = hashtable;
            }
            foreach (string str in attributes)
            {
                hashtable.Add(str, true);
            }
        }

        public override bool IsValidFormAttribute(string attributeName)
        {
            Hashtable hashtable = (Hashtable) this._elementSpecificAttributes["form"];
            return ((hashtable != null) && (hashtable[attributeName] != null));
        }

        protected override bool OnAttributeRender(string name, string value, HtmlTextWriterAttribute key)
        {
            return (((this._commonAttributes[name] != null) && (this._suppressCommonAttributes[base.TagName] == null)) || ((this._elementSpecificAttributes[base.TagName] != null) && (((Hashtable) this._elementSpecificAttributes[base.TagName])[name] != null)));
        }

        protected override bool OnStyleAttributeRender(string name, string value, HtmlTextWriterStyle key)
        {
            if (this._docType == XhtmlMobileDocType.XhtmlBasic)
            {
                return false;
            }
            if (base.TagName.ToLower(CultureInfo.InvariantCulture).Equals("div") && name.ToLower(CultureInfo.InvariantCulture).Equals("border-collapse"))
            {
                return false;
            }
            return true;
        }

        public virtual void RemoveRecognizedAttribute(string elementName, string attributeName)
        {
            Hashtable hashtable = (Hashtable) this._elementSpecificAttributes[elementName];
            if (hashtable == null)
            {
                hashtable = new Hashtable(StringComparer.CurrentCultureIgnoreCase);
                this._elementSpecificAttributes[elementName] = hashtable;
            }
            if ((this._commonAttributes[attributeName] == null) || (this._suppressCommonAttributes[elementName] != null))
            {
                hashtable.Remove(attributeName);
            }
            else
            {
                this._suppressCommonAttributes[elementName] = true;
                foreach (string str in this._commonAttributes.Keys)
                {
                    if (str != attributeName)
                    {
                        hashtable.Add(attributeName, true);
                    }
                }
            }
        }

        public virtual void SetDocType(XhtmlMobileDocType docType)
        {
            this._docType = docType;
            if ((docType != XhtmlMobileDocType.XhtmlBasic) && (this._commonAttributes["style"] == null))
            {
                this._commonAttributes.Add("style", true);
            }
        }

        public override void WriteBreak()
        {
            this.WriteFullBeginTag("br/");
        }

        protected Hashtable CommonAttributes
        {
            get
            {
                return this._commonAttributes;
            }
        }

        protected Hashtable ElementSpecificAttributes
        {
            get
            {
                return this._elementSpecificAttributes;
            }
        }

        internal override bool RenderDivAroundHiddenInputs
        {
            get
            {
                return false;
            }
        }

        protected Hashtable SuppressCommonAttributes
        {
            get
            {
                return this._suppressCommonAttributes;
            }
        }
    }
}

