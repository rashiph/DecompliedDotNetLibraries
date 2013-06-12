namespace System.Reflection
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Text;
    using System.Threading;

    [Serializable, ComDefaultInterface(typeof(_MethodBase)), ClassInterface(ClassInterfaceType.None), ComVisible(true), PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust")]
    public abstract class MethodBase : MemberInfo, _MethodBase
    {
        protected MethodBase()
        {
        }

        [SecurityCritical]
        internal object[] CheckArguments(object[] parameters, Binder binder, BindingFlags invokeAttr, CultureInfo culture, Signature sig)
        {
            int num = (parameters != null) ? parameters.Length : 0;
            object[] objArray = new object[num];
            ParameterInfo[] parametersNoCopy = null;
            for (int i = 0; i < num; i++)
            {
                object defaultValue = parameters[i];
                RuntimeType type = sig.Arguments[i];
                if (defaultValue == Type.Missing)
                {
                    if (parametersNoCopy == null)
                    {
                        parametersNoCopy = this.GetParametersNoCopy();
                    }
                    if (parametersNoCopy[i].DefaultValue == DBNull.Value)
                    {
                        throw new ArgumentException(Environment.GetResourceString("Arg_VarMissNull"), "parameters");
                    }
                    defaultValue = parametersNoCopy[i].DefaultValue;
                }
                objArray[i] = type.CheckValue(defaultValue, binder, culture, invokeAttr);
            }
            return objArray;
        }

        internal virtual string ConstructName()
        {
            StringBuilder builder = new StringBuilder(this.Name);
            builder.Append("(");
            builder.Append(ConstructParameters(this.GetParametersNoCopy(), this.CallingConvention));
            builder.Append(")");
            return builder.ToString();
        }

        internal static string ConstructParameters(ParameterInfo[] parameters, CallingConventions callingConvention)
        {
            Type[] typeArray = new Type[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                typeArray[i] = parameters[i].ParameterType;
            }
            return ConstructParameters(typeArray, callingConvention);
        }

        internal static string ConstructParameters(Type[] parameters, CallingConventions callingConvention)
        {
            StringBuilder builder = new StringBuilder();
            string str = "";
            for (int i = 0; i < parameters.Length; i++)
            {
                Type type = parameters[i];
                builder.Append(str);
                string str2 = type.SigToString();
                if (type.IsByRef)
                {
                    builder.Append(str2.TrimEnd(new char[] { '&' }));
                    builder.Append(" ByRef");
                }
                else
                {
                    builder.Append(str2);
                }
                str = ", ";
            }
            if ((callingConvention & CallingConventions.VarArgs) == CallingConventions.VarArgs)
            {
                builder.Append(str);
                builder.Append("...");
            }
            return builder.ToString();
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        [MethodImpl(MethodImplOptions.NoInlining), SecuritySafeCritical]
        public static MethodBase GetCurrentMethod()
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return RuntimeMethodInfo.InternalGetCurrentMethod(ref lookForMyCaller);
        }

        [ComVisible(true)]
        public virtual Type[] GetGenericArguments()
        {
            throw new NotSupportedException(Environment.GetResourceString("NotSupported_SubclassOverride"));
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        [SecuritySafeCritical, ReflectionPermission(SecurityAction.Demand, Flags=ReflectionPermissionFlag.MemberAccess)]
        public virtual MethodBody GetMethodBody()
        {
            throw new InvalidOperationException();
        }

        [SecurityCritical]
        private IntPtr GetMethodDesc()
        {
            return this.MethodHandle.Value;
        }

        [SecuritySafeCritical]
        public static MethodBase GetMethodFromHandle(RuntimeMethodHandle handle)
        {
            if (handle.IsNullHandle())
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_InvalidHandle"));
            }
            MethodBase methodBase = RuntimeType.GetMethodBase(handle.GetMethodInfo());
            Type declaringType = methodBase.DeclaringType;
            if ((declaringType != null) && declaringType.IsGenericType)
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Argument_MethodDeclaringTypeGeneric"), new object[] { methodBase, declaringType.GetGenericTypeDefinition() }));
            }
            return methodBase;
        }

        [ComVisible(false), SecuritySafeCritical]
        public static MethodBase GetMethodFromHandle(RuntimeMethodHandle handle, RuntimeTypeHandle declaringType)
        {
            if (handle.IsNullHandle())
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_InvalidHandle"));
            }
            return RuntimeType.GetMethodBase(declaringType.GetRuntimeType(), handle.GetMethodInfo());
        }

        public abstract MethodImplAttributes GetMethodImplementationFlags();
        public abstract ParameterInfo[] GetParameters();
        [SecuritySafeCritical]
        internal virtual ParameterInfo[] GetParametersNoCopy()
        {
            return this.GetParameters();
        }

        internal virtual Type[] GetParameterTypes()
        {
            ParameterInfo[] parametersNoCopy = this.GetParametersNoCopy();
            Type[] typeArray = new Type[parametersNoCopy.Length];
            for (int i = 0; i < parametersNoCopy.Length; i++)
            {
                typeArray[i] = parametersNoCopy[i].ParameterType;
            }
            return typeArray;
        }

        [DebuggerStepThrough, DebuggerHidden]
        public object Invoke(object obj, object[] parameters)
        {
            return this.Invoke(obj, BindingFlags.Default, null, parameters, null);
        }

        public abstract object Invoke(object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture);
        public static bool operator ==(MethodBase left, MethodBase right)
        {
            MethodInfo info;
            MethodInfo info2;
            ConstructorInfo info3;
            ConstructorInfo info4;
            if (object.ReferenceEquals(left, right))
            {
                return true;
            }
            if ((left == null) || (right == null))
            {
                return false;
            }
            if (((info = left as MethodInfo) != null) && ((info2 = right as MethodInfo) != null))
            {
                return (info == info2);
            }
            return ((((info3 = left as ConstructorInfo) != null) && ((info4 = right as ConstructorInfo) != null)) && (info3 == info4));
        }

        public static bool operator !=(MethodBase left, MethodBase right)
        {
            return !(left == right);
        }

        void _MethodBase.GetIDsOfNames([In] ref Guid riid, IntPtr rgszNames, uint cNames, uint lcid, IntPtr rgDispId)
        {
            throw new NotImplementedException();
        }

        Type _MethodBase.GetType()
        {
            return base.GetType();
        }

        void _MethodBase.GetTypeInfo(uint iTInfo, uint lcid, IntPtr ppTInfo)
        {
            throw new NotImplementedException();
        }

        void _MethodBase.GetTypeInfoCount(out uint pcTInfo)
        {
            throw new NotImplementedException();
        }

        void _MethodBase.Invoke(uint dispIdMember, [In] ref Guid riid, uint lcid, short wFlags, IntPtr pDispParams, IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr)
        {
            throw new NotImplementedException();
        }

        public abstract MethodAttributes Attributes { get; }

        public virtual CallingConventions CallingConvention
        {
            get
            {
                return CallingConventions.Standard;
            }
        }

        public virtual bool ContainsGenericParameters
        {
            get
            {
                return false;
            }
        }

        public bool IsAbstract
        {
            get
            {
                return ((this.Attributes & MethodAttributes.Abstract) != MethodAttributes.PrivateScope);
            }
        }

        public bool IsAssembly
        {
            get
            {
                return ((this.Attributes & MethodAttributes.MemberAccessMask) == MethodAttributes.Assembly);
            }
        }

        [ComVisible(true)]
        public bool IsConstructor
        {
            get
            {
                return (((this is ConstructorInfo) && !this.IsStatic) && ((this.Attributes & MethodAttributes.RTSpecialName) == MethodAttributes.RTSpecialName));
            }
        }

        public bool IsFamily
        {
            get
            {
                return ((this.Attributes & MethodAttributes.MemberAccessMask) == MethodAttributes.Family);
            }
        }

        public bool IsFamilyAndAssembly
        {
            get
            {
                return ((this.Attributes & MethodAttributes.MemberAccessMask) == MethodAttributes.FamANDAssem);
            }
        }

        public bool IsFamilyOrAssembly
        {
            get
            {
                return ((this.Attributes & MethodAttributes.MemberAccessMask) == MethodAttributes.FamORAssem);
            }
        }

        public bool IsFinal
        {
            get
            {
                return ((this.Attributes & MethodAttributes.Final) != MethodAttributes.PrivateScope);
            }
        }

        public virtual bool IsGenericMethod
        {
            get
            {
                return false;
            }
        }

        public virtual bool IsGenericMethodDefinition
        {
            get
            {
                return false;
            }
        }

        public bool IsHideBySig
        {
            get
            {
                return ((this.Attributes & MethodAttributes.HideBySig) != MethodAttributes.PrivateScope);
            }
        }

        public bool IsPrivate
        {
            get
            {
                return ((this.Attributes & MethodAttributes.MemberAccessMask) == MethodAttributes.Private);
            }
        }

        public bool IsPublic
        {
            get
            {
                return ((this.Attributes & MethodAttributes.MemberAccessMask) == MethodAttributes.Public);
            }
        }

        public virtual bool IsSecurityCritical
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public virtual bool IsSecuritySafeCritical
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public virtual bool IsSecurityTransparent
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool IsSpecialName
        {
            get
            {
                return ((this.Attributes & MethodAttributes.SpecialName) != MethodAttributes.PrivateScope);
            }
        }

        public bool IsStatic
        {
            get
            {
                return ((this.Attributes & MethodAttributes.Static) != MethodAttributes.PrivateScope);
            }
        }

        public bool IsVirtual
        {
            get
            {
                return ((this.Attributes & MethodAttributes.Virtual) != MethodAttributes.PrivateScope);
            }
        }

        public abstract RuntimeMethodHandle MethodHandle { get; }

        bool _MethodBase.IsAbstract
        {
            get
            {
                return this.IsAbstract;
            }
        }

        bool _MethodBase.IsAssembly
        {
            get
            {
                return this.IsAssembly;
            }
        }

        bool _MethodBase.IsConstructor
        {
            get
            {
                return this.IsConstructor;
            }
        }

        bool _MethodBase.IsFamily
        {
            get
            {
                return this.IsFamily;
            }
        }

        bool _MethodBase.IsFamilyAndAssembly
        {
            get
            {
                return this.IsFamilyAndAssembly;
            }
        }

        bool _MethodBase.IsFamilyOrAssembly
        {
            get
            {
                return this.IsFamilyOrAssembly;
            }
        }

        bool _MethodBase.IsFinal
        {
            get
            {
                return this.IsFinal;
            }
        }

        bool _MethodBase.IsHideBySig
        {
            get
            {
                return this.IsHideBySig;
            }
        }

        bool _MethodBase.IsPrivate
        {
            get
            {
                return this.IsPrivate;
            }
        }

        bool _MethodBase.IsPublic
        {
            get
            {
                return this.IsPublic;
            }
        }

        bool _MethodBase.IsSpecialName
        {
            get
            {
                return this.IsSpecialName;
            }
        }

        bool _MethodBase.IsStatic
        {
            get
            {
                return this.IsStatic;
            }
        }

        bool _MethodBase.IsVirtual
        {
            get
            {
                return this.IsVirtual;
            }
        }
    }
}

