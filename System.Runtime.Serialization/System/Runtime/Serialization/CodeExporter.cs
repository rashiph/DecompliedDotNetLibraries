namespace System.Runtime.Serialization
{
    using System;
    using System.CodeDom;
    using System.CodeDom.Compiler;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Text;
    using System.Xml;
    using System.Xml.Schema;

    internal class CodeExporter
    {
        private Dictionary<string, string> clrNamespaces;
        private CodeCompileUnit codeCompileUnit;
        private static readonly object codeUserDataActualTypeKey = new object();
        private DataContractSet dataContractSet;
        private const int MaxIdentifierLength = 0x1ff;
        private Dictionary<string, string> namespaces;
        private ImportOptions options;
        private static readonly object surrogateDataKey = typeof(IDataContractSurrogate);
        private static readonly string typeNameFieldName = "typeName";
        private static readonly string wildcardNamespaceMapping = "*";

        internal CodeExporter(DataContractSet dataContractSet, ImportOptions options, CodeCompileUnit codeCompileUnit)
        {
            this.dataContractSet = dataContractSet;
            this.codeCompileUnit = codeCompileUnit;
            this.AddReferencedAssembly(Assembly.GetExecutingAssembly());
            this.options = options;
            this.namespaces = new Dictionary<string, string>();
            this.clrNamespaces = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (KeyValuePair<XmlQualifiedName, DataContract> pair in dataContractSet)
            {
                DataContract dataContract = pair.Value;
                if (!dataContract.IsBuiltInDataContract && !(dataContract is CollectionDataContract))
                {
                    ContractCodeDomInfo contractCodeDomInfo = this.GetContractCodeDomInfo(dataContract);
                    if (contractCodeDomInfo.IsProcessed && !contractCodeDomInfo.UsesWildcardNamespace)
                    {
                        string clrNamespace = contractCodeDomInfo.ClrNamespace;
                        if ((clrNamespace != null) && !this.clrNamespaces.ContainsKey(clrNamespace))
                        {
                            this.clrNamespaces.Add(clrNamespace, dataContract.StableName.Namespace);
                            this.namespaces.Add(dataContract.StableName.Namespace, clrNamespace);
                        }
                    }
                }
            }
            if (this.options != null)
            {
                foreach (KeyValuePair<string, string> pair2 in options.Namespaces)
                {
                    string str4;
                    string str5;
                    string key = pair2.Key;
                    string str3 = pair2.Value;
                    if (str3 == null)
                    {
                        str3 = string.Empty;
                    }
                    if (this.clrNamespaces.TryGetValue(str3, out str4))
                    {
                        if (key != str4)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.Runtime.Serialization.SR.GetString("CLRNamespaceMappedMultipleTimes", new object[] { str4, key, str3 })));
                        }
                    }
                    else
                    {
                        this.clrNamespaces.Add(str3, key);
                    }
                    if (this.namespaces.TryGetValue(key, out str5))
                    {
                        if (str3 != str5)
                        {
                            this.namespaces.Remove(key);
                            this.namespaces.Add(key, str3);
                        }
                    }
                    else
                    {
                        this.namespaces.Add(key, str3);
                    }
                }
            }
            foreach (CodeNamespace namespace2 in codeCompileUnit.Namespaces)
            {
                string str6 = namespace2.Name ?? string.Empty;
                if (!this.clrNamespaces.ContainsKey(str6))
                {
                    this.clrNamespaces.Add(str6, null);
                }
                if (str6.Length == 0)
                {
                    foreach (CodeTypeDeclaration declaration in namespace2.Types)
                    {
                        this.AddGlobalTypeName(declaration.Name);
                    }
                }
            }
        }

        private void AddBaseMemberNames(ContractCodeDomInfo baseContractCodeDomInfo, ContractCodeDomInfo contractCodeDomInfo)
        {
            if (!baseContractCodeDomInfo.ReferencedTypeExists)
            {
                Dictionary<string, object> memberNames = baseContractCodeDomInfo.GetMemberNames();
                Dictionary<string, object> dictionary2 = contractCodeDomInfo.GetMemberNames();
                foreach (KeyValuePair<string, object> pair in memberNames)
                {
                    dictionary2.Add(pair.Key, pair.Value);
                }
            }
        }

        private void AddExtensionData(ContractCodeDomInfo contractCodeDomInfo)
        {
            if ((contractCodeDomInfo != null) && (contractCodeDomInfo.TypeDeclaration != null))
            {
                CodeTypeDeclaration typeDeclaration = contractCodeDomInfo.TypeDeclaration;
                typeDeclaration.BaseTypes.Add(DataContract.GetClrTypeFullName(Globals.TypeOfIExtensibleDataObject));
                CodeMemberField extensionDataObjectField = this.ExtensionDataObjectField;
                if (this.GenerateSerializableTypes)
                {
                    CodeAttributeDeclaration declaration2 = new CodeAttributeDeclaration(DataContract.GetClrTypeFullName(Globals.TypeOfNonSerializedAttribute));
                    extensionDataObjectField.CustomAttributes.Add(declaration2);
                }
                typeDeclaration.Members.Add(extensionDataObjectField);
                contractCodeDomInfo.GetMemberNames().Add(extensionDataObjectField.Name, null);
                CodeMemberProperty extensionDataObjectProperty = this.ExtensionDataObjectProperty;
                typeDeclaration.Members.Add(extensionDataObjectProperty);
                contractCodeDomInfo.GetMemberNames().Add(extensionDataObjectProperty.Name, null);
            }
        }

        private void AddGlobalTypeName(string typeName)
        {
            if (!this.clrNamespaces.ContainsKey(typeName))
            {
                this.clrNamespaces.Add(typeName, null);
            }
        }

        private void AddImportStatement(string clrNamespace, CodeNamespace codeNamespace)
        {
            if (clrNamespace != codeNamespace.Name)
            {
                CodeNamespaceImportCollection imports = codeNamespace.Imports;
                foreach (CodeNamespaceImport import in imports)
                {
                    if (import.Namespace == clrNamespace)
                    {
                        return;
                    }
                }
                imports.Add(new CodeNamespaceImport(clrNamespace));
            }
        }

        [SecuritySafeCritical]
        private void AddKnownTypeContracts(ClassDataContract dataContract, Dictionary<XmlQualifiedName, DataContract> knownContracts)
        {
            if ((knownContracts != null) && (knownContracts.Count != 0))
            {
                if (dataContract.KnownDataContracts == null)
                {
                    dataContract.KnownDataContracts = new Dictionary<XmlQualifiedName, DataContract>();
                }
                foreach (KeyValuePair<XmlQualifiedName, DataContract> pair in knownContracts)
                {
                    if (((dataContract.StableName != pair.Key) && !dataContract.KnownDataContracts.ContainsKey(pair.Key)) && !pair.Value.IsBuiltInDataContract)
                    {
                        dataContract.KnownDataContracts.Add(pair.Key, pair.Value);
                    }
                }
            }
        }

        private void AddKnownTypes(ClassDataContract dataContract, ContractCodeDomInfo contractCodeDomInfo)
        {
            Dictionary<XmlQualifiedName, DataContract> knownTypeContracts = this.GetKnownTypeContracts(dataContract, new Dictionary<DataContract, object>());
            if ((knownTypeContracts != null) && (knownTypeContracts.Count != 0))
            {
                foreach (DataContract contract in knownTypeContracts.Values)
                {
                    CodeAttributeDeclaration declaration = new CodeAttributeDeclaration(DataContract.GetClrTypeFullName(Globals.TypeOfKnownTypeAttribute));
                    declaration.Arguments.Add(new CodeAttributeArgument(new CodeTypeOfExpression(this.GetCodeTypeReference(contract))));
                    contractCodeDomInfo.TypeDeclaration.CustomAttributes.Add(declaration);
                }
                this.AddImportStatement(Globals.TypeOfKnownTypeAttribute.Namespace, contractCodeDomInfo.CodeNamespace);
            }
        }

        private static void AddNamespaceFragment(StringBuilder builder, int fragmentOffset, int fragmentLength, Dictionary<string, object> fragments)
        {
            if (fragmentLength != 0)
            {
                string key = builder.ToString(fragmentOffset, fragmentLength);
                if (fragments.ContainsKey(key))
                {
                    int num = 1;
                    while (true)
                    {
                        string appendString = num.ToString(NumberFormatInfo.InvariantInfo);
                        string str3 = AppendToValidClrIdentifier(key, appendString);
                        if (!fragments.ContainsKey(str3))
                        {
                            builder.Append(appendString);
                            key = str3;
                            break;
                        }
                        if (num == 0x7fffffff)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(System.Runtime.Serialization.SR.GetString("CannotComputeUniqueName", new object[] { key })));
                        }
                        num++;
                    }
                }
                fragments.Add(key, null);
            }
        }

        private void AddNamespacePair(string dataContractNamespace, string clrNamespace)
        {
            this.Namespaces.Add(dataContractNamespace, clrNamespace);
            this.ClrNamespaces.Add(clrNamespace, dataContractNamespace);
        }

        private void AddPropertyChangedNotifier(ContractCodeDomInfo contractCodeDomInfo, bool isValueType)
        {
            if ((this.EnableDataBinding && this.SupportsDeclareEvents) && ((contractCodeDomInfo != null) && (contractCodeDomInfo.TypeDeclaration != null)))
            {
                CodeTypeDeclaration typeDeclaration = contractCodeDomInfo.TypeDeclaration;
                typeDeclaration.BaseTypes.Add(this.CodeTypeIPropertyChange);
                CodeMemberEvent propertyChangedEvent = this.PropertyChangedEvent;
                typeDeclaration.Members.Add(propertyChangedEvent);
                CodeMemberMethod raisePropertyChangedEventMethod = this.RaisePropertyChangedEventMethod;
                if (!isValueType)
                {
                    raisePropertyChangedEventMethod.Attributes |= MemberAttributes.Family;
                }
                typeDeclaration.Members.Add(raisePropertyChangedEventMethod);
                contractCodeDomInfo.GetMemberNames().Add(propertyChangedEvent.Name, null);
                contractCodeDomInfo.GetMemberNames().Add(raisePropertyChangedEventMethod.Name, null);
            }
        }

        private void AddReferencedAssembly(Assembly assembly)
        {
            string fileName = Path.GetFileName(assembly.Location);
            bool flag = false;
            using (StringEnumerator enumerator = this.codeCompileUnit.ReferencedAssemblies.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    if (string.Compare(enumerator.Current, fileName, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        flag = true;
                        goto Label_0054;
                    }
                }
            }
        Label_0054:
            if (!flag)
            {
                this.codeCompileUnit.ReferencedAssemblies.Add(fileName);
            }
        }

        private void AddSerializableAttribute(bool generateSerializable, CodeTypeDeclaration type, ContractCodeDomInfo contractCodeDomInfo)
        {
            if (generateSerializable)
            {
                type.CustomAttributes.Add(this.SerializableAttribute);
                this.AddImportStatement(Globals.TypeOfSerializableAttribute.Namespace, contractCodeDomInfo.CodeNamespace);
            }
        }

        private static void AddToNamespace(StringBuilder builder, string fragment, Dictionary<string, object> fragments)
        {
            if (fragment != null)
            {
                bool flag = true;
                int length = builder.Length;
                int fragmentLength = 0;
                for (int i = 0; (i < fragment.Length) && (builder.Length < 0x1ff); i++)
                {
                    char c = fragment[i];
                    if (IsValid(c))
                    {
                        if (flag && !IsValidStart(c))
                        {
                            builder.Append("_");
                        }
                        builder.Append(c);
                        fragmentLength++;
                        flag = false;
                    }
                    else if ((((c == '.') || (c == '/')) || (c == ':')) && ((builder.Length == 1) || ((builder.Length > 1) && (builder[builder.Length - 1] != '.'))))
                    {
                        AddNamespaceFragment(builder, length, fragmentLength, fragments);
                        builder.Append('.');
                        length = builder.Length;
                        fragmentLength = 0;
                        flag = true;
                    }
                }
                AddNamespaceFragment(builder, length, fragmentLength, fragments);
            }
        }

        private static string AppendToValidClrIdentifier(string identifier, string appendString)
        {
            int num = 0x1ff - identifier.Length;
            int length = appendString.Length;
            if (num < length)
            {
                identifier = identifier.Substring(0, 0x1ff - length);
            }
            identifier = identifier + appendString;
            return identifier;
        }

        private bool CanDeclareAssemblyAttribute(ContractCodeDomInfo contractCodeDomInfo)
        {
            return (this.SupportsAssemblyAttributes && !contractCodeDomInfo.UsesWildcardNamespace);
        }

        private CodeMemberProperty CreateProperty(CodeTypeReference type, string propertyName, string fieldName, bool isValueType)
        {
            CodeMemberProperty property = new CodeMemberProperty {
                Type = type,
                Name = propertyName,
                Attributes = MemberAttributes.Final
            };
            if (this.GenerateInternalTypes)
            {
                property.Attributes |= MemberAttributes.Assembly;
            }
            else
            {
                property.Attributes |= MemberAttributes.Public;
            }
            CodeMethodReturnStatement statement = new CodeMethodReturnStatement {
                Expression = new CodeFieldReferenceExpression(this.ThisReference, fieldName)
            };
            property.GetStatements.Add(statement);
            CodeAssignStatement statement2 = new CodeAssignStatement {
                Left = new CodeFieldReferenceExpression(this.ThisReference, fieldName),
                Right = new CodePropertySetValueReferenceExpression()
            };
            if (this.EnableDataBinding && this.SupportsDeclareEvents)
            {
                CodeConditionStatement statement3 = new CodeConditionStatement();
                CodeExpression targetObject = new CodeFieldReferenceExpression(this.ThisReference, fieldName);
                CodeExpression right = new CodePropertySetValueReferenceExpression();
                if (!isValueType)
                {
                    targetObject = new CodeMethodInvokeExpression(new CodeTypeReferenceExpression(Globals.TypeOfObject), "ReferenceEquals", new CodeExpression[] { targetObject, right });
                }
                else
                {
                    targetObject = new CodeMethodInvokeExpression(targetObject, "Equals", new CodeExpression[] { right });
                }
                right = new CodePrimitiveExpression(true);
                statement3.Condition = new CodeBinaryOperatorExpression(targetObject, CodeBinaryOperatorType.IdentityInequality, right);
                statement3.TrueStatements.Add(statement2);
                statement3.TrueStatements.Add(new CodeMethodInvokeExpression(this.ThisReference, this.RaisePropertyChangedEventMethod.Name, new CodeExpression[] { new CodePrimitiveExpression(propertyName) }));
                property.SetStatements.Add(statement3);
                return property;
            }
            property.SetStatements.Add(statement2);
            return property;
        }

        private static CodeTypeDeclaration CreateTypeDeclaration(string typeName, DataContract dataContract)
        {
            CodeTypeDeclaration declaration = new CodeTypeDeclaration(typeName);
            CodeAttributeDeclaration declaration2 = new CodeAttributeDeclaration(typeof(DebuggerStepThroughAttribute).FullName);
            CodeAttributeDeclaration declaration3 = new CodeAttributeDeclaration(typeof(GeneratedCodeAttribute).FullName);
            AssemblyName name = Assembly.GetExecutingAssembly().GetName();
            declaration3.Arguments.Add(new CodeAttributeArgument(new CodePrimitiveExpression(name.Name)));
            declaration3.Arguments.Add(new CodeAttributeArgument(new CodePrimitiveExpression(name.Version.ToString())));
            if (!(dataContract is EnumDataContract))
            {
                declaration.CustomAttributes.Add(declaration2);
            }
            declaration.CustomAttributes.Add(declaration3);
            return declaration;
        }

        internal void Export()
        {
            try
            {
                foreach (KeyValuePair<XmlQualifiedName, DataContract> pair in this.dataContractSet)
                {
                    DataContract dataContract = pair.Value;
                    if (!dataContract.IsBuiltInDataContract)
                    {
                        ContractCodeDomInfo contractCodeDomInfo = this.GetContractCodeDomInfo(dataContract);
                        if (!contractCodeDomInfo.IsProcessed)
                        {
                            if (dataContract is ClassDataContract)
                            {
                                ClassDataContract contract2 = (ClassDataContract) dataContract;
                                if (contract2.IsISerializable)
                                {
                                    this.ExportISerializableDataContract(contract2, contractCodeDomInfo);
                                }
                                else
                                {
                                    this.ExportClassDataContractHierarchy(contract2.StableName, contract2, contractCodeDomInfo, new Dictionary<XmlQualifiedName, object>());
                                }
                            }
                            else if (dataContract is CollectionDataContract)
                            {
                                this.ExportCollectionDataContract((CollectionDataContract) dataContract, contractCodeDomInfo);
                            }
                            else if (dataContract is EnumDataContract)
                            {
                                this.ExportEnumDataContract((EnumDataContract) dataContract, contractCodeDomInfo);
                            }
                            else
                            {
                                if (!(dataContract is XmlDataContract))
                                {
                                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(System.Runtime.Serialization.SR.GetString("UnexpectedContractType", new object[] { DataContract.GetClrTypeFullName(dataContract.GetType()), DataContract.GetClrTypeFullName(dataContract.UnderlyingType) })));
                                }
                                this.ExportXmlDataContract((XmlDataContract) dataContract, contractCodeDomInfo);
                            }
                            contractCodeDomInfo.IsProcessed = true;
                        }
                    }
                }
                if (this.dataContractSet.DataContractSurrogate != null)
                {
                    CodeNamespace[] array = new CodeNamespace[this.codeCompileUnit.Namespaces.Count];
                    this.codeCompileUnit.Namespaces.CopyTo(array, 0);
                    foreach (CodeNamespace namespace2 in array)
                    {
                        this.InvokeProcessImportedType(namespace2.Types);
                    }
                }
            }
            finally
            {
                System.CodeDom.Compiler.CodeGenerator.ValidateIdentifiers(this.codeCompileUnit);
            }
        }

        private void ExportClassDataContract(ClassDataContract classDataContract, ContractCodeDomInfo contractCodeDomInfo)
        {
            this.GenerateType(classDataContract, contractCodeDomInfo);
            if (!contractCodeDomInfo.ReferencedTypeExists)
            {
                CodeTypeDeclaration typeDeclaration = contractCodeDomInfo.TypeDeclaration;
                if (this.SupportsPartialTypes)
                {
                    typeDeclaration.IsPartial = true;
                }
                if (classDataContract.IsValueType && this.SupportsDeclareValueTypes)
                {
                    typeDeclaration.IsStruct = true;
                }
                else
                {
                    typeDeclaration.IsClass = true;
                }
                string nameForAttribute = this.GetNameForAttribute(classDataContract.StableName.Name);
                CodeAttributeDeclaration declaration2 = new CodeAttributeDeclaration(DataContract.GetClrTypeFullName(Globals.TypeOfDataContractAttribute));
                declaration2.Arguments.Add(new CodeAttributeArgument("Name", new CodePrimitiveExpression(nameForAttribute)));
                declaration2.Arguments.Add(new CodeAttributeArgument("Namespace", new CodePrimitiveExpression(classDataContract.StableName.Namespace)));
                if (classDataContract.IsReference)
                {
                    declaration2.Arguments.Add(new CodeAttributeArgument("IsReference", new CodePrimitiveExpression(classDataContract.IsReference)));
                }
                typeDeclaration.CustomAttributes.Add(declaration2);
                this.AddImportStatement(Globals.TypeOfDataContractAttribute.Namespace, contractCodeDomInfo.CodeNamespace);
                this.AddSerializableAttribute(this.GenerateSerializableTypes, typeDeclaration, contractCodeDomInfo);
                this.AddKnownTypes(classDataContract, contractCodeDomInfo);
                if (classDataContract.BaseContract == null)
                {
                    if (!typeDeclaration.IsStruct)
                    {
                        typeDeclaration.BaseTypes.Add(Globals.TypeOfObject);
                    }
                    this.AddExtensionData(contractCodeDomInfo);
                    this.AddPropertyChangedNotifier(contractCodeDomInfo, typeDeclaration.IsStruct);
                }
                else
                {
                    ContractCodeDomInfo baseContractCodeDomInfo = this.GetContractCodeDomInfo(classDataContract.BaseContract);
                    typeDeclaration.BaseTypes.Add(baseContractCodeDomInfo.TypeReference);
                    this.AddBaseMemberNames(baseContractCodeDomInfo, contractCodeDomInfo);
                    if (baseContractCodeDomInfo.ReferencedTypeExists)
                    {
                        Type baseType = (Type) baseContractCodeDomInfo.TypeReference.UserData[codeUserDataActualTypeKey];
                        this.ThrowIfReferencedBaseTypeSealed(baseType, classDataContract);
                        if (!Globals.TypeOfIExtensibleDataObject.IsAssignableFrom(baseType))
                        {
                            this.AddExtensionData(contractCodeDomInfo);
                        }
                        this.AddPropertyChangedNotifier(contractCodeDomInfo, typeDeclaration.IsStruct);
                    }
                }
                if (classDataContract.Members != null)
                {
                    for (int i = 0; i < classDataContract.Members.Count; i++)
                    {
                        DataMember key = classDataContract.Members[i];
                        CodeTypeReference elementTypeReference = this.GetElementTypeReference(key.MemberTypeContract, key.IsNullable && key.MemberTypeContract.IsValueType);
                        string memberName = this.GetNameForAttribute(key.Name);
                        string identifier = this.GetMemberName(memberName, contractCodeDomInfo);
                        string fieldName = this.GetMemberName(AppendToValidClrIdentifier(identifier, "Field"), contractCodeDomInfo);
                        CodeMemberField field = new CodeMemberField {
                            Type = elementTypeReference,
                            Name = fieldName,
                            Attributes = MemberAttributes.Private
                        };
                        CodeMemberProperty property = this.CreateProperty(elementTypeReference, identifier, fieldName, key.MemberTypeContract.IsValueType && this.SupportsDeclareValueTypes);
                        if (this.dataContractSet.DataContractSurrogate != null)
                        {
                            property.UserData.Add(surrogateDataKey, this.dataContractSet.GetSurrogateData(key));
                        }
                        CodeAttributeDeclaration declaration3 = new CodeAttributeDeclaration(DataContract.GetClrTypeFullName(Globals.TypeOfDataMemberAttribute));
                        if (memberName != property.Name)
                        {
                            declaration3.Arguments.Add(new CodeAttributeArgument("Name", new CodePrimitiveExpression(memberName)));
                        }
                        if (key.IsRequired)
                        {
                            declaration3.Arguments.Add(new CodeAttributeArgument("IsRequired", new CodePrimitiveExpression(key.IsRequired)));
                        }
                        if (!key.EmitDefaultValue)
                        {
                            declaration3.Arguments.Add(new CodeAttributeArgument("EmitDefaultValue", new CodePrimitiveExpression(key.EmitDefaultValue)));
                        }
                        if (key.Order != 0)
                        {
                            declaration3.Arguments.Add(new CodeAttributeArgument("Order", new CodePrimitiveExpression(key.Order)));
                        }
                        property.CustomAttributes.Add(declaration3);
                        if (this.GenerateSerializableTypes && !key.IsRequired)
                        {
                            CodeAttributeDeclaration declaration4 = new CodeAttributeDeclaration(DataContract.GetClrTypeFullName(Globals.TypeOfOptionalFieldAttribute));
                            field.CustomAttributes.Add(declaration4);
                        }
                        typeDeclaration.Members.Add(field);
                        typeDeclaration.Members.Add(property);
                    }
                }
            }
        }

        private void ExportClassDataContractHierarchy(XmlQualifiedName typeName, ClassDataContract classContract, ContractCodeDomInfo contractCodeDomInfo, Dictionary<XmlQualifiedName, object> contractNamesInHierarchy)
        {
            if (contractNamesInHierarchy.ContainsKey(classContract.StableName))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(System.Runtime.Serialization.SR.GetString("TypeCannotBeImported", new object[] { typeName.Name, typeName.Namespace, System.Runtime.Serialization.SR.GetString("CircularTypeReference", new object[] { classContract.StableName.Name, classContract.StableName.Namespace }) })));
            }
            contractNamesInHierarchy.Add(classContract.StableName, null);
            ClassDataContract baseContract = classContract.BaseContract;
            if (baseContract != null)
            {
                ContractCodeDomInfo info = this.GetContractCodeDomInfo(baseContract);
                if (!info.IsProcessed)
                {
                    this.ExportClassDataContractHierarchy(typeName, baseContract, info, contractNamesInHierarchy);
                    info.IsProcessed = true;
                }
            }
            this.ExportClassDataContract(classContract, contractCodeDomInfo);
        }

        private void ExportCollectionDataContract(CollectionDataContract collectionContract, ContractCodeDomInfo contractCodeDomInfo)
        {
            this.GenerateType(collectionContract, contractCodeDomInfo);
            if (!contractCodeDomInfo.ReferencedTypeExists)
            {
                CodeTypeReference codeTypeReference;
                string nameForAttribute = this.GetNameForAttribute(collectionContract.StableName.Name);
                if (!this.SupportsGenericTypeReference)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.Runtime.Serialization.SR.GetString("CannotUseGenericTypeAsBase", new object[] { nameForAttribute, collectionContract.StableName.Namespace })));
                }
                DataContract itemContract = collectionContract.ItemContract;
                bool isItemTypeNullable = collectionContract.IsItemTypeNullable;
                bool flag2 = this.TryGetReferencedDictionaryType(collectionContract, out codeTypeReference);
                if (!flag2)
                {
                    if (collectionContract.IsDictionary)
                    {
                        this.GenerateKeyValueType(collectionContract.ItemContract as ClassDataContract);
                    }
                    if (!this.TryGetReferencedListType(itemContract, isItemTypeNullable, out codeTypeReference))
                    {
                        if (!this.SupportsGenericTypeReference)
                        {
                            string str2 = "ArrayOf" + itemContract.StableName.Name;
                            string collectionNamespace = DataContract.GetCollectionNamespace(itemContract.StableName.Namespace);
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.Runtime.Serialization.SR.GetString("ReferencedBaseTypeDoesNotExist", new object[] { nameForAttribute, collectionContract.StableName.Namespace, str2, collectionNamespace, DataContract.GetClrTypeFullName(Globals.TypeOfIListGeneric), DataContract.GetClrTypeFullName(Globals.TypeOfICollectionGeneric) })));
                        }
                        codeTypeReference = this.GetCodeTypeReference(Globals.TypeOfListGeneric);
                        codeTypeReference.TypeArguments.Add(this.GetElementTypeReference(itemContract, isItemTypeNullable));
                    }
                }
                CodeTypeDeclaration typeDeclaration = contractCodeDomInfo.TypeDeclaration;
                typeDeclaration.BaseTypes.Add(codeTypeReference);
                CodeAttributeDeclaration declaration2 = new CodeAttributeDeclaration(DataContract.GetClrTypeFullName(Globals.TypeOfCollectionDataContractAttribute));
                declaration2.Arguments.Add(new CodeAttributeArgument("Name", new CodePrimitiveExpression(nameForAttribute)));
                declaration2.Arguments.Add(new CodeAttributeArgument("Namespace", new CodePrimitiveExpression(collectionContract.StableName.Namespace)));
                if (collectionContract.IsReference)
                {
                    declaration2.Arguments.Add(new CodeAttributeArgument("IsReference", new CodePrimitiveExpression(collectionContract.IsReference)));
                }
                declaration2.Arguments.Add(new CodeAttributeArgument("ItemName", new CodePrimitiveExpression(this.GetNameForAttribute(collectionContract.ItemName))));
                if (flag2)
                {
                    declaration2.Arguments.Add(new CodeAttributeArgument("KeyName", new CodePrimitiveExpression(this.GetNameForAttribute(collectionContract.KeyName))));
                    declaration2.Arguments.Add(new CodeAttributeArgument("ValueName", new CodePrimitiveExpression(this.GetNameForAttribute(collectionContract.ValueName))));
                }
                typeDeclaration.CustomAttributes.Add(declaration2);
                this.AddImportStatement(Globals.TypeOfCollectionDataContractAttribute.Namespace, contractCodeDomInfo.CodeNamespace);
                this.AddSerializableAttribute(this.GenerateSerializableTypes, typeDeclaration, contractCodeDomInfo);
            }
        }

        private void ExportEnumDataContract(EnumDataContract enumDataContract, ContractCodeDomInfo contractCodeDomInfo)
        {
            this.GenerateType(enumDataContract, contractCodeDomInfo);
            if (!contractCodeDomInfo.ReferencedTypeExists)
            {
                CodeTypeDeclaration typeDeclaration = contractCodeDomInfo.TypeDeclaration;
                typeDeclaration.IsEnum = true;
                typeDeclaration.BaseTypes.Add(EnumDataContract.GetBaseType(enumDataContract.BaseContractName));
                if (enumDataContract.IsFlags)
                {
                    typeDeclaration.CustomAttributes.Add(new CodeAttributeDeclaration(DataContract.GetClrTypeFullName(Globals.TypeOfFlagsAttribute)));
                    this.AddImportStatement(Globals.TypeOfFlagsAttribute.Namespace, contractCodeDomInfo.CodeNamespace);
                }
                string nameForAttribute = this.GetNameForAttribute(enumDataContract.StableName.Name);
                CodeAttributeDeclaration declaration2 = new CodeAttributeDeclaration(DataContract.GetClrTypeFullName(Globals.TypeOfDataContractAttribute));
                declaration2.Arguments.Add(new CodeAttributeArgument("Name", new CodePrimitiveExpression(nameForAttribute)));
                declaration2.Arguments.Add(new CodeAttributeArgument("Namespace", new CodePrimitiveExpression(enumDataContract.StableName.Namespace)));
                typeDeclaration.CustomAttributes.Add(declaration2);
                this.AddImportStatement(Globals.TypeOfDataContractAttribute.Namespace, contractCodeDomInfo.CodeNamespace);
                if (enumDataContract.Members != null)
                {
                    for (int i = 0; i < enumDataContract.Members.Count; i++)
                    {
                        string name = enumDataContract.Members[i].Name;
                        long num2 = enumDataContract.Values[i];
                        CodeMemberField field = new CodeMemberField();
                        if (enumDataContract.IsULong)
                        {
                            field.InitExpression = new CodeSnippetExpression(enumDataContract.GetStringFromEnumValue(num2));
                        }
                        else
                        {
                            field.InitExpression = new CodePrimitiveExpression(num2);
                        }
                        field.Name = this.GetMemberName(name, contractCodeDomInfo);
                        CodeAttributeDeclaration declaration3 = new CodeAttributeDeclaration(DataContract.GetClrTypeFullName(Globals.TypeOfEnumMemberAttribute));
                        if (field.Name != name)
                        {
                            declaration3.Arguments.Add(new CodeAttributeArgument("Value", new CodePrimitiveExpression(name)));
                        }
                        field.CustomAttributes.Add(declaration3);
                        typeDeclaration.Members.Add(field);
                    }
                }
            }
        }

        private void ExportISerializableDataContract(ClassDataContract dataContract, ContractCodeDomInfo contractCodeDomInfo)
        {
            this.GenerateType(dataContract, contractCodeDomInfo);
            if (!contractCodeDomInfo.ReferencedTypeExists)
            {
                if (DataContract.GetDefaultStableNamespace(contractCodeDomInfo.ClrNamespace) != dataContract.StableName.Namespace)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(System.Runtime.Serialization.SR.GetString("InvalidClrNamespaceGeneratedForISerializable", new object[] { dataContract.StableName.Name, dataContract.StableName.Namespace, DataContract.GetDataContractNamespaceFromUri(dataContract.StableName.Namespace), contractCodeDomInfo.ClrNamespace })));
                }
                string nameForAttribute = this.GetNameForAttribute(dataContract.StableName.Name);
                int num = nameForAttribute.LastIndexOf('.');
                string str2 = ((num <= 0) || (num == (nameForAttribute.Length - 1))) ? nameForAttribute : nameForAttribute.Substring(num + 1);
                if (contractCodeDomInfo.TypeDeclaration.Name != str2)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(System.Runtime.Serialization.SR.GetString("InvalidClrNameGeneratedForISerializable", new object[] { dataContract.StableName.Name, dataContract.StableName.Namespace, contractCodeDomInfo.TypeDeclaration.Name })));
                }
                CodeTypeDeclaration typeDeclaration = contractCodeDomInfo.TypeDeclaration;
                if (this.SupportsPartialTypes)
                {
                    typeDeclaration.IsPartial = true;
                }
                if (dataContract.IsValueType && this.SupportsDeclareValueTypes)
                {
                    typeDeclaration.IsStruct = true;
                }
                else
                {
                    typeDeclaration.IsClass = true;
                }
                this.AddSerializableAttribute(true, typeDeclaration, contractCodeDomInfo);
                this.AddKnownTypes(dataContract, contractCodeDomInfo);
                if (dataContract.BaseContract == null)
                {
                    if (!typeDeclaration.IsStruct)
                    {
                        typeDeclaration.BaseTypes.Add(Globals.TypeOfObject);
                    }
                    typeDeclaration.BaseTypes.Add(DataContract.GetClrTypeFullName(Globals.TypeOfISerializable));
                    typeDeclaration.Members.Add(this.ISerializableBaseConstructor);
                    typeDeclaration.Members.Add(this.SerializationInfoField);
                    typeDeclaration.Members.Add(this.SerializationInfoProperty);
                    typeDeclaration.Members.Add(this.GetObjectDataMethod);
                    this.AddPropertyChangedNotifier(contractCodeDomInfo, typeDeclaration.IsStruct);
                }
                else
                {
                    ContractCodeDomInfo info = this.GetContractCodeDomInfo(dataContract.BaseContract);
                    this.GenerateType(dataContract.BaseContract, info);
                    typeDeclaration.BaseTypes.Add(info.TypeReference);
                    if (info.ReferencedTypeExists)
                    {
                        Type baseType = (Type) info.TypeReference.UserData[codeUserDataActualTypeKey];
                        this.ThrowIfReferencedBaseTypeSealed(baseType, dataContract);
                    }
                    typeDeclaration.Members.Add(this.ISerializableDerivedConstructor);
                }
            }
        }

        private void ExportXmlDataContract(XmlDataContract xmlDataContract, ContractCodeDomInfo contractCodeDomInfo)
        {
            this.GenerateType(xmlDataContract, contractCodeDomInfo);
            if (!contractCodeDomInfo.ReferencedTypeExists)
            {
                CodeTypeDeclaration typeDeclaration = contractCodeDomInfo.TypeDeclaration;
                if (this.SupportsPartialTypes)
                {
                    typeDeclaration.IsPartial = true;
                }
                if (xmlDataContract.IsValueType)
                {
                    typeDeclaration.IsStruct = true;
                }
                else
                {
                    typeDeclaration.IsClass = true;
                    typeDeclaration.BaseTypes.Add(Globals.TypeOfObject);
                }
                this.AddSerializableAttribute(this.GenerateSerializableTypes, typeDeclaration, contractCodeDomInfo);
                typeDeclaration.BaseTypes.Add(DataContract.GetClrTypeFullName(Globals.TypeOfIXmlSerializable));
                typeDeclaration.Members.Add(this.NodeArrayField);
                typeDeclaration.Members.Add(this.NodeArrayProperty);
                typeDeclaration.Members.Add(this.ReadXmlMethod);
                typeDeclaration.Members.Add(this.WriteXmlMethod);
                typeDeclaration.Members.Add(this.GetSchemaMethod);
                if (xmlDataContract.IsAnonymous && !xmlDataContract.HasRoot)
                {
                    typeDeclaration.CustomAttributes.Add(new CodeAttributeDeclaration(DataContract.GetClrTypeFullName(Globals.TypeOfXmlSchemaProviderAttribute), new CodeAttributeArgument[] { new CodeAttributeArgument(this.NullReference), new CodeAttributeArgument("IsAny", new CodePrimitiveExpression(true)) }));
                }
                else
                {
                    CodeMemberField field;
                    typeDeclaration.CustomAttributes.Add(new CodeAttributeDeclaration(DataContract.GetClrTypeFullName(Globals.TypeOfXmlSchemaProviderAttribute), new CodeAttributeArgument[] { new CodeAttributeArgument(new CodePrimitiveExpression("ExportSchema")) }));
                    field = new CodeMemberField(Globals.TypeOfXmlQualifiedName, typeNameFieldName) {
                        Attributes = field.Attributes | (MemberAttributes.Private | MemberAttributes.Static)
                    };
                    XmlQualifiedName name = xmlDataContract.IsAnonymous ? SchemaImporter.ImportActualType(xmlDataContract.XsdType.Annotation, xmlDataContract.StableName, xmlDataContract.StableName) : xmlDataContract.StableName;
                    field.InitExpression = new CodeObjectCreateExpression(Globals.TypeOfXmlQualifiedName, new CodeExpression[] { new CodePrimitiveExpression(name.Name), new CodePrimitiveExpression(name.Namespace) });
                    typeDeclaration.Members.Add(field);
                    typeDeclaration.Members.Add(this.GetSchemaStaticMethod);
                    bool flag = ((xmlDataContract.TopLevelElementName != null) && (xmlDataContract.TopLevelElementName.Value != xmlDataContract.StableName.Name)) || ((xmlDataContract.TopLevelElementNamespace != null) && (xmlDataContract.TopLevelElementNamespace.Value != xmlDataContract.StableName.Namespace));
                    if (flag || !xmlDataContract.IsTopLevelElementNullable)
                    {
                        CodeAttributeDeclaration declaration2 = new CodeAttributeDeclaration(DataContract.GetClrTypeFullName(Globals.TypeOfXmlRootAttribute));
                        if (flag)
                        {
                            if (xmlDataContract.TopLevelElementName != null)
                            {
                                declaration2.Arguments.Add(new CodeAttributeArgument("ElementName", new CodePrimitiveExpression(xmlDataContract.TopLevelElementName.Value)));
                            }
                            if (xmlDataContract.TopLevelElementNamespace != null)
                            {
                                declaration2.Arguments.Add(new CodeAttributeArgument("Namespace", new CodePrimitiveExpression(xmlDataContract.TopLevelElementNamespace.Value)));
                            }
                        }
                        if (!xmlDataContract.IsTopLevelElementNullable)
                        {
                            declaration2.Arguments.Add(new CodeAttributeArgument("IsNullable", new CodePrimitiveExpression(false)));
                        }
                        typeDeclaration.CustomAttributes.Add(declaration2);
                    }
                }
                this.AddPropertyChangedNotifier(contractCodeDomInfo, typeDeclaration.IsStruct);
            }
        }

        private void GenerateKeyValueType(ClassDataContract keyValueContract)
        {
            if (((keyValueContract != null) && (this.dataContractSet[keyValueContract.StableName] == null)) && (this.dataContractSet.GetContractCodeDomInfo(keyValueContract) == null))
            {
                ContractCodeDomInfo info = new ContractCodeDomInfo();
                this.dataContractSet.SetContractCodeDomInfo(keyValueContract, info);
                this.ExportClassDataContract(keyValueContract, info);
                info.IsProcessed = true;
            }
        }

        private void GenerateType(DataContract dataContract, ContractCodeDomInfo contractCodeDomInfo)
        {
            if (!contractCodeDomInfo.IsProcessed)
            {
                CodeTypeReference referencedType = this.GetReferencedType(dataContract);
                if (referencedType != null)
                {
                    contractCodeDomInfo.TypeReference = referencedType;
                    contractCodeDomInfo.ReferencedTypeExists = true;
                }
                else if (contractCodeDomInfo.TypeDeclaration == null)
                {
                    string clrNamespace = this.GetClrNamespace(dataContract, contractCodeDomInfo);
                    CodeNamespace ns = this.GetCodeNamespace(clrNamespace, dataContract.StableName.Namespace, contractCodeDomInfo);
                    CodeTypeDeclaration nestedType = this.GetNestedType(dataContract, contractCodeDomInfo);
                    if (nestedType == null)
                    {
                        string clrIdentifier = GetClrIdentifier(XmlConvert.DecodeName(dataContract.StableName.Name), "GeneratedType");
                        if (this.NamespaceContainsType(ns, clrIdentifier) || this.GlobalTypeNameConflicts(clrNamespace, clrIdentifier))
                        {
                            int num = 1;
                            while (true)
                            {
                                string typeName = AppendToValidClrIdentifier(clrIdentifier, num.ToString(NumberFormatInfo.InvariantInfo));
                                if (!this.NamespaceContainsType(ns, typeName) && !this.GlobalTypeNameConflicts(clrNamespace, typeName))
                                {
                                    clrIdentifier = typeName;
                                    break;
                                }
                                if (num == 0x7fffffff)
                                {
                                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(System.Runtime.Serialization.SR.GetString("CannotComputeUniqueName", new object[] { clrIdentifier })));
                                }
                                num++;
                            }
                        }
                        nestedType = CreateTypeDeclaration(clrIdentifier, dataContract);
                        ns.Types.Add(nestedType);
                        if (string.IsNullOrEmpty(clrNamespace))
                        {
                            this.AddGlobalTypeName(clrIdentifier);
                        }
                        contractCodeDomInfo.TypeReference = new CodeTypeReference(((clrNamespace == null) || (clrNamespace.Length == 0)) ? clrIdentifier : (clrNamespace + "." + clrIdentifier));
                        if (this.GenerateInternalTypes)
                        {
                            nestedType.TypeAttributes = TypeAttributes.AnsiClass;
                        }
                        else
                        {
                            nestedType.TypeAttributes = TypeAttributes.Public;
                        }
                    }
                    if (this.dataContractSet.DataContractSurrogate != null)
                    {
                        nestedType.UserData.Add(surrogateDataKey, this.dataContractSet.GetSurrogateData(dataContract));
                    }
                    contractCodeDomInfo.TypeDeclaration = nestedType;
                }
            }
        }

        private static string GetClrIdentifier(string identifier, string defaultIdentifier)
        {
            if ((identifier.Length <= 0x1ff) && System.CodeDom.Compiler.CodeGenerator.IsValidLanguageIndependentIdentifier(identifier))
            {
                return identifier;
            }
            bool flag = true;
            StringBuilder builder = new StringBuilder();
            for (int i = 0; (i < identifier.Length) && (builder.Length < 0x1ff); i++)
            {
                char c = identifier[i];
                if (IsValid(c))
                {
                    if (flag && !IsValidStart(c))
                    {
                        builder.Append("_");
                    }
                    builder.Append(c);
                    flag = false;
                }
            }
            if (builder.Length == 0)
            {
                return defaultIdentifier;
            }
            return builder.ToString();
        }

        private static string GetClrNamespace(string dataContractNamespace)
        {
            if ((dataContractNamespace == null) || (dataContractNamespace.Length == 0))
            {
                return string.Empty;
            }
            Uri result = null;
            StringBuilder builder = new StringBuilder();
            if (Uri.TryCreate(dataContractNamespace, UriKind.RelativeOrAbsolute, out result))
            {
                Dictionary<string, object> fragments = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                if (!result.IsAbsoluteUri)
                {
                    AddToNamespace(builder, result.OriginalString, fragments);
                }
                else
                {
                    string absoluteUri = result.AbsoluteUri;
                    if (absoluteUri.StartsWith("http://schemas.datacontract.org/2004/07/", StringComparison.Ordinal))
                    {
                        AddToNamespace(builder, absoluteUri.Substring("http://schemas.datacontract.org/2004/07/".Length), fragments);
                    }
                    else
                    {
                        string host = result.Host;
                        if (host != null)
                        {
                            AddToNamespace(builder, host, fragments);
                        }
                        string pathAndQuery = result.PathAndQuery;
                        if (pathAndQuery != null)
                        {
                            AddToNamespace(builder, pathAndQuery, fragments);
                        }
                    }
                }
            }
            if (builder.Length == 0)
            {
                return string.Empty;
            }
            int length = builder.Length;
            if (builder[builder.Length - 1] == '.')
            {
                length--;
            }
            length = Math.Min(0x1ff, length);
            return builder.ToString(0, length);
        }

        private string GetClrNamespace(DataContract dataContract, ContractCodeDomInfo contractCodeDomInfo)
        {
            string clrNamespace = contractCodeDomInfo.ClrNamespace;
            bool flag = false;
            if (clrNamespace == null)
            {
                if (!this.Namespaces.TryGetValue(dataContract.StableName.Namespace, out clrNamespace))
                {
                    if (this.Namespaces.TryGetValue(wildcardNamespaceMapping, out clrNamespace))
                    {
                        flag = true;
                    }
                    else
                    {
                        clrNamespace = GetClrNamespace(dataContract.StableName.Namespace);
                        if (this.ClrNamespaces.ContainsKey(clrNamespace))
                        {
                            string key = null;
                            int num = 1;
                            while (true)
                            {
                                key = ((clrNamespace.Length == 0) ? "GeneratedNamespace" : clrNamespace) + num.ToString(NumberFormatInfo.InvariantInfo);
                                if (!this.ClrNamespaces.ContainsKey(key))
                                {
                                    clrNamespace = key;
                                    break;
                                }
                                num++;
                            }
                        }
                        this.AddNamespacePair(dataContract.StableName.Namespace, clrNamespace);
                    }
                }
                contractCodeDomInfo.ClrNamespace = clrNamespace;
                contractCodeDomInfo.UsesWildcardNamespace = flag;
            }
            return clrNamespace;
        }

        private CodeNamespace GetCodeNamespace(string clrNamespace, string dataContractNamespace, ContractCodeDomInfo contractCodeDomInfo)
        {
            if (contractCodeDomInfo.CodeNamespace != null)
            {
                return contractCodeDomInfo.CodeNamespace;
            }
            CodeNamespaceCollection namespaces = this.codeCompileUnit.Namespaces;
            foreach (CodeNamespace namespace2 in namespaces)
            {
                if (namespace2.Name == clrNamespace)
                {
                    contractCodeDomInfo.CodeNamespace = namespace2;
                    return namespace2;
                }
            }
            CodeNamespace namespace3 = new CodeNamespace(clrNamespace);
            namespaces.Add(namespace3);
            if (this.CanDeclareAssemblyAttribute(contractCodeDomInfo) && this.NeedsExplicitNamespace(dataContractNamespace, clrNamespace))
            {
                CodeAttributeDeclaration declaration = new CodeAttributeDeclaration(DataContract.GetClrTypeFullName(Globals.TypeOfContractNamespaceAttribute));
                declaration.Arguments.Add(new CodeAttributeArgument(new CodePrimitiveExpression(dataContractNamespace)));
                declaration.Arguments.Add(new CodeAttributeArgument("ClrNamespace", new CodePrimitiveExpression(clrNamespace)));
                this.codeCompileUnit.AssemblyCustomAttributes.Add(declaration);
            }
            contractCodeDomInfo.CodeNamespace = namespace3;
            return namespace3;
        }

        internal CodeTypeReference GetCodeTypeReference(DataContract dataContract)
        {
            if (dataContract.IsBuiltInDataContract)
            {
                return this.GetCodeTypeReference(dataContract.UnderlyingType);
            }
            ContractCodeDomInfo contractCodeDomInfo = this.GetContractCodeDomInfo(dataContract);
            this.GenerateType(dataContract, contractCodeDomInfo);
            return contractCodeDomInfo.TypeReference;
        }

        private CodeTypeReference GetCodeTypeReference(Type type)
        {
            this.AddReferencedAssembly(type.Assembly);
            return new CodeTypeReference(type);
        }

        private ContractCodeDomInfo GetContractCodeDomInfo(DataContract dataContract)
        {
            ContractCodeDomInfo contractCodeDomInfo = this.dataContractSet.GetContractCodeDomInfo(dataContract);
            if (contractCodeDomInfo == null)
            {
                contractCodeDomInfo = new ContractCodeDomInfo();
                this.dataContractSet.SetContractCodeDomInfo(dataContract, contractCodeDomInfo);
            }
            return contractCodeDomInfo;
        }

        internal CodeTypeReference GetElementTypeReference(DataContract dataContract, bool isElementTypeNullable)
        {
            CodeTypeReference codeTypeReference = this.GetCodeTypeReference(dataContract);
            if (dataContract.IsValueType && isElementTypeNullable)
            {
                codeTypeReference = this.WrapNullable(codeTypeReference);
            }
            return codeTypeReference;
        }

        private Dictionary<XmlQualifiedName, DataContract> GetKnownTypeContracts(DataContract dataContract)
        {
            if ((this.dataContractSet.KnownTypesForObject != null) && SchemaImporter.IsObjectContract(dataContract))
            {
                return this.dataContractSet.KnownTypesForObject;
            }
            if (dataContract is ClassDataContract)
            {
                ContractCodeDomInfo contractCodeDomInfo = this.GetContractCodeDomInfo(dataContract);
                if (!contractCodeDomInfo.IsProcessed)
                {
                    this.GenerateType(dataContract, contractCodeDomInfo);
                }
                if (contractCodeDomInfo.ReferencedTypeExists)
                {
                    return this.GetKnownTypeContracts((ClassDataContract) dataContract, new Dictionary<DataContract, object>());
                }
            }
            return null;
        }

        private Dictionary<XmlQualifiedName, DataContract> GetKnownTypeContracts(ClassDataContract dataContract, Dictionary<DataContract, object> handledContracts)
        {
            if (!handledContracts.ContainsKey(dataContract))
            {
                handledContracts.Add(dataContract, null);
                if (dataContract.Members != null)
                {
                    bool flag = false;
                    foreach (DataMember member in dataContract.Members)
                    {
                        DataContract memberTypeContract = member.MemberTypeContract;
                        if ((!flag && (this.dataContractSet.KnownTypesForObject != null)) && SchemaImporter.IsObjectContract(memberTypeContract))
                        {
                            this.AddKnownTypeContracts(dataContract, this.dataContractSet.KnownTypesForObject);
                            flag = true;
                        }
                        else if (memberTypeContract is ClassDataContract)
                        {
                            ContractCodeDomInfo contractCodeDomInfo = this.GetContractCodeDomInfo(memberTypeContract);
                            if (!contractCodeDomInfo.IsProcessed)
                            {
                                this.GenerateType(memberTypeContract, contractCodeDomInfo);
                            }
                            if (contractCodeDomInfo.ReferencedTypeExists)
                            {
                                this.AddKnownTypeContracts(dataContract, this.GetKnownTypeContracts((ClassDataContract) memberTypeContract, handledContracts));
                            }
                        }
                    }
                }
            }
            return dataContract.KnownDataContracts;
        }

        internal ICollection<CodeTypeReference> GetKnownTypeReferences(DataContract dataContract)
        {
            Dictionary<XmlQualifiedName, DataContract> knownTypeContracts = this.GetKnownTypeContracts(dataContract);
            if (knownTypeContracts == null)
            {
                return null;
            }
            ICollection<DataContract> values = knownTypeContracts.Values;
            if ((values == null) || (values.Count == 0))
            {
                return null;
            }
            List<CodeTypeReference> list = new List<CodeTypeReference>();
            foreach (DataContract contract in values)
            {
                list.Add(this.GetCodeTypeReference(contract));
            }
            return list;
        }

        private string GetMemberName(string memberName, ContractCodeDomInfo contractCodeDomInfo)
        {
            memberName = GetClrIdentifier(memberName, "GeneratedMember");
            if (memberName == contractCodeDomInfo.TypeDeclaration.Name)
            {
                memberName = AppendToValidClrIdentifier(memberName, "Member");
            }
            if (contractCodeDomInfo.GetMemberNames().ContainsKey(memberName))
            {
                string key = null;
                int num = 1;
                while (true)
                {
                    key = AppendToValidClrIdentifier(memberName, num.ToString(NumberFormatInfo.InvariantInfo));
                    if (!contractCodeDomInfo.GetMemberNames().ContainsKey(key))
                    {
                        memberName = key;
                        break;
                    }
                    num++;
                }
            }
            contractCodeDomInfo.GetMemberNames().Add(memberName, null);
            return memberName;
        }

        private string GetNameForAttribute(string name)
        {
            string strB = XmlConvert.DecodeName(name);
            if (string.CompareOrdinal(name, strB) == 0)
            {
                return name;
            }
            string str2 = DataContract.EncodeLocalName(strB);
            if (string.CompareOrdinal(name, str2) != 0)
            {
                return name;
            }
            return strB;
        }

        private CodeTypeDeclaration GetNestedType(DataContract dataContract, ContractCodeDomInfo contractCodeDomInfo)
        {
            if (!this.SupportsNestedTypes)
            {
                return null;
            }
            string name = dataContract.StableName.Name;
            int length = name.LastIndexOf('.');
            if (length <= 0)
            {
                return null;
            }
            string str2 = name.Substring(0, length);
            DataContract contract = this.dataContractSet[new XmlQualifiedName(str2, dataContract.StableName.Namespace)];
            if (contract == null)
            {
                return null;
            }
            string clrIdentifier = GetClrIdentifier(XmlConvert.DecodeName(name.Substring(length + 1)), "GeneratedType");
            ContractCodeDomInfo info = this.GetContractCodeDomInfo(contract);
            this.GenerateType(contract, info);
            if (info.ReferencedTypeExists)
            {
                return null;
            }
            CodeTypeDeclaration typeDeclaration = info.TypeDeclaration;
            if (this.TypeContainsNestedType(typeDeclaration, clrIdentifier))
            {
                int num2 = 1;
                while (true)
                {
                    string typeName = AppendToValidClrIdentifier(clrIdentifier, num2.ToString(NumberFormatInfo.InvariantInfo));
                    if (!this.TypeContainsNestedType(typeDeclaration, typeName))
                    {
                        clrIdentifier = typeName;
                        break;
                    }
                    num2++;
                }
            }
            CodeTypeDeclaration declaration2 = CreateTypeDeclaration(clrIdentifier, dataContract);
            typeDeclaration.Members.Add(declaration2);
            contractCodeDomInfo.TypeReference = new CodeTypeReference(info.TypeReference.BaseType + "+" + clrIdentifier);
            if (this.GenerateInternalTypes)
            {
                declaration2.TypeAttributes = TypeAttributes.NestedAssembly;
                return declaration2;
            }
            declaration2.TypeAttributes = TypeAttributes.NestedPublic;
            return declaration2;
        }

        private CodeTypeReference GetReferencedCollectionType(CollectionDataContract collectionContract)
        {
            CodeTypeReference reference;
            if (collectionContract == null)
            {
                return null;
            }
            if (!this.HasDefaultCollectionNames(collectionContract))
            {
                return null;
            }
            if (!this.TryGetReferencedDictionaryType(collectionContract, out reference))
            {
                DataContract itemContract = collectionContract.ItemContract;
                if (collectionContract.IsDictionary)
                {
                    this.GenerateKeyValueType(itemContract as ClassDataContract);
                }
                bool isItemTypeNullable = collectionContract.IsItemTypeNullable;
                if (!this.TryGetReferencedListType(itemContract, isItemTypeNullable, out reference))
                {
                    reference = new CodeTypeReference(this.GetElementTypeReference(itemContract, isItemTypeNullable), 1);
                }
            }
            return reference;
        }

        private CodeTypeReference GetReferencedGenericType(GenericInfo genInfo, out DataContract dataContract)
        {
            Type type;
            dataContract = null;
            if (!this.SupportsGenericTypeReference)
            {
                return null;
            }
            if (!this.TryGetReferencedType(genInfo.StableName, null, out type))
            {
                if (genInfo.Parameters != null)
                {
                    return null;
                }
                dataContract = this.dataContractSet[genInfo.StableName];
                if (dataContract == null)
                {
                    return null;
                }
                if (dataContract.GenericInfo != null)
                {
                    return null;
                }
                return this.GetCodeTypeReference(dataContract);
            }
            bool flag = type != Globals.TypeOfNullable;
            CodeTypeReference codeTypeReference = this.GetCodeTypeReference(type);
            codeTypeReference.UserData.Add(codeUserDataActualTypeKey, type);
            if (genInfo.Parameters != null)
            {
                DataContract[] paramContracts = new DataContract[genInfo.Parameters.Count];
                for (int i = 0; i < genInfo.Parameters.Count; i++)
                {
                    CodeTypeReference referencedGenericType;
                    bool isValueType;
                    GenericInfo info = genInfo.Parameters[i];
                    XmlQualifiedName expandedStableName = info.GetExpandedStableName();
                    DataContract contract = this.dataContractSet[expandedStableName];
                    if (contract != null)
                    {
                        referencedGenericType = this.GetCodeTypeReference(contract);
                        isValueType = contract.IsValueType;
                    }
                    else
                    {
                        referencedGenericType = this.GetReferencedGenericType(info, out contract);
                        isValueType = (referencedGenericType != null) && (referencedGenericType.ArrayRank == 0);
                    }
                    paramContracts[i] = contract;
                    if (contract == null)
                    {
                        flag = false;
                    }
                    if (referencedGenericType == null)
                    {
                        return null;
                    }
                    if ((type == Globals.TypeOfNullable) && !isValueType)
                    {
                        return referencedGenericType;
                    }
                    codeTypeReference.TypeArguments.Add(referencedGenericType);
                }
                if (flag)
                {
                    dataContract = DataContract.GetDataContract(type).BindGenericParameters(paramContracts, new Dictionary<DataContract, DataContract>());
                }
            }
            return codeTypeReference;
        }

        [SecuritySafeCritical]
        private CodeTypeReference GetReferencedType(DataContract dataContract)
        {
            Type type = null;
            CodeTypeReference surrogatedTypeReference = this.GetSurrogatedTypeReference(dataContract);
            if (surrogatedTypeReference == null)
            {
                DataContract contract3;
                if ((this.TryGetReferencedType(dataContract.StableName, dataContract, out type) && !type.IsGenericTypeDefinition) && !type.ContainsGenericParameters)
                {
                    if (dataContract is XmlDataContract)
                    {
                        if (!Globals.TypeOfIXmlSerializable.IsAssignableFrom(type))
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(System.Runtime.Serialization.SR.GetString("TypeMustBeIXmlSerializable", new object[] { DataContract.GetClrTypeFullName(type), DataContract.GetClrTypeFullName(Globals.TypeOfIXmlSerializable), dataContract.StableName.Name, dataContract.StableName.Namespace })));
                        }
                        XmlDataContract contract = (XmlDataContract) dataContract;
                        if (contract.IsTypeDefinedOnImport)
                        {
                            if (!contract.Equals(this.dataContractSet.GetDataContract(type)))
                            {
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.Runtime.Serialization.SR.GetString("ReferencedTypeDoesNotMatch", new object[] { type.AssemblyQualifiedName, dataContract.StableName.Name, dataContract.StableName.Namespace })));
                            }
                        }
                        else
                        {
                            contract.IsValueType = type.IsValueType;
                            contract.IsTypeDefinedOnImport = true;
                        }
                        return this.GetCodeTypeReference(type);
                    }
                    if (!this.dataContractSet.GetDataContract(type).Equals(dataContract))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.Runtime.Serialization.SR.GetString("ReferencedTypeDoesNotMatch", new object[] { type.AssemblyQualifiedName, dataContract.StableName.Name, dataContract.StableName.Namespace })));
                    }
                    surrogatedTypeReference = this.GetCodeTypeReference(type);
                    surrogatedTypeReference.UserData.Add(codeUserDataActualTypeKey, type);
                    return surrogatedTypeReference;
                }
                if (dataContract.GenericInfo == null)
                {
                    return this.GetReferencedCollectionType(dataContract as CollectionDataContract);
                }
                XmlQualifiedName expandedStableName = dataContract.GenericInfo.GetExpandedStableName();
                if (expandedStableName != dataContract.StableName)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(System.Runtime.Serialization.SR.GetString("GenericTypeNameMismatch", new object[] { dataContract.StableName.Name, dataContract.StableName.Namespace, expandedStableName.Name, expandedStableName.Namespace })));
                }
                surrogatedTypeReference = this.GetReferencedGenericType(dataContract.GenericInfo, out contract3);
                if ((contract3 != null) && !contract3.Equals(dataContract))
                {
                    type = (Type) surrogatedTypeReference.UserData[codeUserDataActualTypeKey];
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.Runtime.Serialization.SR.GetString("ReferencedTypeDoesNotMatch", new object[] { type.AssemblyQualifiedName, contract3.StableName.Name, contract3.StableName.Namespace })));
                }
            }
            return surrogatedTypeReference;
        }

        private CodeTypeReference GetSurrogatedTypeReference(DataContract dataContract)
        {
            IDataContractSurrogate dataContractSurrogate = this.dataContractSet.DataContractSurrogate;
            if (dataContractSurrogate != null)
            {
                Type type = DataContractSurrogateCaller.GetReferencedTypeOnImport(dataContractSurrogate, dataContract.StableName.Name, dataContract.StableName.Namespace, this.dataContractSet.GetSurrogateData(dataContract));
                if (type != null)
                {
                    CodeTypeReference codeTypeReference = this.GetCodeTypeReference(type);
                    codeTypeReference.UserData.Add(codeUserDataActualTypeKey, type);
                    return codeTypeReference;
                }
            }
            return null;
        }

        private bool GlobalTypeNameConflicts(string clrNamespace, string typeName)
        {
            return (string.IsNullOrEmpty(clrNamespace) && this.clrNamespaces.ContainsKey(typeName));
        }

        private bool HasDefaultCollectionNames(CollectionDataContract collectionContract)
        {
            DataContract itemContract = collectionContract.ItemContract;
            if (collectionContract.ItemName != itemContract.StableName.Name)
            {
                return false;
            }
            if (collectionContract.IsDictionary && ((collectionContract.KeyName != "Key") || (collectionContract.ValueName != "Value")))
            {
                return false;
            }
            XmlQualifiedName arrayTypeName = itemContract.GetArrayTypeName(collectionContract.IsItemTypeNullable);
            return ((collectionContract.StableName.Name == arrayTypeName.Name) && (collectionContract.StableName.Namespace == arrayTypeName.Namespace));
        }

        private void InvokeProcessImportedType(CollectionBase collection)
        {
            object[] array = new object[collection.Count];
            ((ICollection) collection).CopyTo(array, 0);
            foreach (object obj2 in array)
            {
                CodeTypeDeclaration typeDeclaration = obj2 as CodeTypeDeclaration;
                if (typeDeclaration != null)
                {
                    CodeTypeDeclaration declaration2 = DataContractSurrogateCaller.ProcessImportedType(this.dataContractSet.DataContractSurrogate, typeDeclaration, this.codeCompileUnit);
                    if (declaration2 != typeDeclaration)
                    {
                        ((IList) collection).Remove(typeDeclaration);
                        if (declaration2 != null)
                        {
                            ((IList) collection).Add(declaration2);
                        }
                    }
                    if (declaration2 != null)
                    {
                        this.InvokeProcessImportedType(declaration2.Members);
                    }
                }
            }
        }

        private static bool IsValid(char c)
        {
            switch (char.GetUnicodeCategory(c))
            {
                case UnicodeCategory.UppercaseLetter:
                case UnicodeCategory.LowercaseLetter:
                case UnicodeCategory.TitlecaseLetter:
                case UnicodeCategory.ModifierLetter:
                case UnicodeCategory.OtherLetter:
                case UnicodeCategory.NonSpacingMark:
                case UnicodeCategory.SpacingCombiningMark:
                case UnicodeCategory.DecimalDigitNumber:
                case UnicodeCategory.ConnectorPunctuation:
                    return true;
            }
            return false;
        }

        private static bool IsValidStart(char c)
        {
            return (char.GetUnicodeCategory(c) != UnicodeCategory.DecimalDigitNumber);
        }

        private bool NamespaceContainsType(CodeNamespace ns, string typeName)
        {
            foreach (CodeTypeDeclaration declaration in ns.Types)
            {
                if (string.Compare(typeName, declaration.Name, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return true;
                }
            }
            return false;
        }

        private bool NeedsExplicitNamespace(string dataContractNamespace, string clrNamespace)
        {
            return (DataContract.GetDefaultStableNamespace(clrNamespace) != dataContractNamespace);
        }

        private void ThrowIfReferencedBaseTypeSealed(Type baseType, DataContract dataContract)
        {
            if (baseType.IsSealed)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.Runtime.Serialization.SR.GetString("CannotDeriveFromSealedReferenceType", new object[] { dataContract.StableName.Name, dataContract.StableName.Namespace, DataContract.GetClrTypeFullName(baseType) })));
            }
        }

        private bool TryGetReferencedDictionaryType(CollectionDataContract collectionContract, out CodeTypeReference typeReference)
        {
            if (collectionContract.IsDictionary && this.SupportsGenericTypeReference)
            {
                Type typeOfDictionaryGeneric;
                if (!this.TryGetReferencedType(this.GenericDictionaryName, this.GenericDictionaryContract, out typeOfDictionaryGeneric))
                {
                    typeOfDictionaryGeneric = Globals.TypeOfDictionaryGeneric;
                }
                ClassDataContract itemContract = collectionContract.ItemContract as ClassDataContract;
                DataMember member = itemContract.Members[0];
                DataMember member2 = itemContract.Members[1];
                CodeTypeReference elementTypeReference = this.GetElementTypeReference(member.MemberTypeContract, member.IsNullable);
                CodeTypeReference reference2 = this.GetElementTypeReference(member2.MemberTypeContract, member2.IsNullable);
                if ((elementTypeReference != null) && (reference2 != null))
                {
                    typeReference = this.GetCodeTypeReference(typeOfDictionaryGeneric);
                    typeReference.TypeArguments.Add(elementTypeReference);
                    typeReference.TypeArguments.Add(reference2);
                    return true;
                }
            }
            typeReference = null;
            return false;
        }

        private bool TryGetReferencedListType(DataContract itemContract, bool isItemTypeNullable, out CodeTypeReference typeReference)
        {
            Type type;
            if (this.SupportsGenericTypeReference && this.TryGetReferencedType(this.GenericListName, this.GenericListContract, out type))
            {
                typeReference = this.GetCodeTypeReference(type);
                typeReference.TypeArguments.Add(this.GetElementTypeReference(itemContract, isItemTypeNullable));
                return true;
            }
            typeReference = null;
            return false;
        }

        private bool TryGetReferencedType(XmlQualifiedName stableName, DataContract dataContract, out Type type)
        {
            if (dataContract == null)
            {
                if (!this.dataContractSet.TryGetReferencedCollectionType(stableName, dataContract, out type))
                {
                    if (!this.dataContractSet.TryGetReferencedType(stableName, dataContract, out type))
                    {
                        return false;
                    }
                    if (CollectionDataContract.IsCollection(type))
                    {
                        type = null;
                        return false;
                    }
                }
                return true;
            }
            if (dataContract is CollectionDataContract)
            {
                return this.dataContractSet.TryGetReferencedCollectionType(stableName, dataContract, out type);
            }
            XmlDataContract contract = dataContract as XmlDataContract;
            if ((contract != null) && contract.IsAnonymous)
            {
                stableName = SchemaImporter.ImportActualType(contract.XsdType.Annotation, stableName, dataContract.StableName);
            }
            return this.dataContractSet.TryGetReferencedType(stableName, dataContract, out type);
        }

        private bool TypeContainsNestedType(CodeTypeDeclaration containingType, string typeName)
        {
            foreach (CodeTypeMember member in containingType.Members)
            {
                if ((member is CodeTypeDeclaration) && (string.Compare(typeName, ((CodeTypeDeclaration) member).Name, StringComparison.OrdinalIgnoreCase) == 0))
                {
                    return true;
                }
            }
            return false;
        }

        private CodeTypeReference WrapNullable(CodeTypeReference memberType)
        {
            if (!this.SupportsGenericTypeReference)
            {
                return memberType;
            }
            CodeTypeReference codeTypeReference = this.GetCodeTypeReference(Globals.TypeOfNullable);
            codeTypeReference.TypeArguments.Add(memberType);
            return codeTypeReference;
        }

        private Dictionary<string, string> ClrNamespaces
        {
            get
            {
                return this.clrNamespaces;
            }
        }

        private CodeDomProvider CodeProvider
        {
            get
            {
                if (this.options != null)
                {
                    return this.options.CodeProvider;
                }
                return null;
            }
        }

        private CodeTypeReference CodeTypeIPropertyChange
        {
            get
            {
                return this.GetCodeTypeReference(typeof(INotifyPropertyChanged));
            }
        }

        private bool EnableDataBinding
        {
            get
            {
                return ((this.options != null) && this.options.EnableDataBinding);
            }
        }

        private CodeMemberField ExtensionDataObjectField
        {
            get
            {
                return new CodeMemberField { Type = this.GetCodeTypeReference(Globals.TypeOfExtensionDataObject), Name = "extensionDataField", Attributes = MemberAttributes.Private };
            }
        }

        private CodeMemberProperty ExtensionDataObjectProperty
        {
            get
            {
                CodeMemberProperty property = new CodeMemberProperty {
                    Type = this.GetCodeTypeReference(Globals.TypeOfExtensionDataObject),
                    Name = "ExtensionData",
                    Attributes = MemberAttributes.Public | MemberAttributes.Final
                };
                property.ImplementationTypes.Add(Globals.TypeOfIExtensibleDataObject);
                CodeMethodReturnStatement statement = new CodeMethodReturnStatement {
                    Expression = new CodeFieldReferenceExpression(this.ThisReference, "extensionDataField")
                };
                property.GetStatements.Add(statement);
                CodeAssignStatement statement2 = new CodeAssignStatement {
                    Left = new CodeFieldReferenceExpression(this.ThisReference, "extensionDataField"),
                    Right = new CodePropertySetValueReferenceExpression()
                };
                property.SetStatements.Add(statement2);
                return property;
            }
        }

        private string FileExtension
        {
            get
            {
                if (this.CodeProvider != null)
                {
                    return this.CodeProvider.FileExtension;
                }
                return string.Empty;
            }
        }

        private bool GenerateInternalTypes
        {
            get
            {
                return ((this.options != null) && this.options.GenerateInternal);
            }
        }

        private bool GenerateSerializableTypes
        {
            get
            {
                return ((this.options != null) && this.options.GenerateSerializable);
            }
        }

        private CollectionDataContract GenericDictionaryContract
        {
            get
            {
                return (this.dataContractSet.GetDataContract(Globals.TypeOfDictionaryGeneric) as CollectionDataContract);
            }
        }

        private XmlQualifiedName GenericDictionaryName
        {
            get
            {
                return DataContract.GetStableName(Globals.TypeOfDictionaryGeneric);
            }
        }

        private CollectionDataContract GenericListContract
        {
            get
            {
                return (this.dataContractSet.GetDataContract(Globals.TypeOfListGeneric) as CollectionDataContract);
            }
        }

        private XmlQualifiedName GenericListName
        {
            get
            {
                return DataContract.GetStableName(Globals.TypeOfListGeneric);
            }
        }

        private CodeMemberMethod GetObjectDataMethod
        {
            get
            {
                CodeIterationStatement statement5;
                CodeMemberMethod method = new CodeMemberMethod {
                    Name = "GetObjectData"
                };
                method.Parameters.Add(this.SerializationInfoParameter);
                method.Parameters.Add(this.StreamingContextParameter);
                method.Attributes = MemberAttributes.Public | MemberAttributes.Final;
                method.ImplementationTypes.Add(Globals.TypeOfISerializable);
                CodeConditionStatement statement = new CodeConditionStatement {
                    Condition = new CodeBinaryOperatorExpression(new CodePropertyReferenceExpression(this.ThisReference, "SerializationInfo"), CodeBinaryOperatorType.IdentityEquality, this.NullReference)
                };
                statement.TrueStatements.Add(new CodeMethodReturnStatement());
                CodeVariableDeclarationStatement statement2 = new CodeVariableDeclarationStatement {
                    Type = this.GetCodeTypeReference(Globals.TypeOfSerializationInfoEnumerator),
                    Name = "enumerator",
                    InitExpression = new CodeMethodInvokeExpression(new CodePropertyReferenceExpression(this.ThisReference, "SerializationInfo"), "GetEnumerator", new CodeExpression[0])
                };
                CodeVariableDeclarationStatement statement3 = new CodeVariableDeclarationStatement {
                    Type = this.GetCodeTypeReference(Globals.TypeOfSerializationEntry),
                    Name = "entry",
                    InitExpression = new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("enumerator"), "Current")
                };
                CodeExpressionStatement statement4 = new CodeExpressionStatement();
                CodePropertyReferenceExpression expression = new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("entry"), "Name");
                CodePropertyReferenceExpression expression2 = new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("entry"), "Value");
                statement4.Expression = new CodeMethodInvokeExpression(new CodeArgumentReferenceExpression("info"), "AddValue", new CodeExpression[] { expression, expression2 });
                statement5 = new CodeIterationStatement {
                    TestExpression = new CodeMethodInvokeExpression(new CodeVariableReferenceExpression("enumerator"), "MoveNext", new CodeExpression[0]),
                    InitStatement = statement5.IncrementStatement = new CodeSnippetStatement(string.Empty)
                };
                statement5.Statements.Add(statement3);
                statement5.Statements.Add(statement4);
                method.Statements.Add(statement);
                method.Statements.Add(statement2);
                method.Statements.Add(statement5);
                return method;
            }
        }

        private CodeMemberMethod GetSchemaMethod
        {
            get
            {
                CodeMemberMethod method = new CodeMemberMethod {
                    Name = "GetSchema",
                    Attributes = MemberAttributes.Public | MemberAttributes.Final
                };
                method.ImplementationTypes.Add(Globals.TypeOfIXmlSerializable);
                method.ReturnType = this.GetCodeTypeReference(typeof(XmlSchema));
                method.Statements.Add(new CodeMethodReturnStatement(this.NullReference));
                return method;
            }
        }

        private CodeMemberMethod GetSchemaStaticMethod
        {
            get
            {
                CodeMemberMethod method = new CodeMemberMethod {
                    Name = "ExportSchema",
                    ReturnType = this.GetCodeTypeReference(Globals.TypeOfXmlQualifiedName)
                };
                CodeParameterDeclarationExpression expression = new CodeParameterDeclarationExpression(Globals.TypeOfXmlSchemaSet, "schemas");
                method.Parameters.Add(expression);
                method.Attributes = MemberAttributes.Public | MemberAttributes.Static;
                method.Statements.Add(new CodeMethodInvokeExpression(new CodeTypeReferenceExpression(this.GetCodeTypeReference(typeof(XmlSerializableServices))), XmlSerializableServices.AddDefaultSchemaMethodName, new CodeExpression[] { new CodeArgumentReferenceExpression(expression.Name), new CodeFieldReferenceExpression(null, typeNameFieldName) }));
                method.Statements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(null, typeNameFieldName)));
                return method;
            }
        }

        private CodeConstructor ISerializableBaseConstructor
        {
            get
            {
                CodeConstructor constructor = new CodeConstructor {
                    Attributes = MemberAttributes.Public
                };
                constructor.Parameters.Add(this.SerializationInfoParameter);
                constructor.Parameters.Add(this.StreamingContextParameter);
                CodeAssignStatement statement = new CodeAssignStatement {
                    Left = new CodePropertyReferenceExpression(this.ThisReference, "info"),
                    Right = new CodeArgumentReferenceExpression("info")
                };
                constructor.Statements.Add(statement);
                if ((this.EnableDataBinding && this.SupportsDeclareEvents) && (string.CompareOrdinal(this.FileExtension, "vb") != 0))
                {
                    constructor.Statements.Add(new CodeAssignStatement(new CodePropertyReferenceExpression(this.ThisReference, this.PropertyChangedEvent.Name), this.NullReference));
                }
                return constructor;
            }
        }

        private CodeConstructor ISerializableDerivedConstructor
        {
            get
            {
                CodeConstructor constructor = new CodeConstructor {
                    Attributes = MemberAttributes.Public
                };
                constructor.Parameters.Add(this.SerializationInfoParameter);
                constructor.Parameters.Add(this.StreamingContextParameter);
                constructor.BaseConstructorArgs.Add(new CodeVariableReferenceExpression("info"));
                constructor.BaseConstructorArgs.Add(new CodeVariableReferenceExpression("context"));
                return constructor;
            }
        }

        private Dictionary<string, string> Namespaces
        {
            get
            {
                return this.namespaces;
            }
        }

        private CodeMemberField NodeArrayField
        {
            get
            {
                return new CodeMemberField { Type = this.GetCodeTypeReference(Globals.TypeOfXmlNodeArray), Name = "nodesField", Attributes = MemberAttributes.Private };
            }
        }

        private CodeMemberProperty NodeArrayProperty
        {
            get
            {
                return this.CreateProperty(this.GetCodeTypeReference(Globals.TypeOfXmlNodeArray), "Nodes", "nodesField", false);
            }
        }

        private CodePrimitiveExpression NullReference
        {
            get
            {
                return new CodePrimitiveExpression(null);
            }
        }

        private CodeMemberEvent PropertyChangedEvent
        {
            get
            {
                CodeMemberEvent event2 = new CodeMemberEvent {
                    Attributes = MemberAttributes.Public,
                    Name = "PropertyChanged",
                    Type = this.GetCodeTypeReference(typeof(PropertyChangedEventHandler))
                };
                event2.ImplementationTypes.Add(Globals.TypeOfIPropertyChange);
                return event2;
            }
        }

        private CodeMemberMethod RaisePropertyChangedEventMethod
        {
            get
            {
                CodeMemberMethod method = new CodeMemberMethod {
                    Name = "RaisePropertyChanged",
                    Attributes = MemberAttributes.Final
                };
                CodeArgumentReferenceExpression expression = new CodeArgumentReferenceExpression("propertyName");
                method.Parameters.Add(new CodeParameterDeclarationExpression(typeof(string), expression.ParameterName));
                CodeVariableReferenceExpression left = new CodeVariableReferenceExpression("propertyChanged");
                method.Statements.Add(new CodeVariableDeclarationStatement(typeof(PropertyChangedEventHandler), left.VariableName, new CodeEventReferenceExpression(this.ThisReference, this.PropertyChangedEvent.Name)));
                CodeConditionStatement statement = new CodeConditionStatement(new CodeBinaryOperatorExpression(left, CodeBinaryOperatorType.IdentityInequality, this.NullReference), new CodeStatement[0]);
                method.Statements.Add(statement);
                statement.TrueStatements.Add(new CodeDelegateInvokeExpression(left, new CodeExpression[] { this.ThisReference, new CodeObjectCreateExpression(typeof(PropertyChangedEventArgs), new CodeExpression[] { expression }) }));
                return method;
            }
        }

        private CodeMemberMethod ReadXmlMethod
        {
            get
            {
                CodeMemberMethod method = new CodeMemberMethod {
                    Name = "ReadXml"
                };
                CodeParameterDeclarationExpression expression = new CodeParameterDeclarationExpression(typeof(XmlReader), "reader");
                method.Parameters.Add(expression);
                method.Attributes = MemberAttributes.Public | MemberAttributes.Final;
                method.ImplementationTypes.Add(Globals.TypeOfIXmlSerializable);
                CodeAssignStatement statement = new CodeAssignStatement {
                    Left = new CodeFieldReferenceExpression(this.ThisReference, "nodesField"),
                    Right = new CodeMethodInvokeExpression(new CodeTypeReferenceExpression(this.GetCodeTypeReference(Globals.TypeOfXmlSerializableServices)), XmlSerializableServices.ReadNodesMethodName, new CodeExpression[] { new CodeArgumentReferenceExpression(expression.Name) })
                };
                method.Statements.Add(statement);
                return method;
            }
        }

        private CodeAttributeDeclaration SerializableAttribute
        {
            get
            {
                return new CodeAttributeDeclaration(this.GetCodeTypeReference(Globals.TypeOfSerializableAttribute));
            }
        }

        private CodeMemberField SerializationInfoField
        {
            get
            {
                return new CodeMemberField { Type = this.GetCodeTypeReference(Globals.TypeOfSerializationInfo), Name = "info", Attributes = MemberAttributes.Private };
            }
        }

        private CodeParameterDeclarationExpression SerializationInfoParameter
        {
            get
            {
                return new CodeParameterDeclarationExpression(this.GetCodeTypeReference(Globals.TypeOfSerializationInfo), "info");
            }
        }

        private CodeMemberProperty SerializationInfoProperty
        {
            get
            {
                return this.CreateProperty(this.GetCodeTypeReference(Globals.TypeOfSerializationInfo), "SerializationInfo", "info", false);
            }
        }

        private CodeParameterDeclarationExpression StreamingContextParameter
        {
            get
            {
                return new CodeParameterDeclarationExpression(this.GetCodeTypeReference(Globals.TypeOfStreamingContext), "context");
            }
        }

        private bool SupportsAssemblyAttributes
        {
            get
            {
                if (this.CodeProvider != null)
                {
                    return this.CodeProvider.Supports(GeneratorSupport.AssemblyAttributes);
                }
                return true;
            }
        }

        private bool SupportsDeclareEvents
        {
            get
            {
                if (this.CodeProvider != null)
                {
                    return this.CodeProvider.Supports(GeneratorSupport.DeclareEvents);
                }
                return true;
            }
        }

        private bool SupportsDeclareValueTypes
        {
            get
            {
                if (this.CodeProvider != null)
                {
                    return this.CodeProvider.Supports(GeneratorSupport.DeclareValueTypes);
                }
                return true;
            }
        }

        private bool SupportsGenericTypeReference
        {
            get
            {
                if (this.CodeProvider != null)
                {
                    return this.CodeProvider.Supports(GeneratorSupport.GenericTypeReference);
                }
                return true;
            }
        }

        private bool SupportsNestedTypes
        {
            get
            {
                if (this.CodeProvider != null)
                {
                    return this.CodeProvider.Supports(GeneratorSupport.NestedTypes);
                }
                return true;
            }
        }

        private bool SupportsPartialTypes
        {
            get
            {
                if (this.CodeProvider != null)
                {
                    return this.CodeProvider.Supports(GeneratorSupport.PartialTypes);
                }
                return true;
            }
        }

        private CodeThisReferenceExpression ThisReference
        {
            get
            {
                return new CodeThisReferenceExpression();
            }
        }

        private CodeMemberMethod WriteXmlMethod
        {
            get
            {
                CodeMemberMethod method = new CodeMemberMethod {
                    Name = "WriteXml"
                };
                CodeParameterDeclarationExpression expression = new CodeParameterDeclarationExpression(typeof(XmlWriter), "writer");
                method.Parameters.Add(expression);
                method.Attributes = MemberAttributes.Public | MemberAttributes.Final;
                method.ImplementationTypes.Add(Globals.TypeOfIXmlSerializable);
                method.Statements.Add(new CodeMethodInvokeExpression(new CodeTypeReferenceExpression(this.GetCodeTypeReference(Globals.TypeOfXmlSerializableServices)), XmlSerializableServices.WriteNodesMethodName, new CodeExpression[] { new CodeArgumentReferenceExpression(expression.Name), new CodePropertyReferenceExpression(this.ThisReference, "Nodes") }));
                return method;
            }
        }
    }
}

