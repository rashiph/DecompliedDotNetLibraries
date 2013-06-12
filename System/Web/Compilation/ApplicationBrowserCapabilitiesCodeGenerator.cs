namespace System.Web.Compilation
{
    using System;
    using System.CodeDom;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Configuration;
    using System.Reflection;
    using System.Web;
    using System.Web.Configuration;
    using System.Web.UI;
    using System.Xml;

    internal class ApplicationBrowserCapabilitiesCodeGenerator : BrowserCapabilitiesCodeGenerator
    {
        private BrowserCapabilitiesFactoryBase _baseInstance;
        private OrderedDictionary _browserOverrides = new OrderedDictionary();
        private System.Web.Compilation.BuildProvider _buildProvider;
        private OrderedDictionary _defaultBrowserOverrides = new OrderedDictionary();
        internal const string FactoryTypeName = "ApplicationBrowserCapabilitiesFactory";

        internal ApplicationBrowserCapabilitiesCodeGenerator(System.Web.Compilation.BuildProvider buildProvider)
        {
            this._buildProvider = buildProvider;
        }

        private static void AddStringToHashtable(OrderedDictionary table, object key, string content, bool before)
        {
            ArrayList list = (ArrayList) table[key];
            if (list == null)
            {
                list = new ArrayList(1);
                table[key] = list;
            }
            if (before)
            {
                list.Insert(0, content);
            }
            else
            {
                list.Add(content);
            }
        }

        public override void Create()
        {
            throw new NotSupportedException();
        }

        internal void GenerateCode(AssemblyBuilder assemblyBuilder)
        {
            base.ProcessBrowserFiles(true, BrowserCapabilitiesCompiler.AppBrowsersVirtualDir.VirtualPathString);
            base.ProcessCustomBrowserFiles(true, BrowserCapabilitiesCompiler.AppBrowsersVirtualDir.VirtualPathString);
            CodeCompileUnit ccu = new CodeCompileUnit();
            ArrayList list = new ArrayList();
            for (int i = 0; i < base.CustomTreeNames.Count; i++)
            {
                list.Add((BrowserDefinition) ((BrowserTree) base.CustomTreeList[i])[base.CustomTreeNames[i]]);
            }
            CodeNamespace namespace2 = new CodeNamespace("ASP");
            namespace2.Imports.Add(new CodeNamespaceImport("System"));
            namespace2.Imports.Add(new CodeNamespaceImport("System.Web"));
            namespace2.Imports.Add(new CodeNamespaceImport("System.Web.Configuration"));
            namespace2.Imports.Add(new CodeNamespaceImport("System.Reflection"));
            ccu.Namespaces.Add(namespace2);
            Type browserCapabilitiesFactoryBaseType = BrowserCapabilitiesCompiler.GetBrowserCapabilitiesFactoryBaseType();
            CodeTypeDeclaration declaration = new CodeTypeDeclaration {
                Attributes = MemberAttributes.Private,
                IsClass = true,
                Name = this.TypeName
            };
            declaration.BaseTypes.Add(new CodeTypeReference(browserCapabilitiesFactoryBaseType));
            namespace2.Types.Add(declaration);
            BindingFlags bindingAttr = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase;
            BrowserDefinition bd = null;
            CodeMemberMethod method = new CodeMemberMethod {
                Attributes = MemberAttributes.Public | MemberAttributes.Override,
                ReturnType = new CodeTypeReference(typeof(void)),
                Name = "ConfigureCustomCapabilities"
            };
            CodeParameterDeclarationExpression expression = new CodeParameterDeclarationExpression(typeof(NameValueCollection), "headers");
            method.Parameters.Add(expression);
            expression = new CodeParameterDeclarationExpression(typeof(HttpBrowserCapabilities), "browserCaps");
            method.Parameters.Add(expression);
            declaration.Members.Add(method);
            for (int j = 0; j < list.Count; j++)
            {
                base.GenerateSingleProcessCall((BrowserDefinition) list[j], method);
            }
            foreach (DictionaryEntry entry in this._browserOverrides)
            {
                object key = entry.Key;
                BrowserDefinition definition2 = (BrowserDefinition) base.BrowserTree[GetFirstItemFromKey(this._browserOverrides, key)];
                string parentName = definition2.ParentName;
                if (!TargetFrameworkUtil.HasMethod(browserCapabilitiesFactoryBaseType, parentName + "ProcessBrowsers", bindingAttr) || !TargetFrameworkUtil.HasMethod(browserCapabilitiesFactoryBaseType, parentName + "ProcessGateways", bindingAttr))
                {
                    string parentID = definition2.ParentID;
                    if (definition2 != null)
                    {
                        throw new ConfigurationErrorsException(System.Web.SR.GetString("Browser_parentID_Not_Found", new object[] { parentID }), definition2.XmlNode);
                    }
                    throw new ConfigurationErrorsException(System.Web.SR.GetString("Browser_parentID_Not_Found", new object[] { parentID }));
                }
                bool flag = true;
                if (definition2 is GatewayDefinition)
                {
                    flag = false;
                }
                string str3 = parentName + (flag ? "ProcessBrowsers" : "ProcessGateways");
                CodeMemberMethod method2 = new CodeMemberMethod {
                    Name = str3,
                    ReturnType = new CodeTypeReference(typeof(void)),
                    Attributes = MemberAttributes.Family | MemberAttributes.Override
                };
                if (flag)
                {
                    expression = new CodeParameterDeclarationExpression(typeof(bool), "ignoreApplicationBrowsers");
                    method2.Parameters.Add(expression);
                }
                expression = new CodeParameterDeclarationExpression(typeof(NameValueCollection), "headers");
                method2.Parameters.Add(expression);
                expression = new CodeParameterDeclarationExpression(typeof(HttpBrowserCapabilities), "browserCaps");
                method2.Parameters.Add(expression);
                declaration.Members.Add(method2);
                ArrayList list2 = (ArrayList) this._browserOverrides[key];
                CodeStatementCollection stmts = method2.Statements;
                bool flag2 = false;
                foreach (string str4 in list2)
                {
                    bd = (BrowserDefinition) base.BrowserTree[str4];
                    if ((bd is GatewayDefinition) || bd.IsRefID)
                    {
                        base.GenerateSingleProcessCall(bd, method2);
                    }
                    else
                    {
                        if (!flag2)
                        {
                            CodeConditionStatement statement = new CodeConditionStatement {
                                Condition = new CodeVariableReferenceExpression("ignoreApplicationBrowsers")
                            };
                            method2.Statements.Add(statement);
                            stmts = statement.FalseStatements;
                            flag2 = true;
                        }
                        stmts = base.GenerateTrackedSingleProcessCall(stmts, bd, method2);
                        if (this._baseInstance == null)
                        {
                            if (MultiTargetingUtil.IsTargetFramework40OrAbove || (browserCapabilitiesFactoryBaseType.Assembly == BrowserCapabilitiesCompiler.AspBrowserCapsFactoryAssembly))
                            {
                                this._baseInstance = (BrowserCapabilitiesFactoryBase) Activator.CreateInstance(browserCapabilitiesFactoryBaseType);
                            }
                            else
                            {
                                this._baseInstance = new BrowserCapabilitiesFactory35();
                            }
                        }
                        int third = (int) ((Triplet) this._baseInstance.InternalGetBrowserElements()[parentName]).Third;
                        base.AddBrowserToCollectionRecursive(bd, third + 1);
                    }
                }
            }
            foreach (DictionaryEntry entry2 in this._defaultBrowserOverrides)
            {
                object obj3 = entry2.Key;
                BrowserDefinition definition3 = (BrowserDefinition) base.DefaultTree[GetFirstItemFromKey(this._defaultBrowserOverrides, obj3)];
                string str5 = definition3.ParentName;
                if (browserCapabilitiesFactoryBaseType.GetMethod("Default" + str5 + "ProcessBrowsers", bindingAttr) == null)
                {
                    string str6 = definition3.ParentID;
                    if (definition3 != null)
                    {
                        throw new ConfigurationErrorsException(System.Web.SR.GetString("DefaultBrowser_parentID_Not_Found", new object[] { str6 }), definition3.XmlNode);
                    }
                }
                string str7 = "Default" + str5 + "ProcessBrowsers";
                CodeMemberMethod method3 = new CodeMemberMethod {
                    Name = str7,
                    ReturnType = new CodeTypeReference(typeof(void)),
                    Attributes = MemberAttributes.Family | MemberAttributes.Override
                };
                expression = new CodeParameterDeclarationExpression(typeof(bool), "ignoreApplicationBrowsers");
                method3.Parameters.Add(expression);
                expression = new CodeParameterDeclarationExpression(typeof(NameValueCollection), "headers");
                method3.Parameters.Add(expression);
                expression = new CodeParameterDeclarationExpression(typeof(HttpBrowserCapabilities), "browserCaps");
                method3.Parameters.Add(expression);
                declaration.Members.Add(method3);
                ArrayList list3 = (ArrayList) this._defaultBrowserOverrides[obj3];
                CodeConditionStatement statement2 = new CodeConditionStatement {
                    Condition = new CodeVariableReferenceExpression("ignoreApplicationBrowsers")
                };
                method3.Statements.Add(statement2);
                CodeStatementCollection falseStatements = statement2.FalseStatements;
                foreach (string str8 in list3)
                {
                    bd = (BrowserDefinition) base.DefaultTree[str8];
                    if (bd.IsRefID)
                    {
                        base.GenerateSingleProcessCall(bd, method3, "Default");
                    }
                    else
                    {
                        falseStatements = base.GenerateTrackedSingleProcessCall(falseStatements, bd, method3, "Default");
                    }
                }
            }
            foreach (DictionaryEntry entry3 in base.BrowserTree)
            {
                bd = entry3.Value as BrowserDefinition;
                base.GenerateProcessMethod(bd, declaration);
            }
            for (int k = 0; k < list.Count; k++)
            {
                foreach (DictionaryEntry entry4 in (BrowserTree) base.CustomTreeList[k])
                {
                    bd = entry4.Value as BrowserDefinition;
                    base.GenerateProcessMethod(bd, declaration);
                }
            }
            foreach (DictionaryEntry entry5 in base.DefaultTree)
            {
                bd = entry5.Value as BrowserDefinition;
                base.GenerateProcessMethod(bd, declaration, "Default");
            }
            base.GenerateOverrideMatchedHeaders(declaration);
            base.GenerateOverrideBrowserElements(declaration);
            Assembly a = BrowserCapabilitiesCompiler.GetBrowserCapabilitiesFactoryBaseType().Assembly;
            assemblyBuilder.AddAssemblyReference(a, ccu);
            assemblyBuilder.AddCodeCompileUnit(this._buildProvider, ccu);
        }

        private static string GetFirstItemFromKey(OrderedDictionary table, object key)
        {
            ArrayList list = (ArrayList) table[key];
            if ((list != null) && (list.Count > 0))
            {
                return (list[0] as string);
            }
            return null;
        }

        internal override void HandleUnRecognizedParentElement(BrowserDefinition bd, bool isDefault)
        {
            string parentName = bd.ParentName;
            int key = bd.GetType().GetHashCode() ^ parentName.GetHashCode();
            if (isDefault)
            {
                AddStringToHashtable(this._defaultBrowserOverrides, key, bd.Name, bd.IsRefID);
            }
            else
            {
                AddStringToHashtable(this._browserOverrides, key, bd.Name, bd.IsRefID);
            }
        }

        internal override void ProcessBrowserNode(XmlNode node, BrowserTree browserTree)
        {
            if (node.Name == "defaultBrowser")
            {
                throw new ConfigurationErrorsException(System.Web.SR.GetString("Browser_Not_Allowed_InAppLevel", new object[] { node.Name }), node);
            }
            base.ProcessBrowserNode(node, browserTree);
        }

        internal override bool GenerateOverrides
        {
            get
            {
                return false;
            }
        }

        internal override string TypeName
        {
            get
            {
                return "ApplicationBrowserCapabilitiesFactory";
            }
        }
    }
}

