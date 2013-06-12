namespace System.Runtime.CompilerServices
{
    using System;

    [Serializable, AttributeUsage(AttributeTargets.Assembly, AllowMultiple=true)]
    public sealed class DependencyAttribute : Attribute
    {
        private string dependentAssembly;
        private System.Runtime.CompilerServices.LoadHint loadHint;

        public DependencyAttribute(string dependentAssemblyArgument, System.Runtime.CompilerServices.LoadHint loadHintArgument)
        {
            this.dependentAssembly = dependentAssemblyArgument;
            this.loadHint = loadHintArgument;
        }

        public string DependentAssembly
        {
            get
            {
                return this.dependentAssembly;
            }
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

