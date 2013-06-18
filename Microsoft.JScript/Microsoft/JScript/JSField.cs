namespace Microsoft.JScript
{
    using System;
    using System.Reflection;

    public abstract class JSField : FieldInfo
    {
        protected JSField()
        {
        }

        internal virtual string GetClassFullName()
        {
            throw new JScriptException(JSError.InternalError);
        }

        public override object[] GetCustomAttributes(bool inherit)
        {
            return new FieldInfo[0];
        }

        public override object[] GetCustomAttributes(Type t, bool inherit)
        {
            return new FieldInfo[0];
        }

        internal virtual object GetMetaData()
        {
            throw new JScriptException(JSError.InternalError);
        }

        internal virtual PackageScope GetPackage()
        {
            throw new JScriptException(JSError.InternalError);
        }

        public override bool IsDefined(Type type, bool inherit)
        {
            return false;
        }

        public override FieldAttributes Attributes
        {
            get
            {
                return FieldAttributes.PrivateScope;
            }
        }

        public override Type DeclaringType
        {
            get
            {
                return null;
            }
        }

        public override RuntimeFieldHandle FieldHandle
        {
            get
            {
                return ((FieldInfo) this.GetMetaData()).FieldHandle;
            }
        }

        public override Type FieldType
        {
            get
            {
                return Typeob.Object;
            }
        }

        public override MemberTypes MemberType
        {
            get
            {
                return MemberTypes.Field;
            }
        }

        public override string Name
        {
            get
            {
                return "";
            }
        }

        public override Type ReflectedType
        {
            get
            {
                return this.DeclaringType;
            }
        }
    }
}

