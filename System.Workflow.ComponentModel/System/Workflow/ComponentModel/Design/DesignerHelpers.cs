namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.CodeDom;
    using System.CodeDom.Compiler;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Resources;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Windows.Forms;
    using System.Windows.Forms.Design;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Compiler;

    internal static class DesignerHelpers
    {
        internal const string CodeActivityTypeRef = "System.Workflow.Activities.CodeActivity, System.Workflow.Activities, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35";
        internal const string DeclarativeRulesRef = "System.Workflow.Activities.Rules.RuleConditionReference, System.Workflow.Activities, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35";
        internal const string EventHandlersTypeRef = "System.Workflow.Activities.EventHandlersActivity, System.Workflow.Activities, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35";
        internal const string IfElseBranchTypeRef = "System.Workflow.Activities.IfElseBranchActivity, System.Workflow.Activities, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35";
        internal const string SequentialWorkflowTypeRef = "System.Workflow.Activities.SequentialWorkflowActivity, System.Workflow.Activities, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35";
        private static bool ShowingMenu = false;
        internal const string StateMachineWorkflowTypeRef = "System.Workflow.Activities.StateMachineWorkflowActivity, System.Workflow.Activities, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35";
        private static readonly string WorkflowDesignerSubKey = "Workflow Designer";

        internal static bool AreAssociatedDesignersMovable(ICollection components)
        {
            foreach (object obj2 in components)
            {
                Activity activity = obj2 as Activity;
                if (activity == null)
                {
                    System.Workflow.ComponentModel.Design.HitTestInfo info = obj2 as System.Workflow.ComponentModel.Design.HitTestInfo;
                    activity = ((info != null) && (info.AssociatedDesigner != null)) ? info.AssociatedDesigner.Activity : null;
                }
                ActivityDesigner designer = ActivityDesigner.GetDesigner(activity);
                if ((designer != null) && designer.IsLocked)
                {
                    return false;
                }
            }
            return true;
        }

        internal static bool AreComponentsRemovable(ICollection components)
        {
            if (components == null)
            {
                throw new ArgumentNullException("components");
            }
            foreach (object obj2 in components)
            {
                Activity activity = obj2 as Activity;
                ConnectorHitTestInfo info = obj2 as ConnectorHitTestInfo;
                if ((activity == null) && (info == null))
                {
                    return false;
                }
                if (activity != null)
                {
                    ActivityDesigner designer = ActivityDesigner.GetDesigner(activity);
                    if ((designer != null) && designer.IsLocked)
                    {
                        return false;
                    }
                }
                if ((info != null) && !(info.AssociatedDesigner is FreeformActivityDesigner))
                {
                    return false;
                }
            }
            return true;
        }

        internal static string CreateUniqueMethodName(IComponent component, string propName, System.Type delegateType)
        {
            IServiceProvider serviceProvider = component.Site;
            if (serviceProvider == null)
            {
                throw new ArgumentException("component");
            }
            IDesignerHost host = serviceProvider.GetService(typeof(IDesignerHost)) as IDesignerHost;
            if (host == null)
            {
                throw new InvalidOperationException(SR.GetString("General_MissingService", new object[] { typeof(IDesignerHost).FullName }));
            }
            if (((ITypeProvider) serviceProvider.GetService(typeof(ITypeProvider))) == null)
            {
                throw new InvalidOperationException(SR.GetString("General_MissingService", new object[] { typeof(ITypeProvider).FullName }));
            }
            string name = null;
            IReferenceService service = serviceProvider.GetService(typeof(IReferenceService)) as IReferenceService;
            if (service != null)
            {
                name = service.GetName(component);
            }
            else
            {
                ISite site = component.Site;
                if (site != null)
                {
                    name = site.Name;
                }
            }
            if (name == null)
            {
                name = component.GetType().Name;
            }
            name = (name.Replace('.', '_').Replace('/', '_') + "_" + propName).Replace('(', '_').Replace(')', '_').Replace(" ", "");
            DelegateTypeInfo info = new DelegateTypeInfo(delegateType);
            Activity rootComponent = host.RootComponent as Activity;
            if (rootComponent == null)
            {
                Activity activity2 = component as Activity;
                throw new InvalidOperationException(SR.GetString("Error_CantCreateMethod", new object[] { (activity2 != null) ? activity2.QualifiedName : string.Empty }));
            }
            System.Type dataSourceClass = Helpers.GetDataSourceClass(rootComponent, serviceProvider);
            if (dataSourceClass == null)
            {
                throw new InvalidOperationException(SR.GetString("Error_CantCreateMethod", new object[] { rootComponent.QualifiedName }));
            }
            BindingFlags bindingAttr = BindingFlags.FlattenHierarchy | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance;
            MethodInfo[] methods = dataSourceClass.GetMethods(bindingAttr);
            ArrayList list = new ArrayList();
            foreach (MethodInfo info2 in methods)
            {
                if (info2.GetParameters().Length != info.Parameters.Length)
                {
                    continue;
                }
                bool flag = true;
                for (int i = 0; i < info.Parameters.Length; i++)
                {
                    ParameterInfo info3 = info2.GetParameters()[i];
                    CodeParameterDeclarationExpression expression = info.Parameters[i];
                    FieldDirection direction = expression.Direction;
                    if ((((direction == FieldDirection.In) && !info3.IsIn) || ((direction == FieldDirection.Out) && !info3.IsOut)) || (((direction == FieldDirection.Ref) && (!info3.IsIn || !info3.IsOut)) || !Helpers.TypesEqual(expression.Type, info3.ParameterType)))
                    {
                        flag = false;
                        break;
                    }
                }
                if (flag)
                {
                    list.Add(info2.Name);
                }
            }
            int num2 = 0;
            bool flag2 = true;
            string strB = name;
            MemberInfo[] members = dataSourceClass.GetMembers();
            while (flag2 && (num2 < 0x7fffffff))
            {
                flag2 = false;
                foreach (string str3 in list)
                {
                    if (string.Compare(str3, strB, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        flag2 = true;
                        break;
                    }
                }
                if (!flag2)
                {
                    foreach (MemberInfo info4 in members)
                    {
                        if (!(info4 is MethodInfo) && (string.Compare(info4.Name, strB, StringComparison.OrdinalIgnoreCase) == 0))
                        {
                            flag2 = true;
                            break;
                        }
                    }
                }
                if (!flag2)
                {
                    MethodInfo info5 = host.RootComponent.GetType().GetMethod(strB, bindingAttr, null, info.ParameterTypes, null);
                    if ((info5 != null) && !info5.IsPrivate)
                    {
                        flag2 = true;
                    }
                }
                if (flag2)
                {
                    strB = name + "_" + ++num2.ToString(CultureInfo.InvariantCulture);
                }
            }
            return strB;
        }

        internal static bool DeserializeDesignerStates(IDesignerHost designerHost, BinaryReader reader)
        {
            int num = reader.ReadInt32();
            bool flag = num != designerHost.Container.Components.Count;
            for (int i = 0; i < num; i++)
            {
                string str = reader.ReadString();
                int num3 = reader.ReadInt32();
                if (designerHost.Container.Components[str] != null)
                {
                    ActivityDesigner designer = designerHost.GetDesigner(designerHost.Container.Components[str]) as ActivityDesigner;
                    if (designer != null)
                    {
                        ((IPersistUIState) designer).LoadViewState(reader);
                    }
                    else
                    {
                        flag = true;
                        Stream baseStream = reader.BaseStream;
                        baseStream.Position += num3;
                    }
                }
                else
                {
                    flag = true;
                    Stream stream2 = reader.BaseStream;
                    stream2.Position += num3;
                }
            }
            return flag;
        }

        internal static string GenerateUniqueIdentifier(IServiceProvider serviceProvider, string baseIdentifier, string[] existingNames)
        {
            CodeDomProvider provider = null;
            if (serviceProvider != null)
            {
                provider = serviceProvider.GetService(typeof(CodeDomProvider)) as CodeDomProvider;
                if (provider == null)
                {
                    IdentifierCreationService service = serviceProvider.GetService(typeof(IIdentifierCreationService)) as IdentifierCreationService;
                    if (service != null)
                    {
                        provider = service.Provider;
                    }
                }
            }
            if (provider != null)
            {
                baseIdentifier = provider.CreateValidIdentifier(baseIdentifier);
            }
            baseIdentifier = baseIdentifier.Replace('.', '_');
            baseIdentifier = baseIdentifier.Replace('/', '_');
            baseIdentifier = baseIdentifier.Replace('(', '_');
            baseIdentifier = baseIdentifier.Replace(')', '_');
            baseIdentifier = baseIdentifier.Replace(" ", "");
            ArrayList list = new ArrayList(existingNames);
            int num = 1;
            string str = string.Format(CultureInfo.InvariantCulture, "{0}{1}", new object[] { baseIdentifier, num });
            list.Sort();
            while (list.BinarySearch(str.ToLowerInvariant(), StringComparer.OrdinalIgnoreCase) >= 0)
            {
                str = string.Format(CultureInfo.InvariantCulture, "{0}{1}", new object[] { baseIdentifier, num });
                num++;
            }
            return str;
        }

        internal static IDictionary<string, string> GetDeclarativeRules(Activity activity)
        {
            IDictionary<string, string> dictionary = new Dictionary<string, string>();
            System.Type type = System.Type.GetType("System.Workflow.Activities.Rules.RuleConditionReference, System.Workflow.Activities, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35", false);
            if (type != null)
            {
                foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(activity))
                {
                    object obj2 = descriptor.GetValue(activity);
                    if ((obj2 != null) && type.IsAssignableFrom(obj2.GetType()))
                    {
                        PropertyDescriptor descriptor2 = TypeDescriptor.GetConverter(obj2).GetProperties(new System.Workflow.ComponentModel.Design.TypeDescriptorContext(activity.Site, null, obj2), obj2)["ConditionName"];
                        PropertyDescriptor descriptor3 = TypeDescriptor.GetConverter(obj2).GetProperties(new System.Workflow.ComponentModel.Design.TypeDescriptorContext(activity.Site, null, obj2), obj2)["Expression"];
                        if ((descriptor2 != null) && (descriptor3 != null))
                        {
                            string str = descriptor2.GetValue(obj2) as string;
                            object obj3 = descriptor3.GetValue(obj2);
                            if (!string.IsNullOrEmpty(str) && !dictionary.ContainsKey(str))
                            {
                                string str2 = (obj3 != null) ? (descriptor3.Converter.ConvertTo(new System.Workflow.ComponentModel.Design.TypeDescriptorContext(activity.Site, null, obj2), Thread.CurrentThread.CurrentUICulture, obj3, typeof(string)) as string) : null;
                                if (str2 == null)
                                {
                                    str2 = string.Empty;
                                }
                                dictionary.Add(str, str2);
                            }
                        }
                    }
                }
            }
            return dictionary;
        }

        internal static DesignerVerb[] GetDesignerActionVerbs(ActivityDesigner designer, ReadOnlyCollection<DesignerAction> designerActions)
        {
            List<DesignerVerb> list = new List<DesignerVerb>();
            for (int i = 0; i < designerActions.Count; i++)
            {
                DesignerVerb item = new DesignerVerb(designerActions[i].Text, new EventHandler(new EventHandler(DesignerHelpers.OnExecuteDesignerAction).Invoke), new CommandID(WorkflowMenuCommands.MenuGuid, WorkflowMenuCommands.VerbGroupDesignerActions + i));
                item.Properties[DesignerUserDataKeys.DesignerAction] = designerActions[i];
                item.Properties[DesignerUserDataKeys.Designer] = designer;
                list.Add(item);
            }
            return list.ToArray();
        }

        internal static Image GetImageFromPath(DesignerTheme designerTheme, string directory, string path)
        {
            Bitmap bitmap = null;
            if (path.Contains(Path.DirectorySeparatorChar.ToString()) && (directory.Length > 0))
            {
                string str = Uri.UnescapeDataString(new Uri(new Uri(directory), path).LocalPath);
                if (File.Exists(str))
                {
                    try
                    {
                        bitmap = new Bitmap(str);
                    }
                    catch
                    {
                    }
                }
            }
            else if (designerTheme.DesignerType != null)
            {
                int length = path.LastIndexOf('.');
                if (length > 0)
                {
                    string baseName = path.Substring(0, length);
                    string name = path.Substring(length + 1);
                    if (((baseName != null) && (baseName.Length > 0)) && ((name != null) && (name.Length > 0)))
                    {
                        try
                        {
                            ResourceManager manager = new ResourceManager(baseName, designerTheme.DesignerType.Assembly);
                            bitmap = manager.GetObject(name) as Bitmap;
                        }
                        catch
                        {
                        }
                    }
                }
            }
            if (bitmap != null)
            {
                bitmap.MakeTransparent(AmbientTheme.TransparentColor);
            }
            return bitmap;
        }

        internal static Activity GetNextSelectableActivity(Activity currentActivity)
        {
            object obj2;
            ActivityDesigner designer = ActivityDesigner.GetDesigner(currentActivity);
            CompositeActivityDesigner designer2 = (designer != null) ? designer.ParentDesigner : null;
            if (designer2 == null)
            {
                return null;
            }
            DesignerNavigationDirection direction = ((designer2 is ParallelActivityDesigner) || (designer2 is ActivityPreviewDesigner)) ? DesignerNavigationDirection.Right : DesignerNavigationDirection.Down;
            Activity activity = null;
            for (obj2 = designer2.GetNextSelectableObject(currentActivity, direction); ((activity == null) && (obj2 != null)) && (obj2 != currentActivity); obj2 = designer2.GetNextSelectableObject(obj2, direction))
            {
                activity = obj2 as Activity;
            }
            if (activity == null)
            {
                direction = ((designer2 is ParallelActivityDesigner) || (designer2 is ActivityPreviewDesigner)) ? DesignerNavigationDirection.Left : DesignerNavigationDirection.Up;
                for (obj2 = designer2.GetNextSelectableObject(currentActivity, direction); ((activity == null) && (obj2 != null)) && (obj2 != currentActivity); obj2 = designer2.GetNextSelectableObject(obj2, direction))
                {
                    activity = obj2 as Activity;
                }
            }
            if (activity == null)
            {
                activity = designer2.Activity;
            }
            return activity;
        }

        internal static string GetRelativePath(string pathFrom, string pathTo)
        {
            Uri uri = new Uri(pathFrom);
            string str = Uri.UnescapeDataString(uri.MakeRelativeUri(new Uri(pathTo)).ToString()).Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            if (!str.Contains(Path.DirectorySeparatorChar.ToString()))
            {
                str = "." + Path.DirectorySeparatorChar + str;
            }
            return str;
        }

        internal static bool IsValidImageResource(DesignerTheme designerTheme, string directory, string path)
        {
            Image image = GetImageFromPath(designerTheme, directory, path);
            bool flag = image != null;
            if (image != null)
            {
                image.Dispose();
            }
            return flag;
        }

        internal static void MakePropertiesReadOnly(IServiceProvider serviceProvider, object topComponent)
        {
            Hashtable hashtable = new Hashtable();
            Queue queue = new Queue();
            queue.Enqueue(topComponent);
            while (queue.Count > 0)
            {
                object instance = queue.Dequeue();
                if (hashtable[instance.GetHashCode()] == null)
                {
                    hashtable[instance.GetHashCode()] = instance;
                    TypeDescriptor.AddProvider(new ReadonlyTypeDescriptonProvider(TypeDescriptor.GetProvider(instance)), instance);
                    foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(instance, new Attribute[] { BrowsableAttribute.Yes }))
                    {
                        if (!descriptor.PropertyType.IsPrimitive)
                        {
                            object component = descriptor.GetValue(instance);
                            if (component != null)
                            {
                                TypeConverter converter = TypeDescriptor.GetConverter(component);
                                System.Workflow.ComponentModel.Design.TypeDescriptorContext context = new System.Workflow.ComponentModel.Design.TypeDescriptorContext(serviceProvider, descriptor, instance);
                                if (converter.GetPropertiesSupported(context))
                                {
                                    TypeDescriptor.AddProvider(new ReadonlyTypeDescriptonProvider(TypeDescriptor.GetProvider(component)), component);
                                    queue.Enqueue(component);
                                }
                            }
                        }
                    }
                }
            }
        }

        private static void OnExecuteDesignerAction(object sender, EventArgs e)
        {
            DesignerVerb verb = sender as DesignerVerb;
            if (verb != null)
            {
                DesignerAction designerAction = verb.Properties[DesignerUserDataKeys.DesignerAction] as DesignerAction;
                if (designerAction != null)
                {
                    ActivityDesigner designer = verb.Properties[DesignerUserDataKeys.Designer] as ActivityDesigner;
                    if (designer != null)
                    {
                        designer.OnExecuteDesignerAction(designerAction);
                    }
                }
            }
        }

        internal static void RefreshDesignerActions(IServiceProvider serviceProvider)
        {
            if (serviceProvider != null)
            {
                IDesignerHost service = serviceProvider.GetService(typeof(IDesignerHost)) as IDesignerHost;
                if (service != null)
                {
                    foreach (object obj2 in service.Container.Components)
                    {
                        Activity activity = obj2 as Activity;
                        if (activity != null)
                        {
                            ActivityDesigner designer = ActivityDesigner.GetDesigner(activity);
                            if (designer != null)
                            {
                                designer.RefreshDesignerActions();
                            }
                        }
                    }
                }
            }
        }

        internal static void SerializeDesignerStates(IDesignerHost designerHost, BinaryWriter writer)
        {
            writer.Write(designerHost.Container.Components.Count);
            foreach (IComponent component in designerHost.Container.Components)
            {
                writer.Write(component.Site.Name);
                int length = (int) writer.BaseStream.Length;
                writer.Write(0);
                ActivityDesigner designer = designerHost.GetDesigner(component) as ActivityDesigner;
                if (designer != null)
                {
                    int num2 = (int) writer.BaseStream.Length;
                    ((IPersistUIState) designer).SaveViewState(writer);
                    writer.Seek(length, SeekOrigin.Begin);
                    writer.Write((int) (((int) writer.BaseStream.Length) - num2));
                    writer.Seek(0, SeekOrigin.End);
                }
            }
        }

        internal static void ShowDesignerVerbs(ActivityDesigner designer, Point location, ICollection<DesignerVerb> designerVerbs)
        {
            if (!ShowingMenu && (designerVerbs.Count != 0))
            {
                IMenuCommandService service = designer.Activity.Site.GetService(typeof(IMenuCommandService)) as IMenuCommandService;
                if (service == null)
                {
                    throw new Exception(SR.GetString("General_MissingService", new object[] { typeof(IMenuCommandService).FullName }));
                }
                try
                {
                    foreach (DesignerVerb verb in designerVerbs)
                    {
                        service.AddCommand(verb);
                    }
                    ShowingMenu = true;
                    service.ShowContextMenu(WorkflowMenuCommands.DesignerActionsMenu, location.X - 2, location.Y + 1);
                }
                finally
                {
                    ShowingMenu = false;
                    foreach (DesignerVerb verb2 in designerVerbs)
                    {
                        service.RemoveCommand(verb2);
                    }
                }
            }
        }

        internal static void ShowError(IServiceProvider serviceProvider, Exception e)
        {
            if (e != CheckoutException.Canceled)
            {
                while ((e is TargetInvocationException) && (e.InnerException != null))
                {
                    e = e.InnerException;
                }
                string message = e.Message;
                if ((message == null) || (message.Length == 0))
                {
                    message = e.ToString();
                }
                ShowMessage(serviceProvider, message, DR.GetString("WorkflowDesignerTitle", new object[0]), MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1);
            }
        }

        internal static void ShowError(IServiceProvider serviceProvider, string message)
        {
            ShowMessage(serviceProvider, message, DR.GetString("WorkflowDesignerTitle", new object[0]), MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1);
        }

        internal static void ShowHelpFromKeyword(IServiceProvider serviceProvider, string helpKeyword)
        {
            IHelpService service = serviceProvider.GetService(typeof(IHelpService)) as IHelpService;
            if (service != null)
            {
                service.ShowHelpFromKeyword(helpKeyword);
            }
            else
            {
                ShowError(serviceProvider, DR.GetString("NoHelpAvailable", new object[0]));
            }
        }

        internal static DialogResult ShowMessage(IServiceProvider serviceProvider, string message, string title, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton)
        {
            IWin32Window owner = null;
            IUIService service = serviceProvider.GetService(typeof(IUIService)) as IUIService;
            if (service != null)
            {
                owner = service.GetDialogOwnerWindow();
            }
            else
            {
                owner = Form.ActiveForm;
            }
            Control control = owner as Control;
            MessageBoxOptions options = 0;
            if (owner == null)
            {
                return MessageBox.Show(message, title, buttons, icon, defaultButton, options);
            }
            if (control != null)
            {
                options = (control.RightToLeft == RightToLeft.Yes) ? ((MessageBoxOptions) 0) : ((MessageBoxOptions) 0);
            }
            else if (owner.Handle != IntPtr.Zero)
            {
                int windowLong = System.Workflow.ComponentModel.Design.UnsafeNativeMethods.GetWindowLong(owner.Handle, System.Workflow.ComponentModel.Design.UnsafeNativeMethods.GWL_EXSTYLE);
                if ((Marshal.GetLastWin32Error() == 0) && ((windowLong & System.Workflow.ComponentModel.Design.UnsafeNativeMethods.WS_EX_LAYOUTRTL) == System.Workflow.ComponentModel.Design.UnsafeNativeMethods.WS_EX_LAYOUTRTL))
                {
                    options = 0;
                }
            }
            return MessageBox.Show(owner, message, title, buttons, icon, defaultButton, options);
        }

        internal static Point SnapToGrid(Point location)
        {
            if (WorkflowTheme.CurrentTheme.AmbientTheme.ShowGrid)
            {
                Size gridSize = WorkflowTheme.CurrentTheme.AmbientTheme.GridSize;
                gridSize.Width /= 2;
                gridSize.Height /= 2;
                location.X = ((location.X / gridSize.Width) * gridSize.Width) + (((location.X % gridSize.Width) > (gridSize.Width / 2)) ? gridSize.Width : 0);
                location.Y = ((location.Y / gridSize.Height) * gridSize.Height) + (((location.Y % gridSize.Height) > (gridSize.Height / 2)) ? gridSize.Height : 0);
            }
            return location;
        }

        internal static void UpdateSiteName(Activity activity, string newID)
        {
            if (activity == null)
            {
                throw new ArgumentException("activity");
            }
            string str = newID;
            if (Helpers.IsActivityLocked(activity))
            {
                str = InternalHelpers.GenerateQualifiedNameForLockedActivity(activity, newID);
            }
            activity.Site.Name = str;
            if (activity is CompositeActivity)
            {
                foreach (Activity activity2 in Helpers.GetNestedActivities(activity as CompositeActivity))
                {
                    if (Helpers.IsActivityLocked(activity2))
                    {
                        Activity declaringActivity = Helpers.GetDeclaringActivity(activity2);
                        activity2.Site.Name = declaringActivity.Site.Name + "." + activity2.Name;
                    }
                }
            }
        }

        internal static string DesignerPerUserRegistryKey
        {
            get
            {
                return (Helpers.PerUserRegistryKey + @"\" + WorkflowDesignerSubKey);
            }
        }
    }
}

