namespace System.Windows.Forms
{
    using System;

    public class UpDownEventArgs : EventArgs
    {
        private int buttonID;

        public UpDownEventArgs(int buttonPushed)
        {
            this.buttonID = buttonPushed;
        }

        public int ButtonID
        {
            get
            {
                return this.buttonID;
            }
        }
    }
}

