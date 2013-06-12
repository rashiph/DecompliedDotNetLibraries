namespace System.Web.Hosting
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using System.Web;
    using System.Web.Configuration;
    using System.Web.Util;

    [ComVisible(false)]
    public class SimpleWorkerRequest : HttpWorkerRequest
    {
        private string _appPhysPath;
        private string _appVirtPath;
        private bool _hasRuntimeInfo;
        private string _installDir;
        private TextWriter _output;
        private string _page;
        private string _pathInfo;
        private string _queryString;

        private SimpleWorkerRequest()
        {
        }

        public SimpleWorkerRequest(string page, string query, TextWriter output) : this()
        {
            this._queryString = query;
            this._output = output;
            this._page = page;
            this.ExtractPagePathInfo();
            this._appPhysPath = Thread.GetDomain().GetData(".appPath").ToString();
            this._appVirtPath = Thread.GetDomain().GetData(".appVPath").ToString();
            this._installDir = HttpRuntime.AspInstallDirectoryInternal;
            this._hasRuntimeInfo = true;
        }

        public SimpleWorkerRequest(string appVirtualDir, string appPhysicalDir, string page, string query, TextWriter output) : this()
        {
            if (Thread.GetDomain().GetData(".appPath") != null)
            {
                throw new HttpException(System.Web.SR.GetString("Wrong_SimpleWorkerRequest"));
            }
            this._appVirtPath = appVirtualDir;
            this._appPhysPath = appPhysicalDir;
            this._queryString = query;
            this._output = output;
            this._page = page;
            this.ExtractPagePathInfo();
            if (!StringUtil.StringEndsWith(this._appPhysPath, '\\'))
            {
                this._appPhysPath = this._appPhysPath + @"\";
            }
            this._hasRuntimeInfo = false;
        }

        public override void EndOfRequest()
        {
        }

        private void ExtractPagePathInfo()
        {
            int index = this._page.IndexOf('/');
            if (index >= 0)
            {
                this._pathInfo = this._page.Substring(index);
                this._page = this._page.Substring(0, index);
            }
        }

        public override void FlushResponse(bool finalFlush)
        {
        }

        public override string GetAppPath()
        {
            return this._appVirtPath;
        }

        public override string GetAppPathTranslated()
        {
            InternalSecurityPermissions.PathDiscovery(this._appPhysPath).Demand();
            return this._appPhysPath;
        }

        public override string GetFilePath()
        {
            return this.GetPathInternal(false);
        }

        public override string GetFilePathTranslated()
        {
            string path = this._appPhysPath + this._page.Replace('/', '\\');
            InternalSecurityPermissions.PathDiscovery(path).Demand();
            return path;
        }

        public override string GetHttpVerbName()
        {
            return "GET";
        }

        public override string GetHttpVersion()
        {
            return "HTTP/1.0";
        }

        public override string GetLocalAddress()
        {
            return "127.0.0.1";
        }

        public override int GetLocalPort()
        {
            return 80;
        }

        public override string GetPathInfo()
        {
            if (this._pathInfo == null)
            {
                return string.Empty;
            }
            return this._pathInfo;
        }

        private string GetPathInternal(bool includePathInfo)
        {
            string str = this._appVirtPath.Equals("/") ? ("/" + this._page) : (this._appVirtPath + "/" + this._page);
            if (includePathInfo && (this._pathInfo != null))
            {
                return (str + this._pathInfo);
            }
            return str;
        }

        public override string GetQueryString()
        {
            return this._queryString;
        }

        public override string GetRawUrl()
        {
            string queryString = this.GetQueryString();
            if (!string.IsNullOrEmpty(queryString))
            {
                return (this.GetPathInternal(true) + "?" + queryString);
            }
            return this.GetPathInternal(true);
        }

        public override string GetRemoteAddress()
        {
            return "127.0.0.1";
        }

        public override int GetRemotePort()
        {
            return 0;
        }

        public override string GetServerVariable(string name)
        {
            return string.Empty;
        }

        public override string GetUriPath()
        {
            return this.GetPathInternal(true);
        }

        public override IntPtr GetUserToken()
        {
            return IntPtr.Zero;
        }

        public override string MapPath(string path)
        {
            if (!this._hasRuntimeInfo)
            {
                return null;
            }
            string str = null;
            string str2 = this._appPhysPath.Substring(0, this._appPhysPath.Length - 1);
            if (string.IsNullOrEmpty(path) || path.Equals("/"))
            {
                str = str2;
            }
            if (StringUtil.StringStartsWith(path, this._appVirtPath))
            {
                str = str2 + path.Substring(this._appVirtPath.Length).Replace('/', '\\');
            }
            InternalSecurityPermissions.PathDiscovery(str).Demand();
            return str;
        }

        public override void SendKnownResponseHeader(int index, string value)
        {
        }

        public override void SendResponseFromFile(IntPtr handle, long offset, long length)
        {
        }

        public override void SendResponseFromFile(string filename, long offset, long length)
        {
        }

        public override void SendResponseFromMemory(byte[] data, int length)
        {
            this._output.Write(Encoding.Default.GetChars(data, 0, length));
        }

        public override void SendStatus(int statusCode, string statusDescription)
        {
        }

        public override void SendUnknownResponseHeader(string name, string value)
        {
        }

        internal override void UpdateInitialCounters()
        {
            PerfCounters.IncrementGlobalCounter(GlobalPerfCounter.REQUESTS_CURRENT);
            PerfCounters.IncrementCounter(AppPerfCounter.REQUESTS_TOTAL);
        }

        internal override void UpdateRequestCounters(int bytesIn)
        {
            if (!HttpRuntime.UseIntegratedPipeline && (bytesIn > 0))
            {
                PerfCounters.IncrementCounterEx(AppPerfCounter.REQUEST_BYTES_IN, bytesIn);
            }
        }

        internal override void UpdateResponseCounters(bool finalFlush, int bytesOut)
        {
            if (!HttpRuntime.UseIntegratedPipeline)
            {
                if (finalFlush)
                {
                    PerfCounters.DecrementGlobalCounter(GlobalPerfCounter.REQUESTS_CURRENT);
                    PerfCounters.DecrementCounter(AppPerfCounter.REQUESTS_EXECUTING);
                }
                if (bytesOut > 0)
                {
                    PerfCounters.IncrementCounterEx(AppPerfCounter.REQUEST_BYTES_OUT, bytesOut);
                }
            }
        }

        public override string MachineConfigPath
        {
            get
            {
                if (this._hasRuntimeInfo)
                {
                    string machineConfigurationFilePath = HttpConfigurationSystem.MachineConfigurationFilePath;
                    InternalSecurityPermissions.PathDiscovery(machineConfigurationFilePath).Demand();
                    return machineConfigurationFilePath;
                }
                return null;
            }
        }

        public override string MachineInstallDirectory
        {
            get
            {
                if (this._hasRuntimeInfo)
                {
                    InternalSecurityPermissions.PathDiscovery(this._installDir).Demand();
                    return this._installDir;
                }
                return null;
            }
        }

        public override string RootWebConfigPath
        {
            get
            {
                if (this._hasRuntimeInfo)
                {
                    string rootWebConfigurationFilePath = HttpConfigurationSystem.RootWebConfigurationFilePath;
                    InternalSecurityPermissions.PathDiscovery(rootWebConfigurationFilePath).Demand();
                    return rootWebConfigurationFilePath;
                }
                return null;
            }
        }
    }
}

