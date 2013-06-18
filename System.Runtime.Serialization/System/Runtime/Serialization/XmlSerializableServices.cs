namespace System.Runtime.Serialization
{
    using System;
    using System.Collections.Generic;
    using System.Xml;
    using System.Xml.Schema;

    public static class XmlSerializableServices
    {
        internal static string AddDefaultSchemaMethodName = "AddDefaultSchema";
        internal static readonly string ReadNodesMethodName = "ReadNodes";
        internal static string WriteNodesMethodName = "WriteNodes";

        public static void AddDefaultSchema(XmlSchemaSet schemas, XmlQualifiedName typeQName)
        {
            if (schemas == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("schemas");
            }
            if (typeQName == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("typeQName");
            }
            SchemaExporter.AddDefaultXmlType(schemas, typeQName.Name, typeQName.Namespace);
        }

        private static bool IsValidAttribute(XmlReader xmlReader)
        {
            return ((((xmlReader.NamespaceURI != "http://schemas.microsoft.com/2003/10/Serialization/") && (xmlReader.NamespaceURI != "http://www.w3.org/2001/XMLSchema-instance")) && (xmlReader.Prefix != "xmlns")) && (xmlReader.LocalName != "xmlns"));
        }

        public static System.Xml.XmlNode[] ReadNodes(XmlReader xmlReader)
        {
            if (xmlReader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("xmlReader");
            }
            XmlDocument document = new XmlDocument();
            List<System.Xml.XmlNode> list = new List<System.Xml.XmlNode>();
            if (xmlReader.MoveToFirstAttribute())
            {
                do
                {
                    if (IsValidAttribute(xmlReader))
                    {
                        System.Xml.XmlNode item = document.ReadNode(xmlReader);
                        if (item == null)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(System.Runtime.Serialization.SR.GetString("UnexpectedEndOfFile")));
                        }
                        list.Add(item);
                    }
                }
                while (xmlReader.MoveToNextAttribute());
            }
            xmlReader.MoveToElement();
            if (!xmlReader.IsEmptyElement)
            {
                int depth = xmlReader.Depth;
                xmlReader.Read();
                while ((xmlReader.Depth > depth) && (xmlReader.NodeType != XmlNodeType.EndElement))
                {
                    System.Xml.XmlNode node2 = document.ReadNode(xmlReader);
                    if (node2 == null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(System.Runtime.Serialization.SR.GetString("UnexpectedEndOfFile")));
                    }
                    list.Add(node2);
                }
            }
            return list.ToArray();
        }

        public static void WriteNodes(XmlWriter xmlWriter, System.Xml.XmlNode[] nodes)
        {
            if (xmlWriter == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("xmlWriter");
            }
            if (nodes != null)
            {
                for (int i = 0; i < nodes.Length; i++)
                {
                    if (nodes[i] != null)
                    {
                        nodes[i].WriteTo(xmlWriter);
                    }
                }
            }
        }
    }
}

