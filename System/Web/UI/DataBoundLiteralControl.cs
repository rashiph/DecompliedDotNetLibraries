namespace System.Web.UI
{
    using System;
    using System.ComponentModel;
    using System.Text;

    [ToolboxItem(false)]
    public sealed class DataBoundLiteralControl : Control, ITextControl
    {
        private string[] _dataBoundLiteral;
        private bool _hasDataBoundStrings;
        private string[] _staticLiterals;

        public DataBoundLiteralControl(int staticLiteralsCount, int dataBoundLiteralCount)
        {
            this._staticLiterals = new string[staticLiteralsCount];
            this._dataBoundLiteral = new string[dataBoundLiteralCount];
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
                this._dataBoundLiteral = (string[]) savedState;
                this._hasDataBoundStrings = true;
            }
        }

        protected internal override void Render(HtmlTextWriter output)
        {
            int length = this._dataBoundLiteral.Length;
            for (int i = 0; i < this._staticLiterals.Length; i++)
            {
                if (this._staticLiterals[i] != null)
                {
                    output.Write(this._staticLiterals[i]);
                }
                if ((i < length) && (this._dataBoundLiteral[i] != null))
                {
                    output.Write(this._dataBoundLiteral[i]);
                }
            }
        }

        protected override object SaveViewState()
        {
            if (!this._hasDataBoundStrings)
            {
                return null;
            }
            return this._dataBoundLiteral;
        }

        public void SetDataBoundString(int index, string s)
        {
            this._dataBoundLiteral[index] = s;
            this._hasDataBoundStrings = true;
        }

        public void SetStaticString(int index, string s)
        {
            this._staticLiterals[index] = s;
        }

        string ITextControl.Text
        {
            get
            {
                return this.Text;
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        public string Text
        {
            get
            {
                StringBuilder builder = new StringBuilder();
                int length = this._dataBoundLiteral.Length;
                for (int i = 0; i < this._staticLiterals.Length; i++)
                {
                    if (this._staticLiterals[i] != null)
                    {
                        builder.Append(this._staticLiterals[i]);
                    }
                    if ((i < length) && (this._dataBoundLiteral[i] != null))
                    {
                        builder.Append(this._dataBoundLiteral[i]);
                    }
                }
                return builder.ToString();
            }
        }
    }
}

