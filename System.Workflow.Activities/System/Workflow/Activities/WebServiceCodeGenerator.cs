namespace System.Workflow.Activities
{
    using System;
    using System.CodeDom;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Workflow.Activities.Common;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Compiler;

    internal sealed class WebServiceCodeGenerator : ActivityCodeGenerator
    {
        private CodeTypeDeclaration CreateOrGetServiceDeclaration(Activity rootActivity, CodeNamespaceCollection codeNamespaceCollection)
        {
            string namespaceName = "";
            CodeNamespace webServiceCodeNamespace = null;
            string fullName = rootActivity.GetType().FullName;
            CodeTypeDeclaration webserviceCodeTypeDeclaration = null;
            if (rootActivity.GetType().FullName.IndexOf(".") != -1)
            {
                namespaceName = rootActivity.GetType().FullName.Substring(0, rootActivity.GetType().FullName.LastIndexOf('.'));
            }
            foreach (CodeNamespace namespace3 in codeNamespaceCollection)
            {
                if (namespace3.Name == namespaceName)
                {
                    webServiceCodeNamespace = namespace3;
                    break;
                }
            }
            if (webServiceCodeNamespace == null)
            {
                webServiceCodeNamespace = this.GetWebServiceCodeNamespace(namespaceName);
                codeNamespaceCollection.Add(webServiceCodeNamespace);
            }
            string str3 = fullName.Substring(fullName.LastIndexOf('.') + 1) + "_WebService";
            foreach (CodeTypeDeclaration declaration2 in webServiceCodeNamespace.Types)
            {
                if (declaration2.Name == str3)
                {
                    webserviceCodeTypeDeclaration = declaration2;
                    break;
                }
            }
            if (webserviceCodeTypeDeclaration == null)
            {
                webserviceCodeTypeDeclaration = this.GetWebserviceCodeTypeDeclaration(fullName.Substring(fullName.LastIndexOf('.') + 1));
                webServiceCodeNamespace.Types.Add(webserviceCodeTypeDeclaration);
                webServiceCodeNamespace.Imports.Add(new CodeNamespaceImport("System"));
                webServiceCodeNamespace.Imports.Add(new CodeNamespaceImport("System.Web"));
                webServiceCodeNamespace.Imports.Add(new CodeNamespaceImport("System.Web.Services"));
                webServiceCodeNamespace.Imports.Add(new CodeNamespaceImport("System.Web.Services.Protocols"));
                webServiceCodeNamespace.Imports.Add(new CodeNamespaceImport("System.Workflow.Runtime.Hosting"));
                webServiceCodeNamespace.Imports.Add(new CodeNamespaceImport("System.Workflow.Activities"));
            }
            return webserviceCodeTypeDeclaration;
        }

        public override void GenerateCode(CodeGenerationManager manager, object obj)
        {
            WebServiceInputActivity activity = obj as WebServiceInputActivity;
            if (manager == null)
            {
                throw new ArgumentNullException("manager");
            }
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }
            if (activity != null)
            {
                if (!(manager.GetService(typeof(ITypeProvider)) is ITypeProvider))
                {
                    throw new InvalidOperationException(SR.GetString("General_MissingService", new object[] { typeof(ITypeProvider).FullName }));
                }
                CodeNamespaceCollection codeNamespaceCollection = manager.Context[typeof(CodeNamespaceCollection)] as CodeNamespaceCollection;
                if (codeNamespaceCollection == null)
                {
                    throw new InvalidOperationException(SR.GetString("Error_ContextStackItemMissing", new object[] { typeof(CodeNamespaceCollection).Name }));
                }
                CodeTypeDeclaration declaration = this.CreateOrGetServiceDeclaration(Helpers.GetRootActivity(activity), codeNamespaceCollection);
                if (activity.InterfaceType != null)
                {
                    bool flag = false;
                    MethodInfo interfaceMethod = Helpers.GetInterfaceMethod(activity.InterfaceType, activity.MethodName);
                    System.Workflow.Activities.Common.SupportedLanguages supportedLanguage = System.Workflow.Activities.Common.CompilerHelpers.GetSupportedLanguage(manager);
                    foreach (CodeTypeMember member in declaration.Members)
                    {
                        if ((member is CodeMemberMethod) && (string.Compare(member.Name, interfaceMethod.Name, (supportedLanguage == System.Workflow.Activities.Common.SupportedLanguages.CSharp) ? StringComparison.InvariantCulture : StringComparison.InvariantCultureIgnoreCase) == 0))
                        {
                            flag = true;
                        }
                    }
                    if (!flag)
                    {
                        declaration.Members.Add(this.GetWebServiceMethodDeclaraion(interfaceMethod, activity.IsActivating, supportedLanguage));
                    }
                }
                base.GenerateCode(manager, obj);
            }
        }

        private CodeNamespace GetWebServiceCodeNamespace(string namespaceName)
        {
            return new CodeNamespace(namespaceName);
        }

        private CodeTypeDeclaration GetWebserviceCodeTypeDeclaration(string workflowTypeName)
        {
            CodeTypeDeclaration declaration = new CodeTypeDeclaration(workflowTypeName + "_WebService");
            declaration.BaseTypes.Add(new CodeTypeReference("WorkflowWebService"));
            CodeAttributeDeclaration declaration2 = new CodeAttributeDeclaration("WebServiceBinding", new CodeAttributeArgument[] { new CodeAttributeArgument("ConformsTo", new CodeFieldReferenceExpression(new CodeTypeReferenceExpression("WsiProfiles"), "BasicProfile1_1")), new CodeAttributeArgument("EmitConformanceClaims", new CodePrimitiveExpression(true)) });
            declaration.CustomAttributes.Add(declaration2);
            CodeConstructor constructor = new CodeConstructor {
                Attributes = MemberAttributes.Public
            };
            constructor.BaseConstructorArgs.Add(new CodeTypeOfExpression(workflowTypeName));
            declaration.Members.Add(constructor);
            return declaration;
        }

        private CodeMemberMethod GetWebServiceMethodDeclaraion(MethodInfo methodInfo, bool isActivation, System.Workflow.Activities.Common.SupportedLanguages language)
        {
            CodeMemberMethod method = new CodeMemberMethod {
                Attributes = MemberAttributes.Public,
                ReturnType = new CodeTypeReference(methodInfo.ReturnType),
                Name = methodInfo.Name
            };
            CodeAttributeDeclaration declaration = new CodeAttributeDeclaration("WebMethodAttribute");
            declaration.Arguments.Add(new CodeAttributeArgument("Description", new CodePrimitiveExpression(methodInfo.Name)));
            declaration.Arguments.Add(new CodeAttributeArgument("EnableSession", new CodePrimitiveExpression(false)));
            method.CustomAttributes.Add(declaration);
            List<ParameterInfo> list = new List<ParameterInfo>();
            CodeArrayCreateExpression expression = new CodeArrayCreateExpression {
                CreateType = new CodeTypeReference(typeof(object))
            };
            foreach (ParameterInfo info in methodInfo.GetParameters())
            {
                CodeParameterDeclarationExpression expression2 = new CodeParameterDeclarationExpression();
                if (info.IsOut || info.ParameterType.IsByRef)
                {
                    expression2.Type = new CodeTypeReference(info.ParameterType.GetElementType().FullName);
                    expression2.Direction = info.IsOut ? FieldDirection.Out : FieldDirection.Ref;
                    if ((expression2.Direction == FieldDirection.Out) && (language == System.Workflow.Activities.Common.SupportedLanguages.VB))
                    {
                        expression2.CustomAttributes.Add(new CodeAttributeDeclaration(new CodeTypeReference(typeof(OutAttribute))));
                    }
                    list.Add(info);
                }
                else
                {
                    expression2.Type = new CodeTypeReference(info.ParameterType.FullName);
                }
                expression2.Name = info.Name;
                method.Parameters.Add(expression2);
                if (!info.IsOut)
                {
                    expression.Initializers.Add(new CodeArgumentReferenceExpression(info.Name));
                }
            }
            CodeMethodInvokeExpression expression3 = new CodeMethodInvokeExpression {
                Method = new CodeMethodReferenceExpression(new CodeThisReferenceExpression(), "Invoke")
            };
            expression3.Parameters.Add(new CodeTypeOfExpression(methodInfo.DeclaringType));
            expression3.Parameters.Add(new CodePrimitiveExpression(methodInfo.Name));
            expression3.Parameters.Add(new CodePrimitiveExpression(isActivation));
            expression3.Parameters.Add(expression);
            int num = (methodInfo.ReturnType == typeof(void)) ? 0 : 1;
            if (list.Count != 0)
            {
                CodeVariableDeclarationStatement statement = new CodeVariableDeclarationStatement(new CodeTypeReference(new CodeTypeReference(typeof(object)), 1), "results") {
                    InitExpression = expression3
                };
                method.Statements.Add(statement);
                for (int i = 0; i < list.Count; i++)
                {
                    ParameterInfo info2 = list[i];
                    CodeAssignStatement statement2 = new CodeAssignStatement();
                    CodeExpression expression4 = new CodeArgumentReferenceExpression(info2.Name);
                    CodeExpression expression5 = new CodeCastExpression(new CodeTypeReference(info2.ParameterType.GetElementType().FullName), new CodeIndexerExpression(new CodeVariableReferenceExpression("results"), new CodeExpression[] { new CodePrimitiveExpression(i + num) }));
                    statement2.Left = expression4;
                    statement2.Right = expression5;
                    method.Statements.Add(statement2);
                }
            }
            if (methodInfo.ReturnType != typeof(void))
            {
                CodeExpression expression6;
                if (list.Count != 0)
                {
                    expression6 = new CodeVariableReferenceExpression("results");
                }
                else
                {
                    expression6 = expression3;
                }
                CodeMethodReturnStatement statement3 = new CodeMethodReturnStatement(new CodeCastExpression(methodInfo.ReturnType, new CodeIndexerExpression(expression6, new CodeExpression[] { new CodePrimitiveExpression(0) })));
                method.Statements.Add(statement3);
                return method;
            }
            if ((list.Count == 0) && (methodInfo.ReturnType == typeof(void)))
            {
                method.Statements.Add(expression3);
            }
            return method;
        }
    }
}

