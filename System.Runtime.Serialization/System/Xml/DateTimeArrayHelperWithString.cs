namespace System.Xml
{
    using System;

    internal class DateTimeArrayHelperWithString : ArrayHelper<string, DateTime>
    {
        public static readonly DateTimeArrayHelperWithString Instance = new DateTimeArrayHelperWithString();

        protected override int ReadArray(XmlDictionaryReader reader, string localName, string namespaceUri, DateTime[] array, int offset, int count)
        {
            return reader.ReadArray(localName, namespaceUri, array, offset, count);
        }

        protected override void WriteArray(XmlDictionaryWriter writer, string prefix, string localName, string namespaceUri, DateTime[] array, int offset, int count)
        {
            writer.WriteArray(prefix, localName, namespaceUri, array, offset, count);
        }
    }
}

