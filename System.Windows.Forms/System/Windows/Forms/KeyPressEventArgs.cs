namespace System.Windows.Forms
{
    using System;
    using System.Runtime.InteropServices;

    [ComVisible(true)]
    public class KeyPressEventArgs : EventArgs
    {
        private bool handled;
        private char keyChar;

        public KeyPressEventArgs(char keyChar)
        {
            this.keyChar = keyChar;
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

        public char KeyChar
        {
            get
            {
                return this.keyChar;
            }
            set
            {
                this.keyChar = value;
            }
        }
    }
}

