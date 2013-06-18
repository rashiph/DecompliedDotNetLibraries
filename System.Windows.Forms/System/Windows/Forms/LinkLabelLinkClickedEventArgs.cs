namespace System.Windows.Forms
{
    using System;
    using System.Runtime.InteropServices;

    [ComVisible(true)]
    public class LinkLabelLinkClickedEventArgs : EventArgs
    {
        private readonly MouseButtons button;
        private readonly System.Windows.Forms.LinkLabel.Link link;

        public LinkLabelLinkClickedEventArgs(System.Windows.Forms.LinkLabel.Link link)
        {
            this.link = link;
            this.button = MouseButtons.Left;
        }

        public LinkLabelLinkClickedEventArgs(System.Windows.Forms.LinkLabel.Link link, MouseButtons button) : this(link)
        {
            this.button = button;
        }

        public MouseButtons Button
        {
            get
            {
                return this.button;
            }
        }

        public System.Windows.Forms.LinkLabel.Link Link
        {
            get
            {
                return this.link;
            }
        }
    }
}

