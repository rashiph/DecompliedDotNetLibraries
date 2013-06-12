namespace System.Runtime.Serialization.Formatters.Binary
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting.Messaging;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters;
    using System.Security;

    [ComVisible(true)]
    public sealed class BinaryFormatter : IRemotingFormatter, IFormatter
    {
        internal FormatterAssemblyStyle m_assemblyFormat;
        internal SerializationBinder m_binder;
        internal StreamingContext m_context;
        internal object[] m_crossAppDomainArray;
        internal TypeFilterLevel m_securityLevel;
        internal ISurrogateSelector m_surrogates;
        internal FormatterTypeStyle m_typeFormat;
        private static Dictionary<Type, TypeInformation> typeNameCache = new Dictionary<Type, TypeInformation>();

        public BinaryFormatter()
        {
            this.m_typeFormat = FormatterTypeStyle.TypesAlways;
            this.m_securityLevel = TypeFilterLevel.Full;
            this.m_surrogates = null;
            this.m_context = new StreamingContext(StreamingContextStates.All);
        }

        public BinaryFormatter(ISurrogateSelector selector, StreamingContext context)
        {
            this.m_typeFormat = FormatterTypeStyle.TypesAlways;
            this.m_securityLevel = TypeFilterLevel.Full;
            this.m_surrogates = selector;
            this.m_context = context;
        }

        [SecuritySafeCritical]
        public object Deserialize(Stream serializationStream)
        {
            return this.Deserialize(serializationStream, null);
        }

        [SecuritySafeCritical]
        public object Deserialize(Stream serializationStream, HeaderHandler handler)
        {
            return this.Deserialize(serializationStream, handler, true);
        }

        [SecurityCritical]
        internal object Deserialize(Stream serializationStream, HeaderHandler handler, bool fCheck)
        {
            return this.Deserialize(serializationStream, handler, fCheck, null);
        }

        [SecurityCritical]
        internal object Deserialize(Stream serializationStream, HeaderHandler handler, bool fCheck, IMethodCallMessage methodCallMessage)
        {
            return this.Deserialize(serializationStream, handler, fCheck, false, methodCallMessage);
        }

        [SecurityCritical]
        internal object Deserialize(Stream serializationStream, HeaderHandler handler, bool fCheck, bool isCrossAppDomain, IMethodCallMessage methodCallMessage)
        {
            if (serializationStream == null)
            {
                throw new ArgumentNullException("serializationStream", Environment.GetResourceString("ArgumentNull_WithParamName", new object[] { serializationStream }));
            }
            if (serializationStream.CanSeek && (serializationStream.Length == 0L))
            {
                throw new SerializationException(Environment.GetResourceString("Serialization_Stream"));
            }
            InternalFE formatterEnums = new InternalFE {
                FEtypeFormat = this.m_typeFormat,
                FEserializerTypeEnum = InternalSerializerTypeE.Binary,
                FEassemblyFormat = this.m_assemblyFormat,
                FEsecurityLevel = this.m_securityLevel
            };
            ObjectReader objectReader = new ObjectReader(serializationStream, this.m_surrogates, this.m_context, formatterEnums, this.m_binder) {
                crossAppDomainArray = this.m_crossAppDomainArray
            };
            return objectReader.Deserialize(handler, new __BinaryParser(serializationStream, objectReader), fCheck, isCrossAppDomain, methodCallMessage);
        }

        [SecuritySafeCritical]
        public object DeserializeMethodResponse(Stream serializationStream, HeaderHandler handler, IMethodCallMessage methodCallMessage)
        {
            return this.Deserialize(serializationStream, handler, true, methodCallMessage);
        }

        internal static TypeInformation GetTypeInformation(Type type)
        {
            lock (typeNameCache)
            {
                TypeInformation information = null;
                if (!typeNameCache.TryGetValue(type, out information))
                {
                    bool flag;
                    string clrAssemblyName = FormatterServices.GetClrAssemblyName(type, out flag);
                    information = new TypeInformation(FormatterServices.GetClrTypeFullName(type), clrAssemblyName, flag);
                    typeNameCache.Add(type, information);
                }
                return information;
            }
        }

        [SecuritySafeCritical]
        public void Serialize(Stream serializationStream, object graph)
        {
            this.Serialize(serializationStream, graph, null);
        }

        [SecuritySafeCritical]
        public void Serialize(Stream serializationStream, object graph, Header[] headers)
        {
            this.Serialize(serializationStream, graph, headers, true);
        }

        [SecurityCritical]
        internal void Serialize(Stream serializationStream, object graph, Header[] headers, bool fCheck)
        {
            if (serializationStream == null)
            {
                throw new ArgumentNullException("serializationStream", Environment.GetResourceString("ArgumentNull_WithParamName", new object[] { serializationStream }));
            }
            InternalFE formatterEnums = new InternalFE {
                FEtypeFormat = this.m_typeFormat,
                FEserializerTypeEnum = InternalSerializerTypeE.Binary,
                FEassemblyFormat = this.m_assemblyFormat
            };
            ObjectWriter objectWriter = new ObjectWriter(this.m_surrogates, this.m_context, formatterEnums, this.m_binder);
            __BinaryWriter serWriter = new __BinaryWriter(serializationStream, objectWriter, this.m_typeFormat);
            objectWriter.Serialize(graph, headers, serWriter, fCheck);
            this.m_crossAppDomainArray = objectWriter.crossAppDomainArray;
        }

        [SecurityCritical, ComVisible(false)]
        public object UnsafeDeserialize(Stream serializationStream, HeaderHandler handler)
        {
            return this.Deserialize(serializationStream, handler, false);
        }

        [SecurityCritical, ComVisible(false)]
        public object UnsafeDeserializeMethodResponse(Stream serializationStream, HeaderHandler handler, IMethodCallMessage methodCallMessage)
        {
            return this.Deserialize(serializationStream, handler, false, methodCallMessage);
        }

        public FormatterAssemblyStyle AssemblyFormat
        {
            get
            {
                return this.m_assemblyFormat;
            }
            set
            {
                this.m_assemblyFormat = value;
            }
        }

        public SerializationBinder Binder
        {
            get
            {
                return this.m_binder;
            }
            set
            {
                this.m_binder = value;
            }
        }

        public StreamingContext Context
        {
            get
            {
                return this.m_context;
            }
            set
            {
                this.m_context = value;
            }
        }

        public TypeFilterLevel FilterLevel
        {
            get
            {
                return this.m_securityLevel;
            }
            set
            {
                this.m_securityLevel = value;
            }
        }

        public ISurrogateSelector SurrogateSelector
        {
            get
            {
                return this.m_surrogates;
            }
            set
            {
                this.m_surrogates = value;
            }
        }

        public FormatterTypeStyle TypeFormat
        {
            get
            {
                return this.m_typeFormat;
            }
            set
            {
                this.m_typeFormat = value;
            }
        }
    }
}

