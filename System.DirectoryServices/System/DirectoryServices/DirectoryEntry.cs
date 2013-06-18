namespace System.DirectoryServices
{
    using System;
    using System.ComponentModel;
    using System.DirectoryServices.ActiveDirectory;
    using System.DirectoryServices.Design;
    using System.DirectoryServices.Interop;
    using System.Globalization;
    using System.Net;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Threading;

    [TypeConverter(typeof(DirectoryEntryConverter)), DSDescription("DirectoryEntryDesc"), SecurityPermission(SecurityAction.Assert, Flags=SecurityPermissionFlag.UnmanagedCode), EnvironmentPermission(SecurityAction.Assert, Unrestricted=true), DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
    public class DirectoryEntry : System.ComponentModel.Component
    {
        private System.DirectoryServices.Interop.UnsafeNativeMethods.IAds adsObject;
        internal bool allowMultipleChange;
        private AuthenticationTypes authenticationType;
        private bool cacheFilled;
        private NetworkCredential credentials;
        private bool disposed;
        private bool justCreated;
        private ActiveDirectorySecurity objectSecurity;
        private bool objectSecurityInitialized;
        private bool objectSecurityModified;
        private DirectoryEntryConfiguration options;
        private bool passwordIsNull;
        private string path;
        internal bool propertiesAlreadyEnumerated;
        private PropertyCollection propertyCollection;
        private static string securityDescriptorProperty = "ntSecurityDescriptor";
        private bool useCache;
        private bool userNameIsNull;

        [DirectoryServicesPermission(SecurityAction.Demand, Unrestricted=true)]
        public DirectoryEntry()
        {
            this.path = "";
            this.useCache = true;
            this.authenticationType = AuthenticationTypes.Secure;
            this.options = new DirectoryEntryConfiguration(this);
        }

        [DirectoryServicesPermission(SecurityAction.Demand, Unrestricted=true)]
        public DirectoryEntry(object adsObject) : this(adsObject, true, null, null, AuthenticationTypes.Secure, true)
        {
        }

        [DirectoryServicesPermission(SecurityAction.Demand, Unrestricted=true)]
        public DirectoryEntry(string path) : this()
        {
            this.Path = path;
        }

        [DirectoryServicesPermission(SecurityAction.Demand, Unrestricted=true)]
        public DirectoryEntry(string path, string username, string password) : this(path, username, password, AuthenticationTypes.Secure)
        {
        }

        [DirectoryServicesPermission(SecurityAction.Demand, Unrestricted=true)]
        public DirectoryEntry(string path, string username, string password, AuthenticationTypes authenticationType) : this(path)
        {
            this.credentials = new NetworkCredential(username, password);
            if (username == null)
            {
                this.userNameIsNull = true;
            }
            if (password == null)
            {
                this.passwordIsNull = true;
            }
            this.authenticationType = authenticationType;
        }

        internal DirectoryEntry(object adsObject, bool useCache, string username, string password, AuthenticationTypes authenticationType) : this(adsObject, useCache, username, password, authenticationType, false)
        {
        }

        internal DirectoryEntry(string path, bool useCache, string username, string password, AuthenticationTypes authenticationType)
        {
            this.path = "";
            this.useCache = true;
            this.authenticationType = AuthenticationTypes.Secure;
            this.path = path;
            this.useCache = useCache;
            this.credentials = new NetworkCredential(username, password);
            if (username == null)
            {
                this.userNameIsNull = true;
            }
            if (password == null)
            {
                this.passwordIsNull = true;
            }
            this.authenticationType = authenticationType;
            this.options = new DirectoryEntryConfiguration(this);
        }

        internal DirectoryEntry(object adsObject, bool useCache, string username, string password, AuthenticationTypes authenticationType, bool AdsObjIsExternal)
        {
            this.path = "";
            this.useCache = true;
            this.authenticationType = AuthenticationTypes.Secure;
            this.adsObject = adsObject as System.DirectoryServices.Interop.UnsafeNativeMethods.IAds;
            if (this.adsObject == null)
            {
                throw new ArgumentException(Res.GetString("DSDoesNotImplementIADs"));
            }
            this.path = this.adsObject.ADsPath;
            this.useCache = useCache;
            this.authenticationType = authenticationType;
            this.credentials = new NetworkCredential(username, password);
            if (username == null)
            {
                this.userNameIsNull = true;
            }
            if (password == null)
            {
                this.passwordIsNull = true;
            }
            if (!useCache)
            {
                this.CommitChanges();
            }
            this.options = new DirectoryEntryConfiguration(this);
            if (!AdsObjIsExternal)
            {
                this.InitADsObjectOptions();
            }
        }

        private void Bind()
        {
            this.Bind(true);
        }

        internal void Bind(bool throwIfFail)
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(base.GetType().Name);
            }
            if (this.adsObject == null)
            {
                string path = this.Path;
                if ((path == null) || (path.Length == 0))
                {
                    DirectoryEntry entry = new DirectoryEntry("LDAP://RootDSE", true, null, null, AuthenticationTypes.Secure);
                    string str2 = (string) entry.Properties["defaultNamingContext"][0];
                    entry.Dispose();
                    path = "LDAP://" + str2;
                }
                if (Thread.CurrentThread.GetApartmentState() == ApartmentState.Unknown)
                {
                    Thread.CurrentThread.SetApartmentState(ApartmentState.MTA);
                }
                System.Guid iid = new System.Guid("00000000-0000-0000-c000-000000000046");
                object ppObject = null;
                int hr = System.DirectoryServices.Interop.UnsafeNativeMethods.ADsOpenObject(path, this.GetUsername(), this.GetPassword(), (int) this.authenticationType, ref iid, out ppObject);
                if (hr != 0)
                {
                    if (throwIfFail)
                    {
                        throw COMExceptionHelper.CreateFormattedComException(hr);
                    }
                }
                else
                {
                    this.adsObject = (System.DirectoryServices.Interop.UnsafeNativeMethods.IAds) ppObject;
                }
                this.InitADsObjectOptions();
            }
        }

        internal DirectoryEntry CloneBrowsable()
        {
            return new DirectoryEntry(this.Path, this.UsePropertyCache, this.GetUsername(), this.GetPassword(), this.AuthenticationType);
        }

        public void Close()
        {
            this.Unbind();
        }

        public void CommitChanges()
        {
            if (this.justCreated)
            {
                try
                {
                    this.SetObjectSecurityInCache();
                    this.adsObject.SetInfo();
                }
                catch (COMException exception)
                {
                    throw COMExceptionHelper.CreateFormattedComException(exception);
                }
                this.justCreated = false;
                this.objectSecurityInitialized = false;
                this.objectSecurityModified = false;
                this.propertyCollection = null;
            }
            else if ((this.useCache || ((this.objectSecurity != null) && this.objectSecurity.IsModified())) && this.Bound)
            {
                try
                {
                    this.SetObjectSecurityInCache();
                    this.adsObject.SetInfo();
                    this.objectSecurityInitialized = false;
                    this.objectSecurityModified = false;
                }
                catch (COMException exception2)
                {
                    throw COMExceptionHelper.CreateFormattedComException(exception2);
                }
                this.propertyCollection = null;
            }
        }

        internal void CommitIfNotCaching()
        {
            if ((!this.justCreated && !this.useCache) && this.Bound)
            {
                new DirectoryServicesPermission(PermissionState.Unrestricted).Demand();
                try
                {
                    this.SetObjectSecurityInCache();
                    this.adsObject.SetInfo();
                    this.objectSecurityInitialized = false;
                    this.objectSecurityModified = false;
                }
                catch (COMException exception)
                {
                    throw COMExceptionHelper.CreateFormattedComException(exception);
                }
                this.propertyCollection = null;
            }
        }

        public DirectoryEntry CopyTo(DirectoryEntry newParent)
        {
            return this.CopyTo(newParent, null);
        }

        public DirectoryEntry CopyTo(DirectoryEntry newParent, string newName)
        {
            if (!newParent.IsContainer)
            {
                throw new InvalidOperationException(Res.GetString("DSNotAContainer", new object[] { newParent.Path }));
            }
            object adsObject = null;
            try
            {
                adsObject = newParent.ContainerObject.CopyHere(this.Path, newName);
            }
            catch (COMException exception)
            {
                throw COMExceptionHelper.CreateFormattedComException(exception);
            }
            return new DirectoryEntry(adsObject, newParent.UsePropertyCache, this.GetUsername(), this.GetPassword(), this.AuthenticationType);
        }

        public void DeleteTree()
        {
            if (!(this.AdsObject is System.DirectoryServices.Interop.UnsafeNativeMethods.IAdsDeleteOps))
            {
                throw new InvalidOperationException(Res.GetString("DSCannotDelete"));
            }
            System.DirectoryServices.Interop.UnsafeNativeMethods.IAdsDeleteOps adsObject = (System.DirectoryServices.Interop.UnsafeNativeMethods.IAdsDeleteOps) this.AdsObject;
            try
            {
                adsObject.DeleteObject(0);
            }
            catch (COMException exception)
            {
                throw COMExceptionHelper.CreateFormattedComException(exception);
            }
            GC.KeepAlive(this);
        }

        protected override void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                this.Unbind();
                this.disposed = true;
            }
            base.Dispose(disposing);
        }

        public static bool Exists(string path)
        {
            bool bound;
            DirectoryEntry entry = new DirectoryEntry(path);
            try
            {
                entry.Bind(true);
                bound = entry.Bound;
            }
            catch (COMException exception)
            {
                if (((exception.ErrorCode != -2147016656) && (exception.ErrorCode != -2147024893)) && (exception.ErrorCode != -2147022676))
                {
                    throw;
                }
                bound = false;
            }
            finally
            {
                entry.Dispose();
            }
            return bound;
        }

        internal void FillCache(string propertyName)
        {
            if (this.UsePropertyCache)
            {
                if (!this.cacheFilled)
                {
                    this.RefreshCache();
                    this.cacheFilled = true;
                }
            }
            else
            {
                this.Bind();
                try
                {
                    if (propertyName.Length > 0)
                    {
                        this.adsObject.GetInfoEx(new object[] { propertyName }, 0);
                    }
                    else
                    {
                        this.adsObject.GetInfo();
                    }
                }
                catch (COMException exception)
                {
                    throw COMExceptionHelper.CreateFormattedComException(exception);
                }
            }
        }

        private ActiveDirectorySecurity GetObjectSecurityFromCache()
        {
            try
            {
                if (!this.JustCreated)
                {
                    SecurityMasks securityMasks = this.Options.SecurityMasks;
                    this.RefreshCache(new string[] { securityDescriptorProperty });
                    if (!(this.NativeObject is System.DirectoryServices.Interop.UnsafeNativeMethods.IAdsPropertyList))
                    {
                        throw new NotSupportedException(Res.GetString("DSPropertyListUnsupported"));
                    }
                    System.DirectoryServices.Interop.UnsafeNativeMethods.IAdsPropertyList nativeObject = (System.DirectoryServices.Interop.UnsafeNativeMethods.IAdsPropertyList) this.NativeObject;
                    System.DirectoryServices.Interop.UnsafeNativeMethods.IAdsPropertyEntry propertyItem = (System.DirectoryServices.Interop.UnsafeNativeMethods.IAdsPropertyEntry) nativeObject.GetPropertyItem(securityDescriptorProperty, 8);
                    GC.KeepAlive(this);
                    object[] values = (object[]) propertyItem.Values;
                    if (values.Length < 1)
                    {
                        throw new InvalidOperationException(Res.GetString("DSSDNoValues"));
                    }
                    if (values.Length > 1)
                    {
                        throw new NotSupportedException(Res.GetString("DSMultipleSDNotSupported"));
                    }
                    System.DirectoryServices.Interop.UnsafeNativeMethods.IAdsPropertyValue value2 = (System.DirectoryServices.Interop.UnsafeNativeMethods.IAdsPropertyValue) values[0];
                    return new ActiveDirectorySecurity((byte[]) value2.OctetString, securityMasks);
                }
                return null;
            }
            catch (COMException exception)
            {
                if (exception.ErrorCode != -2147463155)
                {
                    throw;
                }
                return null;
            }
        }

        internal string GetPassword()
        {
            if ((this.credentials != null) && !this.passwordIsNull)
            {
                return this.credentials.Password;
            }
            return null;
        }

        internal string GetUsername()
        {
            if ((this.credentials != null) && !this.userNameIsNull)
            {
                return this.credentials.UserName;
            }
            return null;
        }

        internal void InitADsObjectOptions()
        {
            if (this.adsObject is System.DirectoryServices.Interop.UnsafeNativeMethods.IAdsObjectOptions2)
            {
                object obj2 = null;
                int hr = 0;
                hr = ((System.DirectoryServices.Interop.UnsafeNativeMethods.IAdsObjectOptions2) this.adsObject).GetOption(8, out obj2);
                if (hr != 0)
                {
                    if ((hr != -2147467263) && (hr != -2147463160))
                    {
                        throw COMExceptionHelper.CreateFormattedComException(hr);
                    }
                }
                else
                {
                    System.DirectoryServices.Interop.Variant variant = new System.DirectoryServices.Interop.Variant {
                        varType = 11,
                        boolvalue = -1
                    };
                    ((System.DirectoryServices.Interop.UnsafeNativeMethods.IAdsObjectOptions2) this.adsObject).SetOption(8, variant);
                    this.allowMultipleChange = true;
                }
            }
        }

        public object Invoke(string methodName, params object[] args)
        {
            object nativeObject = this.NativeObject;
            Type type = nativeObject.GetType();
            object adsObject = null;
            try
            {
                adsObject = type.InvokeMember(methodName, BindingFlags.InvokeMethod, null, nativeObject, args, CultureInfo.InvariantCulture);
                GC.KeepAlive(this);
            }
            catch (COMException exception)
            {
                throw COMExceptionHelper.CreateFormattedComException(exception);
            }
            catch (TargetInvocationException exception2)
            {
                if ((exception2.InnerException != null) && (exception2.InnerException is COMException))
                {
                    COMException innerException = (COMException) exception2.InnerException;
                    throw new TargetInvocationException(exception2.Message, COMExceptionHelper.CreateFormattedComException(innerException));
                }
                throw exception2;
            }
            if (adsObject is System.DirectoryServices.Interop.UnsafeNativeMethods.IAds)
            {
                return new DirectoryEntry(adsObject, this.UsePropertyCache, this.GetUsername(), this.GetPassword(), this.AuthenticationType);
            }
            return adsObject;
        }

        [ComVisible(false)]
        public object InvokeGet(string propertyName)
        {
            object nativeObject = this.NativeObject;
            Type type = nativeObject.GetType();
            object obj3 = null;
            try
            {
                obj3 = type.InvokeMember(propertyName, BindingFlags.GetProperty, null, nativeObject, null, CultureInfo.InvariantCulture);
                GC.KeepAlive(this);
            }
            catch (COMException exception)
            {
                throw COMExceptionHelper.CreateFormattedComException(exception);
            }
            catch (TargetInvocationException exception2)
            {
                if ((exception2.InnerException != null) && (exception2.InnerException is COMException))
                {
                    COMException innerException = (COMException) exception2.InnerException;
                    throw new TargetInvocationException(exception2.Message, COMExceptionHelper.CreateFormattedComException(innerException));
                }
                throw exception2;
            }
            return obj3;
        }

        [ComVisible(false)]
        public void InvokeSet(string propertyName, params object[] args)
        {
            object nativeObject = this.NativeObject;
            Type type = nativeObject.GetType();
            try
            {
                type.InvokeMember(propertyName, BindingFlags.SetProperty, null, nativeObject, args, CultureInfo.InvariantCulture);
                GC.KeepAlive(this);
            }
            catch (COMException exception)
            {
                throw COMExceptionHelper.CreateFormattedComException(exception);
            }
            catch (TargetInvocationException exception2)
            {
                if ((exception2.InnerException != null) && (exception2.InnerException is COMException))
                {
                    COMException innerException = (COMException) exception2.InnerException;
                    throw new TargetInvocationException(exception2.Message, COMExceptionHelper.CreateFormattedComException(innerException));
                }
                throw exception2;
            }
        }

        public void MoveTo(DirectoryEntry newParent)
        {
            this.MoveTo(newParent, null);
        }

        public void MoveTo(DirectoryEntry newParent, string newName)
        {
            object obj2 = null;
            if (!(newParent.AdsObject is System.DirectoryServices.Interop.UnsafeNativeMethods.IAdsContainer))
            {
                throw new InvalidOperationException(Res.GetString("DSNotAContainer", new object[] { newParent.Path }));
            }
            try
            {
                if (this.AdsObject.ADsPath.StartsWith("WinNT:", StringComparison.Ordinal))
                {
                    string aDsPath = this.AdsObject.ADsPath;
                    string str2 = newParent.AdsObject.ADsPath;
                    if (Utils.Compare(aDsPath, 0, str2.Length, str2, 0, str2.Length) == 0)
                    {
                        uint compareFlags = ((Utils.NORM_IGNORENONSPACE | Utils.NORM_IGNOREKANATYPE) | Utils.NORM_IGNOREWIDTH) | Utils.SORT_STRINGSORT;
                        if (Utils.Compare(aDsPath, 0, str2.Length, str2, 0, str2.Length, compareFlags) != 0)
                        {
                            aDsPath = str2 + aDsPath.Substring(str2.Length);
                        }
                    }
                    obj2 = newParent.ContainerObject.MoveHere(aDsPath, newName);
                }
                else
                {
                    obj2 = newParent.ContainerObject.MoveHere(this.Path, newName);
                }
            }
            catch (COMException exception)
            {
                throw COMExceptionHelper.CreateFormattedComException(exception);
            }
            if (this.Bound)
            {
                Marshal.ReleaseComObject(this.adsObject);
            }
            this.adsObject = (System.DirectoryServices.Interop.UnsafeNativeMethods.IAds) obj2;
            this.path = this.adsObject.ADsPath;
            this.InitADsObjectOptions();
            if (!this.useCache)
            {
                this.CommitChanges();
            }
            else
            {
                this.RefreshCache();
            }
        }

        public void RefreshCache()
        {
            this.Bind();
            try
            {
                this.adsObject.GetInfo();
            }
            catch (COMException exception)
            {
                throw COMExceptionHelper.CreateFormattedComException(exception);
            }
            this.cacheFilled = true;
            this.propertyCollection = null;
            this.objectSecurityInitialized = false;
            this.objectSecurityModified = false;
        }

        public void RefreshCache(string[] propertyNames)
        {
            this.Bind();
            object[] vProperties = new object[propertyNames.Length];
            for (int i = 0; i < propertyNames.Length; i++)
            {
                vProperties[i] = propertyNames[i];
            }
            try
            {
                this.AdsObject.GetInfoEx(vProperties, 0);
            }
            catch (COMException exception)
            {
                throw COMExceptionHelper.CreateFormattedComException(exception);
            }
            this.cacheFilled = true;
            if ((this.propertyCollection != null) && (propertyNames != null))
            {
                for (int j = 0; j < propertyNames.Length; j++)
                {
                    if (propertyNames[j] != null)
                    {
                        string key = propertyNames[j].ToLower(CultureInfo.InvariantCulture);
                        this.propertyCollection.valueTable.Remove(key);
                        string[] strArray = key.Split(new char[] { ';' });
                        if (strArray.Length != 1)
                        {
                            string str2 = "";
                            for (int k = 0; k < strArray.Length; k++)
                            {
                                if (!strArray[k].StartsWith("range=", StringComparison.Ordinal))
                                {
                                    str2 = str2 + strArray[k] + ";";
                                }
                            }
                            str2 = str2.Remove(str2.Length - 1, 1);
                            this.propertyCollection.valueTable.Remove(str2);
                        }
                        if (string.Compare(propertyNames[j], securityDescriptorProperty, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            this.objectSecurityInitialized = false;
                            this.objectSecurityModified = false;
                        }
                    }
                }
            }
        }

        public void Rename(string newName)
        {
            this.MoveTo(this.Parent, newName);
        }

        private void SetObjectSecurityInCache()
        {
            if ((this.objectSecurity != null) && (this.objectSecurityModified || this.objectSecurity.IsModified()))
            {
                System.DirectoryServices.Interop.UnsafeNativeMethods.IAdsPropertyValue value2 = (System.DirectoryServices.Interop.UnsafeNativeMethods.IAdsPropertyValue) new System.DirectoryServices.Interop.UnsafeNativeMethods.PropertyValue();
                value2.ADsType = 8;
                value2.OctetString = this.objectSecurity.GetSecurityDescriptorBinaryForm();
                System.DirectoryServices.Interop.UnsafeNativeMethods.IAdsPropertyEntry varData = (System.DirectoryServices.Interop.UnsafeNativeMethods.IAdsPropertyEntry) new System.DirectoryServices.Interop.UnsafeNativeMethods.PropertyEntry();
                varData.Name = securityDescriptorProperty;
                varData.ADsType = 8;
                varData.ControlCode = 2;
                varData.Values = new object[] { value2 };
                ((System.DirectoryServices.Interop.UnsafeNativeMethods.IAdsPropertyList) this.NativeObject).PutPropertyItem(varData);
            }
        }

        private void Unbind()
        {
            if (this.adsObject != null)
            {
                Marshal.ReleaseComObject(this.adsObject);
            }
            this.adsObject = null;
            this.propertyCollection = null;
            this.objectSecurityInitialized = false;
            this.objectSecurityModified = false;
        }

        internal System.DirectoryServices.Interop.UnsafeNativeMethods.IAds AdsObject
        {
            get
            {
                this.Bind();
                return this.adsObject;
            }
        }

        [DefaultValue(1), DSDescription("DSAuthenticationType")]
        public AuthenticationTypes AuthenticationType
        {
            get
            {
                return this.authenticationType;
            }
            set
            {
                if (this.authenticationType != value)
                {
                    this.authenticationType = value;
                    this.Unbind();
                }
            }
        }

        private bool Bound
        {
            get
            {
                return (this.adsObject != null);
            }
        }

        [DSDescription("DSChildren"), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DirectoryEntries Children
        {
            get
            {
                return new DirectoryEntries(this);
            }
        }

        internal System.DirectoryServices.Interop.UnsafeNativeMethods.IAdsContainer ContainerObject
        {
            get
            {
                this.Bind();
                return (System.DirectoryServices.Interop.UnsafeNativeMethods.IAdsContainer) this.adsObject;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), DSDescription("DSGuid")]
        public System.Guid Guid
        {
            get
            {
                string nativeGuid = this.NativeGuid;
                if (nativeGuid.Length != 0x20)
                {
                    return new System.Guid(nativeGuid);
                }
                byte[] b = new byte[0x10];
                for (int i = 0; i < 0x10; i++)
                {
                    b[i] = Convert.ToByte(new string(new char[] { nativeGuid[i * 2], nativeGuid[(i * 2) + 1] }), 0x10);
                }
                return new System.Guid(b);
            }
        }

        internal bool IsContainer
        {
            get
            {
                this.Bind();
                return (this.adsObject is System.DirectoryServices.Interop.UnsafeNativeMethods.IAdsContainer);
            }
        }

        internal bool JustCreated
        {
            get
            {
                return this.justCreated;
            }
            set
            {
                this.justCreated = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false), DSDescription("DSName")]
        public string Name
        {
            get
            {
                this.Bind();
                string name = this.adsObject.Name;
                GC.KeepAlive(this);
                return name;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), DSDescription("DSNativeGuid"), Browsable(false)]
        public string NativeGuid
        {
            get
            {
                this.FillCache("GUID");
                string gUID = this.adsObject.GUID;
                GC.KeepAlive(this);
                return gUID;
            }
        }

        [DSDescription("DSNativeObject"), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public object NativeObject
        {
            get
            {
                this.Bind();
                return this.adsObject;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), DSDescription("DSObjectSecurity"), Browsable(false)]
        public ActiveDirectorySecurity ObjectSecurity
        {
            get
            {
                if (!this.objectSecurityInitialized)
                {
                    this.objectSecurity = this.GetObjectSecurityFromCache();
                    this.objectSecurityInitialized = true;
                }
                return this.objectSecurity;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                this.objectSecurity = value;
                this.objectSecurityInitialized = true;
                this.objectSecurityModified = true;
                this.CommitIfNotCaching();
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false), DSDescription("DSOptions"), ComVisible(false)]
        public DirectoryEntryConfiguration Options
        {
            get
            {
                if (this.AdsObject is System.DirectoryServices.Interop.UnsafeNativeMethods.IAdsObjectOptions)
                {
                    return this.options;
                }
                return null;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false), DSDescription("DSParent")]
        public DirectoryEntry Parent
        {
            get
            {
                this.Bind();
                return new DirectoryEntry(this.adsObject.Parent, this.UsePropertyCache, this.GetUsername(), this.GetPassword(), this.AuthenticationType);
            }
        }

        [DSDescription("DSPassword"), DefaultValue((string) null), Browsable(false)]
        public string Password
        {
            set
            {
                if (value != this.GetPassword())
                {
                    if (this.credentials == null)
                    {
                        this.credentials = new NetworkCredential();
                        this.userNameIsNull = true;
                    }
                    if (value == null)
                    {
                        this.passwordIsNull = true;
                    }
                    else
                    {
                        this.passwordIsNull = false;
                    }
                    this.credentials.Password = value;
                    this.Unbind();
                }
            }
        }

        [SettingsBindable(true), TypeConverter("System.Diagnostics.Design.StringValueConverter, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), DefaultValue(""), DSDescription("DSPath")]
        public string Path
        {
            get
            {
                return this.path;
            }
            set
            {
                if (value == null)
                {
                    value = "";
                }
                if (Utils.Compare(this.path, value) != 0)
                {
                    this.path = value;
                    this.Unbind();
                }
            }
        }

        [DSDescription("DSProperties"), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public PropertyCollection Properties
        {
            get
            {
                if (this.propertyCollection == null)
                {
                    this.propertyCollection = new PropertyCollection(this);
                }
                return this.propertyCollection;
            }
        }

        [Browsable(false), DSDescription("DSSchemaClassName"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string SchemaClassName
        {
            get
            {
                this.Bind();
                string str = this.adsObject.Class;
                GC.KeepAlive(this);
                return str;
            }
        }

        [DSDescription("DSSchemaEntry"), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DirectoryEntry SchemaEntry
        {
            get
            {
                this.Bind();
                return new DirectoryEntry(this.adsObject.Schema, this.UsePropertyCache, this.GetUsername(), this.GetPassword(), this.AuthenticationType);
            }
        }

        [DefaultValue(true), DSDescription("DSUsePropertyCache")]
        public bool UsePropertyCache
        {
            get
            {
                return this.useCache;
            }
            set
            {
                if (value != this.useCache)
                {
                    if (!value)
                    {
                        this.CommitChanges();
                    }
                    this.cacheFilled = false;
                    this.useCache = value;
                }
            }
        }

        [DSDescription("DSUsername"), TypeConverter("System.Diagnostics.Design.StringValueConverter, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), DefaultValue((string) null), Browsable(false)]
        public string Username
        {
            get
            {
                if ((this.credentials != null) && !this.userNameIsNull)
                {
                    return this.credentials.UserName;
                }
                return null;
            }
            set
            {
                if (value != this.GetUsername())
                {
                    if (this.credentials == null)
                    {
                        this.credentials = new NetworkCredential();
                        this.passwordIsNull = true;
                    }
                    if (value == null)
                    {
                        this.userNameIsNull = true;
                    }
                    else
                    {
                        this.userNameIsNull = false;
                    }
                    this.credentials.UserName = value;
                    this.Unbind();
                }
            }
        }
    }
}

