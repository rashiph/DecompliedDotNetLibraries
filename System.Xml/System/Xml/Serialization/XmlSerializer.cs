namespace System.Xml.Serialization
{
    using System;
    using System.CodeDom.Compiler;
    using System.Collections;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Security.Permissions;
    using System.Security.Policy;
    using System.Threading;
    using System.Xml;

    public class XmlSerializer
    {
        private static TempAssemblyCache cache = new TempAssemblyCache();
        private static XmlSerializerNamespaces defaultNamespaces;
        private XmlDeserializationEvents events;
        private XmlMapping mapping;
        private Type primitiveType;
        private TempAssembly tempAssembly;
        private bool typedSerializer;
        private static Hashtable xmlSerializerTable = new Hashtable();

        public event XmlAttributeEventHandler UnknownAttribute
        {
            add
            {
                this.events.OnUnknownAttribute = (XmlAttributeEventHandler) Delegate.Combine(this.events.OnUnknownAttribute, value);
            }
            remove
            {
                this.events.OnUnknownAttribute = (XmlAttributeEventHandler) Delegate.Remove(this.events.OnUnknownAttribute, value);
            }
        }

        public event XmlElementEventHandler UnknownElement
        {
            add
            {
                this.events.OnUnknownElement = (XmlElementEventHandler) Delegate.Combine(this.events.OnUnknownElement, value);
            }
            remove
            {
                this.events.OnUnknownElement = (XmlElementEventHandler) Delegate.Remove(this.events.OnUnknownElement, value);
            }
        }

        public event XmlNodeEventHandler UnknownNode
        {
            add
            {
                this.events.OnUnknownNode = (XmlNodeEventHandler) Delegate.Combine(this.events.OnUnknownNode, value);
            }
            remove
            {
                this.events.OnUnknownNode = (XmlNodeEventHandler) Delegate.Remove(this.events.OnUnknownNode, value);
            }
        }

        public event UnreferencedObjectEventHandler UnreferencedObject
        {
            add
            {
                this.events.OnUnreferencedObject = (UnreferencedObjectEventHandler) Delegate.Combine(this.events.OnUnreferencedObject, value);
            }
            remove
            {
                this.events.OnUnreferencedObject = (UnreferencedObjectEventHandler) Delegate.Remove(this.events.OnUnreferencedObject, value);
            }
        }

        protected XmlSerializer()
        {
            this.events = new XmlDeserializationEvents();
        }

        public XmlSerializer(Type type) : this(type, (string) null)
        {
        }

        public XmlSerializer(XmlTypeMapping xmlTypeMapping)
        {
            this.events = new XmlDeserializationEvents();
            this.tempAssembly = GenerateTempAssembly(xmlTypeMapping);
            this.mapping = xmlTypeMapping;
        }

        public XmlSerializer(Type type, Type[] extraTypes) : this(type, null, extraTypes, null, null, null)
        {
        }

        public XmlSerializer(Type type, string defaultNamespace)
        {
            this.events = new XmlDeserializationEvents();
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            this.mapping = GetKnownMapping(type, defaultNamespace);
            if (this.mapping != null)
            {
                this.primitiveType = type;
            }
            else
            {
                this.tempAssembly = cache[defaultNamespace, type];
                if (this.tempAssembly == null)
                {
                    lock (cache)
                    {
                        this.tempAssembly = cache[defaultNamespace, type];
                        if (this.tempAssembly == null)
                        {
                            XmlSerializerImplementation implementation;
                            Assembly assembly = TempAssembly.LoadGeneratedAssembly(type, defaultNamespace, out implementation);
                            if (assembly == null)
                            {
                                this.mapping = new XmlReflectionImporter(defaultNamespace).ImportTypeMapping(type, null, defaultNamespace);
                                this.tempAssembly = GenerateTempAssembly(this.mapping, type, defaultNamespace);
                            }
                            else
                            {
                                this.mapping = XmlReflectionImporter.GetTopLevelMapping(type, defaultNamespace);
                                this.tempAssembly = new TempAssembly(new XmlMapping[] { this.mapping }, assembly, implementation);
                            }
                        }
                        cache.Add(defaultNamespace, type, this.tempAssembly);
                    }
                }
                if (this.mapping == null)
                {
                    this.mapping = XmlReflectionImporter.GetTopLevelMapping(type, defaultNamespace);
                }
            }
        }

        public XmlSerializer(Type type, XmlAttributeOverrides overrides) : this(type, overrides, new Type[0], null, null, null)
        {
        }

        public XmlSerializer(Type type, XmlRootAttribute root) : this(type, null, new Type[0], root, null, null)
        {
        }

        public XmlSerializer(Type type, XmlAttributeOverrides overrides, Type[] extraTypes, XmlRootAttribute root, string defaultNamespace) : this(type, overrides, extraTypes, root, defaultNamespace, null)
        {
        }

        public XmlSerializer(Type type, XmlAttributeOverrides overrides, Type[] extraTypes, XmlRootAttribute root, string defaultNamespace, string location) : this(type, overrides, extraTypes, root, defaultNamespace, location, null)
        {
        }

        [Obsolete("This method is obsolete and will be removed in a future release of the .NET Framework. Please use a XmlSerializer constructor overload which does not take an Evidence parameter. See http://go2.microsoft.com/fwlink/?LinkId=131738 for more information.")]
        public XmlSerializer(Type type, XmlAttributeOverrides overrides, Type[] extraTypes, XmlRootAttribute root, string defaultNamespace, string location, Evidence evidence)
        {
            this.events = new XmlDeserializationEvents();
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            XmlReflectionImporter importer = new XmlReflectionImporter(overrides, defaultNamespace);
            if (extraTypes != null)
            {
                for (int i = 0; i < extraTypes.Length; i++)
                {
                    importer.IncludeType(extraTypes[i]);
                }
            }
            this.mapping = importer.ImportTypeMapping(type, root, defaultNamespace);
            if (location != null)
            {
                this.DemandForUserLocation();
            }
            this.tempAssembly = GenerateTempAssembly(this.mapping, type, defaultNamespace, location, evidence);
        }

        public virtual bool CanDeserialize(XmlReader xmlReader)
        {
            if (this.primitiveType != null)
            {
                TypeDesc desc = (TypeDesc) TypeScope.PrimtiveTypes[this.primitiveType];
                return xmlReader.IsStartElement(desc.DataType.Name, string.Empty);
            }
            return ((this.tempAssembly != null) && this.tempAssembly.CanRead(this.mapping, xmlReader));
        }

        protected virtual XmlSerializationReader CreateReader()
        {
            throw new NotImplementedException();
        }

        protected virtual XmlSerializationWriter CreateWriter()
        {
            throw new NotImplementedException();
        }

        [PermissionSet(SecurityAction.Demand, Name="FullTrust")]
        private void DemandForUserLocation()
        {
        }

        public object Deserialize(Stream stream)
        {
            XmlTextReader xmlReader = new XmlTextReader(stream) {
                WhitespaceHandling = WhitespaceHandling.Significant,
                Normalization = true,
                XmlResolver = null
            };
            return this.Deserialize(xmlReader, (string) null);
        }

        public object Deserialize(TextReader textReader)
        {
            XmlTextReader xmlReader = new XmlTextReader(textReader) {
                WhitespaceHandling = WhitespaceHandling.Significant,
                Normalization = true,
                XmlResolver = null
            };
            return this.Deserialize(xmlReader, (string) null);
        }

        protected virtual object Deserialize(XmlSerializationReader reader)
        {
            throw new NotImplementedException();
        }

        public object Deserialize(XmlReader xmlReader)
        {
            return this.Deserialize(xmlReader, (string) null);
        }

        public object Deserialize(XmlReader xmlReader, string encodingStyle)
        {
            return this.Deserialize(xmlReader, encodingStyle, this.events);
        }

        public object Deserialize(XmlReader xmlReader, XmlDeserializationEvents events)
        {
            return this.Deserialize(xmlReader, null, events);
        }

        public object Deserialize(XmlReader xmlReader, string encodingStyle, XmlDeserializationEvents events)
        {
            object obj2;
            events.sender = this;
            try
            {
                if (this.primitiveType != null)
                {
                    if ((encodingStyle != null) && (encodingStyle.Length > 0))
                    {
                        throw new InvalidOperationException(Res.GetString("XmlInvalidEncodingNotEncoded1", new object[] { encodingStyle }));
                    }
                    return this.DeserializePrimitive(xmlReader, events);
                }
                if ((this.tempAssembly == null) || this.typedSerializer)
                {
                    XmlSerializationReader reader = this.CreateReader();
                    reader.Init(xmlReader, events, encodingStyle, this.tempAssembly);
                    try
                    {
                        return this.Deserialize(reader);
                    }
                    finally
                    {
                        reader.Dispose();
                    }
                }
                obj2 = this.tempAssembly.InvokeReader(this.mapping, xmlReader, events, encodingStyle);
            }
            catch (Exception innerException)
            {
                if (((innerException is ThreadAbortException) || (innerException is StackOverflowException)) || (innerException is OutOfMemoryException))
                {
                    throw;
                }
                if (innerException is TargetInvocationException)
                {
                    innerException = innerException.InnerException;
                }
                if (xmlReader is IXmlLineInfo)
                {
                    IXmlLineInfo info = (IXmlLineInfo) xmlReader;
                    throw new InvalidOperationException(Res.GetString("XmlSerializeErrorDetails", new object[] { info.LineNumber.ToString(CultureInfo.InvariantCulture), info.LinePosition.ToString(CultureInfo.InvariantCulture) }), innerException);
                }
                throw new InvalidOperationException(Res.GetString("XmlSerializeError"), innerException);
            }
            return obj2;
        }

        private object DeserializePrimitive(XmlReader xmlReader, XmlDeserializationEvents events)
        {
            XmlSerializationPrimitiveReader reader = new XmlSerializationPrimitiveReader();
            reader.Init(xmlReader, events, null, null);
            switch (Type.GetTypeCode(this.primitiveType))
            {
                case TypeCode.Boolean:
                    return reader.Read_boolean();

                case TypeCode.Char:
                    return reader.Read_char();

                case TypeCode.SByte:
                    return reader.Read_byte();

                case TypeCode.Byte:
                    return reader.Read_unsignedByte();

                case TypeCode.Int16:
                    return reader.Read_short();

                case TypeCode.UInt16:
                    return reader.Read_unsignedShort();

                case TypeCode.Int32:
                    return reader.Read_int();

                case TypeCode.UInt32:
                    return reader.Read_unsignedInt();

                case TypeCode.Int64:
                    return reader.Read_long();

                case TypeCode.UInt64:
                    return reader.Read_unsignedLong();

                case TypeCode.Single:
                    return reader.Read_float();

                case TypeCode.Double:
                    return reader.Read_double();

                case TypeCode.Decimal:
                    return reader.Read_decimal();

                case TypeCode.DateTime:
                    return reader.Read_dateTime();

                case TypeCode.String:
                    return reader.Read_string();
            }
            if (this.primitiveType == typeof(XmlQualifiedName))
            {
                return reader.Read_QName();
            }
            if (this.primitiveType == typeof(byte[]))
            {
                return reader.Read_base64Binary();
            }
            if (this.primitiveType != typeof(Guid))
            {
                throw new InvalidOperationException(Res.GetString("XmlUnxpectedType", new object[] { this.primitiveType.FullName }));
            }
            return reader.Read_guid();
        }

        [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
        public static XmlSerializer[] FromMappings(XmlMapping[] mappings)
        {
            return FromMappings(mappings, (Type) null);
        }

        [Obsolete("This method is obsolete and will be removed in a future release of the .NET Framework. Please use an overload of FromMappings which does not take an Evidence parameter. See http://go2.microsoft.com/fwlink/?LinkId=131738 for more information."), PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
        public static XmlSerializer[] FromMappings(XmlMapping[] mappings, Evidence evidence)
        {
            if ((mappings == null) || (mappings.Length == 0))
            {
                return new XmlSerializer[0];
            }
            if (XmlMapping.IsShallow(mappings))
            {
                return new XmlSerializer[0];
            }
            TempAssembly assembly = new TempAssembly(mappings, new Type[0], null, null, evidence);
            XmlSerializerImplementation contract = assembly.Contract;
            XmlSerializer[] serializerArray = new XmlSerializer[mappings.Length];
            for (int i = 0; i < serializerArray.Length; i++)
            {
                serializerArray[i] = (XmlSerializer) contract.TypedSerializers[mappings[i].Key];
            }
            return serializerArray;
        }

        [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
        public static XmlSerializer[] FromMappings(XmlMapping[] mappings, Type type)
        {
            if ((mappings == null) || (mappings.Length == 0))
            {
                return new XmlSerializer[0];
            }
            XmlSerializerImplementation contract = null;
            Assembly assembly = (type == null) ? null : TempAssembly.LoadGeneratedAssembly(type, null, out contract);
            TempAssembly tempAssembly = null;
            if (assembly == null)
            {
                if (XmlMapping.IsShallow(mappings))
                {
                    return new XmlSerializer[0];
                }
                if (type != null)
                {
                    return GetSerializersFromCache(mappings, type);
                }
                tempAssembly = new TempAssembly(mappings, new Type[] { type }, null, null, null);
                XmlSerializer[] serializerArray = new XmlSerializer[mappings.Length];
                contract = tempAssembly.Contract;
                for (int j = 0; j < serializerArray.Length; j++)
                {
                    serializerArray[j] = (XmlSerializer) contract.TypedSerializers[mappings[j].Key];
                    serializerArray[j].SetTempAssembly(tempAssembly, mappings[j]);
                }
                return serializerArray;
            }
            XmlSerializer[] serializerArray2 = new XmlSerializer[mappings.Length];
            for (int i = 0; i < serializerArray2.Length; i++)
            {
                serializerArray2[i] = (XmlSerializer) contract.TypedSerializers[mappings[i].Key];
            }
            return serializerArray2;
        }

        public static XmlSerializer[] FromTypes(Type[] types)
        {
            if (types == null)
            {
                return new XmlSerializer[0];
            }
            XmlReflectionImporter importer = new XmlReflectionImporter();
            XmlTypeMapping[] mappings = new XmlTypeMapping[types.Length];
            for (int i = 0; i < types.Length; i++)
            {
                mappings[i] = importer.ImportTypeMapping(types[i]);
            }
            return FromMappings(mappings);
        }

        [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
        public static Assembly GenerateSerializer(Type[] types, XmlMapping[] mappings)
        {
            CompilerParameters parameters = new CompilerParameters {
                TempFiles = new TempFileCollection(),
                GenerateInMemory = false,
                IncludeDebugInformation = false
            };
            return GenerateSerializer(types, mappings, parameters);
        }

        [PermissionSet(SecurityAction.Demand, Name="FullTrust")]
        public static Assembly GenerateSerializer(Type[] types, XmlMapping[] mappings, CompilerParameters parameters)
        {
            if ((types == null) || (types.Length == 0))
            {
                return null;
            }
            if (mappings == null)
            {
                throw new ArgumentNullException("mappings");
            }
            if (XmlMapping.IsShallow(mappings))
            {
                throw new InvalidOperationException(Res.GetString("XmlMelformMapping"));
            }
            Assembly assembly = null;
            for (int i = 0; i < types.Length; i++)
            {
                Type type = types[i];
                if (DynamicAssemblies.IsTypeDynamic(type))
                {
                    throw new InvalidOperationException(Res.GetString("XmlPregenTypeDynamic", new object[] { type.FullName }));
                }
                if (assembly == null)
                {
                    assembly = type.Assembly;
                }
                else if (type.Assembly != assembly)
                {
                    throw new ArgumentException(Res.GetString("XmlPregenOrphanType", new object[] { type.FullName, assembly.Location }), "types");
                }
            }
            return TempAssembly.GenerateAssembly(mappings, types, null, null, XmlSerializerCompilerParameters.Create(parameters, true), assembly, new Hashtable());
        }

        internal static TempAssembly GenerateTempAssembly(XmlMapping xmlMapping)
        {
            return GenerateTempAssembly(xmlMapping, null, null);
        }

        internal static TempAssembly GenerateTempAssembly(XmlMapping xmlMapping, Type type, string defaultNamespace)
        {
            if (xmlMapping == null)
            {
                throw new ArgumentNullException("xmlMapping");
            }
            return new TempAssembly(new XmlMapping[] { xmlMapping }, new Type[] { type }, defaultNamespace, null, null);
        }

        internal static TempAssembly GenerateTempAssembly(XmlMapping xmlMapping, Type type, string defaultNamespace, string location, Evidence evidence)
        {
            return new TempAssembly(new XmlMapping[] { xmlMapping }, new Type[] { type }, defaultNamespace, location, evidence);
        }

        private static XmlTypeMapping GetKnownMapping(Type type, string ns)
        {
            if ((ns != null) && (ns != string.Empty))
            {
                return null;
            }
            TypeDesc desc = (TypeDesc) TypeScope.PrimtiveTypes[type];
            if (desc == null)
            {
                return null;
            }
            ElementAccessor accessor = new ElementAccessor {
                Name = desc.DataType.Name
            };
            XmlTypeMapping mapping = new XmlTypeMapping(null, accessor);
            mapping.SetKeyInternal(XmlMapping.GenerateKey(type, null, null));
            return mapping;
        }

        private static XmlSerializer[] GetSerializersFromCache(XmlMapping[] mappings, Type type)
        {
            XmlSerializer[] serializerArray = new XmlSerializer[mappings.Length];
            Hashtable hashtable = null;
            lock (xmlSerializerTable)
            {
                hashtable = xmlSerializerTable[type] as Hashtable;
                if (hashtable == null)
                {
                    hashtable = new Hashtable();
                    xmlSerializerTable[type] = hashtable;
                }
            }
            lock (hashtable)
            {
                Hashtable hashtable2 = new Hashtable();
                for (int i = 0; i < mappings.Length; i++)
                {
                    XmlSerializerMappingKey key = new XmlSerializerMappingKey(mappings[i]);
                    serializerArray[i] = hashtable[key] as XmlSerializer;
                    if (serializerArray[i] == null)
                    {
                        hashtable2.Add(key, i);
                    }
                }
                if (hashtable2.Count <= 0)
                {
                    return serializerArray;
                }
                XmlMapping[] xmlMappings = new XmlMapping[hashtable2.Count];
                int index = 0;
                foreach (XmlSerializerMappingKey key2 in hashtable2.Keys)
                {
                    xmlMappings[index++] = key2.Mapping;
                }
                TempAssembly tempAssembly = new TempAssembly(xmlMappings, new Type[] { type }, null, null, null);
                XmlSerializerImplementation contract = tempAssembly.Contract;
                foreach (XmlSerializerMappingKey key3 in hashtable2.Keys)
                {
                    index = (int) hashtable2[key3];
                    serializerArray[index] = (XmlSerializer) contract.TypedSerializers[key3.Mapping.Key];
                    serializerArray[index].SetTempAssembly(tempAssembly, key3.Mapping);
                    hashtable[key3] = serializerArray[index];
                }
            }
            return serializerArray;
        }

        [PermissionSet(SecurityAction.Demand, Name="FullTrust")]
        public static string GetXmlSerializerAssemblyName(Type type)
        {
            return GetXmlSerializerAssemblyName(type, null);
        }

        [PermissionSet(SecurityAction.Demand, Name="FullTrust")]
        public static string GetXmlSerializerAssemblyName(Type type, string defaultNamespace)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            return Compiler.GetTempAssemblyName(type.Assembly.GetName(), defaultNamespace);
        }

        public void Serialize(Stream stream, object o)
        {
            this.Serialize(stream, o, null);
        }

        public void Serialize(TextWriter textWriter, object o)
        {
            this.Serialize(textWriter, o, null);
        }

        protected virtual void Serialize(object o, XmlSerializationWriter writer)
        {
            throw new NotImplementedException();
        }

        public void Serialize(XmlWriter xmlWriter, object o)
        {
            this.Serialize(xmlWriter, o, null);
        }

        public void Serialize(Stream stream, object o, XmlSerializerNamespaces namespaces)
        {
            XmlTextWriter xmlWriter = new XmlTextWriter(stream, null) {
                Formatting = Formatting.Indented,
                Indentation = 2
            };
            this.Serialize(xmlWriter, o, namespaces);
        }

        public void Serialize(TextWriter textWriter, object o, XmlSerializerNamespaces namespaces)
        {
            XmlTextWriter xmlWriter = new XmlTextWriter(textWriter) {
                Formatting = Formatting.Indented,
                Indentation = 2
            };
            this.Serialize(xmlWriter, o, namespaces);
        }

        public void Serialize(XmlWriter xmlWriter, object o, XmlSerializerNamespaces namespaces)
        {
            this.Serialize(xmlWriter, o, namespaces, null);
        }

        public void Serialize(XmlWriter xmlWriter, object o, XmlSerializerNamespaces namespaces, string encodingStyle)
        {
            this.Serialize(xmlWriter, o, namespaces, encodingStyle, null);
        }

        public void Serialize(XmlWriter xmlWriter, object o, XmlSerializerNamespaces namespaces, string encodingStyle, string id)
        {
            try
            {
                if (this.primitiveType != null)
                {
                    if ((encodingStyle != null) && (encodingStyle.Length > 0))
                    {
                        throw new InvalidOperationException(Res.GetString("XmlInvalidEncodingNotEncoded1", new object[] { encodingStyle }));
                    }
                    this.SerializePrimitive(xmlWriter, o, namespaces);
                }
                else
                {
                    if ((this.tempAssembly == null) || this.typedSerializer)
                    {
                        XmlSerializationWriter writer = this.CreateWriter();
                        writer.Init(xmlWriter, ((namespaces == null) || (namespaces.Count == 0)) ? DefaultNamespaces : namespaces, encodingStyle, id, this.tempAssembly);
                        try
                        {
                            this.Serialize(o, writer);
                            goto Label_00F7;
                        }
                        finally
                        {
                            writer.Dispose();
                        }
                    }
                    this.tempAssembly.InvokeWriter(this.mapping, xmlWriter, o, ((namespaces == null) || (namespaces.Count == 0)) ? DefaultNamespaces : namespaces, encodingStyle, id);
                }
            }
            catch (Exception innerException)
            {
                if (((innerException is ThreadAbortException) || (innerException is StackOverflowException)) || (innerException is OutOfMemoryException))
                {
                    throw;
                }
                if (innerException is TargetInvocationException)
                {
                    innerException = innerException.InnerException;
                }
                throw new InvalidOperationException(Res.GetString("XmlGenError"), innerException);
            }
        Label_00F7:
            xmlWriter.Flush();
        }

        private void SerializePrimitive(XmlWriter xmlWriter, object o, XmlSerializerNamespaces namespaces)
        {
            XmlSerializationPrimitiveWriter writer = new XmlSerializationPrimitiveWriter();
            writer.Init(xmlWriter, namespaces, null, null, null);
            switch (Type.GetTypeCode(this.primitiveType))
            {
                case TypeCode.Boolean:
                    writer.Write_boolean(o);
                    return;

                case TypeCode.Char:
                    writer.Write_char(o);
                    return;

                case TypeCode.SByte:
                    writer.Write_byte(o);
                    return;

                case TypeCode.Byte:
                    writer.Write_unsignedByte(o);
                    return;

                case TypeCode.Int16:
                    writer.Write_short(o);
                    return;

                case TypeCode.UInt16:
                    writer.Write_unsignedShort(o);
                    return;

                case TypeCode.Int32:
                    writer.Write_int(o);
                    return;

                case TypeCode.UInt32:
                    writer.Write_unsignedInt(o);
                    return;

                case TypeCode.Int64:
                    writer.Write_long(o);
                    return;

                case TypeCode.UInt64:
                    writer.Write_unsignedLong(o);
                    return;

                case TypeCode.Single:
                    writer.Write_float(o);
                    return;

                case TypeCode.Double:
                    writer.Write_double(o);
                    return;

                case TypeCode.Decimal:
                    writer.Write_decimal(o);
                    return;

                case TypeCode.DateTime:
                    writer.Write_dateTime(o);
                    return;

                case TypeCode.String:
                    writer.Write_string(o);
                    return;
            }
            if (this.primitiveType == typeof(XmlQualifiedName))
            {
                writer.Write_QName(o);
            }
            else if (this.primitiveType == typeof(byte[]))
            {
                writer.Write_base64Binary(o);
            }
            else
            {
                if (this.primitiveType != typeof(Guid))
                {
                    throw new InvalidOperationException(Res.GetString("XmlUnxpectedType", new object[] { this.primitiveType.FullName }));
                }
                writer.Write_guid(o);
            }
        }

        internal void SetTempAssembly(TempAssembly tempAssembly, XmlMapping mapping)
        {
            this.tempAssembly = tempAssembly;
            this.mapping = mapping;
            this.typedSerializer = true;
        }

        private static XmlSerializerNamespaces DefaultNamespaces
        {
            get
            {
                if (defaultNamespaces == null)
                {
                    XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
                    namespaces.AddInternal("xsi", "http://www.w3.org/2001/XMLSchema-instance");
                    namespaces.AddInternal("xsd", "http://www.w3.org/2001/XMLSchema");
                    if (defaultNamespaces == null)
                    {
                        defaultNamespaces = namespaces;
                    }
                }
                return defaultNamespaces;
            }
        }

        private class XmlSerializerMappingKey
        {
            public XmlMapping Mapping;

            public XmlSerializerMappingKey(XmlMapping mapping)
            {
                this.Mapping = mapping;
            }

            public override bool Equals(object obj)
            {
                XmlSerializer.XmlSerializerMappingKey key = obj as XmlSerializer.XmlSerializerMappingKey;
                if (key == null)
                {
                    return false;
                }
                if (this.Mapping.Key != key.Mapping.Key)
                {
                    return false;
                }
                if (this.Mapping.ElementName != key.Mapping.ElementName)
                {
                    return false;
                }
                if (this.Mapping.Namespace != key.Mapping.Namespace)
                {
                    return false;
                }
                if (this.Mapping.IsSoap != key.Mapping.IsSoap)
                {
                    return false;
                }
                return true;
            }

            public override int GetHashCode()
            {
                int num = this.Mapping.IsSoap ? 0 : 1;
                if (this.Mapping.Key != null)
                {
                    num ^= this.Mapping.Key.GetHashCode();
                }
                if (this.Mapping.ElementName != null)
                {
                    num ^= this.Mapping.ElementName.GetHashCode();
                }
                if (this.Mapping.Namespace != null)
                {
                    num ^= this.Mapping.Namespace.GetHashCode();
                }
                return num;
            }
        }
    }
}

