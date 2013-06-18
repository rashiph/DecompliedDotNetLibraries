namespace System.Data.Common
{
    using System;

    [Serializable, AttributeUsage(AttributeTargets.Property, AllowMultiple=false, Inherited=true)]
    public sealed class DbProviderSpecificTypePropertyAttribute : Attribute
    {
        private bool _isProviderSpecificTypeProperty;

        public DbProviderSpecificTypePropertyAttribute(bool isProviderSpecificTypeProperty)
        {
            this._isProviderSpecificTypeProperty = isProviderSpecificTypeProperty;
        }

        public bool IsProviderSpecificTypeProperty
        {
            get
            {
                return this._isProviderSpecificTypeProperty;
            }
        }
    }
}

