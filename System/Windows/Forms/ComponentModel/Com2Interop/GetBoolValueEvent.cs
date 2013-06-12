namespace System.Windows.Forms.ComponentModel.Com2Interop
{
    using System;

    internal class GetBoolValueEvent : EventArgs
    {
        private bool value;

        public GetBoolValueEvent(bool defValue)
        {
            this.value = defValue;
        }

        public bool Value
        {
            get
            {
                return this.value;
            }
            set
            {
                this.value = value;
            }
        }
    }
}

