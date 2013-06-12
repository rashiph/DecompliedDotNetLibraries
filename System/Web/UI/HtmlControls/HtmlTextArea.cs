namespace System.Web.UI.HtmlControls
{
    using System;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Globalization;
    using System.Web;
    using System.Web.UI;

    [SupportsEventValidation, ValidationProperty("Value"), DefaultEvent("ServerChange")]
    public class HtmlTextArea : HtmlContainerControl, IPostBackDataHandler
    {
        private static readonly object EventServerChange = new object();

        [WebSysDescription("HtmlTextArea_OnServerChange"), WebCategory("Action")]
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

        public HtmlTextArea() : base("textarea")
        {
        }

        protected override void AddParsedSubObject(object obj)
        {
            if (!(obj is LiteralControl) && !(obj is DataBoundLiteralControl))
            {
                throw new HttpException(System.Web.SR.GetString("Cannot_Have_Children_Of_Type", new object[] { "HtmlTextArea", obj.GetType().Name.ToString(CultureInfo.InvariantCulture) }));
            }
            base.AddParsedSubObject(obj);
        }

        protected virtual bool LoadPostData(string postDataKey, NameValueCollection postCollection)
        {
            string str = this.Value;
            string str2 = postCollection.GetValues(postDataKey)[0];
            if ((str != null) && str.Equals(str2))
            {
                return false;
            }
            base.ValidateEvent(postDataKey);
            this.Value = str2;
            return true;
        }

        protected internal override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            if (!base.Disabled)
            {
                if (base.Events[EventServerChange] == null)
                {
                    this.ViewState.SetItemDirty("value", false);
                }
                if (this.Page != null)
                {
                    this.Page.RegisterEnabledControl(this);
                }
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
                this.Page.ClientScript.RegisterForEventValidation(this.RenderedNameAttribute);
            }
            writer.WriteAttribute("name", this.RenderedNameAttribute);
            base.Attributes.Remove("name");
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

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), WebCategory("Appearance"), DefaultValue("")]
        public int Cols
        {
            get
            {
                string s = base.Attributes["cols"];
                if (s == null)
                {
                    return -1;
                }
                return int.Parse(s, CultureInfo.InvariantCulture);
            }
            set
            {
                base.Attributes["cols"] = HtmlControl.MapIntegerAttributeToString(value);
            }
        }

        [DefaultValue(""), WebCategory("Behavior"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
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

        internal string RenderedNameAttribute
        {
            get
            {
                return this.Name;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), WebCategory("Appearance"), DefaultValue("")]
        public int Rows
        {
            get
            {
                string s = base.Attributes["rows"];
                if (s == null)
                {
                    return -1;
                }
                return int.Parse(s, CultureInfo.InvariantCulture);
            }
            set
            {
                base.Attributes["rows"] = HtmlControl.MapIntegerAttributeToString(value);
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), DefaultValue(""), WebCategory("Appearance")]
        public string Value
        {
            get
            {
                return this.InnerText;
            }
            set
            {
                this.InnerText = value;
            }
        }
    }
}

