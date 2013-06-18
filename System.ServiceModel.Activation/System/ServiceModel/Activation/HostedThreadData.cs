namespace System.ServiceModel.Activation
{
    using System;
    using System.Globalization;
    using System.Runtime.Remoting.Messaging;
    using System.Security;
    using System.Security.Permissions;
    using System.Threading;
    using System.Web;

    [SecurityCritical(SecurityCriticalScope.Everything)]
    internal class HostedThreadData
    {
        private CultureInfo cultureInfo = CultureInfo.CurrentCulture;
        private HttpContext httpContext = HttpContext.Current;
        private CultureInfo uiCultureInfo = CultureInfo.CurrentUICulture;

        public IDisposable CreateContext()
        {
            return new HostedAspNetContext(this);
        }

        [SecurityPermission(SecurityAction.Assert, Unrestricted=true)]
        private static void UnsafeApplyData(HostedThreadData data)
        {
            CallContext.HostContext = data.httpContext;
            Thread currentThread = Thread.CurrentThread;
            if (currentThread.CurrentCulture != data.cultureInfo)
            {
                currentThread.CurrentCulture = data.cultureInfo;
            }
            if (currentThread.CurrentUICulture != data.uiCultureInfo)
            {
                currentThread.CurrentUICulture = data.uiCultureInfo;
            }
        }

        private class HostedAspNetContext : IDisposable
        {
            private HostedThreadData oldData = new HostedThreadData();

            public HostedAspNetContext(HostedThreadData newData)
            {
                HostedThreadData.UnsafeApplyData(newData);
            }

            public void Dispose()
            {
                HostedThreadData.UnsafeApplyData(this.oldData);
            }
        }
    }
}

