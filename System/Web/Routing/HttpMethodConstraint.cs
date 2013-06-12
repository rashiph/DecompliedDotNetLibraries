namespace System.Web.Routing
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Web;

    [TypeForwardedFrom("System.Web.Routing, Version=3.5.0.0, Culture=Neutral, PublicKeyToken=31bf3856ad364e35")]
    public class HttpMethodConstraint : IRouteConstraint
    {
        public HttpMethodConstraint(params string[] allowedMethods)
        {
            if (allowedMethods == null)
            {
                throw new ArgumentNullException("allowedMethods");
            }
            this.AllowedMethods = allowedMethods.ToList<string>().AsReadOnly();
        }

        protected virtual bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection)
        {
            Func<string, bool> predicate = null;
            if (httpContext == null)
            {
                throw new ArgumentNullException("httpContext");
            }
            if (route == null)
            {
                throw new ArgumentNullException("route");
            }
            if (parameterName == null)
            {
                throw new ArgumentNullException("parameterName");
            }
            if (values == null)
            {
                throw new ArgumentNullException("values");
            }
            string parameterValueString;
            switch (routeDirection)
            {
                case RouteDirection.IncomingRequest:
                    if (predicate == null)
                    {
                        predicate = method => string.Equals(method, httpContext.Request.HttpMethod, StringComparison.OrdinalIgnoreCase);
                    }
                    return this.AllowedMethods.Any<string>(predicate);

                case RouteDirection.UrlGeneration:
                    object obj2;
                    if (values.TryGetValue(parameterName, out obj2))
                    {
                        parameterValueString = obj2 as string;
                        if (parameterValueString == null)
                        {
                            throw new InvalidOperationException(string.Format(CultureInfo.CurrentUICulture, System.Web.SR.GetString("HttpMethodConstraint_ParameterValueMustBeString"), new object[] { parameterName, route.Url }));
                        }
                        return this.AllowedMethods.Any<string>(method => string.Equals(method, parameterValueString, StringComparison.OrdinalIgnoreCase));
                    }
                    return true;
            }
            return true;
        }

        bool IRouteConstraint.Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection)
        {
            return this.Match(httpContext, route, parameterName, values, routeDirection);
        }

        public ICollection<string> AllowedMethods { get; private set; }
    }
}

