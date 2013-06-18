namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.ComponentModel.Design.Serialization;
    using System.IO;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Compiler;
    using System.Workflow.ComponentModel.Serialization;
    using System.Xml;

    [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust"), PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust")]
    public abstract class WorkflowDesignerLoader : BasicDesignerLoader
    {
        private Hashtable createdServices;
        private CustomActivityDesignerAdapter customActivityDesignerAdapter;
        internal const string DesignerLayoutFileExtension = ".layout";
        private WorkflowDesignerEventsCoordinator eventsCoordinator;
        private bool loadingDesignerLayout;

        static WorkflowDesignerLoader()
        {
            ComponentDispenser.RegisterComponentExtenders(typeof(CustomActivityDesignerAdapter), new IExtenderProvider[] { new CustomActivityPropertyExtender() });
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected WorkflowDesignerLoader()
        {
        }

        public void AddActivityToDesigner(Activity activity)
        {
            if (activity == null)
            {
                throw new ArgumentNullException("activity");
            }
            IDesignerHost service = base.GetService(typeof(IDesignerHost)) as IDesignerHost;
            if (service == null)
            {
                throw new InvalidOperationException(SR.GetString("General_MissingService", new object[] { typeof(IDesignerHost).FullName }));
            }
            if ((activity.Parent == null) && (service.RootComponent == null))
            {
                string str = activity.GetValue(WorkflowMarkupSerializer.XClassProperty) as string;
                string name = !string.IsNullOrEmpty(str) ? Helpers.GetClassName(str) : Helpers.GetClassName(activity.GetType().FullName);
                service.Container.Add(activity, name);
                this.AddTargetFrameworkProvider(activity);
            }
            else
            {
                service.Container.Add(activity, activity.QualifiedName);
                this.AddTargetFrameworkProvider(activity);
            }
            if (activity is CompositeActivity)
            {
                foreach (Activity activity2 in Helpers.GetNestedActivities(activity as CompositeActivity))
                {
                    service.Container.Add(activity2, activity2.QualifiedName);
                    this.AddTargetFrameworkProvider(activity2);
                }
            }
        }

        internal static void AddActivityToDesigner(IServiceProvider serviceProvider, Activity activity)
        {
            WorkflowDesignerLoader service = serviceProvider.GetService(typeof(WorkflowDesignerLoader)) as WorkflowDesignerLoader;
            if (service == null)
            {
                throw new InvalidOperationException(SR.GetString("General_MissingService", new object[] { typeof(WorkflowDesignerLoader).FullName }));
            }
            service.AddActivityToDesigner(activity);
        }

        private void AddTargetFrameworkProvider(IComponent component)
        {
            TypeDescriptionProviderService service = base.GetService(typeof(TypeDescriptionProviderService)) as TypeDescriptionProviderService;
            if ((service != null) && (component != null))
            {
                TypeDescriptor.AddProvider(service.GetProvider(component), component);
            }
        }

        public override void Dispose()
        {
            if (this.eventsCoordinator != null)
            {
                ((IDisposable) this.eventsCoordinator).Dispose();
                this.eventsCoordinator = null;
            }
            if (this.customActivityDesignerAdapter != null)
            {
                ((IDisposable) this.customActivityDesignerAdapter).Dispose();
                this.customActivityDesignerAdapter = null;
            }
            IExtenderProviderService service = base.GetService(typeof(IExtenderProviderService)) as IExtenderProviderService;
            if (service != null)
            {
                foreach (IExtenderProvider provider in ComponentDispenser.Extenders)
                {
                    service.RemoveExtenderProvider(provider);
                }
            }
            if (base.LoaderHost != null)
            {
                if (this.createdServices != null)
                {
                    foreach (Type type in this.createdServices.Keys)
                    {
                        base.LoaderHost.RemoveService(type);
                        this.OnDisposeService(type, this.createdServices[type]);
                    }
                    this.createdServices.Clear();
                    this.createdServices = null;
                }
                base.LoaderHost.RemoveService(typeof(WorkflowDesignerLoader));
            }
            base.Dispose();
        }

        public override void Flush()
        {
            base.Flush();
        }

        public virtual void ForceReload()
        {
            base.Reload(BasicDesignerLoader.ReloadOptions.Force);
        }

        public abstract TextReader GetFileReader(string filePath);
        public abstract TextWriter GetFileWriter(string filePath);
        protected override void Initialize()
        {
            base.Initialize();
            Type type = Type.GetType("System.Workflow.Activities.InvokeWorkflowActivity, System.Workflow.Activities, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
            if (type != null)
            {
                TypeDescriptor.AddAttributes(type, new Attribute[] { new DesignerAttribute(typeof(InvokeWorkflowDesigner), typeof(IDesigner)) });
            }
            base.LoaderHost.AddService(typeof(WorkflowDesignerLoader), this);
            ServiceCreatorCallback callback = new ServiceCreatorCallback(this.OnCreateService);
            if (base.LoaderHost.GetService(typeof(IWorkflowCompilerOptionsService)) == null)
            {
                base.LoaderHost.AddService(typeof(IWorkflowCompilerOptionsService), callback);
            }
            if (base.LoaderHost.GetService(typeof(IIdentifierCreationService)) == null)
            {
                base.LoaderHost.AddService(typeof(IIdentifierCreationService), callback);
            }
            if (base.LoaderHost.GetService(typeof(ComponentSerializationService)) == null)
            {
                base.LoaderHost.AddService(typeof(ComponentSerializationService), callback);
            }
            base.LoaderHost.RemoveService(typeof(IReferenceService));
            if (base.LoaderHost.GetService(typeof(IReferenceService)) == null)
            {
                base.LoaderHost.AddService(typeof(IReferenceService), callback);
            }
            if (base.LoaderHost.GetService(typeof(IDesignerVerbProviderService)) == null)
            {
                base.LoaderHost.AddService(typeof(IDesignerVerbProviderService), callback);
            }
            IExtenderProviderService service = base.GetService(typeof(IExtenderProviderService)) as IExtenderProviderService;
            if (service != null)
            {
                foreach (IExtenderProvider provider in ComponentDispenser.Extenders)
                {
                    service.AddExtenderProvider(provider);
                }
            }
            this.customActivityDesignerAdapter = new CustomActivityDesignerAdapter(base.LoaderHost);
        }

        private void LoadDesignerLayout(out IList layoutErrors)
        {
            layoutErrors = null;
            string designerLayoutFileName = this.DesignerLayoutFileName;
            IWorkflowRootDesigner safeRootDesigner = ActivityDesigner.GetSafeRootDesigner(base.LoaderHost);
            if (((safeRootDesigner != null) && safeRootDesigner.SupportsLayoutPersistence) && File.Exists(designerLayoutFileName))
            {
                using (TextReader reader = this.GetFileReader(designerLayoutFileName))
                {
                    if (reader != null)
                    {
                        using (XmlReader reader2 = XmlReader.Create(reader))
                        {
                            this.LoadDesignerLayout(reader2, out layoutErrors);
                        }
                    }
                }
            }
        }

        protected void LoadDesignerLayout(XmlReader layoutReader, out IList layoutLoadErrors)
        {
            if (layoutReader == null)
            {
                throw new ArgumentNullException("layoutReader");
            }
            ArrayList list = new ArrayList();
            layoutLoadErrors = list;
            ActivityDesigner designer = null;
            IDesignerHost service = base.GetService(typeof(IDesignerHost)) as IDesignerHost;
            if ((service != null) && (service.RootComponent != null))
            {
                designer = service.GetDesigner(service.RootComponent) as ActivityDesigner;
            }
            if (designer != null)
            {
                if (designer.SupportsLayoutPersistence)
                {
                    DesignerSerializationManager manager = new DesignerSerializationManager(base.LoaderHost);
                    using (manager.CreateSession())
                    {
                        WorkflowMarkupSerializationManager serializationManager = new WorkflowMarkupSerializationManager(manager);
                        serializationManager.AddSerializationProvider(new ActivityDesignerLayoutSerializerProvider());
                        try
                        {
                            new WorkflowMarkupSerializer().Deserialize(serializationManager, layoutReader);
                        }
                        catch (Exception exception)
                        {
                            list.Add(new WorkflowMarkupSerializationException(SR.GetString("Error_LayoutDeserialization"), exception));
                        }
                        finally
                        {
                            if (manager.Errors != null)
                            {
                                list.AddRange(manager.Errors);
                            }
                        }
                        return;
                    }
                }
                list.Add(new WorkflowMarkupSerializationException(SR.GetString("Error_LayoutSerializationPersistenceSupport")));
            }
            else
            {
                list.Add(new WorkflowMarkupSerializationException(SR.GetString("Error_LayoutSerializationRootDesignerNotFound")));
            }
        }

        private void LoadDesignerLayoutFromResource(out IList layoutErrors)
        {
            layoutErrors = null;
            IWorkflowRootDesigner safeRootDesigner = ActivityDesigner.GetSafeRootDesigner(base.LoaderHost);
            if ((safeRootDesigner != null) && safeRootDesigner.SupportsLayoutPersistence)
            {
                Type type = safeRootDesigner.Component.GetType();
                string manifestResourceName = type.Name + ".layout";
                this.LoadDesignerLayoutFromResource(type, manifestResourceName, out layoutErrors);
            }
        }

        protected void LoadDesignerLayoutFromResource(Type type, string manifestResourceName, out IList errors)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            if (manifestResourceName == null)
            {
                throw new ArgumentNullException("manifestResourceName");
            }
            if (manifestResourceName.Length == 0)
            {
                throw new ArgumentException(SR.GetString("Error_ParameterCannotBeEmpty"), "manifestResourceName");
            }
            errors = new ArrayList();
            Stream manifestResourceStream = type.Module.Assembly.GetManifestResourceStream(type, manifestResourceName);
            if (manifestResourceStream == null)
            {
                manifestResourceStream = type.Module.Assembly.GetManifestResourceStream(manifestResourceName);
            }
            if (manifestResourceStream != null)
            {
                using (XmlReader reader = XmlReader.Create(manifestResourceStream))
                {
                    if (reader != null)
                    {
                        this.LoadDesignerLayout(reader, out errors);
                    }
                }
            }
        }

        private object OnCreateService(IServiceContainer container, Type serviceType)
        {
            object obj2 = null;
            if (serviceType == typeof(ComponentSerializationService))
            {
                obj2 = new XomlComponentSerializationService(base.LoaderHost);
            }
            else if (serviceType == typeof(IReferenceService))
            {
                obj2 = new System.Workflow.ComponentModel.Design.ReferenceService(base.LoaderHost);
            }
            else if (serviceType == typeof(IIdentifierCreationService))
            {
                obj2 = new IdentifierCreationService(container, this);
            }
            else if (serviceType == typeof(IWorkflowCompilerOptionsService))
            {
                obj2 = new WorkflowCompilerOptionsService();
            }
            else if (serviceType == typeof(IDesignerVerbProviderService))
            {
                obj2 = new DesignerVerbProviderService();
            }
            if (obj2 != null)
            {
                if (this.createdServices == null)
                {
                    this.createdServices = new Hashtable();
                }
                object service = this.createdServices[serviceType];
                this.createdServices[serviceType] = obj2;
                if (service != null)
                {
                    this.OnDisposeService(serviceType, service);
                }
            }
            return obj2;
        }

        private void OnDisposeService(Type serviceType, object service)
        {
            if (serviceType == typeof(IReferenceService))
            {
                System.Workflow.ComponentModel.Design.ReferenceService service2 = service as System.Workflow.ComponentModel.Design.ReferenceService;
                if (service2 != null)
                {
                    service2.Dispose();
                }
            }
        }

        protected override void OnEndLoad(bool successful, ICollection errors)
        {
            base.OnEndLoad(successful, errors);
            if (successful)
            {
                ActivityDesigner rootDesigner = ActivityDesigner.GetRootDesigner(base.LoaderHost);
                if ((this.eventsCoordinator == null) && ((rootDesigner == null) || (rootDesigner.ParentDesigner == null)))
                {
                    this.eventsCoordinator = new WorkflowDesignerEventsCoordinator(base.LoaderHost);
                }
                try
                {
                    this.loadingDesignerLayout = true;
                    string designerLayoutFileName = this.DesignerLayoutFileName;
                    IList layoutErrors = null;
                    if (File.Exists(designerLayoutFileName))
                    {
                        this.LoadDesignerLayout(out layoutErrors);
                    }
                    else if (this.InDebugMode || ((ActivityDesigner.GetRootDesigner(base.LoaderHost) != null) && (ActivityDesigner.GetRootDesigner(base.LoaderHost).ParentDesigner != null)))
                    {
                        this.LoadDesignerLayoutFromResource(out layoutErrors);
                    }
                    if (layoutErrors != null)
                    {
                        if (errors == null)
                        {
                            errors = new ArrayList();
                        }
                        IList list2 = errors as IList;
                        if (list2 != null)
                        {
                            foreach (object obj2 in layoutErrors)
                            {
                                list2.Add(obj2);
                            }
                        }
                    }
                }
                finally
                {
                    this.loadingDesignerLayout = false;
                }
            }
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected override void PerformFlush(IDesignerSerializationManager serializationManager)
        {
            this.SaveDesignerLayout();
        }

        protected override void PerformLoad(IDesignerSerializationManager serializationManager)
        {
        }

        public void RemoveActivityFromDesigner(Activity activity)
        {
            if (activity == null)
            {
                throw new ArgumentNullException("activity");
            }
            IDesignerHost service = base.GetService(typeof(IDesignerHost)) as IDesignerHost;
            if (service == null)
            {
                throw new InvalidOperationException(SR.GetString("General_MissingService", new object[] { typeof(IDesignerHost).FullName }));
            }
            service.DestroyComponent(activity);
            if (activity is CompositeActivity)
            {
                foreach (Activity activity2 in Helpers.GetNestedActivities(activity as CompositeActivity))
                {
                    service.DestroyComponent(activity2);
                }
            }
        }

        internal static void RemoveActivityFromDesigner(IServiceProvider serviceProvider, Activity activity)
        {
            WorkflowDesignerLoader service = serviceProvider.GetService(typeof(WorkflowDesignerLoader)) as WorkflowDesignerLoader;
            if (service == null)
            {
                throw new InvalidOperationException(SR.GetString("General_MissingService", new object[] { typeof(WorkflowDesignerLoader).FullName }));
            }
            service.RemoveActivityFromDesigner(activity);
        }

        private void SaveDesignerLayout()
        {
            string designerLayoutFileName = this.DesignerLayoutFileName;
            ActivityDesigner safeRootDesigner = ActivityDesigner.GetSafeRootDesigner(base.LoaderHost);
            if ((!string.IsNullOrEmpty(designerLayoutFileName) && (safeRootDesigner != null)) && safeRootDesigner.SupportsLayoutPersistence)
            {
                using (TextWriter writer = this.GetFileWriter(designerLayoutFileName))
                {
                    if (writer != null)
                    {
                        IList layoutSaveErrors = null;
                        using (XmlWriter writer2 = Helpers.CreateXmlWriter(writer))
                        {
                            this.SaveDesignerLayout(writer2, safeRootDesigner, out layoutSaveErrors);
                        }
                    }
                }
            }
        }

        protected void SaveDesignerLayout(XmlWriter layoutWriter, ActivityDesigner rootDesigner, out IList layoutSaveErrors)
        {
            if (layoutWriter == null)
            {
                throw new ArgumentNullException("layoutWriter");
            }
            if (rootDesigner == null)
            {
                throw new ArgumentNullException("rootDesigner");
            }
            ArrayList list = new ArrayList();
            layoutSaveErrors = list;
            if (rootDesigner.SupportsLayoutPersistence)
            {
                DesignerSerializationManager manager = new DesignerSerializationManager(base.LoaderHost);
                using (manager.CreateSession())
                {
                    WorkflowMarkupSerializationManager serializationManager = new WorkflowMarkupSerializationManager(manager);
                    serializationManager.AddSerializationProvider(new ActivityDesignerLayoutSerializerProvider());
                    try
                    {
                        new WorkflowMarkupSerializer().Serialize(serializationManager, layoutWriter, rootDesigner);
                    }
                    catch (Exception exception)
                    {
                        list.Add(new WorkflowMarkupSerializationException(SR.GetString("Error_LayoutSerialization"), exception));
                    }
                    finally
                    {
                        if (manager.Errors != null)
                        {
                            list.AddRange(manager.Errors);
                        }
                    }
                    return;
                }
            }
            list.Add(new WorkflowMarkupSerializationException(SR.GetString("Error_LayoutSerializationPersistenceSupport")));
        }

        internal void SetModified(bool modified)
        {
            if (((base.LoaderHost != null) && !base.LoaderHost.Loading) && !this.loadingDesignerLayout)
            {
                this.OnModifying();
                base.Modified = modified;
            }
        }

        private string DesignerLayoutFileName
        {
            get
            {
                string fileName = this.FileName;
                if (!string.IsNullOrEmpty(fileName))
                {
                    fileName = Path.Combine(Path.GetDirectoryName(fileName), Path.GetFileNameWithoutExtension(fileName)) + ".layout";
                }
                return fileName;
            }
        }

        public abstract string FileName { get; }

        public virtual bool InDebugMode
        {
            get
            {
                return false;
            }
        }

        protected virtual TypeDescriptionProvider TargetFrameworkTypeDescriptionProvider
        {
            get
            {
                return null;
            }
        }
    }
}

