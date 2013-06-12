namespace System.Data.Common
{
    using System;

    internal class DbProviderFactoryConfigSection
    {
        private string assemblyQualifiedName;
        private string description;
        private Type factType;
        private string invariantName;
        private string name;

        public DbProviderFactoryConfigSection(Type FactoryType, string FactoryName, string FactoryDescription)
        {
            try
            {
                this.factType = FactoryType;
                this.name = FactoryName;
                this.invariantName = this.factType.Namespace.ToString();
                this.description = FactoryDescription;
                this.assemblyQualifiedName = this.factType.AssemblyQualifiedName.ToString();
            }
            catch
            {
                this.factType = null;
                this.name = string.Empty;
                this.invariantName = string.Empty;
                this.description = string.Empty;
                this.assemblyQualifiedName = string.Empty;
            }
        }

        public DbProviderFactoryConfigSection(string FactoryName, string FactoryInvariantName, string FactoryDescription, string FactoryAssemblyQualifiedName)
        {
            this.factType = null;
            this.name = FactoryName;
            this.invariantName = FactoryInvariantName;
            this.description = FactoryDescription;
            this.assemblyQualifiedName = FactoryAssemblyQualifiedName;
        }

        public bool IsNull()
        {
            return ((this.factType == null) && (this.invariantName == string.Empty));
        }

        public string AssemblyQualifiedName
        {
            get
            {
                return this.assemblyQualifiedName;
            }
        }

        public string Description
        {
            get
            {
                return this.description;
            }
        }

        public string InvariantName
        {
            get
            {
                return this.invariantName;
            }
        }

        public string Name
        {
            get
            {
                return this.name;
            }
        }
    }
}

