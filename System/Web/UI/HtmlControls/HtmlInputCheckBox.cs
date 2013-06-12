namespace System.Web.UI.HtmlControls
{
    using System;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Web;
    using System.Web.UI;

    [SupportsEventValidation, DefaultEvent("ServerChange")]
    public class HtmlInputCheckBox : HtmlInputControl, IPostBackDataHandler
    {
        private static readonly object EventServerChange = new object();

        [WebSysDescription("Control_OnServerCheckChanged"), WebCategory("Action")]
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

        public HtmlInputCheckBox() : base("checkbox")
        {
        }

        protected virtual bool LoadPostData(string postDataKey, NameValueCollection postCollection)
        {
            string str = postCollection[postDataKey];
            bool flag = !string.IsNullOrEmpty(str);
            bool flag2 = flag != this.Checked;
            this.Checked = flag;
            if (flag)
            {
                base.ValidateEvent(postDataKey);
            }
            return flag2;
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
            base.RenderAttributes(writer);
            if (this.Page != null)
            {
                this.Page.ClientScript.RegisterForEventValidation(this.RenderedNameAttribute);
            }
        }

        bool IPostBackDataHandler.LoadPostData(string postDataKey, NameValueCollection postCollection)
        {
            return this.LoadPostData(postDataKey, postCollection);
        }

        void IPostBackDataHandler.RaisePostDataChangedEvent()
        {
            this.RaisePostDataChangedEvent();
        }

        [DefaultValue(""), TypeConverter(typeof(MinimizableAttributeTypeConverter)), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), WebCategory("Default")]
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
    }
}

