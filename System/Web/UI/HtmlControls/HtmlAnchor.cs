namespace System.Web.UI.HtmlControls
{
    using System;
    using System.ComponentModel;
    using System.Web;
    using System.Web.UI;

    [SupportsEventValidation, DefaultEvent("ServerClick")]
    public class HtmlAnchor : HtmlContainerControl, IPostBackEventHandler
    {
        private static readonly object EventServerClick = new object();

        [WebCategory("Action"), WebSysDescription("HtmlControl_OnServerClick")]
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

        public HtmlAnchor() : base("a")
        {
        }

        private PostBackOptions GetPostBackOptions()
        {
            PostBackOptions options = new PostBackOptions(this, string.Empty) {
                RequiresJavaScriptProtocol = true
            };
            if (this.CausesValidation && (this.Page.GetValidators(this.ValidationGroup).Count > 0))
            {
                options.PerformValidation = true;
                options.ValidationGroup = this.ValidationGroup;
            }
            return options;
        }

        protected internal override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            if ((this.Page != null) && (base.Events[EventServerClick] != null))
            {
                this.Page.RegisterPostBackScript();
                if (this.CausesValidation && (this.Page.GetValidators(this.ValidationGroup).Count > 0))
                {
                    this.Page.RegisterWebFormsScript();
                }
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
            if (base.Events[EventServerClick] != null)
            {
                base.Attributes.Remove("href");
                base.RenderAttributes(writer);
                PostBackOptions postBackOptions = this.GetPostBackOptions();
                string postBackEventReference = this.Page.ClientScript.GetPostBackEventReference(postBackOptions, true);
                writer.WriteAttribute("href", postBackEventReference, true);
            }
            else
            {
                base.PreProcessRelativeReferenceAttribute(writer, "href");
                base.RenderAttributes(writer);
            }
        }

        void IPostBackEventHandler.RaisePostBackEvent(string eventArgument)
        {
            this.RaisePostBackEvent(eventArgument);
        }

        [DefaultValue(true), WebCategory("Behavior")]
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

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), UrlProperty, DefaultValue(""), WebCategory("Navigation")]
        public string HRef
        {
            get
            {
                string str = base.Attributes["href"];
                if (str == null)
                {
                    return string.Empty;
                }
                return str;
            }
            set
            {
                base.Attributes["href"] = HtmlControl.MapStringAttributeToString(value);
            }
        }

        [DefaultValue(""), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), WebCategory("Navigation")]
        public string Name
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

        [WebCategory("Navigation"), DefaultValue(""), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
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

        [DefaultValue(""), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), WebCategory("Appearance"), Localizable(true)]
        public string Title
        {
            get
            {
                string str = base.Attributes["title"];
                if (str == null)
                {
                    return string.Empty;
                }
                return str;
            }
            set
            {
                base.Attributes["title"] = HtmlControl.MapStringAttributeToString(value);
            }
        }

        [WebSysDescription("PostBackControl_ValidationGroup"), DefaultValue(""), WebCategory("Behavior")]
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

