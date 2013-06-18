namespace System.Xml
{
    using System;

    internal class DecimalArrayHelperWithString : ArrayHelper<string, decimal>
    {
        public static readonly DecimalArrayHelperWithString Instance = new DecimalArrayHelperWithString();

        protected override int ReadArray(XmlDictionaryReader reader, string localName, string namespaceUri, decimal[] array, int offset, int count)
        {
            return reader.ReadArray(localName, namespaceUri, array, offset, count);
        }

        protected override void WriteArray(XmlDictionaryWriter writer, string prefix, string localName, string namespaceUri, decimal[] array, int offset, int count)
        {
            writer.WriteArray(prefix, localName, namespaceUri, array, offset, count);
        }
    }
}

