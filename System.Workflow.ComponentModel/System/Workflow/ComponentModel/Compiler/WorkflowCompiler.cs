namespace System.Workflow.ComponentModel.Compiler
{
    using System;
    using System.IO;
    using System.Reflection;

    public sealed class WorkflowCompiler
    {
        public WorkflowCompilerResults Compile(WorkflowCompilerParameters parameters, params string[] files)
        {
            WorkflowCompilerResults results2;
            if (parameters == null)
            {
                throw new ArgumentNullException("parameters");
            }
            if (files == null)
            {
                throw new ArgumentNullException("files");
            }
            AppDomainSetup setupInformation = AppDomain.CurrentDomain.SetupInformation;
            setupInformation.LoaderOptimization = LoaderOptimization.MultiDomainHost;
            AppDomain domain = AppDomain.CreateDomain("CompilerDomain", null, setupInformation);
            bool flag = false;
            string outputAssembly = parameters.OutputAssembly;
            try
            {
                WorkflowCompilerInternal internal2;
                if (parameters.GenerateInMemory)
                {
                    flag = true;
                    parameters.GenerateInMemory = false;
                    if (!string.IsNullOrEmpty(parameters.OutputAssembly))
                    {
                        goto Label_007A;
                    }
                    parameters.OutputAssembly = Path.GetTempFileName() + ".dll";
                }
                goto Label_00BC;
            Label_007A:
                try
                {
                    DirectoryInfo info = Directory.CreateDirectory(Path.GetTempPath() + @"\" + Guid.NewGuid());
                    parameters.OutputAssembly = info.FullName + @"\" + parameters.OutputAssembly;
                }
                catch
                {
                    goto Label_007A;
                }
            Label_00BC:
                internal2 = (WorkflowCompilerInternal) domain.CreateInstanceAndUnwrap(Assembly.GetExecutingAssembly().FullName, typeof(WorkflowCompilerInternal).FullName);
                WorkflowCompilerResults results = internal2.Compile(parameters, files);
                if (flag && !results.Errors.HasErrors)
                {
                    results.CompiledAssembly = Assembly.Load(File.ReadAllBytes(results.PathToAssembly));
                    results.PathToAssembly = null;
                    try
                    {
                        File.Delete(parameters.OutputAssembly);
                        Directory.Delete(Path.GetDirectoryName(parameters.OutputAssembly));
                    }
                    catch
                    {
                    }
                }
                results2 = results;
            }
            finally
            {
                string path = parameters.OutputAssembly;
                if (flag)
                {
                    parameters.GenerateInMemory = true;
                    parameters.OutputAssembly = outputAssembly;
                }
                AppDomain.Unload(domain);
                if (flag)
                {
                    try
                    {
                        File.Delete(path);
                    }
                    catch
                    {
                    }
                }
            }
            return results2;
        }
    }
}

