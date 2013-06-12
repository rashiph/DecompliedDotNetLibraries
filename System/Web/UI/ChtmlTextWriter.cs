namespace System.Web.UI
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Web.Util;

    public class ChtmlTextWriter : Html32TextWriter
    {
        private Hashtable _globalSuppressedAttributes;
        private Hashtable _recognizedAttributes;
        private Hashtable _suppressedAttributes;

        public ChtmlTextWriter(TextWriter writer) : this(writer, "\t")
        {
        }

        public ChtmlTextWriter(TextWriter writer, string tabString) : base(writer, tabString)
        {
            this._recognizedAttributes = new Hashtable(StringComparer.CurrentCultureIgnoreCase);
            this._suppressedAttributes = new Hashtable(StringComparer.CurrentCultureIgnoreCase);
            this._globalSuppressedAttributes = new Hashtable(StringComparer.CurrentCultureIgnoreCase);
            this._globalSuppressedAttributes["onclick"] = true;
            this._globalSuppressedAttributes["ondblclick"] = true;
            this._globalSuppressedAttributes["onmousedown"] = true;
            this._globalSuppressedAttributes["onmouseup"] = true;
            this._globalSuppressedAttributes["onmouseover"] = true;
            this._globalSuppressedAttributes["onmousemove"] = true;
            this._globalSuppressedAttributes["onmouseout"] = true;
            this._globalSuppressedAttributes["onkeypress"] = true;
            this._globalSuppressedAttributes["onkeydown"] = true;
            this._globalSuppressedAttributes["onkeyup"] = true;
            this.RemoveRecognizedAttributeInternal("div", "accesskey");
            this.RemoveRecognizedAttributeInternal("div", "cellspacing");
            this.RemoveRecognizedAttributeInternal("div", "cellpadding");
            this.RemoveRecognizedAttributeInternal("div", "gridlines");
            this.RemoveRecognizedAttributeInternal("div", "rules");
            this.RemoveRecognizedAttributeInternal("span", "cellspacing");
            this.RemoveRecognizedAttributeInternal("span", "cellpadding");
            this.RemoveRecognizedAttributeInternal("span", "gridlines");
            this.RemoveRecognizedAttributeInternal("span", "rules");
        }

        public virtual void AddRecognizedAttribute(string elementName, string attributeName)
        {
            Hashtable hashtable = (Hashtable) this._recognizedAttributes[elementName];
            if (hashtable == null)
            {
                hashtable = new Hashtable(StringComparer.CurrentCultureIgnoreCase);
                this._recognizedAttributes[elementName] = hashtable;
            }
            hashtable.Add(attributeName, true);
        }

        protected override bool OnAttributeRender(string name, string value, HtmlTextWriterAttribute key)
        {
            Hashtable hashtable = (Hashtable) this._recognizedAttributes[base.TagName];
            if ((hashtable == null) || (hashtable[name] == null))
            {
                if (this._globalSuppressedAttributes[name] != null)
                {
                    return false;
                }
                Hashtable hashtable2 = (Hashtable) this._suppressedAttributes[base.TagName];
                if ((hashtable2 != null) && (hashtable2[name] != null))
                {
                    return false;
                }
            }
            return true;
        }

        protected override bool OnStyleAttributeRender(string name, string value, HtmlTextWriterStyle key)
        {
            if ((key == HtmlTextWriterStyle.TextDecoration) && StringUtil.EqualsIgnoreCase("line-through", value))
            {
                return false;
            }
            return base.OnStyleAttributeRender(name, value, key);
        }

        protected override bool OnTagRender(string name, HtmlTextWriterTag key)
        {
            return (base.OnTagRender(name, key) && (key != HtmlTextWriterTag.Span));
        }

        public virtual void RemoveRecognizedAttribute(string elementName, string attributeName)
        {
            this.RemoveRecognizedAttributeInternal(elementName, attributeName);
        }

        private void RemoveRecognizedAttributeInternal(string elementName, string attributeName)
        {
            Hashtable hashtable = (Hashtable) this._suppressedAttributes[elementName];
            if (hashtable == null)
            {
                hashtable = new Hashtable(StringComparer.CurrentCultureIgnoreCase);
                this._suppressedAttributes[elementName] = hashtable;
            }
            hashtable.Add(attributeName, true);
            hashtable = (Hashtable) this._recognizedAttributes[elementName];
            if (hashtable == null)
            {
                hashtable = new Hashtable(StringComparer.CurrentCultureIgnoreCase);
                this._recognizedAttributes[elementName] = hashtable;
            }
            hashtable.Remove(attributeName);
        }

        public override void WriteBreak()
        {
            this.Write("<br>");
        }

        public override void WriteEncodedText(string text)
        {
            if ((text != null) && (text.Length != 0))
            {
                int length = text.Length;
                int startIndex = -1;
                for (int i = 0; i < length; i++)
                {
                    int num4 = text[i];
                    if ((num4 > 160) && (num4 < 0x100))
                    {
                        if (startIndex != -1)
                        {
                            base.WriteEncodedText(text.Substring(startIndex, i - startIndex));
                            startIndex = -1;
                        }
                        base.Write(text[i]);
                    }
                    else if (startIndex == -1)
                    {
                        startIndex = i;
                    }
                }
                switch (startIndex)
                {
                    case -1:
                        break;

                    case 0:
                        base.WriteEncodedText(text);
                        return;

                    default:
                        base.WriteEncodedText(text.Substring(startIndex, length - startIndex));
                        break;
                }
            }
        }

        protected Hashtable GlobalSuppressedAttributes
        {
            get
            {
                return this._globalSuppressedAttributes;
            }
        }

        protected Hashtable RecognizedAttributes
        {
            get
            {
                return this._recognizedAttributes;
            }
        }

        protected Hashtable SuppressedAttributes
        {
            get
            {
                return this._suppressedAttributes;
            }
        }
    }
}

