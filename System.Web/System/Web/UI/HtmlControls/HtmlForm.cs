namespace System.Web.UI.HtmlControls
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Globalization;
    using System.Text;
    using System.Web;
    using System.Web.Configuration;
    using System.Web.Security;
    using System.Web.UI;
    using System.Web.UI.WebControls;
    using System.Web.Util;

    public class HtmlForm : HtmlContainerControl
    {
        private const string _aspnetFormID = "aspnetForm";
        private string _defaultButton;
        private string _defaultFocus;
        private bool _submitDisabledControls;

        public HtmlForm() : base("form")
        {
        }

        protected override ControlCollection CreateControlCollection()
        {
            return new ControlCollection(this, 100, 2);
        }

        private string GetActionAttribute()
        {
            string virtualPathString;
            string action = this.Action;
            if (!string.IsNullOrEmpty(action))
            {
                return action;
            }
            VirtualPath clientFilePath = this.Context.Request.ClientFilePath;
            if (this.Context.ServerExecuteDepth == 0)
            {
                virtualPathString = clientFilePath.VirtualPathString;
                int num = virtualPathString.LastIndexOf('/');
                if (num >= 0)
                {
                    virtualPathString = virtualPathString.Substring(num + 1);
                }
            }
            else
            {
                VirtualPath currentExecutionFilePathObject = this.Context.Request.CurrentExecutionFilePathObject;
                virtualPathString = clientFilePath.MakeRelative(currentExecutionFilePathObject).VirtualPathString;
            }
            if ((CookielessHelperClass.UseCookieless(this.Context, false, FormsAuthentication.CookieMode) && (this.Context.Request != null)) && (this.Context.Response != null))
            {
                virtualPathString = this.Context.Response.ApplyAppPathModifier(virtualPathString);
            }
            string clientQueryString = this.Page.ClientQueryString;
            if (!string.IsNullOrEmpty(clientQueryString))
            {
                virtualPathString = virtualPathString + "?" + clientQueryString;
            }
            return virtualPathString;
        }

        protected internal override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            this.Page.SetForm(this);
            this.Page.RegisterViewStateHandler();
        }

        protected internal override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            if (this.Page.SmartNavigation)
            {
                this.Page.ClientScript.RegisterClientScriptResource(typeof(HtmlForm), "SmartNav.js");
            }
        }

        protected internal override void Render(HtmlTextWriter output)
        {
            Page page = this.Page;
            if (page == null)
            {
                throw new HttpException(System.Web.SR.GetString("Form_Needs_Page"));
            }
            if (page.SmartNavigation)
            {
                ((IAttributeAccessor) this).SetAttribute("__smartNavEnabled", "true");
                StringBuilder builder = new StringBuilder("<IFRAME id=\"__hifSmartNav\" name=\"__hifSmartNav\" style=\"display:none\" src=\"");
                builder.Append(HttpEncoderUtility.UrlEncodeSpaces(HttpUtility.HtmlAttributeEncode(this.Page.ClientScript.GetWebResourceUrl(typeof(HtmlForm), "SmartNav.htm"))));
                builder.Append("\"></IFRAME>");
                output.WriteLine(builder.ToString());
            }
            base.Render(output);
        }

        protected override void RenderAttributes(HtmlTextWriter writer)
        {
            ArrayList list = new ArrayList();
            foreach (string str in base.Attributes.Keys)
            {
                if (!writer.IsValidFormAttribute(str))
                {
                    list.Add(str);
                }
            }
            foreach (string str2 in list)
            {
                base.Attributes.Remove(str2);
            }
            bool enableLegacyRendering = base.EnableLegacyRendering;
            Page page = this.Page;
            if (writer.IsValidFormAttribute("name"))
            {
                if ((((page != null) && (page.RequestInternal != null)) && (this.RenderingCompatibility < VersionUtil.Framework40)) && ((page.RequestInternal.Browser.W3CDomVersion.Major == 0) || (page.XhtmlConformanceMode != XhtmlConformanceMode.Strict)))
                {
                    writer.WriteAttribute("name", this.Name);
                }
                base.Attributes.Remove("name");
            }
            writer.WriteAttribute("method", this.Method);
            base.Attributes.Remove("method");
            writer.WriteAttribute("action", this.GetActionAttribute(), true);
            base.Attributes.Remove("action");
            if (page != null)
            {
                string clientOnSubmitEvent = page.ClientOnSubmitEvent;
                if (!string.IsNullOrEmpty(clientOnSubmitEvent))
                {
                    if (base.Attributes["onsubmit"] != null)
                    {
                        string s = base.Attributes["onsubmit"];
                        if (s.Length > 0)
                        {
                            if (!StringUtil.StringEndsWith(s, ';'))
                            {
                                s = s + ";";
                            }
                            if (page.ClientSupportsJavaScript || !s.ToLower(CultureInfo.CurrentCulture).Contains("javascript"))
                            {
                                page.ClientScript.RegisterOnSubmitStatement(typeof(HtmlForm), "OnSubmitScript", s);
                            }
                            base.Attributes.Remove("onsubmit");
                        }
                    }
                    if (page.ClientSupportsJavaScript || !clientOnSubmitEvent.ToLower(CultureInfo.CurrentCulture).Contains("javascript"))
                    {
                        if (enableLegacyRendering)
                        {
                            writer.WriteAttribute("language", "javascript", false);
                        }
                        writer.WriteAttribute("onsubmit", clientOnSubmitEvent);
                    }
                }
                if (((page.RequestInternal != null) && (page.RequestInternal.Browser.EcmaScriptVersion.Major > 0)) && ((page.RequestInternal.Browser.W3CDomVersion.Major > 0) && (this.DefaultButton.Length > 0)))
                {
                    Control button = this.FindControl(this.DefaultButton);
                    if ((button == null) && (this.Page != null))
                    {
                        char[] anyOf = new char[] { '$', ':' };
                        if (this.DefaultButton.IndexOfAny(anyOf) != -1)
                        {
                            button = this.Page.FindControl(this.DefaultButton);
                        }
                    }
                    if (!(button is IButtonControl))
                    {
                        throw new InvalidOperationException(System.Web.SR.GetString("HtmlForm_OnlyIButtonControlCanBeDefaultButton", new object[] { this.ID }));
                    }
                    page.ClientScript.RegisterDefaultButtonScript(button, writer, false);
                }
            }
            base.EnsureID();
            base.RenderAttributes(writer);
        }

        protected internal override void RenderChildren(HtmlTextWriter writer)
        {
            Page page = this.Page;
            if (page != null)
            {
                page.OnFormRender();
                page.BeginFormRender(writer, this.UniqueID);
            }
            HttpWriter innerWriter = writer.InnerWriter as HttpWriter;
            if (((page != null) && (innerWriter != null)) && RuntimeConfig.GetConfig(this.Context).Pages.RenderAllHiddenFieldsAtTopOfForm)
            {
                innerWriter.HasBeenClearedRecently = false;
                int responseBufferCountAfterFlush = innerWriter.GetResponseBufferCountAfterFlush();
                base.RenderChildren(writer);
                int srcIndex = innerWriter.GetResponseBufferCountAfterFlush();
                page.EndFormRenderHiddenFields(writer, this.UniqueID);
                if (!innerWriter.HasBeenClearedRecently)
                {
                    int num3 = innerWriter.GetResponseBufferCountAfterFlush();
                    innerWriter.MoveResponseBufferRangeForward(srcIndex, num3 - srcIndex, responseBufferCountAfterFlush);
                }
                page.EndFormRenderArrayAndExpandoAttribute(writer, this.UniqueID);
                page.EndFormRenderPostBackAndWebFormsScript(writer, this.UniqueID);
                page.OnFormPostRender(writer);
            }
            else
            {
                base.RenderChildren(writer);
                if (page != null)
                {
                    page.EndFormRender(writer, this.UniqueID);
                    page.OnFormPostRender(writer);
                }
            }
        }

        public override void RenderControl(HtmlTextWriter writer)
        {
            if (base.DesignMode)
            {
                base.RenderChildren(writer);
            }
            else
            {
                base.RenderControl(writer);
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), DefaultValue(""), WebCategory("Behavior")]
        public string Action
        {
            get
            {
                string str = base.Attributes["action"];
                if (str == null)
                {
                    return string.Empty;
                }
                return str;
            }
            set
            {
                base.Attributes["action"] = HtmlControl.MapStringAttributeToString(value);
            }
        }

        public override string ClientID
        {
            get
            {
                if (this.EffectiveClientIDMode != ClientIDMode.AutoID)
                {
                    return this.ID;
                }
                return base.ClientID;
            }
        }

        [DefaultValue(""), WebCategory("Behavior")]
        public string DefaultButton
        {
            get
            {
                if (this._defaultButton == null)
                {
                    return string.Empty;
                }
                return this._defaultButton;
            }
            set
            {
                this._defaultButton = value;
            }
        }

        [WebCategory("Behavior"), DefaultValue("")]
        public string DefaultFocus
        {
            get
            {
                if (this._defaultFocus == null)
                {
                    return string.Empty;
                }
                return this._defaultFocus;
            }
            set
            {
                this._defaultFocus = value;
            }
        }

        [WebCategory("Behavior"), DefaultValue(""), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Enctype
        {
            get
            {
                string str = base.Attributes["enctype"];
                if (str == null)
                {
                    return string.Empty;
                }
                return str;
            }
            set
            {
                base.Attributes["enctype"] = HtmlControl.MapStringAttributeToString(value);
            }
        }

        [DefaultValue(""), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), WebCategory("Behavior")]
        public string Method
        {
            get
            {
                string str = base.Attributes["method"];
                if (str == null)
                {
                    return "post";
                }
                return str;
            }
            set
            {
                base.Attributes["method"] = HtmlControl.MapStringAttributeToString(value);
            }
        }

        [WebCategory("Appearance"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), DefaultValue("")]
        public virtual string Name
        {
            get
            {
                return this.UniqueID;
            }
            set
            {
            }
        }

        [WebCategory("Behavior"), DefaultValue(false)]
        public virtual bool SubmitDisabledControls
        {
            get
            {
                return this._submitDisabledControls;
            }
            set
            {
                this._submitDisabledControls = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), WebCategory("Behavior"), DefaultValue("")]
        public string Target
        {
            get
            {
                string str = base.Attributes["target"];
                if (str == null)
                {
                    return string.Empty;
                }
                return str;
            }
            set
            {
                base.Attributes["target"] = HtmlControl.MapStringAttributeToString(value);
            }
        }

        public override string UniqueID
        {
            get
            {
                if (this.NamingContainer == this.Page)
                {
                    return base.UniqueID;
                }
                if (this.EffectiveClientIDMode == ClientIDMode.AutoID)
                {
                    return "aspnetForm";
                }
                return (this.ID ?? "aspnetForm");
            }
        }
    }
}

