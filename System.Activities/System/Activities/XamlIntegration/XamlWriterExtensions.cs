namespace System.Activities.XamlIntegration
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Xaml;

    internal static class XamlWriterExtensions
    {
        public static void PropagateLineInfo(XamlWriter targetWriter, IXamlLineInfo lineInfo)
        {
            if (lineInfo != null)
            {
                (targetWriter as IXamlLineInfoConsumer).SetLineInfo(lineInfo.LineNumber, lineInfo.LinePosition);
            }
        }

        public static void PropagateLineInfo(XamlWriter targetWriter, int lineNumber, int linePosition)
        {
            (targetWriter as IXamlLineInfoConsumer).SetLineInfo(lineNumber, linePosition);
        }

        public static void Transform(XamlReader reader, XamlWriter writer, IXamlLineInfo readerLineInfo, bool closeWriter)
        {
            IXamlLineInfoConsumer consumer = writer as IXamlLineInfoConsumer;
            bool flag = false;
            if (readerLineInfo != null)
            {
                flag = true;
            }
            while (reader.Read())
            {
                if (flag)
                {
                    consumer.SetLineInfo(readerLineInfo.LineNumber, readerLineInfo.LinePosition);
                }
                writer.WriteNode(reader);
            }
            if (closeWriter)
            {
                writer.Close();
            }
        }

        public static void WriteEndMember(this XamlWriter writer, IXamlLineInfo lineInfo)
        {
            PropagateLineInfo(writer, lineInfo);
            writer.WriteEndMember();
        }

        public static void WriteEndObject(this XamlWriter writer, IXamlLineInfo lineInfo)
        {
            PropagateLineInfo(writer, lineInfo);
            writer.WriteEndObject();
        }

        public static void WriteGetObject(this XamlWriter writer, IXamlLineInfo lineInfo)
        {
            PropagateLineInfo(writer, lineInfo);
            writer.WriteGetObject();
        }

        public static void WriteNamespace(this XamlWriter writer, NamespaceDeclaration namespaceDeclaration, IXamlLineInfo lineInfo)
        {
            PropagateLineInfo(writer, lineInfo);
            writer.WriteNamespace(namespaceDeclaration);
        }

        public static void WriteNode(this XamlWriter writer, XamlReader reader, IXamlLineInfo lineInfo)
        {
            PropagateLineInfo(writer, lineInfo);
            writer.WriteNode(reader);
        }

        public static void WriteStartMember(this XamlWriter writer, XamlMember xamlMember, IXamlLineInfo lineInfo)
        {
            PropagateLineInfo(writer, lineInfo);
            writer.WriteStartMember(xamlMember);
        }

        public static void WriteStartMember(this XamlWriter writer, XamlMember xamlMember, int lineNumber, int linePosition)
        {
            PropagateLineInfo(writer, lineNumber, linePosition);
            writer.WriteStartMember(xamlMember);
        }

        public static void WriteStartObject(this XamlWriter writer, XamlType type, IXamlLineInfo lineInfo)
        {
            PropagateLineInfo(writer, lineInfo);
            writer.WriteStartObject(type);
        }

        public static void WriteValue(this XamlWriter writer, object value, IXamlLineInfo lineInfo)
        {
            PropagateLineInfo(writer, lineInfo);
            writer.WriteValue(value);
        }
    }
}

