namespace System.Web.Services.Description
{
    using System;
    using System.CodeDom;
    using System.Data;
    using System.Data.Design;
    using System.Web.Services;
    using System.Web.Services.Protocols;
    using System.Xml;
    using System.Xml.Serialization;

    internal class MimeXmlImporter : MimeImporter
    {
        private XmlCodeExporter exporter;
        private XmlSchemaImporter importer;

        internal override void AddClassMetadata(CodeTypeDeclaration codeClass)
        {
            foreach (CodeAttributeDeclaration declaration in this.Exporter.IncludeMetadata)
            {
                codeClass.CustomAttributes.Add(declaration);
            }
        }

        private void GenerateCode(MimeXmlReturn importedReturn)
        {
            this.Exporter.ExportTypeMapping(importedReturn.TypeMapping);
        }

        internal override void GenerateCode(MimeReturn[] importedReturns, MimeParameterCollection[] importedParameters)
        {
            for (int i = 0; i < importedReturns.Length; i++)
            {
                if (importedReturns[i] is MimeXmlReturn)
                {
                    this.GenerateCode((MimeXmlReturn) importedReturns[i]);
                }
            }
        }

        internal override MimeParameterCollection ImportParameters()
        {
            return null;
        }

        internal override MimeReturn ImportReturn()
        {
            MessagePart part;
            MimeContentBinding binding = (MimeContentBinding) base.ImportContext.OperationBinding.Output.Extensions.Find(typeof(MimeContentBinding));
            if (binding != null)
            {
                if (!ContentType.MatchesBase(binding.Type, "text/xml"))
                {
                    return null;
                }
                return new MimeReturn { TypeName = typeof(XmlElement).FullName, ReaderType = typeof(XmlReturnReader) };
            }
            MimeXmlBinding binding2 = (MimeXmlBinding) base.ImportContext.OperationBinding.Output.Extensions.Find(typeof(MimeXmlBinding));
            if (binding2 == null)
            {
                return null;
            }
            MimeXmlReturn return3 = new MimeXmlReturn();
            switch (base.ImportContext.OutputMessage.Parts.Count)
            {
                case 0:
                    throw new InvalidOperationException(System.Web.Services.Res.GetString("MessageHasNoParts1", new object[] { base.ImportContext.InputMessage.Name }));

                case 1:
                    if ((binding2.Part != null) && (binding2.Part.Length != 0))
                    {
                        part = base.ImportContext.OutputMessage.FindPartByName(binding2.Part);
                        break;
                    }
                    part = base.ImportContext.OutputMessage.Parts[0];
                    break;

                default:
                    part = base.ImportContext.OutputMessage.FindPartByName(binding2.Part);
                    break;
            }
            return3.TypeMapping = this.Importer.ImportTypeMapping(part.Element);
            return3.TypeName = return3.TypeMapping.TypeFullName;
            return3.ReaderType = typeof(XmlReturnReader);
            this.Exporter.AddMappingMetadata(return3.Attributes, return3.TypeMapping, string.Empty);
            return return3;
        }

        private XmlCodeExporter Exporter
        {
            get
            {
                if (this.exporter == null)
                {
                    this.exporter = new XmlCodeExporter(base.ImportContext.CodeNamespace, base.ImportContext.ServiceImporter.CodeCompileUnit, base.ImportContext.ServiceImporter.CodeGenerator, base.ImportContext.ServiceImporter.CodeGenerationOptions, base.ImportContext.ExportContext);
                }
                return this.exporter;
            }
        }

        private XmlSchemaImporter Importer
        {
            get
            {
                if (this.importer == null)
                {
                    this.importer = new XmlSchemaImporter(base.ImportContext.ConcreteSchemas, base.ImportContext.ServiceImporter.CodeGenerationOptions, base.ImportContext.ServiceImporter.CodeGenerator, base.ImportContext.ImportContext);
                    foreach (Type type in base.ImportContext.ServiceImporter.Extensions)
                    {
                        this.importer.Extensions.Add(type.FullName, type);
                    }
                    this.importer.Extensions.Add(new TypedDataSetSchemaImporterExtension());
                    this.importer.Extensions.Add(new DataSetSchemaImporterExtension());
                }
                return this.importer;
            }
        }
    }
}

