namespace System.Workflow.Activities.Common
{
    using Microsoft.Win32;
    using System;
    using System.CodeDom;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Windows.Forms;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Compiler;
    using System.Workflow.ComponentModel.Design;
    using System.Workflow.ComponentModel.Serialization;
    using System.Xml;

    internal static class Helpers
    {
        internal const int FILENAME_MAX = 260;
        private const string INSTALLPROPERTY_INSTALLLOCATION = "InstallLocation";
        private const string ProductCode = "{B644FB52-BB3D-4C43-80EC-57644210536A}";
        internal static readonly string ProductInstallDirectory = GetInstallDirectory(false);
        internal static readonly string ProductInstallDirectory30 = GetInstallDirectory30();
        internal static readonly string ProductRootRegKey = @"SOFTWARE\Microsoft\Net Framework Setup\NDP\v4.0\Setup\Windows Workflow Foundation";
        private static readonly string ProductRootRegKey30 = @"SOFTWARE\Microsoft\Net Framework Setup\NDP\v3.0\Setup\Windows Workflow Foundation";
        private const string ProductSDKCode = "{C8A7718A-FF6D-4DDC-AE36-BBF968D6799B}";
        internal static readonly string ProductSDKInstallDirectory = GetInstallDirectory(true);
        internal static readonly string TypeProviderAssemblyRegValueName = "References";
        private static readonly string VSExtensionProductRegistrySubKey = "Visual Studio Ext for Windows Workflow";

        internal static void AddTypeProviderAssembliesFromRegistry(TypeProvider typeProvider, IServiceProvider serviceProvider)
        {
            if (typeProvider == null)
            {
                throw new ArgumentNullException("typeProvider");
            }
            if (serviceProvider == null)
            {
                throw new ArgumentNullException("serviceProvider");
            }
            RegistryKey key = Registry.CurrentUser.OpenSubKey(TypeProviderRegistryKeyPath);
            if (key != null)
            {
                ITypeProviderCreator service = serviceProvider.GetService(typeof(ITypeProviderCreator)) as ITypeProviderCreator;
                foreach (string str in (string[]) key.GetValue(TypeProviderAssemblyRegValueName))
                {
                    try
                    {
                        if (service != null)
                        {
                            bool flag = true;
                            Assembly transientAssembly = service.GetTransientAssembly(AssemblyName.GetAssemblyName(str));
                            if (transientAssembly == null)
                            {
                                continue;
                            }
                            System.Type[] types = transientAssembly.GetTypes();
                            int index = 0;
                            while (index < types.Length)
                            {
                                System.Type type = types[index];
                                if (typeProvider.GetType(type.AssemblyQualifiedName) != null)
                                {
                                    flag = false;
                                }
                                break;
                            }
                            if (flag)
                            {
                                typeProvider.AddAssembly(transientAssembly);
                            }
                            continue;
                        }
                        typeProvider.AddAssemblyReference(str);
                    }
                    catch
                    {
                    }
                }
                key.Close();
            }
        }

        internal static bool AreAllActivities(ICollection c)
        {
            if (c == null)
            {
                throw new ArgumentNullException("c");
            }
            foreach (object obj2 in c)
            {
                if (!(obj2 is Activity))
                {
                    return false;
                }
            }
            return true;
        }

        internal static XmlWriter CreateXmlWriter(object output)
        {
            XmlWriterSettings settings = new XmlWriterSettings {
                Indent = true,
                IndentChars = "\t",
                OmitXmlDeclaration = true,
                CloseOutput = true
            };
            if (output is string)
            {
                return XmlWriter.Create(output as string, settings);
            }
            if (output is TextWriter)
            {
                return XmlWriter.Create(output as TextWriter, settings);
            }
            return null;
        }

        internal static void DeserializeDesignersFromStream(ICollection activities, Stream stateStream)
        {
            if (stateStream.Length != 0L)
            {
                BinaryReader reader = new BinaryReader(stateStream);
                stateStream.Seek(0L, SeekOrigin.Begin);
                Queue<IComponent> queue = new Queue<IComponent>();
                foreach (IComponent component in activities)
                {
                    queue.Enqueue(component);
                }
                while (queue.Count > 0)
                {
                    IComponent component2 = queue.Dequeue();
                    if ((component2 != null) && (component2.Site != null))
                    {
                        IDesignerHost service = component2.Site.GetService(typeof(IDesignerHost)) as IDesignerHost;
                        if (service == null)
                        {
                            throw new InvalidOperationException(SR.GetString("General_MissingService", new object[] { typeof(IDesignerHost).Name }));
                        }
                        ActivityDesigner designer = service.GetDesigner(component2) as ActivityDesigner;
                        if (designer != null)
                        {
                            try
                            {
                                ((IPersistUIState) designer).LoadViewState(reader);
                                CompositeActivity activity = component2 as CompositeActivity;
                                if (activity != null)
                                {
                                    foreach (IComponent component3 in activity.Activities)
                                    {
                                        queue.Enqueue(component3);
                                    }
                                }
                            }
                            catch
                            {
                            }
                        }
                    }
                }
            }
        }

        internal static AccessTypes GetAccessType(PropertyInfo property, object owner, IServiceProvider serviceProvider)
        {
            if (owner == null)
            {
                throw new ArgumentNullException("owner");
            }
            if (serviceProvider == null)
            {
                throw new ArgumentNullException("serviceProvider");
            }
            if (property != null)
            {
                IDynamicPropertyTypeProvider provider = owner as IDynamicPropertyTypeProvider;
                if (provider != null)
                {
                    return provider.GetAccessType(serviceProvider, property.Name);
                }
            }
            return AccessTypes.Read;
        }

        private static Activity GetActivity(Activity containerActivity, string id)
        {
            if (containerActivity != null)
            {
                Queue queue = new Queue();
                queue.Enqueue(containerActivity);
                while (queue.Count > 0)
                {
                    Activity activity = (Activity) queue.Dequeue();
                    if (activity.Enabled)
                    {
                        if (activity.Name == id)
                        {
                            return activity;
                        }
                        if (activity is CompositeActivity)
                        {
                            foreach (Activity activity2 in ((CompositeActivity) activity).Activities)
                            {
                                queue.Enqueue(activity2);
                            }
                        }
                    }
                }
            }
            return null;
        }

        public static IList<Activity> GetAllEnabledActivities(CompositeActivity compositeActivity)
        {
            if (compositeActivity == null)
            {
                throw new ArgumentNullException("compositeActivity");
            }
            List<Activity> list = new List<Activity>(compositeActivity.EnabledActivities);
            foreach (Activity activity in compositeActivity.Activities)
            {
                if (activity.Enabled && IsFrameworkActivity(activity))
                {
                    list.Add(activity);
                }
            }
            return list;
        }

        internal static Activity[] GetAllNestedActivities(CompositeActivity compositeActivity)
        {
            if (compositeActivity == null)
            {
                throw new ArgumentNullException("compositeActivity");
            }
            ArrayList list = new ArrayList();
            Queue queue = new Queue();
            queue.Enqueue(compositeActivity);
            while (queue.Count > 0)
            {
                CompositeActivity activity = (CompositeActivity) queue.Dequeue();
                if ((activity == compositeActivity) || !IsCustomActivity(activity))
                {
                    foreach (Activity activity2 in activity.Activities)
                    {
                        list.Add(activity2);
                        if (activity2 is CompositeActivity)
                        {
                            queue.Enqueue(activity2);
                        }
                    }
                    foreach (Activity activity3 in activity.EnabledActivities)
                    {
                        if (!list.Contains(activity3))
                        {
                            list.Add(activity3);
                            if (activity3 is CompositeActivity)
                            {
                                queue.Enqueue(activity3);
                            }
                        }
                    }
                }
            }
            return (Activity[]) list.ToArray(typeof(Activity));
        }

        internal static T GetAttributeFromObject<T>(object attributeObject) where T: Attribute
        {
            if (attributeObject is AttributeInfoAttribute)
            {
                return (T) ((AttributeInfoAttribute) attributeObject).AttributeInfo.CreateAttribute();
            }
            if (attributeObject is T)
            {
                return (T) attributeObject;
            }
            return default(T);
        }

        internal static string GetBaseIdentifier(Activity activity)
        {
            string name = activity.GetType().Name;
            StringBuilder builder = new StringBuilder(name.Length);
            for (int i = 0; i < name.Length; i++)
            {
                if (char.IsUpper(name[i]) && (((i == 0) || (i == (name.Length - 1))) || char.IsUpper(name[i + 1])))
                {
                    builder.Append(char.ToLowerInvariant(name[i]));
                }
                else
                {
                    builder.Append(name.Substring(i));
                    break;
                }
            }
            return builder.ToString();
        }

        internal static System.Type GetBaseType(PropertyInfo property, object owner, IServiceProvider serviceProvider)
        {
            if (owner == null)
            {
                throw new ArgumentNullException("owner");
            }
            if (serviceProvider == null)
            {
                throw new ArgumentNullException("serviceProvider");
            }
            if (property == null)
            {
                return null;
            }
            IDynamicPropertyTypeProvider provider = owner as IDynamicPropertyTypeProvider;
            if (provider != null)
            {
                System.Type propertyType = provider.GetPropertyType(serviceProvider, property.Name);
                if (propertyType != null)
                {
                    return propertyType;
                }
            }
            return property.PropertyType;
        }

        internal static string GetClassName(string fullQualifiedName)
        {
            if (fullQualifiedName == null)
            {
                return null;
            }
            string str = fullQualifiedName;
            int num = fullQualifiedName.LastIndexOf('.');
            if (num != -1)
            {
                str = fullQualifiedName.Substring(num + 1);
            }
            return str;
        }

        internal static CodeTypeDeclaration GetCodeNamespaceAndClass(CodeNamespaceCollection namespaces, string namespaceName, string className, out CodeNamespace codeNamespace)
        {
            codeNamespace = null;
            foreach (CodeNamespace namespace2 in namespaces)
            {
                if (namespace2.Name == namespaceName)
                {
                    codeNamespace = namespace2;
                    break;
                }
            }
            if (codeNamespace != null)
            {
                foreach (CodeTypeDeclaration declaration2 in codeNamespace.Types)
                {
                    if (declaration2.Name == className)
                    {
                        return declaration2;
                    }
                }
            }
            return null;
        }

        internal static Activity GetDataSourceActivity(Activity activity, string inputName, out string name)
        {
            if (activity == null)
            {
                throw new ArgumentNullException("activity");
            }
            if (string.IsNullOrEmpty(inputName))
            {
                throw new ArgumentException("inputName");
            }
            name = inputName;
            if (inputName.IndexOf('.') == -1)
            {
                return activity;
            }
            int length = inputName.LastIndexOf('.');
            string activityName = inputName.Substring(0, length);
            name = inputName.Substring(length + 1);
            Activity activity2 = ParseActivityForBind(activity, activityName);
            if (activity2 == null)
            {
                activity2 = ParseActivity(GetRootActivity(activity), activityName);
            }
            return activity2;
        }

        internal static System.Type GetDataSourceClass(Activity activity, IServiceProvider serviceProvider)
        {
            if (activity == null)
            {
                throw new ArgumentNullException("activity");
            }
            if (serviceProvider == null)
            {
                throw new ArgumentNullException("serviceProvider");
            }
            string str = null;
            if (activity == GetRootActivity(activity))
            {
                str = activity.GetValue(WorkflowMarkupSerializer.XClassProperty) as string;
            }
            if (string.IsNullOrEmpty(str))
            {
                return activity.GetType();
            }
            ITypeProvider service = (ITypeProvider) serviceProvider.GetService(typeof(ITypeProvider));
            if (service == null)
            {
                throw new InvalidOperationException(SR.GetString("General_MissingService", new object[] { typeof(ITypeProvider).Name }));
            }
            return service.GetType(str);
        }

        internal static CompositeActivity GetDeclaringActivity(Activity activity)
        {
            if (activity == null)
            {
                throw new ArgumentNullException("activity");
            }
            for (CompositeActivity activity2 = activity.Parent; activity2 != null; activity2 = activity2.Parent)
            {
                if (activity2.Parent == null)
                {
                    return activity2;
                }
                if (IsCustomActivity(activity2))
                {
                    return activity2;
                }
            }
            return null;
        }

        internal static System.Type GetDelegateFromEvent(EventInfo eventInfo)
        {
            if (eventInfo.EventHandlerType != null)
            {
                return eventInfo.EventHandlerType;
            }
            return TypeProvider.GetEventHandlerType(eventInfo);
        }

        internal static string GetDesignTimeTypeName(object owner, object key)
        {
            string str = null;
            DependencyObject obj2 = owner as DependencyObject;
            if (((obj2 != null) && (key != null)) && obj2.UserData.Contains(System.Workflow.Activities.Common.UserDataKeys.DesignTimeTypeNames))
            {
                Hashtable hashtable = obj2.UserData[System.Workflow.Activities.Common.UserDataKeys.DesignTimeTypeNames] as Hashtable;
                if ((hashtable != null) && hashtable.ContainsKey(key))
                {
                    str = hashtable[key] as string;
                }
            }
            return str;
        }

        internal static Activity GetEnclosingActivity(Activity activity)
        {
            if (IsActivityLocked(activity))
            {
                return GetDeclaringActivity(activity);
            }
            return GetRootActivity(activity);
        }

        internal static IList GetIdentifiersInCompositeActivity(CompositeActivity compositeActivity)
        {
            ArrayList list = new ArrayList();
            if (compositeActivity != null)
            {
                list.Add(compositeActivity.Name);
                foreach (Activity activity in GetAllNestedActivities(compositeActivity))
                {
                    list.Add(activity.Name);
                }
            }
            return ArrayList.ReadOnly(list);
        }

        private static string GetInstallDirectory(bool getSDKDir)
        {
            string directoryName = string.Empty;
            try
            {
                int capacity = 0x105;
                StringBuilder lpValueBuf = new StringBuilder(capacity);
                int num2 = MsiGetProductInfoW(getSDKDir ? "{C8A7718A-FF6D-4DDC-AE36-BBF968D6799B}" : "{B644FB52-BB3D-4C43-80EC-57644210536A}", "InstallLocation", lpValueBuf, ref capacity);
                Marshal.GetLastWin32Error();
                if (num2 == 0)
                {
                    directoryName = lpValueBuf.ToString();
                }
            }
            catch
            {
            }
            if (string.IsNullOrEmpty(directoryName))
            {
                try
                {
                    if (!getSDKDir)
                    {
                        using (RegistryKey key = Registry.LocalMachine.OpenSubKey(ProductRootRegKey))
                        {
                            if (key != null)
                            {
                                directoryName = (string) key.GetValue("InstallDir");
                            }
                        }
                    }
                }
                catch
                {
                }
            }
            if (string.IsNullOrEmpty(directoryName))
            {
                directoryName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            }
            return directoryName;
        }

        private static string GetInstallDirectory30()
        {
            string str = string.Empty;
            try
            {
                int capacity = 0x105;
                StringBuilder lpValueBuf = new StringBuilder(capacity);
                int num2 = MsiGetProductInfoW("{B644FB52-BB3D-4C43-80EC-57644210536A}", "InstallLocation", lpValueBuf, ref capacity);
                Marshal.GetLastWin32Error();
                if (num2 == 0)
                {
                    str = lpValueBuf.ToString();
                }
            }
            catch
            {
            }
            if (string.IsNullOrEmpty(str))
            {
                try
                {
                    using (RegistryKey key = Registry.LocalMachine.OpenSubKey(ProductRootRegKey30))
                    {
                        if (key != null)
                        {
                            str = (string) key.GetValue("InstallDir");
                        }
                    }
                }
                catch
                {
                }
            }
            return str;
        }

        internal static MethodInfo GetInterfaceMethod(System.Type interfaceType, string methodName)
        {
            MethodInfo info = null;
            string str = string.Empty;
            string name = string.Empty;
            if (methodName.LastIndexOf('.') > 0)
            {
                str = methodName.Substring(0, methodName.LastIndexOf('.'));
                name = methodName.Substring(methodName.LastIndexOf('.') + 1);
            }
            if (!string.IsNullOrEmpty(str))
            {
                foreach (System.Type type in interfaceType.GetInterfaces())
                {
                    if (string.Compare(type.FullName, str, StringComparison.Ordinal) == 0)
                    {
                        return type.GetMethod(name);
                    }
                }
                return info;
            }
            return interfaceType.GetMethod(methodName);
        }

        internal static MethodInfo GetMethodExactMatch(System.Type type, string name, BindingFlags bindingAttr, Binder binder, System.Type[] types, ParameterModifier[] modifiers)
        {
            foreach (MethodInfo info2 in type.GetMethods(bindingAttr))
            {
                if (((bindingAttr & BindingFlags.IgnoreCase) == BindingFlags.IgnoreCase) ? (string.Compare(info2.Name, name, StringComparison.OrdinalIgnoreCase) == 0) : (string.Compare(info2.Name, name, StringComparison.Ordinal) == 0))
                {
                    bool flag2 = false;
                    if (types != null)
                    {
                        ParameterInfo[] parameters = info2.GetParameters();
                        if (parameters.GetLength(0) == types.Length)
                        {
                            for (int i = 0; !flag2 && (i < parameters.Length); i++)
                            {
                                flag2 = (parameters[i].ParameterType == null) || !parameters[i].ParameterType.IsAssignableFrom(types[i]);
                            }
                        }
                        else
                        {
                            flag2 = true;
                        }
                    }
                    if (!flag2)
                    {
                        return info2;
                    }
                }
            }
            return null;
        }

        internal static void GetNamespaceAndClassName(string fullQualifiedName, out string namespaceName, out string className)
        {
            namespaceName = string.Empty;
            className = string.Empty;
            if (fullQualifiedName != null)
            {
                int length = fullQualifiedName.LastIndexOf('.');
                if (length != -1)
                {
                    namespaceName = fullQualifiedName.Substring(0, length);
                    className = fullQualifiedName.Substring(length + 1);
                }
                else
                {
                    className = fullQualifiedName;
                }
            }
        }

        internal static Activity[] GetNestedActivities(CompositeActivity compositeActivity)
        {
            if (compositeActivity == null)
            {
                throw new ArgumentNullException("compositeActivity");
            }
            ArrayList list2 = new ArrayList();
            Queue queue = new Queue();
            queue.Enqueue(compositeActivity);
            while (queue.Count > 0)
            {
                CompositeActivity activity = (CompositeActivity) queue.Dequeue();
                foreach (Activity activity2 in (IEnumerable<Activity>) activity.Activities)
                {
                    list2.Add(activity2);
                    if (activity2 is CompositeActivity)
                    {
                        queue.Enqueue(activity2);
                    }
                }
            }
            return (Activity[]) list2.ToArray(typeof(Activity));
        }

        internal static Activity GetRootActivity(Activity activity)
        {
            if (activity == null)
            {
                throw new ArgumentNullException("activity");
            }
            while (activity.Parent != null)
            {
                activity = activity.Parent;
            }
            return activity;
        }

        internal static string GetRootNamespace(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
            {
                throw new ArgumentNullException("serviceProvider");
            }
            string rootNamespace = string.Empty;
            IWorkflowCompilerOptionsService service = (IWorkflowCompilerOptionsService) serviceProvider.GetService(typeof(IWorkflowCompilerOptionsService));
            if ((service != null) && (service.RootNamespace != null))
            {
                rootNamespace = service.RootNamespace;
            }
            return rootNamespace;
        }

        private static Guid GetRuntimeContextGuid(Activity currentActivity)
        {
            Activity parent = currentActivity;
            Guid guid = (Guid) parent.GetValue(Activity.ActivityContextGuidProperty);
            while ((guid == Guid.Empty) && (parent.Parent != null))
            {
                parent = parent.Parent;
                guid = (Guid) parent.GetValue(Activity.ActivityContextGuidProperty);
            }
            return guid;
        }

        internal static DesignerSerializationVisibility GetSerializationVisibility(MemberInfo memberInfo)
        {
            if (memberInfo == null)
            {
                throw new ArgumentNullException("memberInfo");
            }
            DesignerSerializationVisibility visible = DesignerSerializationVisibility.Visible;
            object[] customAttributes = memberInfo.GetCustomAttributes(typeof(DesignerSerializationVisibilityAttribute), true);
            if (customAttributes.Length > 0)
            {
                return (customAttributes[0] as DesignerSerializationVisibilityAttribute).Visibility;
            }
            if (Attribute.IsDefined(memberInfo, typeof(DesignerSerializationVisibilityAttribute)))
            {
                visible = (Attribute.GetCustomAttribute(memberInfo, typeof(DesignerSerializationVisibilityAttribute)) as DesignerSerializationVisibilityAttribute).Visibility;
            }
            return visible;
        }

        internal static Activity[] GetTopLevelActivities(ICollection activities)
        {
            if (activities == null)
            {
                throw new ArgumentNullException("activities");
            }
            List<Activity> list = new List<Activity>();
            foreach (object obj2 in activities)
            {
                Activity item = obj2 as Activity;
                if (item != null)
                {
                    bool flag = false;
                    for (Activity activity2 = item.Parent; (activity2 != null) && !flag; activity2 = activity2.Parent)
                    {
                        foreach (object obj3 in activities)
                        {
                            if (obj3 == activity2)
                            {
                                flag = true;
                                break;
                            }
                        }
                    }
                    if (!flag)
                    {
                        list.Add(item);
                    }
                }
            }
            return list.ToArray();
        }

        internal static bool IsActivityLocked(Activity activity)
        {
            if (activity == null)
            {
                throw new ArgumentNullException("activity");
            }
            for (CompositeActivity activity2 = activity.Parent; activity2 != null; activity2 = activity2.Parent)
            {
                if (activity2.Parent == null)
                {
                    return false;
                }
                if (IsCustomActivity(activity2))
                {
                    return true;
                }
            }
            return false;
        }

        internal static bool IsAlternateFlowActivity(Activity activity)
        {
            if (activity == null)
            {
                return false;
            }
            bool flag = false;
            if (!activity.UserData.Contains(typeof(AlternateFlowActivityAttribute)))
            {
                flag = activity.GetType().GetCustomAttributes(typeof(AlternateFlowActivityAttribute), true).Length != 0;
                activity.UserData[typeof(AlternateFlowActivityAttribute)] = flag;
                return flag;
            }
            return (bool) activity.UserData[typeof(AlternateFlowActivityAttribute)];
        }

        internal static bool IsChildActivity(CompositeActivity parent, Activity activity)
        {
            foreach (Activity activity2 in parent.Activities)
            {
                if (activity == activity2)
                {
                    return true;
                }
                if ((activity2 is CompositeActivity) && IsChildActivity(activity2 as CompositeActivity, activity))
                {
                    return true;
                }
            }
            return false;
        }

        internal static bool IsCustomActivity(CompositeActivity compositeActivity)
        {
            if (compositeActivity == null)
            {
                throw new ArgumentNullException("compositeActivity");
            }
            if (compositeActivity.UserData.Contains(System.Workflow.Activities.Common.UserDataKeys.CustomActivity))
            {
                return (bool) compositeActivity.UserData[System.Workflow.Activities.Common.UserDataKeys.CustomActivity];
            }
            try
            {
                CompositeActivity activity = Activator.CreateInstance(compositeActivity.GetType()) as CompositeActivity;
                if ((activity != null) && (activity.Activities.Count > 0))
                {
                    compositeActivity.UserData[System.Workflow.Activities.Common.UserDataKeys.CustomActivityDefaultName] = activity.Name;
                    compositeActivity.UserData[System.Workflow.Activities.Common.UserDataKeys.CustomActivity] = true;
                    return true;
                }
            }
            catch
            {
            }
            compositeActivity.UserData[System.Workflow.Activities.Common.UserDataKeys.CustomActivity] = false;
            return false;
        }

        private static bool IsDeclaringActivityMatchesContext(Activity currentActivity, Activity context)
        {
            CompositeActivity compositeActivity = context as CompositeActivity;
            CompositeActivity declaringActivity = GetDeclaringActivity(currentActivity);
            if (IsActivityLocked(context) && ((compositeActivity == null) || !IsCustomActivity(compositeActivity)))
            {
                compositeActivity = GetDeclaringActivity(context);
            }
            return (compositeActivity == declaringActivity);
        }

        internal static bool IsFileNameValid(string fileName)
        {
            int num = Path.GetInvalidPathChars().GetLength(0) + 5;
            char[] array = new char[num];
            Path.GetInvalidPathChars().CopyTo(array, 0);
            array[num - 5] = ':';
            array[num - 4] = '?';
            array[num - 3] = '*';
            array[num - 2] = '/';
            array[num - 1] = '\\';
            return ((((fileName != null) && (fileName.Length != 0)) && (fileName.Length <= 260)) && (fileName.IndexOfAny(array) == -1));
        }

        public static bool IsFrameworkActivity(Activity activity)
        {
            return (((activity is CancellationHandlerActivity) || (activity is CompensationHandlerActivity)) || (activity is FaultHandlersActivity));
        }

        internal static string MergeNamespaces(string primaryNs, string secondaryNs)
        {
            string str = primaryNs;
            if ((secondaryNs != null) && (secondaryNs.Length > 0))
            {
                if ((str != null) && (str.Length > 0))
                {
                    str = str + "." + secondaryNs;
                }
                else
                {
                    str = secondaryNs;
                }
            }
            if (str == null)
            {
                str = string.Empty;
            }
            return str;
        }

        [DllImport("msi.dll", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        private static extern int MsiGetProductInfoW(string szProduct, string szProperty, StringBuilder lpValueBuf, ref int pcchValueBuf);
        internal static IDictionary PairUpCommonParentActivities(ICollection activities)
        {
            if (activities == null)
            {
                throw new ArgumentNullException("activities");
            }
            Hashtable hashtable = new Hashtable();
            foreach (Activity activity in activities)
            {
                if (activity.Parent != null)
                {
                    ArrayList list = (ArrayList) hashtable[activity.Parent];
                    if (list == null)
                    {
                        list = new ArrayList();
                        hashtable.Add(activity.Parent, list);
                    }
                    list.Add(activity);
                }
            }
            return hashtable;
        }

        internal static Activity ParseActivity(Activity parsingContext, string activityName)
        {
            if (parsingContext == null)
            {
                throw new ArgumentNullException("parsingContext");
            }
            if (activityName == null)
            {
                throw new ArgumentNullException("activityName");
            }
            string id = activityName;
            string str2 = string.Empty;
            int index = activityName.IndexOf(".");
            if (index != -1)
            {
                id = activityName.Substring(0, index);
                str2 = activityName.Substring(index + 1);
                if (str2.Length == 0)
                {
                    return null;
                }
            }
            Activity containerActivity = GetActivity(parsingContext, id);
            if (containerActivity == null)
            {
                return null;
            }
            if (str2.Length > 0)
            {
                if (!(containerActivity is CompositeActivity) || !IsCustomActivity(containerActivity as CompositeActivity))
                {
                    return null;
                }
                string[] strArray = str2.Split(new char[] { '.' });
                for (int i = 0; i < strArray.Length; i++)
                {
                    Activity activity = GetActivity(containerActivity, strArray[i]);
                    if ((activity == null) || !IsActivityLocked(activity))
                    {
                        return null;
                    }
                    CompositeActivity declaringActivity = GetDeclaringActivity(activity);
                    if (containerActivity != declaringActivity)
                    {
                        return null;
                    }
                    containerActivity = activity;
                }
                return containerActivity;
            }
            if (IsActivityLocked(containerActivity) && !IsDeclaringActivityMatchesContext(containerActivity, parsingContext))
            {
                return null;
            }
            return containerActivity;
        }

        internal static Activity ParseActivityForBind(Activity context, string activityName)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            if (activityName == null)
            {
                throw new ArgumentNullException("activityName");
            }
            if (string.Equals(activityName, "/Self", StringComparison.Ordinal))
            {
                return context;
            }
            if (activityName.StartsWith("/Parent", StringComparison.OrdinalIgnoreCase))
            {
                Activity activity = context;
                string[] strArray = activityName.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; (i < strArray.Length) && (activity != null); i++)
                {
                    activity = string.Equals(strArray[i].Trim(), "Parent", StringComparison.OrdinalIgnoreCase) ? activity.Parent : null;
                }
                return activity;
            }
            if (!IsActivityLocked(context))
            {
                Activity activityByName = null;
                Activity activity8 = context;
                bool flag = false;
                CompositeActivity compositeActivity = activity8 as CompositeActivity;
                if (((compositeActivity != null) && (activity8.Parent != null)) && IsCustomActivity(compositeActivity))
                {
                    flag = true;
                    activity8 = activity8.Parent;
                }
                while ((activityByName == null) && (activity8 != null))
                {
                    activityByName = activity8.GetActivityByName(activityName, true);
                    activity8 = activity8.Parent;
                }
                if (flag && (activityByName == null))
                {
                    activityByName = context.GetActivityByName(activityName, true);
                }
                if (activityByName != null)
                {
                    return activityByName;
                }
                return ParseActivity(GetRootActivity(context), activityName);
            }
            Activity activity2 = null;
            Activity declaringActivity = GetDeclaringActivity(context);
            Guid runtimeContextGuid = GetRuntimeContextGuid(context);
            Guid guid2 = GetRuntimeContextGuid(declaringActivity);
            Activity parsingContext = context;
            Activity parent = context.Parent;
            Guid guid3 = GetRuntimeContextGuid(parent);
            while ((activity2 == null) && (guid2 != runtimeContextGuid))
            {
                while ((parent != null) && (guid3 == runtimeContextGuid))
                {
                    parsingContext = parent;
                    parent = parent.Parent;
                    guid3 = GetRuntimeContextGuid(parent);
                }
                activity2 = ParseActivity(parsingContext, activityName);
                runtimeContextGuid = guid3;
            }
            if (activity2 == null)
            {
                activity2 = ParseActivity(declaringActivity, activityName);
            }
            if (activity2 == null)
            {
                if (!declaringActivity.UserData.Contains(System.Workflow.Activities.Common.UserDataKeys.CustomActivityDefaultName))
                {
                    Activity activity6 = Activator.CreateInstance(declaringActivity.GetType()) as Activity;
                    declaringActivity.UserData[System.Workflow.Activities.Common.UserDataKeys.CustomActivityDefaultName] = activity6.Name;
                }
                if (((string) declaringActivity.UserData[System.Workflow.Activities.Common.UserDataKeys.CustomActivityDefaultName]) == activityName)
                {
                    activity2 = declaringActivity;
                }
            }
            return activity2;
        }

        internal static Stream SerializeDesignersToStream(ICollection activities)
        {
            Stream output = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(output);
            Queue<IComponent> queue = new Queue<IComponent>();
            foreach (IComponent component in activities)
            {
                queue.Enqueue(component);
            }
            while (queue.Count > 0)
            {
                IComponent component2 = queue.Dequeue();
                if ((component2 != null) && (component2.Site != null))
                {
                    IDesignerHost service = component2.Site.GetService(typeof(IDesignerHost)) as IDesignerHost;
                    if (service == null)
                    {
                        throw new InvalidOperationException(SR.GetString("General_MissingService", new object[] { typeof(IDesignerHost).Name }));
                    }
                    ActivityDesigner designer = service.GetDesigner(component2) as ActivityDesigner;
                    if (designer != null)
                    {
                        try
                        {
                            ((IPersistUIState) designer).SaveViewState(writer);
                            CompositeActivity activity = component2 as CompositeActivity;
                            if (activity != null)
                            {
                                foreach (IComponent component3 in activity.Activities)
                                {
                                    queue.Enqueue(component3);
                                }
                            }
                        }
                        catch
                        {
                        }
                    }
                }
            }
            return output;
        }

        internal static void SetDesignTimeTypeName(object owner, object key, string value)
        {
            DependencyObject obj2 = owner as DependencyObject;
            if ((obj2 != null) && (key != null))
            {
                if (!obj2.UserData.Contains(System.Workflow.Activities.Common.UserDataKeys.DesignTimeTypeNames))
                {
                    obj2.UserData[System.Workflow.Activities.Common.UserDataKeys.DesignTimeTypeNames] = new Hashtable();
                }
                Hashtable hashtable = obj2.UserData[System.Workflow.Activities.Common.UserDataKeys.DesignTimeTypeNames] as Hashtable;
                hashtable[key] = value;
            }
        }

        internal static bool TypesEqual(CodeTypeReference typeLeft, CodeTypeReference typeRight)
        {
            if (typeLeft.ArrayRank != typeRight.ArrayRank)
            {
                return false;
            }
            if (!typeLeft.BaseType.Equals(typeRight.BaseType))
            {
                return false;
            }
            if (typeLeft.ArrayRank > 0)
            {
                return TypesEqual(typeLeft.ArrayElementType, typeRight.ArrayElementType);
            }
            return true;
        }

        internal static bool TypesEqual(CodeTypeReference typeLeft, System.Type typeRight)
        {
            if (typeRight.IsArray && (typeLeft.ArrayRank != typeRight.GetArrayRank()))
            {
                return false;
            }
            if (!typeLeft.BaseType.Equals(typeRight.FullName))
            {
                return false;
            }
            if (typeLeft.ArrayRank > 0)
            {
                return TypesEqual(typeLeft.ArrayElementType, typeRight.GetElementType());
            }
            return true;
        }

        internal static void UpdateTypeProviderAssembliesRegistry(string assemblyName)
        {
            RegistryKey key = Registry.CurrentUser.CreateSubKey(TypeProviderRegistryKeyPath);
            if (key != null)
            {
                try
                {
                    ArrayList list = null;
                    if (key.ValueCount > 0)
                    {
                        list = new ArrayList((string[]) key.GetValue(TypeProviderAssemblyRegValueName));
                    }
                    else
                    {
                        list = new ArrayList();
                    }
                    if (!list.Contains(assemblyName))
                    {
                        list.Add(assemblyName);
                        key.SetValue(TypeProviderAssemblyRegValueName, (string[]) list.ToArray(typeof(string)));
                    }
                }
                catch
                {
                }
                finally
                {
                    key.Close();
                }
            }
        }

        internal static string PerUserRegistryKey
        {
            get
            {
                string str = string.Empty;
                using (RegistryKey key = Application.UserAppDataRegistry)
                {
                    str = key.ToString().Substring(Registry.CurrentUser.ToString().Length + 1);
                    str = str.Substring(0, str.LastIndexOf(@"\"));
                    return (str + @"\" + VSExtensionProductRegistrySubKey);
                }
            }
        }

        private static string TypeProviderRegistryKeyPath
        {
            get
            {
                return (PerUserRegistryKey + @"\TypeProvider");
            }
        }
    }
}

