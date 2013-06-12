namespace System.Windows.Forms
{
    using System;

    public class ItemChangedEventArgs : EventArgs
    {
        private int index;

        internal ItemChangedEventArgs(int index)
        {
            this.index = index;
        }

        public int Index
        {
            get
            {
                return this.index;
            }
        }
    }
}

