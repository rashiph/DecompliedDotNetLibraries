namespace System.ServiceModel.Description
{
    using System;
    using System.CodeDom;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Net.Security;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Security;

    internal class OperationGenerator
    {
        private Dictionary<MessagePartDescription, CodeAttributeDeclarationCollection> parameterAttributes;
        private Dictionary<MessagePartDescription, CodeTypeReference> parameterTypes;
        private Dictionary<MessagePartDescription, string> specialPartName;

        internal OperationGenerator()
        {
        }

        internal static CodeAttributeDeclaration GenerateAttributeDeclaration(ServiceContractGenerator generator, Attribute attribute)
        {
            return CustomAttributeHelper.GenerateAttributeDeclaration(generator, attribute);
        }

        internal void GenerateOperation(OperationContractGenerationContext context, ref OperationFormatStyle style, bool isEncoded, IWrappedBodyTypeGenerator wrappedBodyTypeGenerator, Dictionary<MessagePartDescription, ICollection<CodeTypeReference>> knownTypes)
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("context"));
            }
            if (context.Operation == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("OperationPropertyIsRequiredForAttributeGeneration")));
            }
            MethodSignatureGenerator generator = new MethodSignatureGenerator(this, context, style, isEncoded, wrappedBodyTypeGenerator, knownTypes);
            if (context.IsAsync)
            {
                generator.GenerateSyncSignature(ref style);
                generator.GenerateAsyncSignature(ref style);
            }
            else
            {
                generator.GenerateSyncSignature(ref style);
            }
        }

        internal Dictionary<MessagePartDescription, CodeAttributeDeclarationCollection> ParameterAttributes
        {
            get
            {
                if (this.parameterAttributes == null)
                {
                    this.parameterAttributes = new Dictionary<MessagePartDescription, CodeAttributeDeclarationCollection>();
                }
                return this.parameterAttributes;
            }
        }

        internal Dictionary<MessagePartDescription, CodeTypeReference> ParameterTypes
        {
            get
            {
                if (this.parameterTypes == null)
                {
                    this.parameterTypes = new Dictionary<MessagePartDescription, CodeTypeReference>();
                }
                return this.parameterTypes;
            }
        }

        internal Dictionary<MessagePartDescription, string> SpecialPartName
        {
            get
            {
                if (this.specialPartName == null)
                {
                    this.specialPartName = new Dictionary<MessagePartDescription, string>();
                }
                return this.specialPartName;
            }
        }

        private static class CustomAttributeHelper
        {
            internal static void CreateOrOverridePropertyDeclaration<V>(CodeAttributeDeclaration attribute, string propertyName, V value)
            {
                SecurityAttributeGenerationHelper.CreateOrOverridePropertyDeclaration<V>(attribute, propertyName, value);
            }

            internal static CodeAttributeDeclaration FindOrCreateAttributeDeclaration<T>(CodeAttributeDeclarationCollection attributes) where T: Attribute
            {
                return SecurityAttributeGenerationHelper.FindOrCreateAttributeDeclaration<T>(attributes);
            }

            internal static CodeAttributeDeclaration GenerateAttributeDeclaration(ServiceContractGenerator generator, Attribute attribute)
            {
                System.Type type = attribute.GetType();
                Attribute attribute2 = (Attribute) Activator.CreateInstance(type);
                MemberInfo[] members = type.GetMembers(BindingFlags.Public | BindingFlags.Instance);
                Array.Sort<MemberInfo>(members, (Comparison<MemberInfo>) ((a, b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal)));
                CodeAttributeDeclaration declaration = new CodeAttributeDeclaration(generator.GetCodeTypeReference(type));
                foreach (MemberInfo info in members)
                {
                    if (info.DeclaringType != typeof(Attribute))
                    {
                        FieldInfo info2 = info as FieldInfo;
                        if (info2 != null)
                        {
                            object objA = info2.GetValue(attribute);
                            object objB = info2.GetValue(attribute2);
                            if (!object.Equals(objA, objB))
                            {
                                declaration.Arguments.Add(new CodeAttributeArgument(info2.Name, GetArgValue(objA)));
                            }
                        }
                        else
                        {
                            PropertyInfo info3 = info as PropertyInfo;
                            if (info3 != null)
                            {
                                object obj4 = info3.GetValue(attribute, null);
                                object obj5 = info3.GetValue(attribute2, null);
                                if (!object.Equals(obj4, obj5))
                                {
                                    declaration.Arguments.Add(new CodeAttributeArgument(info3.Name, GetArgValue(obj4)));
                                }
                            }
                        }
                    }
                }
                return declaration;
            }

            private static CodeExpression GetArgValue(object val)
            {
                System.Type type = val.GetType();
                if (type.IsPrimitive || (type == typeof(string)))
                {
                    return new CodePrimitiveExpression(val);
                }
                if (type.IsEnum)
                {
                    return new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(type), Enum.Format(type, val, "G"));
                }
                return null;
            }
        }

        private class MethodSignatureGenerator
        {
            private IPartCodeGenerator BeginPartCodeGenerator;
            private readonly OperationContractGenerationContext Context;
            private readonly string ContractName;
            private readonly string ContractNS;
            private string DefaultName;
            private readonly string DefaultNS;
            private CodeMemberMethod EndMethod;
            private IPartCodeGenerator EndPartCodeGenerator;
            private readonly bool IsEncoded;
            private bool IsNewRequest;
            private bool IsNewResponse;
            private readonly Dictionary<MessagePartDescription, ICollection<CodeTypeReference>> KnownTypes;
            private System.ServiceModel.Description.MessageContractType MessageContractType;
            private CodeMemberMethod Method;
            private readonly bool Oneway;
            private readonly OperationGenerator Parent;
            private readonly MessageDescription Request;
            private readonly MessageDescription Response;
            private readonly OperationFormatStyle Style;
            private readonly IWrappedBodyTypeGenerator WrappedBodyTypeGenerator;

            internal MethodSignatureGenerator(OperationGenerator parent, OperationContractGenerationContext context, OperationFormatStyle style, bool isEncoded, IWrappedBodyTypeGenerator wrappedBodyTypeGenerator, Dictionary<MessagePartDescription, ICollection<CodeTypeReference>> knownTypes)
            {
                this.Parent = parent;
                this.Context = context;
                this.Style = style;
                this.IsEncoded = isEncoded;
                this.WrappedBodyTypeGenerator = wrappedBodyTypeGenerator;
                this.KnownTypes = knownTypes;
                this.MessageContractType = context.ServiceContractGenerator.OptionsInternal.IsSet(ServiceContractGenerationOptions.TypedMessages) ? System.ServiceModel.Description.MessageContractType.WrappedMessageContract : System.ServiceModel.Description.MessageContractType.None;
                this.ContractName = context.Contract.Contract.CodeName;
                this.ContractNS = context.Operation.DeclaringContract.Namespace;
                this.DefaultNS = (style == OperationFormatStyle.Rpc) ? string.Empty : this.ContractNS;
                this.Oneway = context.Operation.IsOneWay;
                this.Request = context.Operation.Messages[0];
                this.Response = this.Oneway ? null : context.Operation.Messages[1];
                this.IsNewRequest = true;
                this.IsNewResponse = true;
                this.BeginPartCodeGenerator = null;
                this.EndPartCodeGenerator = null;
            }

            private void AddAdditionalAttributes(MessagePartDescription setting, CodeAttributeDeclarationCollection attributes, bool isAdditionalAttributesAllowed)
            {
                if ((this.Parent.parameterAttributes != null) && this.Parent.parameterAttributes.ContainsKey(setting))
                {
                    CodeAttributeDeclarationCollection declarations = this.Parent.parameterAttributes[setting];
                    if ((declarations != null) && (declarations.Count > 0))
                    {
                        if (!isAdditionalAttributesAllowed)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ParameterModeException(System.ServiceModel.SR.GetString("SfxUseTypedMessageForCustomAttributes", new object[] { setting.Name, declarations[0].AttributeType.BaseType })));
                        }
                        attributes.AddRange(declarations);
                    }
                }
            }

            private void AddWrapperPart(XmlName messageName, IWrappedBodyTypeGenerator wrappedBodyTypeGenerator, IPartCodeGenerator partGenerator, MessagePartDescription part, CodeAttributeDeclarationCollection typeAttributes)
            {
                CodeTypeReference codeTypeReference;
                string codeName = part.CodeName;
                if (part.Type == typeof(Stream))
                {
                    codeTypeReference = this.Context.ServiceContractGenerator.GetCodeTypeReference(typeof(byte[]));
                }
                else
                {
                    codeTypeReference = this.GetParameterType(part);
                }
                CodeAttributeDeclarationCollection fieldAttributes = partGenerator.AddPart(codeTypeReference, ref codeName);
                CodeAttributeDeclarationCollection declarations2 = null;
                bool flag = this.Parent.ParameterAttributes.TryGetValue(part, out declarations2);
                wrappedBodyTypeGenerator.AddMemberAttributes(messageName, part, declarations2, typeAttributes, fieldAttributes);
                this.Parent.ParameterTypes.Remove(part);
                if (flag)
                {
                    this.Parent.ParameterAttributes.Remove(part);
                }
            }

            private void CheckAndSetMessageContractTypeToBare()
            {
                if (this.MessageContractType != System.ServiceModel.Description.MessageContractType.BareMessageContract)
                {
                    try
                    {
                        this.WrappedBodyTypeGenerator.ValidateForParameterMode(this.Context.Operation);
                    }
                    catch (ParameterModeException exception)
                    {
                        this.MessageContractType = exception.MessageContractType;
                    }
                }
            }

            private void CreateOrOverrideActionProperties()
            {
                if (this.Request != null)
                {
                    OperationGenerator.CustomAttributeHelper.CreateOrOverridePropertyDeclaration<string>(OperationGenerator.CustomAttributeHelper.FindOrCreateAttributeDeclaration<OperationContractAttribute>(this.Method.CustomAttributes), "Action", this.Request.Action);
                }
                if (this.Response != null)
                {
                    OperationGenerator.CustomAttributeHelper.CreateOrOverridePropertyDeclaration<string>(OperationGenerator.CustomAttributeHelper.FindOrCreateAttributeDeclaration<OperationContractAttribute>(this.Method.CustomAttributes), "ReplyAction", this.Response.Action);
                }
            }

            private void CreateUntypedMessages()
            {
                bool flag = (this.Request != null) && this.Request.IsUntypedMessage;
                bool flag2 = (this.Response != null) && this.Response.IsUntypedMessage;
                if (flag)
                {
                    this.Method.Parameters.Insert(0, new CodeParameterDeclarationExpression(this.Context.ServiceContractGenerator.GetCodeTypeReference(typeof(Message)), "request"));
                }
                if (flag2)
                {
                    this.EndMethod.ReturnType = this.Context.ServiceContractGenerator.GetCodeTypeReference(typeof(Message));
                }
            }

            internal void GenerateAsyncSignature(ref OperationFormatStyle style)
            {
                this.Method = this.Context.BeginMethod;
                this.EndMethod = this.Context.EndMethod;
                this.DefaultName = this.Method.Name.Substring(5);
                this.GenerateOperationSignatures(ref style);
            }

            private void GenerateBodyPart(int order, MessagePartDescription messagePart, IPartCodeGenerator partCodeGenerator, bool generateTypedMessage, bool isEncoded, string defaultNS)
            {
                string codeName;
                if (!generateTypedMessage)
                {
                    order = -1;
                }
                if (!this.Parent.SpecialPartName.TryGetValue(messagePart, out codeName))
                {
                    codeName = messagePart.CodeName;
                }
                CodeTypeReference parameterType = this.GetParameterType(messagePart);
                CodeAttributeDeclarationCollection attributes = partCodeGenerator.AddPart(parameterType, ref codeName);
                if (attributes != null)
                {
                    XmlName defaultName = new XmlName(codeName);
                    if (generateTypedMessage)
                    {
                        TypedMessageHelper.GenerateMessageBodyMemberAttribute(order, messagePart, attributes, defaultName);
                    }
                    else
                    {
                        ParameterizedMessageHelper.GenerateMessageParameterAttribute(messagePart, attributes, defaultName, defaultNS);
                    }
                    this.AddAdditionalAttributes(messagePart, attributes, generateTypedMessage || isEncoded);
                }
            }

            private void GenerateHeaderPart(MessageHeaderDescription setting, IPartCodeGenerator parts)
            {
                string codeName;
                if (!this.Parent.SpecialPartName.TryGetValue(setting, out codeName))
                {
                    codeName = setting.CodeName;
                }
                CodeTypeReference parameterType = this.GetParameterType(setting);
                CodeAttributeDeclarationCollection attributes = parts.AddPart(parameterType, ref codeName);
                TypedMessageHelper.GenerateMessageHeaderAttribute(setting, attributes, new XmlName(codeName));
                this.AddAdditionalAttributes(setting, attributes, true);
            }

            private void GenerateMessageBodyParts(bool generateTypedMessages)
            {
                int num = 0;
                if (this.IsNewRequest)
                {
                    foreach (MessagePartDescription description in this.Request.Body.Parts)
                    {
                        this.GenerateBodyPart(num++, description, this.BeginPartCodeGenerator, generateTypedMessages, this.IsEncoded, this.DefaultNS);
                    }
                }
                if (!this.Oneway && this.IsNewResponse)
                {
                    num = (this.Response.Body.ReturnValue != null) ? 1 : 0;
                    foreach (MessagePartDescription description2 in this.Response.Body.Parts)
                    {
                        this.GenerateBodyPart(num++, description2, this.EndPartCodeGenerator, generateTypedMessages, this.IsEncoded, this.DefaultNS);
                    }
                }
                if (this.IsNewRequest && (this.BeginPartCodeGenerator != null))
                {
                    this.BeginPartCodeGenerator.EndCodeGeneration();
                }
                if (this.IsNewResponse && (this.EndPartCodeGenerator != null))
                {
                    this.EndPartCodeGenerator.EndCodeGeneration();
                }
            }

            private void GenerateOperationSignatures(ref OperationFormatStyle style)
            {
                if (this.MessageContractType != System.ServiceModel.Description.MessageContractType.None)
                {
                    this.CheckAndSetMessageContractTypeToBare();
                    this.GenerateTypedMessageOperation(false, ref style);
                }
                else if (!this.TryGenerateParameterizedOperation())
                {
                    this.GenerateTypedMessageOperation(true, ref style);
                }
            }

            private void GenerateParameterizedOperation()
            {
                ParameterizedMessageHelper.ValidateProtectionLevel(this);
                this.CreateOrOverrideActionProperties();
                if (this.HasUntypedMessages)
                {
                    if (!this.IsCompletelyUntyped)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ParameterModeException(System.ServiceModel.SR.GetString("SFxCannotImportAsParameters_Message", new object[] { this.Context.Operation.CodeName })));
                    }
                    this.CreateUntypedMessages();
                }
                else
                {
                    ParameterizedMessageHelper.ValidateWrapperSettings(this);
                    ParameterizedMessageHelper.ValidateNoHeaders(this);
                    this.WrappedBodyTypeGenerator.ValidateForParameterMode(this.Context.Operation);
                    ParameterizedMethodGenerator generator = new ParameterizedMethodGenerator(this.Method, this.EndMethod);
                    this.BeginPartCodeGenerator = generator.InputGenerator;
                    this.EndPartCodeGenerator = generator.OutputGenerator;
                    if (!this.Oneway && (this.Response.Body.ReturnValue != null))
                    {
                        this.EndMethod.ReturnType = this.GetParameterType(this.Response.Body.ReturnValue);
                        ParameterizedMessageHelper.GenerateMessageParameterAttribute(this.Response.Body.ReturnValue, this.EndMethod.ReturnTypeCustomAttributes, TypeLoader.GetReturnValueName(this.DefaultName), this.DefaultNS);
                        this.AddAdditionalAttributes(this.Response.Body.ReturnValue, this.EndMethod.ReturnTypeCustomAttributes, this.IsEncoded);
                    }
                    this.GenerateMessageBodyParts(false);
                }
            }

            internal void GenerateSyncSignature(ref OperationFormatStyle style)
            {
                this.Method = this.Context.SyncMethod;
                this.EndMethod = this.Context.SyncMethod;
                this.DefaultName = this.Method.Name;
                this.GenerateOperationSignatures(ref style);
            }

            private CodeTypeReference GenerateTypedMessageHeaderAndReturnValueParts(CodeNamespace ns, string defaultName, MessageDescription message, bool isReply, bool hideFromEditor, ref bool isNewMessage, out IPartCodeGenerator partCodeGenerator)
            {
                CodeTypeReference reference;
                if (TypedMessageHelper.FindGeneratedTypedMessage(this.Context.Contract, message, out reference))
                {
                    partCodeGenerator = null;
                    isNewMessage = false;
                    return reference;
                }
                UniqueCodeNamespaceScope scope = new UniqueCodeNamespaceScope(ns);
                CodeTypeDeclaration codeType = this.Context.Contract.TypeFactory.CreateClassType();
                string name = XmlName.IsNullOrEmpty(message.MessageName) ? null : message.MessageName.DecodedName;
                reference = scope.AddUnique(codeType, name, defaultName);
                TypedMessageHelper.AddGeneratedTypedMessage(this.Context.Contract, message, reference);
                if ((this.MessageContractType == System.ServiceModel.Description.MessageContractType.BareMessageContract) && (message.Body.WrapperName != null))
                {
                    this.WrapTypedMessage(ns, codeType.Name, message, isReply, this.Context.IsInherited, hideFromEditor);
                }
                partCodeGenerator = new TypedMessagePartCodeGenerator(codeType);
                if (hideFromEditor)
                {
                    TypedMessageHelper.AddEditorBrowsableAttribute(codeType.CustomAttributes);
                }
                TypedMessageHelper.GenerateWrapperAttribute(message, partCodeGenerator);
                TypedMessageHelper.GenerateProtectionLevelAttribute(message, partCodeGenerator);
                foreach (MessageHeaderDescription description in message.Headers)
                {
                    this.GenerateHeaderPart(description, partCodeGenerator);
                }
                if (isReply && (message.Body.ReturnValue != null))
                {
                    this.GenerateBodyPart(0, message.Body.ReturnValue, partCodeGenerator, true, this.IsEncoded, this.DefaultNS);
                }
                return reference;
            }

            private void GenerateTypedMessageOperation(bool hideFromEditor, ref OperationFormatStyle style)
            {
                this.CreateOrOverrideActionProperties();
                if (this.HasUntypedMessages)
                {
                    this.CreateUntypedMessages();
                    if (this.IsCompletelyUntyped)
                    {
                        return;
                    }
                }
                CodeNamespace ns = this.Context.ServiceContractGenerator.NamespaceManager.EnsureNamespace(this.ContractNS);
                if (!this.Request.IsUntypedMessage)
                {
                    CodeTypeReference type = this.GenerateTypedMessageHeaderAndReturnValueParts(ns, this.DefaultName + "Request", this.Request, false, hideFromEditor, ref this.IsNewRequest, out this.BeginPartCodeGenerator);
                    this.Method.Parameters.Insert(0, new CodeParameterDeclarationExpression(type, "request"));
                }
                if (!this.Oneway && !this.Response.IsUntypedMessage)
                {
                    CodeTypeReference reference2 = this.GenerateTypedMessageHeaderAndReturnValueParts(ns, this.DefaultName + "Response", this.Response, true, hideFromEditor, ref this.IsNewResponse, out this.EndPartCodeGenerator);
                    this.EndMethod.ReturnType = reference2;
                }
                this.GenerateMessageBodyParts(true);
                if (!this.IsEncoded)
                {
                    style = OperationFormatStyle.Document;
                }
            }

            private CodeTypeReference GetParameterType(MessagePartDescription setting)
            {
                if (setting.Type != null)
                {
                    return this.Context.ServiceContractGenerator.GetCodeTypeReference(setting.Type);
                }
                if (!this.Parent.parameterTypes.ContainsKey(setting))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SfxNoTypeSpecifiedForParameter", new object[] { setting.Name })));
                }
                return this.Parent.parameterTypes[setting];
            }

            private string GetWrapperNamespace(MessageDescription messageDescription)
            {
                string defaultNS = this.DefaultNS;
                if (messageDescription.Body.ReturnValue != null)
                {
                    return messageDescription.Body.ReturnValue.Namespace;
                }
                if (messageDescription.Body.Parts.Count > 0)
                {
                    defaultNS = messageDescription.Body.Parts[0].Namespace;
                }
                return defaultNS;
            }

            private bool IsEmpty(MessageDescription message)
            {
                return ((message.Body.Parts.Count == 0) && (message.Headers.Count == 0));
            }

            private bool TryGenerateParameterizedOperation()
            {
                CodeParameterDeclarationExpressionCollection expressions2 = null;
                CodeParameterDeclarationExpressionCollection expressions = new CodeParameterDeclarationExpressionCollection(this.Method.Parameters);
                if (this.EndMethod != null)
                {
                    expressions2 = new CodeParameterDeclarationExpressionCollection(this.EndMethod.Parameters);
                }
                try
                {
                    this.GenerateParameterizedOperation();
                }
                catch (ParameterModeException exception)
                {
                    this.MessageContractType = exception.MessageContractType;
                    CodeMemberMethod method = this.Method;
                    method.Comments.Add(new CodeCommentStatement(System.ServiceModel.SR.GetString("SFxCodeGenWarning", new object[] { exception.Message })));
                    method.Parameters.Clear();
                    method.Parameters.AddRange(expressions);
                    if (this.Context.IsAsync)
                    {
                        CodeMemberMethod endMethod = this.EndMethod;
                        endMethod.Parameters.Clear();
                        endMethod.Parameters.AddRange(expressions2);
                    }
                    return false;
                }
                return true;
            }

            private void WrapTypedMessage(CodeNamespace ns, string typeName, MessageDescription messageDescription, bool isReply, bool isInherited, bool hideFromEditor)
            {
                UniqueCodeNamespaceScope scope = new UniqueCodeNamespaceScope(ns);
                CodeTypeDeclaration codeType = this.Context.Contract.TypeFactory.CreateClassType();
                CodeTypeReference reference = scope.AddUnique(codeType, typeName + "Body", "Body");
                if (hideFromEditor)
                {
                    TypedMessageHelper.AddEditorBrowsableAttribute(codeType.CustomAttributes);
                }
                string wrapperNamespace = this.GetWrapperNamespace(messageDescription);
                string messageName = XmlName.IsNullOrEmpty(messageDescription.MessageName) ? null : messageDescription.MessageName.DecodedName;
                this.WrappedBodyTypeGenerator.AddTypeAttributes(messageName, wrapperNamespace, codeType.CustomAttributes, this.IsEncoded);
                IPartCodeGenerator partGenerator = new TypedMessagePartCodeGenerator(codeType);
                ProtectionLevel none = ProtectionLevel.None;
                bool flag = false;
                if (messageDescription.Body.ReturnValue != null)
                {
                    this.AddWrapperPart(messageDescription.MessageName, this.WrappedBodyTypeGenerator, partGenerator, messageDescription.Body.ReturnValue, codeType.CustomAttributes);
                    none = ProtectionLevelHelper.Max(none, messageDescription.Body.ReturnValue.ProtectionLevel);
                    if (messageDescription.Body.ReturnValue.HasProtectionLevel)
                    {
                        flag = true;
                    }
                }
                List<CodeTypeReference> list = new List<CodeTypeReference>();
                foreach (MessagePartDescription description in messageDescription.Body.Parts)
                {
                    this.AddWrapperPart(messageDescription.MessageName, this.WrappedBodyTypeGenerator, partGenerator, description, codeType.CustomAttributes);
                    none = ProtectionLevelHelper.Max(none, description.ProtectionLevel);
                    if (description.HasProtectionLevel)
                    {
                        flag = true;
                    }
                    ICollection<CodeTypeReference> is2 = null;
                    if ((this.KnownTypes != null) && this.KnownTypes.TryGetValue(description, out is2))
                    {
                        foreach (CodeTypeReference reference2 in is2)
                        {
                            list.Add(reference2);
                        }
                    }
                }
                messageDescription.Body.Parts.Clear();
                MessagePartDescription key = new MessagePartDescription(messageDescription.Body.WrapperName, messageDescription.Body.WrapperNamespace);
                if (this.KnownTypes != null)
                {
                    this.KnownTypes.Add(key, list);
                }
                if (flag)
                {
                    key.ProtectionLevel = none;
                }
                messageDescription.Body.WrapperName = null;
                messageDescription.Body.WrapperNamespace = null;
                if (isReply)
                {
                    messageDescription.Body.ReturnValue = key;
                }
                else
                {
                    messageDescription.Body.Parts.Add(key);
                }
                TypedMessageHelper.GenerateConstructors(codeType);
                this.Parent.ParameterTypes.Add(key, reference);
                this.Parent.SpecialPartName.Add(key, "Body");
            }

            private bool HasUntypedMessages
            {
                get
                {
                    bool flag = (this.Request != null) && this.Request.IsUntypedMessage;
                    bool flag2 = (this.Response != null) && this.Response.IsUntypedMessage;
                    if (!flag)
                    {
                        return flag2;
                    }
                    return true;
                }
            }

            private bool IsCompletelyUntyped
            {
                get
                {
                    bool flag = (this.Request != null) && this.Request.IsUntypedMessage;
                    bool flag2 = (this.Response != null) && this.Response.IsUntypedMessage;
                    if (!flag || !flag2)
                    {
                        if ((flag2 && (this.Request == null)) || this.IsEmpty(this.Request))
                        {
                            return true;
                        }
                        if ((!flag || (this.Response != null)) && !this.IsEmpty(this.Response))
                        {
                            return false;
                        }
                    }
                    return true;
                }
            }

            private interface IPartCodeGenerator
            {
                CodeAttributeDeclarationCollection AddPart(CodeTypeReference type, ref string name);
                void EndCodeGeneration();

                CodeAttributeDeclarationCollection MessageLevelAttributes { get; }
            }

            private static class ParameterizedMessageHelper
            {
                internal static void GenerateMessageParameterAttribute(MessagePartDescription setting, CodeAttributeDeclarationCollection attributes, XmlName defaultName, string defaultNS)
                {
                    if (setting.Name != defaultName.EncodedName)
                    {
                        OperationGenerator.CustomAttributeHelper.CreateOrOverridePropertyDeclaration<string>(OperationGenerator.CustomAttributeHelper.FindOrCreateAttributeDeclaration<MessageParameterAttribute>(attributes), "Name", setting.Name);
                    }
                    if (setting.Namespace != defaultNS)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ParameterModeException(System.ServiceModel.SR.GetString("SFxCannotImportAsParameters_NamespaceMismatch", new object[] { setting.Namespace, defaultNS })));
                    }
                }

                private static bool StringEqualOrNull(string overrideValue, string defaultValue)
                {
                    if (overrideValue != null)
                    {
                        return string.Equals(overrideValue, defaultValue, StringComparison.Ordinal);
                    }
                    return true;
                }

                internal static void ValidateNoHeaders(OperationGenerator.MethodSignatureGenerator parent)
                {
                    if (parent.Request.Headers.Count > 0)
                    {
                        if (!parent.IsEncoded)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ParameterModeException(System.ServiceModel.SR.GetString("SFxCannotImportAsParameters_HeadersAreUnsupported", new object[] { parent.Request.MessageName })));
                        }
                        parent.Context.Contract.ServiceContractGenerator.Errors.Add(new MetadataConversionError(System.ServiceModel.SR.GetString("SFxCannotImportAsParameters_HeadersAreIgnoredInEncoded", new object[] { parent.Request.MessageName }), true));
                    }
                    if (!parent.Oneway && (parent.Response.Headers.Count > 0))
                    {
                        if (!parent.IsEncoded)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ParameterModeException(System.ServiceModel.SR.GetString("SFxCannotImportAsParameters_HeadersAreUnsupported", new object[] { parent.Response.MessageName })));
                        }
                        parent.Context.Contract.ServiceContractGenerator.Errors.Add(new MetadataConversionError(System.ServiceModel.SR.GetString("SFxCannotImportAsParameters_HeadersAreIgnoredInEncoded", new object[] { parent.Response.MessageName }), true));
                    }
                }

                internal static void ValidateProtectionLevel(OperationGenerator.MethodSignatureGenerator parent)
                {
                    if ((parent.Request != null) && parent.Request.HasProtectionLevel)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ParameterModeException(System.ServiceModel.SR.GetString("SFxCannotImportAsParameters_MessageHasProtectionLevel", new object[] { (parent.Request.Action == null) ? "" : parent.Request.Action })));
                    }
                    if ((parent.Response != null) && parent.Response.HasProtectionLevel)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ParameterModeException(System.ServiceModel.SR.GetString("SFxCannotImportAsParameters_MessageHasProtectionLevel", new object[] { (parent.Response.Action == null) ? "" : parent.Response.Action })));
                    }
                }

                internal static void ValidateWrapperSettings(OperationGenerator.MethodSignatureGenerator parent)
                {
                    if ((parent.Request.Body.WrapperName == null) || ((parent.Response != null) && (parent.Response.Body.WrapperName == null)))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ParameterModeException(System.ServiceModel.SR.GetString("SFxCannotImportAsParameters_Bare", new object[] { parent.Context.Operation.CodeName })));
                    }
                    if (!StringEqualOrNull(parent.Request.Body.WrapperNamespace, parent.ContractNS))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ParameterModeException(System.ServiceModel.SR.GetString("SFxCannotImportAsParameters_DifferentWrapperNs", new object[] { parent.Request.MessageName, parent.Request.Body.WrapperNamespace, parent.ContractNS })));
                    }
                    XmlName operationName = new XmlName(parent.DefaultName);
                    if (!string.Equals(parent.Request.Body.WrapperName, operationName.EncodedName, StringComparison.Ordinal))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ParameterModeException(System.ServiceModel.SR.GetString("SFxCannotImportAsParameters_DifferentWrapperName", new object[] { parent.Request.MessageName, parent.Request.Body.WrapperName, operationName.EncodedName })));
                    }
                    if (parent.Response != null)
                    {
                        if (!StringEqualOrNull(parent.Response.Body.WrapperNamespace, parent.ContractNS))
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ParameterModeException(System.ServiceModel.SR.GetString("SFxCannotImportAsParameters_DifferentWrapperNs", new object[] { parent.Response.MessageName, parent.Response.Body.WrapperNamespace, parent.ContractNS })));
                        }
                        if (!string.Equals(parent.Response.Body.WrapperName, TypeLoader.GetBodyWrapperResponseName(operationName).EncodedName, StringComparison.Ordinal))
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ParameterModeException(System.ServiceModel.SR.GetString("SFxCannotImportAsParameters_DifferentWrapperName", new object[] { parent.Response.MessageName, parent.Response.Body.WrapperName, operationName.EncodedName })));
                        }
                    }
                }
            }

            private class ParameterizedMethodGenerator
            {
                private ParametersPartCodeGenerator ins;
                private bool isSync;
                private ParametersPartCodeGenerator outs;

                internal ParameterizedMethodGenerator(CodeMemberMethod beginMethod, CodeMemberMethod endMethod)
                {
                    this.ins = new ParametersPartCodeGenerator(this, beginMethod.Name, beginMethod.Parameters, beginMethod.CustomAttributes, FieldDirection.In);
                    this.outs = new ParametersPartCodeGenerator(this, beginMethod.Name, endMethod.Parameters, beginMethod.CustomAttributes, FieldDirection.Out);
                    this.isSync = beginMethod == endMethod;
                }

                internal CodeParameterDeclarationExpression GetOrCreateParameter(CodeTypeReference type, string name, FieldDirection direction, ref int index, out bool createdNew)
                {
                    ParametersPartCodeGenerator generator = (direction != FieldDirection.In) ? this.ins : this.outs;
                    int num = index;
                    CodeParameterDeclarationExpression parameter = generator.GetParameter(name, ref num);
                    bool flag = (parameter != null) && (parameter.Type.BaseType == type.BaseType);
                    if (flag)
                    {
                        parameter.Direction = FieldDirection.Ref;
                        if (this.isSync)
                        {
                            index = num + 1;
                            createdNew = false;
                            return parameter;
                        }
                    }
                    CodeParameterDeclarationExpression expression2 = new CodeParameterDeclarationExpression {
                        Name = name,
                        Type = type,
                        Direction = direction
                    };
                    if (flag)
                    {
                        expression2.Direction = FieldDirection.Ref;
                    }
                    createdNew = true;
                    return expression2;
                }

                internal OperationGenerator.MethodSignatureGenerator.IPartCodeGenerator InputGenerator
                {
                    get
                    {
                        return this.ins;
                    }
                }

                internal OperationGenerator.MethodSignatureGenerator.IPartCodeGenerator OutputGenerator
                {
                    get
                    {
                        return this.outs;
                    }
                }

                private class ParametersPartCodeGenerator : OperationGenerator.MethodSignatureGenerator.IPartCodeGenerator
                {
                    private FieldDirection direction;
                    private int index;
                    private CodeAttributeDeclarationCollection messageAttrs;
                    private string methodName;
                    private CodeParameterDeclarationExpressionCollection parameters;
                    private OperationGenerator.MethodSignatureGenerator.ParameterizedMethodGenerator parent;

                    internal ParametersPartCodeGenerator(OperationGenerator.MethodSignatureGenerator.ParameterizedMethodGenerator parent, string methodName, CodeParameterDeclarationExpressionCollection parameters, CodeAttributeDeclarationCollection messageAttrs, FieldDirection direction)
                    {
                        this.parent = parent;
                        this.methodName = methodName;
                        this.parameters = parameters;
                        this.messageAttrs = messageAttrs;
                        this.direction = direction;
                        this.index = 0;
                    }

                    private static bool DoesParameterNameExist(string name, object parametersObject)
                    {
                        return ((OperationGenerator.MethodSignatureGenerator.ParameterizedMethodGenerator.ParametersPartCodeGenerator) parametersObject).NameExists(name);
                    }

                    internal CodeParameterDeclarationExpression GetParameter(string name, ref int index)
                    {
                        for (int i = index; i < this.parameters.Count; i++)
                        {
                            CodeParameterDeclarationExpression expression = this.parameters[i];
                            if (string.Compare(expression.Name, name, StringComparison.OrdinalIgnoreCase) == 0)
                            {
                                index = i;
                                return expression;
                            }
                        }
                        return null;
                    }

                    private static string GetUniqueParameterName(string name, OperationGenerator.MethodSignatureGenerator.ParameterizedMethodGenerator.ParametersPartCodeGenerator parameters)
                    {
                        return NamingHelper.GetUniqueName(name, new NamingHelper.DoesNameExist(OperationGenerator.MethodSignatureGenerator.ParameterizedMethodGenerator.ParametersPartCodeGenerator.DoesParameterNameExist), parameters);
                    }

                    public bool NameExists(string name)
                    {
                        if (string.Compare(name, this.methodName, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            return true;
                        }
                        int index = 0;
                        return (this.GetParameter(name, ref index) != null);
                    }

                    CodeAttributeDeclarationCollection OperationGenerator.MethodSignatureGenerator.IPartCodeGenerator.AddPart(CodeTypeReference type, ref string name)
                    {
                        bool flag;
                        name = UniqueCodeIdentifierScope.MakeValid(name, "param");
                        CodeParameterDeclarationExpression expression = this.parent.GetOrCreateParameter(type, name, this.direction, ref this.index, out flag);
                        if (flag)
                        {
                            expression.Name = GetUniqueParameterName(expression.Name, this);
                            this.parameters.Insert(this.index++, expression);
                        }
                        name = expression.Name;
                        if (!flag)
                        {
                            return null;
                        }
                        return expression.CustomAttributes;
                    }

                    void OperationGenerator.MethodSignatureGenerator.IPartCodeGenerator.EndCodeGeneration()
                    {
                    }

                    CodeAttributeDeclarationCollection OperationGenerator.MethodSignatureGenerator.IPartCodeGenerator.MessageLevelAttributes
                    {
                        get
                        {
                            return this.messageAttrs;
                        }
                    }
                }
            }

            private static class TypedMessageHelper
            {
                internal static void AddEditorBrowsableAttribute(CodeAttributeDeclarationCollection attributes)
                {
                    attributes.Add(ClientClassGenerator.CreateEditorBrowsableAttribute(EditorBrowsableState.Advanced));
                }

                internal static void AddGeneratedTypedMessage(ServiceContractGenerationContext contract, MessageDescription message, CodeTypeReference codeTypeReference)
                {
                    if ((message.XsdTypeName != null) && !message.XsdTypeName.IsEmpty)
                    {
                        contract.ServiceContractGenerator.GeneratedTypedMessages.Add(message, codeTypeReference);
                    }
                }

                internal static bool FindGeneratedTypedMessage(ServiceContractGenerationContext contract, MessageDescription message, out CodeTypeReference codeTypeReference)
                {
                    if ((message.XsdTypeName != null) && !message.XsdTypeName.IsEmpty)
                    {
                        return contract.ServiceContractGenerator.GeneratedTypedMessages.TryGetValue(message, out codeTypeReference);
                    }
                    codeTypeReference = null;
                    return false;
                }

                internal static void GenerateConstructors(CodeTypeDeclaration typeDecl)
                {
                    CodeConstructor constructor = new CodeConstructor {
                        Attributes = MemberAttributes.Public
                    };
                    typeDecl.Members.Add(constructor);
                    CodeConstructor constructor2 = new CodeConstructor {
                        Attributes = MemberAttributes.Public
                    };
                    foreach (CodeTypeMember member in typeDecl.Members)
                    {
                        CodeMemberField field = member as CodeMemberField;
                        if (field != null)
                        {
                            CodeParameterDeclarationExpression expression = new CodeParameterDeclarationExpression(field.Type, field.Name);
                            constructor2.Parameters.Add(expression);
                            constructor2.Statements.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), field.Name), new CodeArgumentReferenceExpression(expression.Name)));
                        }
                    }
                    if (constructor2.Parameters.Count > 0)
                    {
                        typeDecl.Members.Add(constructor2);
                    }
                }

                internal static void GenerateMessageBodyMemberAttribute(int order, MessagePartDescription setting, CodeAttributeDeclarationCollection attributes, XmlName defaultName)
                {
                    GenerateMessageContractMemberAttribute<MessageBodyMemberAttribute>(order, setting, attributes, defaultName);
                }

                private static void GenerateMessageContractMemberAttribute<T>(int order, MessagePartDescription setting, CodeAttributeDeclarationCollection attrs, XmlName defaultName) where T: Attribute
                {
                    CodeAttributeDeclaration attribute = OperationGenerator.CustomAttributeHelper.FindOrCreateAttributeDeclaration<T>(attrs);
                    if (setting.Name != defaultName.EncodedName)
                    {
                        OperationGenerator.CustomAttributeHelper.CreateOrOverridePropertyDeclaration<string>(attribute, "Name", setting.Name);
                    }
                    OperationGenerator.CustomAttributeHelper.CreateOrOverridePropertyDeclaration<string>(attribute, "Namespace", setting.Namespace);
                    if (setting.HasProtectionLevel)
                    {
                        OperationGenerator.CustomAttributeHelper.CreateOrOverridePropertyDeclaration<ProtectionLevel>(attribute, "ProtectionLevel", setting.ProtectionLevel);
                    }
                    if (order >= 0)
                    {
                        OperationGenerator.CustomAttributeHelper.CreateOrOverridePropertyDeclaration<int>(attribute, "Order", order);
                    }
                }

                internal static void GenerateMessageHeaderAttribute(MessageHeaderDescription setting, CodeAttributeDeclarationCollection attributes, XmlName defaultName)
                {
                    if (setting.Multiple)
                    {
                        GenerateMessageContractMemberAttribute<MessageHeaderArrayAttribute>(-1, setting, attributes, defaultName);
                    }
                    else
                    {
                        GenerateMessageContractMemberAttribute<MessageHeaderAttribute>(-1, setting, attributes, defaultName);
                    }
                }

                internal static void GenerateProtectionLevelAttribute(MessageDescription message, OperationGenerator.MethodSignatureGenerator.IPartCodeGenerator partCodeGenerator)
                {
                    CodeAttributeDeclaration declaration = OperationGenerator.CustomAttributeHelper.FindOrCreateAttributeDeclaration<MessageContractAttribute>(partCodeGenerator.MessageLevelAttributes);
                    if (message.HasProtectionLevel)
                    {
                        declaration.Arguments.Add(new CodeAttributeArgument("ProtectionLevel", new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(typeof(ProtectionLevel)), message.ProtectionLevel.ToString())));
                    }
                }

                internal static void GenerateWrapperAttribute(MessageDescription message, OperationGenerator.MethodSignatureGenerator.IPartCodeGenerator partCodeGenerator)
                {
                    CodeAttributeDeclaration declaration = OperationGenerator.CustomAttributeHelper.FindOrCreateAttributeDeclaration<MessageContractAttribute>(partCodeGenerator.MessageLevelAttributes);
                    if (message.Body.WrapperName != null)
                    {
                        declaration.Arguments.Add(new CodeAttributeArgument("WrapperName", new CodePrimitiveExpression(NamingHelper.CodeName(message.Body.WrapperName))));
                        declaration.Arguments.Add(new CodeAttributeArgument("WrapperNamespace", new CodePrimitiveExpression(message.Body.WrapperNamespace)));
                        declaration.Arguments.Add(new CodeAttributeArgument("IsWrapped", new CodePrimitiveExpression(true)));
                    }
                    else
                    {
                        declaration.Arguments.Add(new CodeAttributeArgument("IsWrapped", new CodePrimitiveExpression(false)));
                    }
                }
            }

            private class TypedMessagePartCodeGenerator : OperationGenerator.MethodSignatureGenerator.IPartCodeGenerator
            {
                private UniqueCodeIdentifierScope memberScope;
                private CodeTypeDeclaration typeDecl;

                internal TypedMessagePartCodeGenerator(CodeTypeDeclaration typeDecl)
                {
                    this.typeDecl = typeDecl;
                    this.memberScope = new UniqueCodeIdentifierScope();
                    this.memberScope.AddReserved(typeDecl.Name);
                }

                CodeAttributeDeclarationCollection OperationGenerator.MethodSignatureGenerator.IPartCodeGenerator.AddPart(CodeTypeReference type, ref string name)
                {
                    CodeMemberField field = new CodeMemberField {
                        Name = name = this.memberScope.AddUnique(name, "member"),
                        Type = type,
                        Attributes = MemberAttributes.Public
                    };
                    this.typeDecl.Members.Add(field);
                    return field.CustomAttributes;
                }

                void OperationGenerator.MethodSignatureGenerator.IPartCodeGenerator.EndCodeGeneration()
                {
                    OperationGenerator.MethodSignatureGenerator.TypedMessageHelper.GenerateConstructors(this.typeDecl);
                }

                CodeAttributeDeclarationCollection OperationGenerator.MethodSignatureGenerator.IPartCodeGenerator.MessageLevelAttributes
                {
                    get
                    {
                        return this.typeDecl.CustomAttributes;
                    }
                }
            }
        }
    }
}

