namespace System.Xml.Serialization.Advanced
{
    using System;
    using System.CodeDom;
    using System.CodeDom.Compiler;
    using System.Xml.Schema;
    using System.Xml.Serialization;

    public abstract class SchemaImporterExtension
    {
        protected SchemaImporterExtension()
        {
        }

        public virtual string ImportAnyElement(XmlSchemaAny any, bool mixed, XmlSchemas schemas, XmlSchemaImporter importer, CodeCompileUnit compileUnit, CodeNamespace mainNamespace, CodeGenerationOptions options, CodeDomProvider codeProvider)
        {
            return null;
        }

        public virtual CodeExpression ImportDefaultValue(string value, string type)
        {
            return null;
        }

        public virtual string ImportSchemaType(XmlSchemaType type, XmlSchemaObject context, XmlSchemas schemas, XmlSchemaImporter importer, CodeCompileUnit compileUnit, CodeNamespace mainNamespace, CodeGenerationOptions options, CodeDomProvider codeProvider)
        {
            return null;
        }

        public virtual string ImportSchemaType(string name, string ns, XmlSchemaObject context, XmlSchemas schemas, XmlSchemaImporter importer, CodeCompileUnit compileUnit, CodeNamespace mainNamespace, CodeGenerationOptions options, CodeDomProvider codeProvider)
        {
            return null;
        }
    }
}

