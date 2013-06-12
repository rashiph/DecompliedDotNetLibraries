namespace System.Reflection
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    [Serializable, ComVisible(true), ClassInterface(ClassInterfaceType.None), ComDefaultInterface(typeof(_MemberInfo)), PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust")]
    public abstract class MemberInfo : ICustomAttributeProvider, _MemberInfo
    {
        protected MemberInfo()
        {
        }

        internal virtual bool CacheEquals(object o)
        {
            throw new NotImplementedException();
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public abstract object[] GetCustomAttributes(bool inherit);
        public abstract object[] GetCustomAttributes(Type attributeType, bool inherit);
        public virtual IList<CustomAttributeData> GetCustomAttributesData()
        {
            throw new NotImplementedException();
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public abstract bool IsDefined(Type attributeType, bool inherit);
        public static bool operator ==(MemberInfo left, MemberInfo right)
        {
            Type type;
            Type type2;
            MethodBase base2;
            MethodBase base3;
            FieldInfo info;
            FieldInfo info2;
            EventInfo info3;
            EventInfo info4;
            PropertyInfo info5;
            PropertyInfo info6;
            if (object.ReferenceEquals(left, right))
            {
                return true;
            }
            if ((left == null) || (right == null))
            {
                return false;
            }
            if (((type = left as Type) != null) && ((type2 = right as Type) != null))
            {
                return (type == type2);
            }
            if (((base2 = left as MethodBase) != null) && ((base3 = right as MethodBase) != null))
            {
                return (base2 == base3);
            }
            if (((info = left as FieldInfo) != null) && ((info2 = right as FieldInfo) != null))
            {
                return (info == info2);
            }
            if (((info3 = left as EventInfo) != null) && ((info4 = right as EventInfo) != null))
            {
                return (info3 == info4);
            }
            return ((((info5 = left as PropertyInfo) != null) && ((info6 = right as PropertyInfo) != null)) && (info5 == info6));
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static bool operator !=(MemberInfo left, MemberInfo right)
        {
            return !(left == right);
        }

        void _MemberInfo.GetIDsOfNames([In] ref Guid riid, IntPtr rgszNames, uint cNames, uint lcid, IntPtr rgDispId)
        {
            throw new NotImplementedException();
        }

        Type _MemberInfo.GetType()
        {
            return base.GetType();
        }

        void _MemberInfo.GetTypeInfo(uint iTInfo, uint lcid, IntPtr ppTInfo)
        {
            throw new NotImplementedException();
        }

        void _MemberInfo.GetTypeInfoCount(out uint pcTInfo)
        {
            throw new NotImplementedException();
        }

        void _MemberInfo.Invoke(uint dispIdMember, [In] ref Guid riid, uint lcid, short wFlags, IntPtr pDispParams, IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr)
        {
            throw new NotImplementedException();
        }

        public abstract Type DeclaringType { get; }

        public abstract MemberTypes MemberType { get; }

        public virtual int MetadataToken
        {
            get
            {
                throw new InvalidOperationException();
            }
        }

        public virtual System.Reflection.Module Module
        {
            get
            {
                if (!(this is Type))
                {
                    throw new NotImplementedException();
                }
                return ((Type) this).Module;
            }
        }

        public abstract string Name { get; }

        public abstract Type ReflectedType { get; }
    }
}

