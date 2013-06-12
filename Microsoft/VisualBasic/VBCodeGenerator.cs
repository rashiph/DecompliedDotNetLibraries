namespace Microsoft.VisualBasic
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
    using System.Text;
    using System.Text.RegularExpressions;

    internal class VBCodeGenerator : CodeCompiler
    {
        private static readonly string[][] keywords;
        private const GeneratorSupport LanguageSupport = (GeneratorSupport.DeclareIndexerProperties | GeneratorSupport.GenericTypeDeclaration | GeneratorSupport.GenericTypeReference | GeneratorSupport.PartialTypes | GeneratorSupport.Resources | GeneratorSupport.Win32Resources | GeneratorSupport.ComplexExpressions | GeneratorSupport.PublicStaticMembers | GeneratorSupport.MultipleInterfaceMembers | GeneratorSupport.NestedTypes | GeneratorSupport.ChainedConstructorArguments | GeneratorSupport.ReferenceParameters | GeneratorSupport.ParameterAttributes | GeneratorSupport.AssemblyAttributes | GeneratorSupport.DeclareEvents | GeneratorSupport.DeclareInterfaces | GeneratorSupport.DeclareDelegates | GeneratorSupport.DeclareEnums | GeneratorSupport.DeclareValueTypes | GeneratorSupport.ReturnTypeAttributes | GeneratorSupport.TryCatchStatements | GeneratorSupport.StaticConstructors | GeneratorSupport.MultidimensionalArrays | GeneratorSupport.GotoStatements | GeneratorSupport.EntryPointMethod | GeneratorSupport.ArraysOfArrays);
        private const int MaxLineLength = 80;
        private static Regex outputReg;
        private IDictionary<string, string> provOptions;
        private int statementDepth;

        static VBCodeGenerator()
        {
            string[][] strArray = new string[0x10][];
            strArray[1] = new string[] { "as", "do", "if", "in", "is", "me", "of", "on", "or", "to" };
            strArray[2] = new string[] { "and", "dim", "end", "for", "get", "let", "lib", "mod", "new", "not", "rem", "set", "sub", "try", "xor" };
            strArray[3] = new string[] { 
                "ansi", "auto", "byte", "call", "case", "cdbl", "cdec", "char", "cint", "clng", "cobj", "csng", "cstr", "date", "each", "else", 
                "enum", "exit", "goto", "like", "long", "loop", "next", "step", "stop", "then", "true", "wend", "when", "with"
             };
            strArray[4] = new string[] { 
                "alias", "byref", "byval", "catch", "cbool", "cbyte", "cchar", "cdate", "class", "const", "ctype", "cuint", "culng", "endif", "erase", "error", 
                "event", "false", "gosub", "isnot", "redim", "sbyte", "short", "throw", "ulong", "until", "using", "while"
             };
            strArray[5] = new string[] { 
                "csbyte", "cshort", "double", "elseif", "friend", "global", "module", "mybase", "object", "option", "orelse", "public", "resume", "return", "select", "shared", 
                "single", "static", "string", "typeof", "ushort"
             };
            strArray[6] = new string[] { 
                "andalso", "boolean", "cushort", "decimal", "declare", "default", "finally", "gettype", "handles", "imports", "integer", "myclass", "nothing", "partial", "private", "shadows", 
                "trycast", "unicode", "variant"
             };
            strArray[7] = new string[] { "assembly", "continue", "delegate", "function", "inherits", "operator", "optional", "preserve", "property", "readonly", "synclock", "uinteger", "widening" };
            strArray[8] = new string[] { "addressof", "interface", "namespace", "narrowing", "overloads", "overrides", "protected", "structure", "writeonly" };
            strArray[9] = new string[] { "addhandler", "directcast", "implements", "paramarray", "raiseevent", "withevents" };
            strArray[10] = new string[] { "mustinherit", "overridable" };
            strArray[11] = new string[] { "mustoverride" };
            strArray[12] = new string[] { "removehandler" };
            strArray[13] = new string[] { "class_finalize", "notinheritable", "notoverridable" };
            strArray[15] = new string[] { "class_initialize" };
            keywords = strArray;
        }

        internal VBCodeGenerator()
        {
        }

        internal VBCodeGenerator(IDictionary<string, string> providerOptions)
        {
            this.provOptions = providerOptions;
        }

        protected bool AllowLateBound(CodeCompileUnit e)
        {
            object obj2 = e.UserData["AllowLateBound"];
            if ((obj2 != null) && (obj2 is bool))
            {
                return (bool) obj2;
            }
            return true;
        }

        private static void AppendEscapedChar(StringBuilder b, char value)
        {
            b.Append("&Global.Microsoft.VisualBasic.ChrW(");
            b.Append(((int) value).ToString(CultureInfo.InvariantCulture));
            b.Append(")");
        }

        protected override string CmdArgsFromParameters(CompilerParameters options)
        {
            using (StringEnumerator enumerator = options.ReferencedAssemblies.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    if (string.IsNullOrEmpty(enumerator.Current))
                    {
                        throw new ArgumentException(SR.GetString("NullOrEmpty_Value_in_Property", new object[] { "ReferencedAssemblies" }), "options");
                    }
                }
            }
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
            foreach (string str2 in options.ReferencedAssemblies)
            {
                string fileName = Path.GetFileName(str2);
                if ((string.Compare(fileName, "Microsoft.VisualBasic.dll", StringComparison.OrdinalIgnoreCase) != 0) && (string.Compare(fileName, "mscorlib.dll", StringComparison.OrdinalIgnoreCase) != 0))
                {
                    builder.Append("/R:");
                    builder.Append("\"");
                    builder.Append(str2);
                    builder.Append("\"");
                    builder.Append(" ");
                }
            }
            builder.Append("/out:");
            builder.Append("\"");
            builder.Append(options.OutputAssembly);
            builder.Append("\"");
            builder.Append(" ");
            if (options.IncludeDebugInformation)
            {
                builder.Append("/D:DEBUG=1 ");
                builder.Append("/debug+ ");
            }
            else
            {
                builder.Append("/debug- ");
            }
            if (options.Win32Resource != null)
            {
                builder.Append("/win32resource:\"" + options.Win32Resource + "\" ");
            }
            foreach (string str4 in options.EmbeddedResources)
            {
                builder.Append("/res:\"");
                builder.Append(str4);
                builder.Append("\" ");
            }
            foreach (string str5 in options.LinkedResources)
            {
                builder.Append("/linkres:\"");
                builder.Append(str5);
                builder.Append("\" ");
            }
            if (options.TreatWarningsAsErrors)
            {
                builder.Append("/warnaserror+ ");
            }
            if (options.CompilerOptions != null)
            {
                builder.Append(options.CompilerOptions + " ");
            }
            return builder.ToString();
        }

        protected override void ContinueOnNewLine(string st)
        {
            base.Output.Write(st);
            base.Output.WriteLine(" _");
        }

        protected override string CreateEscapedIdentifier(string name)
        {
            if (IsKeyword(name))
            {
                return ("[" + name + "]");
            }
            return name;
        }

        protected override string CreateValidIdentifier(string name)
        {
            if (IsKeyword(name))
            {
                return ("_" + name);
            }
            return name;
        }

        private void EnsureInDoubleQuotes(ref bool fInDoubleQuotes, StringBuilder b)
        {
            if (!fInDoubleQuotes)
            {
                b.Append("&\"");
                fInDoubleQuotes = true;
            }
        }

        private void EnsureNotInDoubleQuotes(ref bool fInDoubleQuotes, StringBuilder b)
        {
            if (fInDoubleQuotes)
            {
                b.Append("\"");
                fInDoubleQuotes = false;
            }
        }

        protected override CompilerResults FromFileBatch(CompilerParameters options, string[] fileNames)
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
            string fileExtension = options.GenerateExecutable ? "exe" : "dll";
            string str3 = '.' + fileExtension;
            if ((options.OutputAssembly == null) || (options.OutputAssembly.Length == 0))
            {
                options.OutputAssembly = results.TempFiles.AddExtension(fileExtension, !options.GenerateInMemory);
                new FileStream(options.OutputAssembly, FileMode.Create, FileAccess.ReadWrite).Close();
                flag = true;
            }
            string outputAssembly = options.OutputAssembly;
            if (!Path.GetExtension(outputAssembly).Equals(str3, StringComparison.OrdinalIgnoreCase))
            {
                outputAssembly = outputAssembly + str3;
            }
            string str5 = "pdb";
            if ((options.CompilerOptions != null) && (options.CompilerOptions.IndexOf("/debug:pdbonly", StringComparison.OrdinalIgnoreCase) != -1))
            {
                results.TempFiles.AddExtension(str5, true);
            }
            else
            {
                results.TempFiles.AddExtension(str5);
            }
            string cmdArgs = this.CmdArgsFromParameters(options) + " " + CodeCompiler.JoinStringArray(fileNames, " ");
            string responseFileCmdArgs = this.GetResponseFileCmdArgs(options, cmdArgs);
            string trueArgs = null;
            if (responseFileCmdArgs != null)
            {
                trueArgs = cmdArgs;
                cmdArgs = responseFileCmdArgs;
            }
            base.Compile(options, RedistVersionInfo.GetCompilerPath(this.provOptions, this.CompilerName), this.CompilerName, cmdArgs, ref outputFile, ref nativeReturnValue, trueArgs);
            results.NativeCompilerReturnValue = nativeReturnValue;
            if ((nativeReturnValue != 0) || (options.WarningLevel > 0))
            {
                byte[] bytes = ReadAllBytes(outputFile, FileShare.ReadWrite);
                foreach (string str10 in Regex.Split(Encoding.UTF8.GetString(bytes), @"\r\n"))
                {
                    results.Output.Add(str10);
                    this.ProcessCompilerOutputLine(results, str10);
                }
                if ((nativeReturnValue != 0) && flag)
                {
                    File.Delete(outputAssembly);
                }
            }
            if (!results.Errors.HasErrors && options.GenerateInMemory)
            {
                FileStream stream = new FileStream(outputAssembly, FileMode.Open, FileAccess.Read, FileShare.Read);
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
            results.PathToAssembly = outputAssembly;
            return results;
        }

        protected override void GenerateArgumentReferenceExpression(CodeArgumentReferenceExpression e)
        {
            this.OutputIdentifier(e.ParameterName);
        }

        protected override void GenerateArrayCreateExpression(CodeArrayCreateExpression e)
        {
            base.Output.Write("New ");
            CodeExpressionCollection initializers = e.Initializers;
            if (initializers.Count > 0)
            {
                string typeOutput = this.GetTypeOutput(e.CreateType);
                base.Output.Write(typeOutput);
                if (typeOutput.IndexOf('(') == -1)
                {
                    base.Output.Write("()");
                }
                base.Output.Write(" {");
                base.Indent++;
                this.OutputExpressionList(initializers);
                base.Indent--;
                base.Output.Write("}");
            }
            else
            {
                string str2 = this.GetTypeOutput(e.CreateType);
                int index = str2.IndexOf('(');
                if (index == -1)
                {
                    base.Output.Write(str2);
                    base.Output.Write('(');
                }
                else
                {
                    base.Output.Write(str2.Substring(0, index + 1));
                }
                if (e.SizeExpression != null)
                {
                    base.Output.Write("(");
                    base.GenerateExpression(e.SizeExpression);
                    base.Output.Write(") - 1");
                }
                else
                {
                    base.Output.Write((int) (e.Size - 1));
                }
                if (index == -1)
                {
                    base.Output.Write(')');
                }
                else
                {
                    base.Output.Write(str2.Substring(index + 1));
                }
                base.Output.Write(" {}");
            }
        }

        protected override void GenerateArrayIndexerExpression(CodeArrayIndexerExpression e)
        {
            base.GenerateExpression(e.TargetObject);
            base.Output.Write("(");
            bool flag = true;
            foreach (CodeExpression expression in e.Indices)
            {
                if (flag)
                {
                    flag = false;
                }
                else
                {
                    base.Output.Write(", ");
                }
                base.GenerateExpression(expression);
            }
            base.Output.Write(")");
        }

        protected override void GenerateAssignStatement(CodeAssignStatement e)
        {
            base.GenerateExpression(e.Left);
            base.Output.Write(" = ");
            base.GenerateExpression(e.Right);
            base.Output.WriteLine("");
        }

        protected override void GenerateAttachEventStatement(CodeAttachEventStatement e)
        {
            base.Output.Write("AddHandler ");
            this.GenerateFormalEventReferenceExpression(e.Event);
            base.Output.Write(", ");
            base.GenerateExpression(e.Listener);
            base.Output.WriteLine("");
        }

        protected override void GenerateAttributeDeclarationsEnd(CodeAttributeDeclarationCollection attributes)
        {
            base.Output.Write(">");
        }

        protected override void GenerateAttributeDeclarationsStart(CodeAttributeDeclarationCollection attributes)
        {
            base.Output.Write("<");
        }

        protected override void GenerateBaseReferenceExpression(CodeBaseReferenceExpression e)
        {
            base.Output.Write("MyBase");
        }

        protected override void GenerateBinaryOperatorExpression(CodeBinaryOperatorExpression e)
        {
            if (e.Operator != CodeBinaryOperatorType.IdentityInequality)
            {
                base.GenerateBinaryOperatorExpression(e);
            }
            else if ((e.Right is CodePrimitiveExpression) && (((CodePrimitiveExpression) e.Right).Value == null))
            {
                this.GenerateNotIsNullExpression(e.Left);
            }
            else if ((e.Left is CodePrimitiveExpression) && (((CodePrimitiveExpression) e.Left).Value == null))
            {
                this.GenerateNotIsNullExpression(e.Right);
            }
            else
            {
                base.GenerateBinaryOperatorExpression(e);
            }
        }

        protected override void GenerateCastExpression(CodeCastExpression e)
        {
            base.Output.Write("CType(");
            base.GenerateExpression(e.Expression);
            base.Output.Write(",");
            this.OutputType(e.TargetType);
            this.OutputArrayPostfix(e.TargetType);
            base.Output.Write(")");
        }

        private void GenerateChecksumPragma(CodeChecksumPragma checksumPragma)
        {
            base.Output.Write("#ExternalChecksum(\"");
            base.Output.Write(checksumPragma.FileName);
            base.Output.Write("\",\"");
            base.Output.Write(checksumPragma.ChecksumAlgorithmId.ToString("B", CultureInfo.InvariantCulture));
            base.Output.Write("\",\"");
            if (checksumPragma.ChecksumData != null)
            {
                foreach (byte num in checksumPragma.ChecksumData)
                {
                    base.Output.Write(num.ToString("X2", CultureInfo.InvariantCulture));
                }
            }
            base.Output.WriteLine("\")");
        }

        private void GenerateCodeRegionDirective(CodeRegionDirective regionDirective)
        {
            if (!this.IsGeneratingStatements())
            {
                if (regionDirective.RegionMode == CodeRegionMode.Start)
                {
                    base.Output.Write("#Region \"");
                    base.Output.Write(regionDirective.RegionText);
                    base.Output.WriteLine("\"");
                }
                else if (regionDirective.RegionMode == CodeRegionMode.End)
                {
                    base.Output.WriteLine("#End Region");
                }
            }
        }

        protected override void GenerateComment(CodeComment e)
        {
            string str = e.DocComment ? "'''" : "'";
            base.Output.Write(str);
            string text = e.Text;
            for (int i = 0; i < text.Length; i++)
            {
                base.Output.Write(text[i]);
                if (text[i] == '\r')
                {
                    if ((i < (text.Length - 1)) && (text[i + 1] == '\n'))
                    {
                        base.Output.Write('\n');
                        i++;
                    }
                    ((IndentedTextWriter) base.Output).InternalOutputTabs();
                    base.Output.Write(str);
                }
                else if (text[i] == '\n')
                {
                    ((IndentedTextWriter) base.Output).InternalOutputTabs();
                    base.Output.Write(str);
                }
                else if (((text[i] == '\u2028') || (text[i] == '\u2029')) || (text[i] == '\x0085'))
                {
                    base.Output.Write(str);
                }
            }
            base.Output.WriteLine();
        }

        protected override void GenerateCommentStatements(CodeCommentStatementCollection e)
        {
            foreach (CodeCommentStatement statement in e)
            {
                if (!this.IsDocComment(statement))
                {
                    this.GenerateCommentStatement(statement);
                }
            }
            foreach (CodeCommentStatement statement2 in e)
            {
                if (this.IsDocComment(statement2))
                {
                    this.GenerateCommentStatement(statement2);
                }
            }
        }

        protected override void GenerateCompileUnit(CodeCompileUnit e)
        {
            this.GenerateCompileUnitStart(e);
            SortedList list = new SortedList(StringComparer.OrdinalIgnoreCase);
            foreach (CodeNamespace namespace2 in e.Namespaces)
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
            foreach (string str in list.Keys)
            {
                base.Output.Write("Imports ");
                this.OutputIdentifier(str);
                base.Output.WriteLine("");
            }
            if (e.AssemblyCustomAttributes.Count > 0)
            {
                this.OutputAttributes(e.AssemblyCustomAttributes, false, "Assembly: ", true);
            }
            base.GenerateNamespaces(e);
            this.GenerateCompileUnitEnd(e);
        }

        protected override void GenerateCompileUnitStart(CodeCompileUnit e)
        {
            base.GenerateCompileUnitStart(e);
            base.Output.WriteLine("'------------------------------------------------------------------------------");
            base.Output.Write("' <");
            base.Output.WriteLine(SR.GetString("AutoGen_Comment_Line1"));
            base.Output.Write("'     ");
            base.Output.WriteLine(SR.GetString("AutoGen_Comment_Line2"));
            base.Output.Write("'     ");
            base.Output.Write(SR.GetString("AutoGen_Comment_Line3"));
            base.Output.WriteLine(Environment.Version.ToString());
            base.Output.WriteLine("'");
            base.Output.Write("'     ");
            base.Output.WriteLine(SR.GetString("AutoGen_Comment_Line4"));
            base.Output.Write("'     ");
            base.Output.WriteLine(SR.GetString("AutoGen_Comment_Line5"));
            base.Output.Write("' </");
            base.Output.WriteLine(SR.GetString("AutoGen_Comment_Line1"));
            base.Output.WriteLine("'------------------------------------------------------------------------------");
            base.Output.WriteLine("");
            if (this.AllowLateBound(e))
            {
                base.Output.WriteLine("Option Strict Off");
            }
            else
            {
                base.Output.WriteLine("Option Strict On");
            }
            if (!this.RequireVariableDeclaration(e))
            {
                base.Output.WriteLine("Option Explicit Off");
            }
            else
            {
                base.Output.WriteLine("Option Explicit On");
            }
            base.Output.WriteLine();
        }

        protected override void GenerateConditionStatement(CodeConditionStatement e)
        {
            base.Output.Write("If ");
            base.GenerateExpression(e.Condition);
            base.Output.WriteLine(" Then");
            base.Indent++;
            this.GenerateVBStatements(e.TrueStatements);
            base.Indent--;
            if (e.FalseStatements.Count > 0)
            {
                base.Output.Write("Else");
                base.Output.WriteLine("");
                base.Indent++;
                this.GenerateVBStatements(e.FalseStatements);
                base.Indent--;
            }
            base.Output.WriteLine("End If");
        }

        protected override void GenerateConstructor(CodeConstructor e, CodeTypeDeclaration c)
        {
            if (base.IsCurrentClass || base.IsCurrentStruct)
            {
                if (e.CustomAttributes.Count > 0)
                {
                    this.OutputAttributes(e.CustomAttributes, false);
                }
                this.OutputMemberAccessModifier(e.Attributes);
                base.Output.Write("Sub New(");
                this.OutputParameters(e.Parameters);
                base.Output.WriteLine(")");
                base.Indent++;
                CodeExpressionCollection baseConstructorArgs = e.BaseConstructorArgs;
                CodeExpressionCollection chainedConstructorArgs = e.ChainedConstructorArgs;
                if (chainedConstructorArgs.Count > 0)
                {
                    base.Output.Write("Me.New(");
                    this.OutputExpressionList(chainedConstructorArgs);
                    base.Output.Write(")");
                    base.Output.WriteLine("");
                }
                else if (baseConstructorArgs.Count > 0)
                {
                    base.Output.Write("MyBase.New(");
                    this.OutputExpressionList(baseConstructorArgs);
                    base.Output.Write(")");
                    base.Output.WriteLine("");
                }
                else if (base.IsCurrentClass)
                {
                    base.Output.WriteLine("MyBase.New");
                }
                this.GenerateVBStatements(e.Statements);
                base.Indent--;
                base.Output.WriteLine("End Sub");
            }
        }

        protected override void GenerateDefaultValueExpression(CodeDefaultValueExpression e)
        {
            base.Output.Write("CType(Nothing, " + this.GetTypeOutput(e.Type) + ")");
        }

        protected override void GenerateDelegateCreateExpression(CodeDelegateCreateExpression e)
        {
            base.Output.Write("AddressOf ");
            base.GenerateExpression(e.TargetObject);
            base.Output.Write(".");
            this.OutputIdentifier(e.MethodName);
        }

        protected override void GenerateDelegateInvokeExpression(CodeDelegateInvokeExpression e)
        {
            if (e.TargetObject != null)
            {
                if (e.TargetObject is CodeEventReferenceExpression)
                {
                    base.Output.Write("RaiseEvent ");
                    this.GenerateFormalEventReferenceExpression((CodeEventReferenceExpression) e.TargetObject);
                }
                else
                {
                    base.GenerateExpression(e.TargetObject);
                }
            }
            if (e.Parameters.Count > 0)
            {
                base.Output.Write("(");
                this.OutputExpressionList(e.Parameters);
                base.Output.Write(")");
            }
        }

        protected override void GenerateDirectionExpression(CodeDirectionExpression e)
        {
            base.GenerateExpression(e.Expression);
        }

        protected override void GenerateDirectives(CodeDirectiveCollection directives)
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

        protected override void GenerateDoubleValue(double d)
        {
            if (double.IsNaN(d))
            {
                base.Output.Write("Double.NaN");
            }
            else if (double.IsNegativeInfinity(d))
            {
                base.Output.Write("Double.NegativeInfinity");
            }
            else if (double.IsPositiveInfinity(d))
            {
                base.Output.Write("Double.PositiveInfinity");
            }
            else
            {
                base.Output.Write(d.ToString("R", CultureInfo.InvariantCulture));
                base.Output.Write("R");
            }
        }

        protected override void GenerateEntryPointMethod(CodeEntryPointMethod e, CodeTypeDeclaration c)
        {
            if (e.CustomAttributes.Count > 0)
            {
                this.OutputAttributes(e.CustomAttributes, false);
            }
            base.Output.WriteLine("Public Shared Sub Main()");
            base.Indent++;
            this.GenerateVBStatements(e.Statements);
            base.Indent--;
            base.Output.WriteLine("End Sub");
        }

        protected override void GenerateEvent(CodeMemberEvent e, CodeTypeDeclaration c)
        {
            if (!base.IsCurrentDelegate && !base.IsCurrentEnum)
            {
                if (e.CustomAttributes.Count > 0)
                {
                    this.OutputAttributes(e.CustomAttributes, false);
                }
                string name = e.Name;
                if (e.PrivateImplementationType != null)
                {
                    string str2 = this.GetBaseTypeOutput(e.PrivateImplementationType).Replace('.', '_');
                    e.Name = str2 + "_" + e.Name;
                }
                this.OutputMemberAccessModifier(e.Attributes);
                base.Output.Write("Event ");
                this.OutputTypeNamePair(e.Type, e.Name);
                if (e.ImplementationTypes.Count > 0)
                {
                    base.Output.Write(" Implements ");
                    bool flag = true;
                    foreach (CodeTypeReference reference in e.ImplementationTypes)
                    {
                        if (flag)
                        {
                            flag = false;
                        }
                        else
                        {
                            base.Output.Write(" , ");
                        }
                        this.OutputType(reference);
                        base.Output.Write(".");
                        this.OutputIdentifier(name);
                    }
                }
                else if (e.PrivateImplementationType != null)
                {
                    base.Output.Write(" Implements ");
                    this.OutputType(e.PrivateImplementationType);
                    base.Output.Write(".");
                    this.OutputIdentifier(name);
                }
                base.Output.WriteLine("");
            }
        }

        protected override void GenerateEventReferenceExpression(CodeEventReferenceExpression e)
        {
            if (e.TargetObject != null)
            {
                bool flag = e.TargetObject is CodeThisReferenceExpression;
                base.GenerateExpression(e.TargetObject);
                base.Output.Write(".");
                if (flag)
                {
                    base.Output.Write(e.EventName + "Event");
                }
                else
                {
                    base.Output.Write(e.EventName);
                }
            }
            else
            {
                this.OutputIdentifier(e.EventName + "Event");
            }
        }

        protected override void GenerateExpressionStatement(CodeExpressionStatement e)
        {
            base.GenerateExpression(e.Expression);
            base.Output.WriteLine("");
        }

        protected override void GenerateField(CodeMemberField e)
        {
            if (!base.IsCurrentDelegate && !base.IsCurrentInterface)
            {
                if (base.IsCurrentEnum)
                {
                    if (e.CustomAttributes.Count > 0)
                    {
                        this.OutputAttributes(e.CustomAttributes, false);
                    }
                    this.OutputIdentifier(e.Name);
                    if (e.InitExpression != null)
                    {
                        base.Output.Write(" = ");
                        base.GenerateExpression(e.InitExpression);
                    }
                    base.Output.WriteLine("");
                }
                else
                {
                    if (e.CustomAttributes.Count > 0)
                    {
                        this.OutputAttributes(e.CustomAttributes, false);
                    }
                    this.OutputMemberAccessModifier(e.Attributes);
                    this.OutputVTableModifier(e.Attributes);
                    this.OutputFieldScopeModifier(e.Attributes);
                    if (this.GetUserData(e, "WithEvents", false))
                    {
                        base.Output.Write("WithEvents ");
                    }
                    this.OutputTypeNamePair(e.Type, e.Name);
                    if (e.InitExpression != null)
                    {
                        base.Output.Write(" = ");
                        base.GenerateExpression(e.InitExpression);
                    }
                    base.Output.WriteLine("");
                }
            }
        }

        protected override void GenerateFieldReferenceExpression(CodeFieldReferenceExpression e)
        {
            if (e.TargetObject != null)
            {
                base.GenerateExpression(e.TargetObject);
                base.Output.Write(".");
            }
            this.OutputIdentifier(e.FieldName);
        }

        private void GenerateFormalEventReferenceExpression(CodeEventReferenceExpression e)
        {
            if ((e.TargetObject != null) && !(e.TargetObject is CodeThisReferenceExpression))
            {
                base.GenerateExpression(e.TargetObject);
                base.Output.Write(".");
            }
            this.OutputIdentifier(e.EventName);
        }

        protected override void GenerateGotoStatement(CodeGotoStatement e)
        {
            base.Output.Write("goto ");
            base.Output.WriteLine(e.Label);
        }

        protected override void GenerateIndexerExpression(CodeIndexerExpression e)
        {
            base.GenerateExpression(e.TargetObject);
            if (e.TargetObject is CodeBaseReferenceExpression)
            {
                base.Output.Write(".Item");
            }
            base.Output.Write("(");
            bool flag = true;
            foreach (CodeExpression expression in e.Indices)
            {
                if (flag)
                {
                    flag = false;
                }
                else
                {
                    base.Output.Write(", ");
                }
                base.GenerateExpression(expression);
            }
            base.Output.Write(")");
        }

        protected override void GenerateIterationStatement(CodeIterationStatement e)
        {
            base.GenerateStatement(e.InitStatement);
            base.Output.Write("Do While ");
            base.GenerateExpression(e.TestExpression);
            base.Output.WriteLine("");
            base.Indent++;
            this.GenerateVBStatements(e.Statements);
            base.GenerateStatement(e.IncrementStatement);
            base.Indent--;
            base.Output.WriteLine("Loop");
        }

        protected override void GenerateLabeledStatement(CodeLabeledStatement e)
        {
            base.Indent--;
            base.Output.Write(e.Label);
            base.Output.WriteLine(":");
            base.Indent++;
            if (e.Statement != null)
            {
                base.GenerateStatement(e.Statement);
            }
        }

        protected override void GenerateLinePragmaEnd(CodeLinePragma e)
        {
            base.Output.WriteLine("");
            base.Output.WriteLine("#End ExternalSource");
        }

        protected override void GenerateLinePragmaStart(CodeLinePragma e)
        {
            base.Output.WriteLine("");
            base.Output.Write("#ExternalSource(\"");
            base.Output.Write(e.FileName);
            base.Output.Write("\",");
            base.Output.Write(e.LineNumber);
            base.Output.WriteLine(")");
        }

        protected override void GenerateMethod(CodeMemberMethod e, CodeTypeDeclaration c)
        {
            if ((base.IsCurrentClass || base.IsCurrentStruct) || base.IsCurrentInterface)
            {
                if (e.CustomAttributes.Count > 0)
                {
                    this.OutputAttributes(e.CustomAttributes, false);
                }
                string name = e.Name;
                if (e.PrivateImplementationType != null)
                {
                    string str2 = this.GetBaseTypeOutput(e.PrivateImplementationType).Replace('.', '_');
                    e.Name = str2 + "_" + e.Name;
                }
                if (!base.IsCurrentInterface)
                {
                    if (e.PrivateImplementationType == null)
                    {
                        this.OutputMemberAccessModifier(e.Attributes);
                        if (this.MethodIsOverloaded(e, c))
                        {
                            base.Output.Write("Overloads ");
                        }
                    }
                    this.OutputVTableModifier(e.Attributes);
                    this.OutputMemberScopeModifier(e.Attributes);
                }
                else
                {
                    this.OutputVTableModifier(e.Attributes);
                }
                bool flag = false;
                if ((e.ReturnType.BaseType.Length == 0) || (string.Compare(e.ReturnType.BaseType, typeof(void).FullName, StringComparison.OrdinalIgnoreCase) == 0))
                {
                    flag = true;
                }
                if (flag)
                {
                    base.Output.Write("Sub ");
                }
                else
                {
                    base.Output.Write("Function ");
                }
                this.OutputIdentifier(e.Name);
                this.OutputTypeParameters(e.TypeParameters);
                base.Output.Write("(");
                this.OutputParameters(e.Parameters);
                base.Output.Write(")");
                if (!flag)
                {
                    base.Output.Write(" As ");
                    if (e.ReturnTypeCustomAttributes.Count > 0)
                    {
                        this.OutputAttributes(e.ReturnTypeCustomAttributes, true);
                    }
                    this.OutputType(e.ReturnType);
                    this.OutputArrayPostfix(e.ReturnType);
                }
                if (e.ImplementationTypes.Count > 0)
                {
                    base.Output.Write(" Implements ");
                    bool flag2 = true;
                    foreach (CodeTypeReference reference in e.ImplementationTypes)
                    {
                        if (flag2)
                        {
                            flag2 = false;
                        }
                        else
                        {
                            base.Output.Write(" , ");
                        }
                        this.OutputType(reference);
                        base.Output.Write(".");
                        this.OutputIdentifier(name);
                    }
                }
                else if (e.PrivateImplementationType != null)
                {
                    base.Output.Write(" Implements ");
                    this.OutputType(e.PrivateImplementationType);
                    base.Output.Write(".");
                    this.OutputIdentifier(name);
                }
                base.Output.WriteLine("");
                if (!base.IsCurrentInterface && ((e.Attributes & MemberAttributes.ScopeMask) != MemberAttributes.Abstract))
                {
                    base.Indent++;
                    this.GenerateVBStatements(e.Statements);
                    base.Indent--;
                    if (flag)
                    {
                        base.Output.WriteLine("End Sub");
                    }
                    else
                    {
                        base.Output.WriteLine("End Function");
                    }
                }
                e.Name = name;
            }
        }

        protected override void GenerateMethodInvokeExpression(CodeMethodInvokeExpression e)
        {
            this.GenerateMethodReferenceExpression(e.Method);
            if (e.Parameters.Count > 0)
            {
                base.Output.Write("(");
                this.OutputExpressionList(e.Parameters);
                base.Output.Write(")");
            }
        }

        protected override void GenerateMethodReferenceExpression(CodeMethodReferenceExpression e)
        {
            if (e.TargetObject != null)
            {
                base.GenerateExpression(e.TargetObject);
                base.Output.Write(".");
                base.Output.Write(e.MethodName);
            }
            else
            {
                this.OutputIdentifier(e.MethodName);
            }
            if (e.TypeArguments.Count > 0)
            {
                base.Output.Write(this.GetTypeArgumentsOutput(e.TypeArguments));
            }
        }

        protected override void GenerateMethodReturnStatement(CodeMethodReturnStatement e)
        {
            if (e.Expression != null)
            {
                base.Output.Write("Return ");
                base.GenerateExpression(e.Expression);
                base.Output.WriteLine("");
            }
            else
            {
                base.Output.WriteLine("Return");
            }
        }

        protected override void GenerateNamespace(CodeNamespace e)
        {
            if (this.GetUserData(e, "GenerateImports", true))
            {
                base.GenerateNamespaceImports(e);
            }
            base.Output.WriteLine();
            this.GenerateCommentStatements(e.Comments);
            this.GenerateNamespaceStart(e);
            base.GenerateTypes(e);
            this.GenerateNamespaceEnd(e);
        }

        protected override void GenerateNamespaceEnd(CodeNamespace e)
        {
            if ((e.Name != null) && (e.Name.Length > 0))
            {
                base.Indent--;
                base.Output.WriteLine("End Namespace");
            }
        }

        protected override void GenerateNamespaceImport(CodeNamespaceImport e)
        {
            base.Output.Write("Imports ");
            this.OutputIdentifier(e.Namespace);
            base.Output.WriteLine("");
        }

        protected override void GenerateNamespaceStart(CodeNamespace e)
        {
            if ((e.Name != null) && (e.Name.Length > 0))
            {
                base.Output.Write("Namespace ");
                string[] strArray = e.Name.Split(new char[] { '.' });
                this.OutputIdentifier(strArray[0]);
                for (int i = 1; i < strArray.Length; i++)
                {
                    base.Output.Write(".");
                    this.OutputIdentifier(strArray[i]);
                }
                base.Output.WriteLine();
                base.Indent++;
            }
        }

        private void GenerateNotIsNullExpression(CodeExpression e)
        {
            base.Output.Write("(Not (");
            base.GenerateExpression(e);
            base.Output.Write(") Is ");
            base.Output.Write(this.NullToken);
            base.Output.Write(")");
        }

        protected override void GenerateObjectCreateExpression(CodeObjectCreateExpression e)
        {
            base.Output.Write("New ");
            this.OutputType(e.CreateType);
            base.Output.Write("(");
            this.OutputExpressionList(e.Parameters);
            base.Output.Write(")");
        }

        protected override void GenerateParameterDeclarationExpression(CodeParameterDeclarationExpression e)
        {
            if (e.CustomAttributes.Count > 0)
            {
                this.OutputAttributes(e.CustomAttributes, true);
            }
            this.OutputDirection(e.Direction);
            this.OutputTypeNamePair(e.Type, e.Name);
        }

        protected override void GeneratePrimitiveExpression(CodePrimitiveExpression e)
        {
            if (e.Value is char)
            {
                base.Output.Write("Global.Microsoft.VisualBasic.ChrW(" + ((IConvertible) e.Value).ToInt32(CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture) + ")");
            }
            else if (e.Value is sbyte)
            {
                base.Output.Write("CSByte(");
                sbyte num2 = (sbyte) e.Value;
                base.Output.Write(num2.ToString(CultureInfo.InvariantCulture));
                base.Output.Write(")");
            }
            else if (e.Value is ushort)
            {
                ushort num3 = (ushort) e.Value;
                base.Output.Write(num3.ToString(CultureInfo.InvariantCulture));
                base.Output.Write("US");
            }
            else if (e.Value is uint)
            {
                uint num4 = (uint) e.Value;
                base.Output.Write(num4.ToString(CultureInfo.InvariantCulture));
                base.Output.Write("UI");
            }
            else if (e.Value is ulong)
            {
                ulong num5 = (ulong) e.Value;
                base.Output.Write(num5.ToString(CultureInfo.InvariantCulture));
                base.Output.Write("UL");
            }
            else
            {
                base.GeneratePrimitiveExpression(e);
            }
        }

        protected override void GenerateProperty(CodeMemberProperty e, CodeTypeDeclaration c)
        {
            if ((base.IsCurrentClass || base.IsCurrentStruct) || base.IsCurrentInterface)
            {
                if (e.CustomAttributes.Count > 0)
                {
                    this.OutputAttributes(e.CustomAttributes, false);
                }
                string name = e.Name;
                if (e.PrivateImplementationType != null)
                {
                    string str2 = this.GetBaseTypeOutput(e.PrivateImplementationType).Replace('.', '_');
                    e.Name = str2 + "_" + e.Name;
                }
                if (!base.IsCurrentInterface)
                {
                    if (e.PrivateImplementationType == null)
                    {
                        this.OutputMemberAccessModifier(e.Attributes);
                        if (this.PropertyIsOverloaded(e, c))
                        {
                            base.Output.Write("Overloads ");
                        }
                    }
                    this.OutputVTableModifier(e.Attributes);
                    this.OutputMemberScopeModifier(e.Attributes);
                }
                else
                {
                    this.OutputVTableModifier(e.Attributes);
                }
                if ((e.Parameters.Count > 0) && (string.Compare(e.Name, "Item", StringComparison.OrdinalIgnoreCase) == 0))
                {
                    base.Output.Write("Default ");
                }
                if (e.HasGet)
                {
                    if (!e.HasSet)
                    {
                        base.Output.Write("ReadOnly ");
                    }
                }
                else if (e.HasSet)
                {
                    base.Output.Write("WriteOnly ");
                }
                base.Output.Write("Property ");
                this.OutputIdentifier(e.Name);
                base.Output.Write("(");
                if (e.Parameters.Count > 0)
                {
                    this.OutputParameters(e.Parameters);
                }
                base.Output.Write(")");
                base.Output.Write(" As ");
                this.OutputType(e.Type);
                this.OutputArrayPostfix(e.Type);
                if (e.ImplementationTypes.Count > 0)
                {
                    base.Output.Write(" Implements ");
                    bool flag = true;
                    foreach (CodeTypeReference reference in e.ImplementationTypes)
                    {
                        if (flag)
                        {
                            flag = false;
                        }
                        else
                        {
                            base.Output.Write(" , ");
                        }
                        this.OutputType(reference);
                        base.Output.Write(".");
                        this.OutputIdentifier(name);
                    }
                }
                else if (e.PrivateImplementationType != null)
                {
                    base.Output.Write(" Implements ");
                    this.OutputType(e.PrivateImplementationType);
                    base.Output.Write(".");
                    this.OutputIdentifier(name);
                }
                base.Output.WriteLine("");
                if (!c.IsInterface && ((e.Attributes & MemberAttributes.ScopeMask) != MemberAttributes.Abstract))
                {
                    base.Indent++;
                    if (e.HasGet)
                    {
                        base.Output.WriteLine("Get");
                        if (!base.IsCurrentInterface)
                        {
                            base.Indent++;
                            this.GenerateVBStatements(e.GetStatements);
                            e.Name = name;
                            base.Indent--;
                            base.Output.WriteLine("End Get");
                        }
                    }
                    if (e.HasSet)
                    {
                        base.Output.WriteLine("Set");
                        if (!base.IsCurrentInterface)
                        {
                            base.Indent++;
                            this.GenerateVBStatements(e.SetStatements);
                            base.Indent--;
                            base.Output.WriteLine("End Set");
                        }
                    }
                    base.Indent--;
                    base.Output.WriteLine("End Property");
                }
                e.Name = name;
            }
        }

        protected override void GeneratePropertyReferenceExpression(CodePropertyReferenceExpression e)
        {
            if (e.TargetObject != null)
            {
                base.GenerateExpression(e.TargetObject);
                base.Output.Write(".");
                base.Output.Write(e.PropertyName);
            }
            else
            {
                this.OutputIdentifier(e.PropertyName);
            }
        }

        protected override void GeneratePropertySetValueReferenceExpression(CodePropertySetValueReferenceExpression e)
        {
            base.Output.Write("value");
        }

        protected override void GenerateRemoveEventStatement(CodeRemoveEventStatement e)
        {
            base.Output.Write("RemoveHandler ");
            this.GenerateFormalEventReferenceExpression(e.Event);
            base.Output.Write(", ");
            base.GenerateExpression(e.Listener);
            base.Output.WriteLine("");
        }

        protected override void GenerateSingleFloatValue(float s)
        {
            if (float.IsNaN(s))
            {
                base.Output.Write("Single.NaN");
            }
            else if (float.IsNegativeInfinity(s))
            {
                base.Output.Write("Single.NegativeInfinity");
            }
            else if (float.IsPositiveInfinity(s))
            {
                base.Output.Write("Single.PositiveInfinity");
            }
            else
            {
                base.Output.Write(s.ToString(CultureInfo.InvariantCulture));
                base.Output.Write('!');
            }
        }

        protected override void GenerateSnippetExpression(CodeSnippetExpression e)
        {
            base.Output.Write(e.Value);
        }

        protected override void GenerateSnippetMember(CodeSnippetTypeMember e)
        {
            base.Output.Write(e.Text);
        }

        protected override void GenerateSnippetStatement(CodeSnippetStatement e)
        {
            base.Output.WriteLine(e.Value);
        }

        protected override void GenerateThisReferenceExpression(CodeThisReferenceExpression e)
        {
            base.Output.Write("Me");
        }

        protected override void GenerateThrowExceptionStatement(CodeThrowExceptionStatement e)
        {
            base.Output.Write("Throw");
            if (e.ToThrow != null)
            {
                base.Output.Write(" ");
                base.GenerateExpression(e.ToThrow);
            }
            base.Output.WriteLine("");
        }

        protected override void GenerateTryCatchFinallyStatement(CodeTryCatchFinallyStatement e)
        {
            base.Output.WriteLine("Try ");
            base.Indent++;
            this.GenerateVBStatements(e.TryStatements);
            base.Indent--;
            CodeCatchClauseCollection catchClauses = e.CatchClauses;
            if (catchClauses.Count > 0)
            {
                IEnumerator enumerator = catchClauses.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    CodeCatchClause current = (CodeCatchClause) enumerator.Current;
                    base.Output.Write("Catch ");
                    this.OutputTypeNamePair(current.CatchExceptionType, current.LocalName);
                    base.Output.WriteLine("");
                    base.Indent++;
                    this.GenerateVBStatements(current.Statements);
                    base.Indent--;
                }
            }
            CodeStatementCollection finallyStatements = e.FinallyStatements;
            if (finallyStatements.Count > 0)
            {
                base.Output.WriteLine("Finally");
                base.Indent++;
                this.GenerateVBStatements(finallyStatements);
                base.Indent--;
            }
            base.Output.WriteLine("End Try");
        }

        protected override void GenerateTypeConstructor(CodeTypeConstructor e)
        {
            if (base.IsCurrentClass || base.IsCurrentStruct)
            {
                if (e.CustomAttributes.Count > 0)
                {
                    this.OutputAttributes(e.CustomAttributes, false);
                }
                base.Output.WriteLine("Shared Sub New()");
                base.Indent++;
                this.GenerateVBStatements(e.Statements);
                base.Indent--;
                base.Output.WriteLine("End Sub");
            }
        }

        protected override void GenerateTypeEnd(CodeTypeDeclaration e)
        {
            if (!base.IsCurrentDelegate)
            {
                string str;
                base.Indent--;
                if (e.IsEnum)
                {
                    str = "End Enum";
                }
                else if (e.IsInterface)
                {
                    str = "End Interface";
                }
                else if (e.IsStruct)
                {
                    str = "End Structure";
                }
                else if (this.IsCurrentModule)
                {
                    str = "End Module";
                }
                else
                {
                    str = "End Class";
                }
                base.Output.WriteLine(str);
            }
        }

        protected override void GenerateTypeOfExpression(CodeTypeOfExpression e)
        {
            base.Output.Write("GetType(");
            base.Output.Write(this.GetTypeOutput(e.Type));
            base.Output.Write(")");
        }

        protected override void GenerateTypeStart(CodeTypeDeclaration e)
        {
            if (!base.IsCurrentDelegate)
            {
                if (e.IsEnum)
                {
                    if (e.CustomAttributes.Count > 0)
                    {
                        this.OutputAttributes(e.CustomAttributes, false);
                    }
                    this.OutputTypeAttributes(e);
                    this.OutputIdentifier(e.Name);
                    if (e.BaseTypes.Count > 0)
                    {
                        base.Output.Write(" As ");
                        this.OutputType(e.BaseTypes[0]);
                    }
                    base.Output.WriteLine("");
                    base.Indent++;
                }
                else
                {
                    if (e.CustomAttributes.Count > 0)
                    {
                        this.OutputAttributes(e.CustomAttributes, false);
                    }
                    this.OutputTypeAttributes(e);
                    this.OutputIdentifier(e.Name);
                    this.OutputTypeParameters(e.TypeParameters);
                    bool flag = false;
                    bool flag2 = false;
                    if (e.IsStruct)
                    {
                        flag = true;
                    }
                    if (e.IsInterface)
                    {
                        flag2 = true;
                    }
                    base.Indent++;
                    foreach (CodeTypeReference reference in e.BaseTypes)
                    {
                        if (!flag && (e.IsInterface || !reference.IsInterface))
                        {
                            base.Output.WriteLine("");
                            base.Output.Write("Inherits ");
                            flag = true;
                        }
                        else if (!flag2)
                        {
                            base.Output.WriteLine("");
                            base.Output.Write("Implements ");
                            flag2 = true;
                        }
                        else
                        {
                            base.Output.Write(", ");
                        }
                        this.OutputType(reference);
                    }
                    base.Output.WriteLine("");
                }
            }
            else
            {
                if (e.CustomAttributes.Count > 0)
                {
                    this.OutputAttributes(e.CustomAttributes, false);
                }
                switch ((e.TypeAttributes & TypeAttributes.NestedFamORAssem))
                {
                    case TypeAttributes.Public:
                        base.Output.Write("Public ");
                        break;
                }
                CodeTypeDelegate delegate2 = (CodeTypeDelegate) e;
                if ((delegate2.ReturnType.BaseType.Length > 0) && (string.Compare(delegate2.ReturnType.BaseType, "System.Void", StringComparison.OrdinalIgnoreCase) != 0))
                {
                    base.Output.Write("Delegate Function ");
                }
                else
                {
                    base.Output.Write("Delegate Sub ");
                }
                this.OutputIdentifier(e.Name);
                base.Output.Write("(");
                this.OutputParameters(delegate2.Parameters);
                base.Output.Write(")");
                if ((delegate2.ReturnType.BaseType.Length > 0) && (string.Compare(delegate2.ReturnType.BaseType, "System.Void", StringComparison.OrdinalIgnoreCase) != 0))
                {
                    base.Output.Write(" As ");
                    this.OutputType(delegate2.ReturnType);
                    this.OutputArrayPostfix(delegate2.ReturnType);
                }
                base.Output.WriteLine("");
            }
        }

        protected override void GenerateVariableDeclarationStatement(CodeVariableDeclarationStatement e)
        {
            bool flag = true;
            base.Output.Write("Dim ");
            CodeTypeReference type = e.Type;
            if ((type.ArrayRank == 1) && (e.InitExpression != null))
            {
                CodeArrayCreateExpression initExpression = e.InitExpression as CodeArrayCreateExpression;
                if ((initExpression != null) && (initExpression.Initializers.Count == 0))
                {
                    flag = false;
                    this.OutputIdentifier(e.Name);
                    base.Output.Write("(");
                    if (initExpression.SizeExpression != null)
                    {
                        base.Output.Write("(");
                        base.GenerateExpression(initExpression.SizeExpression);
                        base.Output.Write(") - 1");
                    }
                    else
                    {
                        base.Output.Write((int) (initExpression.Size - 1));
                    }
                    base.Output.Write(")");
                    if (type.ArrayElementType != null)
                    {
                        this.OutputArrayPostfix(type.ArrayElementType);
                    }
                    base.Output.Write(" As ");
                    this.OutputType(type);
                }
                else
                {
                    this.OutputTypeNamePair(e.Type, e.Name);
                }
            }
            else
            {
                this.OutputTypeNamePair(e.Type, e.Name);
            }
            if (flag && (e.InitExpression != null))
            {
                base.Output.Write(" = ");
                base.GenerateExpression(e.InitExpression);
            }
            base.Output.WriteLine("");
        }

        protected override void GenerateVariableReferenceExpression(CodeVariableReferenceExpression e)
        {
            this.OutputIdentifier(e.VariableName);
        }

        private void GenerateVBStatements(CodeStatementCollection stms)
        {
            this.statementDepth++;
            try
            {
                base.GenerateStatements(stms);
            }
            finally
            {
                this.statementDepth--;
            }
        }

        private string GetArrayPostfix(CodeTypeReference typeRef)
        {
            string arrayPostfix = "";
            if (typeRef.ArrayElementType != null)
            {
                arrayPostfix = this.GetArrayPostfix(typeRef.ArrayElementType);
            }
            if (typeRef.ArrayRank <= 0)
            {
                return arrayPostfix;
            }
            char[] chArray = new char[typeRef.ArrayRank + 1];
            chArray[0] = '(';
            chArray[typeRef.ArrayRank] = ')';
            for (int i = 1; i < typeRef.ArrayRank; i++)
            {
                chArray[i] = ',';
            }
            return (new string(chArray) + arrayPostfix);
        }

        private string GetBaseTypeOutput(CodeTypeReference typeRef)
        {
            string baseType = typeRef.BaseType;
            if (baseType.Length == 0)
            {
                return "Void";
            }
            if (string.Compare(baseType, "System.Byte", StringComparison.OrdinalIgnoreCase) == 0)
            {
                return "Byte";
            }
            if (string.Compare(baseType, "System.SByte", StringComparison.OrdinalIgnoreCase) == 0)
            {
                return "SByte";
            }
            if (string.Compare(baseType, "System.Int16", StringComparison.OrdinalIgnoreCase) == 0)
            {
                return "Short";
            }
            if (string.Compare(baseType, "System.Int32", StringComparison.OrdinalIgnoreCase) == 0)
            {
                return "Integer";
            }
            if (string.Compare(baseType, "System.Int64", StringComparison.OrdinalIgnoreCase) == 0)
            {
                return "Long";
            }
            if (string.Compare(baseType, "System.UInt16", StringComparison.OrdinalIgnoreCase) == 0)
            {
                return "UShort";
            }
            if (string.Compare(baseType, "System.UInt32", StringComparison.OrdinalIgnoreCase) == 0)
            {
                return "UInteger";
            }
            if (string.Compare(baseType, "System.UInt64", StringComparison.OrdinalIgnoreCase) == 0)
            {
                return "ULong";
            }
            if (string.Compare(baseType, "System.String", StringComparison.OrdinalIgnoreCase) == 0)
            {
                return "String";
            }
            if (string.Compare(baseType, "System.DateTime", StringComparison.OrdinalIgnoreCase) == 0)
            {
                return "Date";
            }
            if (string.Compare(baseType, "System.Decimal", StringComparison.OrdinalIgnoreCase) == 0)
            {
                return "Decimal";
            }
            if (string.Compare(baseType, "System.Single", StringComparison.OrdinalIgnoreCase) == 0)
            {
                return "Single";
            }
            if (string.Compare(baseType, "System.Double", StringComparison.OrdinalIgnoreCase) == 0)
            {
                return "Double";
            }
            if (string.Compare(baseType, "System.Boolean", StringComparison.OrdinalIgnoreCase) == 0)
            {
                return "Boolean";
            }
            if (string.Compare(baseType, "System.Char", StringComparison.OrdinalIgnoreCase) == 0)
            {
                return "Char";
            }
            if (string.Compare(baseType, "System.Object", StringComparison.OrdinalIgnoreCase) == 0)
            {
                return "Object";
            }
            StringBuilder sb = new StringBuilder(baseType.Length + 10);
            if ((typeRef.Options & CodeTypeReferenceOptions.GlobalReference) != 0)
            {
                sb.Append("Global.");
            }
            int startIndex = 0;
            int start = 0;
            for (int i = 0; i < baseType.Length; i++)
            {
                switch (baseType[i])
                {
                    case '+':
                    case '.':
                        sb.Append(this.CreateEscapedIdentifier(baseType.Substring(startIndex, i - startIndex)));
                        sb.Append('.');
                        i++;
                        startIndex = i;
                        break;

                    case '`':
                    {
                        sb.Append(this.CreateEscapedIdentifier(baseType.Substring(startIndex, i - startIndex)));
                        i++;
                        int length = 0;
                        while (((i < baseType.Length) && (baseType[i] >= '0')) && (baseType[i] <= '9'))
                        {
                            length = (length * 10) + (baseType[i] - '0');
                            i++;
                        }
                        this.GetTypeArgumentsOutput(typeRef.TypeArguments, start, length, sb);
                        start += length;
                        if ((i < baseType.Length) && ((baseType[i] == '+') || (baseType[i] == '.')))
                        {
                            sb.Append('.');
                            i++;
                        }
                        startIndex = i;
                        break;
                    }
                }
            }
            if (startIndex < baseType.Length)
            {
                sb.Append(this.CreateEscapedIdentifier(baseType.Substring(startIndex)));
            }
            return sb.ToString();
        }

        protected override string GetResponseFileCmdArgs(CompilerParameters options, string cmdArgs)
        {
            return ("/noconfig " + base.GetResponseFileCmdArgs(options, cmdArgs));
        }

        private string GetTypeArgumentsOutput(CodeTypeReferenceCollection typeArguments)
        {
            StringBuilder sb = new StringBuilder(0x80);
            this.GetTypeArgumentsOutput(typeArguments, 0, typeArguments.Count, sb);
            return sb.ToString();
        }

        private void GetTypeArgumentsOutput(CodeTypeReferenceCollection typeArguments, int start, int length, StringBuilder sb)
        {
            sb.Append("(Of ");
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
            sb.Append(')');
        }

        protected override string GetTypeOutput(CodeTypeReference typeRef)
        {
            string str = string.Empty + this.GetTypeOutputWithoutArrayPostFix(typeRef);
            if (typeRef.ArrayRank > 0)
            {
                str = str + this.GetArrayPostfix(typeRef);
            }
            return str;
        }

        private string GetTypeOutputWithoutArrayPostFix(CodeTypeReference typeRef)
        {
            StringBuilder builder = new StringBuilder();
            while (typeRef.ArrayElementType != null)
            {
                typeRef = typeRef.ArrayElementType;
            }
            builder.Append(this.GetBaseTypeOutput(typeRef));
            return builder.ToString();
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

        private bool IsDocComment(CodeCommentStatement comment)
        {
            return (((comment != null) && (comment.Comment != null)) && comment.Comment.DocComment);
        }

        private bool IsGeneratingStatements()
        {
            return (this.statementDepth > 0);
        }

        public static bool IsKeyword(string value)
        {
            return FixedStringLookup.Contains(keywords, value, true);
        }

        protected override bool IsValidIdentifier(string value)
        {
            if ((value == null) || (value.Length == 0))
            {
                return false;
            }
            if (value.Length > 0x3ff)
            {
                return false;
            }
            if ((value[0] != '[') || (value[value.Length - 1] != ']'))
            {
                if (IsKeyword(value))
                {
                    return false;
                }
            }
            else
            {
                value = value.Substring(1, value.Length - 2);
            }
            if ((value.Length == 1) && (value[0] == '_'))
            {
                return false;
            }
            return CodeGenerator.IsValidLanguageIndependentIdentifier(value);
        }

        private bool MethodIsOverloaded(CodeMemberMethod e, CodeTypeDeclaration c)
        {
            if ((e.Attributes & MemberAttributes.Overloaded) != ((MemberAttributes) 0))
            {
                return true;
            }
            IEnumerator enumerator = c.Members.GetEnumerator();
            while (enumerator.MoveNext())
            {
                if (enumerator.Current is CodeMemberMethod)
                {
                    CodeMemberMethod current = (CodeMemberMethod) enumerator.Current;
                    if (((!(enumerator.Current is CodeTypeConstructor) && !(enumerator.Current is CodeConstructor)) && ((current != e) && current.Name.Equals(e.Name, StringComparison.OrdinalIgnoreCase))) && (current.PrivateImplementationType == null))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private void OutputArrayPostfix(CodeTypeReference typeRef)
        {
            if (typeRef.ArrayRank > 0)
            {
                base.Output.Write(this.GetArrayPostfix(typeRef));
            }
        }

        protected override void OutputAttributeArgument(CodeAttributeArgument arg)
        {
            if ((arg.Name != null) && (arg.Name.Length > 0))
            {
                this.OutputIdentifier(arg.Name);
                base.Output.Write(":=");
            }
            ((ICodeGenerator) this).GenerateCodeFromExpression(arg.Value, ((IndentedTextWriter) base.Output).InnerWriter, base.Options);
        }

        private void OutputAttributes(CodeAttributeDeclarationCollection attributes, bool inLine)
        {
            this.OutputAttributes(attributes, inLine, null, false);
        }

        private void OutputAttributes(CodeAttributeDeclarationCollection attributes, bool inLine, string prefix, bool closingLine)
        {
            if (attributes.Count != 0)
            {
                IEnumerator enumerator = attributes.GetEnumerator();
                bool flag = true;
                this.GenerateAttributeDeclarationsStart(attributes);
                while (enumerator.MoveNext())
                {
                    if (flag)
                    {
                        flag = false;
                    }
                    else
                    {
                        base.Output.Write(", ");
                        if (!inLine)
                        {
                            this.ContinueOnNewLine("");
                            base.Output.Write(" ");
                        }
                    }
                    if ((prefix != null) && (prefix.Length > 0))
                    {
                        base.Output.Write(prefix);
                    }
                    CodeAttributeDeclaration current = (CodeAttributeDeclaration) enumerator.Current;
                    if (current.AttributeType != null)
                    {
                        base.Output.Write(this.GetTypeOutput(current.AttributeType));
                    }
                    base.Output.Write("(");
                    bool flag2 = true;
                    foreach (CodeAttributeArgument argument in current.Arguments)
                    {
                        if (flag2)
                        {
                            flag2 = false;
                        }
                        else
                        {
                            base.Output.Write(", ");
                        }
                        this.OutputAttributeArgument(argument);
                    }
                    base.Output.Write(")");
                }
                this.GenerateAttributeDeclarationsEnd(attributes);
                base.Output.Write(" ");
                if (!inLine)
                {
                    if (closingLine)
                    {
                        base.Output.WriteLine();
                    }
                    else
                    {
                        this.ContinueOnNewLine("");
                    }
                }
            }
        }

        protected override void OutputDirection(FieldDirection dir)
        {
            switch (dir)
            {
                case FieldDirection.In:
                    base.Output.Write("ByVal ");
                    return;

                case FieldDirection.Out:
                case FieldDirection.Ref:
                    base.Output.Write("ByRef ");
                    return;
            }
        }

        protected override void OutputFieldScopeModifier(MemberAttributes attributes)
        {
            switch ((attributes & MemberAttributes.ScopeMask))
            {
                case MemberAttributes.Final:
                    base.Output.Write("");
                    return;

                case MemberAttributes.Static:
                    if (!this.IsCurrentModule)
                    {
                        base.Output.Write("Shared ");
                    }
                    return;

                case MemberAttributes.Const:
                    base.Output.Write("Const ");
                    return;
            }
            base.Output.Write("");
        }

        protected override void OutputIdentifier(string ident)
        {
            base.Output.Write(this.CreateEscapedIdentifier(ident));
        }

        protected override void OutputMemberAccessModifier(MemberAttributes attributes)
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
                            base.Output.Write("Protected ");
                        }
                        return;
                    }
                    base.Output.Write("Friend ");
                    return;
                }
            }
            else
            {
                switch (attributes2)
                {
                    case MemberAttributes.FamilyOrAssembly:
                        base.Output.Write("Protected Friend ");
                        return;

                    case MemberAttributes.Private:
                        base.Output.Write("Private ");
                        return;

                    case MemberAttributes.Public:
                        base.Output.Write("Public ");
                        return;
                }
                return;
            }
            base.Output.Write("Friend ");
        }

        protected override void OutputMemberScopeModifier(MemberAttributes attributes)
        {
            switch ((attributes & MemberAttributes.ScopeMask))
            {
                case MemberAttributes.Abstract:
                    base.Output.Write("MustOverride ");
                    return;

                case MemberAttributes.Final:
                    base.Output.Write("");
                    return;

                case MemberAttributes.Static:
                    if (this.IsCurrentModule)
                    {
                        break;
                    }
                    base.Output.Write("Shared ");
                    return;

                case MemberAttributes.Override:
                    base.Output.Write("Overrides ");
                    return;

                case MemberAttributes.Private:
                    base.Output.Write("Private ");
                    return;

                default:
                {
                    MemberAttributes attributes3 = attributes & MemberAttributes.AccessMask;
                    if (((attributes3 != MemberAttributes.Assembly) && (attributes3 != MemberAttributes.Family)) && (attributes3 != MemberAttributes.Public))
                    {
                        return;
                    }
                    base.Output.Write("Overridable ");
                    break;
                }
            }
        }

        protected override void OutputOperator(CodeBinaryOperatorType op)
        {
            switch (op)
            {
                case CodeBinaryOperatorType.Modulus:
                    base.Output.Write("Mod");
                    return;

                case CodeBinaryOperatorType.IdentityInequality:
                    base.Output.Write("<>");
                    return;

                case CodeBinaryOperatorType.IdentityEquality:
                    base.Output.Write("Is");
                    return;

                case CodeBinaryOperatorType.ValueEquality:
                    base.Output.Write("=");
                    return;

                case CodeBinaryOperatorType.BitwiseOr:
                    base.Output.Write("Or");
                    return;

                case CodeBinaryOperatorType.BitwiseAnd:
                    base.Output.Write("And");
                    return;

                case CodeBinaryOperatorType.BooleanOr:
                    base.Output.Write("OrElse");
                    return;

                case CodeBinaryOperatorType.BooleanAnd:
                    base.Output.Write("AndAlso");
                    return;
            }
            base.OutputOperator(op);
        }

        protected override void OutputType(CodeTypeReference typeRef)
        {
            base.Output.Write(this.GetTypeOutputWithoutArrayPostFix(typeRef));
        }

        private void OutputTypeAttributes(CodeTypeDeclaration e)
        {
            if ((e.Attributes & MemberAttributes.New) != ((MemberAttributes) 0))
            {
                base.Output.Write("Shadows ");
            }
            TypeAttributes typeAttributes = e.TypeAttributes;
            if (e.IsPartial)
            {
                base.Output.Write("Partial ");
            }
            switch ((typeAttributes & TypeAttributes.NestedFamORAssem))
            {
                case TypeAttributes.AnsiClass:
                case TypeAttributes.NestedAssembly:
                case TypeAttributes.NestedFamANDAssem:
                    base.Output.Write("Friend ");
                    break;

                case TypeAttributes.Public:
                case TypeAttributes.NestedPublic:
                    base.Output.Write("Public ");
                    break;

                case TypeAttributes.NestedPrivate:
                    base.Output.Write("Private ");
                    break;

                case TypeAttributes.NestedFamily:
                    base.Output.Write("Protected ");
                    break;

                case TypeAttributes.NestedFamORAssem:
                    base.Output.Write("Protected Friend ");
                    break;
            }
            if (e.IsStruct)
            {
                base.Output.Write("Structure ");
            }
            else if (e.IsEnum)
            {
                base.Output.Write("Enum ");
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
                    if (this.IsCurrentModule)
                    {
                        base.Output.Write("Module ");
                        return;
                    }
                    if ((typeAttributes & TypeAttributes.Sealed) == TypeAttributes.Sealed)
                    {
                        base.Output.Write("NotInheritable ");
                    }
                    if ((typeAttributes & TypeAttributes.Abstract) == TypeAttributes.Abstract)
                    {
                        base.Output.Write("MustInherit ");
                    }
                    base.Output.Write("Class ");
                    return;
                }
                base.Output.Write("Interface ");
            }
        }

        protected override void OutputTypeNamePair(CodeTypeReference typeRef, string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                name = "__exception";
            }
            this.OutputIdentifier(name);
            this.OutputArrayPostfix(typeRef);
            base.Output.Write(" As ");
            this.OutputType(typeRef);
        }

        private void OutputTypeParameterConstraints(CodeTypeParameter typeParameter)
        {
            CodeTypeReferenceCollection constraints = typeParameter.Constraints;
            int count = constraints.Count;
            if (typeParameter.HasConstructorConstraint)
            {
                count++;
            }
            if (count != 0)
            {
                base.Output.Write(" As ");
                if (count > 1)
                {
                    base.Output.Write(" {");
                }
                bool flag = true;
                foreach (CodeTypeReference reference in constraints)
                {
                    if (flag)
                    {
                        flag = false;
                    }
                    else
                    {
                        base.Output.Write(", ");
                    }
                    base.Output.Write(this.GetTypeOutput(reference));
                }
                if (typeParameter.HasConstructorConstraint)
                {
                    if (!flag)
                    {
                        base.Output.Write(", ");
                    }
                    base.Output.Write("New");
                }
                if (count > 1)
                {
                    base.Output.Write('}');
                }
            }
        }

        private void OutputTypeParameters(CodeTypeParameterCollection typeParameters)
        {
            if (typeParameters.Count != 0)
            {
                base.Output.Write("(Of ");
                bool flag = true;
                for (int i = 0; i < typeParameters.Count; i++)
                {
                    if (flag)
                    {
                        flag = false;
                    }
                    else
                    {
                        base.Output.Write(", ");
                    }
                    base.Output.Write(typeParameters[i].Name);
                    this.OutputTypeParameterConstraints(typeParameters[i]);
                }
                base.Output.Write(')');
            }
        }

        private void OutputVTableModifier(MemberAttributes attributes)
        {
            MemberAttributes attributes2 = attributes & MemberAttributes.VTableMask;
            if (attributes2 == MemberAttributes.New)
            {
                base.Output.Write("Shadows ");
            }
        }

        protected override void ProcessCompilerOutputLine(CompilerResults results, string line)
        {
            if (outputReg == null)
            {
                outputReg = new Regex(@"^([^(]*)\(?([0-9]*)\)? ?:? ?(error|warning) ([A-Z]+[0-9]+) ?: ((.|\n)*)");
            }
            Match match = outputReg.Match(line);
            if (match.Success)
            {
                CompilerError error = new CompilerError {
                    FileName = match.Groups[1].Value
                };
                string s = match.Groups[2].Value;
                if ((s != null) && (s.Length > 0))
                {
                    error.Line = int.Parse(s, CultureInfo.InvariantCulture);
                }
                if (string.Compare(match.Groups[3].Value, "warning", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    error.IsWarning = true;
                }
                error.ErrorNumber = match.Groups[4].Value;
                error.ErrorText = match.Groups[5].Value;
                results.Errors.Add(error);
            }
        }

        private bool PropertyIsOverloaded(CodeMemberProperty e, CodeTypeDeclaration c)
        {
            if ((e.Attributes & MemberAttributes.Overloaded) != ((MemberAttributes) 0))
            {
                return true;
            }
            IEnumerator enumerator = c.Members.GetEnumerator();
            while (enumerator.MoveNext())
            {
                if (enumerator.Current is CodeMemberProperty)
                {
                    CodeMemberProperty current = (CodeMemberProperty) enumerator.Current;
                    if (((current != e) && current.Name.Equals(e.Name, StringComparison.OrdinalIgnoreCase)) && (current.PrivateImplementationType == null))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        protected override string QuoteSnippetString(string value)
        {
            StringBuilder b = new StringBuilder(value.Length + 5);
            bool fInDoubleQuotes = true;
            Indentation indentation = new Indentation((IndentedTextWriter) base.Output, base.Indent + 1);
            b.Append("\"");
            for (int i = 0; i < value.Length; i++)
            {
                char ch = value[i];
                switch (ch)
                {
                    case '\t':
                        this.EnsureNotInDoubleQuotes(ref fInDoubleQuotes, b);
                        b.Append("&Global.Microsoft.VisualBasic.ChrW(9)");
                        goto Label_0186;

                    case '\n':
                        this.EnsureNotInDoubleQuotes(ref fInDoubleQuotes, b);
                        b.Append("&Global.Microsoft.VisualBasic.ChrW(10)");
                        goto Label_0186;

                    case '\r':
                        this.EnsureNotInDoubleQuotes(ref fInDoubleQuotes, b);
                        if ((i >= (value.Length - 1)) || (value[i + 1] != '\n'))
                        {
                            break;
                        }
                        b.Append("&Global.Microsoft.VisualBasic.ChrW(13)&Global.Microsoft.VisualBasic.ChrW(10)");
                        i++;
                        goto Label_0186;

                    case '"':
                    case '“':
                    case '”':
                    case 0xff02:
                        this.EnsureInDoubleQuotes(ref fInDoubleQuotes, b);
                        b.Append(ch);
                        b.Append(ch);
                        goto Label_0186;

                    case '\0':
                        this.EnsureNotInDoubleQuotes(ref fInDoubleQuotes, b);
                        b.Append("&Global.Microsoft.VisualBasic.ChrW(0)");
                        goto Label_0186;

                    case '\u2028':
                    case '\u2029':
                        this.EnsureNotInDoubleQuotes(ref fInDoubleQuotes, b);
                        AppendEscapedChar(b, ch);
                        goto Label_0186;

                    default:
                        this.EnsureInDoubleQuotes(ref fInDoubleQuotes, b);
                        b.Append(value[i]);
                        goto Label_0186;
                }
                b.Append("&Global.Microsoft.VisualBasic.ChrW(13)");
            Label_0186:
                if ((i > 0) && ((i % 80) == 0))
                {
                    if ((char.IsHighSurrogate(value[i]) && (i < (value.Length - 1))) && char.IsLowSurrogate(value[i + 1]))
                    {
                        b.Append(value[++i]);
                    }
                    if (fInDoubleQuotes)
                    {
                        b.Append("\"");
                    }
                    fInDoubleQuotes = true;
                    b.Append("& _ ");
                    b.Append(Environment.NewLine);
                    b.Append(indentation.IndentationString);
                    b.Append('"');
                }
            }
            if (fInDoubleQuotes)
            {
                b.Append("\"");
            }
            return b.ToString();
        }

        private static byte[] ReadAllBytes(string file, FileShare share)
        {
            byte[] buffer;
            using (FileStream stream = File.Open(file, FileMode.Open, FileAccess.Read, share))
            {
                int offset = 0;
                long length = stream.Length;
                if (length > 0x7fffffffL)
                {
                    throw new ArgumentOutOfRangeException("file");
                }
                int count = (int) length;
                buffer = new byte[count];
                while (count > 0)
                {
                    int num4 = stream.Read(buffer, offset, count);
                    if (num4 == 0)
                    {
                        throw new EndOfStreamException();
                    }
                    offset += num4;
                    count -= num4;
                }
            }
            return buffer;
        }

        protected bool RequireVariableDeclaration(CodeCompileUnit e)
        {
            object obj2 = e.UserData["RequireVariableDeclaration"];
            if ((obj2 != null) && (obj2 is bool))
            {
                return (bool) obj2;
            }
            return true;
        }

        protected override bool Supports(GeneratorSupport support)
        {
            return ((support & (GeneratorSupport.DeclareIndexerProperties | GeneratorSupport.GenericTypeDeclaration | GeneratorSupport.GenericTypeReference | GeneratorSupport.PartialTypes | GeneratorSupport.Resources | GeneratorSupport.Win32Resources | GeneratorSupport.ComplexExpressions | GeneratorSupport.PublicStaticMembers | GeneratorSupport.MultipleInterfaceMembers | GeneratorSupport.NestedTypes | GeneratorSupport.ChainedConstructorArguments | GeneratorSupport.ReferenceParameters | GeneratorSupport.ParameterAttributes | GeneratorSupport.AssemblyAttributes | GeneratorSupport.DeclareEvents | GeneratorSupport.DeclareInterfaces | GeneratorSupport.DeclareDelegates | GeneratorSupport.DeclareEnums | GeneratorSupport.DeclareValueTypes | GeneratorSupport.ReturnTypeAttributes | GeneratorSupport.TryCatchStatements | GeneratorSupport.StaticConstructors | GeneratorSupport.MultidimensionalArrays | GeneratorSupport.GotoStatements | GeneratorSupport.EntryPointMethod | GeneratorSupport.ArraysOfArrays)) == support);
        }

        protected override string CompilerName
        {
            get
            {
                return "vbc.exe";
            }
        }

        protected override string FileExtension
        {
            get
            {
                return ".vb";
            }
        }

        private bool IsCurrentModule
        {
            get
            {
                return (base.IsCurrentClass && this.GetUserData(base.CurrentClass, "Module", false));
            }
        }

        protected override string NullToken
        {
            get
            {
                return "Nothing";
            }
        }
    }
}

