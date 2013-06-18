namespace System.Web.Configuration
{
    using Microsoft.Build.Utilities;
    using Microsoft.CSharp;
    using System;
    using System.CodeDom;
    using System.CodeDom.Compiler;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Configuration;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Security;
    using System.Security.Permissions;
    using System.ServiceProcess;
    using System.Web;
    using System.Web.Compilation;
    using System.Web.Hosting;
    using System.Web.UI;
    using System.Web.Util;
    using System.Xml;
    using System.Xml.Schema;

    [PermissionSet(SecurityAction.LinkDemand, Unrestricted=true), PermissionSet(SecurityAction.InheritanceDemand, Unrestricted=true)]
    public class BrowserCapabilitiesCodeGenerator
    {
        private CodeVariableReferenceExpression _browserCapsRefExpr = new CodeVariableReferenceExpression("browserCaps");
        private BrowserDefinitionCollection _browserDefinitionCollection;
        private const string _browserElementsMethodName = "PopulateBrowserElements";
        private ArrayList _browserFileList;
        private static readonly string _browsersDirectory = (HttpRuntime.ClrInstallDirectoryInternal + @"\config\browsers");
        private System.Web.Configuration.BrowserTree _browserTree;
        private ArrayList _customBrowserDefinitionCollections;
        private ArrayList _customBrowserFileLists;
        private ArrayList _customTreeList;
        private ArrayList _customTreeNames;
        private System.Web.Configuration.BrowserTree _defaultTree;
        private CodeVariableReferenceExpression _dictionaryRefExpr = new CodeVariableReferenceExpression("dictionary");
        private const string _dictionaryRefName = "dictionary";
        private const string _disableOptimizedCacheKeyMethodName = "DisableOptimizedCacheKey";
        private const string _factoryTypeName = "BrowserCapabilitiesFactory";
        private const string _headerDictionaryVarName = "_headerDictionary";
        private CaseInsensitiveStringSet _headers = new CaseInsensitiveStringSet();
        private CodeVariableReferenceExpression _headersRefExpr = new CodeVariableReferenceExpression("headers");
        private const string _headersRefName = "headers";
        private const string _matchedHeadersMethodName = "PopulateMatchedHeaders";
        private const string _processRegexMethod = "ProcessRegex";
        private static string _publicKeyToken;
        private static readonly string _publicKeyTokenFile = (_browsersDirectory + @"\" + _publicKeyTokenFileName);
        private static readonly string _publicKeyTokenFileName = "browserCaps.token";
        private static bool _publicKeyTokenLoaded;
        private CodeVariableReferenceExpression _regexWorkerRefExpr = new CodeVariableReferenceExpression("regexWorker");
        private const string _regexWorkerRefName = "regexWorker";
        private const string _resultVarName = "result";
        private static object _staticLock = new object();
        private static readonly string _strongNameKeyFileName = "browserCaps.snk";
        internal const string browserCapsVariable = "browserCaps";
        internal const string IgnoreApplicationBrowserVariableName = "ignoreApplicationBrowsers";

        internal void AddBrowserToCollectionRecursive(BrowserDefinition bd, int depth)
        {
            if (this._browserDefinitionCollection == null)
            {
                this._browserDefinitionCollection = new BrowserDefinitionCollection();
            }
            bd.Depth = depth;
            bd.IsDeviceNode = true;
            this._browserDefinitionCollection.Add(bd);
            foreach (BrowserDefinition definition in bd.Browsers)
            {
                this.AddBrowserToCollectionRecursive(definition, depth + 1);
            }
        }

        internal void AddComment(string comment, CodeMemberMethod cmm)
        {
            cmm.Statements.Add(new CodeCommentStatement(comment));
        }

        internal void AddCustomBrowserToCollectionRecursive(BrowserDefinition bd, int depth, int index)
        {
            if (this._customBrowserDefinitionCollections[index] == null)
            {
                this._customBrowserDefinitionCollections[index] = new BrowserDefinitionCollection();
            }
            bd.Depth = depth;
            bd.IsDeviceNode = true;
            ((BrowserDefinitionCollection) this._customBrowserDefinitionCollections[index]).Add(bd);
            foreach (BrowserDefinition definition in bd.Browsers)
            {
                this.AddCustomBrowserToCollectionRecursive(definition, depth + 1, index);
            }
        }

        internal void AddCustomFile(string filePath)
        {
            if (this._customBrowserFileLists == null)
            {
                this._customBrowserFileLists = new ArrayList();
            }
            this._customBrowserFileLists.Add(filePath);
        }

        internal void AddFile(string filePath)
        {
            if (this._browserFileList == null)
            {
                this._browserFileList = new ArrayList();
            }
            this._browserFileList.Add(filePath);
        }

        [SecurityPermission(SecurityAction.Demand, Unrestricted=true)]
        public virtual void Create()
        {
            FileInfo[] files = new DirectoryInfo(_browsersDirectory).GetFiles("*.browser");
            if ((files != null) && (files.Length != 0))
            {
                foreach (FileInfo info2 in files)
                {
                    this.AddFile(info2.FullName);
                }
                this.ProcessBrowserFiles();
                this.ProcessCustomBrowserFiles();
                this.Uninstall();
                this.GenerateAssembly();
                this.RestartW3SVCIfNecessary();
            }
        }

        private void GenerateAssembly()
        {
            BrowserDefinition bd = (BrowserDefinition) this._browserTree["Default"];
            BrowserDefinition definition2 = (BrowserDefinition) this._defaultTree["Default"];
            ArrayList list = new ArrayList();
            for (int i = 0; i < this._customTreeNames.Count; i++)
            {
                list.Add((BrowserDefinition) ((System.Web.Configuration.BrowserTree) this._customTreeList[i])[this._customTreeNames[i]]);
            }
            CSharpCodeProvider provider = new CSharpCodeProvider();
            CodeCompileUnit compileUnit = new CodeCompileUnit();
            CodeAttributeDeclaration declaration = new CodeAttributeDeclaration("System.Reflection.AssemblyKeyFile", new CodeAttributeArgument[] { new CodeAttributeArgument(new CodePrimitiveExpression(_strongNameKeyFileName)) });
            CodeAttributeDeclaration declaration2 = new CodeAttributeDeclaration("System.Security.AllowPartiallyTrustedCallers");
            compileUnit.AssemblyCustomAttributes.Add(declaration2);
            compileUnit.AssemblyCustomAttributes.Add(declaration);
            declaration = new CodeAttributeDeclaration("System.Reflection.AssemblyVersion", new CodeAttributeArgument[] { new CodeAttributeArgument(new CodePrimitiveExpression("4.0.0.0")) });
            compileUnit.AssemblyCustomAttributes.Add(declaration);
            CodeNamespace namespace2 = new CodeNamespace("ASP");
            namespace2.Imports.Add(new CodeNamespaceImport("System"));
            namespace2.Imports.Add(new CodeNamespaceImport("System.Web"));
            namespace2.Imports.Add(new CodeNamespaceImport("System.Web.Configuration"));
            namespace2.Imports.Add(new CodeNamespaceImport("System.Reflection"));
            compileUnit.Namespaces.Add(namespace2);
            CodeTypeDeclaration declaration3 = new CodeTypeDeclaration("BrowserCapabilitiesFactory") {
                Attributes = MemberAttributes.Private,
                IsClass = true,
                Name = this.TypeName
            };
            declaration3.BaseTypes.Add(new CodeTypeReference("System.Web.Configuration.BrowserCapabilitiesFactoryBase"));
            namespace2.Types.Add(declaration3);
            CodeMemberMethod method = new CodeMemberMethod {
                Attributes = MemberAttributes.Public | MemberAttributes.Override,
                ReturnType = new CodeTypeReference(typeof(void)),
                Name = "ConfigureBrowserCapabilities"
            };
            CodeParameterDeclarationExpression expression = new CodeParameterDeclarationExpression(typeof(NameValueCollection), "headers");
            method.Parameters.Add(expression);
            expression = new CodeParameterDeclarationExpression(typeof(HttpBrowserCapabilities), "browserCaps");
            method.Parameters.Add(expression);
            declaration3.Members.Add(method);
            this.GenerateSingleProcessCall(bd, method);
            for (int j = 0; j < list.Count; j++)
            {
                this.GenerateSingleProcessCall((BrowserDefinition) list[j], method);
            }
            CodeConditionStatement statement = new CodeConditionStatement();
            CodeMethodInvokeExpression left = new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "IsBrowserUnknown", new CodeExpression[0]);
            left.Parameters.Add(this._browserCapsRefExpr);
            statement.Condition = new CodeBinaryOperatorExpression(left, CodeBinaryOperatorType.ValueEquality, new CodePrimitiveExpression(false));
            statement.TrueStatements.Add(new CodeMethodReturnStatement());
            method.Statements.Add(statement);
            if (definition2 != null)
            {
                this.GenerateSingleProcessCall(definition2, method, "Default");
            }
            for (int k = 0; k < list.Count; k++)
            {
                foreach (DictionaryEntry entry in (System.Web.Configuration.BrowserTree) this._customTreeList[k])
                {
                    BrowserDefinition definition3 = entry.Value as BrowserDefinition;
                    this.GenerateProcessMethod(definition3, declaration3);
                }
            }
            foreach (DictionaryEntry entry2 in this._browserTree)
            {
                BrowserDefinition definition4 = entry2.Value as BrowserDefinition;
                this.GenerateProcessMethod(definition4, declaration3);
            }
            foreach (DictionaryEntry entry3 in this._defaultTree)
            {
                BrowserDefinition definition5 = entry3.Value as BrowserDefinition;
                this.GenerateProcessMethod(definition5, declaration3, "Default");
            }
            this.GenerateOverrideMatchedHeaders(declaration3);
            this.GenerateOverrideBrowserElements(declaration3);
            TextWriter writer = new StreamWriter(new FileStream(_browsersDirectory + @"\BrowserCapsFactory.cs", FileMode.Create));
            try
            {
                provider.GenerateCodeFromCompileUnit(compileUnit, writer, null);
            }
            finally
            {
                if (writer != null)
                {
                    writer.Close();
                }
            }
            bool debug = MTConfigUtil.GetCompilationAppConfig().Debug;
            string filename = _browsersDirectory + @"\" + _strongNameKeyFileName;
            StrongNameUtility.GenerateStrongNameFile(filename);
            string[] assemblyNames = new string[] { "System.dll", "System.Web.dll" };
            CompilerParameters options = new CompilerParameters(assemblyNames, "ASP.BrowserCapsFactory", debug) {
                GenerateInMemory = false,
                OutputAssembly = _browsersDirectory + @"\ASP.BrowserCapsFactory.dll"
            };
            CompilerResults results = null;
            try
            {
                results = provider.CompileAssemblyFromFile(options, new string[] { _browsersDirectory + @"\BrowserCapsFactory.cs" });
            }
            finally
            {
                if (File.Exists(filename))
                {
                    File.Delete(filename);
                }
            }
            if ((results.NativeCompilerReturnValue != 0) || results.Errors.HasErrors)
            {
                foreach (CompilerError error in results.Errors)
                {
                    if (!error.IsWarning)
                    {
                        throw new HttpCompileException(error.ErrorText);
                    }
                }
                throw new HttpCompileException(System.Web.SR.GetString("Browser_compile_error"));
            }
            Assembly compiledAssembly = results.CompiledAssembly;
            new GacUtil().GacInstall(compiledAssembly.Location);
            this.SavePublicKeyTokenFile(_publicKeyTokenFile, compiledAssembly.GetName().GetPublicKeyToken());
        }

        private void GenerateCapturesCode(BrowserDefinition bd, CodeMemberMethod cmm, ref bool regexWorkerGenerated)
        {
            if ((bd.CaptureHeaderChecks.Count != 0) || (bd.CaptureCapabilityChecks.Count != 0))
            {
                if (bd.CaptureHeaderChecks.Count > 0)
                {
                    this.AddComment("Capture: header values", cmm);
                    for (int i = 0; i < bd.CaptureHeaderChecks.Count; i++)
                    {
                        string matchString = ((CheckPair) bd.CaptureHeaderChecks[i]).MatchString;
                        if (!matchString.Equals(".*"))
                        {
                            this.GenerateRegexWorkerIfNecessary(cmm, ref regexWorkerGenerated);
                            CodeMethodInvokeExpression expression = new CodeMethodInvokeExpression(this._regexWorkerRefExpr, "ProcessRegex", new CodeExpression[0]);
                            if (((CheckPair) bd.CaptureHeaderChecks[i]).Header.Equals("User-Agent"))
                            {
                                this._headers.Add(string.Empty);
                                expression.Parameters.Add(new CodeCastExpression(typeof(string), new CodeIndexerExpression(new CodeVariableReferenceExpression("browserCaps"), new CodeExpression[] { new CodePropertyReferenceExpression(new CodeTypeReferenceExpression(typeof(string)), "Empty") })));
                            }
                            else
                            {
                                string header = ((CheckPair) bd.CaptureHeaderChecks[i]).Header;
                                this._headers.Add(header);
                                expression.Parameters.Add(new CodeCastExpression(typeof(string), new CodeIndexerExpression(this._headersRefExpr, new CodeExpression[] { new CodePrimitiveExpression(header) })));
                            }
                            expression.Parameters.Add(new CodePrimitiveExpression(matchString));
                            cmm.Statements.Add(expression);
                        }
                    }
                }
                if (bd.CaptureCapabilityChecks.Count > 0)
                {
                    this.AddComment("Capture: capability values", cmm);
                    for (int j = 0; j < bd.CaptureCapabilityChecks.Count; j++)
                    {
                        string str3 = ((CheckPair) bd.CaptureCapabilityChecks[j]).MatchString;
                        if (!str3.Equals(".*"))
                        {
                            this.GenerateRegexWorkerIfNecessary(cmm, ref regexWorkerGenerated);
                            CodeMethodInvokeExpression expression2 = new CodeMethodInvokeExpression(this._regexWorkerRefExpr, "ProcessRegex", new CodeExpression[0]);
                            expression2.Parameters.Add(new CodeCastExpression(typeof(string), new CodeIndexerExpression(this._dictionaryRefExpr, new CodeExpression[] { new CodePrimitiveExpression(((CheckPair) bd.CaptureCapabilityChecks[j]).Header) })));
                            expression2.Parameters.Add(new CodePrimitiveExpression(str3));
                            cmm.Statements.Add(expression2);
                        }
                    }
                }
            }
        }

        private void GenerateChildProcessInvokeExpression(string methodName, CodeMemberMethod cmm, bool generateTracker)
        {
            CodeMethodInvokeExpression expression = new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), methodName, new CodeExpression[0]);
            if (generateTracker)
            {
                expression.Parameters.Add(new CodeVariableReferenceExpression("ignoreApplicationBrowsers"));
            }
            expression.Parameters.Add(new CodeVariableReferenceExpression("headers"));
            expression.Parameters.Add(new CodeVariableReferenceExpression("browserCaps"));
            cmm.Statements.Add(expression);
        }

        private void GenerateChildProcessMethod(string methodName, CodeTypeDeclaration ctd, bool generateTracker)
        {
            CodeMemberMethod method = new CodeMemberMethod {
                Name = methodName,
                ReturnType = new CodeTypeReference(typeof(void)),
                Attributes = MemberAttributes.Family
            };
            CodeParameterDeclarationExpression expression = null;
            if (generateTracker)
            {
                expression = new CodeParameterDeclarationExpression(typeof(bool), "ignoreApplicationBrowsers");
                method.Parameters.Add(expression);
            }
            expression = new CodeParameterDeclarationExpression(typeof(NameValueCollection), "headers");
            method.Parameters.Add(expression);
            expression = new CodeParameterDeclarationExpression(typeof(HttpBrowserCapabilities), "browserCaps");
            method.Parameters.Add(expression);
            ctd.Members.Add(method);
        }

        private void GenerateIdentificationCode(BrowserDefinition bd, CodeMemberMethod cmm, ref bool regexWorkerGenerated)
        {
            cmm.Statements.Add(new CodeVariableDeclarationStatement(typeof(IDictionary), "dictionary"));
            CodeAssignStatement statement = new CodeAssignStatement(this._dictionaryRefExpr, new CodePropertyReferenceExpression(this._browserCapsRefExpr, "Capabilities"));
            cmm.Statements.Add(statement);
            bool flag = false;
            CodeVariableReferenceExpression left = null;
            CodeVariableReferenceExpression varExpr = null;
            if (bd.IdHeaderChecks.Count > 0)
            {
                this.AddComment("Identification: check header matches", cmm);
                for (int i = 0; i < bd.IdHeaderChecks.Count; i++)
                {
                    string matchString = ((CheckPair) bd.IdHeaderChecks[i]).MatchString;
                    if (!matchString.Equals(".*"))
                    {
                        if (varExpr == null)
                        {
                            varExpr = this.GenerateVarReference(cmm, typeof(string), "headerValue");
                        }
                        CodeAssignStatement statement2 = new CodeAssignStatement();
                        cmm.Statements.Add(statement2);
                        statement2.Left = varExpr;
                        if (((CheckPair) bd.IdHeaderChecks[i]).Header.Equals("User-Agent"))
                        {
                            this._headers.Add(string.Empty);
                            statement2.Right = new CodeCastExpression(typeof(string), new CodeIndexerExpression(new CodeVariableReferenceExpression("browserCaps"), new CodeExpression[] { new CodePropertyReferenceExpression(new CodeTypeReferenceExpression(typeof(string)), "Empty") }));
                        }
                        else
                        {
                            string header = ((CheckPair) bd.IdHeaderChecks[i]).Header;
                            this._headers.Add(header);
                            statement2.Right = new CodeCastExpression(typeof(string), new CodeIndexerExpression(this._headersRefExpr, new CodeExpression[] { new CodePrimitiveExpression(header) }));
                            flag = true;
                        }
                        if (matchString.Equals("."))
                        {
                            this.ReturnIfHeaderValueEmpty(cmm, varExpr);
                        }
                        else
                        {
                            if (left == null)
                            {
                                left = this.GenerateVarReference(cmm, typeof(bool), "result");
                            }
                            this.GenerateRegexWorkerIfNecessary(cmm, ref regexWorkerGenerated);
                            CodeMethodInvokeExpression expression3 = new CodeMethodInvokeExpression(this._regexWorkerRefExpr, "ProcessRegex", new CodeExpression[0]);
                            expression3.Parameters.Add(varExpr);
                            expression3.Parameters.Add(new CodePrimitiveExpression(matchString));
                            statement = new CodeAssignStatement {
                                Left = left,
                                Right = expression3
                            };
                            cmm.Statements.Add(statement);
                            CodeConditionStatement statement3 = new CodeConditionStatement();
                            if (((CheckPair) bd.IdHeaderChecks[i]).NonMatch)
                            {
                                statement3.Condition = new CodeBinaryOperatorExpression(left, CodeBinaryOperatorType.ValueEquality, new CodePrimitiveExpression(true));
                            }
                            else
                            {
                                statement3.Condition = new CodeBinaryOperatorExpression(left, CodeBinaryOperatorType.ValueEquality, new CodePrimitiveExpression(false));
                            }
                            statement3.TrueStatements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(false)));
                            cmm.Statements.Add(statement3);
                        }
                    }
                }
            }
            if (bd.IdCapabilityChecks.Count > 0)
            {
                this.AddComment("Identification: check capability matches", cmm);
                for (int j = 0; j < bd.IdCapabilityChecks.Count; j++)
                {
                    string str3 = ((CheckPair) bd.IdCapabilityChecks[j]).MatchString;
                    if (!str3.Equals(".*"))
                    {
                        if (varExpr == null)
                        {
                            varExpr = this.GenerateVarReference(cmm, typeof(string), "headerValue");
                        }
                        CodeAssignStatement statement4 = new CodeAssignStatement();
                        cmm.Statements.Add(statement4);
                        statement4.Left = varExpr;
                        statement4.Right = new CodeCastExpression(typeof(string), new CodeIndexerExpression(this._dictionaryRefExpr, new CodeExpression[] { new CodePrimitiveExpression(((CheckPair) bd.IdCapabilityChecks[j]).Header) }));
                        if (!str3.Equals("."))
                        {
                            if (left == null)
                            {
                                left = this.GenerateVarReference(cmm, typeof(bool), "result");
                            }
                            this.GenerateRegexWorkerIfNecessary(cmm, ref regexWorkerGenerated);
                            CodeMethodInvokeExpression expression4 = new CodeMethodInvokeExpression(this._regexWorkerRefExpr, "ProcessRegex", new CodeExpression[0]);
                            expression4.Parameters.Add(varExpr);
                            expression4.Parameters.Add(new CodePrimitiveExpression(str3));
                            statement = new CodeAssignStatement {
                                Left = left,
                                Right = expression4
                            };
                            cmm.Statements.Add(statement);
                            CodeConditionStatement statement5 = new CodeConditionStatement();
                            if (((CheckPair) bd.IdCapabilityChecks[j]).NonMatch)
                            {
                                statement5.Condition = new CodeBinaryOperatorExpression(left, CodeBinaryOperatorType.ValueEquality, new CodePrimitiveExpression(true));
                            }
                            else
                            {
                                statement5.Condition = new CodeBinaryOperatorExpression(left, CodeBinaryOperatorType.ValueEquality, new CodePrimitiveExpression(false));
                            }
                            statement5.TrueStatements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(false)));
                            cmm.Statements.Add(statement5);
                        }
                    }
                }
            }
            if (flag)
            {
                CodeMethodInvokeExpression expression5 = new CodeMethodInvokeExpression(this._browserCapsRefExpr, "DisableOptimizedCacheKey", new CodeExpression[0]);
                cmm.Statements.Add(expression5);
            }
        }

        internal void GenerateOverrideBrowserElements(CodeTypeDeclaration typeDeclaration)
        {
            if (this._browserDefinitionCollection != null)
            {
                CodeMemberMethod method = new CodeMemberMethod {
                    Name = "PopulateBrowserElements",
                    Attributes = MemberAttributes.Family | MemberAttributes.Override,
                    ReturnType = new CodeTypeReference(typeof(void))
                };
                CodeParameterDeclarationExpression expression = new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(IDictionary)), "dictionary");
                method.Parameters.Add(expression);
                typeDeclaration.Members.Add(method);
                CodeMethodReferenceExpression expression2 = new CodeMethodReferenceExpression(new CodeBaseReferenceExpression(), "PopulateBrowserElements");
                CodeMethodInvokeExpression expression3 = new CodeMethodInvokeExpression(expression2, new CodeExpression[] { this._dictionaryRefExpr });
                method.Statements.Add(expression3);
                foreach (BrowserDefinition definition in this._browserDefinitionCollection)
                {
                    if (definition.IsDeviceNode)
                    {
                        CodeAssignStatement statement = new CodeAssignStatement {
                            Left = new CodeIndexerExpression(this._dictionaryRefExpr, new CodeExpression[] { new CodePrimitiveExpression(definition.ID) }),
                            Right = new CodeObjectCreateExpression(typeof(Triplet), new CodeExpression[] { new CodePrimitiveExpression(definition.ParentName), new CodePropertyReferenceExpression(new CodeTypeReferenceExpression(typeof(string)), "Empty"), new CodePrimitiveExpression(definition.Depth) })
                        };
                        method.Statements.Add(statement);
                    }
                }
                for (int i = 0; i < this._customTreeNames.Count; i++)
                {
                    foreach (BrowserDefinition definition2 in (BrowserDefinitionCollection) this._customBrowserDefinitionCollections[i])
                    {
                        if (definition2.IsDeviceNode)
                        {
                            CodeAssignStatement statement2 = new CodeAssignStatement {
                                Left = new CodeIndexerExpression(this._dictionaryRefExpr, new CodeExpression[] { new CodePrimitiveExpression(definition2.ID) }),
                                Right = new CodeObjectCreateExpression(typeof(Triplet), new CodeExpression[] { new CodePrimitiveExpression(definition2.ParentName), new CodePropertyReferenceExpression(new CodeTypeReferenceExpression(typeof(string)), "Empty"), new CodePrimitiveExpression(definition2.Depth) })
                            };
                            method.Statements.Add(statement2);
                        }
                    }
                }
            }
        }

        internal void GenerateOverrideMatchedHeaders(CodeTypeDeclaration typeDeclaration)
        {
            CodeMemberMethod method = new CodeMemberMethod {
                Name = "PopulateMatchedHeaders",
                Attributes = MemberAttributes.Family | MemberAttributes.Override,
                ReturnType = new CodeTypeReference(typeof(void))
            };
            CodeParameterDeclarationExpression expression = new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(IDictionary)), "dictionary");
            method.Parameters.Add(expression);
            typeDeclaration.Members.Add(method);
            CodeMethodReferenceExpression expression2 = new CodeMethodReferenceExpression(new CodeBaseReferenceExpression(), "PopulateMatchedHeaders");
            CodeMethodInvokeExpression expression3 = new CodeMethodInvokeExpression(expression2, new CodeExpression[] { this._dictionaryRefExpr });
            method.Statements.Add(expression3);
            foreach (string str in (IEnumerable) this._headers)
            {
                CodeAssignStatement statement = new CodeAssignStatement {
                    Left = new CodeIndexerExpression(this._dictionaryRefExpr, new CodeExpression[] { new CodePrimitiveExpression(str) }),
                    Right = new CodePrimitiveExpression(null)
                };
                method.Statements.Add(statement);
            }
        }

        internal void GenerateProcessMethod(BrowserDefinition bd, CodeTypeDeclaration ctd)
        {
            this.GenerateProcessMethod(bd, ctd, string.Empty);
        }

        internal void GenerateProcessMethod(BrowserDefinition bd, CodeTypeDeclaration ctd, string prefix)
        {
            CodeMemberMethod cmm = new CodeMemberMethod {
                Name = prefix + bd.Name + "Process",
                ReturnType = new CodeTypeReference(typeof(bool)),
                Attributes = MemberAttributes.Private
            };
            CodeParameterDeclarationExpression expression = new CodeParameterDeclarationExpression(typeof(NameValueCollection), "headers");
            cmm.Parameters.Add(expression);
            expression = new CodeParameterDeclarationExpression(typeof(HttpBrowserCapabilities), "browserCaps");
            cmm.Parameters.Add(expression);
            bool regexWorkerGenerated = false;
            this.GenerateIdentificationCode(bd, cmm, ref regexWorkerGenerated);
            this.GenerateCapturesCode(bd, cmm, ref regexWorkerGenerated);
            this.GenerateSetCapabilitiesCode(bd, cmm, ref regexWorkerGenerated);
            this.GenerateSetAdaptersCode(bd, cmm);
            if (bd.IsDeviceNode)
            {
                CodeMethodInvokeExpression expression2 = new CodeMethodInvokeExpression(new CodeVariableReferenceExpression("browserCaps"), "AddBrowser", new CodeExpression[0]);
                expression2.Parameters.Add(new CodePrimitiveExpression(bd.ID));
                cmm.Statements.Add(expression2);
            }
            foreach (BrowserDefinition definition in bd.RefGateways)
            {
                this.AddComment("ref gateways, parent=" + bd.ID, cmm);
                this.GenerateSingleProcessCall(definition, cmm);
            }
            if (this.GenerateOverrides && (prefix.Length == 0))
            {
                string methodName = prefix + bd.Name + "ProcessGateways";
                this.GenerateChildProcessMethod(methodName, ctd, false);
                this.GenerateChildProcessInvokeExpression(methodName, cmm, false);
            }
            foreach (BrowserDefinition definition2 in bd.Gateways)
            {
                this.AddComment("gateway, parent=" + bd.ID, cmm);
                this.GenerateSingleProcessCall(definition2, cmm);
            }
            if (this.GenerateOverrides)
            {
                CodeVariableDeclarationStatement statement = new CodeVariableDeclarationStatement(typeof(bool), "ignoreApplicationBrowsers", new CodePrimitiveExpression(bd.Browsers.Count != 0));
                cmm.Statements.Add(statement);
            }
            if (bd.Browsers.Count > 0)
            {
                CodeStatementCollection stmts = cmm.Statements;
                this.AddComment("browser, parent=" + bd.ID, cmm);
                foreach (BrowserDefinition definition3 in bd.Browsers)
                {
                    stmts = this.GenerateTrackedSingleProcessCall(stmts, definition3, cmm, prefix);
                }
                if (this.GenerateOverrides)
                {
                    CodeAssignStatement statement2 = new CodeAssignStatement {
                        Left = new CodeVariableReferenceExpression("ignoreApplicationBrowsers"),
                        Right = new CodePrimitiveExpression(false)
                    };
                    stmts.Add(statement2);
                }
            }
            foreach (BrowserDefinition definition4 in bd.RefBrowsers)
            {
                this.AddComment("ref browsers, parent=" + bd.ID, cmm);
                if (definition4.IsDefaultBrowser)
                {
                    this.GenerateSingleProcessCall(definition4, cmm, "Default");
                }
                else
                {
                    this.GenerateSingleProcessCall(definition4, cmm);
                }
            }
            if (this.GenerateOverrides)
            {
                string str2 = prefix + bd.Name + "ProcessBrowsers";
                this.GenerateChildProcessMethod(str2, ctd, true);
                this.GenerateChildProcessInvokeExpression(str2, cmm, true);
            }
            CodeMethodReturnStatement statement3 = new CodeMethodReturnStatement(new CodePrimitiveExpression(true));
            cmm.Statements.Add(statement3);
            ctd.Members.Add(cmm);
        }

        private void GenerateRegexWorkerIfNecessary(CodeMemberMethod cmm, ref bool regexWorkerGenerated)
        {
            if (!regexWorkerGenerated)
            {
                regexWorkerGenerated = true;
                cmm.Statements.Add(new CodeVariableDeclarationStatement("RegexWorker", "regexWorker"));
                cmm.Statements.Add(new CodeAssignStatement(this._regexWorkerRefExpr, new CodeObjectCreateExpression("RegexWorker", new CodeExpression[] { this._browserCapsRefExpr })));
            }
        }

        internal void GenerateSetAdaptersCode(BrowserDefinition bd, CodeMemberMethod cmm)
        {
            foreach (DictionaryEntry entry in bd.Adapters)
            {
                string key = (string) entry.Key;
                string str2 = (string) entry.Value;
                CodePropertyReferenceExpression targetObject = new CodePropertyReferenceExpression(this._browserCapsRefExpr, "Adapters");
                CodeIndexerExpression expression2 = new CodeIndexerExpression(targetObject, new CodeExpression[] { new CodePrimitiveExpression(key) });
                CodeAssignStatement statement = new CodeAssignStatement {
                    Left = expression2,
                    Right = new CodePrimitiveExpression(str2)
                };
                cmm.Statements.Add(statement);
            }
            if (bd.HtmlTextWriterString != null)
            {
                CodeAssignStatement statement2 = new CodeAssignStatement {
                    Left = new CodePropertyReferenceExpression(this._browserCapsRefExpr, "HtmlTextWriter"),
                    Right = new CodePrimitiveExpression(bd.HtmlTextWriterString)
                };
                cmm.Statements.Add(statement2);
            }
        }

        private void GenerateSetCapabilitiesCode(BrowserDefinition bd, CodeMemberMethod cmm, ref bool regexWorkerGenerated)
        {
            NameValueCollection capabilities = bd.Capabilities;
            this.AddComment("Capabilities: set capabilities", cmm);
            foreach (string str in capabilities.Keys)
            {
                string str2 = capabilities[str];
                CodeAssignStatement statement = new CodeAssignStatement {
                    Left = new CodeIndexerExpression(this._dictionaryRefExpr, new CodeExpression[] { new CodePrimitiveExpression(str) })
                };
                CodePrimitiveExpression expression = new CodePrimitiveExpression(str2);
                if (RegexWorker.RefPat.Match(str2).Success)
                {
                    this.GenerateRegexWorkerIfNecessary(cmm, ref regexWorkerGenerated);
                    statement.Right = new CodeIndexerExpression(this._regexWorkerRefExpr, new CodeExpression[] { expression });
                }
                else
                {
                    statement.Right = expression;
                }
                cmm.Statements.Add(statement);
            }
        }

        internal void GenerateSingleProcessCall(BrowserDefinition bd, CodeMemberMethod cmm)
        {
            this.GenerateSingleProcessCall(bd, cmm, string.Empty);
        }

        internal void GenerateSingleProcessCall(BrowserDefinition bd, CodeMemberMethod cmm, string prefix)
        {
            CodeMethodInvokeExpression expression = new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), prefix + bd.Name + "Process", new CodeExpression[0]);
            expression.Parameters.Add(new CodeVariableReferenceExpression("headers"));
            expression.Parameters.Add(new CodeVariableReferenceExpression("browserCaps"));
            cmm.Statements.Add(expression);
        }

        internal CodeStatementCollection GenerateTrackedSingleProcessCall(CodeStatementCollection stmts, BrowserDefinition bd, CodeMemberMethod cmm)
        {
            return this.GenerateTrackedSingleProcessCall(stmts, bd, cmm, string.Empty);
        }

        internal CodeStatementCollection GenerateTrackedSingleProcessCall(CodeStatementCollection stmts, BrowserDefinition bd, CodeMemberMethod cmm, string prefix)
        {
            CodeMethodInvokeExpression expression = new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), prefix + bd.Name + "Process", new CodeExpression[0]);
            expression.Parameters.Add(new CodeVariableReferenceExpression("headers"));
            expression.Parameters.Add(new CodeVariableReferenceExpression("browserCaps"));
            CodeConditionStatement statement = new CodeConditionStatement {
                Condition = expression
            };
            stmts.Add(statement);
            return statement.FalseStatements;
        }

        private CodeVariableReferenceExpression GenerateVarReference(CodeMemberMethod cmm, Type varType, string varName)
        {
            cmm.Statements.Add(new CodeVariableDeclarationStatement(varType, varName));
            return new CodeVariableReferenceExpression(varName);
        }

        private static FileInfo[] GetFilesNotHidden(DirectoryInfo rootDirectory, DirectoryInfo browserDirInfo)
        {
            ArrayList list = new ArrayList();
            DirectoryInfo[] directories = rootDirectory.GetDirectories("*", SearchOption.AllDirectories);
            FileInfo[] files = rootDirectory.GetFiles("*.browser", SearchOption.TopDirectoryOnly);
            list.AddRange(files);
            for (int i = 0; i < directories.Length; i++)
            {
                if (!HasHiddenParent(directories[i], browserDirInfo))
                {
                    files = directories[i].GetFiles("*.browser", SearchOption.TopDirectoryOnly);
                    list.AddRange(files);
                }
            }
            return (FileInfo[]) list.ToArray(typeof(FileInfo));
        }

        internal virtual void HandleUnRecognizedParentElement(BrowserDefinition bd, bool isDefault)
        {
            throw new ConfigurationErrorsException(System.Web.SR.GetString("Browser_parentID_Not_Found", new object[] { bd.ParentID }), bd.XmlNode);
        }

        private static bool HasHiddenParent(DirectoryInfo directory, DirectoryInfo browserDirInfo)
        {
            while (!string.Equals(directory.Parent.Name, browserDirInfo.Name))
            {
                if ((directory.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
                {
                    return true;
                }
                directory = directory.Parent;
            }
            return false;
        }

        private bool IsRootNode(string nodeName)
        {
            if (string.Compare(nodeName, "Default", StringComparison.OrdinalIgnoreCase) == 0)
            {
                return true;
            }
            foreach (string str in this._customTreeNames)
            {
                if (string.Compare(nodeName, str, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return true;
                }
            }
            return false;
        }

        private static string LoadPublicKeyTokenFromFile(string filename)
        {
            string str;
            InternalSecurityPermissions.FileReadAccess(filename).Assert();
            if (!File.Exists(filename))
            {
                return null;
            }
            try
            {
                using (FileStream stream = new FileStream(filename, FileMode.Open, FileAccess.Read))
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        str = reader.ReadLine();
                    }
                }
            }
            catch (IOException)
            {
                if (HttpRuntime.HasFilePermission(filename))
                {
                    throw;
                }
                str = null;
            }
            finally
            {
                CodeAccessPermission.RevertAssert();
            }
            return str;
        }

        private string NoPathFileName(string fullPath)
        {
            int num = fullPath.LastIndexOf(@"\", StringComparison.Ordinal);
            if (num > -1)
            {
                return fullPath.Substring(num + 1);
            }
            return fullPath;
        }

        private void NormalizeAndValidateTree(System.Web.Configuration.BrowserTree browserTree, bool isDefaultBrowser)
        {
            this.NormalizeAndValidateTree(browserTree, isDefaultBrowser, false);
        }

        private void NormalizeAndValidateTree(System.Web.Configuration.BrowserTree browserTree, bool isDefaultBrowser, bool isCustomBrowser)
        {
            foreach (DictionaryEntry entry in browserTree)
            {
                BrowserDefinition definition = (BrowserDefinition) entry.Value;
                string parentName = definition.ParentName;
                BrowserDefinition definition2 = null;
                if (!this.IsRootNode(definition.Name))
                {
                    if (parentName.Length > 0)
                    {
                        definition2 = (BrowserDefinition) browserTree[parentName];
                    }
                    if (definition2 != null)
                    {
                        if (definition.IsRefID)
                        {
                            if (definition is GatewayDefinition)
                            {
                                definition2.RefGateways.Add(definition);
                            }
                            else
                            {
                                definition2.RefBrowsers.Add(definition);
                            }
                        }
                        else if (definition is GatewayDefinition)
                        {
                            definition2.Gateways.Add(definition);
                        }
                        else
                        {
                            definition2.Browsers.Add(definition);
                        }
                    }
                    else
                    {
                        if (isCustomBrowser)
                        {
                            throw new ConfigurationErrorsException(System.Web.SR.GetString("Browser_parentID_Not_Found", new object[] { definition.ParentID }), definition.XmlNode);
                        }
                        this.HandleUnRecognizedParentElement(definition, isDefaultBrowser);
                    }
                }
            }
            foreach (DictionaryEntry entry2 in browserTree)
            {
                BrowserDefinition definition3 = (BrowserDefinition) entry2.Value;
                Hashtable hashtable = new Hashtable();
                BrowserDefinition definition4 = definition3;
                for (string str2 = definition4.Name; !this.IsRootNode(str2); str2 = definition4.Name)
                {
                    if (hashtable[str2] != null)
                    {
                        throw new ConfigurationErrorsException(System.Web.SR.GetString("Browser_Circular_Reference", new object[] { str2 }), definition4.XmlNode);
                    }
                    hashtable[str2] = str2;
                    definition4 = (BrowserDefinition) browserTree[definition4.ParentName];
                    if (definition4 == null)
                    {
                        break;
                    }
                }
            }
        }

        internal void ProcessBrowserFiles()
        {
            this.ProcessBrowserFiles(false, string.Empty);
        }

        protected void ProcessBrowserFiles(bool useVirtualPath, string virtualDir)
        {
            this._browserTree = new System.Web.Configuration.BrowserTree();
            this._defaultTree = new System.Web.Configuration.BrowserTree();
            this._customTreeNames = new ArrayList();
            if (this._browserFileList == null)
            {
                this._browserFileList = new ArrayList();
            }
            this._browserFileList.Sort();
            string str = null;
            string str2 = null;
            string str3 = null;
            foreach (string str4 in this._browserFileList)
            {
                if (str4.EndsWith("ie.browser", StringComparison.OrdinalIgnoreCase))
                {
                    str2 = str4;
                }
                else if (str4.EndsWith("mozilla.browser", StringComparison.OrdinalIgnoreCase))
                {
                    str = str4;
                }
                else if (str4.EndsWith("opera.browser", StringComparison.OrdinalIgnoreCase))
                {
                    str3 = str4;
                    break;
                }
            }
            if (str2 != null)
            {
                this._browserFileList.Remove(str2);
                this._browserFileList.Insert(0, str2);
            }
            if (str != null)
            {
                this._browserFileList.Remove(str);
                this._browserFileList.Insert(1, str);
            }
            if (str3 != null)
            {
                this._browserFileList.Remove(str3);
                this._browserFileList.Insert(2, str3);
            }
            foreach (string str5 in this._browserFileList)
            {
                XmlDocument document = new ConfigXmlDocument();
                try
                {
                    document.Load(str5);
                    XmlNode documentElement = document.DocumentElement;
                    if (documentElement.Name != "browsers")
                    {
                        if (useVirtualPath)
                        {
                            throw new HttpParseException(System.Web.SR.GetString("Invalid_browser_root"), null, virtualDir + "/" + this.NoPathFileName(str5), null, 1);
                        }
                        throw new HttpParseException(System.Web.SR.GetString("Invalid_browser_root"), null, str5, null, 1);
                    }
                    foreach (XmlNode node2 in documentElement.ChildNodes)
                    {
                        if (node2.NodeType == XmlNodeType.Element)
                        {
                            if ((node2.Name == "browser") || (node2.Name == "gateway"))
                            {
                                this.ProcessBrowserNode(node2, this._browserTree);
                            }
                            else if (node2.Name == "defaultBrowser")
                            {
                                this.ProcessBrowserNode(node2, this._defaultTree);
                            }
                            else
                            {
                                System.Web.Configuration.HandlerBase.ThrowUnrecognizedElement(node2);
                            }
                        }
                    }
                }
                catch (XmlException exception)
                {
                    if (useVirtualPath)
                    {
                        throw new HttpParseException(exception.Message, null, virtualDir + "/" + this.NoPathFileName(str5), null, exception.LineNumber);
                    }
                    throw new HttpParseException(exception.Message, null, str5, null, exception.LineNumber);
                }
                catch (XmlSchemaException exception2)
                {
                    if (useVirtualPath)
                    {
                        throw new HttpParseException(exception2.Message, null, virtualDir + "/" + this.NoPathFileName(str5), null, exception2.LineNumber);
                    }
                    throw new HttpParseException(exception2.Message, null, str5, null, exception2.LineNumber);
                }
            }
            this.NormalizeAndValidateTree(this._browserTree, false);
            this.NormalizeAndValidateTree(this._defaultTree, true);
            BrowserDefinition bd = (BrowserDefinition) this._browserTree["Default"];
            if (bd != null)
            {
                this.AddBrowserToCollectionRecursive(bd, 0);
            }
        }

        internal virtual void ProcessBrowserNode(XmlNode node, System.Web.Configuration.BrowserTree browserTree)
        {
            BrowserDefinition definition = null;
            if (node.Name == "gateway")
            {
                definition = new GatewayDefinition(node);
            }
            else if (node.Name == "browser")
            {
                definition = new BrowserDefinition(node);
            }
            else
            {
                definition = new BrowserDefinition(node, true);
            }
            BrowserDefinition definition2 = (BrowserDefinition) browserTree[definition.Name];
            if (definition2 != null)
            {
                if (!definition.IsRefID)
                {
                    throw new ConfigurationErrorsException(System.Web.SR.GetString("Duplicate_browser_id", new object[] { definition.ID }), node);
                }
                definition2.MergeWithDefinition(definition);
            }
            else
            {
                browserTree[definition.Name] = definition;
            }
        }

        internal void ProcessCustomBrowserFiles()
        {
            this.ProcessCustomBrowserFiles(false, string.Empty);
        }

        internal void ProcessCustomBrowserFiles(bool useVirtualPath, string virtualDir)
        {
            DirectoryInfo browserDirInfo = null;
            DirectoryInfo[] array = null;
            DirectoryInfo[] directories = null;
            this._customTreeList = new ArrayList();
            this._customBrowserFileLists = new ArrayList();
            this._customBrowserDefinitionCollections = new ArrayList();
            if (!useVirtualPath)
            {
                browserDirInfo = new DirectoryInfo(_browsersDirectory);
            }
            else
            {
                browserDirInfo = new DirectoryInfo(HostingEnvironment.MapPathInternal(virtualDir));
            }
            directories = browserDirInfo.GetDirectories();
            int index = 0;
            int length = directories.Length;
            array = new DirectoryInfo[length];
            for (int i = 0; i < length; i++)
            {
                if ((directories[i].Attributes & FileAttributes.Hidden) != FileAttributes.Hidden)
                {
                    array[index] = directories[i];
                    index++;
                }
            }
            Array.Resize<DirectoryInfo>(ref array, index);
            for (int j = 0; j < array.Length; j++)
            {
                FileInfo[] filesNotHidden = GetFilesNotHidden(array[j], browserDirInfo);
                if ((filesNotHidden != null) && (filesNotHidden.Length != 0))
                {
                    System.Web.Configuration.BrowserTree tree = new System.Web.Configuration.BrowserTree();
                    this._customTreeList.Add(tree);
                    this._customTreeNames.Add(array[j].Name);
                    ArrayList list = new ArrayList();
                    foreach (FileInfo info2 in filesNotHidden)
                    {
                        list.Add(info2.FullName);
                    }
                    this._customBrowserFileLists.Add(list);
                }
            }
            for (int k = 0; k < this._customBrowserFileLists.Count; k++)
            {
                ArrayList list2 = (ArrayList) this._customBrowserFileLists[k];
                foreach (string str in list2)
                {
                    XmlDocument document = new ConfigXmlDocument();
                    try
                    {
                        document.Load(str);
                        XmlNode documentElement = document.DocumentElement;
                        if (documentElement.Name != "browsers")
                        {
                            if (useVirtualPath)
                            {
                                throw new HttpParseException(System.Web.SR.GetString("Invalid_browser_root"), null, virtualDir + "/" + this.NoPathFileName(str), null, 1);
                            }
                            throw new HttpParseException(System.Web.SR.GetString("Invalid_browser_root"), null, str, null, 1);
                        }
                        foreach (XmlNode node2 in documentElement.ChildNodes)
                        {
                            if (node2.NodeType == XmlNodeType.Element)
                            {
                                if ((node2.Name == "browser") || (node2.Name == "gateway"))
                                {
                                    this.ProcessBrowserNode(node2, (System.Web.Configuration.BrowserTree) this._customTreeList[k]);
                                }
                                else
                                {
                                    System.Web.Configuration.HandlerBase.ThrowUnrecognizedElement(node2);
                                }
                            }
                        }
                    }
                    catch (XmlException exception)
                    {
                        if (useVirtualPath)
                        {
                            throw new HttpParseException(exception.Message, null, virtualDir + "/" + this.NoPathFileName(str), null, exception.LineNumber);
                        }
                        throw new HttpParseException(exception.Message, null, str, null, exception.LineNumber);
                    }
                    catch (XmlSchemaException exception2)
                    {
                        if (useVirtualPath)
                        {
                            throw new HttpParseException(exception2.Message, null, virtualDir + "/" + this.NoPathFileName(str), null, exception2.LineNumber);
                        }
                        throw new HttpParseException(exception2.Message, null, str, null, exception2.LineNumber);
                    }
                }
                this.SetCustomTreeRoots((System.Web.Configuration.BrowserTree) this._customTreeList[k], k);
                this.NormalizeAndValidateTree((System.Web.Configuration.BrowserTree) this._customTreeList[k], false, true);
                this._customBrowserDefinitionCollections.Add(new BrowserDefinitionCollection());
                this.AddCustomBrowserToCollectionRecursive((BrowserDefinition) ((System.Web.Configuration.BrowserTree) this._customTreeList[k])[this._customTreeNames[k]], 0, k);
            }
        }

        private void RestartW3SVCIfNecessary()
        {
            try
            {
                ServiceController controller = ServiceController.GetServices().SingleOrDefault<ServiceController>(s => string.Equals(s.ServiceName, "W3SVC", StringComparison.OrdinalIgnoreCase));
                if (controller != null)
                {
                    ServiceControllerStatus status = controller.Status;
                    if ((!status.Equals(ServiceControllerStatus.Stopped) && !status.Equals(ServiceControllerStatus.StopPending)) && !status.Equals(ServiceControllerStatus.StartPending))
                    {
                        controller.Stop();
                        controller.WaitForStatus(ServiceControllerStatus.Stopped, new TimeSpan(0, 5, 0));
                        controller.Start();
                        if (status.Equals(ServiceControllerStatus.Paused) || status.Equals(ServiceControllerStatus.PausePending))
                        {
                            controller.Pause();
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException(System.Web.SR.GetString("Browser_W3SVC_Failure_Helper_Text", new object[] { exception }));
            }
        }

        private void ReturnIfHeaderValueEmpty(CodeMemberMethod cmm, CodeVariableReferenceExpression varExpr)
        {
            CodeConditionStatement statement = new CodeConditionStatement();
            CodeMethodReferenceExpression method = new CodeMethodReferenceExpression(new CodeTypeReferenceExpression(typeof(string)), "IsNullOrEmpty");
            CodeMethodInvokeExpression expression2 = new CodeMethodInvokeExpression(method, new CodeExpression[] { varExpr });
            statement.Condition = expression2;
            statement.TrueStatements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(false)));
            cmm.Statements.Add(statement);
        }

        private void SavePublicKeyTokenFile(string filename, byte[] publicKeyToken)
        {
            using (FileStream stream = new FileStream(filename, FileMode.Create, FileAccess.Write))
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    foreach (byte num in publicKeyToken)
                    {
                        writer.Write("{0:X2}", num);
                    }
                }
            }
        }

        private void SetCustomTreeRoots(System.Web.Configuration.BrowserTree browserTree, int index)
        {
            foreach (DictionaryEntry entry in browserTree)
            {
                BrowserDefinition definition = (BrowserDefinition) entry.Value;
                if (definition.ParentName == null)
                {
                    this._customTreeNames[index] = definition.Name;
                    break;
                }
            }
        }

        [SecurityPermission(SecurityAction.Demand, Unrestricted=true)]
        public bool Uninstall()
        {
            this.RestartW3SVCIfNecessary();
            if (!this.UninstallInternal())
            {
                return false;
            }
            this.RestartW3SVCIfNecessary();
            return true;
        }

        internal bool UninstallInternal()
        {
            if (File.Exists(_publicKeyTokenFile))
            {
                File.Delete(_publicKeyTokenFile);
            }
            GacUtil util = new GacUtil();
            if (!util.GacUnInstall("ASP.BrowserCapsFactory, Version=4.0.0.0, Culture=neutral"))
            {
                return false;
            }
            return true;
        }

        internal static string BrowserCapAssemblyPublicKeyToken
        {
            get
            {
                if (_publicKeyTokenLoaded)
                {
                    return _publicKeyToken;
                }
                lock (_staticLock)
                {
                    if (!_publicKeyTokenLoaded)
                    {
                        string pathToDotNetFrameworkFile;
                        if (MultiTargetingUtil.IsTargetFramework40OrAbove)
                        {
                            pathToDotNetFrameworkFile = _publicKeyTokenFile;
                        }
                        else
                        {
                            pathToDotNetFrameworkFile = ToolLocationHelper.GetPathToDotNetFrameworkFile(@"config\browsers\" + _publicKeyTokenFileName, TargetDotNetFrameworkVersion.Version20);
                        }
                        _publicKeyToken = LoadPublicKeyTokenFromFile(pathToDotNetFrameworkFile);
                        _publicKeyTokenLoaded = true;
                    }
                    return _publicKeyToken;
                }
            }
        }

        internal System.Web.Configuration.BrowserTree BrowserTree
        {
            get
            {
                return this._browserTree;
            }
        }

        internal ArrayList CustomTreeList
        {
            get
            {
                return this._customTreeList;
            }
        }

        internal ArrayList CustomTreeNames
        {
            get
            {
                return this._customTreeNames;
            }
        }

        internal System.Web.Configuration.BrowserTree DefaultTree
        {
            get
            {
                return this._defaultTree;
            }
        }

        internal virtual bool GenerateOverrides
        {
            get
            {
                return true;
            }
        }

        internal virtual string TypeName
        {
            get
            {
                return "BrowserCapabilitiesFactory";
            }
        }
    }
}

