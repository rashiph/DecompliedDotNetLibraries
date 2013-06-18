namespace System.Web.Hosting
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting;
    using System.Security.Permissions;
    using System.Web.Util;

    public sealed class AppManagerAppDomainFactory : IAppManagerAppDomainFactory
    {
        private ApplicationManager _appManager = ApplicationManager.GetApplicationManager();

        [SecurityPermission(SecurityAction.Demand, Unrestricted=true)]
        public AppManagerAppDomainFactory()
        {
            this._appManager.Open();
        }

        internal static string ConstructSimpleAppName(string virtPath)
        {
            if (virtPath.Length <= 1)
            {
                return "root";
            }
            return virtPath.Substring(1).ToLower(CultureInfo.InvariantCulture).Replace('/', '_');
        }

        [return: MarshalAs(UnmanagedType.Interface)]
        public object Create(string appId, string appPath)
        {
            object obj2;
            try
            {
                if (appPath[0] == '.')
                {
                    FileInfo info = new FileInfo(appPath);
                    appPath = info.FullName;
                }
                if (!StringUtil.StringEndsWith(appPath, '\\'))
                {
                    appPath = appPath + @"\";
                }
                ISAPIApplicationHost appHost = new ISAPIApplicationHost(appId, appPath, false);
                ISAPIRuntime o = (ISAPIRuntime) this._appManager.CreateObjectInternal(appId, typeof(ISAPIRuntime), appHost, false, null);
                o.StartProcessing();
                obj2 = new ObjectHandle(o);
            }
            catch (Exception)
            {
                throw;
            }
            return obj2;
        }

        public void Stop()
        {
            this._appManager.Close();
        }
    }
}

