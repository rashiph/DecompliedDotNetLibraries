namespace System.Net
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net.Cache;
    using System.Net.Configuration;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Text;
    using System.Threading;

    internal sealed class NetWebProxyFinder : BaseWebProxyFinder
    {
        private volatile bool aborted;
        private static readonly WaitCallback abortWrapper = new WaitCallback(NetWebProxyFinder.AbortWrapper);
        private RequestCache backupCache;
        private Uri engineScriptLocation;
        private object lockObject;
        private const int MaximumProxyStringLength = 0x80a;
        private volatile WebRequest request;
        private bool scriptDetectionFailed;
        private AutoWebProxyScriptWrapper scriptInstance;
        private Uri scriptLocation;
        private static readonly char[] splitChars = new char[] { ';' };
        private static readonly TimerThread.Callback timerCallback = new TimerThread.Callback(NetWebProxyFinder.RequestTimeoutCallback);
        private static TimerThread.Queue timerQueue;

        public NetWebProxyFinder(AutoWebProxyScriptEngine engine) : base(engine)
        {
            this.backupCache = new SingleItemRequestCache(RequestCacheManager.IsCachingEnabled);
            this.lockObject = new object();
        }

        public override void Abort()
        {
            lock (this.lockObject)
            {
                this.aborted = true;
                if (this.request != null)
                {
                    ThreadPool.UnsafeQueueUserWorkItem(abortWrapper, this.request);
                }
            }
        }

        private static void AbortWrapper(object context)
        {
            if (context != null)
            {
                ((WebRequest) context).Abort();
            }
        }

        private void DetectScriptLocation()
        {
            if (!this.scriptDetectionFailed && (this.scriptLocation == null))
            {
                this.scriptLocation = SafeDetectAutoProxyUrl(UnsafeNclNativeMethods.WinHttp.AutoDetectType.Dhcp);
                if (this.scriptLocation == null)
                {
                    this.scriptLocation = SafeDetectAutoProxyUrl(UnsafeNclNativeMethods.WinHttp.AutoDetectType.DnsA);
                }
                if (this.scriptLocation == null)
                {
                    this.scriptDetectionFailed = true;
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.scriptInstance != null))
            {
                this.scriptInstance.Close();
            }
        }

        private BaseWebProxyFinder.AutoWebProxyState DownloadAndCompile(Uri location)
        {
            BaseWebProxyFinder.AutoWebProxyState downloadFailure = BaseWebProxyFinder.AutoWebProxyState.DownloadFailure;
            WebResponse response = null;
            TimerThread.Timer timer = null;
            AutoWebProxyScriptWrapper scriptInstance = null;
            ExceptionHelper.WebPermissionUnrestricted.Assert();
            try
            {
                lock (this.lockObject)
                {
                    if (this.aborted)
                    {
                        throw new WebException(NetRes.GetWebStatusString("net_requestaborted", WebExceptionStatus.RequestCanceled), WebExceptionStatus.RequestCanceled);
                    }
                    this.request = WebRequest.Create(location);
                }
                this.request.Timeout = -1;
                this.request.CachePolicy = new RequestCachePolicy(RequestCacheLevel.Default);
                this.request.ConnectionGroupName = "__WebProxyScript";
                if (this.request.CacheProtocol != null)
                {
                    this.request.CacheProtocol = new RequestCacheProtocol(this.backupCache, this.request.CacheProtocol.Validator);
                }
                HttpWebRequest request = this.request as HttpWebRequest;
                if (request != null)
                {
                    request.Accept = "*/*";
                    request.UserAgent = base.GetType().FullName + "/" + Environment.Version;
                    request.KeepAlive = false;
                    request.Pipelined = false;
                    request.InternalConnectionGroup = true;
                }
                else
                {
                    FtpWebRequest request2 = this.request as FtpWebRequest;
                    if (request2 != null)
                    {
                        request2.KeepAlive = false;
                    }
                }
                this.request.Proxy = null;
                this.request.Credentials = base.Engine.Credentials;
                if (timerQueue == null)
                {
                    timerQueue = TimerThread.GetOrCreateQueue(SettingsSectionInternal.Section.DownloadTimeout);
                }
                timer = timerQueue.CreateTimer(timerCallback, this.request);
                response = this.request.GetResponse();
                DateTime minValue = DateTime.MinValue;
                HttpWebResponse response2 = response as HttpWebResponse;
                if (response2 != null)
                {
                    minValue = response2.LastModified;
                }
                else
                {
                    FtpWebResponse response3 = response as FtpWebResponse;
                    if (response3 != null)
                    {
                        minValue = response3.LastModified;
                    }
                }
                if (((this.scriptInstance != null) && (minValue != DateTime.MinValue)) && (this.scriptInstance.LastModified == minValue))
                {
                    scriptInstance = this.scriptInstance;
                    downloadFailure = BaseWebProxyFinder.AutoWebProxyState.Completed;
                }
                else
                {
                    string scriptBody = null;
                    byte[] buffer = null;
                    using (Stream stream = response.GetResponseStream())
                    {
                        SingleItemRequestCache.ReadOnlyStream stream2 = stream as SingleItemRequestCache.ReadOnlyStream;
                        if (stream2 != null)
                        {
                            buffer = stream2.Buffer;
                        }
                        if (((this.scriptInstance != null) && (buffer != null)) && (buffer == this.scriptInstance.Buffer))
                        {
                            this.scriptInstance.LastModified = minValue;
                            scriptInstance = this.scriptInstance;
                            downloadFailure = BaseWebProxyFinder.AutoWebProxyState.Completed;
                        }
                        else
                        {
                            using (StreamReader reader = new StreamReader(stream))
                            {
                                scriptBody = reader.ReadToEnd();
                            }
                        }
                    }
                    WebResponse response4 = response;
                    response = null;
                    response4.Close();
                    timer.Cancel();
                    timer = null;
                    if (downloadFailure != BaseWebProxyFinder.AutoWebProxyState.Completed)
                    {
                        if ((this.scriptInstance != null) && (scriptBody == this.scriptInstance.ScriptBody))
                        {
                            this.scriptInstance.LastModified = minValue;
                            if (buffer != null)
                            {
                                this.scriptInstance.Buffer = buffer;
                            }
                            scriptInstance = this.scriptInstance;
                            downloadFailure = BaseWebProxyFinder.AutoWebProxyState.Completed;
                        }
                        else
                        {
                            scriptInstance = new AutoWebProxyScriptWrapper {
                                LastModified = minValue
                            };
                            if (scriptInstance.Compile(location, scriptBody, buffer))
                            {
                                downloadFailure = BaseWebProxyFinder.AutoWebProxyState.Completed;
                            }
                            else
                            {
                                downloadFailure = BaseWebProxyFinder.AutoWebProxyState.CompilationFailure;
                            }
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                if (Logging.On)
                {
                    Logging.PrintWarning(Logging.Web, SR.GetString("net_log_proxy_script_download_compile_error", new object[] { exception }));
                }
            }
            finally
            {
                if (timer != null)
                {
                    timer.Cancel();
                }
                try
                {
                    if (response != null)
                    {
                        response.Close();
                    }
                }
                finally
                {
                    CodeAccessPermission.RevertAssert();
                    this.request = null;
                }
            }
            if ((downloadFailure == BaseWebProxyFinder.AutoWebProxyState.Completed) && (this.scriptInstance != scriptInstance))
            {
                if (this.scriptInstance != null)
                {
                    this.scriptInstance.Close();
                }
                this.scriptInstance = scriptInstance;
            }
            return downloadFailure;
        }

        private void EnsureEngineAvailable()
        {
            if ((base.State == BaseWebProxyFinder.AutoWebProxyState.Uninitialized) || (this.engineScriptLocation == null))
            {
                if (base.Engine.AutomaticallyDetectSettings)
                {
                    this.DetectScriptLocation();
                    if (this.scriptLocation != null)
                    {
                        if (this.scriptLocation.Equals(this.engineScriptLocation))
                        {
                            base.State = BaseWebProxyFinder.AutoWebProxyState.Completed;
                            return;
                        }
                        if (this.DownloadAndCompile(this.scriptLocation) == BaseWebProxyFinder.AutoWebProxyState.Completed)
                        {
                            base.State = BaseWebProxyFinder.AutoWebProxyState.Completed;
                            this.engineScriptLocation = this.scriptLocation;
                            return;
                        }
                    }
                }
                if ((base.Engine.AutomaticConfigurationScript != null) && !this.aborted)
                {
                    if (base.Engine.AutomaticConfigurationScript.Equals(this.engineScriptLocation))
                    {
                        base.State = BaseWebProxyFinder.AutoWebProxyState.Completed;
                        return;
                    }
                    base.State = this.DownloadAndCompile(base.Engine.AutomaticConfigurationScript);
                    if (base.State == BaseWebProxyFinder.AutoWebProxyState.Completed)
                    {
                        this.engineScriptLocation = base.Engine.AutomaticConfigurationScript;
                        return;
                    }
                }
            }
            else
            {
                base.State = this.DownloadAndCompile(this.engineScriptLocation);
                if (base.State == BaseWebProxyFinder.AutoWebProxyState.Completed)
                {
                    return;
                }
                if (!this.engineScriptLocation.Equals(base.Engine.AutomaticConfigurationScript) && !this.aborted)
                {
                    base.State = this.DownloadAndCompile(base.Engine.AutomaticConfigurationScript);
                    if (base.State == BaseWebProxyFinder.AutoWebProxyState.Completed)
                    {
                        this.engineScriptLocation = base.Engine.AutomaticConfigurationScript;
                        return;
                    }
                }
            }
            base.State = BaseWebProxyFinder.AutoWebProxyState.DiscoveryFailure;
            if (this.scriptInstance != null)
            {
                this.scriptInstance.Close();
                this.scriptInstance = null;
            }
            this.engineScriptLocation = null;
        }

        public override bool GetProxies(Uri destination, out IList<string> proxyList)
        {
            bool flag2;
            try
            {
                proxyList = null;
                this.EnsureEngineAvailable();
                if (base.State != BaseWebProxyFinder.AutoWebProxyState.Completed)
                {
                    return false;
                }
                bool flag = false;
                try
                {
                    string scriptReturn = this.scriptInstance.FindProxyForURL(destination.ToString(), destination.Host);
                    proxyList = ParseScriptResult(scriptReturn);
                    flag = true;
                }
                catch (Exception exception)
                {
                    if (Logging.On)
                    {
                        Logging.PrintWarning(Logging.Web, SR.GetString("net_log_proxy_script_execution_error", new object[] { exception }));
                    }
                }
                flag2 = flag;
            }
            finally
            {
                this.aborted = false;
            }
            return flag2;
        }

        private static IList<string> ParseScriptResult(string scriptReturn)
        {
            IList<string> list = new List<string>();
            if (scriptReturn != null)
            {
                foreach (string str2 in scriptReturn.Split(splitChars))
                {
                    string str;
                    string strB = str2.Trim(new char[] { ' ' });
                    if (!strB.StartsWith("PROXY ", StringComparison.OrdinalIgnoreCase))
                    {
                        if (string.Compare("DIRECT", strB, StringComparison.OrdinalIgnoreCase) != 0)
                        {
                            continue;
                        }
                        str = null;
                    }
                    else
                    {
                        str = strB.Substring(6).TrimStart(new char[] { ' ' });
                        Uri result = null;
                        if (((!Uri.TryCreate("http://" + str, UriKind.Absolute, out result) || (result.UserInfo.Length > 0)) || ((result.HostNameType == UriHostNameType.Basic) || (result.AbsolutePath.Length != 1))) || (((str[str.Length - 1] == '/') || (str[str.Length - 1] == '#')) || (str[str.Length - 1] == '?')))
                        {
                            continue;
                        }
                    }
                    list.Add(str);
                }
            }
            return list;
        }

        private static void RequestTimeoutCallback(TimerThread.Timer timer, int timeNoticed, object context)
        {
            ThreadPool.UnsafeQueueUserWorkItem(abortWrapper, context);
        }

        private static unsafe Uri SafeDetectAutoProxyUrl(UnsafeNclNativeMethods.WinHttp.AutoDetectType discoveryMethod)
        {
            Uri result = null;
            string uriString = null;
            if (ComNetOS.IsWinHttp51)
            {
                SafeGlobalFree free;
                if (!UnsafeNclNativeMethods.WinHttp.WinHttpDetectAutoProxyConfigUrl(discoveryMethod, out free))
                {
                    if (free != null)
                    {
                        free.SetHandleAsInvalid();
                    }
                }
                else
                {
                    uriString = new string((char*) free.DangerousGetHandle());
                    free.Close();
                }
            }
            else
            {
                StringBuilder autoProxyUrl = new StringBuilder(0x80a);
                if (UnsafeNclNativeMethods.WinInet.DetectAutoProxyUrl(autoProxyUrl, 0x80a, (int) discoveryMethod))
                {
                    uriString = autoProxyUrl.ToString();
                }
            }
            if (uriString == null)
            {
                if (Logging.On)
                {
                    Logging.PrintWarning(Logging.Web, SR.GetString("net_log_proxy_autodetect_failed"));
                }
                return result;
            }
            if (!Uri.TryCreate(uriString, UriKind.Absolute, out result) && Logging.On)
            {
                Logging.PrintWarning(Logging.Web, SR.GetString("net_log_proxy_autodetect_script_location_parse_error", new object[] { ValidationHelper.ToString(uriString) }));
            }
            return result;
        }
    }
}

