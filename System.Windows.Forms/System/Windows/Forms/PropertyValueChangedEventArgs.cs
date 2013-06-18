namespace System.Windows.Forms
{
    using System;
    using System.Runtime.InteropServices;

    [ComVisible(true)]
    public class PropertyValueChangedEventArgs : EventArgs
    {
        private readonly GridItem changedItem;
        private object oldValue;

        public PropertyValueChangedEventArgs(GridItem changedItem, object oldValue)
        {
            this.changedItem = changedItem;
            this.oldValue = oldValue;
        }

        public GridItem ChangedItem
        {
            get
            {
                return this.changedItem;
            }
        }

        public object OldValue
        {
            get
            {
                return this.oldValue;
            }
        }
    }
}

