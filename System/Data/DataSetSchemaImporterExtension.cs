namespace System.Data
{
    using System;
    using System.CodeDom;
    using System.CodeDom.Compiler;
    using System.Collections;
    using System.Collections.Specialized;
    using System.IO;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;
    using System.Xml.Serialization.Advanced;

    public class DataSetSchemaImporterExtension : SchemaImporterExtension
    {
        private Hashtable importedTypes = new Hashtable();

        internal XmlSchemaElement FindDataSetElement(XmlSchema schema)
        {
            foreach (XmlSchemaObject obj2 in schema.Items)
            {
                if ((obj2 is XmlSchemaElement) && IsDataSet((XmlSchemaElement) obj2))
                {
                    return (XmlSchemaElement) obj2;
                }
            }
            return null;
        }

        internal string GenerateTypedDataSet(XmlSchemaElement element, XmlSchemas schemas, CodeNamespace codeNamespace, StringCollection references, CodeDomProvider codeProvider)
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
            DataSet dataSet = new DataSet();
            using (MemoryStream stream = new MemoryStream())
            {
                schema.Write(stream);
                stream.Position = 0L;
                dataSet.ReadXmlSchema(stream);
            }
            string name = new TypedDataSetGenerator().GenerateCode(dataSet, codeNamespace, codeProvider.CreateGenerator()).Name;
            this.importedTypes.Add(element.SchemaType, name);
            references.Add("System.Data.dll");
            return name;
        }

        public override string ImportSchemaType(XmlSchemaType type, XmlSchemaObject context, XmlSchemas schemas, XmlSchemaImporter importer, CodeCompileUnit compileUnit, CodeNamespace mainNamespace, CodeGenerationOptions options, CodeDomProvider codeProvider)
        {
            if (type != null)
            {
                if (this.importedTypes[type] != null)
                {
                    mainNamespace.Imports.Add(new CodeNamespaceImport(typeof(DataSet).Namespace));
                    compileUnit.ReferencedAssemblies.Add("System.Data.dll");
                    return (string) this.importedTypes[type];
                }
                if (!(context is XmlSchemaElement))
                {
                    return null;
                }
                XmlSchemaElement element1 = (XmlSchemaElement) context;
                if (type is XmlSchemaComplexType)
                {
                    XmlSchemaComplexType type2 = (XmlSchemaComplexType) type;
                    if (type2.Particle is XmlSchemaSequence)
                    {
                        XmlSchemaObjectCollection items = ((XmlSchemaSequence) type2.Particle).Items;
                        if (((2 == items.Count) && (items[0] is XmlSchemaAny)) && (items[1] is XmlSchemaAny))
                        {
                            XmlSchemaAny any2 = (XmlSchemaAny) items[0];
                            XmlSchemaAny any3 = (XmlSchemaAny) items[1];
                            if ((any2.Namespace == "http://www.w3.org/2001/XMLSchema") && (any3.Namespace == "urn:schemas-microsoft-com:xml-diffgram-v1"))
                            {
                                string ns = null;
                                foreach (XmlSchemaAttribute attribute in type2.Attributes)
                                {
                                    if (attribute.Name == "namespace")
                                    {
                                        ns = attribute.FixedValue.Trim();
                                        break;
                                    }
                                }
                                bool flag = false;
                                if (((XmlSchemaSequence) type2.Particle).MaxOccurs == 79228162514264337593543950335M)
                                {
                                    flag = true;
                                }
                                else if (any2.MaxOccurs == 79228162514264337593543950335M)
                                {
                                    flag = false;
                                }
                                else
                                {
                                    return null;
                                }
                                if (ns == null)
                                {
                                    string str4 = flag ? typeof(DataSet).FullName : typeof(DataTable).FullName;
                                    this.importedTypes.Add(type, str4);
                                    mainNamespace.Imports.Add(new CodeNamespaceImport(typeof(DataSet).Namespace));
                                    compileUnit.ReferencedAssemblies.Add("System.Data.dll");
                                    return str4;
                                }
                                foreach (XmlSchema schema2 in schemas.GetSchemas(ns))
                                {
                                    if ((schema2 != null) && (schema2.Id != null))
                                    {
                                        XmlSchemaElement element2 = this.FindDataSetElement(schema2);
                                        if (element2 != null)
                                        {
                                            return this.ImportSchemaType(element2.SchemaType, element2, schemas, importer, compileUnit, mainNamespace, options, codeProvider);
                                        }
                                    }
                                }
                                return null;
                            }
                        }
                    }
                    if ((type2.Particle is XmlSchemaSequence) || (type2.Particle is XmlSchemaAll))
                    {
                        XmlSchemaObjectCollection objects = ((XmlSchemaGroupBase) type2.Particle).Items;
                        if (objects.Count == 2)
                        {
                            if (!(objects[0] is XmlSchemaElement) || !(objects[1] is XmlSchemaAny))
                            {
                                return null;
                            }
                            XmlSchemaElement element3 = (XmlSchemaElement) objects[0];
                            if ((element3.RefName.Name != "schema") || (element3.RefName.Namespace != "http://www.w3.org/2001/XMLSchema"))
                            {
                                return null;
                            }
                            string fullName = typeof(DataSet).FullName;
                            this.importedTypes.Add(type, fullName);
                            mainNamespace.Imports.Add(new CodeNamespaceImport(typeof(DataSet).Namespace));
                            compileUnit.ReferencedAssemblies.Add("System.Data.dll");
                            return fullName;
                        }
                        if (1 == objects.Count)
                        {
                            XmlSchemaAny any = objects[0] as XmlSchemaAny;
                            if (((any != null) && (any.Namespace != null)) && (any.Namespace.IndexOfAny(new char[] { '#', ' ' }) < 0))
                            {
                                foreach (XmlSchema schema in schemas.GetSchemas(any.Namespace))
                                {
                                    if ((schema != null) && (schema.Id != null))
                                    {
                                        XmlSchemaElement element = this.FindDataSetElement(schema);
                                        if (element != null)
                                        {
                                            return this.ImportSchemaType(element.SchemaType, element, schemas, importer, compileUnit, mainNamespace, options, codeProvider);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return null;
        }

        public override string ImportSchemaType(string name, string schemaNamespace, XmlSchemaObject context, XmlSchemas schemas, XmlSchemaImporter importer, CodeCompileUnit compileUnit, CodeNamespace mainNamespace, CodeGenerationOptions options, CodeDomProvider codeProvider)
        {
            IList list = schemas.GetSchemas(schemaNamespace);
            if (list.Count != 1)
            {
                return null;
            }
            XmlSchema schema = list[0] as XmlSchema;
            if (schema == null)
            {
                return null;
            }
            XmlSchemaType type = (XmlSchemaType) schema.SchemaTypes[new XmlQualifiedName(name, schemaNamespace)];
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

