namespace System.Workflow.ComponentModel.Compiler
{
    using System;
    using System.CodeDom;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading;

    public sealed class TypeProvider : ITypeProvider, IServiceProvider, IDisposable
    {
        private List<string> addedAssemblies;
        private List<CodeCompileUnit> addedCompileUnits;
        private Hashtable assemblyLoaders = new Hashtable();
        private Hashtable compileUnitLoaders = new Hashtable();
        private Hashtable designTimeTypes = new Hashtable();
        private bool executingEnsureCurrentTypes;
        private Hashtable hashOfDTTypes = new Hashtable();
        private Hashtable hashOfRTTypes = new Hashtable();
        private Assembly localAssembly;
        internal static readonly char[] nameSeparators = new char[] { '.', '+' };
        private Dictionary<CodeCompileUnit, EventHandler> needRefreshCompileUnits;
        private Hashtable rawAssemblyLoaders = new Hashtable();
        private IServiceProvider serviceProvider;
        private Dictionary<PropertyInfo, bool> supportedProperties;
        private Hashtable typeLoadErrors = new Hashtable();
        private Dictionary<Type, string> typeToAssemblyName;

        public event EventHandler TypeLoadErrorsChanged;

        public event EventHandler TypesChanged;

        public TypeProvider(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public void AddAssembly(Assembly assembly)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException("assembly");
            }
            if (!this.rawAssemblyLoaders.Contains(assembly))
            {
                try
                {
                    this.rawAssemblyLoaders[assembly] = new AssemblyLoader(this, assembly, this.localAssembly == assembly);
                    if (this.TypesChanged != null)
                    {
                        FireEventsNoThrow(this.TypesChanged, new object[] { this, EventArgs.Empty });
                    }
                }
                catch (Exception exception)
                {
                    this.typeLoadErrors[assembly.FullName] = exception;
                    if (this.TypeLoadErrorsChanged != null)
                    {
                        FireEventsNoThrow(this.TypeLoadErrorsChanged, new object[] { this, EventArgs.Empty });
                    }
                }
            }
        }

        public void AddAssemblyReference(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            if ((File.Exists(path) && !this.assemblyLoaders.ContainsKey(path)) && ((this.addedAssemblies == null) || !this.addedAssemblies.Contains(path)))
            {
                if (this.addedAssemblies == null)
                {
                    this.addedAssemblies = new List<string>();
                }
                this.addedAssemblies.Add(path);
                if (this.TypesChanged != null)
                {
                    FireEventsNoThrow(this.TypesChanged, new object[] { this, EventArgs.Empty });
                }
            }
        }

        public void AddCodeCompileUnit(CodeCompileUnit codeCompileUnit)
        {
            if (codeCompileUnit == null)
            {
                throw new ArgumentNullException("codeCompileUnit");
            }
            if (this.compileUnitLoaders.ContainsKey(codeCompileUnit) || ((this.addedCompileUnits != null) && this.addedCompileUnits.Contains(codeCompileUnit)))
            {
                throw new ArgumentException(TypeSystemSR.GetString("Error_DuplicateCodeCompileUnit"), "codeCompileUnit");
            }
            if (this.addedCompileUnits == null)
            {
                this.addedCompileUnits = new List<CodeCompileUnit>();
            }
            this.addedCompileUnits.Add(codeCompileUnit);
            if ((this.needRefreshCompileUnits != null) && this.needRefreshCompileUnits.ContainsKey(codeCompileUnit))
            {
                this.needRefreshCompileUnits.Remove(codeCompileUnit);
            }
            if (this.TypesChanged != null)
            {
                FireEventsNoThrow(this.TypesChanged, new object[] { this, EventArgs.Empty });
            }
        }

        internal void AddType(Type type)
        {
            string fullName = type.FullName;
            if (!this.designTimeTypes.Contains(fullName))
            {
                this.designTimeTypes[fullName] = type;
            }
        }

        public void Dispose()
        {
            if (this.compileUnitLoaders != null)
            {
                foreach (CodeDomLoader loader in this.compileUnitLoaders.Values)
                {
                    loader.Dispose();
                }
                this.compileUnitLoaders.Clear();
            }
            this.addedAssemblies = null;
            this.addedCompileUnits = null;
            this.needRefreshCompileUnits = null;
        }

        private void EnsureCurrentTypes()
        {
            if (!this.executingEnsureCurrentTypes)
            {
                try
                {
                    bool flag = false;
                    this.executingEnsureCurrentTypes = true;
                    if (this.addedAssemblies != null)
                    {
                        string[] strArray = this.addedAssemblies.ToArray();
                        this.addedAssemblies = null;
                        foreach (string str in strArray)
                        {
                            AssemblyLoader loader = null;
                            try
                            {
                                loader = new AssemblyLoader(this, str);
                                this.assemblyLoaders[str] = loader;
                            }
                            catch (Exception exception)
                            {
                                this.typeLoadErrors[str] = exception;
                                flag = true;
                            }
                        }
                    }
                    if (this.addedCompileUnits != null)
                    {
                        CodeCompileUnit[] unitArray = this.addedCompileUnits.ToArray();
                        this.addedCompileUnits = null;
                        foreach (CodeCompileUnit unit in unitArray)
                        {
                            CodeDomLoader loader2 = null;
                            try
                            {
                                loader2 = new CodeDomLoader(this, unit);
                                this.compileUnitLoaders[unit] = loader2;
                            }
                            catch (Exception exception2)
                            {
                                if (loader2 != null)
                                {
                                    loader2.Dispose();
                                }
                                this.typeLoadErrors[unit] = exception2;
                                flag = true;
                            }
                        }
                    }
                    if (this.needRefreshCompileUnits != null)
                    {
                        Dictionary<CodeCompileUnit, EventHandler> dictionary = new Dictionary<CodeCompileUnit, EventHandler>();
                        foreach (KeyValuePair<CodeCompileUnit, EventHandler> pair in this.needRefreshCompileUnits)
                        {
                            dictionary.Add(pair.Key, pair.Value);
                        }
                        this.needRefreshCompileUnits = null;
                        foreach (KeyValuePair<CodeCompileUnit, EventHandler> pair2 in dictionary)
                        {
                            CodeDomLoader loader3 = this.compileUnitLoaders[pair2.Key] as CodeDomLoader;
                            if (loader3 != null)
                            {
                                try
                                {
                                    loader3.Refresh(pair2.Value);
                                }
                                catch (Exception exception3)
                                {
                                    this.typeLoadErrors[pair2.Value] = exception3;
                                    flag = true;
                                }
                            }
                        }
                    }
                    if (flag && (this.TypeLoadErrorsChanged != null))
                    {
                        FireEventsNoThrow(this.TypeLoadErrorsChanged, new object[] { this, EventArgs.Empty });
                    }
                }
                finally
                {
                    this.executingEnsureCurrentTypes = false;
                }
            }
        }

        private static void FireEventsNoThrow(Delegate eventDelegator, object[] args)
        {
            if (eventDelegator != null)
            {
                foreach (Delegate delegate2 in eventDelegator.GetInvocationList())
                {
                    try
                    {
                        delegate2.DynamicInvoke(args);
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }

        public string GetAssemblyName(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            if (this.typeToAssemblyName == null)
            {
                this.typeToAssemblyName = new Dictionary<Type, string>();
            }
            string fullName = null;
            if (!this.typeToAssemblyName.TryGetValue(type, out fullName) && (type.Assembly != null))
            {
                if (this.AssemblyNameResolver != null)
                {
                    fullName = this.AssemblyNameResolver(type);
                }
                else
                {
                    fullName = type.Assembly.FullName;
                }
                this.typeToAssemblyName.Add(type, fullName);
            }
            if (fullName == null)
            {
                fullName = string.Empty;
            }
            return fullName;
        }

        public static string[] GetEnumNames(Type enumType)
        {
            if (enumType == null)
            {
                throw new ArgumentNullException("enumType");
            }
            if (!IsSubclassOf(enumType, typeof(Enum)))
            {
                throw new ArgumentException(TypeSystemSR.GetString("Error_TypeIsNotEnum"));
            }
            FieldInfo[] fields = enumType.GetFields();
            List<string> list = new List<string>();
            for (int i = 0; i < fields.Length; i++)
            {
                list.Add(fields[i].Name);
            }
            list.Sort();
            return list.ToArray();
        }

        public static Type GetEventHandlerType(EventInfo eventInfo)
        {
            if (eventInfo == null)
            {
                throw new ArgumentNullException("eventInfo");
            }
            MethodInfo addMethod = eventInfo.GetAddMethod(true);
            if (addMethod != null)
            {
                ParameterInfo[] parameters = addMethod.GetParameters();
                Type superClass = typeof(Delegate);
                for (int i = 0; i < parameters.Length; i++)
                {
                    Type parameterType = parameters[i].ParameterType;
                    if (IsSubclassOf(parameterType, superClass))
                    {
                        return parameterType;
                    }
                }
            }
            return null;
        }

        public object GetService(Type serviceType)
        {
            if (this.serviceProvider == null)
            {
                return null;
            }
            return this.serviceProvider.GetService(serviceType);
        }

        public Type GetType(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            return this.GetType(name, false);
        }

        public Type GetType(string name, bool throwOnError)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            this.EnsureCurrentTypes();
            bool flag = false;
            Type type = null;
            string typeName = string.Empty;
            string[] parameters = null;
            string elemantDecorator = string.Empty;
            if (ParseHelpers.ParseTypeName(name, ParseHelpers.ParseTypeNameLanguage.NetFramework, out typeName, out parameters, out elemantDecorator))
            {
                if ((parameters != null) && (parameters.Length > 0))
                {
                    Type type2 = this.GetType(typeName, throwOnError);
                    if ((type2 == null) || !type2.IsGenericTypeDefinition)
                    {
                        return null;
                    }
                    Type[] typeArguments = new Type[parameters.Length];
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        Type type3 = this.GetType(parameters[i], throwOnError);
                        if (type3 == null)
                        {
                            return null;
                        }
                        typeArguments[i] = type3;
                    }
                    return type2.MakeGenericType(typeArguments);
                }
                if (elemantDecorator != string.Empty)
                {
                    Type type4 = this.GetType(typeName);
                    if (type4 != null)
                    {
                        StringBuilder builder = new StringBuilder(type4.FullName);
                        for (int j = 0; j < elemantDecorator.Length; j++)
                        {
                            if (elemantDecorator[j] != ' ')
                            {
                                builder.Append(elemantDecorator[j]);
                            }
                        }
                        name = builder.ToString();
                        if (type4.Assembly != null)
                        {
                            type = type4.Assembly.GetType(name, false);
                        }
                        if (type == null)
                        {
                            if (this.hashOfDTTypes.Contains(name))
                            {
                                return (this.hashOfDTTypes[name] as Type);
                            }
                            type = new DesignTimeType(null, name, this);
                            this.hashOfDTTypes.Add(name, type);
                            return type;
                        }
                    }
                }
                else
                {
                    string thatName = string.Empty;
                    int index = name.IndexOf(',');
                    if (index != -1)
                    {
                        typeName = name.Substring(0, index);
                        thatName = name.Substring(index + 1).Trim();
                    }
                    typeName = typeName.Trim();
                    if (typeName.Length > 0)
                    {
                        type = this.designTimeTypes[typeName] as Type;
                        if (type == null)
                        {
                            foreach (DictionaryEntry entry in this.rawAssemblyLoaders)
                            {
                                AssemblyLoader loader = entry.Value as AssemblyLoader;
                                if ((thatName.Length == 0) || ParseHelpers.AssemblyNameEquals(loader.AssemblyName, thatName))
                                {
                                    try
                                    {
                                        type = loader.GetType(typeName);
                                    }
                                    catch (Exception exception)
                                    {
                                        if (!this.typeLoadErrors.Contains(entry.Key))
                                        {
                                            this.typeLoadErrors[entry.Key] = exception;
                                            flag = true;
                                        }
                                        if (throwOnError)
                                        {
                                            throw exception;
                                        }
                                    }
                                    if (type != null)
                                    {
                                        break;
                                    }
                                }
                            }
                        }
                        if (type == null)
                        {
                            foreach (DictionaryEntry entry2 in this.assemblyLoaders)
                            {
                                AssemblyLoader loader2 = entry2.Value as AssemblyLoader;
                                if ((thatName.Length == 0) || ParseHelpers.AssemblyNameEquals(loader2.AssemblyName, thatName))
                                {
                                    try
                                    {
                                        type = loader2.GetType(typeName);
                                    }
                                    catch (Exception exception2)
                                    {
                                        if (!this.typeLoadErrors.Contains(entry2.Key))
                                        {
                                            this.typeLoadErrors[entry2.Key] = exception2;
                                            flag = true;
                                        }
                                        if (throwOnError)
                                        {
                                            throw exception2;
                                        }
                                    }
                                    if (type != null)
                                    {
                                        break;
                                    }
                                }
                            }
                        }
                        if (flag && (this.TypeLoadErrorsChanged != null))
                        {
                            FireEventsNoThrow(this.TypeLoadErrorsChanged, new object[] { this, EventArgs.Empty });
                        }
                        if (((type == null) && (this.localAssembly != null)) && (thatName == this.localAssembly.FullName))
                        {
                            type = this.localAssembly.GetType(typeName);
                        }
                    }
                }
            }
            if (type == null)
            {
                if (throwOnError)
                {
                    throw new Exception(TypeSystemSR.GetString(CultureInfo.CurrentCulture, "Error_TypeResolution", new object[] { name }));
                }
                return null;
            }
            if (((this.designTimeTypes == null) || (this.designTimeTypes.Count <= 0)) || ((type.Assembly == null) || !type.IsGenericTypeDefinition))
            {
                return type;
            }
            if (this.hashOfRTTypes.Contains(type))
            {
                return (Type) this.hashOfRTTypes[type];
            }
            Type type5 = new RTTypeWrapper(this, type);
            this.hashOfRTTypes.Add(type, type5);
            return type5;
        }

        public Type[] GetTypes()
        {
            this.EnsureCurrentTypes();
            bool flag = false;
            this.typeLoadErrors.Clear();
            List<Type> list = new List<Type>();
            foreach (Type type in this.designTimeTypes.Values)
            {
                list.Add(type);
            }
            foreach (DictionaryEntry entry in this.assemblyLoaders)
            {
                AssemblyLoader loader = entry.Value as AssemblyLoader;
                try
                {
                    list.AddRange(loader.GetTypes());
                }
                catch (Exception exception)
                {
                    ReflectionTypeLoadException exception2 = exception as ReflectionTypeLoadException;
                    if (exception2 != null)
                    {
                        foreach (Type type2 in exception2.Types)
                        {
                            if (type2 != null)
                            {
                                list.Add(type2);
                            }
                        }
                    }
                    if (this.typeLoadErrors.Contains(entry.Key))
                    {
                        this.typeLoadErrors.Remove(entry.Key);
                    }
                    this.typeLoadErrors[entry.Key] = exception;
                    flag = true;
                }
            }
            foreach (DictionaryEntry entry2 in this.rawAssemblyLoaders)
            {
                AssemblyLoader loader2 = entry2.Value as AssemblyLoader;
                try
                {
                    list.AddRange(loader2.GetTypes());
                }
                catch (Exception exception3)
                {
                    ReflectionTypeLoadException exception4 = exception3 as ReflectionTypeLoadException;
                    if (exception4 != null)
                    {
                        foreach (Type type3 in exception4.Types)
                        {
                            if (type3 != null)
                            {
                                list.Add(type3);
                            }
                        }
                    }
                    if (this.typeLoadErrors.Contains(entry2.Key))
                    {
                        this.typeLoadErrors.Remove(entry2.Key);
                    }
                    this.typeLoadErrors[entry2.Key] = exception3;
                    flag = true;
                }
            }
            if (flag && (this.TypeLoadErrorsChanged != null))
            {
                FireEventsNoThrow(this.TypeLoadErrorsChanged, new object[] { this, EventArgs.Empty });
            }
            return list.ToArray();
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static bool IsAssignable(Type toType, Type fromType)
        {
            return IsAssignable(toType, fromType, false);
        }

        internal static bool IsAssignable(Type toType, Type fromType, bool equalBasedOnSameTypeRepresenting)
        {
            if ((toType == null) || (fromType == null))
            {
                return false;
            }
            if (equalBasedOnSameTypeRepresenting)
            {
                if (IsRepresentingTheSameType(fromType, toType))
                {
                    return true;
                }
            }
            else if (fromType == toType)
            {
                return true;
            }
            if (!toType.IsGenericTypeDefinition)
            {
                if ((toType.Assembly == null) && (fromType.Assembly != null))
                {
                    return false;
                }
                if ((fromType is RTTypeWrapper) || (fromType is DesignTimeType))
                {
                    if (!(toType is RTTypeWrapper) && !(toType is DesignTimeType))
                    {
                        ITypeProvider provider = (fromType is RTTypeWrapper) ? (fromType as RTTypeWrapper).Provider : (fromType as DesignTimeType).Provider;
                        if (provider != null)
                        {
                            toType = provider.GetType(toType.FullName);
                        }
                    }
                    goto Label_0111;
                }
                if ((toType is RTTypeWrapper) || (toType is DesignTimeType))
                {
                    if (!(fromType is RTTypeWrapper) && !(fromType is DesignTimeType))
                    {
                        ITypeProvider provider2 = (toType is RTTypeWrapper) ? (toType as RTTypeWrapper).Provider : (toType as DesignTimeType).Provider;
                        if (provider2 != null)
                        {
                            fromType = provider2.GetType(fromType.FullName);
                        }
                    }
                    goto Label_0111;
                }
            }
            return toType.IsAssignableFrom(fromType);
        Label_0111:
            if ((toType != null) && (fromType != null))
            {
                if (equalBasedOnSameTypeRepresenting)
                {
                    if (IsRepresentingTheSameType(fromType, toType))
                    {
                        return true;
                    }
                }
                else if (fromType == toType)
                {
                    return true;
                }
                if (IsSubclassOf(fromType, toType))
                {
                    return true;
                }
                if (toType.IsInterface)
                {
                    Type[] interfaces = fromType.GetInterfaces();
                    for (int i = 0; i < interfaces.Length; i++)
                    {
                        if (interfaces[i] == toType)
                        {
                            return true;
                        }
                        if (IsSubclassOf(interfaces[i], toType))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public static bool IsEnum(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            return IsSubclassOf(type, typeof(Enum));
        }

        internal static bool IsRepresentingTheSameType(Type firstType, Type secondType)
        {
            if ((firstType == null) || (secondType == null))
            {
                return false;
            }
            if (firstType != secondType)
            {
                if (firstType.FullName != secondType.FullName)
                {
                    return false;
                }
                if (firstType.Assembly != secondType.Assembly)
                {
                    return false;
                }
                if ((firstType.Assembly != null) && (firstType.AssemblyQualifiedName != secondType.AssemblyQualifiedName))
                {
                    return false;
                }
            }
            return true;
        }

        public static bool IsSubclassOf(Type subclass, Type superClass)
        {
            if (superClass != subclass)
            {
                if ((subclass == null) || (superClass == null))
                {
                    return false;
                }
                if (superClass == typeof(object))
                {
                    return true;
                }
                subclass = subclass.BaseType;
                while (subclass != null)
                {
                    if (superClass == subclass)
                    {
                        return true;
                    }
                    subclass = subclass.BaseType;
                }
            }
            return false;
        }

        public bool IsSupportedProperty(PropertyInfo property, object declaringInstance)
        {
            if (property == null)
            {
                throw new ArgumentNullException("property");
            }
            if (declaringInstance == null)
            {
                throw new ArgumentNullException("declaringInstance");
            }
            if (this.IsSupportedPropertyResolver == null)
            {
                return true;
            }
            if (this.supportedProperties == null)
            {
                this.supportedProperties = new Dictionary<PropertyInfo, bool>();
            }
            bool flag = false;
            if (!this.supportedProperties.TryGetValue(property, out flag))
            {
                flag = this.IsSupportedPropertyResolver(property, declaringInstance);
                this.supportedProperties.Add(property, flag);
            }
            return flag;
        }

        public void RefreshCodeCompileUnit(CodeCompileUnit codeCompileUnit, EventHandler refresher)
        {
            if (codeCompileUnit == null)
            {
                throw new ArgumentNullException("codeCompileUnit");
            }
            if ((!this.compileUnitLoaders.Contains(codeCompileUnit) && (this.addedCompileUnits != null)) && !this.addedCompileUnits.Contains(codeCompileUnit))
            {
                throw new ArgumentException(TypeSystemSR.GetString("Error_NoCodeCompileUnit"), "codeCompileUnit");
            }
            if (this.needRefreshCompileUnits == null)
            {
                this.needRefreshCompileUnits = new Dictionary<CodeCompileUnit, EventHandler>();
            }
            this.needRefreshCompileUnits[codeCompileUnit] = refresher;
            if (this.TypesChanged != null)
            {
                FireEventsNoThrow(this.TypesChanged, new object[] { this, EventArgs.Empty });
            }
        }

        public void RemoveAssembly(Assembly assembly)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException("assembly");
            }
            if (((AssemblyLoader) this.rawAssemblyLoaders[assembly]) != null)
            {
                this.rawAssemblyLoaders.Remove(assembly);
                this.RemoveCachedAssemblyWrappedTypes(assembly);
                if (this.TypesChanged != null)
                {
                    FireEventsNoThrow(this.TypesChanged, new object[] { this, EventArgs.Empty });
                }
            }
        }

        public void RemoveAssemblyReference(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            AssemblyLoader loader = this.assemblyLoaders[path] as AssemblyLoader;
            if (loader != null)
            {
                this.assemblyLoaders.Remove(path);
                this.RemoveCachedAssemblyWrappedTypes(loader.Assembly);
            }
            if ((this.addedAssemblies != null) && this.addedAssemblies.Contains(path))
            {
                this.addedAssemblies.Remove(path);
            }
            if (this.typeLoadErrors.ContainsKey(path))
            {
                this.typeLoadErrors.Remove(path);
                if (this.TypeLoadErrorsChanged != null)
                {
                    FireEventsNoThrow(this.TypeLoadErrorsChanged, new object[] { this, EventArgs.Empty });
                }
            }
            if (this.TypesChanged != null)
            {
                FireEventsNoThrow(this.TypesChanged, new object[] { this, EventArgs.Empty });
            }
        }

        private void RemoveCachedAssemblyWrappedTypes(Assembly assembly)
        {
            ArrayList list = new ArrayList(this.hashOfRTTypes.Keys);
            foreach (Type type in list)
            {
                if (type.IsGenericTypeDefinition)
                {
                    ((RTTypeWrapper) this.hashOfRTTypes[type]).OnAssemblyRemoved(assembly);
                }
                if (type.Assembly == assembly)
                {
                    this.hashOfRTTypes.Remove(type);
                }
            }
        }

        public void RemoveCodeCompileUnit(CodeCompileUnit codeCompileUnit)
        {
            if (codeCompileUnit == null)
            {
                throw new ArgumentNullException("codeCompileUnit");
            }
            CodeDomLoader loader = this.compileUnitLoaders[codeCompileUnit] as CodeDomLoader;
            if (loader != null)
            {
                loader.Dispose();
                this.compileUnitLoaders.Remove(codeCompileUnit);
            }
            if ((this.addedCompileUnits != null) && this.addedCompileUnits.Contains(codeCompileUnit))
            {
                this.addedCompileUnits.Remove(codeCompileUnit);
            }
            if ((this.needRefreshCompileUnits != null) && this.needRefreshCompileUnits.ContainsKey(codeCompileUnit))
            {
                this.needRefreshCompileUnits.Remove(codeCompileUnit);
            }
            if (this.typeLoadErrors.ContainsKey(codeCompileUnit))
            {
                this.typeLoadErrors.Remove(codeCompileUnit);
                if (this.TypeLoadErrorsChanged != null)
                {
                    FireEventsNoThrow(this.TypeLoadErrorsChanged, new object[] { this, EventArgs.Empty });
                }
            }
            if (this.TypesChanged != null)
            {
                FireEventsNoThrow(this.TypesChanged, new object[] { this, EventArgs.Empty });
            }
        }

        internal void RemoveTypes(Type[] types)
        {
            foreach (Type type in types)
            {
                string fullName = type.FullName;
                StringCollection strings = new StringCollection();
                foreach (Type type2 in this.hashOfDTTypes.Values)
                {
                    Type elementType = type2;
                    while ((elementType != null) && elementType.HasElementType)
                    {
                        elementType = elementType.GetElementType();
                    }
                    if (elementType == type)
                    {
                        strings.Add(type2.FullName);
                    }
                }
                foreach (string str2 in strings)
                {
                    this.hashOfDTTypes.Remove(str2);
                }
                this.designTimeTypes.Remove(fullName);
            }
        }

        public void SetLocalAssembly(Assembly assembly)
        {
            this.localAssembly = assembly;
            if (this.TypesChanged != null)
            {
                FireEventsNoThrow(this.TypesChanged, new object[] { this, EventArgs.Empty });
            }
        }

        public Func<Type, string> AssemblyNameResolver
        {
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.<AssemblyNameResolver>k__BackingField;
            }
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.<AssemblyNameResolver>k__BackingField = value;
            }
        }

        public Func<PropertyInfo, object, bool> IsSupportedPropertyResolver
        {
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.<IsSupportedPropertyResolver>k__BackingField;
            }
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.<IsSupportedPropertyResolver>k__BackingField = value;
            }
        }

        public Assembly LocalAssembly
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.localAssembly;
            }
        }

        public ICollection<Assembly> ReferencedAssemblies
        {
            get
            {
                this.EnsureCurrentTypes();
                List<Assembly> list = new List<Assembly>();
                foreach (AssemblyLoader loader in this.assemblyLoaders.Values)
                {
                    if (loader.Assembly != null)
                    {
                        list.Add(loader.Assembly);
                    }
                }
                foreach (Assembly assembly in this.rawAssemblyLoaders.Keys)
                {
                    list.Add(assembly);
                }
                return list.AsReadOnly();
            }
        }

        public IDictionary<object, Exception> TypeLoadErrors
        {
            get
            {
                Dictionary<object, Exception> dictionary = new Dictionary<object, Exception>();
                foreach (DictionaryEntry entry in this.typeLoadErrors)
                {
                    Exception innerException = entry.Value as Exception;
                    while (innerException is TargetInvocationException)
                    {
                        innerException = innerException.InnerException;
                    }
                    if (innerException != null)
                    {
                        string message = null;
                        if (entry.Key is CodeCompileUnit)
                        {
                            message = TypeSystemSR.GetString("Error_CodeCompileUnitNotLoaded", new object[] { innerException.Message });
                        }
                        else if (entry.Key is string)
                        {
                            message = TypeSystemSR.GetString("Error_AssemblyRefNotLoaded", new object[] { entry.Key.ToString(), innerException.Message });
                        }
                        if (message != null)
                        {
                            innerException = new Exception(message, innerException);
                        }
                        dictionary.Add(entry.Key, innerException);
                    }
                }
                return dictionary;
            }
        }
    }
}

