namespace System.Web.Services.Protocols
{
    using System;
    using System.IO;
    using System.Runtime;
    using System.Security.Permissions;
    using System.Text;
    using System.Threading;
    using System.Web;
    using System.Web.Caching;
    using System.Web.Services;

    [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust"), PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust")]
    public abstract class ServerProtocol
    {
        private HttpContext context;
        private WebMethodAttribute methodAttr;
        private HttpRequest request;
        private HttpResponse response;
        private static object s_InternalSyncObject;
        private object target;
        private System.Type type;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected ServerProtocol()
        {
        }

        protected void AddToCache(System.Type protocolType, System.Type serverType, object value)
        {
            HttpRuntime.Cache.Insert(this.CreateKey(protocolType, serverType), value, null, Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration, CacheItemPriority.NotRemovable, null);
        }

        private string CreateKey(System.Type protocolType, System.Type serverType)
        {
            string fullName = protocolType.FullName;
            string str2 = serverType.FullName;
            string str3 = serverType.TypeHandle.Value.ToString();
            string leftPart = this.Request.Url.GetLeftPart(UriPartial.Path);
            int capacity = ((fullName.Length + leftPart.Length) + str2.Length) + str3.Length;
            StringBuilder builder = new StringBuilder(capacity);
            builder.Append(fullName);
            builder.Append(leftPart);
            builder.Append(str2);
            builder.Append(str3);
            return builder.ToString();
        }

        internal virtual void CreateServerInstance()
        {
            this.target = Activator.CreateInstance(this.ServerType.Type);
            WebService target = this.target as WebService;
            if (target != null)
            {
                target.SetContext(this.context);
            }
        }

        internal virtual void DisposeServerInstance()
        {
            if (this.target != null)
            {
                IDisposable target = this.target as IDisposable;
                if (target != null)
                {
                    target.Dispose();
                }
                this.target = null;
            }
        }

        internal string GenerateFaultString(Exception e)
        {
            return this.GenerateFaultString(e, false);
        }

        internal string GenerateFaultString(Exception e, bool htmlEscapeMessage)
        {
            bool flag = (this.Context != null) && !this.Context.IsCustomErrorEnabled;
            if (flag && !htmlEscapeMessage)
            {
                return e.ToString();
            }
            StringBuilder builder = new StringBuilder();
            if (flag)
            {
                GenerateFaultString(e, builder);
            }
            else
            {
                for (Exception exception = e; exception != null; exception = exception.InnerException)
                {
                    string name = htmlEscapeMessage ? System.Web.HttpUtility.HtmlEncode(exception.Message) : exception.Message;
                    if (name.Length == 0)
                    {
                        name = e.GetType().Name;
                    }
                    builder.Append(name);
                    if (exception.InnerException != null)
                    {
                        builder.Append(" ---> ");
                    }
                }
            }
            return builder.ToString();
        }

        private static void GenerateFaultString(Exception e, StringBuilder builder)
        {
            builder.Append(e.GetType().FullName);
            if ((e.Message != null) && (e.Message.Length > 0))
            {
                builder.Append(": ");
                builder.Append(System.Web.HttpUtility.HtmlEncode(e.Message));
            }
            if (e.InnerException != null)
            {
                builder.Append(" ---> ");
                GenerateFaultString(e.InnerException, builder);
                builder.Append(Environment.NewLine);
                builder.Append("   ");
                builder.Append(Res.GetString("StackTraceEnd"));
            }
            if (e.StackTrace != null)
            {
                builder.Append(Environment.NewLine);
                builder.Append(e.StackTrace);
            }
        }

        protected object GetFromCache(System.Type protocolType, System.Type serverType)
        {
            return HttpRuntime.Cache.Get(this.CreateKey(protocolType, serverType));
        }

        internal abstract bool Initialize();
        internal abstract object[] ReadParameters();
        internal void SetContext(System.Type type, HttpContext context, HttpRequest request, HttpResponse response)
        {
            this.type = type;
            this.context = context;
            this.request = request;
            this.response = response;
            this.Initialize();
        }

        internal static void SetHttpResponseStatusCode(HttpResponse httpResponse, int statusCode)
        {
            httpResponse.TrySkipIisCustomErrors = true;
            httpResponse.StatusCode = statusCode;
        }

        internal virtual bool WriteException(Exception e, Stream outputStream)
        {
            return false;
        }

        internal void WriteOneWayResponse()
        {
            this.context.Response.ContentType = null;
            this.Response.StatusCode = 0xca;
        }

        internal abstract void WriteReturns(object[] returns, Stream outputStream);

        protected internal HttpContext Context
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.context;
            }
        }

        internal static object InternalSyncObject
        {
            get
            {
                if (s_InternalSyncObject == null)
                {
                    object obj2 = new object();
                    Interlocked.CompareExchange(ref s_InternalSyncObject, obj2, null);
                }
                return s_InternalSyncObject;
            }
        }

        internal abstract bool IsOneWay { get; }

        internal WebMethodAttribute MethodAttribute
        {
            get
            {
                if (this.methodAttr == null)
                {
                    this.methodAttr = this.MethodInfo.MethodAttribute;
                }
                return this.methodAttr;
            }
        }

        internal abstract LogicalMethodInfo MethodInfo { get; }

        internal virtual Exception OnewayInitException
        {
            get
            {
                return null;
            }
        }

        protected internal HttpRequest Request
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.request;
            }
        }

        protected internal HttpResponse Response
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.response;
            }
        }

        internal abstract System.Web.Services.Protocols.ServerType ServerType { get; }

        protected internal virtual object Target
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.target;
            }
        }

        internal System.Type Type
        {
            get
            {
                return this.type;
            }
        }
    }
}

