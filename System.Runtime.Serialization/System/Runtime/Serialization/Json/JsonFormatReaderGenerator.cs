namespace System.Runtime.Serialization.Json
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Security.Permissions;
    using System.Xml;

    internal sealed class JsonFormatReaderGenerator
    {
        [SecurityCritical]
        private CriticalHelper helper = new CriticalHelper();

        [SecurityCritical]
        public JsonFormatClassReaderDelegate GenerateClassReader(ClassDataContract classContract)
        {
            return this.helper.GenerateClassReader(classContract);
        }

        [SecurityCritical]
        public JsonFormatCollectionReaderDelegate GenerateCollectionReader(CollectionDataContract collectionContract)
        {
            return this.helper.GenerateCollectionReader(collectionContract);
        }

        [SecurityCritical]
        public JsonFormatGetOnlyCollectionReaderDelegate GenerateGetOnlyCollectionReader(CollectionDataContract collectionContract)
        {
            return this.helper.GenerateGetOnlyCollectionReader(collectionContract);
        }

        private class CriticalHelper
        {
            private ArgBuilder collectionContractArg;
            private ArgBuilder contextArg;
            private ArgBuilder emptyDictionaryStringArg;
            private CodeGenerator ilg;
            private ArgBuilder memberNamesArg;
            private LocalBuilder objectLocal;
            private Type objectType;
            private ArgBuilder xmlReaderArg;

            private void BeginMethod(CodeGenerator ilg, string methodName, Type delegateType, bool allowPrivateMemberAccess)
            {
                MethodInfo info = delegateType.GetMethod("Invoke");
                ParameterInfo[] parameters = info.GetParameters();
                Type[] parameterTypes = new Type[parameters.Length];
                for (int i = 0; i < parameters.Length; i++)
                {
                    parameterTypes[i] = parameters[i].ParameterType;
                }
                DynamicMethod dynamicMethod = new DynamicMethod(methodName, info.ReturnType, parameterTypes, typeof(JsonFormatReaderGenerator).Module, allowPrivateMemberAccess);
                ilg.BeginMethod(dynamicMethod, delegateType, methodName, parameterTypes, allowPrivateMemberAccess);
            }

            private void CheckRequiredElements(BitFlagsGenerator expectedElements, byte[] requiredElements, Label throwMissingRequiredMembersLabel)
            {
                for (int i = 0; i < requiredElements.Length; i++)
                {
                    this.ilg.Load(expectedElements.GetLocal(i));
                    this.ilg.Load(requiredElements[i]);
                    this.ilg.And();
                    this.ilg.Load(0);
                    this.ilg.Ceq();
                    this.ilg.Brfalse(throwMissingRequiredMembersLabel);
                }
            }

            private void CreateObject(ClassDataContract classContract)
            {
                Type typeOfValueType = this.objectType = classContract.UnderlyingType;
                if (typeOfValueType.IsValueType && !classContract.IsNonAttributedType)
                {
                    typeOfValueType = Globals.TypeOfValueType;
                }
                this.objectLocal = this.ilg.DeclareLocal(typeOfValueType, "objectDeserialized");
                if (classContract.UnderlyingType == Globals.TypeOfDBNull)
                {
                    this.ilg.LoadMember(Globals.TypeOfDBNull.GetField("Value"));
                    this.ilg.Stloc(this.objectLocal);
                }
                else if (classContract.IsNonAttributedType)
                {
                    if (typeOfValueType.IsValueType)
                    {
                        this.ilg.Ldloca(this.objectLocal);
                        this.ilg.InitObj(typeOfValueType);
                    }
                    else
                    {
                        this.ilg.New(classContract.GetNonAttributedTypeConstructor());
                        this.ilg.Stloc(this.objectLocal);
                    }
                }
                else
                {
                    this.ilg.Call(null, JsonFormatGeneratorStatics.GetUninitializedObjectMethod, DataContract.GetIdForInitialization(classContract));
                    this.ilg.ConvertValue(Globals.TypeOfObject, typeOfValueType);
                    this.ilg.Stloc(this.objectLocal);
                }
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

            public JsonFormatClassReaderDelegate GenerateClassReader(ClassDataContract classContract)
            {
                this.ilg = new CodeGenerator();
                bool allowPrivateMemberAccess = classContract.RequiresMemberAccessForRead(null);
                try
                {
                    this.BeginMethod(this.ilg, "Read" + classContract.StableName.Name + "FromJson", typeof(JsonFormatClassReaderDelegate), allowPrivateMemberAccess);
                }
                catch (SecurityException exception)
                {
                    if (!allowPrivateMemberAccess || !exception.PermissionType.Equals(typeof(ReflectionPermission)))
                    {
                        throw;
                    }
                    classContract.RequiresMemberAccessForRead(exception);
                }
                this.InitArgs();
                this.DemandSerializationFormatterPermission(classContract);
                this.DemandMemberAccessPermission(allowPrivateMemberAccess);
                this.CreateObject(classContract);
                this.ilg.Call(this.contextArg, XmlFormatGeneratorStatics.AddNewObjectMethod, this.objectLocal);
                this.InvokeOnDeserializing(classContract);
                if (classContract.IsISerializable)
                {
                    this.ReadISerializable(classContract);
                }
                else
                {
                    this.ReadClass(classContract);
                }
                if (Globals.TypeOfIDeserializationCallback.IsAssignableFrom(classContract.UnderlyingType))
                {
                    this.ilg.Call(this.objectLocal, JsonFormatGeneratorStatics.OnDeserializationMethod, null);
                }
                this.InvokeOnDeserialized(classContract);
                if (!this.InvokeFactoryMethod(classContract))
                {
                    this.ilg.Load(this.objectLocal);
                    if (classContract.UnderlyingType == Globals.TypeOfDateTimeOffsetAdapter)
                    {
                        this.ilg.ConvertValue(this.objectLocal.LocalType, Globals.TypeOfDateTimeOffsetAdapter);
                        this.ilg.Call(XmlFormatGeneratorStatics.GetDateTimeOffsetMethod);
                        this.ilg.ConvertValue(Globals.TypeOfDateTimeOffset, this.ilg.CurrentMethod.ReturnType);
                    }
                    else
                    {
                        this.ilg.ConvertValue(this.objectLocal.LocalType, this.ilg.CurrentMethod.ReturnType);
                    }
                }
                return (JsonFormatClassReaderDelegate) this.ilg.EndMethod();
            }

            public JsonFormatCollectionReaderDelegate GenerateCollectionReader(CollectionDataContract collectionContract)
            {
                this.ilg = this.GenerateCollectionReaderHelper(collectionContract, false);
                this.ReadCollection(collectionContract);
                this.ilg.Load(this.objectLocal);
                this.ilg.ConvertValue(this.objectLocal.LocalType, this.ilg.CurrentMethod.ReturnType);
                return (JsonFormatCollectionReaderDelegate) this.ilg.EndMethod();
            }

            private CodeGenerator GenerateCollectionReaderHelper(CollectionDataContract collectionContract, bool isGetOnlyCollection)
            {
                this.ilg = new CodeGenerator();
                bool allowPrivateMemberAccess = collectionContract.RequiresMemberAccessForRead(null);
                try
                {
                    if (isGetOnlyCollection)
                    {
                        this.BeginMethod(this.ilg, "Read" + collectionContract.StableName.Name + "FromJsonIsGetOnly", typeof(JsonFormatGetOnlyCollectionReaderDelegate), allowPrivateMemberAccess);
                    }
                    else
                    {
                        this.BeginMethod(this.ilg, "Read" + collectionContract.StableName.Name + "FromJson", typeof(JsonFormatCollectionReaderDelegate), allowPrivateMemberAccess);
                    }
                }
                catch (SecurityException exception)
                {
                    if (!allowPrivateMemberAccess || !exception.PermissionType.Equals(typeof(ReflectionPermission)))
                    {
                        throw;
                    }
                    collectionContract.RequiresMemberAccessForRead(exception);
                }
                this.InitArgs();
                this.DemandMemberAccessPermission(allowPrivateMemberAccess);
                this.collectionContractArg = this.ilg.GetArg(4);
                return this.ilg;
            }

            public JsonFormatGetOnlyCollectionReaderDelegate GenerateGetOnlyCollectionReader(CollectionDataContract collectionContract)
            {
                this.ilg = this.GenerateCollectionReaderHelper(collectionContract, true);
                this.ReadGetOnlyCollection(collectionContract);
                return (JsonFormatGetOnlyCollectionReaderDelegate) this.ilg.EndMethod();
            }

            private void HandleUnexpectedItemInCollection(LocalBuilder iterator)
            {
                this.IsStartElement();
                this.ilg.If();
                this.ilg.Call(this.contextArg, XmlFormatGeneratorStatics.SkipUnknownElementMethod, this.xmlReaderArg);
                this.ilg.Dec(iterator);
                this.ilg.Else();
                this.ThrowUnexpectedStateException(XmlNodeType.Element);
                this.ilg.EndIf();
            }

            private bool HasFactoryMethod(ClassDataContract classContract)
            {
                return Globals.TypeOfIObjectReference.IsAssignableFrom(classContract.UnderlyingType);
            }

            private void InitArgs()
            {
                this.xmlReaderArg = this.ilg.GetArg(0);
                this.contextArg = this.ilg.GetArg(1);
                this.emptyDictionaryStringArg = this.ilg.GetArg(2);
                this.memberNamesArg = this.ilg.GetArg(3);
            }

            private void InternalDeserialize(LocalBuilder value, Type type, string name)
            {
                this.ilg.Load(this.contextArg);
                this.ilg.Load(this.xmlReaderArg);
                Type t = type.IsPointer ? Globals.TypeOfReflectionPointer : type;
                this.ilg.Load(DataContract.GetId(t.TypeHandle));
                this.ilg.Ldtoken(t);
                this.ilg.Load(name);
                this.ilg.Load(string.Empty);
                this.ilg.Call(XmlFormatGeneratorStatics.InternalDeserializeMethod);
                if (type.IsPointer)
                {
                    this.ilg.Call(JsonFormatGeneratorStatics.UnboxPointer);
                }
                else
                {
                    this.ilg.ConvertValue(Globals.TypeOfObject, type);
                }
                this.ilg.Stloc(value);
            }

            private bool InvokeFactoryMethod(ClassDataContract classContract)
            {
                if (this.HasFactoryMethod(classContract))
                {
                    this.ilg.Load(this.contextArg);
                    this.ilg.LoadAddress(this.objectLocal);
                    this.ilg.ConvertAddress(this.objectLocal.LocalType, Globals.TypeOfIObjectReference);
                    this.ilg.Load(Globals.NewObjectId);
                    this.ilg.Call(XmlFormatGeneratorStatics.GetRealObjectMethod);
                    this.ilg.ConvertValue(Globals.TypeOfObject, this.ilg.CurrentMethod.ReturnType);
                    return true;
                }
                return false;
            }

            private void InvokeOnDeserialized(ClassDataContract classContract)
            {
                if (classContract.BaseContract != null)
                {
                    this.InvokeOnDeserialized(classContract.BaseContract);
                }
                if (classContract.OnDeserialized != null)
                {
                    this.ilg.LoadAddress(this.objectLocal);
                    this.ilg.ConvertAddress(this.objectLocal.LocalType, this.objectType);
                    this.ilg.Load(this.contextArg);
                    this.ilg.LoadMember(XmlFormatGeneratorStatics.GetStreamingContextMethod);
                    this.ilg.Call(classContract.OnDeserialized);
                }
            }

            private void InvokeOnDeserializing(ClassDataContract classContract)
            {
                if (classContract.BaseContract != null)
                {
                    this.InvokeOnDeserializing(classContract.BaseContract);
                }
                if (classContract.OnDeserializing != null)
                {
                    this.ilg.LoadAddress(this.objectLocal);
                    this.ilg.ConvertAddress(this.objectLocal.LocalType, this.objectType);
                    this.ilg.Load(this.contextArg);
                    this.ilg.LoadMember(XmlFormatGeneratorStatics.GetStreamingContextMethod);
                    this.ilg.Call(classContract.OnDeserializing);
                }
            }

            private void IsEndElement()
            {
                this.ilg.Load(this.xmlReaderArg);
                this.ilg.LoadMember(JsonFormatGeneratorStatics.NodeTypeProperty);
                this.ilg.Load(XmlNodeType.EndElement);
                this.ilg.Ceq();
            }

            private void IsStartElement()
            {
                this.ilg.Call(this.xmlReaderArg, JsonFormatGeneratorStatics.IsStartElementMethod0);
            }

            private void IsStartElement(ArgBuilder nameArg, ArgBuilder nsArg)
            {
                this.ilg.Call(this.xmlReaderArg, JsonFormatGeneratorStatics.IsStartElementMethod2, nameArg, nsArg);
            }

            private void LoadArray(byte[] array, string name)
            {
                LocalBuilder var = this.ilg.DeclareLocal(Globals.TypeOfByteArray, name);
                this.ilg.NewArray(typeof(byte), array.Length);
                this.ilg.Store(var);
                for (int i = 0; i < array.Length; i++)
                {
                    this.ilg.StoreArrayElement(var, i, array[i]);
                }
                this.ilg.Load(var);
            }

            private void ReadClass(ClassDataContract classContract)
            {
                if (classContract.HasExtensionData)
                {
                    LocalBuilder var = this.ilg.DeclareLocal(Globals.TypeOfExtensionDataObject, "extensionData");
                    this.ilg.New(JsonFormatGeneratorStatics.ExtensionDataObjectCtor);
                    this.ilg.Store(var);
                    this.ReadMembers(classContract, var);
                    for (ClassDataContract contract = classContract; contract != null; contract = contract.BaseContract)
                    {
                        MethodInfo extensionDataSetMethod = contract.ExtensionDataSetMethod;
                        if (extensionDataSetMethod != null)
                        {
                            this.ilg.Call(this.objectLocal, extensionDataSetMethod, var);
                        }
                    }
                }
                else
                {
                    this.ReadMembers(classContract, null);
                }
            }

            private void ReadCollection(CollectionDataContract collectionContract)
            {
                Type underlyingType = collectionContract.UnderlyingType;
                Type itemType = collectionContract.ItemType;
                bool flag = collectionContract.Kind == CollectionKind.Array;
                ConstructorInfo constructor = collectionContract.Constructor;
                if (underlyingType.IsInterface)
                {
                    switch (collectionContract.Kind)
                    {
                        case CollectionKind.GenericDictionary:
                            underlyingType = Globals.TypeOfDictionaryGeneric.MakeGenericType(itemType.GetGenericArguments());
                            constructor = underlyingType.GetConstructor(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, Globals.EmptyTypeArray, null);
                            break;

                        case CollectionKind.Dictionary:
                            underlyingType = Globals.TypeOfHashtable;
                            constructor = underlyingType.GetConstructor(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, Globals.EmptyTypeArray, null);
                            break;

                        case CollectionKind.GenericList:
                        case CollectionKind.GenericCollection:
                        case CollectionKind.List:
                        case CollectionKind.GenericEnumerable:
                        case CollectionKind.Collection:
                        case CollectionKind.Enumerable:
                            underlyingType = itemType.MakeArrayType();
                            flag = true;
                            break;
                    }
                }
                this.objectLocal = this.ilg.DeclareLocal(underlyingType, "objectDeserialized");
                if (!flag)
                {
                    if (underlyingType.IsValueType)
                    {
                        this.ilg.Ldloca(this.objectLocal);
                        this.ilg.InitObj(underlyingType);
                    }
                    else
                    {
                        this.ilg.New(constructor);
                        this.ilg.Stloc(this.objectLocal);
                        this.ilg.Call(this.contextArg, XmlFormatGeneratorStatics.AddNewObjectMethod, this.objectLocal);
                    }
                }
                LocalBuilder local = this.ilg.DeclareLocal(Globals.TypeOfInt, "arraySize");
                this.ilg.Call(this.contextArg, XmlFormatGeneratorStatics.GetArraySizeMethod);
                this.ilg.Stloc(local);
                LocalBuilder builder2 = this.ilg.DeclareLocal(Globals.TypeOfString, "objectIdRead");
                this.ilg.Call(this.contextArg, XmlFormatGeneratorStatics.GetObjectIdMethod);
                this.ilg.Stloc(builder2);
                bool flag2 = false;
                if (flag && this.TryReadPrimitiveArray(itemType, local))
                {
                    flag2 = true;
                    this.ilg.IfNot();
                }
                this.ilg.If(local, Cmp.EqualTo, -1);
                LocalBuilder builder3 = null;
                if (flag)
                {
                    builder3 = this.ilg.DeclareLocal(underlyingType, "growingCollection");
                    this.ilg.NewArray(itemType, 0x20);
                    this.ilg.Stloc(builder3);
                }
                LocalBuilder builder4 = this.ilg.DeclareLocal(Globals.TypeOfInt, "i");
                object forState = this.ilg.For(builder4, 0, 0x7fffffff);
                this.IsStartElement(this.memberNamesArg, this.emptyDictionaryStringArg);
                this.ilg.If();
                this.ilg.Call(this.contextArg, XmlFormatGeneratorStatics.IncrementItemCountMethod, 1);
                LocalBuilder builder5 = this.ReadCollectionItem(collectionContract, itemType);
                if (flag)
                {
                    MethodInfo methodInfo = XmlFormatGeneratorStatics.EnsureArraySizeMethod.MakeGenericMethod(new Type[] { itemType });
                    this.ilg.Call(null, methodInfo, builder3, builder4);
                    this.ilg.Stloc(builder3);
                    this.ilg.StoreArrayElement(builder3, builder4, builder5);
                }
                else
                {
                    this.StoreCollectionValue(this.objectLocal, builder5, collectionContract);
                }
                this.ilg.Else();
                this.IsEndElement();
                this.ilg.If();
                this.ilg.Break(forState);
                this.ilg.Else();
                this.HandleUnexpectedItemInCollection(builder4);
                this.ilg.EndIf();
                this.ilg.EndIf();
                this.ilg.EndFor();
                if (flag)
                {
                    MethodInfo info3 = XmlFormatGeneratorStatics.TrimArraySizeMethod.MakeGenericMethod(new Type[] { itemType });
                    this.ilg.Call(null, info3, builder3, builder4);
                    this.ilg.Stloc(this.objectLocal);
                    this.ilg.Call(this.contextArg, XmlFormatGeneratorStatics.AddNewObjectWithIdMethod, builder2, this.objectLocal);
                }
                this.ilg.Else();
                this.ilg.Call(this.contextArg, XmlFormatGeneratorStatics.IncrementItemCountMethod, local);
                if (flag)
                {
                    this.ilg.NewArray(itemType, local);
                    this.ilg.Stloc(this.objectLocal);
                    this.ilg.Call(this.contextArg, XmlFormatGeneratorStatics.AddNewObjectMethod, this.objectLocal);
                }
                LocalBuilder builder6 = this.ilg.DeclareLocal(Globals.TypeOfInt, "j");
                this.ilg.For(builder6, 0, local);
                this.IsStartElement(this.memberNamesArg, this.emptyDictionaryStringArg);
                this.ilg.If();
                LocalBuilder builder7 = this.ReadCollectionItem(collectionContract, itemType);
                if (flag)
                {
                    this.ilg.StoreArrayElement(this.objectLocal, builder6, builder7);
                }
                else
                {
                    this.StoreCollectionValue(this.objectLocal, builder7, collectionContract);
                }
                this.ilg.Else();
                this.HandleUnexpectedItemInCollection(builder6);
                this.ilg.EndIf();
                this.ilg.EndFor();
                this.ilg.Call(this.contextArg, XmlFormatGeneratorStatics.CheckEndOfArrayMethod, this.xmlReaderArg, local, this.memberNamesArg, this.emptyDictionaryStringArg);
                this.ilg.EndIf();
                if (flag2)
                {
                    this.ilg.Else();
                    this.ilg.Call(this.contextArg, XmlFormatGeneratorStatics.AddNewObjectWithIdMethod, builder2, this.objectLocal);
                    this.ilg.EndIf();
                }
            }

            private LocalBuilder ReadCollectionItem(CollectionDataContract collectionContract, Type itemType)
            {
                if ((collectionContract.Kind != CollectionKind.Dictionary) && (collectionContract.Kind != CollectionKind.GenericDictionary))
                {
                    return this.ReadValue(itemType, "item");
                }
                this.ilg.Call(this.contextArg, XmlFormatGeneratorStatics.ResetAttributesMethod);
                LocalBuilder local = this.ilg.DeclareLocal(itemType, "valueRead");
                this.ilg.Load(this.collectionContractArg);
                this.ilg.Call(JsonFormatGeneratorStatics.GetItemContractMethod);
                this.ilg.Call(JsonFormatGeneratorStatics.GetRevisedItemContractMethod);
                this.ilg.Load(this.xmlReaderArg);
                this.ilg.Load(this.contextArg);
                this.ilg.Call(JsonFormatGeneratorStatics.ReadJsonValueMethod);
                this.ilg.ConvertValue(Globals.TypeOfObject, itemType);
                this.ilg.Stloc(local);
                return local;
            }

            private void ReadGetOnlyCollection(CollectionDataContract collectionContract)
            {
                Type underlyingType = collectionContract.UnderlyingType;
                Type itemType = collectionContract.ItemType;
                bool flag = collectionContract.Kind == CollectionKind.Array;
                this.objectLocal = this.ilg.DeclareLocal(underlyingType, "objectDeserialized");
                this.ilg.Load(this.contextArg);
                this.ilg.LoadMember(XmlFormatGeneratorStatics.GetCollectionMemberMethod);
                this.ilg.ConvertValue(Globals.TypeOfObject, underlyingType);
                this.ilg.Stloc(this.objectLocal);
                this.IsStartElement(this.memberNamesArg, this.emptyDictionaryStringArg);
                this.ilg.If();
                this.ilg.If(this.objectLocal, Cmp.EqualTo, null);
                this.ilg.Call(null, XmlFormatGeneratorStatics.ThrowNullValueReturnedForGetOnlyCollectionExceptionMethod, underlyingType);
                this.ilg.Else();
                LocalBuilder local = this.ilg.DeclareLocal(Globals.TypeOfInt, "arraySize");
                if (flag)
                {
                    this.ilg.Load(this.objectLocal);
                    this.ilg.Call(XmlFormatGeneratorStatics.GetArrayLengthMethod);
                    this.ilg.Stloc(local);
                }
                LocalBuilder builder2 = this.ilg.DeclareLocal(Globals.TypeOfInt, "i");
                object forState = this.ilg.For(builder2, 0, 0x7fffffff);
                this.IsStartElement(this.memberNamesArg, this.emptyDictionaryStringArg);
                this.ilg.If();
                this.ilg.Call(this.contextArg, XmlFormatGeneratorStatics.IncrementItemCountMethod, 1);
                LocalBuilder builder3 = this.ReadCollectionItem(collectionContract, itemType);
                if (flag)
                {
                    this.ilg.If(local, Cmp.EqualTo, builder2);
                    this.ilg.Call(null, XmlFormatGeneratorStatics.ThrowArrayExceededSizeExceptionMethod, local, underlyingType);
                    this.ilg.Else();
                    this.ilg.StoreArrayElement(this.objectLocal, builder2, builder3);
                    this.ilg.EndIf();
                }
                else
                {
                    this.StoreCollectionValue(this.objectLocal, builder3, collectionContract);
                }
                this.ilg.Else();
                this.IsEndElement();
                this.ilg.If();
                this.ilg.Break(forState);
                this.ilg.Else();
                this.HandleUnexpectedItemInCollection(builder2);
                this.ilg.EndIf();
                this.ilg.EndIf();
                this.ilg.EndFor();
                this.ilg.Call(this.contextArg, XmlFormatGeneratorStatics.CheckEndOfArrayMethod, this.xmlReaderArg, local, this.memberNamesArg, this.emptyDictionaryStringArg);
                this.ilg.EndIf();
                this.ilg.EndIf();
            }

            private void ReadISerializable(ClassDataContract classContract)
            {
                ConstructorInfo ctor = classContract.UnderlyingType.GetConstructor(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance, null, JsonFormatGeneratorStatics.SerInfoCtorArgs, null);
                if (ctor == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(System.Runtime.Serialization.SR.GetString("SerializationInfo_ConstructorNotFound", new object[] { DataContract.GetClrTypeFullName(classContract.UnderlyingType) })));
                }
                this.ilg.LoadAddress(this.objectLocal);
                this.ilg.ConvertAddress(this.objectLocal.LocalType, this.objectType);
                this.ilg.Call(this.contextArg, XmlFormatGeneratorStatics.ReadSerializationInfoMethod, this.xmlReaderArg, classContract.UnderlyingType);
                this.ilg.Load(this.contextArg);
                this.ilg.LoadMember(XmlFormatGeneratorStatics.GetStreamingContextMethod);
                this.ilg.Call(ctor);
            }

            private void ReadMembers(ClassDataContract classContract, LocalBuilder extensionDataLocal)
            {
                int length = classContract.MemberNames.Length;
                this.ilg.Call(this.contextArg, XmlFormatGeneratorStatics.IncrementItemCountMethod, length);
                BitFlagsGenerator expectedElements = new BitFlagsGenerator(length, this.ilg, classContract.UnderlyingType.Name + "_ExpectedElements");
                byte[] requiredElements = new byte[expectedElements.GetLocalCount()];
                this.SetRequiredElements(classContract, requiredElements);
                this.SetExpectedElements(expectedElements, 0);
                LocalBuilder builder = this.ilg.DeclareLocal(Globals.TypeOfInt, "memberIndex", -1);
                Label throwDuplicateMemberLabel = this.ilg.DefineLabel();
                Label throwMissingRequiredMembersLabel = this.ilg.DefineLabel();
                object forState = this.ilg.For(null, null, null);
                this.ilg.Call(null, XmlFormatGeneratorStatics.MoveToNextElementMethod, this.xmlReaderArg);
                this.ilg.IfFalseBreak(forState);
                this.ilg.Call(this.contextArg, JsonFormatGeneratorStatics.GetJsonMemberIndexMethod, this.xmlReaderArg, this.memberNamesArg, builder, extensionDataLocal);
                if (length > 0)
                {
                    Label[] memberLabels = this.ilg.Switch(length);
                    this.ReadMembers(classContract, expectedElements, memberLabels, throwDuplicateMemberLabel, builder);
                    this.ilg.EndSwitch();
                }
                else
                {
                    this.ilg.Pop();
                }
                this.ilg.EndFor();
                this.CheckRequiredElements(expectedElements, requiredElements, throwMissingRequiredMembersLabel);
                Label label = this.ilg.DefineLabel();
                this.ilg.Br(label);
                this.ilg.MarkLabel(throwDuplicateMemberLabel);
                this.ilg.Call(null, JsonFormatGeneratorStatics.ThrowDuplicateMemberExceptionMethod, this.objectLocal, this.memberNamesArg, builder);
                this.ilg.MarkLabel(throwMissingRequiredMembersLabel);
                this.ilg.Load(this.objectLocal);
                this.ilg.ConvertValue(this.objectLocal.LocalType, Globals.TypeOfObject);
                this.ilg.Load(this.memberNamesArg);
                expectedElements.LoadArray();
                this.LoadArray(requiredElements, "requiredElements");
                this.ilg.Call(JsonFormatGeneratorStatics.ThrowMissingRequiredMembersMethod);
                this.ilg.MarkLabel(label);
            }

            private int ReadMembers(ClassDataContract classContract, BitFlagsGenerator expectedElements, Label[] memberLabels, Label throwDuplicateMemberLabel, LocalBuilder memberIndexLocal)
            {
                int index = (classContract.BaseContract == null) ? 0 : this.ReadMembers(classContract.BaseContract, expectedElements, memberLabels, throwDuplicateMemberLabel, memberIndexLocal);
                int num2 = 0;
                while (num2 < classContract.Members.Count)
                {
                    DataMember member = classContract.Members[num2];
                    Type memberType = member.MemberType;
                    this.ilg.Case(memberLabels[index], member.Name);
                    this.ilg.Set(memberIndexLocal, index);
                    expectedElements.Load(index);
                    this.ilg.Brfalse(throwDuplicateMemberLabel);
                    LocalBuilder local = null;
                    if (member.IsGetOnlyCollection)
                    {
                        this.ilg.LoadAddress(this.objectLocal);
                        this.ilg.LoadMember(member.MemberInfo);
                        local = this.ilg.DeclareLocal(memberType, member.Name + "Value");
                        this.ilg.Stloc(local);
                        this.ilg.Call(this.contextArg, XmlFormatGeneratorStatics.StoreCollectionMemberInfoMethod, local);
                        this.ReadValue(memberType, member.Name);
                    }
                    else
                    {
                        local = this.ReadValue(memberType, member.Name);
                        this.ilg.LoadAddress(this.objectLocal);
                        this.ilg.ConvertAddress(this.objectLocal.LocalType, this.objectType);
                        this.ilg.Ldloc(local);
                        this.ilg.StoreMember(member.MemberInfo);
                    }
                    this.ResetExpectedElements(expectedElements, index);
                    this.ilg.EndCase();
                    num2++;
                    index++;
                }
                return index;
            }

            private LocalBuilder ReadValue(Type type, string name)
            {
                LocalBuilder builder = this.ilg.DeclareLocal(type, "valueRead");
                LocalBuilder outerValue = null;
                int nullables = 0;
                while (type.IsGenericType && (type.GetGenericTypeDefinition() == Globals.TypeOfNullable))
                {
                    nullables++;
                    type = type.GetGenericArguments()[0];
                }
                PrimitiveDataContract primitiveDataContract = PrimitiveDataContract.GetPrimitiveDataContract(type);
                if (((primitiveDataContract != null) && (primitiveDataContract.UnderlyingType != Globals.TypeOfObject)) || ((nullables != 0) || type.IsValueType))
                {
                    LocalBuilder local = this.ilg.DeclareLocal(Globals.TypeOfString, "objectIdRead");
                    this.ilg.Call(this.contextArg, XmlFormatGeneratorStatics.ReadAttributesMethod, this.xmlReaderArg);
                    this.ilg.Call(this.contextArg, XmlFormatGeneratorStatics.ReadIfNullOrRefMethod, this.xmlReaderArg, type, DataContract.IsTypeSerializable(type));
                    this.ilg.Stloc(local);
                    this.ilg.If(local, Cmp.EqualTo, null);
                    if (nullables != 0)
                    {
                        this.ilg.LoadAddress(builder);
                        this.ilg.InitObj(builder.LocalType);
                    }
                    else if (type.IsValueType)
                    {
                        this.ThrowValidationException(System.Runtime.Serialization.SR.GetString("ValueTypeCannotBeNull", new object[] { DataContract.GetClrTypeFullName(type) }), new object[0]);
                    }
                    else
                    {
                        this.ilg.Load(null);
                        this.ilg.Stloc(builder);
                    }
                    this.ilg.ElseIfIsEmptyString(local);
                    this.ilg.Call(this.contextArg, XmlFormatGeneratorStatics.GetObjectIdMethod);
                    this.ilg.Stloc(local);
                    if (type.IsValueType)
                    {
                        this.ilg.IfNotIsEmptyString(local);
                        this.ThrowValidationException(System.Runtime.Serialization.SR.GetString("ValueTypeCannotHaveId", new object[] { DataContract.GetClrTypeFullName(type) }), new object[0]);
                        this.ilg.EndIf();
                    }
                    if (nullables != 0)
                    {
                        outerValue = builder;
                        builder = this.ilg.DeclareLocal(type, "innerValueRead");
                    }
                    if ((primitiveDataContract != null) && (primitiveDataContract.UnderlyingType != Globals.TypeOfObject))
                    {
                        this.ilg.Call(this.xmlReaderArg, primitiveDataContract.XmlFormatReaderMethod);
                        this.ilg.Stloc(builder);
                        if (!type.IsValueType)
                        {
                            this.ilg.Call(this.contextArg, XmlFormatGeneratorStatics.AddNewObjectMethod, builder);
                        }
                    }
                    else
                    {
                        this.InternalDeserialize(builder, type, name);
                    }
                    this.ilg.Else();
                    if (type.IsValueType)
                    {
                        this.ThrowValidationException(System.Runtime.Serialization.SR.GetString("ValueTypeCannotHaveRef", new object[] { DataContract.GetClrTypeFullName(type) }), new object[0]);
                    }
                    else
                    {
                        this.ilg.Call(this.contextArg, XmlFormatGeneratorStatics.GetExistingObjectMethod, local, type, name, string.Empty);
                        this.ilg.ConvertValue(Globals.TypeOfObject, type);
                        this.ilg.Stloc(builder);
                    }
                    this.ilg.EndIf();
                    if (outerValue != null)
                    {
                        this.ilg.If(local, Cmp.NotEqualTo, null);
                        this.WrapNullableObject(builder, outerValue, nullables);
                        this.ilg.EndIf();
                        builder = outerValue;
                    }
                    return builder;
                }
                this.InternalDeserialize(builder, type, name);
                return builder;
            }

            private void ResetExpectedElements(BitFlagsGenerator expectedElements, int index)
            {
                expectedElements.Store(index, false);
            }

            private void SetExpectedElements(BitFlagsGenerator expectedElements, int startIndex)
            {
                int bitCount = expectedElements.GetBitCount();
                for (int i = startIndex; i < bitCount; i++)
                {
                    expectedElements.Store(i, true);
                }
            }

            private int SetRequiredElements(ClassDataContract contract, byte[] requiredElements)
            {
                int bitIndex = (contract.BaseContract == null) ? 0 : this.SetRequiredElements(contract.BaseContract, requiredElements);
                List<DataMember> members = contract.Members;
                int num2 = 0;
                while (num2 < members.Count)
                {
                    if (members[num2].IsRequired)
                    {
                        BitFlagsGenerator.SetBit(requiredElements, bitIndex);
                    }
                    num2++;
                    bitIndex++;
                }
                return bitIndex;
            }

            private void StoreCollectionValue(LocalBuilder collection, LocalBuilder value, CollectionDataContract collectionContract)
            {
                if ((collectionContract.Kind == CollectionKind.GenericDictionary) || (collectionContract.Kind == CollectionKind.Dictionary))
                {
                    ClassDataContract dataContract = DataContract.GetDataContract(value.LocalType) as ClassDataContract;
                    DataMember member = dataContract.Members[0];
                    DataMember member2 = dataContract.Members[1];
                    LocalBuilder local = this.ilg.DeclareLocal(member.MemberType, member.Name);
                    LocalBuilder builder2 = this.ilg.DeclareLocal(member2.MemberType, member2.Name);
                    this.ilg.LoadAddress(value);
                    this.ilg.LoadMember(member.MemberInfo);
                    this.ilg.Stloc(local);
                    this.ilg.LoadAddress(value);
                    this.ilg.LoadMember(member2.MemberInfo);
                    this.ilg.Stloc(builder2);
                    this.ilg.Call(collection, collectionContract.AddMethod, local, builder2);
                    if (collectionContract.AddMethod.ReturnType != Globals.TypeOfVoid)
                    {
                        this.ilg.Pop();
                    }
                }
                else
                {
                    this.ilg.Call(collection, collectionContract.AddMethod, value);
                    if (collectionContract.AddMethod.ReturnType != Globals.TypeOfVoid)
                    {
                        this.ilg.Pop();
                    }
                }
            }

            private void ThrowUnexpectedStateException(XmlNodeType expectedState)
            {
                this.ilg.Call(null, XmlFormatGeneratorStatics.CreateUnexpectedStateExceptionMethod, expectedState, this.xmlReaderArg);
                this.ilg.Throw();
            }

            private void ThrowValidationException()
            {
                this.ilg.New(JsonFormatGeneratorStatics.SerializationExceptionCtor);
                this.ilg.Throw();
            }

            private void ThrowValidationException(string msg, params object[] values)
            {
                if ((values != null) && (values.Length > 0))
                {
                    this.ilg.CallStringFormat(msg, values);
                }
                else
                {
                    this.ilg.Load(msg);
                }
                this.ThrowValidationException();
            }

            private bool TryReadPrimitiveArray(Type itemType, LocalBuilder size)
            {
                if (PrimitiveDataContract.GetPrimitiveDataContract(itemType) != null)
                {
                    string name = null;
                    switch (Type.GetTypeCode(itemType))
                    {
                        case TypeCode.Int32:
                            name = "TryReadInt32Array";
                            break;

                        case TypeCode.Int64:
                            name = "TryReadInt64Array";
                            break;

                        case TypeCode.Single:
                            name = "TryReadSingleArray";
                            break;

                        case TypeCode.Double:
                            name = "TryReadDoubleArray";
                            break;

                        case TypeCode.Decimal:
                            name = "TryReadDecimalArray";
                            break;

                        case TypeCode.DateTime:
                            name = "TryReadJsonDateTimeArray";
                            break;

                        case TypeCode.Boolean:
                            name = "TryReadBooleanArray";
                            break;
                    }
                    if (name != null)
                    {
                        this.ilg.Load(this.xmlReaderArg);
                        this.ilg.ConvertValue(typeof(XmlReaderDelegator), typeof(JsonReaderDelegator));
                        this.ilg.Load(this.contextArg);
                        this.ilg.Load(this.memberNamesArg);
                        this.ilg.Load(this.emptyDictionaryStringArg);
                        this.ilg.Load(size);
                        this.ilg.Ldloca(this.objectLocal);
                        this.ilg.Call(typeof(JsonReaderDelegator).GetMethod(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance));
                        return true;
                    }
                }
                return false;
            }

            private void WrapNullableObject(LocalBuilder innerValue, LocalBuilder outerValue, int nullables)
            {
                Type localType = innerValue.LocalType;
                Type type2 = outerValue.LocalType;
                this.ilg.LoadAddress(outerValue);
                this.ilg.Load(innerValue);
                for (int i = 1; i < nullables; i++)
                {
                    Type type3 = Globals.TypeOfNullable.MakeGenericType(new Type[] { localType });
                    this.ilg.New(type3.GetConstructor(new Type[] { localType }));
                    localType = type3;
                }
                this.ilg.Call(type2.GetConstructor(new Type[] { localType }));
            }
        }
    }
}

