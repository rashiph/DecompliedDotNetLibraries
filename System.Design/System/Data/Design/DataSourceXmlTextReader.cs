namespace System.Data.Design
{
    using System;
    using System.IO;
    using System.Xml;

    internal class DataSourceXmlTextReader : XmlTextReader
    {
        private DesignDataSource dataSource;
        private bool readingDataSource;

        internal DataSourceXmlTextReader(DesignDataSource dataSource, Stream stream, string baseURI) : base(baseURI, stream)
        {
            this.dataSource = dataSource;
            this.readingDataSource = false;
        }

        internal DataSourceXmlTextReader(DesignDataSource dataSource, TextReader textReader, string baseURI) : base(baseURI, textReader)
        {
            this.dataSource = dataSource;
            this.readingDataSource = false;
        }

        public override bool Read()
        {
            bool flag = base.Read();
            if (((flag && !this.readingDataSource) && ((this.NodeType == XmlNodeType.Element) && (this.LocalName == "DataSource"))) && (this.NamespaceURI == "urn:schemas-microsoft-com:xml-msdatasource"))
            {
                this.readingDataSource = true;
                this.dataSource.ReadDataSourceExtraInformation(this);
                flag = !this.EOF;
            }
            return flag;
        }
    }
}

