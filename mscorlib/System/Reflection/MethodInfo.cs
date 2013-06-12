namespace System.Reflection
{
    using System;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    [Serializable, ClassInterface(ClassInterfaceType.None), ComVisible(true), ComDefaultInterface(typeof(_MethodInfo)), PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust")]
    public abstract class MethodInfo : MethodBase, _MethodInfo
    {
        protected MethodInfo()
        {
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public abstract MethodInfo GetBaseDefinition();
        [ComVisible(true)]
        public override Type[] GetGenericArguments()
        {
            throw new NotSupportedException(Environment.GetResourceString("NotSupported_SubclassOverride"));
        }

        [ComVisible(true)]
        public virtual MethodInfo GetGenericMethodDefinition()
        {
            throw new NotSupportedException(Environment.GetResourceString("NotSupported_SubclassOverride"));
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public virtual MethodInfo MakeGenericMethod(params Type[] typeArguments)
        {
            throw new NotSupportedException(Environment.GetResourceString("NotSupported_SubclassOverride"));
        }

        public static bool operator ==(MethodInfo left, MethodInfo right)
        {
            return (object.ReferenceEquals(left, right) || ((((left != null) && (right != null)) && (!(left is RuntimeMethodInfo) && !(right is RuntimeMethodInfo))) && left.Equals(right)));
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static bool operator !=(MethodInfo left, MethodInfo right)
        {
            return !(left == right);
        }

        void _MethodInfo.GetIDsOfNames([In] ref Guid riid, IntPtr rgszNames, uint cNames, uint lcid, IntPtr rgDispId)
        {
            throw new NotImplementedException();
        }

        Type _MethodInfo.GetType()
        {
            return base.GetType();
        }

        void _MethodInfo.GetTypeInfo(uint iTInfo, uint lcid, IntPtr ppTInfo)
        {
            throw new NotImplementedException();
        }

        void _MethodInfo.GetTypeInfoCount(out uint pcTInfo)
        {
            throw new NotImplementedException();
        }

        void _MethodInfo.Invoke(uint dispIdMember, [In] ref Guid riid, uint lcid, short wFlags, IntPtr pDispParams, IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr)
        {
            throw new NotImplementedException();
        }

        public override MemberTypes MemberType
        {
            get
            {
                return MemberTypes.Method;
            }
        }

        public virtual ParameterInfo ReturnParameter
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public virtual Type ReturnType
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public abstract ICustomAttributeProvider ReturnTypeCustomAttributes { get; }
    }
}

