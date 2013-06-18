namespace System.Management
{
    using System;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Threading;

    [Serializable]
    public class ManagementObject : ManagementBaseObject, ICloneable
    {
        internal const string ID = "ID";
        internal ObjectGetOptions options;
        internal ManagementPath path;
        private bool putButNotGot;
        internal const string RETURNVALUE = "RETURNVALUE";
        internal ManagementScope scope;
        private IWbemClassObjectFreeThreaded wmiClass;

        internal event IdentifierChangedEventHandler IdentifierChanged;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ManagementObject() : this((ManagementScope) null, (ManagementPath) null, null)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ManagementObject(ManagementPath path) : this(null, path, null)
        {
        }

        public ManagementObject(string path) : this(null, new ManagementPath(path), null)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ManagementObject(ManagementPath path, ObjectGetOptions options) : this(null, path, options)
        {
        }

        protected ManagementObject(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            this.ManagementObjectCTOR(null, null, null);
        }

        public ManagementObject(string path, ObjectGetOptions options) : this(new ManagementPath(path), options)
        {
        }

        public ManagementObject(ManagementScope scope, ManagementPath path, ObjectGetOptions options) : base(null)
        {
            this.ManagementObjectCTOR(scope, path, options);
        }

        public ManagementObject(string scopeString, string pathString, ObjectGetOptions options) : this(new ManagementScope(scopeString), new ManagementPath(pathString), options)
        {
        }

        public override object Clone()
        {
            if (this.PutButNotGot)
            {
                this.Get();
                this.PutButNotGot = false;
            }
            IWbemClassObjectFreeThreaded ppCopy = null;
            int errorCode = base.wbemObject.Clone_(out ppCopy);
            if (errorCode < 0)
            {
                if ((errorCode & 0xfffff000L) == 0x80041000L)
                {
                    ManagementException.ThrowWithExtendedInfo((ManagementStatus) errorCode);
                }
                else
                {
                    Marshal.ThrowExceptionForHR(errorCode);
                }
            }
            return GetManagementObject(ppCopy, this);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ManagementPath CopyTo(ManagementPath path)
        {
            return this.CopyTo(path, null);
        }

        public ManagementPath CopyTo(string path)
        {
            return this.CopyTo(new ManagementPath(path), null);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void CopyTo(ManagementOperationObserver watcher, ManagementPath path)
        {
            this.CopyTo(watcher, path, null);
        }

        public void CopyTo(ManagementOperationObserver watcher, string path)
        {
            this.CopyTo(watcher, new ManagementPath(path), null);
        }

        public ManagementPath CopyTo(ManagementPath path, PutOptions options)
        {
            this.Initialize(false);
            ManagementScope scope = null;
            scope = new ManagementScope(path, this.scope);
            scope.Initialize();
            PutOptions options2 = (options != null) ? options : new PutOptions();
            IWbemServices iWbemServices = scope.GetIWbemServices();
            ManagementPath path2 = null;
            IntPtr zero = IntPtr.Zero;
            IntPtr pUnk = IntPtr.Zero;
            IWbemCallResult callResult = null;
            SecurityHandler securityHandler = null;
            int errorCode = 0;
            try
            {
                int num2;
                securityHandler = scope.GetSecurityHandler();
                zero = Marshal.AllocHGlobal(IntPtr.Size);
                Marshal.WriteIntPtr(zero, IntPtr.Zero);
                if (base.IsClass)
                {
                    errorCode = this.scope.GetSecuredIWbemServicesHandler(iWbemServices).PutClass_(base.wbemObject, options2.Flags | 0x10, options2.GetContext(), zero);
                }
                else
                {
                    errorCode = this.scope.GetSecuredIWbemServicesHandler(iWbemServices).PutInstance_(base.wbemObject, options2.Flags | 0x10, options2.GetContext(), zero);
                }
                pUnk = Marshal.ReadIntPtr(zero);
                callResult = (IWbemCallResult) Marshal.GetObjectForIUnknown(pUnk);
                errorCode = callResult.GetCallStatus_(-1, out num2);
                if (errorCode >= 0)
                {
                    errorCode = num2;
                }
                if (errorCode < 0)
                {
                    if ((errorCode & 0xfffff000L) == 0x80041000L)
                    {
                        ManagementException.ThrowWithExtendedInfo((ManagementStatus) errorCode);
                    }
                    else
                    {
                        Marshal.ThrowExceptionForHR(errorCode);
                    }
                }
                path2 = this.GetPath(callResult);
                path2.NamespacePath = path.GetNamespacePath(8);
            }
            finally
            {
                if (securityHandler != null)
                {
                    securityHandler.Reset();
                }
                if (zero != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(zero);
                }
                if (pUnk != IntPtr.Zero)
                {
                    Marshal.Release(pUnk);
                }
                if (callResult != null)
                {
                    Marshal.ReleaseComObject(callResult);
                }
            }
            return path2;
        }

        public ManagementPath CopyTo(string path, PutOptions options)
        {
            return this.CopyTo(new ManagementPath(path), options);
        }

        public void CopyTo(ManagementOperationObserver watcher, ManagementPath path, PutOptions options)
        {
            if (watcher == null)
            {
                throw new ArgumentNullException("watcher");
            }
            this.Initialize(false);
            ManagementScope scope = null;
            scope = new ManagementScope(path, this.scope);
            scope.Initialize();
            PutOptions options2 = (options != null) ? ((PutOptions) options.Clone()) : new PutOptions();
            if (watcher.HaveListenersForProgress)
            {
                options2.SendStatus = true;
            }
            WmiEventSink eventSink = watcher.GetNewPutSink(scope, options2.Context, path.GetNamespacePath(8), base.ClassName);
            IWbemServices iWbemServices = scope.GetIWbemServices();
            SecurityHandler securityHandler = null;
            int errorCode = 0;
            securityHandler = scope.GetSecurityHandler();
            if (base.IsClass)
            {
                errorCode = scope.GetSecuredIWbemServicesHandler(iWbemServices).PutClassAsync_(base.wbemObject, options2.Flags, options2.GetContext(), eventSink.Stub);
            }
            else
            {
                errorCode = scope.GetSecuredIWbemServicesHandler(iWbemServices).PutInstanceAsync_(base.wbemObject, options2.Flags, options2.GetContext(), eventSink.Stub);
            }
            if (securityHandler != null)
            {
                securityHandler.Reset();
            }
            if (errorCode < 0)
            {
                watcher.RemoveSink(eventSink);
                if ((errorCode & 0xfffff000L) == 0x80041000L)
                {
                    ManagementException.ThrowWithExtendedInfo((ManagementStatus) errorCode);
                }
                else
                {
                    Marshal.ThrowExceptionForHR(errorCode);
                }
            }
        }

        public void CopyTo(ManagementOperationObserver watcher, string path, PutOptions options)
        {
            this.CopyTo(watcher, new ManagementPath(path), options);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void Delete()
        {
            this.Delete((DeleteOptions) null);
        }

        public void Delete(DeleteOptions options)
        {
            if ((this.path == null) || (this.path.Path.Length == 0))
            {
                throw new InvalidOperationException();
            }
            this.Initialize(false);
            DeleteOptions options2 = (options != null) ? options : new DeleteOptions();
            IWbemServices iWbemServices = this.scope.GetIWbemServices();
            SecurityHandler securityHandler = null;
            int errorCode = 0;
            try
            {
                securityHandler = this.scope.GetSecurityHandler();
                if (base.IsClass)
                {
                    errorCode = this.scope.GetSecuredIWbemServicesHandler(iWbemServices).DeleteClass_(this.path.RelativePath, options2.Flags, options2.GetContext(), IntPtr.Zero);
                }
                else
                {
                    errorCode = this.scope.GetSecuredIWbemServicesHandler(iWbemServices).DeleteInstance_(this.path.RelativePath, options2.Flags, options2.GetContext(), IntPtr.Zero);
                }
                if (errorCode < 0)
                {
                    if ((errorCode & 0xfffff000L) == 0x80041000L)
                    {
                        ManagementException.ThrowWithExtendedInfo((ManagementStatus) errorCode);
                    }
                    else
                    {
                        Marshal.ThrowExceptionForHR(errorCode);
                    }
                }
            }
            finally
            {
                if (securityHandler != null)
                {
                    securityHandler.Reset();
                }
            }
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void Delete(ManagementOperationObserver watcher)
        {
            this.Delete(watcher, null);
        }

        public void Delete(ManagementOperationObserver watcher, DeleteOptions options)
        {
            if ((this.path == null) || (this.path.Path.Length == 0))
            {
                throw new InvalidOperationException();
            }
            if (watcher == null)
            {
                throw new ArgumentNullException("watcher");
            }
            this.Initialize(false);
            DeleteOptions options2 = (options != null) ? ((DeleteOptions) options.Clone()) : new DeleteOptions();
            if (watcher.HaveListenersForProgress)
            {
                options2.SendStatus = true;
            }
            IWbemServices iWbemServices = this.scope.GetIWbemServices();
            WmiEventSink newSink = watcher.GetNewSink(this.scope, options2.Context);
            SecurityHandler securityHandler = null;
            int errorCode = 0;
            securityHandler = this.scope.GetSecurityHandler();
            if (base.IsClass)
            {
                errorCode = this.scope.GetSecuredIWbemServicesHandler(iWbemServices).DeleteClassAsync_(this.path.RelativePath, options2.Flags, options2.GetContext(), newSink.Stub);
            }
            else
            {
                errorCode = this.scope.GetSecuredIWbemServicesHandler(iWbemServices).DeleteInstanceAsync_(this.path.RelativePath, options2.Flags, options2.GetContext(), newSink.Stub);
            }
            if (securityHandler != null)
            {
                securityHandler.Reset();
            }
            if (errorCode < 0)
            {
                watcher.RemoveSink(newSink);
                if ((errorCode & 0xfffff000L) == 0x80041000L)
                {
                    ManagementException.ThrowWithExtendedInfo((ManagementStatus) errorCode);
                }
                else
                {
                    Marshal.ThrowExceptionForHR(errorCode);
                }
            }
        }

        public void Dispose()
        {
            if (this.wmiClass != null)
            {
                this.wmiClass.Dispose();
                this.wmiClass = null;
            }
            base.Dispose();
            GC.SuppressFinalize(this);
        }

        internal void FireIdentifierChanged()
        {
            if (this.IdentifierChanged != null)
            {
                this.IdentifierChanged(this, null);
            }
        }

        public void Get()
        {
            IWbemClassObjectFreeThreaded ppObject = null;
            this.Initialize(false);
            if ((this.path == null) || (this.path.Path.Length == 0))
            {
                throw new InvalidOperationException();
            }
            ObjectGetOptions options = (this.options == null) ? new ObjectGetOptions() : this.options;
            SecurityHandler securityHandler = null;
            int errorCode = 0;
            try
            {
                securityHandler = this.scope.GetSecurityHandler();
                errorCode = this.scope.GetSecuredIWbemServicesHandler(this.scope.GetIWbemServices()).GetObject_(this.path.RelativePath, options.Flags, options.GetContext(), ref ppObject, IntPtr.Zero);
                if (errorCode < 0)
                {
                    if ((errorCode & 0xfffff000L) == 0x80041000L)
                    {
                        ManagementException.ThrowWithExtendedInfo((ManagementStatus) errorCode);
                    }
                    else
                    {
                        Marshal.ThrowExceptionForHR(errorCode);
                    }
                }
                base.wbemObject = ppObject;
            }
            finally
            {
                if (securityHandler != null)
                {
                    securityHandler.Reset();
                }
            }
        }

        public void Get(ManagementOperationObserver watcher)
        {
            this.Initialize(false);
            if ((this.path == null) || (this.path.Path.Length == 0))
            {
                throw new InvalidOperationException();
            }
            if (watcher == null)
            {
                throw new ArgumentNullException("watcher");
            }
            IWbemServices iWbemServices = this.scope.GetIWbemServices();
            ObjectGetOptions options = ObjectGetOptions._Clone(this.options);
            WmiGetEventSink eventSink = watcher.GetNewGetSink(this.scope, options.Context, this);
            if (watcher.HaveListenersForProgress)
            {
                options.SendStatus = true;
            }
            SecurityHandler securityHandler = null;
            int errorCode = 0;
            securityHandler = this.scope.GetSecurityHandler();
            errorCode = this.scope.GetSecuredIWbemServicesHandler(iWbemServices).GetObjectAsync_(this.path.RelativePath, options.Flags, options.GetContext(), eventSink.Stub);
            if (securityHandler != null)
            {
                securityHandler.Reset();
            }
            if (errorCode < 0)
            {
                watcher.RemoveSink(eventSink);
                if ((errorCode & 0xfffff000L) == 0x80041000L)
                {
                    ManagementException.ThrowWithExtendedInfo((ManagementStatus) errorCode);
                }
                else
                {
                    Marshal.ThrowExceptionForHR(errorCode);
                }
            }
        }

        internal static ManagementObject GetManagementObject(IWbemClassObjectFreeThreaded wbemObject, ManagementObject mgObj)
        {
            ManagementObject obj2 = new ManagementObject {
                wbemObject = wbemObject
            };
            if (mgObj != null)
            {
                obj2.scope = ManagementScope._Clone(mgObj.scope);
                if (mgObj.path != null)
                {
                    obj2.path = ManagementPath._Clone(mgObj.path);
                }
                if (mgObj.options != null)
                {
                    obj2.options = ObjectGetOptions._Clone(mgObj.options);
                }
            }
            return obj2;
        }

        internal static ManagementObject GetManagementObject(IWbemClassObjectFreeThreaded wbemObject, ManagementScope scope)
        {
            ManagementObject obj2 = new ManagementObject {
                wbemObject = wbemObject,
                path = new ManagementPath(ManagementPath.GetManagementPath(wbemObject))
            };
            obj2.path.IdentifierChanged += new IdentifierChangedEventHandler(obj2.HandleIdentifierChange);
            obj2.scope = ManagementScope._Clone(scope, new IdentifierChangedEventHandler(obj2.HandleIdentifierChange));
            return obj2;
        }

        public ManagementBaseObject GetMethodParameters(string methodName)
        {
            ManagementBaseObject obj2;
            IWbemClassObjectFreeThreaded threaded;
            IWbemClassObjectFreeThreaded threaded2;
            this.GetMethodParameters(methodName, out obj2, out threaded, out threaded2);
            return obj2;
        }

        private void GetMethodParameters(string methodName, out ManagementBaseObject inParameters, out IWbemClassObjectFreeThreaded inParametersClass, out IWbemClassObjectFreeThreaded outParametersClass)
        {
            inParameters = null;
            inParametersClass = null;
            outParametersClass = null;
            if (methodName == null)
            {
                throw new ArgumentNullException("methodName");
            }
            this.Initialize(false);
            if (this.wmiClass == null)
            {
                ManagementPath classPath = this.ClassPath;
                if ((classPath == null) || !classPath.IsClass)
                {
                    throw new InvalidOperationException();
                }
                ManagementClass class2 = new ManagementClass(this.scope, classPath, null);
                class2.Get();
                this.wmiClass = class2.wbemObject;
            }
            int errorCode = 0;
            errorCode = this.wmiClass.GetMethod_(methodName, 0, out inParametersClass, out outParametersClass);
            if (errorCode == -2147217406)
            {
                errorCode = -2147217323;
            }
            if ((errorCode >= 0) && (inParametersClass != null))
            {
                IWbemClassObjectFreeThreaded ppNewInstance = null;
                errorCode = inParametersClass.SpawnInstance_(0, out ppNewInstance);
                if (errorCode >= 0)
                {
                    inParameters = new ManagementBaseObject(ppNewInstance);
                }
            }
            if (errorCode < 0)
            {
                if ((errorCode & 0xfffff000L) == 0x80041000L)
                {
                    ManagementException.ThrowWithExtendedInfo((ManagementStatus) errorCode);
                }
                else
                {
                    Marshal.ThrowExceptionForHR(errorCode);
                }
            }
        }

        protected override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }

        private ManagementPath GetPath(IWbemCallResult callResult)
        {
            ManagementPath path = null;
            try
            {
                string pstrResultString = null;
                if (callResult.GetResultString_(-1, out pstrResultString) >= 0)
                {
                    path = new ManagementPath(this.scope.Path.Path) {
                        RelativePath = pstrResultString
                    };
                }
                else
                {
                    object propertyValue = base.GetPropertyValue("__PATH");
                    if (propertyValue != null)
                    {
                        path = new ManagementPath((string) propertyValue);
                    }
                    else
                    {
                        propertyValue = base.GetPropertyValue("__RELPATH");
                        if (propertyValue != null)
                        {
                            path = new ManagementPath(this.scope.Path.Path) {
                                RelativePath = (string) propertyValue
                            };
                        }
                    }
                }
            }
            catch
            {
            }
            if (path == null)
            {
                path = new ManagementPath();
            }
            return path;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ManagementObjectCollection GetRelated()
        {
            return this.GetRelated((string) null);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void GetRelated(ManagementOperationObserver watcher)
        {
            this.GetRelated(watcher, null);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ManagementObjectCollection GetRelated(string relatedClass)
        {
            return this.GetRelated(relatedClass, null, null, null, null, null, false, null);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void GetRelated(ManagementOperationObserver watcher, string relatedClass)
        {
            this.GetRelated(watcher, relatedClass, null, null, null, null, null, false, null);
        }

        public ManagementObjectCollection GetRelated(string relatedClass, string relationshipClass, string relationshipQualifier, string relatedQualifier, string relatedRole, string thisRole, bool classDefinitionsOnly, EnumerationOptions options)
        {
            if ((this.path == null) || (this.path.Path.Length == 0))
            {
                throw new InvalidOperationException();
            }
            this.Initialize(false);
            IEnumWbemClassObject ppEnum = null;
            EnumerationOptions options2 = (options != null) ? options : new EnumerationOptions();
            RelatedObjectQuery query = new RelatedObjectQuery(this.path.Path, relatedClass, relationshipClass, relationshipQualifier, relatedQualifier, relatedRole, thisRole, classDefinitionsOnly);
            options2.EnumerateDeep = true;
            SecurityHandler securityHandler = null;
            int errorCode = 0;
            try
            {
                securityHandler = this.scope.GetSecurityHandler();
                errorCode = this.scope.GetSecuredIWbemServicesHandler(this.scope.GetIWbemServices()).ExecQuery_(query.QueryLanguage, query.QueryString, options2.Flags, options2.GetContext(), ref ppEnum);
                if (errorCode < 0)
                {
                    if ((errorCode & 0xfffff000L) == 0x80041000L)
                    {
                        ManagementException.ThrowWithExtendedInfo((ManagementStatus) errorCode);
                    }
                    else
                    {
                        Marshal.ThrowExceptionForHR(errorCode);
                    }
                }
            }
            finally
            {
                if (securityHandler != null)
                {
                    securityHandler.Reset();
                }
            }
            return new ManagementObjectCollection(this.scope, options2, ppEnum);
        }

        public void GetRelated(ManagementOperationObserver watcher, string relatedClass, string relationshipClass, string relationshipQualifier, string relatedQualifier, string relatedRole, string thisRole, bool classDefinitionsOnly, EnumerationOptions options)
        {
            if ((this.path == null) || (this.path.Path.Length == 0))
            {
                throw new InvalidOperationException();
            }
            this.Initialize(true);
            if (watcher == null)
            {
                throw new ArgumentNullException("watcher");
            }
            EnumerationOptions options2 = (options != null) ? ((EnumerationOptions) options.Clone()) : new EnumerationOptions();
            options2.ReturnImmediately = false;
            if (watcher.HaveListenersForProgress)
            {
                options2.SendStatus = true;
            }
            WmiEventSink newSink = watcher.GetNewSink(this.scope, options2.Context);
            RelatedObjectQuery query = new RelatedObjectQuery(this.path.Path, relatedClass, relationshipClass, relationshipQualifier, relatedQualifier, relatedRole, thisRole, classDefinitionsOnly);
            options2.EnumerateDeep = true;
            SecurityHandler securityHandler = null;
            int errorCode = 0;
            securityHandler = this.scope.GetSecurityHandler();
            errorCode = this.scope.GetSecuredIWbemServicesHandler(this.scope.GetIWbemServices()).ExecQueryAsync_(query.QueryLanguage, query.QueryString, options2.Flags, options2.GetContext(), newSink.Stub);
            securityHandler.Reset();
            if (errorCode < 0)
            {
                watcher.RemoveSink(newSink);
                if ((errorCode & 0xfffff000L) == 0x80041000L)
                {
                    ManagementException.ThrowWithExtendedInfo((ManagementStatus) errorCode);
                }
                else
                {
                    Marshal.ThrowExceptionForHR(errorCode);
                }
            }
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ManagementObjectCollection GetRelationships()
        {
            return this.GetRelationships((string) null);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void GetRelationships(ManagementOperationObserver watcher)
        {
            this.GetRelationships(watcher, null);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ManagementObjectCollection GetRelationships(string relationshipClass)
        {
            return this.GetRelationships(relationshipClass, null, null, false, null);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void GetRelationships(ManagementOperationObserver watcher, string relationshipClass)
        {
            this.GetRelationships(watcher, relationshipClass, null, null, false, null);
        }

        public ManagementObjectCollection GetRelationships(string relationshipClass, string relationshipQualifier, string thisRole, bool classDefinitionsOnly, EnumerationOptions options)
        {
            if ((this.path == null) || (this.path.Path.Length == 0))
            {
                throw new InvalidOperationException();
            }
            this.Initialize(false);
            IEnumWbemClassObject ppEnum = null;
            EnumerationOptions options2 = (options != null) ? options : new EnumerationOptions();
            RelationshipQuery query = new RelationshipQuery(this.path.Path, relationshipClass, relationshipQualifier, thisRole, classDefinitionsOnly);
            options2.EnumerateDeep = true;
            SecurityHandler securityHandler = null;
            int errorCode = 0;
            try
            {
                securityHandler = this.scope.GetSecurityHandler();
                errorCode = this.scope.GetSecuredIWbemServicesHandler(this.scope.GetIWbemServices()).ExecQuery_(query.QueryLanguage, query.QueryString, options2.Flags, options2.GetContext(), ref ppEnum);
                if (errorCode < 0)
                {
                    if ((errorCode & 0xfffff000L) == 0x80041000L)
                    {
                        ManagementException.ThrowWithExtendedInfo((ManagementStatus) errorCode);
                    }
                    else
                    {
                        Marshal.ThrowExceptionForHR(errorCode);
                    }
                }
            }
            finally
            {
                if (securityHandler != null)
                {
                    securityHandler.Reset();
                }
            }
            return new ManagementObjectCollection(this.scope, options2, ppEnum);
        }

        public void GetRelationships(ManagementOperationObserver watcher, string relationshipClass, string relationshipQualifier, string thisRole, bool classDefinitionsOnly, EnumerationOptions options)
        {
            if ((this.path == null) || (this.path.Path.Length == 0))
            {
                throw new InvalidOperationException();
            }
            if (watcher == null)
            {
                throw new ArgumentNullException("watcher");
            }
            this.Initialize(false);
            EnumerationOptions options2 = (options != null) ? ((EnumerationOptions) options.Clone()) : new EnumerationOptions();
            options2.ReturnImmediately = false;
            if (watcher.HaveListenersForProgress)
            {
                options2.SendStatus = true;
            }
            WmiEventSink newSink = watcher.GetNewSink(this.scope, options2.Context);
            RelationshipQuery query = new RelationshipQuery(this.path.Path, relationshipClass, relationshipQualifier, thisRole, classDefinitionsOnly);
            options2.EnumerateDeep = true;
            SecurityHandler securityHandler = null;
            int errorCode = 0;
            securityHandler = this.scope.GetSecurityHandler();
            errorCode = this.scope.GetSecuredIWbemServicesHandler(this.scope.GetIWbemServices()).ExecQueryAsync_(query.QueryLanguage, query.QueryString, options2.Flags, options2.GetContext(), newSink.Stub);
            if (securityHandler != null)
            {
                securityHandler.Reset();
            }
            if (errorCode < 0)
            {
                watcher.RemoveSink(newSink);
                if ((errorCode & 0xfffff000L) == 0x80041000L)
                {
                    ManagementException.ThrowWithExtendedInfo((ManagementStatus) errorCode);
                }
                else
                {
                    Marshal.ThrowExceptionForHR(errorCode);
                }
            }
        }

        private void HandleIdentifierChange(object sender, IdentifierChangedEventArgs e)
        {
            base.wbemObject = null;
        }

        internal void HandleObjectPut(object sender, InternalObjectPutEventArgs e)
        {
            try
            {
                if (sender is WmiEventSink)
                {
                    ((WmiEventSink) sender).InternalObjectPut -= new InternalObjectPutEventHandler(this.HandleObjectPut);
                    this.putButNotGot = true;
                    this.path.SetRelativePath(e.Path.RelativePath);
                }
            }
            catch
            {
            }
        }

        internal override void Initialize(bool getObject)
        {
            bool flag = false;
            lock (this)
            {
                if (this.path == null)
                {
                    this.path = new ManagementPath();
                    this.path.IdentifierChanged += new IdentifierChangedEventHandler(this.HandleIdentifierChange);
                }
                if (!this.IsBound && getObject)
                {
                    flag = true;
                }
                if (this.scope == null)
                {
                    string namespacePath = this.path.GetNamespacePath(8);
                    if (0 < namespacePath.Length)
                    {
                        this.scope = new ManagementScope(namespacePath);
                    }
                    else
                    {
                        this.scope = new ManagementScope();
                    }
                    this.scope.IdentifierChanged += new IdentifierChangedEventHandler(this.HandleIdentifierChange);
                }
                else if ((this.scope.Path == null) || this.scope.Path.IsEmpty)
                {
                    string path = this.path.GetNamespacePath(8);
                    if (0 < path.Length)
                    {
                        this.scope.Path = new ManagementPath(path);
                    }
                    else
                    {
                        this.scope.Path = ManagementPath.DefaultPath;
                    }
                }
                lock (this.scope)
                {
                    if (!this.scope.IsConnected)
                    {
                        this.scope.Initialize();
                        if (getObject)
                        {
                            flag = true;
                        }
                    }
                    if (flag)
                    {
                        if (this.options == null)
                        {
                            this.options = new ObjectGetOptions();
                            this.options.IdentifierChanged += new IdentifierChangedEventHandler(this.HandleIdentifierChange);
                        }
                        IWbemClassObjectFreeThreaded ppObject = null;
                        IWbemServices iWbemServices = this.scope.GetIWbemServices();
                        SecurityHandler securityHandler = null;
                        int errorCode = 0;
                        try
                        {
                            securityHandler = this.scope.GetSecurityHandler();
                            string strObjectPath = null;
                            string relativePath = this.path.RelativePath;
                            if (relativePath.Length > 0)
                            {
                                strObjectPath = relativePath;
                            }
                            errorCode = this.scope.GetSecuredIWbemServicesHandler(iWbemServices).GetObject_(strObjectPath, this.options.Flags, this.options.GetContext(), ref ppObject, IntPtr.Zero);
                            if (errorCode >= 0)
                            {
                                base.wbemObject = ppObject;
                                object pVal = null;
                                int pType = 0;
                                int plFlavor = 0;
                                errorCode = base.wbemObject.Get_("__PATH", 0, ref pVal, ref pType, ref plFlavor);
                                if (errorCode >= 0)
                                {
                                    this.path = (DBNull.Value != pVal) ? new ManagementPath((string) pVal) : new ManagementPath();
                                    this.path.IdentifierChanged += new IdentifierChangedEventHandler(this.HandleIdentifierChange);
                                }
                            }
                            if (errorCode < 0)
                            {
                                if ((errorCode & 0xfffff000L) == 0x80041000L)
                                {
                                    ManagementException.ThrowWithExtendedInfo((ManagementStatus) errorCode);
                                }
                                else
                                {
                                    Marshal.ThrowExceptionForHR(errorCode);
                                }
                            }
                        }
                        finally
                        {
                            if (securityHandler != null)
                            {
                                securityHandler.Reset();
                            }
                        }
                    }
                }
            }
        }

        public object InvokeMethod(string methodName, object[] args)
        {
            ManagementBaseObject obj3;
            IWbemClassObjectFreeThreaded threaded;
            IWbemClassObjectFreeThreaded threaded2;
            if ((this.path == null) || (this.path.Path.Length == 0))
            {
                throw new InvalidOperationException();
            }
            if (methodName == null)
            {
                throw new ArgumentNullException("methodName");
            }
            this.Initialize(false);
            this.GetMethodParameters(methodName, out obj3, out threaded, out threaded2);
            MapInParameters(args, obj3, threaded);
            ManagementBaseObject outParams = this.InvokeMethod(methodName, obj3, null);
            return MapOutParameters(args, outParams, threaded2);
        }

        public void InvokeMethod(ManagementOperationObserver watcher, string methodName, object[] args)
        {
            ManagementBaseObject obj2;
            IWbemClassObjectFreeThreaded threaded;
            IWbemClassObjectFreeThreaded threaded2;
            if ((this.path == null) || (this.path.Path.Length == 0))
            {
                throw new InvalidOperationException();
            }
            if (watcher == null)
            {
                throw new ArgumentNullException("watcher");
            }
            if (methodName == null)
            {
                throw new ArgumentNullException("methodName");
            }
            this.Initialize(false);
            this.GetMethodParameters(methodName, out obj2, out threaded, out threaded2);
            MapInParameters(args, obj2, threaded);
            this.InvokeMethod(watcher, methodName, obj2, null);
        }

        public ManagementBaseObject InvokeMethod(string methodName, ManagementBaseObject inParameters, InvokeMethodOptions options)
        {
            ManagementBaseObject obj2 = null;
            if ((this.path == null) || (this.path.Path.Length == 0))
            {
                throw new InvalidOperationException();
            }
            if (methodName == null)
            {
                throw new ArgumentNullException("methodName");
            }
            this.Initialize(false);
            InvokeMethodOptions options2 = (options != null) ? options : new InvokeMethodOptions();
            this.scope.GetIWbemServices();
            SecurityHandler securityHandler = null;
            int errorCode = 0;
            try
            {
                securityHandler = this.scope.GetSecurityHandler();
                IWbemClassObjectFreeThreaded pInParams = (inParameters == null) ? null : inParameters.wbemObject;
                IWbemClassObjectFreeThreaded ppOutParams = null;
                errorCode = this.scope.GetSecuredIWbemServicesHandler(this.scope.GetIWbemServices()).ExecMethod_(this.path.RelativePath, methodName, options2.Flags, options2.GetContext(), pInParams, ref ppOutParams, IntPtr.Zero);
                if (errorCode < 0)
                {
                    if ((errorCode & 0xfffff000L) == 0x80041000L)
                    {
                        ManagementException.ThrowWithExtendedInfo((ManagementStatus) errorCode);
                    }
                    else
                    {
                        Marshal.ThrowExceptionForHR(errorCode);
                    }
                }
                if (ppOutParams != null)
                {
                    obj2 = new ManagementBaseObject(ppOutParams);
                }
            }
            finally
            {
                if (securityHandler != null)
                {
                    securityHandler.Reset();
                }
            }
            return obj2;
        }

        public void InvokeMethod(ManagementOperationObserver watcher, string methodName, ManagementBaseObject inParameters, InvokeMethodOptions options)
        {
            if ((this.path == null) || (this.path.Path.Length == 0))
            {
                throw new InvalidOperationException();
            }
            if (watcher == null)
            {
                throw new ArgumentNullException("watcher");
            }
            if (methodName == null)
            {
                throw new ArgumentNullException("methodName");
            }
            this.Initialize(false);
            InvokeMethodOptions options2 = (options != null) ? ((InvokeMethodOptions) options.Clone()) : new InvokeMethodOptions();
            if (watcher.HaveListenersForProgress)
            {
                options2.SendStatus = true;
            }
            WmiEventSink newSink = watcher.GetNewSink(this.scope, options2.Context);
            SecurityHandler securityHandler = null;
            int errorCode = 0;
            securityHandler = this.scope.GetSecurityHandler();
            IWbemClassObjectFreeThreaded pInParams = null;
            if (inParameters != null)
            {
                pInParams = inParameters.wbemObject;
            }
            errorCode = this.scope.GetSecuredIWbemServicesHandler(this.scope.GetIWbemServices()).ExecMethodAsync_(this.path.RelativePath, methodName, options2.Flags, options2.GetContext(), pInParams, newSink.Stub);
            if (securityHandler != null)
            {
                securityHandler.Reset();
            }
            if (errorCode < 0)
            {
                watcher.RemoveSink(newSink);
                if ((errorCode & 0xfffff000L) == 0x80041000L)
                {
                    ManagementException.ThrowWithExtendedInfo((ManagementStatus) errorCode);
                }
                else
                {
                    Marshal.ThrowExceptionForHR(errorCode);
                }
            }
        }

        private void ManagementObjectCTOR(ManagementScope scope, ManagementPath path, ObjectGetOptions options)
        {
            string namespacePath = string.Empty;
            if ((path != null) && !path.IsEmpty)
            {
                if ((base.GetType() == typeof(ManagementObject)) && path.IsClass)
                {
                    throw new ArgumentOutOfRangeException("path");
                }
                if ((base.GetType() == typeof(ManagementClass)) && path.IsInstance)
                {
                    throw new ArgumentOutOfRangeException("path");
                }
                namespacePath = path.GetNamespacePath(8);
                if ((scope != null) && (scope.Path.NamespacePath.Length > 0))
                {
                    path = new ManagementPath(path.RelativePath);
                    path.NamespacePath = scope.Path.GetNamespacePath(8);
                }
                if (path.IsClass || path.IsInstance)
                {
                    this.path = ManagementPath._Clone(path, new IdentifierChangedEventHandler(this.HandleIdentifierChange));
                }
                else
                {
                    this.path = ManagementPath._Clone(null, new IdentifierChangedEventHandler(this.HandleIdentifierChange));
                }
            }
            if (options != null)
            {
                this.options = ObjectGetOptions._Clone(options, new IdentifierChangedEventHandler(this.HandleIdentifierChange));
            }
            if (scope != null)
            {
                this.scope = ManagementScope._Clone(scope, new IdentifierChangedEventHandler(this.HandleIdentifierChange));
            }
            else if (namespacePath.Length > 0)
            {
                this.scope = new ManagementScope(namespacePath);
                this.scope.IdentifierChanged += new IdentifierChangedEventHandler(this.HandleIdentifierChange);
            }
            this.IdentifierChanged += new IdentifierChangedEventHandler(this.HandleIdentifierChange);
            this.putButNotGot = false;
        }

        private static void MapInParameters(object[] args, ManagementBaseObject inParams, IWbemClassObjectFreeThreaded inParamsClass)
        {
            int errorCode = 0;
            if (((inParamsClass != null) && (args != null)) && (0 < args.Length))
            {
                int upperBound = args.GetUpperBound(0);
                int lowerBound = args.GetLowerBound(0);
                int num4 = upperBound - lowerBound;
                errorCode = inParamsClass.BeginEnumeration_(0x40);
                while (errorCode >= 0)
                {
                    object pVal = null;
                    int pType = 0;
                    string strName = null;
                    IWbemQualifierSetFreeThreaded ppQualSet = null;
                    errorCode = inParamsClass.Next_(0, ref strName, ref pVal, ref pType, ref pType);
                    if (errorCode >= 0)
                    {
                        if (strName == null)
                        {
                            break;
                        }
                        errorCode = inParamsClass.GetPropertyQualifierSet_(strName, out ppQualSet);
                        if (errorCode >= 0)
                        {
                            try
                            {
                                object obj3 = 0;
                                ppQualSet.Get_("ID", 0, ref obj3, ref pType);
                                int num6 = (int) obj3;
                                if ((0 <= num6) && (num4 >= num6))
                                {
                                    inParams[strName] = args[lowerBound + num6];
                                }
                            }
                            finally
                            {
                                ppQualSet.Dispose();
                            }
                        }
                    }
                }
                if (errorCode < 0)
                {
                    if ((errorCode & 0xfffff000L) == 0x80041000L)
                    {
                        ManagementException.ThrowWithExtendedInfo((ManagementStatus) errorCode);
                    }
                    else
                    {
                        Marshal.ThrowExceptionForHR(errorCode);
                    }
                }
            }
        }

        private static object MapOutParameters(object[] args, ManagementBaseObject outParams, IWbemClassObjectFreeThreaded outParamsClass)
        {
            object obj2 = null;
            int upperBound = 0;
            int lowerBound = 0;
            int num3 = 0;
            int errorCode = 0;
            if (outParamsClass != null)
            {
                if ((args != null) && (0 < args.Length))
                {
                    upperBound = args.GetUpperBound(0);
                    lowerBound = args.GetLowerBound(0);
                    num3 = upperBound - lowerBound;
                }
                errorCode = outParamsClass.BeginEnumeration_(0x40);
                while (errorCode >= 0)
                {
                    object pVal = null;
                    int pType = 0;
                    string strName = null;
                    IWbemQualifierSetFreeThreaded ppQualSet = null;
                    errorCode = outParamsClass.Next_(0, ref strName, ref pVal, ref pType, ref pType);
                    if (errorCode >= 0)
                    {
                        if (strName == null)
                        {
                            break;
                        }
                        if (string.Compare(strName, "RETURNVALUE", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            obj2 = outParams["RETURNVALUE"];
                        }
                        else
                        {
                            errorCode = outParamsClass.GetPropertyQualifierSet_(strName, out ppQualSet);
                            if (errorCode >= 0)
                            {
                                try
                                {
                                    object obj4 = 0;
                                    ppQualSet.Get_("ID", 0, ref obj4, ref pType);
                                    int num6 = (int) obj4;
                                    if ((0 <= num6) && (num3 >= num6))
                                    {
                                        args[lowerBound + num6] = outParams[strName];
                                    }
                                }
                                finally
                                {
                                    ppQualSet.Dispose();
                                }
                            }
                        }
                    }
                }
                if (errorCode < 0)
                {
                    if ((errorCode & 0xfffff000L) == 0x80041000L)
                    {
                        ManagementException.ThrowWithExtendedInfo((ManagementStatus) errorCode);
                        return obj2;
                    }
                    Marshal.ThrowExceptionForHR(errorCode);
                }
            }
            return obj2;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ManagementPath Put()
        {
            return this.Put((PutOptions) null);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void Put(ManagementOperationObserver watcher)
        {
            this.Put(watcher, null);
        }

        public ManagementPath Put(PutOptions options)
        {
            ManagementPath path = null;
            this.Initialize(true);
            PutOptions options2 = (options != null) ? options : new PutOptions();
            IWbemServices iWbemServices = this.scope.GetIWbemServices();
            IntPtr zero = IntPtr.Zero;
            IntPtr pUnk = IntPtr.Zero;
            IWbemCallResult callResult = null;
            SecurityHandler securityHandler = null;
            int errorCode = 0;
            try
            {
                int num2;
                securityHandler = this.scope.GetSecurityHandler();
                zero = Marshal.AllocHGlobal(IntPtr.Size);
                Marshal.WriteIntPtr(zero, IntPtr.Zero);
                if (base.IsClass)
                {
                    errorCode = this.scope.GetSecuredIWbemServicesHandler(iWbemServices).PutClass_(base.wbemObject, options2.Flags | 0x10, options2.GetContext(), zero);
                }
                else
                {
                    errorCode = this.scope.GetSecuredIWbemServicesHandler(iWbemServices).PutInstance_(base.wbemObject, options2.Flags | 0x10, options2.GetContext(), zero);
                }
                pUnk = Marshal.ReadIntPtr(zero);
                callResult = (IWbemCallResult) Marshal.GetObjectForIUnknown(pUnk);
                errorCode = callResult.GetCallStatus_(-1, out num2);
                if (errorCode >= 0)
                {
                    errorCode = num2;
                }
                if (errorCode < 0)
                {
                    if ((errorCode & 0xfffff000L) == 0x80041000L)
                    {
                        ManagementException.ThrowWithExtendedInfo((ManagementStatus) errorCode);
                    }
                    else
                    {
                        Marshal.ThrowExceptionForHR(errorCode);
                    }
                }
                path = this.GetPath(callResult);
            }
            finally
            {
                if (securityHandler != null)
                {
                    securityHandler.Reset();
                }
                if (zero != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(zero);
                }
                if (pUnk != IntPtr.Zero)
                {
                    Marshal.Release(pUnk);
                }
                if (callResult != null)
                {
                    Marshal.ReleaseComObject(callResult);
                }
            }
            this.putButNotGot = true;
            this.path.SetRelativePath(path.RelativePath);
            return path;
        }

        public void Put(ManagementOperationObserver watcher, PutOptions options)
        {
            if (watcher == null)
            {
                throw new ArgumentNullException("watcher");
            }
            this.Initialize(false);
            PutOptions options2 = (options == null) ? new PutOptions() : ((PutOptions) options.Clone());
            if (watcher.HaveListenersForProgress)
            {
                options2.SendStatus = true;
            }
            IWbemServices iWbemServices = this.scope.GetIWbemServices();
            WmiEventSink eventSink = watcher.GetNewPutSink(this.scope, options2.Context, this.scope.Path.GetNamespacePath(8), base.ClassName);
            eventSink.InternalObjectPut += new InternalObjectPutEventHandler(this.HandleObjectPut);
            SecurityHandler securityHandler = null;
            int errorCode = -2147217407;
            securityHandler = this.scope.GetSecurityHandler();
            if (base.IsClass)
            {
                errorCode = this.scope.GetSecuredIWbemServicesHandler(iWbemServices).PutClassAsync_(base.wbemObject, options2.Flags, options2.GetContext(), eventSink.Stub);
            }
            else
            {
                errorCode = this.scope.GetSecuredIWbemServicesHandler(iWbemServices).PutInstanceAsync_(base.wbemObject, options2.Flags, options2.GetContext(), eventSink.Stub);
            }
            if (securityHandler != null)
            {
                securityHandler.Reset();
            }
            if (errorCode < 0)
            {
                eventSink.InternalObjectPut -= new InternalObjectPutEventHandler(this.HandleObjectPut);
                watcher.RemoveSink(eventSink);
                if ((errorCode & 0xfffff000L) == 0x80041000L)
                {
                    ManagementException.ThrowWithExtendedInfo((ManagementStatus) errorCode);
                }
                else
                {
                    Marshal.ThrowExceptionForHR(errorCode);
                }
            }
        }

        public override string ToString()
        {
            if (this.path != null)
            {
                return this.path.Path;
            }
            return "";
        }

        public override ManagementPath ClassPath
        {
            get
            {
                object pVal = null;
                object obj3 = null;
                object obj4 = null;
                int pType = 0;
                int plFlavor = 0;
                if (this.PutButNotGot)
                {
                    this.Get();
                    this.PutButNotGot = false;
                }
                int errorCode = base.wbemObject.Get_("__SERVER", 0, ref pVal, ref pType, ref plFlavor);
                if (errorCode >= 0)
                {
                    errorCode = base.wbemObject.Get_("__NAMESPACE", 0, ref obj3, ref pType, ref plFlavor);
                    if (errorCode >= 0)
                    {
                        errorCode = base.wbemObject.Get_("__CLASS", 0, ref obj4, ref pType, ref plFlavor);
                    }
                }
                if (errorCode < 0)
                {
                    if ((errorCode & 0xfffff000L) == 0x80041000L)
                    {
                        ManagementException.ThrowWithExtendedInfo((ManagementStatus) errorCode);
                    }
                    else
                    {
                        Marshal.ThrowExceptionForHR(errorCode);
                    }
                }
                ManagementPath path = new ManagementPath {
                    Server = string.Empty,
                    NamespacePath = string.Empty,
                    ClassName = string.Empty
                };
                try
                {
                    path.Server = (pVal is DBNull) ? "" : ((string) pVal);
                    path.NamespacePath = (obj3 is DBNull) ? "" : ((string) obj3);
                    path.ClassName = (obj4 is DBNull) ? "" : ((string) obj4);
                }
                catch
                {
                }
                return path;
            }
        }

        internal bool IsBound
        {
            get
            {
                return (base._wbemObject != null);
            }
        }

        public ObjectGetOptions Options
        {
            get
            {
                if (this.options == null)
                {
                    return (this.options = ObjectGetOptions._Clone(null));
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
                this.options = ObjectGetOptions._Clone(value, new IdentifierChangedEventHandler(this.HandleIdentifierChange));
                this.FireIdentifierChanged();
            }
        }

        public virtual ManagementPath Path
        {
            get
            {
                if (this.path == null)
                {
                    return (this.path = ManagementPath._Clone(null));
                }
                return this.path;
            }
            set
            {
                ManagementPath path = (value != null) ? value : new ManagementPath();
                string namespacePath = path.GetNamespacePath(8);
                if (((namespacePath.Length > 0) && (this.scope != null)) && this.scope.IsDefaulted)
                {
                    this.Scope = new ManagementScope(namespacePath);
                }
                if ((((base.GetType() != typeof(ManagementObject)) || !path.IsInstance) && ((base.GetType() != typeof(ManagementClass)) || !path.IsClass)) && !path.IsEmpty)
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                if (this.path != null)
                {
                    this.path.IdentifierChanged -= new IdentifierChangedEventHandler(this.HandleIdentifierChange);
                }
                this.path = ManagementPath._Clone(value, new IdentifierChangedEventHandler(this.HandleIdentifierChange));
                this.FireIdentifierChanged();
            }
        }

        internal bool PutButNotGot
        {
            get
            {
                return this.putButNotGot;
            }
            set
            {
                this.putButNotGot = value;
            }
        }

        public ManagementScope Scope
        {
            get
            {
                if (this.scope == null)
                {
                    return (this.scope = ManagementScope._Clone(null));
                }
                return this.scope;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                if (this.scope != null)
                {
                    this.scope.IdentifierChanged -= new IdentifierChangedEventHandler(this.HandleIdentifierChange);
                }
                this.scope = ManagementScope._Clone(value, new IdentifierChangedEventHandler(this.HandleIdentifierChange));
                this.FireIdentifierChanged();
            }
        }
    }
}

