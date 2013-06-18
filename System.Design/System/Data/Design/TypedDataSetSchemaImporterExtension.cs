namespace System.Data.Design
{
    using System;
    using System.CodeDom;
    using System.CodeDom.Compiler;
    using System.Collections;
    using System.IO;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;
    using System.Xml.Serialization.Advanced;

    public class TypedDataSetSchemaImporterExtension : SchemaImporterExtension
    {
        private TypedDataSetGenerator.GenerateOption dataSetGenerateOptions;
        private Hashtable importedTypes;

        public TypedDataSetSchemaImporterExtension() : this(TypedDataSetGenerator.GenerateOption.None)
        {
        }

        protected TypedDataSetSchemaImporterExtension(TypedDataSetGenerator.GenerateOption dataSetGenerateOptions)
        {
            this.importedTypes = new Hashtable();
            this.dataSetGenerateOptions = dataSetGenerateOptions;
        }

        internal XmlSchemaElement FindDataSetElement(XmlSchema schema, XmlSchemas schemas)
        {
            foreach (XmlSchemaObject obj2 in schema.Items)
            {
                if ((obj2 is XmlSchemaElement) && IsDataSet((XmlSchemaElement) obj2))
                {
                    XmlSchemaElement element = (XmlSchemaElement) obj2;
                    return (XmlSchemaElement) schemas.Find(element.QualifiedName, typeof(XmlSchemaElement));
                }
            }
            return null;
        }

        internal string GenerateTypedDataSet(XmlSchemaElement element, XmlSchemas schemas, CodeCompileUnit compileUnit, CodeNamespace mainNamespace, CodeDomProvider codeProvider)
        {
            if (element == null)
            {
                return null;
            }
            if (this.importedTypes[element.SchemaType] != null)
            {
                return (string) this.importedTypes[element.SchemaType];
            }
            IList list = schemas.GetSchemas(element.QualifiedName.Namespace);
            if (list.Count != 1)
            {
                return null;
            }
            XmlSchema schema = list[0] as XmlSchema;
            if (schema == null)
            {
                return null;
            }
            MemoryStream stream = new MemoryStream();
            schema.Write(stream);
            stream.Position = 0L;
            DesignDataSource designDS = new DesignDataSource();
            designDS.ReadXmlSchema(stream, null);
            stream.Close();
            string str = TypedDataSetGenerator.GenerateInternal(designDS, compileUnit, mainNamespace, codeProvider, this.dataSetGenerateOptions, null);
            this.importedTypes.Add(element.SchemaType, str);
            return str;
        }

        public override string ImportSchemaType(XmlSchemaType type, XmlSchemaObject context, XmlSchemas schemas, XmlSchemaImporter importer, CodeCompileUnit compileUnit, CodeNamespace mainNamespace, CodeGenerationOptions options, CodeDomProvider codeProvider)
        {
            if (type != null)
            {
                if (!(context is XmlSchemaElement))
                {
                    return null;
                }
                XmlSchemaElement e = (XmlSchemaElement) context;
                if (IsDataSet(e))
                {
                    if (this.importedTypes[type] != null)
                    {
                        return (string) this.importedTypes[type];
                    }
                    return this.GenerateTypedDataSet(e, schemas, compileUnit, mainNamespace, codeProvider);
                }
                if (type is XmlSchemaComplexType)
                {
                    XmlSchemaComplexType type2 = (XmlSchemaComplexType) type;
                    if (type2.Particle is XmlSchemaSequence)
                    {
                        XmlSchemaObjectCollection items = ((XmlSchemaSequence) type2.Particle).Items;
                        if (((items.Count == 2) && (items[0] is XmlSchemaAny)) && (items[1] is XmlSchemaAny))
                        {
                            XmlSchemaAny any = (XmlSchemaAny) items[0];
                            XmlSchemaAny any2 = (XmlSchemaAny) items[1];
                            if ((any.Namespace == "http://www.w3.org/2001/XMLSchema") && (any2.Namespace == "urn:schemas-microsoft-com:xml-diffgram-v1"))
                            {
                                string ns = null;
                                string str2 = null;
                                foreach (XmlSchemaAttribute attribute in type2.Attributes)
                                {
                                    if (attribute.Name == "namespace")
                                    {
                                        ns = attribute.FixedValue.Trim();
                                    }
                                    else if (attribute.Name == "tableTypeName")
                                    {
                                        str2 = attribute.FixedValue.Trim();
                                    }
                                    if ((ns != null) && (str2 != null))
                                    {
                                        break;
                                    }
                                }
                                if (ns == null)
                                {
                                    return null;
                                }
                                IList list = schemas.GetSchemas(ns);
                                if (list.Count != 1)
                                {
                                    return null;
                                }
                                XmlSchema schema = list[0] as XmlSchema;
                                if ((schema == null) || (schema.Id == null))
                                {
                                    return null;
                                }
                                XmlSchemaElement element2 = this.FindDataSetElement(schema, schemas);
                                if (element2 == null)
                                {
                                    return null;
                                }
                                string str3 = this.ImportSchemaType(element2.SchemaType, element2, schemas, importer, compileUnit, mainNamespace, options, codeProvider);
                                if (str2 == null)
                                {
                                    return str3;
                                }
                                return CodeGenHelper.GetTypeName(codeProvider, str3, str2);
                            }
                        }
                    }
                    if ((type2.Particle is XmlSchemaSequence) || (type2.Particle is XmlSchemaAll))
                    {
                        XmlSchemaObjectCollection objects2 = ((XmlSchemaGroupBase) type2.Particle).Items;
                        if (objects2.Count == 1)
                        {
                            if (objects2[0] is XmlSchemaAny)
                            {
                                XmlSchemaAny any3 = (XmlSchemaAny) objects2[0];
                                if (any3.Namespace == null)
                                {
                                    return null;
                                }
                                if (any3.Namespace.IndexOf('#') >= 0)
                                {
                                    return null;
                                }
                                if (any3.Namespace.IndexOf(' ') >= 0)
                                {
                                    return null;
                                }
                                IList list2 = schemas.GetSchemas(any3.Namespace);
                                if (list2.Count != 1)
                                {
                                    return null;
                                }
                                XmlSchema schema2 = list2[0] as XmlSchema;
                                if (schema2 == null)
                                {
                                    return null;
                                }
                                if (schema2.Id == null)
                                {
                                    return null;
                                }
                                XmlSchemaElement element3 = this.FindDataSetElement(schema2, schemas);
                                if (element3 != null)
                                {
                                    return this.ImportSchemaType(element3.SchemaType, element3, schemas, importer, compileUnit, mainNamespace, options, codeProvider);
                                }
                            }
                            return null;
                        }
                    }
                }
            }
            return null;
        }

        public override string ImportSchemaType(string name, string namespaceName, XmlSchemaObject context, XmlSchemas schemas, XmlSchemaImporter importer, CodeCompileUnit compileUnit, CodeNamespace mainNamespace, CodeGenerationOptions options, CodeDomProvider codeProvider)
        {
            IList list = schemas.GetSchemas(namespaceName);
            if (list.Count != 1)
            {
                return null;
            }
            XmlSchema schema = list[0] as XmlSchema;
            if (schema == null)
            {
                return null;
            }
            XmlSchemaType type = (XmlSchemaType) schema.SchemaTypes[new XmlQualifiedName(name, namespaceName)];
            return this.ImportSchemaType(type, context, schemas, importer, compileUnit, mainNamespace, options, codeProvider);
        }

        internal static bool IsDataSet(XmlSchemaElement e)
        {
            if (e.UnhandledAttributes != null)
            {
                foreach (XmlAttribute attribute in e.UnhandledAttributes)
                {
                    if (((attribute.LocalName == "IsDataSet") && (attribute.NamespaceURI == "urn:schemas-microsoft-com:xml-msdata")) && (((attribute.Value == "True") || (attribute.Value == "true")) || (attribute.Value == "1")))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}

