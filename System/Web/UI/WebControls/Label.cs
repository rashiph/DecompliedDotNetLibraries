namespace System.Web.UI.WebControls
{
    using System;
    using System.ComponentModel;
    using System.Web;
    using System.Web.UI;
    using System.Web.Util;

    [DataBindingHandler("System.Web.UI.Design.TextDataBindingHandler, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), Designer("System.Web.UI.Design.WebControls.LabelDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), ToolboxData("<{0}:Label runat=\"server\" Text=\"Label\"></{0}:Label>"), DefaultProperty("Text"), ParseChildren(false), ControlBuilder(typeof(LabelControlBuilder)), ControlValueProperty("Text")]
    public class Label : WebControl, ITextControl
    {
        private bool _textSetByAddParsedSubObject;

        public Label()
        {
        }

        internal Label(HtmlTextWriterTag tag) : base(tag)
        {
        }

        protected override void AddAttributesToRender(HtmlTextWriter writer)
        {
            string associatedControlID = this.AssociatedControlID;
            if (associatedControlID.Length != 0)
            {
                if (this.AssociatedControlInControlTree)
                {
                    Control control = this.FindControl(associatedControlID);
                    if (control == null)
                    {
                        if (!base.DesignMode)
                        {
                            throw new HttpException(System.Web.SR.GetString("LabelForNotFound", new object[] { associatedControlID, this.ID }));
                        }
                    }
                    else
                    {
                        writer.AddAttribute(HtmlTextWriterAttribute.For, control.ClientID);
                    }
                }
                else
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.For, associatedControlID);
                }
            }
            base.AddAttributesToRender(writer);
        }

        protected override void AddParsedSubObject(object obj)
        {
            if (this.HasControls())
            {
                base.AddParsedSubObject(obj);
            }
            else if (obj is LiteralControl)
            {
                if (this._textSetByAddParsedSubObject)
                {
                    this.Text = this.Text + ((LiteralControl) obj).Text;
                }
                else
                {
                    this.Text = ((LiteralControl) obj).Text;
                }
                this._textSetByAddParsedSubObject = true;
            }
            else
            {
                string text = this.Text;
                if (text.Length != 0)
                {
                    this.Text = string.Empty;
                    base.AddParsedSubObject(new LiteralControl(text));
                }
                base.AddParsedSubObject(obj);
            }
        }

        protected override void LoadViewState(object savedState)
        {
            if (savedState != null)
            {
                base.LoadViewState(savedState);
                if ((((string) this.ViewState["Text"]) != null) && this.HasControls())
                {
                    this.Controls.Clear();
                }
            }
        }

        protected internal override void RenderContents(HtmlTextWriter writer)
        {
            if (base.HasRenderingData())
            {
                base.RenderContents(writer);
            }
            else
            {
                writer.Write(this.Text);
            }
        }

        [IDReferenceProperty, DefaultValue(""), TypeConverter(typeof(AssociatedControlConverter)), WebCategory("Accessibility"), WebSysDescription("Label_AssociatedControlID"), Themeable(false)]
        public virtual string AssociatedControlID
        {
            get
            {
                string str = (string) this.ViewState["AssociatedControlID"];
                if (str != null)
                {
                    return str;
                }
                return string.Empty;
            }
            set
            {
                this.ViewState["AssociatedControlID"] = value;
            }
        }

        internal bool AssociatedControlInControlTree
        {
            get
            {
                object obj2 = this.ViewState["AssociatedControlNotInControlTree"];
                if (obj2 != null)
                {
                    return (bool) obj2;
                }
                return true;
            }
            set
            {
                this.ViewState["AssociatedControlNotInControlTree"] = value;
            }
        }

        internal override bool RequiresLegacyRendering
        {
            get
            {
                return true;
            }
        }

        public override bool SupportsDisabledAttribute
        {
            get
            {
                return (this.RenderingCompatibility < VersionUtil.Framework40);
            }
        }

        protected override HtmlTextWriterTag TagKey
        {
            get
            {
                if (this.AssociatedControlID.Length != 0)
                {
                    return HtmlTextWriterTag.Label;
                }
                return base.TagKey;
            }
        }

        [PersistenceMode(PersistenceMode.InnerDefaultProperty), WebSysDescription("Label_Text"), Localizable(true), Bindable(true), WebCategory("Appearance"), DefaultValue("")]
        public virtual string Text
        {
            get
            {
                object obj2 = this.ViewState["Text"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return string.Empty;
            }
            set
            {
                if (this.HasControls())
                {
                    this.Controls.Clear();
                }
                this.ViewState["Text"] = value;
            }
        }
    }
}

