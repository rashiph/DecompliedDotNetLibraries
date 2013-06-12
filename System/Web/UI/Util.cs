namespace System.Web.UI
{
    using System;
    using System.CodeDom.Compiler;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Text;
    using System.Web;
    using System.Web.Compilation;
    using System.Web.Configuration;
    using System.Web.Hosting;
    using System.Web.Security;
    using System.Web.UI.HtmlControls;
    using System.Web.UI.WebControls;
    using System.Web.Util;

    internal static class Util
    {
        internal const char DeviceFilterSeparator = ':';
        private static char[] invalidFileNameChars = new char[] { '/', '\\', '?', '*', ':' };
        private static string[] s_invalidCultureNames = new string[] { "aspx", "ascx", "master" };
        internal const string XmlnsAttribute = "xmlns:";

        internal static void AddAssembliesToStringCollection(ICollection fromList, StringCollection toList)
        {
            if ((fromList != null) && (toList != null))
            {
                foreach (Assembly assembly in fromList)
                {
                    AddAssemblyToStringCollection(assembly, toList);
                }
            }
        }

        internal static void AddAssemblyToStringCollection(Assembly assembly, StringCollection toList)
        {
            string path = null;
            if (!MultiTargetingUtil.EnableReferenceAssemblyResolution)
            {
                path = GetAssemblyCodeBase(assembly);
            }
            else if (AssemblyResolver.GetPathToReferenceAssembly(assembly, out path) == ReferenceAssemblyType.FrameworkAssemblyOnlyPresentInHigherVersion)
            {
                return;
            }
            if (!toList.Contains(path))
            {
                toList.Add(path);
            }
        }

        internal static bool CanConvertToFrom(TypeConverter converter, Type type)
        {
            return ((((converter != null) && converter.CanConvertTo(type)) && converter.CanConvertFrom(type)) && !(converter is ReferenceConverter));
        }

        internal static void CheckAssignableType(Type baseType, Type type)
        {
            if (!baseType.IsAssignableFrom(type))
            {
                throw new HttpException(System.Web.SR.GetString("Type_doesnt_inherit_from_type", new object[] { type.FullName, baseType.FullName }));
            }
        }

        internal static void CheckThemeAttribute(string themeName)
        {
            if (themeName.Length > 0)
            {
                if (!FileUtil.IsValidDirectoryName(themeName))
                {
                    throw new HttpException(System.Web.SR.GetString("Page_theme_invalid_name", new object[] { themeName }));
                }
                if (!ThemeExists(themeName))
                {
                    throw new HttpException(System.Web.SR.GetString("Page_theme_not_found", new object[] { themeName }));
                }
            }
        }

        internal static void CheckUnknownDirectiveAttributes(string directiveName, IDictionary directive)
        {
            CheckUnknownDirectiveAttributes(directiveName, directive, "Attr_not_supported_in_directive");
        }

        internal static void CheckUnknownDirectiveAttributes(string directiveName, IDictionary directive, string resourceKey)
        {
            if (directive.Count > 0)
            {
                throw new HttpException(System.Web.SR.GetString(resourceKey, new object[] { FirstDictionaryKey(directive), directiveName }));
            }
        }

        internal static void CheckVirtualFileExists(VirtualPath virtualPath)
        {
            if (!virtualPath.FileExists())
            {
                throw new HttpException(0x194, System.Web.SR.GetString("FileName_does_not_exist", new object[] { virtualPath.VirtualPathString }));
            }
        }

        internal static void ClearReadOnlyAttribute(string path)
        {
            FileAttributes attributes = File.GetAttributes(path);
            if ((attributes & FileAttributes.ReadOnly) != 0)
            {
                File.SetAttributes(path, attributes & ~FileAttributes.ReadOnly);
            }
        }

        internal static int CommaIndexInTypeName(string typeName)
        {
            int num = typeName.LastIndexOf(',');
            if (num < 0)
            {
                return -1;
            }
            int num2 = typeName.LastIndexOf(']');
            if (num2 > num)
            {
                return -1;
            }
            return typeName.IndexOf(',', num2 + 1);
        }

        internal static bool ContainsWhiteSpace(string s)
        {
            for (int i = s.Length - 1; i >= 0; i--)
            {
                if (char.IsWhiteSpace(s[i]))
                {
                    return true;
                }
            }
            return false;
        }

        internal static void CopyBaseAttributesToInnerControl(WebControl control, WebControl child)
        {
            short tabIndex = control.TabIndex;
            string accessKey = control.AccessKey;
            try
            {
                control.AccessKey = string.Empty;
                control.TabIndex = 0;
                child.CopyBaseAttributes(control);
            }
            finally
            {
                control.TabIndex = tabIndex;
                control.AccessKey = accessKey;
            }
        }

        public static string CreateFilteredName(string deviceName, string name)
        {
            if (deviceName.Length > 0)
            {
                return (deviceName + ':' + name);
            }
            return name;
        }

        internal static void DeleteFileIfExistsNoException(string path)
        {
            if (File.Exists(path))
            {
                DeleteFileNoException(path);
            }
        }

        internal static void DeleteFileNoException(string path)
        {
            try
            {
                File.Delete(path);
            }
            catch
            {
            }
        }

        [SecurityPermission(SecurityAction.Assert, Flags=SecurityPermissionFlag.SerializationFormatter)]
        internal static object DeserializeWithAssert(IStateFormatter formatter, string serializedState)
        {
            return formatter.Deserialize(serializedState);
        }

        internal static string EnsureEndWithSemiColon(string value)
        {
            if (value != null)
            {
                int length = value.Length;
                if ((length > 0) && (value[length - 1] != ';'))
                {
                    return (value + ";");
                }
            }
            return value;
        }

        internal static string FilePathFromFileUrl(string url)
        {
            Uri uri = new Uri(url);
            return HttpUtility.UrlDecode(uri.LocalPath);
        }

        private static string FirstDictionaryKey(IDictionary dict)
        {
            IDictionaryEnumerator enumerator = dict.GetEnumerator();
            enumerator.MoveNext();
            return (string) enumerator.Key;
        }

        internal static int FirstNonWhiteSpaceIndex(string s)
        {
            for (int i = 0; i < s.Length; i++)
            {
                if (!char.IsWhiteSpace(s[i]))
                {
                    return i;
                }
            }
            return -1;
        }

        private static string GetAndRemove(IDictionary dict, string key)
        {
            string str = (string) dict[key];
            if (str != null)
            {
                dict.Remove(key);
                str = str.Trim();
            }
            return str;
        }

        internal static bool GetAndRemoveBooleanAttribute(IDictionary directives, string key, ref bool val)
        {
            string andRemove = GetAndRemove(directives, key);
            if (andRemove == null)
            {
                return false;
            }
            val = GetBooleanAttribute(key, andRemove);
            return true;
        }

        internal static object GetAndRemoveEnumAttribute(IDictionary directives, Type enumType, string key)
        {
            string andRemove = GetAndRemove(directives, key);
            if (andRemove == null)
            {
                return null;
            }
            return GetEnumAttribute(key, andRemove, enumType);
        }

        internal static string GetAndRemoveNonEmptyAttribute(IDictionary directives, string key)
        {
            return GetAndRemoveNonEmptyAttribute(directives, key, false);
        }

        internal static string GetAndRemoveNonEmptyAttribute(IDictionary directives, string key, bool required)
        {
            string andRemove = GetAndRemove(directives, key);
            if (andRemove != null)
            {
                return GetNonEmptyAttribute(key, andRemove);
            }
            if (required)
            {
                throw new HttpException(System.Web.SR.GetString("Missing_attr", new object[] { key }));
            }
            return null;
        }

        internal static string GetAndRemoveNonEmptyIdentifierAttribute(IDictionary directives, string key, bool required)
        {
            string str = GetAndRemoveNonEmptyNoSpaceAttribute(directives, key, required);
            if (str == null)
            {
                return null;
            }
            return GetNonEmptyIdentifierAttribute(key, str);
        }

        internal static string GetAndRemoveNonEmptyNoSpaceAttribute(IDictionary directives, string key)
        {
            return GetAndRemoveNonEmptyNoSpaceAttribute(directives, key, false);
        }

        internal static string GetAndRemoveNonEmptyNoSpaceAttribute(IDictionary directives, string key, bool required)
        {
            string str = GetAndRemoveNonEmptyAttribute(directives, key, required);
            if (str == null)
            {
                return null;
            }
            return GetNonEmptyNoSpaceAttribute(key, str);
        }

        internal static bool GetAndRemoveNonNegativeIntegerAttribute(IDictionary directives, string key, ref int val)
        {
            string andRemove = GetAndRemove(directives, key);
            if (andRemove == null)
            {
                return false;
            }
            val = GetNonNegativeIntegerAttribute(key, andRemove);
            return true;
        }

        internal static bool GetAndRemovePositiveIntegerAttribute(IDictionary directives, string key, ref int val)
        {
            string andRemove = GetAndRemove(directives, key);
            if (andRemove == null)
            {
                return false;
            }
            try
            {
                val = int.Parse(andRemove, CultureInfo.InvariantCulture);
            }
            catch
            {
                throw new HttpException(System.Web.SR.GetString("Invalid_positive_integer_attribute", new object[] { key }));
            }
            if (val <= 0)
            {
                throw new HttpException(System.Web.SR.GetString("Invalid_positive_integer_attribute", new object[] { key }));
            }
            return true;
        }

        internal static string GetAndRemoveRequiredAttribute(IDictionary directives, string key)
        {
            return GetAndRemoveNonEmptyAttribute(directives, key, true);
        }

        internal static VirtualPath GetAndRemoveVirtualPathAttribute(IDictionary directives, string key)
        {
            return GetAndRemoveVirtualPathAttribute(directives, key, false);
        }

        internal static VirtualPath GetAndRemoveVirtualPathAttribute(IDictionary directives, string key, bool required)
        {
            string virtualPath = GetAndRemoveNonEmptyAttribute(directives, key, required);
            if (virtualPath == null)
            {
                return null;
            }
            return VirtualPath.Create(virtualPath);
        }

        [FileIOPermission(SecurityAction.Assert, AllFiles=FileIOPermissionAccess.PathDiscovery)]
        internal static string GetAssemblyCodeBase(Assembly assembly)
        {
            string location = assembly.Location;
            if (string.IsNullOrEmpty(location))
            {
                return null;
            }
            return location;
        }

        internal static string GetAssemblyNameFromFileName(string fileName)
        {
            if (StringUtil.EqualsIgnoreCase(Path.GetExtension(fileName), ".dll"))
            {
                return fileName.Substring(0, fileName.Length - 4);
            }
            return fileName;
        }

        internal static string GetAssemblyPathFromType(Type t)
        {
            return FilePathFromFileUrl(t.Assembly.EscapedCodeBase);
        }

        [FileIOPermission(SecurityAction.Assert, AllFiles=FileIOPermissionAccess.PathDiscovery)]
        internal static string GetAssemblyQualifiedTypeName(Type t)
        {
            if (t.Assembly.GlobalAssemblyCache)
            {
                return t.AssemblyQualifiedName;
            }
            return (t.FullName + ", " + t.Assembly.GetName().Name);
        }

        internal static string GetAssemblySafePathFromType(Type t)
        {
            return HttpRuntime.GetSafePath(GetAssemblyPathFromType(t));
        }

        internal static string GetAssemblyShortName(Assembly a)
        {
            InternalSecurityPermissions.Unrestricted.Assert();
            return a.GetName().Name;
        }

        internal static bool GetBooleanAttribute(string name, string value)
        {
            bool flag;
            try
            {
                flag = bool.Parse(value);
            }
            catch
            {
                throw new HttpException(System.Web.SR.GetString("Invalid_boolean_attribute", new object[] { name }));
            }
            return flag;
        }

        internal static string GetClientValidatedPostback(Control control, string validationGroup)
        {
            return GetClientValidatedPostback(control, validationGroup, string.Empty);
        }

        internal static string GetClientValidatedPostback(Control control, string validationGroup, string argument)
        {
            string str = control.Page.ClientScript.GetPostBackEventReference(control, argument, true);
            return (GetClientValidateEvent(validationGroup) + str);
        }

        internal static string GetClientValidateEvent(string validationGroup)
        {
            if (validationGroup == null)
            {
                validationGroup = string.Empty;
            }
            return ("if (typeof(Page_ClientValidate) == 'function') Page_ClientValidate('" + validationGroup + "'); ");
        }

        internal static string GetCultureName(string virtualPath)
        {
            if (virtualPath == null)
            {
                return null;
            }
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(virtualPath);
            if (fileNameWithoutExtension == null)
            {
                return null;
            }
            int num = fileNameWithoutExtension.LastIndexOf('.');
            if (num < 0)
            {
                return null;
            }
            string s = fileNameWithoutExtension.Substring(num + 1);
            if (!IsCultureName(s))
            {
                return null;
            }
            return s;
        }

        internal static string GetCurrentAccountName()
        {
            try
            {
                return HttpApplication.GetCurrentWindowsIdentityWithAssert().Name;
            }
            catch
            {
                return "?";
            }
        }

        internal static Encoding GetEncodingFromConfigPath(VirtualPath configPath)
        {
            Encoding fileEncoding = null;
            fileEncoding = RuntimeConfig.GetConfig(configPath).Globalization.FileEncoding;
            if (fileEncoding == null)
            {
                fileEncoding = Encoding.Default;
            }
            return fileEncoding;
        }

        internal static object GetEnumAttribute(string name, string value, Type enumType)
        {
            return GetEnumAttribute(name, value, enumType, false);
        }

        internal static object GetEnumAttribute(string name, string value, Type enumType, bool allowMultiple)
        {
            object obj2;
            try
            {
                if ((char.IsDigit(value[0]) || (value[0] == '-')) || (!allowMultiple && (value.IndexOf(',') >= 0)))
                {
                    throw new FormatException(System.Web.SR.GetString("EnumAttributeInvalidString", new object[] { value, name, enumType.FullName }));
                }
                obj2 = Enum.Parse(enumType, value, true);
            }
            catch
            {
                string str = null;
                foreach (string str2 in Enum.GetNames(enumType))
                {
                    if (str == null)
                    {
                        str = str2;
                    }
                    else
                    {
                        str = str + ", " + str2;
                    }
                }
                throw new HttpException(System.Web.SR.GetString("Invalid_enum_attribute", new object[] { name, str }));
            }
            return obj2;
        }

        internal static string GetNamespaceAndTypeNameFromVirtualPath(VirtualPath virtualPath, int chunksToIgnore, out string typeName)
        {
            string fileName = virtualPath.FileName;
            string[] strArray = fileName.Split(new char[] { '.' });
            int num = strArray.Length - chunksToIgnore;
            if (IsWhiteSpaceString(strArray[num - 1]))
            {
                throw new HttpException(System.Web.SR.GetString("Unsupported_filename", new object[] { fileName }));
            }
            typeName = MakeValidTypeNameFromString(strArray[num - 1]);
            for (int i = 0; i < (num - 1); i++)
            {
                if (IsWhiteSpaceString(strArray[i]))
                {
                    throw new HttpException(System.Web.SR.GetString("Unsupported_filename", new object[] { fileName }));
                }
                strArray[i] = MakeValidTypeNameFromString(strArray[i]);
            }
            return string.Join(".", strArray, 0, num - 1);
        }

        internal static string GetNamespaceFromVirtualPath(VirtualPath virtualPath)
        {
            string str;
            return GetNamespaceAndTypeNameFromVirtualPath(virtualPath, 1, out str);
        }

        internal static string GetNonEmptyAttribute(string name, string value)
        {
            value = value.Trim();
            if (value.Length == 0)
            {
                throw new HttpException(System.Web.SR.GetString("Empty_attribute", new object[] { name }));
            }
            return value;
        }

        internal static string GetNonEmptyFullClassNameAttribute(string name, string value, ref string ns)
        {
            value = GetNonEmptyNoSpaceAttribute(name, value);
            string[] strArray = value.Split(new char[] { '.' });
            foreach (string str in strArray)
            {
                if (!CodeGenerator.IsValidLanguageIndependentIdentifier(str))
                {
                    throw new HttpException(System.Web.SR.GetString("Invalid_attribute_value", new object[] { value, name }));
                }
            }
            if (strArray.Length > 1)
            {
                ns = string.Join(".", strArray, 0, strArray.Length - 1);
            }
            return strArray[strArray.Length - 1];
        }

        internal static string GetNonEmptyIdentifierAttribute(string name, string value)
        {
            value = GetNonEmptyNoSpaceAttribute(name, value);
            if (!CodeGenerator.IsValidLanguageIndependentIdentifier(value))
            {
                throw new HttpException(System.Web.SR.GetString("Invalid_attribute_value", new object[] { value, name }));
            }
            return value;
        }

        internal static string GetNonEmptyNoSpaceAttribute(string name, string value)
        {
            value = GetNonEmptyAttribute(name, value);
            return GetNoSpaceAttribute(name, value);
        }

        internal static int GetNonNegativeIntegerAttribute(string name, string value)
        {
            int num;
            try
            {
                num = int.Parse(value, CultureInfo.InvariantCulture);
            }
            catch
            {
                throw new HttpException(System.Web.SR.GetString("Invalid_nonnegative_integer_attribute", new object[] { name }));
            }
            if (num < 0)
            {
                throw new HttpException(System.Web.SR.GetString("Invalid_nonnegative_integer_attribute", new object[] { name }));
            }
            return num;
        }

        internal static Type GetNonPrivateFieldType(Type classType, string fieldName)
        {
            FieldInfo field = classType.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if ((field != null) && !field.IsPrivate)
            {
                return field.FieldType;
            }
            return null;
        }

        internal static Type GetNonPrivatePropertyType(Type classType, string propName)
        {
            PropertyInfo property = null;
            BindingFlags bindingAttr = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase;
            try
            {
                property = classType.GetProperty(propName, bindingAttr);
            }
            catch (AmbiguousMatchException)
            {
                bindingAttr |= BindingFlags.DeclaredOnly;
                property = classType.GetProperty(propName, bindingAttr);
            }
            if (property != null)
            {
                MethodInfo setMethod = property.GetSetMethod(true);
                if ((setMethod != null) && !setMethod.IsPrivate)
                {
                    return property.PropertyType;
                }
            }
            return null;
        }

        internal static string GetNoSpaceAttribute(string name, string value)
        {
            if (ContainsWhiteSpace(value))
            {
                throw new HttpException(System.Web.SR.GetString("Space_attribute", new object[] { name }));
            }
            return value;
        }

        internal static long GetRecompilationHash(PagesSection ps)
        {
            HashCodeCombiner combiner = new HashCodeCombiner();
            combiner.AddObject(ps.Buffer);
            combiner.AddObject(ps.EnableViewState);
            combiner.AddObject(ps.EnableViewStateMac);
            combiner.AddObject(ps.EnableEventValidation);
            combiner.AddObject(ps.SmartNavigation);
            combiner.AddObject(ps.ValidateRequest);
            combiner.AddObject(ps.AutoEventWireup);
            if (ps.PageBaseTypeInternal != null)
            {
                combiner.AddObject(ps.PageBaseTypeInternal.FullName);
            }
            if (ps.UserControlBaseTypeInternal != null)
            {
                combiner.AddObject(ps.UserControlBaseTypeInternal.FullName);
            }
            if (ps.PageParserFilterTypeInternal != null)
            {
                combiner.AddObject(ps.PageParserFilterTypeInternal.FullName);
            }
            combiner.AddObject(ps.MasterPageFile);
            combiner.AddObject(ps.Theme);
            combiner.AddObject(ps.StyleSheetTheme);
            combiner.AddObject(ps.EnableSessionState);
            combiner.AddObject(ps.CompilationMode);
            combiner.AddObject(ps.MaxPageStateFieldLength);
            combiner.AddObject(ps.ViewStateEncryptionMode);
            combiner.AddObject(ps.MaintainScrollPositionOnPostBack);
            NamespaceCollection namespaces = ps.Namespaces;
            combiner.AddObject(namespaces.AutoImportVBNamespace);
            if (namespaces.Count == 0)
            {
                combiner.AddObject("__clearnamespaces");
            }
            else
            {
                foreach (NamespaceInfo info in namespaces)
                {
                    combiner.AddObject(info.Namespace);
                }
            }
            TagPrefixCollection controls = ps.Controls;
            if (controls.Count == 0)
            {
                combiner.AddObject("__clearcontrols");
            }
            else
            {
                foreach (TagPrefixInfo info2 in controls)
                {
                    combiner.AddObject(info2.TagPrefix);
                    if ((info2.TagName != null) && (info2.TagName.Length != 0))
                    {
                        combiner.AddObject(info2.TagName);
                        combiner.AddObject(info2.Source);
                    }
                    else
                    {
                        combiner.AddObject(info2.Namespace);
                        combiner.AddObject(info2.Assembly);
                    }
                }
            }
            TagMapCollection tagMapping = ps.TagMapping;
            if (tagMapping.Count == 0)
            {
                combiner.AddObject("__cleartagmapping");
            }
            else
            {
                foreach (TagMapInfo info3 in tagMapping)
                {
                    combiner.AddObject(info3.TagType);
                    combiner.AddObject(info3.MappedTagType);
                }
            }
            return combiner.CombinedHash;
        }

        internal static AssemblySet GetReferencedAssemblies(Assembly a)
        {
            AssemblySet set = new AssemblySet();
            foreach (AssemblyName name in a.GetReferencedAssemblies())
            {
                Assembly o = Assembly.Load(name);
                if (!(o == typeof(string).Assembly))
                {
                    set.Add(o);
                }
            }
            return set;
        }

        internal static VirtualPath GetScriptLocation()
        {
            string format = (string) RuntimeConfig.GetAppConfig().WebControls["clientScriptsLocation"];
            if (format.IndexOf("{0}", StringComparison.Ordinal) >= 0)
            {
                string str2 = "system_web";
                string str3 = VersionInfo.EngineVersion.Substring(0, VersionInfo.EngineVersion.LastIndexOf('.')).Replace('.', '_');
                format = string.Format(CultureInfo.InvariantCulture, format, new object[] { str2, str3 });
            }
            return VirtualPath.Create(format);
        }

        private static ArrayList GetSpecificCultures(string shortName)
        {
            CultureInfo[] cultures = CultureInfo.GetCultures(CultureTypes.SpecificCultures);
            ArrayList list = new ArrayList();
            for (int i = 0; i < cultures.Length; i++)
            {
                if (StringUtil.StringStartsWith(cultures[i].Name, shortName))
                {
                    list.Add(cultures[i]);
                }
            }
            return list;
        }

        internal static string GetSpecificCulturesFormattedList(CultureInfo cultureInfo)
        {
            ArrayList specificCultures = GetSpecificCultures(cultureInfo.Name);
            string name = null;
            foreach (CultureInfo info in specificCultures)
            {
                if (name == null)
                {
                    name = info.Name;
                }
                else
                {
                    name = name + ", " + info.Name;
                }
            }
            return name;
        }

        internal static string GetStringFromBool(bool flag)
        {
            if (!flag)
            {
                return "false";
            }
            return "true";
        }

        [ReflectionPermission(SecurityAction.Assert, Flags=ReflectionPermissionFlag.MemberAccess)]
        internal static Type GetTypeFromAssemblies(IEnumerable assemblies, string typeName, bool ignoreCase)
        {
            if (assemblies == null)
            {
                return null;
            }
            Type t = null;
            foreach (Assembly assembly in assemblies)
            {
                Type type2 = assembly.GetType(typeName, false, ignoreCase);
                if (type2 != null)
                {
                    if ((t != null) && (type2 != t))
                    {
                        throw new HttpException(System.Web.SR.GetString("Ambiguous_type", new object[] { typeName, GetAssemblySafePathFromType(t), GetAssemblySafePathFromType(type2) }));
                    }
                    t = type2;
                }
            }
            return t;
        }

        internal static bool HasWriteAccessToDirectory(string dir)
        {
            if (!Directory.Exists(dir))
            {
                return false;
            }
            string path = Path.Combine(dir, string.Concat(new object[] { "~AspAccessCheck_", HostingEnvironment.AppDomainUniqueInteger.ToString("x", CultureInfo.InvariantCulture), SafeNativeMethods.GetCurrentThreadId(), ".tmp" }));
            FileStream stream = null;
            bool flag = false;
            try
            {
                stream = new FileStream(path, FileMode.Create);
            }
            catch
            {
            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                    File.Delete(path);
                    flag = true;
                }
            }
            return flag;
        }

        internal static object InvokeMethod(MethodInfo methodInfo, object obj, object[] parameters)
        {
            object obj2;
            try
            {
                obj2 = methodInfo.Invoke(obj, parameters);
            }
            catch (TargetInvocationException exception)
            {
                throw exception.InnerException;
            }
            return obj2;
        }

        internal static bool IsCultureName(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return false;
            }
            foreach (string str in s_invalidCultureNames)
            {
                if (StringUtil.EqualsIgnoreCase(str, s))
                {
                    return false;
                }
            }
            CultureInfo info = null;
            try
            {
                info = HttpServerUtility.CreateReadOnlyCultureInfo(s);
            }
            catch
            {
            }
            return (info != null);
        }

        internal static bool IsFalseString(string s)
        {
            return ((s != null) && StringUtil.EqualsIgnoreCase(s, "false"));
        }

        internal static bool IsLateBoundComClassicType(Type t)
        {
            return (string.Compare(t.FullName, "System.__ComObject", StringComparison.Ordinal) == 0);
        }

        internal static bool IsMultiInstanceTemplateProperty(PropertyInfo pInfo)
        {
            object[] customAttributes = pInfo.GetCustomAttributes(typeof(TemplateInstanceAttribute), false);
            if ((customAttributes != null) && (customAttributes.Length != 0))
            {
                return (((TemplateInstanceAttribute) customAttributes[0]).Instances == TemplateInstance.Multiple);
            }
            return true;
        }

        internal static bool IsNonEmptyDirectory(string dir)
        {
            if (!Directory.Exists(dir))
            {
                return false;
            }
            try
            {
                return (Directory.GetFileSystemEntries(dir).Length > 0);
            }
            catch
            {
                return true;
            }
        }

        internal static bool IsTrueString(string s)
        {
            return ((s != null) && StringUtil.EqualsIgnoreCase(s, "true"));
        }

        internal static bool IsUserAllowedToPath(HttpContext context, VirtualPath virtualPath)
        {
            if (FileAuthorizationModule.IsWindowsIdentity(context))
            {
                if (HttpRuntime.IsFullTrust)
                {
                    if (!IsUserAllowedToPathWithNoAssert(context, virtualPath))
                    {
                        return false;
                    }
                }
                else if (!IsUserAllowedToPathWithAssert(context, virtualPath))
                {
                    return false;
                }
            }
            return UrlAuthorizationModule.IsUserAllowedToPath(context, virtualPath);
        }

        [FileIOPermission(SecurityAction.Assert, Unrestricted=true)]
        private static bool IsUserAllowedToPathWithAssert(HttpContext context, VirtualPath virtualPath)
        {
            return IsUserAllowedToPathWithNoAssert(context, virtualPath);
        }

        private static bool IsUserAllowedToPathWithNoAssert(HttpContext context, VirtualPath virtualPath)
        {
            return FileAuthorizationModule.IsUserAllowedToPath(context, virtualPath);
        }

        internal static bool IsValidFileName(string fileName)
        {
            if ((fileName == ".") || (fileName == ".."))
            {
                return false;
            }
            if (fileName.IndexOfAny(invalidFileNameChars) >= 0)
            {
                return false;
            }
            return true;
        }

        internal static bool IsWhiteSpaceString(string s)
        {
            return (s.Trim().Length == 0);
        }

        internal static int LineCount(string text, int offset, int newoffset)
        {
            int num = 0;
            while (offset < newoffset)
            {
                if ((text[offset] == '\r') || ((text[offset] == '\n') && ((offset == 0) || (text[offset - 1] != '\r'))))
                {
                    num++;
                }
                offset++;
            }
            return num;
        }

        internal static string MakeFullTypeName(string ns, string typeName)
        {
            if (string.IsNullOrEmpty(ns))
            {
                return typeName;
            }
            return (ns + "." + typeName);
        }

        internal static string MakeValidFileName(string fileName)
        {
            if (!IsValidFileName(fileName))
            {
                for (int i = 0; i < invalidFileNameChars.Length; i++)
                {
                    fileName = fileName.Replace(invalidFileNameChars[i], '_');
                }
            }
            return fileName;
        }

        internal static string MakeValidTypeNameFromString(string s)
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < s.Length; i++)
            {
                if ((i == 0) && char.IsDigit(s[0]))
                {
                    builder.Append('_');
                }
                if (char.IsLetterOrDigit(s[i]))
                {
                    builder.Append(s[i]);
                }
                else
                {
                    builder.Append('_');
                }
            }
            return builder.ToString();
        }

        internal static string MergeScript(string firstScript, string secondScript)
        {
            if (!string.IsNullOrEmpty(firstScript))
            {
                return (firstScript + secondScript);
            }
            if (secondScript.TrimStart(new char[0]).StartsWith("javascript:", StringComparison.Ordinal))
            {
                return secondScript;
            }
            return ("javascript:" + secondScript);
        }

        public static string ParsePropertyDeviceFilter(string input, out string propName)
        {
            string str = string.Empty;
            if (input.IndexOf(':') < 0)
            {
                propName = input;
                return str;
            }
            if (StringUtil.StringStartsWithIgnoreCase(input, "xmlns:"))
            {
                propName = input;
                return str;
            }
            string[] strArray = input.Split(new char[] { ':' });
            if (strArray.Length > 2)
            {
                throw new HttpException(System.Web.SR.GetString("Too_many_filters", new object[] { input }));
            }
            if (MTConfigUtil.GetPagesConfig().IgnoreDeviceFilters[strArray[0]] != null)
            {
                propName = input;
                return str;
            }
            str = strArray[0];
            propName = strArray[1];
            return str;
        }

        internal static string QuoteJScriptString(string value)
        {
            return QuoteJScriptString(value, false);
        }

        internal static string QuoteJScriptString(string value, bool forUrl)
        {
            StringBuilder builder = null;
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }
            int startIndex = 0;
            int count = 0;
            for (int i = 0; i < value.Length; i++)
            {
                switch (value[i])
                {
                    case '%':
                    {
                        if (!forUrl)
                        {
                            break;
                        }
                        if (builder == null)
                        {
                            builder = new StringBuilder(value.Length + 6);
                        }
                        if (count > 0)
                        {
                            builder.Append(value, startIndex, count);
                        }
                        builder.Append("%25");
                        startIndex = i + 1;
                        count = 0;
                        continue;
                    }
                    case '\'':
                    {
                        if (builder == null)
                        {
                            builder = new StringBuilder(value.Length + 5);
                        }
                        if (count > 0)
                        {
                            builder.Append(value, startIndex, count);
                        }
                        builder.Append(@"\'");
                        startIndex = i + 1;
                        count = 0;
                        continue;
                    }
                    case '\\':
                    {
                        if (builder == null)
                        {
                            builder = new StringBuilder(value.Length + 5);
                        }
                        if (count > 0)
                        {
                            builder.Append(value, startIndex, count);
                        }
                        builder.Append(@"\\");
                        startIndex = i + 1;
                        count = 0;
                        continue;
                    }
                    case '\t':
                    {
                        if (builder == null)
                        {
                            builder = new StringBuilder(value.Length + 5);
                        }
                        if (count > 0)
                        {
                            builder.Append(value, startIndex, count);
                        }
                        builder.Append(@"\t");
                        startIndex = i + 1;
                        count = 0;
                        continue;
                    }
                    case '\n':
                    {
                        if (builder == null)
                        {
                            builder = new StringBuilder(value.Length + 5);
                        }
                        if (count > 0)
                        {
                            builder.Append(value, startIndex, count);
                        }
                        builder.Append(@"\n");
                        startIndex = i + 1;
                        count = 0;
                        continue;
                    }
                    case '\r':
                    {
                        if (builder == null)
                        {
                            builder = new StringBuilder(value.Length + 5);
                        }
                        if (count > 0)
                        {
                            builder.Append(value, startIndex, count);
                        }
                        builder.Append(@"\r");
                        startIndex = i + 1;
                        count = 0;
                        continue;
                    }
                    case '"':
                    {
                        if (builder == null)
                        {
                            builder = new StringBuilder(value.Length + 5);
                        }
                        if (count > 0)
                        {
                            builder.Append(value, startIndex, count);
                        }
                        builder.Append("\\\"");
                        startIndex = i + 1;
                        count = 0;
                        continue;
                    }
                }
                count++;
            }
            if (builder == null)
            {
                return value;
            }
            if (count > 0)
            {
                builder.Append(value, startIndex, count);
            }
            return builder.ToString();
        }

        internal static StreamReader ReaderFromFile(string filename, VirtualPath configPath)
        {
            StreamReader reader;
            Encoding encodingFromConfigPath = Encoding.Default;
            if (configPath != null)
            {
                encodingFromConfigPath = GetEncodingFromConfigPath(configPath);
            }
            try
            {
                reader = new StreamReader(filename, encodingFromConfigPath, true, 0x1000);
            }
            catch (UnauthorizedAccessException)
            {
                if (FileUtil.DirectoryExists(filename))
                {
                    throw new HttpException(System.Web.SR.GetString("Unexpected_Directory", new object[] { HttpRuntime.GetSafePath(filename) }));
                }
                throw;
            }
            return reader;
        }

        internal static StreamReader ReaderFromStream(Stream stream, VirtualPath configPath)
        {
            return new StreamReader(stream, GetEncodingFromConfigPath(configPath), true, 0x1000);
        }

        internal static bool RemoveOrRenameFile(FileInfo f)
        {
            try
            {
                f.Delete();
                return true;
            }
            catch
            {
                try
                {
                    if (f.Extension != ".delete")
                    {
                        string str = DateTime.Now.Ticks.GetHashCode().ToString("x", CultureInfo.InvariantCulture);
                        string destFileName = f.FullName + "." + str + ".delete";
                        f.MoveTo(destFileName);
                    }
                }
                catch
                {
                }
            }
            return false;
        }

        internal static void RemoveOrRenameFile(string filename)
        {
            FileInfo f = new FileInfo(filename);
            RemoveOrRenameFile(f);
        }

        [SecurityPermission(SecurityAction.Assert, Flags=SecurityPermissionFlag.SerializationFormatter)]
        internal static string SerializeWithAssert(IStateFormatter formatter, object stateGraph)
        {
            return formatter.Serialize(stateGraph);
        }

        internal static string StringFromFile(string path)
        {
            Encoding encoding = Encoding.Default;
            return StringFromFile(path, ref encoding);
        }

        internal static string StringFromFile(string path, ref Encoding encoding)
        {
            string str2;
            StreamReader reader = new StreamReader(path, encoding, true);
            try
            {
                string str = reader.ReadToEnd();
                encoding = reader.CurrentEncoding;
                str2 = str;
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                }
            }
            return str2;
        }

        internal static string StringFromFileIfExists(string path)
        {
            if (!File.Exists(path))
            {
                return null;
            }
            return StringFromFile(path);
        }

        internal static string StringFromVirtualPath(VirtualPath virtualPath)
        {
            using (Stream stream = virtualPath.OpenFile())
            {
                return ReaderFromStream(stream, virtualPath).ReadToEnd();
            }
        }

        internal static bool ThemeExists(string themeName)
        {
            if (!VirtualDirectoryExistsWithAssert(ThemeDirectoryCompiler.GetAppThemeVirtualDir(themeName)) && !VirtualDirectoryExistsWithAssert(ThemeDirectoryCompiler.GetGlobalThemeVirtualDir(themeName)))
            {
                return false;
            }
            return true;
        }

        internal static bool TypeNameContainsAssembly(string typeName)
        {
            return (CommaIndexInTypeName(typeName) > 0);
        }

        private static bool VirtualDirectoryExistsWithAssert(VirtualPath virtualDir)
        {
            try
            {
                string path = virtualDir.MapPathInternal();
                if (path != null)
                {
                    new FileIOPermission(FileIOPermissionAccess.Read, path).Assert();
                }
                return virtualDir.DirectoryExists();
            }
            catch
            {
                return false;
            }
        }

        internal static bool VirtualFileExistsWithAssert(VirtualPath virtualPath)
        {
            string path = virtualPath.MapPathInternal();
            if (path != null)
            {
                InternalSecurityPermissions.PathDiscovery(path).Assert();
            }
            return virtualPath.FileExists();
        }

        internal static void WriteOnClickAttribute(HtmlTextWriter writer, HtmlControl control, bool submitsAutomatically, bool submitsProgramatically, bool causesValidation, string validationGroup)
        {
            System.Web.UI.AttributeCollection attributes = control.Attributes;
            string clientValidateEvent = null;
            if (submitsAutomatically)
            {
                if (causesValidation)
                {
                    clientValidateEvent = GetClientValidateEvent(validationGroup);
                }
                control.Page.ClientScript.RegisterForEventValidation(control.UniqueID);
            }
            else if (submitsProgramatically)
            {
                if (causesValidation)
                {
                    clientValidateEvent = GetClientValidatedPostback(control, validationGroup);
                }
                else
                {
                    clientValidateEvent = control.Page.ClientScript.GetPostBackEventReference(control, string.Empty, true);
                }
            }
            else
            {
                control.Page.ClientScript.RegisterForEventValidation(control.UniqueID);
            }
            if (clientValidateEvent != null)
            {
                string str2 = attributes["onclick"];
                if (str2 != null)
                {
                    attributes.Remove("onclick");
                    writer.WriteAttribute("onclick", str2 + " " + clientValidateEvent);
                }
                else
                {
                    writer.WriteAttribute("onclick", clientValidateEvent);
                }
            }
        }
    }
}

