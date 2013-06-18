namespace System.Web.Services.Description
{
    using System;
    using System.Collections.Specialized;
    using System.Web.Services;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;

    internal class SchemaCompiler
    {
        private static StringCollection warnings;

        private static void AddImport(XmlSchema schema, string ns)
        {
            if (schema.TargetNamespace != ns)
            {
                foreach (XmlSchemaExternal external in schema.Includes)
                {
                    XmlSchemaImport import = external as XmlSchemaImport;
                    if ((import != null) && (import.Namespace == ns))
                    {
                        return;
                    }
                }
                XmlSchemaImport item = new XmlSchemaImport {
                    Namespace = ns
                };
                schema.Includes.Add(item);
            }
        }

        private static void AddImports(XmlSchemas schemas)
        {
            foreach (XmlSchema schema in schemas)
            {
                AddImport(schema, "http://schemas.xmlsoap.org/soap/encoding/");
                AddImport(schema, "http://schemas.xmlsoap.org/wsdl/");
            }
        }

        internal static StringCollection Compile(XmlSchemas schemas)
        {
            AddImports(schemas);
            Warnings.Clear();
            schemas.Compile(new ValidationEventHandler(SchemaCompiler.ValidationCallbackWithErrorCode), true);
            return Warnings;
        }

        internal static XmlQualifiedName GetParentName(XmlSchemaObject item)
        {
            while (item.Parent != null)
            {
                if (item.Parent is XmlSchemaType)
                {
                    XmlSchemaType parent = (XmlSchemaType) item.Parent;
                    if ((parent.Name != null) && (parent.Name.Length != 0))
                    {
                        return parent.QualifiedName;
                    }
                }
                item = item.Parent;
            }
            return XmlQualifiedName.Empty;
        }

        private static string GetSchemaItem(XmlSchemaObject o, string ns, string details)
        {
            if (o != null)
            {
                while ((o.Parent != null) && !(o.Parent is XmlSchema))
                {
                    o = o.Parent;
                }
                if ((ns == null) || (ns.Length == 0))
                {
                    XmlSchemaObject parent = o;
                    while (parent.Parent != null)
                    {
                        parent = parent.Parent;
                    }
                    if (parent is XmlSchema)
                    {
                        ns = ((XmlSchema) parent).TargetNamespace;
                    }
                }
                if (o is XmlSchemaNotation)
                {
                    return System.Web.Services.Res.GetString("XmlSchemaNamedItem", new object[] { ns, "notation", ((XmlSchemaNotation) o).Name, details });
                }
                if (o is XmlSchemaGroup)
                {
                    return System.Web.Services.Res.GetString("XmlSchemaNamedItem", new object[] { ns, "group", ((XmlSchemaGroup) o).Name, details });
                }
                if (o is XmlSchemaElement)
                {
                    XmlSchemaElement element = (XmlSchemaElement) o;
                    if ((element.Name == null) || (element.Name.Length == 0))
                    {
                        XmlQualifiedName parentName = GetParentName(o);
                        return System.Web.Services.Res.GetString("XmlSchemaElementReference", new object[] { element.RefName.ToString(), parentName.Name, parentName.Namespace });
                    }
                    return System.Web.Services.Res.GetString("XmlSchemaNamedItem", new object[] { ns, "element", element.Name, details });
                }
                if (o is XmlSchemaType)
                {
                    return System.Web.Services.Res.GetString("XmlSchemaNamedItem", new object[] { ns, (o.GetType() == typeof(XmlSchemaSimpleType)) ? "simpleType" : "complexType", ((XmlSchemaType) o).Name, details });
                }
                if (o is XmlSchemaAttributeGroup)
                {
                    return System.Web.Services.Res.GetString("XmlSchemaNamedItem", new object[] { ns, "attributeGroup", ((XmlSchemaAttributeGroup) o).Name, details });
                }
                if (o is XmlSchemaAttribute)
                {
                    XmlSchemaAttribute attribute = (XmlSchemaAttribute) o;
                    if ((attribute.Name == null) || (attribute.Name.Length == 0))
                    {
                        XmlQualifiedName name2 = GetParentName(o);
                        return System.Web.Services.Res.GetString("XmlSchemaAttributeReference", new object[] { attribute.RefName.ToString(), name2.Name, name2.Namespace });
                    }
                    return System.Web.Services.Res.GetString("XmlSchemaNamedItem", new object[] { ns, "attribute", attribute.Name, details });
                }
                if (o is XmlSchemaContent)
                {
                    XmlQualifiedName name3 = GetParentName(o);
                    return System.Web.Services.Res.GetString("XmlSchemaContentDef", new object[] { name3.Name, name3.Namespace, details });
                }
                if (o is XmlSchemaExternal)
                {
                    string str2 = (o is XmlSchemaImport) ? "import" : ((o is XmlSchemaInclude) ? "include" : ((o is XmlSchemaRedefine) ? "redefine" : o.GetType().Name));
                    return System.Web.Services.Res.GetString("XmlSchemaItem", new object[] { ns, str2, details });
                }
                if (o is XmlSchema)
                {
                    return System.Web.Services.Res.GetString("XmlSchema", new object[] { ns, details });
                }
                object[] args = new object[4];
                args[0] = ns;
                args[1] = o.GetType().Name;
                args[3] = details;
                return System.Web.Services.Res.GetString("XmlSchemaNamedItem", args);
            }
            return null;
        }

        private static void ValidationCallbackWithErrorCode(object sender, ValidationEventArgs args)
        {
            Warnings.Add(System.Web.Services.Res.GetString((args.Severity == XmlSeverityType.Error) ? "SchemaValidationError" : "SchemaValidationWarning", new object[] { WarningDetails(args.Exception, args.Message) }));
        }

        internal static string WarningDetails(XmlSchemaException exception, string message)
        {
            XmlSchemaObject sourceSchemaObject = exception.SourceSchemaObject;
            if ((exception.LineNumber == 0) && (exception.LinePosition == 0))
            {
                return GetSchemaItem(sourceSchemaObject, null, message);
            }
            string targetNamespace = null;
            if (sourceSchemaObject != null)
            {
                while (sourceSchemaObject.Parent != null)
                {
                    sourceSchemaObject = sourceSchemaObject.Parent;
                }
                if (sourceSchemaObject is XmlSchema)
                {
                    targetNamespace = ((XmlSchema) sourceSchemaObject).TargetNamespace;
                }
            }
            return System.Web.Services.Res.GetString("SchemaSyntaxErrorDetails", new object[] { targetNamespace, message, exception.LineNumber, exception.LinePosition });
        }

        internal static StringCollection Warnings
        {
            get
            {
                if (warnings == null)
                {
                    warnings = new StringCollection();
                }
                return warnings;
            }
        }
    }
}

