namespace System.Web.Hosting
{
    using System;
    using System.Configuration;
    using System.Web;
    using System.Web.Util;

    internal sealed class PreloadHost : MarshalByRefObject, IRegisteredObject
    {
        public PreloadHost()
        {
            HostingEnvironment.RegisterObject(this);
        }

        public void CreateIProcessHostPreloadClientInstanceAndCallPreload(string preloadObjTypeName, string[] paramsForStartupObj)
        {
            using (new ApplicationImpersonationContext())
            {
                Type c = null;
                try
                {
                    c = Type.GetType(preloadObjTypeName, true);
                }
                catch (Exception exception)
                {
                    throw new InvalidOperationException(Misc.FormatExceptionMessage(exception, new string[] { System.Web.SR.GetString("Failure_Create_Application_Preload_Provider_Type", new object[] { preloadObjTypeName }) }));
                }
                if (!typeof(IProcessHostPreloadClient).IsAssignableFrom(c))
                {
                    throw new ConfigurationErrorsException(System.Web.SR.GetString("Invalid_Application_Preload_Provider_Type", new object[] { preloadObjTypeName }));
                }
                ((IProcessHostPreloadClient) Activator.CreateInstance(c)).Preload(paramsForStartupObj);
            }
        }

        public override object InitializeLifetimeService()
        {
            return null;
        }

        void IRegisteredObject.Stop(bool immediate)
        {
            HostingEnvironment.UnregisterObject(this);
        }

        internal Exception InitializationException
        {
            get
            {
                return HttpRuntime.InitializationException;
            }
        }
    }
}

