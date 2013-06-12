namespace System.Reflection
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;

    [Serializable, ComDefaultInterface(typeof(_FieldInfo)), ComVisible(true), ClassInterface(ClassInterfaceType.None), PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust")]
    public abstract class FieldInfo : MemberInfo, _FieldInfo
    {
        protected FieldInfo()
        {
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        [SecuritySafeCritical]
        public static FieldInfo GetFieldFromHandle(RuntimeFieldHandle handle)
        {
            if (handle.IsNullHandle())
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_InvalidHandle"));
            }
            FieldInfo fieldInfo = RuntimeType.GetFieldInfo(handle.GetRuntimeFieldInfo());
            Type declaringType = fieldInfo.DeclaringType;
            if ((declaringType != null) && declaringType.IsGenericType)
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Argument_FieldDeclaringTypeGeneric"), new object[] { fieldInfo.Name, declaringType.GetGenericTypeDefinition() }));
            }
            return fieldInfo;
        }

        [ComVisible(false), SecuritySafeCritical]
        public static FieldInfo GetFieldFromHandle(RuntimeFieldHandle handle, RuntimeTypeHandle declaringType)
        {
            if (handle.IsNullHandle())
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_InvalidHandle"));
            }
            return RuntimeType.GetFieldInfo(declaringType.GetRuntimeType(), handle.GetRuntimeFieldInfo());
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public virtual Type[] GetOptionalCustomModifiers()
        {
            throw new NotImplementedException();
        }

        public virtual object GetRawConstantValue()
        {
            throw new NotSupportedException(Environment.GetResourceString("NotSupported_AbstractNonCLS"));
        }

        public virtual Type[] GetRequiredCustomModifiers()
        {
            throw new NotImplementedException();
        }

        public abstract object GetValue(object obj);
        [CLSCompliant(false)]
        public virtual object GetValueDirect(TypedReference obj)
        {
            throw new NotSupportedException(Environment.GetResourceString("NotSupported_AbstractNonCLS"));
        }

        public static bool operator ==(FieldInfo left, FieldInfo right)
        {
            return (object.ReferenceEquals(left, right) || ((((left != null) && (right != null)) && (!(left is RuntimeFieldInfo) && !(right is RuntimeFieldInfo))) && left.Equals(right)));
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static bool operator !=(FieldInfo left, FieldInfo right)
        {
            return !(left == right);
        }

        [DebuggerStepThrough, DebuggerHidden]
        public void SetValue(object obj, object value)
        {
            this.SetValue(obj, value, BindingFlags.Default, Type.DefaultBinder, null);
        }

        public abstract void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, CultureInfo culture);
        [CLSCompliant(false)]
        public virtual void SetValueDirect(TypedReference obj, object value)
        {
            throw new NotSupportedException(Environment.GetResourceString("NotSupported_AbstractNonCLS"));
        }

        void _FieldInfo.GetIDsOfNames([In] ref Guid riid, IntPtr rgszNames, uint cNames, uint lcid, IntPtr rgDispId)
        {
            throw new NotImplementedException();
        }

        Type _FieldInfo.GetType()
        {
            return base.GetType();
        }

        void _FieldInfo.GetTypeInfo(uint iTInfo, uint lcid, IntPtr ppTInfo)
        {
            throw new NotImplementedException();
        }

        void _FieldInfo.GetTypeInfoCount(out uint pcTInfo)
        {
            throw new NotImplementedException();
        }

        void _FieldInfo.Invoke(uint dispIdMember, [In] ref Guid riid, uint lcid, short wFlags, IntPtr pDispParams, IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr)
        {
            throw new NotImplementedException();
        }

        public abstract FieldAttributes Attributes { get; }

        public abstract RuntimeFieldHandle FieldHandle { get; }

        public abstract Type FieldType { get; }

        public bool IsAssembly
        {
            get
            {
                return ((this.Attributes & FieldAttributes.FieldAccessMask) == FieldAttributes.Assembly);
            }
        }

        public bool IsFamily
        {
            get
            {
                return ((this.Attributes & FieldAttributes.FieldAccessMask) == FieldAttributes.Family);
            }
        }

        public bool IsFamilyAndAssembly
        {
            get
            {
                return ((this.Attributes & FieldAttributes.FieldAccessMask) == FieldAttributes.FamANDAssem);
            }
        }

        public bool IsFamilyOrAssembly
        {
            get
            {
                return ((this.Attributes & FieldAttributes.FieldAccessMask) == FieldAttributes.FamORAssem);
            }
        }

        public bool IsInitOnly
        {
            get
            {
                return ((this.Attributes & FieldAttributes.InitOnly) != FieldAttributes.PrivateScope);
            }
        }

        public bool IsLiteral
        {
            get
            {
                return ((this.Attributes & FieldAttributes.Literal) != FieldAttributes.PrivateScope);
            }
        }

        public bool IsNotSerialized
        {
            get
            {
                return ((this.Attributes & FieldAttributes.NotSerialized) != FieldAttributes.PrivateScope);
            }
        }

        public bool IsPinvokeImpl
        {
            get
            {
                return ((this.Attributes & FieldAttributes.PinvokeImpl) != FieldAttributes.PrivateScope);
            }
        }

        public bool IsPrivate
        {
            get
            {
                return ((this.Attributes & FieldAttributes.FieldAccessMask) == FieldAttributes.Private);
            }
        }

        public bool IsPublic
        {
            get
            {
                return ((this.Attributes & FieldAttributes.FieldAccessMask) == FieldAttributes.Public);
            }
        }

        public virtual bool IsSecurityCritical
        {
            get
            {
                return this.FieldHandle.IsSecurityCritical();
            }
        }

        public virtual bool IsSecuritySafeCritical
        {
            get
            {
                return this.FieldHandle.IsSecuritySafeCritical();
            }
        }

        public virtual bool IsSecurityTransparent
        {
            get
            {
                return this.FieldHandle.IsSecurityTransparent();
            }
        }

        public bool IsSpecialName
        {
            get
            {
                return ((this.Attributes & FieldAttributes.SpecialName) != FieldAttributes.PrivateScope);
            }
        }

        public bool IsStatic
        {
            get
            {
                return ((this.Attributes & FieldAttributes.Static) != FieldAttributes.PrivateScope);
            }
        }

        public override MemberTypes MemberType
        {
            get
            {
                return MemberTypes.Field;
            }
        }
    }
}

