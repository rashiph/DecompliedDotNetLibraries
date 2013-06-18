namespace System.Resources
{
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Security.Permissions;
    using System.Text;
    using System.Windows.Forms;
    using System.Xml;

    [PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust"), PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
    public class ResXResourceWriter : IResourceWriter, IDisposable
    {
        internal const string AliasStr = "alias";
        internal const string AssemblyStr = "assembly";
        private string basePath;
        internal static readonly string Beta2CompatSerializedObjectMimeType = "text/microsoft-urt/psuedoml-serialized/base64";
        private IFormatter binaryFormatter;
        public static readonly string BinSerializedObjectMimeType = "application/x-microsoft.net.object.binary.base64";
        public static readonly string ByteArraySerializedObjectMimeType = "application/x-microsoft.net.object.bytearray.base64";
        private Hashtable cachedAliases;
        internal const string CommentStr = "comment";
        internal static readonly string CompatBinSerializedObjectMimeType = "text/microsoft-urt/binary-serialized/base64";
        internal static readonly string CompatSoapSerializedObjectMimeType = "text/microsoft-urt/soap-serialized/base64";
        internal const string DataStr = "data";
        public static readonly string DefaultSerializedObjectMimeType = BinSerializedObjectMimeType;
        private string fileName;
        private bool hasBeenSaved;
        private bool initialized;
        internal const string MetadataStr = "metadata";
        internal const string MimeTypeStr = "mimetype";
        internal const string NameStr = "name";
        internal const string ReaderStr = "reader";
        internal const string ResHeaderStr = "resheader";
        public static readonly string ResMimeType = "text/microsoft-resx";
        internal const string ResMimeTypeStr = "resmimetype";
        public static readonly string ResourceSchema = ("\r\n    <!-- \r\n    Microsoft ResX Schema \r\n    \r\n    Version " + Version + "\r\n    \r\n    The primary goals of this format is to allow a simple XML format \r\n    that is mostly human readable. The generation and parsing of the \r\n    various data types are done through the TypeConverter classes \r\n    associated with the data types.\r\n    \r\n    Example:\r\n    \r\n    ... ado.net/XML headers & schema ...\r\n    <resheader name=\"resmimetype\">text/microsoft-resx</resheader>\r\n    <resheader name=\"version\">" + Version + "</resheader>\r\n    <resheader name=\"reader\">System.Resources.ResXResourceReader, System.Windows.Forms, ...</resheader>\r\n    <resheader name=\"writer\">System.Resources.ResXResourceWriter, System.Windows.Forms, ...</resheader>\r\n    <data name=\"Name1\"><value>this is my long string</value><comment>this is a comment</comment></data>\r\n    <data name=\"Color1\" type=\"System.Drawing.Color, System.Drawing\">Blue</data>\r\n    <data name=\"Bitmap1\" mimetype=\"" + BinSerializedObjectMimeType + "\">\r\n        <value>[base64 mime encoded serialized .NET Framework object]</value>\r\n    </data>\r\n    <data name=\"Icon1\" type=\"System.Drawing.Icon, System.Drawing\" mimetype=\"" + ByteArraySerializedObjectMimeType + "\">\r\n        <value>[base64 mime encoded string representing a byte array form of the .NET Framework object]</value>\r\n        <comment>This is a comment</comment>\r\n    </data>\r\n                \r\n    There are any number of \"resheader\" rows that contain simple \r\n    name/value pairs.\r\n    \r\n    Each data row contains a name, and value. The row also contains a \r\n    type or mimetype. Type corresponds to a .NET class that support \r\n    text/value conversion through the TypeConverter architecture. \r\n    Classes that don't support this are serialized and stored with the \r\n    mimetype set.\r\n    \r\n    The mimetype is used for serialized objects, and tells the \r\n    ResXResourceReader how to depersist the object. This is currently not \r\n    extensible. For a given mimetype the value must be set accordingly:\r\n    \r\n    Note - " + BinSerializedObjectMimeType + " is the format \r\n    that the ResXResourceWriter will generate, however the reader can \r\n    read any of the formats listed below.\r\n    \r\n    mimetype: " + BinSerializedObjectMimeType + "\r\n    value   : The object must be serialized with \r\n            : System.Runtime.Serialization.Formatters.Binary.BinaryFormatter\r\n            : and then encoded with base64 encoding.\r\n    \r\n    mimetype: " + SoapSerializedObjectMimeType + "\r\n    value   : The object must be serialized with \r\n            : System.Runtime.Serialization.Formatters.Soap.SoapFormatter\r\n            : and then encoded with base64 encoding.\r\n\r\n    mimetype: " + ByteArraySerializedObjectMimeType + "\r\n    value   : The object must be serialized into a byte array \r\n            : using a System.ComponentModel.TypeConverter\r\n            : and then encoded with base64 encoding.\r\n    -->\r\n    <xsd:schema id=\"root\" xmlns=\"\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:msdata=\"urn:schemas-microsoft-com:xml-msdata\">\r\n        <xsd:import namespace=\"http://www.w3.org/XML/1998/namespace\"/>\r\n        <xsd:element name=\"root\" msdata:IsDataSet=\"true\">\r\n            <xsd:complexType>\r\n                <xsd:choice maxOccurs=\"unbounded\">\r\n                    <xsd:element name=\"metadata\">\r\n                        <xsd:complexType>\r\n                            <xsd:sequence>\r\n                            <xsd:element name=\"value\" type=\"xsd:string\" minOccurs=\"0\"/>\r\n                            </xsd:sequence>\r\n                            <xsd:attribute name=\"name\" use=\"required\" type=\"xsd:string\"/>\r\n                            <xsd:attribute name=\"type\" type=\"xsd:string\"/>\r\n                            <xsd:attribute name=\"mimetype\" type=\"xsd:string\"/>\r\n                            <xsd:attribute ref=\"xml:space\"/>                            \r\n                        </xsd:complexType>\r\n                    </xsd:element>\r\n                    <xsd:element name=\"assembly\">\r\n                      <xsd:complexType>\r\n                        <xsd:attribute name=\"alias\" type=\"xsd:string\"/>\r\n                        <xsd:attribute name=\"name\" type=\"xsd:string\"/>\r\n                      </xsd:complexType>\r\n                    </xsd:element>\r\n                    <xsd:element name=\"data\">\r\n                        <xsd:complexType>\r\n                            <xsd:sequence>\r\n                                <xsd:element name=\"value\" type=\"xsd:string\" minOccurs=\"0\" msdata:Ordinal=\"1\" />\r\n                                <xsd:element name=\"comment\" type=\"xsd:string\" minOccurs=\"0\" msdata:Ordinal=\"2\" />\r\n                            </xsd:sequence>\r\n                            <xsd:attribute name=\"name\" type=\"xsd:string\" use=\"required\" msdata:Ordinal=\"1\" />\r\n                            <xsd:attribute name=\"type\" type=\"xsd:string\" msdata:Ordinal=\"3\" />\r\n                            <xsd:attribute name=\"mimetype\" type=\"xsd:string\" msdata:Ordinal=\"4\" />\r\n                            <xsd:attribute ref=\"xml:space\"/>\r\n                        </xsd:complexType>\r\n                    </xsd:element>\r\n                    <xsd:element name=\"resheader\">\r\n                        <xsd:complexType>\r\n                            <xsd:sequence>\r\n                                <xsd:element name=\"value\" type=\"xsd:string\" minOccurs=\"0\" msdata:Ordinal=\"1\" />\r\n                            </xsd:sequence>\r\n                            <xsd:attribute name=\"name\" type=\"xsd:string\" use=\"required\" />\r\n                        </xsd:complexType>\r\n                    </xsd:element>\r\n                </xsd:choice>\r\n            </xsd:complexType>\r\n        </xsd:element>\r\n        </xsd:schema>\r\n        ");
        private static TraceSwitch ResValueProviderSwitch = new TraceSwitch("ResX", "Debug the resource value provider");
        public static readonly string SoapSerializedObjectMimeType = "application/x-microsoft.net.object.soap.base64";
        private Stream stream;
        private TextWriter textWriter;
        private Func<System.Type, string> typeNameConverter;
        internal const string TypeStr = "type";
        internal const string ValueStr = "value";
        public static readonly string Version = "2.0";
        internal const string VersionStr = "version";
        internal const string WriterStr = "writer";
        private XmlTextWriter xmlTextWriter;

        public ResXResourceWriter(Stream stream)
        {
            this.binaryFormatter = new BinaryFormatter();
            this.stream = stream;
        }

        public ResXResourceWriter(TextWriter textWriter)
        {
            this.binaryFormatter = new BinaryFormatter();
            this.textWriter = textWriter;
        }

        public ResXResourceWriter(string fileName)
        {
            this.binaryFormatter = new BinaryFormatter();
            this.fileName = fileName;
        }

        public ResXResourceWriter(Stream stream, Func<System.Type, string> typeNameConverter)
        {
            this.binaryFormatter = new BinaryFormatter();
            this.stream = stream;
            this.typeNameConverter = typeNameConverter;
        }

        public ResXResourceWriter(TextWriter textWriter, Func<System.Type, string> typeNameConverter)
        {
            this.binaryFormatter = new BinaryFormatter();
            this.textWriter = textWriter;
            this.typeNameConverter = typeNameConverter;
        }

        public ResXResourceWriter(string fileName, Func<System.Type, string> typeNameConverter)
        {
            this.binaryFormatter = new BinaryFormatter();
            this.fileName = fileName;
            this.typeNameConverter = typeNameConverter;
        }

        public virtual void AddAlias(string aliasName, AssemblyName assemblyName)
        {
            if (assemblyName == null)
            {
                throw new ArgumentNullException("assemblyName");
            }
            if (this.cachedAliases == null)
            {
                this.cachedAliases = new Hashtable();
            }
            this.cachedAliases[assemblyName.FullName] = aliasName;
        }

        private void AddAssemblyRow(string elementName, string alias, string name)
        {
            this.Writer.WriteStartElement(elementName);
            if (!string.IsNullOrEmpty(alias))
            {
                this.Writer.WriteAttributeString("alias", alias);
            }
            if (!string.IsNullOrEmpty(name))
            {
                this.Writer.WriteAttributeString("name", name);
            }
            this.Writer.WriteEndElement();
        }

        private void AddDataRow(string elementName, string name, byte[] value)
        {
            this.AddDataRow(elementName, name, ToBase64WrappedString(value), this.TypeNameWithAssembly(typeof(byte[])), null, null);
        }

        private void AddDataRow(string elementName, string name, object value)
        {
            if (value is string)
            {
                this.AddDataRow(elementName, name, (string) value);
            }
            else if (value is byte[])
            {
                this.AddDataRow(elementName, name, (byte[]) value);
            }
            else if (value is ResXFileRef)
            {
                ResXFileRef fileRef = (ResXFileRef) value;
                ResXDataNode node = new ResXDataNode(name, fileRef, this.typeNameConverter);
                if (fileRef != null)
                {
                    fileRef.MakeFilePathRelative(this.BasePath);
                }
                DataNodeInfo dataNodeInfo = node.GetDataNodeInfo();
                this.AddDataRow(elementName, dataNodeInfo.Name, dataNodeInfo.ValueData, dataNodeInfo.TypeName, dataNodeInfo.MimeType, dataNodeInfo.Comment);
            }
            else
            {
                DataNodeInfo info2 = new ResXDataNode(name, value, this.typeNameConverter).GetDataNodeInfo();
                this.AddDataRow(elementName, info2.Name, info2.ValueData, info2.TypeName, info2.MimeType, info2.Comment);
            }
        }

        private void AddDataRow(string elementName, string name, string value)
        {
            if (value == null)
            {
                this.AddDataRow(elementName, name, value, MultitargetUtil.GetAssemblyQualifiedName(typeof(ResXNullRef), this.typeNameConverter), null, null);
            }
            else
            {
                this.AddDataRow(elementName, name, value, null, null, null);
            }
        }

        private void AddDataRow(string elementName, string name, string value, string type, string mimeType, string comment)
        {
            if (this.hasBeenSaved)
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("ResXResourceWriterSaved"));
            }
            string aliasFromName = null;
            if (!string.IsNullOrEmpty(type) && (elementName == "data"))
            {
                if (string.IsNullOrEmpty(this.GetFullName(type)))
                {
                    try
                    {
                        System.Type type2 = System.Type.GetType(type);
                        if (type2 == typeof(string))
                        {
                            type = null;
                        }
                        else if (type2 != null)
                        {
                            string fullName = this.GetFullName(MultitargetUtil.GetAssemblyQualifiedName(type2, this.typeNameConverter));
                            aliasFromName = this.GetAliasFromName(new AssemblyName(fullName));
                        }
                    }
                    catch
                    {
                    }
                }
                else
                {
                    aliasFromName = this.GetAliasFromName(new AssemblyName(this.GetFullName(type)));
                }
            }
            this.Writer.WriteStartElement(elementName);
            this.Writer.WriteAttributeString("name", name);
            if ((!string.IsNullOrEmpty(aliasFromName) && !string.IsNullOrEmpty(type)) && (elementName == "data"))
            {
                string str4 = this.GetTypeName(type) + ", " + aliasFromName;
                this.Writer.WriteAttributeString("type", str4);
            }
            else if (type != null)
            {
                this.Writer.WriteAttributeString("type", type);
            }
            if (mimeType != null)
            {
                this.Writer.WriteAttributeString("mimetype", mimeType);
            }
            if (((type == null) && (mimeType == null)) || ((type != null) && type.StartsWith("System.Char", StringComparison.Ordinal)))
            {
                this.Writer.WriteAttributeString("xml", "space", null, "preserve");
            }
            this.Writer.WriteStartElement("value");
            if (!string.IsNullOrEmpty(value))
            {
                this.Writer.WriteString(value);
            }
            this.Writer.WriteEndElement();
            if (!string.IsNullOrEmpty(comment))
            {
                this.Writer.WriteStartElement("comment");
                this.Writer.WriteString(comment);
                this.Writer.WriteEndElement();
            }
            this.Writer.WriteEndElement();
        }

        public void AddMetadata(string name, byte[] value)
        {
            this.AddDataRow("metadata", name, value);
        }

        public void AddMetadata(string name, object value)
        {
            this.AddDataRow("metadata", name, value);
        }

        public void AddMetadata(string name, string value)
        {
            this.AddDataRow("metadata", name, value);
        }

        public void AddResource(ResXDataNode node)
        {
            ResXDataNode node2 = node.DeepClone();
            ResXFileRef fileRef = node2.FileRef;
            string basePath = this.BasePath;
            if (!string.IsNullOrEmpty(basePath))
            {
                if (!basePath.EndsWith(@"\"))
                {
                    basePath = basePath + @"\";
                }
                if (fileRef != null)
                {
                    fileRef.MakeFilePathRelative(basePath);
                }
            }
            DataNodeInfo dataNodeInfo = node2.GetDataNodeInfo();
            this.AddDataRow("data", dataNodeInfo.Name, dataNodeInfo.ValueData, dataNodeInfo.TypeName, dataNodeInfo.MimeType, dataNodeInfo.Comment);
        }

        public void AddResource(string name, byte[] value)
        {
            this.AddDataRow("data", name, value);
        }

        public void AddResource(string name, object value)
        {
            if (value is ResXDataNode)
            {
                this.AddResource((ResXDataNode) value);
            }
            else
            {
                this.AddDataRow("data", name, value);
            }
        }

        public void AddResource(string name, string value)
        {
            this.AddDataRow("data", name, value);
        }

        public void Close()
        {
            this.Dispose();
        }

        public virtual void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (!this.hasBeenSaved)
                {
                    this.Generate();
                }
                if (this.xmlTextWriter != null)
                {
                    this.xmlTextWriter.Close();
                    this.xmlTextWriter = null;
                }
                if (this.stream != null)
                {
                    this.stream.Close();
                    this.stream = null;
                }
                if (this.textWriter != null)
                {
                    this.textWriter.Close();
                    this.textWriter = null;
                }
            }
        }

        ~ResXResourceWriter()
        {
            this.Dispose(false);
        }

        public void Generate()
        {
            if (this.hasBeenSaved)
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("ResXResourceWriterSaved"));
            }
            this.hasBeenSaved = true;
            this.Writer.WriteEndElement();
            this.Writer.Flush();
        }

        private string GetAliasFromName(AssemblyName assemblyName)
        {
            if (this.cachedAliases == null)
            {
                this.cachedAliases = new Hashtable();
            }
            string name = (string) this.cachedAliases[assemblyName.FullName];
            if (string.IsNullOrEmpty(name))
            {
                name = assemblyName.Name;
                this.AddAlias(name, assemblyName);
                this.AddAssemblyRow("assembly", name, assemblyName.FullName);
            }
            return name;
        }

        private string GetFullName(string typeName)
        {
            int index = typeName.IndexOf(",");
            if (index == -1)
            {
                return null;
            }
            return typeName.Substring(index + 2);
        }

        private string GetTypeName(string typeName)
        {
            int index = typeName.IndexOf(",");
            if (index != -1)
            {
                return typeName.Substring(0, index);
            }
            return typeName;
        }

        private void InitializeWriter()
        {
            if (this.xmlTextWriter == null)
            {
                bool flag = false;
                if (this.textWriter != null)
                {
                    this.textWriter.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
                    flag = true;
                    this.xmlTextWriter = new XmlTextWriter(this.textWriter);
                }
                else if (this.stream != null)
                {
                    this.xmlTextWriter = new XmlTextWriter(this.stream, Encoding.UTF8);
                }
                else
                {
                    this.xmlTextWriter = new XmlTextWriter(this.fileName, Encoding.UTF8);
                }
                this.xmlTextWriter.Formatting = Formatting.Indented;
                this.xmlTextWriter.Indentation = 2;
                if (!flag)
                {
                    this.xmlTextWriter.WriteStartDocument();
                }
            }
            else
            {
                this.xmlTextWriter.WriteStartDocument();
            }
            this.xmlTextWriter.WriteStartElement("root");
            XmlTextReader reader = new XmlTextReader(new StringReader(ResourceSchema)) {
                WhitespaceHandling = WhitespaceHandling.None
            };
            this.xmlTextWriter.WriteNode(reader, true);
            this.xmlTextWriter.WriteStartElement("resheader");
            this.xmlTextWriter.WriteAttributeString("name", "resmimetype");
            this.xmlTextWriter.WriteStartElement("value");
            this.xmlTextWriter.WriteString(ResMimeType);
            this.xmlTextWriter.WriteEndElement();
            this.xmlTextWriter.WriteEndElement();
            this.xmlTextWriter.WriteStartElement("resheader");
            this.xmlTextWriter.WriteAttributeString("name", "version");
            this.xmlTextWriter.WriteStartElement("value");
            this.xmlTextWriter.WriteString(Version);
            this.xmlTextWriter.WriteEndElement();
            this.xmlTextWriter.WriteEndElement();
            this.xmlTextWriter.WriteStartElement("resheader");
            this.xmlTextWriter.WriteAttributeString("name", "reader");
            this.xmlTextWriter.WriteStartElement("value");
            this.xmlTextWriter.WriteString(MultitargetUtil.GetAssemblyQualifiedName(typeof(ResXResourceReader), this.typeNameConverter));
            this.xmlTextWriter.WriteEndElement();
            this.xmlTextWriter.WriteEndElement();
            this.xmlTextWriter.WriteStartElement("resheader");
            this.xmlTextWriter.WriteAttributeString("name", "writer");
            this.xmlTextWriter.WriteStartElement("value");
            this.xmlTextWriter.WriteString(MultitargetUtil.GetAssemblyQualifiedName(typeof(ResXResourceWriter), this.typeNameConverter));
            this.xmlTextWriter.WriteEndElement();
            this.xmlTextWriter.WriteEndElement();
            this.initialized = true;
        }

        private static string ToBase64WrappedString(byte[] data)
        {
            string str = Convert.ToBase64String(data);
            if (str.Length <= 80)
            {
                return str;
            }
            StringBuilder builder = new StringBuilder(str.Length + ((str.Length / 80) * 3));
            int startIndex = 0;
            while (startIndex < (str.Length - 80))
            {
                builder.Append("\r\n");
                builder.Append("        ");
                builder.Append(str, startIndex, 80);
                startIndex += 80;
            }
            builder.Append("\r\n");
            builder.Append("        ");
            builder.Append(str, startIndex, str.Length - startIndex);
            builder.Append("\r\n");
            return builder.ToString();
        }

        private string TypeNameWithAssembly(System.Type type)
        {
            return MultitargetUtil.GetAssemblyQualifiedName(type, this.typeNameConverter);
        }

        public string BasePath
        {
            get
            {
                return this.basePath;
            }
            set
            {
                this.basePath = value;
            }
        }

        private XmlWriter Writer
        {
            get
            {
                if (!this.initialized)
                {
                    this.InitializeWriter();
                }
                return this.xmlTextWriter;
            }
        }
    }
}

