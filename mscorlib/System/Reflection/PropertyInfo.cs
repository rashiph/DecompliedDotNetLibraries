namespace System.Reflection
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    [Serializable, ClassInterface(ClassInterfaceType.None), ComDefaultInterface(typeof(_PropertyInfo)), ComVisible(true), PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust")]
    public abstract class PropertyInfo : MemberInfo, _PropertyInfo
    {
        protected PropertyInfo()
        {
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public MethodInfo[] GetAccessors()
        {
            return this.GetAccessors(false);
        }

        public abstract MethodInfo[] GetAccessors(bool nonPublic);
        public virtual object GetConstantValue()
        {
            throw new NotImplementedException();
        }

        public MethodInfo GetGetMethod()
        {
            return this.GetGetMethod(false);
        }

        public abstract MethodInfo GetGetMethod(bool nonPublic);
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public abstract ParameterInfo[] GetIndexParameters();
        public virtual Type[] GetOptionalCustomModifiers()
        {
            return new Type[0];
        }

        public virtual object GetRawConstantValue()
        {
            throw new NotImplementedException();
        }

        public virtual Type[] GetRequiredCustomModifiers()
        {
            return new Type[0];
        }

        public MethodInfo GetSetMethod()
        {
            return this.GetSetMethod(false);
        }

        public abstract MethodInfo GetSetMethod(bool nonPublic);
        [DebuggerHidden, DebuggerStepThrough]
        public virtual object GetValue(object obj, object[] index)
        {
            return this.GetValue(obj, BindingFlags.Default, null, index, null);
        }

        public abstract object GetValue(object obj, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture);
        public static bool operator ==(PropertyInfo left, PropertyInfo right)
        {
            return (object.ReferenceEquals(left, right) || ((((left != null) && (right != null)) && (!(left is RuntimePropertyInfo) && !(right is RuntimePropertyInfo))) && left.Equals(right)));
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static bool operator !=(PropertyInfo left, PropertyInfo right)
        {
            return !(left == right);
        }

        [DebuggerStepThrough, DebuggerHidden, TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public virtual void SetValue(object obj, object value, object[] index)
        {
            this.SetValue(obj, value, BindingFlags.Default, null, index, null);
        }

        public abstract void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture);
        void _PropertyInfo.GetIDsOfNames([In] ref Guid riid, IntPtr rgszNames, uint cNames, uint lcid, IntPtr rgDispId)
        {
            throw new NotImplementedException();
        }

        Type _PropertyInfo.GetType()
        {
            return base.GetType();
        }

        void _PropertyInfo.GetTypeInfo(uint iTInfo, uint lcid, IntPtr ppTInfo)
        {
            throw new NotImplementedException();
        }

        void _PropertyInfo.GetTypeInfoCount(out uint pcTInfo)
        {
            throw new NotImplementedException();
        }

        void _PropertyInfo.Invoke(uint dispIdMember, [In] ref Guid riid, uint lcid, short wFlags, IntPtr pDispParams, IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr)
        {
            throw new NotImplementedException();
        }

        public abstract PropertyAttributes Attributes { get; }

        public abstract bool CanRead { get; }

        public abstract bool CanWrite { get; }

        public bool IsSpecialName
        {
            get
            {
                return ((this.Attributes & PropertyAttributes.SpecialName) != PropertyAttributes.None);
            }
        }

        public override MemberTypes MemberType
        {
            get
            {
                return MemberTypes.Property;
            }
        }

        public abstract Type PropertyType { get; }
    }
}

