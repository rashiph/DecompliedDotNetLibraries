namespace System.Workflow.ComponentModel.Compiler
{
    using System;
    using System.CodeDom;
    using System.CodeDom.Compiler;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel.Design.Serialization;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Design;
    using System.Workflow.ComponentModel.Serialization;
    using System.Xml;

    internal static class XomlCompilerHelper
    {
        internal static object ColumnNumber = new object();
        internal static object LineNumber = new object();
        private static StringCollection standardAssemblies = null;
        private static char[] trimCharsArray = null;

        private static bool CheckFileNameUsingPaths(string fileName, StringCollection paths, out string fullFileName)
        {
            fullFileName = null;
            string str = fileName.Trim(new char[] { '"' });
            FileInfo info = new FileInfo(str);
            if (str.Length != info.Name.Length)
            {
                if (info.Exists)
                {
                    fullFileName = info.FullName;
                }
                return info.Exists;
            }
            using (StringEnumerator enumerator = paths.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    string str3 = enumerator.Current + Path.DirectorySeparatorChar + str;
                    FileInfo info2 = new FileInfo(str3);
                    if (info2.Exists)
                    {
                        fullFileName = str3;
                        return true;
                    }
                }
            }
            return false;
        }

        internal static bool CheckPathName(string pathName)
        {
            return Directory.Exists(pathName.Trim(new char[] { '"' }).TrimEnd(new char[] { Path.DirectorySeparatorChar }));
        }

        internal static CompilerParameters CloneCompilerParameters(WorkflowCompilerParameters sourceParams)
        {
            bool flag;
            bool flag2;
            CompilerParameters parameters = new CompilerParameters {
                CompilerOptions = ProcessCompilerOptions(sourceParams.CompilerOptions, out flag, out flag2)
            };
            foreach (string str in sourceParams.EmbeddedResources)
            {
                parameters.EmbeddedResources.Add(str);
            }
            parameters.GenerateExecutable = sourceParams.GenerateExecutable;
            parameters.GenerateInMemory = sourceParams.GenerateInMemory;
            parameters.IncludeDebugInformation = sourceParams.IncludeDebugInformation;
            foreach (string str2 in sourceParams.LinkedResources)
            {
                parameters.LinkedResources.Add(str2);
            }
            parameters.MainClass = sourceParams.MainClass;
            parameters.OutputAssembly = sourceParams.OutputAssembly;
            foreach (string str3 in sourceParams.ReferencedAssemblies)
            {
                parameters.ReferencedAssemblies.Add(str3);
            }
            parameters.TreatWarningsAsErrors = sourceParams.TreatWarningsAsErrors;
            parameters.UserToken = sourceParams.UserToken;
            parameters.WarningLevel = sourceParams.WarningLevel;
            parameters.Win32Resource = sourceParams.Win32Resource;
            return parameters;
        }

        internal static WorkflowCompilerError CreateXomlCompilerError(ValidationError error, WorkflowCompilerParameters parameters)
        {
            WorkflowCompilerError error2 = new WorkflowCompilerError(GetFileName(error), (int) GetValue(error, LineNumber), (int) GetValue(error, ColumnNumber), string.Empty, GetPrettifiedErrorText(error));
            if (!parameters.TreatWarningsAsErrors)
            {
                error2.IsWarning = error.IsWarning;
            }
            error2.ErrorNumber = "WF" + error.ErrorNumber.ToString(CultureInfo.InvariantCulture);
            if (error.UserData != null)
            {
                foreach (DictionaryEntry entry in error.UserData)
                {
                    if ((entry.Key == typeof(Activity)) && (entry.Value is Activity))
                    {
                        error2.UserData[entry.Key] = ((Activity) entry.Value).QualifiedName;
                    }
                    else
                    {
                        error2.UserData[entry.Key] = entry.Value;
                    }
                }
            }
            return error2;
        }

        private static bool ExtractCompilerOptionSwitch(ref string options, string compilerSwitch, out string compilerSwitchValue)
        {
            int index = options.IndexOf(compilerSwitch, StringComparison.OrdinalIgnoreCase);
            if (index != -1)
            {
                int startIndex = index + compilerSwitch.Length;
                int length = 0;
                while (((startIndex + length) < options.Length) && !char.IsWhiteSpace(options[startIndex + length]))
                {
                    length++;
                }
                if (length > 0)
                {
                    compilerSwitchValue = options.Substring(startIndex, length);
                }
                else
                {
                    compilerSwitchValue = string.Empty;
                }
                RemoveCompilerOptionSwitch(ref options, index, compilerSwitch.Length + length);
                return true;
            }
            compilerSwitchValue = string.Empty;
            return false;
        }

        internal static void FixReferencedAssemblies(WorkflowCompilerParameters parameters, WorkflowCompilerResults results, StringCollection libraryPaths)
        {
            foreach (string str in StandardAssemblies)
            {
                bool flag = true;
                foreach (string str2 in parameters.ReferencedAssemblies)
                {
                    if ((str2 != null) && (str2.Length > 0))
                    {
                        string fileName = Path.GetFileName(str2);
                        string strB = Path.GetFileName(str);
                        if (((fileName != null) && (strB != null)) && (string.Compare(fileName, strB, StringComparison.OrdinalIgnoreCase) == 0))
                        {
                            flag = false;
                            break;
                        }
                    }
                }
                if (flag)
                {
                    parameters.ReferencedAssemblies.Add(str);
                }
            }
            StringCollection strings = ResolveAssemblyReferences(parameters.ReferencedAssemblies, GetCompleteLibraryPaths(libraryPaths), results);
            parameters.ReferencedAssemblies.Clear();
            foreach (string str5 in strings)
            {
                if (!parameters.ReferencedAssemblies.Contains(str5))
                {
                    parameters.ReferencedAssemblies.Add(str5);
                }
            }
        }

        internal static string[] GenerateFiles(CodeDomProvider codeDomProvider, CompilerParameters parameters, CodeCompileUnit[] ccus)
        {
            CodeGeneratorOptions options = new CodeGeneratorOptions {
                BracingStyle = "C"
            };
            string[] strArray = new string[ccus.Length];
            for (int i = 0; i < ccus.Length; i++)
            {
                ResolveReferencedAssemblies(parameters, ccus[i]);
                strArray[i] = parameters.TempFiles.AddExtension(i + codeDomProvider.FileExtension);
                Stream stream = new FileStream(strArray[i], FileMode.Create, FileAccess.Write, FileShare.Read);
                try
                {
                    using (StreamWriter writer = new StreamWriter(stream, Encoding.UTF8))
                    {
                        codeDomProvider.GenerateCodeFromCompileUnit(ccus[i], writer, options);
                        writer.Flush();
                    }
                }
                finally
                {
                    stream.Close();
                }
            }
            return strArray;
        }

        private static StringCollection GetCompleteLibraryPaths(StringCollection userLibraryPaths)
        {
            StringCollection strings = new StringCollection();
            strings.Add(Environment.CurrentDirectory);
            strings.Add(TrimDirectorySeparatorChar(RuntimeEnvironment.GetRuntimeDirectory()));
            string[] array = new string[userLibraryPaths.Count];
            userLibraryPaths.CopyTo(array, 0);
            strings.AddRange(array);
            string environmentVariable = Environment.GetEnvironmentVariable("LIB");
            if ((environmentVariable != null) && (environmentVariable.Length > 0))
            {
                string[] strArray2 = Environment.GetEnvironmentVariable("LIB").Split(new char[] { ',', ';' });
                strings.AddRange(strArray2);
            }
            return strings;
        }

        private static string GetFileName(ValidationError error)
        {
            Activity parent = error.UserData[typeof(Activity)] as Activity;
            while ((parent != null) && (parent.Parent != null))
            {
                parent = parent.Parent;
            }
            string str = string.Empty;
            if (parent != null)
            {
                str = parent.GetValue(ActivityCodeDomSerializer.MarkupFileNameProperty) as string;
            }
            if (str == null)
            {
                str = string.Empty;
            }
            return str;
        }

        private static string GetPrettifiedErrorText(ValidationError error)
        {
            string errorText = error.ErrorText;
            Activity activity = error.UserData[typeof(Activity)] as Activity;
            if (activity == null)
            {
                return errorText;
            }
            string str2 = (Helpers.GetRootActivity(activity) != activity) ? activity.QualifiedName : activity.GetType().Name;
            if ((str2 == null) || (str2.Length == 0))
            {
                str2 = SR.GetString("EmptyValue");
            }
            if (error.IsWarning)
            {
                return (SR.GetString("Warning_ActivityValidation", new object[] { str2 }) + " " + errorText);
            }
            return (SR.GetString("Error_ActivityValidation", new object[] { str2 }) + " " + errorText);
        }

        private static uint GetValue(ValidationError error, object key)
        {
            Activity parent = error.UserData[typeof(Activity)] as Activity;
            while ((parent != null) && (parent.Parent != null))
            {
                parent = parent.Parent;
            }
            uint num = 0;
            if ((parent != null) && (parent.UserData[key] != null))
            {
                num = (uint) parent.UserData[key];
            }
            return num;
        }

        internal static bool HasCodeWithin(Activity rootActivity)
        {
            bool hasCodeWithin = false;
            Walker walker = new Walker();
            walker.FoundActivity += delegate (Walker walker, WalkerEventArgs e) {
                Activity currentActivity = e.CurrentActivity;
                if (!currentActivity.Enabled)
                {
                    e.Action = WalkerAction.Skip;
                }
                else
                {
                    CodeTypeMemberCollection members = currentActivity.GetValue(WorkflowMarkupSerializer.XCodeProperty) as CodeTypeMemberCollection;
                    if ((members != null) && (members.Count != 0))
                    {
                        hasCodeWithin = true;
                        e.Action = WalkerAction.Abort;
                    }
                }
            };
            walker.Walk(rootActivity);
            return hasCodeWithin;
        }

        internal static void InternalCompileFromDomBatch(string[] files, string[] codeFiles, WorkflowCompilerParameters parameters, WorkflowCompilerResults results, string localAssemblyPath)
        {
            foreach (string str in parameters.LibraryPaths)
            {
                if (!CheckPathName(str))
                {
                    int num5 = 0x160;
                    WorkflowCompilerError error = new WorkflowCompilerError(string.Empty, 0, 0, num5.ToString(CultureInfo.InvariantCulture), string.Format(CultureInfo.CurrentCulture, SR.GetString("LibraryPathIsInvalid"), new object[] { str })) {
                        IsWarning = true
                    };
                    results.Errors.Add(error);
                }
            }
            IList<AuthorizedType> authorizedTypes = null;
            if (parameters.CheckTypes)
            {
                authorizedTypes = WorkflowCompilationContext.Current.GetAuthorizedTypes();
                if (authorizedTypes == null)
                {
                    ValidationError error2 = new ValidationError(SR.GetString("Error_ConfigFileMissingOrInvalid"), 0x178);
                    results.Errors.Add(CreateXomlCompilerError(error2, parameters));
                    return;
                }
            }
            ITypeProvider service = WorkflowCompilationContext.Current.ServiceProvider.GetService(typeof(ITypeProvider)) as ITypeProvider;
            ArrayList list2 = new ArrayList();
            using (PDBReader reader = new PDBReader(localAssemblyPath))
            {
                foreach (Type type in service.LocalAssembly.GetTypes())
                {
                    if (TypeProvider.IsAssignable(typeof(Activity), type) && !type.IsAbstract)
                    {
                        string fileLocation = string.Empty;
                        WorkflowMarkupSourceAttribute[] customAttributes = (WorkflowMarkupSourceAttribute[]) type.GetCustomAttributes(typeof(WorkflowMarkupSourceAttribute), false);
                        if ((customAttributes != null) && (customAttributes.Length > 0))
                        {
                            fileLocation = customAttributes[0].FileName;
                        }
                        else
                        {
                            ConstructorInfo constructor = type.GetConstructor(Type.EmptyTypes);
                            if (constructor != null)
                            {
                                try
                                {
                                    uint line = 0;
                                    uint column = 0;
                                    reader.GetSourceLocationForOffset((uint) constructor.MetadataToken, 0, out fileLocation, out line, out column);
                                }
                                catch
                                {
                                }
                            }
                            if (string.IsNullOrEmpty(fileLocation))
                            {
                                MethodInfo info2 = type.GetMethod("InitializeComponent", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null);
                                if (info2 != null)
                                {
                                    try
                                    {
                                        uint num3 = 0;
                                        uint num4 = 0;
                                        reader.GetSourceLocationForOffset((uint) info2.MetadataToken, 0, out fileLocation, out num3, out num4);
                                        if (!string.IsNullOrEmpty(fileLocation))
                                        {
                                            if (fileLocation.EndsWith(".designer.cs", StringComparison.OrdinalIgnoreCase))
                                            {
                                                fileLocation = fileLocation.Substring(0, fileLocation.Length - ".designer.cs".Length) + ".cs";
                                            }
                                            else if (fileLocation.EndsWith(".designer.vb", StringComparison.OrdinalIgnoreCase))
                                            {
                                                fileLocation = fileLocation.Substring(0, fileLocation.Length - ".designer.vb".Length) + ".vb";
                                            }
                                        }
                                    }
                                    catch
                                    {
                                    }
                                }
                            }
                        }
                        Activity activity = null;
                        try
                        {
                            try
                            {
                                Activity.ActivityType = type;
                                activity = Activator.CreateInstance(type) as Activity;
                            }
                            finally
                            {
                                Activity.ActivityType = null;
                            }
                            activity.UserData[UserDataKeys.CustomActivity] = false;
                            if (activity is CompositeActivity)
                            {
                                CompositeActivity activity2 = activity as CompositeActivity;
                                if (activity2.CanModifyActivities)
                                {
                                    results.Errors.Add(CreateXomlCompilerError(new ValidationError(SR.GetString("Error_Missing_CanModifyProperties_False", new object[] { activity.GetType().FullName }), 0x117), parameters));
                                }
                            }
                            if (customAttributes.Length > 0)
                            {
                                DesignerSerializationManager manager = new DesignerSerializationManager(WorkflowCompilationContext.Current.ServiceProvider);
                                Activity activity3 = null;
                                using (manager.CreateSession())
                                {
                                    WorkflowMarkupSerializationManager serializationManager = new WorkflowMarkupSerializationManager(manager) {
                                        LocalAssembly = parameters.LocalAssembly
                                    };
                                    using (XmlReader reader2 = XmlReader.Create(customAttributes[0].FileName))
                                    {
                                        activity3 = new WorkflowMarkupSerializer().Deserialize(serializationManager, reader2) as Activity;
                                    }
                                }
                                if (activity3 is CompositeActivity)
                                {
                                    ActivityMarkupSerializer.ReplaceChildActivities(activity as CompositeActivity, activity3 as CompositeActivity);
                                }
                            }
                        }
                        catch (TargetInvocationException exception)
                        {
                            if ((exception.InnerException is TypeInitializationException) && (exception.InnerException.InnerException != null))
                            {
                                results.Errors.Add(CreateXomlCompilerError(new ValidationError(SR.GetString("Error_CustomActivityCantCreate", new object[] { type.FullName, exception.InnerException.InnerException.ToString() }), 0x117), parameters));
                            }
                            else if (exception.InnerException.InnerException != null)
                            {
                                results.Errors.Add(CreateXomlCompilerError(new ValidationError(exception.InnerException.InnerException.ToString(), 0x117), parameters));
                            }
                            else
                            {
                                results.Errors.Add(CreateXomlCompilerError(new ValidationError(SR.GetString("Error_CustomActivityCantCreate", new object[] { type.FullName, exception.InnerException.ToString() }), 0x117), parameters));
                            }
                            continue;
                        }
                        catch (Exception exception2)
                        {
                            results.Errors.Add(CreateXomlCompilerError(new ValidationError(SR.GetString("Error_CustomActivityCantCreate", new object[] { type.FullName, exception2.ToString() }), 0x117), parameters));
                            continue;
                        }
                        activity.SetValue(ActivityCodeDomSerializer.MarkupFileNameProperty, fileLocation);
                        activity.SetValue(WorkflowMarkupSerializer.XClassProperty, type.FullName);
                        ValidateActivity(activity, parameters, results);
                        list2.Add(activity);
                    }
                }
            }
            foreach (KeyValuePair<object, Exception> pair in service.TypeLoadErrors)
            {
                int num7 = 0x161;
                WorkflowCompilerError error3 = new WorkflowCompilerError(string.Empty, 0, 0, num7.ToString(CultureInfo.InvariantCulture), pair.Value.Message) {
                    IsWarning = true
                };
                results.Errors.Add(error3);
            }
            results.CompiledUnit = WorkflowCompilerInternal.GenerateCodeFromFileBatch(files, parameters, results);
            WorkflowCompilationContext current = WorkflowCompilationContext.Current;
            if (current == null)
            {
                throw new Exception(SR.GetString("Error_MissingCompilationContext"));
            }
            WorkflowMarkupSerializationHelpers.ReapplyRootNamespace(results.CompiledUnit.Namespaces, current.RootNamespace, CompilerHelpers.GetSupportedLanguage(current.Language));
            WorkflowMarkupSerializationHelpers.FixStandardNamespacesAndRootNamespace(results.CompiledUnit.Namespaces, current.RootNamespace, CompilerHelpers.GetSupportedLanguage(current.Language));
            if (!results.Errors.HasErrors)
            {
                CodeGenerationManager manager3 = new CodeGenerationManager(WorkflowCompilationContext.Current.ServiceProvider);
                manager3.Context.Push(results.CompiledUnit.Namespaces);
                foreach (Activity activity4 in list2)
                {
                    if (activity4.Parent == null)
                    {
                        foreach (ActivityCodeGenerator generator in manager3.GetCodeGenerators(activity4.GetType()))
                        {
                            generator.GenerateCode(manager3, activity4);
                        }
                    }
                }
                if (!parameters.GenerateCodeCompileUnitOnly || parameters.CheckTypes)
                {
                    CodeDomProvider codeDomProvider = CompilerHelpers.GetCodeDomProvider(CompilerHelpers.GetSupportedLanguage(parameters.LanguageToUse), parameters.CompilerVersion);
                    ArrayList list3 = new ArrayList((ICollection) parameters.UserCodeCompileUnits);
                    list3.Add(results.CompiledUnit);
                    ArrayList list4 = new ArrayList();
                    list4.AddRange(codeFiles);
                    list4.AddRange(GenerateFiles(codeDomProvider, parameters, (CodeCompileUnit[]) list3.ToArray(typeof(CodeCompileUnit))));
                    CompilerResults results2 = codeDomProvider.CompileAssemblyFromFile(parameters, (string[]) list4.ToArray(typeof(string)));
                    results.AddCompilerErrorsFromCompilerResults(results2);
                    results.PathToAssembly = results2.PathToAssembly;
                    results.NativeCompilerReturnValue = results2.NativeCompilerReturnValue;
                    if (!results.Errors.HasErrors && parameters.CheckTypes)
                    {
                        foreach (string str3 in MetaDataReader.GetTypeRefNames(results2.CompiledAssembly.Location))
                        {
                            bool flag = false;
                            foreach (AuthorizedType type2 in authorizedTypes)
                            {
                                if (type2.RegularExpression.IsMatch(str3))
                                {
                                    flag = string.Compare(bool.TrueString, type2.Authorized, StringComparison.OrdinalIgnoreCase) == 0;
                                    if (!flag)
                                    {
                                        break;
                                    }
                                }
                            }
                            if (!flag)
                            {
                                ValidationError error4 = new ValidationError(SR.GetString("Error_TypeNotAuthorized", new object[] { str3 }), 0x16b);
                                results.Errors.Add(CreateXomlCompilerError(error4, parameters));
                            }
                        }
                    }
                    if (((!results.Errors.HasErrors && !parameters.GenerateCodeCompileUnitOnly) && parameters.GenerateInMemory) && (string.IsNullOrEmpty(parameters.CompilerOptions) || !parameters.CompilerOptions.ToLower(CultureInfo.InvariantCulture).Contains("/delaysign")))
                    {
                        results.CompiledAssembly = results2.CompiledAssembly;
                    }
                }
            }
        }

        internal static ValidationErrorCollection MorphIntoFriendlyValidationErrors(IEnumerable<ValidationError> errors)
        {
            ValidationErrorCollection errors2 = new ValidationErrorCollection();
            foreach (ValidationError error in errors)
            {
                if (error != null)
                {
                    if (error.GetType() == typeof(ValidationError))
                    {
                        ValidationError item = new ValidationError(GetPrettifiedErrorText(error), error.ErrorNumber, error.IsWarning);
                        errors2.Add(item);
                    }
                    else
                    {
                        errors2.Add(error);
                    }
                }
            }
            return errors2;
        }

        internal static string ProcessCompilerOptions(string options, out bool noCode, out bool checkTypes)
        {
            string str;
            if (string.IsNullOrEmpty(options))
            {
                noCode = false;
                checkTypes = false;
                return options;
            }
            noCode = ExtractCompilerOptionSwitch(ref options, "/nocode", out str);
            checkTypes = ExtractCompilerOptionSwitch(ref options, "/checktypes", out str);
            return options;
        }

        private static void RemoveCompilerOptionSwitch(ref string options, int startPos, int length)
        {
            if ((startPos > 0) && char.IsWhiteSpace(options[startPos - 1]))
            {
                options = options.Remove(startPos - 1, length + 1);
            }
            else if (((startPos == 0) && (((startPos + length) + 1) < options.Length)) && char.IsWhiteSpace(options[(startPos + length) + 1]))
            {
                options = options.Remove(startPos, length + 1);
            }
            else
            {
                options = options.Remove(startPos, length);
            }
        }

        private static StringCollection ResolveAssemblyReferences(StringCollection originalReferences, StringCollection libraryPaths, WorkflowCompilerResults results)
        {
            StringCollection strings = new StringCollection();
            foreach (string str in originalReferences)
            {
                string str2;
                if (CheckFileNameUsingPaths(str, libraryPaths, out str2))
                {
                    strings.Add(str2);
                }
                else
                {
                    int num = 0x162;
                    WorkflowCompilerError error = new WorkflowCompilerError(string.Empty, 0, 0, num.ToString(CultureInfo.InvariantCulture), SR.GetString("Error_ReferencedAssemblyIsInvalid", new object[] { str }));
                    results.Errors.Add(error);
                }
            }
            return strings;
        }

        private static void ResolveReferencedAssemblies(CompilerParameters parameters, CodeCompileUnit cu)
        {
            if (cu.ReferencedAssemblies.Count > 0)
            {
                foreach (string str in cu.ReferencedAssemblies)
                {
                    if (!parameters.ReferencedAssemblies.Contains(str))
                    {
                        parameters.ReferencedAssemblies.Add(str);
                    }
                }
            }
        }

        internal static string TrimDirectorySeparatorChar(string dir)
        {
            if (trimCharsArray == null)
            {
                trimCharsArray = new char[] { Path.DirectorySeparatorChar };
            }
            return dir.TrimEnd(trimCharsArray);
        }

        internal static void ValidateActivity(Activity activity, WorkflowCompilerParameters parameters, WorkflowCompilerResults results)
        {
            ValidationManager manager = new ValidationManager(WorkflowCompilationContext.Current.ServiceProvider);
            foreach (Validator validator in manager.GetValidators(activity.GetType()))
            {
                try
                {
                    foreach (ValidationError error in validator.Validate(manager, activity))
                    {
                        if (!error.UserData.Contains(typeof(Activity)))
                        {
                            error.UserData[typeof(Activity)] = activity;
                        }
                        results.Errors.Add(CreateXomlCompilerError(error, parameters));
                    }
                }
                catch (TargetInvocationException exception)
                {
                    Exception exception2 = exception.InnerException ?? exception;
                    ValidationError error2 = new ValidationError(SR.GetString("Error_ValidatorThrewException", new object[] { exception2.GetType().FullName, validator.GetType().FullName, activity.Name, exception2.ToString() }), 0x627);
                    results.Errors.Add(CreateXomlCompilerError(error2, parameters));
                }
                catch (Exception exception3)
                {
                    ValidationError error3 = new ValidationError(SR.GetString("Error_ValidatorThrewException", new object[] { exception3.GetType().FullName, validator.GetType().FullName, activity.Name, exception3.ToString() }), 0x627);
                    results.Errors.Add(CreateXomlCompilerError(error3, parameters));
                }
            }
        }

        private static StringCollection StandardAssemblies
        {
            get
            {
                if (standardAssemblies == null)
                {
                    StringCollection strings = new StringCollection();
                    strings.Add("System.Workflow.ComponentModel.dll");
                    strings.Add("System.Workflow.Runtime.dll");
                    strings.Add("System.Workflow.Activities.dll");
                    strings.Add("System.dll");
                    strings.Add("System.Transactions.dll");
                    strings.Add("System.drawing.dll");
                    strings.Add("System.Web.dll");
                    strings.Add("System.Web.Services.dll");
                    standardAssemblies = strings;
                }
                return standardAssemblies;
            }
        }
    }
}

