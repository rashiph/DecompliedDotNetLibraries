namespace System.ComponentModel
{
    using System;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    [Serializable, HostProtection(SecurityAction.LinkDemand, SharedState=true)]
    public class LicenseException : SystemException
    {
        private object instance;
        private Type type;

        public LicenseException(Type type) : this(type, null, SR.GetString("LicExceptionTypeOnly", new object[] { type.FullName }))
        {
        }

        protected LicenseException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            this.type = (Type) info.GetValue("type", typeof(Type));
            this.instance = info.GetValue("instance", typeof(object));
        }

        public LicenseException(Type type, object instance) : this(type, null, SR.GetString("LicExceptionTypeAndInstance", new object[] { type.FullName, instance.GetType().FullName }))
        {
        }

        public LicenseException(Type type, object instance, string message) : base(message)
        {
            this.type = type;
            this.instance = instance;
            base.HResult = -2146232063;
        }

        public LicenseException(Type type, object instance, string message, Exception innerException) : base(message, innerException)
        {
            this.type = type;
            this.instance = instance;
            base.HResult = -2146232063;
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            info.AddValue("type", this.type);
            info.AddValue("instance", this.instance);
            base.GetObjectData(info, context);
        }

        public Type LicensedType
        {
            get
            {
                return this.type;
            }
        }
    }
}

