namespace System.Xml
{
    using System;

    internal class GuidArrayHelperWithString : ArrayHelper<string, Guid>
    {
        public static readonly GuidArrayHelperWithString Instance = new GuidArrayHelperWithString();

        protected override int ReadArray(XmlDictionaryReader reader, string localName, string namespaceUri, Guid[] array, int offset, int count)
        {
            return reader.ReadArray(localName, namespaceUri, array, offset, count);
        }

        protected override void WriteArray(XmlDictionaryWriter writer, string prefix, string localName, string namespaceUri, Guid[] array, int offset, int count)
        {
            writer.WriteArray(prefix, localName, namespaceUri, array, offset, count);
        }
    }
}

