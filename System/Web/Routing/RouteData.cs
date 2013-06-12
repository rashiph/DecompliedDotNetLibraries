namespace System.Web.Routing
{
    using System;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Web;

    [TypeForwardedFrom("System.Web.Routing, Version=3.5.0.0, Culture=Neutral, PublicKeyToken=31bf3856ad364e35")]
    public class RouteData
    {
        private RouteValueDictionary _dataTokens;
        private IRouteHandler _routeHandler;
        private RouteValueDictionary _values;

        public RouteData()
        {
            this._values = new RouteValueDictionary();
            this._dataTokens = new RouteValueDictionary();
        }

        public RouteData(RouteBase route, IRouteHandler routeHandler)
        {
            this._values = new RouteValueDictionary();
            this._dataTokens = new RouteValueDictionary();
            this.Route = route;
            this.RouteHandler = routeHandler;
        }

        public string GetRequiredString(string valueName)
        {
            object obj2;
            if (this.Values.TryGetValue(valueName, out obj2))
            {
                string str = obj2 as string;
                if (!string.IsNullOrEmpty(str))
                {
                    return str;
                }
            }
            throw new InvalidOperationException(string.Format(CultureInfo.CurrentUICulture, System.Web.SR.GetString("RouteData_RequiredValue"), new object[] { valueName }));
        }

        public RouteValueDictionary DataTokens
        {
            get
            {
                return this._dataTokens;
            }
        }

        public RouteBase Route { get; set; }

        public IRouteHandler RouteHandler
        {
            get
            {
                return this._routeHandler;
            }
            set
            {
                this._routeHandler = value;
            }
        }

        public RouteValueDictionary Values
        {
            get
            {
                return this._values;
            }
        }
    }
}

