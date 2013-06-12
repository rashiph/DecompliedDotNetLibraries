namespace System.Reflection
{
    using System;
    using System.Globalization;
    using System.Runtime.InteropServices;

    [Serializable, StructLayout(LayoutKind.Sequential), ComVisible(true)]
    public struct CustomAttributeNamedArgument
    {
        private System.Reflection.MemberInfo m_memberInfo;
        private CustomAttributeTypedArgument m_value;
        public static bool operator ==(CustomAttributeNamedArgument left, CustomAttributeNamedArgument right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(CustomAttributeNamedArgument left, CustomAttributeNamedArgument right)
        {
            return !left.Equals(right);
        }

        public CustomAttributeNamedArgument(System.Reflection.MemberInfo memberInfo, object value)
        {
            if (memberInfo == null)
            {
                throw new ArgumentNullException("memberInfo");
            }
            Type argumentType = null;
            FieldInfo info = memberInfo as FieldInfo;
            PropertyInfo info2 = memberInfo as PropertyInfo;
            if (info != null)
            {
                argumentType = info.FieldType;
            }
            else
            {
                if (info2 == null)
                {
                    throw new ArgumentException(Environment.GetResourceString("Argument_InvalidMemberForNamedArgument"));
                }
                argumentType = info2.PropertyType;
            }
            this.m_memberInfo = memberInfo;
            this.m_value = new CustomAttributeTypedArgument(argumentType, value);
        }

        public CustomAttributeNamedArgument(System.Reflection.MemberInfo memberInfo, CustomAttributeTypedArgument typedArgument)
        {
            if (memberInfo == null)
            {
                throw new ArgumentNullException("memberInfo");
            }
            this.m_memberInfo = memberInfo;
            this.m_value = typedArgument;
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "{0} = {1}", new object[] { this.MemberInfo.Name, this.TypedValue.ToString(this.ArgumentType != typeof(object)) });
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return (obj == this);
        }

        internal Type ArgumentType
        {
            get
            {
                if (this.m_memberInfo is FieldInfo)
                {
                    return ((FieldInfo) this.m_memberInfo).FieldType;
                }
                return ((PropertyInfo) this.m_memberInfo).PropertyType;
            }
        }
        public System.Reflection.MemberInfo MemberInfo
        {
            get
            {
                return this.m_memberInfo;
            }
        }
        public CustomAttributeTypedArgument TypedValue
        {
            get
            {
                return this.m_value;
            }
        }
    }
}

