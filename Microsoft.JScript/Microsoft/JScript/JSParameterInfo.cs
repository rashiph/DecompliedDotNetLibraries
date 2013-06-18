namespace Microsoft.JScript
{
    using System;
    using System.Reflection;

    internal sealed class JSParameterInfo : ParameterInfo
    {
        private object[] attributes;
        private ParameterInfo parameter;
        private Type parameterType;

        internal JSParameterInfo(ParameterInfo parameter)
        {
            this.parameter = parameter;
        }

        public sealed override object[] GetCustomAttributes(bool inherit)
        {
            object[] attributes = this.attributes;
            if (attributes != null)
            {
                return attributes;
            }
            return (this.attributes = this.parameter.GetCustomAttributes(true));
        }

        public sealed override object[] GetCustomAttributes(Type type, bool inherit)
        {
            object[] attributes = this.attributes;
            if (attributes != null)
            {
                return attributes;
            }
            return (this.attributes = Microsoft.JScript.CustomAttribute.GetCustomAttributes(this.parameter, type, true));
        }

        public sealed override bool IsDefined(Type type, bool inherit)
        {
            object[] attributes = this.attributes;
            if (attributes == null)
            {
                this.attributes = attributes = Microsoft.JScript.CustomAttribute.GetCustomAttributes(this.parameter, type, true);
            }
            return (attributes.Length > 0);
        }

        public override object DefaultValue
        {
            get
            {
                return TypeReferences.GetDefaultParameterValue(this.parameter);
            }
        }

        public override string Name
        {
            get
            {
                return this.parameter.Name;
            }
        }

        public override Type ParameterType
        {
            get
            {
                Type parameterType = this.parameterType;
                if (parameterType != null)
                {
                    return parameterType;
                }
                return (this.parameterType = this.parameter.ParameterType);
            }
        }
    }
}

