namespace System.Runtime.Serialization.Json
{
    using System;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Security.Permissions;
    using System.Xml;

    internal class JsonFormatWriterGenerator
    {
        [SecurityCritical]
        private CriticalHelper helper = new CriticalHelper();

        [SecurityCritical]
        internal JsonFormatClassWriterDelegate GenerateClassWriter(ClassDataContract classContract)
        {
            return this.helper.GenerateClassWriter(classContract);
        }

        [SecurityCritical]
        internal JsonFormatCollectionWriterDelegate GenerateCollectionWriter(CollectionDataContract collectionContract)
        {
            return this.helper.GenerateCollectionWriter(collectionContract);
        }

        private class CriticalHelper
        {
            private int childElementIndex;
            private ArgBuilder contextArg;
            private ArgBuilder dataContractArg;
            private CodeGenerator ilg;
            private ArgBuilder memberNamesArg;
            private LocalBuilder objectLocal;
            private int typeIndex = 1;
            private ArgBuilder xmlWriterArg;

            private void BeginMethod(CodeGenerator ilg, string methodName, Type delegateType, bool allowPrivateMemberAccess)
            {
                MethodInfo info = delegateType.GetMethod("Invoke");
                ParameterInfo[] parameters = info.GetParameters();
                Type[] parameterTypes = new Type[parameters.Length];
                for (int i = 0; i < parameters.Length; i++)
                {
                    parameterTypes[i] = parameters[i].ParameterType;
                }
                DynamicMethod dynamicMethod = new DynamicMethod(methodName, info.ReturnType, parameterTypes, typeof(JsonFormatWriterGenerator).Module, allowPrivateMemberAccess);
                ilg.BeginMethod(dynamicMethod, delegateType, methodName, parameterTypes, allowPrivateMemberAccess);
            }

            private void DemandMemberAccessPermission(bool memberAccessFlag)
            {
                if (memberAccessFlag)
                {
                    this.ilg.Call(this.contextArg, XmlFormatGeneratorStatics.DemandMemberAccessPermissionMethod);
                }
            }

            private void DemandSerializationFormatterPermission(ClassDataContract classContract)
            {
                if (!classContract.HasDataContract && !classContract.IsNonAttributedType)
                {
                    this.ilg.Call(this.contextArg, XmlFormatGeneratorStatics.DemandSerializationFormatterPermissionMethod);
                }
            }

            internal JsonFormatClassWriterDelegate GenerateClassWriter(ClassDataContract classContract)
            {
                this.ilg = new CodeGenerator();
                bool allowPrivateMemberAccess = classContract.RequiresMemberAccessForWrite(null);
                try
                {
                    this.BeginMethod(this.ilg, "Write" + classContract.StableName.Name + "ToJson", typeof(JsonFormatClassWriterDelegate), allowPrivateMemberAccess);
                }
                catch (SecurityException exception)
                {
                    if (!allowPrivateMemberAccess || !exception.PermissionType.Equals(typeof(ReflectionPermission)))
                    {
                        throw;
                    }
                    classContract.RequiresMemberAccessForWrite(exception);
                }
                this.InitArgs(classContract.UnderlyingType);
                this.memberNamesArg = this.ilg.GetArg(4);
                this.DemandSerializationFormatterPermission(classContract);
                this.DemandMemberAccessPermission(allowPrivateMemberAccess);
                this.WriteClass(classContract);
                return (JsonFormatClassWriterDelegate) this.ilg.EndMethod();
            }

            internal JsonFormatCollectionWriterDelegate GenerateCollectionWriter(CollectionDataContract collectionContract)
            {
                this.ilg = new CodeGenerator();
                bool allowPrivateMemberAccess = collectionContract.RequiresMemberAccessForWrite(null);
                try
                {
                    this.BeginMethod(this.ilg, "Write" + collectionContract.StableName.Name + "ToJson", typeof(JsonFormatCollectionWriterDelegate), allowPrivateMemberAccess);
                }
                catch (SecurityException exception)
                {
                    if (!allowPrivateMemberAccess || !exception.PermissionType.Equals(typeof(ReflectionPermission)))
                    {
                        throw;
                    }
                    collectionContract.RequiresMemberAccessForWrite(exception);
                }
                this.InitArgs(collectionContract.UnderlyingType);
                this.DemandMemberAccessPermission(allowPrivateMemberAccess);
                this.WriteCollection(collectionContract);
                return (JsonFormatCollectionWriterDelegate) this.ilg.EndMethod();
            }

            private void InitArgs(Type objType)
            {
                this.xmlWriterArg = this.ilg.GetArg(0);
                this.contextArg = this.ilg.GetArg(2);
                this.dataContractArg = this.ilg.GetArg(3);
                this.objectLocal = this.ilg.DeclareLocal(objType, "objSerialized");
                ArgBuilder arg = this.ilg.GetArg(1);
                this.ilg.Load(arg);
                if (objType == Globals.TypeOfDateTimeOffsetAdapter)
                {
                    this.ilg.ConvertValue(arg.ArgType, Globals.TypeOfDateTimeOffset);
                    this.ilg.Call(XmlFormatGeneratorStatics.GetDateTimeOffsetAdapterMethod);
                }
                else
                {
                    this.ilg.ConvertValue(arg.ArgType, objType);
                }
                this.ilg.Stloc(this.objectLocal);
            }

            private void InternalSerialize(MethodInfo methodInfo, LocalBuilder memberValue, Type memberType, bool writeXsiType)
            {
                this.ilg.Load(this.contextArg);
                this.ilg.Load(this.xmlWriterArg);
                this.ilg.Load(memberValue);
                this.ilg.ConvertValue(memberValue.LocalType, Globals.TypeOfObject);
                LocalBuilder local = this.ilg.DeclareLocal(typeof(RuntimeTypeHandle), "typeHandleValue");
                this.ilg.Call(null, typeof(Type).GetMethod("GetTypeHandle"), memberValue);
                this.ilg.Stloc(local);
                this.ilg.LoadAddress(local);
                this.ilg.Ldtoken(memberType);
                this.ilg.Call(typeof(RuntimeTypeHandle).GetMethod("Equals", new Type[] { typeof(RuntimeTypeHandle) }));
                this.ilg.Load(writeXsiType);
                this.ilg.Load(DataContract.GetId(memberType.TypeHandle));
                this.ilg.Ldtoken(memberType);
                this.ilg.Call(methodInfo);
            }

            private void InvokeOnSerialized(ClassDataContract classContract)
            {
                if (classContract.BaseContract != null)
                {
                    this.InvokeOnSerialized(classContract.BaseContract);
                }
                if (classContract.OnSerialized != null)
                {
                    this.ilg.LoadAddress(this.objectLocal);
                    this.ilg.Load(this.contextArg);
                    this.ilg.Call(XmlFormatGeneratorStatics.GetStreamingContextMethod);
                    this.ilg.Call(classContract.OnSerialized);
                }
            }

            private void InvokeOnSerializing(ClassDataContract classContract)
            {
                if (classContract.BaseContract != null)
                {
                    this.InvokeOnSerializing(classContract.BaseContract);
                }
                if (classContract.OnSerializing != null)
                {
                    this.ilg.LoadAddress(this.objectLocal);
                    this.ilg.Load(this.contextArg);
                    this.ilg.Call(XmlFormatGeneratorStatics.GetStreamingContextMethod);
                    this.ilg.Call(classContract.OnSerializing);
                }
            }

            private LocalBuilder LoadMemberValue(DataMember member)
            {
                this.ilg.LoadAddress(this.objectLocal);
                this.ilg.LoadMember(member.MemberInfo);
                LocalBuilder local = this.ilg.DeclareLocal(member.MemberType, member.Name + "Value");
                this.ilg.Stloc(local);
                return local;
            }

            private bool TryWritePrimitive(Type type, LocalBuilder value, MemberInfo memberInfo, LocalBuilder arrayItemIndex, LocalBuilder name, int nameIndex)
            {
                PrimitiveDataContract primitiveDataContract = PrimitiveDataContract.GetPrimitiveDataContract(type);
                if ((primitiveDataContract == null) || (primitiveDataContract.UnderlyingType == Globals.TypeOfObject))
                {
                    return false;
                }
                if (type.IsValueType)
                {
                    this.ilg.Load(this.xmlWriterArg);
                }
                else
                {
                    this.ilg.Load(this.contextArg);
                    this.ilg.Load(this.xmlWriterArg);
                }
                if (value != null)
                {
                    this.ilg.Load(value);
                }
                else if (memberInfo != null)
                {
                    this.ilg.LoadAddress(this.objectLocal);
                    this.ilg.LoadMember(memberInfo);
                }
                else
                {
                    this.ilg.LoadArrayElement(this.objectLocal, arrayItemIndex);
                }
                if (name != null)
                {
                    this.ilg.Load(name);
                }
                else
                {
                    this.ilg.LoadArrayElement(this.memberNamesArg, nameIndex);
                }
                this.ilg.Load(null);
                this.ilg.Call(primitiveDataContract.XmlFormatWriterMethod);
                return true;
            }

            private bool TryWritePrimitiveArray(Type type, Type itemType, LocalBuilder value, LocalBuilder itemName)
            {
                if (PrimitiveDataContract.GetPrimitiveDataContract(itemType) != null)
                {
                    string name = null;
                    switch (Type.GetTypeCode(itemType))
                    {
                        case TypeCode.Int32:
                            name = "WriteJsonInt32Array";
                            break;

                        case TypeCode.Int64:
                            name = "WriteJsonInt64Array";
                            break;

                        case TypeCode.Single:
                            name = "WriteJsonSingleArray";
                            break;

                        case TypeCode.Double:
                            name = "WriteJsonDoubleArray";
                            break;

                        case TypeCode.Decimal:
                            name = "WriteJsonDecimalArray";
                            break;

                        case TypeCode.DateTime:
                            name = "WriteJsonDateTimeArray";
                            break;

                        case TypeCode.Boolean:
                            name = "WriteJsonBooleanArray";
                            break;
                    }
                    if (name != null)
                    {
                        this.WriteArrayAttribute();
                        this.ilg.Call(this.xmlWriterArg, typeof(JsonWriterDelegator).GetMethod(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance, null, new Type[] { type, typeof(XmlDictionaryString), typeof(XmlDictionaryString) }, null), value, itemName, null);
                        return true;
                    }
                }
                return false;
            }

            private LocalBuilder UnwrapNullableObject(LocalBuilder memberValue)
            {
                Type localType = memberValue.LocalType;
                Label label = this.ilg.DefineLabel();
                Label label2 = this.ilg.DefineLabel();
                this.ilg.Load(memberValue);
                while (localType.IsGenericType && (localType.GetGenericTypeDefinition() == Globals.TypeOfNullable))
                {
                    Type type2 = localType.GetGenericArguments()[0];
                    this.ilg.Dup();
                    this.ilg.Call(XmlFormatGeneratorStatics.GetHasValueMethod.MakeGenericMethod(new Type[] { type2 }));
                    this.ilg.Brfalse(label);
                    this.ilg.Call(XmlFormatGeneratorStatics.GetNullableValueMethod.MakeGenericMethod(new Type[] { type2 }));
                    localType = type2;
                }
                memberValue = this.ilg.DeclareLocal(localType, "nullableUnwrappedMemberValue");
                this.ilg.Stloc(memberValue);
                this.ilg.Load(false);
                this.ilg.Br(label2);
                this.ilg.MarkLabel(label);
                this.ilg.Pop();
                this.ilg.Call(XmlFormatGeneratorStatics.GetDefaultValueMethod.MakeGenericMethod(new Type[] { localType }));
                this.ilg.Stloc(memberValue);
                this.ilg.Load(true);
                this.ilg.MarkLabel(label2);
                return memberValue;
            }

            private void WriteArrayAttribute()
            {
                this.ilg.Call(this.xmlWriterArg, JsonFormatGeneratorStatics.WriteAttributeStringMethod, null, "type", string.Empty, "array");
            }

            private void WriteClass(ClassDataContract classContract)
            {
                this.InvokeOnSerializing(classContract);
                if (classContract.IsISerializable)
                {
                    this.ilg.Call(this.contextArg, JsonFormatGeneratorStatics.WriteJsonISerializableMethod, this.xmlWriterArg, this.objectLocal);
                }
                else if (classContract.HasExtensionData)
                {
                    LocalBuilder var = this.ilg.DeclareLocal(Globals.TypeOfExtensionDataObject, "extensionData");
                    this.ilg.Load(this.objectLocal);
                    this.ilg.ConvertValue(this.objectLocal.LocalType, Globals.TypeOfIExtensibleDataObject);
                    this.ilg.LoadMember(JsonFormatGeneratorStatics.ExtensionDataProperty);
                    this.ilg.Store(var);
                    this.ilg.Call(this.contextArg, XmlFormatGeneratorStatics.WriteExtensionDataMethod, this.xmlWriterArg, var, -1);
                    this.WriteMembers(classContract, var, classContract);
                }
                else
                {
                    this.WriteMembers(classContract, null, classContract);
                }
                this.InvokeOnSerialized(classContract);
            }

            private void WriteCollection(CollectionDataContract collectionContract)
            {
                LocalBuilder var = this.ilg.DeclareLocal(typeof(XmlDictionaryString), "itemName");
                this.ilg.Load(this.contextArg);
                this.ilg.LoadMember(JsonFormatGeneratorStatics.CollectionItemNameProperty);
                this.ilg.Store(var);
                if (collectionContract.Kind == CollectionKind.Array)
                {
                    Type itemType = collectionContract.ItemType;
                    LocalBuilder local = this.ilg.DeclareLocal(Globals.TypeOfInt, "i");
                    this.ilg.Call(this.contextArg, XmlFormatGeneratorStatics.IncrementArrayCountMethod, this.xmlWriterArg, this.objectLocal);
                    if (!this.TryWritePrimitiveArray(collectionContract.UnderlyingType, itemType, this.objectLocal, var))
                    {
                        this.WriteArrayAttribute();
                        this.ilg.For(local, 0, this.objectLocal);
                        if (!this.TryWritePrimitive(itemType, null, null, local, var, 0))
                        {
                            this.WriteStartElement(var, 0);
                            this.ilg.LoadArrayElement(this.objectLocal, local);
                            LocalBuilder builder3 = this.ilg.DeclareLocal(itemType, "memberValue");
                            this.ilg.Stloc(builder3);
                            this.WriteValue(builder3);
                            this.WriteEndElement();
                        }
                        this.ilg.EndFor();
                    }
                }
                else
                {
                    MethodInfo methodInfo = null;
                    switch (collectionContract.Kind)
                    {
                        case CollectionKind.GenericDictionary:
                            methodInfo = XmlFormatGeneratorStatics.IncrementCollectionCountGenericMethod.MakeGenericMethod(new Type[] { Globals.TypeOfKeyValuePair.MakeGenericType(collectionContract.ItemType.GetGenericArguments()) });
                            break;

                        case CollectionKind.Dictionary:
                        case CollectionKind.List:
                        case CollectionKind.Collection:
                            methodInfo = XmlFormatGeneratorStatics.IncrementCollectionCountMethod;
                            break;

                        case CollectionKind.GenericList:
                        case CollectionKind.GenericCollection:
                            methodInfo = XmlFormatGeneratorStatics.IncrementCollectionCountGenericMethod.MakeGenericMethod(new Type[] { collectionContract.ItemType });
                            break;
                    }
                    if (methodInfo != null)
                    {
                        this.ilg.Call(this.contextArg, methodInfo, this.xmlWriterArg, this.objectLocal);
                    }
                    bool flag = false;
                    bool flag2 = false;
                    Type type = null;
                    Type[] typeArguments = null;
                    if (collectionContract.Kind == CollectionKind.GenericDictionary)
                    {
                        flag2 = true;
                        typeArguments = collectionContract.ItemType.GetGenericArguments();
                        type = Globals.TypeOfGenericDictionaryEnumerator.MakeGenericType(typeArguments);
                    }
                    else if (collectionContract.Kind == CollectionKind.Dictionary)
                    {
                        flag = true;
                        typeArguments = new Type[] { Globals.TypeOfObject, Globals.TypeOfObject };
                        type = Globals.TypeOfDictionaryEnumerator;
                    }
                    else
                    {
                        type = collectionContract.GetEnumeratorMethod.ReturnType;
                    }
                    MethodInfo moveNextMethod = type.GetMethod("MoveNext", BindingFlags.Public | BindingFlags.Instance, null, Globals.EmptyTypeArray, null);
                    MethodInfo getCurrentMethod = type.GetMethod("get_Current", BindingFlags.Public | BindingFlags.Instance, null, Globals.EmptyTypeArray, null);
                    if ((moveNextMethod == null) || (getCurrentMethod == null))
                    {
                        if (type.IsInterface)
                        {
                            if (moveNextMethod == null)
                            {
                                moveNextMethod = JsonFormatGeneratorStatics.MoveNextMethod;
                            }
                            if (getCurrentMethod == null)
                            {
                                getCurrentMethod = JsonFormatGeneratorStatics.GetCurrentMethod;
                            }
                        }
                        else
                        {
                            Type typeOfIEnumerator = Globals.TypeOfIEnumerator;
                            switch (collectionContract.Kind)
                            {
                                case CollectionKind.GenericDictionary:
                                case CollectionKind.GenericCollection:
                                case CollectionKind.GenericEnumerable:
                                    foreach (Type type4 in type.GetInterfaces())
                                    {
                                        if ((type4.IsGenericType && (type4.GetGenericTypeDefinition() == Globals.TypeOfIEnumeratorGeneric)) && (type4.GetGenericArguments()[0] == collectionContract.ItemType))
                                        {
                                            typeOfIEnumerator = type4;
                                            break;
                                        }
                                    }
                                    break;
                            }
                            if (moveNextMethod == null)
                            {
                                moveNextMethod = CollectionDataContract.GetTargetMethodWithName("MoveNext", type, typeOfIEnumerator);
                            }
                            if (getCurrentMethod == null)
                            {
                                getCurrentMethod = CollectionDataContract.GetTargetMethodWithName("get_Current", type, typeOfIEnumerator);
                            }
                        }
                    }
                    Type returnType = getCurrentMethod.ReturnType;
                    LocalBuilder builder4 = this.ilg.DeclareLocal(returnType, "currentValue");
                    LocalBuilder builder5 = this.ilg.DeclareLocal(type, "enumerator");
                    this.ilg.Call(this.objectLocal, collectionContract.GetEnumeratorMethod);
                    if (flag)
                    {
                        ConstructorInfo constructorInfo = type.GetConstructor(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance, null, new Type[] { Globals.TypeOfIDictionaryEnumerator }, null);
                        this.ilg.ConvertValue(collectionContract.GetEnumeratorMethod.ReturnType, Globals.TypeOfIDictionaryEnumerator);
                        this.ilg.New(constructorInfo);
                    }
                    else if (flag2)
                    {
                        Type target = Globals.TypeOfIEnumeratorGeneric.MakeGenericType(new Type[] { Globals.TypeOfKeyValuePair.MakeGenericType(typeArguments) });
                        ConstructorInfo info5 = type.GetConstructor(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance, null, new Type[] { target }, null);
                        this.ilg.ConvertValue(collectionContract.GetEnumeratorMethod.ReturnType, target);
                        this.ilg.New(info5);
                    }
                    this.ilg.Stloc(builder5);
                    this.WriteArrayAttribute();
                    this.ilg.ForEach(builder4, returnType, type, builder5, getCurrentMethod);
                    if (methodInfo == null)
                    {
                        this.ilg.Call(this.contextArg, XmlFormatGeneratorStatics.IncrementItemCountMethod, 1);
                    }
                    if (!this.TryWritePrimitive(returnType, builder4, null, null, var, 0))
                    {
                        this.WriteStartElement(var, 0);
                        if (flag2 || flag)
                        {
                            this.ilg.Call(this.dataContractArg, JsonFormatGeneratorStatics.GetItemContractMethod);
                            this.ilg.Call(JsonFormatGeneratorStatics.GetRevisedItemContractMethod);
                            this.ilg.Call(JsonFormatGeneratorStatics.GetJsonDataContractMethod);
                            this.ilg.Load(this.xmlWriterArg);
                            this.ilg.Load(builder4);
                            this.ilg.ConvertValue(builder4.LocalType, Globals.TypeOfObject);
                            this.ilg.Load(this.contextArg);
                            this.ilg.Load(builder4.LocalType);
                            this.ilg.LoadMember(JsonFormatGeneratorStatics.TypeHandleProperty);
                            this.ilg.Call(JsonFormatGeneratorStatics.WriteJsonValueMethod);
                        }
                        else
                        {
                            this.WriteValue(builder4);
                        }
                        this.WriteEndElement();
                    }
                    this.ilg.EndForEach(moveNextMethod);
                }
            }

            private void WriteEndElement()
            {
                this.ilg.Call(this.xmlWriterArg, JsonFormatGeneratorStatics.WriteEndElementMethod);
            }

            private int WriteMembers(ClassDataContract classContract, LocalBuilder extensionDataLocal, ClassDataContract derivedMostClassContract)
            {
                int num = (classContract.BaseContract == null) ? 0 : this.WriteMembers(classContract.BaseContract, extensionDataLocal, derivedMostClassContract);
                this.ilg.Call(this.contextArg, XmlFormatGeneratorStatics.IncrementItemCountMethod, classContract.Members.Count);
                int index = 0;
                while (index < classContract.Members.Count)
                {
                    DataMember member = classContract.Members[index];
                    Type memberType = member.MemberType;
                    LocalBuilder builder = null;
                    if (member.IsGetOnlyCollection)
                    {
                        this.ilg.Load(this.contextArg);
                        this.ilg.Call(XmlFormatGeneratorStatics.StoreIsGetOnlyCollectionMethod);
                    }
                    if (!member.EmitDefaultValue)
                    {
                        builder = this.LoadMemberValue(member);
                        this.ilg.IfNotDefaultValue(builder);
                    }
                    bool flag = DataContractJsonSerializer.CheckIfXmlNameRequiresMapping(classContract.MemberNames[index]);
                    if (flag || !this.TryWritePrimitive(memberType, builder, member.MemberInfo, null, null, index + this.childElementIndex))
                    {
                        if (flag)
                        {
                            this.ilg.Call(null, JsonFormatGeneratorStatics.WriteJsonNameWithMappingMethod, this.xmlWriterArg, this.memberNamesArg, index + this.childElementIndex);
                        }
                        else
                        {
                            this.WriteStartElement(null, index + this.childElementIndex);
                        }
                        if (builder == null)
                        {
                            builder = this.LoadMemberValue(member);
                        }
                        this.WriteValue(builder);
                        this.WriteEndElement();
                    }
                    if (classContract.HasExtensionData)
                    {
                        this.ilg.Call(this.contextArg, XmlFormatGeneratorStatics.WriteExtensionDataMethod, this.xmlWriterArg, extensionDataLocal, num);
                    }
                    if (!member.EmitDefaultValue)
                    {
                        if (member.IsRequired)
                        {
                            this.ilg.Else();
                            this.ilg.Call(null, XmlFormatGeneratorStatics.ThrowRequiredMemberMustBeEmittedMethod, member.Name, classContract.UnderlyingType);
                        }
                        this.ilg.EndIf();
                    }
                    index++;
                    num++;
                }
                this.typeIndex++;
                this.childElementIndex += classContract.Members.Count;
                return num;
            }

            private void WriteStartElement(LocalBuilder nameLocal, int nameIndex)
            {
                this.ilg.Load(this.xmlWriterArg);
                if (nameLocal == null)
                {
                    this.ilg.LoadArrayElement(this.memberNamesArg, nameIndex);
                }
                else
                {
                    this.ilg.Load(nameLocal);
                }
                this.ilg.Load(null);
                this.ilg.Call(JsonFormatGeneratorStatics.WriteStartElementMethod);
            }

            private void WriteValue(LocalBuilder memberValue)
            {
                Type localType = memberValue.LocalType;
                if (localType.IsPointer)
                {
                    this.ilg.Load(memberValue);
                    this.ilg.Load(localType);
                    this.ilg.Call(JsonFormatGeneratorStatics.BoxPointer);
                    localType = Globals.TypeOfReflectionPointer;
                    memberValue = this.ilg.DeclareLocal(localType, "memberValueRefPointer");
                    this.ilg.Store(memberValue);
                }
                bool flag = localType.IsGenericType && (localType.GetGenericTypeDefinition() == Globals.TypeOfNullable);
                if (localType.IsValueType && !flag)
                {
                    PrimitiveDataContract primitiveDataContract = PrimitiveDataContract.GetPrimitiveDataContract(localType);
                    if (primitiveDataContract != null)
                    {
                        this.ilg.Call(this.xmlWriterArg, primitiveDataContract.XmlFormatContentWriterMethod, memberValue);
                    }
                    else
                    {
                        this.InternalSerialize(XmlFormatGeneratorStatics.InternalSerializeMethod, memberValue, localType, false);
                    }
                }
                else
                {
                    if (flag)
                    {
                        memberValue = this.UnwrapNullableObject(memberValue);
                        localType = memberValue.LocalType;
                    }
                    else
                    {
                        this.ilg.Load(memberValue);
                        this.ilg.Load(null);
                        this.ilg.Ceq();
                    }
                    this.ilg.If();
                    this.ilg.Call(this.contextArg, XmlFormatGeneratorStatics.WriteNullMethod, this.xmlWriterArg, localType, DataContract.IsTypeSerializable(localType));
                    this.ilg.Else();
                    PrimitiveDataContract contract2 = PrimitiveDataContract.GetPrimitiveDataContract(localType);
                    if ((contract2 != null) && (contract2.UnderlyingType != Globals.TypeOfObject))
                    {
                        if (flag)
                        {
                            this.ilg.Call(this.xmlWriterArg, contract2.XmlFormatContentWriterMethod, memberValue);
                        }
                        else
                        {
                            this.ilg.Call(this.contextArg, contract2.XmlFormatContentWriterMethod, this.xmlWriterArg, memberValue);
                        }
                    }
                    else
                    {
                        if (((localType == Globals.TypeOfObject) || (localType == Globals.TypeOfValueType)) || Globals.TypeOfNullable.GetInterfaces().Contains(localType))
                        {
                            this.ilg.Load(memberValue);
                            this.ilg.ConvertValue(memberValue.LocalType, Globals.TypeOfObject);
                            memberValue = this.ilg.DeclareLocal(Globals.TypeOfObject, "unwrappedMemberValue");
                            localType = memberValue.LocalType;
                            this.ilg.Stloc(memberValue);
                            this.ilg.If(memberValue, Cmp.EqualTo, null);
                            this.ilg.Call(this.contextArg, XmlFormatGeneratorStatics.WriteNullMethod, this.xmlWriterArg, localType, DataContract.IsTypeSerializable(localType));
                            this.ilg.Else();
                        }
                        this.InternalSerialize(flag ? XmlFormatGeneratorStatics.InternalSerializeMethod : XmlFormatGeneratorStatics.InternalSerializeReferenceMethod, memberValue, localType, false);
                        if (localType == Globals.TypeOfObject)
                        {
                            this.ilg.EndIf();
                        }
                    }
                    this.ilg.EndIf();
                }
            }
        }
    }
}

