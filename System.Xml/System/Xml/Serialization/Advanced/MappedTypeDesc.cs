namespace System.Xml.Serialization.Advanced
{
    using System;
    using System.CodeDom;
    using System.Collections.Specialized;
    using System.Xml;
    using System.Xml.Schema;

    internal class MappedTypeDesc
    {
        private string clrType;
        private CodeNamespace code;
        private XmlSchemaObject context;
        private bool exported;
        private SchemaImporterExtension extension;
        private string name;
        private string ns;
        private StringCollection references;
        private XmlSchemaType xsdType;

        internal MappedTypeDesc(string clrType, string name, string ns, XmlSchemaType xsdType, XmlSchemaObject context, SchemaImporterExtension extension, CodeNamespace code, StringCollection references)
        {
            this.clrType = clrType.Replace('+', '.');
            this.name = name;
            this.ns = ns;
            this.xsdType = xsdType;
            this.context = context;
            this.code = code;
            this.references = references;
            this.extension = extension;
        }

        internal CodeTypeDeclaration ExportTypeDefinition(CodeNamespace codeNamespace, CodeCompileUnit codeCompileUnit)
        {
            if (this.exported)
            {
                return null;
            }
            this.exported = true;
            foreach (CodeNamespaceImport import in this.code.Imports)
            {
                codeNamespace.Imports.Add(import);
            }
            CodeTypeDeclaration declaration = null;
            string text = Res.GetString("XmlExtensionComment", new object[] { this.extension.GetType().FullName });
            foreach (CodeTypeDeclaration declaration2 in this.code.Types)
            {
                if (this.clrType == declaration2.Name)
                {
                    if (declaration != null)
                    {
                        throw new InvalidOperationException(Res.GetString("XmlExtensionDuplicateDefinition", new object[] { this.extension.GetType().FullName, this.clrType }));
                    }
                    declaration = declaration2;
                }
                declaration2.Comments.Add(new CodeCommentStatement(text, false));
                codeNamespace.Types.Add(declaration2);
            }
            if (codeCompileUnit != null)
            {
                foreach (string str2 in this.ReferencedAssemblies)
                {
                    if (!codeCompileUnit.ReferencedAssemblies.Contains(str2))
                    {
                        codeCompileUnit.ReferencedAssemblies.Add(str2);
                    }
                }
            }
            return declaration;
        }

        internal SchemaImporterExtension Extension
        {
            get
            {
                return this.extension;
            }
        }

        internal string Name
        {
            get
            {
                return this.clrType;
            }
        }

        internal StringCollection ReferencedAssemblies
        {
            get
            {
                if (this.references == null)
                {
                    this.references = new StringCollection();
                }
                return this.references;
            }
        }
    }
}

