namespace System.Security
{
    using System;
    using System.Runtime.InteropServices;

    [ComVisible(true), AttributeUsage(AttributeTargets.Assembly, AllowMultiple=false, Inherited=false)]
    public sealed class AllowPartiallyTrustedCallersAttribute : Attribute
    {
        private System.Security.PartialTrustVisibilityLevel _visibilityLevel;

        public System.Security.PartialTrustVisibilityLevel PartialTrustVisibilityLevel
        {
            get
            {
                return this._visibilityLevel;
            }
            set
            {
                this._visibilityLevel = value;
            }
        }
    }
}

