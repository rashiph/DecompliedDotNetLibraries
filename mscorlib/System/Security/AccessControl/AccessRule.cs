namespace System.Security.AccessControl
{
    using System;
    using System.Security.Principal;

    public abstract class AccessRule : AuthorizationRule
    {
        private readonly System.Security.AccessControl.AccessControlType _type;

        protected AccessRule(IdentityReference identity, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, System.Security.AccessControl.AccessControlType type) : base(identity, accessMask, isInherited, inheritanceFlags, propagationFlags)
        {
            if ((type != System.Security.AccessControl.AccessControlType.Allow) && (type != System.Security.AccessControl.AccessControlType.Deny))
            {
                throw new ArgumentOutOfRangeException("type", Environment.GetResourceString("ArgumentOutOfRange_Enum"));
            }
            if ((inheritanceFlags < InheritanceFlags.None) || (inheritanceFlags > (InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit)))
            {
                throw new ArgumentOutOfRangeException("inheritanceFlags", Environment.GetResourceString("Argument_InvalidEnumValue", new object[] { inheritanceFlags, "InheritanceFlags" }));
            }
            if ((propagationFlags < PropagationFlags.None) || (propagationFlags > (PropagationFlags.InheritOnly | PropagationFlags.NoPropagateInherit)))
            {
                throw new ArgumentOutOfRangeException("propagationFlags", Environment.GetResourceString("Argument_InvalidEnumValue", new object[] { inheritanceFlags, "PropagationFlags" }));
            }
            this._type = type;
        }

        public System.Security.AccessControl.AccessControlType AccessControlType
        {
            get
            {
                return this._type;
            }
        }
    }
}

