namespace System.Security.AccessControl
{
    using System;
    using System.Runtime.Serialization;
    using System.Security;

    [Serializable]
    public sealed class PrivilegeNotHeldException : UnauthorizedAccessException, ISerializable
    {
        private readonly string _privilegeName;

        public PrivilegeNotHeldException() : base(Environment.GetResourceString("PrivilegeNotHeld_Default"))
        {
        }

        public PrivilegeNotHeldException(string privilege) : base(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("PrivilegeNotHeld_Named"), new object[] { privilege }))
        {
            this._privilegeName = privilege;
        }

        internal PrivilegeNotHeldException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            this._privilegeName = info.GetString("PrivilegeName");
        }

        public PrivilegeNotHeldException(string privilege, Exception inner) : base(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("PrivilegeNotHeld_Named"), new object[] { privilege }), inner)
        {
            this._privilegeName = privilege;
        }

        [SecurityCritical]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            base.GetObjectData(info, context);
            info.AddValue("PrivilegeName", this._privilegeName, typeof(string));
        }

        public string PrivilegeName
        {
            get
            {
                return this._privilegeName;
            }
        }
    }
}

