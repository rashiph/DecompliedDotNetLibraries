namespace System.Web.UI.WebControls
{
    using System;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Globalization;
    using System.Web;
    using System.Web.UI;

    [Designer("System.Web.UI.Design.WebControls.CheckBoxDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), SupportsEventValidation]
    public class RadioButton : CheckBox, IPostBackDataHandler
    {
        private string _uniqueGroupName;

        protected override bool LoadPostData(string postDataKey, NameValueCollection postCollection)
        {
            string eventArgument = postCollection[this.UniqueGroupName];
            bool flag = false;
            if ((eventArgument != null) && eventArgument.Equals(this.ValueAttribute))
            {
                base.ValidateEvent(this.UniqueGroupName, eventArgument);
                if (!this.Checked)
                {
                    this.Checked = true;
                    flag = true;
                }
                return flag;
            }
            if (this.Checked)
            {
                this.Checked = false;
            }
            return flag;
        }

        protected internal override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            if (((this.Page != null) && !this.Checked) && this.Enabled)
            {
                this.Page.RegisterRequiresPostBack(this);
            }
        }

        protected override void RaisePostDataChangedEvent()
        {
            if (this.AutoPostBack && !this.Page.IsPostBackEventControlRegistered)
            {
                this.Page.AutoPostBackControl = this;
                if (this.CausesValidation)
                {
                    this.Page.Validate(this.ValidationGroup);
                }
            }
            this.OnCheckedChanged(EventArgs.Empty);
        }

        internal override void RenderInputTag(HtmlTextWriter writer, string clientID, string onClick)
        {
            writer.AddAttribute(HtmlTextWriterAttribute.Id, clientID);
            writer.AddAttribute(HtmlTextWriterAttribute.Type, "radio");
            writer.AddAttribute(HtmlTextWriterAttribute.Name, this.UniqueGroupName);
            writer.AddAttribute(HtmlTextWriterAttribute.Value, this.ValueAttribute);
            if (this.Page != null)
            {
                this.Page.ClientScript.RegisterForEventValidation(this.UniqueGroupName, this.ValueAttribute);
            }
            if (this.Checked)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Checked, "checked");
            }
            if (!base.IsEnabled && this.SupportsDisabledAttribute)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Disabled, "disabled");
            }
            if ((this.AutoPostBack && !this.Checked) && (this.Page != null))
            {
                PostBackOptions options = new PostBackOptions(this, string.Empty);
                if (this.CausesValidation)
                {
                    options.PerformValidation = true;
                    options.ValidationGroup = this.ValidationGroup;
                }
                if (this.Page.Form != null)
                {
                    options.AutoPostBack = true;
                }
                onClick = Util.MergeScript(onClick, this.Page.ClientScript.GetPostBackEventReference(options));
                writer.AddAttribute(HtmlTextWriterAttribute.Onclick, onClick);
                if (base.EnableLegacyRendering)
                {
                    writer.AddAttribute("language", "javascript", false);
                }
            }
            else if (onClick != null)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Onclick, onClick);
            }
            string accessKey = this.AccessKey;
            if (accessKey.Length > 0)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Accesskey, accessKey);
            }
            int tabIndex = this.TabIndex;
            if (tabIndex != 0)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Tabindex, tabIndex.ToString(NumberFormatInfo.InvariantInfo));
            }
            if ((base._inputAttributes != null) && (base._inputAttributes.Count != 0))
            {
                base._inputAttributes.AddAttributes(writer);
            }
            writer.RenderBeginTag(HtmlTextWriterTag.Input);
            writer.RenderEndTag();
        }

        bool IPostBackDataHandler.LoadPostData(string postDataKey, NameValueCollection postCollection)
        {
            return this.LoadPostData(postDataKey, postCollection);
        }

        void IPostBackDataHandler.RaisePostDataChangedEvent()
        {
            this.RaisePostDataChangedEvent();
        }

        [WebSysDescription("RadioButton_GroupName"), WebCategory("Behavior"), Themeable(false), DefaultValue("")]
        public virtual string GroupName
        {
            get
            {
                string str = (string) this.ViewState["GroupName"];
                if (str != null)
                {
                    return str;
                }
                return string.Empty;
            }
            set
            {
                this.ViewState["GroupName"] = value;
            }
        }

        internal string UniqueGroupName
        {
            get
            {
                if (this._uniqueGroupName == null)
                {
                    string groupName = this.GroupName;
                    string uniqueID = this.UniqueID;
                    if (uniqueID != null)
                    {
                        int length = uniqueID.LastIndexOf(base.IdSeparator);
                        if (length >= 0)
                        {
                            if (groupName.Length > 0)
                            {
                                groupName = uniqueID.Substring(0, length + 1) + groupName;
                            }
                            else if (this.NamingContainer is RadioButtonList)
                            {
                                groupName = uniqueID.Substring(0, length);
                            }
                        }
                        if (groupName.Length == 0)
                        {
                            groupName = uniqueID;
                        }
                    }
                    this._uniqueGroupName = groupName;
                }
                return this._uniqueGroupName;
            }
        }

        internal string ValueAttribute
        {
            get
            {
                string str = base.Attributes["value"];
                if (str != null)
                {
                    return str;
                }
                base.EnsureID();
                if (this.ID != null)
                {
                    return this.ID;
                }
                return this.UniqueID;
            }
        }
    }
}

