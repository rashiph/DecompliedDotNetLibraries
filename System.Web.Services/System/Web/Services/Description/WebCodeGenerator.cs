namespace System.Web.Services.Description
{
    using System;
    using System.CodeDom;
    using System.CodeDom.Compiler;
    using System.Collections;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Reflection;
    using System.Threading;
    using System.Web.Services;
    using System.Web.Services.Protocols;
    using System.Xml.Serialization;

    internal class WebCodeGenerator
    {
        private static CodeAttributeDeclaration generatedCodeAttribute;

        private WebCodeGenerator()
        {
        }

        internal static CodeMemberMethod AddAsyncMethod(CodeTypeDeclaration codeClass, string methodName, string[] parameterTypeNames, string[] parameterNames, string callbackMember, string callbackName, string userState)
        {
            CodeMemberMethod method = AddMethod(codeClass, methodName, new CodeFlags[parameterNames.Length], parameterTypeNames, parameterNames, typeof(void).FullName, null, CodeFlags.IsPublic);
            method.Comments.Add(new CodeCommentStatement(Res.GetString("CodeRemarks"), true));
            CodeMethodInvokeExpression expression = new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), methodName, new CodeExpression[0]);
            for (int i = 0; i < parameterNames.Length; i++)
            {
                expression.Parameters.Add(new CodeArgumentReferenceExpression(parameterNames[i]));
            }
            expression.Parameters.Add(new CodePrimitiveExpression(null));
            method.Statements.Add(expression);
            method = AddMethod(codeClass, methodName, new CodeFlags[parameterNames.Length], parameterTypeNames, parameterNames, typeof(void).FullName, null, CodeFlags.IsPublic);
            method.Comments.Add(new CodeCommentStatement(Res.GetString("CodeRemarks"), true));
            method.Parameters.Add(new CodeParameterDeclarationExpression(typeof(object), userState));
            CodeFieldReferenceExpression left = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), callbackMember);
            CodeBinaryOperatorExpression condition = new CodeBinaryOperatorExpression(left, CodeBinaryOperatorType.IdentityEquality, new CodePrimitiveExpression(null));
            CodeDelegateCreateExpression right = new CodeDelegateCreateExpression {
                DelegateType = new CodeTypeReference(typeof(SendOrPostCallback)),
                TargetObject = new CodeThisReferenceExpression(),
                MethodName = callbackName
            };
            CodeStatement[] trueStatements = new CodeStatement[] { new CodeAssignStatement(left, right) };
            method.Statements.Add(new CodeConditionStatement(condition, trueStatements, new CodeStatement[0]));
            return method;
        }

        internal static void AddCallbackDeclaration(CodeTypeMemberCollection members, string callbackMember)
        {
            CodeMemberField field = new CodeMemberField {
                Type = new CodeTypeReference(typeof(SendOrPostCallback)),
                Name = callbackMember
            };
            members.Add(field);
        }

        internal static void AddCallbackImplementation(CodeTypeDeclaration codeClass, string callbackName, string handlerName, string handlerArgs, bool methodHasOutParameters)
        {
            CodeFlags[] parameterFlags = new CodeFlags[1];
            CodeMemberMethod method = AddMethod(codeClass, callbackName, parameterFlags, new string[] { typeof(object).FullName }, new string[] { "arg" }, typeof(void).FullName, null, (CodeFlags) 0);
            CodeEventReferenceExpression left = new CodeEventReferenceExpression(new CodeThisReferenceExpression(), handlerName);
            CodeBinaryOperatorExpression condition = new CodeBinaryOperatorExpression(left, CodeBinaryOperatorType.IdentityInequality, new CodePrimitiveExpression(null));
            CodeStatement[] trueStatements = new CodeStatement[2];
            trueStatements[0] = new CodeVariableDeclarationStatement(typeof(InvokeCompletedEventArgs), "invokeArgs", new CodeCastExpression(typeof(InvokeCompletedEventArgs), new CodeArgumentReferenceExpression("arg")));
            CodeVariableReferenceExpression targetObject = new CodeVariableReferenceExpression("invokeArgs");
            CodeObjectCreateExpression expression4 = new CodeObjectCreateExpression();
            if (methodHasOutParameters)
            {
                expression4.CreateType = new CodeTypeReference(handlerArgs);
                expression4.Parameters.Add(new CodePropertyReferenceExpression(targetObject, "Results"));
            }
            else
            {
                expression4.CreateType = new CodeTypeReference(typeof(AsyncCompletedEventArgs));
            }
            expression4.Parameters.Add(new CodePropertyReferenceExpression(targetObject, "Error"));
            expression4.Parameters.Add(new CodePropertyReferenceExpression(targetObject, "Cancelled"));
            expression4.Parameters.Add(new CodePropertyReferenceExpression(targetObject, "UserState"));
            trueStatements[1] = new CodeExpressionStatement(new CodeDelegateInvokeExpression(new CodeEventReferenceExpression(new CodeThisReferenceExpression(), handlerName), new CodeExpression[] { new CodeThisReferenceExpression(), expression4 }));
            method.Statements.Add(new CodeConditionStatement(condition, trueStatements, new CodeStatement[0]));
        }

        internal static CodeTypeDeclaration AddClass(CodeNamespace codeNamespace, string className, string baseClassName, string[] implementedInterfaceNames, CodeAttributeDeclarationCollection metadata, CodeFlags flags, bool isPartial)
        {
            CodeTypeDeclaration declaration = CreateClass(className, baseClassName, implementedInterfaceNames, metadata, flags, isPartial);
            codeNamespace.Types.Add(declaration);
            return declaration;
        }

        internal static CodeConstructor AddConstructor(CodeTypeDeclaration codeClass, string[] parameterTypeNames, string[] parameterNames, CodeAttributeDeclarationCollection metadata, CodeFlags flags)
        {
            CodeConstructor constructor = new CodeConstructor();
            if ((flags & CodeFlags.IsPublic) != ((CodeFlags) 0))
            {
                constructor.Attributes = (constructor.Attributes & ~MemberAttributes.AccessMask) | MemberAttributes.Public;
            }
            if ((flags & CodeFlags.IsAbstract) != ((CodeFlags) 0))
            {
                constructor.Attributes |= MemberAttributes.Abstract;
            }
            constructor.CustomAttributes = metadata;
            for (int i = 0; i < parameterTypeNames.Length; i++)
            {
                CodeParameterDeclarationExpression expression = new CodeParameterDeclarationExpression(parameterTypeNames[i], parameterNames[i]);
                constructor.Parameters.Add(expression);
            }
            codeClass.Members.Add(constructor);
            return constructor;
        }

        internal static CodeAttributeDeclarationCollection AddCustomAttribute(CodeAttributeDeclarationCollection metadata, Type type, CodeAttributeArgument[] arguments)
        {
            if (metadata == null)
            {
                metadata = new CodeAttributeDeclarationCollection();
            }
            CodeAttributeDeclaration declaration = new CodeAttributeDeclaration(type.FullName, arguments);
            metadata.Add(declaration);
            return metadata;
        }

        internal static CodeAttributeDeclarationCollection AddCustomAttribute(CodeAttributeDeclarationCollection metadata, Type type, CodeExpression[] arguments)
        {
            return AddCustomAttribute(metadata, type, arguments, new string[0], new CodeExpression[0]);
        }

        internal static CodeAttributeDeclarationCollection AddCustomAttribute(CodeAttributeDeclarationCollection metadata, Type type, CodeExpression[] parameters, string[] propNames, CodeExpression[] propValues)
        {
            int num = ((parameters == null) ? 0 : parameters.Length) + ((propNames == null) ? 0 : propNames.Length);
            CodeAttributeArgument[] arguments = new CodeAttributeArgument[num];
            for (int i = 0; i < parameters.Length; i++)
            {
                arguments[i] = new CodeAttributeArgument(null, parameters[i]);
            }
            for (int j = 0; j < propNames.Length; j++)
            {
                arguments[parameters.Length + j] = new CodeAttributeArgument(propNames[j], propValues[j]);
            }
            return AddCustomAttribute(metadata, type, arguments);
        }

        internal static void AddDelegate(CodeTypeDeclarationCollection codeClasses, string handlerType, string handlerArgs)
        {
            CodeTypeDelegate delegate2 = new CodeTypeDelegate(handlerType);
            delegate2.CustomAttributes.Add(GeneratedCodeAttribute);
            delegate2.Parameters.Add(new CodeParameterDeclarationExpression(typeof(object), "sender"));
            delegate2.Parameters.Add(new CodeParameterDeclarationExpression(handlerArgs, "e"));
            delegate2.Comments.Add(new CodeCommentStatement(Res.GetString("CodeRemarks"), true));
            codeClasses.Add(delegate2);
        }

        internal static void AddEvent(CodeTypeMemberCollection members, string handlerType, string handlerName)
        {
            CodeMemberEvent event2;
            event2 = new CodeMemberEvent {
                Type = new CodeTypeReference(handlerType),
                Name = handlerName,
                Attributes = (event2.Attributes & ~MemberAttributes.AccessMask) | MemberAttributes.Public
            };
            event2.Comments.Add(new CodeCommentStatement(Res.GetString("CodeRemarks"), true));
            members.Add(event2);
        }

        internal static void AddImports(CodeNamespace codeNamespace, string[] namespaces)
        {
            foreach (string str in namespaces)
            {
                codeNamespace.Imports.Add(new CodeNamespaceImport(str));
            }
        }

        internal static CodeTypeMember AddMember(CodeTypeDeclaration codeClass, string typeName, string memberName, CodeExpression initializer, CodeAttributeDeclarationCollection metadata, CodeFlags flags, CodeGenerationOptions options)
        {
            CodeTypeMember member;
            bool flag = (options & CodeGenerationOptions.GenerateProperties) != CodeGenerationOptions.None;
            string name = flag ? MakeFieldName(memberName) : memberName;
            CodeMemberField field = new CodeMemberField(typeName, name) {
                InitExpression = initializer
            };
            if (flag)
            {
                codeClass.Members.Add(field);
                member = CreatePropertyDeclaration(field, memberName, typeName);
            }
            else
            {
                member = field;
            }
            member.CustomAttributes = metadata;
            if ((flags & CodeFlags.IsPublic) != ((CodeFlags) 0))
            {
                member.Attributes = (field.Attributes & ~MemberAttributes.AccessMask) | MemberAttributes.Public;
            }
            codeClass.Members.Add(member);
            return member;
        }

        internal static CodeMemberMethod AddMethod(CodeTypeDeclaration codeClass, string methodName, CodeFlags[] parameterFlags, string[] parameterTypeNames, string[] parameterNames, string returnTypeName, CodeAttributeDeclarationCollection metadata, CodeFlags flags)
        {
            return AddMethod(codeClass, methodName, parameterFlags, parameterTypeNames, parameterNames, new CodeAttributeDeclarationCollection[0], returnTypeName, metadata, flags);
        }

        internal static CodeMemberMethod AddMethod(CodeTypeDeclaration codeClass, string methodName, CodeFlags[] parameterFlags, string[] parameterTypeNames, string[] parameterNames, CodeAttributeDeclarationCollection[] parameterAttributes, string returnTypeName, CodeAttributeDeclarationCollection metadata, CodeFlags flags)
        {
            CodeMemberMethod method = new CodeMemberMethod {
                Name = methodName,
                ReturnType = new CodeTypeReference(returnTypeName),
                CustomAttributes = metadata
            };
            if ((flags & CodeFlags.IsPublic) != ((CodeFlags) 0))
            {
                method.Attributes = (method.Attributes & ~MemberAttributes.AccessMask) | MemberAttributes.Public;
            }
            if ((flags & CodeFlags.IsAbstract) != ((CodeFlags) 0))
            {
                method.Attributes = (method.Attributes & ~MemberAttributes.ScopeMask) | MemberAttributes.Abstract;
            }
            if ((flags & CodeFlags.IsNew) != ((CodeFlags) 0))
            {
                method.Attributes = (method.Attributes & ~MemberAttributes.VTableMask) | MemberAttributes.New;
            }
            for (int i = 0; i < parameterNames.Length; i++)
            {
                CodeParameterDeclarationExpression expression = new CodeParameterDeclarationExpression(parameterTypeNames[i], parameterNames[i]);
                if ((parameterFlags[i] & CodeFlags.IsByRef) != ((CodeFlags) 0))
                {
                    expression.Direction = FieldDirection.Ref;
                }
                else if ((parameterFlags[i] & CodeFlags.IsOut) != ((CodeFlags) 0))
                {
                    expression.Direction = FieldDirection.Out;
                }
                if (i < parameterAttributes.Length)
                {
                    expression.CustomAttributes = parameterAttributes[i];
                }
                method.Parameters.Add(expression);
            }
            codeClass.Members.Add(method);
            return method;
        }

        internal static CodeTypeDeclaration CreateArgsClass(string name, string[] paramTypes, string[] paramNames, bool isPartial)
        {
            CodeConstructor constructor;
            CodeTypeDeclaration declaration = new CodeTypeDeclaration(name);
            declaration.CustomAttributes.Add(GeneratedCodeAttribute);
            declaration.CustomAttributes.Add(new CodeAttributeDeclaration(typeof(DebuggerStepThroughAttribute).FullName));
            declaration.CustomAttributes.Add(new CodeAttributeDeclaration(typeof(DesignerCategoryAttribute).FullName, new CodeAttributeArgument[] { new CodeAttributeArgument(new CodePrimitiveExpression("code")) }));
            declaration.IsPartial = isPartial;
            declaration.BaseTypes.Add(new CodeTypeReference(typeof(AsyncCompletedEventArgs)));
            CodeIdentifiers identifiers = new CodeIdentifiers();
            identifiers.AddUnique("Error", "Error");
            identifiers.AddUnique("Cancelled", "Cancelled");
            identifiers.AddUnique("UserState", "UserState");
            for (int i = 0; i < paramNames.Length; i++)
            {
                if (paramNames[i] != null)
                {
                    identifiers.AddUnique(paramNames[i], paramNames[i]);
                }
            }
            string str = identifiers.AddUnique("results", "results");
            CodeMemberField field = new CodeMemberField(typeof(object[]), str);
            declaration.Members.Add(field);
            constructor = new CodeConstructor {
                Attributes = (constructor.Attributes & ~MemberAttributes.AccessMask) | MemberAttributes.Assembly
            };
            CodeParameterDeclarationExpression expression = new CodeParameterDeclarationExpression(typeof(object[]), str);
            constructor.Parameters.Add(expression);
            constructor.Parameters.Add(new CodeParameterDeclarationExpression(typeof(Exception), "exception"));
            constructor.Parameters.Add(new CodeParameterDeclarationExpression(typeof(bool), "cancelled"));
            constructor.Parameters.Add(new CodeParameterDeclarationExpression(typeof(object), "userState"));
            constructor.BaseConstructorArgs.Add(new CodeArgumentReferenceExpression("exception"));
            constructor.BaseConstructorArgs.Add(new CodeArgumentReferenceExpression("cancelled"));
            constructor.BaseConstructorArgs.Add(new CodeArgumentReferenceExpression("userState"));
            constructor.Statements.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), field.Name), new CodeArgumentReferenceExpression(str)));
            declaration.Members.Add(constructor);
            int num2 = 0;
            for (int j = 0; j < paramNames.Length; j++)
            {
                if (paramNames[j] != null)
                {
                    declaration.Members.Add(CreatePropertyDeclaration(field, paramNames[j], paramTypes[j], num2++));
                }
            }
            declaration.Comments.Add(new CodeCommentStatement(Res.GetString("CodeRemarks"), true));
            return declaration;
        }

        internal static CodeTypeDeclaration CreateClass(string className, string baseClassName, string[] implementedInterfaceNames, CodeAttributeDeclarationCollection metadata, CodeFlags flags, bool isPartial)
        {
            CodeTypeDeclaration declaration = new CodeTypeDeclaration(className);
            if ((baseClassName != null) && (baseClassName.Length > 0))
            {
                declaration.BaseTypes.Add(baseClassName);
            }
            foreach (string str in implementedInterfaceNames)
            {
                declaration.BaseTypes.Add(str);
            }
            declaration.IsStruct = (flags & CodeFlags.IsStruct) != ((CodeFlags) 0);
            if ((flags & CodeFlags.IsPublic) != ((CodeFlags) 0))
            {
                declaration.TypeAttributes |= TypeAttributes.Public;
            }
            else
            {
                declaration.TypeAttributes &= ~TypeAttributes.Public;
            }
            if ((flags & CodeFlags.IsAbstract) != ((CodeFlags) 0))
            {
                declaration.TypeAttributes |= TypeAttributes.Abstract;
            }
            else
            {
                declaration.TypeAttributes &= ~TypeAttributes.Abstract;
            }
            if ((flags & CodeFlags.IsInterface) != ((CodeFlags) 0))
            {
                declaration.IsInterface = true;
            }
            else
            {
                declaration.IsPartial = isPartial;
            }
            declaration.CustomAttributes = metadata;
            declaration.CustomAttributes.Add(GeneratedCodeAttribute);
            return declaration;
        }

        private static CodeMemberProperty CreatePropertyDeclaration(CodeMemberField field, string name, string typeName)
        {
            CodeMemberProperty property = new CodeMemberProperty {
                Type = new CodeTypeReference(typeName),
                Name = name
            };
            CodeMethodReturnStatement statement = new CodeMethodReturnStatement {
                Expression = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), field.Name)
            };
            property.GetStatements.Add(statement);
            CodeExpression left = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), field.Name);
            CodeExpression right = new CodeArgumentReferenceExpression("value");
            property.SetStatements.Add(new CodeAssignStatement(left, right));
            return property;
        }

        private static CodeMemberProperty CreatePropertyDeclaration(CodeMemberField field, string name, string typeName, int index)
        {
            CodeMemberProperty property;
            property = new CodeMemberProperty {
                Type = new CodeTypeReference(typeName),
                Name = name,
                Attributes = (property.Attributes & ~MemberAttributes.AccessMask) | MemberAttributes.Public
            };
            property.GetStatements.Add(new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "RaiseExceptionIfNecessary", new CodeExpression[0]));
            CodeArrayIndexerExpression expression = new CodeArrayIndexerExpression {
                TargetObject = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), field.Name)
            };
            expression.Indices.Add(new CodePrimitiveExpression(index));
            CodeMethodReturnStatement statement = new CodeMethodReturnStatement {
                Expression = new CodeCastExpression(typeName, expression)
            };
            property.GetStatements.Add(statement);
            property.Comments.Add(new CodeCommentStatement(Res.GetString("CodeRemarks"), true));
            return property;
        }

        internal static string FullTypeName(XmlMemberMapping mapping, CodeDomProvider codeProvider)
        {
            return mapping.GenerateTypeName(codeProvider);
        }

        internal static string[] GetNamespacesForTypes(Type[] types)
        {
            Hashtable hashtable = new Hashtable();
            for (int i = 0; i < types.Length; i++)
            {
                string fullName = types[i].FullName;
                int length = fullName.LastIndexOf('.');
                if (length > 0)
                {
                    hashtable[fullName.Substring(0, length)] = types[i];
                }
            }
            string[] array = new string[hashtable.Keys.Count];
            hashtable.Keys.CopyTo(array, 0);
            return array;
        }

        private static string GetProductVersion(Assembly assembly)
        {
            object[] customAttributes = assembly.GetCustomAttributes(true);
            for (int i = 0; i < customAttributes.Length; i++)
            {
                if (customAttributes[i] is AssemblyInformationalVersionAttribute)
                {
                    AssemblyInformationalVersionAttribute attribute = (AssemblyInformationalVersionAttribute) customAttributes[i];
                    return attribute.InformationalVersion;
                }
            }
            return null;
        }

        private static string MakeFieldName(string name)
        {
            return (CodeIdentifier.MakeCamel(name) + "Field");
        }

        internal static CodeAttributeDeclaration GeneratedCodeAttribute
        {
            get
            {
                if (generatedCodeAttribute == null)
                {
                    CodeAttributeDeclaration declaration = new CodeAttributeDeclaration(typeof(System.CodeDom.Compiler.GeneratedCodeAttribute).FullName);
                    Assembly entryAssembly = Assembly.GetEntryAssembly();
                    if (entryAssembly == null)
                    {
                        entryAssembly = Assembly.GetExecutingAssembly();
                        if (entryAssembly == null)
                        {
                            entryAssembly = typeof(WebCodeGenerator).Assembly;
                        }
                    }
                    AssemblyName name = entryAssembly.GetName();
                    declaration.Arguments.Add(new CodeAttributeArgument(new CodePrimitiveExpression(name.Name)));
                    string productVersion = GetProductVersion(entryAssembly);
                    declaration.Arguments.Add(new CodeAttributeArgument(new CodePrimitiveExpression((productVersion == null) ? name.Version.ToString() : productVersion)));
                    generatedCodeAttribute = declaration;
                }
                return generatedCodeAttribute;
            }
        }
    }
}

