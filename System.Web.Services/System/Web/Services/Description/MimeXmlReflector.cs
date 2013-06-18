namespace System.Web.Services.Description
{
    using System;
    using System.Web.Services.Protocols;
    using System.Xml;
    using System.Xml.Serialization;

    internal class MimeXmlReflector : MimeReflector
    {
        internal override bool ReflectParameters()
        {
            return false;
        }

        internal override bool ReflectReturn()
        {
            MessagePart messagePart = new MessagePart {
                Name = "Body"
            };
            base.ReflectionContext.OutputMessage.Parts.Add(messagePart);
            if (typeof(XmlNode).IsAssignableFrom(base.ReflectionContext.Method.ReturnType))
            {
                MimeContentBinding extension = new MimeContentBinding {
                    Type = "text/xml",
                    Part = messagePart.Name
                };
                base.ReflectionContext.OperationBinding.Output.Extensions.Add(extension);
            }
            else
            {
                MimeXmlBinding binding2 = new MimeXmlBinding {
                    Part = messagePart.Name
                };
                LogicalMethodInfo method = base.ReflectionContext.Method;
                XmlAttributes attributes = new XmlAttributes(method.ReturnTypeCustomAttributeProvider);
                XmlTypeMapping xmlTypeMapping = base.ReflectionContext.ReflectionImporter.ImportTypeMapping(method.ReturnType, attributes.XmlRoot);
                xmlTypeMapping.SetKey(method.GetKey() + ":Return");
                base.ReflectionContext.SchemaExporter.ExportTypeMapping(xmlTypeMapping);
                messagePart.Element = new XmlQualifiedName(xmlTypeMapping.XsdElementName, xmlTypeMapping.Namespace);
                base.ReflectionContext.OperationBinding.Output.Extensions.Add(binding2);
            }
            return true;
        }
    }
}

