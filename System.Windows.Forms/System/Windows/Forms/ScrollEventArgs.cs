namespace System.Windows.Forms
{
    using System;
    using System.Runtime.InteropServices;

    [ComVisible(true)]
    public class ScrollEventArgs : EventArgs
    {
        private int newValue;
        private int oldValue;
        private System.Windows.Forms.ScrollOrientation scrollOrientation;
        private readonly ScrollEventType type;

        public ScrollEventArgs(ScrollEventType type, int newValue)
        {
            this.oldValue = -1;
            this.type = type;
            this.newValue = newValue;
        }

        public ScrollEventArgs(ScrollEventType type, int oldValue, int newValue)
        {
            this.oldValue = -1;
            this.type = type;
            this.newValue = newValue;
            this.oldValue = oldValue;
        }

        public ScrollEventArgs(ScrollEventType type, int newValue, System.Windows.Forms.ScrollOrientation scroll)
        {
            this.oldValue = -1;
            this.type = type;
            this.newValue = newValue;
            this.scrollOrientation = scroll;
        }

        public ScrollEventArgs(ScrollEventType type, int oldValue, int newValue, System.Windows.Forms.ScrollOrientation scroll)
        {
            this.oldValue = -1;
            this.type = type;
            this.newValue = newValue;
            this.scrollOrientation = scroll;
            this.oldValue = oldValue;
        }

        public int NewValue
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

        public int OldValue
        {
            get
            {
                return this.oldValue;
            }
        }

        public System.Windows.Forms.ScrollOrientation ScrollOrientation
        {
            get
            {
                return this.scrollOrientation;
            }
        }

        public ScrollEventType Type
        {
            get
            {
                return this.type;
            }
        }
    }
}

