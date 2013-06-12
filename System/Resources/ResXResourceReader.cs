namespace System.Resources
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel.Design;
    using System.Drawing;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Security.Permissions;
    using System.Windows.Forms;
    using System.Xml;

    [PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust"), PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
    public class ResXResourceReader : IResourceReader, IEnumerable, IDisposable
    {
        private IAliasResolver aliasResolver;
        private AssemblyName[] assemblyNames;
        private string basePath;
        private string fileContents;
        private string fileName;
        private bool isReaderDirty;
        private TextReader reader;
        private ListDictionary resData;
        private string resHeaderMimeType;
        private string resHeaderReaderType;
        private string resHeaderVersion;
        private string resHeaderWriterType;
        private ListDictionary resMetadata;
        private Stream stream;
        private ITypeResolutionService typeResolver;
        private bool useResXDataNodes;

        private ResXResourceReader(ITypeResolutionService typeResolver)
        {
            this.typeResolver = typeResolver;
            this.aliasResolver = new ReaderAliasResolver();
        }

        private ResXResourceReader(AssemblyName[] assemblyNames)
        {
            this.assemblyNames = assemblyNames;
            this.aliasResolver = new ReaderAliasResolver();
        }

        public ResXResourceReader(Stream stream) : this(stream, (ITypeResolutionService) null, null)
        {
        }

        public ResXResourceReader(TextReader reader) : this(reader, (ITypeResolutionService) null, null)
        {
        }

        public ResXResourceReader(string fileName) : this(fileName, (ITypeResolutionService) null, null)
        {
        }

        public ResXResourceReader(Stream stream, ITypeResolutionService typeResolver) : this(stream, typeResolver, null)
        {
        }

        public ResXResourceReader(Stream stream, AssemblyName[] assemblyNames) : this(stream, assemblyNames, null)
        {
        }

        public ResXResourceReader(TextReader reader, ITypeResolutionService typeResolver) : this(reader, typeResolver, null)
        {
        }

        public ResXResourceReader(TextReader reader, AssemblyName[] assemblyNames) : this(reader, assemblyNames, null)
        {
        }

        public ResXResourceReader(string fileName, ITypeResolutionService typeResolver) : this(fileName, typeResolver, null)
        {
        }

        public ResXResourceReader(string fileName, AssemblyName[] assemblyNames) : this(fileName, assemblyNames, null)
        {
        }

        internal ResXResourceReader(Stream stream, ITypeResolutionService typeResolver, IAliasResolver aliasResolver)
        {
            this.stream = stream;
            this.typeResolver = typeResolver;
            this.aliasResolver = aliasResolver;
            if (this.aliasResolver == null)
            {
                this.aliasResolver = new ReaderAliasResolver();
            }
        }

        internal ResXResourceReader(Stream stream, AssemblyName[] assemblyNames, IAliasResolver aliasResolver)
        {
            this.stream = stream;
            this.assemblyNames = assemblyNames;
            this.aliasResolver = aliasResolver;
            if (this.aliasResolver == null)
            {
                this.aliasResolver = new ReaderAliasResolver();
            }
        }

        internal ResXResourceReader(TextReader reader, ITypeResolutionService typeResolver, IAliasResolver aliasResolver)
        {
            this.reader = reader;
            this.typeResolver = typeResolver;
            this.aliasResolver = aliasResolver;
            if (this.aliasResolver == null)
            {
                this.aliasResolver = new ReaderAliasResolver();
            }
        }

        internal ResXResourceReader(TextReader reader, AssemblyName[] assemblyNames, IAliasResolver aliasResolver)
        {
            this.reader = reader;
            this.assemblyNames = assemblyNames;
            this.aliasResolver = aliasResolver;
            if (this.aliasResolver == null)
            {
                this.aliasResolver = new ReaderAliasResolver();
            }
        }

        internal ResXResourceReader(string fileName, ITypeResolutionService typeResolver, IAliasResolver aliasResolver)
        {
            this.fileName = fileName;
            this.typeResolver = typeResolver;
            this.aliasResolver = aliasResolver;
            if (this.aliasResolver == null)
            {
                this.aliasResolver = new ReaderAliasResolver();
            }
        }

        internal ResXResourceReader(string fileName, AssemblyName[] assemblyNames, IAliasResolver aliasResolver)
        {
            this.fileName = fileName;
            this.assemblyNames = assemblyNames;
            this.aliasResolver = aliasResolver;
            if (this.aliasResolver == null)
            {
                this.aliasResolver = new ReaderAliasResolver();
            }
        }

        public void Close()
        {
            ((IDisposable) this).Dispose();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if ((this.fileName != null) && (this.stream != null))
                {
                    this.stream.Close();
                    this.stream = null;
                }
                if (this.reader != null)
                {
                    this.reader.Close();
                    this.reader = null;
                }
            }
        }

        private void EnsureResData()
        {
            if (this.resData == null)
            {
                this.resData = new ListDictionary();
                this.resMetadata = new ListDictionary();
                XmlTextReader reader = null;
                try
                {
                    if (this.fileContents != null)
                    {
                        reader = new XmlTextReader(new StringReader(this.fileContents));
                    }
                    else if (this.reader != null)
                    {
                        reader = new XmlTextReader(this.reader);
                    }
                    else if ((this.fileName != null) || (this.stream != null))
                    {
                        if (this.stream == null)
                        {
                            this.stream = new FileStream(this.fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
                        }
                        reader = new XmlTextReader(this.stream);
                    }
                    this.SetupNameTable(reader);
                    reader.WhitespaceHandling = WhitespaceHandling.None;
                    this.ParseXml(reader);
                }
                finally
                {
                    if ((this.fileName != null) && (this.stream != null))
                    {
                        this.stream.Close();
                        this.stream = null;
                    }
                }
            }
        }

        ~ResXResourceReader()
        {
            this.Dispose(false);
        }

        public static ResXResourceReader FromFileContents(string fileContents)
        {
            return FromFileContents(fileContents, (ITypeResolutionService) null);
        }

        public static ResXResourceReader FromFileContents(string fileContents, ITypeResolutionService typeResolver)
        {
            return new ResXResourceReader(typeResolver) { fileContents = fileContents };
        }

        public static ResXResourceReader FromFileContents(string fileContents, AssemblyName[] assemblyNames)
        {
            return new ResXResourceReader(assemblyNames) { fileContents = fileContents };
        }

        private string GetAliasFromTypeName(string typeName)
        {
            int index = typeName.IndexOf(",");
            return typeName.Substring(index + 2);
        }

        public IDictionaryEnumerator GetEnumerator()
        {
            this.isReaderDirty = true;
            this.EnsureResData();
            return this.resData.GetEnumerator();
        }

        public IDictionaryEnumerator GetMetadataEnumerator()
        {
            this.EnsureResData();
            return this.resMetadata.GetEnumerator();
        }

        private Point GetPosition(XmlReader reader)
        {
            Point point = new Point(0, 0);
            IXmlLineInfo info = reader as IXmlLineInfo;
            if (info != null)
            {
                point.Y = info.LineNumber;
                point.X = info.LinePosition;
            }
            return point;
        }

        private string GetTypeFromTypeName(string typeName)
        {
            int index = typeName.IndexOf(",");
            return typeName.Substring(0, index);
        }

        private void ParseAssemblyNode(XmlReader reader, bool isMetaData)
        {
            string str = reader["alias"];
            string assemblyName = reader["name"];
            AssemblyName name = new AssemblyName(assemblyName);
            if (string.IsNullOrEmpty(str))
            {
                str = name.Name;
            }
            this.aliasResolver.PushAlias(str, name);
        }

        private void ParseDataNode(XmlTextReader reader, bool isMetaData)
        {
            DataNodeInfo nodeInfo = new DataNodeInfo {
                Name = reader["name"]
            };
            string str = reader["type"];
            string aliasFromTypeName = null;
            AssemblyName name = null;
            if (!string.IsNullOrEmpty(str))
            {
                aliasFromTypeName = this.GetAliasFromTypeName(str);
            }
            if (!string.IsNullOrEmpty(aliasFromTypeName))
            {
                name = this.aliasResolver.ResolveAlias(aliasFromTypeName);
            }
            if (name != null)
            {
                nodeInfo.TypeName = this.GetTypeFromTypeName(str) + ", " + name.FullName;
            }
            else
            {
                nodeInfo.TypeName = reader["type"];
            }
            nodeInfo.MimeType = reader["mimetype"];
            bool flag = false;
            nodeInfo.ReaderPosition = this.GetPosition(reader);
            while (!flag && reader.Read())
            {
                if ((reader.NodeType == XmlNodeType.EndElement) && (reader.LocalName.Equals("data") || reader.LocalName.Equals("metadata")))
                {
                    flag = true;
                }
                else
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        if (reader.Name.Equals("value"))
                        {
                            WhitespaceHandling whitespaceHandling = reader.WhitespaceHandling;
                            try
                            {
                                reader.WhitespaceHandling = WhitespaceHandling.Significant;
                                nodeInfo.ValueData = reader.ReadString();
                                continue;
                            }
                            finally
                            {
                                reader.WhitespaceHandling = whitespaceHandling;
                            }
                        }
                        if (reader.Name.Equals("comment"))
                        {
                            nodeInfo.Comment = reader.ReadString();
                        }
                        continue;
                    }
                    nodeInfo.ValueData = reader.Value.Trim();
                }
            }
            if (nodeInfo.Name == null)
            {
                throw new ArgumentException(System.Windows.Forms.SR.GetString("InvalidResXResourceNoName", new object[] { nodeInfo.ValueData }));
            }
            ResXDataNode node = new ResXDataNode(nodeInfo, this.BasePath);
            if (this.UseResXDataNodes)
            {
                this.resData[nodeInfo.Name] = node;
            }
            else
            {
                IDictionary dictionary = isMetaData ? this.resMetadata : this.resData;
                if (this.assemblyNames == null)
                {
                    dictionary[nodeInfo.Name] = node.GetValue(this.typeResolver);
                }
                else
                {
                    dictionary[nodeInfo.Name] = node.GetValue(this.assemblyNames);
                }
            }
        }

        private void ParseResHeaderNode(XmlReader reader)
        {
            string objA = reader["name"];
            if (objA != null)
            {
                reader.ReadStartElement();
                if (object.Equals(objA, "version"))
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        this.resHeaderVersion = reader.ReadElementString();
                    }
                    else
                    {
                        this.resHeaderVersion = reader.Value.Trim();
                    }
                }
                else if (object.Equals(objA, "resmimetype"))
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        this.resHeaderMimeType = reader.ReadElementString();
                    }
                    else
                    {
                        this.resHeaderMimeType = reader.Value.Trim();
                    }
                }
                else if (object.Equals(objA, "reader"))
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        this.resHeaderReaderType = reader.ReadElementString();
                    }
                    else
                    {
                        this.resHeaderReaderType = reader.Value.Trim();
                    }
                }
                else if (object.Equals(objA, "writer"))
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        this.resHeaderWriterType = reader.ReadElementString();
                    }
                    else
                    {
                        this.resHeaderWriterType = reader.Value.Trim();
                    }
                }
                else
                {
                    string str2 = objA.ToLower(CultureInfo.InvariantCulture);
                    if (str2 != null)
                    {
                        if (!(str2 == "version"))
                        {
                            if (!(str2 == "resmimetype"))
                            {
                                if (!(str2 == "reader"))
                                {
                                    if (str2 == "writer")
                                    {
                                        if (reader.NodeType == XmlNodeType.Element)
                                        {
                                            this.resHeaderWriterType = reader.ReadElementString();
                                            return;
                                        }
                                        this.resHeaderWriterType = reader.Value.Trim();
                                    }
                                    return;
                                }
                                if (reader.NodeType == XmlNodeType.Element)
                                {
                                    this.resHeaderReaderType = reader.ReadElementString();
                                    return;
                                }
                                this.resHeaderReaderType = reader.Value.Trim();
                                return;
                            }
                        }
                        else
                        {
                            if (reader.NodeType == XmlNodeType.Element)
                            {
                                this.resHeaderVersion = reader.ReadElementString();
                                return;
                            }
                            this.resHeaderVersion = reader.Value.Trim();
                            return;
                        }
                        if (reader.NodeType == XmlNodeType.Element)
                        {
                            this.resHeaderMimeType = reader.ReadElementString();
                        }
                        else
                        {
                            this.resHeaderMimeType = reader.Value.Trim();
                        }
                    }
                }
            }
        }

        private void ParseXml(XmlTextReader reader)
        {
            bool flag = false;
            try
            {
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        string localName = reader.LocalName;
                        if (reader.LocalName.Equals("assembly"))
                        {
                            this.ParseAssemblyNode(reader, false);
                        }
                        else
                        {
                            if (reader.LocalName.Equals("data"))
                            {
                                this.ParseDataNode(reader, false);
                                continue;
                            }
                            if (reader.LocalName.Equals("resheader"))
                            {
                                this.ParseResHeaderNode(reader);
                                continue;
                            }
                            if (reader.LocalName.Equals("metadata"))
                            {
                                this.ParseDataNode(reader, true);
                            }
                        }
                    }
                }
                flag = true;
            }
            catch (SerializationException exception)
            {
                Point position = this.GetPosition(reader);
                string message = System.Windows.Forms.SR.GetString("SerializationException", new object[] { reader["type"], position.Y, position.X, exception.Message });
                XmlException innerException = new XmlException(message, exception, position.Y, position.X);
                SerializationException exception3 = new SerializationException(message, innerException);
                throw exception3;
            }
            catch (TargetInvocationException exception4)
            {
                Point point2 = this.GetPosition(reader);
                string str2 = System.Windows.Forms.SR.GetString("InvocationException", new object[] { reader["type"], point2.Y, point2.X, exception4.InnerException.Message });
                XmlException inner = new XmlException(str2, exception4.InnerException, point2.Y, point2.X);
                TargetInvocationException exception6 = new TargetInvocationException(str2, inner);
                throw exception6;
            }
            catch (XmlException exception7)
            {
                throw new ArgumentException(System.Windows.Forms.SR.GetString("InvalidResXFile", new object[] { exception7.Message }), exception7);
            }
            catch (Exception exception8)
            {
                if (System.Windows.Forms.ClientUtils.IsSecurityOrCriticalException(exception8))
                {
                    throw;
                }
                Point point3 = this.GetPosition(reader);
                XmlException exception9 = new XmlException(exception8.Message, exception8, point3.Y, point3.X);
                throw new ArgumentException(System.Windows.Forms.SR.GetString("InvalidResXFile", new object[] { exception9.Message }), exception9);
            }
            finally
            {
                if (!flag)
                {
                    this.resData = null;
                    this.resMetadata = null;
                }
            }
            bool flag2 = false;
            if (object.Equals(this.resHeaderMimeType, ResXResourceWriter.ResMimeType))
            {
                System.Type type = typeof(ResXResourceReader);
                System.Type type2 = typeof(ResXResourceWriter);
                string resHeaderReaderType = this.resHeaderReaderType;
                string resHeaderWriterType = this.resHeaderWriterType;
                if ((resHeaderReaderType != null) && (resHeaderReaderType.IndexOf(',') != -1))
                {
                    resHeaderReaderType = resHeaderReaderType.Split(new char[] { ',' })[0].Trim();
                }
                if ((resHeaderWriterType != null) && (resHeaderWriterType.IndexOf(',') != -1))
                {
                    resHeaderWriterType = resHeaderWriterType.Split(new char[] { ',' })[0].Trim();
                }
                if (((resHeaderReaderType != null) && (resHeaderWriterType != null)) && (resHeaderReaderType.Equals(type.FullName) && resHeaderWriterType.Equals(type2.FullName)))
                {
                    flag2 = true;
                }
            }
            if (!flag2)
            {
                this.resData = null;
                this.resMetadata = null;
                throw new ArgumentException(System.Windows.Forms.SR.GetString("InvalidResXFileReaderWriterTypes"));
            }
        }

        private void SetupNameTable(XmlReader reader)
        {
            reader.NameTable.Add("type");
            reader.NameTable.Add("name");
            reader.NameTable.Add("data");
            reader.NameTable.Add("metadata");
            reader.NameTable.Add("mimetype");
            reader.NameTable.Add("value");
            reader.NameTable.Add("resheader");
            reader.NameTable.Add("version");
            reader.NameTable.Add("resmimetype");
            reader.NameTable.Add("reader");
            reader.NameTable.Add("writer");
            reader.NameTable.Add(ResXResourceWriter.BinSerializedObjectMimeType);
            reader.NameTable.Add(ResXResourceWriter.SoapSerializedObjectMimeType);
            reader.NameTable.Add("assembly");
            reader.NameTable.Add("alias");
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        void IDisposable.Dispose()
        {
            GC.SuppressFinalize(this);
            this.Dispose(true);
        }

        public string BasePath
        {
            get
            {
                return this.basePath;
            }
            set
            {
                if (this.isReaderDirty)
                {
                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("InvalidResXBasePathOperation"));
                }
                this.basePath = value;
            }
        }

        public bool UseResXDataNodes
        {
            get
            {
                return this.useResXDataNodes;
            }
            set
            {
                if (this.isReaderDirty)
                {
                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("InvalidResXBasePathOperation"));
                }
                this.useResXDataNodes = value;
            }
        }

        private sealed class ReaderAliasResolver : IAliasResolver
        {
            private Hashtable cachedAliases = new Hashtable();

            internal ReaderAliasResolver()
            {
            }

            public void PushAlias(string alias, AssemblyName name)
            {
                if ((this.cachedAliases != null) && !string.IsNullOrEmpty(alias))
                {
                    this.cachedAliases[alias] = name;
                }
            }

            public AssemblyName ResolveAlias(string alias)
            {
                AssemblyName name = null;
                if (this.cachedAliases != null)
                {
                    name = (AssemblyName) this.cachedAliases[alias];
                }
                return name;
            }
        }
    }
}

