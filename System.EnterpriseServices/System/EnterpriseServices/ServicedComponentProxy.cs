namespace System.EnterpriseServices
{
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.EnterpriseServices.Thunk;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Activation;
    using System.Runtime.Remoting.Messaging;
    using System.Runtime.Remoting.Proxies;
    using System.Runtime.Remoting.Services;
    using System.Security.Permissions;
    using System.Threading;

    internal class ServicedComponentProxy : RealProxy, IProxyInvoke, IManagedPoolAction
    {
        private static bool _asyncFinalizeEnabled = true;
        private Callback _callback;
        private static Thread _cleanupThread;
        private IntPtr _context;
        private static Queue _ctxQueue;
        private static ManualResetEvent _exitCleanupThread;
        private bool _fDeliverADC;
        private bool _fFinalized;
        private bool _filterConstructors;
        private bool _fIsActive;
        private bool _fIsJitActivated;
        private bool _fIsObjectPooled;
        private bool _fIsServerActivated;
        private bool _fReturnedByFinalizer;
        private bool _fUseIntfDispatch;
        private static MethodInfo _getComIUnknownMethod = typeof(MarshalByRefObject).GetMethod("GetComIUnknown", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(bool) }, null);
        private static MethodInfo _getHashCodeMethod = typeof(object).GetMethod("GetHashCode");
        private static MethodBase _getIDisposableDispose = typeof(IDisposable).GetMethod("Dispose", new Type[0]);
        private static MethodInfo _getLifetimeServiceMethod = typeof(MarshalByRefObject).GetMethod("GetLifetimeService", new Type[0]);
        private static MethodBase _getServicedComponentDispose = typeof(ServicedComponent).GetMethod("Dispose", new Type[0]);
        private static MethodInfo _getTypeMethod = typeof(object).GetMethod("GetType");
        private int _gitCookie;
        private static Queue _gitQueue;
        private static MethodInfo _initializeLifetimeServiceMethod = typeof(MarshalByRefObject).GetMethod("InitializeLifetimeService", new Type[0]);
        private static MethodInfo _internalDeactivateMethod = typeof(ServicedComponent).GetMethod("_internalDeactivate", BindingFlags.NonPublic | BindingFlags.Instance);
        private IntPtr _pPoolUnk;
        private ProxyTearoff _proxyTearoff;
        private static int _QueuedItemsCount;
        private static Guid _s_IID_IManagedObjectInfo = Marshal.GenerateGuidForType(typeof(System.EnterpriseServices.IManagedObjectInfo));
        private static Guid _s_IID_IManagedPoolAction = Marshal.GenerateGuidForType(typeof(IManagedPoolAction));
        private static Guid _s_IID_IObjectConstruct = Marshal.GenerateGuidForType(typeof(IObjectConstruct));
        private static Guid _s_IID_IObjectControl = Marshal.GenerateGuidForType(typeof(IObjectControl));
        private ServicedComponentStub _scstub;
        private static MethodInfo _setCOMIUnknownMethod = typeof(ServicedComponent).GetMethod("DoSetCOMIUnknown", BindingFlags.NonPublic | BindingFlags.Instance);
        private static IntPtr _stub = Proxy.GetContextCheck();
        private bool _tabled;
        private IntPtr _token;
        private Tracker _tracker;
        private static AutoResetEvent _Wakeup;
        private static readonly IntPtr NegativeOne = new IntPtr(-1);

        static ServicedComponentProxy()
        {
            try
            {
                System.EnterpriseServices.BooleanSwitch switch2 = new System.EnterpriseServices.BooleanSwitch("DisableAsyncFinalization");
                _asyncFinalizeEnabled = !switch2.Enabled;
            }
            catch (Exception exception)
            {
                if ((exception is NullReferenceException) || (exception is SEHException))
                {
                    throw;
                }
                _asyncFinalizeEnabled = true;
            }
            if (_asyncFinalizeEnabled)
            {
                _ctxQueue = new Queue();
                _gitQueue = new Queue();
                _Wakeup = new AutoResetEvent(false);
                _exitCleanupThread = new ManualResetEvent(false);
                _cleanupThread = new Thread(new ThreadStart(ServicedComponentProxy.QueueCleaner));
                _cleanupThread.IsBackground = true;
                _cleanupThread.Start();
                AppDomain.CurrentDomain.DomainUnload += new EventHandler(ServicedComponentProxy.ShutdownDomain);
            }
        }

        private ServicedComponentProxy()
        {
        }

        internal ServicedComponentProxy(Type serverType, bool fIsJitActivated, bool fIsPooled, bool fAreMethodsSecure, bool fCreateRealServer) : base(serverType, _stub, -1)
        {
            this._gitCookie = 0;
            this._fIsObjectPooled = fIsPooled;
            this._fIsJitActivated = fIsJitActivated;
            this._fDeliverADC = this._fIsObjectPooled || this._fIsJitActivated;
            this._fIsActive = !this._fDeliverADC;
            this._tabled = false;
            this._fUseIntfDispatch = fAreMethodsSecure;
            this._context = NegativeOne;
            this._token = NegativeOne;
            this._tracker = null;
            this._callback = new Callback();
            this._pPoolUnk = IntPtr.Zero;
            if (Util.ExtendedLifetime)
            {
                this._scstub = new ServicedComponentStub(this);
            }
            if (fCreateRealServer)
            {
                try
                {
                    this.ConstructServer();
                }
                catch (Exception exception)
                {
                    if ((exception is NullReferenceException) || (exception is SEHException))
                    {
                        throw;
                    }
                    this.ReleaseContext();
                    if (!Util.ExtendedLifetime)
                    {
                        this.ReleaseGitCookie();
                    }
                    this._fIsServerActivated = false;
                    GC.SuppressFinalize(this);
                    throw;
                }
                this.SendCreationEvents();
            }
        }

        internal void ActivateObject()
        {
            IntPtr currentContextToken = Proxy.GetCurrentContextToken();
            if ((this.IsObjectPooled && this.IsJitActivated) && (this.HomeToken != currentContextToken))
            {
                object obj2 = IdentityTable.FindObject(currentContextToken);
                if (obj2 != null)
                {
                    ServicedComponentProxy realProxy = (ServicedComponentProxy) RemotingServices.GetRealProxy(obj2);
                    ProxyTearoff proxyTearoff = null;
                    ServicedComponent server = this.DisconnectForPooling(ref proxyTearoff);
                    proxyTearoff.SetCanBePooled(false);
                    realProxy.ConnectForPooling(this, server, proxyTearoff, true);
                    EnterpriseServicesHelper.SwitchWrappers(this, realProxy);
                    realProxy.ActivateProxy();
                    return;
                }
            }
            this.ActivateProxy();
        }

        internal void ActivateProxy()
        {
            if (!this._fIsActive)
            {
                this._fIsActive = true;
                this.SetupContext(false);
                this.DispatchActivate();
            }
        }

        private MemberInfo AliasCall(MethodInfo mi)
        {
            if (mi == null)
            {
                return null;
            }
            MethodInfo baseDefinition = mi.GetBaseDefinition();
            if (baseDefinition == _internalDeactivateMethod)
            {
                return _getIDisposableDispose;
            }
            if ((!(baseDefinition == _initializeLifetimeServiceMethod) && !(baseDefinition == _getLifetimeServiceMethod)) && (!(baseDefinition == _getComIUnknownMethod) && !(baseDefinition == _setCOMIUnknownMethod)))
            {
                return null;
            }
            ComMemberType method = ComMemberType.Method;
            return Marshal.GetMethodInfoForComSlot(typeof(IManagedObject), 3, ref method);
        }

        private void AssertValid()
        {
            if ((this._context == NegativeOne) || (this._context == IntPtr.Zero))
            {
                throw new ObjectDisposedException("ServicedComponent");
            }
        }

        internal static bool CleanupQueues(bool bGit)
        {
            bool flag = true;
            bool flag2 = true;
            if (!_asyncFinalizeEnabled)
            {
                return true;
            }
            if (bGit)
            {
                if (_gitQueue.Count > 0)
                {
                    bool flag3 = false;
                    int cookie = 0;
                    lock (_gitQueue)
                    {
                        if (_gitQueue.Count > 0)
                        {
                            cookie = (int) _gitQueue.Dequeue();
                            flag3 = true;
                            flag = _gitQueue.Count <= 0;
                        }
                    }
                    if (flag3)
                    {
                        Proxy.RevokeObject(cookie);
                    }
                }
            }
            else if (_gitQueue.Count > 0)
            {
                lock (_gitQueue)
                {
                    if ((_gitQueue.Count > 0) && (_QueuedItemsCount < 0x19))
                    {
                        try
                        {
                            ThreadPool.QueueUserWorkItem(new WaitCallback(ServicedComponentProxy.RevokeAsync), _gitQueue.Count);
                            Interlocked.Increment(ref _QueuedItemsCount);
                        }
                        catch (Exception exception)
                        {
                            if ((exception is NullReferenceException) || (exception is SEHException))
                            {
                                throw;
                            }
                        }
                    }
                }
            }
            object obj2 = null;
            if (_ctxQueue.Count > 0)
            {
                lock (_ctxQueue)
                {
                    if (_ctxQueue.Count > 0)
                    {
                        obj2 = _ctxQueue.Dequeue();
                        flag2 = _ctxQueue.Count <= 0;
                    }
                }
                if (obj2 != null)
                {
                    if (!Util.ExtendedLifetime)
                    {
                        Marshal.Release((IntPtr) obj2);
                    }
                    else
                    {
                        ServicedComponentProxy proxy = (ServicedComponentProxy) obj2;
                        try
                        {
                            proxy.SendDestructionEvents(false);
                        }
                        catch (Exception exception2)
                        {
                            if ((exception2 is NullReferenceException) || (exception2 is SEHException))
                            {
                                throw;
                            }
                        }
                        try
                        {
                            proxy.ReleaseContext();
                        }
                        catch (Exception exception3)
                        {
                            if ((exception3 is NullReferenceException) || (exception3 is SEHException))
                            {
                                throw;
                            }
                        }
                    }
                }
            }
            return (flag2 & flag);
        }

        internal void ConnectForPooling(ServicedComponentProxy oldscp, ServicedComponent server, ProxyTearoff proxyTearoff, bool fForJit)
        {
            if (oldscp != null)
            {
                this._fReturnedByFinalizer = oldscp._fFinalized;
                if (fForJit)
                {
                    this._pPoolUnk = oldscp._pPoolUnk;
                    oldscp._pPoolUnk = IntPtr.Zero;
                }
            }
            if (server != null)
            {
                base.AttachServer(server);
            }
            this._proxyTearoff = proxyTearoff;
            this._proxyTearoff.Init(this);
        }

        internal void ConstructServer()
        {
            this.SetupContext(true);
            IConstructionReturnMessage message = base.InitializeServerObject(null);
            if ((message != null) && (message.Exception != null))
            {
                ((ServicedComponent) this.GetTransparentProxy())._callFinalize(true);
                base.DetachServer();
                throw message.Exception;
            }
        }

        public override ObjRef CreateObjRef(Type requestedType)
        {
            return new ServicedComponentMarshaler((MarshalByRefObject) this.GetTransparentProxy(), requestedType);
        }

        [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        private IMessage CrossCtxInvoke(IMessage reqMsg)
        {
            IMessage message = null;
            this.AssertValid();
            message = this.HandleDispose(reqMsg);
            if (message != null)
            {
                return message;
            }
            message = this.HandleSetCOMIUnknown(reqMsg);
            if (message != null)
            {
                return message;
            }
            message = this.HandleSpecialMethods(reqMsg);
            if (message != null)
            {
                return message;
            }
            object transparentProxy = this.GetTransparentProxy();
            MethodBase methodBase = ((IMethodMessage) reqMsg).MethodBase;
            MemberInfo mi = methodBase;
            MemberInfo mb = mi;
            MemberInfo info3 = null;
            MemberInfo m = ReflectionCache.ConvertToClassMI(base.GetProxiedType(), mi);
            bool fIsAutoDone = false;
            int num = ServicedComponentInfo.MICachedLookup(m);
            if (reqMsg is IConstructionCallMessage)
            {
                ComMemberType method = ComMemberType.Method;
                mb = Marshal.GetMethodInfoForComSlot(typeof(IManagedObject), 3, ref method);
            }
            else
            {
                info3 = this.AliasCall(methodBase as MethodInfo);
                if (info3 != null)
                {
                    mb = info3;
                }
                else if (this._fUseIntfDispatch || ((num & 4) != 0))
                {
                    mb = ReflectionCache.ConvertToInterfaceMI(mi);
                    if (mb == null)
                    {
                        throw new ServicedComponentException(Resource.FormatString("ServicedComponentException_SecurityMapping"));
                    }
                }
                else
                {
                    fIsAutoDone = (num & 2) != 0;
                }
            }
            return this._callback.DoCallback(transparentProxy, reqMsg, this._context, fIsAutoDone, mb, this._gitCookie != 0);
        }

        internal void DeactivateProxy(bool disposing)
        {
            if (this._fIsActive)
            {
                object transparentProxy = this.GetTransparentProxy();
                if (base.GetUnwrappedServer() != null)
                {
                    this.DispatchDeactivate();
                    ((ServicedComponent) transparentProxy)._callFinalize(disposing);
                    base.DetachServer();
                }
                RealProxy.SetStubData(this, NegativeOne);
                this._fIsActive = false;
                if (!this.IsJitActivated)
                {
                    this.ReleaseGitCookie();
                }
                this.ReleasePoolUnk();
            }
        }

        internal ServicedComponent DisconnectForPooling(ref ProxyTearoff proxyTearoff)
        {
            if (this._fIsServerActivated)
            {
                this.DispatchDeactivate();
            }
            proxyTearoff = this._proxyTearoff;
            this._proxyTearoff = null;
            if (base.GetUnwrappedServer() != null)
            {
                return (ServicedComponent) base.DetachServer();
            }
            return null;
        }

        private void DispatchActivate()
        {
            if (this._fDeliverADC)
            {
                this._fIsServerActivated = true;
                ServicedComponent transparentProxy = (ServicedComponent) this.GetTransparentProxy();
                try
                {
                    transparentProxy.Activate();
                }
                catch (Exception exception)
                {
                    if ((exception is NullReferenceException) || (exception is SEHException))
                    {
                        throw;
                    }
                    this.SendDestructionEvents(false);
                    this.ReleasePoolUnk();
                    this.ReleaseContext();
                    this.ReleaseGitCookie();
                    this._fIsServerActivated = false;
                    try
                    {
                        EventLog log = new EventLog {
                            Source = "System.EnterpriseServices"
                        };
                        string message = Resource.FormatString("Err_ActivationFailed", exception.ToString());
                        log.WriteEntry(message, EventLogEntryType.Error);
                    }
                    catch (Exception exception2)
                    {
                        if ((exception2 is NullReferenceException) || (exception2 is SEHException))
                        {
                            throw;
                        }
                    }
                    throw new COMException(Resource.FormatString("ServicedComponentException_ActivationFailed"), -2147164123);
                }
            }
        }

        internal void DispatchConstruct(string str)
        {
            ((ServicedComponent) this.GetTransparentProxy()).Construct(str);
        }

        private void DispatchDeactivate()
        {
            if (this._fDeliverADC)
            {
                ServicedComponent transparentProxy = (ServicedComponent) this.GetTransparentProxy();
                this._fIsServerActivated = false;
                try
                {
                    if (!this._fFinalized)
                    {
                        transparentProxy.Deactivate();
                    }
                }
                catch (Exception exception)
                {
                    if ((exception is NullReferenceException) || (exception is SEHException))
                    {
                        throw;
                    }
                }
                if (this.IsObjectPooled)
                {
                    bool fCanBePooled = false;
                    try
                    {
                        if (!this._fFinalized)
                        {
                            fCanBePooled = transparentProxy.CanBePooled();
                        }
                        this._proxyTearoff.SetCanBePooled(fCanBePooled);
                    }
                    catch (Exception exception2)
                    {
                        if ((exception2 is NullReferenceException) || (exception2 is SEHException))
                        {
                            throw;
                        }
                        this._proxyTearoff.SetCanBePooled(false);
                    }
                }
            }
        }

        internal void Dispose(bool disposing)
        {
            if (Util.ExtendedLifetime && (disposing || !_asyncFinalizeEnabled))
            {
                this.SendDestructionEvents(disposing);
            }
            if (this._fIsActive)
            {
                ServicedComponent transparentProxy = (ServicedComponent) this.GetTransparentProxy();
                try
                {
                    transparentProxy._internalDeactivate(disposing);
                }
                catch (ObjectDisposedException)
                {
                }
            }
            if ((!disposing && this.IsObjectPooled) && (base.GetUnwrappedServer() != null))
            {
                this.FinalizeHere();
            }
            this.ReleasePoolUnk();
            if ((Util.ExtendedLifetime && !disposing) && _asyncFinalizeEnabled)
            {
                this.SendDestructionEventsAsync();
            }
            this.ReleaseGitCookie();
            if ((disposing || !_asyncFinalizeEnabled) || AppDomain.CurrentDomain.IsFinalizingForUnload())
            {
                this.ReleaseContext();
            }
            else if (!Util.ExtendedLifetime)
            {
                this.ReleaseContextAsync();
            }
            this._fIsActive = false;
            if (disposing)
            {
                GC.SuppressFinalize(this);
            }
        }

        internal void FilterConstructors()
        {
            if (this._fIsJitActivated)
            {
                throw new ServicedComponentException(Resource.FormatString("ServicedComponentException_BadConfiguration"));
            }
            this._filterConstructors = true;
            RealProxy.SetStubData(this, NegativeOne);
        }

        ~ServicedComponentProxy()
        {
            this._fFinalized = true;
            try
            {
                if (this._gitCookie != 0)
                {
                    GC.ReRegisterForFinalize(this);
                    if (_asyncFinalizeEnabled)
                    {
                        this.ReleaseGitCookieAsync();
                    }
                    else
                    {
                        this.ReleaseGitCookie();
                    }
                    if (this._proxyTearoff != null)
                    {
                        Marshal.ChangeWrapperHandleStrength(this._proxyTearoff, false);
                    }
                    Marshal.ChangeWrapperHandleStrength(this.GetTransparentProxy(), false);
                }
                else
                {
                    if (Util.ExtendedLifetime)
                    {
                        this.RefreshStub();
                    }
                    this.Dispose(this._pPoolUnk != IntPtr.Zero);
                }
            }
            catch (Exception exception)
            {
                if ((exception is NullReferenceException) || (exception is SEHException))
                {
                    throw;
                }
            }
        }

        private void FinalizeHere()
        {
            IntPtr currentContextToken = Proxy.GetCurrentContextToken();
            IntPtr currentContext = Proxy.GetCurrentContext();
            try
            {
                RealProxy.SetStubData(this, currentContextToken);
                ((ServicedComponent) this.GetTransparentProxy())._callFinalize(false);
            }
            finally
            {
                Marshal.Release(currentContext);
                RealProxy.SetStubData(this, NegativeOne);
            }
        }

        public override IntPtr GetCOMIUnknown(bool fIsBeingMarshalled)
        {
            if (((this._token == IntPtr.Zero) || (this._token == NegativeOne)) || (this._token == Proxy.GetCurrentContextToken()))
            {
                if (!fIsBeingMarshalled)
                {
                    return base.GetCOMIUnknown(false);
                }
                IntPtr zero = IntPtr.Zero;
                IntPtr standardMarshal = IntPtr.Zero;
                try
                {
                    zero = base.GetCOMIUnknown(false);
                    standardMarshal = Proxy.GetStandardMarshal(zero);
                }
                finally
                {
                    if (zero != IntPtr.Zero)
                    {
                        Marshal.Release(zero);
                    }
                }
                return standardMarshal;
            }
            if (Util.ExtendedLifetime)
            {
                IntPtr cOMIUnknown = base.GetCOMIUnknown(false);
                IntPtr ptr4 = IntPtr.Zero;
                try
                {
                    ptr4 = Proxy.UnmarshalObject(this._callback.SwitchMarshal(this._context, cOMIUnknown));
                }
                finally
                {
                    if (cOMIUnknown != IntPtr.Zero)
                    {
                        Marshal.Release(cOMIUnknown);
                    }
                }
                return ptr4;
            }
            if (this._gitCookie == 0)
            {
                return base.GetCOMIUnknown(false);
            }
            return Proxy.GetObject(this._gitCookie);
        }

        public IntPtr GetOuterIUnknown()
        {
            IntPtr zero = IntPtr.Zero;
            IntPtr ppv = IntPtr.Zero;
            try
            {
                zero = base.GetCOMIUnknown(false);
                Guid iid = Util.IID_IUnknown;
                int errorCode = Marshal.QueryInterface(zero, ref iid, out ppv);
                if (errorCode != 0)
                {
                    Marshal.ThrowExceptionForHR(errorCode);
                }
            }
            finally
            {
                if (zero != IntPtr.Zero)
                {
                    Marshal.Release(zero);
                }
            }
            return ppv;
        }

        internal ProxyTearoff GetProxyTearoff()
        {
            if (this._proxyTearoff == null)
            {
                if (Util.ExtendedLifetime)
                {
                    this._proxyTearoff = new WeakProxyTearoff();
                }
                else
                {
                    this._proxyTearoff = new ClassicProxyTearoff();
                }
                this._proxyTearoff.Init(this);
            }
            return this._proxyTearoff;
        }

        private IMessage HandleDispose(IMessage msg)
        {
            IMethodCallMessage message = msg as IMethodCallMessage;
            if (message != null)
            {
                MethodBase methodBase = message.MethodBase;
                if ((methodBase == _getServicedComponentDispose) || (methodBase == _getIDisposableDispose))
                {
                    ServicedComponent.DisposeObject((ServicedComponent) this.GetTransparentProxy());
                    IMethodCallMessage mcm = (IMethodCallMessage) msg;
                    return new ReturnMessage(null, null, 0, mcm.LogicalCallContext, mcm);
                }
            }
            return null;
        }

        private IMessage HandleSetCOMIUnknown(IMessage reqMsg)
        {
            if (((IMethodMessage) reqMsg).MethodBase == _setCOMIUnknownMethod)
            {
                IMethodCallMessage mcm = (IMethodCallMessage) reqMsg;
                IntPtr i = (IntPtr) mcm.InArgs[0];
                if (i != IntPtr.Zero)
                {
                    this.SetCOMIUnknown(i);
                    return new ReturnMessage(null, null, 0, mcm.LogicalCallContext, mcm);
                }
            }
            return null;
        }

        private IMessage HandleSpecialMethods(IMessage reqMsg)
        {
            MethodBase methodBase = ((IMethodMessage) reqMsg).MethodBase;
            if (methodBase == _getTypeMethod)
            {
                IMethodCallMessage mcm = (IMethodCallMessage) reqMsg;
                return new ReturnMessage(base.GetProxiedType(), null, 0, mcm.LogicalCallContext, mcm);
            }
            if (methodBase == _getHashCodeMethod)
            {
                int hashCode = this.GetHashCode();
                IMethodCallMessage message2 = (IMethodCallMessage) reqMsg;
                return new ReturnMessage(hashCode, null, 0, message2.LogicalCallContext, message2);
            }
            return null;
        }

        public override IMessage Invoke(IMessage request)
        {
            if (this._token == Proxy.GetCurrentContextToken())
            {
                return this.LocalInvoke(request);
            }
            return this.CrossCtxInvoke(request);
        }

        private bool IsRealCall(MethodBase mb)
        {
            return ((((mb != _internalDeactivateMethod) && (mb != _initializeLifetimeServiceMethod)) && ((mb != _getLifetimeServiceMethod) && (mb != _getComIUnknownMethod))) && (((mb != _setCOMIUnknownMethod) && (mb != _getTypeMethod)) && (mb != _getHashCodeMethod)));
        }

        public IMessage LocalInvoke(IMessage reqMsg)
        {
            IMessage message = null;
            if (reqMsg is IConstructionCallMessage)
            {
                this.ActivateProxy();
                if (this._filterConstructors)
                {
                    this._filterConstructors = false;
                    RealProxy.SetStubData(this, this._token);
                }
                if (((IConstructionCallMessage) reqMsg).ArgCount > 0)
                {
                    throw new ServicedComponentException(Resource.FormatString("ServicedComponentException_ConstructorArguments"));
                }
                MarshalByRefObject transparentProxy = (MarshalByRefObject) this.GetTransparentProxy();
                return EnterpriseServicesHelper.CreateConstructionReturnMessage((IConstructionCallMessage) reqMsg, transparentProxy);
            }
            if (reqMsg is IMethodCallMessage)
            {
                message = this.HandleSpecialMethods(reqMsg);
                if (message != null)
                {
                    return message;
                }
                if ((base.GetUnwrappedServer() == null) || (((IntPtr) RealProxy.GetStubData(this)) == NegativeOne))
                {
                    throw new ObjectDisposedException("ServicedComponent");
                }
                bool flag = this.SendMethodCall(reqMsg);
                try
                {
                    message = RemotingServices.ExecuteMessage((MarshalByRefObject) this.GetTransparentProxy(), (IMethodCallMessage) reqMsg);
                    if (flag)
                    {
                        this.SendMethodReturn(reqMsg, ((IMethodReturnMessage) message).Exception);
                    }
                }
                catch (Exception exception)
                {
                    if ((exception is NullReferenceException) || (exception is SEHException))
                    {
                        throw;
                    }
                    if (flag)
                    {
                        this.SendMethodReturn(reqMsg, exception);
                    }
                    throw;
                }
            }
            return message;
        }

        private static void QueueCleaner()
        {
            while (!_exitCleanupThread.WaitOne(0, false))
            {
                CleanupQueues(true);
                if ((_gitQueue.Count == 0) && (_ctxQueue.Count == 0))
                {
                    _Wakeup.WaitOne(0x9c4, false);
                }
            }
        }

        private void RefreshStub()
        {
            if (this._proxyTearoff != null)
            {
                this._proxyTearoff.Init(this);
            }
            if (this._scstub != null)
            {
                this._scstub.Refresh(this);
            }
        }

        private void ReleaseContext()
        {
            if (this._token != NegativeOne)
            {
                object transparentProxy = this.GetTransparentProxy();
                if (this.IsJitActivated && this._tabled)
                {
                    IdentityTable.RemoveObject(this._token, transparentProxy);
                    this._tabled = false;
                }
                if (this._tracker != null)
                {
                    this._tracker.Release();
                }
                Marshal.Release(this._context);
                this._context = NegativeOne;
                this._token = NegativeOne;
            }
        }

        private void ReleaseContextAsync()
        {
            if (this._token != NegativeOne)
            {
                if (AppDomain.CurrentDomain.IsFinalizingForUnload())
                {
                    this.ReleaseContext();
                }
                else
                {
                    object transparentProxy = this.GetTransparentProxy();
                    if (this.IsJitActivated && this._tabled)
                    {
                        IdentityTable.RemoveObject(this._token, transparentProxy);
                        this._tabled = false;
                    }
                    lock (_ctxQueue)
                    {
                        _ctxQueue.Enqueue(this._context);
                    }
                    this._context = NegativeOne;
                    this._token = NegativeOne;
                }
            }
        }

        private void ReleaseGitCookie()
        {
            int cookie = Interlocked.Exchange(ref this._gitCookie, 0);
            if (cookie != 0)
            {
                Proxy.RevokeObject(cookie);
            }
        }

        private void ReleaseGitCookieAsync()
        {
            if (this._gitCookie != 0)
            {
                if (AppDomain.CurrentDomain.IsFinalizingForUnload())
                {
                    this.ReleaseGitCookie();
                }
                else
                {
                    int num = this._gitCookie;
                    this._gitCookie = 0;
                    lock (_gitQueue)
                    {
                        _gitQueue.Enqueue(num);
                    }
                }
            }
        }

        private void ReleasePoolUnk()
        {
            if (this._pPoolUnk != IntPtr.Zero)
            {
                IntPtr pPooledObject = this._pPoolUnk;
                this._pPoolUnk = IntPtr.Zero;
                Proxy.PoolUnmark(pPooledObject);
            }
        }

        internal static void RevokeAsync(object o)
        {
            int num = (int) o;
            try
            {
                for (int i = 0; i < num; i++)
                {
                    if (CleanupQueues(true))
                    {
                        goto Label_0032;
                    }
                }
            }
            catch (Exception exception)
            {
                if ((exception is NullReferenceException) || (exception is SEHException))
                {
                    throw;
                }
            }
        Label_0032:
            Interlocked.Decrement(ref _QueuedItemsCount);
        }

        internal void SendCreationEvents()
        {
            if ((Util.ExtendedLifetime && (this._context != IntPtr.Zero)) && (this._context != NegativeOne))
            {
                IntPtr stub = this.SupportsInterface(ref _s_IID_IManagedObjectInfo);
                if (stub != IntPtr.Zero)
                {
                    try
                    {
                        Proxy.SendCreationEvents(this._context, stub, this.IsJitActivated);
                    }
                    finally
                    {
                        Marshal.Release(stub);
                    }
                }
            }
        }

        internal void SendDestructionEvents(bool disposing)
        {
            if ((Util.ExtendedLifetime && (this._context != IntPtr.Zero)) && (this._context != NegativeOne))
            {
                IntPtr stub = this.SupportsInterface(ref _s_IID_IManagedObjectInfo);
                if (stub != IntPtr.Zero)
                {
                    try
                    {
                        Proxy.SendDestructionEvents(this._context, stub, disposing);
                    }
                    finally
                    {
                        Marshal.Release(stub);
                    }
                }
            }
        }

        private void SendDestructionEventsAsync()
        {
            if (AppDomain.CurrentDomain.IsFinalizingForUnload())
            {
                this.SendDestructionEvents(false);
            }
            else
            {
                lock (_ctxQueue)
                {
                    _ctxQueue.Enqueue(this);
                }
            }
        }

        private bool SendMethodCall(IMessage req)
        {
            bool flag = false;
            if (this._tracker != null)
            {
                IntPtr zero = IntPtr.Zero;
                try
                {
                    IMethodCallMessage message = req as IMethodCallMessage;
                    if (!this.IsRealCall(message.MethodBase))
                    {
                        return false;
                    }
                    if (Util.ExtendedLifetime)
                    {
                        zero = this.SupportsInterface(ref _s_IID_IManagedObjectInfo);
                    }
                    else
                    {
                        zero = this.GetOuterIUnknown();
                    }
                    MethodBase method = ReflectionCache.ConvertToInterfaceMI(message.MethodBase) as MethodBase;
                    if (method != null)
                    {
                        this._tracker.SendMethodCall(zero, method);
                        flag = true;
                    }
                }
                catch (Exception exception)
                {
                    if ((exception is NullReferenceException) || (exception is SEHException))
                    {
                        throw;
                    }
                    return flag;
                }
                finally
                {
                    if (zero != IntPtr.Zero)
                    {
                        Marshal.Release(zero);
                    }
                }
            }
            return flag;
        }

        private void SendMethodReturn(IMessage req, Exception except)
        {
            if (this._tracker != null)
            {
                IntPtr zero = IntPtr.Zero;
                try
                {
                    IMethodCallMessage message = req as IMethodCallMessage;
                    if (this.IsRealCall(message.MethodBase))
                    {
                        if (Util.ExtendedLifetime)
                        {
                            zero = this.SupportsInterface(ref _s_IID_IManagedObjectInfo);
                        }
                        else
                        {
                            zero = this.GetOuterIUnknown();
                        }
                        MethodBase method = ReflectionCache.ConvertToInterfaceMI(message.MethodBase) as MethodBase;
                        if (method != null)
                        {
                            this._tracker.SendMethodReturn(zero, method, except);
                        }
                    }
                }
                catch (Exception exception)
                {
                    if ((exception is NullReferenceException) || (exception is SEHException))
                    {
                        throw;
                    }
                }
                finally
                {
                    if (zero != IntPtr.Zero)
                    {
                        Marshal.Release(zero);
                    }
                }
            }
        }

        [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        public override void SetCOMIUnknown(IntPtr i)
        {
            bool flag = false;
            if ((this._gitCookie == 0) && !Util.ExtendedLifetime)
            {
                try
                {
                    if (i == IntPtr.Zero)
                    {
                        flag = true;
                        i = Marshal.GetIUnknownForObject(this.GetTransparentProxy());
                    }
                    this._gitCookie = Proxy.StoreObject(i);
                    if (this._proxyTearoff != null)
                    {
                        Marshal.ChangeWrapperHandleStrength(this._proxyTearoff, true);
                    }
                    Marshal.ChangeWrapperHandleStrength(this.GetTransparentProxy(), true);
                }
                finally
                {
                    if (flag && (i != IntPtr.Zero))
                    {
                        Marshal.Release(i);
                    }
                }
            }
        }

        internal void SetInPool(bool fInPool, IntPtr pPooledObject)
        {
            if (!fInPool)
            {
                Proxy.PoolMark(pPooledObject);
                this._pPoolUnk = pPooledObject;
            }
        }

        private void SetupContext(bool construction)
        {
            IntPtr currentContextToken = Proxy.GetCurrentContextToken();
            if (this._token != currentContextToken)
            {
                if (this._token != NegativeOne)
                {
                    this.ReleaseContext();
                }
                this._token = currentContextToken;
                this._context = Proxy.GetCurrentContext();
                this._tracker = Proxy.FindTracker(this._context);
            }
            if (!this._filterConstructors)
            {
                RealProxy.SetStubData(this, this._token);
            }
            if ((this.IsJitActivated && !this._tabled) && !construction)
            {
                IdentityTable.AddObject(this._token, this.GetTransparentProxy());
                this._tabled = true;
            }
        }

        private static void ShutdownDomain(object sender, EventArgs e)
        {
            _exitCleanupThread.Set();
            _Wakeup.Set();
            while (!CleanupQueues(true))
            {
            }
        }

        public override IntPtr SupportsInterface(ref Guid iid)
        {
            if (_s_IID_IObjectControl.Equals((Guid) iid))
            {
                return Marshal.GetComInterfaceForObject(this.GetProxyTearoff(), typeof(IObjectControl));
            }
            if (_s_IID_IObjectConstruct.Equals((Guid) iid))
            {
                return Marshal.GetComInterfaceForObject(this.GetProxyTearoff(), typeof(IObjectConstruct));
            }
            if (_s_IID_IManagedPoolAction.Equals((Guid) iid))
            {
                return Marshal.GetComInterfaceForObject(this, typeof(IManagedPoolAction));
            }
            if (Util.ExtendedLifetime && _s_IID_IManagedObjectInfo.Equals((Guid) iid))
            {
                return Marshal.GetComInterfaceForObject(this._scstub, typeof(System.EnterpriseServices.IManagedObjectInfo));
            }
            return IntPtr.Zero;
        }

        internal void SuppressFinalizeServer()
        {
            GC.SuppressFinalize(base.GetUnwrappedServer());
        }

        void IManagedPoolAction.LastRelease()
        {
            if (this.IsObjectPooled && (base.GetUnwrappedServer() != null))
            {
                this.ReleaseContext();
                IntPtr currentContextToken = Proxy.GetCurrentContextToken();
                IntPtr currentContext = Proxy.GetCurrentContext();
                try
                {
                    RealProxy.SetStubData(this, currentContextToken);
                    ((ServicedComponent) this.GetTransparentProxy())._callFinalize(!this._fFinalized && !this._fReturnedByFinalizer);
                    GC.SuppressFinalize(this);
                }
                finally
                {
                    Marshal.Release(currentContext);
                    RealProxy.SetStubData(this, this._token);
                }
            }
        }

        internal bool AreMethodsSecure
        {
            get
            {
                return this._fUseIntfDispatch;
            }
        }

        internal IntPtr HomeToken
        {
            get
            {
                return this._token;
            }
        }

        internal bool IsJitActivated
        {
            get
            {
                return this._fIsJitActivated;
            }
        }

        internal bool IsObjectPooled
        {
            get
            {
                return this._fIsObjectPooled;
            }
        }

        internal bool IsProxyDeactivated
        {
            get
            {
                return !this._fIsActive;
            }
        }
    }
}

