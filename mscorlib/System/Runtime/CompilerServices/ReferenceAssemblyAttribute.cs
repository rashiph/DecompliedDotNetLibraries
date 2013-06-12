namespace System.Runtime.CompilerServices
{
    using System;

    [Serializable, AttributeUsage(AttributeTargets.Assembly, AllowMultiple=false)]
    public sealed class ReferenceAssemblyAttribute : Attribute
    {
        private string _description;

        public ReferenceAssemblyAttribute()
        {
        }

        public ReferenceAssemblyAttribute(string description)
        {
            this._description = description;
        }

        public string Description
        {
            get
            {
                return this._description;
            }
        }
    }
}

