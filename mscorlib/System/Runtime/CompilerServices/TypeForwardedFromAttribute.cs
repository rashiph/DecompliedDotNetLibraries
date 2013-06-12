namespace System.Runtime.CompilerServices
{
    using System;

    [AttributeUsage(AttributeTargets.Delegate | AttributeTargets.Interface | AttributeTargets.Enum | AttributeTargets.Struct | AttributeTargets.Class, Inherited=false, AllowMultiple=false)]
    public sealed class TypeForwardedFromAttribute : Attribute
    {
        private string assemblyFullName;

        private TypeForwardedFromAttribute()
        {
        }

        public TypeForwardedFromAttribute(string assemblyFullName)
        {
            if (string.IsNullOrEmpty(assemblyFullName))
            {
                throw new ArgumentNullException("assemblyFullName");
            }
            this.assemblyFullName = assemblyFullName;
        }

        public string AssemblyFullName
        {
            get
            {
                return this.assemblyFullName;
            }
        }
    }
}

