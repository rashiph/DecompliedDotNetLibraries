namespace System.Management
{
    using System;
    using System.CodeDom;
    using System.Collections.Specialized;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;

    [Serializable]
    public class ManagementClass : ManagementObject
    {
        private MethodDataCollection methods;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ManagementClass() : this((ManagementScope) null, (ManagementPath) null, null)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ManagementClass(ManagementPath path) : this(null, path, null)
        {
        }

        public ManagementClass(string path) : this(null, new ManagementPath(path), null)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ManagementClass(ManagementPath path, ObjectGetOptions options) : this(null, path, options)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected ManagementClass(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public ManagementClass(string path, ObjectGetOptions options) : this(null, new ManagementPath(path), options)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ManagementClass(ManagementScope scope, ManagementPath path, ObjectGetOptions options) : base(scope, path, options)
        {
        }

        public ManagementClass(string scope, string path, ObjectGetOptions options) : base(new ManagementScope(scope), new ManagementPath(path), options)
        {
        }

        public override object Clone()
        {
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
            return GetManagementClass(ppCopy, this);
        }

        public ManagementObject CreateInstance()
        {
            ManagementObject obj2 = null;
            if (base.PutButNotGot)
            {
                base.Get();
                base.PutButNotGot = false;
            }
            IWbemClassObjectFreeThreaded ppNewInstance = null;
            int errorCode = base.wbemObject.SpawnInstance_(0, out ppNewInstance);
            if (errorCode >= 0)
            {
                return ManagementObject.GetManagementObject(ppNewInstance, base.Scope);
            }
            if ((errorCode & 0xfffff000L) == 0x80041000L)
            {
                ManagementException.ThrowWithExtendedInfo((ManagementStatus) errorCode);
                return obj2;
            }
            Marshal.ThrowExceptionForHR(errorCode);
            return obj2;
        }

        public ManagementClass Derive(string newClassName)
        {
            ManagementClass managementClass = null;
            if (newClassName == null)
            {
                throw new ArgumentNullException("newClassName");
            }
            ManagementPath path = new ManagementPath();
            try
            {
                path.ClassName = newClassName;
            }
            catch
            {
                throw new ArgumentOutOfRangeException("newClassName");
            }
            if (!path.IsClass)
            {
                throw new ArgumentOutOfRangeException("newClassName");
            }
            if (base.PutButNotGot)
            {
                base.Get();
                base.PutButNotGot = false;
            }
            IWbemClassObjectFreeThreaded ppNewClass = null;
            int errorCode = base.wbemObject.SpawnDerivedClass_(0, out ppNewClass);
            if (errorCode >= 0)
            {
                object pVal = newClassName;
                errorCode = ppNewClass.Put_("__CLASS", 0, ref pVal, 0);
                if (errorCode >= 0)
                {
                    managementClass = GetManagementClass(ppNewClass, this);
                }
            }
            if (errorCode < 0)
            {
                if ((errorCode & 0xfffff000L) == 0x80041000L)
                {
                    ManagementException.ThrowWithExtendedInfo((ManagementStatus) errorCode);
                    return managementClass;
                }
                Marshal.ThrowExceptionForHR(errorCode);
            }
            return managementClass;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ManagementObjectCollection GetInstances()
        {
            return this.GetInstances((EnumerationOptions) null);
        }

        public ManagementObjectCollection GetInstances(EnumerationOptions options)
        {
            if (((this.Path == null) || (this.Path.Path == null)) || (this.Path.Path.Length == 0))
            {
                throw new InvalidOperationException();
            }
            this.Initialize(false);
            IEnumWbemClassObject ppEnum = null;
            EnumerationOptions options2 = (options == null) ? new EnumerationOptions() : ((EnumerationOptions) options.Clone());
            options2.EnsureLocatable = false;
            options2.PrototypeOnly = false;
            SecurityHandler securityHandler = null;
            int errorCode = 0;
            try
            {
                securityHandler = base.Scope.GetSecurityHandler();
                errorCode = base.scope.GetSecuredIWbemServicesHandler(base.Scope.GetIWbemServices()).CreateInstanceEnum_(base.ClassName, options2.Flags, options2.GetContext(), ref ppEnum);
            }
            finally
            {
                if (securityHandler != null)
                {
                    securityHandler.Reset();
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
            return new ManagementObjectCollection(base.Scope, options2, ppEnum);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void GetInstances(ManagementOperationObserver watcher)
        {
            this.GetInstances(watcher, null);
        }

        public void GetInstances(ManagementOperationObserver watcher, EnumerationOptions options)
        {
            if (watcher == null)
            {
                throw new ArgumentNullException("watcher");
            }
            if (((this.Path == null) || (this.Path.Path == null)) || (this.Path.Path.Length == 0))
            {
                throw new InvalidOperationException();
            }
            this.Initialize(false);
            EnumerationOptions options2 = (options == null) ? new EnumerationOptions() : ((EnumerationOptions) options.Clone());
            options2.EnsureLocatable = false;
            options2.PrototypeOnly = false;
            options2.ReturnImmediately = false;
            if (watcher.HaveListenersForProgress)
            {
                options2.SendStatus = true;
            }
            WmiEventSink newSink = watcher.GetNewSink(base.Scope, options2.Context);
            SecurityHandler securityHandler = null;
            int errorCode = 0;
            securityHandler = base.Scope.GetSecurityHandler();
            errorCode = base.scope.GetSecuredIWbemServicesHandler(base.Scope.GetIWbemServices()).CreateInstanceEnumAsync_(base.ClassName, options2.Flags, options2.GetContext(), newSink.Stub);
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

        internal static ManagementClass GetManagementClass(IWbemClassObjectFreeThreaded wbemObject, ManagementClass mgObj)
        {
            ManagementClass class2 = new ManagementClass {
                wbemObject = wbemObject
            };
            if (mgObj != null)
            {
                class2.scope = ManagementScope._Clone(mgObj.scope);
                ManagementPath path = mgObj.Path;
                if (path != null)
                {
                    class2.path = ManagementPath._Clone(path);
                }
                object pVal = null;
                int pType = 0;
                int errorCode = wbemObject.Get_("__CLASS", 0, ref pVal, ref pType, ref pType);
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
                if (pVal != DBNull.Value)
                {
                    class2.path.internalClassName = (string) pVal;
                }
                ObjectGetOptions options = mgObj.Options;
                if (options != null)
                {
                    class2.options = ObjectGetOptions._Clone(options);
                }
            }
            return class2;
        }

        internal static ManagementClass GetManagementClass(IWbemClassObjectFreeThreaded wbemObject, ManagementScope scope)
        {
            ManagementClass class2 = new ManagementClass {
                path = new ManagementPath(ManagementPath.GetManagementPath(wbemObject))
            };
            if (scope != null)
            {
                class2.scope = ManagementScope._Clone(scope);
            }
            class2.wbemObject = wbemObject;
            return class2;
        }

        protected override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ManagementObjectCollection GetRelatedClasses()
        {
            return this.GetRelatedClasses((string) null);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void GetRelatedClasses(ManagementOperationObserver watcher)
        {
            this.GetRelatedClasses(watcher, null);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ManagementObjectCollection GetRelatedClasses(string relatedClass)
        {
            return this.GetRelatedClasses(relatedClass, null, null, null, null, null, null);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void GetRelatedClasses(ManagementOperationObserver watcher, string relatedClass)
        {
            this.GetRelatedClasses(watcher, relatedClass, null, null, null, null, null, null);
        }

        public ManagementObjectCollection GetRelatedClasses(string relatedClass, string relationshipClass, string relationshipQualifier, string relatedQualifier, string relatedRole, string thisRole, EnumerationOptions options)
        {
            if (((this.Path == null) || (this.Path.Path == null)) || (this.Path.Path.Length == 0))
            {
                throw new InvalidOperationException();
            }
            this.Initialize(false);
            IEnumWbemClassObject ppEnum = null;
            EnumerationOptions options2 = (options != null) ? ((EnumerationOptions) options.Clone()) : new EnumerationOptions();
            options2.EnumerateDeep = true;
            RelatedObjectQuery query = new RelatedObjectQuery(true, this.Path.Path, relatedClass, relationshipClass, relatedQualifier, relationshipQualifier, relatedRole, thisRole);
            SecurityHandler securityHandler = null;
            int errorCode = 0;
            try
            {
                securityHandler = base.Scope.GetSecurityHandler();
                errorCode = base.scope.GetSecuredIWbemServicesHandler(base.Scope.GetIWbemServices()).ExecQuery_(query.QueryLanguage, query.QueryString, options2.Flags, options2.GetContext(), ref ppEnum);
            }
            finally
            {
                if (securityHandler != null)
                {
                    securityHandler.Reset();
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
            return new ManagementObjectCollection(base.Scope, options2, ppEnum);
        }

        public void GetRelatedClasses(ManagementOperationObserver watcher, string relatedClass, string relationshipClass, string relationshipQualifier, string relatedQualifier, string relatedRole, string thisRole, EnumerationOptions options)
        {
            if (((this.Path == null) || (this.Path.Path == null)) || (this.Path.Path.Length == 0))
            {
                throw new InvalidOperationException();
            }
            this.Initialize(true);
            if (watcher == null)
            {
                throw new ArgumentNullException("watcher");
            }
            EnumerationOptions options2 = (options != null) ? ((EnumerationOptions) options.Clone()) : new EnumerationOptions();
            options2.EnumerateDeep = true;
            options2.ReturnImmediately = false;
            if (watcher.HaveListenersForProgress)
            {
                options2.SendStatus = true;
            }
            WmiEventSink newSink = watcher.GetNewSink(base.Scope, options2.Context);
            RelatedObjectQuery query = new RelatedObjectQuery(true, this.Path.Path, relatedClass, relationshipClass, relatedQualifier, relationshipQualifier, relatedRole, thisRole);
            SecurityHandler securityHandler = null;
            int errorCode = 0;
            securityHandler = base.Scope.GetSecurityHandler();
            errorCode = base.scope.GetSecuredIWbemServicesHandler(base.Scope.GetIWbemServices()).ExecQueryAsync_(query.QueryLanguage, query.QueryString, options2.Flags, options2.GetContext(), newSink.Stub);
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

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ManagementObjectCollection GetRelationshipClasses()
        {
            return this.GetRelationshipClasses((string) null);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void GetRelationshipClasses(ManagementOperationObserver watcher)
        {
            this.GetRelationshipClasses(watcher, null);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ManagementObjectCollection GetRelationshipClasses(string relationshipClass)
        {
            return this.GetRelationshipClasses(relationshipClass, null, null, null);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void GetRelationshipClasses(ManagementOperationObserver watcher, string relationshipClass)
        {
            this.GetRelationshipClasses(watcher, relationshipClass, null, null, null);
        }

        public ManagementObjectCollection GetRelationshipClasses(string relationshipClass, string relationshipQualifier, string thisRole, EnumerationOptions options)
        {
            if (((this.Path == null) || (this.Path.Path == null)) || (this.Path.Path.Length == 0))
            {
                throw new InvalidOperationException();
            }
            this.Initialize(false);
            IEnumWbemClassObject ppEnum = null;
            EnumerationOptions options2 = (options != null) ? options : new EnumerationOptions();
            options2.EnumerateDeep = true;
            RelationshipQuery query = new RelationshipQuery(true, this.Path.Path, relationshipClass, relationshipQualifier, thisRole);
            SecurityHandler securityHandler = null;
            int errorCode = 0;
            try
            {
                securityHandler = base.Scope.GetSecurityHandler();
                errorCode = base.scope.GetSecuredIWbemServicesHandler(base.Scope.GetIWbemServices()).ExecQuery_(query.QueryLanguage, query.QueryString, options2.Flags, options2.GetContext(), ref ppEnum);
            }
            finally
            {
                if (securityHandler != null)
                {
                    securityHandler.Reset();
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
            return new ManagementObjectCollection(base.Scope, options2, ppEnum);
        }

        public void GetRelationshipClasses(ManagementOperationObserver watcher, string relationshipClass, string relationshipQualifier, string thisRole, EnumerationOptions options)
        {
            if (((this.Path == null) || (this.Path.Path == null)) || (this.Path.Path.Length == 0))
            {
                throw new InvalidOperationException();
            }
            if (watcher == null)
            {
                throw new ArgumentNullException("watcher");
            }
            this.Initialize(true);
            EnumerationOptions options2 = (options != null) ? ((EnumerationOptions) options.Clone()) : new EnumerationOptions();
            options2.EnumerateDeep = true;
            options2.ReturnImmediately = false;
            if (watcher.HaveListenersForProgress)
            {
                options2.SendStatus = true;
            }
            WmiEventSink newSink = watcher.GetNewSink(base.Scope, options2.Context);
            RelationshipQuery query = new RelationshipQuery(true, this.Path.Path, relationshipClass, relationshipQualifier, thisRole);
            SecurityHandler securityHandler = null;
            int errorCode = 0;
            securityHandler = base.Scope.GetSecurityHandler();
            errorCode = base.scope.GetSecuredIWbemServicesHandler(base.Scope.GetIWbemServices()).ExecQueryAsync_(query.QueryLanguage, query.QueryString, options2.Flags, options2.GetContext(), newSink.Stub);
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

        public CodeTypeDeclaration GetStronglyTypedClassCode(bool includeSystemClassInClassDef, bool systemPropertyClass)
        {
            base.Get();
            ManagementClassGenerator generator = new ManagementClassGenerator(this);
            return generator.GenerateCode(includeSystemClassInClassDef, systemPropertyClass);
        }

        public bool GetStronglyTypedClassCode(CodeLanguage lang, string filePath, string classNamespace)
        {
            base.Get();
            ManagementClassGenerator generator = new ManagementClassGenerator(this);
            return generator.GenerateCode(lang, filePath, classNamespace);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ManagementObjectCollection GetSubclasses()
        {
            return this.GetSubclasses((EnumerationOptions) null);
        }

        public ManagementObjectCollection GetSubclasses(EnumerationOptions options)
        {
            if (this.Path == null)
            {
                throw new InvalidOperationException();
            }
            this.Initialize(false);
            IEnumWbemClassObject ppEnum = null;
            EnumerationOptions options2 = (options == null) ? new EnumerationOptions() : ((EnumerationOptions) options.Clone());
            options2.EnsureLocatable = false;
            options2.PrototypeOnly = false;
            SecurityHandler securityHandler = null;
            int errorCode = 0;
            try
            {
                securityHandler = base.Scope.GetSecurityHandler();
                errorCode = base.scope.GetSecuredIWbemServicesHandler(base.Scope.GetIWbemServices()).CreateClassEnum_(base.ClassName, options2.Flags, options2.GetContext(), ref ppEnum);
            }
            finally
            {
                if (securityHandler != null)
                {
                    securityHandler.Reset();
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
            return new ManagementObjectCollection(base.Scope, options2, ppEnum);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void GetSubclasses(ManagementOperationObserver watcher)
        {
            this.GetSubclasses(watcher, null);
        }

        public void GetSubclasses(ManagementOperationObserver watcher, EnumerationOptions options)
        {
            if (watcher == null)
            {
                throw new ArgumentNullException("watcher");
            }
            if (this.Path == null)
            {
                throw new InvalidOperationException();
            }
            this.Initialize(false);
            EnumerationOptions options2 = (options == null) ? new EnumerationOptions() : ((EnumerationOptions) options.Clone());
            options2.EnsureLocatable = false;
            options2.PrototypeOnly = false;
            options2.ReturnImmediately = false;
            if (watcher.HaveListenersForProgress)
            {
                options2.SendStatus = true;
            }
            WmiEventSink newSink = watcher.GetNewSink(base.Scope, options2.Context);
            SecurityHandler securityHandler = null;
            int errorCode = 0;
            securityHandler = base.Scope.GetSecurityHandler();
            errorCode = base.scope.GetSecuredIWbemServicesHandler(base.Scope.GetIWbemServices()).CreateClassEnumAsync_(base.ClassName, options2.Flags, options2.GetContext(), newSink.Stub);
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

        public StringCollection Derivation
        {
            get
            {
                StringCollection strings = new StringCollection();
                int pType = 0;
                int plFlavor = 0;
                object pVal = null;
                int errorCode = base.wbemObject.Get_("__DERIVATION", 0, ref pVal, ref pType, ref plFlavor);
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
                if (pVal != null)
                {
                    strings.AddRange((string[]) pVal);
                }
                return strings;
            }
        }

        public MethodDataCollection Methods
        {
            get
            {
                this.Initialize(true);
                if (this.methods == null)
                {
                    this.methods = new MethodDataCollection(this);
                }
                return this.methods;
            }
        }

        public override ManagementPath Path
        {
            get
            {
                return base.Path;
            }
            set
            {
                if (((value != null) && !value.IsClass) && !value.IsEmpty)
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                base.Path = value;
            }
        }
    }
}

