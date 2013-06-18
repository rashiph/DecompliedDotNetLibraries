namespace Microsoft.Compiler.VisualBasic
{
    using System;

    internal sealed class Import
    {
        private string m_alias;
        private string m_importedEntity;

        public Import(string importedEntity)
        {
            if (string.IsNullOrEmpty(importedEntity))
            {
                throw new ArgumentNullException("importedEntity");
            }
            this.m_alias = string.Empty;
            this.m_importedEntity = importedEntity;
        }

        public Import(string alias, string importedEntity)
        {
            if (string.IsNullOrEmpty(alias))
            {
                throw new ArgumentNullException("alias");
            }
            if (string.IsNullOrEmpty(importedEntity))
            {
                throw new ArgumentNullException("importedEntity");
            }
            this.m_alias = alias;
            this.m_importedEntity = importedEntity;
        }

        public string Alias
        {
            get
            {
                return this.m_alias;
            }
        }

        public string ImportedEntity
        {
            get
            {
                return this.m_importedEntity;
            }
        }
    }
}

