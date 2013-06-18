namespace System.ServiceModel.Description
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ServiceModel;
    using System.Xml;
    using System.Xml.Schema;

    internal static class SchemaHelper
    {
        private static IList<string> dataContractPrimitives = new string[] { "char", "guid" };
        private static string dataContractSerializerNamespace = "http://schemas.microsoft.com/2003/10/Serialization/";
        private static string xmlSerializerNamespace = "http://microsoft.com/wsdl/types/";
        private static IList<string> xmlSerializerPrimitives = new string[] { "char", "guid" };
        private static IList<string> xsdValueTypePrimitives = new string[] { 
            "boolean", "float", "double", "decimal", "long", "unsignedLong", "int", "unsignedInt", "short", "unsignedShort", "byte", "unsignedByte", "duration", "dateTime", "integer", "positiveInteger", 
            "negativeInteger", "nonPositiveInteger"
         };

        internal static void AddElementForm(XmlSchemaElement element, System.Xml.Schema.XmlSchema schema)
        {
            if (schema.ElementFormDefault != XmlSchemaForm.Qualified)
            {
                element.Form = XmlSchemaForm.Qualified;
            }
        }

        internal static void AddElementToSchema(XmlSchemaElement element, System.Xml.Schema.XmlSchema schema, XmlSchemaSet schemaSet)
        {
            XmlSchemaElement element2 = (XmlSchemaElement) schema.Elements[new XmlQualifiedName(element.Name, schema.TargetNamespace)];
            if (element2 != null)
            {
                if ((element.SchemaType != element2.SchemaType) || (element.SchemaTypeName != element2.SchemaTypeName))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxConflictingGlobalElement", new object[] { element.Name, schema.TargetNamespace, GetTypeName(element), GetTypeName(element2) })));
                }
            }
            else
            {
                schema.Items.Add(element);
                if (!element.SchemaTypeName.IsEmpty)
                {
                    AddImportToSchema(element.SchemaTypeName.Namespace, schema);
                }
                schemaSet.Reprocess(schema);
            }
        }

        internal static void AddImportToSchema(string ns, System.Xml.Schema.XmlSchema schema)
        {
            if ((!NamespacesEqual(ns, schema.TargetNamespace) && !NamespacesEqual(ns, "http://www.w3.org/2001/XMLSchema")) && !NamespacesEqual(ns, "http://www.w3.org/2001/XMLSchema-instance"))
            {
                foreach (object obj2 in schema.Includes)
                {
                    if ((obj2 is XmlSchemaImport) && NamespacesEqual(ns, ((XmlSchemaImport) obj2).Namespace))
                    {
                        return;
                    }
                }
                XmlSchemaImport item = new XmlSchemaImport();
                if ((ns != null) && (ns.Length > 0))
                {
                    item.Namespace = ns;
                }
                schema.Includes.Add(item);
            }
        }

        internal static void AddTypeToSchema(XmlSchemaType type, System.Xml.Schema.XmlSchema schema, XmlSchemaSet schemaSet)
        {
            XmlSchemaType type2 = (XmlSchemaType) schema.SchemaTypes[new XmlQualifiedName(type.Name, schema.TargetNamespace)];
            if (type2 != null)
            {
                if (type2 != type)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxConflictingGlobalType", new object[] { type.Name, schema.TargetNamespace })));
                }
            }
            else
            {
                schema.Items.Add(type);
                schemaSet.Reprocess(schema);
            }
        }

        internal static void Compile(XmlSchemaSet schemaSet, Collection<MetadataConversionError> errors)
        {
            ValidationEventHandler handler = (sender, args) => HandleSchemaValidationError(sender, args, errors);
            schemaSet.ValidationEventHandler += handler;
            schemaSet.Compile();
            schemaSet.ValidationEventHandler -= handler;
        }

        internal static System.Xml.Schema.XmlSchema GetSchema(string ns, XmlSchemaSet schemaSet)
        {
            if (ns == null)
            {
                ns = string.Empty;
            }
            foreach (System.Xml.Schema.XmlSchema schema in schemaSet.Schemas())
            {
                if ((schema.TargetNamespace == null) && (ns.Length == 0))
                {
                    return schema;
                }
                if (ns.Equals(schema.TargetNamespace))
                {
                    return schema;
                }
            }
            System.Xml.Schema.XmlSchema schema2 = new System.Xml.Schema.XmlSchema {
                ElementFormDefault = XmlSchemaForm.Qualified
            };
            if (ns.Length > 0)
            {
                schema2.TargetNamespace = ns;
            }
            schemaSet.Add(schema2);
            return schema2;
        }

        private static string GetTypeName(XmlSchemaElement element)
        {
            if (element.SchemaType != null)
            {
                return "anonymous";
            }
            if (!element.SchemaTypeName.IsEmpty)
            {
                return element.SchemaTypeName.ToString();
            }
            return string.Empty;
        }

        internal static void HandleSchemaValidationError(object sender, ValidationEventArgs args, Collection<MetadataConversionError> errors)
        {
            MetadataConversionError item = null;
            if ((args.Exception != null) && (args.Exception.SourceUri != null))
            {
                XmlSchemaException exception = args.Exception;
                item = new MetadataConversionError(System.ServiceModel.SR.GetString("SchemaValidationError", new object[] { exception.SourceUri, exception.LineNumber, exception.LinePosition, exception.Message }));
            }
            else
            {
                item = new MetadataConversionError(System.ServiceModel.SR.GetString("GeneralSchemaValidationError", new object[] { args.Message }));
            }
            if (!errors.Contains(item))
            {
                errors.Add(item);
            }
        }

        internal static bool IsElementValueType(XmlSchemaElement element)
        {
            XmlQualifiedName schemaTypeName = element.SchemaTypeName;
            if ((schemaTypeName == null) || schemaTypeName.IsEmpty)
            {
                return false;
            }
            if (schemaTypeName.Namespace == "http://www.w3.org/2001/XMLSchema")
            {
                return xsdValueTypePrimitives.Contains(schemaTypeName.Name);
            }
            if (schemaTypeName.Namespace == dataContractSerializerNamespace)
            {
                return dataContractPrimitives.Contains(schemaTypeName.Name);
            }
            return ((schemaTypeName.Namespace == xmlSerializerNamespace) && dataContractPrimitives.Contains(schemaTypeName.Name));
        }

        internal static bool IsMatch(XmlSchemaElement e1, XmlSchemaElement e2)
        {
            if ((e1.SchemaType != null) || (e2.SchemaType != null))
            {
                return false;
            }
            if (e1.SchemaTypeName != e2.SchemaTypeName)
            {
                return false;
            }
            if (e1.Form != e2.Form)
            {
                return false;
            }
            if (e1.IsNillable != e2.IsNillable)
            {
                return false;
            }
            return true;
        }

        internal static bool NamespacesEqual(string ns1, string ns2)
        {
            if ((ns1 != null) && (ns1.Length != 0))
            {
                return (ns1 == ns2);
            }
            if (ns2 != null)
            {
                return (ns2.Length == 0);
            }
            return true;
        }
    }
}

