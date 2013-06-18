namespace System.Xaml.Hosting
{
    using System;
    using System.Collections;
    using System.Configuration;
    using System.Reflection;
    using System.Web;
    using System.Web.Compilation;
    using System.Web.Hosting;
    using System.Xaml.Hosting.Configuration;

    internal sealed class XamlHttpHandlerFactory : IHttpHandlerFactory
    {
        private static object CreateInstance(Type type)
        {
            return Activator.CreateInstance(type, BindingFlags.CreateInstance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, null, null);
        }

        public IHttpHandler GetHandler(HttpContext context, string requestType, string url, string pathTranslated)
        {
            return PathCache.EnsurePathInfo(context.Request.AppRelativeCurrentExecutionFilePath).GetHandler(context, requestType, url, pathTranslated);
        }

        public void ReleaseHandler(IHttpHandler httphandler)
        {
            if (httphandler is HandlerWrapper)
            {
                ((HandlerWrapper) httphandler).ReleaseWrappedHandler();
            }
        }

        private class HandlerWrapper : IHttpHandler
        {
            private IHttpHandlerFactory factory;
            private IHttpHandler httpHandler;

            private HandlerWrapper(IHttpHandler httpHandler, IHttpHandlerFactory factory)
            {
                this.httpHandler = httpHandler;
                this.factory = factory;
            }

            public static IHttpHandler Create(IHttpHandler httpHandler, IHttpHandlerFactory factory)
            {
                if (httpHandler is IHttpAsyncHandler)
                {
                    return new AsyncHandlerWrapper((IHttpAsyncHandler) httpHandler, factory);
                }
                return new XamlHttpHandlerFactory.HandlerWrapper(httpHandler, factory);
            }

            public void ProcessRequest(HttpContext context)
            {
                this.httpHandler.ProcessRequest(context);
            }

            public void ReleaseWrappedHandler()
            {
                this.factory.ReleaseHandler(this.httpHandler);
            }

            public bool IsReusable
            {
                get
                {
                    return this.httpHandler.IsReusable;
                }
            }

            private class AsyncHandlerWrapper : XamlHttpHandlerFactory.HandlerWrapper, IHttpAsyncHandler, IHttpHandler
            {
                private IHttpAsyncHandler httpAsyncHandler;

                public AsyncHandlerWrapper(IHttpAsyncHandler httpAsyncHandler, IHttpHandlerFactory factory) : base(httpAsyncHandler, factory)
                {
                    this.httpAsyncHandler = httpAsyncHandler;
                }

                public IAsyncResult BeginProcessRequest(HttpContext context, AsyncCallback cb, object extraData)
                {
                    return this.httpAsyncHandler.BeginProcessRequest(context, cb, extraData);
                }

                public void EndProcessRequest(IAsyncResult result)
                {
                    this.httpAsyncHandler.EndProcessRequest(result);
                }
            }
        }

        private static class PathCache
        {
            private static Hashtable pathCache = new Hashtable(StringComparer.OrdinalIgnoreCase);
            private static object writeLock = new object();

            public static XamlHttpHandlerFactory.PathInfo EnsurePathInfo(string path)
            {
                XamlHttpHandlerFactory.PathInfo info = (XamlHttpHandlerFactory.PathInfo) pathCache[path];
                if (info != null)
                {
                    return info;
                }
                lock (writeLock)
                {
                    info = (XamlHttpHandlerFactory.PathInfo) pathCache[path];
                    if (info == null)
                    {
                        if (!HostingEnvironment.VirtualPathProvider.FileExists(path))
                        {
                            throw FxTrace.Exception.AsError(new HttpException(0x194, System.Xaml.Hosting.SR.ResourceNotFound));
                        }
                        info = new XamlHttpHandlerFactory.PathInfo();
                        pathCache.Add(path, info);
                    }
                    return info;
                }
            }
        }

        private class PathInfo
        {
            private object cachedResult;
            private Type hostedXamlType;
            private object writeLock = new object();

            private Type GetCompiledCustomString(string normalizedVirtualPath)
            {
                Type compiledType;
                try
                {
                    using (IDisposable disposable = null)
                    {
                        try
                        {
                        }
                        finally
                        {
                            disposable = HostingEnvironmentWrapper.UnsafeImpersonate();
                        }
                        compiledType = BuildManager.GetCompiledType(normalizedVirtualPath);
                    }
                }
                catch
                {
                    throw;
                }
                return compiledType;
            }

            public IHttpHandler GetHandler(HttpContext context, string requestType, string url, string pathTranslated)
            {
                if (this.cachedResult == null)
                {
                    lock (this.writeLock)
                    {
                        if (this.cachedResult == null)
                        {
                            return this.GetHandlerFirstTime(context, requestType, url, pathTranslated);
                        }
                    }
                }
                return this.GetHandlerSubSequent(context, requestType, url, pathTranslated);
            }

            private IHttpHandler GetHandlerFirstTime(HttpContext context, string requestType, string url, string pathTranslated)
            {
                Type type;
                ConfigurationErrorsException exception;
                if (this.hostedXamlType == null)
                {
                    this.hostedXamlType = this.GetCompiledCustomString(context.Request.AppRelativeCurrentExecutionFilePath);
                }
                if (XamlHostingConfiguration.TryGetHttpHandlerType(url, this.hostedXamlType, out type))
                {
                    if (TD.HttpHandlerPickedForUrlIsEnabled())
                    {
                        TD.HttpHandlerPickedForUrl(url, this.hostedXamlType.FullName, type.FullName);
                    }
                    if (typeof(IHttpHandler).IsAssignableFrom(type))
                    {
                        IHttpHandler handler = (IHttpHandler) XamlHttpHandlerFactory.CreateInstance(type);
                        if (handler.IsReusable)
                        {
                            this.cachedResult = handler;
                            return handler;
                        }
                        this.cachedResult = type;
                        return handler;
                    }
                    if (typeof(IHttpHandlerFactory).IsAssignableFrom(type))
                    {
                        IHttpHandlerFactory factory = (IHttpHandlerFactory) XamlHttpHandlerFactory.CreateInstance(type);
                        this.cachedResult = factory;
                        return XamlHttpHandlerFactory.HandlerWrapper.Create(factory.GetHandler(context, requestType, url, pathTranslated), factory);
                    }
                    exception = new ConfigurationErrorsException(System.Xaml.Hosting.SR.NotHttpHandlerType(url, this.hostedXamlType, type.FullName));
                    this.cachedResult = exception;
                    throw FxTrace.Exception.AsError(exception);
                }
                exception = new ConfigurationErrorsException(System.Xaml.Hosting.SR.HttpHandlerForXamlTypeNotFound(url, this.hostedXamlType, "system.xaml.hosting/httpHandlers"));
                this.cachedResult = exception;
                throw FxTrace.Exception.AsError(exception);
            }

            private IHttpHandler GetHandlerSubSequent(HttpContext context, string requestType, string url, string pathTranslated)
            {
                if (this.cachedResult is IHttpHandler)
                {
                    return (IHttpHandler) this.cachedResult;
                }
                if (this.cachedResult is IHttpHandlerFactory)
                {
                    IHttpHandlerFactory cachedResult = (IHttpHandlerFactory) this.cachedResult;
                    return XamlHttpHandlerFactory.HandlerWrapper.Create(cachedResult.GetHandler(context, requestType, url, pathTranslated), cachedResult);
                }
                if (!(this.cachedResult is Type))
                {
                    throw FxTrace.Exception.AsError((ConfigurationErrorsException) this.cachedResult);
                }
                return (IHttpHandler) XamlHttpHandlerFactory.CreateInstance((Type) this.cachedResult);
            }
        }
    }
}

