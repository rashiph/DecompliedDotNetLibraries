namespace System.Web.UI.WebControls
{
    using System;
    using System.ComponentModel;

    public class DetailsViewModeEventArgs : CancelEventArgs
    {
        private bool _cancelingEdit;
        private DetailsViewMode _mode;

        public DetailsViewModeEventArgs(DetailsViewMode mode, bool cancelingEdit) : base(false)
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

        public DetailsViewMode NewMode
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

