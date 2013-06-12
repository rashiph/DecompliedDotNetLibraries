namespace System.Xml.Schema
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Threading;
    using System.Xml;

    [Obsolete("Use System.Xml.Schema.XmlSchemaSet for schema compilation and validation. http://go.microsoft.com/fwlink/?linkid=14202")]
    public sealed class XmlSchemaCollection : ICollection, IEnumerable
    {
        private Hashtable collection;
        private bool isThreadSafe;
        private XmlNameTable nameTable;
        private SchemaNames schemaNames;
        private int timeout;
        private ReaderWriterLock wLock;
        private System.Xml.XmlResolver xmlResolver;

        public event System.Xml.Schema.ValidationEventHandler ValidationEventHandler;

        public XmlSchemaCollection() : this(new System.Xml.NameTable())
        {
        }

        public XmlSchemaCollection(XmlNameTable nametable)
        {
            this.timeout = -1;
            this.isThreadSafe = true;
            if (nametable == null)
            {
                throw new ArgumentNullException("nametable");
            }
            this.nameTable = nametable;
            this.collection = Hashtable.Synchronized(new Hashtable());
            this.xmlResolver = new XmlUrlResolver();
            this.isThreadSafe = true;
            if (this.isThreadSafe)
            {
                this.wLock = new ReaderWriterLock();
            }
        }

        public XmlSchema Add(XmlSchema schema)
        {
            return this.Add(schema, this.xmlResolver);
        }

        public void Add(XmlSchemaCollection schema)
        {
            if (schema == null)
            {
                throw new ArgumentNullException("schema");
            }
            if (this != schema)
            {
                IDictionaryEnumerator enumerator = schema.collection.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    XmlSchemaCollectionNode node = (XmlSchemaCollectionNode) enumerator.Value;
                    this.Add(node.NamespaceURI, node);
                }
            }
        }

        public XmlSchema Add(string ns, string uri)
        {
            if ((uri == null) || (uri.Length == 0))
            {
                throw new ArgumentNullException("uri");
            }
            XmlTextReader reader = new XmlTextReader(uri, this.nameTable) {
                XmlResolver = this.xmlResolver
            };
            XmlSchema schema = null;
            try
            {
                schema = this.Add(ns, reader, this.xmlResolver);
                while (reader.Read())
                {
                }
            }
            finally
            {
                reader.Close();
            }
            return schema;
        }

        private void Add(string ns, XmlSchemaCollectionNode node)
        {
            if (this.isThreadSafe)
            {
                this.wLock.AcquireWriterLock(this.timeout);
            }
            try
            {
                if (this.collection[ns] != null)
                {
                    this.collection.Remove(ns);
                }
                this.collection.Add(ns, node);
            }
            finally
            {
                if (this.isThreadSafe)
                {
                    this.wLock.ReleaseWriterLock();
                }
            }
        }

        public XmlSchema Add(string ns, XmlReader reader)
        {
            return this.Add(ns, reader, this.xmlResolver);
        }

        public XmlSchema Add(XmlSchema schema, System.Xml.XmlResolver resolver)
        {
            if (schema == null)
            {
                throw new ArgumentNullException("schema");
            }
            SchemaInfo schemaInfo = new SchemaInfo {
                SchemaType = SchemaType.XSD
            };
            return this.Add(schema.TargetNamespace, schemaInfo, schema, true, resolver);
        }

        public XmlSchema Add(string ns, XmlReader reader, System.Xml.XmlResolver resolver)
        {
            SchemaType type;
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }
            XmlNameTable nameTable = reader.NameTable;
            SchemaInfo schemaInfo = new SchemaInfo();
            System.Xml.Schema.Parser parser = new System.Xml.Schema.Parser(SchemaType.None, nameTable, this.GetSchemaNames(nameTable), this.validationEventHandler) {
                XmlResolver = resolver
            };
            try
            {
                type = parser.Parse(reader, ns);
            }
            catch (XmlSchemaException exception)
            {
                this.SendValidationEvent(exception);
                return null;
            }
            if (type == SchemaType.XSD)
            {
                schemaInfo.SchemaType = SchemaType.XSD;
                return this.Add(ns, schemaInfo, parser.XmlSchema, true, resolver);
            }
            SchemaInfo xdrSchema = parser.XdrSchema;
            return this.Add(ns, parser.XdrSchema, null, true, resolver);
        }

        internal XmlSchema Add(string ns, SchemaInfo schemaInfo, XmlSchema schema, bool compile)
        {
            return this.Add(ns, schemaInfo, schema, compile, this.xmlResolver);
        }

        private XmlSchema Add(string ns, SchemaInfo schemaInfo, XmlSchema schema, bool compile, System.Xml.XmlResolver resolver)
        {
            int num = 0;
            if (schema != null)
            {
                if ((schema.ErrorCount == 0) && compile)
                {
                    if (!schema.CompileSchema(this, resolver, schemaInfo, ns, this.validationEventHandler, this.nameTable, true))
                    {
                        num = 1;
                    }
                    ns = (schema.TargetNamespace == null) ? string.Empty : schema.TargetNamespace;
                }
                num += schema.ErrorCount;
            }
            else
            {
                num += schemaInfo.ErrorCount;
                ns = this.NameTable.Add(ns);
            }
            if (num == 0)
            {
                XmlSchemaCollectionNode node = new XmlSchemaCollectionNode {
                    NamespaceURI = ns,
                    SchemaInfo = schemaInfo,
                    Schema = schema
                };
                this.Add(ns, node);
                return schema;
            }
            return null;
        }

        public bool Contains(string ns)
        {
            return (this.collection[(ns != null) ? ns : string.Empty] != null);
        }

        public bool Contains(XmlSchema schema)
        {
            if (schema == null)
            {
                throw new ArgumentNullException("schema");
            }
            return (this[schema.TargetNamespace] != null);
        }

        public void CopyTo(XmlSchema[] array, int index)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException("index");
            }
            XmlSchemaCollectionEnumerator enumerator = this.GetEnumerator();
            while (enumerator.MoveNext())
            {
                if (enumerator.Current != null)
                {
                    if (index == array.Length)
                    {
                        throw new ArgumentOutOfRangeException("index");
                    }
                    array[index++] = enumerator.Current;
                }
            }
        }

        public XmlSchemaCollectionEnumerator GetEnumerator()
        {
            return new XmlSchemaCollectionEnumerator(this.collection);
        }

        internal SchemaInfo GetSchemaInfo(string ns)
        {
            XmlSchemaCollectionNode node = (XmlSchemaCollectionNode) this.collection[(ns != null) ? ns : string.Empty];
            if (node == null)
            {
                return null;
            }
            return node.SchemaInfo;
        }

        internal SchemaNames GetSchemaNames(XmlNameTable nt)
        {
            if (this.nameTable != nt)
            {
                return new SchemaNames(nt);
            }
            if (this.schemaNames == null)
            {
                this.schemaNames = new SchemaNames(this.nameTable);
            }
            return this.schemaNames;
        }

        private void SendValidationEvent(XmlSchemaException e)
        {
            if (this.validationEventHandler == null)
            {
                throw e;
            }
            this.validationEventHandler(this, new ValidationEventArgs(e));
        }

        void ICollection.CopyTo(Array array, int index)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException("index");
            }
            XmlSchemaCollectionEnumerator enumerator = this.GetEnumerator();
            while (enumerator.MoveNext())
            {
                if ((index == array.Length) && array.IsFixedSize)
                {
                    throw new ArgumentOutOfRangeException("index");
                }
                array.SetValue(enumerator.Current, index++);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new XmlSchemaCollectionEnumerator(this.collection);
        }

        public int Count
        {
            get
            {
                return this.collection.Count;
            }
        }

        internal System.Xml.Schema.ValidationEventHandler EventHandler
        {
            get
            {
                return this.validationEventHandler;
            }
            set
            {
                this.validationEventHandler = value;
            }
        }

        public XmlSchema this[string ns]
        {
            get
            {
                XmlSchemaCollectionNode node = (XmlSchemaCollectionNode) this.collection[(ns != null) ? ns : string.Empty];
                if (node == null)
                {
                    return null;
                }
                return node.Schema;
            }
        }

        public XmlNameTable NameTable
        {
            get
            {
                return this.nameTable;
            }
        }

        int ICollection.Count
        {
            get
            {
                return this.collection.Count;
            }
        }

        bool ICollection.IsSynchronized
        {
            get
            {
                return true;
            }
        }

        object ICollection.SyncRoot
        {
            get
            {
                return this;
            }
        }

        internal System.Xml.XmlResolver XmlResolver
        {
            set
            {
                this.xmlResolver = value;
            }
        }
    }
}

