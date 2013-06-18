namespace System.Web.UI.Design
{
    using System;
    using System.Drawing;

    public sealed class DesignerRegionMouseEventArgs : EventArgs
    {
        private Point _location;
        private DesignerRegion _region;

        public DesignerRegionMouseEventArgs(DesignerRegion region, Point location)
        {
            this._location = location;
            this._region = region;
        }

        public Point Location
        {
            get
            {
                return this._location;
            }
        }

        public DesignerRegion Region
        {
            get
            {
                return this._region;
            }
        }
    }
}

