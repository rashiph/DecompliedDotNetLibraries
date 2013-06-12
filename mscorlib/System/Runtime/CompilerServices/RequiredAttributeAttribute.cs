namespace System.Runtime.CompilerServices
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, AttributeUsage(AttributeTargets.Interface | AttributeTargets.Enum | AttributeTargets.Struct | AttributeTargets.Class, AllowMultiple=true, Inherited=false), ComVisible(true)]
    public sealed class RequiredAttributeAttribute : Attribute
    {
        private Type requiredContract;

        public RequiredAttributeAttribute(Type requiredContract)
        {
            this.requiredContract = requiredContract;
        }

        public Type RequiredContract
        {
            get
            {
                return this.requiredContract;
            }
        }
    }
}

