namespace System.Runtime.Versioning
{
    using System;
    using System.Diagnostics;

    [Conditional("RESOURCE_ANNOTATION_WORK"), AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Constructor, Inherited=false)]
    public sealed class ResourceExposureAttribute : Attribute
    {
        private ResourceScope _resourceExposureLevel;

        public ResourceExposureAttribute(ResourceScope exposureLevel)
        {
            this._resourceExposureLevel = exposureLevel;
        }

        public ResourceScope ResourceExposureLevel
        {
            get
            {
                return this._resourceExposureLevel;
            }
        }
    }
}

