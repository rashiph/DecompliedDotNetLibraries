namespace System.Web.UI.Adapters
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    public abstract class PageAdapter : ControlAdapter
    {
        private IDictionary _radioButtonGroups;

        protected PageAdapter()
        {
        }

        public virtual NameValueCollection DeterminePostBackMode()
        {
            if (base.Control != null)
            {
                return base.Control.Page.DeterminePostBackMode();
            }
            return null;
        }

        protected internal virtual string GetPostBackFormReference(string formId)
        {
            return ("document.forms['" + formId + "']");
        }

        public virtual ICollection GetRadioButtonsByGroup(string groupName)
        {
            if (this._radioButtonGroups == null)
            {
                return null;
            }
            return (ICollection) this._radioButtonGroups[groupName];
        }

        public virtual PageStatePersister GetStatePersister()
        {
            return new HiddenFieldPageStatePersister(base.Page);
        }

        public virtual void RegisterRadioButton(RadioButton radioButton)
        {
            string groupName = radioButton.GroupName;
            if (!string.IsNullOrEmpty(groupName))
            {
                ArrayList list = null;
                if (this._radioButtonGroups == null)
                {
                    this._radioButtonGroups = new ListDictionary();
                }
                if (this._radioButtonGroups.Contains(groupName))
                {
                    list = (ArrayList) this._radioButtonGroups[groupName];
                }
                else
                {
                    list = new ArrayList();
                    this._radioButtonGroups[groupName] = list;
                }
                list.Add(radioButton);
            }
        }

        public virtual void RenderBeginHyperlink(HtmlTextWriter writer, string targetUrl, bool encodeUrl, string softkeyLabel)
        {
            this.RenderBeginHyperlink(writer, targetUrl, encodeUrl, softkeyLabel, null);
        }

        public virtual void RenderBeginHyperlink(HtmlTextWriter writer, string targetUrl, bool encodeUrl, string softkeyLabel, string accessKey)
        {
            string str;
            if ((accessKey != null) && (accessKey.Length > 1))
            {
                throw new ArgumentOutOfRangeException("accessKey");
            }
            if (encodeUrl)
            {
                str = HttpUtility.HtmlAttributeEncode(targetUrl);
            }
            else
            {
                str = targetUrl;
            }
            writer.AddAttribute("href", str);
            if (!string.IsNullOrEmpty(accessKey))
            {
                writer.AddAttribute("accessKey", accessKey);
            }
            writer.RenderBeginTag("a");
        }

        public virtual void RenderEndHyperlink(HtmlTextWriter writer)
        {
            writer.WriteEndTag("a");
        }

        public virtual void RenderPostBackEvent(HtmlTextWriter writer, string target, string argument, string softkeyLabel, string text)
        {
            this.RenderPostBackEvent(writer, target, argument, softkeyLabel, text, null, null);
        }

        public virtual void RenderPostBackEvent(HtmlTextWriter writer, string target, string argument, string softkeyLabel, string text, string postUrl, string accessKey)
        {
            this.RenderPostBackEvent(writer, target, argument, softkeyLabel, text, postUrl, accessKey, false);
        }

        protected void RenderPostBackEvent(HtmlTextWriter writer, string target, string argument, string softkeyLabel, string text, string postUrl, string accessKey, bool encode)
        {
            string str = encode ? "&amp;" : "&";
            bool flag = !string.IsNullOrEmpty(postUrl);
            writer.WriteBeginTag("a");
            writer.Write(" href=\"");
            string url = null;
            if (!flag)
            {
                if (base.Browser["requiresAbsolutePostbackUrl"] == "true")
                {
                    url = base.Page.Response.ApplyAppPathModifier(base.Page.Request.CurrentExecutionFilePath);
                }
                else
                {
                    url = base.Page.RelativeFilePath;
                }
            }
            else
            {
                url = postUrl;
                base.Page.ContainsCrossPagePost = true;
            }
            writer.WriteEncodedUrl(url);
            writer.Write("?");
            if (this.ClientState != null)
            {
                ICollection is2 = base.Page.DecomposeViewStateIntoChunks();
                if (is2.Count > 1)
                {
                    writer.Write("__VIEWSTATEFIELDCOUNT=" + is2.Count + str);
                }
                int num = 0;
                foreach (string str4 in is2)
                {
                    writer.Write("__VIEWSTATE");
                    if (num > 0)
                    {
                        writer.Write(num.ToString(CultureInfo.CurrentCulture));
                    }
                    writer.Write("=" + HttpUtility.UrlEncode(str4));
                    writer.Write(str);
                    num++;
                }
            }
            if (flag)
            {
                writer.Write("__PREVIOUSPAGE");
                writer.Write("=" + Page.EncryptString(base.Page.Request.CurrentExecutionFilePath));
                writer.Write(str);
            }
            writer.Write("__EVENTTARGET=" + HttpUtility.UrlEncode(target));
            writer.Write(str);
            writer.Write("__EVENTARGUMENT=" + HttpUtility.UrlEncode(argument));
            string queryString = this.QueryString;
            if (!string.IsNullOrEmpty(queryString))
            {
                writer.Write(str);
                writer.Write(queryString);
            }
            writer.Write("\"");
            if (!string.IsNullOrEmpty(accessKey))
            {
                writer.WriteAttribute("accessKey", accessKey);
            }
            writer.Write(">");
            writer.Write(text);
            writer.WriteEndTag("a");
        }

        public virtual string TransformText(string text)
        {
            return text;
        }

        public virtual StringCollection CacheVaryByHeaders
        {
            get
            {
                return null;
            }
        }

        public virtual StringCollection CacheVaryByParams
        {
            get
            {
                return null;
            }
        }

        protected string ClientState
        {
            get
            {
                if (base.Page != null)
                {
                    return base.Page.ClientState;
                }
                return null;
            }
        }

        internal virtual char IdSeparator
        {
            get
            {
                return '$';
            }
        }

        internal string QueryString
        {
            get
            {
                string clientQueryString = base.Page.ClientQueryString;
                if (!base.Page.Request.Browser.RequiresUniqueFilePathSuffix)
                {
                    return clientQueryString;
                }
                if (!string.IsNullOrEmpty(clientQueryString))
                {
                    clientQueryString = clientQueryString + "&";
                }
                return (clientQueryString + base.Page.UniqueFilePathSuffix);
            }
        }
    }
}

