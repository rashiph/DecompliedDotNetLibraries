namespace System.Web.Configuration
{
    using System;
    using System.Configuration;
    using System.Security.Permissions;
    using System.Web;
    using System.Web.Compilation;

    internal class HandlerFactoryCache
    {
        private IHttpHandlerFactory _factory;

        internal HandlerFactoryCache(string type)
        {
            object obj2 = this.Create(type);
            if (obj2 is IHttpHandler)
            {
                this._factory = new HandlerFactoryWrapper((IHttpHandler) obj2, this.GetHandlerType(type));
            }
            else
            {
                if (!(obj2 is IHttpHandlerFactory))
                {
                    throw new HttpException(System.Web.SR.GetString("Type_not_factory_or_handler", new object[] { obj2.GetType().FullName }));
                }
                this._factory = (IHttpHandlerFactory) obj2;
            }
        }

        internal HandlerFactoryCache(HttpHandlerAction mapping)
        {
            object obj2 = mapping.Create();
            if (obj2 is IHttpHandler)
            {
                this._factory = new HandlerFactoryWrapper((IHttpHandler) obj2, this.GetHandlerType(mapping));
            }
            else
            {
                if (!(obj2 is IHttpHandlerFactory))
                {
                    throw new HttpException(System.Web.SR.GetString("Type_not_factory_or_handler", new object[] { obj2.GetType().FullName }));
                }
                this._factory = (IHttpHandlerFactory) obj2;
            }
        }

        internal object Create(string type)
        {
            return HttpRuntime.CreateNonPublicInstance(this.GetHandlerType(type));
        }

        internal Type GetHandlerType(string type)
        {
            Type typeWithAssert = this.GetTypeWithAssert(type);
            HttpRuntime.FailIfNoAPTCABit(typeWithAssert, null, null);
            if (!ConfigUtil.IsTypeHandlerOrFactory(typeWithAssert))
            {
                throw new ConfigurationErrorsException(System.Web.SR.GetString("Type_not_factory_or_handler", new object[] { type }));
            }
            return typeWithAssert;
        }

        internal Type GetHandlerType(HttpHandlerAction handlerAction)
        {
            Type typeWithAssert = this.GetTypeWithAssert(handlerAction.Type);
            if (!ConfigUtil.IsTypeHandlerOrFactory(typeWithAssert))
            {
                throw new ConfigurationErrorsException(System.Web.SR.GetString("Type_not_factory_or_handler", new object[] { handlerAction.Type }), handlerAction.ElementInformation.Source, handlerAction.ElementInformation.LineNumber);
            }
            return typeWithAssert;
        }

        [FileIOPermission(SecurityAction.Assert, AllFiles=FileIOPermissionAccess.PathDiscovery | FileIOPermissionAccess.Read)]
        private Type GetTypeWithAssert(string type)
        {
            bool throwOnError = true;
            bool ignoreCase = false;
            return BuildManager.GetType(type, throwOnError, ignoreCase);
        }

        internal IHttpHandlerFactory Factory
        {
            get
            {
                return this._factory;
            }
        }
    }
}

