namespace System.Runtime.Versioning
{
    using System;

    [AttributeUsage(AttributeTargets.Delegate | AttributeTargets.Interface | AttributeTargets.Event | AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Enum | AttributeTargets.Struct | AttributeTargets.Class | AttributeTargets.Module | AttributeTargets.Assembly, AllowMultiple=false, Inherited=false)]
    public sealed class ComponentGuaranteesAttribute : Attribute
    {
        private ComponentGuaranteesOptions _guarantees;

        public ComponentGuaranteesAttribute(ComponentGuaranteesOptions guarantees)
        {
            this._guarantees = guarantees;
        }

        public ComponentGuaranteesOptions Guarantees
        {
            get
            {
                return this._guarantees;
            }
        }
    }
}

