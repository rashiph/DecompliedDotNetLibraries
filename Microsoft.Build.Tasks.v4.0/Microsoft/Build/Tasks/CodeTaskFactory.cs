namespace Microsoft.Build.Tasks
{
    using Microsoft.Build.Framework;
    using Microsoft.Build.Shared;
    using Microsoft.Build.Utilities;
    using Microsoft.CSharp;
    using System;
    using System.CodeDom;
    using System.CodeDom.Compiler;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Xml;

    public class CodeTaskFactory : ITaskFactory
    {
        private Assembly compiledAssembly;
        private readonly string[] defaultReferencedAssemblies = new string[] { "Microsoft.Build.Utilities.v4.0", "Microsoft.Build.Framework", "System.Core" };
        private readonly string[] defaultUsingNamespaces = new string[] { "System", "System.Collections", "System.Collections.Generic", "System.Text", "System.Linq", "System.IO", "Microsoft.Build.Framework", "Microsoft.Build.Utilities" };
        private string language = "cs";
        private TaskLoggingHelper log;
        private string nameOfTask;
        private string[] referencedAssemblies;
        private string sourceCode;
        private string sourcePath;
        private XmlNode taskNode;
        private IDictionary<string, TaskPropertyInfo> taskParameterTypeInfo;
        private string type;
        private bool typeIsFragment;
        private bool typeIsMethod;
        private string[] usingNamespaces;

        private void AddReferenceAssemblyToReferenceList(List<string> referenceAssemblyList, string referenceAssembly)
        {
            if (referenceAssemblyList != null)
            {
                string item = null;
                if (!string.IsNullOrEmpty(referenceAssembly))
                {
                    try
                    {
                        if (!File.Exists(referenceAssembly))
                        {
                            if (!referenceAssembly.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) || !referenceAssembly.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                            {
                                Assembly assembly = Assembly.LoadWithPartialName(referenceAssembly);
                                if (assembly != null)
                                {
                                    item = assembly.Location;
                                }
                            }
                        }
                        else
                        {
                            try
                            {
                                Assembly assembly2 = Assembly.UnsafeLoadFrom(referenceAssembly);
                                if (assembly2 != null)
                                {
                                    item = assembly2.Location;
                                }
                            }
                            catch (BadImageFormatException)
                            {
                                AssemblyName.GetAssemblyName(referenceAssembly);
                                item = referenceAssembly;
                                this.log.LogMessageFromResources(MessageImportance.Low, "CodeTaskFactory.HaveReflectionOnlyAssembly", new object[] { referenceAssembly });
                            }
                        }
                    }
                    catch (Exception exception)
                    {
                        if (Microsoft.Build.Shared.ExceptionHandling.IsCriticalException(exception))
                        {
                            throw;
                        }
                        this.log.LogErrorWithCodeFromResources("CodeTaskFactory.ReferenceAssemblyIsInvalid", new object[] { referenceAssembly, exception.Message });
                    }
                }
                if (item != null)
                {
                    referenceAssemblyList.Add(item);
                }
                else
                {
                    this.log.LogErrorWithCodeFromResources("CodeTaskFactory.CouldNotFindReferenceAssembly", new object[] { referenceAssembly });
                }
            }
        }

        public void CleanupTask(ITask task)
        {
            Microsoft.Build.Shared.ErrorUtilities.VerifyThrowArgumentNull(task, "task");
        }

        private void CombineReferencedAssemblies(List<string> finalReferenceList)
        {
            foreach (string str in this.defaultReferencedAssemblies)
            {
                this.AddReferenceAssemblyToReferenceList(finalReferenceList, str);
            }
            if (this.referencedAssemblies != null)
            {
                foreach (string str2 in this.referencedAssemblies)
                {
                    this.AddReferenceAssemblyToReferenceList(finalReferenceList, str2);
                }
            }
        }

        private string[] CombineUsingNamespaces()
        {
            int length = this.defaultUsingNamespaces.Length;
            if (this.usingNamespaces != null)
            {
                length += this.usingNamespaces.Length;
            }
            string[] array = new string[length];
            this.defaultUsingNamespaces.CopyTo(array, 0);
            if (this.usingNamespaces != null)
            {
                this.usingNamespaces.CopyTo(array, this.defaultUsingNamespaces.Length);
            }
            return array;
        }

        private Assembly CompileInMemoryAssembly()
        {
            List<string> finalReferenceList = new List<string>();
            this.CombineReferencedAssemblies(finalReferenceList);
            string[] strArray = this.CombineUsingNamespaces();
            using (CodeDomProvider provider = CodeDomProvider.CreateProvider(this.language))
            {
                if (provider is CSharpCodeProvider)
                {
                    this.AddReferenceAssemblyToReferenceList(finalReferenceList, "System");
                }
                CompilerParameters parameters = new CompilerParameters(finalReferenceList.ToArray()) {
                    IncludeDebugInformation = true,
                    GenerateInMemory = true,
                    GenerateExecutable = false
                };
                StringBuilder sb = new StringBuilder();
                StringWriter writer = new StringWriter(sb, CultureInfo.CurrentCulture);
                CodeGeneratorOptions options = new CodeGeneratorOptions {
                    BlankLinesBetweenMembers = true,
                    VerbatimOrder = true
                };
                CodeCompileUnit compileUnit = new CodeCompileUnit();
                if (this.sourcePath != null)
                {
                    this.sourceCode = File.ReadAllText(this.sourcePath);
                }
                string sourceCode = this.sourceCode;
                if (this.typeIsFragment || this.typeIsMethod)
                {
                    CodeTypeDeclaration codeTypeDeclaration = this.CreateTaskClass();
                    this.CreateTaskProperties(codeTypeDeclaration);
                    if (this.typeIsFragment)
                    {
                        CreateExecuteMethodFromFragment(codeTypeDeclaration, this.sourceCode);
                    }
                    else
                    {
                        CreateTaskBody(codeTypeDeclaration, this.sourceCode);
                    }
                    CodeNamespace namespace2 = new CodeNamespace("InlineCode");
                    foreach (string str2 in strArray)
                    {
                        namespace2.Imports.Add(new CodeNamespaceImport(str2));
                    }
                    namespace2.Types.Add(codeTypeDeclaration);
                    compileUnit.Namespaces.Add(namespace2);
                    provider.GenerateCodeFromCompileUnit(compileUnit, writer, options);
                }
                else
                {
                    provider.GenerateCodeFromStatement(new CodeSnippetStatement(this.sourceCode), writer, options);
                }
                sourceCode = sb.ToString();
                CompilerResults results = provider.CompileAssemblyFromSource(parameters, new string[] { sourceCode });
                string path = null;
                if ((results.Errors.Count > 0) || (Environment.GetEnvironmentVariable("MSBUILDLOGCODETASKFACTORYOUTPUT") != null))
                {
                    string tempPath = Path.GetTempPath();
                    string str5 = "MSBUILDCodeTaskFactoryGeneratedFile" + Guid.NewGuid().ToString() + ".txt";
                    path = Path.Combine(tempPath, str5);
                    File.WriteAllText(path, sourceCode);
                }
                if ((results.NativeCompilerReturnValue != 0) && (results.Errors.Count > 0))
                {
                    this.log.LogErrorWithCodeFromResources("CodeTaskFactory.FindSourceFileAt", new object[] { path });
                    foreach (CompilerError error in results.Errors)
                    {
                        this.log.LogErrorWithCodeFromResources("CodeTaskFactory.CompilerError", new object[] { error.ToString() });
                    }
                    return null;
                }
                return results.CompiledAssembly;
            }
        }

        private static void CreateExecuteMethodFromFragment(CodeTypeDeclaration codeTypeDeclaration, string executeCode)
        {
            CodeMemberMethod method = new CodeMemberMethod {
                Name = "Execute",
                Attributes = MemberAttributes.Public | MemberAttributes.Override
            };
            method.Statements.Add(new CodeSnippetStatement(executeCode));
            method.ReturnType = new CodeTypeReference(typeof(bool));
            method.Statements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(null, "_Success")));
            codeTypeDeclaration.Members.Add(method);
        }

        private static void CreateProperty(CodeTypeDeclaration codeTypeDeclaration, TaskPropertyInfo propInfo, object defaultValue)
        {
            CreateProperty(codeTypeDeclaration, propInfo.Name, propInfo.PropertyType, defaultValue);
        }

        private static void CreateProperty(CodeTypeDeclaration ctd, string propertyName, Type propertyType, object defaultValue)
        {
            CodeMemberField field = new CodeMemberField(new CodeTypeReference(propertyType), "_" + propertyName) {
                Attributes = MemberAttributes.Private
            };
            if (defaultValue != null)
            {
                field.InitExpression = new CodePrimitiveExpression(defaultValue);
            }
            ctd.Members.Add(field);
            CodeMemberProperty property = new CodeMemberProperty {
                Name = propertyName,
                Type = new CodeTypeReference(propertyType),
                Attributes = MemberAttributes.Public,
                HasGet = true,
                HasSet = true
            };
            CodeFieldReferenceExpression expression = new CodeFieldReferenceExpression {
                FieldName = field.Name
            };
            property.GetStatements.Add(new CodeMethodReturnStatement(expression));
            CodeAssignStatement statement = new CodeAssignStatement {
                Left = expression,
                Right = new CodeArgumentReferenceExpression("value")
            };
            property.SetStatements.Add(statement);
            ctd.Members.Add(property);
        }

        public ITask CreateTask(IBuildEngine loggingHost)
        {
            if (this.compiledAssembly == null)
            {
                return null;
            }
            TaskLoggingHelper helper = new TaskLoggingHelper(loggingHost, this.nameOfTask) {
                TaskResources = Microsoft.Build.Shared.AssemblyResources.PrimaryResources,
                HelpKeywordPrefix = "MSBuild."
            };
            ITask task = Activator.CreateInstance(this.TaskType) as ITask;
            if (task == null)
            {
                helper.LogErrorWithCodeFromResources("CodeTaskFactory.NeedsITaskInterface", new object[] { this.nameOfTask });
            }
            return task;
        }

        private static void CreateTaskBody(CodeTypeDeclaration codeTypeDeclaration, string taskCode)
        {
            CodeSnippetTypeMember member = new CodeSnippetTypeMember(taskCode);
            codeTypeDeclaration.Members.Add(member);
        }

        private CodeTypeDeclaration CreateTaskClass()
        {
            CodeTypeDeclaration declaration = new CodeTypeDeclaration {
                IsClass = true,
                Name = this.nameOfTask,
                TypeAttributes = TypeAttributes.Public,
                Attributes = MemberAttributes.Final
            };
            declaration.BaseTypes.Add("Microsoft.Build.Utilities.Task");
            return declaration;
        }

        private void CreateTaskProperties(CodeTypeDeclaration codeTypeDeclaration)
        {
            if (this.typeIsFragment)
            {
                CreateProperty(codeTypeDeclaration, "Success", typeof(bool), true);
            }
            foreach (TaskPropertyInfo info in this.taskParameterTypeInfo.Values)
            {
                CreateProperty(codeTypeDeclaration, info, null);
            }
        }

        private string[] ExtractReferencedAssemblies()
        {
            XmlNodeList list = this.taskNode.SelectNodes("//*[local-name()='Reference']");
            List<string> list2 = new List<string>();
            for (int i = 0; i < list.Count; i++)
            {
                XmlAttribute attribute = list[i].Attributes["Include"];
                if (this.HasInvalidChildNodes(list[i], new XmlNodeType[] { XmlNodeType.Comment, XmlNodeType.Whitespace }))
                {
                    return null;
                }
                if ((attribute == null) || (attribute.Value.Length == 0))
                {
                    this.log.LogErrorWithCodeFromResources("CodeTaskFactory.AttributeEmpty", new object[] { "Include" });
                    return null;
                }
                list2.Add(attribute.Value);
            }
            return list2.ToArray();
        }

        private XmlNode ExtractTaskContent(string taskElementContents)
        {
            XmlDocument document = new XmlDocument();
            this.taskNode = document.CreateElement("Task");
            document.AppendChild(this.taskNode);
            this.taskNode.InnerXml = taskElementContents;
            XmlNodeList list = this.taskNode.SelectNodes("//*[local-name()='Code']");
            if (list.Count > 1)
            {
                this.log.LogErrorWithCodeFromResources("CodeTaskFactory.MultipleCodeNodes", new object[0]);
                return null;
            }
            if (list.Count == 0)
            {
                this.log.LogErrorWithCodeFromResources("CodeTaskFactory.CodeElementIsMissing", new object[] { this.nameOfTask });
                return null;
            }
            if (this.HasInvalidChildNodes(list[0], new XmlNodeType[] { XmlNodeType.Comment, XmlNodeType.Whitespace, XmlNodeType.Text, XmlNodeType.CDATA }))
            {
                return null;
            }
            return list[0];
        }

        private string[] ExtractUsingNamespaces()
        {
            XmlNodeList list = this.taskNode.SelectNodes("//*[local-name()='Using']");
            List<string> list2 = new List<string>();
            for (int i = 0; i < list.Count; i++)
            {
                if (this.HasInvalidChildNodes(list[i], new XmlNodeType[] { XmlNodeType.Comment, XmlNodeType.Whitespace }))
                {
                    return null;
                }
                XmlAttribute attribute = list[i].Attributes["Namespace"];
                if ((attribute == null) || (attribute.Value.Length == 0))
                {
                    this.log.LogErrorWithCodeFromResources("CodeTaskFactory.AttributeEmpty", new object[] { "Namespace" });
                    return null;
                }
                list2.Add(attribute.Value);
            }
            return list2.ToArray();
        }

        public TaskPropertyInfo[] GetTaskParameters()
        {
            TaskPropertyInfo[] array = new TaskPropertyInfo[this.taskParameterTypeInfo.Count];
            this.taskParameterTypeInfo.Values.CopyTo(array, 0);
            return array;
        }

        private bool HasInvalidChildNodes(XmlNode parentNode, XmlNodeType[] allowedNodeTypes)
        {
            bool flag = false;
            if (parentNode.HasChildNodes)
            {
                foreach (XmlNode node in parentNode.ChildNodes)
                {
                    bool flag2 = false;
                    foreach (XmlNodeType type in allowedNodeTypes)
                    {
                        if (type == node.NodeType)
                        {
                            flag2 = true;
                            break;
                        }
                    }
                    if (!flag2)
                    {
                        this.log.LogErrorWithCodeFromResources("CodeTaskFactory.InvalidElementLocation", new object[] { node.Name, parentNode.Name });
                        flag = true;
                    }
                }
            }
            return flag;
        }

        public bool Initialize(string taskName, IDictionary<string, TaskPropertyInfo> taskParameters, string taskElementContents, IBuildEngine taskFactoryLoggingHost)
        {
            this.nameOfTask = taskName;
            this.log = new TaskLoggingHelper(taskFactoryLoggingHost, taskName);
            this.log.TaskResources = Microsoft.Build.Shared.AssemblyResources.PrimaryResources;
            this.log.HelpKeywordPrefix = "MSBuild.";
            XmlNode node = this.ExtractTaskContent(taskElementContents);
            if (node == null)
            {
                return false;
            }
            if (!this.ValidateTaskNode())
            {
                return false;
            }
            if (node.Attributes["Type"] != null)
            {
                this.type = node.Attributes["Type"].Value;
                if (this.type.Length == 0)
                {
                    this.log.LogErrorWithCodeFromResources("CodeTaskFactory.AttributeEmpty", new object[] { "Type" });
                    return false;
                }
            }
            if (node.Attributes["Language"] != null)
            {
                this.language = node.Attributes["Language"].Value;
                if (this.language.Length == 0)
                {
                    this.log.LogErrorWithCodeFromResources("CodeTaskFactory.AttributeEmpty", new object[] { "Language" });
                    return false;
                }
            }
            if (node.Attributes["Source"] != null)
            {
                this.sourcePath = node.Attributes["Source"].Value;
                if (this.sourcePath.Length == 0)
                {
                    this.log.LogErrorWithCodeFromResources("CodeTaskFactory.AttributeEmpty", new object[] { "Source" });
                    return false;
                }
                if (this.type == null)
                {
                    this.type = "Class";
                }
            }
            this.referencedAssemblies = this.ExtractReferencedAssemblies();
            if (this.log.HasLoggedErrors)
            {
                return false;
            }
            this.usingNamespaces = this.ExtractUsingNamespaces();
            if (this.log.HasLoggedErrors)
            {
                return false;
            }
            this.sourceCode = node.InnerText;
            if (this.log.HasLoggedErrors)
            {
                return false;
            }
            if (this.type == null)
            {
                this.type = "Fragment";
            }
            if (this.language == null)
            {
                this.language = "cs";
            }
            if (string.Equals(this.type, "Fragment", StringComparison.OrdinalIgnoreCase))
            {
                this.typeIsFragment = true;
                this.typeIsMethod = false;
            }
            else if (string.Equals(this.type, "Method", StringComparison.OrdinalIgnoreCase))
            {
                this.typeIsFragment = false;
                this.typeIsMethod = true;
            }
            this.taskParameterTypeInfo = taskParameters;
            this.compiledAssembly = this.CompileInMemoryAssembly();
            Type[] exportedTypes = this.compiledAssembly.GetExportedTypes();
            Type type = null;
            Type type2 = null;
            foreach (Type type3 in exportedTypes)
            {
                string fullName = type3.FullName;
                if (fullName.Equals(this.nameOfTask, StringComparison.OrdinalIgnoreCase))
                {
                    type = type3;
                    break;
                }
                if ((type2 == null) && fullName.EndsWith(this.nameOfTask, StringComparison.OrdinalIgnoreCase))
                {
                    type2 = type3;
                }
            }
            this.TaskType = type ?? type2;
            if (this.TaskType == null)
            {
                this.log.LogErrorWithCodeFromResources("CodeTaskFactory.CouldNotFindTaskInAssembly", new object[] { this.nameOfTask });
            }
            return !this.log.HasLoggedErrors;
        }

        private bool ValidateTaskNode()
        {
            bool flag = false;
            if (this.taskNode.HasChildNodes)
            {
                foreach (XmlNode node in this.taskNode.ChildNodes)
                {
                    switch (node.NodeType)
                    {
                        case XmlNodeType.Element:
                            if ((node.Name.Equals("Code", StringComparison.OrdinalIgnoreCase) || node.Name.Equals("Reference", StringComparison.OrdinalIgnoreCase)) || node.Name.Equals("Using", StringComparison.OrdinalIgnoreCase))
                            {
                                continue;
                            }
                            flag = true;
                            break;

                        case XmlNodeType.Text:
                        case XmlNodeType.Comment:
                        case XmlNodeType.Whitespace:
                        {
                            continue;
                        }
                        default:
                            flag = true;
                            break;
                    }
                    if (flag)
                    {
                        this.log.LogErrorWithCodeFromResources("CodeTaskFactory.InvalidElementLocation", new object[] { node.Name, this.taskNode.Name });
                        return false;
                    }
                }
            }
            return true;
        }

        public string FactoryName
        {
            get
            {
                return "Code Task Factory";
            }
        }

        public Type TaskType { get; private set; }
    }
}

