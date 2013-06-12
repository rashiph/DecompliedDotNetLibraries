namespace System.Web.UI
{
    using System;
    using System.ComponentModel;

    [ToolboxItem(false), DataBindingHandler("System.Web.UI.Design.TextDataBindingHandler, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
    public sealed class DesignerDataBoundLiteralControl : Control
    {
        private string _text;

        public DesignerDataBoundLiteralControl()
        {
            base.PreventAutoID();
        }

        protected override ControlCollection CreateControlCollection()
        {
            return new EmptyControlCollection(this);
        }

        protected override void LoadViewState(object savedState)
        {
            if (savedState != null)
            {
                this._text = (string) savedState;
            }
        }

        protected internal override void Render(HtmlTextWriter output)
        {
            output.Write(this._text);
        }

        protected override object SaveViewState()
        {
            return this._text;
        }

        public string Text
        {
            get
            {
                return this._text;
            }
            set
            {
                this._text = (value != null) ? value : string.Empty;
            }
        }
    }
}

