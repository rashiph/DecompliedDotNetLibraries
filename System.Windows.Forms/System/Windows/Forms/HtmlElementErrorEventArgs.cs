namespace System.Windows.Forms
{
    using System;

    public sealed class HtmlElementErrorEventArgs : EventArgs
    {
        private string description;
        private bool handled;
        private int lineNumber;
        private Uri url;
        private string urlString;

        internal HtmlElementErrorEventArgs(string description, string urlString, int lineNumber)
        {
            this.description = description;
            this.urlString = urlString;
            this.lineNumber = lineNumber;
        }

        public string Description
        {
            get
            {
                return this.description;
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

        public int LineNumber
        {
            get
            {
                return this.lineNumber;
            }
        }

        public Uri Url
        {
            get
            {
                if (this.url == null)
                {
                    this.url = new Uri(this.urlString);
                }
                return this.url;
            }
        }
    }
}

