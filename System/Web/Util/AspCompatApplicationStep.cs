namespace System.Web.Util
{
    using Microsoft.Win32;
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting.Messaging;
    using System.Security.Permissions;
    using System.Text;
    using System.Threading;
    using System.Web;
    using System.Web.Caching;
    using System.Web.SessionState;

    internal class AspCompatApplicationStep : HttpApplication.IExecutionStep, IManagedContext
    {
        private HttpApplication _app;
        private HttpAsyncResult _ar;
        private AspCompatCallback _code;
        private EventArgs _codeEventArgs;
        private EventHandler _codeEventHandler;
        private object _codeEventSource;
        private WorkItemCallback _compCallback;
        private HttpContext _context;
        private Exception _error;
        private AspCompatCallback _execCallback;
        private GCHandle _rootedThis;
        private string _sessionId;
        private ArrayList _staComponents;
        private bool _syncCaller;
        private static char[] TabOrBackSpace = new char[] { '\t', '\b' };

        internal AspCompatApplicationStep(HttpContext context, AspCompatCallback code)
        {
            this._code = code;
            this.Init(context, context.ApplicationInstance);
        }

        private AspCompatApplicationStep(HttpContext context, HttpApplication app, string sessionId, EventHandler codeEventHandler, object codeEventSource, EventArgs codeEventArgs)
        {
            this._codeEventHandler = codeEventHandler;
            this._codeEventSource = codeEventSource;
            this._codeEventArgs = codeEventArgs;
            this._sessionId = sessionId;
            this.Init(context, app);
        }

        internal static bool AnyStaObjectsInSessionState(HttpSessionState session)
        {
            if (session != null)
            {
                int count = session.Count;
                for (int i = 0; i < count; i++)
                {
                    object obj2 = session[i];
                    if (((obj2 != null) && (obj2.GetType().FullName == "System.__ComObject")) && (System.Web.UnsafeNativeMethods.AspCompatIsApartmentComponent(obj2) != 0))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        [SecurityPermission(SecurityAction.Demand, UnmanagedCode=true)]
        internal IAsyncResult BeginAspCompatExecution(AsyncCallback cb, object extraData)
        {
            if (IsInAspCompatMode)
            {
                bool completedSynchronously = true;
                Exception error = this._app.ExecuteStep(this, ref completedSynchronously);
                this._ar = new HttpAsyncResult(cb, extraData, true, null, error);
                this._syncCaller = true;
            }
            else
            {
                this._ar = new HttpAsyncResult(cb, extraData);
                this._syncCaller = cb == null;
                this._rootedThis = GCHandle.Alloc(this);
                bool sharedActivity = this._sessionId != null;
                int activityHash = sharedActivity ? this._sessionId.GetHashCode() : 0;
                if (System.Web.UnsafeNativeMethods.AspCompatProcessRequest(this._execCallback, this, sharedActivity, activityHash) != 1)
                {
                    this._rootedThis.Free();
                    this._ar.Complete(true, null, new HttpException(System.Web.SR.GetString("Cannot_access_AspCompat")));
                }
            }
            return this._ar;
        }

        internal static void CheckThreadingModel(string progidDisplayName, Guid clsid)
        {
            if (!IsInAspCompatMode)
            {
                CacheInternal cacheInternal = HttpRuntime.CacheInternal;
                string str = "s" + progidDisplayName;
                string str2 = (string) cacheInternal.Get(str);
                RegistryKey key = null;
                if (str2 == null)
                {
                    try
                    {
                        key = Registry.ClassesRoot.OpenSubKey(@"CLSID\{" + clsid + @"}\InprocServer32");
                        if (key != null)
                        {
                            str2 = (string) key.GetValue("ThreadingModel");
                        }
                    }
                    catch
                    {
                    }
                    finally
                    {
                        if (key != null)
                        {
                            key.Close();
                        }
                    }
                    if (str2 == null)
                    {
                        str2 = string.Empty;
                    }
                    cacheInternal.UtcInsert(str, str2);
                }
                if (StringUtil.EqualsIgnoreCase(str2, "Apartment"))
                {
                    throw new HttpException(System.Web.SR.GetString("Apartment_component_not_allowed", new object[] { progidDisplayName }));
                }
            }
        }

        private static string CollectionToString(NameValueCollection c)
        {
            int count = c.Count;
            if (count == 0)
            {
                return string.Empty;
            }
            StringBuilder builder = new StringBuilder(0x100);
            for (int i = 0; i < count; i++)
            {
                string str = EncodeTab(c.GetKey(i));
                string[] values = c.GetValues(i);
                int num3 = (values != null) ? values.Length : 0;
                builder.Append(string.Concat(new object[] { str, "\t", num3, "\t" }));
                for (int j = 0; j < num3; j++)
                {
                    builder.Append(EncodeTab(values[j]));
                    if (j < (values.Length - 1))
                    {
                        builder.Append("\t");
                    }
                }
                if (i < (count - 1))
                {
                    builder.Append("\t");
                }
            }
            return builder.ToString();
        }

        private static string CookiesToString(HttpCookieCollection cc)
        {
            StringBuilder builder = new StringBuilder(0x100);
            StringBuilder builder2 = new StringBuilder(0x80);
            int count = cc.Count;
            builder.Append(count.ToString() + "\t");
            for (int i = 0; i < count; i++)
            {
                HttpCookie cookie = cc[i];
                string str = EncodeTab(cookie.Name);
                string str2 = EncodeTab(cookie.Value);
                builder.Append(str + "\t" + str2 + "\t");
                if (i > 0)
                {
                    builder2.Append(";" + str + "=" + str2);
                }
                else
                {
                    builder2.Append(str + "=" + str2);
                }
                NameValueCollection values = cookie.Values;
                int num3 = values.Count;
                bool flag = false;
                if (values.HasKeys())
                {
                    for (int j = 0; j < num3; j++)
                    {
                        if (!string.IsNullOrEmpty(values.GetKey(j)))
                        {
                            flag = true;
                            break;
                        }
                    }
                }
                if (flag)
                {
                    builder.Append(num3 + "\t");
                    for (int k = 0; k < num3; k++)
                    {
                        builder.Append(EncodeTab(values.GetKey(k)) + "\t" + EncodeTab(values.Get(k)) + "\t");
                    }
                }
                else
                {
                    builder.Append("0\t");
                }
            }
            builder2.Append("\t");
            builder2.Append(builder.ToString());
            return builder2.ToString();
        }

        private static string DictEnumKeysToString(IDictionaryEnumerator e)
        {
            StringBuilder builder = new StringBuilder(0x100);
            if (e.MoveNext())
            {
                builder.Append(EncodeTab(e.Key));
                while (e.MoveNext())
                {
                    builder.Append("\t");
                    builder.Append(EncodeTab(e.Key));
                }
            }
            return builder.ToString();
        }

        private static string EncodeTab(object value)
        {
            return EncodeTab((string) value);
        }

        private static string EncodeTab(string value)
        {
            if (!string.IsNullOrEmpty(value) && (value.IndexOfAny(TabOrBackSpace) >= 0))
            {
                return value.Replace("\b", "\bB").Replace("\t", "\bT");
            }
            return value;
        }

        internal void EndAspCompatExecution(IAsyncResult ar)
        {
            this._ar.End();
        }

        private static string EnumKeysToString(IEnumerator e)
        {
            StringBuilder builder = new StringBuilder(0x100);
            if (e.MoveNext())
            {
                builder.Append(EncodeTab(e.Current));
                while (e.MoveNext())
                {
                    builder.Append("\t");
                    builder.Append(EncodeTab(e.Current));
                }
            }
            return builder.ToString();
        }

        private void ExecuteAspCompatCode()
        {
            this.MarkCallContext(this);
            try
            {
                bool completedSynchronously = true;
                if (this._context != null)
                {
                    HttpApplication.ThreadContext context = null;
                    try
                    {
                        context = this._app.OnThreadEnter();
                        this._error = this._app.ExecuteStep(this, ref completedSynchronously);
                        return;
                    }
                    finally
                    {
                        if (context != null)
                        {
                            context.Leave();
                        }
                    }
                }
                this._error = this._app.ExecuteStep(this, ref completedSynchronously);
            }
            finally
            {
                this.MarkCallContext(null);
            }
        }

        private void Init(HttpContext context, HttpApplication app)
        {
            this._context = context;
            this._app = app;
            this._execCallback = new AspCompatCallback(this.OnAspCompatExecution);
            this._compCallback = new WorkItemCallback(this.OnAspCompatCompletion);
            if (((this._sessionId == null) && (this._context != null)) && (this._context.Session != null))
            {
                this._sessionId = this._context.Session.SessionID;
            }
        }

        private bool IsStaComponentInSessionState(object component)
        {
            if (this._context != null)
            {
                HttpSessionState session = this._context.Session;
                if (session == null)
                {
                    return false;
                }
                int count = session.Count;
                for (int i = 0; i < count; i++)
                {
                    if (component == session[i])
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private void MarkCallContext(AspCompatApplicationStep mark)
        {
            CallContext.SetData("AspCompat", mark);
        }

        private void OnAspCompatCompletion()
        {
            this._rootedThis.Free();
            this._ar.Complete(false, null, this._error);
        }

        private void OnAspCompatExecution()
        {
            try
            {
                if (this._syncCaller)
                {
                    this.ExecuteAspCompatCode();
                }
                else
                {
                    lock (this._app)
                    {
                        this.ExecuteAspCompatCode();
                    }
                }
            }
            finally
            {
                System.Web.UnsafeNativeMethods.AspCompatOnPageEnd();
                if (this._staComponents != null)
                {
                    foreach (object obj2 in this._staComponents)
                    {
                        if (!this.IsStaComponentInSessionState(obj2))
                        {
                            Marshal.ReleaseComObject(obj2);
                        }
                    }
                }
                this._ar.SetComplete();
                WorkItem.PostInternal(this._compCallback);
            }
        }

        internal static void OnPageStart(object component)
        {
            if (IsInAspCompatMode)
            {
                if (System.Web.UnsafeNativeMethods.AspCompatOnPageStart(component) != 1)
                {
                    throw new HttpException(System.Web.SR.GetString("Error_onpagestart"));
                }
                if (System.Web.UnsafeNativeMethods.AspCompatIsApartmentComponent(component) != 0)
                {
                    Current.RememberStaComponent(component);
                }
            }
        }

        internal static void OnPageStartSessionObjects()
        {
            if (IsInAspCompatMode)
            {
                HttpContext context = Current._context;
                if (context != null)
                {
                    HttpSessionState session = context.Session;
                    if (session != null)
                    {
                        int count = session.Count;
                        for (int i = 0; i < count; i++)
                        {
                            object obj2 = session[i];
                            if (((obj2 != null) && !(obj2 is string)) && (System.Web.UnsafeNativeMethods.AspCompatOnPageStart(obj2) != 1))
                            {
                                throw new HttpException(System.Web.SR.GetString("Error_onpagestart"));
                            }
                        }
                    }
                }
            }
        }

        internal static void RaiseAspCompatEvent(HttpContext context, HttpApplication app, string sessionId, EventHandler eventHandler, object source, EventArgs eventArgs)
        {
            AspCompatApplicationStep step = new AspCompatApplicationStep(context, app, sessionId, eventHandler, source, eventArgs);
            IAsyncResult ar = step.BeginAspCompatExecution(null, null);
            if (!ar.IsCompleted)
            {
                WaitHandle asyncWaitHandle = ar.AsyncWaitHandle;
                if (asyncWaitHandle == null)
                {
                    while (!ar.IsCompleted)
                    {
                        Thread.Sleep(1);
                    }
                }
                else
                {
                    asyncWaitHandle.WaitOne();
                }
            }
            step.EndAspCompatExecution(ar);
        }

        private void RememberStaComponent(object component)
        {
            if (this._staComponents == null)
            {
                this._staComponents = new ArrayList();
            }
            this._staComponents.Add(component);
        }

        private static string StringArrayToString(string[] ss)
        {
            StringBuilder builder = new StringBuilder(0x100);
            if (ss != null)
            {
                for (int i = 0; i < ss.Length; i++)
                {
                    if (i > 0)
                    {
                        builder.Append("\t");
                    }
                    builder.Append(EncodeTab(ss[i]));
                }
            }
            return builder.ToString();
        }

        void HttpApplication.IExecutionStep.Execute()
        {
            if (this._code != null)
            {
                this._code();
            }
            else if (this._codeEventHandler != null)
            {
                this._codeEventHandler(this._codeEventSource, this._codeEventArgs);
            }
        }

        string IManagedContext.Application_GetContentsNames()
        {
            return StringArrayToString(this._context.Application.AllKeys);
        }

        object IManagedContext.Application_GetContentsObject(string name)
        {
            return this._context.Application[name];
        }

        string IManagedContext.Application_GetStaticNames()
        {
            return DictEnumKeysToString((IDictionaryEnumerator) this._context.Application.StaticObjects.GetEnumerator());
        }

        object IManagedContext.Application_GetStaticObject(string name)
        {
            return this._context.Application.StaticObjects[name];
        }

        void IManagedContext.Application_Lock()
        {
            this._context.Application.Lock();
        }

        void IManagedContext.Application_RemoveAllContentsObjects()
        {
            this._context.Application.RemoveAll();
        }

        void IManagedContext.Application_RemoveContentsObject(string name)
        {
            this._context.Application.Remove(name);
        }

        void IManagedContext.Application_SetContentsObject(string name, object obj)
        {
            this._context.Application[name] = obj;
        }

        void IManagedContext.Application_UnLock()
        {
            this._context.Application.UnLock();
        }

        int IManagedContext.Context_IsPresent()
        {
            if (this._context == null)
            {
                return 0;
            }
            return 1;
        }

        int IManagedContext.Request_BinaryRead(byte[] bytes, int size)
        {
            return this._context.Request.InputStream.Read(bytes, 0, size);
        }

        string IManagedContext.Request_GetAsString(int what)
        {
            string str = string.Empty;
            switch (((RequestString) what))
            {
                case RequestString.QueryString:
                    return CollectionToString(this._context.Request.QueryString);

                case RequestString.Form:
                    return CollectionToString(this._context.Request.Form);

                case RequestString.Cookies:
                    return string.Empty;

                case RequestString.ServerVars:
                    return CollectionToString(this._context.Request.ServerVariables);
            }
            return str;
        }

        string IManagedContext.Request_GetCookiesAsString()
        {
            return CookiesToString(this._context.Request.Cookies);
        }

        int IManagedContext.Request_GetTotalBytes()
        {
            return this._context.Request.TotalBytes;
        }

        void IManagedContext.Response_AddCookie(string name)
        {
            this._context.Response.Cookies.Add(new HttpCookie(name));
        }

        void IManagedContext.Response_AddHeader(string name, string value)
        {
            this._context.Response.AppendHeader(name, value);
        }

        void IManagedContext.Response_AppendToLog(string entry)
        {
            this._context.Response.AppendToLog(entry);
        }

        void IManagedContext.Response_BinaryWrite(byte[] bytes, int size)
        {
            this._context.Response.OutputStream.Write(bytes, 0, size);
        }

        void IManagedContext.Response_Clear()
        {
            this._context.Response.Clear();
        }

        void IManagedContext.Response_End()
        {
            this._context.Response.End();
        }

        void IManagedContext.Response_Flush()
        {
            this._context.Response.Flush();
        }

        string IManagedContext.Response_GetCacheControl()
        {
            return this._context.Response.CacheControl;
        }

        string IManagedContext.Response_GetCharSet()
        {
            return this._context.Response.Charset;
        }

        string IManagedContext.Response_GetContentType()
        {
            return this._context.Response.ContentType;
        }

        string IManagedContext.Response_GetCookiesAsString()
        {
            return CookiesToString(this._context.Response.Cookies);
        }

        double IManagedContext.Response_GetExpiresAbsolute()
        {
            return this._context.Response.ExpiresAbsolute.ToOADate();
        }

        int IManagedContext.Response_GetExpiresMinutes()
        {
            return this._context.Response.Expires;
        }

        int IManagedContext.Response_GetIsBuffering()
        {
            if (!this._context.Response.BufferOutput)
            {
                return 0;
            }
            return 1;
        }

        string IManagedContext.Response_GetStatus()
        {
            return this._context.Response.Status;
        }

        int IManagedContext.Response_IsClientConnected()
        {
            if (!this._context.Response.IsClientConnected)
            {
                return 0;
            }
            return 1;
        }

        void IManagedContext.Response_Pics(string value)
        {
            this._context.Response.Pics(value);
        }

        void IManagedContext.Response_Redirect(string url)
        {
            this._context.Response.Redirect(url);
        }

        void IManagedContext.Response_SetCacheControl(string cacheControl)
        {
            this._context.Response.CacheControl = cacheControl;
        }

        void IManagedContext.Response_SetCharSet(string charSet)
        {
            this._context.Response.Charset = charSet;
        }

        void IManagedContext.Response_SetContentType(string contentType)
        {
            this._context.Response.ContentType = contentType;
        }

        void IManagedContext.Response_SetCookieDomain(string name, string domain)
        {
            this._context.Response.Cookies[name].Domain = domain;
        }

        void IManagedContext.Response_SetCookieExpires(string name, double dtExpires)
        {
            this._context.Response.Cookies[name].Expires = DateTime.FromOADate(dtExpires);
        }

        void IManagedContext.Response_SetCookiePath(string name, string path)
        {
            this._context.Response.Cookies[name].Path = path;
        }

        void IManagedContext.Response_SetCookieSecure(string name, int secure)
        {
            this._context.Response.Cookies[name].Secure = secure != 0;
        }

        void IManagedContext.Response_SetCookieSubValue(string name, string key, string value)
        {
            this._context.Response.Cookies[name].Values[key] = value;
        }

        void IManagedContext.Response_SetCookieText(string name, string text)
        {
            this._context.Response.Cookies[name].Value = text;
        }

        void IManagedContext.Response_SetExpiresAbsolute(double dtExpires)
        {
            this._context.Response.ExpiresAbsolute = DateTime.FromOADate(dtExpires);
        }

        void IManagedContext.Response_SetExpiresMinutes(int expiresMinutes)
        {
            this._context.Response.Expires = expiresMinutes;
        }

        void IManagedContext.Response_SetIsBuffering(int isBuffering)
        {
            this._context.Response.BufferOutput = isBuffering != 0;
        }

        void IManagedContext.Response_SetStatus(string status)
        {
            this._context.Response.Status = status;
        }

        void IManagedContext.Response_Write(string text)
        {
            this._context.Response.Write(text);
        }

        object IManagedContext.Server_CreateObject(string progId)
        {
            return this._context.Server.CreateObject(progId);
        }

        void IManagedContext.Server_Execute(string url)
        {
            this._context.Server.Execute(url);
        }

        int IManagedContext.Server_GetScriptTimeout()
        {
            return this._context.Server.ScriptTimeout;
        }

        string IManagedContext.Server_HTMLEncode(string str)
        {
            return HttpUtility.HtmlEncode(str);
        }

        string IManagedContext.Server_MapPath(string logicalPath)
        {
            return this._context.Server.MapPath(logicalPath);
        }

        void IManagedContext.Server_SetScriptTimeout(int timeoutSeconds)
        {
            this._context.Server.ScriptTimeout = timeoutSeconds;
        }

        void IManagedContext.Server_Transfer(string url)
        {
            this._context.Server.Transfer(url);
        }

        string IManagedContext.Server_URLEncode(string str)
        {
            return this._context.Server.UrlEncode(str);
        }

        string IManagedContext.Server_URLPathEncode(string str)
        {
            return this._context.Server.UrlPathEncode(str);
        }

        void IManagedContext.Session_Abandon()
        {
            this._context.Session.Abandon();
        }

        int IManagedContext.Session_GetCodePage()
        {
            return this._context.Session.CodePage;
        }

        string IManagedContext.Session_GetContentsNames()
        {
            return EnumKeysToString(this._context.Session.GetEnumerator());
        }

        object IManagedContext.Session_GetContentsObject(string name)
        {
            return this._context.Session[name];
        }

        string IManagedContext.Session_GetID()
        {
            return this._context.Session.SessionID;
        }

        int IManagedContext.Session_GetLCID()
        {
            return this._context.Session.LCID;
        }

        string IManagedContext.Session_GetStaticNames()
        {
            return DictEnumKeysToString((IDictionaryEnumerator) this._context.Session.StaticObjects.GetEnumerator());
        }

        object IManagedContext.Session_GetStaticObject(string name)
        {
            return this._context.Session.StaticObjects[name];
        }

        int IManagedContext.Session_GetTimeout()
        {
            return this._context.Session.Timeout;
        }

        int IManagedContext.Session_IsPresent()
        {
            if (this._context.Session == null)
            {
                return 0;
            }
            return 1;
        }

        void IManagedContext.Session_RemoveAllContentsObjects()
        {
            this._context.Session.RemoveAll();
        }

        void IManagedContext.Session_RemoveContentsObject(string name)
        {
            this._context.Session.Remove(name);
        }

        void IManagedContext.Session_SetCodePage(int value)
        {
            this._context.Session.CodePage = value;
        }

        void IManagedContext.Session_SetContentsObject(string name, object obj)
        {
            this._context.Session[name] = obj;
        }

        void IManagedContext.Session_SetLCID(int value)
        {
            this._context.Session.LCID = value;
        }

        void IManagedContext.Session_SetTimeout(int value)
        {
            this._context.Session.Timeout = value;
        }

        private static AspCompatApplicationStep Current
        {
            get
            {
                return (AspCompatApplicationStep) CallContext.GetData("AspCompat");
            }
        }

        internal static bool IsInAspCompatMode
        {
            get
            {
                return (Current != null);
            }
        }

        bool HttpApplication.IExecutionStep.CompletedSynchronously
        {
            get
            {
                return true;
            }
        }

        bool HttpApplication.IExecutionStep.IsCancellable
        {
            get
            {
                return (this._context != null);
            }
        }
    }
}

