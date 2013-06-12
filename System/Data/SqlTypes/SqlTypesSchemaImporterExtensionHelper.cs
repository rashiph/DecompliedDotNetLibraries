namespace System.Data.SqlTypes
{
    using System;
    using System.CodeDom;
    using System.CodeDom.Compiler;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;
    using System.Xml.Serialization.Advanced;

    public class SqlTypesSchemaImporterExtensionHelper : SchemaImporterExtension
    {
        private string m_destinationType;
        private bool m_direct;
        private string m_name;
        private CodeNamespaceImport[] m_namespaceImports;
        private string[] m_references;
        private string m_targetNamespace;
        protected static readonly string SqlTypesNamespace = "http://schemas.microsoft.com/sqlserver/2004/sqltypes";

        public SqlTypesSchemaImporterExtensionHelper(string name, string destinationType)
        {
            this.Init(name, SqlTypesNamespace, null, null, destinationType, true);
        }

        public SqlTypesSchemaImporterExtensionHelper(string name, string destinationType, bool direct)
        {
            this.Init(name, SqlTypesNamespace, null, null, destinationType, direct);
        }

        public SqlTypesSchemaImporterExtensionHelper(string name, string targetNamespace, string[] references, CodeNamespaceImport[] namespaceImports, string destinationType, bool direct)
        {
            this.Init(name, targetNamespace, references, namespaceImports, destinationType, direct);
        }

        public override string ImportSchemaType(XmlSchemaType type, XmlSchemaObject context, XmlSchemas schemas, XmlSchemaImporter importer, CodeCompileUnit compileUnit, CodeNamespace mainNamespace, CodeGenerationOptions options, CodeDomProvider codeProvider)
        {
            if ((!this.m_direct && (type is XmlSchemaSimpleType)) && (context is XmlSchemaElement))
            {
                XmlQualifiedName qualifiedName = ((XmlSchemaSimpleType) type).BaseXmlSchemaType.QualifiedName;
                if ((string.CompareOrdinal(this.m_name, qualifiedName.Name) == 0) && (string.CompareOrdinal(this.m_targetNamespace, qualifiedName.Namespace) == 0))
                {
                    compileUnit.ReferencedAssemblies.AddRange(this.m_references);
                    mainNamespace.Imports.AddRange(this.m_namespaceImports);
                    return this.m_destinationType;
                }
            }
            return null;
        }

        public override string ImportSchemaType(string name, string xmlNamespace, XmlSchemaObject context, XmlSchemas schemas, XmlSchemaImporter importer, CodeCompileUnit compileUnit, CodeNamespace mainNamespace, CodeGenerationOptions options, CodeDomProvider codeProvider)
        {
            if ((this.m_direct && (context is XmlSchemaElement)) && ((string.CompareOrdinal(this.m_name, name) == 0) && (string.CompareOrdinal(this.m_targetNamespace, xmlNamespace) == 0)))
            {
                compileUnit.ReferencedAssemblies.AddRange(this.m_references);
                mainNamespace.Imports.AddRange(this.m_namespaceImports);
                return this.m_destinationType;
            }
            return null;
        }

        private void Init(string name, string targetNamespace, string[] references, CodeNamespaceImport[] namespaceImports, string destinationType, bool direct)
        {
            this.m_name = name;
            this.m_targetNamespace = targetNamespace;
            if (references == null)
            {
                this.m_references = new string[] { "System.Data.dll" };
            }
            else
            {
                this.m_references = references;
            }
            if (namespaceImports == null)
            {
                this.m_namespaceImports = new CodeNamespaceImport[] { new CodeNamespaceImport("System.Data"), new CodeNamespaceImport("System.Data.SqlTypes") };
            }
            else
            {
                this.m_namespaceImports = namespaceImports;
            }
            this.m_destinationType = destinationType;
            this.m_direct = direct;
        }
    }
}

