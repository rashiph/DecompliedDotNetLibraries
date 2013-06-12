namespace System.Web.UI.WebControls
{
    using System;
    using System.ComponentModel;

    public class FormViewModeEventArgs : CancelEventArgs
    {
        private bool _cancelingEdit;
        private FormViewMode _mode;

        public FormViewModeEventArgs(FormViewMode mode, bool cancelingEdit) : base(false)
        {
            this._mode = mode;
            this._cancelingEdit = cancelingEdit;
        }

        public bool CancelingEdit
        {
            get
            {
                return this._cancelingEdit;
            }
        }

        public FormViewMode NewMode
        {
            get
            {
                return this._mode;
            }
            set
            {
                this._mode = value;
            }
        }
    }
}

