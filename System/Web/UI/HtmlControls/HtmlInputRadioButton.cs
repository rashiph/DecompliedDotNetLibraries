namespace System.Web.UI.HtmlControls
{
    using System;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Web;
    using System.Web.UI;

    [DefaultEvent("ServerChange"), SupportsEventValidation]
    public class HtmlInputRadioButton : HtmlInputControl, IPostBackDataHandler
    {
        private static readonly object EventServerChange = new object();

        [WebCategory("Action"), WebSysDescription("Control_OnServerCheckChanged")]
        public event EventHandler ServerChange
        {
            add
            {
                base.Events.AddHandler(EventServerChange, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventServerChange, value);
            }
        }

        public HtmlInputRadioButton() : base("radio")
        {
        }

        protected virtual bool LoadPostData(string postDataKey, NameValueCollection postCollection)
        {
            string str = postCollection[this.RenderedNameAttribute];
            bool flag = false;
            if ((str != null) && str.Equals(this.Value))
            {
                if (!this.Checked)
                {
                    base.ValidateEvent(this.Value, this.RenderedNameAttribute);
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
            if ((this.Page != null) && !base.Disabled)
            {
                this.Page.RegisterRequiresPostBack(this);
                this.Page.RegisterEnabledControl(this);
            }
            if ((base.Events[EventServerChange] == null) && !base.Disabled)
            {
                this.ViewState.SetItemDirty("checked", false);
            }
        }

        protected virtual void OnServerChange(EventArgs e)
        {
            EventHandler handler = (EventHandler) base.Events[EventServerChange];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void RaisePostDataChangedEvent()
        {
            this.OnServerChange(EventArgs.Empty);
        }

        protected override void RenderAttributes(HtmlTextWriter writer)
        {
            if (this.Page != null)
            {
                this.Page.ClientScript.RegisterForEventValidation(this.Value, this.RenderedNameAttribute);
            }
            writer.WriteAttribute("value", this.Value);
            base.Attributes.Remove("value");
            base.RenderAttributes(writer);
        }

        bool IPostBackDataHandler.LoadPostData(string postDataKey, NameValueCollection postCollection)
        {
            return this.LoadPostData(postDataKey, postCollection);
        }

        void IPostBackDataHandler.RaisePostDataChangedEvent()
        {
            this.RaisePostDataChangedEvent();
        }

        [WebCategory("Default"), DefaultValue(""), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool Checked
        {
            get
            {
                string str = base.Attributes["checked"];
                if (str == null)
                {
                    return false;
                }
                return str.Equals("checked");
            }
            set
            {
                if (value)
                {
                    base.Attributes["checked"] = "checked";
                }
                else
                {
                    base.Attributes["checked"] = null;
                }
            }
        }

        public override string Name
        {
            get
            {
                string str = base.Attributes["name"];
                if (str == null)
                {
                    return string.Empty;
                }
                return str;
            }
            set
            {
                base.Attributes["name"] = HtmlControl.MapStringAttributeToString(value);
            }
        }

        internal override string RenderedNameAttribute
        {
            get
            {
                string renderedNameAttribute = base.RenderedNameAttribute;
                string uniqueID = this.UniqueID;
                int num = uniqueID.LastIndexOf(base.IdSeparator);
                if (num >= 0)
                {
                    renderedNameAttribute = uniqueID.Substring(0, num + 1) + renderedNameAttribute;
                }
                return renderedNameAttribute;
            }
        }

        public override string Value
        {
            get
            {
                string iD = base.Value;
                if (iD.Length != 0)
                {
                    return iD;
                }
                iD = this.ID;
                if (iD != null)
                {
                    return iD;
                }
                return this.UniqueID;
            }
            set
            {
                base.Value = value;
            }
        }
    }
}

