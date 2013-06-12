namespace System.Web.Compilation
{
    using System;
    using System.CodeDom;
    using System.Collections;
    using System.Configuration;
    using System.Reflection;
    using System.Web;
    using System.Web.Configuration;
    using System.Web.Profile;

    internal class ProfileBuildProvider : System.Web.Compilation.BuildProvider
    {
        private const string ProfileTypeName = "ProfileCommon";

        private ProfileBuildProvider()
        {
        }

        private void AddCodeForGetProfileForUser(CodeTypeDeclaration type)
        {
            CodeMemberMethod method = new CodeMemberMethod {
                Name = "GetProfile",
                Attributes = MemberAttributes.Public,
                ReturnType = new CodeTypeReference("ProfileCommon")
            };
            method.Parameters.Add(new CodeParameterDeclarationExpression(typeof(string), "username"));
            CodeMethodInvokeExpression expression = new CodeMethodInvokeExpression {
                Method = { TargetObject = new CodeTypeReferenceExpression("ProfileBase"), MethodName = "Create" }
            };
            expression.Parameters.Add(new CodeArgumentReferenceExpression("username"));
            CodeMethodReturnStatement statement = new CodeMethodReturnStatement(new CodeCastExpression(method.ReturnType, expression));
            MTConfigUtil.GetProfileAppConfig();
            method.Statements.Add(statement);
            type.Members.Add(method);
        }

        private void AddPropertyGroup(AssemblyBuilder assemblyBuilder, string groupName, string propertyNames, Hashtable properties, CodeTypeDeclaration type, CodeNamespace ns)
        {
            CodeMemberProperty property = new CodeMemberProperty {
                Name = groupName,
                Attributes = MemberAttributes.Public,
                HasGet = true,
                Type = new CodeTypeReference("ProfileGroup" + groupName)
            };
            CodeMethodInvokeExpression expression = new CodeMethodInvokeExpression {
                Method = { TargetObject = new CodeThisReferenceExpression(), MethodName = "GetProfileGroup" }
            };
            expression.Parameters.Add(new CodePrimitiveExpression(property.Name));
            CodeMethodReturnStatement statement = new CodeMethodReturnStatement(new CodeCastExpression(property.Type, expression));
            property.GetStatements.Add(statement);
            type.Members.Add(property);
            CodeTypeDeclaration declaration = new CodeTypeDeclaration {
                Name = "ProfileGroup" + groupName
            };
            declaration.BaseTypes.Add(new CodeTypeReference(typeof(ProfileGroupBase)));
            foreach (string str in propertyNames.Split(new char[] { ';' }))
            {
                this.CreateCodeForProperty(assemblyBuilder, declaration, (ProfileNameTypeStruct) properties[str]);
            }
            ns.Types.Add(declaration);
        }

        internal static ProfileBuildProvider Create()
        {
            ProfileBuildProvider provider = new ProfileBuildProvider();
            provider.SetVirtualPath(HttpRuntime.AppDomainAppVirtualPathObject.SimpleCombine("Profile"));
            return provider;
        }

        private void CreateCodeForProperty(AssemblyBuilder assemblyBuilder, CodeTypeDeclaration type, ProfileNameTypeStruct property)
        {
            string name = property.Name;
            int index = name.IndexOf('.');
            if (index > 0)
            {
                name = name.Substring(index + 1);
            }
            if (!assemblyBuilder.CodeDomProvider.IsValidIdentifier(name))
            {
                throw new ConfigurationErrorsException(System.Web.SR.GetString("Profile_bad_name"), property.FileName, property.LineNumber);
            }
            CodeMemberProperty property2 = new CodeMemberProperty {
                Name = name,
                Attributes = MemberAttributes.Public,
                HasGet = true,
                Type = property.PropertyCodeRefType
            };
            CodeMethodInvokeExpression expression = new CodeMethodInvokeExpression {
                Method = { TargetObject = new CodeThisReferenceExpression(), MethodName = "GetPropertyValue" }
            };
            expression.Parameters.Add(new CodePrimitiveExpression(name));
            CodeMethodReturnStatement statement = new CodeMethodReturnStatement(new CodeCastExpression(property2.Type, expression));
            property2.GetStatements.Add(statement);
            if (!property.IsReadOnly)
            {
                CodeMethodInvokeExpression expression2 = new CodeMethodInvokeExpression {
                    Method = { TargetObject = new CodeThisReferenceExpression(), MethodName = "SetPropertyValue" }
                };
                expression2.Parameters.Add(new CodePrimitiveExpression(name));
                expression2.Parameters.Add(new CodePropertySetValueReferenceExpression());
                property2.HasSet = true;
                property2.SetStatements.Add(expression2);
            }
            type.Members.Add(property2);
        }

        public override void GenerateCode(AssemblyBuilder assemblyBuilder)
        {
            Hashtable propertiesForCompilation = ProfileBase.GetPropertiesForCompilation();
            CodeCompileUnit ccu = new CodeCompileUnit();
            Hashtable hashtable2 = new Hashtable();
            Type type = Type.GetType(ProfileBase.InheritsFromTypeString, false);
            CodeNamespace ns = new CodeNamespace();
            ns.Imports.Add(new CodeNamespaceImport("System"));
            ns.Imports.Add(new CodeNamespaceImport("System.Web"));
            ns.Imports.Add(new CodeNamespaceImport("System.Web.Profile"));
            CodeTypeDeclaration declaration = new CodeTypeDeclaration {
                Name = "ProfileCommon"
            };
            if (type != null)
            {
                declaration.BaseTypes.Add(new CodeTypeReference(type));
                assemblyBuilder.AddAssemblyReference(type.Assembly, ccu);
            }
            else
            {
                declaration.BaseTypes.Add(new CodeTypeReference(ProfileBase.InheritsFromTypeString));
                ProfileSection profileAppConfig = MTConfigUtil.GetProfileAppConfig();
                if (profileAppConfig != null)
                {
                    PropertyInformation information = profileAppConfig.ElementInformation.Properties["inherits"];
                    if (((information != null) && (information.Source != null)) && (information.LineNumber > 0))
                    {
                        declaration.LinePragma = new CodeLinePragma(HttpRuntime.GetSafePath(information.Source), information.LineNumber);
                    }
                }
            }
            assemblyBuilder.GenerateTypeFactory("ProfileCommon");
            foreach (DictionaryEntry entry in propertiesForCompilation)
            {
                ProfileNameTypeStruct property = (ProfileNameTypeStruct) entry.Value;
                if (property.PropertyType != null)
                {
                    assemblyBuilder.AddAssemblyReference(property.PropertyType.Assembly, ccu);
                }
                int index = property.Name.IndexOf('.');
                if (index < 0)
                {
                    this.CreateCodeForProperty(assemblyBuilder, declaration, property);
                }
                else
                {
                    string str = property.Name.Substring(0, index);
                    if (!assemblyBuilder.CodeDomProvider.IsValidIdentifier(str))
                    {
                        throw new ConfigurationErrorsException(System.Web.SR.GetString("Profile_bad_group", new object[] { str }), property.FileName, property.LineNumber);
                    }
                    if (hashtable2[str] == null)
                    {
                        hashtable2.Add(str, property.Name);
                    }
                    else
                    {
                        hashtable2[str] = ((string) hashtable2[str]) + ";" + property.Name;
                    }
                }
            }
            foreach (DictionaryEntry entry2 in hashtable2)
            {
                this.AddPropertyGroup(assemblyBuilder, (string) entry2.Key, (string) entry2.Value, propertiesForCompilation, declaration, ns);
            }
            this.AddCodeForGetProfileForUser(declaration);
            ns.Types.Add(declaration);
            ccu.Namespaces.Add(ns);
            assemblyBuilder.AddCodeCompileUnit(this, ccu);
        }

        internal static Type GetProfileTypeFromAssembly(Assembly assembly, bool isPrecompiledApp)
        {
            if (!HasCompilableProfile)
            {
                return null;
            }
            Type type = assembly.GetType("ProfileCommon");
            if ((type == null) && isPrecompiledApp)
            {
                throw new HttpException(System.Web.SR.GetString("Profile_not_precomped"));
            }
            return type;
        }

        internal static bool HasCompilableProfile
        {
            get
            {
                if (!ProfileManager.Enabled)
                {
                    return false;
                }
                if (((ProfileBase.GetPropertiesForCompilation().Count == 0) && !ProfileBase.InheritsFromCustomType) && (ProfileManager.DynamicProfileProperties.Count == 0))
                {
                    return false;
                }
                return true;
            }
        }
    }
}

