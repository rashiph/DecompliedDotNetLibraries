namespace System.Web.UI.HtmlControls
{
    using System;
    using System.ComponentModel;
    using System.Web;
    using System.Web.UI;

    [SupportsEventValidation, DefaultEvent("ServerClick")]
    public class HtmlInputButton : HtmlInputControl, IPostBackEventHandler
    {
        private static readonly object EventServerClick = new object();

        [WebSysDescription("HtmlControl_OnServerClick"), WebCategory("Action")]
        public event EventHandler ServerClick
        {
            add
            {
                base.Events.AddHandler(EventServerClick, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventServerClick, value);
            }
        }

        public HtmlInputButton() : base("button")
        {
        }

        public HtmlInputButton(string type) : base(type)
        {
        }

        protected internal override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            if ((this.Page != null) && (base.Events[EventServerClick] != null))
            {
                this.Page.RegisterPostBackScript();
            }
        }

        protected virtual void OnServerClick(EventArgs e)
        {
            EventHandler handler = (EventHandler) base.Events[EventServerClick];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void RaisePostBackEvent(string eventArgument)
        {
            base.ValidateEvent(this.UniqueID, eventArgument);
            if (this.CausesValidation)
            {
                this.Page.Validate(this.ValidationGroup);
            }
            this.OnServerClick(EventArgs.Empty);
        }

        protected override void RenderAttributes(HtmlTextWriter writer)
        {
            this.RenderAttributesInternal(writer);
            base.RenderAttributes(writer);
        }

        internal virtual void RenderAttributesInternal(HtmlTextWriter writer)
        {
            bool submitsProgramatically = base.Events[EventServerClick] != null;
            if (this.Page != null)
            {
                if (submitsProgramatically)
                {
                    Util.WriteOnClickAttribute(writer, this, false, submitsProgramatically, this.CausesValidation && (this.Page.GetValidators(this.ValidationGroup).Count > 0), this.ValidationGroup);
                }
                else
                {
                    this.Page.ClientScript.RegisterForEventValidation(this.UniqueID);
                }
            }
        }

        void IPostBackEventHandler.RaisePostBackEvent(string eventArgument)
        {
            this.RaisePostBackEvent(eventArgument);
        }

        [WebCategory("Behavior"), DefaultValue(true)]
        public virtual bool CausesValidation
        {
            get
            {
                object obj2 = this.ViewState["CausesValidation"];
                if (obj2 != null)
                {
                    return (bool) obj2;
                }
                return true;
            }
            set
            {
                this.ViewState["CausesValidation"] = value;
            }
        }

        [WebSysDescription("PostBackControl_ValidationGroup"), WebCategory("Behavior"), DefaultValue("")]
        public virtual string ValidationGroup
        {
            get
            {
                string str = (string) this.ViewState["ValidationGroup"];
                if (str != null)
                {
                    return str;
                }
                return string.Empty;
            }
            set
            {
                this.ViewState["ValidationGroup"] = value;
            }
        }
    }
}

