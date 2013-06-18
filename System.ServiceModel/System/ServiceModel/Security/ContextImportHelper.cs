namespace System.ServiceModel.Security
{
    using System;
    using System.IO;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Xml;

    internal static class ContextImportHelper
    {
        internal static XmlDictionaryReader CreateSplicedReader(byte[] decryptedBuffer, XmlAttributeHolder[] outerContext1, XmlAttributeHolder[] outerContext2, XmlAttributeHolder[] outerContext3, XmlDictionaryReaderQuotas quotas)
        {
            MemoryStream stream = new MemoryStream();
            XmlDictionaryWriter writer = XmlDictionaryWriter.CreateTextWriter(stream);
            writer.WriteStartElement("x");
            WriteNamespaceDeclarations(outerContext1, writer);
            writer.WriteStartElement("y");
            WriteNamespaceDeclarations(outerContext2, writer);
            writer.WriteStartElement("z");
            WriteNamespaceDeclarations(outerContext3, writer);
            writer.WriteString(" ");
            writer.WriteEndElement();
            writer.WriteEndElement();
            writer.WriteEndElement();
            writer.Flush();
            XmlDictionaryReader reader = XmlDictionaryReader.CreateTextReader(SpliceBuffers(decryptedBuffer, stream.GetBuffer(), (int) stream.Length, 3), quotas);
            reader.ReadStartElement("x");
            reader.ReadStartElement("y");
            reader.ReadStartElement("z");
            if (reader.NodeType != XmlNodeType.Element)
            {
                reader.MoveToContent();
            }
            return reader;
        }

        internal static string GetPrefixIfNamespaceDeclaration(string prefix, string localName)
        {
            if (prefix == "xmlns")
            {
                return localName;
            }
            if ((prefix.Length == 0) && (localName == "xmlns"))
            {
                return string.Empty;
            }
            return null;
        }

        private static bool IsNamespaceDeclaration(string prefix, string localName)
        {
            return (GetPrefixIfNamespaceDeclaration(prefix, localName) != null);
        }

        internal static byte[] SpliceBuffers(byte[] middle, byte[] wrapper, int wrapperLength, int wrappingDepth)
        {
            int num = 0;
            int index = wrapperLength - 1;
            while (index >= 0)
            {
                if (wrapper[index] == 60)
                {
                    num++;
                    if (num == wrappingDepth)
                    {
                        break;
                    }
                }
                index--;
            }
            byte[] dst = DiagnosticUtility.Utility.AllocateByteArray((middle.Length + wrapperLength) - 1);
            int dstOffset = 0;
            int count = index - 1;
            Buffer.BlockCopy(wrapper, 0, dst, dstOffset, count);
            dstOffset += count;
            count = middle.Length;
            Buffer.BlockCopy(middle, 0, dst, dstOffset, count);
            dstOffset += count;
            count = wrapperLength - index;
            Buffer.BlockCopy(wrapper, index, dst, dstOffset, count);
            return dst;
        }

        private static void WriteNamespaceDeclarations(XmlAttributeHolder[] attributes, XmlWriter writer)
        {
            if (attributes != null)
            {
                for (int i = 0; i < attributes.Length; i++)
                {
                    XmlAttributeHolder holder = attributes[i];
                    if (IsNamespaceDeclaration(holder.Prefix, holder.LocalName))
                    {
                        holder.WriteTo(writer);
                    }
                }
            }
        }
    }
}

