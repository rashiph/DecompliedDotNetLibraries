namespace System.Management.Instrumentation
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Configuration.Install;
    using System.Diagnostics;
    using System.IO;
    using System.Management;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Text;

    internal class SchemaNaming
    {
        private Assembly assembly;
        private AssemblySpecificNaming assemblyInfo;
        private const string DecoupledProviderClassName = "MSFT_DecoupledProvider";
        private const string DecoupledProviderCLSID = "{54D8502C-527D-43f7-A506-A9DA075E229C}";
        private const string EventProviderRegistrationClassName = "__EventProviderRegistration";
        private const string GlobalWmiNetNamespace = @"root\MicrosoftWmiNet";
        private const string InstanceProviderRegistrationClassName = "__InstanceProviderRegistration";
        private const string InstrumentationClassName = "WMINET_Instrumentation";
        private const string InstrumentedAssembliesClassName = "WMINET_InstrumentedAssembly";
        private const string InstrumentedNamespacesClassName = "WMINET_InstrumentedNamespaces";
        private const string iwoaDef = "class IWOA\r\n{\r\nprotected const string DllName = \"wminet_utils.dll\";\r\nprotected const string EntryPointName = \"UFunc\";\r\n[SuppressUnmanagedCodeSecurity, DllImport(DllName, EntryPoint=\"GetPropertyHandle\")] public static extern int GetPropertyHandle_f27(int vFunc, IntPtr pWbemClassObject, [In][MarshalAs(UnmanagedType.LPWStr)]  string   wszPropertyName, [Out] out Int32 pType, [Out] out Int32 plHandle);\r\n//[SuppressUnmanagedCodeSecurity, DllImport(DllName, EntryPoint=\"WritePropertyValue\")] public static extern int WritePropertyValue_f28(int vFunc, IntPtr pWbemClassObject, [In] Int32 lHandle, [In] Int32 lNumBytes, [In] ref Byte aData);\r\n[SuppressUnmanagedCodeSecurity, DllImport(DllName, EntryPoint=\"ReadPropertyValue\")] public static extern int ReadPropertyValue_f29(int vFunc, IntPtr pWbemClassObject, [In] Int32 lHandle, [In] Int32 lBufferSize, [Out] out Int32 plNumBytes, [Out] out Byte aData);\r\n[SuppressUnmanagedCodeSecurity, DllImport(DllName, EntryPoint=\"ReadDWORD\")] public static extern int ReadDWORD_f30(int vFunc, IntPtr pWbemClassObject, [In] Int32 lHandle, [Out] out UInt32 pdw);\r\n[SuppressUnmanagedCodeSecurity, DllImport(DllName, EntryPoint=\"WriteDWORD\")] public static extern int WriteDWORD_f31(int vFunc, IntPtr pWbemClassObject, [In] Int32 lHandle, [In] UInt32 dw);\r\n[SuppressUnmanagedCodeSecurity, DllImport(DllName, EntryPoint=\"ReadQWORD\")] public static extern int ReadQWORD_f32(int vFunc, IntPtr pWbemClassObject, [In] Int32 lHandle, [Out] out UInt64 pqw);\r\n[SuppressUnmanagedCodeSecurity, DllImport(DllName, EntryPoint=\"WriteQWORD\")] public static extern int WriteQWORD_f33(int vFunc, IntPtr pWbemClassObject, [In] Int32 lHandle, [In] UInt64 pw);\r\n[SuppressUnmanagedCodeSecurity, DllImport(DllName, EntryPoint=\"GetPropertyInfoByHandle\")] public static extern int GetPropertyInfoByHandle_f34(int vFunc, IntPtr pWbemClassObject, [In] Int32 lHandle, [Out][MarshalAs(UnmanagedType.BStr)]  out string   pstrName, [Out] out Int32 pType);\r\n[SuppressUnmanagedCodeSecurity, DllImport(DllName, EntryPoint=\"Lock\")] public static extern int Lock_f35(int vFunc, IntPtr pWbemClassObject, [In] Int32 lFlags);\r\n[SuppressUnmanagedCodeSecurity, DllImport(DllName, EntryPoint=\"Unlock\")] public static extern int Unlock_f36(int vFunc, IntPtr pWbemClassObject, [In] Int32 lFlags);\r\n\r\n[SuppressUnmanagedCodeSecurity, DllImport(DllName, EntryPoint=\"Put\")] public static extern int Put_f5(int vFunc, IntPtr pWbemClassObject, [In][MarshalAs(UnmanagedType.LPWStr)]  string   wszName, [In] Int32 lFlags, [In] ref object pVal, [In] Int32 Type);\r\n[SuppressUnmanagedCodeSecurity, DllImport(DllName, EntryPoint=\"WritePropertyValue\")] public static extern int WritePropertyValue_f28(int vFunc, IntPtr pWbemClassObject, [In] Int32 lHandle, [In] Int32 lNumBytes, [In][MarshalAs(UnmanagedType.LPWStr)] string str);\r\n[SuppressUnmanagedCodeSecurity, DllImport(DllName, EntryPoint=\"WritePropertyValue\")] public static extern int WritePropertyValue_f28(int vFunc, IntPtr pWbemClassObject, [In] Int32 lHandle, [In] Int32 lNumBytes, [In] ref Byte n);\r\n[SuppressUnmanagedCodeSecurity, DllImport(DllName, EntryPoint=\"WritePropertyValue\")] public static extern int WritePropertyValue_f28(int vFunc, IntPtr pWbemClassObject, [In] Int32 lHandle, [In] Int32 lNumBytes, [In] ref SByte n);\r\n[SuppressUnmanagedCodeSecurity, DllImport(DllName, EntryPoint=\"WritePropertyValue\")] public static extern int WritePropertyValue_f28(int vFunc, IntPtr pWbemClassObject, [In] Int32 lHandle, [In] Int32 lNumBytes, [In] ref Int16 n);\r\n[SuppressUnmanagedCodeSecurity, DllImport(DllName, EntryPoint=\"WritePropertyValue\")] public static extern int WritePropertyValue_f28(int vFunc, IntPtr pWbemClassObject, [In] Int32 lHandle, [In] Int32 lNumBytes, [In] ref UInt16 n);\r\n[SuppressUnmanagedCodeSecurity, DllImport(DllName, EntryPoint=\"WritePropertyValue\", CharSet=CharSet.Unicode)] public static extern int WritePropertyValue_f28(int vFunc, IntPtr pWbemClassObject, [In] Int32 lHandle, [In] Int32 lNumBytes, [In] ref Char c);\r\n[SuppressUnmanagedCodeSecurity, DllImport(DllName, EntryPoint=\"WriteDWORD\")] public static extern int WriteDWORD_f31(int vFunc, IntPtr pWbemClassObject, [In] Int32 lHandle, [In] Int32 dw);\r\n[SuppressUnmanagedCodeSecurity, DllImport(DllName, EntryPoint=\"WriteSingle\")] public static extern int WriteDWORD_f31(int vFunc, IntPtr pWbemClassObject, [In] Int32 lHandle, [In] Single dw);\r\n[SuppressUnmanagedCodeSecurity, DllImport(DllName, EntryPoint=\"WriteQWORD\")] public static extern int WriteQWORD_f33(int vFunc, IntPtr pWbemClassObject, [In] Int32 lHandle, [In] Int64 pw);\r\n[SuppressUnmanagedCodeSecurity, DllImport(DllName, EntryPoint=\"WriteDouble\")] public static extern int WriteQWORD_f33(int vFunc, IntPtr pWbemClassObject, [In] Int32 lHandle, [In] Double pw);\r\n[SuppressUnmanagedCodeSecurity, DllImport(DllName, EntryPoint=\"Clone\")] public static extern int Clone_f(int vFunc, IntPtr pWbemClassObject, [Out] out IntPtr ppCopy);\r\n}\r\ninterface IWmiConverter\r\n{\r\n    void ToWMI(object obj);\r\n    ManagementObject GetInstance();\r\n}\r\nclass SafeAssign\r\n{\r\n    public static UInt16 boolTrue = 0xffff;\r\n    public static UInt16 boolFalse = 0;\r\n    static Hashtable validTypes = new Hashtable();\r\n    static SafeAssign()\r\n    {\r\n        validTypes.Add(typeof(SByte), null);\r\n        validTypes.Add(typeof(Byte), null);\r\n        validTypes.Add(typeof(Int16), null);\r\n        validTypes.Add(typeof(UInt16), null);\r\n        validTypes.Add(typeof(Int32), null);\r\n        validTypes.Add(typeof(UInt32), null);\r\n        validTypes.Add(typeof(Int64), null);\r\n        validTypes.Add(typeof(UInt64), null);\r\n        validTypes.Add(typeof(Single), null);\r\n        validTypes.Add(typeof(Double), null);\r\n        validTypes.Add(typeof(Boolean), null);\r\n        validTypes.Add(typeof(String), null);\r\n        validTypes.Add(typeof(Char), null);\r\n        validTypes.Add(typeof(DateTime), null);\r\n        validTypes.Add(typeof(TimeSpan), null);\r\n        validTypes.Add(typeof(ManagementObject), null);\r\n        nullClass.SystemProperties [\"__CLASS\"].Value = \"nullInstance\";\r\n    }\r\n    public static object GetInstance(object o)\r\n    {\r\n        if(o is ManagementObject)\r\n            return o;\r\n        return null;\r\n    }\r\n    static ManagementClass nullClass = new ManagementClass(";
        private const string iwoaDefEnd = ");\r\n    \r\n    public static ManagementObject GetManagementObject(object o)\r\n    {\r\n        if(o != null && o is ManagementObject)\r\n            return o as ManagementObject;\r\n        // Must return empty instance\r\n        return nullClass.CreateInstance();\r\n    }\r\n    public static object GetValue(object o)\r\n    {\r\n        Type t = o.GetType();\r\n        if(t.IsArray)\r\n            t = t.GetElementType();\r\n        if(validTypes.Contains(t))\r\n            return o;\r\n        return null;\r\n    }\r\n    public static string WMITimeToString(DateTime dt)\r\n    {\r\n        TimeSpan ts = dt.Subtract(dt.ToUniversalTime());\r\n        int diffUTC = (ts.Minutes + ts.Hours * 60);\r\n        if(diffUTC >= 0)\r\n            return String.Format(\"{0:D4}{1:D2}{2:D2}{3:D2}{4:D2}{5:D2}.{6:D3}000+{7:D3}\", dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, dt.Millisecond, diffUTC);\r\n        return String.Format(\"{0:D4}{1:D2}{2:D2}{3:D2}{4:D2}{5:D2}.{6:D3}000-{7:D3}\", dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, dt.Millisecond, -diffUTC);\r\n    }\r\n    public static string WMITimeToString(TimeSpan ts)\r\n    {\r\n        return String.Format(\"{0:D8}{1:D2}{2:D2}{3:D2}.{4:D3}000:000\", ts.Days, ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds);\r\n    }\r\n    public static string[] WMITimeArrayToStringArray(DateTime[] dates)\r\n    {\r\n        string[] strings = new string[dates.Length];\r\n        for(int i=0;i<dates.Length;i++)\r\n            strings[i] = WMITimeToString(dates[i]);\r\n        return strings;\r\n    }\r\n    public static string[] WMITimeArrayToStringArray(TimeSpan[] timeSpans)\r\n    {\r\n        string[] strings = new string[timeSpans.Length];\r\n        for(int i=0;i<timeSpans.Length;i++)\r\n            strings[i] = WMITimeToString(timeSpans[i]);\r\n        return strings;\r\n    }\r\n}\r\n";
        private const string NamingClassName = "WMINET_Naming";
        private const string ProviderClassName = "WMINET_ManagedAssemblyProvider";
        private ManagementObject registrationInstance;
        private const string Win32ProviderClassName = "__Win32Provider";

        private SchemaNaming(string namespaceName, string securityDescriptor, Assembly assembly)
        {
            this.assembly = assembly;
            this.assemblyInfo = new AssemblySpecificNaming(namespaceName, securityDescriptor, assembly);
            if (!DoesInstanceExist(this.RegistrationPath))
            {
                this.assemblyInfo.DecoupledProviderInstanceName = AssemblyNameUtility.UniqueToAssemblyMinorVersion(assembly);
            }
        }

        private static string AppendProperty(string classPath, string propertyName, string propertyValue)
        {
            return string.Concat(new object[] { classPath, '.', propertyName, "=\"", propertyValue, '"' });
        }

        private static bool DoesClassExist(string objectPath)
        {
            bool flag = false;
            try
            {
                new ManagementClass(objectPath).Get();
                flag = true;
            }
            catch (ManagementException exception)
            {
                if (((ManagementStatus.InvalidNamespace != exception.ErrorCode) && (ManagementStatus.InvalidClass != exception.ErrorCode)) && (ManagementStatus.NotFound != exception.ErrorCode))
                {
                    throw exception;
                }
            }
            return flag;
        }

        private static bool DoesInstanceExist(string objectPath)
        {
            bool flag = false;
            try
            {
                new ManagementObject(objectPath).Get();
                flag = true;
            }
            catch (ManagementException exception)
            {
                if (((ManagementStatus.InvalidNamespace != exception.ErrorCode) && (ManagementStatus.InvalidClass != exception.ErrorCode)) && (ManagementStatus.NotFound != exception.ErrorCode))
                {
                    throw exception;
                }
            }
            return flag;
        }

        private static void EnsureClassExists(InstallLogWrapper context, string classPath, ClassMaker classMakerFunction)
        {
            try
            {
                context.LogMessage(RC.GetString("CLASS_ENSURE") + " " + classPath);
                new ManagementClass(classPath).Get();
            }
            catch (ManagementException exception)
            {
                if (exception.ErrorCode != ManagementStatus.NotFound)
                {
                    throw exception;
                }
                context.LogMessage(RC.GetString("CLASS_ENSURECREATE") + " " + classPath);
                classMakerFunction().Put();
            }
        }

        private static void EnsureNamespace(InstallLogWrapper context, string namespaceName)
        {
            context.LogMessage(RC.GetString("NAMESPACE_ENSURE") + " " + namespaceName);
            string baseNamespace = null;
            foreach (string str2 in namespaceName.Split(new char[] { '\\' }))
            {
                if (baseNamespace == null)
                {
                    baseNamespace = str2;
                }
                else
                {
                    EnsureNamespace(baseNamespace, str2);
                    baseNamespace = baseNamespace + @"\" + str2;
                }
            }
        }

        private static void EnsureNamespace(string baseNamespace, string childNamespaceName)
        {
            if (!DoesInstanceExist(baseNamespace + ":__NAMESPACE.Name=\"" + childNamespaceName + "\""))
            {
                ManagementObject obj2 = new ManagementClass(baseNamespace + ":__NAMESPACE").CreateInstance();
                obj2["Name"] = childNamespaceName;
                obj2.Put();
            }
        }

        private static string EnsureNamespaceInMof(string namespaceName)
        {
            string str = "";
            string baseNamespace = null;
            foreach (string str3 in namespaceName.Split(new char[] { '\\' }))
            {
                if (baseNamespace == null)
                {
                    baseNamespace = str3;
                }
                else
                {
                    str = str + EnsureNamespaceInMof(baseNamespace, str3);
                    baseNamespace = baseNamespace + @"\" + str3;
                }
            }
            return str;
        }

        private static string EnsureNamespaceInMof(string baseNamespace, string childNamespaceName)
        {
            return string.Format("{0}instance of __Namespace\r\n{{\r\n  Name = \"{1}\";\r\n}};\r\n\r\n", PragmaNamespace(baseNamespace), childNamespaceName);
        }

        private string GenerateMof(string[] mofs)
        {
            return ("//**************************************************************************\r\n" + string.Format("//* {0}\r\n", this.DecoupledProviderInstanceName) + string.Format("//* {0}\r\n", this.AssemblyUniqueIdentifier) + "//**************************************************************************\r\n" + "#pragma autorecover\r\n" + EnsureNamespaceInMof(this.GlobalRegistrationNamespace) + EnsureNamespaceInMof(this.NamespaceName) + PragmaNamespace(this.GlobalRegistrationNamespace) + GetMofFormat(new ManagementClass(this.GlobalInstrumentationClassPath)) + GetMofFormat(new ManagementClass(this.GlobalRegistrationClassPath)) + GetMofFormat(new ManagementClass(this.GlobalNamingClassPath)) + GetMofFormat(new ManagementObject(this.GlobalRegistrationPath)) + PragmaNamespace(this.NamespaceName) + GetMofFormat(new ManagementClass(this.InstrumentationClassPath)) + GetMofFormat(new ManagementClass(this.RegistrationClassPath)) + GetMofFormat(new ManagementClass(this.DecoupledProviderClassPath)) + GetMofFormat(new ManagementClass(this.ProviderClassPath)) + GetMofFormat(new ManagementObject(this.ProviderPath)) + GetMofFormat(new ManagementObject(this.EventProviderRegistrationPath)) + GetMofFormat(new ManagementObject(this.InstanceProviderRegistrationPath)) + string.Concat(mofs) + GetMofFormat(new ManagementObject(this.RegistrationPath)));
        }

        private static string GetMofFormat(ManagementObject obj)
        {
            return (obj.GetText(TextFormat.Mof).Replace("\n", "\r\n") + "\r\n");
        }

        public static SchemaNaming GetSchemaNaming(Assembly assembly)
        {
            InstrumentedAttribute attribute = InstrumentedAttribute.GetAttribute(assembly);
            if (attribute == null)
            {
                return null;
            }
            return new SchemaNaming(attribute.NamespaceName, attribute.SecurityDescriptor, assembly);
        }

        public bool IsAssemblyRegistered()
        {
            if (DoesInstanceExist(this.RegistrationPath))
            {
                ManagementObject obj2 = new ManagementObject(this.RegistrationPath);
                return (0 == string.Compare(this.AssemblyUniqueIdentifier, obj2["RegisteredBuild"].ToString(), StringComparison.OrdinalIgnoreCase));
            }
            return false;
        }

        private bool IsClassAlreadyPresentInRepository(ManagementObject obj)
        {
            bool flag = false;
            string objectPath = MakeClassPath(this.NamespaceName, (string) obj.SystemProperties["__CLASS"].Value);
            if (DoesClassExist(objectPath))
            {
                flag = new ManagementClass(objectPath).CompareTo(obj, ComparisonSettings.IgnoreCase | ComparisonSettings.IgnoreObjectSource);
            }
            return flag;
        }

        private bool IsSchemaToBeCompared()
        {
            bool flag = false;
            if (DoesInstanceExist(this.RegistrationPath))
            {
                ManagementObject obj2 = new ManagementObject(this.RegistrationPath);
                flag = 0 != string.Compare(this.AssemblyUniqueIdentifier, obj2["RegisteredBuild"].ToString(), StringComparison.OrdinalIgnoreCase);
            }
            return flag;
        }

        private static string MakeClassPath(string namespaceName, string className)
        {
            return (namespaceName + ":" + className);
        }

        private ManagementClass MakeDecoupledProviderClass()
        {
            ManagementClass class3 = new ManagementClass(this.Win32ProviderClassPath).Derive("MSFT_DecoupledProvider");
            PropertyDataCollection properties = class3.Properties;
            properties.Add("HostingModel", "Decoupled:Com", CimType.String);
            properties.Add("SecurityDescriptor", CimType.String, false);
            properties.Add("Version", 1, CimType.UInt32);
            properties["CLSID"].Value = "{54D8502C-527D-43f7-A506-A9DA075E229C}";
            return class3;
        }

        private ManagementClass MakeGlobalInstrumentationClass()
        {
            ManagementClass class2 = new ManagementClass(@"root\MicrosoftWmiNet", "", null);
            class2.SystemProperties["__CLASS"].Value = "WMINET_Instrumentation";
            class2.Qualifiers.Add("abstract", true);
            return class2;
        }

        private ManagementClass MakeInstrumentationClass()
        {
            ManagementClass class2 = new ManagementClass(this.NamespaceName, "", null);
            class2.SystemProperties["__CLASS"].Value = "WMINET_Instrumentation";
            class2.Qualifiers.Add("abstract", true);
            return class2;
        }

        private ManagementClass MakeNamespaceRegistrationClass()
        {
            ManagementClass class3 = new ManagementClass(this.GlobalInstrumentationClassPath).Derive("WMINET_InstrumentedNamespaces");
            PropertyDataCollection properties = class3.Properties;
            properties.Add("NamespaceName", CimType.String, false);
            PropertyData data = properties["NamespaceName"];
            data.Qualifiers.Add("key", true);
            return class3;
        }

        private ManagementClass MakeNamingClass()
        {
            ManagementClass class3 = new ManagementClass(this.GlobalInstrumentationClassPath).Derive("WMINET_Naming");
            class3.Qualifiers.Add("abstract", true);
            class3.Properties.Add("InstrumentedAssembliesClassName", "WMINET_InstrumentedAssembly", CimType.String);
            return class3;
        }

        private ManagementClass MakeProviderClass()
        {
            ManagementClass class3 = new ManagementClass(this.DecoupledProviderClassPath).Derive("WMINET_ManagedAssemblyProvider");
            class3.Properties.Add("Assembly", CimType.String, false);
            return class3;
        }

        private ManagementClass MakeRegistrationClass()
        {
            ManagementClass class3 = new ManagementClass(this.InstrumentationClassPath).Derive("WMINET_InstrumentedAssembly");
            PropertyDataCollection properties = class3.Properties;
            properties.Add("Name", CimType.String, false);
            PropertyData data = properties["Name"];
            data.Qualifiers.Add("key", true);
            properties.Add("RegisteredBuild", CimType.String, false);
            properties.Add("FullName", CimType.String, false);
            properties.Add("PathToAssembly", CimType.String, false);
            properties.Add("Code", CimType.String, false);
            properties.Add("Mof", CimType.String, false);
            return class3;
        }

        private static string PragmaNamespace(string namespaceName)
        {
            return string.Format("#pragma namespace(\"\\\\\\\\.\\\\{0}\")\r\n\r\n", namespaceName.Replace(@"\", @"\\"));
        }

        private void RegisterAssemblyAsInstrumented()
        {
            ManagementObject obj2 = new ManagementClass(this.RegistrationClassPath).CreateInstance();
            obj2["Name"] = this.DecoupledProviderInstanceName;
            obj2["RegisteredBuild"] = this.AssemblyUniqueIdentifier;
            obj2["FullName"] = this.AssemblyName;
            obj2["PathToAssembly"] = this.AssemblyPath;
            obj2["Code"] = "";
            obj2["Mof"] = "";
            obj2.Put();
        }

        private void RegisterAssemblySpecificDecoupledProviderInstance()
        {
            ManagementObject obj2 = new ManagementClass(this.ProviderClassPath).CreateInstance();
            obj2["Name"] = this.DecoupledProviderInstanceName;
            obj2["HostingModel"] = "Decoupled:Com";
            if (this.SecurityDescriptor != null)
            {
                obj2["SecurityDescriptor"] = this.SecurityDescriptor;
            }
            obj2.Put();
        }

        public void RegisterAssemblySpecificSchema()
        {
            SecurityHelper.UnmanagedCode.Demand();
            Type[] instrumentedTypes = InstrumentedAttribute.GetInstrumentedTypes(this.assembly);
            StringCollection events = new StringCollection();
            StringCollection strings2 = new StringCollection();
            StringCollection strings3 = new StringCollection();
            string[] mofs = new string[instrumentedTypes.Length];
            CodeWriter writer = new CodeWriter();
            ReferencesCollection referencess = new ReferencesCollection();
            writer.AddChild(referencess.UsingCode);
            referencess.Add(typeof(object));
            referencess.Add(typeof(ManagementClass));
            referencess.Add(typeof(Marshal));
            referencess.Add(typeof(SuppressUnmanagedCodeSecurityAttribute));
            referencess.Add(typeof(FieldInfo));
            referencess.Add(typeof(Hashtable));
            writer.Line();
            CodeWriter writer2 = writer.AddChild("public class WMINET_Converter");
            writer2.Line("public static Hashtable mapTypeToConverter = new Hashtable();");
            CodeWriter writer3 = writer2.AddChild("static WMINET_Converter()");
            Hashtable mapTypeToConverterClassName = new Hashtable();
            for (int i = 0; i < instrumentedTypes.Length; i++)
            {
                mapTypeToConverterClassName[instrumentedTypes[i]] = "ConvertClass_" + i;
            }
            bool flag = this.IsSchemaToBeCompared();
            bool flag2 = false;
            if (!flag)
            {
                flag2 = !this.IsAssemblyRegistered();
            }
            for (int j = 0; j < instrumentedTypes.Length; j++)
            {
                SchemaMapping mapping = new SchemaMapping(instrumentedTypes[j], this, mapTypeToConverterClassName);
                writer3.Line(string.Format("mapTypeToConverter[typeof({0})] = typeof({1});", mapping.ClassType.FullName.Replace('+', '.'), mapping.CodeClassName));
                if (flag && !this.IsClassAlreadyPresentInRepository(mapping.NewClass))
                {
                    flag2 = true;
                }
                ReplaceClassIfNecessary(mapping.ClassPath, mapping.NewClass);
                mofs[j] = GetMofFormat(mapping.NewClass);
                writer.AddChild(mapping.Code);
                switch (mapping.InstrumentationType)
                {
                    case InstrumentationType.Instance:
                        strings2.Add(mapping.ClassName);
                        break;

                    case InstrumentationType.Event:
                        events.Add(mapping.ClassName);
                        break;

                    case InstrumentationType.Abstract:
                        strings3.Add(mapping.ClassName);
                        break;
                }
            }
            this.RegisterAssemblySpecificDecoupledProviderInstance();
            this.RegisterProviderAsEventProvider(events);
            this.RegisterProviderAsInstanceProvider();
            this.RegisterAssemblyAsInstrumented();
            Directory.CreateDirectory(this.DataDirectory);
            using (StreamWriter writer4 = new StreamWriter(this.CodePath, false, Encoding.Unicode))
            {
                writer4.WriteLine(writer);
                writer4.WriteLine("class IWOA\r\n{\r\nprotected const string DllName = \"wminet_utils.dll\";\r\nprotected const string EntryPointName = \"UFunc\";\r\n[SuppressUnmanagedCodeSecurity, DllImport(DllName, EntryPoint=\"GetPropertyHandle\")] public static extern int GetPropertyHandle_f27(int vFunc, IntPtr pWbemClassObject, [In][MarshalAs(UnmanagedType.LPWStr)]  string   wszPropertyName, [Out] out Int32 pType, [Out] out Int32 plHandle);\r\n//[SuppressUnmanagedCodeSecurity, DllImport(DllName, EntryPoint=\"WritePropertyValue\")] public static extern int WritePropertyValue_f28(int vFunc, IntPtr pWbemClassObject, [In] Int32 lHandle, [In] Int32 lNumBytes, [In] ref Byte aData);\r\n[SuppressUnmanagedCodeSecurity, DllImport(DllName, EntryPoint=\"ReadPropertyValue\")] public static extern int ReadPropertyValue_f29(int vFunc, IntPtr pWbemClassObject, [In] Int32 lHandle, [In] Int32 lBufferSize, [Out] out Int32 plNumBytes, [Out] out Byte aData);\r\n[SuppressUnmanagedCodeSecurity, DllImport(DllName, EntryPoint=\"ReadDWORD\")] public static extern int ReadDWORD_f30(int vFunc, IntPtr pWbemClassObject, [In] Int32 lHandle, [Out] out UInt32 pdw);\r\n[SuppressUnmanagedCodeSecurity, DllImport(DllName, EntryPoint=\"WriteDWORD\")] public static extern int WriteDWORD_f31(int vFunc, IntPtr pWbemClassObject, [In] Int32 lHandle, [In] UInt32 dw);\r\n[SuppressUnmanagedCodeSecurity, DllImport(DllName, EntryPoint=\"ReadQWORD\")] public static extern int ReadQWORD_f32(int vFunc, IntPtr pWbemClassObject, [In] Int32 lHandle, [Out] out UInt64 pqw);\r\n[SuppressUnmanagedCodeSecurity, DllImport(DllName, EntryPoint=\"WriteQWORD\")] public static extern int WriteQWORD_f33(int vFunc, IntPtr pWbemClassObject, [In] Int32 lHandle, [In] UInt64 pw);\r\n[SuppressUnmanagedCodeSecurity, DllImport(DllName, EntryPoint=\"GetPropertyInfoByHandle\")] public static extern int GetPropertyInfoByHandle_f34(int vFunc, IntPtr pWbemClassObject, [In] Int32 lHandle, [Out][MarshalAs(UnmanagedType.BStr)]  out string   pstrName, [Out] out Int32 pType);\r\n[SuppressUnmanagedCodeSecurity, DllImport(DllName, EntryPoint=\"Lock\")] public static extern int Lock_f35(int vFunc, IntPtr pWbemClassObject, [In] Int32 lFlags);\r\n[SuppressUnmanagedCodeSecurity, DllImport(DllName, EntryPoint=\"Unlock\")] public static extern int Unlock_f36(int vFunc, IntPtr pWbemClassObject, [In] Int32 lFlags);\r\n\r\n[SuppressUnmanagedCodeSecurity, DllImport(DllName, EntryPoint=\"Put\")] public static extern int Put_f5(int vFunc, IntPtr pWbemClassObject, [In][MarshalAs(UnmanagedType.LPWStr)]  string   wszName, [In] Int32 lFlags, [In] ref object pVal, [In] Int32 Type);\r\n[SuppressUnmanagedCodeSecurity, DllImport(DllName, EntryPoint=\"WritePropertyValue\")] public static extern int WritePropertyValue_f28(int vFunc, IntPtr pWbemClassObject, [In] Int32 lHandle, [In] Int32 lNumBytes, [In][MarshalAs(UnmanagedType.LPWStr)] string str);\r\n[SuppressUnmanagedCodeSecurity, DllImport(DllName, EntryPoint=\"WritePropertyValue\")] public static extern int WritePropertyValue_f28(int vFunc, IntPtr pWbemClassObject, [In] Int32 lHandle, [In] Int32 lNumBytes, [In] ref Byte n);\r\n[SuppressUnmanagedCodeSecurity, DllImport(DllName, EntryPoint=\"WritePropertyValue\")] public static extern int WritePropertyValue_f28(int vFunc, IntPtr pWbemClassObject, [In] Int32 lHandle, [In] Int32 lNumBytes, [In] ref SByte n);\r\n[SuppressUnmanagedCodeSecurity, DllImport(DllName, EntryPoint=\"WritePropertyValue\")] public static extern int WritePropertyValue_f28(int vFunc, IntPtr pWbemClassObject, [In] Int32 lHandle, [In] Int32 lNumBytes, [In] ref Int16 n);\r\n[SuppressUnmanagedCodeSecurity, DllImport(DllName, EntryPoint=\"WritePropertyValue\")] public static extern int WritePropertyValue_f28(int vFunc, IntPtr pWbemClassObject, [In] Int32 lHandle, [In] Int32 lNumBytes, [In] ref UInt16 n);\r\n[SuppressUnmanagedCodeSecurity, DllImport(DllName, EntryPoint=\"WritePropertyValue\", CharSet=CharSet.Unicode)] public static extern int WritePropertyValue_f28(int vFunc, IntPtr pWbemClassObject, [In] Int32 lHandle, [In] Int32 lNumBytes, [In] ref Char c);\r\n[SuppressUnmanagedCodeSecurity, DllImport(DllName, EntryPoint=\"WriteDWORD\")] public static extern int WriteDWORD_f31(int vFunc, IntPtr pWbemClassObject, [In] Int32 lHandle, [In] Int32 dw);\r\n[SuppressUnmanagedCodeSecurity, DllImport(DllName, EntryPoint=\"WriteSingle\")] public static extern int WriteDWORD_f31(int vFunc, IntPtr pWbemClassObject, [In] Int32 lHandle, [In] Single dw);\r\n[SuppressUnmanagedCodeSecurity, DllImport(DllName, EntryPoint=\"WriteQWORD\")] public static extern int WriteQWORD_f33(int vFunc, IntPtr pWbemClassObject, [In] Int32 lHandle, [In] Int64 pw);\r\n[SuppressUnmanagedCodeSecurity, DllImport(DllName, EntryPoint=\"WriteDouble\")] public static extern int WriteQWORD_f33(int vFunc, IntPtr pWbemClassObject, [In] Int32 lHandle, [In] Double pw);\r\n[SuppressUnmanagedCodeSecurity, DllImport(DllName, EntryPoint=\"Clone\")] public static extern int Clone_f(int vFunc, IntPtr pWbemClassObject, [Out] out IntPtr ppCopy);\r\n}\r\ninterface IWmiConverter\r\n{\r\n    void ToWMI(object obj);\r\n    ManagementObject GetInstance();\r\n}\r\nclass SafeAssign\r\n{\r\n    public static UInt16 boolTrue = 0xffff;\r\n    public static UInt16 boolFalse = 0;\r\n    static Hashtable validTypes = new Hashtable();\r\n    static SafeAssign()\r\n    {\r\n        validTypes.Add(typeof(SByte), null);\r\n        validTypes.Add(typeof(Byte), null);\r\n        validTypes.Add(typeof(Int16), null);\r\n        validTypes.Add(typeof(UInt16), null);\r\n        validTypes.Add(typeof(Int32), null);\r\n        validTypes.Add(typeof(UInt32), null);\r\n        validTypes.Add(typeof(Int64), null);\r\n        validTypes.Add(typeof(UInt64), null);\r\n        validTypes.Add(typeof(Single), null);\r\n        validTypes.Add(typeof(Double), null);\r\n        validTypes.Add(typeof(Boolean), null);\r\n        validTypes.Add(typeof(String), null);\r\n        validTypes.Add(typeof(Char), null);\r\n        validTypes.Add(typeof(DateTime), null);\r\n        validTypes.Add(typeof(TimeSpan), null);\r\n        validTypes.Add(typeof(ManagementObject), null);\r\n        nullClass.SystemProperties [\"__CLASS\"].Value = \"nullInstance\";\r\n    }\r\n    public static object GetInstance(object o)\r\n    {\r\n        if(o is ManagementObject)\r\n            return o;\r\n        return null;\r\n    }\r\n    static ManagementClass nullClass = new ManagementClass(new ManagementPath(@\"" + this.NamespaceName + "\"));\r\n    \r\n    public static ManagementObject GetManagementObject(object o)\r\n    {\r\n        if(o != null && o is ManagementObject)\r\n            return o as ManagementObject;\r\n        // Must return empty instance\r\n        return nullClass.CreateInstance();\r\n    }\r\n    public static object GetValue(object o)\r\n    {\r\n        Type t = o.GetType();\r\n        if(t.IsArray)\r\n            t = t.GetElementType();\r\n        if(validTypes.Contains(t))\r\n            return o;\r\n        return null;\r\n    }\r\n    public static string WMITimeToString(DateTime dt)\r\n    {\r\n        TimeSpan ts = dt.Subtract(dt.ToUniversalTime());\r\n        int diffUTC = (ts.Minutes + ts.Hours * 60);\r\n        if(diffUTC >= 0)\r\n            return String.Format(\"{0:D4}{1:D2}{2:D2}{3:D2}{4:D2}{5:D2}.{6:D3}000+{7:D3}\", dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, dt.Millisecond, diffUTC);\r\n        return String.Format(\"{0:D4}{1:D2}{2:D2}{3:D2}{4:D2}{5:D2}.{6:D3}000-{7:D3}\", dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, dt.Millisecond, -diffUTC);\r\n    }\r\n    public static string WMITimeToString(TimeSpan ts)\r\n    {\r\n        return String.Format(\"{0:D8}{1:D2}{2:D2}{3:D2}.{4:D3}000:000\", ts.Days, ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds);\r\n    }\r\n    public static string[] WMITimeArrayToStringArray(DateTime[] dates)\r\n    {\r\n        string[] strings = new string[dates.Length];\r\n        for(int i=0;i<dates.Length;i++)\r\n            strings[i] = WMITimeToString(dates[i]);\r\n        return strings;\r\n    }\r\n    public static string[] WMITimeArrayToStringArray(TimeSpan[] timeSpans)\r\n    {\r\n        string[] strings = new string[timeSpans.Length];\r\n        for(int i=0;i<timeSpans.Length;i++)\r\n            strings[i] = WMITimeToString(timeSpans[i]);\r\n        return strings;\r\n    }\r\n}\r\n");
            }
            using (StreamWriter writer5 = new StreamWriter(this.MofPath, false, Encoding.Unicode))
            {
                writer5.WriteLine(this.GenerateMof(mofs));
            }
            if (flag2)
            {
                RegisterSchemaUsingMofcomp(this.MofPath);
            }
        }

        private void RegisterNamespaceAsInstrumented()
        {
            ManagementObject obj2 = new ManagementClass(this.GlobalRegistrationClassPath).CreateInstance();
            obj2["NamespaceName"] = this.NamespaceName;
            obj2.Put();
        }

        public void RegisterNonAssemblySpecificSchema(InstallContext installContext)
        {
            SecurityHelper.UnmanagedCode.Demand();
            WmiNetUtilsHelper.VerifyClientKey_f();
            InstallLogWrapper context = new InstallLogWrapper(installContext);
            EnsureNamespace(context, this.GlobalRegistrationNamespace);
            EnsureClassExists(context, this.GlobalInstrumentationClassPath, new ClassMaker(this.MakeGlobalInstrumentationClass));
            EnsureClassExists(context, this.GlobalRegistrationClassPath, new ClassMaker(this.MakeNamespaceRegistrationClass));
            EnsureClassExists(context, this.GlobalNamingClassPath, new ClassMaker(this.MakeNamingClass));
            EnsureNamespace(context, this.NamespaceName);
            EnsureClassExists(context, this.InstrumentationClassPath, new ClassMaker(this.MakeInstrumentationClass));
            EnsureClassExists(context, this.RegistrationClassPath, new ClassMaker(this.MakeRegistrationClass));
            try
            {
                ManagementClass class2 = new ManagementClass(this.DecoupledProviderClassPath);
                if (class2["HostingModel"].ToString() != "Decoupled:Com")
                {
                    class2.Delete();
                }
            }
            catch (ManagementException exception)
            {
                if (exception.ErrorCode != ManagementStatus.NotFound)
                {
                    throw exception;
                }
            }
            EnsureClassExists(context, this.DecoupledProviderClassPath, new ClassMaker(this.MakeDecoupledProviderClass));
            EnsureClassExists(context, this.ProviderClassPath, new ClassMaker(this.MakeProviderClass));
            if (!DoesInstanceExist(this.GlobalRegistrationPath))
            {
                this.RegisterNamespaceAsInstrumented();
            }
        }

        private string RegisterProviderAsEventProvider(StringCollection events)
        {
            ManagementObject obj2 = new ManagementClass(this.EventProviderRegistrationClassPath).CreateInstance();
            obj2["provider"] = @"\\.\" + this.ProviderPath;
            string[] strArray = new string[events.Count];
            int num = 0;
            foreach (string str in events)
            {
                strArray[num++] = "select * from " + str;
            }
            obj2["EventQueryList"] = strArray;
            return obj2.Put().Path;
        }

        private string RegisterProviderAsInstanceProvider()
        {
            ManagementObject obj2 = new ManagementClass(this.InstanceProviderRegistrationClassPath).CreateInstance();
            obj2["provider"] = @"\\.\" + this.ProviderPath;
            obj2["SupportsGet"] = true;
            obj2["SupportsEnumeration"] = true;
            return obj2.Put().Path;
        }

        private static void RegisterSchemaUsingMofcomp(string mofPath)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo {
                Arguments = mofPath,
                FileName = WMICapabilities.InstallationDirectory + @"\mofcomp.exe",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };
            Process.Start(startInfo).WaitForExit();
        }

        private static void ReplaceClassIfNecessary(string classPath, ManagementClass newClass)
        {
            try
            {
                ManagementClass class2 = SafeGetClass(classPath);
                if (class2 == null)
                {
                    newClass.Put();
                }
                else if (newClass.GetText(TextFormat.Mof) != class2.GetText(TextFormat.Mof))
                {
                    class2.Delete();
                    newClass.Put();
                }
            }
            catch (ManagementException exception)
            {
                throw new ArgumentException(string.Format(RC.GetString("CLASS_NOTREPLACED_EXCEPT") + "\r\n{0}\r\n{1}", classPath, newClass.GetText(TextFormat.Mof)), exception);
            }
        }

        private static ManagementClass SafeGetClass(string classPath)
        {
            ManagementClass class2 = null;
            try
            {
                ManagementClass class3 = new ManagementClass(classPath);
                class3.Get();
                class2 = class3;
            }
            catch (ManagementException exception)
            {
                if (exception.ErrorCode != ManagementStatus.NotFound)
                {
                    throw exception;
                }
            }
            return class2;
        }

        private string AssemblyName
        {
            get
            {
                return this.assemblyInfo.AssemblyName;
            }
        }

        private string AssemblyPath
        {
            get
            {
                return this.assemblyInfo.AssemblyPath;
            }
        }

        private string AssemblyUniqueIdentifier
        {
            get
            {
                return this.assemblyInfo.AssemblyUniqueIdentifier;
            }
        }

        public string Code
        {
            get
            {
                using (StreamReader reader = new StreamReader(this.CodePath))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        private string CodePath
        {
            get
            {
                return Path.Combine(this.DataDirectory, this.DecoupledProviderInstanceName + ".cs");
            }
        }

        private string DataDirectory
        {
            get
            {
                return Path.Combine(WMICapabilities.FrameworkDirectory, this.NamespaceName);
            }
        }

        private string DecoupledProviderClassPath
        {
            get
            {
                return MakeClassPath(this.assemblyInfo.NamespaceName, "MSFT_DecoupledProvider");
            }
        }

        public string DecoupledProviderInstanceName
        {
            get
            {
                return this.assemblyInfo.DecoupledProviderInstanceName;
            }
            set
            {
                this.assemblyInfo.DecoupledProviderInstanceName = value;
            }
        }

        private string EventProviderRegistrationClassPath
        {
            get
            {
                return MakeClassPath(this.assemblyInfo.NamespaceName, "__EventProviderRegistration");
            }
        }

        private string EventProviderRegistrationPath
        {
            get
            {
                return AppendProperty(this.EventProviderRegistrationClassPath, "provider", @"\\\\.\\" + this.ProviderPath.Replace(@"\", @"\\").Replace("\"", "\\\""));
            }
        }

        private string GlobalInstrumentationClassPath
        {
            get
            {
                return MakeClassPath(@"root\MicrosoftWmiNet", "WMINET_Instrumentation");
            }
        }

        private string GlobalNamingClassPath
        {
            get
            {
                return MakeClassPath(@"root\MicrosoftWmiNet", "WMINET_Naming");
            }
        }

        private string GlobalRegistrationClassPath
        {
            get
            {
                return MakeClassPath(@"root\MicrosoftWmiNet", "WMINET_InstrumentedNamespaces");
            }
        }

        private string GlobalRegistrationNamespace
        {
            get
            {
                return @"root\MicrosoftWmiNet";
            }
        }

        private string GlobalRegistrationPath
        {
            get
            {
                return AppendProperty(this.GlobalRegistrationClassPath, "NamespaceName", this.assemblyInfo.NamespaceName.Replace(@"\", @"\\"));
            }
        }

        private string InstanceProviderRegistrationClassPath
        {
            get
            {
                return MakeClassPath(this.assemblyInfo.NamespaceName, "__InstanceProviderRegistration");
            }
        }

        private string InstanceProviderRegistrationPath
        {
            get
            {
                return AppendProperty(this.InstanceProviderRegistrationClassPath, "provider", @"\\\\.\\" + this.ProviderPath.Replace(@"\", @"\\").Replace("\"", "\\\""));
            }
        }

        private string InstrumentationClassPath
        {
            get
            {
                return MakeClassPath(this.assemblyInfo.NamespaceName, "WMINET_Instrumentation");
            }
        }

        public string Mof
        {
            get
            {
                using (StreamReader reader = new StreamReader(this.MofPath))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        private string MofPath
        {
            get
            {
                return Path.Combine(this.DataDirectory, this.DecoupledProviderInstanceName + ".mof");
            }
        }

        public string NamespaceName
        {
            get
            {
                return this.assemblyInfo.NamespaceName;
            }
        }

        public Assembly PrecompiledAssembly
        {
            get
            {
                if (File.Exists(this.PrecompiledAssemblyPath))
                {
                    return Assembly.LoadFrom(this.PrecompiledAssemblyPath);
                }
                return null;
            }
        }

        private string PrecompiledAssemblyPath
        {
            get
            {
                return Path.Combine(this.DataDirectory, this.DecoupledProviderInstanceName + ".dll");
            }
        }

        private string ProviderClassPath
        {
            get
            {
                return MakeClassPath(this.assemblyInfo.NamespaceName, "WMINET_ManagedAssemblyProvider");
            }
        }

        private string ProviderPath
        {
            get
            {
                return AppendProperty(this.ProviderClassPath, "Name", this.assemblyInfo.DecoupledProviderInstanceName);
            }
        }

        private string RegistrationClassPath
        {
            get
            {
                return MakeClassPath(this.assemblyInfo.NamespaceName, "WMINET_InstrumentedAssembly");
            }
        }

        private ManagementObject RegistrationInstance
        {
            get
            {
                if (this.registrationInstance == null)
                {
                    this.registrationInstance = new ManagementObject(this.RegistrationPath);
                }
                return this.registrationInstance;
            }
        }

        private string RegistrationPath
        {
            get
            {
                return AppendProperty(this.RegistrationClassPath, "Name", this.assemblyInfo.DecoupledProviderInstanceName);
            }
        }

        public string SecurityDescriptor
        {
            get
            {
                return this.assemblyInfo.SecurityDescriptor;
            }
        }

        private string Win32ProviderClassPath
        {
            get
            {
                return MakeClassPath(this.assemblyInfo.NamespaceName, "__Win32Provider");
            }
        }

        private class AssemblySpecificNaming
        {
            private string assemblyName;
            private string assemblyPath;
            private string assemblyUniqueIdentifier;
            private string decoupledProviderInstanceName;
            private string namespaceName;
            private string securityDescriptor;

            public AssemblySpecificNaming(string namespaceName, string securityDescriptor, Assembly assembly)
            {
                this.namespaceName = namespaceName;
                this.securityDescriptor = securityDescriptor;
                this.decoupledProviderInstanceName = AssemblyNameUtility.UniqueToAssemblyFullVersion(assembly);
                this.assemblyUniqueIdentifier = AssemblyNameUtility.UniqueToAssemblyBuild(assembly);
                this.assemblyName = assembly.FullName;
                this.assemblyPath = assembly.Location;
            }

            public string AssemblyName
            {
                get
                {
                    return this.assemblyName;
                }
            }

            public string AssemblyPath
            {
                get
                {
                    return this.assemblyPath;
                }
            }

            public string AssemblyUniqueIdentifier
            {
                get
                {
                    return this.assemblyUniqueIdentifier;
                }
            }

            public string DecoupledProviderInstanceName
            {
                get
                {
                    return this.decoupledProviderInstanceName;
                }
                set
                {
                    this.decoupledProviderInstanceName = value;
                }
            }

            public string NamespaceName
            {
                get
                {
                    return this.namespaceName;
                }
            }

            public string SecurityDescriptor
            {
                get
                {
                    return this.securityDescriptor;
                }
            }
        }

        private delegate ManagementClass ClassMaker();

        private class InstallLogWrapper
        {
            private InstallContext context;

            public InstallLogWrapper(InstallContext context)
            {
                this.context = context;
            }

            public void LogMessage(string str)
            {
                if (this.context != null)
                {
                    this.context.LogMessage(str);
                }
            }
        }
    }
}

