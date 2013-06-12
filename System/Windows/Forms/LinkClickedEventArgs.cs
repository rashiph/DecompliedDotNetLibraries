namespace System.Windows.Forms
{
    using System;
    using System.Runtime.InteropServices;

    [ComVisible(true)]
    public class LinkClickedEventArgs : EventArgs
    {
        private string linkText;

        public LinkClickedEventArgs(string linkText)
        {
            this.linkText = linkText;
        }

        public string LinkText
        {
            get
            {
                return this.linkText;
            }
        }
    }
}

