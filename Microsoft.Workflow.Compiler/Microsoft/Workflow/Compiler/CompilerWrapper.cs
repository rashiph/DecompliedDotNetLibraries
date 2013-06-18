namespace Microsoft.Workflow.Compiler
{
    using System;
    using System.CodeDom;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Workflow.ComponentModel.Compiler;
    using System.Xml;

    public sealed class CompilerWrapper
    {
        private static string compilerPath;
        private WorkflowCompilerResults results;

        public WorkflowCompilerResults Compile(WorkflowCompilerParameters parameters, params string[] files)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException("parameters");
            }
            if (files == null)
            {
                throw new ArgumentNullException("files");
            }
            if (EnvironmentExtension.Is64BitOS() && EnvironmentExtension.IsWowProcess())
            {
                if (Has64bitAssembliesInReferences(parameters))
                {
                    this.CompileInSeparateProcess(parameters, files);
                }
                else
                {
                    this.CompileInSameProcess(parameters, files);
                }
            }
            else
            {
                this.CompileInSameProcess(parameters, files);
            }
            return this.results;
        }

        private void CompileInSameProcess(WorkflowCompilerParameters parameters, string[] files)
        {
            this.results = new WorkflowCompiler().Compile(parameters, files);
        }

        private void CompileInSeparateProcess(WorkflowCompilerParameters parameters, string[] files)
        {
            string str = SerializeInputToWrapper(parameters, files);
            string tempFileName = Path.GetTempFileName();
            try
            {
                ProcessStartInfo info = new ProcessStartInfo(CompilerPath) {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    ErrorDialog = false,
                    Arguments = string.Format("\"{0}\" \"{1}\"", str, tempFileName)
                };
                Process process = new Process {
                    StartInfo = info
                };
                process.Start();
                process.WaitForExit();
                this.results = DeserializeWrapperOutput(tempFileName);
            }
            finally
            {
                File.Delete(str);
                File.Delete(tempFileName);
            }
        }

        private static WorkflowCompilerResults DeserializeWrapperOutput(string fileName)
        {
            WorkflowCompilerResults results;
            using (Stream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (XmlReader reader = XmlReader.Create(stream))
                {
                    NetDataContractSerializer serializer = new NetDataContractSerializer();
                    SurrogateSelector selector = new SurrogateSelector();
                    selector.AddSurrogate(typeof(MemberAttributes), serializer.Context, new CompilerResultsSurrogate());
                    serializer.SurrogateSelector = selector;
                    results = (WorkflowCompilerResults) serializer.ReadObject(reader);
                }
            }
            return results;
        }

        private static bool Has64bitAssembliesInReferences(WorkflowCompilerParameters parameters)
        {
            for (int i = 0; i < parameters.ReferencedAssemblies.Count; i++)
            {
                if (PEHeader.Is64BitRequiredExecutable(parameters.ReferencedAssemblies[i]))
                {
                    return true;
                }
            }
            return false;
        }

        private static string SerializeInputToWrapper(WorkflowCompilerParameters parameters, string[] files)
        {
            string tempFileName = Path.GetTempFileName();
            using (Stream stream = new FileStream(tempFileName, FileMode.Create, FileAccess.Write, FileShare.Read))
            {
                XmlWriterSettings settings = new XmlWriterSettings {
                    Indent = true
                };
                using (XmlWriter writer = XmlWriter.Create(stream, settings))
                {
                    CompilerInput graph = new CompilerInput(MultiTargetingInfo.MultiTargetingUtilities.NormalizeReferencedAssemblies(parameters), files);
                    new DataContractSerializer(typeof(CompilerInput)).WriteObject(writer, graph);
                }
            }
            return tempFileName;
        }

        private static string CompilerPath
        {
            get
            {
                if (compilerPath == null)
                {
                    compilerPath = string.Format(@"{0}\Microsoft.Workflow.Compiler.exe", RuntimeEnvironment.GetRuntimeDirectory());
                }
                return compilerPath;
            }
        }
    }
}

