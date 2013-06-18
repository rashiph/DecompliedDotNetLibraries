namespace System.Runtime.Serialization
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Security;
    using System.Security.Permissions;
    using System.Xml;

    internal sealed class XmlFormatWriterGenerator
    {
        [SecurityCritical]
        private CriticalHelper helper = new CriticalHelper();

        [SecurityCritical]
        internal XmlFormatClassWriterDelegate GenerateClassWriter(ClassDataContract classContract)
        {
            return this.helper.GenerateClassWriter(classContract);
        }

        [SecurityCritical]
        internal XmlFormatCollectionWriterDelegate GenerateCollectionWriter(CollectionDataContract collectionContract)
        {
            return this.helper.GenerateCollectionWriter(collectionContract);
        }

        private class CriticalHelper
        {
            private int childElementIndex;
            private LocalBuilder childElementNamespacesLocal;
            private ArgBuilder contextArg;
            private LocalBuilder contractNamespacesLocal;
            private ArgBuilder dataContractArg;
            private CodeGenerator ilg;
            private LocalBuilder memberNamesLocal;
            private LocalBuilder objectLocal;
            private int typeIndex = 1;
            private ArgBuilder xmlWriterArg;

            private bool CheckIfConflictingMembersHaveDifferentTypes(DataMember member)
            {
                while (member.ConflictingMember != null)
                {
                    if (member.MemberType != member.ConflictingMember.MemberType)
                    {
                        return true;
                    }
                    member = member.ConflictingMember;
                }
                return false;
            }

            private bool CheckIfMemberHasConflict(DataMember member, ClassDataContract classContract, ClassDataContract derivedMostClassContract)
            {
                if (this.CheckIfConflictingMembersHaveDifferentTypes(member))
                {
                    return true;
                }
                string name = member.Name;
                string str2 = classContract.StableName.Namespace;
                for (ClassDataContract contract = derivedMostClassContract; (contract != null) && (contract != classContract); contract = contract.BaseContract)
                {
                    if (str2 == contract.StableName.Namespace)
                    {
                        List<DataMember> members = contract.Members;
                        for (int i = 0; i < members.Count; i++)
                        {
                            if (name == members[i].Name)
                            {
                                return this.CheckIfConflictingMembersHaveDifferentTypes(members[i]);
                            }
                        }
                    }
                }
                return false;
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

            internal XmlFormatClassWriterDelegate GenerateClassWriter(ClassDataContract classContract)
            {
                this.ilg = new CodeGenerator();
                bool allowPrivateMemberAccess = classContract.RequiresMemberAccessForWrite(null);
                try
                {
                    this.ilg.BeginMethod("Write" + classContract.StableName.Name + "ToXml", Globals.TypeOfXmlFormatClassWriterDelegate, allowPrivateMemberAccess);
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
                this.DemandSerializationFormatterPermission(classContract);
                this.DemandMemberAccessPermission(allowPrivateMemberAccess);
                this.WriteClass(classContract);
                return (XmlFormatClassWriterDelegate) this.ilg.EndMethod();
            }

            internal XmlFormatCollectionWriterDelegate GenerateCollectionWriter(CollectionDataContract collectionContract)
            {
                this.ilg = new CodeGenerator();
                bool allowPrivateMemberAccess = collectionContract.RequiresMemberAccessForWrite(null);
                try
                {
                    this.ilg.BeginMethod("Write" + collectionContract.StableName.Name + "ToXml", Globals.TypeOfXmlFormatCollectionWriterDelegate, allowPrivateMemberAccess);
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
                return (XmlFormatCollectionWriterDelegate) this.ilg.EndMethod();
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

            private bool NeedsPrefix(Type type, XmlDictionaryString ns)
            {
                if (!(type == Globals.TypeOfXmlQualifiedName))
                {
                    return false;
                }
                return (((ns != null) && (ns.Value != null)) && (ns.Value.Length > 0));
            }

            private bool TryWritePrimitive(Type type, LocalBuilder value, MemberInfo memberInfo, LocalBuilder arrayItemIndex, LocalBuilder ns, LocalBuilder name, int nameIndex)
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
                    this.ilg.LoadArrayElement(this.memberNamesLocal, nameIndex);
                }
                this.ilg.Load(ns);
                this.ilg.Call(primitiveDataContract.XmlFormatWriterMethod);
                return true;
            }

            private bool TryWritePrimitiveArray(Type type, Type itemType, LocalBuilder value, LocalBuilder itemName, LocalBuilder itemNamespace)
            {
                if (PrimitiveDataContract.GetPrimitiveDataContract(itemType) != null)
                {
                    string name = null;
                    switch (Type.GetTypeCode(itemType))
                    {
                        case TypeCode.Int32:
                            name = "WriteInt32Array";
                            break;

                        case TypeCode.Int64:
                            name = "WriteInt64Array";
                            break;

                        case TypeCode.Single:
                            name = "WriteSingleArray";
                            break;

                        case TypeCode.Double:
                            name = "WriteDoubleArray";
                            break;

                        case TypeCode.Decimal:
                            name = "WriteDecimalArray";
                            break;

                        case TypeCode.DateTime:
                            name = "WriteDateTimeArray";
                            break;

                        case TypeCode.Boolean:
                            name = "WriteBooleanArray";
                            break;
                    }
                    if (name != null)
                    {
                        this.ilg.Load(this.xmlWriterArg);
                        this.ilg.Load(value);
                        this.ilg.Load(itemName);
                        this.ilg.Load(itemNamespace);
                        this.ilg.Call(typeof(XmlWriterDelegator).GetMethod(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance, null, new Type[] { type, typeof(XmlDictionaryString), typeof(XmlDictionaryString) }, null));
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

            private void WriteClass(ClassDataContract classContract)
            {
                this.InvokeOnSerializing(classContract);
                if (classContract.IsISerializable)
                {
                    this.ilg.Call(this.contextArg, XmlFormatGeneratorStatics.WriteISerializableMethod, this.xmlWriterArg, this.objectLocal);
                }
                else
                {
                    if (classContract.ContractNamespaces.Length > 1)
                    {
                        this.contractNamespacesLocal = this.ilg.DeclareLocal(typeof(XmlDictionaryString[]), "contractNamespaces");
                        this.ilg.Load(this.dataContractArg);
                        this.ilg.LoadMember(XmlFormatGeneratorStatics.ContractNamespacesField);
                        this.ilg.Store(this.contractNamespacesLocal);
                    }
                    this.memberNamesLocal = this.ilg.DeclareLocal(typeof(XmlDictionaryString[]), "memberNames");
                    this.ilg.Load(this.dataContractArg);
                    this.ilg.LoadMember(XmlFormatGeneratorStatics.MemberNamesField);
                    this.ilg.Store(this.memberNamesLocal);
                    for (int i = 0; i < classContract.ChildElementNamespaces.Length; i++)
                    {
                        if (classContract.ChildElementNamespaces[i] != null)
                        {
                            this.childElementNamespacesLocal = this.ilg.DeclareLocal(typeof(XmlDictionaryString[]), "childElementNamespaces");
                            this.ilg.Load(this.dataContractArg);
                            this.ilg.LoadMember(XmlFormatGeneratorStatics.ChildElementNamespacesProperty);
                            this.ilg.Store(this.childElementNamespacesLocal);
                        }
                    }
                    if (classContract.HasExtensionData)
                    {
                        LocalBuilder var = this.ilg.DeclareLocal(Globals.TypeOfExtensionDataObject, "extensionData");
                        this.ilg.Load(this.objectLocal);
                        this.ilg.ConvertValue(this.objectLocal.LocalType, Globals.TypeOfIExtensibleDataObject);
                        this.ilg.LoadMember(XmlFormatGeneratorStatics.ExtensionDataProperty);
                        this.ilg.Store(var);
                        this.ilg.Call(this.contextArg, XmlFormatGeneratorStatics.WriteExtensionDataMethod, this.xmlWriterArg, var, -1);
                        this.WriteMembers(classContract, var, classContract);
                    }
                    else
                    {
                        this.WriteMembers(classContract, null, classContract);
                    }
                }
                this.InvokeOnSerialized(classContract);
            }

            private void WriteCollection(CollectionDataContract collectionContract)
            {
                LocalBuilder var = this.ilg.DeclareLocal(typeof(XmlDictionaryString), "itemNamespace");
                this.ilg.Load(this.dataContractArg);
                this.ilg.LoadMember(XmlFormatGeneratorStatics.NamespaceProperty);
                this.ilg.Store(var);
                LocalBuilder builder2 = this.ilg.DeclareLocal(typeof(XmlDictionaryString), "itemName");
                this.ilg.Load(this.dataContractArg);
                this.ilg.LoadMember(XmlFormatGeneratorStatics.CollectionItemNameProperty);
                this.ilg.Store(builder2);
                if (collectionContract.ChildElementNamespace != null)
                {
                    this.ilg.Load(this.xmlWriterArg);
                    this.ilg.Load(this.dataContractArg);
                    this.ilg.LoadMember(XmlFormatGeneratorStatics.ChildElementNamespaceProperty);
                    this.ilg.Call(XmlFormatGeneratorStatics.WriteNamespaceDeclMethod);
                }
                if (collectionContract.Kind == CollectionKind.Array)
                {
                    Type itemType = collectionContract.ItemType;
                    LocalBuilder local = this.ilg.DeclareLocal(Globals.TypeOfInt, "i");
                    this.ilg.Call(this.contextArg, XmlFormatGeneratorStatics.IncrementArrayCountMethod, this.xmlWriterArg, this.objectLocal);
                    if (!this.TryWritePrimitiveArray(collectionContract.UnderlyingType, itemType, this.objectLocal, builder2, var))
                    {
                        this.ilg.For(local, 0, this.objectLocal);
                        if (!this.TryWritePrimitive(itemType, null, null, local, var, builder2, 0))
                        {
                            this.WriteStartElement(itemType, collectionContract.Namespace, var, builder2, 0);
                            this.ilg.LoadArrayElement(this.objectLocal, local);
                            LocalBuilder builder4 = this.ilg.DeclareLocal(itemType, "memberValue");
                            this.ilg.Stloc(builder4);
                            this.WriteValue(builder4, false);
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
                                moveNextMethod = XmlFormatGeneratorStatics.MoveNextMethod;
                            }
                            if (getCurrentMethod == null)
                            {
                                getCurrentMethod = XmlFormatGeneratorStatics.GetCurrentMethod;
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
                    LocalBuilder builder5 = this.ilg.DeclareLocal(returnType, "currentValue");
                    LocalBuilder builder6 = this.ilg.DeclareLocal(type, "enumerator");
                    this.ilg.Call(this.objectLocal, collectionContract.GetEnumeratorMethod);
                    if (flag)
                    {
                        this.ilg.ConvertValue(collectionContract.GetEnumeratorMethod.ReturnType, Globals.TypeOfIDictionaryEnumerator);
                        this.ilg.New(XmlFormatGeneratorStatics.DictionaryEnumeratorCtor);
                    }
                    else if (flag2)
                    {
                        Type target = Globals.TypeOfIEnumeratorGeneric.MakeGenericType(new Type[] { Globals.TypeOfKeyValuePair.MakeGenericType(typeArguments) });
                        ConstructorInfo constructorInfo = type.GetConstructor(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance, null, new Type[] { target }, null);
                        this.ilg.ConvertValue(collectionContract.GetEnumeratorMethod.ReturnType, target);
                        this.ilg.New(constructorInfo);
                    }
                    this.ilg.Stloc(builder6);
                    this.ilg.ForEach(builder5, returnType, type, builder6, getCurrentMethod);
                    if (methodInfo == null)
                    {
                        this.ilg.Call(this.contextArg, XmlFormatGeneratorStatics.IncrementItemCountMethod, 1);
                    }
                    if (!this.TryWritePrimitive(returnType, builder5, null, null, var, builder2, 0))
                    {
                        this.WriteStartElement(returnType, collectionContract.Namespace, var, builder2, 0);
                        if (flag2 || flag)
                        {
                            this.ilg.Call(this.dataContractArg, XmlFormatGeneratorStatics.GetItemContractMethod);
                            this.ilg.Load(this.xmlWriterArg);
                            this.ilg.Load(builder5);
                            this.ilg.ConvertValue(builder5.LocalType, Globals.TypeOfObject);
                            this.ilg.Load(this.contextArg);
                            this.ilg.Call(XmlFormatGeneratorStatics.WriteXmlValueMethod);
                        }
                        else
                        {
                            this.WriteValue(builder5, false);
                        }
                        this.WriteEndElement();
                    }
                    this.ilg.EndForEach(moveNextMethod);
                }
            }

            private void WriteEndElement()
            {
                this.ilg.Call(this.xmlWriterArg, XmlFormatGeneratorStatics.WriteEndElementMethod);
            }

            private int WriteMembers(ClassDataContract classContract, LocalBuilder extensionDataLocal, ClassDataContract derivedMostClassContract)
            {
                int num = (classContract.BaseContract == null) ? 0 : this.WriteMembers(classContract.BaseContract, extensionDataLocal, derivedMostClassContract);
                LocalBuilder var = this.ilg.DeclareLocal(typeof(XmlDictionaryString), "ns");
                if (this.contractNamespacesLocal == null)
                {
                    this.ilg.Load(this.dataContractArg);
                    this.ilg.LoadMember(XmlFormatGeneratorStatics.NamespaceProperty);
                }
                else
                {
                    this.ilg.LoadArrayElement(this.contractNamespacesLocal, this.typeIndex - 1);
                }
                this.ilg.Store(var);
                this.ilg.Call(this.contextArg, XmlFormatGeneratorStatics.IncrementItemCountMethod, classContract.Members.Count);
                int num2 = 0;
                while (num2 < classContract.Members.Count)
                {
                    DataMember member = classContract.Members[num2];
                    Type memberType = member.MemberType;
                    LocalBuilder builder2 = null;
                    if (member.IsGetOnlyCollection)
                    {
                        this.ilg.Load(this.contextArg);
                        this.ilg.Call(XmlFormatGeneratorStatics.StoreIsGetOnlyCollectionMethod);
                    }
                    if (!member.EmitDefaultValue)
                    {
                        builder2 = this.LoadMemberValue(member);
                        this.ilg.IfNotDefaultValue(builder2);
                    }
                    bool writeXsiType = this.CheckIfMemberHasConflict(member, classContract, derivedMostClassContract);
                    if (writeXsiType || !this.TryWritePrimitive(memberType, builder2, member.MemberInfo, null, var, null, num2 + this.childElementIndex))
                    {
                        this.WriteStartElement(memberType, classContract.Namespace, var, null, num2 + this.childElementIndex);
                        if (classContract.ChildElementNamespaces[num2 + this.childElementIndex] != null)
                        {
                            this.ilg.Load(this.xmlWriterArg);
                            this.ilg.LoadArrayElement(this.childElementNamespacesLocal, num2 + this.childElementIndex);
                            this.ilg.Call(XmlFormatGeneratorStatics.WriteNamespaceDeclMethod);
                        }
                        if (builder2 == null)
                        {
                            builder2 = this.LoadMemberValue(member);
                        }
                        this.WriteValue(builder2, writeXsiType);
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
                    num2++;
                    num++;
                }
                this.typeIndex++;
                this.childElementIndex += classContract.Members.Count;
                return num;
            }

            private void WriteStartElement(Type type, XmlDictionaryString ns, LocalBuilder namespaceLocal, LocalBuilder nameLocal, int nameIndex)
            {
                bool flag = this.NeedsPrefix(type, ns);
                this.ilg.Load(this.xmlWriterArg);
                if (flag)
                {
                    this.ilg.Load("q");
                }
                if (nameLocal == null)
                {
                    this.ilg.LoadArrayElement(this.memberNamesLocal, nameIndex);
                }
                else
                {
                    this.ilg.Load(nameLocal);
                }
                this.ilg.Load(namespaceLocal);
                this.ilg.Call(flag ? XmlFormatGeneratorStatics.WriteStartElementMethod3 : XmlFormatGeneratorStatics.WriteStartElementMethod2);
            }

            private void WriteValue(LocalBuilder memberValue, bool writeXsiType)
            {
                Type localType = memberValue.LocalType;
                if (localType.IsPointer)
                {
                    this.ilg.Load(memberValue);
                    this.ilg.Load(localType);
                    this.ilg.Call(XmlFormatGeneratorStatics.BoxPointer);
                    localType = Globals.TypeOfReflectionPointer;
                    memberValue = this.ilg.DeclareLocal(localType, "memberValueRefPointer");
                    this.ilg.Store(memberValue);
                }
                bool flag = localType.IsGenericType && (localType.GetGenericTypeDefinition() == Globals.TypeOfNullable);
                if (localType.IsValueType && !flag)
                {
                    PrimitiveDataContract primitiveDataContract = PrimitiveDataContract.GetPrimitiveDataContract(localType);
                    if ((primitiveDataContract != null) && !writeXsiType)
                    {
                        this.ilg.Call(this.xmlWriterArg, primitiveDataContract.XmlFormatContentWriterMethod, memberValue);
                    }
                    else
                    {
                        this.InternalSerialize(XmlFormatGeneratorStatics.InternalSerializeMethod, memberValue, localType, writeXsiType);
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
                    if (((contract2 != null) && (contract2.UnderlyingType != Globals.TypeOfObject)) && !writeXsiType)
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
                        this.InternalSerialize(flag ? XmlFormatGeneratorStatics.InternalSerializeMethod : XmlFormatGeneratorStatics.InternalSerializeReferenceMethod, memberValue, localType, writeXsiType);
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

