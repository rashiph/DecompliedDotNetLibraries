namespace Microsoft.Workflow.Compiler
{
    using System;
    using System.CodeDom;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Workflow.ComponentModel.Compiler;
    using System.Xml;

    internal class Program
    {
        private static void Main(string[] args)
        {
            if ((args == null) || (args.Length != 2))
            {
                throw new ArgumentException(WrapperSR.GetString("InvalidArgumentsToMain"), "args");
            }
            CompilerInput input = ReadCompilerInput(args[0]);
            WorkflowCompilerResults results = new WorkflowCompiler().Compile(MultiTargetingInfo.MultiTargetingUtilities.RenormalizeReferencedAssemblies(input.Parameters), input.Files);
            WriteCompilerOutput(args[1], results);
        }

        private static CompilerInput ReadCompilerInput(string path)
        {
            using (Stream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                XmlReader reader = XmlReader.Create(stream);
                return (CompilerInput) new DataContractSerializer(typeof(CompilerInput)).ReadObject(reader);
            }
        }

        private static void WriteCompilerOutput(string path, WorkflowCompilerResults results)
        {
            using (Stream stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                XmlWriterSettings settings = new XmlWriterSettings {
                    Indent = true
                };
                using (XmlWriter writer = XmlWriter.Create(stream, settings))
                {
                    NetDataContractSerializer serializer = new NetDataContractSerializer();
                    SurrogateSelector selector = new SurrogateSelector();
                    selector.AddSurrogate(typeof(MemberAttributes), serializer.Context, new CompilerResultsSurrogate());
                    serializer.SurrogateSelector = selector;
                    serializer.WriteObject(writer, results);
                }
            }
        }
    }
}

