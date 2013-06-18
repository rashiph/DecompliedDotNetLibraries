namespace Microsoft.JScript
{
    using Microsoft.JScript.Vsa;
    using System;
    using System.CodeDom;
    using System.CodeDom.Compiler;
    using System.Collections;
    using System.ComponentModel;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Text;
    using System.Text.RegularExpressions;

    [DesignerCategory("code")]
    internal sealed class JSCodeGenerator : CodeCompiler
    {
        private bool forLoopHack;
        private bool isArgumentList = true;
        private static Hashtable keywords = new Hashtable(150);
        private const GeneratorSupport LanguageSupport = (GeneratorSupport.PublicStaticMembers | GeneratorSupport.AssemblyAttributes | GeneratorSupport.DeclareInterfaces | GeneratorSupport.DeclareEnums | GeneratorSupport.TryCatchStatements | GeneratorSupport.StaticConstructors | GeneratorSupport.MultidimensionalArrays | GeneratorSupport.EntryPointMethod | GeneratorSupport.ArraysOfArrays);
        private string mainClassName;
        private string mainMethodName;
        private const int MaxLineLength = 80;
        private static Regex outputReg = new Regex(@"(([^(]+)(\(([0-9]+),([0-9]+)\))[ \t]*:[ \t]+)?(fatal )?(error|warning)[ \t]+([A-Z]+[0-9]+)[ \t]*:[ \t]*(.*)");

        static JSCodeGenerator()
        {
            object obj2 = new object();
            keywords["abstract"] = obj2;
            keywords["assert"] = obj2;
            keywords["boolean"] = obj2;
            keywords["break"] = obj2;
            keywords["byte"] = obj2;
            keywords["case"] = obj2;
            keywords["catch"] = obj2;
            keywords["char"] = obj2;
            keywords["class"] = obj2;
            keywords["const"] = obj2;
            keywords["continue"] = obj2;
            keywords["debugger"] = obj2;
            keywords["decimal"] = obj2;
            keywords["default"] = obj2;
            keywords["delete"] = obj2;
            keywords["do"] = obj2;
            keywords["double"] = obj2;
            keywords["else"] = obj2;
            keywords["ensure"] = obj2;
            keywords["enum"] = obj2;
            keywords["event"] = obj2;
            keywords["export"] = obj2;
            keywords["extends"] = obj2;
            keywords["false"] = obj2;
            keywords["final"] = obj2;
            keywords["finally"] = obj2;
            keywords["float"] = obj2;
            keywords["for"] = obj2;
            keywords["function"] = obj2;
            keywords["get"] = obj2;
            keywords["goto"] = obj2;
            keywords["if"] = obj2;
            keywords["implements"] = obj2;
            keywords["import"] = obj2;
            keywords["in"] = obj2;
            keywords["instanceof"] = obj2;
            keywords["int"] = obj2;
            keywords["invariant"] = obj2;
            keywords["interface"] = obj2;
            keywords["internal"] = obj2;
            keywords["long"] = obj2;
            keywords["namespace"] = obj2;
            keywords["native"] = obj2;
            keywords["new"] = obj2;
            keywords["null"] = obj2;
            keywords["package"] = obj2;
            keywords["private"] = obj2;
            keywords["protected"] = obj2;
            keywords["public"] = obj2;
            keywords["require"] = obj2;
            keywords["return"] = obj2;
            keywords["sbyte"] = obj2;
            keywords["scope"] = obj2;
            keywords["set"] = obj2;
            keywords["short"] = obj2;
            keywords["static"] = obj2;
            keywords["super"] = obj2;
            keywords["switch"] = obj2;
            keywords["synchronized"] = obj2;
            keywords["this"] = obj2;
            keywords["throw"] = obj2;
            keywords["throws"] = obj2;
            keywords["transient"] = obj2;
            keywords["true"] = obj2;
            keywords["try"] = obj2;
            keywords["typeof"] = obj2;
            keywords["use"] = obj2;
            keywords["uint"] = obj2;
            keywords["ulong"] = obj2;
            keywords["ushort"] = obj2;
            keywords["var"] = obj2;
            keywords["void"] = obj2;
            keywords["volatile"] = obj2;
            keywords["while"] = obj2;
            keywords["with"] = obj2;
        }

        protected override string CmdArgsFromParameters(CompilerParameters options)
        {
            StringBuilder builder = new StringBuilder(0x80);
            string str = (Path.DirectorySeparatorChar == '/') ? "-" : "/";
            builder.Append(str + "utf8output ");
            object obj2 = new object();
            Hashtable hashtable = new Hashtable(20);
            foreach (string str2 in options.ReferencedAssemblies)
            {
                if (hashtable[str2] == null)
                {
                    hashtable[str2] = obj2;
                    builder.Append(str + "r:");
                    builder.Append("\"");
                    builder.Append(str2);
                    builder.Append("\" ");
                }
            }
            builder.Append(str + "out:");
            builder.Append("\"");
            builder.Append(options.OutputAssembly);
            builder.Append("\" ");
            if (options.IncludeDebugInformation)
            {
                builder.Append(str + "d:DEBUG ");
                builder.Append(str + "debug+ ");
            }
            else
            {
                builder.Append(str + "debug- ");
            }
            if (options.TreatWarningsAsErrors)
            {
                builder.Append(str + "warnaserror ");
            }
            if (options.WarningLevel >= 0)
            {
                builder.Append(str + "w:" + options.WarningLevel.ToString(CultureInfo.InvariantCulture) + " ");
            }
            if (options.Win32Resource != null)
            {
                builder.Append(str + "win32res:\"" + options.Win32Resource + "\" ");
            }
            return builder.ToString();
        }

        protected override string CreateEscapedIdentifier(string name)
        {
            if (this.IsKeyword(name))
            {
                return (@"\" + name);
            }
            return name;
        }

        protected override string CreateValidIdentifier(string name)
        {
            if (this.IsKeyword(name))
            {
                return ("$" + name);
            }
            return name;
        }

        protected override CompilerResults FromFileBatch(CompilerParameters options, string[] fileNames)
        {
            string outputFile = options.TempFiles.AddExtension("out");
            CompilerResults results = new CompilerResults(options.TempFiles);
            if ((options.OutputAssembly == null) || (options.OutputAssembly.Length == 0))
            {
                options.OutputAssembly = results.TempFiles.AddExtension("dll", !options.GenerateInMemory);
            }
            string partialCmdLine = null;
            if (options.IncludeDebugInformation)
            {
                results.TempFiles.AddExtension("pdb");
                partialCmdLine = this.CmdArgsFromParameters(options);
            }
            results.NativeCompilerReturnValue = 0;
            try
            {
                results.NativeCompilerReturnValue = new JSInProcCompiler().Compile(options, partialCmdLine, fileNames, outputFile);
            }
            catch
            {
                results.NativeCompilerReturnValue = 10;
            }
            try
            {
                StreamReader reader = new StreamReader(outputFile);
                try
                {
                    for (string str3 = reader.ReadLine(); str3 != null; str3 = reader.ReadLine())
                    {
                        results.Output.Add(str3);
                        this.ProcessCompilerOutputLine(results, str3);
                    }
                }
                finally
                {
                    reader.Close();
                }
            }
            catch (Exception exception)
            {
                results.Output.Add(JScriptException.Localize("No error output", CultureInfo.CurrentUICulture));
                results.Output.Add(exception.ToString());
            }
            if ((results.NativeCompilerReturnValue == 0) && options.GenerateInMemory)
            {
                FileStream stream = new FileStream(options.OutputAssembly, FileMode.Open, FileAccess.Read, FileShare.Read);
                try
                {
                    int length = (int) stream.Length;
                    byte[] buffer = new byte[length];
                    stream.Read(buffer, 0, length);
                    results.CompiledAssembly = Assembly.Load(buffer, null, options.Evidence);
                }
                finally
                {
                    stream.Close();
                }
            }
            else
            {
                results.PathToAssembly = Path.GetFullPath(options.OutputAssembly);
            }
            results.Evidence = options.Evidence;
            return results;
        }

        protected override void GenerateArgumentReferenceExpression(CodeArgumentReferenceExpression e)
        {
            this.OutputIdentifier(e.ParameterName);
        }

        protected override void GenerateArrayCreateExpression(CodeArrayCreateExpression e)
        {
            CodeExpressionCollection initializers = e.Initializers;
            if (initializers.Count > 0)
            {
                base.Output.Write("[");
                base.Indent++;
                this.OutputExpressionList(initializers);
                base.Indent--;
                base.Output.Write("]");
            }
            else
            {
                base.Output.Write("new ");
                base.Output.Write(this.GetBaseTypeOutput(e.CreateType.BaseType));
                base.Output.Write("[");
                if (e.SizeExpression != null)
                {
                    base.GenerateExpression(e.SizeExpression);
                }
                else
                {
                    base.Output.Write(e.Size.ToString(CultureInfo.InvariantCulture));
                }
                base.Output.Write("]");
            }
        }

        protected override void GenerateArrayIndexerExpression(CodeArrayIndexerExpression e)
        {
            base.GenerateExpression(e.TargetObject);
            base.Output.Write("[");
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
            base.Output.Write("]");
        }

        private void GenerateAssemblyAttributes(CodeAttributeDeclarationCollection attributes)
        {
            if (attributes.Count != 0)
            {
                IEnumerator enumerator = attributes.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    base.Output.Write("[");
                    base.Output.Write("assembly: ");
                    CodeAttributeDeclaration current = (CodeAttributeDeclaration) enumerator.Current;
                    base.Output.Write(this.GetBaseTypeOutput(current.Name));
                    base.Output.Write("(");
                    bool flag = true;
                    foreach (CodeAttributeArgument argument in current.Arguments)
                    {
                        if (flag)
                        {
                            flag = false;
                        }
                        else
                        {
                            base.Output.Write(", ");
                        }
                        this.OutputAttributeArgument(argument);
                    }
                    base.Output.Write(")");
                    base.Output.Write("]");
                    base.Output.WriteLine();
                }
            }
        }

        protected override void GenerateAssignStatement(CodeAssignStatement e)
        {
            base.GenerateExpression(e.Left);
            base.Output.Write(" = ");
            base.GenerateExpression(e.Right);
            if (!this.forLoopHack)
            {
                base.Output.WriteLine(";");
            }
        }

        protected override void GenerateAttachEventStatement(CodeAttachEventStatement e)
        {
            base.GenerateExpression(e.Event.TargetObject);
            base.Output.Write(".add_");
            base.Output.Write(e.Event.EventName);
            base.Output.Write("(");
            base.GenerateExpression(e.Listener);
            base.Output.WriteLine(");");
        }

        protected override void GenerateAttributeDeclarationsEnd(CodeAttributeDeclarationCollection attributes)
        {
        }

        protected override void GenerateAttributeDeclarationsStart(CodeAttributeDeclarationCollection attributes)
        {
        }

        protected override void GenerateBaseReferenceExpression(CodeBaseReferenceExpression e)
        {
            base.Output.Write("super");
        }

        protected override void GenerateCastExpression(CodeCastExpression e)
        {
            this.OutputType(e.TargetType);
            base.Output.Write("(");
            base.GenerateExpression(e.Expression);
            base.Output.Write(")");
        }

        protected override void GenerateComment(CodeComment e)
        {
            string text = e.Text;
            StringBuilder builder = new StringBuilder(text.Length * 2);
            string str2 = e.DocComment ? "///" : "//";
            builder.Append(str2);
            for (int i = 0; i < text.Length; i++)
            {
                switch (text[i])
                {
                    case '\u2028':
                        builder.Append("\u2028" + str2);
                        break;

                    case '\u2029':
                        builder.Append("\u2029" + str2);
                        break;

                    case '@':
                        break;

                    case '\n':
                        builder.Append("\n" + str2);
                        break;

                    case '\r':
                        if ((i < (text.Length - 1)) && (text[i + 1] == '\n'))
                        {
                            builder.Append("\r\n" + str2);
                            i++;
                        }
                        else
                        {
                            builder.Append("\r" + str2);
                        }
                        break;

                    default:
                        builder.Append(text[i]);
                        break;
                }
            }
            base.Output.WriteLine(builder.ToString());
        }

        protected override void GenerateCompileUnitStart(CodeCompileUnit e)
        {
            base.Output.WriteLine("//------------------------------------------------------------------------------");
            base.Output.WriteLine("/// <autogenerated>");
            base.Output.WriteLine("///     This code was generated by a tool.");
            base.Output.WriteLine("///     Runtime Version: " + Environment.Version.ToString());
            base.Output.WriteLine("///");
            base.Output.WriteLine("///     Changes to this file may cause incorrect behavior and will be lost if ");
            base.Output.WriteLine("///     the code is regenerated.");
            base.Output.WriteLine("/// </autogenerated>");
            base.Output.WriteLine("//------------------------------------------------------------------------------");
            base.Output.WriteLine("");
            if (e.AssemblyCustomAttributes.Count > 0)
            {
                this.GenerateAssemblyAttributes(e.AssemblyCustomAttributes);
                base.Output.WriteLine("");
            }
        }

        protected override void GenerateConditionStatement(CodeConditionStatement e)
        {
            base.Output.Write("if (");
            base.Indent += 2;
            base.GenerateExpression(e.Condition);
            base.Indent -= 2;
            base.Output.Write(")");
            this.OutputStartingBrace();
            base.Indent++;
            base.GenerateStatements(e.TrueStatements);
            base.Indent--;
            if (e.FalseStatements.Count > 0)
            {
                base.Output.Write("}");
                if (base.Options.ElseOnClosing)
                {
                    base.Output.Write(" ");
                }
                else
                {
                    base.Output.WriteLine("");
                }
                base.Output.Write("else");
                this.OutputStartingBrace();
                base.Indent++;
                base.GenerateStatements(e.FalseStatements);
                base.Indent--;
            }
            base.Output.WriteLine("}");
        }

        protected override void GenerateConstructor(CodeConstructor e, CodeTypeDeclaration c)
        {
            if (base.IsCurrentClass || base.IsCurrentStruct)
            {
                this.OutputMemberAccessModifier(e.Attributes);
                if (e.CustomAttributes.Count > 0)
                {
                    this.OutputAttributeDeclarations(e.CustomAttributes);
                }
                base.Output.Write("function ");
                this.OutputIdentifier(base.CurrentTypeName);
                base.Output.Write("(");
                this.OutputParameters(e.Parameters);
                base.Output.Write(")");
                CodeExpressionCollection baseConstructorArgs = e.BaseConstructorArgs;
                CodeExpressionCollection chainedConstructorArgs = e.ChainedConstructorArgs;
                this.OutputStartingBrace();
                base.Indent++;
                if (baseConstructorArgs.Count > 0)
                {
                    base.Output.Write("super(");
                    this.OutputExpressionList(baseConstructorArgs);
                    base.Output.WriteLine(");");
                }
                if (chainedConstructorArgs.Count > 0)
                {
                    base.Output.Write("this(");
                    this.OutputExpressionList(chainedConstructorArgs);
                    base.Output.WriteLine(");");
                }
                base.GenerateStatements(e.Statements);
                base.Output.WriteLine();
                base.Indent--;
                base.Output.WriteLine("}");
            }
        }

        protected override void GenerateDelegateCreateExpression(CodeDelegateCreateExpression e)
        {
            bool flag = e.DelegateType != null;
            if (flag)
            {
                this.OutputType(e.DelegateType);
                base.Output.Write("(");
            }
            base.GenerateExpression(e.TargetObject);
            base.Output.Write(".");
            this.OutputIdentifier(e.MethodName);
            if (flag)
            {
                base.Output.Write(")");
            }
        }

        protected override void GenerateDelegateInvokeExpression(CodeDelegateInvokeExpression e)
        {
            if (e.TargetObject != null)
            {
                base.GenerateExpression(e.TargetObject);
            }
            base.Output.Write("(");
            this.OutputExpressionList(e.Parameters);
            base.Output.Write(")");
        }

        protected override void GenerateEntryPointMethod(CodeEntryPointMethod e, CodeTypeDeclaration c)
        {
            base.Output.Write("public static ");
            if (e.CustomAttributes.Count > 0)
            {
                this.OutputAttributeDeclarations(e.CustomAttributes);
            }
            base.Output.Write("function Main()");
            this.OutputStartingBrace();
            base.Indent++;
            base.GenerateStatements(e.Statements);
            base.Indent--;
            base.Output.WriteLine("}");
            this.mainClassName = base.CurrentTypeName;
            this.mainMethodName = "Main";
        }

        protected override void GenerateEvent(CodeMemberEvent e, CodeTypeDeclaration c)
        {
            throw new Exception(JScriptException.Localize("No event declarations", CultureInfo.CurrentUICulture));
        }

        protected override void GenerateEventReferenceExpression(CodeEventReferenceExpression e)
        {
            throw new Exception(JScriptException.Localize("No event references", CultureInfo.CurrentUICulture));
        }

        protected override void GenerateExpressionStatement(CodeExpressionStatement e)
        {
            base.GenerateExpression(e.Expression);
            if (!this.forLoopHack)
            {
                base.Output.WriteLine(";");
            }
        }

        protected override void GenerateField(CodeMemberField e)
        {
            if (base.IsCurrentDelegate || base.IsCurrentInterface)
            {
                throw new Exception(JScriptException.Localize("Only methods on interfaces", CultureInfo.CurrentUICulture));
            }
            if (base.IsCurrentEnum)
            {
                this.OutputIdentifier(e.Name);
                if (e.InitExpression != null)
                {
                    base.Output.Write(" = ");
                    base.GenerateExpression(e.InitExpression);
                }
                base.Output.WriteLine(",");
            }
            else
            {
                this.OutputMemberAccessModifier(e.Attributes);
                if ((e.Attributes & MemberAttributes.ScopeMask) == MemberAttributes.Static)
                {
                    base.Output.Write("static ");
                }
                if (e.CustomAttributes.Count > 0)
                {
                    this.OutputAttributeDeclarations(e.CustomAttributes);
                    base.Output.WriteLine("");
                }
                if ((e.Attributes & MemberAttributes.Const) == MemberAttributes.Const)
                {
                    if ((e.Attributes & MemberAttributes.ScopeMask) != MemberAttributes.Static)
                    {
                        base.Output.Write("static ");
                    }
                    base.Output.Write("const ");
                }
                else
                {
                    base.Output.Write("var ");
                }
                this.OutputTypeNamePair(e.Type, e.Name);
                if (e.InitExpression != null)
                {
                    base.Output.Write(" = ");
                    base.GenerateExpression(e.InitExpression);
                }
                base.Output.WriteLine(";");
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

        protected override void GenerateGotoStatement(CodeGotoStatement e)
        {
            throw new Exception(JScriptException.Localize("No goto statements", CultureInfo.CurrentUICulture));
        }

        protected override void GenerateIndexerExpression(CodeIndexerExpression e)
        {
            base.GenerateExpression(e.TargetObject);
            base.Output.Write("[");
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
            base.Output.Write("]");
        }

        protected override void GenerateIterationStatement(CodeIterationStatement e)
        {
            this.forLoopHack = true;
            base.Output.Write("for (");
            base.GenerateStatement(e.InitStatement);
            base.Output.Write("; ");
            base.GenerateExpression(e.TestExpression);
            base.Output.Write("; ");
            base.GenerateStatement(e.IncrementStatement);
            base.Output.Write(")");
            this.OutputStartingBrace();
            this.forLoopHack = false;
            base.Indent++;
            base.GenerateStatements(e.Statements);
            base.Indent--;
            base.Output.WriteLine("}");
        }

        protected override void GenerateLabeledStatement(CodeLabeledStatement e)
        {
            throw new Exception(JScriptException.Localize("No goto statements", CultureInfo.CurrentUICulture));
        }

        protected override void GenerateLinePragmaEnd(CodeLinePragma e)
        {
            base.Output.WriteLine("");
            base.Output.WriteLine("//@set @position(end)");
        }

        protected override void GenerateLinePragmaStart(CodeLinePragma e)
        {
            base.Output.WriteLine("");
            base.Output.WriteLine("//@cc_on");
            base.Output.Write("//@set @position(file=\"");
            base.Output.Write(Regex.Replace(e.FileName, @"\\", @"\\"));
            base.Output.Write("\";line=");
            base.Output.Write(e.LineNumber.ToString(CultureInfo.InvariantCulture));
            base.Output.WriteLine(")");
        }

        protected override void GenerateMethod(CodeMemberMethod e, CodeTypeDeclaration c)
        {
            if (!base.IsCurrentInterface)
            {
                if (e.PrivateImplementationType == null)
                {
                    this.OutputMemberAccessModifier(e.Attributes);
                    this.OutputMemberVTableModifier(e.Attributes);
                    this.OutputMemberScopeModifier(e.Attributes);
                }
            }
            else
            {
                this.OutputMemberVTableModifier(e.Attributes);
            }
            if (e.CustomAttributes.Count > 0)
            {
                this.OutputAttributeDeclarations(e.CustomAttributes);
            }
            base.Output.Write("function ");
            if ((e.PrivateImplementationType != null) && !base.IsCurrentInterface)
            {
                base.Output.Write(e.PrivateImplementationType.BaseType);
                base.Output.Write(".");
            }
            this.OutputIdentifier(e.Name);
            base.Output.Write("(");
            this.isArgumentList = false;
            try
            {
                this.OutputParameters(e.Parameters);
            }
            finally
            {
                this.isArgumentList = true;
            }
            base.Output.Write(")");
            if ((e.ReturnType.BaseType.Length > 0) && (string.Compare(e.ReturnType.BaseType, typeof(void).FullName, StringComparison.Ordinal) != 0))
            {
                base.Output.Write(" : ");
                this.OutputType(e.ReturnType);
            }
            if (!base.IsCurrentInterface && ((e.Attributes & MemberAttributes.ScopeMask) != MemberAttributes.Abstract))
            {
                this.OutputStartingBrace();
                base.Indent++;
                base.GenerateStatements(e.Statements);
                base.Indent--;
                base.Output.WriteLine("}");
            }
            else
            {
                base.Output.WriteLine(";");
            }
        }

        protected override void GenerateMethodInvokeExpression(CodeMethodInvokeExpression e)
        {
            this.GenerateMethodReferenceExpression(e.Method);
            base.Output.Write("(");
            this.OutputExpressionList(e.Parameters);
            base.Output.Write(")");
        }

        protected override void GenerateMethodReferenceExpression(CodeMethodReferenceExpression e)
        {
            if (e.TargetObject != null)
            {
                if (e.TargetObject is CodeBinaryOperatorExpression)
                {
                    base.Output.Write("(");
                    base.GenerateExpression(e.TargetObject);
                    base.Output.Write(")");
                }
                else
                {
                    base.GenerateExpression(e.TargetObject);
                }
                base.Output.Write(".");
            }
            this.OutputIdentifier(e.MethodName);
        }

        protected override void GenerateMethodReturnStatement(CodeMethodReturnStatement e)
        {
            base.Output.Write("return");
            if (e.Expression != null)
            {
                base.Output.Write(" ");
                base.GenerateExpression(e.Expression);
            }
            base.Output.WriteLine(";");
        }

        protected override void GenerateNamespace(CodeNamespace e)
        {
            base.Output.WriteLine("//@cc_on");
            base.Output.WriteLine("//@set @debug(off)");
            base.Output.WriteLine("");
            base.GenerateNamespaceImports(e);
            base.Output.WriteLine("");
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
                base.Output.WriteLine("}");
            }
            if (this.mainClassName != null)
            {
                if (e.Name != null)
                {
                    this.OutputIdentifier(e.Name);
                    base.Output.Write(".");
                }
                this.OutputIdentifier(this.mainClassName);
                base.Output.Write(".");
                this.OutputIdentifier(this.mainMethodName);
                base.Output.WriteLine("();");
                this.mainClassName = null;
            }
        }

        protected override void GenerateNamespaceImport(CodeNamespaceImport e)
        {
            base.Output.Write("import ");
            this.OutputIdentifier(e.Namespace);
            base.Output.WriteLine(";");
        }

        protected override void GenerateNamespaceStart(CodeNamespace e)
        {
            if ((e.Name != null) && (e.Name.Length > 0))
            {
                base.Output.Write("package ");
                this.OutputIdentifier(e.Name);
                this.OutputStartingBrace();
                base.Indent++;
            }
        }

        protected override void GenerateObjectCreateExpression(CodeObjectCreateExpression e)
        {
            base.Output.Write("new ");
            this.OutputType(e.CreateType);
            base.Output.Write("(");
            this.OutputExpressionList(e.Parameters);
            base.Output.Write(")");
        }

        protected override void GenerateParameterDeclarationExpression(CodeParameterDeclarationExpression e)
        {
            if (e.CustomAttributes.Count > 0)
            {
                CodeAttributeDeclaration declaration = e.CustomAttributes[0];
                if (declaration.Name != "ParamArrayAttribute")
                {
                    throw new Exception(JScriptException.Localize("No parameter attributes", CultureInfo.CurrentUICulture));
                }
                base.Output.Write("... ");
            }
            this.OutputDirection(e.Direction);
            this.OutputTypeNamePair(e.Type, e.Name);
        }

        private void GeneratePrimitiveChar(char c)
        {
            base.Output.Write('\'');
            switch (c)
            {
                case '\u2028':
                    base.Output.Write(@"\u2028");
                    break;

                case '\u2029':
                    base.Output.Write(@"\u2029");
                    break;

                case '\\':
                    base.Output.Write(@"\\");
                    break;

                case '\'':
                    base.Output.Write(@"\'");
                    break;

                case '\t':
                    base.Output.Write(@"\t");
                    break;

                case '\n':
                    base.Output.Write(@"\n");
                    break;

                case '\r':
                    base.Output.Write(@"\r");
                    break;

                case '"':
                    base.Output.Write("\\\"");
                    break;

                case '\0':
                    base.Output.Write(@"\0");
                    break;

                default:
                    base.Output.Write(c);
                    break;
            }
            base.Output.Write('\'');
        }

        protected override void GeneratePrimitiveExpression(CodePrimitiveExpression e)
        {
            if (e.Value == null)
            {
                base.Output.Write("undefined");
            }
            else if (e.Value is DBNull)
            {
                base.Output.Write("null");
            }
            else if (e.Value is char)
            {
                this.GeneratePrimitiveChar((char) e.Value);
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
                if (e.HasGet)
                {
                    if (!base.IsCurrentInterface)
                    {
                        if (e.PrivateImplementationType == null)
                        {
                            this.OutputMemberAccessModifier(e.Attributes);
                            this.OutputMemberVTableModifier(e.Attributes);
                            this.OutputMemberScopeModifier(e.Attributes);
                        }
                    }
                    else
                    {
                        this.OutputMemberVTableModifier(e.Attributes);
                    }
                    if (e.CustomAttributes.Count > 0)
                    {
                        if (base.IsCurrentInterface)
                        {
                            base.Output.Write("public ");
                        }
                        this.OutputAttributeDeclarations(e.CustomAttributes);
                        base.Output.WriteLine("");
                    }
                    base.Output.Write("function get ");
                    if ((e.PrivateImplementationType != null) && !base.IsCurrentInterface)
                    {
                        base.Output.Write(e.PrivateImplementationType.BaseType);
                        base.Output.Write(".");
                    }
                    this.OutputIdentifier(e.Name);
                    if (e.Parameters.Count > 0)
                    {
                        throw new Exception(JScriptException.Localize("No indexer declarations", CultureInfo.CurrentUICulture));
                    }
                    base.Output.Write("() : ");
                    this.OutputType(e.Type);
                    if (base.IsCurrentInterface || ((e.Attributes & MemberAttributes.ScopeMask) == MemberAttributes.Abstract))
                    {
                        base.Output.WriteLine(";");
                    }
                    else
                    {
                        this.OutputStartingBrace();
                        base.Indent++;
                        base.GenerateStatements(e.GetStatements);
                        base.Indent--;
                        base.Output.WriteLine("}");
                    }
                }
                if (e.HasSet)
                {
                    if (!base.IsCurrentInterface)
                    {
                        if (e.PrivateImplementationType == null)
                        {
                            this.OutputMemberAccessModifier(e.Attributes);
                            this.OutputMemberVTableModifier(e.Attributes);
                            this.OutputMemberScopeModifier(e.Attributes);
                        }
                    }
                    else
                    {
                        this.OutputMemberVTableModifier(e.Attributes);
                    }
                    if ((e.CustomAttributes.Count > 0) && !e.HasGet)
                    {
                        if (base.IsCurrentInterface)
                        {
                            base.Output.Write("public ");
                        }
                        this.OutputAttributeDeclarations(e.CustomAttributes);
                        base.Output.WriteLine("");
                    }
                    base.Output.Write("function set ");
                    if ((e.PrivateImplementationType != null) && !base.IsCurrentInterface)
                    {
                        base.Output.Write(e.PrivateImplementationType.BaseType);
                        base.Output.Write(".");
                    }
                    this.OutputIdentifier(e.Name);
                    base.Output.Write("(");
                    this.OutputTypeNamePair(e.Type, "value");
                    if (e.Parameters.Count > 0)
                    {
                        throw new Exception(JScriptException.Localize("No indexer declarations", CultureInfo.CurrentUICulture));
                    }
                    base.Output.Write(")");
                    if (base.IsCurrentInterface || ((e.Attributes & MemberAttributes.ScopeMask) == MemberAttributes.Abstract))
                    {
                        base.Output.WriteLine(";");
                    }
                    else
                    {
                        this.OutputStartingBrace();
                        base.Indent++;
                        base.GenerateStatements(e.SetStatements);
                        base.Indent--;
                        base.Output.WriteLine("}");
                    }
                }
            }
        }

        protected override void GeneratePropertyReferenceExpression(CodePropertyReferenceExpression e)
        {
            if (e.TargetObject != null)
            {
                base.GenerateExpression(e.TargetObject);
                base.Output.Write(".");
            }
            this.OutputIdentifier(e.PropertyName);
        }

        protected override void GeneratePropertySetValueReferenceExpression(CodePropertySetValueReferenceExpression e)
        {
            base.Output.Write("value");
        }

        protected override void GenerateRemoveEventStatement(CodeRemoveEventStatement e)
        {
            base.GenerateExpression(e.Event.TargetObject);
            base.Output.Write(".remove_");
            base.Output.Write(e.Event.EventName);
            base.Output.Write("(");
            base.GenerateExpression(e.Listener);
            base.Output.WriteLine(");");
        }

        protected override void GenerateSingleFloatValue(float s)
        {
            base.Output.Write("float(");
            base.Output.Write(s.ToString(CultureInfo.InvariantCulture));
            base.Output.Write(")");
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
            base.Output.Write("this");
        }

        protected override void GenerateThrowExceptionStatement(CodeThrowExceptionStatement e)
        {
            base.Output.Write("throw");
            if (e.ToThrow != null)
            {
                base.Output.Write(" ");
                base.GenerateExpression(e.ToThrow);
            }
            base.Output.WriteLine(";");
        }

        protected override void GenerateTryCatchFinallyStatement(CodeTryCatchFinallyStatement e)
        {
            base.Output.Write("try");
            this.OutputStartingBrace();
            base.Indent++;
            base.GenerateStatements(e.TryStatements);
            base.Indent--;
            CodeCatchClauseCollection catchClauses = e.CatchClauses;
            if (catchClauses.Count > 0)
            {
                IEnumerator enumerator = catchClauses.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    base.Output.Write("}");
                    if (base.Options.ElseOnClosing)
                    {
                        base.Output.Write(" ");
                    }
                    else
                    {
                        base.Output.WriteLine("");
                    }
                    CodeCatchClause current = (CodeCatchClause) enumerator.Current;
                    base.Output.Write("catch (");
                    this.OutputIdentifier(current.LocalName);
                    base.Output.Write(" : ");
                    this.OutputType(current.CatchExceptionType);
                    base.Output.Write(")");
                    this.OutputStartingBrace();
                    base.Indent++;
                    base.GenerateStatements(current.Statements);
                    base.Indent--;
                }
            }
            CodeStatementCollection finallyStatements = e.FinallyStatements;
            if (finallyStatements.Count > 0)
            {
                base.Output.Write("}");
                if (base.Options.ElseOnClosing)
                {
                    base.Output.Write(" ");
                }
                else
                {
                    base.Output.WriteLine("");
                }
                base.Output.Write("finally");
                this.OutputStartingBrace();
                base.Indent++;
                base.GenerateStatements(finallyStatements);
                base.Indent--;
            }
            base.Output.WriteLine("}");
        }

        protected override void GenerateTypeConstructor(CodeTypeConstructor e)
        {
            if (base.IsCurrentClass || base.IsCurrentStruct)
            {
                base.Output.Write("static ");
                this.OutputIdentifier(base.CurrentTypeName);
                this.OutputStartingBrace();
                base.Indent++;
                base.GenerateStatements(e.Statements);
                base.Indent--;
                base.Output.WriteLine("}");
            }
        }

        protected override void GenerateTypeEnd(CodeTypeDeclaration e)
        {
            if (!base.IsCurrentDelegate)
            {
                base.Indent--;
                base.Output.WriteLine("}");
            }
        }

        protected override void GenerateTypeOfExpression(CodeTypeOfExpression e)
        {
            this.OutputType(e.Type);
        }

        protected override void GenerateTypeStart(CodeTypeDeclaration e)
        {
            if (base.IsCurrentDelegate)
            {
                throw new Exception(JScriptException.Localize("No delegate declarations", CultureInfo.CurrentUICulture));
            }
            this.OutputTypeVisibility(e.TypeAttributes);
            if (e.CustomAttributes.Count > 0)
            {
                this.OutputAttributeDeclarations(e.CustomAttributes);
                base.Output.WriteLine("");
            }
            this.OutputTypeAttributes(e.TypeAttributes, base.IsCurrentStruct, base.IsCurrentEnum);
            this.OutputIdentifier(e.Name);
            if (base.IsCurrentEnum)
            {
                if (e.BaseTypes.Count > 1)
                {
                    throw new Exception(JScriptException.Localize("Too many base types", CultureInfo.CurrentUICulture));
                }
                if (e.BaseTypes.Count == 1)
                {
                    base.Output.Write(" : ");
                    this.OutputType(e.BaseTypes[0]);
                }
            }
            else
            {
                bool flag = true;
                bool flag2 = false;
                foreach (CodeTypeReference reference in e.BaseTypes)
                {
                    if (flag)
                    {
                        base.Output.Write(" extends ");
                        flag = false;
                        flag2 = true;
                    }
                    else if (flag2)
                    {
                        base.Output.Write(" implements ");
                        flag2 = false;
                    }
                    else
                    {
                        base.Output.Write(", ");
                    }
                    this.OutputType(reference);
                }
            }
            this.OutputStartingBrace();
            base.Indent++;
        }

        protected override void GenerateVariableDeclarationStatement(CodeVariableDeclarationStatement e)
        {
            base.Output.Write("var ");
            this.OutputTypeNamePair(e.Type, e.Name);
            if (e.InitExpression != null)
            {
                base.Output.Write(" = ");
                base.GenerateExpression(e.InitExpression);
            }
            base.Output.WriteLine(";");
        }

        protected override void GenerateVariableReferenceExpression(CodeVariableReferenceExpression e)
        {
            this.OutputIdentifier(e.VariableName);
        }

        private string GetBaseTypeOutput(string baseType)
        {
            if (baseType.Length == 0)
            {
                return "void";
            }
            if (string.Compare(baseType, "System.Byte", StringComparison.Ordinal) == 0)
            {
                return "byte";
            }
            if (string.Compare(baseType, "System.Int16", StringComparison.Ordinal) == 0)
            {
                return "short";
            }
            if (string.Compare(baseType, "System.Int32", StringComparison.Ordinal) == 0)
            {
                return "int";
            }
            if (string.Compare(baseType, "System.Int64", StringComparison.Ordinal) == 0)
            {
                return "long";
            }
            if (string.Compare(baseType, "System.SByte", StringComparison.Ordinal) == 0)
            {
                return "sbyte";
            }
            if (string.Compare(baseType, "System.UInt16", StringComparison.Ordinal) == 0)
            {
                return "ushort";
            }
            if (string.Compare(baseType, "System.UInt32", StringComparison.Ordinal) == 0)
            {
                return "uint";
            }
            if (string.Compare(baseType, "System.UInt64", StringComparison.Ordinal) == 0)
            {
                return "ulong";
            }
            if (string.Compare(baseType, "System.Decimal", StringComparison.Ordinal) == 0)
            {
                return "decimal";
            }
            if (string.Compare(baseType, "System.Single", StringComparison.Ordinal) == 0)
            {
                return "float";
            }
            if (string.Compare(baseType, "System.Double", StringComparison.Ordinal) == 0)
            {
                return "double";
            }
            if (string.Compare(baseType, "System.Boolean", StringComparison.Ordinal) == 0)
            {
                return "boolean";
            }
            if (string.Compare(baseType, "System.Char", StringComparison.Ordinal) == 0)
            {
                return "char";
            }
            baseType = baseType.Replace('+', '.');
            return this.CreateEscapedIdentifier(baseType);
        }

        protected override string GetTypeOutput(CodeTypeReference typeRef)
        {
            string typeOutput;
            if (typeRef.ArrayElementType != null)
            {
                typeOutput = this.GetTypeOutput(typeRef.ArrayElementType);
            }
            else
            {
                typeOutput = this.GetBaseTypeOutput(typeRef.BaseType);
            }
            if (typeRef.ArrayRank <= 0)
            {
                return typeOutput;
            }
            char[] chArray = new char[typeRef.ArrayRank + 1];
            chArray[0] = '[';
            chArray[typeRef.ArrayRank] = ']';
            for (int i = 1; i < typeRef.ArrayRank; i++)
            {
                chArray[i] = ',';
            }
            return (typeOutput + new string(chArray));
        }

        private bool IsKeyword(string value)
        {
            return keywords.ContainsKey(value);
        }

        private bool IsSurrogateEnd(char c)
        {
            return ((0xdc00 <= c) && (c <= 0xdfff));
        }

        private bool IsSurrogateStart(char c)
        {
            return ((0xd800 <= c) && (c <= 0xdbff));
        }

        protected override bool IsValidIdentifier(string value)
        {
            return (((value != null) && (value.Length != 0)) && VsaEngine.CreateEngine().IsValidIdentifier(value));
        }

        protected override void OutputAttributeDeclarations(CodeAttributeDeclarationCollection attributes)
        {
            if (attributes.Count != 0)
            {
                this.GenerateAttributeDeclarationsStart(attributes);
                IEnumerator enumerator = attributes.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    CodeAttributeDeclaration current = (CodeAttributeDeclaration) enumerator.Current;
                    base.Output.Write(this.GetBaseTypeOutput(current.Name));
                    base.Output.Write("(");
                    bool flag = true;
                    foreach (CodeAttributeArgument argument in current.Arguments)
                    {
                        if (flag)
                        {
                            flag = false;
                        }
                        else
                        {
                            base.Output.Write(", ");
                        }
                        this.OutputAttributeArgument(argument);
                    }
                    base.Output.Write(") ");
                }
                this.GenerateAttributeDeclarationsEnd(attributes);
            }
        }

        protected override void OutputDirection(FieldDirection dir)
        {
            switch (dir)
            {
                case FieldDirection.In:
                    break;

                case FieldDirection.Out:
                case FieldDirection.Ref:
                    if (!this.isArgumentList)
                    {
                        throw new Exception(JScriptException.Localize("No parameter direction", CultureInfo.CurrentUICulture));
                    }
                    base.Output.Write("&");
                    break;

                default:
                    return;
            }
        }

        protected override void OutputIdentifier(string ident)
        {
            base.Output.Write(this.CreateEscapedIdentifier(ident));
        }

        protected override void OutputMemberAccessModifier(MemberAttributes attributes)
        {
            switch ((attributes & MemberAttributes.AccessMask))
            {
                case MemberAttributes.Family:
                    base.Output.Write("protected ");
                    return;

                case MemberAttributes.FamilyOrAssembly:
                    base.Output.Write("protected internal ");
                    return;

                case MemberAttributes.Public:
                    base.Output.Write("public ");
                    return;

                case MemberAttributes.Assembly:
                    base.Output.Write("internal ");
                    return;

                case MemberAttributes.FamilyAndAssembly:
                    base.Output.Write("internal ");
                    return;
            }
            base.Output.Write("private ");
        }

        protected override void OutputMemberScopeModifier(MemberAttributes attributes)
        {
            switch ((attributes & MemberAttributes.ScopeMask))
            {
                case MemberAttributes.Abstract:
                    base.Output.Write("abstract ");
                    return;

                case MemberAttributes.Final:
                    base.Output.Write("final ");
                    return;

                case MemberAttributes.Static:
                    base.Output.Write("static ");
                    return;

                case MemberAttributes.Override:
                    base.Output.Write("override ");
                    return;
            }
        }

        private void OutputMemberVTableModifier(MemberAttributes attributes)
        {
            MemberAttributes attributes2 = attributes & MemberAttributes.VTableMask;
            if (attributes2 == MemberAttributes.New)
            {
                base.Output.Write("hide ");
            }
        }

        protected override void OutputParameters(CodeParameterDeclarationExpressionCollection parameters)
        {
            bool flag = true;
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
                    base.Output.Write(", ");
                }
                base.GenerateExpression(current);
            }
        }

        private void OutputStartingBrace()
        {
            if (base.Options.BracingStyle == "C")
            {
                base.Output.WriteLine("");
                base.Output.WriteLine("{");
            }
            else
            {
                base.Output.WriteLine(" {");
            }
        }

        protected override void OutputType(CodeTypeReference typeRef)
        {
            base.Output.Write(this.GetTypeOutput(typeRef));
        }

        protected override void OutputTypeAttributes(TypeAttributes attributes, bool isStruct, bool isEnum)
        {
            if (isEnum)
            {
                base.Output.Write("enum ");
            }
            else
            {
                TypeAttributes attributes2 = attributes & TypeAttributes.ClassSemanticsMask;
                if (attributes2 != TypeAttributes.AnsiClass)
                {
                    if (attributes2 != TypeAttributes.ClassSemanticsMask)
                    {
                        return;
                    }
                }
                else
                {
                    if ((attributes & TypeAttributes.Sealed) == TypeAttributes.Sealed)
                    {
                        base.Output.Write("final ");
                    }
                    if ((attributes & TypeAttributes.Abstract) == TypeAttributes.Abstract)
                    {
                        base.Output.Write("abstract ");
                    }
                    base.Output.Write("class ");
                    return;
                }
                base.Output.Write("interface ");
            }
        }

        protected override void OutputTypeNamePair(CodeTypeReference typeRef, string name)
        {
            this.OutputIdentifier(name);
            base.Output.Write(" : ");
            this.OutputType(typeRef);
        }

        private void OutputTypeVisibility(TypeAttributes attributes)
        {
            switch ((attributes & TypeAttributes.NestedFamORAssem))
            {
                case TypeAttributes.AnsiClass:
                    base.Output.Write("internal ");
                    return;

                case TypeAttributes.NestedPublic:
                    base.Output.Write("public static ");
                    return;

                case TypeAttributes.NestedPrivate:
                    base.Output.Write("private static ");
                    return;

                case TypeAttributes.NestedFamily:
                    base.Output.Write("protected static ");
                    return;

                case TypeAttributes.NestedAssembly:
                case TypeAttributes.NestedFamANDAssem:
                    base.Output.Write("internal static ");
                    return;

                case TypeAttributes.NestedFamORAssem:
                    base.Output.Write("protected internal static ");
                    return;
            }
            base.Output.Write("public ");
        }

        protected override void ProcessCompilerOutputLine(CompilerResults results, string line)
        {
            Match match = outputReg.Match(line);
            if (match.Success)
            {
                CompilerError error = new CompilerError();
                if (match.Groups[1].Success)
                {
                    error.FileName = match.Groups[2].Value;
                    error.Line = int.Parse(match.Groups[4].Value, CultureInfo.InvariantCulture);
                    error.Column = int.Parse(match.Groups[5].Value, CultureInfo.InvariantCulture);
                }
                if (string.Compare(match.Groups[7].Value, "warning", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    error.IsWarning = true;
                }
                error.ErrorNumber = match.Groups[8].Value;
                error.ErrorText = match.Groups[9].Value;
                results.Errors.Add(error);
            }
        }

        protected override string QuoteSnippetString(string value)
        {
            return this.QuoteSnippetStringCStyle(value);
        }

        private string QuoteSnippetStringCStyle(string value)
        {
            char[] chArray = value.ToCharArray();
            StringBuilder builder = new StringBuilder(value.Length + 5);
            builder.Append("\"");
            int num = 80;
            for (int i = 0; i < chArray.Length; i++)
            {
                switch (chArray[i])
                {
                    case '\u2028':
                        builder.Append(@"\u2028");
                        break;

                    case '\u2029':
                        builder.Append(@"\u2029");
                        break;

                    case '\\':
                        builder.Append(@"\\");
                        break;

                    case '\'':
                        builder.Append(@"\'");
                        break;

                    case '\t':
                        builder.Append(@"\t");
                        break;

                    case '\n':
                        builder.Append(@"\n");
                        break;

                    case '\r':
                        builder.Append(@"\r");
                        break;

                    case '"':
                        builder.Append("\\\"");
                        break;

                    case '\0':
                        builder.Append(@"\0");
                        break;

                    default:
                        builder.Append(chArray[i]);
                        break;
                }
                if (((i >= num) && ((i + 1) < chArray.Length)) && (!this.IsSurrogateStart(chArray[i]) || !this.IsSurrogateEnd(chArray[i + 1])))
                {
                    num = i + 80;
                    builder.Append("\" + \r\n\"");
                }
            }
            builder.Append("\"");
            return builder.ToString();
        }

        protected override bool Supports(GeneratorSupport support)
        {
            return ((support & (GeneratorSupport.PublicStaticMembers | GeneratorSupport.AssemblyAttributes | GeneratorSupport.DeclareInterfaces | GeneratorSupport.DeclareEnums | GeneratorSupport.TryCatchStatements | GeneratorSupport.StaticConstructors | GeneratorSupport.MultidimensionalArrays | GeneratorSupport.EntryPointMethod | GeneratorSupport.ArraysOfArrays)) == support);
        }

        protected override string CompilerName
        {
            get
            {
                return "jsc.exe";
            }
        }

        protected override string FileExtension
        {
            get
            {
                return ".js";
            }
        }

        protected override string NullToken
        {
            get
            {
                return "null";
            }
        }
    }
}

