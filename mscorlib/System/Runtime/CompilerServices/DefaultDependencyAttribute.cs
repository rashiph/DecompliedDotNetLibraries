namespace System.Runtime.CompilerServices
{
    using System;

    [Serializable, AttributeUsage(AttributeTargets.Assembly)]
    public sealed class DefaultDependencyAttribute : Attribute
    {
        private System.Runtime.CompilerServices.LoadHint loadHint;

        public DefaultDependencyAttribute(System.Runtime.CompilerServices.LoadHint loadHintArgument)
        {
            this.loadHint = loadHintArgument;
        }

        public System.Runtime.CompilerServices.LoadHint LoadHint
        {
            get
            {
                return this.loadHint;
            }
        }
    }
}

