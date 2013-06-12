namespace System.Xml.Serialization
{
    using Microsoft.CSharp;
    using System;
    using System.CodeDom.Compiler;
    using System.Collections;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Security.Policy;
    using System.Security.Principal;
    using System.Threading;
    using System.Xml;

    internal class Compiler
    {
        private bool debugEnabled = DiagnosticsSwitches.KeepTempFiles.Enabled;
        private Hashtable imports = new Hashtable();
        private StringWriter writer = new StringWriter(CultureInfo.InvariantCulture);

        internal void AddImport(Assembly assembly)
        {
            TempAssembly.FileIOPermission.Assert();
            this.imports[assembly] = assembly.Location;
        }

        internal void AddImport(Type type, Hashtable types)
        {
            if (((type != null) && !TypeScope.IsKnownType(type)) && (types[type] == null))
            {
                types[type] = type;
                Type baseType = type.BaseType;
                if (baseType != null)
                {
                    this.AddImport(baseType, types);
                }
                Type declaringType = type.DeclaringType;
                if (declaringType != null)
                {
                    this.AddImport(declaringType, types);
                }
                foreach (Type type4 in type.GetInterfaces())
                {
                    this.AddImport(type4, types);
                }
                ConstructorInfo[] constructors = type.GetConstructors();
                for (int i = 0; i < constructors.Length; i++)
                {
                    ParameterInfo[] parameters = constructors[i].GetParameters();
                    for (int j = 0; j < parameters.Length; j++)
                    {
                        this.AddImport(parameters[j].ParameterType, types);
                    }
                }
                if (type.IsGenericType)
                {
                    Type[] genericArguments = type.GetGenericArguments();
                    for (int k = 0; k < genericArguments.Length; k++)
                    {
                        this.AddImport(genericArguments[k], types);
                    }
                }
                TempAssembly.FileIOPermission.Assert();
                Assembly a = type.Module.Assembly;
                if (DynamicAssemblies.IsTypeDynamic(type))
                {
                    DynamicAssemblies.Add(a);
                }
                else
                {
                    object[] customAttributes = type.GetCustomAttributes(typeof(TypeForwardedFromAttribute), false);
                    if (customAttributes.Length > 0)
                    {
                        TypeForwardedFromAttribute attribute = customAttributes[0] as TypeForwardedFromAttribute;
                        Assembly assembly2 = Assembly.Load(attribute.AssemblyFullName);
                        this.imports[assembly2] = assembly2.Location;
                    }
                    this.imports[a] = a.Location;
                }
            }
        }

        private static string AssemblyNameFromOptions(string options)
        {
            if ((options == null) || (options.Length == 0))
            {
                return null;
            }
            string str = null;
            string[] strArray = options.ToLower(CultureInfo.InvariantCulture).Split(null);
            for (int i = 0; i < strArray.Length; i++)
            {
                string str2 = strArray[i].Trim();
                if (str2.StartsWith("/out:", StringComparison.Ordinal))
                {
                    str = str2.Substring(5);
                }
            }
            return str;
        }

        internal void Close()
        {
        }

        internal Assembly Compile(Assembly parent, string ns, XmlSerializerCompilerParameters xmlParameters, Evidence evidence)
        {
            CodeDomProvider provider = new CSharpCodeProvider();
            CompilerParameters codeDomParameters = xmlParameters.CodeDomParameters;
            codeDomParameters.ReferencedAssemblies.AddRange(this.Imports);
            if (this.debugEnabled)
            {
                codeDomParameters.GenerateInMemory = false;
                codeDomParameters.IncludeDebugInformation = true;
                codeDomParameters.TempFiles.KeepFiles = true;
            }
            PermissionSet set = new PermissionSet(PermissionState.None);
            if (xmlParameters.IsNeedTempDirAccess)
            {
                set.AddPermission(TempAssembly.FileIOPermission);
            }
            set.AddPermission(new EnvironmentPermission(PermissionState.Unrestricted));
            set.AddPermission(new SecurityPermission(SecurityPermissionFlag.UnmanagedCode));
            set.AddPermission(new SecurityPermission(SecurityPermissionFlag.ControlEvidence));
            set.Assert();
            if ((parent != null) && ((codeDomParameters.OutputAssembly == null) || (codeDomParameters.OutputAssembly.Length == 0)))
            {
                string str = AssemblyNameFromOptions(codeDomParameters.CompilerOptions);
                if (str == null)
                {
                    str = GetTempAssemblyPath(codeDomParameters.TempFiles.TempDir, parent, ns);
                }
                codeDomParameters.OutputAssembly = str;
            }
            if ((codeDomParameters.CompilerOptions == null) || (codeDomParameters.CompilerOptions.Length == 0))
            {
                codeDomParameters.CompilerOptions = "/nostdlib";
            }
            else
            {
                codeDomParameters.CompilerOptions = codeDomParameters.CompilerOptions + " /nostdlib";
            }
            codeDomParameters.CompilerOptions = codeDomParameters.CompilerOptions + " /D:_DYNAMIC_XMLSERIALIZER_COMPILATION";
            codeDomParameters.Evidence = evidence;
            CompilerResults results = null;
            Assembly compiledAssembly = null;
            try
            {
                results = provider.CompileAssemblyFromSource(codeDomParameters, new string[] { this.writer.ToString() });
                if (results.Errors.Count > 0)
                {
                    StringWriter writer = new StringWriter(CultureInfo.InvariantCulture);
                    writer.WriteLine(Res.GetString("XmlCompilerError", new object[] { results.NativeCompilerReturnValue.ToString(CultureInfo.InvariantCulture) }));
                    bool flag = false;
                    foreach (CompilerError error in results.Errors)
                    {
                        error.FileName = "";
                        if (!error.IsWarning || (error.ErrorNumber == "CS1595"))
                        {
                            flag = true;
                            writer.WriteLine(error.ToString());
                        }
                    }
                    if (flag)
                    {
                        throw new InvalidOperationException(writer.ToString());
                    }
                }
                compiledAssembly = results.CompiledAssembly;
            }
            catch (UnauthorizedAccessException)
            {
                string currentUser = GetCurrentUser();
                if ((currentUser == null) || (currentUser.Length == 0))
                {
                    throw new UnauthorizedAccessException(Res.GetString("XmlSerializerAccessDenied"));
                }
                throw new UnauthorizedAccessException(Res.GetString("XmlIdentityAccessDenied", new object[] { currentUser }));
            }
            catch (FileLoadException exception)
            {
                throw new InvalidOperationException(Res.GetString("XmlSerializerCompileFailed"), exception);
            }
            finally
            {
                CodeAccessPermission.RevertAssert();
            }
            if (compiledAssembly == null)
            {
                throw new InvalidOperationException(Res.GetString("XmlInternalError"));
            }
            return compiledAssembly;
        }

        internal static string GetCurrentUser()
        {
            try
            {
                WindowsIdentity current = WindowsIdentity.GetCurrent();
                if ((current != null) && (current.Name != null))
                {
                    return current.Name;
                }
            }
            catch (Exception exception)
            {
                if (((exception is ThreadAbortException) || (exception is StackOverflowException)) || (exception is OutOfMemoryException))
                {
                    throw;
                }
            }
            return "";
        }

        internal static string GetTempAssemblyName(AssemblyName parent, string ns)
        {
            return (parent.Name + ".XmlSerializers" + (((ns == null) || (ns.Length == 0)) ? "" : ("." + ns.GetHashCode())));
        }

        internal static string GetTempAssemblyPath(string baseDir, Assembly assembly, string defaultNamespace)
        {
            if (assembly.IsDynamic)
            {
                throw new InvalidOperationException(Res.GetString("XmlPregenAssemblyDynamic"));
            }
            PermissionSet set = new PermissionSet(PermissionState.None);
            set.AddPermission(new FileIOPermission(PermissionState.Unrestricted));
            set.AddPermission(new EnvironmentPermission(PermissionState.Unrestricted));
            set.Assert();
            try
            {
                if ((baseDir != null) && (baseDir.Length > 0))
                {
                    if (!Directory.Exists(baseDir))
                    {
                        throw new UnauthorizedAccessException(Res.GetString("XmlPregenMissingDirectory", new object[] { baseDir }));
                    }
                }
                else
                {
                    baseDir = Path.GetTempPath();
                    if (!Directory.Exists(baseDir))
                    {
                        throw new UnauthorizedAccessException(Res.GetString("XmlPregenMissingTempDirectory"));
                    }
                }
                if (baseDir.EndsWith(@"\", StringComparison.Ordinal))
                {
                    baseDir = baseDir + GetTempAssemblyName(assembly.GetName(), defaultNamespace);
                }
                else
                {
                    baseDir = baseDir + @"\" + GetTempAssemblyName(assembly.GetName(), defaultNamespace);
                }
            }
            finally
            {
                CodeAccessPermission.RevertAssert();
            }
            return (baseDir + ".dll");
        }

        protected string[] Imports
        {
            get
            {
                string[] array = new string[this.imports.Values.Count];
                this.imports.Values.CopyTo(array, 0);
                return array;
            }
        }

        internal TextWriter Source
        {
            get
            {
                return this.writer;
            }
        }
    }
}

