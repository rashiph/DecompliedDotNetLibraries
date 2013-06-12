namespace System.Security.AccessControl
{
    using System;
    using System.Security.Principal;

    public abstract class KnownAce : GenericAce
    {
        private int _accessMask;
        private System.Security.Principal.SecurityIdentifier _sid;
        internal const int AccessMaskLength = 4;

        internal KnownAce(AceType type, AceFlags flags, int accessMask, System.Security.Principal.SecurityIdentifier securityIdentifier) : base(type, flags)
        {
            if (securityIdentifier == null)
            {
                throw new ArgumentNullException("securityIdentifier");
            }
            this.AccessMask = accessMask;
            this.SecurityIdentifier = securityIdentifier;
        }

        public int AccessMask
        {
            get
            {
                return this._accessMask;
            }
            set
            {
                this._accessMask = value;
            }
        }

        public System.Security.Principal.SecurityIdentifier SecurityIdentifier
        {
            get
            {
                return this._sid;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                this._sid = value;
            }
        }
    }
}

