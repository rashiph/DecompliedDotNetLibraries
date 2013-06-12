namespace System.Runtime.Versioning
{
    using System;
    using System.Diagnostics;

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Constructor, Inherited=false), Conditional("RESOURCE_ANNOTATION_WORK")]
    public sealed class ResourceConsumptionAttribute : Attribute
    {
        private System.Runtime.Versioning.ResourceScope _consumptionScope;
        private System.Runtime.Versioning.ResourceScope _resourceScope;

        public ResourceConsumptionAttribute(System.Runtime.Versioning.ResourceScope resourceScope)
        {
            this._resourceScope = resourceScope;
            this._consumptionScope = this._resourceScope;
        }

        public ResourceConsumptionAttribute(System.Runtime.Versioning.ResourceScope resourceScope, System.Runtime.Versioning.ResourceScope consumptionScope)
        {
            this._resourceScope = resourceScope;
            this._consumptionScope = consumptionScope;
        }

        public System.Runtime.Versioning.ResourceScope ConsumptionScope
        {
            get
            {
                return this._consumptionScope;
            }
        }

        public System.Runtime.Versioning.ResourceScope ResourceScope
        {
            get
            {
                return this._resourceScope;
            }
        }
    }
}

