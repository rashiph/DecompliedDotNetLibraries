namespace System.Reflection
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;

    [Serializable, ComVisible(true), ComDefaultInterface(typeof(_ParameterInfo)), ClassInterface(ClassInterfaceType.None)]
    public class ParameterInfo : _ParameterInfo, ICustomAttributeProvider, IObjectReference
    {
        [OptionalField]
        private IntPtr _importer;
        [OptionalField]
        private int _token;
        protected ParameterAttributes AttrsImpl;
        [OptionalField]
        private bool bExtraConstChecked;
        protected Type ClassImpl;
        protected object DefaultValueImpl;
        protected MemberInfo MemberImpl;
        protected string NameImpl;
        protected int PositionImpl;

        protected ParameterInfo()
        {
        }

        public virtual object[] GetCustomAttributes(bool inherit)
        {
            return new object[0];
        }

        public virtual object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            if (attributeType == null)
            {
                throw new ArgumentNullException("attributeType");
            }
            return new object[0];
        }

        public virtual IList<CustomAttributeData> GetCustomAttributesData()
        {
            throw new NotImplementedException();
        }

        public virtual Type[] GetOptionalCustomModifiers()
        {
            return new Type[0];
        }

        [SecurityCritical]
        public object GetRealObject(StreamingContext context)
        {
            if (this.MemberImpl == null)
            {
                throw new SerializationException(Environment.GetResourceString("Serialization_InsufficientState"));
            }
            ParameterInfo[] indexParametersNoCopy = null;
            MemberTypes memberType = this.MemberImpl.MemberType;
            if ((memberType != MemberTypes.Constructor) && (memberType != MemberTypes.Method))
            {
                if (memberType != MemberTypes.Property)
                {
                    throw new SerializationException(Environment.GetResourceString("Serialization_NoParameterInfo"));
                }
            }
            else
            {
                if (this.PositionImpl == -1)
                {
                    if (this.MemberImpl.MemberType != MemberTypes.Method)
                    {
                        throw new SerializationException(Environment.GetResourceString("Serialization_BadParameterInfo"));
                    }
                    return ((MethodInfo) this.MemberImpl).ReturnParameter;
                }
                indexParametersNoCopy = ((MethodBase) this.MemberImpl).GetParametersNoCopy();
                if ((indexParametersNoCopy == null) || (this.PositionImpl >= indexParametersNoCopy.Length))
                {
                    throw new SerializationException(Environment.GetResourceString("Serialization_BadParameterInfo"));
                }
                return indexParametersNoCopy[this.PositionImpl];
            }
            indexParametersNoCopy = ((RuntimePropertyInfo) this.MemberImpl).GetIndexParametersNoCopy();
            if (((indexParametersNoCopy == null) || (this.PositionImpl <= -1)) || (this.PositionImpl >= indexParametersNoCopy.Length))
            {
                throw new SerializationException(Environment.GetResourceString("Serialization_BadParameterInfo"));
            }
            return indexParametersNoCopy[this.PositionImpl];
        }

        public virtual Type[] GetRequiredCustomModifiers()
        {
            return new Type[0];
        }

        public virtual bool IsDefined(Type attributeType, bool inherit)
        {
            if (attributeType == null)
            {
                throw new ArgumentNullException("attributeType");
            }
            return false;
        }

        internal void SetAttributes(ParameterAttributes attributes)
        {
            this.AttrsImpl = attributes;
        }

        internal void SetName(string name)
        {
            this.NameImpl = name;
        }

        void _ParameterInfo.GetIDsOfNames([In] ref Guid riid, IntPtr rgszNames, uint cNames, uint lcid, IntPtr rgDispId)
        {
            throw new NotImplementedException();
        }

        void _ParameterInfo.GetTypeInfo(uint iTInfo, uint lcid, IntPtr ppTInfo)
        {
            throw new NotImplementedException();
        }

        void _ParameterInfo.GetTypeInfoCount(out uint pcTInfo)
        {
            throw new NotImplementedException();
        }

        void _ParameterInfo.Invoke(uint dispIdMember, [In] ref Guid riid, uint lcid, short wFlags, IntPtr pDispParams, IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return (this.ParameterType.SigToString() + " " + this.Name);
        }

        public virtual ParameterAttributes Attributes
        {
            get
            {
                return this.AttrsImpl;
            }
        }

        public virtual object DefaultValue
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool IsIn
        {
            get
            {
                return ((this.Attributes & ParameterAttributes.In) != ParameterAttributes.None);
            }
        }

        public bool IsLcid
        {
            get
            {
                return ((this.Attributes & ParameterAttributes.Lcid) != ParameterAttributes.None);
            }
        }

        public bool IsOptional
        {
            get
            {
                return ((this.Attributes & ParameterAttributes.Optional) != ParameterAttributes.None);
            }
        }

        public bool IsOut
        {
            get
            {
                return ((this.Attributes & ParameterAttributes.Out) != ParameterAttributes.None);
            }
        }

        public bool IsRetval
        {
            get
            {
                return ((this.Attributes & ParameterAttributes.Retval) != ParameterAttributes.None);
            }
        }

        public virtual MemberInfo Member
        {
            get
            {
                return this.MemberImpl;
            }
        }

        public virtual int MetadataToken
        {
            get
            {
                RuntimeParameterInfo info = this as RuntimeParameterInfo;
                if (info != null)
                {
                    return info.MetadataToken;
                }
                return 0x8000000;
            }
        }

        public virtual string Name
        {
            get
            {
                return this.NameImpl;
            }
        }

        public virtual Type ParameterType
        {
            get
            {
                return this.ClassImpl;
            }
        }

        public virtual int Position
        {
            get
            {
                return this.PositionImpl;
            }
        }

        public virtual object RawDefaultValue
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    }
}

