namespace System.Runtime.Serialization.Formatters.Soap
{
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Messaging;
    using System.Runtime.Remoting.Metadata.W3cXsd2001;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters;
    using System.Security;
    using System.Security.Permissions;
    using System.Text;

    internal sealed class ObjectReader
    {
        internal Exception deserializationSecurityException;
        internal string faultString;
        internal InternalFE formatterEnums;
        internal HeaderHandler handler;
        internal object handlerObject;
        internal Header[] headers;
        private string inKeyId;
        internal bool IsFakeTopObject;
        internal bool isHeaderHandlerCalled;
        internal bool isTopObjectResolved = true;
        internal bool isTopObjectSecondPass;
        internal SerializationBinder m_binder;
        internal StreamingContext m_context;
        internal IFormatterConverter m_formatterConverter;
        internal ObjectIDGenerator m_idGenerator;
        internal ObjectManager m_objectManager;
        internal Stream m_stream;
        internal ISurrogateSelector m_surrogates;
        internal int majorVersion;
        internal int minorVersion;
        internal Header[] newheaders;
        internal long objectIds;
        internal Hashtable objectIdTable = new Hashtable(0x19);
        private long outKeyId;
        internal int paramPosition;
        private StringBuilder sbf = new StringBuilder();
        internal static SecurityPermission serializationPermission = new SecurityPermission(SecurityPermissionFlag.SerializationFormatter);
        internal SerObjectInfoInit serObjectInfoInit;
        private static FileIOPermission sfileIOPermission = new FileIOPermission(PermissionState.Unrestricted);
        internal long soapFaultId;
        internal SoapHandler soapHandler;
        internal SerStack stack = new SerStack("ObjectReader Object Stack");
        internal long topId;
        internal object topObject;
        internal SerStack topStack;
        private NameCache typeCache = new NameCache();
        internal SerStack valueFixupStack = new SerStack("ValueType Fixup Stack");

        internal ObjectReader(Stream stream, ISurrogateSelector selector, StreamingContext context, InternalFE formatterEnums, SerializationBinder binder)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream", SoapUtil.GetResourceString("ArgumentNull_Stream"));
            }
            this.m_stream = stream;
            this.m_surrogates = selector;
            this.m_context = context;
            this.m_binder = binder;
            this.formatterEnums = formatterEnums;
            if (formatterEnums.FEtopObject != null)
            {
                this.IsFakeTopObject = true;
            }
            else
            {
                this.IsFakeTopObject = false;
            }
            this.m_formatterConverter = new FormatterConverter();
        }

        internal Type Bind(string assemblyString, string typeString)
        {
            Type type = null;
            if ((this.m_binder != null) && !this.IsInternalType(assemblyString, typeString))
            {
                type = this.m_binder.BindToType(assemblyString, typeString);
            }
            return type;
        }

        private void CheckSecurity(ParseRecord pr)
        {
            Type pRdtType = pr.PRdtType;
            if ((pRdtType != null) && this.IsRemoting)
            {
                if (typeof(MarshalByRefObject).IsAssignableFrom(pRdtType))
                {
                    throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, SoapUtil.GetResourceString("Serialization_MBRAsMBV"), new object[] { pRdtType.FullName }));
                }
                FormatterServices.CheckTypeSecurity(pRdtType, this.formatterEnums.FEsecurityLevel);
            }
            if (this.deserializationSecurityException != null)
            {
                if (pRdtType != null)
                {
                    if (pRdtType.IsPrimitive || (pRdtType == Converter.typeofString))
                    {
                        return;
                    }
                    if (typeof(Enum).IsAssignableFrom(pRdtType))
                    {
                        return;
                    }
                    if (pRdtType.IsArray)
                    {
                        Type elementType = pRdtType.GetElementType();
                        if (elementType.IsPrimitive || (elementType == Converter.typeofString))
                        {
                            return;
                        }
                    }
                }
                throw this.deserializationSecurityException;
            }
        }

        private void CheckSerializable(Type t)
        {
            if (!t.IsSerializable && !this.HasSurrogate(t))
            {
                throw new SerializationException(string.Format(CultureInfo.CurrentCulture, SoapUtil.GetResourceString("Serialization_NonSerType"), new object[] { t.FullName, t.Module.Assembly.FullName }));
            }
        }

        internal ReadObjectInfo CreateReadObjectInfo(Type objectType, string assemblyName)
        {
            ReadObjectInfo info = ReadObjectInfo.Create(objectType, this.m_surrogates, this.m_context, this.m_objectManager, this.serObjectInfoInit, this.m_formatterConverter, assemblyName);
            info.SetVersion(this.majorVersion, this.minorVersion);
            return info;
        }

        internal ReadObjectInfo CreateReadObjectInfo(Type objectType, string[] memberNames, Type[] memberTypes, string assemblyName)
        {
            ReadObjectInfo info = ReadObjectInfo.Create(objectType, memberNames, memberTypes, this.m_surrogates, this.m_context, this.m_objectManager, this.serObjectInfoInit, this.m_formatterConverter, assemblyName);
            info.SetVersion(this.majorVersion, this.minorVersion);
            return info;
        }

        internal object Deserialize(HeaderHandler handler, ISerParser serParser)
        {
            if (serParser == null)
            {
                throw new ArgumentNullException("serParser", string.Format(CultureInfo.CurrentCulture, SoapUtil.GetResourceString("ArgumentNull_WithParamName"), new object[] { serParser }));
            }
            this.deserializationSecurityException = null;
            try
            {
                serializationPermission.Demand();
            }
            catch (Exception exception)
            {
                this.deserializationSecurityException = exception;
            }
            this.handler = handler;
            this.isTopObjectSecondPass = false;
            this.isHeaderHandlerCalled = false;
            if (handler != null)
            {
                this.IsFakeTopObject = true;
            }
            this.m_idGenerator = new ObjectIDGenerator();
            this.m_objectManager = this.GetObjectManager();
            this.serObjectInfoInit = new SerObjectInfoInit();
            this.objectIdTable.Clear();
            this.objectIds = 0L;
            serParser.Run();
            if (handler != null)
            {
                this.m_objectManager.DoFixups();
                if (this.handlerObject == null)
                {
                    this.handlerObject = handler(this.newheaders);
                }
                if ((this.soapFaultId > 0L) && (this.handlerObject != null))
                {
                    this.topStack = new SerStack("Top ParseRecords");
                    ParseRecord record = new ParseRecord {
                        PRparseTypeEnum = InternalParseTypeE.Object,
                        PRobjectPositionEnum = InternalObjectPositionE.Top,
                        PRparseStateEnum = InternalParseStateE.Object,
                        PRname = "Response"
                    };
                    this.topStack.Push(record);
                    record = new ParseRecord {
                        PRparseTypeEnum = InternalParseTypeE.Member,
                        PRobjectPositionEnum = InternalObjectPositionE.Child,
                        PRmemberTypeEnum = InternalMemberTypeE.Field,
                        PRmemberValueEnum = InternalMemberValueE.Reference,
                        PRparseStateEnum = InternalParseStateE.Member,
                        PRname = "__fault",
                        PRidRef = this.soapFaultId
                    };
                    this.topStack.Push(record);
                    record = new ParseRecord {
                        PRparseTypeEnum = InternalParseTypeE.ObjectEnd,
                        PRobjectPositionEnum = InternalObjectPositionE.Top,
                        PRparseStateEnum = InternalParseStateE.Object,
                        PRname = "Response"
                    };
                    this.topStack.Push(record);
                    this.isTopObjectResolved = false;
                }
            }
            if (!this.isTopObjectResolved)
            {
                this.isTopObjectSecondPass = true;
                this.topStack.Reverse();
                int num = this.topStack.Count();
                ParseRecord pr = null;
                for (int i = 0; i < num; i++)
                {
                    pr = (ParseRecord) this.topStack.Pop();
                    this.Parse(pr);
                }
            }
            this.m_objectManager.DoFixups();
            if (this.topObject == null)
            {
                throw new SerializationException(SoapUtil.GetResourceString("Serialization_TopObject"));
            }
            if (this.HasSurrogate(this.topObject.GetType()) && (this.topId != 0L))
            {
                this.topObject = this.m_objectManager.GetObject(this.topId);
            }
            if (this.topObject is IObjectReference)
            {
                this.topObject = ((IObjectReference) this.topObject).GetRealObject(this.m_context);
            }
            this.m_objectManager.RaiseDeserializationEvent();
            if ((this.formatterEnums.FEtopObject != null) && (this.topObject is InternalSoapMessage))
            {
                InternalSoapMessage topObject = (InternalSoapMessage) this.topObject;
                ISoapMessage fEtopObject = this.formatterEnums.FEtopObject;
                fEtopObject.MethodName = topObject.methodName;
                fEtopObject.XmlNameSpace = topObject.xmlNameSpace;
                fEtopObject.ParamNames = topObject.paramNames;
                fEtopObject.ParamValues = topObject.paramValues;
                fEtopObject.Headers = this.headers;
                this.topObject = fEtopObject;
                this.isTopObjectResolved = true;
            }
            return this.topObject;
        }

        internal Type FastBindToType(string assemblyName, string typeName)
        {
            Type typeFromAssembly = null;
            TypeNAssembly cachedValue = this.typeCache.GetCachedValue(typeName) as TypeNAssembly;
            if ((cachedValue == null) || (cachedValue.assemblyName != assemblyName))
            {
                Assembly assem = this.LoadAssemblyFromString(assemblyName);
                if (assem == null)
                {
                    return null;
                }
                typeFromAssembly = FormatterServices.GetTypeFromAssembly(assem, typeName);
                if (typeFromAssembly == null)
                {
                    return null;
                }
                cachedValue = new TypeNAssembly {
                    type = typeFromAssembly,
                    assemblyName = assemblyName
                };
                this.typeCache.SetCachedValue(cachedValue);
            }
            return cachedValue.type;
        }

        internal string FilterBin64(string value)
        {
            this.sbf.Length = 0;
            for (int i = 0; i < value.Length; i++)
            {
                if (((value[i] != ' ') && (value[i] != '\n')) && (value[i] != '\r'))
                {
                    this.sbf.Append(value[i]);
                }
            }
            return this.sbf.ToString();
        }

        internal long GetId(string keyId)
        {
            if (keyId == null)
            {
                throw new ArgumentNullException("keyId", string.Format(CultureInfo.CurrentCulture, SoapUtil.GetResourceString("ArgumentNull_WithParamName"), new object[] { "keyId" }));
            }
            if (keyId != this.inKeyId)
            {
                this.inKeyId = keyId;
                string str = null;
                if (keyId[0] == '#')
                {
                    str = keyId.Substring(1);
                }
                else
                {
                    str = keyId;
                }
                object obj2 = this.objectIdTable[str];
                if (obj2 == null)
                {
                    this.outKeyId = this.objectIds += 1L;
                    this.objectIdTable[str] = this.outKeyId;
                }
                else
                {
                    this.outKeyId = (long) obj2;
                }
            }
            return this.outKeyId;
        }

        private ObjectManager GetObjectManager()
        {
            new SecurityPermission(SecurityPermissionFlag.SerializationFormatter).Assert();
            return new ObjectManager(this.m_surrogates, this.m_context);
        }

        private bool HasSurrogate(Type t)
        {
            ISurrogateSelector selector;
            if (this.m_surrogates == null)
            {
                return false;
            }
            return (this.m_surrogates.GetSurrogate(t, this.m_context, out selector) != null);
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

        private bool IsInternalType(string assemblyString, string typeString)
        {
            if (!(assemblyString == Converter.urtAssemblyString))
            {
                return false;
            }
            if (!(typeString == "System.DelegateSerializationHolder") && !(typeString == "System.UnitySerializationHolder"))
            {
                return (typeString == "System.MemberInfoSerializationHolder");
            }
            return true;
        }

        private bool IsWhiteSpace(string value)
        {
            for (int i = 0; i < value.Length; i++)
            {
                if (((value[i] != ' ') && (value[i] != '\n')) && (value[i] != '\r'))
                {
                    return false;
                }
            }
            return true;
        }

        internal Assembly LoadAssemblyFromString(string assemblyString)
        {
            Assembly assembly = null;
            if (this.formatterEnums.FEassemblyFormat == FormatterAssemblyStyle.Simple)
            {
                try
                {
                    sfileIOPermission.Assert();
                    try
                    {
                        assembly = Assembly.LoadWithPartialName(assemblyString, null);
                    }
                    finally
                    {
                        CodeAccessPermission.RevertAssert();
                    }
                }
                catch (Exception)
                {
                }
                return assembly;
            }
            try
            {
                sfileIOPermission.Assert();
                try
                {
                    assembly = Assembly.Load(assemblyString);
                }
                finally
                {
                    CodeAccessPermission.RevertAssert();
                }
            }
            catch (Exception)
            {
            }
            return assembly;
        }

        private void NextRectangleMap(ParseRecord pr)
        {
            for (int i = pr.PRrank - 1; i > -1; i--)
            {
                if (pr.PRrectangularMap[i] < (pr.PRlengthA[i] - 1))
                {
                    pr.PRrectangularMap[i]++;
                    if (i < (pr.PRrank - 1))
                    {
                        for (int j = i + 1; j < pr.PRrank; j++)
                        {
                            pr.PRrectangularMap[j] = 0;
                        }
                    }
                    Array.Copy(pr.PRrectangularMap, pr.PRindexMap, pr.PRrank);
                    return;
                }
            }
        }

        internal void Parse(ParseRecord pr)
        {
            switch (pr.PRparseTypeEnum)
            {
                case InternalParseTypeE.SerializedStreamHeader:
                    this.ParseSerializedStreamHeader(pr);
                    return;

                case InternalParseTypeE.Object:
                    this.ParseObject(pr);
                    return;

                case InternalParseTypeE.Member:
                    this.ParseMember(pr);
                    return;

                case InternalParseTypeE.ObjectEnd:
                    this.ParseObjectEnd(pr);
                    return;

                case InternalParseTypeE.MemberEnd:
                    this.ParseMemberEnd(pr);
                    return;

                case InternalParseTypeE.SerializedStreamHeaderEnd:
                    this.ParseSerializedStreamHeaderEnd(pr);
                    return;

                case InternalParseTypeE.Envelope:
                case InternalParseTypeE.EnvelopeEnd:
                case InternalParseTypeE.Body:
                case InternalParseTypeE.BodyEnd:
                    return;
            }
            throw new SerializationException(string.Format(CultureInfo.CurrentCulture, SoapUtil.GetResourceString("Serialization_XMLElement"), new object[] { pr.PRname }));
        }

        private void ParseArray(ParseRecord pr)
        {
            if (pr.PRobjectId < 1L)
            {
                pr.PRobjectId = this.GetId("GenId-" + this.objectIds);
            }
            if ((pr.PRarrayElementType != null) && pr.PRarrayElementType.IsEnum)
            {
                pr.PRisEnum = true;
            }
            if (pr.PRarrayTypeEnum == InternalArrayTypeE.Base64)
            {
                if (pr.PRvalue == null)
                {
                    pr.PRnewObj = new byte[0];
                    this.CheckSecurity(pr);
                }
                else if (pr.PRdtType == Converter.typeofSoapBase64Binary)
                {
                    pr.PRnewObj = SoapBase64Binary.Parse(pr.PRvalue);
                    this.CheckSecurity(pr);
                }
                else if (pr.PRvalue.Length > 0)
                {
                    pr.PRnewObj = Convert.FromBase64String(this.FilterBin64(pr.PRvalue));
                    this.CheckSecurity(pr);
                }
                else
                {
                    pr.PRnewObj = new byte[0];
                    this.CheckSecurity(pr);
                }
                if (this.stack.Peek() == pr)
                {
                    this.stack.Pop();
                }
                if (pr.PRobjectPositionEnum == InternalObjectPositionE.Top)
                {
                    this.topObject = pr.PRnewObj;
                    this.isTopObjectResolved = true;
                }
                ParseRecord objectPr = (ParseRecord) this.stack.Peek();
                this.RegisterObject(pr.PRnewObj, pr, objectPr);
            }
            else if ((pr.PRnewObj != null) && Converter.IsWriteAsByteArray(pr.PRarrayElementTypeCode))
            {
                if (pr.PRobjectPositionEnum == InternalObjectPositionE.Top)
                {
                    this.topObject = pr.PRnewObj;
                    this.isTopObjectResolved = true;
                }
                ParseRecord record2 = (ParseRecord) this.stack.Peek();
                this.RegisterObject(pr.PRnewObj, pr, record2);
            }
            else if ((pr.PRarrayTypeEnum == InternalArrayTypeE.Jagged) || (pr.PRarrayTypeEnum == InternalArrayTypeE.Single))
            {
                if ((pr.PRlowerBoundA == null) || (pr.PRlowerBoundA[0] == 0))
                {
                    pr.PRnewObj = Array.CreateInstance(pr.PRarrayElementType, (pr.PRrank > 0) ? pr.PRlengthA[0] : 0);
                    pr.PRisLowerBound = false;
                }
                else
                {
                    pr.PRnewObj = Array.CreateInstance(pr.PRarrayElementType, pr.PRlengthA, pr.PRlowerBoundA);
                    pr.PRisLowerBound = true;
                }
                if (pr.PRarrayTypeEnum == InternalArrayTypeE.Single)
                {
                    if (!pr.PRisLowerBound && Converter.IsWriteAsByteArray(pr.PRarrayElementTypeCode))
                    {
                        pr.PRprimitiveArray = new PrimitiveArray(pr.PRarrayElementTypeCode, (Array) pr.PRnewObj);
                    }
                    else if (!pr.PRarrayElementType.IsValueType && (pr.PRlowerBoundA == null))
                    {
                        pr.PRobjectA = (object[]) pr.PRnewObj;
                    }
                }
                this.CheckSecurity(pr);
                if (pr.PRobjectPositionEnum == InternalObjectPositionE.Headers)
                {
                    this.headers = (Header[]) pr.PRnewObj;
                }
                pr.PRindexMap = new int[1];
            }
            else
            {
                if (pr.PRarrayTypeEnum != InternalArrayTypeE.Rectangular)
                {
                    throw new SerializationException(string.Format(CultureInfo.CurrentCulture, SoapUtil.GetResourceString("Serialization_ArrayType"), new object[] { pr.PRarrayTypeEnum.ToString() }));
                }
                pr.PRisLowerBound = false;
                if (pr.PRlowerBoundA != null)
                {
                    for (int j = 0; j < pr.PRrank; j++)
                    {
                        if (pr.PRlowerBoundA[j] != 0)
                        {
                            pr.PRisLowerBound = true;
                        }
                    }
                }
                if (!pr.PRisLowerBound)
                {
                    pr.PRnewObj = Array.CreateInstance(pr.PRarrayElementType, pr.PRlengthA);
                }
                else
                {
                    pr.PRnewObj = Array.CreateInstance(pr.PRarrayElementType, pr.PRlengthA, pr.PRlowerBoundA);
                }
                this.CheckSecurity(pr);
                int num3 = 1;
                for (int i = 0; i < pr.PRrank; i++)
                {
                    num3 *= pr.PRlengthA[i];
                }
                pr.PRindexMap = new int[pr.PRrank];
                pr.PRrectangularMap = new int[pr.PRrank];
                pr.PRlinearlength = num3;
            }
        }

        private void ParseArrayMember(ParseRecord pr)
        {
            ParseRecord record = (ParseRecord) this.stack.Peek();
            if (record.PRarrayTypeEnum == InternalArrayTypeE.Rectangular)
            {
                if (pr.PRpositionA != null)
                {
                    Array.Copy(pr.PRpositionA, record.PRindexMap, record.PRindexMap.Length);
                    if (record.PRlowerBoundA == null)
                    {
                        Array.Copy(pr.PRpositionA, record.PRrectangularMap, record.PRrectangularMap.Length);
                    }
                    else
                    {
                        for (int i = 0; i < record.PRrectangularMap.Length; i++)
                        {
                            record.PRrectangularMap[i] = pr.PRpositionA[i] - record.PRlowerBoundA[i];
                        }
                    }
                }
                else
                {
                    if (record.PRmemberIndex > 0)
                    {
                        this.NextRectangleMap(record);
                    }
                    for (int j = 0; j < record.PRrank; j++)
                    {
                        int num3 = 0;
                        if (record.PRlowerBoundA != null)
                        {
                            num3 = record.PRlowerBoundA[j];
                        }
                        record.PRindexMap[j] = record.PRrectangularMap[j] + num3;
                    }
                }
            }
            else if (!record.PRisLowerBound)
            {
                if (pr.PRpositionA == null)
                {
                    record.PRindexMap[0] = record.PRmemberIndex;
                }
                else
                {
                    record.PRindexMap[0] = record.PRmemberIndex = pr.PRpositionA[0];
                }
            }
            else if (pr.PRpositionA == null)
            {
                record.PRindexMap[0] = record.PRmemberIndex + record.PRlowerBoundA[0];
            }
            else
            {
                record.PRindexMap[0] = pr.PRpositionA[0];
                record.PRmemberIndex = pr.PRpositionA[0] - record.PRlowerBoundA[0];
            }
            if (pr.PRmemberValueEnum == InternalMemberValueE.Reference)
            {
                object obj2 = this.m_objectManager.GetObject(pr.PRidRef);
                if (obj2 == null)
                {
                    int[] destinationArray = new int[record.PRrank];
                    Array.Copy(record.PRindexMap, 0, destinationArray, 0, record.PRrank);
                    this.m_objectManager.RecordArrayElementFixup(record.PRobjectId, destinationArray, pr.PRidRef);
                }
                else if (record.PRobjectA != null)
                {
                    record.PRobjectA[record.PRindexMap[0]] = obj2;
                }
                else
                {
                    ((Array) record.PRnewObj).SetValue(obj2, record.PRindexMap);
                }
            }
            else if (pr.PRmemberValueEnum == InternalMemberValueE.Nested)
            {
                if (pr.PRdtType == null)
                {
                    pr.PRdtType = record.PRarrayElementType;
                }
                this.ParseObject(pr);
                this.stack.Push(pr);
                if (record.PRarrayElementType.IsValueType && (pr.PRarrayElementTypeCode == InternalPrimitiveTypeE.Invalid))
                {
                    pr.PRisValueTypeFixup = true;
                    this.valueFixupStack.Push(new ValueFixup((Array) record.PRnewObj, record.PRindexMap));
                }
                else if (record.PRobjectA != null)
                {
                    record.PRobjectA[record.PRindexMap[0]] = pr.PRnewObj;
                }
                else
                {
                    ((Array) record.PRnewObj).SetValue(pr.PRnewObj, record.PRindexMap);
                }
            }
            else if (pr.PRmemberValueEnum == InternalMemberValueE.InlineValue)
            {
                if (record.PRarrayElementType == Converter.typeofString)
                {
                    this.ParseString(pr, record);
                    if (record.PRobjectA != null)
                    {
                        record.PRobjectA[record.PRindexMap[0]] = pr.PRvalue;
                    }
                    else
                    {
                        ((Array) record.PRnewObj).SetValue(pr.PRvalue, record.PRindexMap);
                    }
                }
                else if (record.PRisEnum)
                {
                    object obj3 = Enum.Parse(record.PRarrayElementType, pr.PRvalue);
                    if (record.PRobjectA != null)
                    {
                        record.PRobjectA[record.PRindexMap[0]] = (Enum) obj3;
                    }
                    else
                    {
                        ((Array) record.PRnewObj).SetValue((Enum) obj3, record.PRindexMap);
                    }
                }
                else if (record.PRisArrayVariant)
                {
                    if ((pr.PRdtType == null) && (pr.PRkeyDt == null))
                    {
                        throw new SerializationException(SoapUtil.GetResourceString("Serialization_ArrayTypeObject"));
                    }
                    object pRvalue = null;
                    if (pr.PRdtType == Converter.typeofString)
                    {
                        this.ParseString(pr, record);
                        pRvalue = pr.PRvalue;
                    }
                    else if (pr.PRdtType.IsEnum)
                    {
                        pRvalue = Enum.Parse(pr.PRdtType, pr.PRvalue);
                    }
                    else if (pr.PRdtTypeCode == InternalPrimitiveTypeE.Invalid)
                    {
                        this.CheckSerializable(pr.PRdtType);
                        if (this.IsRemoting && (this.formatterEnums.FEsecurityLevel != TypeFilterLevel.Full))
                        {
                            pRvalue = FormatterServices.GetSafeUninitializedObject(pr.PRdtType);
                        }
                        else
                        {
                            pRvalue = FormatterServices.GetUninitializedObject(pr.PRdtType);
                        }
                    }
                    else if (pr.PRvarValue != null)
                    {
                        pRvalue = pr.PRvarValue;
                    }
                    else
                    {
                        pRvalue = Converter.FromString(pr.PRvalue, pr.PRdtTypeCode);
                    }
                    if (record.PRobjectA != null)
                    {
                        record.PRobjectA[record.PRindexMap[0]] = pRvalue;
                    }
                    else
                    {
                        ((Array) record.PRnewObj).SetValue(pRvalue, record.PRindexMap);
                    }
                }
                else if (record.PRprimitiveArray != null)
                {
                    record.PRprimitiveArray.SetValue(pr.PRvalue, record.PRindexMap[0]);
                }
                else
                {
                    object pRvarValue = null;
                    if (pr.PRvarValue != null)
                    {
                        pRvarValue = pr.PRvarValue;
                    }
                    else
                    {
                        pRvarValue = Converter.FromString(pr.PRvalue, record.PRarrayElementTypeCode);
                    }
                    if (record.PRarrayElementTypeCode == InternalPrimitiveTypeE.QName)
                    {
                        SoapQName name = (SoapQName) pRvarValue;
                        if (name.Key.Length == 0)
                        {
                            name.Namespace = (string) this.soapHandler.keyToNamespaceTable["xmlns"];
                        }
                        else
                        {
                            name.Namespace = (string) this.soapHandler.keyToNamespaceTable["xmlns:" + name.Key];
                        }
                    }
                    if (record.PRobjectA != null)
                    {
                        record.PRobjectA[record.PRindexMap[0]] = pRvarValue;
                    }
                    else
                    {
                        ((Array) record.PRnewObj).SetValue(pRvarValue, record.PRindexMap);
                    }
                }
            }
            else if (pr.PRmemberValueEnum != InternalMemberValueE.Null)
            {
                this.ParseError(pr, record);
            }
            record.PRmemberIndex++;
        }

        private void ParseArrayMemberEnd(ParseRecord pr)
        {
            if (pr.PRmemberValueEnum == InternalMemberValueE.Nested)
            {
                this.ParseObjectEnd(pr);
            }
        }

        private void ParseError(ParseRecord processing, ParseRecord onStack)
        {
            throw new SerializationException(string.Format(CultureInfo.CurrentCulture, SoapUtil.GetResourceString("Serialization_ParseError"), new object[] { onStack.PRname + " " + onStack.PRparseTypeEnum.ToString() + " " + processing.PRname + " " + processing.PRparseTypeEnum.ToString() }));
        }

        private void ParseMember(ParseRecord pr)
        {
            ParseRecord parentPr = (ParseRecord) this.stack.Peek();
            if (parentPr != null)
            {
                string pRname = parentPr.PRname;
            }
            if ((parentPr.PRdtType == Converter.typeofSoapFault) && (pr.PRname.ToLower(CultureInfo.InvariantCulture) == "faultstring"))
            {
                this.faultString = pr.PRvalue;
            }
            if ((parentPr.PRobjectPositionEnum == InternalObjectPositionE.Top) && !this.isTopObjectResolved)
            {
                if (pr.PRdtType == Converter.typeofString)
                {
                    this.ParseString(pr, parentPr);
                }
                this.topStack.Push(pr.Copy());
            }
            else
            {
                switch (pr.PRmemberTypeEnum)
                {
                    case InternalMemberTypeE.Item:
                        this.ParseArrayMember(pr);
                        return;
                }
                if (parentPr.PRobjectInfo != null)
                {
                    parentPr.PRobjectInfo.AddMemberSeen();
                }
                bool flag = ((this.IsFakeTopObject && (parentPr.PRobjectPositionEnum == InternalObjectPositionE.Top)) && (parentPr.PRobjectInfo != null)) && (parentPr.PRdtType != Converter.typeofSoapFault);
                if ((pr.PRdtType == null) && parentPr.PRobjectInfo.isTyped)
                {
                    if (flag)
                    {
                        pr.PRdtType = parentPr.PRobjectInfo.GetType(this.paramPosition++);
                    }
                    else
                    {
                        pr.PRdtType = parentPr.PRobjectInfo.GetType(pr.PRname);
                    }
                    if (pr.PRdtType == null)
                    {
                        throw new SerializationException(string.Format(CultureInfo.CurrentCulture, SoapUtil.GetResourceString("Serialization_TypeResolved"), new object[] { parentPr.PRnewObj + " " + pr.PRname }));
                    }
                    pr.PRdtTypeCode = Converter.ToCode(pr.PRdtType);
                }
                else if (flag)
                {
                    this.paramPosition++;
                }
                if (pr.PRmemberValueEnum == InternalMemberValueE.Null)
                {
                    parentPr.PRobjectInfo.AddValue(pr.PRname, null);
                }
                else if (pr.PRmemberValueEnum == InternalMemberValueE.Nested)
                {
                    this.ParseObject(pr);
                    this.stack.Push(pr);
                    if ((pr.PRobjectInfo != null) && pr.PRobjectInfo.objectType.IsValueType)
                    {
                        if (this.IsFakeTopObject)
                        {
                            parentPr.PRobjectInfo.AddParamName(pr.PRname);
                        }
                        pr.PRisValueTypeFixup = true;
                        this.valueFixupStack.Push(new ValueFixup(parentPr.PRnewObj, pr.PRname, parentPr.PRobjectInfo));
                    }
                    else
                    {
                        parentPr.PRobjectInfo.AddValue(pr.PRname, pr.PRnewObj);
                    }
                }
                else if (pr.PRmemberValueEnum == InternalMemberValueE.Reference)
                {
                    object obj2 = this.m_objectManager.GetObject(pr.PRidRef);
                    if (obj2 == null)
                    {
                        parentPr.PRobjectInfo.AddValue(pr.PRname, null);
                        parentPr.PRobjectInfo.RecordFixup(parentPr.PRobjectId, pr.PRname, pr.PRidRef);
                    }
                    else
                    {
                        parentPr.PRobjectInfo.AddValue(pr.PRname, obj2);
                    }
                }
                else if (pr.PRmemberValueEnum == InternalMemberValueE.InlineValue)
                {
                    if (pr.PRdtType == Converter.typeofString)
                    {
                        this.ParseString(pr, parentPr);
                        parentPr.PRobjectInfo.AddValue(pr.PRname, pr.PRvalue);
                    }
                    else if (pr.PRdtTypeCode != InternalPrimitiveTypeE.Invalid)
                    {
                        object pRvarValue = null;
                        if (pr.PRvarValue != null)
                        {
                            pRvarValue = pr.PRvarValue;
                        }
                        else
                        {
                            pRvarValue = Converter.FromString(pr.PRvalue, pr.PRdtTypeCode);
                        }
                        if ((pr.PRdtTypeCode == InternalPrimitiveTypeE.QName) && (pRvarValue != null))
                        {
                            SoapQName name = (SoapQName) pRvarValue;
                            if (name.Key != null)
                            {
                                if (name.Key.Length == 0)
                                {
                                    name.Namespace = (string) this.soapHandler.keyToNamespaceTable["xmlns"];
                                }
                                else
                                {
                                    name.Namespace = (string) this.soapHandler.keyToNamespaceTable["xmlns:" + name.Key];
                                }
                            }
                        }
                        parentPr.PRobjectInfo.AddValue(pr.PRname, pRvarValue);
                    }
                    else if (pr.PRarrayTypeEnum == InternalArrayTypeE.Base64)
                    {
                        parentPr.PRobjectInfo.AddValue(pr.PRname, Convert.FromBase64String(this.FilterBin64(pr.PRvalue)));
                    }
                    else if ((pr.PRdtType != Converter.typeofObject) || (pr.PRvalue == null))
                    {
                        if ((pr.PRdtType != null) && pr.PRdtType.IsEnum)
                        {
                            object obj3 = Enum.Parse(pr.PRdtType, pr.PRvalue);
                            parentPr.PRobjectInfo.AddValue(pr.PRname, obj3);
                        }
                        else if ((pr.PRdtType != null) && (pr.PRdtType == Converter.typeofTypeArray))
                        {
                            parentPr.PRobjectInfo.AddValue(pr.PRname, pr.PRvarValue);
                        }
                        else
                        {
                            if (!pr.PRisRegistered && (pr.PRobjectId > 0L))
                            {
                                if (pr.PRvalue == null)
                                {
                                    pr.PRvalue = "";
                                }
                                this.RegisterObject(pr.PRvalue, pr, parentPr);
                            }
                            if (pr.PRdtType == Converter.typeofSystemVoid)
                            {
                                parentPr.PRobjectInfo.AddValue(pr.PRname, pr.PRdtType);
                            }
                            else if (parentPr.PRobjectInfo.isSi)
                            {
                                parentPr.PRobjectInfo.AddValue(pr.PRname, pr.PRvalue);
                            }
                        }
                    }
                    else if ((parentPr != null) && (parentPr.PRdtType == Converter.typeofHeader))
                    {
                        pr.PRdtType = Converter.typeofString;
                        this.ParseString(pr, parentPr);
                        parentPr.PRobjectInfo.AddValue(pr.PRname, pr.PRvalue);
                    }
                }
                else
                {
                    this.ParseError(pr, parentPr);
                }
            }
        }

        private void ParseMemberEnd(ParseRecord pr)
        {
            switch (pr.PRmemberTypeEnum)
            {
                case InternalMemberTypeE.Field:
                    if (pr.PRmemberValueEnum != InternalMemberValueE.Nested)
                    {
                        break;
                    }
                    this.ParseObjectEnd(pr);
                    return;

                case InternalMemberTypeE.Item:
                    this.ParseArrayMemberEnd(pr);
                    return;

                default:
                    if (pr.PRmemberValueEnum == InternalMemberValueE.Nested)
                    {
                        this.ParseObjectEnd(pr);
                        return;
                    }
                    this.ParseError(pr, (ParseRecord) this.stack.Peek());
                    break;
            }
        }

        private void ParseObject(ParseRecord pr)
        {
            if (pr.PRobjectPositionEnum == InternalObjectPositionE.Top)
            {
                this.topId = pr.PRobjectId;
            }
            if (pr.PRparseTypeEnum == InternalParseTypeE.Object)
            {
                this.stack.Push(pr);
            }
            if (pr.PRobjectTypeEnum == InternalObjectTypeE.Array)
            {
                this.ParseArray(pr);
            }
            else
            {
                if ((pr.PRdtType == null) && !this.IsFakeTopObject)
                {
                    throw new SerializationException(string.Format(CultureInfo.CurrentCulture, SoapUtil.GetResourceString("Serialization_TopObjectInstantiate"), new object[] { pr.PRname }));
                }
                if (((pr.PRobjectPositionEnum == InternalObjectPositionE.Top) && this.IsFakeTopObject) && (pr.PRdtType != Converter.typeofSoapFault))
                {
                    if (this.handler == null)
                    {
                        if (this.formatterEnums.FEtopObject != null)
                        {
                            if (!this.isTopObjectSecondPass)
                            {
                                this.isTopObjectResolved = false;
                                this.topStack = new SerStack("Top ParseRecords");
                                this.topStack.Push(pr.Copy());
                                return;
                            }
                            pr.PRnewObj = new InternalSoapMessage();
                            pr.PRdtType = typeof(InternalSoapMessage);
                            this.CheckSecurity(pr);
                            if (this.formatterEnums.FEtopObject != null)
                            {
                                ISoapMessage fEtopObject = this.formatterEnums.FEtopObject;
                                pr.PRobjectInfo = this.CreateReadObjectInfo(pr.PRdtType, fEtopObject.ParamNames, fEtopObject.ParamTypes, pr.PRassemblyName);
                            }
                        }
                    }
                    else
                    {
                        if (!this.isHeaderHandlerCalled)
                        {
                            this.newheaders = null;
                            this.isHeaderHandlerCalled = true;
                            if (this.headers == null)
                            {
                                this.newheaders = new Header[1];
                            }
                            else
                            {
                                this.newheaders = new Header[this.headers.Length + 1];
                                Array.Copy(this.headers, 0, this.newheaders, 1, this.headers.Length);
                            }
                            Header header = new Header("__methodName", pr.PRname, false, pr.PRnameXmlKey);
                            this.newheaders[0] = header;
                            this.handlerObject = this.handler(this.newheaders);
                        }
                        if (!this.isHeaderHandlerCalled)
                        {
                            this.isTopObjectResolved = false;
                            this.topStack = new SerStack("Top ParseRecords");
                            this.topStack.Push(pr.Copy());
                            return;
                        }
                        pr.PRnewObj = this.handlerObject;
                        pr.PRdtType = this.handlerObject.GetType();
                        this.CheckSecurity(pr);
                        if (pr.PRnewObj is IFieldInfo)
                        {
                            IFieldInfo pRnewObj = (IFieldInfo) pr.PRnewObj;
                            if ((pRnewObj.FieldTypes != null) && (pRnewObj.FieldTypes.Length > 0))
                            {
                                pr.PRobjectInfo = this.CreateReadObjectInfo(pr.PRdtType, pRnewObj.FieldNames, pRnewObj.FieldTypes, pr.PRassemblyName);
                            }
                        }
                    }
                }
                else
                {
                    if (pr.PRdtType == Converter.typeofString)
                    {
                        if (pr.PRvalue != null)
                        {
                            pr.PRnewObj = pr.PRvalue;
                            if (pr.PRobjectPositionEnum == InternalObjectPositionE.Top)
                            {
                                this.isTopObjectResolved = true;
                                this.topObject = pr.PRnewObj;
                                return;
                            }
                            this.stack.Pop();
                            this.RegisterObject(pr.PRnewObj, pr, (ParseRecord) this.stack.Peek());
                        }
                        return;
                    }
                    if (pr.PRdtType == null)
                    {
                        ParseRecord record = (ParseRecord) this.stack.Peek();
                        if (record.PRdtType == Converter.typeofSoapFault)
                        {
                            throw new ServerException(string.Format(CultureInfo.CurrentCulture, SoapUtil.GetResourceString("Serialization_SoapFault"), new object[] { this.faultString }));
                        }
                        throw new SerializationException(string.Format(CultureInfo.CurrentCulture, SoapUtil.GetResourceString("Serialization_TypeElement"), new object[] { pr.PRname }));
                    }
                    this.CheckSerializable(pr.PRdtType);
                    if (this.IsRemoting && (this.formatterEnums.FEsecurityLevel != TypeFilterLevel.Full))
                    {
                        pr.PRnewObj = FormatterServices.GetSafeUninitializedObject(pr.PRdtType);
                    }
                    else
                    {
                        pr.PRnewObj = FormatterServices.GetUninitializedObject(pr.PRdtType);
                    }
                    this.CheckSecurity(pr);
                    this.m_objectManager.RaiseOnDeserializingEvent(pr.PRnewObj);
                }
                if (pr.PRnewObj == null)
                {
                    throw new SerializationException(string.Format(CultureInfo.CurrentCulture, SoapUtil.GetResourceString("Serialization_TopObjectInstantiate"), new object[] { pr.PRdtType }));
                }
                if (pr.PRobjectId < 1L)
                {
                    pr.PRobjectId = this.GetId("GenId-" + this.objectIds);
                }
                if (this.IsFakeTopObject && (pr.PRobjectPositionEnum == InternalObjectPositionE.Top))
                {
                    this.isTopObjectResolved = true;
                    this.topObject = pr.PRnewObj;
                }
                if (pr.PRobjectInfo == null)
                {
                    pr.PRobjectInfo = this.CreateReadObjectInfo(pr.PRdtType, pr.PRassemblyName);
                }
                pr.PRobjectInfo.obj = pr.PRnewObj;
                if (this.IsFakeTopObject && (pr.PRobjectPositionEnum == InternalObjectPositionE.Top))
                {
                    pr.PRobjectInfo.AddValue("__methodName", pr.PRname);
                    pr.PRobjectInfo.AddValue("__keyToNamespaceTable", this.soapHandler.keyToNamespaceTable);
                    pr.PRobjectInfo.AddValue("__paramNameList", pr.PRobjectInfo.SetFakeObject());
                    if (this.formatterEnums.FEtopObject != null)
                    {
                        pr.PRobjectInfo.AddValue("__xmlNameSpace", pr.PRxmlNameSpace);
                    }
                }
            }
        }

        private void ParseObjectEnd(ParseRecord pr)
        {
            ParseRecord record = (ParseRecord) this.stack.Peek();
            if (record == null)
            {
                record = pr;
            }
            if (record.PRobjectPositionEnum == InternalObjectPositionE.Top)
            {
                if (record.PRdtType == Converter.typeofString)
                {
                    if (record.PRvalue == null)
                    {
                        record.PRvalue = string.Empty;
                    }
                    record.PRnewObj = record.PRvalue;
                    this.CheckSecurity(record);
                    this.isTopObjectResolved = true;
                    this.topObject = record.PRnewObj;
                    return;
                }
                if ((((record.PRdtType != null) && (record.PRvalue != null)) && !this.IsWhiteSpace(record.PRvalue)) && (record.PRdtType.IsPrimitive || (record.PRdtType == Converter.typeofTimeSpan)))
                {
                    record.PRnewObj = Converter.FromString(record.PRvalue, Converter.ToCode(record.PRdtType));
                    this.CheckSecurity(record);
                    this.isTopObjectResolved = true;
                    this.topObject = record.PRnewObj;
                    return;
                }
                if (!this.isTopObjectResolved && (record.PRdtType != Converter.typeofSoapFault))
                {
                    this.topStack.Push(pr.Copy());
                    if (record.PRparseRecordId == pr.PRparseRecordId)
                    {
                        this.stack.Pop();
                    }
                    return;
                }
            }
            this.stack.Pop();
            ParseRecord objectPr = (ParseRecord) this.stack.Peek();
            if (record.PRobjectTypeEnum == InternalObjectTypeE.Array)
            {
                if (record.PRobjectPositionEnum == InternalObjectPositionE.Top)
                {
                    this.isTopObjectResolved = true;
                    this.topObject = record.PRnewObj;
                }
                this.RegisterObject(record.PRnewObj, record, objectPr);
            }
            else
            {
                if (record.PRobjectInfo != null)
                {
                    record.PRobjectInfo.PopulateObjectMembers();
                }
                if (record.PRnewObj == null)
                {
                    if (record.PRdtType != Converter.typeofString)
                    {
                        throw new SerializationException(string.Format(CultureInfo.CurrentCulture, SoapUtil.GetResourceString("Serialization_ObjectMissing"), new object[] { pr.PRname }));
                    }
                    if (record.PRvalue == null)
                    {
                        record.PRvalue = string.Empty;
                    }
                    record.PRnewObj = record.PRvalue;
                    this.CheckSecurity(record);
                }
                if (!record.PRisRegistered && (record.PRobjectId > 0L))
                {
                    this.RegisterObject(record.PRnewObj, record, objectPr);
                }
                if (record.PRisValueTypeFixup)
                {
                    ((ValueFixup) this.valueFixupStack.Pop()).Fixup(record, objectPr);
                }
                if (record.PRobjectPositionEnum == InternalObjectPositionE.Top)
                {
                    this.isTopObjectResolved = true;
                    this.topObject = record.PRnewObj;
                }
                if (record.PRnewObj is SoapFault)
                {
                    this.soapFaultId = record.PRobjectId;
                }
                if (record.PRobjectInfo != null)
                {
                    if (record.PRobjectInfo.bfake && !record.PRobjectInfo.bSoapFault)
                    {
                        record.PRobjectInfo.AddValue("__fault", null);
                    }
                    record.PRobjectInfo.ObjectEnd();
                }
            }
        }

        private void ParseSerializedStreamHeader(ParseRecord pr)
        {
            this.stack.Push(pr);
        }

        private void ParseSerializedStreamHeaderEnd(ParseRecord pr)
        {
            this.stack.Pop();
        }

        private void ParseString(ParseRecord pr, ParseRecord parentPr)
        {
            if (pr.PRvalue == null)
            {
                pr.PRvalue = "";
            }
            if (!pr.PRisRegistered && (pr.PRobjectId > 0L))
            {
                this.RegisterObject(pr.PRvalue, pr, parentPr);
            }
        }

        private void RegisterObject(object obj, ParseRecord pr, ParseRecord objectPr)
        {
            if (!pr.PRisRegistered)
            {
                pr.PRisRegistered = true;
                SerializationInfo si = null;
                long idOfContainingObj = 0L;
                MemberInfo member = null;
                int[] arrayIndex = null;
                if (objectPr != null)
                {
                    arrayIndex = objectPr.PRindexMap;
                    idOfContainingObj = objectPr.PRobjectId;
                    if ((objectPr.PRobjectInfo != null) && !objectPr.PRobjectInfo.isSi)
                    {
                        member = objectPr.PRobjectInfo.GetMemberInfo(pr.PRname);
                    }
                }
                if (pr.PRobjectInfo != null)
                {
                    si = pr.PRobjectInfo.si;
                }
                this.m_objectManager.RegisterObject(obj, pr.PRobjectId, si, idOfContainingObj, member, arrayIndex);
            }
        }

        internal void SetVersion(int major, int minor)
        {
            if (this.formatterEnums.FEassemblyFormat != FormatterAssemblyStyle.Simple)
            {
                this.majorVersion = major;
                this.minorVersion = minor;
            }
        }

        private bool IsRemoting
        {
            get
            {
                return this.IsFakeTopObject;
            }
        }

        internal class TypeNAssembly
        {
            public string assemblyName;
            public Type type;
        }
    }
}

