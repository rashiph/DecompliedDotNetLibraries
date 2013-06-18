namespace System.Workflow.ComponentModel.Serialization
{
    using System;
    using System.CodeDom;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.ComponentModel.Design.Serialization;
    using System.Drawing;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Workflow.ComponentModel.Compiler;
    using System.Workflow.ComponentModel.Design;
    using System.Xml;

    public class WorkflowMarkupSerializationManager : IDesignerSerializationManager, IServiceProvider
    {
        private Dictionary<XmlQualifiedName, Type> cachedXmlQualifiedNameTypes = new Dictionary<XmlQualifiedName, Type>();
        private Dictionary<int, WorkflowMarkupSerializerMapping> clrNamespaceBasedMappings = new Dictionary<int, WorkflowMarkupSerializerMapping>();
        private bool designMode;
        private List<WorkflowMarkupSerializer> extendedPropertiesProviders;
        private Assembly localAssembly;
        private Dictionary<string, List<WorkflowMarkupSerializerMapping>> prefixBasedMappings = new Dictionary<string, List<WorkflowMarkupSerializerMapping>>();
        private IDesignerSerializationManager serializationManager;
        private Stack serializationStack = new Stack();
        private ContextStack workflowMarkupStack = new ContextStack();
        private int writerDepth;
        private Dictionary<string, List<WorkflowMarkupSerializerMapping>> xmlNamespaceBasedMappings = new Dictionary<string, List<WorkflowMarkupSerializerMapping>>();

        internal event EventHandler<WorkflowMarkupElementEventArgs> FoundDefTag;

        event ResolveNameEventHandler IDesignerSerializationManager.ResolveName
        {
            add
            {
            }
            remove
            {
            }
        }

        event EventHandler IDesignerSerializationManager.SerializationComplete
        {
            add
            {
            }
            remove
            {
            }
        }

        public WorkflowMarkupSerializationManager(IDesignerSerializationManager manager)
        {
            if (manager == null)
            {
                throw new ArgumentNullException("manager");
            }
            this.serializationManager = manager;
            this.AddSerializationProvider(new WellKnownTypeSerializationProvider());
            this.AddMappings(WorkflowMarkupSerializerMapping.WellKnownMappings);
            ITypeProvider service = manager.GetService(typeof(ITypeProvider)) as ITypeProvider;
            if (service != null)
            {
                this.LocalAssembly = service.LocalAssembly;
            }
            this.designMode = manager.GetService(typeof(ITypeResolutionService)) != null;
        }

        internal void AddMappings(IList<WorkflowMarkupSerializerMapping> mappingsToAdd)
        {
            foreach (WorkflowMarkupSerializerMapping mapping in mappingsToAdd)
            {
                if (!this.clrNamespaceBasedMappings.ContainsKey(mapping.GetHashCode()))
                {
                    this.clrNamespaceBasedMappings.Add(mapping.GetHashCode(), mapping);
                }
                List<WorkflowMarkupSerializerMapping> list = null;
                if (!this.xmlNamespaceBasedMappings.TryGetValue(mapping.XmlNamespace, out list))
                {
                    list = new List<WorkflowMarkupSerializerMapping>();
                    this.xmlNamespaceBasedMappings.Add(mapping.XmlNamespace, list);
                }
                list.Add(mapping);
                List<WorkflowMarkupSerializerMapping> list2 = null;
                if (!this.prefixBasedMappings.TryGetValue(mapping.Prefix, out list2))
                {
                    list2 = new List<WorkflowMarkupSerializerMapping>();
                    this.prefixBasedMappings.Add(mapping.Prefix, list2);
                }
                list2.Add(mapping);
            }
        }

        public void AddSerializationProvider(IDesignerSerializationProvider provider)
        {
            if (provider == null)
            {
                throw new ArgumentNullException("provider");
            }
            this.serializationManager.AddSerializationProvider(provider);
        }

        internal void FireFoundDefTag(WorkflowMarkupElementEventArgs args)
        {
            if (this.FoundDefTag != null)
            {
                this.FoundDefTag(this, args);
            }
        }

        internal ExtendedPropertyInfo[] GetExtendedProperties(object extendee)
        {
            List<ExtendedPropertyInfo> list = new List<ExtendedPropertyInfo>();
            foreach (WorkflowMarkupSerializer serializer in this.ExtendedPropertiesProviders)
            {
                list.AddRange(serializer.GetExtendedProperties(this, extendee));
            }
            return list.ToArray();
        }

        public object GetSerializer(Type objectType, Type serializerType)
        {
            return this.serializationManager.GetSerializer(objectType, serializerType);
        }

        public object GetService(Type serviceType)
        {
            if (serviceType == null)
            {
                throw new ArgumentNullException("serviceType");
            }
            return this.serializationManager.GetService(serviceType);
        }

        public virtual Type GetType(string typeName)
        {
            if (typeName == null)
            {
                throw new ArgumentNullException("typeName");
            }
            Type type = null;
            if (this.designMode)
            {
                try
                {
                    type = this.serializationManager.GetType(typeName);
                }
                catch
                {
                }
            }
            if (type == null)
            {
                ITypeProvider service = this.GetService(typeof(ITypeProvider)) as ITypeProvider;
                if (service != null)
                {
                    type = service.GetType(typeName, false);
                }
            }
            if (type != null)
            {
                return type;
            }
            string assemblyString = string.Empty;
            int index = typeName.IndexOf(",");
            string str2 = typeName;
            if (index > 0)
            {
                assemblyString = typeName.Substring(index + 1);
                typeName = typeName.Substring(0, index);
            }
            Assembly assembly = null;
            assemblyString = assemblyString.Trim();
            if (assemblyString.Length <= 0)
            {
                return type;
            }
            if (assemblyString.IndexOf(',') >= 0)
            {
                try
                {
                    assembly = Assembly.Load(assemblyString);
                }
                catch
                {
                }
            }
            typeName = typeName.Trim();
            if (assembly != null)
            {
                return assembly.GetType(typeName, false);
            }
            return Type.GetType(str2, false);
        }

        public virtual Type GetType(XmlQualifiedName xmlQualifiedName)
        {
            if (xmlQualifiedName == null)
            {
                throw new ArgumentNullException("xmlQualifiedName");
            }
            string xmlns = xmlQualifiedName.Namespace;
            string typeName = WorkflowMarkupSerializer.EnsureMarkupExtensionTypeName(xmlQualifiedName);
            Type type = null;
            this.cachedXmlQualifiedNameTypes.TryGetValue(xmlQualifiedName, out type);
            if (type == null)
            {
                type = WorkflowMarkupSerializerMapping.ResolveWellKnownTypes(this, xmlns, typeName);
            }
            if (type == null)
            {
                List<WorkflowMarkupSerializerMapping> list = null;
                if (!this.xmlNamespaceBasedMappings.TryGetValue(xmlns, out list))
                {
                    IList<WorkflowMarkupSerializerMapping> matchingMappings = null;
                    IList<WorkflowMarkupSerializerMapping> collectedMappings = null;
                    WorkflowMarkupSerializerMapping.GetMappingsFromXmlNamespace(this, xmlns, out matchingMappings, out collectedMappings);
                    this.AddMappings(matchingMappings);
                    this.AddMappings(collectedMappings);
                    list = new List<WorkflowMarkupSerializerMapping>(matchingMappings);
                }
                foreach (WorkflowMarkupSerializerMapping mapping in list)
                {
                    string assemblyName = mapping.AssemblyName;
                    string clrNamespace = mapping.ClrNamespace;
                    string name = xmlQualifiedName.Name;
                    if (clrNamespace.Length > 0)
                    {
                        name = clrNamespace + "." + xmlQualifiedName.Name;
                    }
                    if (assemblyName.Equals(Assembly.GetExecutingAssembly().FullName, StringComparison.Ordinal))
                    {
                        type = Assembly.GetExecutingAssembly().GetType(name);
                    }
                    else if (assemblyName.Length == 0)
                    {
                        if (this.localAssembly != null)
                        {
                            type = this.localAssembly.GetType(name);
                        }
                    }
                    else
                    {
                        string str6 = name;
                        if (assemblyName.Length > 0)
                        {
                            str6 = str6 + ", " + assemblyName;
                        }
                        try
                        {
                            type = this.GetType(str6);
                        }
                        catch
                        {
                        }
                        if (type == null)
                        {
                            type = this.GetType(name);
                            if ((type != null) && !type.AssemblyQualifiedName.Equals(str6, StringComparison.Ordinal))
                            {
                                type = null;
                            }
                        }
                    }
                    if (type != null)
                    {
                        this.cachedXmlQualifiedNameTypes[xmlQualifiedName] = type;
                        return type;
                    }
                }
            }
            return type;
        }

        public virtual XmlQualifiedName GetXmlQualifiedName(Type type, out string prefix)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            string str = (type.Namespace != null) ? type.Namespace : string.Empty;
            string str2 = ((type.Assembly != null) && (type.Assembly != this.localAssembly)) ? type.Assembly.FullName : string.Empty;
            WorkflowMarkupSerializerMapping mapping = null;
            int key = str.GetHashCode() ^ str2.GetHashCode();
            if (!this.clrNamespaceBasedMappings.TryGetValue(key, out mapping))
            {
                IList<WorkflowMarkupSerializerMapping> collectedMappings = null;
                WorkflowMarkupSerializerMapping.GetMappingFromType(this, type, out mapping, out collectedMappings);
                this.AddMappings(new List<WorkflowMarkupSerializerMapping>(new WorkflowMarkupSerializerMapping[] { mapping }));
                this.AddMappings(collectedMappings);
            }
            string name = WorkflowMarkupSerializer.EnsureMarkupExtensionTypeName(type);
            prefix = mapping.Prefix.Equals("wf", StringComparison.Ordinal) ? string.Empty : mapping.Prefix;
            return new XmlQualifiedName(name, mapping.XmlNamespace);
        }

        public void RemoveSerializationProvider(IDesignerSerializationProvider provider)
        {
            if (provider == null)
            {
                throw new ArgumentNullException("provider");
            }
            this.serializationManager.RemoveSerializationProvider(provider);
        }

        public void ReportError(object errorInformation)
        {
            if (errorInformation == null)
            {
                throw new ArgumentNullException("errorInformation");
            }
            this.serializationManager.ReportError(errorInformation);
        }

        object IDesignerSerializationManager.CreateInstance(Type type, ICollection arguments, string name, bool addToContainer)
        {
            return this.serializationManager.CreateInstance(type, arguments, name, addToContainer);
        }

        object IDesignerSerializationManager.GetInstance(string name)
        {
            return this.serializationManager.GetInstance(name);
        }

        string IDesignerSerializationManager.GetName(object value)
        {
            return this.serializationManager.GetName(value);
        }

        void IDesignerSerializationManager.SetName(object instance, string name)
        {
            this.serializationManager.SetName(instance, name);
        }

        internal IDictionary<int, WorkflowMarkupSerializerMapping> ClrNamespaceBasedMappings
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.clrNamespaceBasedMappings;
            }
        }

        public ContextStack Context
        {
            get
            {
                return this.serializationManager.Context;
            }
        }

        internal IList<WorkflowMarkupSerializer> ExtendedPropertiesProviders
        {
            get
            {
                if (this.extendedPropertiesProviders == null)
                {
                    this.extendedPropertiesProviders = new List<WorkflowMarkupSerializer>();
                }
                return this.extendedPropertiesProviders;
            }
        }

        public Assembly LocalAssembly
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.localAssembly;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.localAssembly = value;
            }
        }

        internal Dictionary<string, List<WorkflowMarkupSerializerMapping>> PrefixBasedMappings
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.prefixBasedMappings;
            }
        }

        protected internal IDesignerSerializationManager SerializationManager
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.serializationManager;
            }
            set
            {
                this.serializationManager = value;
                this.serializationManager.AddSerializationProvider(new WellKnownTypeSerializationProvider());
            }
        }

        internal Stack SerializationStack
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.serializationStack;
            }
        }

        PropertyDescriptorCollection IDesignerSerializationManager.Properties
        {
            get
            {
                return this.serializationManager.Properties;
            }
        }

        internal ContextStack WorkflowMarkupStack
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.workflowMarkupStack;
            }
        }

        internal int WriterDepth
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.writerDepth;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.writerDepth = value;
            }
        }

        internal IDictionary<string, List<WorkflowMarkupSerializerMapping>> XmlNamespaceBasedMappings
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.xmlNamespaceBasedMappings;
            }
        }

        private sealed class WellKnownTypeSerializationProvider : IDesignerSerializationProvider
        {
            object IDesignerSerializationProvider.GetSerializer(IDesignerSerializationManager manager, object currentSerializer, Type objectType, Type serializerType)
            {
                if ((serializerType == typeof(WorkflowMarkupSerializer)) && (objectType != null))
                {
                    if ((TypeProvider.IsAssignable(typeof(ICollection<string>), objectType) && TypeProvider.IsAssignable(objectType, typeof(List<string>))) && !TypeProvider.IsAssignable(typeof(Array), objectType))
                    {
                        return new StringCollectionMarkupSerializer();
                    }
                    if (typeof(Color).IsAssignableFrom(objectType))
                    {
                        return new ColorMarkupSerializer();
                    }
                    if (typeof(Size).IsAssignableFrom(objectType))
                    {
                        return new SizeMarkupSerializer();
                    }
                    if (typeof(Point).IsAssignableFrom(objectType))
                    {
                        return new PointMarkupSerializer();
                    }
                    if (objectType == typeof(CodeTypeReference))
                    {
                        return new CodeTypeReferenceSerializer();
                    }
                }
                return null;
            }
        }
    }
}

