namespace System.Web.UI
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Reflection;
    using System.Security.Permissions;
    using System.Web;
    using System.Web.Compilation;
    using System.Web.Util;
    using System.Xml;

    public abstract class TemplateControl : Control, INamingContainer, IFilterResolutionService
    {
        private static object _emptyEventSingleton = new object();
        private static Hashtable _eventListCache = new Hashtable();
        private static IDictionary _eventObjects = new Hashtable(0x10);
        private static object _lockObject = new object();
        private int _maxResourceOffset;
        private BuildResultNoCompileTemplateControl _noCompileBuildResult;
        private const string _onTransactionAbortEventName = "OnTransactionAbort";
        private const string _onTransactionCommitEventName = "OnTransactionCommit";
        private const string _pageAbortTransactionEventName = "Page_AbortTransaction";
        private const string _pageCommitTransactionEventName = "Page_CommitTransaction";
        private const string _pageDataBindEventName = "Page_DataBind";
        private const string _pageErrorEventName = "Page_Error";
        private const string _pageInitCompleteEventName = "Page_InitComplete";
        private const string _pageInitEventName = "Page_Init";
        private const string _pageLoadCompleteEventName = "Page_LoadComplete";
        private const string _pageLoadEventName = "Page_Load";
        private const string _pagePreInitEventName = "Page_PreInit";
        private const string _pagePreLoadEventName = "Page_PreLoad";
        private const string _pagePreRenderCompleteEventName = "Page_PreRenderComplete";
        private const string _pagePreRenderEventName = "Page_PreRender";
        private const string _pageSaveStateCompleteEventName = "Page_SaveStateComplete";
        private const string _pageUnloadEventName = "Page_Unload";
        private IResourceProvider _resourceProvider;
        private IntPtr _stringResourcePointer;
        private System.Web.VirtualPath _virtualPath;
        private static readonly object EventAbortTransaction = new object();
        private static readonly object EventCommitTransaction = new object();
        private static readonly object EventError = new object();

        [WebSysDescription("Page_OnAbortTransaction")]
        public event EventHandler AbortTransaction
        {
            add
            {
                base.Events.AddHandler(EventAbortTransaction, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventAbortTransaction, value);
            }
        }

        [WebSysDescription("Page_OnCommitTransaction")]
        public event EventHandler CommitTransaction
        {
            add
            {
                base.Events.AddHandler(EventCommitTransaction, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventCommitTransaction, value);
            }
        }

        [WebSysDescription("Page_Error")]
        public event EventHandler Error
        {
            add
            {
                base.Events.AddHandler(EventError, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventError, value);
            }
        }

        static TemplateControl()
        {
            _eventObjects.Add("Page_PreInit", Page.EventPreInit);
            _eventObjects.Add("Page_Init", Control.EventInit);
            _eventObjects.Add("Page_InitComplete", Page.EventInitComplete);
            _eventObjects.Add("Page_Load", Control.EventLoad);
            _eventObjects.Add("Page_PreLoad", Page.EventPreLoad);
            _eventObjects.Add("Page_LoadComplete", Page.EventLoadComplete);
            _eventObjects.Add("Page_PreRenderComplete", Page.EventPreRenderComplete);
            _eventObjects.Add("Page_DataBind", Control.EventDataBinding);
            _eventObjects.Add("Page_PreRender", Control.EventPreRender);
            _eventObjects.Add("Page_SaveStateComplete", Page.EventSaveStateComplete);
            _eventObjects.Add("Page_Unload", Control.EventUnload);
            _eventObjects.Add("Page_Error", EventError);
            _eventObjects.Add("Page_AbortTransaction", EventAbortTransaction);
            _eventObjects.Add("OnTransactionAbort", EventAbortTransaction);
            _eventObjects.Add("Page_CommitTransaction", EventCommitTransaction);
            _eventObjects.Add("OnTransactionCommit", EventCommitTransaction);
        }

        protected TemplateControl()
        {
            this.Construct();
        }

        [ReflectionPermission(SecurityAction.Assert, Flags=ReflectionPermissionFlag.MemberAccess)]
        private void AddStackContextToHashCode(HashCodeCombiner combinedHashCode)
        {
            StackTrace trace = new StackTrace();
            int index = 2;
            while (true)
            {
                if (trace.GetFrame(index).GetMethod().DeclaringType != typeof(TemplateControl))
                {
                    break;
                }
                index++;
            }
            for (int i = index; i < (index + 2); i++)
            {
                StackFrame frame = trace.GetFrame(i);
                MethodBase method = frame.GetMethod();
                combinedHashCode.AddObject(method.DeclaringType.AssemblyQualifiedName);
                combinedHashCode.AddObject(method.Name);
                combinedHashCode.AddObject(frame.GetNativeOffset());
            }
        }

        private void CheckPageExists()
        {
            if (this.Page == null)
            {
                throw new InvalidOperationException(System.Web.SR.GetString("TemplateControl_DataBindingRequiresPage"));
            }
        }

        protected virtual void Construct()
        {
        }

        protected LiteralControl CreateResourceBasedLiteralControl(int offset, int size, bool fAsciiOnly)
        {
            return new ResourceBasedLiteralControl(this, offset, size, fAsciiOnly);
        }

        protected internal object Eval(string expression)
        {
            this.CheckPageExists();
            return DataBinder.Eval(this.Page.GetDataItem(), expression);
        }

        protected internal string Eval(string expression, string format)
        {
            this.CheckPageExists();
            return DataBinder.Eval(this.Page.GetDataItem(), expression, format);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        protected virtual void FrameworkInitialize()
        {
            if (this.NoCompile)
            {
                if ((!HttpRuntime.DisableProcessRequestInApplicationTrust && (HttpRuntime.NamedPermissionSet != null)) && !HttpRuntime.ProcessRequestInApplicationTrust)
                {
                    HttpRuntime.NamedPermissionSet.PermitOnly();
                }
                this._noCompileBuildResult.FrameworkInitialize(this);
            }
        }

        private void GetDelegateInformation(IDictionary dictionary)
        {
            if (HttpRuntime.IsFullTrust)
            {
                this.GetDelegateInformationWithNoAssert(dictionary);
            }
            else
            {
                this.GetDelegateInformationWithAssert(dictionary);
            }
        }

        private bool GetDelegateInformationFromMethod(string methodName, IDictionary dictionary)
        {
            EventHandler handler = (EventHandler) Delegate.CreateDelegate(typeof(EventHandler), this, methodName, true, false);
            if (handler != null)
            {
                dictionary[methodName] = new EventMethodInfo(handler.Method, false);
                return true;
            }
            VoidMethod method = (VoidMethod) Delegate.CreateDelegate(typeof(VoidMethod), this, methodName, true, false);
            if (method != null)
            {
                dictionary[methodName] = new EventMethodInfo(method.Method, true);
                return true;
            }
            return false;
        }

        [ReflectionPermission(SecurityAction.Assert, Flags=ReflectionPermissionFlag.MemberAccess)]
        private void GetDelegateInformationWithAssert(IDictionary dictionary)
        {
            this.GetDelegateInformationWithNoAssert(dictionary);
        }

        private void GetDelegateInformationWithNoAssert(IDictionary dictionary)
        {
            if (this is Page)
            {
                this.GetDelegateInformationFromMethod("Page_PreInit", dictionary);
                this.GetDelegateInformationFromMethod("Page_PreLoad", dictionary);
                this.GetDelegateInformationFromMethod("Page_LoadComplete", dictionary);
                this.GetDelegateInformationFromMethod("Page_PreRenderComplete", dictionary);
                this.GetDelegateInformationFromMethod("Page_InitComplete", dictionary);
                this.GetDelegateInformationFromMethod("Page_SaveStateComplete", dictionary);
            }
            this.GetDelegateInformationFromMethod("Page_Init", dictionary);
            this.GetDelegateInformationFromMethod("Page_Load", dictionary);
            this.GetDelegateInformationFromMethod("Page_DataBind", dictionary);
            this.GetDelegateInformationFromMethod("Page_PreRender", dictionary);
            this.GetDelegateInformationFromMethod("Page_Unload", dictionary);
            this.GetDelegateInformationFromMethod("Page_Error", dictionary);
            if (!this.GetDelegateInformationFromMethod("Page_AbortTransaction", dictionary))
            {
                this.GetDelegateInformationFromMethod("OnTransactionAbort", dictionary);
            }
            if (!this.GetDelegateInformationFromMethod("Page_CommitTransaction", dictionary))
            {
                this.GetDelegateInformationFromMethod("OnTransactionCommit", dictionary);
            }
        }

        protected object GetGlobalResourceObject(string className, string resourceKey)
        {
            return ResourceExpressionBuilder.GetGlobalResourceObject(className, resourceKey, null, null, null);
        }

        protected object GetGlobalResourceObject(string className, string resourceKey, Type objType, string propName)
        {
            return ResourceExpressionBuilder.GetGlobalResourceObject(className, resourceKey, objType, propName, null);
        }

        protected object GetLocalResourceObject(string resourceKey)
        {
            if (this._resourceProvider == null)
            {
                this._resourceProvider = ResourceExpressionBuilder.GetLocalResourceProvider(this);
            }
            return ResourceExpressionBuilder.GetResourceObject(this._resourceProvider, resourceKey, null);
        }

        protected object GetLocalResourceObject(string resourceKey, Type objType, string propName)
        {
            if (this._resourceProvider == null)
            {
                this._resourceProvider = ResourceExpressionBuilder.GetLocalResourceProvider(this);
            }
            return ResourceExpressionBuilder.GetResourceObject(this._resourceProvider, resourceKey, null, objType, propName);
        }

        internal override TemplateControl GetTemplateControl()
        {
            return this;
        }

        internal void HookUpAutomaticHandlers()
        {
            if (this.SupportAutoEvents)
            {
                object obj2 = _eventListCache[base.GetType()];
                IDictionary dictionary = null;
                if (obj2 == null)
                {
                    lock (_lockObject)
                    {
                        obj2 = _eventListCache[base.GetType()];
                        if (obj2 == null)
                        {
                            dictionary = new ListDictionary();
                            this.GetDelegateInformation(dictionary);
                            if (dictionary.Count == 0)
                            {
                                obj2 = _emptyEventSingleton;
                            }
                            else
                            {
                                obj2 = dictionary;
                            }
                            _eventListCache[base.GetType()] = obj2;
                        }
                    }
                }
                if (obj2 != _emptyEventSingleton)
                {
                    dictionary = (IDictionary) obj2;
                    foreach (string str in dictionary.Keys)
                    {
                        EventMethodInfo info = (EventMethodInfo) dictionary[str];
                        bool flag2 = false;
                        MethodInfo methodInfo = info.MethodInfo;
                        Delegate delegate2 = base.Events[_eventObjects[str]];
                        if (delegate2 != null)
                        {
                            foreach (Delegate delegate3 in delegate2.GetInvocationList())
                            {
                                if (delegate3.Method.Equals(methodInfo))
                                {
                                    flag2 = true;
                                    break;
                                }
                            }
                        }
                        if (!flag2)
                        {
                            IntPtr functionPointer = methodInfo.MethodHandle.GetFunctionPointer();
                            EventHandler handler = new CalliEventHandlerDelegateProxy(this, functionPointer, info.IsArgless).Handler;
                            base.Events.AddHandler(_eventObjects[str], handler);
                        }
                    }
                }
            }
        }

        public Control LoadControl(string virtualPath)
        {
            return this.LoadControl(System.Web.VirtualPath.Create(virtualPath));
        }

        internal Control LoadControl(System.Web.VirtualPath virtualPath)
        {
            virtualPath = System.Web.VirtualPath.Combine(base.TemplateControlVirtualDirectory, virtualPath);
            BuildResult vPathBuildResult = BuildManager.GetVPathBuildResult(this.Context, virtualPath);
            return this.LoadControl((IWebObjectFactory) vPathBuildResult, virtualPath, null, null);
        }

        public Control LoadControl(Type t, object[] parameters)
        {
            return this.LoadControl(null, null, t, parameters);
        }

        private Control LoadControl(IWebObjectFactory objectFactory, System.Web.VirtualPath virtualPath, Type t, object[] parameters)
        {
            BuildResultCompiledType type = null;
            BuildResultNoCompileUserControl control = null;
            PartialCachingAttribute cachingAttribute;
            if (objectFactory != null)
            {
                type = objectFactory as BuildResultCompiledType;
                if (type != null)
                {
                    t = type.ResultType;
                    Util.CheckAssignableType(typeof(UserControl), t);
                }
                else
                {
                    control = (BuildResultNoCompileUserControl) objectFactory;
                }
            }
            else if (t != null)
            {
                Util.CheckAssignableType(typeof(Control), t);
            }
            if (t != null)
            {
                cachingAttribute = (PartialCachingAttribute) TypeDescriptor.GetAttributes(t)[typeof(PartialCachingAttribute)];
            }
            else
            {
                cachingAttribute = control.CachingAttribute;
            }
            if (cachingAttribute == null)
            {
                Control control2;
                if (objectFactory != null)
                {
                    control2 = (Control) objectFactory.CreateInstance();
                }
                else
                {
                    control2 = (Control) HttpRuntime.CreatePublicInstance(t, parameters);
                }
                UserControl control3 = control2 as UserControl;
                if (control3 != null)
                {
                    if (virtualPath != null)
                    {
                        control3.TemplateControlVirtualPath = virtualPath;
                    }
                    control3.InitializeAsUserControl(this.Page);
                }
                return control2;
            }
            HashCodeCombiner combinedHashCode = new HashCodeCombiner();
            if (objectFactory != null)
            {
                combinedHashCode.AddObject(objectFactory);
            }
            else
            {
                combinedHashCode.AddObject(t);
            }
            if (!cachingAttribute.Shared)
            {
                this.AddStackContextToHashCode(combinedHashCode);
            }
            string combinedHashString = combinedHashCode.CombinedHashString;
            return new PartialCachingControl(objectFactory, t, cachingAttribute, "_" + combinedHashString, parameters);
        }

        public ITemplate LoadTemplate(string virtualPath)
        {
            return this.LoadTemplate(System.Web.VirtualPath.Create(virtualPath));
        }

        internal ITemplate LoadTemplate(System.Web.VirtualPath virtualPath)
        {
            virtualPath = System.Web.VirtualPath.Combine(base.TemplateControlVirtualDirectory, virtualPath);
            return new SimpleTemplate((ITypedWebObjectFactory) BuildManager.GetVPathBuildResult(this.Context, virtualPath));
        }

        protected virtual void OnAbortTransaction(EventArgs e)
        {
            EventHandler handler = (EventHandler) base.Events[EventAbortTransaction];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnCommitTransaction(EventArgs e)
        {
            EventHandler handler = (EventHandler) base.Events[EventCommitTransaction];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnError(EventArgs e)
        {
            EventHandler handler = (EventHandler) base.Events[EventError];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public Control ParseControl(string content)
        {
            return this.ParseControl(content, true);
        }

        public Control ParseControl(string content, bool ignoreParserFilter)
        {
            return TemplateParser.ParseControl(content, System.Web.VirtualPath.Create(this.AppRelativeVirtualPath), ignoreParserFilter);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public object ReadStringResource()
        {
            return StringResourceManager.ReadSafeStringResource(base.GetType());
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static object ReadStringResource(Type t)
        {
            return StringResourceManager.ReadSafeStringResource(t);
        }

        internal void SetNoCompileBuildResult(BuildResultNoCompileTemplateControl noCompileBuildResult)
        {
            this._noCompileBuildResult = noCompileBuildResult;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        protected void SetStringResourcePointer(object stringResourcePointer, int maxResourceOffset)
        {
            SafeStringResource resource = (SafeStringResource) stringResourcePointer;
            this._stringResourcePointer = resource.StringResourcePointer;
            this._maxResourceOffset = resource.ResourceSize;
        }

        int IFilterResolutionService.CompareFilters(string filter1, string filter2)
        {
            return BrowserCapabilitiesCompiler.BrowserCapabilitiesFactory.CompareFilters(filter1, filter2);
        }

        bool IFilterResolutionService.EvaluateFilter(string filterName)
        {
            return this.TestDeviceFilter(filterName);
        }

        public virtual bool TestDeviceFilter(string filterName)
        {
            return this.Context.Request.Browser.IsBrowser(filterName);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        protected void WriteUTF8ResourceString(HtmlTextWriter output, int offset, int size, bool fAsciiOnly)
        {
            if (((offset < 0) || (size < 0)) || ((offset + size) > this._maxResourceOffset))
            {
                throw new ArgumentOutOfRangeException("offset");
            }
            output.WriteUTF8ResourceString(this.StringResourcePointer, offset, size, fAsciiOnly);
        }

        protected internal object XPath(string xPathExpression)
        {
            this.CheckPageExists();
            return XPathBinder.Eval(this.Page.GetDataItem(), xPathExpression);
        }

        protected internal string XPath(string xPathExpression, string format)
        {
            this.CheckPageExists();
            return XPathBinder.Eval(this.Page.GetDataItem(), xPathExpression, format);
        }

        protected internal object XPath(string xPathExpression, IXmlNamespaceResolver resolver)
        {
            this.CheckPageExists();
            return XPathBinder.Eval(this.Page.GetDataItem(), xPathExpression, resolver);
        }

        protected internal string XPath(string xPathExpression, string format, IXmlNamespaceResolver resolver)
        {
            this.CheckPageExists();
            return XPathBinder.Eval(this.Page.GetDataItem(), xPathExpression, format, resolver);
        }

        protected internal IEnumerable XPathSelect(string xPathExpression)
        {
            this.CheckPageExists();
            return XPathBinder.Select(this.Page.GetDataItem(), xPathExpression);
        }

        protected internal IEnumerable XPathSelect(string xPathExpression, IXmlNamespaceResolver resolver)
        {
            this.CheckPageExists();
            return XPathBinder.Select(this.Page.GetDataItem(), xPathExpression, resolver);
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Advanced)]
        public string AppRelativeVirtualPath
        {
            get
            {
                return System.Web.VirtualPath.GetAppRelativeVirtualPathString(this.TemplateControlVirtualPath);
            }
            set
            {
                this.TemplateControlVirtualPath = System.Web.VirtualPath.CreateNonRelative(value);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Obsolete("Use of this property is not recommended because it is no longer useful. http://go.microsoft.com/fwlink/?linkid=14202")]
        protected virtual int AutoHandlers
        {
            get
            {
                return 0;
            }
            set
            {
            }
        }

        [Browsable(true)]
        public override bool EnableTheming
        {
            get
            {
                return base.EnableTheming;
            }
            set
            {
                base.EnableTheming = value;
            }
        }

        internal int MaxResourceOffset
        {
            get
            {
                return this._maxResourceOffset;
            }
        }

        internal bool NoCompile
        {
            get
            {
                return (this._noCompileBuildResult != null);
            }
        }

        internal IntPtr StringResourcePointer
        {
            get
            {
                return this._stringResourcePointer;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        protected virtual bool SupportAutoEvents
        {
            get
            {
                return true;
            }
        }

        internal System.Web.VirtualPath TemplateControlVirtualPath
        {
            get
            {
                return this._virtualPath;
            }
            set
            {
                this._virtualPath = value;
                base.TemplateControlVirtualDirectory = this._virtualPath.Parent;
            }
        }

        internal System.Web.VirtualPath VirtualPath
        {
            get
            {
                return this._virtualPath;
            }
        }

        private class EventMethodInfo
        {
            private bool _isArgless;
            private System.Reflection.MethodInfo _methodInfo;

            internal EventMethodInfo(System.Reflection.MethodInfo methodInfo, bool isArgless)
            {
                this._isArgless = isArgless;
                this._methodInfo = methodInfo;
            }

            internal bool IsArgless
            {
                get
                {
                    return this._isArgless;
                }
            }

            internal System.Reflection.MethodInfo MethodInfo
            {
                get
                {
                    return this._methodInfo;
                }
            }
        }

        internal class SimpleTemplate : ITemplate
        {
            private IWebObjectFactory _objectFactory;

            internal SimpleTemplate(ITypedWebObjectFactory objectFactory)
            {
                Util.CheckAssignableType(typeof(UserControl), objectFactory.InstantiatedType);
                this._objectFactory = objectFactory;
            }

            public virtual void InstantiateIn(Control control)
            {
                UserControl child = (UserControl) this._objectFactory.CreateInstance();
                child.InitializeAsUserControl(control.Page);
                control.Controls.Add(child);
            }
        }
    }
}

