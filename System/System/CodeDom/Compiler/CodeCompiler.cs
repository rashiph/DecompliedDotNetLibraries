namespace System.CodeDom.Compiler
{
    using System;
    using System.CodeDom;
    using System.IO;
    using System.Reflection;
    using System.Security;
    using System.Security.Permissions;
    using System.Security.Principal;
    using System.Text;

    [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust"), PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust")]
    public abstract class CodeCompiler : CodeGenerator, ICodeCompiler
    {
        protected CodeCompiler()
        {
        }

        protected abstract string CmdArgsFromParameters(CompilerParameters options);
        internal void Compile(CompilerParameters options, string compilerDirectory, string compilerExe, string arguments, ref string outputFile, ref int nativeReturnValue, string trueArgs)
        {
            string errorName = null;
            outputFile = options.TempFiles.AddExtension("out");
            string path = Path.Combine(compilerDirectory, compilerExe);
            if (!File.Exists(path))
            {
                throw new InvalidOperationException(SR.GetString("CompilerNotFound", new object[] { path }));
            }
            string trueCmdLine = null;
            if (trueArgs != null)
            {
                trueCmdLine = "\"" + path + "\" " + trueArgs;
            }
            nativeReturnValue = Executor.ExecWaitWithCapture(options.SafeUserToken, "\"" + path + "\" " + arguments, Environment.CurrentDirectory, options.TempFiles, ref outputFile, ref errorName, trueCmdLine);
        }

        protected virtual CompilerResults FromDom(CompilerParameters options, CodeCompileUnit e)
        {
            if (options == null)
            {
                throw new ArgumentNullException("options");
            }
            new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Demand();
            CodeCompileUnit[] ea = new CodeCompileUnit[] { e };
            return this.FromDomBatch(options, ea);
        }

        protected virtual CompilerResults FromDomBatch(CompilerParameters options, CodeCompileUnit[] ea)
        {
            if (options == null)
            {
                throw new ArgumentNullException("options");
            }
            if (ea == null)
            {
                throw new ArgumentNullException("ea");
            }
            new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Demand();
            string[] fileNames = new string[ea.Length];
            CompilerResults results = null;
            try
            {
                WindowsImpersonationContext impersonation = Executor.RevertImpersonation();
                try
                {
                    for (int i = 0; i < ea.Length; i++)
                    {
                        if (ea[i] != null)
                        {
                            this.ResolveReferencedAssemblies(options, ea[i]);
                            fileNames[i] = options.TempFiles.AddExtension(i + this.FileExtension);
                            Stream stream = new FileStream(fileNames[i], FileMode.Create, FileAccess.Write, FileShare.Read);
                            try
                            {
                                using (StreamWriter writer = new StreamWriter(stream, Encoding.UTF8))
                                {
                                    ((ICodeGenerator) this).GenerateCodeFromCompileUnit(ea[i], writer, base.Options);
                                    writer.Flush();
                                }
                            }
                            finally
                            {
                                stream.Close();
                            }
                        }
                    }
                    results = this.FromFileBatch(options, fileNames);
                }
                finally
                {
                    Executor.ReImpersonate(impersonation);
                }
            }
            catch
            {
                throw;
            }
            return results;
        }

        protected virtual CompilerResults FromFile(CompilerParameters options, string fileName)
        {
            if (options == null)
            {
                throw new ArgumentNullException("options");
            }
            if (fileName == null)
            {
                throw new ArgumentNullException("fileName");
            }
            new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Demand();
            using (File.OpenRead(fileName))
            {
            }
            string[] fileNames = new string[] { fileName };
            return this.FromFileBatch(options, fileNames);
        }

        protected virtual CompilerResults FromFileBatch(CompilerParameters options, string[] fileNames)
        {
            if (options == null)
            {
                throw new ArgumentNullException("options");
            }
            if (fileNames == null)
            {
                throw new ArgumentNullException("fileNames");
            }
            new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Demand();
            string outputFile = null;
            int nativeReturnValue = 0;
            CompilerResults results = new CompilerResults(options.TempFiles);
            new SecurityPermission(SecurityPermissionFlag.ControlEvidence).Assert();
            try
            {
                results.Evidence = options.Evidence;
            }
            finally
            {
                CodeAccessPermission.RevertAssert();
            }
            bool flag = false;
            if ((options.OutputAssembly == null) || (options.OutputAssembly.Length == 0))
            {
                string fileExtension = options.GenerateExecutable ? "exe" : "dll";
                options.OutputAssembly = results.TempFiles.AddExtension(fileExtension, !options.GenerateInMemory);
                new FileStream(options.OutputAssembly, FileMode.Create, FileAccess.ReadWrite).Close();
                flag = true;
            }
            results.TempFiles.AddExtension("pdb");
            string cmdArgs = this.CmdArgsFromParameters(options) + " " + JoinStringArray(fileNames, " ");
            string responseFileCmdArgs = this.GetResponseFileCmdArgs(options, cmdArgs);
            string trueArgs = null;
            if (responseFileCmdArgs != null)
            {
                trueArgs = cmdArgs;
                cmdArgs = responseFileCmdArgs;
            }
            this.Compile(options, Executor.GetRuntimeInstallDirectory(), this.CompilerName, cmdArgs, ref outputFile, ref nativeReturnValue, trueArgs);
            results.NativeCompilerReturnValue = nativeReturnValue;
            if ((nativeReturnValue != 0) || (options.WarningLevel > 0))
            {
                FileStream stream = new FileStream(outputFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                try
                {
                    if (stream.Length > 0L)
                    {
                        string str6;
                        StreamReader reader = new StreamReader(stream, Encoding.UTF8);
                        do
                        {
                            str6 = reader.ReadLine();
                            if (str6 != null)
                            {
                                results.Output.Add(str6);
                                this.ProcessCompilerOutputLine(results, str6);
                            }
                        }
                        while (str6 != null);
                    }
                }
                finally
                {
                    stream.Close();
                }
                if ((nativeReturnValue != 0) && flag)
                {
                    File.Delete(options.OutputAssembly);
                }
            }
            if (!results.Errors.HasErrors && options.GenerateInMemory)
            {
                FileStream stream2 = new FileStream(options.OutputAssembly, FileMode.Open, FileAccess.Read, FileShare.Read);
                try
                {
                    int length = (int) stream2.Length;
                    byte[] buffer = new byte[length];
                    stream2.Read(buffer, 0, length);
                    new SecurityPermission(SecurityPermissionFlag.ControlEvidence).Assert();
                    try
                    {
                        results.CompiledAssembly = Assembly.Load(buffer, null, options.Evidence);
                    }
                    finally
                    {
                        CodeAccessPermission.RevertAssert();
                    }
                    return results;
                }
                finally
                {
                    stream2.Close();
                }
            }
            results.PathToAssembly = options.OutputAssembly;
            return results;
        }

        protected virtual CompilerResults FromSource(CompilerParameters options, string source)
        {
            if (options == null)
            {
                throw new ArgumentNullException("options");
            }
            new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Demand();
            string[] sources = new string[] { source };
            return this.FromSourceBatch(options, sources);
        }

        protected virtual CompilerResults FromSourceBatch(CompilerParameters options, string[] sources)
        {
            if (options == null)
            {
                throw new ArgumentNullException("options");
            }
            if (sources == null)
            {
                throw new ArgumentNullException("sources");
            }
            new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Demand();
            string[] fileNames = new string[sources.Length];
            CompilerResults results = null;
            try
            {
                WindowsImpersonationContext impersonation = Executor.RevertImpersonation();
                try
                {
                    for (int i = 0; i < sources.Length; i++)
                    {
                        string path = options.TempFiles.AddExtension(i + this.FileExtension);
                        Stream stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read);
                        try
                        {
                            using (StreamWriter writer = new StreamWriter(stream, Encoding.UTF8))
                            {
                                writer.Write(sources[i]);
                                writer.Flush();
                            }
                        }
                        finally
                        {
                            stream.Close();
                        }
                        fileNames[i] = path;
                    }
                    results = this.FromFileBatch(options, fileNames);
                }
                finally
                {
                    Executor.ReImpersonate(impersonation);
                }
            }
            catch
            {
                throw;
            }
            return results;
        }

        protected virtual string GetResponseFileCmdArgs(CompilerParameters options, string cmdArgs)
        {
            string path = options.TempFiles.AddExtension("cmdline");
            Stream stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read);
            try
            {
                using (StreamWriter writer = new StreamWriter(stream, Encoding.UTF8))
                {
                    writer.Write(cmdArgs);
                    writer.Flush();
                }
            }
            finally
            {
                stream.Close();
            }
            return ("@\"" + path + "\"");
        }

        protected static string JoinStringArray(string[] sa, string separator)
        {
            if ((sa == null) || (sa.Length == 0))
            {
                return string.Empty;
            }
            if (sa.Length == 1)
            {
                return ("\"" + sa[0] + "\"");
            }
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < (sa.Length - 1); i++)
            {
                builder.Append("\"");
                builder.Append(sa[i]);
                builder.Append("\"");
                builder.Append(separator);
            }
            builder.Append("\"");
            builder.Append(sa[sa.Length - 1]);
            builder.Append("\"");
            return builder.ToString();
        }

        protected abstract void ProcessCompilerOutputLine(CompilerResults results, string line);
        private void ResolveReferencedAssemblies(CompilerParameters options, CodeCompileUnit e)
        {
            if (e.ReferencedAssemblies.Count > 0)
            {
                foreach (string str in e.ReferencedAssemblies)
                {
                    if (!options.ReferencedAssemblies.Contains(str))
                    {
                        options.ReferencedAssemblies.Add(str);
                    }
                }
            }
        }

        CompilerResults ICodeCompiler.CompileAssemblyFromDom(CompilerParameters options, CodeCompileUnit e)
        {
            CompilerResults results;
            if (options == null)
            {
                throw new ArgumentNullException("options");
            }
            try
            {
                results = this.FromDom(options, e);
            }
            finally
            {
                options.TempFiles.SafeDelete();
            }
            return results;
        }

        CompilerResults ICodeCompiler.CompileAssemblyFromDomBatch(CompilerParameters options, CodeCompileUnit[] ea)
        {
            CompilerResults results;
            if (options == null)
            {
                throw new ArgumentNullException("options");
            }
            try
            {
                results = this.FromDomBatch(options, ea);
            }
            finally
            {
                options.TempFiles.SafeDelete();
            }
            return results;
        }

        CompilerResults ICodeCompiler.CompileAssemblyFromFile(CompilerParameters options, string fileName)
        {
            CompilerResults results;
            if (options == null)
            {
                throw new ArgumentNullException("options");
            }
            try
            {
                results = this.FromFile(options, fileName);
            }
            finally
            {
                options.TempFiles.SafeDelete();
            }
            return results;
        }

        CompilerResults ICodeCompiler.CompileAssemblyFromFileBatch(CompilerParameters options, string[] fileNames)
        {
            CompilerResults results;
            if (options == null)
            {
                throw new ArgumentNullException("options");
            }
            if (fileNames == null)
            {
                throw new ArgumentNullException("fileNames");
            }
            try
            {
                foreach (string str in fileNames)
                {
                    using (File.OpenRead(str))
                    {
                    }
                }
                results = this.FromFileBatch(options, fileNames);
            }
            finally
            {
                options.TempFiles.SafeDelete();
            }
            return results;
        }

        CompilerResults ICodeCompiler.CompileAssemblyFromSource(CompilerParameters options, string source)
        {
            CompilerResults results;
            if (options == null)
            {
                throw new ArgumentNullException("options");
            }
            try
            {
                results = this.FromSource(options, source);
            }
            finally
            {
                options.TempFiles.SafeDelete();
            }
            return results;
        }

        CompilerResults ICodeCompiler.CompileAssemblyFromSourceBatch(CompilerParameters options, string[] sources)
        {
            CompilerResults results;
            if (options == null)
            {
                throw new ArgumentNullException("options");
            }
            try
            {
                results = this.FromSourceBatch(options, sources);
            }
            finally
            {
                options.TempFiles.SafeDelete();
            }
            return results;
        }

        protected abstract string CompilerName { get; }

        protected abstract string FileExtension { get; }
    }
}

