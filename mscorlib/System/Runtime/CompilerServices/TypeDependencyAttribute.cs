namespace System.Runtime.CompilerServices
{
    using System;

    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Struct | AttributeTargets.Class, AllowMultiple=true, Inherited=false)]
    internal sealed class TypeDependencyAttribute : Attribute
    {
        private string typeName;

        public TypeDependencyAttribute(string typeName)
        {
            if (typeName == null)
            {
                throw new ArgumentNullException("typeName");
            }
            this.typeName = typeName;
        }
    }
}

