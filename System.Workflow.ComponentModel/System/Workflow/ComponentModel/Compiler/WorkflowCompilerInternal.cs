namespace System.Workflow.ComponentModel.Compiler
{
    using System;
    using System.CodeDom;
    using System.CodeDom.Compiler;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel.Design;
    using System.ComponentModel.Design.Serialization;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Serialization;
    using System.Xml;

    internal sealed class WorkflowCompilerInternal : MarshalByRefObject
    {
        public WorkflowCompilerResults Compile(WorkflowCompilerParameters parameters, string[] allFiles)
        {
            WorkflowCompilerResults results = new WorkflowCompilerResults(parameters.TempFiles);
            StringCollection strings = new StringCollection();
            StringCollection strings2 = new StringCollection();
            foreach (string str in allFiles)
            {
                if (str.EndsWith(".xoml", StringComparison.OrdinalIgnoreCase))
                {
                    strings.Add(str);
                }
                else
                {
                    strings2.Add(str);
                }
            }
            string[] array = new string[strings.Count];
            strings.CopyTo(array, 0);
            string[] strArray2 = new string[strings2.Count];
            strings2.CopyTo(strArray2, 0);
            string location = typeof(object).Assembly.Location;
            ServiceContainer serviceProvider = new ServiceContainer();
            if (parameters.MultiTargetingInformation == null)
            {
                XomlCompilerHelper.FixReferencedAssemblies(parameters, results, parameters.LibraryPaths);
            }
            string fileName = Path.GetFileName(location);
            ReferencedAssemblyResolver resolver = new ReferencedAssemblyResolver(parameters.ReferencedAssemblies, parameters.LocalAssembly);
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(resolver.ResolveEventHandler);
            TypeProvider serviceInstance = new TypeProvider(new ServiceContainer());
            int index = -1;
            if ((parameters.ReferencedAssemblies != null) && (parameters.ReferencedAssemblies.Count > 0))
            {
                for (int i = 0; i < parameters.ReferencedAssemblies.Count; i++)
                {
                    string path = parameters.ReferencedAssemblies[i];
                    if ((index == -1) && (string.Compare(fileName, Path.GetFileName(path), StringComparison.OrdinalIgnoreCase) == 0))
                    {
                        index = i;
                        location = path;
                    }
                    serviceInstance.AddAssemblyReference(path);
                }
            }
            if (index != -1)
            {
                parameters.ReferencedAssemblies.RemoveAt(index);
            }
            else
            {
                serviceInstance.AddAssemblyReference(location);
            }
            serviceProvider.AddService(typeof(ITypeProvider), serviceInstance);
            TempFileCollection files = null;
            string localAssemblyPath = string.Empty;
            try
            {
                using (WorkflowCompilationContext.CreateScope(serviceProvider, parameters))
                {
                    parameters.LocalAssembly = this.GenerateLocalAssembly(array, strArray2, parameters, results, out files, out localAssemblyPath);
                    if (parameters.LocalAssembly != null)
                    {
                        resolver.SetLocalAssembly(parameters.LocalAssembly);
                        serviceInstance.SetLocalAssembly(parameters.LocalAssembly);
                        serviceInstance.AddAssembly(parameters.LocalAssembly);
                        results.Errors.Clear();
                        XomlCompilerHelper.InternalCompileFromDomBatch(array, strArray2, parameters, results, localAssemblyPath);
                    }
                }
                return results;
            }
            catch (Exception exception)
            {
                int num4 = 0x15c;
                results.Errors.Add(new WorkflowCompilerError(string.Empty, -1, -1, num4.ToString(CultureInfo.InvariantCulture), SR.GetString("Error_CompilationFailed", new object[] { exception.Message })));
            }
            finally
            {
                if ((files != null) && !parameters.TempFiles.KeepFiles)
                {
                    string directoryName = string.Empty;
                    if (File.Exists(localAssemblyPath))
                    {
                        directoryName = Path.GetDirectoryName(localAssemblyPath);
                    }
                    foreach (string str7 in files)
                    {
                        try
                        {
                            File.Delete(str7);
                        }
                        catch
                        {
                        }
                    }
                    try
                    {
                        if (!string.IsNullOrEmpty(directoryName))
                        {
                            Directory.Delete(directoryName);
                        }
                    }
                    catch
                    {
                    }
                }
            }
            return results;
        }

        internal static CodeCompileUnit GenerateCodeFromFileBatch(string[] files, WorkflowCompilerParameters parameters, WorkflowCompilerResults results)
        {
            WorkflowCompilationContext current = WorkflowCompilationContext.Current;
            if (current == null)
            {
                throw new Exception(SR.GetString("Error_MissingCompilationContext"));
            }
            CodeCompileUnit unit = new CodeCompileUnit();
            foreach (string str in files)
            {
                Activity rootActivity = null;
                try
                {
                    DesignerSerializationManager manager = new DesignerSerializationManager(current.ServiceProvider);
                    using (manager.CreateSession())
                    {
                        WorkflowMarkupSerializationManager xomlSerializationManager = new WorkflowMarkupSerializationManager(manager);
                        xomlSerializationManager.WorkflowMarkupStack.Push(parameters);
                        xomlSerializationManager.LocalAssembly = parameters.LocalAssembly;
                        using (XmlReader reader = XmlReader.Create(str))
                        {
                            rootActivity = WorkflowMarkupSerializationHelpers.LoadXomlDocument(xomlSerializationManager, reader, str);
                        }
                        if (parameters.LocalAssembly != null)
                        {
                            foreach (object obj2 in manager.Errors)
                            {
                                if (obj2 is WorkflowMarkupSerializationException)
                                {
                                    results.Errors.Add(new WorkflowCompilerError(str, (WorkflowMarkupSerializationException) obj2));
                                }
                                else
                                {
                                    int num2 = 0x15b;
                                    results.Errors.Add(new WorkflowCompilerError(str, -1, -1, num2.ToString(CultureInfo.InvariantCulture), obj2.ToString()));
                                }
                            }
                        }
                    }
                }
                catch (WorkflowMarkupSerializationException exception)
                {
                    results.Errors.Add(new WorkflowCompilerError(str, exception));
                    continue;
                }
                catch (Exception exception2)
                {
                    int num3 = 0x15b;
                    results.Errors.Add(new WorkflowCompilerError(str, -1, -1, num3.ToString(CultureInfo.InvariantCulture), SR.GetString("Error_CompilationFailed", new object[] { exception2.Message })));
                    continue;
                }
                if (rootActivity == null)
                {
                    int num4 = 0x15b;
                    results.Errors.Add(new WorkflowCompilerError(str, 1, 1, num4.ToString(CultureInfo.InvariantCulture), SR.GetString("Error_RootActivityTypeInvalid")));
                }
                else if (string.IsNullOrEmpty(rootActivity.GetValue(WorkflowMarkupSerializer.XClassProperty) as string))
                {
                    int num5 = 0x15b;
                    results.Errors.Add(new WorkflowCompilerError(str, 1, 1, num5.ToString(CultureInfo.InvariantCulture), SR.GetString("Error_CannotCompile_No_XClass")));
                }
                else
                {
                    if (parameters.CompileWithNoCode && XomlCompilerHelper.HasCodeWithin(rootActivity))
                    {
                        ValidationError error = new ValidationError(SR.GetString("Error_CodeWithinNotAllowed"), 0x16a);
                        error.UserData[typeof(Activity)] = rootActivity;
                        results.Errors.Add(XomlCompilerHelper.CreateXomlCompilerError(error, parameters));
                    }
                    ValidationErrorCollection errors = new ValidationErrorCollection();
                    foreach (ValidationError error2 in ValidateIdentifiers(current.ServiceProvider, rootActivity))
                    {
                        results.Errors.Add(XomlCompilerHelper.CreateXomlCompilerError(error2, parameters));
                    }
                    if (!results.Errors.HasErrors)
                    {
                        unit.Namespaces.AddRange(WorkflowMarkupSerializationHelpers.GenerateCodeFromXomlDocument(rootActivity, str, current.RootNamespace, CompilerHelpers.GetSupportedLanguage(current.Language), current.ServiceProvider));
                    }
                }
            }
            WorkflowMarkupSerializationHelpers.FixStandardNamespacesAndRootNamespace(unit.Namespaces, current.RootNamespace, CompilerHelpers.GetSupportedLanguage(current.Language));
            return unit;
        }

        private Assembly GenerateLocalAssembly(string[] files, string[] codeFiles, WorkflowCompilerParameters parameters, WorkflowCompilerResults results, out TempFileCollection tempFiles2, out string localAssemblyPath)
        {
            localAssemblyPath = string.Empty;
            tempFiles2 = null;
            CodeCompileUnit unit = GenerateCodeFromFileBatch(files, parameters, results);
            if (results.Errors.HasErrors)
            {
                return null;
            }
            SupportedLanguages supportedLanguage = CompilerHelpers.GetSupportedLanguage(parameters.LanguageToUse);
            CodeDomProvider codeDomProvider = CompilerHelpers.GetCodeDomProvider(supportedLanguage, parameters.CompilerVersion);
            CompilerParameters parameters2 = XomlCompilerHelper.CloneCompilerParameters(parameters);
            parameters2.TempFiles.KeepFiles = true;
            tempFiles2 = parameters2.TempFiles;
            parameters2.GenerateInMemory = true;
            if (string.IsNullOrEmpty(parameters.OutputAssembly))
            {
                localAssemblyPath = parameters2.OutputAssembly = parameters2.TempFiles.AddExtension("dll");
                goto Label_00FE;
            }
            string basePath = parameters2.TempFiles.BasePath;
            int num = 0;
        Label_009F:
            try
            {
                Directory.CreateDirectory(basePath);
            }
            catch
            {
                basePath = parameters2.TempFiles.BasePath + num++;
                goto Label_009F;
            }
            localAssemblyPath = parameters2.OutputAssembly = basePath + @"\" + Path.GetFileName(parameters2.OutputAssembly);
            parameters2.TempFiles.AddFile(localAssemblyPath, true);
        Label_00FE:
            parameters2.TreatWarningsAsErrors = false;
            if ((parameters2.CompilerOptions != null) && (parameters2.CompilerOptions.Length > 0))
            {
                string compilerOptions = parameters2.CompilerOptions;
                ArrayList list = new ArrayList();
                int startIndex = 0;
                int num3 = 0;
                bool flag = false;
                while (num3 < compilerOptions.Length)
                {
                    if (compilerOptions[num3] == '"')
                    {
                        flag = !flag;
                    }
                    else if ((compilerOptions[num3] == ' ') && !flag)
                    {
                        if (startIndex == num3)
                        {
                            startIndex++;
                        }
                        else
                        {
                            string str3 = compilerOptions.Substring(startIndex, num3 - startIndex);
                            list.Add(str3);
                            startIndex = num3 + 1;
                        }
                    }
                    num3++;
                }
                if (startIndex != num3)
                {
                    string str4 = compilerOptions.Substring(startIndex, num3 - startIndex);
                    list.Add(str4);
                }
                string[] strArray = list.ToArray(typeof(string)) as string[];
                parameters2.CompilerOptions = string.Empty;
                foreach (string str5 in strArray)
                {
                    if (((str5.Length > 0) && !str5.StartsWith("/delaysign", StringComparison.OrdinalIgnoreCase)) && (!str5.StartsWith("/keyfile", StringComparison.OrdinalIgnoreCase) && !str5.StartsWith("/keycontainer", StringComparison.OrdinalIgnoreCase)))
                    {
                        parameters2.CompilerOptions = parameters2.CompilerOptions + " " + str5;
                    }
                }
            }
            parameters2.CompilerOptions = (parameters2.CompilerOptions == null) ? "/optimize-" : (parameters2.CompilerOptions + " /optimize-");
            parameters2.IncludeDebugInformation = true;
            if (supportedLanguage == SupportedLanguages.CSharp)
            {
                parameters2.CompilerOptions = parameters2.CompilerOptions + " /unsafe";
            }
            ArrayList list2 = new ArrayList((ICollection) parameters.UserCodeCompileUnits);
            list2.Add(unit);
            ArrayList list3 = new ArrayList();
            list3.AddRange(codeFiles);
            list3.AddRange(XomlCompilerHelper.GenerateFiles(codeDomProvider, parameters2, (CodeCompileUnit[]) list2.ToArray(typeof(CodeCompileUnit))));
            CompilerResults results2 = codeDomProvider.CompileAssemblyFromFile(parameters2, (string[]) list3.ToArray(typeof(string)));
            if (results2.Errors.HasErrors)
            {
                results.AddCompilerErrorsFromCompilerResults(results2);
                return null;
            }
            return results2.CompiledAssembly;
        }

        public override object InitializeLifetimeService()
        {
            return null;
        }

        private static ValidationErrorCollection ValidateIdentifiers(IServiceProvider serviceProvider, Activity activity)
        {
            ValidationErrorCollection validationErrors = new ValidationErrorCollection();
            Dictionary<string, int> names = new Dictionary<string, int>();
            Walker walker = new Walker();
            walker.FoundActivity += delegate (Walker walker2, WalkerEventArgs e) {
                Activity currentActivity = e.CurrentActivity;
                if (!currentActivity.Enabled)
                {
                    e.Action = WalkerAction.Skip;
                }
                else
                {
                    ValidationError item = null;
                    if (names.ContainsKey(currentActivity.QualifiedName))
                    {
                        if (names[currentActivity.QualifiedName] != 1)
                        {
                            item = new ValidationError(SR.GetString("Error_DuplicatedActivityID", new object[] { currentActivity.QualifiedName }), 0x602, false, "Name");
                            item.UserData[typeof(Activity)] = currentActivity;
                            validationErrors.Add(item);
                            names[currentActivity.QualifiedName] = 1;
                        }
                    }
                    else if (!string.IsNullOrEmpty(currentActivity.Name))
                    {
                        names[currentActivity.Name] = 0;
                        item = ValidationHelpers.ValidateIdentifier("Name", serviceProvider, currentActivity.Name);
                        if (item != null)
                        {
                            item.UserData[typeof(Activity)] = currentActivity;
                            validationErrors.Add(item);
                        }
                    }
                }
            };
            walker.Walk(activity);
            return validationErrors;
        }
    }
}

