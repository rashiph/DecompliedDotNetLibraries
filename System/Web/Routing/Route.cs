namespace System.Web.Routing
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Text.RegularExpressions;
    using System.Web;

    [TypeForwardedFrom("System.Web.Routing, Version=3.5.0.0, Culture=Neutral, PublicKeyToken=31bf3856ad364e35")]
    public class Route : RouteBase
    {
        private ParsedRoute _parsedRoute;
        private string _url;
        private const string HttpMethodParameterName = "httpMethod";

        public Route(string url, IRouteHandler routeHandler)
        {
            this.Url = url;
            this.RouteHandler = routeHandler;
        }

        public Route(string url, RouteValueDictionary defaults, IRouteHandler routeHandler)
        {
            this.Url = url;
            this.Defaults = defaults;
            this.RouteHandler = routeHandler;
        }

        public Route(string url, RouteValueDictionary defaults, RouteValueDictionary constraints, IRouteHandler routeHandler)
        {
            this.Url = url;
            this.Defaults = defaults;
            this.Constraints = constraints;
            this.RouteHandler = routeHandler;
        }

        public Route(string url, RouteValueDictionary defaults, RouteValueDictionary constraints, RouteValueDictionary dataTokens, IRouteHandler routeHandler)
        {
            this.Url = url;
            this.Defaults = defaults;
            this.Constraints = constraints;
            this.DataTokens = dataTokens;
            this.RouteHandler = routeHandler;
        }

        public override RouteData GetRouteData(HttpContextBase httpContext)
        {
            string virtualPath = httpContext.Request.AppRelativeCurrentExecutionFilePath.Substring(2) + httpContext.Request.PathInfo;
            RouteValueDictionary values = this._parsedRoute.Match(virtualPath, this.Defaults);
            if (values == null)
            {
                return null;
            }
            RouteData data = new RouteData(this, this.RouteHandler);
            if (!this.ProcessConstraints(httpContext, values, RouteDirection.IncomingRequest))
            {
                return null;
            }
            foreach (KeyValuePair<string, object> pair in values)
            {
                data.Values.Add(pair.Key, pair.Value);
            }
            if (this.DataTokens != null)
            {
                foreach (KeyValuePair<string, object> pair2 in this.DataTokens)
                {
                    data.DataTokens[pair2.Key] = pair2.Value;
                }
            }
            return data;
        }

        public override VirtualPathData GetVirtualPath(RequestContext requestContext, RouteValueDictionary values)
        {
            BoundUrl url = this._parsedRoute.Bind(requestContext.RouteData.Values, values, this.Defaults, this.Constraints);
            if (url == null)
            {
                return null;
            }
            if (!this.ProcessConstraints(requestContext.HttpContext, url.Values, RouteDirection.UrlGeneration))
            {
                return null;
            }
            VirtualPathData data = new VirtualPathData(this, url.Url);
            if (this.DataTokens != null)
            {
                foreach (KeyValuePair<string, object> pair in this.DataTokens)
                {
                    data.DataTokens[pair.Key] = pair.Value;
                }
            }
            return data;
        }

        protected virtual bool ProcessConstraint(HttpContextBase httpContext, object constraint, string parameterName, RouteValueDictionary values, RouteDirection routeDirection)
        {
            object obj2;
            IRouteConstraint constraint2 = constraint as IRouteConstraint;
            if (constraint2 != null)
            {
                return constraint2.Match(httpContext, this, parameterName, values, routeDirection);
            }
            string str = constraint as string;
            if (str == null)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentUICulture, System.Web.SR.GetString("Route_ValidationMustBeStringOrCustomConstraint"), new object[] { parameterName, this.Url }));
            }
            values.TryGetValue(parameterName, out obj2);
            string input = Convert.ToString(obj2, CultureInfo.InvariantCulture);
            string pattern = "^(" + str + ")$";
            return Regex.IsMatch(input, pattern, RegexOptions.CultureInvariant | RegexOptions.Compiled | RegexOptions.IgnoreCase);
        }

        private bool ProcessConstraints(HttpContextBase httpContext, RouteValueDictionary values, RouteDirection routeDirection)
        {
            if (this.Constraints != null)
            {
                foreach (KeyValuePair<string, object> pair in this.Constraints)
                {
                    if (!this.ProcessConstraint(httpContext, pair.Value, pair.Key, values, routeDirection))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public RouteValueDictionary Constraints { get; set; }

        public RouteValueDictionary DataTokens { get; set; }

        public RouteValueDictionary Defaults { get; set; }

        public IRouteHandler RouteHandler { get; set; }

        public string Url
        {
            get
            {
                return (this._url ?? string.Empty);
            }
            set
            {
                this._parsedRoute = RouteParser.Parse(value);
                this._url = value;
            }
        }
    }
}

