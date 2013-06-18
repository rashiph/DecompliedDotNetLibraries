namespace System.Web.UI
{
    using System;
    using System.Web;

    internal sealed class ResourceBasedLiteralControl : LiteralControl
    {
        private bool _fAsciiOnly;
        private int _offset;
        private int _size;
        private TemplateControl _tplControl;

        internal ResourceBasedLiteralControl(TemplateControl tplControl, int offset, int size, bool fAsciiOnly)
        {
            if ((offset < 0) || ((offset + size) > tplControl.MaxResourceOffset))
            {
                throw new ArgumentException();
            }
            this._tplControl = tplControl;
            this._offset = offset;
            this._size = size;
            this._fAsciiOnly = fAsciiOnly;
            base.PreventAutoID();
            this.EnableViewState = false;
        }

        protected internal override void Render(HtmlTextWriter output)
        {
            if (this._size == 0)
            {
                base.Render(output);
            }
            else
            {
                output.WriteUTF8ResourceString(this._tplControl.StringResourcePointer, this._offset, this._size, this._fAsciiOnly);
            }
        }

        public override string Text
        {
            get
            {
                if (this._size == 0)
                {
                    return base.Text;
                }
                return StringResourceManager.ResourceToString(this._tplControl.StringResourcePointer, this._offset, this._size);
            }
            set
            {
                this._size = 0;
                base.Text = value;
            }
        }
    }
}

