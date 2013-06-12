namespace System.Security
{
    using System;

    [AttributeUsage(AttributeTargets.Delegate | AttributeTargets.Interface | AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Enum | AttributeTargets.Struct | AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple=false, Inherited=false)]
    public sealed class SecurityCriticalAttribute : Attribute
    {
        private SecurityCriticalScope _val;

        public SecurityCriticalAttribute()
        {
        }

        public SecurityCriticalAttribute(SecurityCriticalScope scope)
        {
            this._val = scope;
        }

        [Obsolete("SecurityCriticalScope is only used for .NET 2.0 transparency compatibility.")]
        public SecurityCriticalScope Scope
        {
            get
            {
                return this._val;
            }
        }
    }
}

