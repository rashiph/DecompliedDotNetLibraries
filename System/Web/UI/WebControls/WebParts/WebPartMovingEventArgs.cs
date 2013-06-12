namespace System.Web.UI.WebControls.WebParts
{
    using System;

    public class WebPartMovingEventArgs : WebPartCancelEventArgs
    {
        private WebPartZoneBase _zone;
        private int _zoneIndex;

        public WebPartMovingEventArgs(WebPart webPart, WebPartZoneBase zone, int zoneIndex) : base(webPart)
        {
            this._zone = zone;
            this._zoneIndex = zoneIndex;
        }

        public WebPartZoneBase Zone
        {
            get
            {
                return this._zone;
            }
            set
            {
                this._zone = value;
            }
        }

        public int ZoneIndex
        {
            get
            {
                return this._zoneIndex;
            }
            set
            {
                this._zoneIndex = value;
            }
        }
    }
}

