namespace System.ServiceModel
{
    using System;

    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Method | AttributeTargets.Class, Inherited=true, AllowMultiple=true)]
    public sealed class ServiceKnownTypeAttribute : Attribute
    {
        private System.Type declaringType;
        private string methodName;
        private System.Type type;

        private ServiceKnownTypeAttribute()
        {
        }

        public ServiceKnownTypeAttribute(string methodName)
        {
            this.methodName = methodName;
        }

        public ServiceKnownTypeAttribute(System.Type type)
        {
            this.type = type;
        }

        public ServiceKnownTypeAttribute(string methodName, System.Type declaringType)
        {
            this.methodName = methodName;
            this.declaringType = declaringType;
        }

        public System.Type DeclaringType
        {
            get
            {
                return this.declaringType;
            }
        }

        public string MethodName
        {
            get
            {
                return this.methodName;
            }
        }

        public System.Type Type
        {
            get
            {
                return this.type;
            }
        }
    }
}

