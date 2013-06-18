namespace Microsoft.JScript
{
    using Microsoft.JScript.Vsa;
    using System;
    using System.CodeDom.Compiler;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Text;
    using System.Text.RegularExpressions;

    internal class JSInProcCompiler
    {
        private int codeItemCounter;
        private string debugCommandLine;

        private void AddAssemblyReference(IJSVsaEngine engine, string filename)
        {
            IJSVsaReferenceItem item = (IJSVsaReferenceItem) engine.Items.CreateItem(filename, JSVsaItemType.Reference, JSVsaItemFlag.None);
            item.AssemblyName = filename;
        }

        private void AddDefinition(string def, Hashtable definitions, VsaEngine engine)
        {
            string str;
            int index = def.IndexOf("=");
            object obj2 = null;
            if (index == -1)
            {
                str = def.Trim();
                obj2 = true;
            }
            else
            {
                str = def.Substring(0, index).Trim();
                string strA = def.Substring(index + 1).Trim();
                if (string.Compare(strA, "true", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    obj2 = true;
                }
                else if (string.Compare(strA, "false", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    obj2 = false;
                }
                else
                {
                    try
                    {
                        obj2 = int.Parse(strA, CultureInfo.InvariantCulture);
                    }
                    catch
                    {
                        throw new CmdLineException(CmdLineError.InvalidDefinition, str, engine.ErrorCultureInfo);
                    }
                }
            }
            if (str.Length == 0)
            {
                throw new CmdLineException(CmdLineError.MissingDefineArgument, engine.ErrorCultureInfo);
            }
            definitions[str] = obj2;
        }

        private void AddResourceFile(ResInfo resinfo, Hashtable resources, Hashtable resourceFiles, VsaEngine engine)
        {
            if (!File.Exists(resinfo.fullpath))
            {
                throw new CmdLineException(CmdLineError.ManagedResourceNotFound, resinfo.filename, engine.ErrorCultureInfo);
            }
            if (resourceFiles[resinfo.fullpath] != null)
            {
                throw new CmdLineException(CmdLineError.DuplicateResourceFile, resinfo.filename, engine.ErrorCultureInfo);
            }
            if (resources[resinfo.name] != null)
            {
                throw new CmdLineException(CmdLineError.DuplicateResourceName, resinfo.name, engine.ErrorCultureInfo);
            }
            resources[resinfo.name] = resinfo;
            resourceFiles[resinfo.fullpath] = resinfo;
        }

        private void AddSourceFile(VsaEngine engine, string filename)
        {
            string name = "$SourceFile_" + this.codeItemCounter++;
            IJSVsaCodeItem item = (IJSVsaCodeItem) engine.Items.CreateItem(name, JSVsaItemType.Code, JSVsaItemFlag.None);
            item.SetOption("codebase", filename);
            item.SourceText = this.ReadFile(filename, engine);
        }

        internal int Compile(CompilerParameters options, string partialCmdLine, string[] sourceFiles, string outputFile)
        {
            StreamWriter output = null;
            int num = 0;
            try
            {
                output = new StreamWriter(outputFile) {
                    AutoFlush = true
                };
                if (options.IncludeDebugInformation)
                {
                    this.PrintOptions(output, options);
                    this.debugCommandLine = partialCmdLine;
                }
                VsaEngine engine = null;
                try
                {
                    engine = this.CreateAndInitEngine(options, sourceFiles, outputFile, output);
                }
                catch (CmdLineException exception)
                {
                    output.WriteLine(exception.Message);
                    num = 10;
                }
                catch (Exception exception2)
                {
                    output.WriteLine("fatal error JS2999: " + exception2);
                    num = 10;
                }
                if (engine == null)
                {
                    return num;
                }
                if (options.IncludeDebugInformation)
                {
                    StringBuilder builder = new StringBuilder(this.debugCommandLine);
                    foreach (string str in sourceFiles)
                    {
                        builder.Append(" \"");
                        builder.Append(str);
                        builder.Append("\"");
                    }
                    this.debugCommandLine = builder.ToString();
                    string path = options.TempFiles.AddExtension("cmdline");
                    StreamWriter writer2 = null;
                    try
                    {
                        writer2 = new StreamWriter(path);
                        writer2.WriteLine(this.debugCommandLine);
                        writer2.Flush();
                    }
                    finally
                    {
                        if (writer2 != null)
                        {
                            writer2.Close();
                        }
                    }
                    StringBuilder builder2 = new StringBuilder();
                    builder2.Append(Environment.NewLine);
                    builder2.Append(JScriptException.Localize("CmdLine helper", CultureInfo.CurrentUICulture));
                    builder2.Append(":");
                    builder2.Append(Environment.NewLine);
                    builder2.Append("    ");
                    builder2.Append(options.TempFiles.TempDir);
                    builder2.Append("> jsc.exe @\"");
                    builder2.Append(path);
                    builder2.Append("\"");
                    builder2.Append(Environment.NewLine);
                    output.WriteLine(builder2.ToString());
                    this.PrintBanner(engine, output);
                }
                try
                {
                    if (!engine.Compile())
                    {
                        return 10;
                    }
                    return 0;
                }
                catch (JSVsaException exception3)
                {
                    if (exception3.ErrorCode == JSVsaError.AssemblyExpected)
                    {
                        if ((exception3.InnerException != null) && (exception3.InnerException is BadImageFormatException))
                        {
                            CmdLineException exception4 = new CmdLineException(CmdLineError.InvalidAssembly, exception3.Message, engine.ErrorCultureInfo);
                            output.WriteLine(exception4.Message);
                        }
                        else if ((exception3.InnerException != null) && (exception3.InnerException is FileNotFoundException))
                        {
                            CmdLineException exception5 = new CmdLineException(CmdLineError.AssemblyNotFound, exception3.Message, engine.ErrorCultureInfo);
                            output.WriteLine(exception5.Message);
                        }
                        else
                        {
                            CmdLineException exception6 = new CmdLineException(CmdLineError.InvalidAssembly, engine.ErrorCultureInfo);
                            output.WriteLine(exception6.Message);
                        }
                    }
                    else if (exception3.ErrorCode == JSVsaError.SaveCompiledStateFailed)
                    {
                        CmdLineException exception7 = new CmdLineException(CmdLineError.ErrorSavingCompiledState, exception3.Message, engine.ErrorCultureInfo);
                        output.WriteLine(exception7.Message);
                    }
                    else
                    {
                        output.WriteLine(JScriptException.Localize("INTERNAL COMPILER ERROR", engine.ErrorCultureInfo));
                        output.WriteLine(exception3);
                    }
                    num = 10;
                }
                catch (Exception exception8)
                {
                    output.WriteLine(JScriptException.Localize("INTERNAL COMPILER ERROR", engine.ErrorCultureInfo));
                    output.WriteLine(exception8);
                    num = 10;
                }
            }
            finally
            {
                if (output != null)
                {
                    output.Close();
                }
            }
            return num;
        }

        private VsaEngine CreateAndInitEngine(CompilerParameters options, string[] sourceFiles, string outputFile, TextWriter output)
        {
            VsaEngine engine = new VsaEngine(true);
            VsaSite site = new VsaSite(output);
            engine.InitVsaEngine("JSCodeGenerator://Microsoft.JScript.Vsa.VsaEngine", site);
            this.ValidateOptions(options, engine);
            engine.GenerateDebugInfo = options.IncludeDebugInformation;
            engine.SetOption("referenceLoaderAPI", "LoadFile");
            engine.SetOption("fast", true);
            engine.SetOption("print", false);
            engine.SetOption("VersionSafe", false);
            engine.SetOption("output", options.OutputAssembly);
            if (options.GenerateExecutable)
            {
                engine.SetOption("PEFileKind", PEFileKinds.ConsoleApplication);
            }
            else
            {
                engine.SetOption("PEFileKind", PEFileKinds.Dll);
            }
            site.treatWarningsAsErrors = options.TreatWarningsAsErrors;
            engine.SetOption("warnaserror", options.TreatWarningsAsErrors);
            site.warningLevel = options.WarningLevel;
            engine.SetOption("WarningLevel", options.WarningLevel);
            if ((options.Win32Resource != null) && (options.Win32Resource.Length > 0))
            {
                engine.SetOption("win32resource", options.Win32Resource);
            }
            bool flag = false;
            foreach (string str in options.ReferencedAssemblies)
            {
                if (string.Compare(Path.GetFileName(str), "mscorlib.dll", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    flag = true;
                }
                this.AddAssemblyReference(engine, str);
            }
            if (!flag)
            {
                this.AddAssemblyReference(engine, "mscorlib.dll");
            }
            StringCollection args = this.SplitCmdLineArguments(options.CompilerOptions);
            this.ParseCompilerOptions(engine, args, output, options.GenerateExecutable);
            for (int i = 0; i < sourceFiles.Length; i++)
            {
                this.AddSourceFile(engine, sourceFiles[i]);
            }
            return engine;
        }

        private void GetAllDefines(string definitionList, Hashtable defines, VsaEngine engine)
        {
            int argumentSeparatorIndex;
            int startIndex = 0;
            do
            {
                string str;
                argumentSeparatorIndex = this.GetArgumentSeparatorIndex(definitionList, startIndex);
                if (argumentSeparatorIndex == -1)
                {
                    str = definitionList.Substring(startIndex);
                }
                else
                {
                    str = definitionList.Substring(startIndex, argumentSeparatorIndex - startIndex);
                }
                this.AddDefinition(str, defines, engine);
                startIndex = argumentSeparatorIndex + 1;
            }
            while (argumentSeparatorIndex > -1);
        }

        private int GetArgumentSeparatorIndex(string argList, int startIndex)
        {
            int index = argList.IndexOf(",", startIndex);
            int num2 = argList.IndexOf(";", startIndex);
            if (index != -1)
            {
                if (num2 == -1)
                {
                    return index;
                }
                if (index < num2)
                {
                    return index;
                }
            }
            return num2;
        }

        private void ParseCompilerOptions(VsaEngine engine, StringCollection args, TextWriter output, bool generateExe)
        {
            string environmentVariable = Environment.GetEnvironmentVariable("LIB");
            bool flag = false;
            Hashtable defines = new Hashtable(10);
            Hashtable resources = new Hashtable(10);
            Hashtable resourceFiles = new Hashtable(10);
            bool flag2 = false;
            StringBuilder builder = null;
            if (this.debugCommandLine != null)
            {
                builder = new StringBuilder(this.debugCommandLine);
            }
            string str2 = (Path.DirectorySeparatorChar == '/') ? "-" : "/";
            int num = 0;
            int count = args.Count;
            while (num < count)
            {
                object obj2;
                string str4 = args[num];
                if ((str4 == null) || (str4.Length == 0))
                {
                    goto Label_09C7;
                }
                if (str4[0] == '@')
                {
                    throw new CmdLineException(CmdLineError.InvalidForCompilerOptions, "@<filename>", engine.ErrorCultureInfo);
                }
                if (('-' != str4[0]) && (('/' != str4[0]) || (Path.DirectorySeparatorChar == '/')))
                {
                    break;
                }
                string option = str4.Substring(1);
                if (option.Length > 0)
                {
                    switch (option[0])
                    {
                        case '?':
                            throw new CmdLineException(CmdLineError.InvalidForCompilerOptions, "/?", engine.ErrorCultureInfo);

                        case 'A':
                        case 'a':
                            goto Label_01DE;

                        case 'C':
                        case 'c':
                            goto Label_0223;

                        case 'D':
                        case 'd':
                            goto Label_024E;

                        case 'F':
                        case 'f':
                            goto Label_02F5;

                        case 'L':
                        case 'l':
                            goto Label_033A;

                        case 'N':
                        case 'n':
                            goto Label_0488;

                        case 'O':
                        case 'o':
                            goto Label_04DB;

                        case 'P':
                        case 'p':
                            goto Label_0506;

                        case 'R':
                        case 'r':
                            goto Label_0631;

                        case 'T':
                        case 't':
                            goto Label_06AD;

                        case 'U':
                        case 'u':
                            goto Label_07C8;

                        case 'V':
                        case 'v':
                            goto Label_07F3;

                        case 'W':
                        case 'w':
                            goto Label_0838;
                    }
                }
                goto Label_09B4;
            Label_01DE:
                obj2 = CmdLineOptionParser.IsBooleanOption(option, "autoref");
                if (obj2 == null)
                {
                    goto Label_09B4;
                }
                engine.SetOption("autoref", obj2);
                if (builder != null)
                {
                    builder.Append(str4);
                    builder.Append(" ");
                }
                goto Label_09C7;
            Label_0223:
                if (CmdLineOptionParser.IsArgumentOption(option, "codepage") == null)
                {
                    goto Label_09B4;
                }
                throw new CmdLineException(CmdLineError.InvalidForCompilerOptions, "/codepage:<id>", engine.ErrorCultureInfo);
            Label_024E:
                obj2 = CmdLineOptionParser.IsBooleanOption(option, "debug");
                if (obj2 != null)
                {
                    engine.GenerateDebugInfo = (bool) obj2;
                    if (builder != null)
                    {
                        builder.Append(str4);
                        builder.Append(" ");
                    }
                }
                else
                {
                    obj2 = CmdLineOptionParser.IsArgumentOption(option, "d", "define");
                    if (obj2 == null)
                    {
                        goto Label_09B4;
                    }
                    this.GetAllDefines((string) obj2, defines, engine);
                    if (builder != null)
                    {
                        builder.Append(str2 + "d:\"");
                        builder.Append((string) obj2);
                        builder.Append("\" ");
                    }
                }
                goto Label_09C7;
            Label_02F5:
                obj2 = CmdLineOptionParser.IsBooleanOption(option, "fast");
                if (obj2 == null)
                {
                    goto Label_09B4;
                }
                engine.SetOption("fast", obj2);
                if (builder != null)
                {
                    builder.Append(str4);
                    builder.Append(" ");
                }
                goto Label_09C7;
            Label_033A:
                obj2 = CmdLineOptionParser.IsArgumentOption(option, "lcid");
                if (obj2 != null)
                {
                    if (((string) obj2).Length == 0)
                    {
                        throw new CmdLineException(CmdLineError.NoLocaleID, str4, engine.ErrorCultureInfo);
                    }
                    try
                    {
                        engine.LCID = int.Parse((string) obj2, CultureInfo.InvariantCulture);
                        goto Label_09C7;
                    }
                    catch
                    {
                        throw new CmdLineException(CmdLineError.InvalidLocaleID, (string) obj2, engine.ErrorCultureInfo);
                    }
                }
                obj2 = CmdLineOptionParser.IsArgumentOption(option, "lib");
                if (obj2 != null)
                {
                    string str5 = (string) obj2;
                    if (str5.Length == 0)
                    {
                        throw new CmdLineException(CmdLineError.MissingLibArgument, engine.ErrorCultureInfo);
                    }
                    environmentVariable = str5.Replace(',', Path.PathSeparator) + Path.PathSeparator + environmentVariable;
                    if (builder != null)
                    {
                        builder.Append(str2 + "lib:\"");
                        builder.Append((string) obj2);
                        builder.Append("\" ");
                    }
                    goto Label_09C7;
                }
                obj2 = CmdLineOptionParser.IsArgumentOption(option, "linkres", "linkresource");
                if (obj2 == null)
                {
                    goto Label_09B4;
                }
                try
                {
                    ResInfo resinfo = new ResInfo((string) obj2, true);
                    this.AddResourceFile(resinfo, resources, resourceFiles, engine);
                    goto Label_09C7;
                }
                catch (CmdLineException)
                {
                    throw;
                }
                catch
                {
                    throw new CmdLineException(CmdLineError.ManagedResourceNotFound, engine.ErrorCultureInfo);
                }
            Label_0488:
                if (CmdLineOptionParser.IsBooleanOption(option, "nologo") != null)
                {
                    throw new CmdLineException(CmdLineError.InvalidForCompilerOptions, "/nologo[+|-]", engine.ErrorCultureInfo);
                }
                if (CmdLineOptionParser.IsBooleanOption(option, "nostdlib") == null)
                {
                    goto Label_09B4;
                }
                throw new CmdLineException(CmdLineError.InvalidForCompilerOptions, "/nostdlib[+|-]", engine.ErrorCultureInfo);
            Label_04DB:
                if (CmdLineOptionParser.IsArgumentOption(option, "out") == null)
                {
                    goto Label_09B4;
                }
                throw new CmdLineException(CmdLineError.InvalidForCompilerOptions, "/out:<filename>", engine.ErrorCultureInfo);
            Label_0506:
                obj2 = CmdLineOptionParser.IsBooleanOption(option, "print");
                if (obj2 != null)
                {
                    engine.SetOption("print", obj2);
                    if (builder != null)
                    {
                        builder.Append(str4);
                        builder.Append(" ");
                    }
                }
                else
                {
                    PortableExecutableKinds iLOnly;
                    ImageFileMachine machine;
                    obj2 = CmdLineOptionParser.IsArgumentOption(option, "platform");
                    if (obj2 == null)
                    {
                        goto Label_09B4;
                    }
                    string strA = (string) obj2;
                    if (string.Compare(strA, "x86", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        iLOnly = PortableExecutableKinds.Required32Bit | PortableExecutableKinds.ILOnly;
                        machine = ImageFileMachine.I386;
                    }
                    else if (string.Compare(strA, "Itanium", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        iLOnly = PortableExecutableKinds.PE32Plus | PortableExecutableKinds.ILOnly;
                        machine = ImageFileMachine.IA64;
                    }
                    else if (string.Compare(strA, "x64", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        iLOnly = PortableExecutableKinds.PE32Plus | PortableExecutableKinds.ILOnly;
                        machine = ImageFileMachine.AMD64;
                    }
                    else
                    {
                        if (string.Compare(strA, "anycpu", StringComparison.OrdinalIgnoreCase) != 0)
                        {
                            throw new CmdLineException(CmdLineError.InvalidPlatform, (string) obj2, engine.ErrorCultureInfo);
                        }
                        iLOnly = PortableExecutableKinds.ILOnly;
                        machine = ImageFileMachine.I386;
                    }
                    engine.SetOption("PortableExecutableKind", iLOnly);
                    engine.SetOption("ImageFileMachine", machine);
                    if (builder != null)
                    {
                        builder.Append(str4);
                        builder.Append(" ");
                    }
                }
                goto Label_09C7;
            Label_0631:
                if (CmdLineOptionParser.IsArgumentOption(option, "r", "reference") != null)
                {
                    throw new CmdLineException(CmdLineError.InvalidForCompilerOptions, "/r[eference]:<file list>", engine.ErrorCultureInfo);
                }
                obj2 = CmdLineOptionParser.IsArgumentOption(option, "res", "resource");
                if (obj2 == null)
                {
                    goto Label_09B4;
                }
                try
                {
                    ResInfo info2 = new ResInfo((string) obj2, false);
                    this.AddResourceFile(info2, resources, resourceFiles, engine);
                    goto Label_09C7;
                }
                catch (CmdLineException)
                {
                    throw;
                }
                catch
                {
                    throw new CmdLineException(CmdLineError.ManagedResourceNotFound, engine.ErrorCultureInfo);
                }
            Label_06AD:
                obj2 = CmdLineOptionParser.IsArgumentOption(option, "t", "target");
                if (obj2 == null)
                {
                    goto Label_09B4;
                }
                if (string.Compare((string) obj2, "exe", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    if (!generateExe)
                    {
                        throw new CmdLineException(CmdLineError.IncompatibleTargets, str4, engine.ErrorCultureInfo);
                    }
                    if (flag2)
                    {
                        throw new CmdLineException(CmdLineError.MultipleTargets, engine.ErrorCultureInfo);
                    }
                    flag2 = true;
                }
                else if (string.Compare((string) obj2, "winexe", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    if (!generateExe)
                    {
                        throw new CmdLineException(CmdLineError.IncompatibleTargets, str4, engine.ErrorCultureInfo);
                    }
                    if (flag2)
                    {
                        throw new CmdLineException(CmdLineError.MultipleTargets, engine.ErrorCultureInfo);
                    }
                    engine.SetOption("PEFileKind", PEFileKinds.WindowApplication);
                    flag = true;
                    flag2 = true;
                }
                else
                {
                    if (string.Compare((string) obj2, "library", StringComparison.OrdinalIgnoreCase) != 0)
                    {
                        throw new CmdLineException(CmdLineError.InvalidTarget, (string) obj2, engine.ErrorCultureInfo);
                    }
                    if (generateExe)
                    {
                        throw new CmdLineException(CmdLineError.IncompatibleTargets, engine.ErrorCultureInfo);
                    }
                    if (flag2)
                    {
                        throw new CmdLineException(CmdLineError.MultipleTargets, engine.ErrorCultureInfo);
                    }
                    flag2 = true;
                }
                goto Label_09C7;
            Label_07C8:
                if (CmdLineOptionParser.IsArgumentOption(option, "utf8output") == null)
                {
                    goto Label_09B4;
                }
                throw new CmdLineException(CmdLineError.InvalidForCompilerOptions, "/utf8output[+|-]", engine.ErrorCultureInfo);
            Label_07F3:
                obj2 = CmdLineOptionParser.IsBooleanOption(option, "VersionSafe");
                if (obj2 == null)
                {
                    goto Label_09B4;
                }
                engine.SetOption("VersionSafe", obj2);
                if (builder != null)
                {
                    builder.Append(str4);
                    builder.Append(" ");
                }
                goto Label_09C7;
            Label_0838:
                obj2 = CmdLineOptionParser.IsArgumentOption(option, "w", "warn");
                if (obj2 != null)
                {
                    if (((string) obj2).Length == 0)
                    {
                        throw new CmdLineException(CmdLineError.NoWarningLevel, str4, engine.ErrorCultureInfo);
                    }
                    if (((string) obj2).Length == 1)
                    {
                        if (builder != null)
                        {
                            builder.Append(str4);
                            builder.Append(" ");
                        }
                        switch (((string) obj2)[0])
                        {
                            case '0':
                                engine.SetOption("WarningLevel", 0);
                                goto Label_09C7;

                            case '1':
                                engine.SetOption("WarningLevel", 1);
                                goto Label_09C7;

                            case '2':
                                engine.SetOption("WarningLevel", 2);
                                goto Label_09C7;

                            case '3':
                                engine.SetOption("WarningLevel", 3);
                                goto Label_09C7;

                            case '4':
                                engine.SetOption("WarningLevel", 4);
                                goto Label_09C7;
                        }
                    }
                    throw new CmdLineException(CmdLineError.InvalidWarningLevel, str4, engine.ErrorCultureInfo);
                }
                obj2 = CmdLineOptionParser.IsBooleanOption(option, "warnaserror");
                if (obj2 != null)
                {
                    engine.SetOption("warnaserror", obj2);
                    if (builder != null)
                    {
                        builder.Append(str4);
                        builder.Append(" ");
                    }
                    goto Label_09C7;
                }
                if (CmdLineOptionParser.IsArgumentOption(option, "win32res") != null)
                {
                    throw new CmdLineException(CmdLineError.InvalidForCompilerOptions, "/win32res:<filename>", engine.ErrorCultureInfo);
                }
            Label_09B4:
                throw new CmdLineException(CmdLineError.UnknownOption, str4, engine.ErrorCultureInfo);
            Label_09C7:
                num++;
            }
            if (builder != null)
            {
                if (generateExe)
                {
                    if (flag)
                    {
                        builder.Append(str2 + "t:winexe ");
                    }
                    else
                    {
                        builder.Append(str2 + "t:exe ");
                    }
                }
                else
                {
                    builder.Append(str2 + "t:library ");
                }
                this.debugCommandLine = builder.ToString();
            }
            engine.SetOption("libpath", environmentVariable);
            engine.SetOption("defines", defines);
        }

        internal void PrintBanner(VsaEngine engine, TextWriter output)
        {
            string[] strArray = new string[5];
            strArray[0] = 10.ToString(CultureInfo.InvariantCulture);
            strArray[1] = ".";
            int num2 = 0;
            strArray[2] = num2.ToString(CultureInfo.InvariantCulture).PadLeft(2, '0');
            strArray[3] = ".";
            int num3 = 0x766f;
            strArray[4] = num3.ToString(CultureInfo.InvariantCulture).PadLeft(4, '0');
            string str = string.Concat(strArray);
            Version version = Environment.Version;
            string str2 = version.Major.ToString(CultureInfo.InvariantCulture) + "." + version.Minor.ToString(CultureInfo.InvariantCulture) + "." + version.Build.ToString(CultureInfo.InvariantCulture).PadLeft(4, '0');
            output.WriteLine(string.Format(engine.ErrorCultureInfo, JScriptException.Localize("Banner line 1", engine.ErrorCultureInfo), new object[] { str }));
            output.WriteLine(string.Format(engine.ErrorCultureInfo, JScriptException.Localize("Banner line 2", engine.ErrorCultureInfo), new object[] { str2 }));
            output.WriteLine(JScriptException.Localize("Banner line 3", engine.ErrorCultureInfo) + Environment.NewLine);
        }

        private void PrintOptions(TextWriter output, CompilerParameters options)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("CompilerParameters.CompilerOptions        : \"");
            builder.Append(options.CompilerOptions);
            builder.Append("\"");
            builder.Append(Environment.NewLine);
            builder.Append("CompilerParameters.GenerateExecutable     : ");
            builder.Append(options.GenerateExecutable.ToString(CultureInfo.InvariantCulture));
            builder.Append(Environment.NewLine);
            builder.Append("CompilerParameters.GenerateInMemory       : ");
            builder.Append(options.GenerateInMemory.ToString(CultureInfo.InvariantCulture));
            builder.Append(Environment.NewLine);
            builder.Append("CompilerParameters.IncludeDebugInformation: ");
            builder.Append(options.IncludeDebugInformation.ToString(CultureInfo.InvariantCulture));
            builder.Append(Environment.NewLine);
            builder.Append("CompilerParameters.MainClass              : \"");
            builder.Append(options.MainClass);
            builder.Append("\"");
            builder.Append(Environment.NewLine);
            builder.Append("CompilerParameters.OutputAssembly         : \"");
            builder.Append(options.OutputAssembly);
            builder.Append("\"");
            builder.Append(Environment.NewLine);
            builder.Append("CompilerParameters.ReferencedAssemblies   : ");
            foreach (string str in options.ReferencedAssemblies)
            {
                builder.Append(Environment.NewLine);
                builder.Append("        \"");
                builder.Append(str);
                builder.Append("\"");
            }
            builder.Append(Environment.NewLine);
            builder.Append("CompilerParameters.TreatWarningsAsErrors  : ");
            builder.Append(options.TreatWarningsAsErrors.ToString(CultureInfo.InvariantCulture));
            builder.Append(Environment.NewLine);
            builder.Append("CompilerParameters.WarningLevel           : ");
            builder.Append(options.WarningLevel.ToString(CultureInfo.InvariantCulture));
            builder.Append(Environment.NewLine);
            builder.Append("CompilerParameters.Win32Resource          : \"");
            builder.Append(options.Win32Resource);
            builder.Append("\"");
            builder.Append(Environment.NewLine);
            output.WriteLine(builder.ToString());
        }

        protected string ReadFile(string fileName, VsaEngine engine)
        {
            string str = "";
            FileStream stream = null;
            try
            {
                stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            }
            catch (ArgumentException)
            {
                throw new CmdLineException(CmdLineError.InvalidCharacters, fileName, engine.ErrorCultureInfo);
            }
            catch (FileNotFoundException)
            {
                throw new CmdLineException(CmdLineError.SourceNotFound, fileName, engine.ErrorCultureInfo);
            }
            try
            {
                if (stream.Length == 0L)
                {
                    return str;
                }
                StreamReader reader = new StreamReader(stream, true);
                try
                {
                    str = reader.ReadToEnd();
                }
                finally
                {
                    reader.Close();
                }
            }
            finally
            {
                stream.Close();
            }
            return str;
        }

        private StringCollection SplitCmdLineArguments(string argumentString)
        {
            StringCollection strings = new StringCollection();
            if ((argumentString != null) && (argumentString.Length != 0))
            {
                string pattern = "\\s*([^\\s\\\"]|(\\\"[^\\\"\\n]*\\\"))+";
                MatchCollection matchs = new Regex(pattern).Matches(argumentString);
                if ((matchs == null) || (matchs.Count == 0))
                {
                    return strings;
                }
                foreach (Match match in matchs)
                {
                    string str2 = match.ToString().Trim();
                    int startIndex = 0;
                    while ((startIndex = str2.IndexOf("\"", startIndex)) != -1)
                    {
                        if (startIndex == 0)
                        {
                            str2 = str2.Substring(1);
                        }
                        else
                        {
                            if (str2[startIndex - 1] == '\\')
                            {
                                startIndex++;
                                continue;
                            }
                            str2 = str2.Remove(startIndex, 1);
                        }
                    }
                    strings.Add(str2);
                }
            }
            return strings;
        }

        private void ValidateOptions(CompilerParameters options, VsaEngine engine)
        {
            string outputAssembly = options.OutputAssembly;
            try
            {
                if (Path.GetFileName(outputAssembly).Length == 0)
                {
                    throw new CmdLineException(CmdLineError.NoFileName, outputAssembly, engine.ErrorCultureInfo);
                }
            }
            catch (ArgumentException)
            {
                throw new CmdLineException(CmdLineError.NoFileName, engine.ErrorCultureInfo);
            }
            if (Path.GetExtension(outputAssembly).Length == 0)
            {
                throw new CmdLineException(CmdLineError.MissingExtension, outputAssembly, engine.ErrorCultureInfo);
            }
            if (options.WarningLevel == -1)
            {
                options.WarningLevel = 4;
            }
            if ((options.WarningLevel < 0) || (options.WarningLevel > 4))
            {
                throw new CmdLineException(CmdLineError.InvalidWarningLevel, options.WarningLevel.ToString(CultureInfo.InvariantCulture), engine.ErrorCultureInfo);
            }
            if (((options.Win32Resource != null) && (options.Win32Resource.Length > 0)) && !File.Exists(options.Win32Resource))
            {
                throw new CmdLineException(CmdLineError.ResourceNotFound, options.Win32Resource, engine.ErrorCultureInfo);
            }
        }
    }
}

