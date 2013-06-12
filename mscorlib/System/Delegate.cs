namespace System
{
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;

    [Serializable, ClassInterface(ClassInterfaceType.AutoDual), ComVisible(true)]
    public abstract class Delegate : ICloneable, ISerializable
    {
        internal object _methodBase;
        [ForceTokenStabilization]
        internal IntPtr _methodPtr;
        [ForceTokenStabilization]
        internal IntPtr _methodPtrAux;
        [ForceTokenStabilization]
        internal object _target;

        private Delegate()
        {
        }

        [SecuritySafeCritical]
        protected Delegate(object target, string method)
        {
            if (target == null)
            {
                throw new ArgumentNullException("target");
            }
            if (method == null)
            {
                throw new ArgumentNullException("method");
            }
            if (!this.BindToMethodName(target, (RuntimeType) target.GetType(), method, DelegateBindingFlags.ClosedDelegateOnly | DelegateBindingFlags.InstanceMethodOnly))
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_DlgtTargMeth"));
            }
        }

        [SecuritySafeCritical]
        protected Delegate(Type target, string method)
        {
            if (target == null)
            {
                throw new ArgumentNullException("target");
            }
            if (target.IsGenericType && target.ContainsGenericParameters)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_UnboundGenParam"), "target");
            }
            if (method == null)
            {
                throw new ArgumentNullException("method");
            }
            if (!(target is RuntimeType))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_MustBeRuntimeType"), "target");
            }
            this.BindToMethodName(null, target.TypeHandle.GetRuntimeType(), method, DelegateBindingFlags.CaselessMatching | DelegateBindingFlags.OpenDelegateOnly | DelegateBindingFlags.StaticMethodOnly);
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal extern IntPtr AdjustTarget(object target, IntPtr methodPtr);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private extern bool BindToMethodInfo(object target, IRuntimeMethodInfo method, RuntimeType methodType, DelegateBindingFlags flags);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private extern bool BindToMethodName(object target, RuntimeType methodType, string method, DelegateBindingFlags flags);
        [SecuritySafeCritical]
        public virtual object Clone()
        {
            return base.MemberwiseClone();
        }

        [ComVisible(true)]
        public static Delegate Combine(params Delegate[] delegates)
        {
            if ((delegates == null) || (delegates.Length == 0))
            {
                return null;
            }
            Delegate a = delegates[0];
            for (int i = 1; i < delegates.Length; i++)
            {
                a = Combine(a, delegates[i]);
            }
            return a;
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries"), SecuritySafeCritical]
        public static Delegate Combine(Delegate a, Delegate b)
        {
            if (a == null)
            {
                return b;
            }
            return a.CombineImpl(b);
        }

        protected virtual Delegate CombineImpl(Delegate d)
        {
            throw new MulticastNotSupportedException(Environment.GetResourceString("Multicast_Combine"));
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern bool CompareUnmanagedFunctionPtrs(Delegate d1, Delegate d2);
        public static Delegate CreateDelegate(Type type, MethodInfo method)
        {
            return CreateDelegate(type, method, true);
        }

        public static Delegate CreateDelegate(Type type, object firstArgument, MethodInfo method)
        {
            return CreateDelegate(type, firstArgument, method, true);
        }

        [SecuritySafeCritical]
        internal static Delegate CreateDelegate(Type type, object target, RuntimeMethodHandle method)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            if (method.IsNullHandle())
            {
                throw new ArgumentNullException("method");
            }
            if (!(type is RuntimeType))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_MustBeRuntimeType"), "type");
            }
            Type baseType = type.BaseType;
            if ((baseType == null) || (baseType != typeof(MulticastDelegate)))
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_MustBeDelegate"), "type");
            }
            Delegate delegate2 = InternalAlloc(type.TypeHandle.GetRuntimeType());
            if (!delegate2.BindToMethodInfo(target, method.GetMethodInfo(), RuntimeMethodHandle.GetDeclaringType(method.GetMethodInfo()), DelegateBindingFlags.RelaxedSignature))
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_DlgtTargMeth"));
            }
            return delegate2;
        }

        public static Delegate CreateDelegate(Type type, object target, string method)
        {
            return CreateDelegate(type, target, method, false, true);
        }

        [SecuritySafeCritical]
        public static Delegate CreateDelegate(Type type, MethodInfo method, bool throwOnBindFailure)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            if (method == null)
            {
                throw new ArgumentNullException("method");
            }
            if (!(type is RuntimeType))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_MustBeRuntimeType"), "type");
            }
            RuntimeMethodInfo info = method as RuntimeMethodInfo;
            if (info == null)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_MustBeRuntimeMethodInfo"), "method");
            }
            Type baseType = type.BaseType;
            if ((baseType == null) || (baseType != typeof(MulticastDelegate)))
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_MustBeDelegate"), "type");
            }
            Delegate delegate2 = InternalAlloc(type.TypeHandle.GetRuntimeType());
            if (delegate2.BindToMethodInfo(null, info.MethodHandle.GetMethodInfo(), info.GetDeclaringTypeInternal().TypeHandle.GetRuntimeType(), DelegateBindingFlags.RelaxedSignature | DelegateBindingFlags.OpenDelegateOnly))
            {
                return delegate2;
            }
            if (throwOnBindFailure)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_DlgtTargMeth"));
            }
            return null;
        }

        public static Delegate CreateDelegate(Type type, Type target, string method)
        {
            return CreateDelegate(type, target, method, false, true);
        }

        [SecuritySafeCritical]
        public static Delegate CreateDelegate(Type type, object firstArgument, MethodInfo method, bool throwOnBindFailure)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            if (method == null)
            {
                throw new ArgumentNullException("method");
            }
            if (!(type is RuntimeType))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_MustBeRuntimeType"), "type");
            }
            RuntimeMethodInfo info = method as RuntimeMethodInfo;
            if (info == null)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_MustBeRuntimeMethodInfo"), "method");
            }
            Type baseType = type.BaseType;
            if ((baseType == null) || (baseType != typeof(MulticastDelegate)))
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_MustBeDelegate"), "type");
            }
            Delegate delegate2 = InternalAlloc(type.TypeHandle.GetRuntimeType());
            if (delegate2.BindToMethodInfo(firstArgument, info.MethodHandle.GetMethodInfo(), info.GetDeclaringTypeInternal().TypeHandle.GetRuntimeType(), DelegateBindingFlags.RelaxedSignature))
            {
                return delegate2;
            }
            if (throwOnBindFailure)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_DlgtTargMeth"));
            }
            return null;
        }

        public static Delegate CreateDelegate(Type type, object target, string method, bool ignoreCase)
        {
            return CreateDelegate(type, target, method, ignoreCase, true);
        }

        public static Delegate CreateDelegate(Type type, Type target, string method, bool ignoreCase)
        {
            return CreateDelegate(type, target, method, ignoreCase, true);
        }

        [SecuritySafeCritical]
        public static Delegate CreateDelegate(Type type, object target, string method, bool ignoreCase, bool throwOnBindFailure)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            if (target == null)
            {
                throw new ArgumentNullException("target");
            }
            if (method == null)
            {
                throw new ArgumentNullException("method");
            }
            if (!(type is RuntimeType))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_MustBeRuntimeType"), "type");
            }
            Type baseType = type.BaseType;
            if ((baseType == null) || (baseType != typeof(MulticastDelegate)))
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_MustBeDelegate"), "type");
            }
            Delegate delegate2 = InternalAlloc(type.TypeHandle.GetRuntimeType());
            if (delegate2.BindToMethodName(target, (RuntimeType) target.GetType(), method, (DelegateBindingFlags.NeverCloseOverNull | DelegateBindingFlags.ClosedDelegateOnly | DelegateBindingFlags.InstanceMethodOnly) | (ignoreCase ? DelegateBindingFlags.CaselessMatching : ((DelegateBindingFlags) 0))))
            {
                return delegate2;
            }
            if (throwOnBindFailure)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_DlgtTargMeth"));
            }
            return null;
        }

        [SecuritySafeCritical]
        public static Delegate CreateDelegate(Type type, Type target, string method, bool ignoreCase, bool throwOnBindFailure)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            if (target == null)
            {
                throw new ArgumentNullException("target");
            }
            if (target.IsGenericType && target.ContainsGenericParameters)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_UnboundGenParam"), "target");
            }
            if (method == null)
            {
                throw new ArgumentNullException("method");
            }
            if (!(type is RuntimeType))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_MustBeRuntimeType"), "type");
            }
            if (!(target is RuntimeType))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_MustBeRuntimeType"), "target");
            }
            Type baseType = type.BaseType;
            if ((baseType == null) || (baseType != typeof(MulticastDelegate)))
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_MustBeDelegate"), "type");
            }
            Delegate delegate2 = InternalAlloc(type.TypeHandle.GetRuntimeType());
            if (delegate2.BindToMethodName(null, target.TypeHandle.GetRuntimeType(), method, (DelegateBindingFlags.OpenDelegateOnly | DelegateBindingFlags.StaticMethodOnly) | (ignoreCase ? DelegateBindingFlags.CaselessMatching : ((DelegateBindingFlags) 0))))
            {
                return delegate2;
            }
            if (throwOnBindFailure)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_DlgtTargMeth"));
            }
            return null;
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private extern void DelegateConstruct(object target, IntPtr slot);
        [SecuritySafeCritical]
        public object DynamicInvoke(params object[] args)
        {
            return this.DynamicInvokeImpl(args);
        }

        [SecuritySafeCritical]
        protected virtual object DynamicInvokeImpl(object[] args)
        {
            RuntimeMethodHandleInternal methodHandle = new RuntimeMethodHandleInternal(this.GetInvokeMethod());
            RuntimeMethodInfo methodBase = (RuntimeMethodInfo) RuntimeType.GetMethodBase((RuntimeType) base.GetType(), methodHandle);
            return methodBase.Invoke(this, BindingFlags.Default, null, args, null, true);
        }

        [SecuritySafeCritical]
        public override bool Equals(object obj)
        {
            if ((obj == null) || !InternalEqualTypes(this, obj))
            {
                return false;
            }
            Delegate right = (Delegate) obj;
            if (((this._target == right._target) && (this._methodPtr == right._methodPtr)) && (this._methodPtrAux == right._methodPtrAux))
            {
                return true;
            }
            if (this._methodPtrAux.IsNull())
            {
                if (!right._methodPtrAux.IsNull())
                {
                    return false;
                }
                if (this._target != right._target)
                {
                    return false;
                }
            }
            else
            {
                if (right._methodPtrAux.IsNull())
                {
                    return false;
                }
                if (this._methodPtrAux == right._methodPtrAux)
                {
                    return true;
                }
            }
            if (((this._methodBase != null) && (right._methodBase != null)) && ((this._methodBase is MethodInfo) && (right._methodBase is MethodInfo)))
            {
                return this._methodBase.Equals(right._methodBase);
            }
            return InternalEqualMethodHandles(this, right);
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        internal extern IRuntimeMethodInfo FindMethodHandle();
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal extern IntPtr GetCallStub(IntPtr methodPtr);
        public override int GetHashCode()
        {
            return base.GetType().GetHashCode();
        }

        public virtual Delegate[] GetInvocationList()
        {
            return new Delegate[] { this };
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal extern IntPtr GetInvokeMethod();
        [SecuritySafeCritical]
        protected virtual MethodInfo GetMethodImpl()
        {
            if ((this._methodBase == null) || !(this._methodBase is MethodInfo))
            {
                IRuntimeMethodInfo method = this.FindMethodHandle();
                RuntimeType declaringType = RuntimeMethodHandle.GetDeclaringType(method);
                if ((RuntimeTypeHandle.IsGenericTypeDefinition(declaringType) || RuntimeTypeHandle.HasInstantiation(declaringType)) && ((RuntimeMethodHandle.GetAttributes(method) & MethodAttributes.Static) == MethodAttributes.PrivateScope))
                {
                    if (!(this._methodPtrAux == IntPtr.Zero))
                    {
                        declaringType = (RuntimeType) base.GetType().GetMethod("Invoke").GetParameters()[0].ParameterType;
                    }
                    else
                    {
                        Type type = this._target.GetType();
                        Type genericTypeDefinition = declaringType.GetGenericTypeDefinition();
                        while (true)
                        {
                            if (type.IsGenericType && (type.GetGenericTypeDefinition() == genericTypeDefinition))
                            {
                                break;
                            }
                            type = type.BaseType;
                        }
                        declaringType = type as RuntimeType;
                    }
                }
                this._methodBase = (MethodInfo) RuntimeType.GetMethodBase(declaringType, method);
            }
            return (MethodInfo) this._methodBase;
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal extern IntPtr GetMulticastInvoke();
        [SecurityCritical]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            throw new NotSupportedException();
        }

        internal virtual object GetTarget()
        {
            if (!this._methodPtrAux.IsNull())
            {
                return null;
            }
            return this._target;
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern MulticastDelegate InternalAlloc(RuntimeType type);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern MulticastDelegate InternalAllocLike(Delegate d);
        [SecuritySafeCritical]
        internal static Delegate InternalCreateDelegate(Type type, object firstArgument, MethodInfo method)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            if (method == null)
            {
                throw new ArgumentNullException("method");
            }
            RuntimeMethodInfo info = method as RuntimeMethodInfo;
            if (info == null)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_MustBeRuntimeMethodInfo"), "method");
            }
            Type baseType = type.BaseType;
            if ((baseType == null) || (baseType != typeof(MulticastDelegate)))
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_MustBeDelegate"), "type");
            }
            Delegate delegate2 = InternalAlloc(type.TypeHandle.GetRuntimeType());
            if (!delegate2.BindToMethodInfo(firstArgument, info.MethodHandle.GetMethodInfo(), info.GetDeclaringTypeInternal(), DelegateBindingFlags.RelaxedSignature | DelegateBindingFlags.SkipSecurityChecks))
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_DlgtTargMeth"));
            }
            return delegate2;
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern bool InternalEqualMethodHandles(Delegate left, Delegate right);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern bool InternalEqualTypes(object a, object b);
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static bool operator ==(Delegate d1, Delegate d2)
        {
            if (d1 == null)
            {
                return (d2 == null);
            }
            return d1.Equals(d2);
        }

        public static bool operator !=(Delegate d1, Delegate d2)
        {
            if (d1 == null)
            {
                return (d2 != null);
            }
            return !d1.Equals(d2);
        }

        [SecuritySafeCritical]
        public static Delegate Remove(Delegate source, Delegate value)
        {
            if (source == null)
            {
                return null;
            }
            if (value == null)
            {
                return source;
            }
            if (!InternalEqualTypes(source, value))
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_DlgtTypeMis"));
            }
            return source.RemoveImpl(value);
        }

        public static Delegate RemoveAll(Delegate source, Delegate value)
        {
            Delegate delegate2 = null;
            do
            {
                delegate2 = source;
                source = Remove(source, value);
            }
            while (delegate2 != source);
            return delegate2;
        }

        protected virtual Delegate RemoveImpl(Delegate d)
        {
            if (!d.Equals(this))
            {
                return this;
            }
            return null;
        }

        public MethodInfo Method
        {
            [SecuritySafeCritical]
            get
            {
                return this.GetMethodImpl();
            }
        }

        public object Target
        {
            get
            {
                return this.GetTarget();
            }
        }
    }
}

