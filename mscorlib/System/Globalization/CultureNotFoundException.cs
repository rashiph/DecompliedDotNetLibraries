namespace System.Globalization
{
    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;

    [Serializable, ComVisible(true)]
    public class CultureNotFoundException : ArgumentException, ISerializable
    {
        private int? m_invalidCultureId;
        private string m_invalidCultureName;

        public CultureNotFoundException() : base(DefaultMessage)
        {
        }

        public CultureNotFoundException(string message) : base(message)
        {
        }

        [SecuritySafeCritical]
        protected CultureNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            this.m_invalidCultureId = (int?) info.GetValue("InvalidCultureId", typeof(int?));
            this.m_invalidCultureName = (string) info.GetValue("InvalidCultureName", typeof(string));
        }

        public CultureNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public CultureNotFoundException(string paramName, string message) : base(message, paramName)
        {
        }

        public CultureNotFoundException(string message, int invalidCultureId, Exception innerException) : base(message, innerException)
        {
            this.m_invalidCultureId = new int?(invalidCultureId);
        }

        public CultureNotFoundException(string paramName, int invalidCultureId, string message) : base(message, paramName)
        {
            this.m_invalidCultureId = new int?(invalidCultureId);
        }

        public CultureNotFoundException(string message, string invalidCultureName, Exception innerException) : base(message, innerException)
        {
            this.m_invalidCultureName = invalidCultureName;
        }

        public CultureNotFoundException(string paramName, string invalidCultureName, string message) : base(message, paramName)
        {
            this.m_invalidCultureName = invalidCultureName;
        }

        [SecurityCritical]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            base.GetObjectData(info, context);
            info.AddValue("InvalidCultureId", this.m_invalidCultureId, typeof(int?));
            info.AddValue("InvalidCultureName", this.m_invalidCultureName, typeof(string));
        }

        private static string DefaultMessage
        {
            get
            {
                return Environment.GetResourceString("Argument_CultureNotSupported");
            }
        }

        private string FormatedInvalidCultureId
        {
            get
            {
                if (this.InvalidCultureId.HasValue)
                {
                    return string.Format(CultureInfo.InvariantCulture, "{0} (0x{0:x4})", new object[] { this.InvalidCultureId.Value });
                }
                return this.InvalidCultureName;
            }
        }

        public virtual int? InvalidCultureId
        {
            get
            {
                return this.m_invalidCultureId;
            }
        }

        public virtual string InvalidCultureName
        {
            get
            {
                return this.m_invalidCultureName;
            }
        }

        public override string Message
        {
            get
            {
                string message = base.Message;
                if (!this.m_invalidCultureId.HasValue && (this.m_invalidCultureName == null))
                {
                    return message;
                }
                string resourceString = Environment.GetResourceString("Argument_CultureInvalidIdentifier", new object[] { this.FormatedInvalidCultureId });
                if (message == null)
                {
                    return resourceString;
                }
                return (message + Environment.NewLine + resourceString);
            }
        }
    }
}

