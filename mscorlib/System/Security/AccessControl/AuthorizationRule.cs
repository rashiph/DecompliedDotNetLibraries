namespace System.Security.AccessControl
{
    using System;
    using System.Security.Principal;

    public abstract class AuthorizationRule
    {
        private readonly int _accessMask;
        private readonly System.Security.Principal.IdentityReference _identity;
        private readonly System.Security.AccessControl.InheritanceFlags _inheritanceFlags;
        private readonly bool _isInherited;
        private readonly System.Security.AccessControl.PropagationFlags _propagationFlags;

        protected internal AuthorizationRule(System.Security.Principal.IdentityReference identity, int accessMask, bool isInherited, System.Security.AccessControl.InheritanceFlags inheritanceFlags, System.Security.AccessControl.PropagationFlags propagationFlags)
        {
            if (identity == null)
            {
                throw new ArgumentNullException("identity");
            }
            if (accessMask == 0)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_ArgumentZero"), "accessMask");
            }
            if ((inheritanceFlags < System.Security.AccessControl.InheritanceFlags.None) || (inheritanceFlags > (System.Security.AccessControl.InheritanceFlags.ObjectInherit | System.Security.AccessControl.InheritanceFlags.ContainerInherit)))
            {
                throw new ArgumentOutOfRangeException("inheritanceFlags", Environment.GetResourceString("Argument_InvalidEnumValue", new object[] { inheritanceFlags, "InheritanceFlags" }));
            }
            if ((propagationFlags < System.Security.AccessControl.PropagationFlags.None) || (propagationFlags > (System.Security.AccessControl.PropagationFlags.InheritOnly | System.Security.AccessControl.PropagationFlags.NoPropagateInherit)))
            {
                throw new ArgumentOutOfRangeException("propagationFlags", Environment.GetResourceString("Argument_InvalidEnumValue", new object[] { inheritanceFlags, "PropagationFlags" }));
            }
            if (!identity.IsValidTargetType(typeof(SecurityIdentifier)))
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_MustBeIdentityReferenceType"), "identity");
            }
            this._identity = identity;
            this._accessMask = accessMask;
            this._isInherited = isInherited;
            this._inheritanceFlags = inheritanceFlags;
            if (inheritanceFlags != System.Security.AccessControl.InheritanceFlags.None)
            {
                this._propagationFlags = propagationFlags;
            }
            else
            {
                this._propagationFlags = System.Security.AccessControl.PropagationFlags.None;
            }
        }

        protected internal int AccessMask
        {
            get
            {
                return this._accessMask;
            }
        }

        public System.Security.Principal.IdentityReference IdentityReference
        {
            get
            {
                return this._identity;
            }
        }

        public System.Security.AccessControl.InheritanceFlags InheritanceFlags
        {
            get
            {
                return this._inheritanceFlags;
            }
        }

        public bool IsInherited
        {
            get
            {
                return this._isInherited;
            }
        }

        public System.Security.AccessControl.PropagationFlags PropagationFlags
        {
            get
            {
                return this._propagationFlags;
            }
        }
    }
}

