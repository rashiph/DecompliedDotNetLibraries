namespace System.Runtime.Serialization.Formatters.Binary
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Resources;
    using System.Runtime.CompilerServices;
    using System.Runtime.Remoting.Messaging;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters;
    using System.Security;
    using System.Security.Permissions;
    using System.Text;
    using System.Threading;

    internal sealed class ObjectReader
    {
        private bool bFullDeserialization;
        private BinaryMethodCall binaryMethodCall;
        private BinaryMethodReturn binaryMethodReturn;
        private bool bIsCrossAppDomain;
        private bool bMethodCall;
        private bool bMethodReturn;
        private bool bOldFormatDetected;
        internal bool bSimpleAssembly;
        internal object[] crossAppDomainArray;
        internal InternalFE formatterEnums;
        internal HeaderHandler handler;
        internal object handlerObject;
        internal Header[] headers;
        internal SerializationBinder m_binder;
        internal StreamingContext m_context;
        internal IFormatterConverter m_formatterConverter;
        internal ObjectManager m_objectManager;
        internal Stream m_stream;
        internal ISurrogateSelector m_surrogates;
        internal object m_topObject;
        private string previousAssemblyString;
        private string previousName;
        private Type previousType;
        internal SerObjectInfoInit serObjectInfoInit;
        private static FileIOPermission sfileIOPermission = new FileIOPermission(PermissionState.Unrestricted);
        internal SerStack stack;
        private const int THRESHOLD_FOR_VALUETYPE_IDS = 0x7fffffff;
        internal long topId;
        private NameCache typeCache = new NameCache();
        private IntSizedArray valTypeObjectIdTable;
        private SerStack valueFixupStack;

        internal ObjectReader(Stream stream, ISurrogateSelector selector, StreamingContext context, InternalFE formatterEnums, SerializationBinder binder)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream", Environment.GetResourceString("ArgumentNull_Stream"));
            }
            this.m_stream = stream;
            this.m_surrogates = selector;
            this.m_context = context;
            this.m_binder = binder;
            if (this.m_binder != null)
            {
                ResourceReader.TypeLimitingDeserializationBinder binder2 = this.m_binder as ResourceReader.TypeLimitingDeserializationBinder;
                if (binder2 != null)
                {
                    binder2.ObjectReader = this;
                }
            }
            this.formatterEnums = formatterEnums;
        }

        [SecurityCritical]
        internal Type Bind(string assemblyString, string typeString)
        {
            Type type = null;
            if (this.m_binder != null)
            {
                type = this.m_binder.BindToType(assemblyString, typeString);
            }
            if (type == null)
            {
                type = this.FastBindToType(assemblyString, typeString);
            }
            return type;
        }

        [SecurityCritical]
        internal void CheckSecurity(ParseRecord pr)
        {
            Type pRdtType = pr.PRdtType;
            if ((pRdtType != null) && this.IsRemoting)
            {
                if (typeof(MarshalByRefObject).IsAssignableFrom(pRdtType))
                {
                    throw new ArgumentException(Environment.GetResourceString("Serialization_MBRAsMBV", new object[] { pRdtType.FullName }));
                }
                FormatterServices.CheckTypeSecurity(pRdtType, this.formatterEnums.FEsecurityLevel);
            }
        }

        [SecurityCritical]
        private void CheckSerializable(Type t)
        {
            if (!t.IsSerializable && !this.HasSurrogate(t))
            {
                throw new SerializationException(string.Format(CultureInfo.InvariantCulture, Environment.GetResourceString("Serialization_NonSerType"), new object[] { t.FullName, t.Assembly.FullName }));
            }
        }

        [SecuritySafeCritical]
        private static void CheckTypeForwardedTo(Assembly sourceAssembly, Assembly destAssembly, Type resolvedType)
        {
            if ((!FormatterServices.UnsafeTypeForwardersIsEnabled() && (sourceAssembly != destAssembly)) && !destAssembly.PermissionSet.IsSubsetOf(sourceAssembly.PermissionSet))
            {
                TypeInformation typeInformation = BinaryFormatter.GetTypeInformation(resolvedType);
                if (!typeInformation.HasTypeForwardedFrom)
                {
                    SecurityException exception2 = new SecurityException {
                        Demanded = sourceAssembly.PermissionSet
                    };
                    throw exception2;
                }
                Assembly assembly = null;
                try
                {
                    assembly = Assembly.Load(typeInformation.AssemblyString);
                }
                catch
                {
                }
                if (assembly != sourceAssembly)
                {
                    SecurityException exception = new SecurityException {
                        Demanded = sourceAssembly.PermissionSet
                    };
                    throw exception;
                }
            }
        }

        [SecurityCritical]
        internal ReadObjectInfo CreateReadObjectInfo(Type objectType)
        {
            return ReadObjectInfo.Create(objectType, this.m_surrogates, this.m_context, this.m_objectManager, this.serObjectInfoInit, this.m_formatterConverter, this.bSimpleAssembly);
        }

        [SecurityCritical]
        internal ReadObjectInfo CreateReadObjectInfo(Type objectType, string[] memberNames, Type[] memberTypes)
        {
            return ReadObjectInfo.Create(objectType, memberNames, memberTypes, this.m_surrogates, this.m_context, this.m_objectManager, this.serObjectInfoInit, this.m_formatterConverter, this.bSimpleAssembly);
        }

        internal object CrossAppDomainArray(int index)
        {
            return this.crossAppDomainArray[index];
        }

        [SecurityCritical]
        internal object Deserialize(HeaderHandler handler, __BinaryParser serParser, bool fCheck, bool isCrossAppDomain, IMethodCallMessage methodCallMessage)
        {
            if (serParser == null)
            {
                throw new ArgumentNullException("serParser", Environment.GetResourceString("ArgumentNull_WithParamName", new object[] { serParser }));
            }
            this.bFullDeserialization = false;
            this.TopObject = null;
            this.topId = 0L;
            this.bMethodCall = false;
            this.bMethodReturn = false;
            this.bIsCrossAppDomain = isCrossAppDomain;
            this.bSimpleAssembly = this.formatterEnums.FEassemblyFormat == FormatterAssemblyStyle.Simple;
            if (fCheck)
            {
                CodeAccessPermission.Demand(PermissionType.SecuritySerialization);
            }
            this.handler = handler;
            if (this.bFullDeserialization)
            {
                this.m_objectManager = new ObjectManager(this.m_surrogates, this.m_context, false, this.bIsCrossAppDomain);
                this.serObjectInfoInit = new SerObjectInfoInit();
            }
            serParser.Run();
            if (this.bFullDeserialization)
            {
                this.m_objectManager.DoFixups();
            }
            if (!this.bMethodCall && !this.bMethodReturn)
            {
                if (this.TopObject == null)
                {
                    throw new SerializationException(Environment.GetResourceString("Serialization_TopObject"));
                }
                if (this.HasSurrogate(this.TopObject.GetType()) && (this.topId != 0L))
                {
                    this.TopObject = this.m_objectManager.GetObject(this.topId);
                }
                if (this.TopObject is IObjectReference)
                {
                    this.TopObject = ((IObjectReference) this.TopObject).GetRealObject(this.m_context);
                }
            }
            if (this.bFullDeserialization)
            {
                this.m_objectManager.RaiseDeserializationEvent();
            }
            if (handler != null)
            {
                this.handlerObject = handler(this.headers);
            }
            if (this.bMethodCall)
            {
                object[] topObject = this.TopObject as object[];
                this.TopObject = this.binaryMethodCall.ReadArray(topObject, this.handlerObject);
            }
            else if (this.bMethodReturn)
            {
                object[] returnA = this.TopObject as object[];
                this.TopObject = this.binaryMethodReturn.ReadArray(returnA, methodCallMessage, this.handlerObject);
            }
            return this.TopObject;
        }

        [SecurityCritical]
        internal Type FastBindToType(string assemblyName, string typeName)
        {
            Type typeFromAssembly = null;
            TypeNAssembly cachedValue = (TypeNAssembly) this.typeCache.GetCachedValue(typeName);
            if ((cachedValue == null) || (cachedValue.assemblyName != assemblyName))
            {
                Assembly assm = null;
                if (this.bSimpleAssembly)
                {
                    try
                    {
                        sfileIOPermission.Assert();
                        try
                        {
                            assm = ResolveSimpleAssemblyName(new AssemblyName(assemblyName));
                        }
                        finally
                        {
                            CodeAccessPermission.RevertAssert();
                        }
                    }
                    catch (Exception)
                    {
                    }
                    if (assm == null)
                    {
                        return null;
                    }
                    GetSimplyNamedTypeFromAssembly(assm, typeName, ref typeFromAssembly);
                }
                else
                {
                    try
                    {
                        sfileIOPermission.Assert();
                        try
                        {
                            assm = Assembly.Load(assemblyName);
                        }
                        finally
                        {
                            CodeAccessPermission.RevertAssert();
                        }
                    }
                    catch (Exception)
                    {
                    }
                    if (assm == null)
                    {
                        return null;
                    }
                    typeFromAssembly = FormatterServices.GetTypeFromAssembly(assm, typeName);
                }
                if (typeFromAssembly == null)
                {
                    return null;
                }
                CheckTypeForwardedTo(assm, typeFromAssembly.Assembly, typeFromAssembly);
                cachedValue = new TypeNAssembly {
                    type = typeFromAssembly,
                    assemblyName = assemblyName
                };
                this.typeCache.SetCachedValue(cachedValue);
            }
            return cachedValue.type;
        }

        [SecurityCritical]
        internal long GetId(long objectId)
        {
            if (!this.bFullDeserialization)
            {
                this.InitFullDeserialization();
            }
            if (objectId > 0L)
            {
                return objectId;
            }
            if (!this.bOldFormatDetected && (objectId != -1L))
            {
                return (-1L * objectId);
            }
            this.bOldFormatDetected = true;
            if (this.valTypeObjectIdTable == null)
            {
                this.valTypeObjectIdTable = new IntSizedArray();
            }
            long num = 0L;
            num = this.valTypeObjectIdTable[(int) objectId];
            if (num == 0L)
            {
                num = 0x7fffffffL + objectId;
                this.valTypeObjectIdTable[(int) objectId] = (int) num;
            }
            return num;
        }

        [SecurityCritical]
        private static void GetSimplyNamedTypeFromAssembly(Assembly assm, string typeName, ref Type type)
        {
            try
            {
                type = FormatterServices.GetTypeFromAssembly(assm, typeName);
            }
            catch (TypeLoadException)
            {
            }
            catch (FileNotFoundException)
            {
            }
            catch (FileLoadException)
            {
            }
            catch (BadImageFormatException)
            {
            }
            if (type == null)
            {
                type = Type.GetType(typeName, new Func<AssemblyName, Assembly>(ObjectReader.ResolveSimpleAssemblyName), new Func<Assembly, string, bool, Type>(new TopLevelAssemblyTypeResolver(assm).ResolveType), false);
            }
        }

        [SecurityCritical]
        internal Type GetType(BinaryAssemblyInfo assemblyInfo, string name)
        {
            Type typeFromAssembly = null;
            if ((((this.previousName != null) && (this.previousName.Length == name.Length)) && (this.previousName.Equals(name) && (this.previousAssemblyString != null))) && ((this.previousAssemblyString.Length == assemblyInfo.assemblyString.Length) && this.previousAssemblyString.Equals(assemblyInfo.assemblyString)))
            {
                return this.previousType;
            }
            typeFromAssembly = this.Bind(assemblyInfo.assemblyString, name);
            if (typeFromAssembly == null)
            {
                Assembly assm = assemblyInfo.GetAssembly();
                if (this.bSimpleAssembly)
                {
                    GetSimplyNamedTypeFromAssembly(assm, name, ref typeFromAssembly);
                }
                else
                {
                    typeFromAssembly = FormatterServices.GetTypeFromAssembly(assm, name);
                }
                if (typeFromAssembly != null)
                {
                    CheckTypeForwardedTo(assm, typeFromAssembly.Assembly, typeFromAssembly);
                }
            }
            this.previousAssemblyString = assemblyInfo.assemblyString;
            this.previousName = name;
            this.previousType = typeFromAssembly;
            return typeFromAssembly;
        }

        [SecurityCritical]
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

        [SecurityCritical]
        private void InitFullDeserialization()
        {
            this.bFullDeserialization = true;
            this.stack = new SerStack("ObjectReader Object Stack");
            this.m_objectManager = new ObjectManager(this.m_surrogates, this.m_context, false, this.bIsCrossAppDomain);
            if (this.m_formatterConverter == null)
            {
                this.m_formatterConverter = new FormatterConverter();
            }
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

        [SecurityCritical]
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
            throw new SerializationException(Environment.GetResourceString("Serialization_XMLElement", new object[] { pr.PRname }));
        }

        [SecurityCritical]
        private void ParseArray(ParseRecord pr)
        {
            long pRobjectId = pr.PRobjectId;
            if (pr.PRarrayTypeEnum == InternalArrayTypeE.Base64)
            {
                if (pr.PRvalue.Length > 0)
                {
                    pr.PRnewObj = Convert.FromBase64String(pr.PRvalue);
                }
                else
                {
                    pr.PRnewObj = new byte[0];
                }
                if (this.stack.Peek() == pr)
                {
                    this.stack.Pop();
                }
                if (pr.PRobjectPositionEnum == InternalObjectPositionE.Top)
                {
                    this.TopObject = pr.PRnewObj;
                }
                ParseRecord objectPr = (ParseRecord) this.stack.Peek();
                this.RegisterObject(pr.PRnewObj, pr, objectPr);
            }
            else if ((pr.PRnewObj != null) && Converter.IsWriteAsByteArray(pr.PRarrayElementTypeCode))
            {
                if (pr.PRobjectPositionEnum == InternalObjectPositionE.Top)
                {
                    this.TopObject = pr.PRnewObj;
                }
                ParseRecord record2 = (ParseRecord) this.stack.Peek();
                this.RegisterObject(pr.PRnewObj, pr, record2);
            }
            else if ((pr.PRarrayTypeEnum == InternalArrayTypeE.Jagged) || (pr.PRarrayTypeEnum == InternalArrayTypeE.Single))
            {
                bool flag = true;
                if ((pr.PRlowerBoundA == null) || (pr.PRlowerBoundA[0] == 0))
                {
                    if (object.ReferenceEquals(pr.PRarrayElementType, Converter.typeofString))
                    {
                        pr.PRobjectA = new string[pr.PRlengthA[0]];
                        pr.PRnewObj = pr.PRobjectA;
                        flag = false;
                    }
                    else if (object.ReferenceEquals(pr.PRarrayElementType, Converter.typeofObject))
                    {
                        pr.PRobjectA = new object[pr.PRlengthA[0]];
                        pr.PRnewObj = pr.PRobjectA;
                        flag = false;
                    }
                    else if (pr.PRarrayElementType != null)
                    {
                        pr.PRnewObj = Array.UnsafeCreateInstance(pr.PRarrayElementType, pr.PRlengthA[0]);
                    }
                    pr.PRisLowerBound = false;
                }
                else
                {
                    if (pr.PRarrayElementType != null)
                    {
                        pr.PRnewObj = Array.UnsafeCreateInstance(pr.PRarrayElementType, pr.PRlengthA, pr.PRlowerBoundA);
                    }
                    pr.PRisLowerBound = true;
                }
                if (pr.PRarrayTypeEnum == InternalArrayTypeE.Single)
                {
                    if (!pr.PRisLowerBound && Converter.IsWriteAsByteArray(pr.PRarrayElementTypeCode))
                    {
                        pr.PRprimitiveArray = new PrimitiveArray(pr.PRarrayElementTypeCode, (Array) pr.PRnewObj);
                    }
                    else if ((flag && (pr.PRarrayElementType != null)) && (!pr.PRarrayElementType.IsValueType && !pr.PRisLowerBound))
                    {
                        pr.PRobjectA = (object[]) pr.PRnewObj;
                    }
                }
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
                    throw new SerializationException(Environment.GetResourceString("Serialization_ArrayType", new object[] { pr.PRarrayTypeEnum }));
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
                if (pr.PRarrayElementType != null)
                {
                    if (!pr.PRisLowerBound)
                    {
                        pr.PRnewObj = Array.UnsafeCreateInstance(pr.PRarrayElementType, pr.PRlengthA);
                    }
                    else
                    {
                        pr.PRnewObj = Array.UnsafeCreateInstance(pr.PRarrayElementType, pr.PRlengthA, pr.PRlowerBoundA);
                    }
                }
                int num2 = 1;
                for (int i = 0; i < pr.PRrank; i++)
                {
                    num2 *= pr.PRlengthA[i];
                }
                pr.PRindexMap = new int[pr.PRrank];
                pr.PRrectangularMap = new int[pr.PRrank];
                pr.PRlinearlength = num2;
            }
            this.CheckSecurity(pr);
        }

        [SecurityCritical]
        private void ParseArrayMember(ParseRecord pr)
        {
            ParseRecord record = (ParseRecord) this.stack.Peek();
            if (record.PRarrayTypeEnum == InternalArrayTypeE.Rectangular)
            {
                if (record.PRmemberIndex > 0)
                {
                    this.NextRectangleMap(record);
                }
                if (record.PRisLowerBound)
                {
                    for (int i = 0; i < record.PRrank; i++)
                    {
                        record.PRindexMap[i] = record.PRrectangularMap[i] + record.PRlowerBoundA[i];
                    }
                }
            }
            else if (!record.PRisLowerBound)
            {
                record.PRindexMap[0] = record.PRmemberIndex;
            }
            else
            {
                record.PRindexMap[0] = record.PRlowerBoundA[0] + record.PRmemberIndex;
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
                if (record.PRarrayElementType != null)
                {
                    if (record.PRarrayElementType.IsValueType && (pr.PRarrayElementTypeCode == InternalPrimitiveTypeE.Invalid))
                    {
                        pr.PRisValueTypeFixup = true;
                        this.ValueFixupStack.Push(new ValueFixup((Array) record.PRnewObj, record.PRindexMap));
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
            }
            else if (pr.PRmemberValueEnum == InternalMemberValueE.InlineValue)
            {
                if (object.ReferenceEquals(record.PRarrayElementType, Converter.typeofString) || object.ReferenceEquals(pr.PRdtType, Converter.typeofString))
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
                else if (record.PRisArrayVariant)
                {
                    if (pr.PRkeyDt == null)
                    {
                        throw new SerializationException(Environment.GetResourceString("Serialization_ArrayTypeObject"));
                    }
                    object pRvalue = null;
                    if (object.ReferenceEquals(pr.PRdtType, Converter.typeofString))
                    {
                        this.ParseString(pr, record);
                        pRvalue = pr.PRvalue;
                    }
                    else if (object.ReferenceEquals(pr.PRdtTypeCode, InternalPrimitiveTypeE.Invalid))
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
            else if (pr.PRmemberValueEnum == InternalMemberValueE.Null)
            {
                record.PRmemberIndex += pr.PRnullCount - 1;
            }
            else
            {
                this.ParseError(pr, record);
            }
            record.PRmemberIndex++;
        }

        [SecurityCritical]
        private void ParseArrayMemberEnd(ParseRecord pr)
        {
            if (pr.PRmemberValueEnum == InternalMemberValueE.Nested)
            {
                this.ParseObjectEnd(pr);
            }
        }

        private void ParseError(ParseRecord processing, ParseRecord onStack)
        {
            throw new SerializationException(Environment.GetResourceString("Serialization_ParseError", new object[] { string.Concat(new object[] { onStack.PRname, " ", onStack.PRparseTypeEnum, " ", processing.PRname, " ", processing.PRparseTypeEnum }) }));
        }

        [SecurityCritical]
        private void ParseMember(ParseRecord pr)
        {
            ParseRecord parentPr = (ParseRecord) this.stack.Peek();
            if (parentPr != null)
            {
                string pRname = parentPr.PRname;
            }
            switch (pr.PRmemberTypeEnum)
            {
                case InternalMemberTypeE.Item:
                    this.ParseArrayMember(pr);
                    return;
            }
            if ((pr.PRdtType == null) && parentPr.PRobjectInfo.isTyped)
            {
                pr.PRdtType = parentPr.PRobjectInfo.GetType(pr.PRname);
                if (pr.PRdtType != null)
                {
                    pr.PRdtTypeCode = Converter.ToCode(pr.PRdtType);
                }
            }
            if (pr.PRmemberValueEnum == InternalMemberValueE.Null)
            {
                parentPr.PRobjectInfo.AddValue(pr.PRname, null, ref parentPr.PRsi, ref parentPr.PRmemberData);
            }
            else if (pr.PRmemberValueEnum == InternalMemberValueE.Nested)
            {
                this.ParseObject(pr);
                this.stack.Push(pr);
                if (((pr.PRobjectInfo != null) && (pr.PRobjectInfo.objectType != null)) && pr.PRobjectInfo.objectType.IsValueType)
                {
                    pr.PRisValueTypeFixup = true;
                    this.ValueFixupStack.Push(new ValueFixup(parentPr.PRnewObj, pr.PRname, parentPr.PRobjectInfo));
                }
                else
                {
                    parentPr.PRobjectInfo.AddValue(pr.PRname, pr.PRnewObj, ref parentPr.PRsi, ref parentPr.PRmemberData);
                }
            }
            else if (pr.PRmemberValueEnum == InternalMemberValueE.Reference)
            {
                object obj2 = this.m_objectManager.GetObject(pr.PRidRef);
                if (obj2 == null)
                {
                    parentPr.PRobjectInfo.AddValue(pr.PRname, null, ref parentPr.PRsi, ref parentPr.PRmemberData);
                    parentPr.PRobjectInfo.RecordFixup(parentPr.PRobjectId, pr.PRname, pr.PRidRef);
                }
                else
                {
                    parentPr.PRobjectInfo.AddValue(pr.PRname, obj2, ref parentPr.PRsi, ref parentPr.PRmemberData);
                }
            }
            else if (pr.PRmemberValueEnum == InternalMemberValueE.InlineValue)
            {
                if (object.ReferenceEquals(pr.PRdtType, Converter.typeofString))
                {
                    this.ParseString(pr, parentPr);
                    parentPr.PRobjectInfo.AddValue(pr.PRname, pr.PRvalue, ref parentPr.PRsi, ref parentPr.PRmemberData);
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
                    parentPr.PRobjectInfo.AddValue(pr.PRname, pRvarValue, ref parentPr.PRsi, ref parentPr.PRmemberData);
                }
                else if (pr.PRarrayTypeEnum == InternalArrayTypeE.Base64)
                {
                    parentPr.PRobjectInfo.AddValue(pr.PRname, Convert.FromBase64String(pr.PRvalue), ref parentPr.PRsi, ref parentPr.PRmemberData);
                }
                else
                {
                    if (object.ReferenceEquals(pr.PRdtType, Converter.typeofObject))
                    {
                        throw new SerializationException(Environment.GetResourceString("Serialization_TypeMissing", new object[] { pr.PRname }));
                    }
                    this.ParseString(pr, parentPr);
                    if (object.ReferenceEquals(pr.PRdtType, Converter.typeofSystemVoid))
                    {
                        parentPr.PRobjectInfo.AddValue(pr.PRname, pr.PRdtType, ref parentPr.PRsi, ref parentPr.PRmemberData);
                    }
                    else if (parentPr.PRobjectInfo.isSi)
                    {
                        parentPr.PRobjectInfo.AddValue(pr.PRname, pr.PRvalue, ref parentPr.PRsi, ref parentPr.PRmemberData);
                    }
                }
            }
            else
            {
                this.ParseError(pr, parentPr);
            }
        }

        [SecurityCritical]
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
                    this.ParseError(pr, (ParseRecord) this.stack.Peek());
                    break;
            }
        }

        [SecurityCritical]
        private void ParseObject(ParseRecord pr)
        {
            if (!this.bFullDeserialization)
            {
                this.InitFullDeserialization();
            }
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
            else if (pr.PRdtType == null)
            {
                pr.PRnewObj = new TypeLoadExceptionHolder(pr.PRkeyDt);
            }
            else if (object.ReferenceEquals(pr.PRdtType, Converter.typeofString))
            {
                if (pr.PRvalue != null)
                {
                    pr.PRnewObj = pr.PRvalue;
                    if (pr.PRobjectPositionEnum == InternalObjectPositionE.Top)
                    {
                        this.TopObject = pr.PRnewObj;
                    }
                    else
                    {
                        this.stack.Pop();
                        this.RegisterObject(pr.PRnewObj, pr, (ParseRecord) this.stack.Peek());
                    }
                }
            }
            else
            {
                this.CheckSerializable(pr.PRdtType);
                if (this.IsRemoting && (this.formatterEnums.FEsecurityLevel != TypeFilterLevel.Full))
                {
                    pr.PRnewObj = FormatterServices.GetSafeUninitializedObject(pr.PRdtType);
                }
                else
                {
                    pr.PRnewObj = FormatterServices.GetUninitializedObject(pr.PRdtType);
                }
                this.m_objectManager.RaiseOnDeserializingEvent(pr.PRnewObj);
                if (pr.PRnewObj == null)
                {
                    throw new SerializationException(Environment.GetResourceString("Serialization_TopObjectInstantiate", new object[] { pr.PRdtType }));
                }
                if (pr.PRobjectPositionEnum == InternalObjectPositionE.Top)
                {
                    this.TopObject = pr.PRnewObj;
                }
                if (pr.PRobjectInfo == null)
                {
                    pr.PRobjectInfo = ReadObjectInfo.Create(pr.PRdtType, this.m_surrogates, this.m_context, this.m_objectManager, this.serObjectInfoInit, this.m_formatterConverter, this.bSimpleAssembly);
                }
                this.CheckSecurity(pr);
            }
        }

        [SecurityCritical]
        private void ParseObjectEnd(ParseRecord pr)
        {
            ParseRecord record = (ParseRecord) this.stack.Peek();
            if (record == null)
            {
                record = pr;
            }
            if ((record.PRobjectPositionEnum == InternalObjectPositionE.Top) && object.ReferenceEquals(record.PRdtType, Converter.typeofString))
            {
                record.PRnewObj = record.PRvalue;
                this.TopObject = record.PRnewObj;
            }
            else
            {
                this.stack.Pop();
                ParseRecord objectPr = (ParseRecord) this.stack.Peek();
                if (record.PRnewObj != null)
                {
                    if (record.PRobjectTypeEnum == InternalObjectTypeE.Array)
                    {
                        if (record.PRobjectPositionEnum == InternalObjectPositionE.Top)
                        {
                            this.TopObject = record.PRnewObj;
                        }
                        this.RegisterObject(record.PRnewObj, record, objectPr);
                    }
                    else
                    {
                        record.PRobjectInfo.PopulateObjectMembers(record.PRnewObj, record.PRmemberData);
                        if (!record.PRisRegistered && (record.PRobjectId > 0L))
                        {
                            this.RegisterObject(record.PRnewObj, record, objectPr);
                        }
                        if (record.PRisValueTypeFixup)
                        {
                            ((ValueFixup) this.ValueFixupStack.Pop()).Fixup(record, objectPr);
                        }
                        if (record.PRobjectPositionEnum == InternalObjectPositionE.Top)
                        {
                            this.TopObject = record.PRnewObj;
                        }
                        record.PRobjectInfo.ObjectEnd();
                    }
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

        [SecurityCritical]
        private void ParseString(ParseRecord pr, ParseRecord parentPr)
        {
            if (!pr.PRisRegistered && (pr.PRobjectId > 0L))
            {
                this.RegisterObject(pr.PRvalue, pr, parentPr, true);
            }
        }

        [SecurityCritical]
        private void RegisterObject(object obj, ParseRecord pr, ParseRecord objectPr)
        {
            this.RegisterObject(obj, pr, objectPr, false);
        }

        [SecurityCritical]
        private void RegisterObject(object obj, ParseRecord pr, ParseRecord objectPr, bool bIsString)
        {
            if (!pr.PRisRegistered)
            {
                pr.PRisRegistered = true;
                SerializationInfo pRsi = null;
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
                pRsi = pr.PRsi;
                if (bIsString)
                {
                    this.m_objectManager.RegisterString((string) obj, pr.PRobjectId, pRsi, idOfContainingObj, member);
                }
                else
                {
                    this.m_objectManager.RegisterObject(obj, pr.PRobjectId, pRsi, idOfContainingObj, member, arrayIndex);
                }
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining), SecurityCritical]
        private static Assembly ResolveSimpleAssemblyName(AssemblyName assemblyName)
        {
            StackCrawlMark lookForMe = StackCrawlMark.LookForMe;
            Assembly assembly = RuntimeAssembly.LoadWithPartialNameInternal(assemblyName, null, ref lookForMe);
            if ((assembly == null) && (assemblyName != null))
            {
                assembly = RuntimeAssembly.LoadWithPartialNameInternal(assemblyName.Name, null, ref lookForMe);
            }
            return assembly;
        }

        internal void SetMethodCall(BinaryMethodCall binaryMethodCall)
        {
            this.bMethodCall = true;
            this.binaryMethodCall = binaryMethodCall;
        }

        internal void SetMethodReturn(BinaryMethodReturn binaryMethodReturn)
        {
            this.bMethodReturn = true;
            this.binaryMethodReturn = binaryMethodReturn;
        }

        private bool IsRemoting
        {
            get
            {
                if (!this.bMethodCall)
                {
                    return this.bMethodReturn;
                }
                return true;
            }
        }

        internal object TopObject
        {
            get
            {
                return this.m_topObject;
            }
            set
            {
                this.m_topObject = value;
                if (this.m_objectManager != null)
                {
                    this.m_objectManager.TopObject = value;
                }
            }
        }

        private SerStack ValueFixupStack
        {
            get
            {
                if (this.valueFixupStack == null)
                {
                    this.valueFixupStack = new SerStack("ValueType Fixup Stack");
                }
                return this.valueFixupStack;
            }
        }

        internal sealed class TopLevelAssemblyTypeResolver
        {
            private Assembly m_topLevelAssembly;

            public TopLevelAssemblyTypeResolver(Assembly topLevelAssembly)
            {
                this.m_topLevelAssembly = topLevelAssembly;
            }

            public Type ResolveType(Assembly assembly, string simpleTypeName, bool ignoreCase)
            {
                if (assembly == null)
                {
                    assembly = this.m_topLevelAssembly;
                }
                return assembly.GetType(simpleTypeName, false, ignoreCase);
            }
        }

        internal class TypeNAssembly
        {
            public string assemblyName;
            public Type type;
        }
    }
}

