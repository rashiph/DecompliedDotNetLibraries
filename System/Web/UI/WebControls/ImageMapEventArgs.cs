namespace System.Web.UI.WebControls
{
    using System;

    public class ImageMapEventArgs : EventArgs
    {
        private string _postBackValue;

        public ImageMapEventArgs(string value)
        {
            this._postBackValue = value;
        }

        public string PostBackValue
        {
            get
            {
                return this._postBackValue;
            }
        }
    }
}

