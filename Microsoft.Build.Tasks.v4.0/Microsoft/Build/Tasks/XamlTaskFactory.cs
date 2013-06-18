namespace Microsoft.Build.Tasks
{
    using Microsoft.Build.Framework;
    using Microsoft.Build.Shared;
    using Microsoft.Build.Tasks.Xaml;
    using Microsoft.Build.Utilities;
    using System;
    using System.CodeDom;
    using System.CodeDom.Compiler;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading;

    public class XamlTaskFactory : ITaskFactory
    {
        private Assembly taskAssembly;
        private Type taskType;
        private const string XamlTaskNamespace = "XamlTaskNamespace";

        public void CleanupTask(ITask task)
        {
            Microsoft.Build.Shared.ErrorUtilities.VerifyThrowArgumentNull(task, "task");
        }

        public ITask CreateTask(IBuildEngine taskFactoryLoggingHost)
        {
            string typeName = this.TaskNamespace + "." + this.TaskName;
            return (ITask) this.taskAssembly.CreateInstance(typeName);
        }

        public TaskPropertyInfo[] GetTaskParameters()
        {
            PropertyInfo[] properties = this.TaskType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            TaskPropertyInfo[] infoArray2 = new TaskPropertyInfo[properties.Length];
            for (int i = 0; i < properties.Length; i++)
            {
                infoArray2[i] = new TaskPropertyInfo(properties[i].Name, properties[i].PropertyType, properties[i].GetCustomAttributes(typeof(OutputAttribute), false).Length > 0, properties[i].GetCustomAttributes(typeof(RequiredAttribute), false).Length > 0);
            }
            return infoArray2;
        }

        public bool Initialize(string taskName, IDictionary<string, TaskPropertyInfo> taskParameters, string taskElementContents, IBuildEngine taskFactoryLoggingHost)
        {
            CompilerResults results;
            Microsoft.Build.Shared.ErrorUtilities.VerifyThrowArgumentNull(taskName, "taskName");
            Microsoft.Build.Shared.ErrorUtilities.VerifyThrowArgumentNull(taskParameters, "taskParameters");
            TaskLoggingHelper helper = new TaskLoggingHelper(taskFactoryLoggingHost, taskName) {
                TaskResources = Microsoft.Build.Shared.AssemblyResources.PrimaryResources,
                HelpKeywordPrefix = "MSBuild."
            };
            if (taskElementContents == null)
            {
                helper.LogErrorWithCodeFromResources("Xaml.MissingTaskBody", new object[0]);
                return false;
            }
            this.TaskElementContents = taskElementContents.Trim();
            TaskParser parser = new TaskParser();
            parser.Parse(this.TaskElementContents, taskName);
            this.TaskName = parser.GeneratedTaskName;
            this.TaskNamespace = parser.Namespace;
            CodeCompileUnit compileUnit = new TaskGenerator(parser).GenerateCode();
            Assembly assembly = Assembly.LoadWithPartialName("System");
            Assembly assembly2 = Assembly.LoadWithPartialName("Microsoft.Build.Framework");
            Assembly assembly3 = Assembly.LoadWithPartialName("Microsoft.Build.Utilities.V4.0");
            Assembly assembly4 = Assembly.LoadWithPartialName("Microsoft.Build.Tasks.V4.0");
            CompilerParameters parameters = new CompilerParameters(new string[] { assembly.Location, assembly2.Location, assembly3.Location, assembly4.Location }) {
                GenerateInMemory = true,
                TreatWarningsAsErrors = false
            };
            CodeDomProvider provider = CodeDomProvider.CreateProvider("cs");
            bool flag = Environment.GetEnvironmentVariable("MSBUILDWRITEXAMLTASK") == "1";
            if (flag)
            {
                using (StreamWriter writer = new StreamWriter(taskName + "_XamlTask.cs"))
                {
                    CodeGeneratorOptions options = new CodeGeneratorOptions {
                        BlankLinesBetweenMembers = true,
                        BracingStyle = "C"
                    };
                    provider.GenerateCodeFromCompileUnit(compileUnit, writer, options);
                }
                results = provider.CompileAssemblyFromFile(parameters, new string[] { taskName + "_XamlTask.cs" });
            }
            else
            {
                results = provider.CompileAssemblyFromDom(parameters, new CodeCompileUnit[] { compileUnit });
            }
            try
            {
                this.taskAssembly = results.CompiledAssembly;
            }
            catch (FileNotFoundException)
            {
            }
            if (this.taskAssembly == null)
            {
                StringBuilder builder = new StringBuilder();
                builder.AppendLine();
                foreach (CompilerError error in results.Errors)
                {
                    if (!error.IsWarning)
                    {
                        if (flag)
                        {
                            builder.AppendLine(string.Format(Thread.CurrentThread.CurrentUICulture, "({0},{1}) {2}", new object[] { error.Line, error.Column, error.ErrorText }));
                        }
                        else
                        {
                            builder.AppendLine(error.ErrorText);
                        }
                    }
                }
                helper.LogErrorWithCodeFromResources("Xaml.TaskCreationFailed", new object[] { builder.ToString() });
            }
            return !helper.HasLoggedErrors;
        }

        public string FactoryName
        {
            get
            {
                return "XamlTaskFactory";
            }
        }

        public string TaskElementContents { get; private set; }

        public string TaskName { get; private set; }

        public string TaskNamespace { get; private set; }

        public Type TaskType
        {
            get
            {
                if (this.taskType == null)
                {
                    this.taskType = this.taskAssembly.GetType("XamlTaskNamespace" + "." + this.TaskName, true);
                }
                return this.taskType;
            }
        }
    }
}

