namespace System.Xml
{
    using System;

    internal class BooleanArrayHelperWithString : ArrayHelper<string, bool>
    {
        public static readonly BooleanArrayHelperWithString Instance = new BooleanArrayHelperWithString();

        protected override int ReadArray(XmlDictionaryReader reader, string localName, string namespaceUri, bool[] array, int offset, int count)
        {
            return reader.ReadArray(localName, namespaceUri, array, offset, count);
        }

        protected override void WriteArray(XmlDictionaryWriter writer, string prefix, string localName, string namespaceUri, bool[] array, int offset, int count)
        {
            writer.WriteArray(prefix, localName, namespaceUri, array, offset, count);
        }
    }
}

