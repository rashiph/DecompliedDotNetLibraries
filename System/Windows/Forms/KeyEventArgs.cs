namespace System.Windows.Forms
{
    using System;
    using System.Runtime.InteropServices;

    [ComVisible(true)]
    public class KeyEventArgs : EventArgs
    {
        private bool handled;
        private readonly Keys keyData;
        private bool suppressKeyPress;

        public KeyEventArgs(Keys keyData)
        {
            this.keyData = keyData;
        }

        public virtual bool Alt
        {
            get
            {
                return ((this.keyData & Keys.Alt) == Keys.Alt);
            }
        }

        public bool Control
        {
            get
            {
                return ((this.keyData & Keys.Control) == Keys.Control);
            }
        }

        public bool Handled
        {
            get
            {
                return this.handled;
            }
            set
            {
                this.handled = value;
            }
        }

        public Keys KeyCode
        {
            get
            {
                Keys keys = this.keyData & Keys.KeyCode;
                if (!Enum.IsDefined(typeof(Keys), (int) keys))
                {
                    return Keys.None;
                }
                return keys;
            }
        }

        public Keys KeyData
        {
            get
            {
                return this.keyData;
            }
        }

        public int KeyValue
        {
            get
            {
                return (((int) this.keyData) & 0xffff);
            }
        }

        public Keys Modifiers
        {
            get
            {
                return (this.keyData & ~Keys.KeyCode);
            }
        }

        public virtual bool Shift
        {
            get
            {
                return ((this.keyData & Keys.Shift) == Keys.Shift);
            }
        }

        public bool SuppressKeyPress
        {
            get
            {
                return this.suppressKeyPress;
            }
            set
            {
                this.suppressKeyPress = value;
                this.handled = value;
            }
        }
    }
}

