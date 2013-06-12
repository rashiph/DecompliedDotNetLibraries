namespace System.Web
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.IO;
    using System.Security.Permissions;
    using System.Text;
    using System.Threading;
    using System.Web.Hosting;
    using System.Web.UI;
    using System.Web.Util;

    public sealed class HttpServerUtility
    {
        private HttpApplication _application;
        private HttpContext _context;
        private static IDictionary _cultureCache = Hashtable.Synchronized(new Hashtable());
        private static string _machineName;
        private static object _machineNameLock = new object();
        private const int _maxMachineNameLength = 0x100;

        internal HttpServerUtility(HttpApplication application)
        {
            this._application = application;
        }

        internal HttpServerUtility(HttpContext context)
        {
            this._context = context;
        }

        public void ClearError()
        {
            if (this._context != null)
            {
                this._context.ClearError();
            }
            else if (this._application != null)
            {
                this._application.ClearError();
            }
        }

        [SecurityPermission(SecurityAction.Demand, Unrestricted=true)]
        public object CreateObject(string progID)
        {
            Type typeFromProgID = null;
            object component = null;
            try
            {
                typeFromProgID = Type.GetTypeFromProgID(progID);
            }
            catch
            {
            }
            if (typeFromProgID == null)
            {
                throw new HttpException(System.Web.SR.GetString("Could_not_create_object_of_type", new object[] { progID }));
            }
            AspCompatApplicationStep.CheckThreadingModel(progID, typeFromProgID.GUID);
            component = Activator.CreateInstance(typeFromProgID);
            AspCompatApplicationStep.OnPageStart(component);
            return component;
        }

        [SecurityPermission(SecurityAction.Demand, UnmanagedCode=true)]
        public object CreateObject(Type type)
        {
            AspCompatApplicationStep.CheckThreadingModel(type.FullName, type.GUID);
            object component = Activator.CreateInstance(type);
            AspCompatApplicationStep.OnPageStart(component);
            return component;
        }

        [SecurityPermission(SecurityAction.Demand, UnmanagedCode=true)]
        public object CreateObjectFromClsid(string clsid)
        {
            object component = null;
            Guid guid = new Guid(clsid);
            AspCompatApplicationStep.CheckThreadingModel(clsid, guid);
            try
            {
                component = Activator.CreateInstance(Type.GetTypeFromCLSID(guid, null, true));
            }
            catch
            {
            }
            if (component == null)
            {
                throw new HttpException(System.Web.SR.GetString("Could_not_create_object_from_clsid", new object[] { clsid }));
            }
            AspCompatApplicationStep.OnPageStart(component);
            return component;
        }

        internal static CultureInfo CreateReadOnlyCultureInfo(int culture)
        {
            if (!_cultureCache.Contains(culture))
            {
                lock (_cultureCache)
                {
                    if (_cultureCache[culture] == null)
                    {
                        _cultureCache[culture] = CultureInfo.ReadOnly(new CultureInfo(culture));
                    }
                }
            }
            return (CultureInfo) _cultureCache[culture];
        }

        internal static CultureInfo CreateReadOnlyCultureInfo(string name)
        {
            if (!_cultureCache.Contains(name))
            {
                lock (_cultureCache)
                {
                    if (_cultureCache[name] == null)
                    {
                        _cultureCache[name] = CultureInfo.ReadOnly(new CultureInfo(name));
                    }
                }
            }
            return (CultureInfo) _cultureCache[name];
        }

        internal static CultureInfo CreateReadOnlySpecificCultureInfo(string name)
        {
            if (name.IndexOf('-') > 0)
            {
                return CreateReadOnlyCultureInfo(name);
            }
            CultureInfo ci = CultureInfo.CreateSpecificCulture(name);
            if (!_cultureCache.Contains(ci.Name))
            {
                lock (_cultureCache)
                {
                    if (_cultureCache[ci.Name] == null)
                    {
                        _cultureCache[ci.Name] = CultureInfo.ReadOnly(ci);
                    }
                }
            }
            return (CultureInfo) _cultureCache[ci.Name];
        }

        public void Execute(string path)
        {
            this.Execute(path, null, true);
        }

        public void Execute(string path, bool preserveForm)
        {
            this.Execute(path, null, preserveForm);
        }

        public void Execute(string path, TextWriter writer)
        {
            this.Execute(path, writer, true);
        }

        public void Execute(string path, TextWriter writer, bool preserveForm)
        {
            if (this._context == null)
            {
                throw new HttpException(System.Web.SR.GetString("Server_not_available"));
            }
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            string queryStringOverride = null;
            HttpRequest request = this._context.Request;
            path = this._context.Response.RemoveAppPathModifier(path);
            int index = path.IndexOf('?');
            if (index >= 0)
            {
                queryStringOverride = path.Substring(index + 1);
                path = path.Substring(0, index);
            }
            if (!UrlPath.IsValidVirtualPathWithoutProtocol(path))
            {
                throw new ArgumentException(System.Web.SR.GetString("Invalid_path_for_child_request", new object[] { path }));
            }
            VirtualPath virtualPath = VirtualPath.Create(path);
            IHttpHandler handler = null;
            string filename = request.MapPath(virtualPath);
            VirtualPath path3 = request.FilePathObject.Combine(virtualPath);
            InternalSecurityPermissions.FileReadAccess(filename).Demand();
            if (HttpRuntime.IsLegacyCas)
            {
                InternalSecurityPermissions.Unrestricted.Assert();
            }
            try
            {
                if (StringUtil.StringEndsWith(virtualPath.VirtualPathString, '.'))
                {
                    throw new HttpException(0x194, string.Empty);
                }
                bool useAppConfig = !path3.IsWithinAppRoot;
                using (new DisposableHttpContextWrapper(this._context))
                {
                    try
                    {
                        this._context.ServerExecuteDepth++;
                        if (this._context.WorkerRequest is IIS7WorkerRequest)
                        {
                            handler = this._context.ApplicationInstance.MapIntegratedHttpHandler(this._context, request.RequestType, path3, filename, useAppConfig, true);
                        }
                        else
                        {
                            handler = this._context.ApplicationInstance.MapHttpHandler(this._context, request.RequestType, path3, filename, useAppConfig);
                        }
                    }
                    finally
                    {
                        this._context.ServerExecuteDepth--;
                    }
                }
            }
            catch (Exception exception)
            {
                if (exception is HttpException)
                {
                    int httpCode = ((HttpException) exception).GetHttpCode();
                    if ((httpCode != 500) && (httpCode != 0x194))
                    {
                        exception = null;
                    }
                }
                throw new HttpException(System.Web.SR.GetString("Error_executing_child_request_for_path", new object[] { path }), exception);
            }
            this.ExecuteInternal(handler, writer, preserveForm, true, virtualPath, path3, filename, null, queryStringOverride);
        }

        public void Execute(IHttpHandler handler, TextWriter writer, bool preserveForm)
        {
            if (this._context == null)
            {
                throw new HttpException(System.Web.SR.GetString("Server_not_available"));
            }
            this.Execute(handler, writer, preserveForm, true);
        }

        internal void Execute(IHttpHandler handler, TextWriter writer, bool preserveForm, bool setPreviousPage)
        {
            HttpRequest request = this._context.Request;
            VirtualPath currentExecutionFilePathObject = request.CurrentExecutionFilePathObject;
            string physPath = request.MapPath(currentExecutionFilePathObject);
            this.ExecuteInternal(handler, writer, preserveForm, setPreviousPage, null, currentExecutionFilePathObject, physPath, null, null);
        }

        private void ExecuteInternal(IHttpHandler handler, TextWriter writer, bool preserveForm, bool setPreviousPage, VirtualPath path, VirtualPath filePath, string physPath, Exception error, string queryStringOverride)
        {
            if (handler == null)
            {
                throw new ArgumentNullException("handler");
            }
            HttpRequest request = this._context.Request;
            HttpResponse response = this._context.Response;
            HttpApplication applicationInstance = this._context.ApplicationInstance;
            HttpValueCollection form = null;
            VirtualPath path2 = null;
            string queryStringText = null;
            TextWriter writer2 = null;
            AspNetSynchronizationContext syncContext = null;
            this.VerifyTransactionFlow(handler);
            this._context.PushTraceContext();
            this._context.SetCurrentHandler(handler);
            bool enabled = this._context.SyncContext.Enabled;
            this._context.SyncContext.Disable();
            try
            {
                try
                {
                    this._context.ServerExecuteDepth++;
                    path2 = request.SwitchCurrentExecutionFilePath(filePath);
                    if (!preserveForm)
                    {
                        form = request.SwitchForm(new HttpValueCollection());
                        if (queryStringOverride == null)
                        {
                            queryStringOverride = string.Empty;
                        }
                    }
                    if (queryStringOverride != null)
                    {
                        queryStringText = request.QueryStringText;
                        request.QueryStringText = queryStringOverride;
                    }
                    if (writer != null)
                    {
                        writer2 = response.SwitchWriter(writer);
                    }
                    Page page = handler as Page;
                    if (page != null)
                    {
                        if (setPreviousPage)
                        {
                            page.SetPreviousPage(this._context.PreviousHandler as Page);
                        }
                        Page page2 = this._context.Handler as Page;
                        if ((page2 != null) && page2.SmartNavigation)
                        {
                            page.SmartNavigation = true;
                        }
                        if (page is IHttpAsyncHandler)
                        {
                            syncContext = this._context.InstallNewAspNetSynchronizationContext();
                        }
                    }
                    if (((handler is StaticFileHandler) || (handler is DefaultHttpHandler)) && !DefaultHttpHandler.IsClassicAspRequest(filePath.VirtualPathString))
                    {
                        try
                        {
                            response.WriteFile(physPath);
                        }
                        catch
                        {
                            error = new HttpException(0x194, string.Empty);
                        }
                    }
                    else if (!(handler is Page))
                    {
                        error = new HttpException(0x194, string.Empty);
                    }
                    else
                    {
                        if (handler is IHttpAsyncHandler)
                        {
                            bool isInCancellablePeriod = this._context.IsInCancellablePeriod;
                            if (isInCancellablePeriod)
                            {
                                this._context.EndCancellablePeriod();
                            }
                            try
                            {
                                IHttpAsyncHandler handler2 = (IHttpAsyncHandler) handler;
                                IAsyncResult result = handler2.BeginProcessRequest(this._context, null, null);
                                if (!result.IsCompleted)
                                {
                                    bool flag3 = false;
                                    try
                                    {
                                        try
                                        {
                                        }
                                        finally
                                        {
                                            Monitor.Exit(applicationInstance);
                                            flag3 = true;
                                        }
                                        WaitHandle asyncWaitHandle = result.AsyncWaitHandle;
                                        if (asyncWaitHandle == null)
                                        {
                                            goto Label_0210;
                                        }
                                        asyncWaitHandle.WaitOne();
                                        goto Label_0226;
                                    Label_020A:
                                        Thread.Sleep(1);
                                    Label_0210:
                                        if (!result.IsCompleted)
                                        {
                                            goto Label_020A;
                                        }
                                    }
                                    finally
                                    {
                                        if (flag3)
                                        {
                                            Monitor.Enter(applicationInstance);
                                        }
                                    }
                                }
                            Label_0226:
                                try
                                {
                                    handler2.EndProcessRequest(result);
                                }
                                catch (Exception exception)
                                {
                                    error = exception;
                                }
                                goto Label_0306;
                            }
                            finally
                            {
                                if (isInCancellablePeriod)
                                {
                                    this._context.BeginCancellablePeriod();
                                }
                            }
                        }
                        using (new DisposableHttpContextWrapper(this._context))
                        {
                            try
                            {
                                handler.ProcessRequest(this._context);
                            }
                            catch (Exception exception2)
                            {
                                error = exception2;
                            }
                        }
                    }
                }
                finally
                {
                    this._context.ServerExecuteDepth--;
                    this._context.RestoreCurrentHandler();
                    if (writer2 != null)
                    {
                        response.SwitchWriter(writer2);
                    }
                    if ((queryStringOverride != null) && (queryStringText != null))
                    {
                        request.QueryStringText = queryStringText;
                    }
                    if (form != null)
                    {
                        request.SwitchForm(form);
                    }
                    request.SwitchCurrentExecutionFilePath(path2);
                    if (syncContext != null)
                    {
                        this._context.RestoreSavedAspNetSynchronizationContext(syncContext);
                    }
                    if (enabled)
                    {
                        this._context.SyncContext.Enable();
                    }
                    this._context.PopTraceContext();
                }
            }
            catch
            {
                throw;
            }
        Label_0306:
            if (error == null)
            {
                return;
            }
            if ((error is HttpException) && (((HttpException) error).GetHttpCode() != 500))
            {
                error = null;
            }
            if (path != null)
            {
                throw new HttpException(System.Web.SR.GetString("Error_executing_child_request_for_path", new object[] { path }), error);
            }
            throw new HttpException(System.Web.SR.GetString("Error_executing_child_request_for_handler", new object[] { handler.GetType().ToString() }), error);
        }

        internal static void ExecuteLocalRequestAndCaptureResponse(string path, TextWriter writer, ErrorFormatterGenerator errorFormatterGenerator)
        {
            HttpRequest request = new HttpRequest(VirtualPath.CreateAbsolute(path), string.Empty);
            HttpResponse response = new HttpResponse(writer);
            HttpContext context = new HttpContext(request, response);
            HttpApplication applicationInstance = HttpApplicationFactory.GetApplicationInstance(context) as HttpApplication;
            context.ApplicationInstance = applicationInstance;
            try
            {
                context.Server.Execute(path);
            }
            catch (HttpException exception)
            {
                if (errorFormatterGenerator != null)
                {
                    context.Response.SetOverrideErrorFormatter(errorFormatterGenerator.GetErrorFormatter(exception));
                }
                context.Response.ReportRuntimeError(exception, false, true);
            }
            finally
            {
                if (applicationInstance != null)
                {
                    context.ApplicationInstance = null;
                    HttpApplicationFactory.RecycleApplicationInstance(applicationInstance);
                }
            }
        }

        public Exception GetLastError()
        {
            if (this._context != null)
            {
                return this._context.Error;
            }
            if (this._application != null)
            {
                return this._application.LastError;
            }
            return null;
        }

        internal static string GetMachineNameInternal()
        {
            if (_machineName == null)
            {
                lock (_machineNameLock)
                {
                    if (_machineName != null)
                    {
                        return _machineName;
                    }
                    StringBuilder nameBuffer = new StringBuilder(0x100);
                    int bufferSize = 0x100;
                    if (System.Web.UnsafeNativeMethods.GetComputerName(nameBuffer, ref bufferSize) == 0)
                    {
                        throw new HttpException(System.Web.SR.GetString("Get_computer_name_failed"));
                    }
                    _machineName = nameBuffer.ToString();
                }
            }
            return _machineName;
        }

        public string HtmlDecode(string s)
        {
            return HttpUtility.HtmlDecode(s);
        }

        public void HtmlDecode(string s, TextWriter output)
        {
            HttpUtility.HtmlDecode(s, output);
        }

        public string HtmlEncode(string s)
        {
            return HttpUtility.HtmlEncode(s);
        }

        public void HtmlEncode(string s, TextWriter output)
        {
            HttpUtility.HtmlEncode(s, output);
        }

        public string MapPath(string path)
        {
            string str;
            if (this._context == null)
            {
                throw new HttpException(System.Web.SR.GetString("Server_not_available"));
            }
            bool hideRequestResponse = this._context.HideRequestResponse;
            try
            {
                if (hideRequestResponse)
                {
                    this._context.HideRequestResponse = false;
                }
                str = this._context.Request.MapPath(path);
            }
            finally
            {
                if (hideRequestResponse)
                {
                    this._context.HideRequestResponse = true;
                }
            }
            return str;
        }

        public void Transfer(string path)
        {
            bool preventPostback = this._context.PreventPostback;
            this._context.PreventPostback = true;
            this.Transfer(path, true);
            this._context.PreventPostback = preventPostback;
        }

        public void Transfer(string path, bool preserveForm)
        {
            Page handler = this._context.Handler as Page;
            if ((handler != null) && handler.IsCallback)
            {
                throw new ApplicationException(System.Web.SR.GetString("Transfer_not_allowed_in_callback"));
            }
            this.Execute(path, null, preserveForm);
            this._context.Response.End();
        }

        public void Transfer(IHttpHandler handler, bool preserveForm)
        {
            Page page = handler as Page;
            if ((page != null) && page.IsCallback)
            {
                throw new ApplicationException(System.Web.SR.GetString("Transfer_not_allowed_in_callback"));
            }
            this.Execute(handler, null, preserveForm);
            this._context.Response.End();
        }

        public void TransferRequest(string path)
        {
            this.TransferRequest(path, false, null, null);
        }

        public void TransferRequest(string path, bool preserveForm)
        {
            this.TransferRequest(path, preserveForm, null, null);
        }

        public void TransferRequest(string path, bool preserveForm, string method, NameValueCollection headers)
        {
            if (!HttpRuntime.UseIntegratedPipeline)
            {
                throw new PlatformNotSupportedException(System.Web.SR.GetString("Requires_Iis_Integrated_Mode"));
            }
            if (this._context == null)
            {
                throw new HttpException(System.Web.SR.GetString("Server_not_available"));
            }
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            IIS7WorkerRequest workerRequest = this._context.WorkerRequest as IIS7WorkerRequest;
            HttpRequest request = this._context.Request;
            HttpResponse response = this._context.Response;
            if (workerRequest == null)
            {
                throw new HttpException(System.Web.SR.GetString("Server_not_available"));
            }
            path = response.RemoveAppPathModifier(path);
            string queryString = null;
            int index = path.IndexOf('?');
            if (index >= 0)
            {
                queryString = (index < (path.Length - 1)) ? path.Substring(index + 1) : string.Empty;
                path = path.Substring(0, index);
            }
            if (!UrlPath.IsValidVirtualPathWithoutProtocol(path))
            {
                throw new ArgumentException(System.Web.SR.GetString("Invalid_path_for_child_request", new object[] { path }));
            }
            VirtualPath path2 = request.FilePathObject.Combine(VirtualPath.Create(path));
            bool preserveUser = true;
            workerRequest.ScheduleExecuteUrl(path2.VirtualPathString, queryString, method, preserveForm, preserveForm ? request.EntityBody : null, headers, preserveUser);
            this._context.ApplicationInstance.EnsureReleaseState();
            this._context.ApplicationInstance.CompleteRequest();
        }

        public string UrlDecode(string s)
        {
            Encoding e = (this._context != null) ? this._context.Request.ContentEncoding : Encoding.UTF8;
            return HttpUtility.UrlDecode(s, e);
        }

        public void UrlDecode(string s, TextWriter output)
        {
            if (s != null)
            {
                output.Write(this.UrlDecode(s));
            }
        }

        public string UrlEncode(string s)
        {
            Encoding e = (this._context != null) ? this._context.Response.ContentEncoding : Encoding.UTF8;
            return HttpUtility.UrlEncode(s, e);
        }

        public void UrlEncode(string s, TextWriter output)
        {
            if (s != null)
            {
                output.Write(this.UrlEncode(s));
            }
        }

        public string UrlPathEncode(string s)
        {
            return HttpUtility.UrlPathEncode(s);
        }

        public static byte[] UrlTokenDecode(string input)
        {
            return HttpEncoder.Current.UrlTokenDecode(input);
        }

        public static string UrlTokenEncode(byte[] input)
        {
            return HttpEncoder.Current.UrlTokenEncode(input);
        }

        private void VerifyTransactionFlow(IHttpHandler handler)
        {
            Page page = this._context.Handler as Page;
            Page page2 = handler as Page;
            if ((((page2 != null) && page2.IsInAspCompatMode) && ((page != null) && !page.IsInAspCompatMode)) && Transactions.Utils.IsInTransaction)
            {
                throw new HttpException(System.Web.SR.GetString("Transacted_page_calls_aspcompat"));
            }
        }

        public string MachineName
        {
            [AspNetHostingPermission(SecurityAction.Demand, Level=AspNetHostingPermissionLevel.Medium)]
            get
            {
                return GetMachineNameInternal();
            }
        }

        public int ScriptTimeout
        {
            get
            {
                if (this._context != null)
                {
                    return Convert.ToInt32(this._context.Timeout.TotalSeconds, CultureInfo.InvariantCulture);
                }
                return 110;
            }
            [AspNetHostingPermission(SecurityAction.Demand, Level=AspNetHostingPermissionLevel.Medium)]
            set
            {
                if (this._context == null)
                {
                    throw new HttpException(System.Web.SR.GetString("Server_not_available"));
                }
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                this._context.Timeout = new TimeSpan(0, 0, value);
            }
        }
    }
}

