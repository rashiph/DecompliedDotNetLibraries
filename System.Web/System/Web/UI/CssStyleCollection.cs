namespace System.Web.UI
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.IO;
    using System.Reflection;
    using System.Text.RegularExpressions;

    public sealed class CssStyleCollection
    {
        private IDictionary _intTable;
        private StateBag _state;
        private string _style;
        private static readonly Regex _styleAttribRegex = new Regex(@"\G(\s*(;\s*)*(?<stylename>[^:]+?)\s*:\s*(?<styleval>[^;]*))*\s*(;\s*)*$", RegexOptions.Singleline | RegexOptions.ExplicitCapture | RegexOptions.Multiline);
        private IDictionary _table;

        internal CssStyleCollection() : this(null)
        {
        }

        internal CssStyleCollection(StateBag state)
        {
            this._state = state;
        }

        public void Add(string key, string value)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException("key");
            }
            if (this._table == null)
            {
                this.ParseString();
            }
            this._table[key] = value;
            if (this._intTable != null)
            {
                HtmlTextWriterStyle styleKey = CssTextWriter.GetStyleKey(key);
                if (styleKey != ~HtmlTextWriterStyle.BackgroundColor)
                {
                    this._intTable.Remove(styleKey);
                }
            }
            if (this._state != null)
            {
                this._state["style"] = this.BuildString();
            }
            this._style = null;
        }

        public void Add(HtmlTextWriterStyle key, string value)
        {
            if (this._intTable == null)
            {
                this._intTable = new HybridDictionary();
            }
            this._intTable[(int) key] = value;
            string styleName = CssTextWriter.GetStyleName(key);
            if (styleName.Length != 0)
            {
                if (this._table == null)
                {
                    this.ParseString();
                }
                this._table.Remove(styleName);
            }
            if (this._state != null)
            {
                this._state["style"] = this.BuildString();
            }
            this._style = null;
        }

        private string BuildString()
        {
            if (((this._table == null) || (this._table.Count == 0)) && ((this._intTable == null) || (this._intTable.Count == 0)))
            {
                return null;
            }
            StringWriter writer = new StringWriter();
            CssTextWriter writer2 = new CssTextWriter(writer);
            this.Render(writer2);
            return writer.ToString();
        }

        public void Clear()
        {
            this._table = null;
            this._intTable = null;
            if (this._state != null)
            {
                this._state.Remove("style");
            }
            this._style = null;
        }

        private void ParseString()
        {
            Match match;
            this._table = new HybridDictionary(true);
            string input = (this._state == null) ? this._style : ((string) this._state["style"]);
            if ((input != null) && (match = _styleAttribRegex.Match(input, 0)).Success)
            {
                CaptureCollection captures = match.Groups["stylename"].Captures;
                CaptureCollection captures2 = match.Groups["styleval"].Captures;
                for (int i = 0; i < captures.Count; i++)
                {
                    string str2 = captures[i].ToString();
                    string str3 = captures2[i].ToString();
                    this._table[str2] = str3;
                }
            }
        }

        public void Remove(string key)
        {
            if (this._table == null)
            {
                this.ParseString();
            }
            if (this._table[key] != null)
            {
                this._table.Remove(key);
                if (this._state != null)
                {
                    this._state["style"] = this.BuildString();
                }
                this._style = null;
            }
        }

        public void Remove(HtmlTextWriterStyle key)
        {
            if (this._intTable != null)
            {
                this._intTable.Remove((int) key);
                if (this._state != null)
                {
                    this._state["style"] = this.BuildString();
                }
                this._style = null;
            }
        }

        internal void Render(CssTextWriter writer)
        {
            if ((this._table != null) && (this._table.Count > 0))
            {
                foreach (DictionaryEntry entry in this._table)
                {
                    writer.WriteAttribute((string) entry.Key, (string) entry.Value);
                }
            }
            if ((this._intTable != null) && (this._intTable.Count > 0))
            {
                foreach (DictionaryEntry entry2 in this._intTable)
                {
                    writer.WriteAttribute((HtmlTextWriterStyle) entry2.Key, (string) entry2.Value);
                }
            }
        }

        internal void Render(HtmlTextWriter writer)
        {
            if ((this._table != null) && (this._table.Count > 0))
            {
                foreach (DictionaryEntry entry in this._table)
                {
                    writer.AddStyleAttribute((string) entry.Key, (string) entry.Value);
                }
            }
            if ((this._intTable != null) && (this._intTable.Count > 0))
            {
                foreach (DictionaryEntry entry2 in this._intTable)
                {
                    writer.AddStyleAttribute((HtmlTextWriterStyle) entry2.Key, (string) entry2.Value);
                }
            }
        }

        public int Count
        {
            get
            {
                if (this._table == null)
                {
                    this.ParseString();
                }
                return (this._table.Count + ((this._intTable != null) ? this._intTable.Count : 0));
            }
        }

        public string this[string key]
        {
            get
            {
                if (this._table == null)
                {
                    this.ParseString();
                }
                string str = (string) this._table[key];
                if (str == null)
                {
                    HtmlTextWriterStyle styleKey = CssTextWriter.GetStyleKey(key);
                    if (styleKey != ~HtmlTextWriterStyle.BackgroundColor)
                    {
                        str = this[styleKey];
                    }
                }
                return str;
            }
            set
            {
                this.Add(key, value);
            }
        }

        public string this[HtmlTextWriterStyle key]
        {
            get
            {
                if (this._intTable == null)
                {
                    return null;
                }
                return (string) this._intTable[(int) key];
            }
            set
            {
                this.Add(key, value);
            }
        }

        public ICollection Keys
        {
            get
            {
                if (this._table == null)
                {
                    this.ParseString();
                }
                if (this._intTable == null)
                {
                    return this._table.Keys;
                }
                string[] strArray = new string[this._table.Count + this._intTable.Count];
                int index = 0;
                foreach (string str in this._table.Keys)
                {
                    strArray[index] = str;
                    index++;
                }
                foreach (HtmlTextWriterStyle style in this._intTable.Keys)
                {
                    strArray[index] = CssTextWriter.GetStyleName(style);
                    index++;
                }
                return strArray;
            }
        }

        public string Value
        {
            get
            {
                if (this._state != null)
                {
                    return (string) this._state["style"];
                }
                if (this._style == null)
                {
                    this._style = this.BuildString();
                }
                return this._style;
            }
            set
            {
                if (this._state == null)
                {
                    this._style = value;
                }
                else
                {
                    this._state["style"] = value;
                }
                this._table = null;
            }
        }
    }
}

