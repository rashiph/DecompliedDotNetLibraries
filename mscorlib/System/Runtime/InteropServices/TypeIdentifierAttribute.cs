namespace System.Runtime.InteropServices
{
    using System;

    [AttributeUsage(AttributeTargets.Delegate | AttributeTargets.Interface | AttributeTargets.Enum | AttributeTargets.Struct, AllowMultiple=false, Inherited=false), ComVisible(false)]
    public sealed class TypeIdentifierAttribute : Attribute
    {
        internal string Identifier_;
        internal string Scope_;

        public TypeIdentifierAttribute()
        {
        }

        public TypeIdentifierAttribute(string scope, string identifier)
        {
            this.Scope_ = scope;
            this.Identifier_ = identifier;
        }

        public string Identifier
        {
            get
            {
                return this.Identifier_;
            }
        }

        public string Scope
        {
            get
            {
                return this.Scope_;
            }
        }
    }
}

