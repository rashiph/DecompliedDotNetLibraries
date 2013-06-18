namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.ComponentModel.Design.Serialization;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Serialization;
    using System.Xml;

    [Serializable]
    internal class WorkflowMarkupSerializationStore : SerializationStore, ISerializable
    {
        private List<Activity> activities;
        private AssemblyName[] assemblies;
        private const string AssembliesKey = "Assemblies";
        private List<MemberDescriptor> memberList;
        private List<string> parentObjectNameList;
        private string serializedXmlString;
        private const string SerializedXmlString = "XmlString";
        private IServiceProvider serviceProvider;

        internal WorkflowMarkupSerializationStore(IServiceProvider serviceProvider)
        {
            this.activities = new List<Activity>();
            this.parentObjectNameList = new List<string>();
            this.memberList = new List<MemberDescriptor>();
            this.serviceProvider = serviceProvider;
        }

        private WorkflowMarkupSerializationStore(SerializationInfo info, StreamingContext context)
        {
            this.activities = new List<Activity>();
            this.parentObjectNameList = new List<string>();
            this.memberList = new List<MemberDescriptor>();
            this.serializedXmlString = (string) info.GetValue("XmlString", typeof(string));
            this.assemblies = (AssemblyName[]) info.GetValue("Assemblies", typeof(AssemblyName[]));
        }

        internal void AddMember(object value, MemberDescriptor member)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            if (member == null)
            {
                throw new ArgumentNullException("member");
            }
            if (this.serializedXmlString != null)
            {
                throw new InvalidOperationException(DR.GetString("InvalidOperationStoreAlreadyClosed", new object[0]));
            }
            IReferenceService service = this.serviceProvider.GetService(typeof(IReferenceService)) as IReferenceService;
            this.parentObjectNameList.Add(service.GetName(value));
            this.memberList.Add(member);
        }

        internal void AddObject(object value)
        {
            if (this.serializedXmlString != null)
            {
                throw new InvalidOperationException(DR.GetString("InvalidOperationStoreAlreadyClosed", new object[0]));
            }
            Activity item = value as Activity;
            if (item == null)
            {
                throw new ArgumentException("value");
            }
            this.activities.Add(item);
        }

        public override void Close()
        {
            if (this.serializedXmlString == null)
            {
                DesignerSerializationManager manager = new LocalDesignerSerializationManager(this, this.serviceProvider);
                using (manager.CreateSession())
                {
                    WorkflowMarkupSerializationManager serializationManager = new WorkflowMarkupSerializationManager(manager);
                    StringWriter w = new StringWriter(CultureInfo.InvariantCulture);
                    using (XmlTextWriter writer2 = new XmlTextWriter(w))
                    {
                        if (this.memberList.Count == 0)
                        {
                            WorkflowMarkupSerializer serializer = new WorkflowMarkupSerializer();
                            foreach (Activity activity in this.activities)
                            {
                                serializer.SerializeObject(serializationManager, activity, writer2);
                            }
                        }
                        else
                        {
                            PropertySegmentSerializationProvider provider = new PropertySegmentSerializationProvider();
                            serializationManager.AddSerializationProvider(provider);
                            serializationManager.Context.Push(new StringWriter(CultureInfo.InvariantCulture));
                            IReferenceService service = this.serviceProvider.GetService(typeof(IReferenceService)) as IReferenceService;
                            if (service != null)
                            {
                                for (int i = 0; i < this.memberList.Count; i++)
                                {
                                    object reference = service.GetReference(this.parentObjectNameList[i]);
                                    PropertySegmentSerializer serializer2 = new PropertySegmentSerializer(null);
                                    if (this.memberList[i] is PropertyDescriptor)
                                    {
                                        PropertyInfo property = XomlComponentSerializationService.GetProperty(reference.GetType(), (this.memberList[i] as PropertyDescriptor).Name, BindingFlags.Public | BindingFlags.Instance);
                                        if (property != null)
                                        {
                                            serializer2.SerializeObject(serializationManager, new PropertySegment(this.serviceProvider, reference, property), writer2);
                                        }
                                        else
                                        {
                                            serializer2.SerializeObject(serializationManager, new PropertySegment(this.serviceProvider, reference, this.memberList[i] as PropertyDescriptor), writer2);
                                        }
                                    }
                                    else if (this.memberList[i] is EventDescriptor)
                                    {
                                        IEventBindingService service2 = this.serviceProvider.GetService(typeof(IEventBindingService)) as IEventBindingService;
                                        if (service2 != null)
                                        {
                                            PropertySegment segment = new PropertySegment(this.serviceProvider, reference, service2.GetEventProperty(this.memberList[i] as EventDescriptor));
                                            serializer2.SerializeObject(serializationManager, segment, writer2);
                                        }
                                    }
                                }
                            }
                            serializationManager.Context.Pop();
                            serializationManager.RemoveSerializationProvider(provider);
                        }
                    }
                    this.serializedXmlString = w.ToString();
                    List<AssemblyName> list = new List<AssemblyName>();
                    foreach (Activity activity2 in this.activities)
                    {
                        Assembly assembly = activity2.GetType().Assembly;
                        list.Add(assembly.GetName(true));
                    }
                    this.assemblies = list.ToArray();
                    this.activities.Clear();
                    this.activities = null;
                }
            }
        }

        internal IList Deserialize(IServiceProvider serviceProvider)
        {
            IList list2;
            DesignerSerializationManager manager = new LocalDesignerSerializationManager(this, serviceProvider);
            using (manager.CreateSession())
            {
                EventHandler<WorkflowMarkupElementEventArgs> handler = null;
                ArrayList list = new ArrayList();
                WorkflowMarkupSerializationManager xomlSerializationManager = new WorkflowMarkupSerializationManager(manager);
                XmlTextReader reader = new XmlTextReader(this.serializedXmlString, XmlNodeType.Element, null);
                reader.MoveToElement();
            Label_0042:
                if (!reader.Read())
                {
                    list2 = list;
                }
                else
                {
                    if (handler == null)
                    {
                        handler = delegate (object sender, WorkflowMarkupElementEventArgs eventArgs) {
                            if ((eventArgs.XmlReader.LookupNamespace(eventArgs.XmlReader.Prefix) == "http://schemas.microsoft.com/winfx/2006/xaml") && (xomlSerializationManager.Context.Current is Activity))
                            {
                                WorkflowMarkupSerializationHelpers.ProcessDefTag(xomlSerializationManager, eventArgs.XmlReader, xomlSerializationManager.Context.Current as Activity, true, string.Empty);
                            }
                        };
                    }
                    xomlSerializationManager.FoundDefTag += handler;
                    object obj2 = new WorkflowMarkupSerializer().DeserializeObject(xomlSerializationManager, reader);
                    if (obj2 == null)
                    {
                        throw new InvalidOperationException(DR.GetString("InvalidOperationDeserializationReturnedNonActivity", new object[0]));
                    }
                    if (obj2 is Activity)
                    {
                        (obj2 as Activity).UserData.Remove(UserDataKeys.CustomActivity);
                    }
                    list.Add(obj2);
                    goto Label_0042;
                }
            }
            return list2;
        }

        internal ICollection Deserialize(IServiceProvider serviceProvider, IContainer container)
        {
            throw new NotImplementedException();
        }

        internal void DeserializeTo(IServiceProvider serviceProvider, IContainer container)
        {
            DesignerSerializationManager manager = new LocalDesignerSerializationManager(this, serviceProvider);
            using (manager.CreateSession())
            {
                WorkflowMarkupSerializationManager serializationManager = new WorkflowMarkupSerializationManager(manager);
                PropertySegmentSerializationProvider provider = new PropertySegmentSerializationProvider();
                serializationManager.AddSerializationProvider(provider);
                StringReader input = new StringReader(this.serializedXmlString);
                using (XmlTextReader reader2 = new XmlTextReader(input))
                {
                    while (((reader2.NodeType != XmlNodeType.Element) && (reader2.NodeType != XmlNodeType.ProcessingInstruction)) && reader2.Read())
                    {
                    }
                    IReferenceService service = this.serviceProvider.GetService(typeof(IReferenceService)) as IReferenceService;
                    IComponentChangeService service2 = this.serviceProvider.GetService(typeof(IComponentChangeService)) as IComponentChangeService;
                    for (int i = 0; i < this.memberList.Count; i++)
                    {
                        object reference = service.GetReference(this.parentObjectNameList[i]);
                        if (reference != null)
                        {
                            bool flag = (service2 != null) && (!(reference is IComponent) || (((IComponent) reference).Site == null));
                            PropertyDescriptor member = this.memberList[i] as PropertyDescriptor;
                            if (flag)
                            {
                                service2.OnComponentChanging(reference, member);
                            }
                            serializationManager.Context.Push(reference);
                            new PropertySegmentSerializer(null).DeserializeObject(serializationManager, reader2);
                            serializationManager.Context.Pop();
                            if (flag)
                            {
                                service2.OnComponentChanged(reference, member, null, null);
                            }
                        }
                    }
                }
                serializationManager.RemoveSerializationProvider(provider);
            }
        }

        public override void Save(Stream stream)
        {
            this.Close();
            new BinaryFormatter().Serialize(stream, this);
        }

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("XmlString", this.serializedXmlString);
            info.AddValue("Assemblies", this.assemblies);
        }

        private AssemblyName[] AssemblyNames
        {
            get
            {
                return this.assemblies;
            }
        }

        public override ICollection Errors
        {
            get
            {
                return null;
            }
        }

        private class LocalDesignerSerializationManager : DesignerSerializationManager
        {
            private WorkflowMarkupSerializationStore store;

            internal LocalDesignerSerializationManager(WorkflowMarkupSerializationStore store, IServiceProvider provider) : base(provider)
            {
                this.store = store;
            }

            protected override Type GetType(string name)
            {
                Type type = base.GetType(name);
                if (type == null)
                {
                    int index = name.IndexOf(",");
                    if (index != -1)
                    {
                        name = name.Substring(0, index);
                    }
                    AssemblyName[] assemblyNames = this.store.AssemblyNames;
                    foreach (AssemblyName name2 in assemblyNames)
                    {
                        Assembly assembly = Assembly.Load(name2);
                        if (assembly != null)
                        {
                            type = assembly.GetType(name);
                            if (type != null)
                            {
                                break;
                            }
                        }
                    }
                    if (type == null)
                    {
                        foreach (AssemblyName name3 in assemblyNames)
                        {
                            Assembly assembly2 = Assembly.Load(name3);
                            if (assembly2 != null)
                            {
                                foreach (AssemblyName name4 in assembly2.GetReferencedAssemblies())
                                {
                                    Assembly assembly3 = Assembly.Load(name4);
                                    if (assembly3 != null)
                                    {
                                        type = assembly3.GetType(name);
                                        if (type != null)
                                        {
                                            break;
                                        }
                                    }
                                }
                                if (type != null)
                                {
                                    return type;
                                }
                            }
                        }
                    }
                }
                return type;
            }
        }
    }
}

