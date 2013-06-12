namespace Microsoft.CSharp
{
    using System;
    using System.CodeDom;
    using System.CodeDom.Compiler;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Security;
    using System.Security.Permissions;
    using System.Security.Principal;
    using System.Text;
    using System.Text.RegularExpressions;

    internal class CSharpCodeGenerator : ICodeCompiler, ICodeGenerator
    {
        private CodeTypeDeclaration currentClass;
        private CodeTypeMember currentMember;
        private bool generatingForLoop;
        private bool inNestedBinary;
        private static readonly string[][] keywords;
        private const GeneratorSupport LanguageSupport = (GeneratorSupport.DeclareIndexerProperties | GeneratorSupport.GenericTypeDeclaration | GeneratorSupport.GenericTypeReference | GeneratorSupport.PartialTypes | GeneratorSupport.Resources | GeneratorSupport.Win32Resources | GeneratorSupport.ComplexExpressions | GeneratorSupport.PublicStaticMembers | GeneratorSupport.MultipleInterfaceMembers | GeneratorSupport.NestedTypes | GeneratorSupport.ChainedConstructorArguments | GeneratorSupport.ReferenceParameters | GeneratorSupport.ParameterAttributes | GeneratorSupport.AssemblyAttributes | GeneratorSupport.DeclareEvents | GeneratorSupport.DeclareInterfaces | GeneratorSupport.DeclareDelegates | GeneratorSupport.DeclareEnums | GeneratorSupport.DeclareValueTypes | GeneratorSupport.ReturnTypeAttributes | GeneratorSupport.TryCatchStatements | GeneratorSupport.StaticConstructors | GeneratorSupport.MultidimensionalArrays | GeneratorSupport.GotoStatements | GeneratorSupport.EntryPointMethod | GeneratorSupport.ArraysOfArrays);
        private const int MaxLineLength = 80;
        private CodeGeneratorOptions options;
        private IndentedTextWriter output;
        private static Regex outputRegSimple;
        private static Regex outputRegWithFileAndLine;
        private const int ParameterMultilineThreshold = 15;
        private IDictionary<string, string> provOptions;

        static CSharpCodeGenerator()
        {
            string[][] strArray = new string[10][];
            strArray[1] = new string[] { "as", "do", "if", "in", "is" };
            strArray[2] = new string[] { "for", "int", "new", "out", "ref", "try" };
            strArray[3] = new string[] { "base", "bool", "byte", "case", "char", "else", "enum", "goto", "lock", "long", "null", "this", "true", "uint", "void" };
            strArray[4] = new string[] { "break", "catch", "class", "const", "event", "false", "fixed", "float", "sbyte", "short", "throw", "ulong", "using", "while" };
            strArray[5] = new string[] { "double", "extern", "object", "params", "public", "return", "sealed", "sizeof", "static", "string", "struct", "switch", "typeof", "unsafe", "ushort" };
            strArray[6] = new string[] { "checked", "decimal", "default", "finally", "foreach", "private", "virtual" };
            strArray[7] = new string[] { "abstract", "continue", "delegate", "explicit", "implicit", "internal", "operator", "override", "readonly", "volatile" };
            strArray[8] = new string[] { "__arglist", "__makeref", "__reftype", "interface", "namespace", "protected", "unchecked" };
            strArray[9] = new string[] { "__refvalue", "stackalloc" };
            keywords = strArray;
        }

        internal CSharpCodeGenerator()
        {
        }

        internal CSharpCodeGenerator(IDictionary<string, string> providerOptions)
        {
            this.provOptions = providerOptions;
        }

        private void AppendEscapedChar(StringBuilder b, char value)
        {
            if (b == null)
            {
                this.Output.Write(@"\u");
                int num = value;
                this.Output.Write(num.ToString("X4", CultureInfo.InvariantCulture));
            }
            else
            {
                b.Append(@"\u");
                b.Append(((int) value).ToString("X4", CultureInfo.InvariantCulture));
            }
        }

        private string CmdArgsFromParameters(CompilerParameters options)
        {
            StringBuilder builder = new StringBuilder(0x80);
            if (options.GenerateExecutable)
            {
                builder.Append("/t:exe ");
                if ((options.MainClass != null) && (options.MainClass.Length > 0))
                {
                    builder.Append("/main:");
                    builder.Append(options.MainClass);
                    builder.Append(" ");
                }
            }
            else
            {
                builder.Append("/t:library ");
            }
            builder.Append("/utf8output ");
            foreach (string str in options.ReferencedAssemblies)
            {
                builder.Append("/R:");
                builder.Append("\"");
                builder.Append(str);
                builder.Append("\"");
                builder.Append(" ");
            }
            builder.Append("/out:");
            builder.Append("\"");
            builder.Append(options.OutputAssembly);
            builder.Append("\"");
            builder.Append(" ");
            if (options.IncludeDebugInformation)
            {
                builder.Append("/D:DEBUG ");
                builder.Append("/debug+ ");
                builder.Append("/optimize- ");
            }
            else
            {
                builder.Append("/debug- ");
                builder.Append("/optimize+ ");
            }
            if (options.Win32Resource != null)
            {
                builder.Append("/win32res:\"" + options.Win32Resource + "\" ");
            }
            foreach (string str2 in options.EmbeddedResources)
            {
                builder.Append("/res:\"");
                builder.Append(str2);
                builder.Append("\" ");
            }
            foreach (string str3 in options.LinkedResources)
            {
                builder.Append("/linkres:\"");
                builder.Append(str3);
                builder.Append("\" ");
            }
            if (options.TreatWarningsAsErrors)
            {
                builder.Append("/warnaserror ");
            }
            if (options.WarningLevel >= 0)
            {
                builder.Append("/w:" + options.WarningLevel + " ");
            }
            if (options.CompilerOptions != null)
            {
                builder.Append(options.CompilerOptions + " ");
            }
            return builder.ToString();
        }

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

        private void ContinueOnNewLine(string st)
        {
            this.Output.WriteLine(st);
        }

        public string CreateEscapedIdentifier(string name)
        {
            if (!IsKeyword(name) && !IsPrefixTwoUnderscore(name))
            {
                return name;
            }
            return ("@" + name);
        }

        public string CreateValidIdentifier(string name)
        {
            if (IsPrefixTwoUnderscore(name))
            {
                name = "_" + name;
            }
            while (IsKeyword(name))
            {
                name = "_" + name;
            }
            return name;
        }

        private CompilerResults FromDom(CompilerParameters options, CodeCompileUnit e)
        {
            if (options == null)
            {
                throw new ArgumentNullException("options");
            }
            new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Demand();
            CodeCompileUnit[] ea = new CodeCompileUnit[] { e };
            return this.FromDomBatch(options, ea);
        }

        private CompilerResults FromDomBatch(CompilerParameters options, CodeCompileUnit[] ea)
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
                                    ((ICodeGenerator) this).GenerateCodeFromCompileUnit(ea[i], writer, this.Options);
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

        private CompilerResults FromFile(CompilerParameters options, string fileName)
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

        private CompilerResults FromFileBatch(CompilerParameters options, string[] fileNames)
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
                string str2 = options.GenerateExecutable ? "exe" : "dll";
                options.OutputAssembly = results.TempFiles.AddExtension(str2, !options.GenerateInMemory);
                new FileStream(options.OutputAssembly, FileMode.Create, FileAccess.ReadWrite).Close();
                flag = true;
            }
            string fileExtension = "pdb";
            if ((options.CompilerOptions != null) && (CultureInfo.InvariantCulture.CompareInfo.IndexOf(options.CompilerOptions, "/debug:pdbonly", CompareOptions.IgnoreCase) != -1))
            {
                results.TempFiles.AddExtension(fileExtension, true);
            }
            else
            {
                results.TempFiles.AddExtension(fileExtension);
            }
            string cmdArgs = this.CmdArgsFromParameters(options) + " " + JoinStringArray(fileNames, " ");
            string responseFileCmdArgs = this.GetResponseFileCmdArgs(options, cmdArgs);
            string trueArgs = null;
            if (responseFileCmdArgs != null)
            {
                trueArgs = cmdArgs;
                cmdArgs = responseFileCmdArgs;
            }
            this.Compile(options, RedistVersionInfo.GetCompilerPath(this.provOptions, this.CompilerName), this.CompilerName, cmdArgs, ref outputFile, ref nativeReturnValue, trueArgs);
            results.NativeCompilerReturnValue = nativeReturnValue;
            if ((nativeReturnValue != 0) || (options.WarningLevel > 0))
            {
                foreach (string str7 in ReadAllLines(outputFile, Encoding.UTF8, FileShare.ReadWrite))
                {
                    results.Output.Add(str7);
                    this.ProcessCompilerOutputLine(results, str7);
                }
                if ((nativeReturnValue != 0) && flag)
                {
                    File.Delete(options.OutputAssembly);
                }
            }
            if (!results.Errors.HasErrors && options.GenerateInMemory)
            {
                FileStream stream = new FileStream(options.OutputAssembly, FileMode.Open, FileAccess.Read, FileShare.Read);
                try
                {
                    int length = (int) stream.Length;
                    byte[] buffer = new byte[length];
                    stream.Read(buffer, 0, length);
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
                    stream.Close();
                }
            }
            results.PathToAssembly = options.OutputAssembly;
            return results;
        }

        private CompilerResults FromSource(CompilerParameters options, string source)
        {
            if (options == null)
            {
                throw new ArgumentNullException("options");
            }
            new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Demand();
            string[] sources = new string[] { source };
            return this.FromSourceBatch(options, sources);
        }

        private CompilerResults FromSourceBatch(CompilerParameters options, string[] sources)
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

        private void GenerateArgumentReferenceExpression(CodeArgumentReferenceExpression e)
        {
            this.OutputIdentifier(e.ParameterName);
        }

        private void GenerateArrayCreateExpression(CodeArrayCreateExpression e)
        {
            this.Output.Write("new ");
            CodeExpressionCollection initializers = e.Initializers;
            if (initializers.Count > 0)
            {
                this.OutputType(e.CreateType);
                if (e.CreateType.ArrayRank == 0)
                {
                    this.Output.Write("[]");
                }
                this.Output.WriteLine(" {");
                this.Indent++;
                this.OutputExpressionList(initializers, true);
                this.Indent--;
                this.Output.Write("}");
            }
            else
            {
                this.Output.Write(this.GetBaseTypeOutput(e.CreateType));
                this.Output.Write("[");
                if (e.SizeExpression != null)
                {
                    this.GenerateExpression(e.SizeExpression);
                }
                else
                {
                    this.Output.Write(e.Size);
                }
                this.Output.Write("]");
            }
        }

        private void GenerateArrayIndexerExpression(CodeArrayIndexerExpression e)
        {
            this.GenerateExpression(e.TargetObject);
            this.Output.Write("[");
            bool flag = true;
            foreach (CodeExpression expression in e.Indices)
            {
                if (flag)
                {
                    flag = false;
                }
                else
                {
                    this.Output.Write(", ");
                }
                this.GenerateExpression(expression);
            }
            this.Output.Write("]");
        }

        private void GenerateAssignStatement(CodeAssignStatement e)
        {
            this.GenerateExpression(e.Left);
            this.Output.Write(" = ");
            this.GenerateExpression(e.Right);
            if (!this.generatingForLoop)
            {
                this.Output.WriteLine(";");
            }
        }

        private void GenerateAttachEventStatement(CodeAttachEventStatement e)
        {
            this.GenerateEventReferenceExpression(e.Event);
            this.Output.Write(" += ");
            this.GenerateExpression(e.Listener);
            this.Output.WriteLine(";");
        }

        private void GenerateAttributeDeclarationsEnd(CodeAttributeDeclarationCollection attributes)
        {
            this.Output.Write("]");
        }

        private void GenerateAttributeDeclarationsStart(CodeAttributeDeclarationCollection attributes)
        {
            this.Output.Write("[");
        }

        private void GenerateAttributes(CodeAttributeDeclarationCollection attributes)
        {
            this.GenerateAttributes(attributes, null, false);
        }

        private void GenerateAttributes(CodeAttributeDeclarationCollection attributes, string prefix)
        {
            this.GenerateAttributes(attributes, prefix, false);
        }

        private void GenerateAttributes(CodeAttributeDeclarationCollection attributes, string prefix, bool inLine)
        {
            if (attributes.Count != 0)
            {
                IEnumerator enumerator = attributes.GetEnumerator();
                bool flag = false;
                while (enumerator.MoveNext())
                {
                    CodeAttributeDeclaration current = (CodeAttributeDeclaration) enumerator.Current;
                    if (current.Name.Equals("system.paramarrayattribute", StringComparison.OrdinalIgnoreCase))
                    {
                        flag = true;
                    }
                    else
                    {
                        this.GenerateAttributeDeclarationsStart(attributes);
                        if (prefix != null)
                        {
                            this.Output.Write(prefix);
                        }
                        if (current.AttributeType != null)
                        {
                            this.Output.Write(this.GetTypeOutput(current.AttributeType));
                        }
                        this.Output.Write("(");
                        bool flag2 = true;
                        foreach (CodeAttributeArgument argument in current.Arguments)
                        {
                            if (flag2)
                            {
                                flag2 = false;
                            }
                            else
                            {
                                this.Output.Write(", ");
                            }
                            this.OutputAttributeArgument(argument);
                        }
                        this.Output.Write(")");
                        this.GenerateAttributeDeclarationsEnd(attributes);
                        if (inLine)
                        {
                            this.Output.Write(" ");
                            continue;
                        }
                        this.Output.WriteLine();
                    }
                }
                if (flag)
                {
                    if (prefix != null)
                    {
                        this.Output.Write(prefix);
                    }
                    this.Output.Write("params");
                    if (inLine)
                    {
                        this.Output.Write(" ");
                    }
                    else
                    {
                        this.Output.WriteLine();
                    }
                }
            }
        }

        private void GenerateBaseReferenceExpression(CodeBaseReferenceExpression e)
        {
            this.Output.Write("base");
        }

        private void GenerateBinaryOperatorExpression(CodeBinaryOperatorExpression e)
        {
            bool flag = false;
            this.Output.Write("(");
            this.GenerateExpression(e.Left);
            this.Output.Write(" ");
            if ((e.Left is CodeBinaryOperatorExpression) || (e.Right is CodeBinaryOperatorExpression))
            {
                if (!this.inNestedBinary)
                {
                    flag = true;
                    this.inNestedBinary = true;
                    this.Indent += 3;
                }
                this.ContinueOnNewLine("");
            }
            this.OutputOperator(e.Operator);
            this.Output.Write(" ");
            this.GenerateExpression(e.Right);
            this.Output.Write(")");
            if (flag)
            {
                this.Indent -= 3;
                this.inNestedBinary = false;
            }
        }

        private void GenerateCastExpression(CodeCastExpression e)
        {
            this.Output.Write("((");
            this.OutputType(e.TargetType);
            this.Output.Write(")(");
            this.GenerateExpression(e.Expression);
            this.Output.Write("))");
        }

        private void GenerateChecksumPragma(CodeChecksumPragma checksumPragma)
        {
            this.Output.Write("#pragma checksum \"");
            this.Output.Write(checksumPragma.FileName);
            this.Output.Write("\" \"");
            this.Output.Write(checksumPragma.ChecksumAlgorithmId.ToString("B", CultureInfo.InvariantCulture));
            this.Output.Write("\" \"");
            if (checksumPragma.ChecksumData != null)
            {
                foreach (byte num in checksumPragma.ChecksumData)
                {
                    this.Output.Write(num.ToString("X2", CultureInfo.InvariantCulture));
                }
            }
            this.Output.WriteLine("\"");
        }

        public void GenerateCodeFromMember(CodeTypeMember member, TextWriter writer, CodeGeneratorOptions options)
        {
            if (this.output != null)
            {
                throw new InvalidOperationException(SR.GetString("CodeGenReentrance"));
            }
            this.options = (options == null) ? new CodeGeneratorOptions() : options;
            this.output = new IndentedTextWriter(writer, this.options.IndentString);
            try
            {
                CodeTypeDeclaration declaredType = new CodeTypeDeclaration();
                this.currentClass = declaredType;
                this.GenerateTypeMember(member, declaredType);
            }
            finally
            {
                this.currentClass = null;
                this.output = null;
                this.options = null;
            }
        }

        private void GenerateCodeRegionDirective(CodeRegionDirective regionDirective)
        {
            if (regionDirective.RegionMode == CodeRegionMode.Start)
            {
                this.Output.Write("#region ");
                this.Output.WriteLine(regionDirective.RegionText);
            }
            else if (regionDirective.RegionMode == CodeRegionMode.End)
            {
                this.Output.WriteLine("#endregion");
            }
        }

        private void GenerateComment(CodeComment e)
        {
            string str = e.DocComment ? "///" : "//";
            this.Output.Write(str);
            this.Output.Write(" ");
            string text = e.Text;
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] != '\0')
                {
                    this.Output.Write(text[i]);
                    if (text[i] == '\r')
                    {
                        if ((i < (text.Length - 1)) && (text[i + 1] == '\n'))
                        {
                            this.Output.Write('\n');
                            i++;
                        }
                        ((IndentedTextWriter) this.Output).InternalOutputTabs();
                        this.Output.Write(str);
                    }
                    else if (text[i] == '\n')
                    {
                        ((IndentedTextWriter) this.Output).InternalOutputTabs();
                        this.Output.Write(str);
                    }
                    else if (((text[i] == '\u2028') || (text[i] == '\u2029')) || (text[i] == '\x0085'))
                    {
                        this.Output.Write(str);
                    }
                }
            }
            this.Output.WriteLine();
        }

        private void GenerateCommentStatement(CodeCommentStatement e)
        {
            if (e.Comment == null)
            {
                throw new ArgumentException(SR.GetString("Argument_NullComment", new object[] { "e" }), "e");
            }
            this.GenerateComment(e.Comment);
        }

        private void GenerateCommentStatements(CodeCommentStatementCollection e)
        {
            foreach (CodeCommentStatement statement in e)
            {
                this.GenerateCommentStatement(statement);
            }
        }

        private void GenerateCompileUnit(CodeCompileUnit e)
        {
            this.GenerateCompileUnitStart(e);
            this.GenerateNamespaces(e);
            this.GenerateCompileUnitEnd(e);
        }

        private void GenerateCompileUnitEnd(CodeCompileUnit e)
        {
            if (e.EndDirectives.Count > 0)
            {
                this.GenerateDirectives(e.EndDirectives);
            }
        }

        private void GenerateCompileUnitStart(CodeCompileUnit e)
        {
            if (e.StartDirectives.Count > 0)
            {
                this.GenerateDirectives(e.StartDirectives);
            }
            this.Output.WriteLine("//------------------------------------------------------------------------------");
            this.Output.Write("// <");
            this.Output.WriteLine(SR.GetString("AutoGen_Comment_Line1"));
            this.Output.Write("//     ");
            this.Output.WriteLine(SR.GetString("AutoGen_Comment_Line2"));
            this.Output.Write("//     ");
            this.Output.Write(SR.GetString("AutoGen_Comment_Line3"));
            this.Output.WriteLine(Environment.Version.ToString());
            this.Output.WriteLine("//");
            this.Output.Write("//     ");
            this.Output.WriteLine(SR.GetString("AutoGen_Comment_Line4"));
            this.Output.Write("//     ");
            this.Output.WriteLine(SR.GetString("AutoGen_Comment_Line5"));
            this.Output.Write("// </");
            this.Output.WriteLine(SR.GetString("AutoGen_Comment_Line1"));
            this.Output.WriteLine("//------------------------------------------------------------------------------");
            this.Output.WriteLine("");
            SortedList list = new SortedList(StringComparer.Ordinal);
            foreach (CodeNamespace namespace2 in e.Namespaces)
            {
                if (string.IsNullOrEmpty(namespace2.Name))
                {
                    namespace2.UserData["GenerateImports"] = false;
                    foreach (CodeNamespaceImport import in namespace2.Imports)
                    {
                        if (!list.Contains(import.Namespace))
                        {
                            list.Add(import.Namespace, import.Namespace);
                        }
                    }
                }
            }
            foreach (string str in list.Keys)
            {
                this.Output.Write("using ");
                this.OutputIdentifier(str);
                this.Output.WriteLine(";");
            }
            if (list.Keys.Count > 0)
            {
                this.Output.WriteLine("");
            }
            if (e.AssemblyCustomAttributes.Count > 0)
            {
                this.GenerateAttributes(e.AssemblyCustomAttributes, "assembly: ");
                this.Output.WriteLine("");
            }
        }

        private void GenerateConditionStatement(CodeConditionStatement e)
        {
            this.Output.Write("if (");
            this.GenerateExpression(e.Condition);
            this.Output.Write(")");
            this.OutputStartingBrace();
            this.Indent++;
            this.GenerateStatements(e.TrueStatements);
            this.Indent--;
            if (e.FalseStatements.Count > 0)
            {
                this.Output.Write("}");
                if (this.Options.ElseOnClosing)
                {
                    this.Output.Write(" ");
                }
                else
                {
                    this.Output.WriteLine("");
                }
                this.Output.Write("else");
                this.OutputStartingBrace();
                this.Indent++;
                this.GenerateStatements(e.FalseStatements);
                this.Indent--;
            }
            this.Output.WriteLine("}");
        }

        private void GenerateConstructor(CodeConstructor e, CodeTypeDeclaration c)
        {
            if (this.IsCurrentClass || this.IsCurrentStruct)
            {
                if (e.CustomAttributes.Count > 0)
                {
                    this.GenerateAttributes(e.CustomAttributes);
                }
                this.OutputMemberAccessModifier(e.Attributes);
                this.OutputIdentifier(this.CurrentTypeName);
                this.Output.Write("(");
                this.OutputParameters(e.Parameters);
                this.Output.Write(")");
                CodeExpressionCollection baseConstructorArgs = e.BaseConstructorArgs;
                CodeExpressionCollection chainedConstructorArgs = e.ChainedConstructorArgs;
                if (baseConstructorArgs.Count > 0)
                {
                    this.Output.WriteLine(" : ");
                    this.Indent++;
                    this.Indent++;
                    this.Output.Write("base(");
                    this.OutputExpressionList(baseConstructorArgs);
                    this.Output.Write(")");
                    this.Indent--;
                    this.Indent--;
                }
                if (chainedConstructorArgs.Count > 0)
                {
                    this.Output.WriteLine(" : ");
                    this.Indent++;
                    this.Indent++;
                    this.Output.Write("this(");
                    this.OutputExpressionList(chainedConstructorArgs);
                    this.Output.Write(")");
                    this.Indent--;
                    this.Indent--;
                }
                this.OutputStartingBrace();
                this.Indent++;
                this.GenerateStatements(e.Statements);
                this.Indent--;
                this.Output.WriteLine("}");
            }
        }

        private void GenerateConstructors(CodeTypeDeclaration e)
        {
            IEnumerator enumerator = e.Members.GetEnumerator();
            while (enumerator.MoveNext())
            {
                if (enumerator.Current is CodeConstructor)
                {
                    this.currentMember = (CodeTypeMember) enumerator.Current;
                    if (this.options.BlankLinesBetweenMembers)
                    {
                        this.Output.WriteLine();
                    }
                    if (this.currentMember.StartDirectives.Count > 0)
                    {
                        this.GenerateDirectives(this.currentMember.StartDirectives);
                    }
                    this.GenerateCommentStatements(this.currentMember.Comments);
                    CodeConstructor current = (CodeConstructor) enumerator.Current;
                    if (current.LinePragma != null)
                    {
                        this.GenerateLinePragmaStart(current.LinePragma);
                    }
                    this.GenerateConstructor(current, e);
                    if (current.LinePragma != null)
                    {
                        this.GenerateLinePragmaEnd(current.LinePragma);
                    }
                    if (this.currentMember.EndDirectives.Count > 0)
                    {
                        this.GenerateDirectives(this.currentMember.EndDirectives);
                    }
                }
            }
        }

        private void GenerateDecimalValue(decimal d)
        {
            this.Output.Write(d.ToString(CultureInfo.InvariantCulture));
            this.Output.Write('m');
        }

        private void GenerateDefaultValueExpression(CodeDefaultValueExpression e)
        {
            this.Output.Write("default(");
            this.OutputType(e.Type);
            this.Output.Write(")");
        }

        private void GenerateDelegateCreateExpression(CodeDelegateCreateExpression e)
        {
            this.Output.Write("new ");
            this.OutputType(e.DelegateType);
            this.Output.Write("(");
            this.GenerateExpression(e.TargetObject);
            this.Output.Write(".");
            this.OutputIdentifier(e.MethodName);
            this.Output.Write(")");
        }

        private void GenerateDelegateInvokeExpression(CodeDelegateInvokeExpression e)
        {
            if (e.TargetObject != null)
            {
                this.GenerateExpression(e.TargetObject);
            }
            this.Output.Write("(");
            this.OutputExpressionList(e.Parameters);
            this.Output.Write(")");
        }

        private void GenerateDirectionExpression(CodeDirectionExpression e)
        {
            this.OutputDirection(e.Direction);
            this.GenerateExpression(e.Expression);
        }

        private void GenerateDirectives(CodeDirectiveCollection directives)
        {
            for (int i = 0; i < directives.Count; i++)
            {
                CodeDirective directive = directives[i];
                if (directive is CodeChecksumPragma)
                {
                    this.GenerateChecksumPragma((CodeChecksumPragma) directive);
                }
                else if (directive is CodeRegionDirective)
                {
                    this.GenerateCodeRegionDirective((CodeRegionDirective) directive);
                }
            }
        }

        private void GenerateDoubleValue(double d)
        {
            if (double.IsNaN(d))
            {
                this.Output.Write("double.NaN");
            }
            else if (double.IsNegativeInfinity(d))
            {
                this.Output.Write("double.NegativeInfinity");
            }
            else if (double.IsPositiveInfinity(d))
            {
                this.Output.Write("double.PositiveInfinity");
            }
            else
            {
                this.Output.Write(d.ToString("R", CultureInfo.InvariantCulture));
                this.Output.Write("D");
            }
        }

        private void GenerateEntryPointMethod(CodeEntryPointMethod e, CodeTypeDeclaration c)
        {
            if (e.CustomAttributes.Count > 0)
            {
                this.GenerateAttributes(e.CustomAttributes);
            }
            this.Output.Write("public static ");
            this.OutputType(e.ReturnType);
            this.Output.Write(" Main()");
            this.OutputStartingBrace();
            this.Indent++;
            this.GenerateStatements(e.Statements);
            this.Indent--;
            this.Output.WriteLine("}");
        }

        private void GenerateEvent(CodeMemberEvent e, CodeTypeDeclaration c)
        {
            if (!this.IsCurrentDelegate && !this.IsCurrentEnum)
            {
                if (e.CustomAttributes.Count > 0)
                {
                    this.GenerateAttributes(e.CustomAttributes);
                }
                if (e.PrivateImplementationType == null)
                {
                    this.OutputMemberAccessModifier(e.Attributes);
                }
                this.Output.Write("event ");
                string name = e.Name;
                if (e.PrivateImplementationType != null)
                {
                    name = this.GetBaseTypeOutput(e.PrivateImplementationType) + "." + name;
                }
                this.OutputTypeNamePair(e.Type, name);
                this.Output.WriteLine(";");
            }
        }

        private void GenerateEventReferenceExpression(CodeEventReferenceExpression e)
        {
            if (e.TargetObject != null)
            {
                this.GenerateExpression(e.TargetObject);
                this.Output.Write(".");
            }
            this.OutputIdentifier(e.EventName);
        }

        private void GenerateEvents(CodeTypeDeclaration e)
        {
            IEnumerator enumerator = e.Members.GetEnumerator();
            while (enumerator.MoveNext())
            {
                if (enumerator.Current is CodeMemberEvent)
                {
                    this.currentMember = (CodeTypeMember) enumerator.Current;
                    if (this.options.BlankLinesBetweenMembers)
                    {
                        this.Output.WriteLine();
                    }
                    if (this.currentMember.StartDirectives.Count > 0)
                    {
                        this.GenerateDirectives(this.currentMember.StartDirectives);
                    }
                    this.GenerateCommentStatements(this.currentMember.Comments);
                    CodeMemberEvent current = (CodeMemberEvent) enumerator.Current;
                    if (current.LinePragma != null)
                    {
                        this.GenerateLinePragmaStart(current.LinePragma);
                    }
                    this.GenerateEvent(current, e);
                    if (current.LinePragma != null)
                    {
                        this.GenerateLinePragmaEnd(current.LinePragma);
                    }
                    if (this.currentMember.EndDirectives.Count > 0)
                    {
                        this.GenerateDirectives(this.currentMember.EndDirectives);
                    }
                }
            }
        }

        private void GenerateExpression(CodeExpression e)
        {
            if (e is CodeArrayCreateExpression)
            {
                this.GenerateArrayCreateExpression((CodeArrayCreateExpression) e);
            }
            else if (e is CodeBaseReferenceExpression)
            {
                this.GenerateBaseReferenceExpression((CodeBaseReferenceExpression) e);
            }
            else if (e is CodeBinaryOperatorExpression)
            {
                this.GenerateBinaryOperatorExpression((CodeBinaryOperatorExpression) e);
            }
            else if (e is CodeCastExpression)
            {
                this.GenerateCastExpression((CodeCastExpression) e);
            }
            else if (e is CodeDelegateCreateExpression)
            {
                this.GenerateDelegateCreateExpression((CodeDelegateCreateExpression) e);
            }
            else if (e is CodeFieldReferenceExpression)
            {
                this.GenerateFieldReferenceExpression((CodeFieldReferenceExpression) e);
            }
            else if (e is CodeArgumentReferenceExpression)
            {
                this.GenerateArgumentReferenceExpression((CodeArgumentReferenceExpression) e);
            }
            else if (e is CodeVariableReferenceExpression)
            {
                this.GenerateVariableReferenceExpression((CodeVariableReferenceExpression) e);
            }
            else if (e is CodeIndexerExpression)
            {
                this.GenerateIndexerExpression((CodeIndexerExpression) e);
            }
            else if (e is CodeArrayIndexerExpression)
            {
                this.GenerateArrayIndexerExpression((CodeArrayIndexerExpression) e);
            }
            else if (e is CodeSnippetExpression)
            {
                this.GenerateSnippetExpression((CodeSnippetExpression) e);
            }
            else if (e is CodeMethodInvokeExpression)
            {
                this.GenerateMethodInvokeExpression((CodeMethodInvokeExpression) e);
            }
            else if (e is CodeMethodReferenceExpression)
            {
                this.GenerateMethodReferenceExpression((CodeMethodReferenceExpression) e);
            }
            else if (e is CodeEventReferenceExpression)
            {
                this.GenerateEventReferenceExpression((CodeEventReferenceExpression) e);
            }
            else if (e is CodeDelegateInvokeExpression)
            {
                this.GenerateDelegateInvokeExpression((CodeDelegateInvokeExpression) e);
            }
            else if (e is CodeObjectCreateExpression)
            {
                this.GenerateObjectCreateExpression((CodeObjectCreateExpression) e);
            }
            else if (e is CodeParameterDeclarationExpression)
            {
                this.GenerateParameterDeclarationExpression((CodeParameterDeclarationExpression) e);
            }
            else if (e is CodeDirectionExpression)
            {
                this.GenerateDirectionExpression((CodeDirectionExpression) e);
            }
            else if (e is CodePrimitiveExpression)
            {
                this.GeneratePrimitiveExpression((CodePrimitiveExpression) e);
            }
            else if (e is CodePropertyReferenceExpression)
            {
                this.GeneratePropertyReferenceExpression((CodePropertyReferenceExpression) e);
            }
            else if (e is CodePropertySetValueReferenceExpression)
            {
                this.GeneratePropertySetValueReferenceExpression((CodePropertySetValueReferenceExpression) e);
            }
            else if (e is CodeThisReferenceExpression)
            {
                this.GenerateThisReferenceExpression((CodeThisReferenceExpression) e);
            }
            else if (e is CodeTypeReferenceExpression)
            {
                this.GenerateTypeReferenceExpression((CodeTypeReferenceExpression) e);
            }
            else if (e is CodeTypeOfExpression)
            {
                this.GenerateTypeOfExpression((CodeTypeOfExpression) e);
            }
            else if (e is CodeDefaultValueExpression)
            {
                this.GenerateDefaultValueExpression((CodeDefaultValueExpression) e);
            }
            else
            {
                if (e == null)
                {
                    throw new ArgumentNullException("e");
                }
                throw new ArgumentException(SR.GetString("InvalidElementType", new object[] { e.GetType().FullName }), "e");
            }
        }

        private void GenerateExpressionStatement(CodeExpressionStatement e)
        {
            this.GenerateExpression(e.Expression);
            if (!this.generatingForLoop)
            {
                this.Output.WriteLine(";");
            }
        }

        private void GenerateField(CodeMemberField e)
        {
            if (!this.IsCurrentDelegate && !this.IsCurrentInterface)
            {
                if (this.IsCurrentEnum)
                {
                    if (e.CustomAttributes.Count > 0)
                    {
                        this.GenerateAttributes(e.CustomAttributes);
                    }
                    this.OutputIdentifier(e.Name);
                    if (e.InitExpression != null)
                    {
                        this.Output.Write(" = ");
                        this.GenerateExpression(e.InitExpression);
                    }
                    this.Output.WriteLine(",");
                }
                else
                {
                    if (e.CustomAttributes.Count > 0)
                    {
                        this.GenerateAttributes(e.CustomAttributes);
                    }
                    this.OutputMemberAccessModifier(e.Attributes);
                    this.OutputVTableModifier(e.Attributes);
                    this.OutputFieldScopeModifier(e.Attributes);
                    this.OutputTypeNamePair(e.Type, e.Name);
                    if (e.InitExpression != null)
                    {
                        this.Output.Write(" = ");
                        this.GenerateExpression(e.InitExpression);
                    }
                    this.Output.WriteLine(";");
                }
            }
        }

        private void GenerateFieldReferenceExpression(CodeFieldReferenceExpression e)
        {
            if (e.TargetObject != null)
            {
                this.GenerateExpression(e.TargetObject);
                this.Output.Write(".");
            }
            this.OutputIdentifier(e.FieldName);
        }

        private void GenerateFields(CodeTypeDeclaration e)
        {
            IEnumerator enumerator = e.Members.GetEnumerator();
            while (enumerator.MoveNext())
            {
                if (enumerator.Current is CodeMemberField)
                {
                    this.currentMember = (CodeTypeMember) enumerator.Current;
                    if (this.options.BlankLinesBetweenMembers)
                    {
                        this.Output.WriteLine();
                    }
                    if (this.currentMember.StartDirectives.Count > 0)
                    {
                        this.GenerateDirectives(this.currentMember.StartDirectives);
                    }
                    this.GenerateCommentStatements(this.currentMember.Comments);
                    CodeMemberField current = (CodeMemberField) enumerator.Current;
                    if (current.LinePragma != null)
                    {
                        this.GenerateLinePragmaStart(current.LinePragma);
                    }
                    this.GenerateField(current);
                    if (current.LinePragma != null)
                    {
                        this.GenerateLinePragmaEnd(current.LinePragma);
                    }
                    if (this.currentMember.EndDirectives.Count > 0)
                    {
                        this.GenerateDirectives(this.currentMember.EndDirectives);
                    }
                }
            }
        }

        private void GenerateGotoStatement(CodeGotoStatement e)
        {
            this.Output.Write("goto ");
            this.Output.Write(e.Label);
            this.Output.WriteLine(";");
        }

        private void GenerateIndexerExpression(CodeIndexerExpression e)
        {
            this.GenerateExpression(e.TargetObject);
            this.Output.Write("[");
            bool flag = true;
            foreach (CodeExpression expression in e.Indices)
            {
                if (flag)
                {
                    flag = false;
                }
                else
                {
                    this.Output.Write(", ");
                }
                this.GenerateExpression(expression);
            }
            this.Output.Write("]");
        }

        private void GenerateIterationStatement(CodeIterationStatement e)
        {
            this.generatingForLoop = true;
            this.Output.Write("for (");
            this.GenerateStatement(e.InitStatement);
            this.Output.Write("; ");
            this.GenerateExpression(e.TestExpression);
            this.Output.Write("; ");
            this.GenerateStatement(e.IncrementStatement);
            this.Output.Write(")");
            this.OutputStartingBrace();
            this.generatingForLoop = false;
            this.Indent++;
            this.GenerateStatements(e.Statements);
            this.Indent--;
            this.Output.WriteLine("}");
        }

        private void GenerateLabeledStatement(CodeLabeledStatement e)
        {
            this.Indent--;
            this.Output.Write(e.Label);
            this.Output.WriteLine(":");
            this.Indent++;
            if (e.Statement != null)
            {
                this.GenerateStatement(e.Statement);
            }
        }

        private void GenerateLinePragmaEnd(CodeLinePragma e)
        {
            this.Output.WriteLine();
            this.Output.WriteLine("#line default");
            this.Output.WriteLine("#line hidden");
        }

        private void GenerateLinePragmaStart(CodeLinePragma e)
        {
            this.Output.WriteLine("");
            this.Output.Write("#line ");
            this.Output.Write(e.LineNumber);
            this.Output.Write(" \"");
            this.Output.Write(e.FileName);
            this.Output.Write("\"");
            this.Output.WriteLine("");
        }

        private void GenerateMethod(CodeMemberMethod e, CodeTypeDeclaration c)
        {
            if ((this.IsCurrentClass || this.IsCurrentStruct) || this.IsCurrentInterface)
            {
                if (e.CustomAttributes.Count > 0)
                {
                    this.GenerateAttributes(e.CustomAttributes);
                }
                if (e.ReturnTypeCustomAttributes.Count > 0)
                {
                    this.GenerateAttributes(e.ReturnTypeCustomAttributes, "return: ");
                }
                if (!this.IsCurrentInterface)
                {
                    if (e.PrivateImplementationType == null)
                    {
                        this.OutputMemberAccessModifier(e.Attributes);
                        this.OutputVTableModifier(e.Attributes);
                        this.OutputMemberScopeModifier(e.Attributes);
                    }
                }
                else
                {
                    this.OutputVTableModifier(e.Attributes);
                }
                this.OutputType(e.ReturnType);
                this.Output.Write(" ");
                if (e.PrivateImplementationType != null)
                {
                    this.Output.Write(this.GetBaseTypeOutput(e.PrivateImplementationType));
                    this.Output.Write(".");
                }
                this.OutputIdentifier(e.Name);
                this.OutputTypeParameters(e.TypeParameters);
                this.Output.Write("(");
                this.OutputParameters(e.Parameters);
                this.Output.Write(")");
                this.OutputTypeParameterConstraints(e.TypeParameters);
                if (!this.IsCurrentInterface && ((e.Attributes & MemberAttributes.ScopeMask) != MemberAttributes.Abstract))
                {
                    this.OutputStartingBrace();
                    this.Indent++;
                    this.GenerateStatements(e.Statements);
                    this.Indent--;
                    this.Output.WriteLine("}");
                }
                else
                {
                    this.Output.WriteLine(";");
                }
            }
        }

        private void GenerateMethodInvokeExpression(CodeMethodInvokeExpression e)
        {
            this.GenerateMethodReferenceExpression(e.Method);
            this.Output.Write("(");
            this.OutputExpressionList(e.Parameters);
            this.Output.Write(")");
        }

        private void GenerateMethodReferenceExpression(CodeMethodReferenceExpression e)
        {
            if (e.TargetObject != null)
            {
                if (e.TargetObject is CodeBinaryOperatorExpression)
                {
                    this.Output.Write("(");
                    this.GenerateExpression(e.TargetObject);
                    this.Output.Write(")");
                }
                else
                {
                    this.GenerateExpression(e.TargetObject);
                }
                this.Output.Write(".");
            }
            this.OutputIdentifier(e.MethodName);
            if (e.TypeArguments.Count > 0)
            {
                this.Output.Write(this.GetTypeArgumentsOutput(e.TypeArguments));
            }
        }

        private void GenerateMethodReturnStatement(CodeMethodReturnStatement e)
        {
            this.Output.Write("return");
            if (e.Expression != null)
            {
                this.Output.Write(" ");
                this.GenerateExpression(e.Expression);
            }
            this.Output.WriteLine(";");
        }

        private void GenerateMethods(CodeTypeDeclaration e)
        {
            IEnumerator enumerator = e.Members.GetEnumerator();
            while (enumerator.MoveNext())
            {
                if (((enumerator.Current is CodeMemberMethod) && !(enumerator.Current is CodeTypeConstructor)) && !(enumerator.Current is CodeConstructor))
                {
                    this.currentMember = (CodeTypeMember) enumerator.Current;
                    if (this.options.BlankLinesBetweenMembers)
                    {
                        this.Output.WriteLine();
                    }
                    if (this.currentMember.StartDirectives.Count > 0)
                    {
                        this.GenerateDirectives(this.currentMember.StartDirectives);
                    }
                    this.GenerateCommentStatements(this.currentMember.Comments);
                    CodeMemberMethod current = (CodeMemberMethod) enumerator.Current;
                    if (current.LinePragma != null)
                    {
                        this.GenerateLinePragmaStart(current.LinePragma);
                    }
                    if (enumerator.Current is CodeEntryPointMethod)
                    {
                        this.GenerateEntryPointMethod((CodeEntryPointMethod) enumerator.Current, e);
                    }
                    else
                    {
                        this.GenerateMethod(current, e);
                    }
                    if (current.LinePragma != null)
                    {
                        this.GenerateLinePragmaEnd(current.LinePragma);
                    }
                    if (this.currentMember.EndDirectives.Count > 0)
                    {
                        this.GenerateDirectives(this.currentMember.EndDirectives);
                    }
                }
            }
        }

        private void GenerateNamespace(CodeNamespace e)
        {
            this.GenerateCommentStatements(e.Comments);
            this.GenerateNamespaceStart(e);
            if (this.GetUserData(e, "GenerateImports", true))
            {
                this.GenerateNamespaceImports(e);
            }
            this.Output.WriteLine("");
            this.GenerateTypes(e);
            this.GenerateNamespaceEnd(e);
        }

        private void GenerateNamespaceEnd(CodeNamespace e)
        {
            if ((e.Name != null) && (e.Name.Length > 0))
            {
                this.Indent--;
                this.Output.WriteLine("}");
            }
        }

        private void GenerateNamespaceImport(CodeNamespaceImport e)
        {
            this.Output.Write("using ");
            this.OutputIdentifier(e.Namespace);
            this.Output.WriteLine(";");
        }

        private void GenerateNamespaceImports(CodeNamespace e)
        {
            IEnumerator enumerator = e.Imports.GetEnumerator();
            while (enumerator.MoveNext())
            {
                CodeNamespaceImport current = (CodeNamespaceImport) enumerator.Current;
                if (current.LinePragma != null)
                {
                    this.GenerateLinePragmaStart(current.LinePragma);
                }
                this.GenerateNamespaceImport(current);
                if (current.LinePragma != null)
                {
                    this.GenerateLinePragmaEnd(current.LinePragma);
                }
            }
        }

        private void GenerateNamespaces(CodeCompileUnit e)
        {
            foreach (CodeNamespace namespace2 in e.Namespaces)
            {
                ((ICodeGenerator) this).GenerateCodeFromNamespace(namespace2, this.output.InnerWriter, this.options);
            }
        }

        private void GenerateNamespaceStart(CodeNamespace e)
        {
            if ((e.Name != null) && (e.Name.Length > 0))
            {
                this.Output.Write("namespace ");
                string[] strArray = e.Name.Split(new char[] { '.' });
                this.OutputIdentifier(strArray[0]);
                for (int i = 1; i < strArray.Length; i++)
                {
                    this.Output.Write(".");
                    this.OutputIdentifier(strArray[i]);
                }
                this.OutputStartingBrace();
                this.Indent++;
            }
        }

        private void GenerateNestedTypes(CodeTypeDeclaration e)
        {
            IEnumerator enumerator = e.Members.GetEnumerator();
            while (enumerator.MoveNext())
            {
                if (enumerator.Current is CodeTypeDeclaration)
                {
                    if (this.options.BlankLinesBetweenMembers)
                    {
                        this.Output.WriteLine();
                    }
                    CodeTypeDeclaration current = (CodeTypeDeclaration) enumerator.Current;
                    ((ICodeGenerator) this).GenerateCodeFromType(current, this.output.InnerWriter, this.options);
                }
            }
        }

        private void GenerateObjectCreateExpression(CodeObjectCreateExpression e)
        {
            this.Output.Write("new ");
            this.OutputType(e.CreateType);
            this.Output.Write("(");
            this.OutputExpressionList(e.Parameters);
            this.Output.Write(")");
        }

        private void GenerateParameterDeclarationExpression(CodeParameterDeclarationExpression e)
        {
            if (e.CustomAttributes.Count > 0)
            {
                this.GenerateAttributes(e.CustomAttributes, null, true);
            }
            this.OutputDirection(e.Direction);
            this.OutputTypeNamePair(e.Type, e.Name);
        }

        private void GeneratePrimitiveChar(char c)
        {
            this.Output.Write('\'');
            switch (c)
            {
                case '\t':
                    this.Output.Write(@"\t");
                    break;

                case '\n':
                    this.Output.Write(@"\n");
                    break;

                case '\r':
                    this.Output.Write(@"\r");
                    break;

                case '"':
                    this.Output.Write("\\\"");
                    break;

                case '\0':
                    this.Output.Write(@"\0");
                    break;

                case '\'':
                    this.Output.Write(@"\'");
                    break;

                case '\\':
                    this.Output.Write(@"\\");
                    break;

                case '\x0084':
                case '\x0085':
                case '\u2028':
                case '\u2029':
                    this.AppendEscapedChar(null, c);
                    break;

                default:
                    if (char.IsSurrogate(c))
                    {
                        this.AppendEscapedChar(null, c);
                    }
                    else
                    {
                        this.Output.Write(c);
                    }
                    break;
            }
            this.Output.Write('\'');
        }

        private void GeneratePrimitiveExpression(CodePrimitiveExpression e)
        {
            if (e.Value is char)
            {
                this.GeneratePrimitiveChar((char) e.Value);
            }
            else if (e.Value is sbyte)
            {
                sbyte num = (sbyte) e.Value;
                this.Output.Write(num.ToString(CultureInfo.InvariantCulture));
            }
            else if (e.Value is ushort)
            {
                ushort num2 = (ushort) e.Value;
                this.Output.Write(num2.ToString(CultureInfo.InvariantCulture));
            }
            else if (e.Value is uint)
            {
                uint num3 = (uint) e.Value;
                this.Output.Write(num3.ToString(CultureInfo.InvariantCulture));
                this.Output.Write("u");
            }
            else if (e.Value is ulong)
            {
                ulong num4 = (ulong) e.Value;
                this.Output.Write(num4.ToString(CultureInfo.InvariantCulture));
                this.Output.Write("ul");
            }
            else
            {
                this.GeneratePrimitiveExpressionBase(e);
            }
        }

        private void GeneratePrimitiveExpressionBase(CodePrimitiveExpression e)
        {
            if (e.Value == null)
            {
                this.Output.Write(this.NullToken);
            }
            else if (e.Value is string)
            {
                this.Output.Write(this.QuoteSnippetString((string) e.Value));
            }
            else if (e.Value is char)
            {
                this.Output.Write("'" + e.Value.ToString() + "'");
            }
            else if (e.Value is byte)
            {
                byte num = (byte) e.Value;
                this.Output.Write(num.ToString(CultureInfo.InvariantCulture));
            }
            else if (e.Value is short)
            {
                short num2 = (short) e.Value;
                this.Output.Write(num2.ToString(CultureInfo.InvariantCulture));
            }
            else if (e.Value is int)
            {
                int num3 = (int) e.Value;
                this.Output.Write(num3.ToString(CultureInfo.InvariantCulture));
            }
            else if (e.Value is long)
            {
                long num4 = (long) e.Value;
                this.Output.Write(num4.ToString(CultureInfo.InvariantCulture));
            }
            else if (e.Value is float)
            {
                this.GenerateSingleFloatValue((float) e.Value);
            }
            else if (e.Value is double)
            {
                this.GenerateDoubleValue((double) e.Value);
            }
            else if (e.Value is decimal)
            {
                this.GenerateDecimalValue((decimal) e.Value);
            }
            else
            {
                if (!(e.Value is bool))
                {
                    throw new ArgumentException(SR.GetString("InvalidPrimitiveType", new object[] { e.Value.GetType().ToString() }));
                }
                if ((bool) e.Value)
                {
                    this.Output.Write("true");
                }
                else
                {
                    this.Output.Write("false");
                }
            }
        }

        private void GenerateProperties(CodeTypeDeclaration e)
        {
            IEnumerator enumerator = e.Members.GetEnumerator();
            while (enumerator.MoveNext())
            {
                if (enumerator.Current is CodeMemberProperty)
                {
                    this.currentMember = (CodeTypeMember) enumerator.Current;
                    if (this.options.BlankLinesBetweenMembers)
                    {
                        this.Output.WriteLine();
                    }
                    if (this.currentMember.StartDirectives.Count > 0)
                    {
                        this.GenerateDirectives(this.currentMember.StartDirectives);
                    }
                    this.GenerateCommentStatements(this.currentMember.Comments);
                    CodeMemberProperty current = (CodeMemberProperty) enumerator.Current;
                    if (current.LinePragma != null)
                    {
                        this.GenerateLinePragmaStart(current.LinePragma);
                    }
                    this.GenerateProperty(current, e);
                    if (current.LinePragma != null)
                    {
                        this.GenerateLinePragmaEnd(current.LinePragma);
                    }
                    if (this.currentMember.EndDirectives.Count > 0)
                    {
                        this.GenerateDirectives(this.currentMember.EndDirectives);
                    }
                }
            }
        }

        private void GenerateProperty(CodeMemberProperty e, CodeTypeDeclaration c)
        {
            if ((this.IsCurrentClass || this.IsCurrentStruct) || this.IsCurrentInterface)
            {
                if (e.CustomAttributes.Count > 0)
                {
                    this.GenerateAttributes(e.CustomAttributes);
                }
                if (!this.IsCurrentInterface)
                {
                    if (e.PrivateImplementationType == null)
                    {
                        this.OutputMemberAccessModifier(e.Attributes);
                        this.OutputVTableModifier(e.Attributes);
                        this.OutputMemberScopeModifier(e.Attributes);
                    }
                }
                else
                {
                    this.OutputVTableModifier(e.Attributes);
                }
                this.OutputType(e.Type);
                this.Output.Write(" ");
                if ((e.PrivateImplementationType != null) && !this.IsCurrentInterface)
                {
                    this.Output.Write(this.GetBaseTypeOutput(e.PrivateImplementationType));
                    this.Output.Write(".");
                }
                if ((e.Parameters.Count > 0) && (string.Compare(e.Name, "Item", StringComparison.OrdinalIgnoreCase) == 0))
                {
                    this.Output.Write("this[");
                    this.OutputParameters(e.Parameters);
                    this.Output.Write("]");
                }
                else
                {
                    this.OutputIdentifier(e.Name);
                }
                this.OutputStartingBrace();
                this.Indent++;
                if (e.HasGet)
                {
                    if (this.IsCurrentInterface || ((e.Attributes & MemberAttributes.ScopeMask) == MemberAttributes.Abstract))
                    {
                        this.Output.WriteLine("get;");
                    }
                    else
                    {
                        this.Output.Write("get");
                        this.OutputStartingBrace();
                        this.Indent++;
                        this.GenerateStatements(e.GetStatements);
                        this.Indent--;
                        this.Output.WriteLine("}");
                    }
                }
                if (e.HasSet)
                {
                    if (this.IsCurrentInterface || ((e.Attributes & MemberAttributes.ScopeMask) == MemberAttributes.Abstract))
                    {
                        this.Output.WriteLine("set;");
                    }
                    else
                    {
                        this.Output.Write("set");
                        this.OutputStartingBrace();
                        this.Indent++;
                        this.GenerateStatements(e.SetStatements);
                        this.Indent--;
                        this.Output.WriteLine("}");
                    }
                }
                this.Indent--;
                this.Output.WriteLine("}");
            }
        }

        private void GeneratePropertyReferenceExpression(CodePropertyReferenceExpression e)
        {
            if (e.TargetObject != null)
            {
                this.GenerateExpression(e.TargetObject);
                this.Output.Write(".");
            }
            this.OutputIdentifier(e.PropertyName);
        }

        private void GeneratePropertySetValueReferenceExpression(CodePropertySetValueReferenceExpression e)
        {
            this.Output.Write("value");
        }

        private void GenerateRemoveEventStatement(CodeRemoveEventStatement e)
        {
            this.GenerateEventReferenceExpression(e.Event);
            this.Output.Write(" -= ");
            this.GenerateExpression(e.Listener);
            this.Output.WriteLine(";");
        }

        private void GenerateSingleFloatValue(float s)
        {
            if (float.IsNaN(s))
            {
                this.Output.Write("float.NaN");
            }
            else if (float.IsNegativeInfinity(s))
            {
                this.Output.Write("float.NegativeInfinity");
            }
            else if (float.IsPositiveInfinity(s))
            {
                this.Output.Write("float.PositiveInfinity");
            }
            else
            {
                this.Output.Write(s.ToString(CultureInfo.InvariantCulture));
                this.Output.Write('F');
            }
        }

        private void GenerateSnippetCompileUnit(CodeSnippetCompileUnit e)
        {
            this.GenerateDirectives(e.StartDirectives);
            if (e.LinePragma != null)
            {
                this.GenerateLinePragmaStart(e.LinePragma);
            }
            this.Output.WriteLine(e.Value);
            if (e.LinePragma != null)
            {
                this.GenerateLinePragmaEnd(e.LinePragma);
            }
            if (e.EndDirectives.Count > 0)
            {
                this.GenerateDirectives(e.EndDirectives);
            }
        }

        private void GenerateSnippetExpression(CodeSnippetExpression e)
        {
            this.Output.Write(e.Value);
        }

        private void GenerateSnippetMember(CodeSnippetTypeMember e)
        {
            this.Output.Write(e.Text);
        }

        private void GenerateSnippetMembers(CodeTypeDeclaration e)
        {
            IEnumerator enumerator = e.Members.GetEnumerator();
            bool flag = false;
            while (enumerator.MoveNext())
            {
                if (enumerator.Current is CodeSnippetTypeMember)
                {
                    flag = true;
                    this.currentMember = (CodeTypeMember) enumerator.Current;
                    if (this.options.BlankLinesBetweenMembers)
                    {
                        this.Output.WriteLine();
                    }
                    if (this.currentMember.StartDirectives.Count > 0)
                    {
                        this.GenerateDirectives(this.currentMember.StartDirectives);
                    }
                    this.GenerateCommentStatements(this.currentMember.Comments);
                    CodeSnippetTypeMember current = (CodeSnippetTypeMember) enumerator.Current;
                    if (current.LinePragma != null)
                    {
                        this.GenerateLinePragmaStart(current.LinePragma);
                    }
                    int indent = this.Indent;
                    this.Indent = 0;
                    this.GenerateSnippetMember(current);
                    this.Indent = indent;
                    if (current.LinePragma != null)
                    {
                        this.GenerateLinePragmaEnd(current.LinePragma);
                    }
                    if (this.currentMember.EndDirectives.Count > 0)
                    {
                        this.GenerateDirectives(this.currentMember.EndDirectives);
                    }
                }
            }
            if (flag)
            {
                this.Output.WriteLine();
            }
        }

        private void GenerateSnippetStatement(CodeSnippetStatement e)
        {
            this.Output.WriteLine(e.Value);
        }

        private void GenerateStatement(CodeStatement e)
        {
            if (e.StartDirectives.Count > 0)
            {
                this.GenerateDirectives(e.StartDirectives);
            }
            if (e.LinePragma != null)
            {
                this.GenerateLinePragmaStart(e.LinePragma);
            }
            if (e is CodeCommentStatement)
            {
                this.GenerateCommentStatement((CodeCommentStatement) e);
            }
            else if (e is CodeMethodReturnStatement)
            {
                this.GenerateMethodReturnStatement((CodeMethodReturnStatement) e);
            }
            else if (e is CodeConditionStatement)
            {
                this.GenerateConditionStatement((CodeConditionStatement) e);
            }
            else if (e is CodeTryCatchFinallyStatement)
            {
                this.GenerateTryCatchFinallyStatement((CodeTryCatchFinallyStatement) e);
            }
            else if (e is CodeAssignStatement)
            {
                this.GenerateAssignStatement((CodeAssignStatement) e);
            }
            else if (e is CodeExpressionStatement)
            {
                this.GenerateExpressionStatement((CodeExpressionStatement) e);
            }
            else if (e is CodeIterationStatement)
            {
                this.GenerateIterationStatement((CodeIterationStatement) e);
            }
            else if (e is CodeThrowExceptionStatement)
            {
                this.GenerateThrowExceptionStatement((CodeThrowExceptionStatement) e);
            }
            else if (e is CodeSnippetStatement)
            {
                int indent = this.Indent;
                this.Indent = 0;
                this.GenerateSnippetStatement((CodeSnippetStatement) e);
                this.Indent = indent;
            }
            else if (e is CodeVariableDeclarationStatement)
            {
                this.GenerateVariableDeclarationStatement((CodeVariableDeclarationStatement) e);
            }
            else if (e is CodeAttachEventStatement)
            {
                this.GenerateAttachEventStatement((CodeAttachEventStatement) e);
            }
            else if (e is CodeRemoveEventStatement)
            {
                this.GenerateRemoveEventStatement((CodeRemoveEventStatement) e);
            }
            else if (e is CodeGotoStatement)
            {
                this.GenerateGotoStatement((CodeGotoStatement) e);
            }
            else
            {
                if (!(e is CodeLabeledStatement))
                {
                    throw new ArgumentException(SR.GetString("InvalidElementType", new object[] { e.GetType().FullName }), "e");
                }
                this.GenerateLabeledStatement((CodeLabeledStatement) e);
            }
            if (e.LinePragma != null)
            {
                this.GenerateLinePragmaEnd(e.LinePragma);
            }
            if (e.EndDirectives.Count > 0)
            {
                this.GenerateDirectives(e.EndDirectives);
            }
        }

        private void GenerateStatements(CodeStatementCollection stms)
        {
            IEnumerator enumerator = stms.GetEnumerator();
            while (enumerator.MoveNext())
            {
                ((ICodeGenerator) this).GenerateCodeFromStatement((CodeStatement) enumerator.Current, this.output.InnerWriter, this.options);
            }
        }

        private void GenerateThisReferenceExpression(CodeThisReferenceExpression e)
        {
            this.Output.Write("this");
        }

        private void GenerateThrowExceptionStatement(CodeThrowExceptionStatement e)
        {
            this.Output.Write("throw");
            if (e.ToThrow != null)
            {
                this.Output.Write(" ");
                this.GenerateExpression(e.ToThrow);
            }
            this.Output.WriteLine(";");
        }

        private void GenerateTryCatchFinallyStatement(CodeTryCatchFinallyStatement e)
        {
            this.Output.Write("try");
            this.OutputStartingBrace();
            this.Indent++;
            this.GenerateStatements(e.TryStatements);
            this.Indent--;
            CodeCatchClauseCollection catchClauses = e.CatchClauses;
            if (catchClauses.Count > 0)
            {
                IEnumerator enumerator = catchClauses.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    this.Output.Write("}");
                    if (this.Options.ElseOnClosing)
                    {
                        this.Output.Write(" ");
                    }
                    else
                    {
                        this.Output.WriteLine("");
                    }
                    CodeCatchClause current = (CodeCatchClause) enumerator.Current;
                    this.Output.Write("catch (");
                    this.OutputType(current.CatchExceptionType);
                    this.Output.Write(" ");
                    this.OutputIdentifier(current.LocalName);
                    this.Output.Write(")");
                    this.OutputStartingBrace();
                    this.Indent++;
                    this.GenerateStatements(current.Statements);
                    this.Indent--;
                }
            }
            CodeStatementCollection finallyStatements = e.FinallyStatements;
            if (finallyStatements.Count > 0)
            {
                this.Output.Write("}");
                if (this.Options.ElseOnClosing)
                {
                    this.Output.Write(" ");
                }
                else
                {
                    this.Output.WriteLine("");
                }
                this.Output.Write("finally");
                this.OutputStartingBrace();
                this.Indent++;
                this.GenerateStatements(finallyStatements);
                this.Indent--;
            }
            this.Output.WriteLine("}");
        }

        private void GenerateType(CodeTypeDeclaration e)
        {
            this.currentClass = e;
            if (e.StartDirectives.Count > 0)
            {
                this.GenerateDirectives(e.StartDirectives);
            }
            this.GenerateCommentStatements(e.Comments);
            if (e.LinePragma != null)
            {
                this.GenerateLinePragmaStart(e.LinePragma);
            }
            this.GenerateTypeStart(e);
            if (this.Options.VerbatimOrder)
            {
                foreach (CodeTypeMember member in e.Members)
                {
                    this.GenerateTypeMember(member, e);
                }
            }
            else
            {
                this.GenerateFields(e);
                this.GenerateSnippetMembers(e);
                this.GenerateTypeConstructors(e);
                this.GenerateConstructors(e);
                this.GenerateProperties(e);
                this.GenerateEvents(e);
                this.GenerateMethods(e);
                this.GenerateNestedTypes(e);
            }
            this.currentClass = e;
            this.GenerateTypeEnd(e);
            if (e.LinePragma != null)
            {
                this.GenerateLinePragmaEnd(e.LinePragma);
            }
            if (e.EndDirectives.Count > 0)
            {
                this.GenerateDirectives(e.EndDirectives);
            }
        }

        private void GenerateTypeConstructor(CodeTypeConstructor e)
        {
            if (this.IsCurrentClass || this.IsCurrentStruct)
            {
                if (e.CustomAttributes.Count > 0)
                {
                    this.GenerateAttributes(e.CustomAttributes);
                }
                this.Output.Write("static ");
                this.Output.Write(this.CurrentTypeName);
                this.Output.Write("()");
                this.OutputStartingBrace();
                this.Indent++;
                this.GenerateStatements(e.Statements);
                this.Indent--;
                this.Output.WriteLine("}");
            }
        }

        private void GenerateTypeConstructors(CodeTypeDeclaration e)
        {
            IEnumerator enumerator = e.Members.GetEnumerator();
            while (enumerator.MoveNext())
            {
                if (enumerator.Current is CodeTypeConstructor)
                {
                    this.currentMember = (CodeTypeMember) enumerator.Current;
                    if (this.options.BlankLinesBetweenMembers)
                    {
                        this.Output.WriteLine();
                    }
                    if (this.currentMember.StartDirectives.Count > 0)
                    {
                        this.GenerateDirectives(this.currentMember.StartDirectives);
                    }
                    this.GenerateCommentStatements(this.currentMember.Comments);
                    CodeTypeConstructor current = (CodeTypeConstructor) enumerator.Current;
                    if (current.LinePragma != null)
                    {
                        this.GenerateLinePragmaStart(current.LinePragma);
                    }
                    this.GenerateTypeConstructor(current);
                    if (current.LinePragma != null)
                    {
                        this.GenerateLinePragmaEnd(current.LinePragma);
                    }
                    if (this.currentMember.EndDirectives.Count > 0)
                    {
                        this.GenerateDirectives(this.currentMember.EndDirectives);
                    }
                }
            }
        }

        private void GenerateTypeEnd(CodeTypeDeclaration e)
        {
            if (!this.IsCurrentDelegate)
            {
                this.Indent--;
                this.Output.WriteLine("}");
            }
        }

        private void GenerateTypeMember(CodeTypeMember member, CodeTypeDeclaration declaredType)
        {
            if (this.options.BlankLinesBetweenMembers)
            {
                this.Output.WriteLine();
            }
            if (member is CodeTypeDeclaration)
            {
                ((ICodeGenerator) this).GenerateCodeFromType((CodeTypeDeclaration) member, this.output.InnerWriter, this.options);
                this.currentClass = declaredType;
            }
            else
            {
                if (member.StartDirectives.Count > 0)
                {
                    this.GenerateDirectives(member.StartDirectives);
                }
                this.GenerateCommentStatements(member.Comments);
                if (member.LinePragma != null)
                {
                    this.GenerateLinePragmaStart(member.LinePragma);
                }
                if (member is CodeMemberField)
                {
                    this.GenerateField((CodeMemberField) member);
                }
                else if (member is CodeMemberProperty)
                {
                    this.GenerateProperty((CodeMemberProperty) member, declaredType);
                }
                else if (member is CodeMemberMethod)
                {
                    if (member is CodeConstructor)
                    {
                        this.GenerateConstructor((CodeConstructor) member, declaredType);
                    }
                    else if (member is CodeTypeConstructor)
                    {
                        this.GenerateTypeConstructor((CodeTypeConstructor) member);
                    }
                    else if (member is CodeEntryPointMethod)
                    {
                        this.GenerateEntryPointMethod((CodeEntryPointMethod) member, declaredType);
                    }
                    else
                    {
                        this.GenerateMethod((CodeMemberMethod) member, declaredType);
                    }
                }
                else if (member is CodeMemberEvent)
                {
                    this.GenerateEvent((CodeMemberEvent) member, declaredType);
                }
                else if (member is CodeSnippetTypeMember)
                {
                    int indent = this.Indent;
                    this.Indent = 0;
                    this.GenerateSnippetMember((CodeSnippetTypeMember) member);
                    this.Indent = indent;
                    this.Output.WriteLine();
                }
                if (member.LinePragma != null)
                {
                    this.GenerateLinePragmaEnd(member.LinePragma);
                }
                if (member.EndDirectives.Count > 0)
                {
                    this.GenerateDirectives(member.EndDirectives);
                }
            }
        }

        private void GenerateTypeOfExpression(CodeTypeOfExpression e)
        {
            this.Output.Write("typeof(");
            this.OutputType(e.Type);
            this.Output.Write(")");
        }

        private void GenerateTypeReferenceExpression(CodeTypeReferenceExpression e)
        {
            this.OutputType(e.Type);
        }

        private void GenerateTypes(CodeNamespace e)
        {
            foreach (CodeTypeDeclaration declaration in e.Types)
            {
                if (this.options.BlankLinesBetweenMembers)
                {
                    this.Output.WriteLine();
                }
                ((ICodeGenerator) this).GenerateCodeFromType(declaration, this.output.InnerWriter, this.options);
            }
        }

        private void GenerateTypeStart(CodeTypeDeclaration e)
        {
            if (e.CustomAttributes.Count > 0)
            {
                this.GenerateAttributes(e.CustomAttributes);
            }
            if (!this.IsCurrentDelegate)
            {
                this.OutputTypeAttributes(e);
                this.OutputIdentifier(e.Name);
                this.OutputTypeParameters(e.TypeParameters);
                bool flag = true;
                foreach (CodeTypeReference reference in e.BaseTypes)
                {
                    if (flag)
                    {
                        this.Output.Write(" : ");
                        flag = false;
                    }
                    else
                    {
                        this.Output.Write(", ");
                    }
                    this.OutputType(reference);
                }
                this.OutputTypeParameterConstraints(e.TypeParameters);
                this.OutputStartingBrace();
                this.Indent++;
            }
            else
            {
                switch ((e.TypeAttributes & TypeAttributes.NestedFamORAssem))
                {
                    case TypeAttributes.Public:
                        this.Output.Write("public ");
                        break;
                }
                CodeTypeDelegate delegate2 = (CodeTypeDelegate) e;
                this.Output.Write("delegate ");
                this.OutputType(delegate2.ReturnType);
                this.Output.Write(" ");
                this.OutputIdentifier(e.Name);
                this.Output.Write("(");
                this.OutputParameters(delegate2.Parameters);
                this.Output.WriteLine(");");
            }
        }

        private void GenerateVariableDeclarationStatement(CodeVariableDeclarationStatement e)
        {
            this.OutputTypeNamePair(e.Type, e.Name);
            if (e.InitExpression != null)
            {
                this.Output.Write(" = ");
                this.GenerateExpression(e.InitExpression);
            }
            if (!this.generatingForLoop)
            {
                this.Output.WriteLine(";");
            }
        }

        private void GenerateVariableReferenceExpression(CodeVariableReferenceExpression e)
        {
            this.OutputIdentifier(e.VariableName);
        }

        private string GetBaseTypeOutput(CodeTypeReference typeRef)
        {
            string baseType = typeRef.BaseType;
            if (baseType.Length == 0)
            {
                return "void";
            }
            switch (baseType.ToLower(CultureInfo.InvariantCulture))
            {
                case "system.int16":
                    return "short";

                case "system.int32":
                    return "int";

                case "system.int64":
                    return "long";

                case "system.string":
                    return "string";

                case "system.object":
                    return "object";

                case "system.boolean":
                    return "bool";

                case "system.void":
                    return "void";

                case "system.char":
                    return "char";

                case "system.byte":
                    return "byte";

                case "system.uint16":
                    return "ushort";

                case "system.uint32":
                    return "uint";

                case "system.uint64":
                    return "ulong";

                case "system.sbyte":
                    return "sbyte";

                case "system.single":
                    return "float";

                case "system.double":
                    return "double";

                case "system.decimal":
                    return "decimal";
            }
            StringBuilder sb = new StringBuilder(baseType.Length + 10);
            if ((typeRef.Options & CodeTypeReferenceOptions.GlobalReference) != 0)
            {
                sb.Append("global::");
            }
            string str3 = typeRef.BaseType;
            int startIndex = 0;
            int start = 0;
            for (int i = 0; i < str3.Length; i++)
            {
                switch (str3[i])
                {
                    case '+':
                    case '.':
                        sb.Append(this.CreateEscapedIdentifier(str3.Substring(startIndex, i - startIndex)));
                        sb.Append('.');
                        i++;
                        startIndex = i;
                        break;

                    case '`':
                    {
                        sb.Append(this.CreateEscapedIdentifier(str3.Substring(startIndex, i - startIndex)));
                        i++;
                        int length = 0;
                        while (((i < str3.Length) && (str3[i] >= '0')) && (str3[i] <= '9'))
                        {
                            length = (length * 10) + (str3[i] - '0');
                            i++;
                        }
                        this.GetTypeArgumentsOutput(typeRef.TypeArguments, start, length, sb);
                        start += length;
                        if ((i < str3.Length) && ((str3[i] == '+') || (str3[i] == '.')))
                        {
                            sb.Append('.');
                            i++;
                        }
                        startIndex = i;
                        break;
                    }
                }
            }
            if (startIndex < str3.Length)
            {
                sb.Append(this.CreateEscapedIdentifier(str3.Substring(startIndex)));
            }
            return sb.ToString();
        }

        private string GetResponseFileCmdArgs(CompilerParameters options, string cmdArgs)
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
            return ("/noconfig /fullpaths @\"" + path + "\"");
        }

        private string GetTypeArgumentsOutput(CodeTypeReferenceCollection typeArguments)
        {
            StringBuilder sb = new StringBuilder(0x80);
            this.GetTypeArgumentsOutput(typeArguments, 0, typeArguments.Count, sb);
            return sb.ToString();
        }

        private void GetTypeArgumentsOutput(CodeTypeReferenceCollection typeArguments, int start, int length, StringBuilder sb)
        {
            sb.Append('<');
            bool flag = true;
            for (int i = start; i < (start + length); i++)
            {
                if (flag)
                {
                    flag = false;
                }
                else
                {
                    sb.Append(", ");
                }
                if (i < typeArguments.Count)
                {
                    sb.Append(this.GetTypeOutput(typeArguments[i]));
                }
            }
            sb.Append('>');
        }

        public string GetTypeOutput(CodeTypeReference typeRef)
        {
            string str = string.Empty;
            CodeTypeReference arrayElementType = typeRef;
            while (arrayElementType.ArrayElementType != null)
            {
                arrayElementType = arrayElementType.ArrayElementType;
            }
            str = str + this.GetBaseTypeOutput(arrayElementType);
            while ((typeRef != null) && (typeRef.ArrayRank > 0))
            {
                char[] chArray = new char[typeRef.ArrayRank + 1];
                chArray[0] = '[';
                chArray[typeRef.ArrayRank] = ']';
                for (int i = 1; i < typeRef.ArrayRank; i++)
                {
                    chArray[i] = ',';
                }
                str = str + new string(chArray);
                typeRef = typeRef.ArrayElementType;
            }
            return str;
        }

        private bool GetUserData(CodeObject e, string property, bool defaultValue)
        {
            object obj2 = e.UserData[property];
            if ((obj2 != null) && (obj2 is bool))
            {
                return (bool) obj2;
            }
            return defaultValue;
        }

        private static bool IsKeyword(string value)
        {
            return FixedStringLookup.Contains(keywords, value, false);
        }

        private static bool IsPrefixTwoUnderscore(string value)
        {
            if (value.Length < 3)
            {
                return false;
            }
            return (((value[0] == '_') && (value[1] == '_')) && (value[2] != '_'));
        }

        public bool IsValidIdentifier(string value)
        {
            if ((value == null) || (value.Length == 0))
            {
                return false;
            }
            if (value.Length > 0x200)
            {
                return false;
            }
            if (value[0] != '@')
            {
                if (IsKeyword(value))
                {
                    return false;
                }
            }
            else
            {
                value = value.Substring(1);
            }
            return CodeGenerator.IsValidLanguageIndependentIdentifier(value);
        }

        private static string JoinStringArray(string[] sa, string separator)
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

        private void OutputAttributeArgument(CodeAttributeArgument arg)
        {
            if ((arg.Name != null) && (arg.Name.Length > 0))
            {
                this.OutputIdentifier(arg.Name);
                this.Output.Write("=");
            }
            ((ICodeGenerator) this).GenerateCodeFromExpression(arg.Value, this.output.InnerWriter, this.options);
        }

        private void OutputDirection(FieldDirection dir)
        {
            switch (dir)
            {
                case FieldDirection.In:
                    break;

                case FieldDirection.Out:
                    this.Output.Write("out ");
                    return;

                case FieldDirection.Ref:
                    this.Output.Write("ref ");
                    break;

                default:
                    return;
            }
        }

        private void OutputExpressionList(CodeExpressionCollection expressions)
        {
            this.OutputExpressionList(expressions, false);
        }

        private void OutputExpressionList(CodeExpressionCollection expressions, bool newlineBetweenItems)
        {
            bool flag = true;
            IEnumerator enumerator = expressions.GetEnumerator();
            this.Indent++;
            while (enumerator.MoveNext())
            {
                if (flag)
                {
                    flag = false;
                }
                else if (newlineBetweenItems)
                {
                    this.ContinueOnNewLine(",");
                }
                else
                {
                    this.Output.Write(", ");
                }
                ((ICodeGenerator) this).GenerateCodeFromExpression((CodeExpression) enumerator.Current, this.output.InnerWriter, this.options);
            }
            this.Indent--;
        }

        private void OutputFieldScopeModifier(MemberAttributes attributes)
        {
            switch ((attributes & MemberAttributes.ScopeMask))
            {
                case MemberAttributes.Final:
                case MemberAttributes.Override:
                    break;

                case MemberAttributes.Static:
                    this.Output.Write("static ");
                    return;

                case MemberAttributes.Const:
                    this.Output.Write("const ");
                    break;

                default:
                    return;
            }
        }

        private void OutputIdentifier(string ident)
        {
            this.Output.Write(this.CreateEscapedIdentifier(ident));
        }

        private void OutputMemberAccessModifier(MemberAttributes attributes)
        {
            MemberAttributes attributes2 = attributes & MemberAttributes.AccessMask;
            if (attributes2 <= MemberAttributes.Family)
            {
                if (attributes2 != MemberAttributes.Assembly)
                {
                    if (attributes2 != MemberAttributes.FamilyAndAssembly)
                    {
                        if (attributes2 == MemberAttributes.Family)
                        {
                            this.Output.Write("protected ");
                        }
                        return;
                    }
                    this.Output.Write("internal ");
                    return;
                }
            }
            else
            {
                switch (attributes2)
                {
                    case MemberAttributes.FamilyOrAssembly:
                        this.Output.Write("protected internal ");
                        return;

                    case MemberAttributes.Private:
                        this.Output.Write("private ");
                        return;

                    case MemberAttributes.Public:
                        this.Output.Write("public ");
                        return;
                }
                return;
            }
            this.Output.Write("internal ");
        }

        private void OutputMemberScopeModifier(MemberAttributes attributes)
        {
            switch ((attributes & MemberAttributes.ScopeMask))
            {
                case MemberAttributes.Abstract:
                    this.Output.Write("abstract ");
                    return;

                case MemberAttributes.Final:
                    this.Output.Write("");
                    return;

                case MemberAttributes.Static:
                    this.Output.Write("static ");
                    return;

                case MemberAttributes.Override:
                    this.Output.Write("override ");
                    return;
            }
            switch ((attributes & MemberAttributes.AccessMask))
            {
                case MemberAttributes.Assembly:
                case MemberAttributes.Family:
                case MemberAttributes.Public:
                    this.Output.Write("virtual ");
                    break;
            }
        }

        private void OutputOperator(CodeBinaryOperatorType op)
        {
            switch (op)
            {
                case CodeBinaryOperatorType.Add:
                    this.Output.Write("+");
                    return;

                case CodeBinaryOperatorType.Subtract:
                    this.Output.Write("-");
                    return;

                case CodeBinaryOperatorType.Multiply:
                    this.Output.Write("*");
                    return;

                case CodeBinaryOperatorType.Divide:
                    this.Output.Write("/");
                    return;

                case CodeBinaryOperatorType.Modulus:
                    this.Output.Write("%");
                    return;

                case CodeBinaryOperatorType.Assign:
                    this.Output.Write("=");
                    return;

                case CodeBinaryOperatorType.IdentityInequality:
                    this.Output.Write("!=");
                    return;

                case CodeBinaryOperatorType.IdentityEquality:
                    this.Output.Write("==");
                    return;

                case CodeBinaryOperatorType.ValueEquality:
                    this.Output.Write("==");
                    return;

                case CodeBinaryOperatorType.BitwiseOr:
                    this.Output.Write("|");
                    return;

                case CodeBinaryOperatorType.BitwiseAnd:
                    this.Output.Write("&");
                    return;

                case CodeBinaryOperatorType.BooleanOr:
                    this.Output.Write("||");
                    return;

                case CodeBinaryOperatorType.BooleanAnd:
                    this.Output.Write("&&");
                    return;

                case CodeBinaryOperatorType.LessThan:
                    this.Output.Write("<");
                    return;

                case CodeBinaryOperatorType.LessThanOrEqual:
                    this.Output.Write("<=");
                    return;

                case CodeBinaryOperatorType.GreaterThan:
                    this.Output.Write(">");
                    return;

                case CodeBinaryOperatorType.GreaterThanOrEqual:
                    this.Output.Write(">=");
                    return;
            }
        }

        private void OutputParameters(CodeParameterDeclarationExpressionCollection parameters)
        {
            bool flag = true;
            bool flag2 = parameters.Count > 15;
            if (flag2)
            {
                this.Indent += 3;
            }
            IEnumerator enumerator = parameters.GetEnumerator();
            while (enumerator.MoveNext())
            {
                CodeParameterDeclarationExpression current = (CodeParameterDeclarationExpression) enumerator.Current;
                if (flag)
                {
                    flag = false;
                }
                else
                {
                    this.Output.Write(", ");
                }
                if (flag2)
                {
                    this.ContinueOnNewLine("");
                }
                this.GenerateExpression(current);
            }
            if (flag2)
            {
                this.Indent -= 3;
            }
        }

        private void OutputStartingBrace()
        {
            if (this.Options.BracingStyle == "C")
            {
                this.Output.WriteLine("");
                this.Output.WriteLine("{");
            }
            else
            {
                this.Output.WriteLine(" {");
            }
        }

        private void OutputType(CodeTypeReference typeRef)
        {
            this.Output.Write(this.GetTypeOutput(typeRef));
        }

        private void OutputTypeAttributes(CodeTypeDeclaration e)
        {
            if ((e.Attributes & MemberAttributes.New) != ((MemberAttributes) 0))
            {
                this.Output.Write("new ");
            }
            TypeAttributes typeAttributes = e.TypeAttributes;
            switch ((typeAttributes & TypeAttributes.NestedFamORAssem))
            {
                case TypeAttributes.AnsiClass:
                case TypeAttributes.NestedAssembly:
                case TypeAttributes.NestedFamANDAssem:
                    this.Output.Write("internal ");
                    break;

                case TypeAttributes.Public:
                case TypeAttributes.NestedPublic:
                    this.Output.Write("public ");
                    break;

                case TypeAttributes.NestedPrivate:
                    this.Output.Write("private ");
                    break;

                case TypeAttributes.NestedFamily:
                    this.Output.Write("protected ");
                    break;

                case TypeAttributes.NestedFamORAssem:
                    this.Output.Write("protected internal ");
                    break;
            }
            if (e.IsStruct)
            {
                if (e.IsPartial)
                {
                    this.Output.Write("partial ");
                }
                this.Output.Write("struct ");
            }
            else if (e.IsEnum)
            {
                this.Output.Write("enum ");
            }
            else
            {
                TypeAttributes attributes3 = typeAttributes & TypeAttributes.ClassSemanticsMask;
                if (attributes3 != TypeAttributes.AnsiClass)
                {
                    if (attributes3 != TypeAttributes.ClassSemanticsMask)
                    {
                        return;
                    }
                }
                else
                {
                    if ((typeAttributes & TypeAttributes.Sealed) == TypeAttributes.Sealed)
                    {
                        this.Output.Write("sealed ");
                    }
                    if ((typeAttributes & TypeAttributes.Abstract) == TypeAttributes.Abstract)
                    {
                        this.Output.Write("abstract ");
                    }
                    if (e.IsPartial)
                    {
                        this.Output.Write("partial ");
                    }
                    this.Output.Write("class ");
                    return;
                }
                if (e.IsPartial)
                {
                    this.Output.Write("partial ");
                }
                this.Output.Write("interface ");
            }
        }

        private void OutputTypeNamePair(CodeTypeReference typeRef, string name)
        {
            this.OutputType(typeRef);
            this.Output.Write(" ");
            this.OutputIdentifier(name);
        }

        private void OutputTypeParameterConstraints(CodeTypeParameterCollection typeParameters)
        {
            if (typeParameters.Count != 0)
            {
                for (int i = 0; i < typeParameters.Count; i++)
                {
                    this.Output.WriteLine();
                    this.Indent++;
                    bool flag = true;
                    if (typeParameters[i].Constraints.Count > 0)
                    {
                        foreach (CodeTypeReference reference in typeParameters[i].Constraints)
                        {
                            if (flag)
                            {
                                this.Output.Write("where ");
                                this.Output.Write(typeParameters[i].Name);
                                this.Output.Write(" : ");
                                flag = false;
                            }
                            else
                            {
                                this.Output.Write(", ");
                            }
                            this.OutputType(reference);
                        }
                    }
                    if (typeParameters[i].HasConstructorConstraint)
                    {
                        if (flag)
                        {
                            this.Output.Write("where ");
                            this.Output.Write(typeParameters[i].Name);
                            this.Output.Write(" : new()");
                        }
                        else
                        {
                            this.Output.Write(", new ()");
                        }
                    }
                    this.Indent--;
                }
            }
        }

        private void OutputTypeParameters(CodeTypeParameterCollection typeParameters)
        {
            if (typeParameters.Count != 0)
            {
                this.Output.Write('<');
                bool flag = true;
                for (int i = 0; i < typeParameters.Count; i++)
                {
                    if (flag)
                    {
                        flag = false;
                    }
                    else
                    {
                        this.Output.Write(", ");
                    }
                    if (typeParameters[i].CustomAttributes.Count > 0)
                    {
                        this.GenerateAttributes(typeParameters[i].CustomAttributes, null, true);
                        this.Output.Write(' ');
                    }
                    this.Output.Write(typeParameters[i].Name);
                }
                this.Output.Write('>');
            }
        }

        private void OutputVTableModifier(MemberAttributes attributes)
        {
            MemberAttributes attributes2 = attributes & MemberAttributes.VTableMask;
            if (attributes2 == MemberAttributes.New)
            {
                this.Output.Write("new ");
            }
        }

        private void ProcessCompilerOutputLine(CompilerResults results, string line)
        {
            bool flag;
            if (outputRegSimple == null)
            {
                outputRegWithFileAndLine = new Regex(@"(^(.*)(\(([0-9]+),([0-9]+)\)): )(error|warning) ([A-Z]+[0-9]+) ?: (.*)");
                outputRegSimple = new Regex("(error|warning) ([A-Z]+[0-9]+) ?: (.*)");
            }
            Match match = outputRegWithFileAndLine.Match(line);
            if (match.Success)
            {
                flag = true;
            }
            else
            {
                match = outputRegSimple.Match(line);
                flag = false;
            }
            if (match.Success)
            {
                CompilerError error = new CompilerError();
                if (flag)
                {
                    error.FileName = match.Groups[2].Value;
                    error.Line = int.Parse(match.Groups[4].Value, CultureInfo.InvariantCulture);
                    error.Column = int.Parse(match.Groups[5].Value, CultureInfo.InvariantCulture);
                }
                if (string.Compare(match.Groups[flag ? 6 : 1].Value, "warning", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    error.IsWarning = true;
                }
                error.ErrorNumber = match.Groups[flag ? 7 : 2].Value;
                error.ErrorText = match.Groups[flag ? 8 : 3].Value;
                results.Errors.Add(error);
            }
        }

        private string QuoteSnippetString(string value)
        {
            if (((value.Length >= 0x100) && (value.Length <= 0x5dc)) && (value.IndexOf('\0') == -1))
            {
                return this.QuoteSnippetStringVerbatimStyle(value);
            }
            return this.QuoteSnippetStringCStyle(value);
        }

        private string QuoteSnippetStringCStyle(string value)
        {
            StringBuilder b = new StringBuilder(value.Length + 5);
            Indentation indentation = new Indentation((IndentedTextWriter) this.Output, this.Indent + 1);
            b.Append("\"");
            for (int i = 0; i < value.Length; i++)
            {
                switch (value[i])
                {
                    case '\u2028':
                    case '\u2029':
                        this.AppendEscapedChar(b, value[i]);
                        break;

                    case '\\':
                        b.Append(@"\\");
                        break;

                    case '\'':
                        b.Append(@"\'");
                        break;

                    case '\t':
                        b.Append(@"\t");
                        break;

                    case '\n':
                        b.Append(@"\n");
                        break;

                    case '\r':
                        b.Append(@"\r");
                        break;

                    case '"':
                        b.Append("\\\"");
                        break;

                    case '\0':
                        b.Append(@"\0");
                        break;

                    default:
                        b.Append(value[i]);
                        break;
                }
                if ((i > 0) && ((i % 80) == 0))
                {
                    if ((char.IsHighSurrogate(value[i]) && (i < (value.Length - 1))) && char.IsLowSurrogate(value[i + 1]))
                    {
                        b.Append(value[++i]);
                    }
                    b.Append("\" +");
                    b.Append(Environment.NewLine);
                    b.Append(indentation.IndentationString);
                    b.Append('"');
                }
            }
            b.Append("\"");
            return b.ToString();
        }

        private string QuoteSnippetStringVerbatimStyle(string value)
        {
            StringBuilder builder = new StringBuilder(value.Length + 5);
            builder.Append("@\"");
            for (int i = 0; i < value.Length; i++)
            {
                if (value[i] == '"')
                {
                    builder.Append("\"\"");
                }
                else
                {
                    builder.Append(value[i]);
                }
            }
            builder.Append("\"");
            return builder.ToString();
        }

        private static string[] ReadAllLines(string file, Encoding encoding, FileShare share)
        {
            using (FileStream stream = File.Open(file, FileMode.Open, FileAccess.Read, share))
            {
                List<string> list = new List<string>();
                using (StreamReader reader = new StreamReader(stream, encoding))
                {
                    string str;
                    while ((str = reader.ReadLine()) != null)
                    {
                        list.Add(str);
                    }
                }
                return list.ToArray();
            }
        }

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

        public bool Supports(GeneratorSupport support)
        {
            return ((support & (GeneratorSupport.DeclareIndexerProperties | GeneratorSupport.GenericTypeDeclaration | GeneratorSupport.GenericTypeReference | GeneratorSupport.PartialTypes | GeneratorSupport.Resources | GeneratorSupport.Win32Resources | GeneratorSupport.ComplexExpressions | GeneratorSupport.PublicStaticMembers | GeneratorSupport.MultipleInterfaceMembers | GeneratorSupport.NestedTypes | GeneratorSupport.ChainedConstructorArguments | GeneratorSupport.ReferenceParameters | GeneratorSupport.ParameterAttributes | GeneratorSupport.AssemblyAttributes | GeneratorSupport.DeclareEvents | GeneratorSupport.DeclareInterfaces | GeneratorSupport.DeclareDelegates | GeneratorSupport.DeclareEnums | GeneratorSupport.DeclareValueTypes | GeneratorSupport.ReturnTypeAttributes | GeneratorSupport.TryCatchStatements | GeneratorSupport.StaticConstructors | GeneratorSupport.MultidimensionalArrays | GeneratorSupport.GotoStatements | GeneratorSupport.EntryPointMethod | GeneratorSupport.ArraysOfArrays)) == support);
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

        void ICodeGenerator.GenerateCodeFromCompileUnit(CodeCompileUnit e, TextWriter w, CodeGeneratorOptions o)
        {
            bool flag = false;
            if ((this.output != null) && (w != this.output.InnerWriter))
            {
                throw new InvalidOperationException(SR.GetString("CodeGenOutputWriter"));
            }
            if (this.output == null)
            {
                flag = true;
                this.options = (o == null) ? new CodeGeneratorOptions() : o;
                this.output = new IndentedTextWriter(w, this.options.IndentString);
            }
            try
            {
                if (e is CodeSnippetCompileUnit)
                {
                    this.GenerateSnippetCompileUnit((CodeSnippetCompileUnit) e);
                }
                else
                {
                    this.GenerateCompileUnit(e);
                }
            }
            finally
            {
                if (flag)
                {
                    this.output = null;
                    this.options = null;
                }
            }
        }

        void ICodeGenerator.GenerateCodeFromExpression(CodeExpression e, TextWriter w, CodeGeneratorOptions o)
        {
            bool flag = false;
            if ((this.output != null) && (w != this.output.InnerWriter))
            {
                throw new InvalidOperationException(SR.GetString("CodeGenOutputWriter"));
            }
            if (this.output == null)
            {
                flag = true;
                this.options = (o == null) ? new CodeGeneratorOptions() : o;
                this.output = new IndentedTextWriter(w, this.options.IndentString);
            }
            try
            {
                this.GenerateExpression(e);
            }
            finally
            {
                if (flag)
                {
                    this.output = null;
                    this.options = null;
                }
            }
        }

        void ICodeGenerator.GenerateCodeFromNamespace(CodeNamespace e, TextWriter w, CodeGeneratorOptions o)
        {
            bool flag = false;
            if ((this.output != null) && (w != this.output.InnerWriter))
            {
                throw new InvalidOperationException(SR.GetString("CodeGenOutputWriter"));
            }
            if (this.output == null)
            {
                flag = true;
                this.options = (o == null) ? new CodeGeneratorOptions() : o;
                this.output = new IndentedTextWriter(w, this.options.IndentString);
            }
            try
            {
                this.GenerateNamespace(e);
            }
            finally
            {
                if (flag)
                {
                    this.output = null;
                    this.options = null;
                }
            }
        }

        void ICodeGenerator.GenerateCodeFromStatement(CodeStatement e, TextWriter w, CodeGeneratorOptions o)
        {
            bool flag = false;
            if ((this.output != null) && (w != this.output.InnerWriter))
            {
                throw new InvalidOperationException(SR.GetString("CodeGenOutputWriter"));
            }
            if (this.output == null)
            {
                flag = true;
                this.options = (o == null) ? new CodeGeneratorOptions() : o;
                this.output = new IndentedTextWriter(w, this.options.IndentString);
            }
            try
            {
                this.GenerateStatement(e);
            }
            finally
            {
                if (flag)
                {
                    this.output = null;
                    this.options = null;
                }
            }
        }

        void ICodeGenerator.GenerateCodeFromType(CodeTypeDeclaration e, TextWriter w, CodeGeneratorOptions o)
        {
            bool flag = false;
            if ((this.output != null) && (w != this.output.InnerWriter))
            {
                throw new InvalidOperationException(SR.GetString("CodeGenOutputWriter"));
            }
            if (this.output == null)
            {
                flag = true;
                this.options = (o == null) ? new CodeGeneratorOptions() : o;
                this.output = new IndentedTextWriter(w, this.options.IndentString);
            }
            try
            {
                this.GenerateType(e);
            }
            finally
            {
                if (flag)
                {
                    this.output = null;
                    this.options = null;
                }
            }
        }

        public void ValidateIdentifier(string value)
        {
            if (!this.IsValidIdentifier(value))
            {
                throw new ArgumentException(SR.GetString("InvalidIdentifier", new object[] { value }));
            }
        }

        private string CompilerName
        {
            get
            {
                return "csc.exe";
            }
        }

        private string CurrentTypeName
        {
            get
            {
                if (this.currentClass != null)
                {
                    return this.currentClass.Name;
                }
                return "<% unknown %>";
            }
        }

        private string FileExtension
        {
            get
            {
                return ".cs";
            }
        }

        private int Indent
        {
            get
            {
                return this.output.Indent;
            }
            set
            {
                this.output.Indent = value;
            }
        }

        private bool IsCurrentClass
        {
            get
            {
                return (((this.currentClass != null) && !(this.currentClass is CodeTypeDelegate)) && this.currentClass.IsClass);
            }
        }

        private bool IsCurrentDelegate
        {
            get
            {
                return ((this.currentClass != null) && (this.currentClass is CodeTypeDelegate));
            }
        }

        private bool IsCurrentEnum
        {
            get
            {
                return (((this.currentClass != null) && !(this.currentClass is CodeTypeDelegate)) && this.currentClass.IsEnum);
            }
        }

        private bool IsCurrentInterface
        {
            get
            {
                return (((this.currentClass != null) && !(this.currentClass is CodeTypeDelegate)) && this.currentClass.IsInterface);
            }
        }

        private bool IsCurrentStruct
        {
            get
            {
                return (((this.currentClass != null) && !(this.currentClass is CodeTypeDelegate)) && this.currentClass.IsStruct);
            }
        }

        private string NullToken
        {
            get
            {
                return "null";
            }
        }

        private CodeGeneratorOptions Options
        {
            get
            {
                return this.options;
            }
        }

        private TextWriter Output
        {
            get
            {
                return this.output;
            }
        }
    }
}

