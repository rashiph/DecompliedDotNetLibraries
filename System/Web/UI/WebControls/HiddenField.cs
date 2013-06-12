namespace System.Web.UI.WebControls
{
    using System;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Web;
    using System.Web.UI;

    [ControlValueProperty("Value"), NonVisualControl, ParseChildren(true), PersistChildren(false), DefaultEvent("ValueChanged"), Designer("System.Web.UI.Design.WebControls.HiddenFieldDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), SupportsEventValidation, DefaultProperty("Value")]
    public class HiddenField : Control, IPostBackDataHandler
    {
        private static readonly object EventValueChanged = new object();

        [WebCategory("Action"), WebSysDescription("HiddenField_OnValueChanged")]
        public event EventHandler ValueChanged
        {
            add
            {
                base.Events.AddHandler(EventValueChanged, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventValueChanged, value);
            }
        }

        protected override ControlCollection CreateControlCollection()
        {
            return new EmptyControlCollection(this);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override void Focus()
        {
            throw new NotSupportedException(System.Web.SR.GetString("NoFocusSupport", new object[] { base.GetType().Name }));
        }

        protected virtual bool LoadPostData(string postDataKey, NameValueCollection postCollection)
        {
            base.ValidateEvent(this.UniqueID);
            string str = this.Value;
            string str2 = postCollection[postDataKey];
            if (!str.Equals(str2, StringComparison.Ordinal))
            {
                this.Value = str2;
                return true;
            }
            return false;
        }

        protected internal override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            if (!this.SaveValueViewState)
            {
                this.ViewState.SetItemDirty("Value", false);
            }
        }

        protected virtual void OnValueChanged(EventArgs e)
        {
            EventHandler handler = (EventHandler) base.Events[EventValueChanged];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void RaisePostDataChangedEvent()
        {
            this.OnValueChanged(EventArgs.Empty);
        }

        protected internal override void Render(HtmlTextWriter writer)
        {
            string uniqueID = this.UniqueID;
            if (this.Page != null)
            {
                this.Page.VerifyRenderingInServerForm(this);
                this.Page.ClientScript.RegisterForEventValidation(uniqueID);
            }
            writer.AddAttribute(HtmlTextWriterAttribute.Type, "hidden");
            if (uniqueID != null)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Name, uniqueID);
            }
            if (this.ID != null)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Id, this.ClientID);
            }
            string str2 = this.Value;
            if (str2.Length > 0)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Value, str2);
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

        [EditorBrowsable(EditorBrowsableState.Never), DefaultValue(false)]
        public override bool EnableTheming
        {
            get
            {
                return false;
            }
            set
            {
                throw new NotSupportedException(System.Web.SR.GetString("NoThemingSupport", new object[] { base.GetType().Name }));
            }
        }

        private bool SaveValueViewState
        {
            get
            {
                if (((base.Events[EventValueChanged] == null) && this.Visible) && !(base.GetType() != typeof(HiddenField)))
                {
                    return false;
                }
                return true;
            }
        }

        [DefaultValue(""), EditorBrowsable(EditorBrowsableState.Never)]
        public override string SkinID
        {
            get
            {
                return string.Empty;
            }
            set
            {
                throw new NotSupportedException(System.Web.SR.GetString("NoThemingSupport", new object[] { base.GetType().Name }));
            }
        }

        [Bindable(true), WebCategory("Behavior"), DefaultValue(""), WebSysDescription("HiddenField_Value")]
        public virtual string Value
        {
            get
            {
                string str = (string) this.ViewState["Value"];
                if (str == null)
                {
                    return string.Empty;
                }
                return str;
            }
            set
            {
                this.ViewState["Value"] = value;
            }
        }
    }
}

