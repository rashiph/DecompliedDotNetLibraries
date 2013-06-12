namespace System.Windows.Forms
{
    using System;
    using System.Runtime.InteropServices;

    [ComVisible(true)]
    public class ItemCheckEventArgs : EventArgs
    {
        private readonly CheckState currentValue;
        private readonly int index;
        private CheckState newValue;

        public ItemCheckEventArgs(int index, CheckState newCheckValue, CheckState currentValue)
        {
            this.index = index;
            this.newValue = newCheckValue;
            this.currentValue = currentValue;
        }

        public CheckState CurrentValue
        {
            get
            {
                return this.currentValue;
            }
        }

        public int Index
        {
            get
            {
                return this.index;
            }
        }

        public CheckState NewValue
        {
            get
            {
                return this.newValue;
            }
            set
            {
                this.newValue = value;
            }
        }
    }
}

