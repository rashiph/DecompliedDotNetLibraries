namespace System.Web.Hosting
{
    using System;
    using System.Runtime.InteropServices;

    public sealed class AppDomainFactory : IAppDomainFactory
    {
        private AppManagerAppDomainFactory _realFactory = new AppManagerAppDomainFactory();

        [return: MarshalAs(UnmanagedType.Interface)]
        public object Create(string module, string typeName, string appId, string appPath, string strUrlOfAppOrigin, int iZone)
        {
            return this._realFactory.Create(appId, appPath);
        }
    }
}

