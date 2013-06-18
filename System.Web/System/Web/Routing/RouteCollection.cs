namespace System.Web.Routing
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Web;
    using System.Web.Hosting;

    [TypeForwardedFrom("System.Web.Routing, Version=3.5.0.0, Culture=Neutral, PublicKeyToken=31bf3856ad364e35")]
    public class RouteCollection : Collection<RouteBase>
    {
        private Dictionary<string, RouteBase> _namedMap;
        private ReaderWriterLockSlim _rwLock;
        private VirtualPathProvider _vpp;

        public RouteCollection() : this(HostingEnvironment.VirtualPathProvider)
        {
        }

        public RouteCollection(VirtualPathProvider virtualPathProvider)
        {
            this._namedMap = new Dictionary<string, RouteBase>(StringComparer.OrdinalIgnoreCase);
            this._rwLock = new ReaderWriterLockSlim();
            this._vpp = virtualPathProvider;
        }

        public void Add(string name, RouteBase item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }
            if (!string.IsNullOrEmpty(name) && this._namedMap.ContainsKey(name))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentUICulture, System.Web.SR.GetString("RouteCollection_DuplicateName"), new object[] { name }), "name");
            }
            base.Add(item);
            if (!string.IsNullOrEmpty(name))
            {
                this._namedMap[name] = item;
            }
        }

        protected override void ClearItems()
        {
            this._namedMap.Clear();
            base.ClearItems();
        }

        public IDisposable GetReadLock()
        {
            this._rwLock.EnterReadLock();
            return new ReadLockDisposable(this._rwLock);
        }

        private RequestContext GetRequestContext(RequestContext requestContext)
        {
            if (requestContext != null)
            {
                return requestContext;
            }
            HttpContext current = HttpContext.Current;
            if (current == null)
            {
                throw new InvalidOperationException(System.Web.SR.GetString("RouteCollection_RequiresContext"));
            }
            return new RequestContext(new HttpContextWrapper(current), new RouteData());
        }

        public RouteData GetRouteData(HttpContextBase httpContext)
        {
            if (httpContext == null)
            {
                throw new ArgumentNullException("httpContext");
            }
            if (httpContext.Request == null)
            {
                throw new ArgumentException(System.Web.SR.GetString("RouteTable_ContextMissingRequest"), "httpContext");
            }
            if (base.Count != 0)
            {
                if (!this.RouteExistingFiles)
                {
                    string appRelativeCurrentExecutionFilePath = httpContext.Request.AppRelativeCurrentExecutionFilePath;
                    if (((appRelativeCurrentExecutionFilePath != "~/") && (this._vpp != null)) && (this._vpp.FileExists(appRelativeCurrentExecutionFilePath) || this._vpp.DirectoryExists(appRelativeCurrentExecutionFilePath)))
                    {
                        return null;
                    }
                }
                using (this.GetReadLock())
                {
                    foreach (RouteBase base2 in this)
                    {
                        RouteData routeData = base2.GetRouteData(httpContext);
                        if (routeData != null)
                        {
                            return routeData;
                        }
                    }
                }
            }
            return null;
        }

        private static string GetUrlWithApplicationPath(RequestContext requestContext, string url)
        {
            string str = requestContext.HttpContext.Request.ApplicationPath ?? string.Empty;
            if (!str.EndsWith("/", StringComparison.OrdinalIgnoreCase))
            {
                str = str + "/";
            }
            return requestContext.HttpContext.Response.ApplyAppPathModifier(str + url);
        }

        public VirtualPathData GetVirtualPath(RequestContext requestContext, RouteValueDictionary values)
        {
            requestContext = this.GetRequestContext(requestContext);
            using (this.GetReadLock())
            {
                foreach (RouteBase base2 in this)
                {
                    VirtualPathData virtualPath = base2.GetVirtualPath(requestContext, values);
                    if (virtualPath != null)
                    {
                        virtualPath.VirtualPath = GetUrlWithApplicationPath(requestContext, virtualPath.VirtualPath);
                        return virtualPath;
                    }
                }
            }
            return null;
        }

        public VirtualPathData GetVirtualPath(RequestContext requestContext, string name, RouteValueDictionary values)
        {
            RouteBase base2;
            bool flag;
            requestContext = this.GetRequestContext(requestContext);
            if (string.IsNullOrEmpty(name))
            {
                return this.GetVirtualPath(requestContext, values);
            }
            using (this.GetReadLock())
            {
                flag = this._namedMap.TryGetValue(name, out base2);
            }
            if (!flag)
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentUICulture, System.Web.SR.GetString("RouteCollection_NameNotFound"), new object[] { name }), "name");
            }
            VirtualPathData virtualPath = base2.GetVirtualPath(requestContext, values);
            if (virtualPath == null)
            {
                return null;
            }
            virtualPath.VirtualPath = GetUrlWithApplicationPath(requestContext, virtualPath.VirtualPath);
            return virtualPath;
        }

        public IDisposable GetWriteLock()
        {
            this._rwLock.EnterWriteLock();
            return new WriteLockDisposable(this._rwLock);
        }

        public void Ignore(string url)
        {
            this.Ignore(url, null);
        }

        public void Ignore(string url, object constraints)
        {
            if (url == null)
            {
                throw new ArgumentNullException("url");
            }
            IgnoreRouteInternal item = new IgnoreRouteInternal(url) {
                Constraints = new RouteValueDictionary(constraints)
            };
            base.Add(item);
        }

        protected override void InsertItem(int index, RouteBase item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }
            if (base.Contains(item))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentUICulture, System.Web.SR.GetString("RouteCollection_DuplicateEntry"), new object[0]), "item");
            }
            base.InsertItem(index, item);
        }

        public Route MapPageRoute(string routeName, string routeUrl, string physicalFile)
        {
            return this.MapPageRoute(routeName, routeUrl, physicalFile, true, null, null, null);
        }

        public Route MapPageRoute(string routeName, string routeUrl, string physicalFile, bool checkPhysicalUrlAccess)
        {
            return this.MapPageRoute(routeName, routeUrl, physicalFile, checkPhysicalUrlAccess, null, null, null);
        }

        public Route MapPageRoute(string routeName, string routeUrl, string physicalFile, bool checkPhysicalUrlAccess, RouteValueDictionary defaults)
        {
            return this.MapPageRoute(routeName, routeUrl, physicalFile, checkPhysicalUrlAccess, defaults, null, null);
        }

        public Route MapPageRoute(string routeName, string routeUrl, string physicalFile, bool checkPhysicalUrlAccess, RouteValueDictionary defaults, RouteValueDictionary constraints)
        {
            return this.MapPageRoute(routeName, routeUrl, physicalFile, checkPhysicalUrlAccess, defaults, constraints, null);
        }

        public Route MapPageRoute(string routeName, string routeUrl, string physicalFile, bool checkPhysicalUrlAccess, RouteValueDictionary defaults, RouteValueDictionary constraints, RouteValueDictionary dataTokens)
        {
            if (routeUrl == null)
            {
                throw new ArgumentNullException("routeUrl");
            }
            Route item = new Route(routeUrl, defaults, constraints, dataTokens, new PageRouteHandler(physicalFile, checkPhysicalUrlAccess));
            this.Add(routeName, item);
            return item;
        }

        protected override void RemoveItem(int index)
        {
            this.RemoveRouteName(index);
            base.RemoveItem(index);
        }

        private void RemoveRouteName(int index)
        {
            RouteBase base2 = base[index];
            foreach (KeyValuePair<string, RouteBase> pair in this._namedMap)
            {
                if (pair.Value == base2)
                {
                    this._namedMap.Remove(pair.Key);
                    break;
                }
            }
        }

        protected override void SetItem(int index, RouteBase item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }
            if (base.Contains(item))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentUICulture, System.Web.SR.GetString("RouteCollection_DuplicateEntry"), new object[0]), "item");
            }
            this.RemoveRouteName(index);
            base.SetItem(index, item);
        }

        public RouteBase this[string name]
        {
            get
            {
                RouteBase base2;
                if (!string.IsNullOrEmpty(name) && this._namedMap.TryGetValue(name, out base2))
                {
                    return base2;
                }
                return null;
            }
        }

        public bool RouteExistingFiles { get; set; }

        private sealed class IgnoreRouteInternal : Route
        {
            public IgnoreRouteInternal(string url) : base(url, new StopRoutingHandler())
            {
            }

            public override VirtualPathData GetVirtualPath(RequestContext requestContext, RouteValueDictionary routeValues)
            {
                return null;
            }
        }

        private class ReadLockDisposable : IDisposable
        {
            private ReaderWriterLockSlim _rwLock;

            public ReadLockDisposable(ReaderWriterLockSlim rwLock)
            {
                this._rwLock = rwLock;
            }

            void IDisposable.Dispose()
            {
                this._rwLock.ExitReadLock();
            }
        }

        private class WriteLockDisposable : IDisposable
        {
            private ReaderWriterLockSlim _rwLock;

            public WriteLockDisposable(ReaderWriterLockSlim rwLock)
            {
                this._rwLock = rwLock;
            }

            void IDisposable.Dispose()
            {
                this._rwLock.ExitWriteLock();
            }
        }
    }
}

