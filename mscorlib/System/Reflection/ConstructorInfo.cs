namespace System.Reflection
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    [Serializable, ComDefaultInterface(typeof(_ConstructorInfo)), ClassInterface(ClassInterfaceType.None), ComVisible(true), PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust")]
    public abstract class ConstructorInfo : MethodBase, _ConstructorInfo
    {
        [ComVisible(true)]
        public static readonly string ConstructorName = ".ctor";
        [ComVisible(true)]
        public static readonly string TypeConstructorName = ".cctor";

        protected ConstructorInfo()
        {
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        internal virtual Type GetReturnType()
        {
            throw new NotImplementedException();
        }

        [DebuggerStepThrough, DebuggerHidden]
        public object Invoke(object[] parameters)
        {
            return this.Invoke(BindingFlags.Default, null, parameters, null);
        }

        public abstract object Invoke(BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture);
        public static bool operator ==(ConstructorInfo left, ConstructorInfo right)
        {
            return (object.ReferenceEquals(left, right) || ((((left != null) && (right != null)) && (!(left is RuntimeConstructorInfo) && !(right is RuntimeConstructorInfo))) && left.Equals(right)));
        }

        public static bool operator !=(ConstructorInfo left, ConstructorInfo right)
        {
            return !(left == right);
        }

        void _ConstructorInfo.GetIDsOfNames([In] ref Guid riid, IntPtr rgszNames, uint cNames, uint lcid, IntPtr rgDispId)
        {
            throw new NotImplementedException();
        }

        Type _ConstructorInfo.GetType()
        {
            return base.GetType();
        }

        void _ConstructorInfo.GetTypeInfo(uint iTInfo, uint lcid, IntPtr ppTInfo)
        {
            throw new NotImplementedException();
        }

        void _ConstructorInfo.GetTypeInfoCount(out uint pcTInfo)
        {
            throw new NotImplementedException();
        }

        void _ConstructorInfo.Invoke(uint dispIdMember, [In] ref Guid riid, uint lcid, short wFlags, IntPtr pDispParams, IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr)
        {
            throw new NotImplementedException();
        }

        object _ConstructorInfo.Invoke_2(object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
        {
            return this.Invoke(obj, invokeAttr, binder, parameters, culture);
        }

        object _ConstructorInfo.Invoke_3(object obj, object[] parameters)
        {
            return base.Invoke(obj, parameters);
        }

        object _ConstructorInfo.Invoke_4(BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
        {
            return this.Invoke(invokeAttr, binder, parameters, culture);
        }

        object _ConstructorInfo.Invoke_5(object[] parameters)
        {
            return this.Invoke(parameters);
        }

        [ComVisible(true)]
        public override MemberTypes MemberType
        {
            get
            {
                return MemberTypes.Constructor;
            }
        }
    }
}

