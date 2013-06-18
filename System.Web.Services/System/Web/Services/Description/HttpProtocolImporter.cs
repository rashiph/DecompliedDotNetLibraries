namespace System.Web.Services.Description
{
    using System;
    using System.CodeDom;
    using System.CodeDom.Compiler;
    using System.Collections;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.EnterpriseServices;
    using System.Web.Services;
    using System.Web.Services.Configuration;
    using System.Web.Services.Protocols;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;

    internal abstract class HttpProtocolImporter : ProtocolImporter
    {
        private ArrayList codeClasses = new ArrayList();
        private bool hasInputPayload;
        private ArrayList[] importedParameters;
        private ArrayList[] importedReturns;
        private MimeImporter[] importers;

        protected HttpProtocolImporter(bool hasInputPayload)
        {
            Type[] mimeImporterTypes = WebServicesSection.Current.MimeImporterTypes;
            this.importers = new MimeImporter[mimeImporterTypes.Length];
            this.importedParameters = new ArrayList[mimeImporterTypes.Length];
            this.importedReturns = new ArrayList[mimeImporterTypes.Length];
            for (int i = 0; i < this.importers.Length; i++)
            {
                MimeImporter importer = (MimeImporter) Activator.CreateInstance(mimeImporterTypes[i]);
                importer.ImportContext = this;
                this.importedParameters[i] = new ArrayList();
                this.importedReturns[i] = new ArrayList();
                this.importers[i] = importer;
            }
            this.hasInputPayload = hasInputPayload;
        }

        private static void AppendMetadata(CodeAttributeDeclarationCollection from, CodeAttributeDeclarationCollection to)
        {
            foreach (CodeAttributeDeclaration declaration in from)
            {
                to.Add(declaration);
            }
        }

        protected override CodeTypeDeclaration BeginClass()
        {
            base.MethodNames.Clear();
            base.ExtraCodeClasses.Clear();
            CodeAttributeDeclarationCollection metadata = new CodeAttributeDeclarationCollection();
            if (base.Style == ServiceDescriptionImportStyle.Client)
            {
                WebCodeGenerator.AddCustomAttribute(metadata, typeof(DebuggerStepThroughAttribute), new CodeExpression[0]);
                WebCodeGenerator.AddCustomAttribute(metadata, typeof(DesignerCategoryAttribute), new CodeExpression[] { new CodePrimitiveExpression("code") });
            }
            Type[] types = new Type[] { typeof(SoapDocumentMethodAttribute), typeof(XmlAttributeAttribute), typeof(WebService), typeof(object), typeof(DebuggerStepThroughAttribute), typeof(DesignerCategoryAttribute), typeof(TransactionOption) };
            WebCodeGenerator.AddImports(base.CodeNamespace, WebCodeGenerator.GetNamespacesForTypes(types));
            CodeFlags isAbstract = (CodeFlags) 0;
            if (base.Style == ServiceDescriptionImportStyle.Server)
            {
                isAbstract = CodeFlags.IsAbstract;
            }
            else if (base.Style == ServiceDescriptionImportStyle.ServerInterface)
            {
                isAbstract = CodeFlags.IsInterface;
            }
            CodeTypeDeclaration codeClass = WebCodeGenerator.CreateClass(base.ClassName, this.BaseClass.FullName, new string[0], metadata, CodeFlags.IsPublic | isAbstract, base.ServiceImporter.CodeGenerator.Supports(GeneratorSupport.PartialTypes));
            codeClass.Comments.Add(new CodeCommentStatement(System.Web.Services.Res.GetString("CodeRemarks"), true));
            CodeConstructor ctor = WebCodeGenerator.AddConstructor(codeClass, new string[0], new string[0], null, CodeFlags.IsPublic);
            ctor.Comments.Add(new CodeCommentStatement(System.Web.Services.Res.GetString("CodeRemarks"), true));
            HttpAddressBinding binding = (base.Port == null) ? null : ((HttpAddressBinding) base.Port.Extensions.Find(typeof(HttpAddressBinding)));
            string url = (binding != null) ? binding.Location : null;
            ServiceDescription serviceDescription = base.Binding.ServiceDescription;
            ProtocolImporterUtil.GenerateConstructorStatements(ctor, url, serviceDescription.AppSettingUrlKey, serviceDescription.AppSettingBaseUrl, false);
            this.codeClasses.Add(codeClass);
            return codeClass;
        }

        private void CreateInvokeParams(CodeExpression[] invokeParams, HttpMethodInfo method, string[] parameterNames)
        {
            invokeParams[0] = new CodePrimitiveExpression(method.Name);
            CodeExpression left = new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "Url");
            CodeExpression right = new CodePrimitiveExpression(method.Href);
            invokeParams[1] = new CodeBinaryOperatorExpression(left, CodeBinaryOperatorType.Add, right);
            CodeExpression[] initializers = new CodeExpression[parameterNames.Length];
            for (int i = 0; i < parameterNames.Length; i++)
            {
                initializers[i] = new CodeArgumentReferenceExpression(parameterNames[i]);
            }
            invokeParams[2] = new CodeArrayCreateExpression(typeof(object).FullName, initializers);
        }

        protected override void EndNamespace()
        {
            for (int i = 0; i < this.importers.Length; i++)
            {
                this.importers[i].GenerateCode((MimeReturn[]) this.importedReturns[i].ToArray(typeof(MimeReturn)), (MimeParameterCollection[]) this.importedParameters[i].ToArray(typeof(MimeParameterCollection)));
            }
            foreach (CodeTypeDeclaration declaration in this.codeClasses)
            {
                if (declaration.CustomAttributes == null)
                {
                    declaration.CustomAttributes = new CodeAttributeDeclarationCollection();
                }
                for (int j = 0; j < this.importers.Length; j++)
                {
                    this.importers[j].AddClassMetadata(declaration);
                }
            }
            foreach (CodeTypeDeclaration declaration2 in base.ExtraCodeClasses)
            {
                base.CodeNamespace.Types.Add(declaration2);
            }
            CodeGenerator.ValidateIdentifiers(base.CodeNamespace);
        }

        protected override CodeMemberMethod GenerateMethod()
        {
            HttpOperationBinding binding = (HttpOperationBinding) base.OperationBinding.Extensions.Find(typeof(HttpOperationBinding));
            if (binding == null)
            {
                throw base.OperationBindingSyntaxException(System.Web.Services.Res.GetString("MissingHttpOperationElement0"));
            }
            HttpMethodInfo info = new HttpMethodInfo();
            if (this.hasInputPayload)
            {
                info.MimeParameters = this.ImportMimeParameters();
                if (info.MimeParameters == null)
                {
                    base.UnsupportedOperationWarning(System.Web.Services.Res.GetString("NoInputMIMEFormatsWereRecognized0"));
                    return null;
                }
            }
            else
            {
                info.UrlParameters = this.ImportUrlParameters();
                if (info.UrlParameters == null)
                {
                    base.UnsupportedOperationWarning(System.Web.Services.Res.GetString("NoInputHTTPFormatsWereRecognized0"));
                    return null;
                }
            }
            info.MimeReturn = this.ImportMimeReturn();
            if (info.MimeReturn == null)
            {
                base.UnsupportedOperationWarning(System.Web.Services.Res.GetString("NoOutputMIMEFormatsWereRecognized0"));
                return null;
            }
            info.Name = base.MethodNames.AddUnique(base.MethodName, info);
            info.Href = binding.Location;
            return this.GenerateMethod(info);
        }

        private CodeMemberMethod GenerateMethod(HttpMethodInfo method)
        {
            MimeParameterCollection parameters = (method.MimeParameters != null) ? method.MimeParameters : method.UrlParameters;
            string[] parameterTypeNames = new string[parameters.Count];
            string[] parameterNames = new string[parameters.Count];
            for (int i = 0; i < parameters.Count; i++)
            {
                MimeParameter parameter = parameters[i];
                parameterNames[i] = parameter.Name;
                parameterTypeNames[i] = parameter.TypeName;
            }
            CodeAttributeDeclarationCollection metadata = new CodeAttributeDeclarationCollection();
            CodeExpression[] expressionArray = new CodeExpression[2];
            if (method.MimeReturn.ReaderType == null)
            {
                expressionArray[0] = new CodeTypeOfExpression(typeof(NopReturnReader).FullName);
            }
            else
            {
                expressionArray[0] = new CodeTypeOfExpression(method.MimeReturn.ReaderType.FullName);
            }
            if (method.MimeParameters != null)
            {
                expressionArray[1] = new CodeTypeOfExpression(method.MimeParameters.WriterType.FullName);
            }
            else
            {
                expressionArray[1] = new CodeTypeOfExpression(typeof(UrlParameterWriter).FullName);
            }
            WebCodeGenerator.AddCustomAttribute(metadata, typeof(HttpMethodAttribute), expressionArray, new string[0], new CodeExpression[0]);
            CodeMemberMethod method2 = WebCodeGenerator.AddMethod(base.CodeTypeDeclaration, method.Name, new CodeFlags[parameterTypeNames.Length], parameterTypeNames, parameterNames, method.MimeReturn.TypeName, metadata, CodeFlags.IsPublic | ((base.Style == ServiceDescriptionImportStyle.Client) ? ((CodeFlags) 0) : CodeFlags.IsAbstract));
            AppendMetadata(method.MimeReturn.Attributes, method2.ReturnTypeCustomAttributes);
            method2.Comments.Add(new CodeCommentStatement(System.Web.Services.Res.GetString("CodeRemarks"), true));
            for (int j = 0; j < parameters.Count; j++)
            {
                AppendMetadata(parameters[j].Attributes, method2.Parameters[j].CustomAttributes);
            }
            if (base.Style == ServiceDescriptionImportStyle.Client)
            {
                bool flag = (base.ServiceImporter.CodeGenerationOptions & CodeGenerationOptions.GenerateOldAsync) != CodeGenerationOptions.None;
                bool flag2 = (((base.ServiceImporter.CodeGenerationOptions & CodeGenerationOptions.GenerateNewAsync) != CodeGenerationOptions.None) && base.ServiceImporter.CodeGenerator.Supports(GeneratorSupport.DeclareEvents)) && base.ServiceImporter.CodeGenerator.Supports(GeneratorSupport.DeclareDelegates);
                CodeExpression[] invokeParams = new CodeExpression[3];
                this.CreateInvokeParams(invokeParams, method, parameterNames);
                CodeMethodInvokeExpression expression = new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "Invoke", invokeParams);
                if (method.MimeReturn.ReaderType != null)
                {
                    method2.Statements.Add(new CodeMethodReturnStatement(new CodeCastExpression(method.MimeReturn.TypeName, expression)));
                }
                else
                {
                    method2.Statements.Add(new CodeExpressionStatement(expression));
                }
                metadata = new CodeAttributeDeclarationCollection();
                string[] array = new string[parameterTypeNames.Length + 2];
                parameterTypeNames.CopyTo(array, 0);
                array[parameterTypeNames.Length] = typeof(AsyncCallback).FullName;
                array[parameterTypeNames.Length + 1] = typeof(object).FullName;
                string[] strArray4 = new string[parameterNames.Length + 2];
                parameterNames.CopyTo(strArray4, 0);
                strArray4[parameterNames.Length] = "callback";
                strArray4[parameterNames.Length + 1] = "asyncState";
                if (flag)
                {
                    CodeMemberMethod method3 = WebCodeGenerator.AddMethod(base.CodeTypeDeclaration, "Begin" + method.Name, new CodeFlags[array.Length], array, strArray4, typeof(IAsyncResult).FullName, metadata, CodeFlags.IsPublic);
                    method3.Comments.Add(new CodeCommentStatement(System.Web.Services.Res.GetString("CodeRemarks"), true));
                    invokeParams = new CodeExpression[5];
                    this.CreateInvokeParams(invokeParams, method, parameterNames);
                    invokeParams[3] = new CodeArgumentReferenceExpression("callback");
                    invokeParams[4] = new CodeArgumentReferenceExpression("asyncState");
                    expression = new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "BeginInvoke", invokeParams);
                    method3.Statements.Add(new CodeMethodReturnStatement(expression));
                    CodeMemberMethod method4 = WebCodeGenerator.AddMethod(base.CodeTypeDeclaration, "End" + method.Name, new CodeFlags[1], new string[] { typeof(IAsyncResult).FullName }, new string[] { "asyncResult" }, method.MimeReturn.TypeName, metadata, CodeFlags.IsPublic);
                    method4.Comments.Add(new CodeCommentStatement(System.Web.Services.Res.GetString("CodeRemarks"), true));
                    CodeExpression expression2 = new CodeArgumentReferenceExpression("asyncResult");
                    expression = new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "EndInvoke", new CodeExpression[] { expression2 });
                    if (method.MimeReturn.ReaderType != null)
                    {
                        method4.Statements.Add(new CodeMethodReturnStatement(new CodeCastExpression(method.MimeReturn.TypeName, expression)));
                    }
                    else
                    {
                        method4.Statements.Add(new CodeExpressionStatement(expression));
                    }
                }
                if (!flag2)
                {
                    return method2;
                }
                metadata = new CodeAttributeDeclarationCollection();
                string name = method.Name;
                string str2 = ProtocolImporter.MethodSignature(name, method.MimeReturn.TypeName, new CodeFlags[parameterTypeNames.Length], parameterTypeNames);
                DelegateInfo info = (DelegateInfo) base.ExportContext[str2];
                if (info == null)
                {
                    string handlerType = base.ClassNames.AddUnique(name + "CompletedEventHandler", name);
                    string handlerArgs = base.ClassNames.AddUnique(name + "CompletedEventArgs", name);
                    info = new DelegateInfo(handlerType, handlerArgs);
                }
                string handlerName = base.MethodNames.AddUnique(name + "Completed", name);
                string methodName = base.MethodNames.AddUnique(name + "Async", name);
                string callbackMember = base.MethodNames.AddUnique(name + "OperationCompleted", name);
                string callbackName = base.MethodNames.AddUnique("On" + name + "OperationCompleted", name);
                WebCodeGenerator.AddEvent(base.CodeTypeDeclaration.Members, info.handlerType, handlerName);
                WebCodeGenerator.AddCallbackDeclaration(base.CodeTypeDeclaration.Members, callbackMember);
                string userState = ProtocolImporter.UniqueName("userState", parameterNames);
                CodeMemberMethod method5 = WebCodeGenerator.AddAsyncMethod(base.CodeTypeDeclaration, methodName, parameterTypeNames, parameterNames, callbackMember, callbackName, userState);
                invokeParams = new CodeExpression[5];
                this.CreateInvokeParams(invokeParams, method, parameterNames);
                invokeParams[3] = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), callbackMember);
                invokeParams[4] = new CodeArgumentReferenceExpression(userState);
                expression = new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "InvokeAsync", invokeParams);
                method5.Statements.Add(expression);
                bool methodHasOutParameters = method.MimeReturn.ReaderType != null;
                WebCodeGenerator.AddCallbackImplementation(base.CodeTypeDeclaration, callbackName, handlerName, info.handlerArgs, methodHasOutParameters);
                if (base.ExportContext[str2] != null)
                {
                    return method2;
                }
                WebCodeGenerator.AddDelegate(base.ExtraCodeClasses, info.handlerType, methodHasOutParameters ? info.handlerArgs : typeof(AsyncCompletedEventArgs).FullName);
                if (methodHasOutParameters)
                {
                    base.ExtraCodeClasses.Add(WebCodeGenerator.CreateArgsClass(info.handlerArgs, new string[] { method.MimeReturn.TypeName }, new string[] { "Result" }, base.ServiceImporter.CodeGenerator.Supports(GeneratorSupport.PartialTypes)));
                }
                base.ExportContext[str2] = info;
            }
            return method2;
        }

        private MimeParameterCollection ImportMimeParameters()
        {
            for (int i = 0; i < this.importers.Length; i++)
            {
                MimeParameterCollection parameters = this.importers[i].ImportParameters();
                if (parameters != null)
                {
                    this.importedParameters[i].Add(parameters);
                    return parameters;
                }
            }
            return null;
        }

        private MimeReturn ImportMimeReturn()
        {
            if (base.OperationBinding.Output.Extensions.Count == 0)
            {
                return new MimeReturn { TypeName = typeof(void).FullName };
            }
            for (int i = 0; i < this.importers.Length; i++)
            {
                MimeReturn return2 = this.importers[i].ImportReturn();
                if (return2 != null)
                {
                    this.importedReturns[i].Add(return2);
                    return return2;
                }
            }
            return null;
        }

        internal MimeParameterCollection ImportStringParametersMessage()
        {
            MimeParameterCollection parameters = new MimeParameterCollection();
            foreach (MessagePart part in base.InputMessage.Parts)
            {
                MimeParameter parameter = this.ImportUrlParameter(part);
                if (parameter == null)
                {
                    return null;
                }
                parameters.Add(parameter);
            }
            return parameters;
        }

        private MimeParameter ImportUrlParameter(MessagePart part)
        {
            return new MimeParameter { Name = CodeIdentifier.MakeValid(XmlConvert.DecodeName(part.Name)), TypeName = this.IsRepeatingParameter(part) ? typeof(string[]).FullName : typeof(string).FullName };
        }

        private MimeParameterCollection ImportUrlParameters()
        {
            if (((HttpUrlEncodedBinding) base.OperationBinding.Input.Extensions.Find(typeof(HttpUrlEncodedBinding))) == null)
            {
                return new MimeParameterCollection();
            }
            return this.ImportStringParametersMessage();
        }

        protected override bool IsOperationFlowSupported(OperationFlow flow)
        {
            return (flow == OperationFlow.RequestResponse);
        }

        private bool IsRepeatingParameter(MessagePart part)
        {
            XmlSchemaComplexType type = (XmlSchemaComplexType) base.Schemas.Find(part.Type, typeof(XmlSchemaComplexType));
            if (type == null)
            {
                return false;
            }
            if (type.ContentModel == null)
            {
                return false;
            }
            if (type.ContentModel.Content == null)
            {
                throw new ArgumentException(System.Web.Services.Res.GetString("Missing2", new object[] { type.Name, type.ContentModel.GetType().Name }), "part");
            }
            if (type.ContentModel.Content is XmlSchemaComplexContentExtension)
            {
                return (((XmlSchemaComplexContentExtension) type.ContentModel.Content).BaseTypeName == new XmlQualifiedName("Array", "http://schemas.xmlsoap.org/soap/encoding/"));
            }
            return ((type.ContentModel.Content is XmlSchemaComplexContentRestriction) && (((XmlSchemaComplexContentRestriction) type.ContentModel.Content).BaseTypeName == new XmlQualifiedName("Array", "http://schemas.xmlsoap.org/soap/encoding/")));
        }

        internal abstract Type BaseClass { get; }
    }
}

