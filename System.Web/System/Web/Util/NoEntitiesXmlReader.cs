namespace System.Web.Util
{
    using System;
    using System.IO;
    using System.Xml;

    internal sealed class NoEntitiesXmlReader : XmlTextReader
    {
        public NoEntitiesXmlReader(Stream datastream) : base(datastream)
        {
            this.Initialize();
        }

        public NoEntitiesXmlReader(TextReader reader) : base(reader)
        {
            this.Initialize();
        }

        public NoEntitiesXmlReader(string filepath) : base(filepath)
        {
            this.Initialize();
        }

        public NoEntitiesXmlReader(string baseURI, Stream contentStream) : base(baseURI, contentStream)
        {
            base.EntityHandling = EntityHandling.ExpandCharEntities;
        }

        private void Initialize()
        {
            base.EntityHandling = EntityHandling.ExpandCharEntities;
        }

        public override void ResolveEntity()
        {
        }
    }
}

