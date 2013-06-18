namespace System.Web.Compilation
{
    using System;

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class DesignTimeResourceProviderFactoryAttribute : Attribute
    {
        private string _factoryTypeName;

        public DesignTimeResourceProviderFactoryAttribute(string factoryTypeName)
        {
            this._factoryTypeName = factoryTypeName;
        }

        public DesignTimeResourceProviderFactoryAttribute(Type factoryType)
        {
            this._factoryTypeName = factoryType.AssemblyQualifiedName;
        }

        public override bool IsDefaultAttribute()
        {
            return (this._factoryTypeName == null);
        }

        public string FactoryTypeName
        {
            get
            {
                return this._factoryTypeName;
            }
        }
    }
}

