namespace System.Windows.Forms
{
    using System;
    using System.Runtime.InteropServices;

    [ComVisible(true)]
    public class QueryContinueDragEventArgs : EventArgs
    {
        private DragAction action;
        private readonly bool escapePressed;
        private readonly int keyState;

        public QueryContinueDragEventArgs(int keyState, bool escapePressed, DragAction action)
        {
            this.keyState = keyState;
            this.escapePressed = escapePressed;
            this.action = action;
        }

        public DragAction Action
        {
            get
            {
                return this.action;
            }
            set
            {
                this.action = value;
            }
        }

        public bool EscapePressed
        {
            get
            {
                return this.escapePressed;
            }
        }

        public int KeyState
        {
            get
            {
                return this.keyState;
            }
        }
    }
}

