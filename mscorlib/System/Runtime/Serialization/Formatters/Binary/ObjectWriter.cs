namespace System.Runtime.Serialization.Formatters.Binary
{
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Messaging;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters;
    using System.Security;
    using System.Text;

    internal sealed class ObjectWriter
    {
        private Hashtable assemblyToIdTable;
        internal object[] crossAppDomainArray;
        private InternalFE formatterEnums;
        private Header[] headers;
        internal ArrayList internalCrossAppDomainArray;
        private SerializationBinder m_binder;
        private StreamingContext m_context;
        private int m_currentId = 1;
        private IFormatterConverter m_formatterConverter;
        private ObjectIDGenerator m_idGenerator;
        private SerializationObjectManager m_objectManager;
        private Queue m_objectQueue;
        private ISurrogateSelector m_surrogates;
        private SerStack niPool = new SerStack("NameInfo Pool");
        private InternalPrimitiveTypeE previousCode;
        private long previousId;
        private object previousObj;
        private Type previousType;
        private SerObjectInfoInit serObjectInfoInit;
        private __BinaryWriter serWriter;
        private long topId;
        private string topName;

        internal ObjectWriter(ISurrogateSelector selector, StreamingContext context, InternalFE formatterEnums, SerializationBinder binder)
        {
            this.m_surrogates = selector;
            this.m_context = context;
            this.m_binder = binder;
            this.formatterEnums = formatterEnums;
            this.m_objectManager = new SerializationObjectManager(context);
        }

        private bool CheckForNull(WriteObjectInfo objectInfo, NameInfo memberNameInfo, NameInfo typeNameInfo, object data)
        {
            bool flag = false;
            if (data == null)
            {
                flag = true;
            }
            if (flag && ((((this.formatterEnums.FEserializerTypeEnum == InternalSerializerTypeE.Binary) || memberNameInfo.NIisArrayItem) || (memberNameInfo.NItransmitTypeOnObject || memberNameInfo.NItransmitTypeOnMember)) || (objectInfo.isSi || this.CheckTypeFormat(this.formatterEnums.FEtypeFormat, FormatterTypeStyle.TypesAlways))))
            {
                if (typeNameInfo.NIisArrayItem)
                {
                    if (typeNameInfo.NIarrayEnum == InternalArrayTypeE.Single)
                    {
                        this.serWriter.WriteDelayedNullItem();
                        return flag;
                    }
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
            if (this.assemblyToIdTable == null)
            {
                this.assemblyToIdTable = new Hashtable(5);
            }
            long num = 0L;
            bool isNew = false;
            string assemblyString = objectInfo.GetAssemblyString();
            string str2 = assemblyString;
            if (assemblyString.Length == 0)
            {
                return 0L;
            }
            if (assemblyString.Equals(Converter.urtAssemblyString))
            {
                return 0L;
            }
            if (this.assemblyToIdTable.ContainsKey(assemblyString))
            {
                num = (long) this.assemblyToIdTable[assemblyString];
                isNew = false;
            }
            else
            {
                num = this.InternalGetId("___AssemblyString___" + assemblyString, false, null, out isNew);
                this.assemblyToIdTable[assemblyString] = num;
            }
            this.serWriter.WriteAssembly(objectInfo.objectType, str2, (int) num, isNew);
            return num;
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
                throw new SerializationException(Environment.GetResourceString("Serialization_ObjNoID", new object[] { obj3 }));
            }
            return obj2;
        }

        [SecurityCritical]
        private Type GetType(object obj)
        {
            if (RemotingServices.IsTransparentProxy(obj))
            {
                return Converter.typeofMarshalByRefObject;
            }
            return obj.GetType();
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

        private long InternalGetId(object obj, bool assignUniqueIdToValueType, Type type, out bool isNew)
        {
            if (obj == this.previousObj)
            {
                isNew = false;
                return this.previousId;
            }
            this.m_idGenerator.m_currentCount = this.m_currentId;
            if (((type != null) && type.IsValueType) && !assignUniqueIdToValueType)
            {
                isNew = false;
                return (long) (-1 * this.m_currentId++);
            }
            this.m_currentId++;
            long id = this.m_idGenerator.GetId(obj, out isNew);
            this.previousObj = obj;
            this.previousId = id;
            return id;
        }

        private NameInfo MemberToNameInfo(string name)
        {
            NameInfo nameInfo = this.GetNameInfo();
            nameInfo.NIname = name;
            return nameInfo;
        }

        private void PutNameInfo(NameInfo nameInfo)
        {
            this.niPool.Push(nameInfo);
        }

        private long Schedule(object obj, bool assignUniqueIdToValueType, Type type)
        {
            return this.Schedule(obj, assignUniqueIdToValueType, type, null);
        }

        private long Schedule(object obj, bool assignUniqueIdToValueType, Type type, WriteObjectInfo objectInfo)
        {
            bool flag;
            if (obj == null)
            {
                return 0L;
            }
            long num = this.InternalGetId(obj, assignUniqueIdToValueType, type, out flag);
            if (flag && (num > 0L))
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

        [SecurityCritical]
        internal void Serialize(object graph, Header[] inHeaders, __BinaryWriter serWriter, bool fCheck)
        {
            if (graph == null)
            {
                throw new ArgumentNullException("graph", Environment.GetResourceString("ArgumentNull_Graph"));
            }
            if (serWriter == null)
            {
                throw new ArgumentNullException("serWriter", Environment.GetResourceString("ArgumentNull_WithParamName", new object[] { "serWriter" }));
            }
            if (fCheck)
            {
                CodeAccessPermission.Demand(PermissionType.SecuritySerialization);
            }
            this.serWriter = serWriter;
            this.headers = inHeaders;
            serWriter.WriteBegin();
            long headerId = 0L;
            bool flag2 = false;
            bool flag3 = false;
            IMethodCallMessage mcm = graph as IMethodCallMessage;
            if (mcm != null)
            {
                flag2 = true;
                graph = this.WriteMethodCall(mcm);
            }
            else
            {
                IMethodReturnMessage mrm = graph as IMethodReturnMessage;
                if (mrm != null)
                {
                    flag3 = true;
                    graph = this.WriteMethodReturn(mrm);
                }
            }
            if (graph == null)
            {
                this.WriteSerializedStreamHeader(this.topId, headerId);
                if (flag2)
                {
                    serWriter.WriteMethodCall();
                }
                else if (flag3)
                {
                    serWriter.WriteMethodReturn();
                }
                serWriter.WriteSerializationHeaderEnd();
                serWriter.WriteEnd();
            }
            else
            {
                object obj2;
                long num2;
                bool flag;
                this.m_idGenerator = new ObjectIDGenerator();
                this.m_objectQueue = new Queue();
                this.m_formatterConverter = new FormatterConverter();
                this.serObjectInfoInit = new SerObjectInfoInit();
                this.topId = this.InternalGetId(graph, false, null, out flag);
                if (this.headers != null)
                {
                    headerId = this.InternalGetId(this.headers, false, null, out flag);
                }
                else
                {
                    headerId = -1L;
                }
                this.WriteSerializedStreamHeader(this.topId, headerId);
                if (flag2)
                {
                    serWriter.WriteMethodCall();
                }
                else if (flag3)
                {
                    serWriter.WriteMethodReturn();
                }
                if ((this.headers != null) && (this.headers.Length > 0))
                {
                    this.m_objectQueue.Enqueue(this.headers);
                }
                if (graph != null)
                {
                    this.m_objectQueue.Enqueue(graph);
                }
                while ((obj2 = this.GetNext(out num2)) != null)
                {
                    WriteObjectInfo objectInfo = null;
                    if (obj2 is WriteObjectInfo)
                    {
                        objectInfo = (WriteObjectInfo) obj2;
                    }
                    else
                    {
                        objectInfo = WriteObjectInfo.Serialize(obj2, this.m_surrogates, this.m_context, this.serObjectInfoInit, this.m_formatterConverter, this, this.m_binder);
                        objectInfo.assemId = this.GetAssemblyId(objectInfo);
                    }
                    objectInfo.objectId = num2;
                    NameInfo memberNameInfo = this.TypeToNameInfo(objectInfo);
                    this.Write(objectInfo, memberNameInfo, memberNameInfo);
                    this.PutNameInfo(memberNameInfo);
                    objectInfo.ObjectEnd();
                }
                serWriter.WriteSerializationHeaderEnd();
                serWriter.WriteEnd();
                this.m_objectManager.RaiseOnSerializedEvent();
            }
        }

        [SecurityCritical]
        private static object[] StoreUserPropertiesForMethodMessage(IMethodMessage msg)
        {
            ArrayList list = null;
            IDictionary properties = msg.Properties;
            if (properties != null)
            {
                MessageDictionary dictionary2 = properties as MessageDictionary;
                if (dictionary2 != null)
                {
                    if (!dictionary2.HasUserData())
                    {
                        return null;
                    }
                    int num = 0;
                    foreach (DictionaryEntry entry in dictionary2.InternalDictionary)
                    {
                        if (list == null)
                        {
                            list = new ArrayList();
                        }
                        list.Add(entry);
                        num++;
                    }
                    return list.ToArray();
                }
                int num2 = 0;
                foreach (DictionaryEntry entry2 in properties)
                {
                    if (list == null)
                    {
                        list = new ArrayList();
                    }
                    list.Add(entry2);
                    num2++;
                }
                if (list != null)
                {
                    return list.ToArray();
                }
            }
            return null;
        }

        internal InternalPrimitiveTypeE ToCode(Type type)
        {
            if (object.ReferenceEquals(this.previousType, type))
            {
                return this.previousCode;
            }
            InternalPrimitiveTypeE ee = Converter.ToCode(type);
            if (ee != InternalPrimitiveTypeE.Invalid)
            {
                this.previousType = type;
                this.previousCode = ee;
            }
            return ee;
        }

        private NameInfo TypeToNameInfo(WriteObjectInfo objectInfo)
        {
            return this.TypeToNameInfo(objectInfo.objectType, objectInfo, this.ToCode(objectInfo.objectType), null);
        }

        private NameInfo TypeToNameInfo(Type type)
        {
            return this.TypeToNameInfo(type, null, this.ToCode(type), null);
        }

        private NameInfo TypeToNameInfo(WriteObjectInfo objectInfo, NameInfo nameInfo)
        {
            return this.TypeToNameInfo(objectInfo.objectType, objectInfo, this.ToCode(objectInfo.objectType), nameInfo);
        }

        private void TypeToNameInfo(Type type, NameInfo nameInfo)
        {
            this.TypeToNameInfo(type, null, this.ToCode(type), nameInfo);
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
            if ((code == InternalPrimitiveTypeE.Invalid) && (objectInfo != null))
            {
                nameInfo.NIname = objectInfo.GetTypeFullName();
                nameInfo.NIassemId = objectInfo.assemId;
            }
            nameInfo.NIprimitiveTypeEnum = code;
            nameInfo.NItype = type;
            return nameInfo;
        }

        [SecurityCritical]
        private void Write(WriteObjectInfo objectInfo, NameInfo memberNameInfo, NameInfo typeNameInfo)
        {
            object obj2 = objectInfo.obj;
            if (obj2 == null)
            {
                throw new ArgumentNullException("objectInfo.obj", Environment.GetResourceString("ArgumentNull_Obj"));
            }
            Type objectType = objectInfo.objectType;
            long objectId = objectInfo.objectId;
            if (object.ReferenceEquals(objectType, Converter.typeofString))
            {
                memberNameInfo.NIobjectId = objectId;
                this.serWriter.WriteObjectString((int) objectId, obj2.ToString());
            }
            else if (objectInfo.isArray)
            {
                this.WriteArray(objectInfo, memberNameInfo, null);
            }
            else
            {
                string[] strArray;
                Type[] typeArray;
                object[] objArray;
                objectInfo.GetMemberInfo(out strArray, out typeArray, out objArray);
                if (objectInfo.isSi || this.CheckTypeFormat(this.formatterEnums.FEtypeFormat, FormatterTypeStyle.TypesAlways))
                {
                    memberNameInfo.NItransmitTypeOnObject = true;
                    memberNameInfo.NIisParentTypeOnObject = true;
                    typeNameInfo.NItransmitTypeOnObject = true;
                    typeNameInfo.NIisParentTypeOnObject = true;
                }
                WriteObjectInfo[] memberObjectInfos = new WriteObjectInfo[strArray.Length];
                for (int i = 0; i < typeArray.Length; i++)
                {
                    Type type;
                    if (typeArray[i] != null)
                    {
                        type = typeArray[i];
                    }
                    else if (objArray[i] != null)
                    {
                        type = this.GetType(objArray[i]);
                    }
                    else
                    {
                        type = Converter.typeofObject;
                    }
                    if ((this.ToCode(type) == InternalPrimitiveTypeE.Invalid) && !object.ReferenceEquals(type, Converter.typeofString))
                    {
                        if (objArray[i] != null)
                        {
                            memberObjectInfos[i] = WriteObjectInfo.Serialize(objArray[i], this.m_surrogates, this.m_context, this.serObjectInfoInit, this.m_formatterConverter, this, this.m_binder);
                            memberObjectInfos[i].assemId = this.GetAssemblyId(memberObjectInfos[i]);
                        }
                        else
                        {
                            memberObjectInfos[i] = WriteObjectInfo.Serialize(typeArray[i], this.m_surrogates, this.m_context, this.serObjectInfoInit, this.m_formatterConverter, this.m_binder);
                            memberObjectInfos[i].assemId = this.GetAssemblyId(memberObjectInfos[i]);
                        }
                    }
                }
                this.Write(objectInfo, memberNameInfo, typeNameInfo, strArray, typeArray, objArray, memberObjectInfos);
            }
        }

        [SecurityCritical]
        private void Write(WriteObjectInfo objectInfo, NameInfo memberNameInfo, NameInfo typeNameInfo, string[] memberNames, Type[] memberTypes, object[] memberData, WriteObjectInfo[] memberObjectInfos)
        {
            int length = memberNames.Length;
            NameInfo nameInfo = null;
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
            else if (!object.ReferenceEquals(objectInfo.objectType, Converter.typeofString))
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
                this.WriteMemberSetup(objectInfo, memberNameInfo, typeNameInfo, memberNames[i], memberTypes[i], memberData[i], memberObjectInfos[i]);
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
            else if (!object.ReferenceEquals(objectInfo.objectType, Converter.typeofString))
            {
                this.serWriter.WriteObjectEnd(typeNameInfo, typeNameInfo);
            }
        }

        [SecurityCritical]
        private void WriteArray(WriteObjectInfo objectInfo, NameInfo memberNameInfo, WriteObjectInfo memberObjectInfo)
        {
            InternalArrayTypeE jagged;
            bool flag2;
            bool flag = false;
            if (memberNameInfo == null)
            {
                memberNameInfo = this.TypeToNameInfo(objectInfo);
                flag = true;
            }
            memberNameInfo.NIisArray = true;
            long objectId = objectInfo.objectId;
            memberNameInfo.NIobjectId = objectInfo.objectId;
            Array array = (Array) objectInfo.obj;
            Type elementType = objectInfo.objectType.GetElementType();
            WriteObjectInfo info = null;
            if (!elementType.IsPrimitive)
            {
                info = WriteObjectInfo.Serialize(elementType, this.m_surrogates, this.m_context, this.serObjectInfoInit, this.m_formatterConverter, this.m_binder);
                info.assemId = this.GetAssemblyId(info);
            }
            NameInfo arrayElemTypeNameInfo = null;
            if (info == null)
            {
                arrayElemTypeNameInfo = this.TypeToNameInfo(elementType);
            }
            else
            {
                arrayElemTypeNameInfo = this.TypeToNameInfo(info);
            }
            arrayElemTypeNameInfo.NIisArray = arrayElemTypeNameInfo.NItype.IsArray;
            NameInfo arrayNameInfo = memberNameInfo;
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
            if (arrayElemTypeNameInfo.NIisArray)
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
            arrayElemTypeNameInfo.NIarrayEnum = jagged;
            if ((object.ReferenceEquals(elementType, Converter.typeofByte) && (rank == 1)) && (lowerBoundA[0] == 0))
            {
                this.serWriter.WriteObjectByteArray(memberNameInfo, arrayNameInfo, info, arrayElemTypeNameInfo, lengthA[0], lowerBoundA[0], (byte[]) array);
                return;
            }
            if (object.ReferenceEquals(elementType, Converter.typeofObject) || (Nullable.GetUnderlyingType(elementType) != null))
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
                    this.serWriter.WriteSingleArray(memberNameInfo, arrayNameInfo, info, arrayElemTypeNameInfo, lengthA[0], lowerBoundA[0], array);
                    if (!Converter.IsWriteAsByteArray(arrayElemTypeNameInfo.NIprimitiveTypeEnum) || (lowerBoundA[0] != 0))
                    {
                        object[] objArray = null;
                        if (!elementType.IsValueType)
                        {
                            objArray = (object[]) array;
                        }
                        int num4 = numArray3[0] + 1;
                        for (int j = lowerBoundA[0]; j < num4; j++)
                        {
                            if (objArray == null)
                            {
                                this.WriteArrayMember(objectInfo, arrayElemTypeNameInfo, array.GetValue(j));
                            }
                            else
                            {
                                this.WriteArrayMember(objectInfo, arrayElemTypeNameInfo, objArray[j]);
                            }
                        }
                        this.serWriter.WriteItemEnd();
                    }
                    goto Label_0365;

                case InternalArrayTypeE.Jagged:
                {
                    arrayNameInfo.NIobjectId = objectId;
                    this.serWriter.WriteJaggedArray(memberNameInfo, arrayNameInfo, info, arrayElemTypeNameInfo, lengthA[0], lowerBoundA[0]);
                    object[] objArray2 = (object[]) array;
                    for (int k = lowerBoundA[0]; k < (numArray3[0] + 1); k++)
                    {
                        this.WriteArrayMember(objectInfo, arrayElemTypeNameInfo, objArray2[k]);
                    }
                    this.serWriter.WriteItemEnd();
                    goto Label_0365;
                }
                default:
                    arrayNameInfo.NIobjectId = objectId;
                    this.serWriter.WriteRectangleArray(memberNameInfo, arrayNameInfo, info, arrayElemTypeNameInfo, rank, lengthA, lowerBoundA);
                    flag2 = false;
                    for (int m = 0; m < rank; m++)
                    {
                        if (lengthA[m] == 0)
                        {
                            flag2 = true;
                            break;
                        }
                    }
                    break;
            }
            if (!flag2)
            {
                this.WriteRectangle(objectInfo, rank, lengthA, array, arrayElemTypeNameInfo, lowerBoundA);
            }
            this.serWriter.WriteItemEnd();
        Label_0365:
            this.serWriter.WriteObjectEnd(memberNameInfo, arrayNameInfo);
            this.PutNameInfo(arrayElemTypeNameInfo);
            if (flag)
            {
                this.PutNameInfo(memberNameInfo);
            }
        }

        [SecurityCritical]
        private void WriteArrayMember(WriteObjectInfo objectInfo, NameInfo arrayElemTypeNameInfo, object data)
        {
            arrayElemTypeNameInfo.NIisArrayItem = true;
            if (!this.CheckForNull(objectInfo, arrayElemTypeNameInfo, arrayElemTypeNameInfo, data))
            {
                NameInfo typeNameInfo = null;
                Type objB = null;
                bool flag = false;
                if (arrayElemTypeNameInfo.NItransmitTypeOnMember)
                {
                    flag = true;
                }
                if (!flag && !arrayElemTypeNameInfo.IsSealed)
                {
                    objB = this.GetType(data);
                    if (!object.ReferenceEquals(arrayElemTypeNameInfo.NItype, objB))
                    {
                        flag = true;
                    }
                }
                if (flag)
                {
                    if (objB == null)
                    {
                        objB = this.GetType(data);
                    }
                    typeNameInfo = this.TypeToNameInfo(objB);
                    typeNameInfo.NItransmitTypeOnMember = true;
                    typeNameInfo.NIobjectId = arrayElemTypeNameInfo.NIobjectId;
                    typeNameInfo.NIassemId = arrayElemTypeNameInfo.NIassemId;
                    typeNameInfo.NIisArrayItem = true;
                }
                else
                {
                    typeNameInfo = arrayElemTypeNameInfo;
                    typeNameInfo.NIisArrayItem = true;
                }
                if (!this.WriteKnownValueClass(arrayElemTypeNameInfo, typeNameInfo, data))
                {
                    object obj2 = data;
                    bool assignUniqueIdToValueType = false;
                    if (object.ReferenceEquals(arrayElemTypeNameInfo.NItype, Converter.typeofObject))
                    {
                        assignUniqueIdToValueType = true;
                    }
                    long num = this.Schedule(obj2, assignUniqueIdToValueType, typeNameInfo.NItype);
                    arrayElemTypeNameInfo.NIobjectId = num;
                    typeNameInfo.NIobjectId = num;
                    if (num < 1L)
                    {
                        WriteObjectInfo info2 = WriteObjectInfo.Serialize(obj2, this.m_surrogates, this.m_context, this.serObjectInfoInit, this.m_formatterConverter, this, this.m_binder);
                        info2.objectId = num;
                        if (!object.ReferenceEquals(arrayElemTypeNameInfo.NItype, Converter.typeofObject) && (Nullable.GetUnderlyingType(arrayElemTypeNameInfo.NItype) == null))
                        {
                            info2.assemId = typeNameInfo.NIassemId;
                        }
                        else
                        {
                            info2.assemId = this.GetAssemblyId(info2);
                        }
                        NameInfo info3 = this.TypeToNameInfo(info2);
                        info3.NIobjectId = num;
                        info2.objectId = num;
                        this.Write(info2, typeNameInfo, info3);
                        info2.ObjectEnd();
                    }
                    else
                    {
                        this.serWriter.WriteItemObjectRef(arrayElemTypeNameInfo, (int) num);
                    }
                }
                if (arrayElemTypeNameInfo.NItransmitTypeOnMember)
                {
                    this.PutNameInfo(typeNameInfo);
                }
            }
        }

        private bool WriteKnownValueClass(NameInfo memberNameInfo, NameInfo typeNameInfo, object data)
        {
            if (object.ReferenceEquals(typeNameInfo.NItype, Converter.typeofString))
            {
                this.WriteString(memberNameInfo, typeNameInfo, data);
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
                else
                {
                    this.serWriter.WriteMember(memberNameInfo, typeNameInfo, data);
                }
            }
            return true;
        }

        [SecurityCritical]
        private void WriteMembers(NameInfo memberNameInfo, NameInfo memberTypeNameInfo, object memberData, WriteObjectInfo objectInfo, NameInfo typeNameInfo, WriteObjectInfo memberObjectInfo)
        {
            Type nItype = memberNameInfo.NItype;
            bool assignUniqueIdToValueType = false;
            if (object.ReferenceEquals(nItype, Converter.typeofObject) || (Nullable.GetUnderlyingType(nItype) != null))
            {
                memberTypeNameInfo.NItransmitTypeOnMember = true;
                memberNameInfo.NItransmitTypeOnMember = true;
            }
            if (this.CheckTypeFormat(this.formatterEnums.FEtypeFormat, FormatterTypeStyle.TypesAlways) || objectInfo.isSi)
            {
                memberTypeNameInfo.NItransmitTypeOnObject = true;
                memberNameInfo.NItransmitTypeOnObject = true;
                memberNameInfo.NIisParentTypeOnObject = true;
            }
            if (!this.CheckForNull(objectInfo, memberNameInfo, memberTypeNameInfo, memberData))
            {
                object obj2 = memberData;
                Type objB = null;
                if (memberTypeNameInfo.NIprimitiveTypeEnum == InternalPrimitiveTypeE.Invalid)
                {
                    objB = this.GetType(obj2);
                    if (!object.ReferenceEquals(nItype, objB))
                    {
                        memberTypeNameInfo.NItransmitTypeOnMember = true;
                        memberNameInfo.NItransmitTypeOnMember = true;
                    }
                }
                if (object.ReferenceEquals(nItype, Converter.typeofObject))
                {
                    assignUniqueIdToValueType = true;
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
                    if (objB == null)
                    {
                        objB = this.GetType(obj2);
                    }
                    objectId = this.Schedule(obj2, false, null, memberObjectInfo);
                    if (objectId > 0L)
                    {
                        memberNameInfo.NIobjectId = objectId;
                        this.WriteObjectRef(memberNameInfo, objectId);
                    }
                    else
                    {
                        this.serWriter.WriteMemberNested(memberNameInfo);
                        memberObjectInfo.objectId = objectId;
                        memberNameInfo.NIobjectId = objectId;
                        this.WriteArray(memberObjectInfo, memberNameInfo, memberObjectInfo);
                        objectInfo.ObjectEnd();
                    }
                }
                else if (!this.WriteKnownValueClass(memberNameInfo, memberTypeNameInfo, memberData))
                {
                    if (objB == null)
                    {
                        objB = this.GetType(obj2);
                    }
                    long num2 = this.Schedule(obj2, assignUniqueIdToValueType, objB, memberObjectInfo);
                    if (num2 < 0L)
                    {
                        memberObjectInfo.objectId = num2;
                        NameInfo info = this.TypeToNameInfo(memberObjectInfo);
                        info.NIobjectId = num2;
                        this.Write(memberObjectInfo, memberNameInfo, info);
                        this.PutNameInfo(info);
                        memberObjectInfo.ObjectEnd();
                    }
                    else
                    {
                        memberNameInfo.NIobjectId = num2;
                        this.WriteObjectRef(memberNameInfo, num2);
                    }
                }
            }
        }

        [SecurityCritical]
        private void WriteMemberSetup(WriteObjectInfo objectInfo, NameInfo memberNameInfo, NameInfo typeNameInfo, string memberName, Type memberType, object memberData, WriteObjectInfo memberObjectInfo)
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
            info.NItransmitTypeOnObject = memberNameInfo.NItransmitTypeOnObject;
            info.NIisParentTypeOnObject = memberNameInfo.NIisParentTypeOnObject;
            this.WriteMembers(info, memberTypeNameInfo, memberData, objectInfo, typeNameInfo, memberObjectInfo);
            this.PutNameInfo(info);
            this.PutNameInfo(memberTypeNameInfo);
        }

        [SecurityCritical]
        private object[] WriteMethodCall(IMethodCallMessage mcm)
        {
            string uri = mcm.Uri;
            string methodName = mcm.MethodName;
            string typeName = mcm.TypeName;
            object methodSignature = null;
            object callContext = null;
            object[] properties = null;
            Type[] instArgs = null;
            if (mcm.MethodBase.IsGenericMethod)
            {
                instArgs = mcm.MethodBase.GetGenericArguments();
            }
            object[] args = mcm.Args;
            IInternalMessage message = mcm as IInternalMessage;
            if ((message == null) || message.HasProperties())
            {
                properties = StoreUserPropertiesForMethodMessage(mcm);
            }
            if ((mcm.MethodSignature != null) && RemotingServices.IsMethodOverloaded(mcm))
            {
                methodSignature = mcm.MethodSignature;
            }
            LogicalCallContext logicalCallContext = mcm.LogicalCallContext;
            if (logicalCallContext == null)
            {
                callContext = null;
            }
            else if (logicalCallContext.HasInfo)
            {
                callContext = logicalCallContext;
            }
            else
            {
                callContext = logicalCallContext.RemotingData.LogicalCallID;
            }
            return this.serWriter.WriteCallArray(uri, methodName, typeName, instArgs, args, methodSignature, callContext, properties);
        }

        [SecurityCritical]
        private object[] WriteMethodReturn(IMethodReturnMessage mrm)
        {
            object logicalCallID;
            object returnValue = mrm.ReturnValue;
            object[] args = mrm.Args;
            Exception exception = mrm.Exception;
            object[] properties = null;
            ReturnMessage message = mrm as ReturnMessage;
            if ((message == null) || message.HasProperties())
            {
                properties = StoreUserPropertiesForMethodMessage(mrm);
            }
            LogicalCallContext logicalCallContext = mrm.LogicalCallContext;
            if (logicalCallContext == null)
            {
                logicalCallID = null;
            }
            else if (logicalCallContext.HasInfo)
            {
                logicalCallID = logicalCallContext;
            }
            else
            {
                logicalCallID = logicalCallContext.RemotingData.LogicalCallID;
            }
            return this.serWriter.WriteReturnArray(returnValue, args, exception, logicalCallID, properties);
        }

        private void WriteObjectRef(NameInfo nameInfo, long objectId)
        {
            this.serWriter.WriteMemberObjectRef(nameInfo, (int) objectId);
        }

        [SecurityCritical]
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
                objectId = this.InternalGetId(stringObject, false, null, out isNew);
            }
            typeNameInfo.NIobjectId = objectId;
            if (isNew || (objectId < 0L))
            {
                this.serWriter.WriteMemberString(memberNameInfo, typeNameInfo, (string) stringObject);
            }
            else
            {
                this.WriteObjectRef(memberNameInfo, objectId);
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

