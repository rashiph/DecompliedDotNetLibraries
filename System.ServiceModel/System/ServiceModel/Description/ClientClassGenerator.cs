namespace System.ServiceModel.Description
{
    using System;
    using System.CodeDom;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Globalization;
    using System.ServiceModel;

    internal class ClientClassGenerator : IServiceContractGenerationExtension
    {
        private static Type asyncCallbackType = typeof(AsyncCallback);
        private static Type asyncCompletedEventArgsType = typeof(AsyncCompletedEventArgs);
        private static Type asyncResultType = typeof(IAsyncResult);
        private static CodeTypeReference asyncResultTypeRef = new CodeTypeReference(typeof(IAsyncResult));
        private static string beginOperationDelegateTypeName = "BeginOperationDelegate";
        private static Type bindingType = typeof(Binding);
        private static Type boolType = typeof(bool);
        private static Type clientBaseType = typeof(ClientBase<>);
        private static string[][] ClientCtorParamNames = new string[][] { new string[0], new string[] { "endpointConfigurationName" }, new string[] { "endpointConfigurationName", "remoteAddress" }, new string[] { "endpointConfigurationName", "remoteAddress" }, new string[] { "binding", "remoteAddress" } };
        private static Type[][] ClientCtorParamTypes = new Type[][] { new Type[0], new Type[] { stringType }, new Type[] { stringType, stringType }, new Type[] { stringType, endpointAddressType }, new Type[] { bindingType, endpointAddressType } };
        private static Type duplexClientBaseType = typeof(DuplexClientBase<>);
        private static string endOperationDelegateTypeName = "EndOperationDelegate";
        private static Type endpointAddressType = typeof(EndpointAddress);
        private static string[] EventArgsCtorParamNames = new string[] { "results", "exception", "cancelled", "userState" };
        private static Type[] EventArgsCtorParamTypes = new Type[] { objectArrayType, exceptionType, boolType, objectType };
        private static string[] EventArgsPropertyNames = new string[] { "Results", "Error", "Cancelled", "UserState" };
        private static Type eventHandlerType = typeof(EventHandler<>);
        private static Type exceptionType = typeof(Exception);
        private bool generateEventAsyncMethods;
        private static string getDefaultValueForInitializationMethodName = "GetDefaultValueForInitialization";
        private static string inputInstanceName = "callbackInstance";
        private static Type instanceContextType = typeof(InstanceContext);
        private static string invokeAsyncCompletedEventArgsTypeName = "InvokeAsyncCompletedEventArgs";
        private static string invokeAsyncMethodName = "InvokeAsync";
        private static Type objectArrayType = typeof(object[]);
        private static Type objectType = typeof(object);
        private static string raiseExceptionIfNecessaryMethodName = "RaiseExceptionIfNecessary";
        private static Type sendOrPostCallbackType = typeof(SendOrPostCallback);
        private static Type stringType = typeof(string);
        private bool tryAddHelperMethod;
        private static Type uriType = typeof(Uri);
        private static Type voidType = typeof(void);
        private static CodeTypeReference voidTypeRef = new CodeTypeReference(typeof(void));

        internal ClientClassGenerator(bool tryAddHelperMethod) : this(tryAddHelperMethod, false)
        {
        }

        internal ClientClassGenerator(bool tryAddHelperMethod, bool generateEventAsyncMethods)
        {
            this.tryAddHelperMethod = tryAddHelperMethod;
            this.generateEventAsyncMethods = generateEventAsyncMethods;
        }

        private static void AddMethodImpl(CodeMemberMethod method)
        {
            CodeMethodInvokeExpression expression = new CodeMethodInvokeExpression(GetChannelReference(), method.Name, new CodeExpression[0]);
            foreach (CodeParameterDeclarationExpression expression2 in method.Parameters)
            {
                expression.Parameters.Add(new CodeDirectionExpression(expression2.Direction, new CodeVariableReferenceExpression(expression2.Name)));
            }
            if (IsVoid(method))
            {
                method.Statements.Add(expression);
            }
            else
            {
                method.Statements.Add(new CodeMethodReturnStatement(expression));
            }
        }

        private static CodeMemberField CreateBeginOperationDelegate(ServiceContractGenerationContext context, CodeTypeDeclaration clientType, string syncMethodName)
        {
            CodeMemberField field = new CodeMemberField {
                Attributes = MemberAttributes.Private,
                Type = new CodeTypeReference(beginOperationDelegateTypeName),
                Name = NamingHelper.GetUniqueName(GetBeginOperationDelegateName(syncMethodName), new NamingHelper.DoesNameExist(ClientClassGenerator.DoesMethodNameExist), context.Operations)
            };
            clientType.Members.Add(field);
            return field;
        }

        private static CodeMemberMethod CreateBeginOperationMethod(ServiceContractGenerationContext context, CodeTypeDeclaration clientType, string syncMethodName, CodeMemberMethod beginMethod)
        {
            CodeMemberMethod method = new CodeMemberMethod {
                Attributes = MemberAttributes.Private,
                ReturnType = new CodeTypeReference(asyncResultType),
                Name = NamingHelper.GetUniqueName(GetBeginOperationMethodName(syncMethodName), new NamingHelper.DoesNameExist(ClientClassGenerator.DoesMethodNameExist), context.Operations)
            };
            CodeParameterDeclarationExpression expression = new CodeParameterDeclarationExpression {
                Type = new CodeTypeReference(objectArrayType),
                Name = NamingHelper.GetUniqueName("inValues", new NamingHelper.DoesNameExist(ClientClassGenerator.DoesParameterNameExist), beginMethod)
            };
            method.Parameters.Add(expression);
            CodeMethodInvokeExpression expression2 = new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), beginMethod.Name, new CodeExpression[0]);
            CodeExpression targetObject = new CodeVariableReferenceExpression(expression.Name);
            for (int i = 0; i < (beginMethod.Parameters.Count - 2); i++)
            {
                CodeVariableDeclarationStatement statement;
                statement = new CodeVariableDeclarationStatement {
                    Type = beginMethod.Parameters[i].Type,
                    Name = beginMethod.Parameters[i].Name,
                    InitExpression = new CodeCastExpression(statement.Type, new CodeArrayIndexerExpression(targetObject, new CodeExpression[] { new CodePrimitiveExpression(i) }))
                };
                method.Statements.Add(statement);
                expression2.Parameters.Add(new CodeDirectionExpression(beginMethod.Parameters[i].Direction, new CodeVariableReferenceExpression(statement.Name)));
            }
            for (int j = beginMethod.Parameters.Count - 2; j < beginMethod.Parameters.Count; j++)
            {
                method.Parameters.Add(new CodeParameterDeclarationExpression(beginMethod.Parameters[j].Type, beginMethod.Parameters[j].Name));
                expression2.Parameters.Add(new CodeVariableReferenceExpression(beginMethod.Parameters[j].Name));
            }
            method.Statements.Add(new CodeMethodReturnStatement(expression2));
            clientType.Members.Add(method);
            return method;
        }

        private static CodeStatement CreateDelegateIfNotNull(CodeMemberField delegateField, CodeMemberMethod delegateMethod)
        {
            return new CodeConditionStatement(new CodeBinaryOperatorExpression(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), delegateField.Name), CodeBinaryOperatorType.IdentityEquality, new CodePrimitiveExpression(null)), new CodeStatement[] { new CodeAssignStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), delegateField.Name), new CodeDelegateCreateExpression(delegateField.Type, new CodeThisReferenceExpression(), delegateMethod.Name)) });
        }

        internal static CodeAttributeDeclaration CreateEditorBrowsableAttribute(EditorBrowsableState editorBrowsableState)
        {
            CodeAttributeDeclaration declaration = new CodeAttributeDeclaration(new CodeTypeReference(typeof(EditorBrowsableAttribute)));
            CodeTypeReferenceExpression targetObject = new CodeTypeReferenceExpression(typeof(EditorBrowsableState));
            CodeAttributeArgument argument = new CodeAttributeArgument(new CodeFieldReferenceExpression(targetObject, editorBrowsableState.ToString()));
            declaration.Arguments.Add(argument);
            return declaration;
        }

        private static CodeMemberField CreateEndOperationDelegate(ServiceContractGenerationContext context, CodeTypeDeclaration clientType, string syncMethodName)
        {
            CodeMemberField field = new CodeMemberField {
                Attributes = MemberAttributes.Private,
                Type = new CodeTypeReference(endOperationDelegateTypeName),
                Name = NamingHelper.GetUniqueName(GetEndOperationDelegateName(syncMethodName), new NamingHelper.DoesNameExist(ClientClassGenerator.DoesMethodNameExist), context.Operations)
            };
            clientType.Members.Add(field);
            return field;
        }

        private static CodeMemberMethod CreateEndOperationMethod(ServiceContractGenerationContext context, CodeTypeDeclaration clientType, string syncMethodName, CodeMemberMethod endMethod)
        {
            CodeMemberMethod method = new CodeMemberMethod {
                Attributes = MemberAttributes.Private,
                ReturnType = new CodeTypeReference(objectArrayType),
                Name = NamingHelper.GetUniqueName(GetEndOperationMethodName(syncMethodName), new NamingHelper.DoesNameExist(ClientClassGenerator.DoesMethodNameExist), context.Operations)
            };
            int asyncResultParamIndex = GetAsyncResultParamIndex(endMethod);
            CodeMethodInvokeExpression expression = new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), endMethod.Name, new CodeExpression[0]);
            CodeArrayCreateExpression expression2 = new CodeArrayCreateExpression {
                CreateType = new CodeTypeReference(objectArrayType)
            };
            for (int i = 0; i < endMethod.Parameters.Count; i++)
            {
                if (i == asyncResultParamIndex)
                {
                    method.Parameters.Add(new CodeParameterDeclarationExpression(endMethod.Parameters[i].Type, endMethod.Parameters[i].Name));
                    expression.Parameters.Add(new CodeVariableReferenceExpression(endMethod.Parameters[i].Name));
                }
                else
                {
                    CodeVariableDeclarationStatement statement = new CodeVariableDeclarationStatement(endMethod.Parameters[i].Type, endMethod.Parameters[i].Name);
                    CodeMethodReferenceExpression expression3 = new CodeMethodReferenceExpression(new CodeThisReferenceExpression(), getDefaultValueForInitializationMethodName, new CodeTypeReference[] { endMethod.Parameters[i].Type });
                    statement.InitExpression = new CodeMethodInvokeExpression(expression3, new CodeExpression[0]);
                    method.Statements.Add(statement);
                    expression.Parameters.Add(new CodeDirectionExpression(endMethod.Parameters[i].Direction, new CodeVariableReferenceExpression(statement.Name)));
                    expression2.Initializers.Add(new CodeVariableReferenceExpression(statement.Name));
                }
            }
            if (endMethod.ReturnType.BaseType != voidTypeRef.BaseType)
            {
                CodeVariableDeclarationStatement statement2 = new CodeVariableDeclarationStatement {
                    Type = endMethod.ReturnType,
                    Name = NamingHelper.GetUniqueName("retVal", new NamingHelper.DoesNameExist(ClientClassGenerator.DoesParameterNameExist), endMethod),
                    InitExpression = expression
                };
                expression2.Initializers.Add(new CodeVariableReferenceExpression(statement2.Name));
                method.Statements.Add(statement2);
            }
            else
            {
                method.Statements.Add(expression);
            }
            if (expression2.Initializers.Count > 0)
            {
                method.Statements.Add(new CodeMethodReturnStatement(expression2));
            }
            else
            {
                method.Statements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(null)));
            }
            clientType.Members.Add(method);
            return method;
        }

        private static CodeMemberProperty CreateEventAsyncCompletedArgsTypeProperty(CodeTypeDeclaration ownerTypeDecl, CodeTypeReference propertyType, string propertyName, CodeExpression propertyValueExpr)
        {
            CodeMemberProperty property = new CodeMemberProperty {
                Attributes = MemberAttributes.Public | MemberAttributes.Final,
                Type = propertyType,
                Name = propertyName,
                HasSet = false,
                HasGet = true
            };
            CodeCastExpression expression = new CodeCastExpression(propertyType, propertyValueExpr);
            CodeMethodReturnStatement statement = new CodeMethodReturnStatement(expression);
            property.GetStatements.Add(new CodeMethodInvokeExpression(new CodeBaseReferenceExpression(), raiseExceptionIfNecessaryMethodName, new CodeExpression[0]));
            property.GetStatements.Add(statement);
            ownerTypeDecl.Members.Add(property);
            return property;
        }

        private static CodeMemberMethod CreateEventAsyncMethod(ServiceContractGenerationContext context, CodeTypeDeclaration clientType, string syncMethodName, CodeMemberMethod beginMethod, CodeMemberField beginOperationDelegate, CodeMemberMethod beginOperationMethod, CodeMemberField endOperationDelegate, CodeMemberMethod endOperationMethod, CodeMemberField operationCompletedDelegate, CodeMemberMethod operationCompletedMethod)
        {
            CodeMemberMethod nameCollection = new CodeMemberMethod {
                Name = NamingHelper.GetUniqueName(GetEventAsyncMethodName(syncMethodName), new NamingHelper.DoesNameExist(ClientClassGenerator.DoesMethodNameExist), context.Operations),
                Attributes = MemberAttributes.Public | MemberAttributes.Final,
                ReturnType = new CodeTypeReference(voidType)
            };
            CodeArrayCreateExpression expression = new CodeArrayCreateExpression(new CodeTypeReference(objectArrayType), new CodeExpression[0]);
            for (int i = 0; i < (beginMethod.Parameters.Count - 2); i++)
            {
                CodeParameterDeclarationExpression expression2 = beginMethod.Parameters[i];
                CodeParameterDeclarationExpression expression3 = new CodeParameterDeclarationExpression(expression2.Type, expression2.Name) {
                    Direction = FieldDirection.In
                };
                nameCollection.Parameters.Add(expression3);
                expression.Initializers.Add(new CodeVariableReferenceExpression(expression3.Name));
            }
            string name = NamingHelper.GetUniqueName("userState", new NamingHelper.DoesNameExist(ClientClassGenerator.DoesParameterNameExist), nameCollection);
            nameCollection.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(objectType), name));
            nameCollection.Statements.Add(CreateDelegateIfNotNull(beginOperationDelegate, beginOperationMethod));
            nameCollection.Statements.Add(CreateDelegateIfNotNull(endOperationDelegate, endOperationMethod));
            nameCollection.Statements.Add(CreateDelegateIfNotNull(operationCompletedDelegate, operationCompletedMethod));
            CodeMethodInvokeExpression expression4 = new CodeMethodInvokeExpression(new CodeBaseReferenceExpression(), invokeAsyncMethodName, new CodeExpression[0]);
            expression4.Parameters.Add(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), beginOperationDelegate.Name));
            if (expression.Initializers.Count > 0)
            {
                expression4.Parameters.Add(expression);
            }
            else
            {
                expression4.Parameters.Add(new CodePrimitiveExpression(null));
            }
            expression4.Parameters.Add(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), endOperationDelegate.Name));
            expression4.Parameters.Add(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), operationCompletedDelegate.Name));
            expression4.Parameters.Add(new CodeVariableReferenceExpression(name));
            nameCollection.Statements.Add(new CodeExpressionStatement(expression4));
            clientType.Members.Add(nameCollection);
            return nameCollection;
        }

        private static CodeMemberMethod CreateEventAsyncMethodOverload(CodeTypeDeclaration clientType, CodeMemberMethod eventAsyncMethod)
        {
            CodeMemberMethod method = new CodeMemberMethod {
                Attributes = eventAsyncMethod.Attributes,
                Name = eventAsyncMethod.Name,
                ReturnType = eventAsyncMethod.ReturnType
            };
            CodeMethodInvokeExpression expression = new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), eventAsyncMethod.Name, new CodeExpression[0]);
            for (int i = 0; i < (eventAsyncMethod.Parameters.Count - 1); i++)
            {
                method.Parameters.Add(new CodeParameterDeclarationExpression(eventAsyncMethod.Parameters[i].Type, eventAsyncMethod.Parameters[i].Name));
                expression.Parameters.Add(new CodeVariableReferenceExpression(eventAsyncMethod.Parameters[i].Name));
            }
            expression.Parameters.Add(new CodePrimitiveExpression(null));
            method.Statements.Add(expression);
            int index = clientType.Members.IndexOf(eventAsyncMethod);
            clientType.Members.Insert(index, method);
            return method;
        }

        private static CodeMemberField CreateOperationCompletedDelegate(ServiceContractGenerationContext context, CodeTypeDeclaration clientType, string syncMethodName)
        {
            CodeMemberField field = new CodeMemberField {
                Attributes = MemberAttributes.Private,
                Type = new CodeTypeReference(sendOrPostCallbackType),
                Name = NamingHelper.GetUniqueName(GetOperationCompletedDelegateName(syncMethodName), new NamingHelper.DoesNameExist(ClientClassGenerator.DoesMethodNameExist), context.Operations)
            };
            clientType.Members.Add(field);
            return field;
        }

        private static CodeMemberEvent CreateOperationCompletedEvent(ServiceContractGenerationContext context, CodeTypeDeclaration clientType, string syncMethodName, CodeTypeDeclaration operationCompletedEventArgsType)
        {
            CodeMemberEvent event2 = new CodeMemberEvent {
                Attributes = MemberAttributes.Public,
                Type = new CodeTypeReference(eventHandlerType)
            };
            if (operationCompletedEventArgsType == null)
            {
                event2.Type.TypeArguments.Add(asyncCompletedEventArgsType);
            }
            else
            {
                event2.Type.TypeArguments.Add(operationCompletedEventArgsType.Name);
            }
            event2.Name = NamingHelper.GetUniqueName(GetOperationCompletedEventName(syncMethodName), new NamingHelper.DoesNameExist(ClientClassGenerator.DoesMethodNameExist), context.Operations);
            clientType.Members.Add(event2);
            return event2;
        }

        private static CodeTypeDeclaration CreateOperationCompletedEventArgsType(ServiceContractGenerationContext context, string syncMethodName, CodeMemberMethod endMethod)
        {
            if ((endMethod.Parameters.Count == 1) && (endMethod.ReturnType.BaseType == voidTypeRef.BaseType))
            {
                return null;
            }
            CodeTypeDeclaration ownerTypeDecl = context.TypeFactory.CreateClassType();
            ownerTypeDecl.BaseTypes.Add(new CodeTypeReference(asyncCompletedEventArgsType));
            CodeMemberField field = new CodeMemberField {
                Type = new CodeTypeReference(objectArrayType)
            };
            CodeFieldReferenceExpression left = new CodeFieldReferenceExpression {
                TargetObject = new CodeThisReferenceExpression()
            };
            CodeConstructor constructor = new CodeConstructor {
                Attributes = MemberAttributes.Public
            };
            for (int i = 0; i < EventArgsCtorParamTypes.Length; i++)
            {
                constructor.Parameters.Add(new CodeParameterDeclarationExpression(EventArgsCtorParamTypes[i], EventArgsCtorParamNames[i]));
                if (i > 0)
                {
                    constructor.BaseConstructorArgs.Add(new CodeVariableReferenceExpression(EventArgsCtorParamNames[i]));
                }
            }
            ownerTypeDecl.Members.Add(constructor);
            constructor.Statements.Add(new CodeAssignStatement(left, new CodeVariableReferenceExpression(EventArgsCtorParamNames[0])));
            int asyncResultParamIndex = GetAsyncResultParamIndex(endMethod);
            int num3 = 0;
            for (int j = 0; j < endMethod.Parameters.Count; j++)
            {
                if (j != asyncResultParamIndex)
                {
                    CreateEventAsyncCompletedArgsTypeProperty(ownerTypeDecl, endMethod.Parameters[j].Type, endMethod.Parameters[j].Name, new CodeArrayIndexerExpression(left, new CodeExpression[] { new CodePrimitiveExpression(num3++) }));
                }
            }
            if (endMethod.ReturnType.BaseType != voidTypeRef.BaseType)
            {
                CreateEventAsyncCompletedArgsTypeProperty(ownerTypeDecl, endMethod.ReturnType, NamingHelper.GetUniqueName("Result", new NamingHelper.DoesNameExist(ClientClassGenerator.DoesMemberNameExist), ownerTypeDecl), new CodeArrayIndexerExpression(left, new CodeExpression[] { new CodePrimitiveExpression(num3) }));
            }
            field.Name = NamingHelper.GetUniqueName("results", new NamingHelper.DoesNameExist(ClientClassGenerator.DoesMemberNameExist), ownerTypeDecl);
            left.FieldName = field.Name;
            ownerTypeDecl.Members.Add(field);
            ownerTypeDecl.Name = NamingHelper.GetUniqueName(GetOperationCompletedEventArgsTypeName(syncMethodName), new NamingHelper.DoesNameExist(ClientClassGenerator.DoesTypeAndMemberNameExist), new object[] { context.Namespace.Types, ownerTypeDecl });
            context.Namespace.Types.Add(ownerTypeDecl);
            return ownerTypeDecl;
        }

        private static CodeMemberMethod CreateOperationCompletedMethod(ServiceContractGenerationContext context, CodeTypeDeclaration clientType, string syncMethodName, CodeTypeDeclaration operationCompletedEventArgsType, CodeMemberEvent operationCompletedEvent)
        {
            CodeObjectCreateExpression expression;
            CodeMemberMethod method = new CodeMemberMethod {
                Attributes = MemberAttributes.Private,
                Name = NamingHelper.GetUniqueName(GetOperationCompletedMethodName(syncMethodName), new NamingHelper.DoesNameExist(ClientClassGenerator.DoesMethodNameExist), context.Operations)
            };
            method.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(objectType), "state"));
            method.ReturnType = new CodeTypeReference(voidType);
            CodeVariableDeclarationStatement statement = new CodeVariableDeclarationStatement(invokeAsyncCompletedEventArgsTypeName, "e") {
                InitExpression = new CodeCastExpression(invokeAsyncCompletedEventArgsTypeName, new CodeArgumentReferenceExpression(method.Parameters[0].Name))
            };
            CodeVariableReferenceExpression targetObject = new CodeVariableReferenceExpression(statement.Name);
            if (operationCompletedEventArgsType != null)
            {
                expression = new CodeObjectCreateExpression(operationCompletedEventArgsType.Name, new CodeExpression[] { new CodePropertyReferenceExpression(targetObject, EventArgsPropertyNames[0]), new CodePropertyReferenceExpression(targetObject, EventArgsPropertyNames[1]), new CodePropertyReferenceExpression(targetObject, EventArgsPropertyNames[2]), new CodePropertyReferenceExpression(targetObject, EventArgsPropertyNames[3]) });
            }
            else
            {
                expression = new CodeObjectCreateExpression(asyncCompletedEventArgsType, new CodeExpression[] { new CodePropertyReferenceExpression(targetObject, EventArgsPropertyNames[1]), new CodePropertyReferenceExpression(targetObject, EventArgsPropertyNames[2]), new CodePropertyReferenceExpression(targetObject, EventArgsPropertyNames[3]) });
            }
            CodeEventReferenceExpression expression3 = new CodeEventReferenceExpression(new CodeThisReferenceExpression(), operationCompletedEvent.Name);
            CodeDelegateInvokeExpression expression4 = new CodeDelegateInvokeExpression(expression3, new CodeExpression[] { new CodeThisReferenceExpression(), expression });
            CodeConditionStatement statement2 = new CodeConditionStatement(new CodeBinaryOperatorExpression(expression3, CodeBinaryOperatorType.IdentityInequality, new CodePrimitiveExpression(null)), new CodeStatement[] { statement, new CodeExpressionStatement(expression4) });
            method.Statements.Add(statement2);
            clientType.Members.Add(method);
            return method;
        }

        internal static bool DoesMemberNameExist(string name, object typeDeclarationObject)
        {
            CodeTypeDeclaration declaration = (CodeTypeDeclaration) typeDeclarationObject;
            if (string.Compare(declaration.Name, name, StringComparison.OrdinalIgnoreCase) == 0)
            {
                return true;
            }
            foreach (CodeTypeMember member in declaration.Members)
            {
                if (string.Compare(member.Name, name, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return true;
                }
            }
            return false;
        }

        internal static bool DoesMethodNameExist(string name, object operationsObject)
        {
            Collection<OperationContractGenerationContext> collection = (Collection<OperationContractGenerationContext>) operationsObject;
            foreach (OperationContractGenerationContext context in collection)
            {
                if (string.Compare(context.SyncMethod.Name, name, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return true;
                }
                if (context.IsAsync)
                {
                    if (string.Compare(context.BeginMethod.Name, name, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        return true;
                    }
                    if (string.Compare(context.EndMethod.Name, name, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        internal static bool DoesParameterNameExist(string name, object methodObject)
        {
            CodeMemberMethod method = (CodeMemberMethod) methodObject;
            if (string.Compare(method.Name, name, StringComparison.OrdinalIgnoreCase) == 0)
            {
                return true;
            }
            foreach (CodeParameterDeclarationExpression expression in method.Parameters)
            {
                if (string.Compare(expression.Name, name, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return true;
                }
            }
            return false;
        }

        internal static bool DoesTypeAndMemberNameExist(string name, object nameCollection)
        {
            object[] objArray = (object[]) nameCollection;
            return (DoesTypeNameExists(name, objArray[0]) || DoesMemberNameExist(name, objArray[1]));
        }

        internal static bool DoesTypeNameExists(string name, object codeTypeDeclarationCollectionObject)
        {
            CodeTypeDeclarationCollection declarations = (CodeTypeDeclarationCollection) codeTypeDeclarationCollectionObject;
            foreach (CodeTypeDeclaration declaration in declarations)
            {
                if (string.Compare(declaration.Name, name, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return true;
                }
            }
            return false;
        }

        private static CodeMemberMethod GenerateClientClassMethod(CodeTypeDeclaration clientType, CodeTypeReference contractTypeRef, CodeMemberMethod method, bool addHelperMethod, CodeTypeReference declaringContractTypeRef)
        {
            CodeMemberMethod implementationOfMethod = GetImplementationOfMethod(contractTypeRef, method);
            AddMethodImpl(implementationOfMethod);
            int num = clientType.Members.Add(implementationOfMethod);
            CodeMemberMethod method3 = null;
            if (addHelperMethod)
            {
                method3 = GenerateHelperMethod(declaringContractTypeRef, implementationOfMethod);
                if (method3 != null)
                {
                    clientType.Members[num].CustomAttributes.Add(CreateEditorBrowsableAttribute(EditorBrowsableState.Advanced));
                    clientType.Members.Add(method3);
                }
            }
            if (method3 == null)
            {
                return implementationOfMethod;
            }
            return method3;
        }

        private static void GenerateEventAsyncMethods(ServiceContractGenerationContext context, CodeTypeDeclaration clientType, string syncMethodName, CodeMemberMethod beginMethod, CodeMemberMethod endMethod)
        {
            CodeTypeDeclaration operationCompletedEventArgsType = CreateOperationCompletedEventArgsType(context, syncMethodName, endMethod);
            CodeMemberEvent operationCompletedEvent = CreateOperationCompletedEvent(context, clientType, syncMethodName, operationCompletedEventArgsType);
            CodeMemberField beginOperationDelegate = CreateBeginOperationDelegate(context, clientType, syncMethodName);
            CodeMemberMethod beginOperationMethod = CreateBeginOperationMethod(context, clientType, syncMethodName, beginMethod);
            CodeMemberField endOperationDelegate = CreateEndOperationDelegate(context, clientType, syncMethodName);
            CodeMemberMethod endOperationMethod = CreateEndOperationMethod(context, clientType, syncMethodName, endMethod);
            CodeMemberField operationCompletedDelegate = CreateOperationCompletedDelegate(context, clientType, syncMethodName);
            CodeMemberMethod operationCompletedMethod = CreateOperationCompletedMethod(context, clientType, syncMethodName, operationCompletedEventArgsType, operationCompletedEvent);
            CodeMemberMethod eventAsyncMethod = CreateEventAsyncMethod(context, clientType, syncMethodName, beginMethod, beginOperationDelegate, beginOperationMethod, endOperationDelegate, endOperationMethod, operationCompletedDelegate, operationCompletedMethod);
            CreateEventAsyncMethodOverload(clientType, eventAsyncMethod);
            beginMethod.CustomAttributes.Add(CreateEditorBrowsableAttribute(EditorBrowsableState.Advanced));
            endMethod.CustomAttributes.Add(CreateEditorBrowsableAttribute(EditorBrowsableState.Advanced));
        }

        private static CodeMemberMethod GenerateHelperMethod(CodeTypeReference ifaceType, CodeMemberMethod method)
        {
            CodeMemberMethod helperMethod = new CodeMemberMethod {
                Name = method.Name,
                Attributes = MemberAttributes.Public | MemberAttributes.Final
            };
            CodeMethodInvokeExpression expression = new CodeMethodInvokeExpression(new CodeMethodReferenceExpression(new CodeCastExpression(ifaceType, new CodeThisReferenceExpression()), method.Name), new CodeExpression[0]);
            bool flag = false;
            foreach (CodeParameterDeclarationExpression expression2 in method.Parameters)
            {
                CodeTypeDeclaration codeType = ServiceContractGenerator.NamespaceHelper.GetCodeType(expression2.Type);
                if (codeType != null)
                {
                    flag = true;
                    CodeVariableReferenceExpression expression3 = new CodeVariableReferenceExpression("inValue");
                    helperMethod.Statements.Add(new CodeVariableDeclarationStatement(expression2.Type, expression3.VariableName, new CodeObjectCreateExpression(expression2.Type, new CodeExpression[0])));
                    expression.Parameters.Add(expression3);
                    GenerateParameters(helperMethod, codeType, expression3, FieldDirection.In);
                }
                else
                {
                    helperMethod.Parameters.Add(new CodeParameterDeclarationExpression(expression2.Type, expression2.Name));
                    expression.Parameters.Add(new CodeArgumentReferenceExpression(expression2.Name));
                }
            }
            if (method.ReturnType.BaseType == voidTypeRef.BaseType)
            {
                helperMethod.Statements.Add(expression);
            }
            else
            {
                CodeTypeDeclaration codeTypeDeclaration = ServiceContractGenerator.NamespaceHelper.GetCodeType(method.ReturnType);
                if (codeTypeDeclaration != null)
                {
                    flag = true;
                    CodeVariableReferenceExpression target = new CodeVariableReferenceExpression("retVal");
                    helperMethod.Statements.Add(new CodeVariableDeclarationStatement(method.ReturnType, target.VariableName, expression));
                    CodeMethodReturnStatement statement = GenerateParameters(helperMethod, codeTypeDeclaration, target, FieldDirection.Out);
                    if (statement != null)
                    {
                        helperMethod.Statements.Add(statement);
                    }
                }
                else
                {
                    helperMethod.Statements.Add(new CodeMethodReturnStatement(expression));
                    helperMethod.ReturnType = method.ReturnType;
                }
            }
            if (flag)
            {
                method.PrivateImplementationType = ifaceType;
            }
            if (!flag)
            {
                return null;
            }
            return helperMethod;
        }

        private static CodeMethodReturnStatement GenerateParameters(CodeMemberMethod helperMethod, CodeTypeDeclaration codeTypeDeclaration, CodeExpression target, FieldDirection dir)
        {
            CodeMethodReturnStatement statement = null;
            foreach (CodeTypeMember member in codeTypeDeclaration.Members)
            {
                CodeMemberField field = member as CodeMemberField;
                if (field != null)
                {
                    CodeFieldReferenceExpression left = new CodeFieldReferenceExpression(target, field.Name);
                    CodeTypeDeclaration codeType = ServiceContractGenerator.NamespaceHelper.GetCodeType(field.Type);
                    if (codeType != null)
                    {
                        if (dir == FieldDirection.In)
                        {
                            helperMethod.Statements.Add(new CodeAssignStatement(left, new CodeObjectCreateExpression(field.Type, new CodeExpression[0])));
                        }
                        statement = GenerateParameters(helperMethod, codeType, left, dir);
                    }
                    else
                    {
                        CodeParameterDeclarationExpression expression2 = GetRefParameter(helperMethod.Parameters, dir, field);
                        if (((expression2 == null) && (dir == FieldDirection.Out)) && (helperMethod.ReturnType.BaseType == voidTypeRef.BaseType))
                        {
                            helperMethod.ReturnType = field.Type;
                            statement = new CodeMethodReturnStatement(left);
                        }
                        else
                        {
                            if (expression2 == null)
                            {
                                expression2 = new CodeParameterDeclarationExpression(field.Type, NamingHelper.GetUniqueName(field.Name, new NamingHelper.DoesNameExist(ClientClassGenerator.DoesParameterNameExist), helperMethod)) {
                                    Direction = dir
                                };
                                helperMethod.Parameters.Add(expression2);
                            }
                            if (dir == FieldDirection.Out)
                            {
                                helperMethod.Statements.Add(new CodeAssignStatement(new CodeArgumentReferenceExpression(expression2.Name), left));
                            }
                            else
                            {
                                helperMethod.Statements.Add(new CodeAssignStatement(left, new CodeArgumentReferenceExpression(expression2.Name)));
                            }
                        }
                    }
                }
            }
            return statement;
        }

        private static int GetAsyncResultParamIndex(CodeMemberMethod endMethod)
        {
            int num = endMethod.Parameters.Count - 1;
            if (endMethod.Parameters[num].Type.BaseType != asyncResultTypeRef.BaseType)
            {
                num = 0;
            }
            return num;
        }

        private static string GetBeginOperationDelegateName(string syncMethodName)
        {
            return string.Format(CultureInfo.InvariantCulture, "onBegin{0}Delegate", new object[] { syncMethodName });
        }

        private static string GetBeginOperationMethodName(string syncMethodName)
        {
            return string.Format(CultureInfo.InvariantCulture, "OnBegin{0}", new object[] { syncMethodName });
        }

        private static CodeExpression GetChannelReference()
        {
            return new CodePropertyReferenceExpression(new CodeBaseReferenceExpression(), "Channel");
        }

        private static string GetClassName(string interfaceName)
        {
            if (((interfaceName.Length >= 2) && (string.Compare(interfaceName, 0, "I", 0, "I".Length, StringComparison.Ordinal) == 0)) && char.IsUpper(interfaceName, 1))
            {
                return interfaceName.Substring(1);
            }
            return interfaceName;
        }

        internal static string GetClientClassName(string interfaceName)
        {
            return (GetClassName(interfaceName) + "Client");
        }

        private static string GetEndOperationDelegateName(string syncMethodName)
        {
            return string.Format(CultureInfo.InvariantCulture, "onEnd{0}Delegate", new object[] { syncMethodName });
        }

        private static string GetEndOperationMethodName(string syncMethodName)
        {
            return string.Format(CultureInfo.InvariantCulture, "OnEnd{0}", new object[] { syncMethodName });
        }

        private static string GetEventAsyncMethodName(string syncMethodName)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}Async", new object[] { syncMethodName });
        }

        private static CodeMemberMethod GetImplementationOfMethod(CodeTypeReference ifaceType, CodeMemberMethod method)
        {
            CodeMemberMethod method2 = new CodeMemberMethod {
                Name = method.Name
            };
            method2.ImplementationTypes.Add(ifaceType);
            method2.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            foreach (CodeParameterDeclarationExpression expression in method.Parameters)
            {
                CodeParameterDeclarationExpression expression2 = new CodeParameterDeclarationExpression(expression.Type, expression.Name) {
                    Direction = expression.Direction
                };
                method2.Parameters.Add(expression2);
            }
            method2.ReturnType = method.ReturnType;
            return method2;
        }

        private static string GetOperationCompletedDelegateName(string syncMethodName)
        {
            return string.Format(CultureInfo.InvariantCulture, "on{0}CompletedDelegate", new object[] { syncMethodName });
        }

        private static string GetOperationCompletedEventArgsTypeName(string syncMethodName)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}CompletedEventArgs", new object[] { syncMethodName });
        }

        private static string GetOperationCompletedEventName(string syncMethodName)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}Completed", new object[] { syncMethodName });
        }

        private static string GetOperationCompletedMethodName(string syncMethodName)
        {
            return string.Format(CultureInfo.InvariantCulture, "On{0}Completed", new object[] { syncMethodName });
        }

        private static CodeParameterDeclarationExpression GetRefParameter(CodeParameterDeclarationExpressionCollection parameters, FieldDirection dir, CodeMemberField field)
        {
            foreach (CodeParameterDeclarationExpression expression in parameters)
            {
                if (expression.Name == field.Name)
                {
                    if ((expression.Direction != dir) && (expression.Type.BaseType == field.Type.BaseType))
                    {
                        expression.Direction = FieldDirection.Ref;
                        return expression;
                    }
                    return null;
                }
            }
            return null;
        }

        private static bool IsVoid(CodeMemberMethod method)
        {
            if (method.ReturnType != null)
            {
                return (string.Compare(method.ReturnType.BaseType, typeof(void).FullName, StringComparison.Ordinal) == 0);
            }
            return true;
        }

        void IServiceContractGenerationExtension.GenerateContract(ServiceContractGenerationContext context)
        {
            CodeTypeDeclaration clientType = context.TypeFactory.CreateClassType();
            clientType.Name = NamingHelper.GetUniqueName(GetClientClassName(context.ContractType.Name), new NamingHelper.DoesNameExist(ClientClassGenerator.DoesMethodNameExist), context.Operations);
            CodeTypeReference contractTypeReference = context.ContractTypeReference;
            if (context.DuplexCallbackType == null)
            {
                clientType.BaseTypes.Add(new CodeTypeReference(context.ServiceContractGenerator.GetCodeTypeReference(typeof(ClientBase<>)).BaseType, new CodeTypeReference[] { context.ContractTypeReference }));
            }
            else
            {
                clientType.BaseTypes.Add(new CodeTypeReference(context.ServiceContractGenerator.GetCodeTypeReference(typeof(DuplexClientBase<>)).BaseType, new CodeTypeReference[] { context.ContractTypeReference }));
            }
            clientType.BaseTypes.Add(context.ContractTypeReference);
            if (ClientCtorParamNames.Length != ClientCtorParamTypes.Length)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Invalid client generation constructor table initialization", new object[0])));
            }
            for (int i = 0; i < ClientCtorParamNames.Length; i++)
            {
                if (ClientCtorParamNames[i].Length != ClientCtorParamTypes[i].Length)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Invalid client generation constructor table initialization", new object[0])));
                }
                CodeConstructor constructor = new CodeConstructor {
                    Attributes = MemberAttributes.Public
                };
                if (context.DuplexCallbackType != null)
                {
                    constructor.Parameters.Add(new CodeParameterDeclarationExpression(typeof(InstanceContext), inputInstanceName));
                    constructor.BaseConstructorArgs.Add(new CodeVariableReferenceExpression(inputInstanceName));
                }
                for (int j = 0; j < ClientCtorParamNames[i].Length; j++)
                {
                    constructor.Parameters.Add(new CodeParameterDeclarationExpression(ClientCtorParamTypes[i][j], ClientCtorParamNames[i][j]));
                    constructor.BaseConstructorArgs.Add(new CodeVariableReferenceExpression(ClientCtorParamNames[i][j]));
                }
                clientType.Members.Add(constructor);
            }
            foreach (OperationContractGenerationContext context2 in context.Operations)
            {
                if (!context2.Operation.IsServerInitiated())
                {
                    CodeTypeReference declaringTypeReference = context2.DeclaringTypeReference;
                    GenerateClientClassMethod(clientType, contractTypeReference, context2.SyncMethod, this.tryAddHelperMethod, declaringTypeReference);
                    if (context2.IsAsync)
                    {
                        CodeMemberMethod beginMethod = GenerateClientClassMethod(clientType, contractTypeReference, context2.BeginMethod, this.tryAddHelperMethod, declaringTypeReference);
                        CodeMemberMethod endMethod = GenerateClientClassMethod(clientType, contractTypeReference, context2.EndMethod, this.tryAddHelperMethod, declaringTypeReference);
                        if (this.generateEventAsyncMethods)
                        {
                            GenerateEventAsyncMethods(context, clientType, context2.SyncMethod.Name, beginMethod, endMethod);
                        }
                    }
                }
            }
            context.Namespace.Types.Add(clientType);
            context.ClientType = clientType;
            context.ClientTypeReference = ServiceContractGenerator.NamespaceHelper.GetCodeTypeReference(context.Namespace, clientType);
        }

        private static class Strings
        {
            public const string ClientBaseChannelProperty = "Channel";
            public const string ClientTypeSuffix = "Client";
            public const string InterfaceTypePrefix = "I";
        }
    }
}

