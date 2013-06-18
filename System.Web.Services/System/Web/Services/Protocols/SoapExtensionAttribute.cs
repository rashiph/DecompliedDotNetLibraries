namespace System.Web.Services.Protocols
{
    using System;
    using System.Runtime;

    public abstract class SoapExtensionAttribute : Attribute
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected SoapExtensionAttribute()
        {
        }

        public abstract Type ExtensionType { get; }

        public abstract int Priority { get; set; }
    }
}

