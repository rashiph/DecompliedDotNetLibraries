namespace System.Management
{
    using System;
    using System.ComponentModel;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Threading;

    [TypeConverter(typeof(ManagementScopeConverter))]
    public class ManagementScope : ICloneable
    {
        internal bool IsDefaulted;
        private ConnectionOptions options;
        private ManagementPath validatedPath;
        private IWbemServices wbemServices;

        internal event IdentifierChangedEventHandler IdentifierChanged;

        public ManagementScope() : this(new ManagementPath(ManagementPath.DefaultPath.Path))
        {
            this.IsDefaulted = true;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ManagementScope(ManagementPath path) : this(path, (ConnectionOptions) null)
        {
        }

        public ManagementScope(string path) : this(new ManagementPath(path), (ConnectionOptions) null)
        {
        }

        public ManagementScope(ManagementPath path, ConnectionOptions options)
        {
            if (path != null)
            {
                this.prvpath = ManagementPath._Clone(path, new IdentifierChangedEventHandler(this.HandleIdentifierChange));
            }
            else
            {
                this.prvpath = ManagementPath._Clone(null);
            }
            if (options != null)
            {
                this.options = ConnectionOptions._Clone(options, new IdentifierChangedEventHandler(this.HandleIdentifierChange));
            }
            else
            {
                this.options = null;
            }
            this.IsDefaulted = false;
        }

        internal ManagementScope(ManagementPath path, ManagementScope scope) : this(path, (scope != null) ? scope.options : null)
        {
        }

        public ManagementScope(string path, ConnectionOptions options) : this(new ManagementPath(path), options)
        {
        }

        internal ManagementScope(ManagementPath path, IWbemServices wbemServices, ConnectionOptions options)
        {
            if (path != null)
            {
                this.Path = path;
            }
            if (options != null)
            {
                this.Options = options;
            }
            this.wbemServices = wbemServices;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal static ManagementScope _Clone(ManagementScope scope)
        {
            return _Clone(scope, null);
        }

        internal static ManagementScope _Clone(ManagementScope scope, IdentifierChangedEventHandler handler)
        {
            ManagementScope scope2 = new ManagementScope(null, null, null);
            if (handler != null)
            {
                scope2.IdentifierChanged = handler;
            }
            else if (scope != null)
            {
                scope2.IdentifierChanged = new IdentifierChangedEventHandler(scope.HandleIdentifierChange);
            }
            if (scope == null)
            {
                scope2.prvpath = ManagementPath._Clone(ManagementPath.DefaultPath, new IdentifierChangedEventHandler(scope2.HandleIdentifierChange));
                scope2.IsDefaulted = true;
                scope2.wbemServices = null;
                scope2.options = null;
                return scope2;
            }
            if (scope.prvpath == null)
            {
                scope2.prvpath = ManagementPath._Clone(ManagementPath.DefaultPath, new IdentifierChangedEventHandler(scope2.HandleIdentifierChange));
                scope2.IsDefaulted = true;
            }
            else
            {
                scope2.prvpath = ManagementPath._Clone(scope.prvpath, new IdentifierChangedEventHandler(scope2.HandleIdentifierChange));
                scope2.IsDefaulted = scope.IsDefaulted;
            }
            scope2.wbemServices = scope.wbemServices;
            if (scope.options != null)
            {
                scope2.options = ConnectionOptions._Clone(scope.options, new IdentifierChangedEventHandler(scope2.HandleIdentifierChange));
            }
            return scope2;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ManagementScope Clone()
        {
            return _Clone(this);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void Connect()
        {
            this.Initialize();
        }

        private void FireIdentifierChanged()
        {
            if (this.IdentifierChanged != null)
            {
                this.IdentifierChanged(this, null);
            }
        }

        internal IWbemServices GetIWbemServices()
        {
            IWbemServices wbemServices = this.wbemServices;
            IntPtr iUnknownForObject = Marshal.GetIUnknownForObject(this.wbemServices);
            object objectForIUnknown = Marshal.GetObjectForIUnknown(iUnknownForObject);
            Marshal.Release(iUnknownForObject);
            if (!object.ReferenceEquals(objectForIUnknown, this.wbemServices))
            {
                SecurityHandler securityHandler = this.GetSecurityHandler();
                securityHandler.SecureIUnknown(objectForIUnknown);
                wbemServices = (IWbemServices) objectForIUnknown;
                securityHandler.Secure(wbemServices);
            }
            return wbemServices;
        }

        internal SecuredConnectHandler GetSecuredConnectHandler()
        {
            return new SecuredConnectHandler(this);
        }

        internal SecuredIEnumWbemClassObjectHandler GetSecuredIEnumWbemClassObjectHandler(IEnumWbemClassObject pEnumWbemClassObject)
        {
            return new SecuredIEnumWbemClassObjectHandler(this, pEnumWbemClassObject);
        }

        internal SecuredIWbemServicesHandler GetSecuredIWbemServicesHandler(IWbemServices pWbemServiecs)
        {
            return new SecuredIWbemServicesHandler(this, pWbemServiecs);
        }

        internal SecurityHandler GetSecurityHandler()
        {
            return new SecurityHandler(this);
        }

        private void HandleIdentifierChange(object sender, IdentifierChangedEventArgs args)
        {
            this.wbemServices = null;
            this.FireIdentifierChanged();
        }

        internal void Initialize()
        {
            if (this.prvpath == null)
            {
                throw new InvalidOperationException();
            }
            if (!this.IsConnected)
            {
                lock (this)
                {
                    if (!this.IsConnected)
                    {
                        if (!MTAHelper.IsNoContextMTA())
                        {
                            new ThreadDispatch(new ThreadDispatch.ThreadWorkerMethodWithParam(this.InitializeGuts)) { Parameter = this }.Start();
                        }
                        else
                        {
                            this.InitializeGuts(this);
                        }
                    }
                }
            }
        }

        private void InitializeGuts(object o)
        {
            ManagementScope scope = (ManagementScope) o;
            IWbemLocator locator1 = (IWbemLocator) new WbemLocator();
            if (this.options == null)
            {
                scope.Options = new ConnectionOptions();
            }
            string namespacePath = scope.prvpath.GetNamespacePath(8);
            if ((namespacePath == null) || (namespacePath.Length == 0))
            {
                bool flag;
                namespacePath = scope.prvpath.SetNamespacePath(ManagementPath.DefaultPath.Path, out flag);
            }
            int errorCode = 0;
            scope.wbemServices = null;
            if ((Environment.OSVersion.Platform == PlatformID.Win32NT) && (((Environment.OSVersion.Version.Major == 5) && (Environment.OSVersion.Version.Minor >= 1)) || (Environment.OSVersion.Version.Major >= 6)))
            {
                scope.options.Flags |= 0x80;
            }
            try
            {
                scope.options.GetPassword();
                errorCode = this.GetSecuredConnectHandler().ConnectNSecureIWbemServices(namespacePath, ref scope.wbemServices);
            }
            catch (COMException exception)
            {
                ManagementException.ThrowWithExtendedInfo(exception);
            }
            if ((errorCode & 0xfffff000L) == 0x80041000L)
            {
                ManagementException.ThrowWithExtendedInfo((ManagementStatus) errorCode);
            }
            else if ((errorCode & 0x80000000L) != 0L)
            {
                Marshal.ThrowExceptionForHR(errorCode);
            }
        }

        [DllImport("rpcrt4.dll")]
        private static extern int RpcMgmtEnableIdleCleanup();
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        object ICloneable.Clone()
        {
            return this.Clone();
        }

        public bool IsConnected
        {
            get
            {
                return (null != this.wbemServices);
            }
        }

        public ConnectionOptions Options
        {
            get
            {
                if (this.options == null)
                {
                    return (this.options = ConnectionOptions._Clone(null));
                }
                return this.options;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                if (this.options != null)
                {
                    this.options.IdentifierChanged -= new IdentifierChangedEventHandler(this.HandleIdentifierChange);
                }
                this.options = ConnectionOptions._Clone(value, new IdentifierChangedEventHandler(this.HandleIdentifierChange));
                this.HandleIdentifierChange(this, null);
            }
        }

        public ManagementPath Path
        {
            get
            {
                if (this.prvpath == null)
                {
                    ManagementPath path;
                    this.prvpath = path = ManagementPath._Clone(null);
                    return path;
                }
                return this.prvpath;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                if (this.prvpath != null)
                {
                    this.prvpath.IdentifierChanged -= new IdentifierChangedEventHandler(this.HandleIdentifierChange);
                }
                this.IsDefaulted = false;
                this.prvpath = ManagementPath._Clone(value, new IdentifierChangedEventHandler(this.HandleIdentifierChange));
                this.HandleIdentifierChange(this, null);
            }
        }

        private ManagementPath prvpath
        {
            get
            {
                return this.validatedPath;
            }
            set
            {
                if ((value != null) && !ManagementPath.IsValidNamespaceSyntax(value.Path))
                {
                    ManagementException.ThrowWithExtendedInfo(ManagementStatus.InvalidNamespace);
                }
                this.validatedPath = value;
            }
        }
    }
}

