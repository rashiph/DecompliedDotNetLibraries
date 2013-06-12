namespace System.Xml.Serialization
{
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Security.Policy;
    using System.Text;
    using System.Threading;
    using System.Xml;

    internal class TempAssembly
    {
        private Hashtable assemblies;
        private Assembly assembly;
        private XmlSerializerImplementation contract;
        private static object[] emptyObjectArray = new object[0];
        private static System.Security.Permissions.FileIOPermission fileIOPermission;
        private const string GeneratedAssemblyNamespace = "Microsoft.Xml.Serialization.GeneratedAssembly";
        private TempMethodDictionary methods;
        private bool pregeneratedAssmbly;
        private Hashtable readerMethods;
        private Hashtable writerMethods;

        private TempAssembly()
        {
            this.assemblies = new Hashtable();
        }

        internal TempAssembly(XmlSerializerImplementation contract)
        {
            this.assemblies = new Hashtable();
            this.contract = contract;
            this.pregeneratedAssmbly = true;
        }

        internal TempAssembly(XmlMapping[] xmlMappings, Assembly assembly, XmlSerializerImplementation contract)
        {
            this.assemblies = new Hashtable();
            this.assembly = assembly;
            this.InitAssemblyMethods(xmlMappings);
            this.contract = contract;
            this.pregeneratedAssmbly = true;
        }

        internal TempAssembly(XmlMapping[] xmlMappings, Type[] types, string defaultNamespace, string location, Evidence evidence)
        {
            this.assemblies = new Hashtable();
            this.assembly = GenerateAssembly(xmlMappings, types, defaultNamespace, evidence, XmlSerializerCompilerParameters.Create(location), null, this.assemblies);
            this.InitAssemblyMethods(xmlMappings);
        }

        internal bool CanRead(XmlMapping mapping, XmlReader xmlReader)
        {
            if (mapping == null)
            {
                return false;
            }
            if (mapping.Accessor.Any)
            {
                return true;
            }
            TempMethod method = this.methods[mapping.Key];
            return xmlReader.IsStartElement(method.name, method.ns);
        }

        internal static Assembly GenerateAssembly(XmlMapping[] xmlMappings, Type[] types, string defaultNamespace, Evidence evidence, XmlSerializerCompilerParameters parameters, Assembly assembly, Hashtable assemblies)
        {
            Assembly assembly3;
            FileIOPermission.Assert();
            for (int i = 0; i < xmlMappings.Length; i++)
            {
                xmlMappings[i].CheckShallow();
            }
            Compiler compiler = new Compiler();
            try
            {
                Hashtable hashtable = new Hashtable();
                foreach (XmlMapping mapping in xmlMappings)
                {
                    hashtable[mapping.Scope] = mapping;
                }
                TypeScope[] array = new TypeScope[hashtable.Keys.Count];
                hashtable.Keys.CopyTo(array, 0);
                assemblies.Clear();
                Hashtable hashtable2 = new Hashtable();
                foreach (TypeScope scope in array)
                {
                    foreach (Type type in scope.Types)
                    {
                        compiler.AddImport(type, hashtable2);
                        Assembly assembly2 = type.Assembly;
                        string fullName = assembly2.FullName;
                        if ((assemblies[fullName] == null) && !assembly2.GlobalAssemblyCache)
                        {
                            assemblies[fullName] = assembly2;
                        }
                    }
                }
                for (int j = 0; j < types.Length; j++)
                {
                    compiler.AddImport(types[j], hashtable2);
                }
                compiler.AddImport(typeof(object).Assembly);
                compiler.AddImport(typeof(XmlSerializer).Assembly);
                IndentedWriter writer = new IndentedWriter(compiler.Source, false);
                writer.WriteLine("#if _DYNAMIC_XMLSERIALIZER_COMPILATION");
                writer.WriteLine("[assembly:System.Security.AllowPartiallyTrustedCallers()]");
                writer.WriteLine("[assembly:System.Security.SecurityTransparent()]");
                writer.WriteLine("[assembly:System.Security.SecurityRules(System.Security.SecurityRuleSet.Level1)]");
                writer.WriteLine("#endif");
                if (((types != null) && (types.Length > 0)) && (types[0] != null))
                {
                    writer.WriteLine("[assembly:System.Reflection.AssemblyVersionAttribute(\"" + types[0].Assembly.GetName().Version.ToString() + "\")]");
                }
                if ((assembly != null) && (types.Length > 0))
                {
                    for (int num3 = 0; num3 < types.Length; num3++)
                    {
                        Type type2 = types[num3];
                        if ((type2 != null) && DynamicAssemblies.IsTypeDynamic(type2))
                        {
                            throw new InvalidOperationException(Res.GetString("XmlPregenTypeDynamic", new object[] { types[num3].FullName }));
                        }
                    }
                    writer.Write("[assembly:");
                    writer.Write(typeof(XmlSerializerVersionAttribute).FullName);
                    writer.Write("(");
                    writer.Write("ParentAssemblyId=");
                    ReflectionAwareCodeGen.WriteQuotedCSharpString(writer, GenerateAssemblyId(types[0]));
                    writer.Write(", Version=");
                    ReflectionAwareCodeGen.WriteQuotedCSharpString(writer, "4.0.0.0");
                    if (defaultNamespace != null)
                    {
                        writer.Write(", Namespace=");
                        ReflectionAwareCodeGen.WriteQuotedCSharpString(writer, defaultNamespace);
                    }
                    writer.WriteLine(")]");
                }
                CodeIdentifiers classes = new CodeIdentifiers();
                classes.AddUnique("XmlSerializationWriter", "XmlSerializationWriter");
                classes.AddUnique("XmlSerializationReader", "XmlSerializationReader");
                string str2 = null;
                if (((types != null) && (types.Length == 1)) && (types[0] != null))
                {
                    str2 = CodeIdentifier.MakeValid(types[0].Name);
                    if (types[0].IsArray)
                    {
                        str2 = str2 + "Array";
                    }
                }
                writer.WriteLine("namespace Microsoft.Xml.Serialization.GeneratedAssembly {");
                writer.Indent++;
                writer.WriteLine();
                string identifier = "XmlSerializationWriter" + str2;
                identifier = classes.AddUnique(identifier, identifier);
                XmlSerializationWriterCodeGen gen = new XmlSerializationWriterCodeGen(writer, array, "public", identifier);
                gen.GenerateBegin();
                string[] writerMethods = new string[xmlMappings.Length];
                for (int k = 0; k < xmlMappings.Length; k++)
                {
                    writerMethods[k] = gen.GenerateElement(xmlMappings[k]);
                }
                gen.GenerateEnd();
                writer.WriteLine();
                string str4 = "XmlSerializationReader" + str2;
                str4 = classes.AddUnique(str4, str4);
                XmlSerializationReaderCodeGen gen2 = new XmlSerializationReaderCodeGen(writer, array, "public", str4);
                gen2.GenerateBegin();
                string[] methods = new string[xmlMappings.Length];
                for (int m = 0; m < xmlMappings.Length; m++)
                {
                    methods[m] = gen2.GenerateElement(xmlMappings[m]);
                }
                gen2.GenerateEnd(methods, xmlMappings, types);
                string baseSerializer = gen2.GenerateBaseSerializer("XmlSerializer1", str4, identifier, classes);
                Hashtable serializers = new Hashtable();
                for (int n = 0; n < xmlMappings.Length; n++)
                {
                    if (serializers[xmlMappings[n].Key] == null)
                    {
                        serializers[xmlMappings[n].Key] = gen2.GenerateTypedSerializer(methods[n], writerMethods[n], xmlMappings[n], classes, baseSerializer, str4, identifier);
                    }
                }
                gen2.GenerateSerializerContract("XmlSerializerContract", xmlMappings, types, str4, methods, identifier, writerMethods, serializers);
                writer.Indent--;
                writer.WriteLine("}");
                assembly3 = compiler.Compile(assembly, defaultNamespace, parameters, evidence);
            }
            finally
            {
                compiler.Close();
            }
            return assembly3;
        }

        private static string GenerateAssemblyId(Type type)
        {
            Module[] modules = type.Assembly.GetModules();
            ArrayList list = new ArrayList();
            for (int i = 0; i < modules.Length; i++)
            {
                list.Add(modules[i].ModuleVersionId.ToString());
            }
            list.Sort();
            StringBuilder builder = new StringBuilder();
            for (int j = 0; j < list.Count; j++)
            {
                builder.Append(list[j].ToString());
                builder.Append(",");
            }
            return builder.ToString();
        }

        private static MethodInfo GetMethodFromType(Type type, string methodName, Assembly assembly)
        {
            MethodInfo method = type.GetMethod(methodName);
            if (method != null)
            {
                return method;
            }
            MissingMethodException innerException = new MissingMethodException(type.FullName, methodName);
            if (assembly != null)
            {
                throw new InvalidOperationException(Res.GetString("XmlSerializerExpired", new object[] { assembly.FullName, assembly.CodeBase }), innerException);
            }
            throw innerException;
        }

        private static AssemblyName GetName(Assembly assembly, bool copyName)
        {
            PermissionSet set = new PermissionSet(PermissionState.None);
            set.AddPermission(new System.Security.Permissions.FileIOPermission(PermissionState.Unrestricted));
            set.Assert();
            return assembly.GetName(copyName);
        }

        internal Assembly GetReferencedAssembly(string name)
        {
            if ((this.assemblies != null) && (name != null))
            {
                return (Assembly) this.assemblies[name];
            }
            return null;
        }

        internal static Type GetTypeFromAssembly(Assembly assembly, string typeName)
        {
            typeName = "Microsoft.Xml.Serialization.GeneratedAssembly." + typeName;
            Type type = assembly.GetType(typeName);
            if (type == null)
            {
                throw new InvalidOperationException(Res.GetString("XmlMissingType", new object[] { typeName, assembly.FullName }));
            }
            return type;
        }

        internal void InitAssemblyMethods(XmlMapping[] xmlMappings)
        {
            this.methods = new TempMethodDictionary();
            for (int i = 0; i < xmlMappings.Length; i++)
            {
                TempMethod method = new TempMethod {
                    isSoap = xmlMappings[i].IsSoap,
                    methodKey = xmlMappings[i].Key
                };
                XmlTypeMapping mapping = xmlMappings[i] as XmlTypeMapping;
                if (mapping != null)
                {
                    method.name = mapping.ElementName;
                    method.ns = mapping.Namespace;
                }
                this.methods.Add(xmlMappings[i].Key, method);
            }
        }

        internal object InvokeReader(XmlMapping mapping, XmlReader xmlReader, XmlDeserializationEvents events, string encodingStyle)
        {
            XmlSerializationReader reader = null;
            object obj2;
            try
            {
                encodingStyle = this.ValidateEncodingStyle(encodingStyle, mapping.Key);
                reader = this.Contract.Reader;
                reader.Init(xmlReader, events, encodingStyle, this);
                if (this.methods[mapping.Key].readMethod == null)
                {
                    if (this.readerMethods == null)
                    {
                        this.readerMethods = this.Contract.ReadMethods;
                    }
                    string methodName = (string) this.readerMethods[mapping.Key];
                    if (methodName == null)
                    {
                        throw new InvalidOperationException(Res.GetString("XmlNotSerializable", new object[] { mapping.Accessor.Name }));
                    }
                    this.methods[mapping.Key].readMethod = GetMethodFromType(reader.GetType(), methodName, this.pregeneratedAssmbly ? this.assembly : null);
                }
                obj2 = this.methods[mapping.Key].readMethod.Invoke(reader, emptyObjectArray);
            }
            catch (SecurityException exception)
            {
                throw new InvalidOperationException(Res.GetString("XmlNoPartialTrust"), exception);
            }
            finally
            {
                if (reader != null)
                {
                    reader.Dispose();
                }
            }
            return obj2;
        }

        internal void InvokeWriter(XmlMapping mapping, XmlWriter xmlWriter, object o, XmlSerializerNamespaces namespaces, string encodingStyle, string id)
        {
            XmlSerializationWriter writer = null;
            try
            {
                encodingStyle = this.ValidateEncodingStyle(encodingStyle, mapping.Key);
                writer = this.Contract.Writer;
                writer.Init(xmlWriter, namespaces, encodingStyle, id, this);
                if (this.methods[mapping.Key].writeMethod == null)
                {
                    if (this.writerMethods == null)
                    {
                        this.writerMethods = this.Contract.WriteMethods;
                    }
                    string methodName = (string) this.writerMethods[mapping.Key];
                    if (methodName == null)
                    {
                        throw new InvalidOperationException(Res.GetString("XmlNotSerializable", new object[] { mapping.Accessor.Name }));
                    }
                    this.methods[mapping.Key].writeMethod = GetMethodFromType(writer.GetType(), methodName, this.pregeneratedAssmbly ? this.assembly : null);
                }
                this.methods[mapping.Key].writeMethod.Invoke(writer, new object[] { o });
            }
            catch (SecurityException exception)
            {
                throw new InvalidOperationException(Res.GetString("XmlNoPartialTrust"), exception);
            }
            finally
            {
                if (writer != null)
                {
                    writer.Dispose();
                }
            }
        }

        private static bool IsSerializerVersionMatch(Assembly serializer, Type type, string defaultNamespace, string location)
        {
            if (serializer == null)
            {
                return false;
            }
            object[] customAttributes = serializer.GetCustomAttributes(typeof(XmlSerializerVersionAttribute), false);
            if (customAttributes.Length != 1)
            {
                return false;
            }
            XmlSerializerVersionAttribute attribute = (XmlSerializerVersionAttribute) customAttributes[0];
            return ((attribute.ParentAssemblyId == GenerateAssemblyId(type)) && (attribute.Namespace == defaultNamespace));
        }

        internal static Assembly LoadGeneratedAssembly(Type type, string defaultNamespace, out XmlSerializerImplementation contract)
        {
            Assembly serializer = null;
            contract = null;
            string partialName = null;
            bool enabled = DiagnosticsSwitches.PregenEventLog.Enabled;
            object[] customAttributes = type.GetCustomAttributes(typeof(XmlSerializerAssemblyAttribute), false);
            if (customAttributes.Length == 0)
            {
                AssemblyName parent = GetName(type.Assembly, true);
                partialName = Compiler.GetTempAssemblyName(parent, defaultNamespace);
                parent.Name = partialName;
                parent.CodeBase = null;
                parent.CultureInfo = CultureInfo.InvariantCulture;
                try
                {
                    serializer = Assembly.Load(parent);
                }
                catch (Exception exception)
                {
                    if (((exception is ThreadAbortException) || (exception is StackOverflowException)) || (exception is OutOfMemoryException))
                    {
                        throw;
                    }
                    if (enabled)
                    {
                        Log(exception.Message, EventLogEntryType.Information);
                    }
                    byte[] publicKeyToken = parent.GetPublicKeyToken();
                    if ((publicKeyToken != null) && (publicKeyToken.Length > 0))
                    {
                        return null;
                    }
                    serializer = Assembly.LoadWithPartialName(partialName, null);
                }
                if (serializer == null)
                {
                    if (enabled)
                    {
                        Log(Res.GetString("XmlPregenCannotLoad", new object[] { partialName }), EventLogEntryType.Information);
                    }
                    return null;
                }
                if (!IsSerializerVersionMatch(serializer, type, defaultNamespace, null))
                {
                    if (enabled)
                    {
                        Log(Res.GetString("XmlSerializerExpiredDetails", new object[] { partialName, type.FullName }), EventLogEntryType.Error);
                    }
                    return null;
                }
            }
            else
            {
                XmlSerializerAssemblyAttribute attribute = (XmlSerializerAssemblyAttribute) customAttributes[0];
                if ((attribute.AssemblyName != null) && (attribute.CodeBase != null))
                {
                    throw new InvalidOperationException(Res.GetString("XmlPregenInvalidXmlSerializerAssemblyAttribute", new object[] { "AssemblyName", "CodeBase" }));
                }
                if (attribute.AssemblyName != null)
                {
                    partialName = attribute.AssemblyName;
                    serializer = Assembly.LoadWithPartialName(partialName, null);
                }
                else if ((attribute.CodeBase != null) && (attribute.CodeBase.Length > 0))
                {
                    partialName = attribute.CodeBase;
                    serializer = Assembly.LoadFrom(partialName);
                }
                else
                {
                    partialName = type.Assembly.FullName;
                    serializer = type.Assembly;
                }
                if (serializer == null)
                {
                    throw new FileNotFoundException(null, partialName);
                }
            }
            Type typeFromAssembly = GetTypeFromAssembly(serializer, "XmlSerializerContract");
            contract = (XmlSerializerImplementation) Activator.CreateInstance(typeFromAssembly);
            if (contract.CanSerialize(type))
            {
                return serializer;
            }
            if (enabled)
            {
                Log(Res.GetString("XmlSerializerExpiredDetails", new object[] { partialName, type.FullName }), EventLogEntryType.Error);
            }
            return null;
        }

        private static void Log(string message, EventLogEntryType type)
        {
            new EventLogPermission(PermissionState.Unrestricted).Assert();
            EventLog.WriteEntry("XmlSerializer", message, type);
        }

        private string ValidateEncodingStyle(string encodingStyle, string methodKey)
        {
            if ((encodingStyle != null) && (encodingStyle.Length > 0))
            {
                if (!this.methods[methodKey].isSoap)
                {
                    throw new InvalidOperationException(Res.GetString("XmlInvalidEncodingNotEncoded1", new object[] { encodingStyle }));
                }
                if ((encodingStyle != "http://schemas.xmlsoap.org/soap/encoding/") && (encodingStyle != "http://www.w3.org/2003/05/soap-encoding"))
                {
                    throw new InvalidOperationException(Res.GetString("XmlInvalidEncoding3", new object[] { encodingStyle, "http://schemas.xmlsoap.org/soap/encoding/", "http://www.w3.org/2003/05/soap-encoding" }));
                }
                return encodingStyle;
            }
            if (this.methods[methodKey].isSoap)
            {
                encodingStyle = "http://schemas.xmlsoap.org/soap/encoding/";
            }
            return encodingStyle;
        }

        internal XmlSerializerImplementation Contract
        {
            get
            {
                if (this.contract == null)
                {
                    this.contract = (XmlSerializerImplementation) Activator.CreateInstance(GetTypeFromAssembly(this.assembly, "XmlSerializerContract"));
                }
                return this.contract;
            }
        }

        internal static System.Security.Permissions.FileIOPermission FileIOPermission
        {
            get
            {
                if (fileIOPermission == null)
                {
                    fileIOPermission = new System.Security.Permissions.FileIOPermission(PermissionState.Unrestricted);
                }
                return fileIOPermission;
            }
        }

        internal bool NeedAssembyResolve
        {
            get
            {
                return ((this.assemblies != null) && (this.assemblies.Count > 0));
            }
        }

        internal class TempMethod
        {
            internal bool isSoap;
            internal string methodKey;
            internal string name;
            internal string ns;
            internal MethodInfo readMethod;
            internal MethodInfo writeMethod;
        }

        internal sealed class TempMethodDictionary : DictionaryBase
        {
            internal void Add(string key, TempAssembly.TempMethod value)
            {
                base.Dictionary.Add(key, value);
            }

            internal TempAssembly.TempMethod this[string key]
            {
                get
                {
                    return (TempAssembly.TempMethod) base.Dictionary[key];
                }
            }
        }
    }
}

