namespace System.Net
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    internal sealed class WinHttpWebProxyBuilder : WebProxyDataBuilder
    {
        protected override void BuildInternal()
        {
            UnsafeNclNativeMethods.WinHttp.WINHTTP_CURRENT_USER_IE_PROXY_CONFIG proxyConfig = new UnsafeNclNativeMethods.WinHttp.WINHTTP_CURRENT_USER_IE_PROXY_CONFIG();
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                if (UnsafeNclNativeMethods.WinHttp.WinHttpGetIEProxyConfigForCurrentUser(ref proxyConfig))
                {
                    string addressString = null;
                    string bypassListString = null;
                    string autoConfigUrl = null;
                    addressString = Marshal.PtrToStringUni(proxyConfig.Proxy);
                    bypassListString = Marshal.PtrToStringUni(proxyConfig.ProxyBypass);
                    autoConfigUrl = Marshal.PtrToStringUni(proxyConfig.AutoConfigUrl);
                    base.SetProxyAndBypassList(addressString, bypassListString);
                    base.SetAutoDetectSettings(proxyConfig.AutoDetect);
                    base.SetAutoProxyUrl(autoConfigUrl);
                }
                else
                {
                    if (Marshal.GetLastWin32Error() == 8)
                    {
                        throw new OutOfMemoryException();
                    }
                    base.SetAutoDetectSettings(true);
                }
            }
            finally
            {
                Marshal.FreeHGlobal(proxyConfig.Proxy);
                Marshal.FreeHGlobal(proxyConfig.ProxyBypass);
                Marshal.FreeHGlobal(proxyConfig.AutoConfigUrl);
            }
        }
    }
}

