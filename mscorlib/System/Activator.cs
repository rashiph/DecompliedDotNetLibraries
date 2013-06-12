namespace System
{
    using System.Configuration.Assemblies;
    using System.Diagnostics;
    using System.Globalization;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Activation;
    using System.Security;
    using System.Security.Policy;
    using System.Threading;

    [ClassInterface(ClassInterfaceType.None), ComVisible(true), ComDefaultInterface(typeof(_Activator))]
    public sealed class Activator : _Activator
    {
        internal const BindingFlags ConLookup = (BindingFlags.Public | BindingFlags.Instance);
        internal const BindingFlags ConstructorDefault = (BindingFlags.CreateInstance | BindingFlags.Public | BindingFlags.Instance);
        internal const int LookupMask = 0xff;

        private Activator()
        {
        }

        [SecuritySafeCritical]
        public static ObjectHandle CreateComInstanceFrom(string assemblyName, string typeName)
        {
            return CreateComInstanceFrom(assemblyName, typeName, null, AssemblyHashAlgorithm.None);
        }

        [SecuritySafeCritical]
        public static ObjectHandle CreateComInstanceFrom(string assemblyName, string typeName, byte[] hashValue, AssemblyHashAlgorithm hashAlgorithm)
        {
            Assembly assembly = Assembly.LoadFrom(assemblyName, hashValue, hashAlgorithm);
            Type type = assembly.GetType(typeName, true, false);
            object[] customAttributes = type.GetCustomAttributes(typeof(ComVisibleAttribute), false);
            if ((customAttributes.Length > 0) && !((ComVisibleAttribute) customAttributes[0]).Value)
            {
                throw new TypeLoadException(Environment.GetResourceString("Argument_TypeMustBeVisibleFromCom"));
            }
            if (assembly == null)
            {
                return null;
            }
            object o = CreateInstance(type, BindingFlags.CreateInstance | BindingFlags.Public | BindingFlags.Instance, null, null, null, null);
            if (o == null)
            {
                return null;
            }
            return new ObjectHandle(o);
        }

        [SecuritySafeCritical]
        public static T CreateInstance<T>()
        {
            RuntimeType type = typeof(T) as RuntimeType;
            if (type.HasElementType)
            {
                throw new MissingMethodException(Environment.GetResourceString("Arg_NoDefCTor"));
            }
            return (T) type.CreateInstanceDefaultCtor(true, true, true, true);
        }

        [SecuritySafeCritical]
        public static ObjectHandle CreateInstance(ActivationContext activationContext)
        {
            AppDomainManager domainManager = AppDomain.CurrentDomain.DomainManager;
            if (domainManager == null)
            {
                domainManager = new AppDomainManager();
            }
            return domainManager.ApplicationActivator.CreateInstance(activationContext);
        }

        [SecuritySafeCritical]
        public static object CreateInstance(Type type)
        {
            return CreateInstance(type, false);
        }

        [SecuritySafeCritical]
        public static ObjectHandle CreateInstance(ActivationContext activationContext, string[] activationCustomData)
        {
            AppDomainManager domainManager = AppDomain.CurrentDomain.DomainManager;
            if (domainManager == null)
            {
                domainManager = new AppDomainManager();
            }
            return domainManager.ApplicationActivator.CreateInstance(activationContext, activationCustomData);
        }

        [MethodImpl(MethodImplOptions.NoInlining), SecuritySafeCritical]
        public static ObjectHandle CreateInstance(string assemblyName, string typeName)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return CreateInstance(assemblyName, typeName, false, BindingFlags.CreateInstance | BindingFlags.Public | BindingFlags.Instance, null, null, null, null, null, ref lookForMyCaller);
        }

        [SecuritySafeCritical]
        public static object CreateInstance(Type type, params object[] args)
        {
            return CreateInstance(type, BindingFlags.CreateInstance | BindingFlags.Public | BindingFlags.Instance, null, args, null, null);
        }

        public static object CreateInstance(Type type, bool nonPublic)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            RuntimeType underlyingSystemType = type.UnderlyingSystemType as RuntimeType;
            if (underlyingSystemType == null)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_MustBeType"), "type");
            }
            return underlyingSystemType.CreateInstanceDefaultCtor(!nonPublic);
        }

        [SecurityCritical]
        public static ObjectHandle CreateInstance(AppDomain domain, string assemblyName, string typeName)
        {
            if (domain == null)
            {
                throw new ArgumentNullException("domain");
            }
            return domain.InternalCreateInstanceWithNoSecurity(assemblyName, typeName);
        }

        [MethodImpl(MethodImplOptions.NoInlining), SecuritySafeCritical]
        public static ObjectHandle CreateInstance(string assemblyName, string typeName, object[] activationAttributes)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return CreateInstance(assemblyName, typeName, false, BindingFlags.CreateInstance | BindingFlags.Public | BindingFlags.Instance, null, null, null, activationAttributes, null, ref lookForMyCaller);
        }

        [SecuritySafeCritical]
        public static object CreateInstance(Type type, object[] args, object[] activationAttributes)
        {
            return CreateInstance(type, BindingFlags.CreateInstance | BindingFlags.Public | BindingFlags.Instance, null, args, null, activationAttributes);
        }

        [SecuritySafeCritical]
        public static object CreateInstance(Type type, BindingFlags bindingAttr, Binder binder, object[] args, CultureInfo culture)
        {
            return CreateInstance(type, bindingAttr, binder, args, culture, null);
        }

        [SecuritySafeCritical]
        public static object CreateInstance(Type type, BindingFlags bindingAttr, Binder binder, object[] args, CultureInfo culture, object[] activationAttributes)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            if (type is TypeBuilder)
            {
                throw new NotSupportedException(Environment.GetResourceString("NotSupported_CreateInstanceWithTypeBuilder"));
            }
            if ((bindingAttr & 0xff) == BindingFlags.Default)
            {
                bindingAttr |= BindingFlags.CreateInstance | BindingFlags.Public | BindingFlags.Instance;
            }
            if ((activationAttributes != null) && (activationAttributes.Length > 0))
            {
                if (!type.IsMarshalByRef)
                {
                    throw new NotSupportedException(Environment.GetResourceString("NotSupported_ActivAttrOnNonMBR"));
                }
                if (!type.IsContextful && ((activationAttributes.Length > 1) || !(activationAttributes[0] is UrlAttribute)))
                {
                    throw new NotSupportedException(Environment.GetResourceString("NotSupported_NonUrlAttrOnMBR"));
                }
            }
            RuntimeType underlyingSystemType = type.UnderlyingSystemType as RuntimeType;
            if (underlyingSystemType == null)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_MustBeType"), "type");
            }
            return underlyingSystemType.CreateInstanceImpl(bindingAttr, binder, args, culture, activationAttributes);
        }

        [MethodImpl(MethodImplOptions.NoInlining), SecuritySafeCritical]
        public static ObjectHandle CreateInstance(string assemblyName, string typeName, bool ignoreCase, BindingFlags bindingAttr, Binder binder, object[] args, CultureInfo culture, object[] activationAttributes)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return CreateInstance(assemblyName, typeName, ignoreCase, bindingAttr, binder, args, culture, activationAttributes, null, ref lookForMyCaller);
        }

        [SecurityCritical]
        public static ObjectHandle CreateInstance(AppDomain domain, string assemblyName, string typeName, bool ignoreCase, BindingFlags bindingAttr, Binder binder, object[] args, CultureInfo culture, object[] activationAttributes)
        {
            if (domain == null)
            {
                throw new ArgumentNullException("domain");
            }
            return domain.InternalCreateInstanceWithNoSecurity(assemblyName, typeName, ignoreCase, bindingAttr, binder, args, culture, activationAttributes, null);
        }

        [MethodImpl(MethodImplOptions.NoInlining), Obsolete("Methods which use evidence to sandbox are obsolete and will be removed in a future release of the .NET Framework. Please use an overload of CreateInstance which does not take an Evidence parameter. See http://go.microsoft.com/fwlink/?LinkID=155570 for more information."), SecuritySafeCritical]
        public static ObjectHandle CreateInstance(string assemblyName, string typeName, bool ignoreCase, BindingFlags bindingAttr, Binder binder, object[] args, CultureInfo culture, object[] activationAttributes, Evidence securityInfo)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return CreateInstance(assemblyName, typeName, ignoreCase, bindingAttr, binder, args, culture, activationAttributes, securityInfo, ref lookForMyCaller);
        }

        [SecurityCritical, Obsolete("Methods which use evidence to sandbox are obsolete and will be removed in a future release of the .NET Framework. Please use an overload of CreateInstance which does not take an Evidence parameter. See http://go.microsoft.com/fwlink/?LinkID=155570 for more information.")]
        public static ObjectHandle CreateInstance(AppDomain domain, string assemblyName, string typeName, bool ignoreCase, BindingFlags bindingAttr, Binder binder, object[] args, CultureInfo culture, object[] activationAttributes, Evidence securityAttributes)
        {
            if (domain == null)
            {
                throw new ArgumentNullException("domain");
            }
            if ((securityAttributes != null) && !AppDomain.CurrentDomain.IsLegacyCasPolicyEnabled)
            {
                throw new NotSupportedException(Environment.GetResourceString("NotSupported_RequiresCasPolicyImplicit"));
            }
            return domain.InternalCreateInstanceWithNoSecurity(assemblyName, typeName, ignoreCase, bindingAttr, binder, args, culture, activationAttributes, securityAttributes);
        }

        [SecurityCritical]
        internal static ObjectHandle CreateInstance(string assemblyName, string typeName, bool ignoreCase, BindingFlags bindingAttr, Binder binder, object[] args, CultureInfo culture, object[] activationAttributes, Evidence securityInfo, ref StackCrawlMark stackMark)
        {
            Assembly executingAssembly;
            if ((securityInfo != null) && !AppDomain.CurrentDomain.IsLegacyCasPolicyEnabled)
            {
                throw new NotSupportedException(Environment.GetResourceString("NotSupported_RequiresCasPolicyImplicit"));
            }
            if (assemblyName == null)
            {
                executingAssembly = RuntimeAssembly.GetExecutingAssembly(ref stackMark);
            }
            else
            {
                executingAssembly = RuntimeAssembly.InternalLoad(assemblyName, securityInfo, ref stackMark, false);
            }
            if (executingAssembly == null)
            {
                return null;
            }
            object o = CreateInstance(executingAssembly.GetType(typeName, true, ignoreCase), bindingAttr, binder, args, culture, activationAttributes);
            if (o == null)
            {
                return null;
            }
            return new ObjectHandle(o);
        }

        [SecuritySafeCritical]
        public static ObjectHandle CreateInstanceFrom(string assemblyFile, string typeName)
        {
            return CreateInstanceFrom(assemblyFile, typeName, null);
        }

        [SecurityCritical]
        public static ObjectHandle CreateInstanceFrom(AppDomain domain, string assemblyFile, string typeName)
        {
            if (domain == null)
            {
                throw new ArgumentNullException("domain");
            }
            return domain.InternalCreateInstanceFromWithNoSecurity(assemblyFile, typeName);
        }

        [SecuritySafeCritical]
        public static ObjectHandle CreateInstanceFrom(string assemblyFile, string typeName, object[] activationAttributes)
        {
            return CreateInstanceFrom(assemblyFile, typeName, false, BindingFlags.CreateInstance | BindingFlags.Public | BindingFlags.Instance, null, null, null, activationAttributes);
        }

        public static ObjectHandle CreateInstanceFrom(string assemblyFile, string typeName, bool ignoreCase, BindingFlags bindingAttr, Binder binder, object[] args, CultureInfo culture, object[] activationAttributes)
        {
            return CreateInstanceFromInternal(assemblyFile, typeName, ignoreCase, bindingAttr, binder, args, culture, activationAttributes, null);
        }

        [SecurityCritical]
        public static ObjectHandle CreateInstanceFrom(AppDomain domain, string assemblyFile, string typeName, bool ignoreCase, BindingFlags bindingAttr, Binder binder, object[] args, CultureInfo culture, object[] activationAttributes)
        {
            if (domain == null)
            {
                throw new ArgumentNullException("domain");
            }
            return domain.InternalCreateInstanceFromWithNoSecurity(assemblyFile, typeName, ignoreCase, bindingAttr, binder, args, culture, activationAttributes, null);
        }

        [Obsolete("Methods which use evidence to sandbox are obsolete and will be removed in a future release of the .NET Framework. Please use an overload of CreateInstanceFrom which does not take an Evidence parameter. See http://go.microsoft.com/fwlink/?LinkID=155570 for more information.")]
        public static ObjectHandle CreateInstanceFrom(string assemblyFile, string typeName, bool ignoreCase, BindingFlags bindingAttr, Binder binder, object[] args, CultureInfo culture, object[] activationAttributes, Evidence securityInfo)
        {
            if ((securityInfo != null) && !AppDomain.CurrentDomain.IsLegacyCasPolicyEnabled)
            {
                throw new NotSupportedException(Environment.GetResourceString("NotSupported_RequiresCasPolicyImplicit"));
            }
            return CreateInstanceFromInternal(assemblyFile, typeName, ignoreCase, bindingAttr, binder, args, culture, activationAttributes, securityInfo);
        }

        [SecurityCritical, Obsolete("Methods which use Evidence to sandbox are obsolete and will be removed in a future release of the .NET Framework. Please use an overload of CreateInstanceFrom which does not take an Evidence parameter. See http://go.microsoft.com/fwlink/?LinkID=155570 for more information.")]
        public static ObjectHandle CreateInstanceFrom(AppDomain domain, string assemblyFile, string typeName, bool ignoreCase, BindingFlags bindingAttr, Binder binder, object[] args, CultureInfo culture, object[] activationAttributes, Evidence securityAttributes)
        {
            if (domain == null)
            {
                throw new ArgumentNullException("domain");
            }
            if ((securityAttributes != null) && !AppDomain.CurrentDomain.IsLegacyCasPolicyEnabled)
            {
                throw new NotSupportedException(Environment.GetResourceString("NotSupported_RequiresCasPolicyImplicit"));
            }
            return domain.InternalCreateInstanceFromWithNoSecurity(assemblyFile, typeName, ignoreCase, bindingAttr, binder, args, culture, activationAttributes, securityAttributes);
        }

        [SecuritySafeCritical]
        private static ObjectHandle CreateInstanceFromInternal(string assemblyFile, string typeName, bool ignoreCase, BindingFlags bindingAttr, Binder binder, object[] args, CultureInfo culture, object[] activationAttributes, Evidence securityInfo)
        {
            object o = CreateInstance(Assembly.LoadFrom(assemblyFile, securityInfo).GetType(typeName, true, ignoreCase), bindingAttr, binder, args, culture, activationAttributes);
            if (o == null)
            {
                return null;
            }
            return new ObjectHandle(o);
        }

        [SecurityCritical]
        public static object GetObject(Type type, string url)
        {
            return GetObject(type, url, null);
        }

        [SecurityCritical]
        public static object GetObject(Type type, string url, object state)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            return RemotingServices.Connect(type, url, state);
        }

        [Conditional("_DEBUG")]
        private static void Log(bool test, string title, string success, string failure)
        {
        }

        void _Activator.GetIDsOfNames([In] ref Guid riid, IntPtr rgszNames, uint cNames, uint lcid, IntPtr rgDispId)
        {
            throw new NotImplementedException();
        }

        void _Activator.GetTypeInfo(uint iTInfo, uint lcid, IntPtr ppTInfo)
        {
            throw new NotImplementedException();
        }

        void _Activator.GetTypeInfoCount(out uint pcTInfo)
        {
            throw new NotImplementedException();
        }

        void _Activator.Invoke(uint dispIdMember, [In] ref Guid riid, uint lcid, short wFlags, IntPtr pDispParams, IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr)
        {
            throw new NotImplementedException();
        }
    }
}

