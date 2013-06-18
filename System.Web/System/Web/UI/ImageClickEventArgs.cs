namespace System.Web.UI
{
    using System;

    public sealed class ImageClickEventArgs : EventArgs
    {
        public int X;
        public int Y;

        public ImageClickEventArgs(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }
    }
}

