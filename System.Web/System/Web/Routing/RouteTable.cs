namespace System.Web.Routing
{
    using System;
    using System.Runtime.CompilerServices;

    [TypeForwardedFrom("System.Web.Routing, Version=3.5.0.0, Culture=Neutral, PublicKeyToken=31bf3856ad364e35")]
    public class RouteTable
    {
        private static RouteCollection _instance = new RouteCollection();

        public static RouteCollection Routes
        {
            get
            {
                return _instance;
            }
        }
    }
}

