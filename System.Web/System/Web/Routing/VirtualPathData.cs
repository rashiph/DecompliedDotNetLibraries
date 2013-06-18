namespace System.Web.Routing
{
    using System;
    using System.Runtime.CompilerServices;

    [TypeForwardedFrom("System.Web.Routing, Version=3.5.0.0, Culture=Neutral, PublicKeyToken=31bf3856ad364e35")]
    public class VirtualPathData
    {
        private RouteValueDictionary _dataTokens = new RouteValueDictionary();
        private string _virtualPath;

        public VirtualPathData(RouteBase route, string virtualPath)
        {
            this.Route = route;
            this.VirtualPath = virtualPath;
        }

        public RouteValueDictionary DataTokens
        {
            get
            {
                return this._dataTokens;
            }
        }

        public RouteBase Route { get; set; }

        public string VirtualPath
        {
            get
            {
                return (this._virtualPath ?? string.Empty);
            }
            set
            {
                this._virtualPath = value;
            }
        }
    }
}

