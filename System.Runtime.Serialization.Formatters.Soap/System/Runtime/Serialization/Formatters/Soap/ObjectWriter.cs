namespace System.Runtime.Serialization.Formatters.Soap
{
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Messaging;
    using System.Runtime.Remoting.Metadata;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters;
    using System.Security.Permissions;
    using System.Text;

    internal sealed class ObjectWriter
    {
        private Hashtable assemblyToIdTable = new Hashtable(20);
        private bool bRemoting;
        private InternalFE formatterEnums;
        private string headerNamespace = "http://schemas.microsoft.com/clr/soap";
        private Header[] headers;
        private StreamingContext m_context;
        private IFormatterConverter m_formatterConverter;
        private ObjectIDGenerator m_idGenerator;
        private SerializationObjectManager m_objectManager;
        private Queue m_objectQueue;
        private Hashtable m_serializedTypeTable;
        private Stream m_stream;
        private ISurrogateSelector m_surrogates;
        private SerStack niPool = new SerStack("NameInfo Pool");
        private long previousId;
        private object previousObj;
        private PrimitiveArray primitiveArray;
        private StringBuilder sburi = new StringBuilder(50);
        internal static SecurityPermission serializationPermission = new SecurityPermission(SecurityPermissionFlag.SerializationFormatter);
        private SerObjectInfoInit serObjectInfoInit;
        private SoapWriter serWriter;
        private long topId;
        private string topName;

        internal ObjectWriter(Stream stream, ISurrogateSelector selector, StreamingContext context, InternalFE formatterEnums)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream", SoapUtil.GetResourceString("ArgumentNull_Stream"));
            }
            this.m_stream = stream;
            this.m_surrogates = selector;
            this.m_context = context;
            this.formatterEnums = formatterEnums;
            this.m_objectManager = new SerializationObjectManager(context);
            this.m_formatterConverter = new FormatterConverter();
        }

        private void ArrayNameToDisplayName(WriteObjectInfo objectInfo, NameInfo arrayElemTypeNameInfo)
        {
            string nIname = arrayElemTypeNameInfo.NIname;
            int index = nIname.IndexOf('[');
            if (index <= 0)
            {
                if (nIname.Equals("System.Object"))
                {
                    arrayElemTypeNameInfo.NIname = "anyType";
                    arrayElemTypeNameInfo.NInameSpaceEnum = InternalNameSpaceE.XdrPrimitive;
                }
            }
            else
            {
                string str2 = nIname.Substring(0, index);
                InternalPrimitiveTypeE code = Converter.ToCode(str2);
                string str3 = null;
                bool flag = false;
                switch (code)
                {
                    case InternalPrimitiveTypeE.Invalid:
                        if (!str2.Equals("String") && !str2.Equals("System.String"))
                        {
                            if (str2.Equals("System.Object"))
                            {
                                flag = true;
                                str3 = "anyType";
                                arrayElemTypeNameInfo.NInameSpaceEnum = InternalNameSpaceE.XdrPrimitive;
                            }
                            else
                            {
                                str3 = str2;
                            }
                            break;
                        }
                        flag = true;
                        str3 = "string";
                        arrayElemTypeNameInfo.NInameSpaceEnum = InternalNameSpaceE.XdrString;
                        break;

                    case InternalPrimitiveTypeE.Char:
                        str3 = str2;
                        arrayElemTypeNameInfo.NInameSpaceEnum = InternalNameSpaceE.UrtSystem;
                        break;

                    default:
                    {
                        flag = true;
                        str3 = Converter.ToXmlDataType(code);
                        string typeName = null;
                        arrayElemTypeNameInfo.NInameSpaceEnum = Converter.GetNameSpaceEnum(code, null, objectInfo, out typeName);
                        break;
                    }
                }
                if (flag)
                {
                    arrayElemTypeNameInfo.NIname = str3 + nIname.Substring(index);
                }
            }
        }

        private NameInfo ArrayTypeToNameInfo(WriteObjectInfo objectInfo, out NameInfo arrayElemTypeNameInfo)
        {
            NameInfo info = this.TypeToNameInfo(objectInfo);
            arrayElemTypeNameInfo = this.TypeToNameInfo(objectInfo.arrayElemObjectInfo);
            this.ArrayNameToDisplayName(objectInfo, arrayElemTypeNameInfo);
            info.NInameSpaceEnum = arrayElemTypeNameInfo.NInameSpaceEnum;
            arrayElemTypeNameInfo.NIisArray = arrayElemTypeNameInfo.NItype.IsArray;
            return info;
        }

        private bool CheckForNull(WriteObjectInfo objectInfo, NameInfo memberNameInfo, NameInfo typeNameInfo, object data)
        {
            bool flag = false;
            if (data == null)
            {
                flag = true;
            }
            if (flag)
            {
                if (typeNameInfo.NItype.IsArray)
                {
                    this.ArrayNameToDisplayName(objectInfo, typeNameInfo);
                }
                if (typeNameInfo.NIisArrayItem)
                {
                    this.serWriter.WriteNullItem(memberNameInfo, typeNameInfo);
                    return flag;
                }
                this.serWriter.WriteNullMember(memberNameInfo, typeNameInfo);
            }
            return flag;
        }

        private bool CheckTypeFormat(FormatterTypeStyle test, FormatterTypeStyle want)
        {
            return ((test & want) == want);
        }

        private long GetAssemblyId(WriteObjectInfo objectInfo)
        {
            long id = 0L;
            bool isNew = false;
            string assemblyString = objectInfo.GetAssemblyString();
            string assemName = assemblyString;
            if (assemblyString.Length == 0)
            {
                return 0L;
            }
            if (assemblyString.Equals(Converter.urtAssemblyString))
            {
                id = 0L;
                isNew = false;
                this.serWriter.WriteAssembly(objectInfo.GetTypeFullName(), objectInfo.objectType, null, (int) id, isNew, objectInfo.IsAttributeNameSpace());
                return id;
            }
            if (this.assemblyToIdTable.ContainsKey(assemblyString))
            {
                id = (long) this.assemblyToIdTable[assemblyString];
                isNew = false;
            }
            else
            {
                id = this.m_idGenerator.GetId("___AssemblyString___" + assemblyString, out isNew);
                this.assemblyToIdTable[assemblyString] = id;
            }
            if (((assemblyString != null) && !objectInfo.IsInteropNameSpace()) && (this.formatterEnums.FEassemblyFormat == FormatterAssemblyStyle.Simple))
            {
                int index = assemblyString.IndexOf(',');
                if (index > 0)
                {
                    assemName = assemblyString.Substring(0, index);
                }
            }
            this.serWriter.WriteAssembly(objectInfo.GetTypeFullName(), objectInfo.objectType, assemName, (int) id, isNew, objectInfo.IsInteropNameSpace());
            return id;
        }

        private NameInfo GetNameInfo()
        {
            NameInfo info = null;
            if (!this.niPool.IsEmpty())
            {
                info = (NameInfo) this.niPool.Pop();
                info.Init();
                return info;
            }
            return new NameInfo();
        }

        private object GetNext(out long objID)
        {
            bool flag;
            if (this.m_objectQueue.Count == 0)
            {
                objID = 0L;
                return null;
            }
            object obj2 = this.m_objectQueue.Dequeue();
            object obj3 = null;
            if (obj2 is WriteObjectInfo)
            {
                obj3 = ((WriteObjectInfo) obj2).obj;
            }
            else
            {
                obj3 = obj2;
            }
            objID = this.m_idGenerator.HasId(obj3, out flag);
            if (flag)
            {
                throw new SerializationException(string.Format(CultureInfo.CurrentCulture, SoapUtil.GetResourceString("Serialization_ObjNoID"), new object[] { obj2 }));
            }
            return obj2;
        }

        private Type GetType(object obj)
        {
            if (RemotingServices.IsTransparentProxy(obj))
            {
                return Converter.typeofMarshalByRefObject;
            }
            return obj.GetType();
        }

        private void HeaderNamespace(Header header, NameInfo nameInfo)
        {
            if (header.HeaderNamespace == null)
            {
                nameInfo.NInamespace = this.headerNamespace;
            }
            else
            {
                nameInfo.NInamespace = header.HeaderNamespace;
            }
            bool isNew = false;
            nameInfo.NIheaderPrefix = "h" + this.InternalGetId(nameInfo.NInamespace, Converter.typeofString, out isNew);
        }

        [Conditional("SER_LOGGING")]
        private void IndexTraceMessage(string message, int[] index)
        {
            StringBuilder builder = new StringBuilder(10);
            builder.Append("[");
            for (int i = 0; i < index.Length; i++)
            {
                builder.Append(index[i]);
                if (i != (index.Length - 1))
                {
                    builder.Append(",");
                }
            }
            builder.Append("]");
        }

        private long InternalGetId(object obj, Type type, out bool isNew)
        {
            if (obj == this.previousObj)
            {
                isNew = false;
                return this.previousId;
            }
            if (type.IsValueType)
            {
                isNew = false;
                this.previousObj = obj;
                this.previousId = -1L;
                return -1L;
            }
            long id = this.m_idGenerator.GetId(obj, out isNew);
            this.previousObj = obj;
            this.previousId = id;
            return id;
        }

        private bool IsEmbeddedAttribute(Type type)
        {
            if (type.IsValueType)
            {
                return true;
            }
            SoapTypeAttribute cachedSoapAttribute = (SoapTypeAttribute) InternalRemotingServices.GetCachedSoapAttribute(type);
            return cachedSoapAttribute.Embedded;
        }

        private NameInfo MemberToNameInfo(string name)
        {
            NameInfo nameInfo = this.GetNameInfo();
            nameInfo.NInameSpaceEnum = InternalNameSpaceE.MemberName;
            nameInfo.NIname = name;
            return nameInfo;
        }

        private void ProcessHeaders(long headerId)
        {
            long num;
            object obj2;
            this.serWriter.WriteHeader((int) headerId, this.headers.Length);
            for (int i = 0; i < this.headers.Length; i++)
            {
                Type type = null;
                if (this.headers[i].Value != null)
                {
                    type = this.GetType(this.headers[i].Value);
                }
                if ((type != null) && (type == Converter.typeofString))
                {
                    NameInfo nameInfo = this.GetNameInfo();
                    nameInfo.NInameSpaceEnum = InternalNameSpaceE.UserNameSpace;
                    nameInfo.NIname = this.headers[i].Name;
                    nameInfo.NIisMustUnderstand = this.headers[i].MustUnderstand;
                    nameInfo.NIobjectId = -1L;
                    this.HeaderNamespace(this.headers[i], nameInfo);
                    this.serWriter.WriteHeaderString(nameInfo, this.headers[i].Value.ToString());
                    this.PutNameInfo(nameInfo);
                }
                else if (this.headers[i].Name.Equals("__MethodSignature"))
                {
                    if (!(this.headers[i].Value is Type[]))
                    {
                        throw new SerializationException(string.Format(CultureInfo.CurrentCulture, SoapUtil.GetResourceString("Serialization_MethodSignature"), new object[] { type }));
                    }
                    Type[] typeArray = (Type[]) this.headers[i].Value;
                    NameInfo[] typeNameInfos = new NameInfo[typeArray.Length];
                    WriteObjectInfo[] infoArray2 = new WriteObjectInfo[typeArray.Length];
                    for (int j = 0; j < typeArray.Length; j++)
                    {
                        infoArray2[j] = WriteObjectInfo.Serialize(typeArray[j], this.m_surrogates, this.m_context, this.serObjectInfoInit, this.m_formatterConverter, null);
                        infoArray2[j].objectId = -1L;
                        infoArray2[j].assemId = this.GetAssemblyId(infoArray2[j]);
                        typeNameInfos[j] = this.TypeToNameInfo(infoArray2[j]);
                    }
                    NameInfo info2 = this.MemberToNameInfo(this.headers[i].Name);
                    info2.NIisMustUnderstand = this.headers[i].MustUnderstand;
                    info2.NItransmitTypeOnMember = true;
                    info2.NIisNestedObject = true;
                    info2.NIisHeader = true;
                    this.HeaderNamespace(this.headers[i], info2);
                    this.serWriter.WriteHeaderMethodSignature(info2, typeNameInfos);
                    for (int k = 0; k < typeArray.Length; k++)
                    {
                        this.PutNameInfo(typeNameInfos[k]);
                        infoArray2[k].ObjectEnd();
                    }
                    this.PutNameInfo(info2);
                }
                else
                {
                    InternalPrimitiveTypeE invalid = InternalPrimitiveTypeE.Invalid;
                    if (type != null)
                    {
                        invalid = Converter.ToCode(type);
                    }
                    if ((type != null) && (invalid == InternalPrimitiveTypeE.Invalid))
                    {
                        long num5 = this.Schedule(this.headers[i].Value, type);
                        if (num5 == -1L)
                        {
                            WriteObjectInfo objectInfo = WriteObjectInfo.Serialize(this.headers[i].Value, this.m_surrogates, this.m_context, this.serObjectInfoInit, this.m_formatterConverter, null, this);
                            objectInfo.objectId = -1L;
                            objectInfo.assemId = this.GetAssemblyId(objectInfo);
                            NameInfo typeNameInfo = this.TypeToNameInfo(objectInfo);
                            NameInfo info5 = this.MemberToNameInfo(this.headers[i].Name);
                            info5.NIisMustUnderstand = this.headers[i].MustUnderstand;
                            info5.NItransmitTypeOnMember = true;
                            info5.NIisNestedObject = true;
                            info5.NIisHeader = true;
                            this.HeaderNamespace(this.headers[i], info5);
                            this.Write(objectInfo, info5, typeNameInfo);
                            this.PutNameInfo(typeNameInfo);
                            this.PutNameInfo(info5);
                            objectInfo.ObjectEnd();
                        }
                        else
                        {
                            NameInfo info6 = this.MemberToNameInfo(this.headers[i].Name);
                            info6.NIisMustUnderstand = this.headers[i].MustUnderstand;
                            info6.NIobjectId = num5;
                            info6.NItransmitTypeOnMember = true;
                            info6.NIisNestedObject = true;
                            this.HeaderNamespace(this.headers[i], info6);
                            this.serWriter.WriteHeaderObjectRef(info6);
                            this.PutNameInfo(info6);
                        }
                    }
                    else
                    {
                        NameInfo info7 = this.GetNameInfo();
                        info7.NInameSpaceEnum = InternalNameSpaceE.UserNameSpace;
                        info7.NIname = this.headers[i].Name;
                        info7.NIisMustUnderstand = this.headers[i].MustUnderstand;
                        info7.NIprimitiveTypeEnum = invalid;
                        this.HeaderNamespace(this.headers[i], info7);
                        NameInfo info8 = null;
                        if (type != null)
                        {
                            info8 = this.TypeToNameInfo(type);
                            info8.NItransmitTypeOnMember = true;
                        }
                        this.serWriter.WriteHeaderEntry(info7, info8, this.headers[i].Value);
                        this.PutNameInfo(info7);
                        if (type != null)
                        {
                            this.PutNameInfo(info8);
                        }
                    }
                }
            }
            this.serWriter.WriteHeaderArrayEnd();
            while ((obj2 = this.GetNext(out num)) != null)
            {
                WriteObjectInfo info9 = null;
                if (obj2 is WriteObjectInfo)
                {
                    info9 = (WriteObjectInfo) obj2;
                }
                else
                {
                    info9 = WriteObjectInfo.Serialize(obj2, this.m_surrogates, this.m_context, this.serObjectInfoInit, this.m_formatterConverter, null, this);
                    info9.assemId = this.GetAssemblyId(info9);
                }
                info9.objectId = num;
                NameInfo memberNameInfo = this.TypeToNameInfo(info9);
                this.Write(info9, memberNameInfo, memberNameInfo);
                this.PutNameInfo(memberNameInfo);
                info9.ObjectEnd();
            }
            this.serWriter.WriteHeaderSectionEnd();
        }

        private XsdVersion ProcessTypeAttributes(Type type)
        {
            SoapTypeAttribute cachedSoapAttribute = InternalRemotingServices.GetCachedSoapAttribute(type) as SoapTypeAttribute;
            XsdVersion version = XsdVersion.V2001;
            if (cachedSoapAttribute != null)
            {
                SoapOption option;
                if ((option = cachedSoapAttribute.SoapOptions & SoapOption.Option1) == SoapOption.Option1)
                {
                    return XsdVersion.V1999;
                }
                if ((option &= SoapOption.Option1) == SoapOption.Option2)
                {
                    version = XsdVersion.V2000;
                }
            }
            return version;
        }

        private void PutNameInfo(NameInfo nameInfo)
        {
            this.niPool.Push(nameInfo);
        }

        private long Schedule(object obj, Type type)
        {
            return this.Schedule(obj, type, null);
        }

        private long Schedule(object obj, Type type, WriteObjectInfo objectInfo)
        {
            bool flag;
            if (obj == null)
            {
                return 0L;
            }
            long num = this.InternalGetId(obj, type, out flag);
            if (flag)
            {
                if (objectInfo == null)
                {
                    this.m_objectQueue.Enqueue(obj);
                    return num;
                }
                this.m_objectQueue.Enqueue(objectInfo);
            }
            return num;
        }

        internal void Serialize(object graph, Header[] inHeaders, SoapWriter serWriter)
        {
            object obj2;
            long num2;
            bool flag;
            serializationPermission.Demand();
            if (graph == null)
            {
                throw new ArgumentNullException("graph", SoapUtil.GetResourceString("ArgumentNull_Graph"));
            }
            if (serWriter == null)
            {
                throw new ArgumentNullException("serWriter", string.Format(CultureInfo.CurrentCulture, SoapUtil.GetResourceString("ArgumentNull_WithParamName"), new object[] { "serWriter" }));
            }
            this.serObjectInfoInit = new SerObjectInfoInit();
            this.serWriter = serWriter;
            this.headers = inHeaders;
            if (graph is IMethodMessage)
            {
                this.bRemoting = true;
                MethodBase methodBase = ((IMethodMessage) graph).MethodBase;
                if (methodBase != null)
                {
                    serWriter.WriteXsdVersion(this.ProcessTypeAttributes(methodBase.ReflectedType));
                }
                else
                {
                    serWriter.WriteXsdVersion(XsdVersion.V2001);
                }
            }
            else
            {
                serWriter.WriteXsdVersion(XsdVersion.V2001);
            }
            this.m_idGenerator = new ObjectIDGenerator();
            this.m_objectQueue = new Queue();
            if (graph is ISoapMessage)
            {
                this.bRemoting = true;
                ISoapMessage message = (ISoapMessage) graph;
                graph = new InternalSoapMessage(message.MethodName, message.XmlNameSpace, message.ParamNames, message.ParamValues, message.ParamTypes);
                this.headers = message.Headers;
            }
            this.m_serializedTypeTable = new Hashtable();
            serWriter.WriteBegin();
            long headerId = 0L;
            this.topId = this.m_idGenerator.GetId(graph, out flag);
            if (this.headers != null)
            {
                headerId = this.m_idGenerator.GetId(this.headers, out flag);
            }
            else
            {
                headerId = -1L;
            }
            this.WriteSerializedStreamHeader(this.topId, headerId);
            if ((this.headers != null) && (this.headers.Length != 0))
            {
                this.ProcessHeaders(headerId);
            }
            this.m_objectQueue.Enqueue(graph);
            while ((obj2 = this.GetNext(out num2)) != null)
            {
                WriteObjectInfo objectInfo = null;
                if (obj2 is WriteObjectInfo)
                {
                    objectInfo = (WriteObjectInfo) obj2;
                }
                else
                {
                    objectInfo = WriteObjectInfo.Serialize(obj2, this.m_surrogates, this.m_context, this.serObjectInfoInit, this.m_formatterConverter, null, this);
                    objectInfo.assemId = this.GetAssemblyId(objectInfo);
                }
                objectInfo.objectId = num2;
                NameInfo memberNameInfo = this.TypeToNameInfo(objectInfo);
                memberNameInfo.NIisTopLevelObject = true;
                if (this.bRemoting && (obj2 == graph))
                {
                    memberNameInfo.NIisRemoteRecord = true;
                }
                this.Write(objectInfo, memberNameInfo, memberNameInfo);
                this.PutNameInfo(memberNameInfo);
                objectInfo.ObjectEnd();
            }
            serWriter.WriteSerializationHeaderEnd();
            serWriter.WriteEnd();
            this.m_idGenerator = new ObjectIDGenerator();
            this.m_serializedTypeTable = new Hashtable();
            this.m_objectManager.RaiseOnSerializedEvent();
        }

        private NameInfo TypeToNameInfo(WriteObjectInfo objectInfo)
        {
            return this.TypeToNameInfo(objectInfo.objectType, objectInfo, Converter.ToCode(objectInfo.objectType), null);
        }

        private NameInfo TypeToNameInfo(Type type)
        {
            return this.TypeToNameInfo(type, null, Converter.ToCode(type), null);
        }

        private NameInfo TypeToNameInfo(WriteObjectInfo objectInfo, NameInfo nameInfo)
        {
            return this.TypeToNameInfo(objectInfo.objectType, objectInfo, Converter.ToCode(objectInfo.objectType), nameInfo);
        }

        private void TypeToNameInfo(Type type, NameInfo nameInfo)
        {
            this.TypeToNameInfo(type, null, Converter.ToCode(type), nameInfo);
        }

        private NameInfo TypeToNameInfo(Type type, WriteObjectInfo objectInfo, InternalPrimitiveTypeE code, NameInfo nameInfo)
        {
            if (nameInfo == null)
            {
                nameInfo = this.GetNameInfo();
            }
            else
            {
                nameInfo.Init();
            }
            nameInfo.NIisSealed = type.IsSealed;
            string typeName = null;
            nameInfo.NInameSpaceEnum = Converter.GetNameSpaceEnum(code, type, objectInfo, out typeName);
            nameInfo.NIprimitiveTypeEnum = code;
            nameInfo.NItype = type;
            nameInfo.NIname = typeName;
            if (objectInfo != null)
            {
                nameInfo.NIattributeInfo = objectInfo.typeAttributeInfo;
                nameInfo.NIassemId = objectInfo.assemId;
            }
            switch (nameInfo.NInameSpaceEnum)
            {
                case InternalNameSpaceE.XdrPrimitive:
                case InternalNameSpaceE.UrtSystem:
                    return nameInfo;

                case InternalNameSpaceE.XdrString:
                    nameInfo.NIname = "string";
                    return nameInfo;

                case InternalNameSpaceE.UrtUser:
                    if (type.Module.Assembly == Converter.urtAssembly)
                    {
                    }
                    return nameInfo;
            }
            return nameInfo;
        }

        private void Write(WriteObjectInfo objectInfo, NameInfo memberNameInfo, NameInfo typeNameInfo)
        {
            object obj2 = objectInfo.obj;
            if (obj2 == null)
            {
                throw new ArgumentNullException("objectInfo.obj", string.Format(CultureInfo.CurrentCulture, SoapUtil.GetResourceString("Serialization_ArgumentNull_Obj"), new object[] { objectInfo.objectType }));
            }
            if (objectInfo.objectType.IsGenericType)
            {
                throw new SerializationException(string.Format(CultureInfo.CurrentCulture, SoapUtil.GetResourceString("Serialization_SoapNoGenericsSupport"), new object[] { objectInfo.objectType }));
            }
            Type objectType = objectInfo.objectType;
            long objectId = objectInfo.objectId;
            if (objectType == Converter.typeofString)
            {
                memberNameInfo.NIobjectId = objectId;
                this.serWriter.WriteObjectString(memberNameInfo, obj2.ToString());
            }
            else if (objectType == Converter.typeofTimeSpan)
            {
                this.serWriter.WriteTopPrimitive(memberNameInfo, obj2);
            }
            else
            {
                if (objectType.IsArray)
                {
                    this.WriteArray(objectInfo, null, null);
                }
                else
                {
                    string[] strArray;
                    Type[] typeArray;
                    object[] objArray;
                    SoapAttributeInfo[] infoArray;
                    objectInfo.GetMemberInfo(out strArray, out typeArray, out objArray, out infoArray);
                    if (this.CheckTypeFormat(this.formatterEnums.FEtypeFormat, FormatterTypeStyle.TypesAlways))
                    {
                        memberNameInfo.NItransmitTypeOnObject = true;
                        memberNameInfo.NIisParentTypeOnObject = true;
                        typeNameInfo.NItransmitTypeOnObject = true;
                        typeNameInfo.NIisParentTypeOnObject = true;
                    }
                    WriteObjectInfo[] memberObjectInfos = new WriteObjectInfo[strArray.Length];
                    for (int i = 0; i < typeArray.Length; i++)
                    {
                        if (Nullable.GetUnderlyingType(typeArray[i]) != null)
                        {
                            throw new SerializationException(string.Format(CultureInfo.CurrentCulture, SoapUtil.GetResourceString("Serialization_SoapNoGenericsSupport"), new object[] { typeArray[i] }));
                        }
                        Type type = null;
                        if (objArray[i] != null)
                        {
                            type = this.GetType(objArray[i]);
                        }
                        else
                        {
                            type = typeof(object);
                        }
                        if (((Converter.ToCode(type) == InternalPrimitiveTypeE.Invalid) && (type != Converter.typeofString)) || (((objectInfo.cache.memberAttributeInfos != null) && (objectInfo.cache.memberAttributeInfos[i] != null)) && (objectInfo.cache.memberAttributeInfos[i].IsXmlAttribute() || objectInfo.cache.memberAttributeInfos[i].IsXmlElement())))
                        {
                            if (objArray[i] != null)
                            {
                                memberObjectInfos[i] = WriteObjectInfo.Serialize(objArray[i], this.m_surrogates, this.m_context, this.serObjectInfoInit, this.m_formatterConverter, (infoArray == null) ? null : infoArray[i], this);
                                memberObjectInfos[i].assemId = this.GetAssemblyId(memberObjectInfos[i]);
                            }
                            else
                            {
                                memberObjectInfos[i] = WriteObjectInfo.Serialize(typeArray[i], this.m_surrogates, this.m_context, this.serObjectInfoInit, this.m_formatterConverter, (infoArray == null) ? null : infoArray[i]);
                                memberObjectInfos[i].assemId = this.GetAssemblyId(memberObjectInfos[i]);
                            }
                        }
                    }
                    this.Write(objectInfo, memberNameInfo, typeNameInfo, strArray, typeArray, objArray, memberObjectInfos);
                }
                if (!this.m_serializedTypeTable.ContainsKey(objectType))
                {
                    this.m_serializedTypeTable.Add(objectType, objectType);
                }
            }
        }

        private void Write(WriteObjectInfo objectInfo, NameInfo memberNameInfo, NameInfo typeNameInfo, string[] memberNames, Type[] memberTypes, object[] memberData, WriteObjectInfo[] memberObjectInfos)
        {
            int length = memberNames.Length;
            NameInfo nameInfo = null;
            if (objectInfo.cache.memberAttributeInfos != null)
            {
                for (int j = 0; j < objectInfo.cache.memberAttributeInfos.Length; j++)
                {
                    if ((objectInfo.cache.memberAttributeInfos[j] != null) && objectInfo.cache.memberAttributeInfos[j].IsXmlAttribute())
                    {
                        this.WriteMemberSetup(objectInfo, memberNameInfo, typeNameInfo, memberNames[j], memberTypes[j], memberData[j], memberObjectInfos[j], true);
                    }
                }
            }
            if (memberNameInfo != null)
            {
                memberNameInfo.NIobjectId = objectInfo.objectId;
                this.serWriter.WriteObject(memberNameInfo, typeNameInfo, length, memberNames, memberTypes, memberObjectInfos);
            }
            else if ((objectInfo.objectId == this.topId) && (this.topName != null))
            {
                nameInfo = this.MemberToNameInfo(this.topName);
                nameInfo.NIobjectId = objectInfo.objectId;
                this.serWriter.WriteObject(nameInfo, typeNameInfo, length, memberNames, memberTypes, memberObjectInfos);
            }
            else if (objectInfo.objectType != Converter.typeofString)
            {
                typeNameInfo.NIobjectId = objectInfo.objectId;
                this.serWriter.WriteObject(typeNameInfo, null, length, memberNames, memberTypes, memberObjectInfos);
            }
            if (memberNameInfo.NIisParentTypeOnObject)
            {
                memberNameInfo.NItransmitTypeOnObject = true;
                memberNameInfo.NIisParentTypeOnObject = false;
            }
            else
            {
                memberNameInfo.NItransmitTypeOnObject = false;
            }
            for (int i = 0; i < length; i++)
            {
                if (((objectInfo.cache.memberAttributeInfos == null) || (objectInfo.cache.memberAttributeInfos[i] == null)) || !objectInfo.cache.memberAttributeInfos[i].IsXmlAttribute())
                {
                    this.WriteMemberSetup(objectInfo, memberNameInfo, typeNameInfo, memberNames[i], memberTypes[i], memberData[i], memberObjectInfos[i], false);
                }
            }
            if (memberNameInfo != null)
            {
                memberNameInfo.NIobjectId = objectInfo.objectId;
                this.serWriter.WriteObjectEnd(memberNameInfo, typeNameInfo);
            }
            else if ((objectInfo.objectId == this.topId) && (this.topName != null))
            {
                this.serWriter.WriteObjectEnd(nameInfo, typeNameInfo);
                this.PutNameInfo(nameInfo);
            }
            else if (objectInfo.objectType != Converter.typeofString)
            {
                objectInfo.GetTypeFullName();
                this.serWriter.WriteObjectEnd(typeNameInfo, typeNameInfo);
            }
        }

        private void WriteArray(WriteObjectInfo objectInfo, NameInfo memberNameInfo, WriteObjectInfo memberObjectInfo)
        {
            InternalArrayTypeE jagged;
            bool flag2;
            bool flag = false;
            if (memberNameInfo == null)
            {
                memberNameInfo = this.TypeToNameInfo(objectInfo);
                memberNameInfo.NIisTopLevelObject = true;
                flag = true;
            }
            memberNameInfo.NIisArray = true;
            long objectId = objectInfo.objectId;
            memberNameInfo.NIobjectId = objectInfo.objectId;
            Array array = (Array) objectInfo.obj;
            Type elementType = objectInfo.objectType.GetElementType();
            if (Nullable.GetUnderlyingType(elementType) != null)
            {
                throw new SerializationException(string.Format(CultureInfo.CurrentCulture, SoapUtil.GetResourceString("Serialization_SoapNoGenericsSupport"), new object[] { elementType }));
            }
            WriteObjectInfo info = WriteObjectInfo.Serialize(elementType, this.m_surrogates, this.m_context, this.serObjectInfoInit, this.m_formatterConverter, (memberObjectInfo == null) ? null : memberObjectInfo.typeAttributeInfo);
            info.assemId = this.GetAssemblyId(info);
            NameInfo arrayElemTypeNameInfo = null;
            NameInfo arrayNameInfo = this.ArrayTypeToNameInfo(objectInfo, out arrayElemTypeNameInfo);
            arrayNameInfo.NIobjectId = objectId;
            arrayNameInfo.NIisArray = true;
            arrayElemTypeNameInfo.NIobjectId = objectId;
            arrayElemTypeNameInfo.NItransmitTypeOnMember = memberNameInfo.NItransmitTypeOnMember;
            arrayElemTypeNameInfo.NItransmitTypeOnObject = memberNameInfo.NItransmitTypeOnObject;
            arrayElemTypeNameInfo.NIisParentTypeOnObject = memberNameInfo.NIisParentTypeOnObject;
            int rank = array.Rank;
            int[] lengthA = new int[rank];
            int[] lowerBoundA = new int[rank];
            int[] numArray3 = new int[rank];
            for (int i = 0; i < rank; i++)
            {
                lengthA[i] = array.GetLength(i);
                lowerBoundA[i] = array.GetLowerBound(i);
                numArray3[i] = array.GetUpperBound(i);
            }
            if (elementType.IsArray)
            {
                if (rank == 1)
                {
                    jagged = InternalArrayTypeE.Jagged;
                }
                else
                {
                    jagged = InternalArrayTypeE.Rectangular;
                }
            }
            else if (rank == 1)
            {
                jagged = InternalArrayTypeE.Single;
            }
            else
            {
                jagged = InternalArrayTypeE.Rectangular;
            }
            if (((elementType == Converter.typeofByte) && (rank == 1)) && (lowerBoundA[0] == 0))
            {
                this.serWriter.WriteObjectByteArray(memberNameInfo, arrayNameInfo, info, arrayElemTypeNameInfo, lengthA[0], lowerBoundA[0], (byte[]) array);
                return;
            }
            if (elementType == Converter.typeofObject)
            {
                memberNameInfo.NItransmitTypeOnMember = true;
                arrayElemTypeNameInfo.NItransmitTypeOnMember = true;
            }
            if (this.CheckTypeFormat(this.formatterEnums.FEtypeFormat, FormatterTypeStyle.TypesAlways))
            {
                memberNameInfo.NItransmitTypeOnObject = true;
                arrayElemTypeNameInfo.NItransmitTypeOnObject = true;
            }
            switch (jagged)
            {
                case InternalArrayTypeE.Single:
                    arrayNameInfo.NIname = string.Concat(new object[] { arrayElemTypeNameInfo.NIname, "[", lengthA[0], "]" });
                    this.serWriter.WriteSingleArray(memberNameInfo, arrayNameInfo, info, arrayElemTypeNameInfo, lengthA[0], lowerBoundA[0], array);
                    if (Converter.IsWriteAsByteArray(arrayElemTypeNameInfo.NIprimitiveTypeEnum) && (lowerBoundA[0] == 0))
                    {
                        arrayElemTypeNameInfo.NIobjectId = 0L;
                        if (this.primitiveArray == null)
                        {
                            this.primitiveArray = new PrimitiveArray(arrayElemTypeNameInfo.NIprimitiveTypeEnum, array);
                        }
                        else
                        {
                            this.primitiveArray.Init(arrayElemTypeNameInfo.NIprimitiveTypeEnum, array);
                        }
                        int num4 = numArray3[0] + 1;
                        for (int j = lowerBoundA[0]; j < num4; j++)
                        {
                            this.serWriter.WriteItemString(arrayElemTypeNameInfo, arrayElemTypeNameInfo, this.primitiveArray.GetValue(j));
                        }
                    }
                    else
                    {
                        object[] objArray = null;
                        if (!elementType.IsValueType)
                        {
                            objArray = (object[]) array;
                        }
                        int num6 = numArray3[0] + 1;
                        if (objArray != null)
                        {
                            int num7 = lowerBoundA[0] - 1;
                            for (int m = lowerBoundA[0]; m < num6; m++)
                            {
                                if (objArray[m] != null)
                                {
                                    num7 = m;
                                }
                            }
                            num6 = num7 + 1;
                        }
                        for (int k = lowerBoundA[0]; k < num6; k++)
                        {
                            if (objArray == null)
                            {
                                this.WriteArrayMember(objectInfo, arrayElemTypeNameInfo, array.GetValue(k));
                            }
                            else
                            {
                                this.WriteArrayMember(objectInfo, arrayElemTypeNameInfo, objArray[k]);
                            }
                        }
                    }
                    goto Label_053D;

                case InternalArrayTypeE.Jagged:
                {
                    int index = arrayNameInfo.NIname.IndexOf('[');
                    if (index < 0)
                    {
                        throw new SerializationException(string.Format(CultureInfo.CurrentCulture, SoapUtil.GetResourceString("Serialization_Dimensions"), new object[] { arrayElemTypeNameInfo.NIname }));
                    }
                    arrayNameInfo.NIname.Substring(index);
                    arrayNameInfo.NIname = string.Concat(new object[] { arrayElemTypeNameInfo.NIname, "[", lengthA[0], "]" });
                    arrayNameInfo.NIobjectId = objectId;
                    this.serWriter.WriteJaggedArray(memberNameInfo, arrayNameInfo, info, arrayElemTypeNameInfo, lengthA[0], lowerBoundA[0]);
                    object[] objArray2 = (object[]) array;
                    for (int n = lowerBoundA[0]; n < (numArray3[0] + 1); n++)
                    {
                        this.WriteArrayMember(objectInfo, arrayElemTypeNameInfo, objArray2[n]);
                    }
                    goto Label_053D;
                }
                default:
                {
                    arrayNameInfo.NIname.IndexOf('[');
                    StringBuilder builder = new StringBuilder(10);
                    builder.Append(arrayElemTypeNameInfo.NIname);
                    builder.Append('[');
                    for (int num12 = 0; num12 < rank; num12++)
                    {
                        builder.Append(lengthA[num12]);
                        if (num12 < (rank - 1))
                        {
                            builder.Append(',');
                        }
                    }
                    builder.Append(']');
                    arrayNameInfo.NIname = builder.ToString();
                    arrayNameInfo.NIobjectId = objectId;
                    this.serWriter.WriteRectangleArray(memberNameInfo, arrayNameInfo, info, arrayElemTypeNameInfo, rank, lengthA, lowerBoundA);
                    flag2 = false;
                    for (int num13 = 0; num13 < rank; num13++)
                    {
                        if (lengthA[num13] == 0)
                        {
                            flag2 = true;
                            break;
                        }
                    }
                    break;
                }
            }
            if (!flag2)
            {
                this.WriteRectangle(objectInfo, rank, lengthA, array, arrayElemTypeNameInfo, lowerBoundA);
            }
        Label_053D:
            this.serWriter.WriteObjectEnd(memberNameInfo, arrayNameInfo);
            this.PutNameInfo(arrayElemTypeNameInfo);
            this.PutNameInfo(arrayNameInfo);
            if (flag)
            {
                this.PutNameInfo(memberNameInfo);
            }
        }

        private void WriteArrayMember(WriteObjectInfo objectInfo, NameInfo arrayElemTypeNameInfo, object data)
        {
            arrayElemTypeNameInfo.NIisArrayItem = true;
            if (!this.CheckForNull(objectInfo, arrayElemTypeNameInfo, arrayElemTypeNameInfo, data))
            {
                NameInfo typeNameInfo = null;
                Type type = null;
                bool flag = false;
                if (arrayElemTypeNameInfo.NItransmitTypeOnMember)
                {
                    flag = true;
                }
                if (!flag && !arrayElemTypeNameInfo.NIisSealed)
                {
                    type = this.GetType(data);
                    if (arrayElemTypeNameInfo.NItype != type)
                    {
                        flag = true;
                    }
                }
                if (flag)
                {
                    if (type == null)
                    {
                        type = this.GetType(data);
                    }
                    typeNameInfo = this.TypeToNameInfo(type);
                    typeNameInfo.NItransmitTypeOnMember = true;
                    typeNameInfo.NIobjectId = arrayElemTypeNameInfo.NIobjectId;
                    typeNameInfo.NIassemId = arrayElemTypeNameInfo.NIassemId;
                    typeNameInfo.NIisArrayItem = true;
                    typeNameInfo.NIitemName = arrayElemTypeNameInfo.NIitemName;
                }
                else
                {
                    typeNameInfo = arrayElemTypeNameInfo;
                    typeNameInfo.NIisArrayItem = true;
                }
                if (!this.WriteKnownValueClass(arrayElemTypeNameInfo, typeNameInfo, data, false))
                {
                    object obj2 = data;
                    if (typeNameInfo.NItype.IsEnum)
                    {
                        WriteObjectInfo info2 = WriteObjectInfo.Serialize(obj2, this.m_surrogates, this.m_context, this.serObjectInfoInit, this.m_formatterConverter, null, this);
                        typeNameInfo.NIassemId = this.GetAssemblyId(info2);
                        this.WriteEnum(arrayElemTypeNameInfo, typeNameInfo, data, false);
                    }
                    else
                    {
                        long num = this.Schedule(obj2, typeNameInfo.NItype);
                        arrayElemTypeNameInfo.NIobjectId = num;
                        typeNameInfo.NIobjectId = num;
                        if (num < 1L)
                        {
                            WriteObjectInfo info3 = WriteObjectInfo.Serialize(obj2, this.m_surrogates, this.m_context, this.serObjectInfoInit, this.m_formatterConverter, null, this);
                            info3.objectId = num;
                            info3.assemId = this.GetAssemblyId(info3);
                            if (type == null)
                            {
                                type = this.GetType(data);
                            }
                            if ((data != null) && type.IsArray)
                            {
                                this.WriteArray(info3, typeNameInfo, null);
                            }
                            else
                            {
                                typeNameInfo.NIisNestedObject = true;
                                NameInfo info4 = this.TypeToNameInfo(info3);
                                info4.NIobjectId = num;
                                info3.objectId = num;
                                this.Write(info3, typeNameInfo, info4);
                            }
                            info3.ObjectEnd();
                        }
                        else
                        {
                            this.serWriter.WriteItemObjectRef(arrayElemTypeNameInfo, (int) num);
                        }
                    }
                }
                if (arrayElemTypeNameInfo.NItransmitTypeOnMember)
                {
                    this.PutNameInfo(typeNameInfo);
                }
            }
        }

        private void WriteEnum(NameInfo memberNameInfo, NameInfo typeNameInfo, object data, bool isAttribute)
        {
            if (isAttribute)
            {
                this.serWriter.WriteAttributeValue(memberNameInfo, typeNameInfo, ((Enum) data).ToString());
            }
            else
            {
                this.serWriter.WriteMember(memberNameInfo, typeNameInfo, ((Enum) data).ToString());
            }
        }

        private bool WriteKnownValueClass(NameInfo memberNameInfo, NameInfo typeNameInfo, object data, bool isAttribute)
        {
            if (typeNameInfo.NItype == Converter.typeofString)
            {
                if (isAttribute)
                {
                    this.serWriter.WriteAttributeValue(memberNameInfo, typeNameInfo, (string) data);
                }
                else
                {
                    this.WriteString(memberNameInfo, typeNameInfo, data);
                }
            }
            else
            {
                if (typeNameInfo.NIprimitiveTypeEnum == InternalPrimitiveTypeE.Invalid)
                {
                    return false;
                }
                if (typeNameInfo.NIisArray)
                {
                    this.serWriter.WriteItem(memberNameInfo, typeNameInfo, data);
                }
                else if (isAttribute)
                {
                    this.serWriter.WriteAttributeValue(memberNameInfo, typeNameInfo, data);
                }
                else
                {
                    this.serWriter.WriteMember(memberNameInfo, typeNameInfo, data);
                }
            }
            return true;
        }

        private void WriteMembers(NameInfo memberNameInfo, NameInfo memberTypeNameInfo, object memberData, WriteObjectInfo objectInfo, NameInfo typeNameInfo, WriteObjectInfo memberObjectInfo, bool isAttribute)
        {
            Type nItype = memberNameInfo.NItype;
            if ((nItype == Converter.typeofObject) || ((nItype.IsValueType && objectInfo.isSi) && Converter.IsSiTransmitType(memberTypeNameInfo.NIprimitiveTypeEnum)))
            {
                memberTypeNameInfo.NItransmitTypeOnMember = true;
                memberNameInfo.NItransmitTypeOnMember = true;
            }
            if (this.CheckTypeFormat(this.formatterEnums.FEtypeFormat, FormatterTypeStyle.TypesAlways))
            {
                memberTypeNameInfo.NItransmitTypeOnObject = true;
                memberNameInfo.NItransmitTypeOnObject = true;
                memberNameInfo.NIisParentTypeOnObject = true;
            }
            if (!this.CheckForNull(objectInfo, memberNameInfo, memberTypeNameInfo, memberData))
            {
                object proxy = memberData;
                Type type = null;
                if (memberTypeNameInfo.NIprimitiveTypeEnum == InternalPrimitiveTypeE.Invalid)
                {
                    if (RemotingServices.IsTransparentProxy(proxy))
                    {
                        type = Converter.typeofMarshalByRefObject;
                    }
                    else
                    {
                        type = this.GetType(proxy);
                        if (nItype != type)
                        {
                            memberTypeNameInfo.NItransmitTypeOnMember = true;
                            memberNameInfo.NItransmitTypeOnMember = true;
                        }
                    }
                }
                if (nItype == Converter.typeofObject)
                {
                    nItype = this.GetType(memberData);
                    if (memberObjectInfo == null)
                    {
                        this.TypeToNameInfo(nItype, memberTypeNameInfo);
                    }
                    else
                    {
                        this.TypeToNameInfo(memberObjectInfo, memberTypeNameInfo);
                    }
                }
                if ((memberObjectInfo != null) && memberObjectInfo.isArray)
                {
                    long objectId = 0L;
                    if (!objectInfo.IsEmbeddedAttribute(memberNameInfo.NIname) && !this.IsEmbeddedAttribute(nItype))
                    {
                        objectId = this.Schedule(proxy, type, memberObjectInfo);
                    }
                    if (objectId > 0L)
                    {
                        memberNameInfo.NIobjectId = objectId;
                        this.WriteObjectRef(memberNameInfo, memberTypeNameInfo, objectId);
                    }
                    else
                    {
                        this.serWriter.WriteMemberNested(memberNameInfo);
                        memberObjectInfo.objectId = objectId;
                        memberNameInfo.NIobjectId = objectId;
                        memberNameInfo.NIisNestedObject = true;
                        this.WriteArray(memberObjectInfo, memberNameInfo, memberObjectInfo);
                    }
                }
                else if (!this.WriteKnownValueClass(memberNameInfo, memberTypeNameInfo, memberData, isAttribute))
                {
                    if (memberTypeNameInfo.NItype.IsEnum)
                    {
                        this.WriteEnum(memberNameInfo, memberTypeNameInfo, memberData, isAttribute);
                    }
                    else
                    {
                        if (isAttribute)
                        {
                            throw new SerializationException(string.Format(CultureInfo.CurrentCulture, SoapUtil.GetResourceString("Serialization_NonPrimitive_XmlAttribute"), new object[] { memberNameInfo.NIname }));
                        }
                        if ((nItype.IsValueType || objectInfo.IsEmbeddedAttribute(memberNameInfo.NIname)) || this.IsEmbeddedAttribute(type))
                        {
                            this.serWriter.WriteMemberNested(memberNameInfo);
                            memberObjectInfo.objectId = -1L;
                            NameInfo info = this.TypeToNameInfo(memberObjectInfo);
                            info.NIobjectId = -1L;
                            memberNameInfo.NIisNestedObject = true;
                            if (objectInfo.isSi)
                            {
                                memberTypeNameInfo.NItransmitTypeOnMember = true;
                                memberNameInfo.NItransmitTypeOnMember = true;
                            }
                            this.Write(memberObjectInfo, memberNameInfo, info);
                            this.PutNameInfo(info);
                            memberObjectInfo.ObjectEnd();
                        }
                        else
                        {
                            long num2 = 0L;
                            num2 = this.Schedule(proxy, type, memberObjectInfo);
                            if (num2 < 0L)
                            {
                                this.serWriter.WriteMemberNested(memberNameInfo);
                                memberObjectInfo.objectId = -1L;
                                NameInfo info2 = this.TypeToNameInfo(memberObjectInfo);
                                info2.NIobjectId = -1L;
                                memberNameInfo.NIisNestedObject = true;
                                this.Write(memberObjectInfo, memberNameInfo, info2);
                                this.PutNameInfo(info2);
                                memberObjectInfo.ObjectEnd();
                            }
                            else
                            {
                                memberNameInfo.NIobjectId = num2;
                                this.WriteObjectRef(memberNameInfo, memberTypeNameInfo, num2);
                            }
                        }
                    }
                }
            }
        }

        private void WriteMemberSetup(WriteObjectInfo objectInfo, NameInfo memberNameInfo, NameInfo typeNameInfo, string memberName, Type memberType, object memberData, WriteObjectInfo memberObjectInfo, bool isAttribute)
        {
            NameInfo info = this.MemberToNameInfo(memberName);
            if (memberObjectInfo != null)
            {
                info.NIassemId = memberObjectInfo.assemId;
            }
            info.NItype = memberType;
            NameInfo memberTypeNameInfo = null;
            if (memberObjectInfo == null)
            {
                memberTypeNameInfo = this.TypeToNameInfo(memberType);
            }
            else
            {
                memberTypeNameInfo = this.TypeToNameInfo(memberObjectInfo);
            }
            info.NIisRemoteRecord = typeNameInfo.NIisRemoteRecord;
            info.NItransmitTypeOnObject = memberNameInfo.NItransmitTypeOnObject;
            info.NIisParentTypeOnObject = memberNameInfo.NIisParentTypeOnObject;
            this.WriteMembers(info, memberTypeNameInfo, memberData, objectInfo, typeNameInfo, memberObjectInfo, isAttribute);
            this.PutNameInfo(info);
            this.PutNameInfo(memberTypeNameInfo);
        }

        private void WriteObjectRef(NameInfo nameInfo, NameInfo typeNameInfo, long objectId)
        {
            this.serWriter.WriteMemberObjectRef(nameInfo, typeNameInfo, (int) objectId);
        }

        private void WriteRectangle(WriteObjectInfo objectInfo, int rank, int[] maxA, Array array, NameInfo arrayElemNameTypeInfo, int[] lowerBoundA)
        {
            int[] indices = new int[rank];
            int[] numArray2 = null;
            bool flag = false;
            if (lowerBoundA != null)
            {
                for (int i = 0; i < rank; i++)
                {
                    if (lowerBoundA[i] != 0)
                    {
                        flag = true;
                    }
                }
            }
            if (flag)
            {
                numArray2 = new int[rank];
            }
            bool flag2 = true;
            while (flag2)
            {
                flag2 = false;
                if (flag)
                {
                    for (int k = 0; k < rank; k++)
                    {
                        numArray2[k] = indices[k] + lowerBoundA[k];
                    }
                    this.WriteArrayMember(objectInfo, arrayElemNameTypeInfo, array.GetValue(numArray2));
                }
                else
                {
                    this.WriteArrayMember(objectInfo, arrayElemNameTypeInfo, array.GetValue(indices));
                }
                for (int j = rank - 1; j > -1; j--)
                {
                    if (indices[j] < (maxA[j] - 1))
                    {
                        indices[j]++;
                        if (j < (rank - 1))
                        {
                            for (int m = j + 1; m < rank; m++)
                            {
                                indices[m] = 0;
                            }
                        }
                        flag2 = true;
                        continue;
                    }
                }
            }
        }

        private void WriteSerializedStreamHeader(long topId, long headerId)
        {
            this.serWriter.WriteSerializationHeader((int) topId, (int) headerId, 1, 0);
        }

        private void WriteString(NameInfo memberNameInfo, NameInfo typeNameInfo, object stringObject)
        {
            bool isNew = true;
            long objectId = -1L;
            if (!this.CheckTypeFormat(this.formatterEnums.FEtypeFormat, FormatterTypeStyle.XsdString))
            {
                objectId = this.InternalGetId(stringObject, typeNameInfo.NItype, out isNew);
            }
            typeNameInfo.NIobjectId = objectId;
            if (isNew || (objectId < 0L))
            {
                if (typeNameInfo.NIisArray)
                {
                    this.serWriter.WriteItemString(memberNameInfo, typeNameInfo, (string) stringObject);
                }
                else
                {
                    this.serWriter.WriteMemberString(memberNameInfo, typeNameInfo, (string) stringObject);
                }
            }
            else
            {
                this.WriteObjectRef(memberNameInfo, typeNameInfo, objectId);
            }
        }

        internal SerializationObjectManager ObjectManager
        {
            get
            {
                return this.m_objectManager;
            }
        }
    }
}

