namespace System.Web.UI.HtmlControls
{
    using System;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Web;
    using System.Web.UI;

    [SupportsEventValidation, DefaultEvent("ServerChange")]
    public class HtmlInputHidden : HtmlInputControl, IPostBackDataHandler
    {
        private static readonly object EventServerChange = new object();

        [WebCategory("Action"), WebSysDescription("HtmlInputHidden_OnServerChange")]
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

        public HtmlInputHidden() : base("hidden")
        {
        }

        protected virtual bool LoadPostData(string postDataKey, NameValueCollection postCollection)
        {
            string str = this.Value;
            string str2 = postCollection.GetValues(postDataKey)[0];
            if (!str.Equals(str2))
            {
                base.ValidateEvent(postDataKey);
                this.Value = str2;
                return true;
            }
            return false;
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
    }
}

