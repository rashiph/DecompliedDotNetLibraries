namespace System.Runtime.Serialization
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Xml;
    using System.Xml.Schema;

    internal static class SchemaHelper
    {
        internal static void AddElementForm(XmlSchemaElement element, XmlSchema schema)
        {
            if (schema.ElementFormDefault != XmlSchemaForm.Qualified)
            {
                element.Form = XmlSchemaForm.Qualified;
            }
        }

        internal static void AddSchemaImport(string ns, XmlSchema schema)
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

        private static XmlSchema CreateSchema(string ns, XmlSchemaSet schemas)
        {
            XmlSchema schema = new XmlSchema {
                ElementFormDefault = XmlSchemaForm.Qualified
            };
            if (ns.Length > 0)
            {
                schema.TargetNamespace = ns;
                schema.Namespaces.Add("tns", ns);
            }
            schemas.Add(schema);
            return schema;
        }

        internal static XmlQualifiedName GetGlobalElementDeclaration(XmlSchemaSet schemas, XmlQualifiedName typeQName, out bool isNullable)
        {
            ICollection is2 = schemas.Schemas();
            if (typeQName.Namespace == null)
            {
            }
            isNullable = false;
            foreach (XmlSchema schema in is2)
            {
                foreach (XmlSchemaElement element in schema.Items)
                {
                    if ((element != null) && element.SchemaTypeName.Equals(typeQName))
                    {
                        isNullable = element.IsNillable;
                        return new XmlQualifiedName(element.Name, schema.TargetNamespace);
                    }
                }
            }
            return null;
        }

        internal static XmlSchema GetSchema(string ns, XmlSchemaSet schemas)
        {
            if (ns == null)
            {
                ns = string.Empty;
            }
            foreach (XmlSchema schema in schemas.Schemas())
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
            return CreateSchema(ns, schemas);
        }

        internal static XmlSchemaElement GetSchemaElement(Dictionary<XmlQualifiedName, SchemaObjectInfo> schemaInfo, XmlQualifiedName elementName)
        {
            SchemaObjectInfo info;
            if (schemaInfo.TryGetValue(elementName, out info))
            {
                return info.element;
            }
            return null;
        }

        internal static XmlSchemaElement GetSchemaElement(XmlSchemaSet schemas, XmlQualifiedName elementQName, out XmlSchema outSchema)
        {
            outSchema = null;
            ICollection is2 = schemas.Schemas();
            string str = elementQName.Namespace;
            foreach (XmlSchema schema in is2)
            {
                if (NamespacesEqual(str, schema.TargetNamespace))
                {
                    outSchema = schema;
                    foreach (XmlSchemaElement element in schema.Items)
                    {
                        if ((element != null) && (element.Name == elementQName.Name))
                        {
                            return element;
                        }
                    }
                }
            }
            return null;
        }

        internal static XmlSchemaType GetSchemaType(Dictionary<XmlQualifiedName, SchemaObjectInfo> schemaInfo, XmlQualifiedName typeName)
        {
            SchemaObjectInfo info;
            if (schemaInfo.TryGetValue(typeName, out info))
            {
                return info.type;
            }
            return null;
        }

        internal static XmlSchemaType GetSchemaType(XmlSchemaSet schemas, XmlQualifiedName typeQName, out XmlSchema outSchema)
        {
            outSchema = null;
            ICollection is2 = schemas.Schemas();
            string str = typeQName.Namespace;
            foreach (XmlSchema schema in is2)
            {
                if (NamespacesEqual(str, schema.TargetNamespace))
                {
                    outSchema = schema;
                    foreach (XmlSchemaType type in schema.Items)
                    {
                        if ((type != null) && (type.Name == typeQName.Name))
                        {
                            return type;
                        }
                    }
                }
            }
            return null;
        }

        internal static XmlSchema GetSchemaWithGlobalElementDeclaration(XmlSchemaElement element, XmlSchemaSet schemas)
        {
            foreach (XmlSchema schema in schemas.Schemas())
            {
                foreach (XmlSchemaElement element2 in schema.Items)
                {
                    if ((element2 != null) && (element2 == element))
                    {
                        return schema;
                    }
                }
            }
            return null;
        }

        internal static XmlSchema GetSchemaWithType(Dictionary<XmlQualifiedName, SchemaObjectInfo> schemaInfo, XmlSchemaSet schemas, XmlQualifiedName typeName)
        {
            SchemaObjectInfo info;
            if (schemaInfo.TryGetValue(typeName, out info) && (info.schema != null))
            {
                return info.schema;
            }
            ICollection is2 = schemas.Schemas();
            string str = typeName.Namespace;
            foreach (XmlSchema schema in is2)
            {
                if (NamespacesEqual(str, schema.TargetNamespace))
                {
                    return schema;
                }
            }
            return null;
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

