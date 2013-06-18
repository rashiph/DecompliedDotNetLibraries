namespace System.Web.UI.WebControls
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Web;
    using System.Web.UI;

    [Designer("System.Web.UI.Design.WebControls.LiteralDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), ControlBuilder(typeof(LiteralControlBuilder)), DataBindingHandler("System.Web.UI.Design.TextDataBindingHandler, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), DefaultProperty("Text")]
    public class Literal : Control, ITextControl
    {
        protected override void AddParsedSubObject(object obj)
        {
            if (!(obj is LiteralControl))
            {
                throw new HttpException(System.Web.SR.GetString("Cannot_Have_Children_Of_Type", new object[] { "Literal", obj.GetType().Name.ToString(CultureInfo.InvariantCulture) }));
            }
            this.Text = ((LiteralControl) obj).Text;
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

        protected internal override void Render(HtmlTextWriter writer)
        {
            string text = this.Text;
            if (text.Length != 0)
            {
                if (this.Mode != LiteralMode.Encode)
                {
                    writer.Write(text);
                }
                else
                {
                    HttpUtility.HtmlEncode(text, writer);
                }
            }
        }

        [WebSysDescription("Literal_Mode"), DefaultValue(0), WebCategory("Behavior")]
        public LiteralMode Mode
        {
            get
            {
                object obj2 = this.ViewState["Mode"];
                if (obj2 != null)
                {
                    return (LiteralMode) obj2;
                }
                return LiteralMode.Transform;
            }
            set
            {
                if ((value < LiteralMode.Transform) || (value > LiteralMode.Encode))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                this.ViewState["Mode"] = value;
            }
        }

        [Bindable(true), WebCategory("Appearance"), Localizable(true), DefaultValue(""), WebSysDescription("Literal_Text")]
        public string Text
        {
            get
            {
                string str = (string) this.ViewState["Text"];
                if (str == null)
                {
                    return string.Empty;
                }
                return str;
            }
            set
            {
                this.ViewState["Text"] = value;
            }
        }
    }
}

