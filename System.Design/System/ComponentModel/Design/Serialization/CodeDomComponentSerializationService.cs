namespace System.ComponentModel.Design.Serialization
{
    using System;
    using System.CodeDom;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Design;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Resources;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;

    public sealed class CodeDomComponentSerializationService : ComponentSerializationService
    {
        private IServiceProvider _provider;

        public CodeDomComponentSerializationService() : this(null)
        {
        }

        public CodeDomComponentSerializationService(IServiceProvider provider)
        {
            this._provider = provider;
        }

        public override SerializationStore CreateStore()
        {
            return new CodeDomSerializationStore(this._provider);
        }

        public override ICollection Deserialize(SerializationStore store)
        {
            if (store == null)
            {
                throw new ArgumentNullException("store");
            }
            CodeDomSerializationStore store2 = store as CodeDomSerializationStore;
            if (store2 == null)
            {
                throw new InvalidOperationException(System.Design.SR.GetString("CodeDomComponentSerializationServiceUnknownStore"));
            }
            return store2.Deserialize(this._provider);
        }

        public override ICollection Deserialize(SerializationStore store, IContainer container)
        {
            if (store == null)
            {
                throw new ArgumentNullException("store");
            }
            if (container == null)
            {
                throw new ArgumentNullException("container");
            }
            CodeDomSerializationStore store2 = store as CodeDomSerializationStore;
            if (store2 == null)
            {
                throw new InvalidOperationException(System.Design.SR.GetString("CodeDomComponentSerializationServiceUnknownStore"));
            }
            return store2.Deserialize(this._provider, container);
        }

        public override void DeserializeTo(SerializationStore store, IContainer container, bool validateRecycledTypes, bool applyDefaults)
        {
            if (store == null)
            {
                throw new ArgumentNullException("store");
            }
            if (container == null)
            {
                throw new ArgumentNullException("container");
            }
            CodeDomSerializationStore store2 = store as CodeDomSerializationStore;
            if (store2 == null)
            {
                throw new InvalidOperationException(System.Design.SR.GetString("CodeDomComponentSerializationServiceUnknownStore"));
            }
            store2.DeserializeTo(this._provider, container, validateRecycledTypes, applyDefaults);
        }

        public override SerializationStore LoadStore(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            return CodeDomSerializationStore.Load(stream);
        }

        public override void Serialize(SerializationStore store, object value)
        {
            if (store == null)
            {
                throw new ArgumentNullException("store");
            }
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            CodeDomSerializationStore store2 = store as CodeDomSerializationStore;
            if (store2 == null)
            {
                throw new InvalidOperationException(System.Design.SR.GetString("CodeDomComponentSerializationServiceUnknownStore"));
            }
            store2.AddObject(value, false);
        }

        public override void SerializeAbsolute(SerializationStore store, object value)
        {
            if (store == null)
            {
                throw new ArgumentNullException("store");
            }
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            CodeDomSerializationStore store2 = store as CodeDomSerializationStore;
            if (store2 == null)
            {
                throw new InvalidOperationException(System.Design.SR.GetString("CodeDomComponentSerializationServiceUnknownStore"));
            }
            store2.AddObject(value, true);
        }

        public override void SerializeMember(SerializationStore store, object owningObject, MemberDescriptor member)
        {
            if (store == null)
            {
                throw new ArgumentNullException("store");
            }
            if (owningObject == null)
            {
                throw new ArgumentNullException("owningObject");
            }
            if (member == null)
            {
                throw new ArgumentNullException("member");
            }
            CodeDomSerializationStore store2 = store as CodeDomSerializationStore;
            if (store2 == null)
            {
                throw new InvalidOperationException(System.Design.SR.GetString("CodeDomComponentSerializationServiceUnknownStore"));
            }
            store2.AddMember(owningObject, member, false);
        }

        public override void SerializeMemberAbsolute(SerializationStore store, object owningObject, MemberDescriptor member)
        {
            if (store == null)
            {
                throw new ArgumentNullException("store");
            }
            if (owningObject == null)
            {
                throw new ArgumentNullException("owningObject");
            }
            if (member == null)
            {
                throw new ArgumentNullException("member");
            }
            CodeDomSerializationStore store2 = store as CodeDomSerializationStore;
            if (store2 == null)
            {
                throw new InvalidOperationException(System.Design.SR.GetString("CodeDomComponentSerializationServiceUnknownStore"));
            }
            store2.AddMember(owningObject, member, true);
        }

        [Serializable]
        private sealed class CodeDomSerializationStore : SerializationStore, ISerializable
        {
            private AssemblyName[] _assemblies;
            private const string _assembliesKey = "Assemblies";
            private ICollection _errors;
            private const string _nameKey = "Names";
            private ArrayList _objectNames;
            private Hashtable _objects;
            private Hashtable _objectState;
            private IServiceProvider _provider;
            private LocalResourceManager _resources;
            private const string _resourcesKey = "Resources";
            private MemoryStream _resourceStream;
            private const string _shimKey = "Shim";
            private List<string> _shimObjectNames;
            private const int _stateCode = 0;
            private const int _stateCtx = 1;
            private const int _stateEvents = 4;
            private const string _stateKey = "State";
            private const int _stateModifier = 5;
            private const int _stateProperties = 2;
            private const int _stateResources = 3;

            internal CodeDomSerializationStore(IServiceProvider provider)
            {
                this._provider = provider;
                this._objects = new Hashtable();
                this._objectNames = new ArrayList();
                this._shimObjectNames = new List<string>();
            }

            private CodeDomSerializationStore(SerializationInfo info, StreamingContext context)
            {
                this._objectState = (Hashtable) info.GetValue("State", typeof(Hashtable));
                this._objectNames = (ArrayList) info.GetValue("Names", typeof(ArrayList));
                this._assemblies = (AssemblyName[]) info.GetValue("Assemblies", typeof(AssemblyName[]));
                this._shimObjectNames = (List<string>) info.GetValue("Shim", typeof(List<string>));
                Hashtable data = (Hashtable) info.GetValue("Resources", typeof(Hashtable));
                if (data != null)
                {
                    this._resources = new LocalResourceManager(data);
                }
            }

            internal void AddMember(object value, MemberDescriptor member, bool absolute)
            {
                if (this._objectState != null)
                {
                    throw new InvalidOperationException(System.Design.SR.GetString("CodeDomComponentSerializationServiceClosedStore"));
                }
                ObjectData data = (ObjectData) this._objects[value];
                if (data == null)
                {
                    data = new ObjectData {
                        Name = this.GetObjectName(value),
                        Value = value
                    };
                    this._objects[value] = data;
                    this._objectNames.Add(data.Name);
                }
                data.Members.Add(new MemberData(member, absolute));
            }

            internal void AddObject(object value, bool absolute)
            {
                if (this._objectState != null)
                {
                    throw new InvalidOperationException(System.Design.SR.GetString("CodeDomComponentSerializationServiceClosedStore"));
                }
                ObjectData data = (ObjectData) this._objects[value];
                if (data == null)
                {
                    data = new ObjectData {
                        Name = this.GetObjectName(value),
                        Value = value
                    };
                    this._objects[value] = data;
                    this._objectNames.Add(data.Name);
                }
                data.EntireObject = true;
                data.Absolute = absolute;
            }

            public override void Close()
            {
                if (this._objectState == null)
                {
                    Hashtable objectState = new Hashtable(this._objects.Count);
                    DesignerSerializationManager manager = new DesignerSerializationManager(new LocalServices(this, this._provider));
                    DesignerSerializationManager service = this._provider.GetService(typeof(IDesignerSerializationManager)) as DesignerSerializationManager;
                    if (service != null)
                    {
                        foreach (IDesignerSerializationProvider provider in service.SerializationProviders)
                        {
                            ((IDesignerSerializationManager) manager).AddSerializationProvider(provider);
                        }
                    }
                    using (manager.CreateSession())
                    {
                        foreach (ObjectData data in this._objects.Values)
                        {
                            ((IDesignerSerializationManager) manager).SetName(data.Value, data.Name);
                        }
                        ComponentListCodeDomSerializer.Instance.Serialize(manager, this._objects, objectState, this._shimObjectNames);
                        this._errors = manager.Errors;
                    }
                    if ((this._resources != null) && (this._resourceStream == null))
                    {
                        BinaryFormatter formatter = new BinaryFormatter();
                        this._resourceStream = new MemoryStream();
                        formatter.Serialize(this._resourceStream, this._resources.Data);
                    }
                    Hashtable hashtable2 = new Hashtable(this._objects.Count);
                    foreach (object obj2 in this._objects.Keys)
                    {
                        Assembly assembly = obj2.GetType().Assembly;
                        hashtable2[assembly] = null;
                    }
                    this._assemblies = new AssemblyName[hashtable2.Count];
                    int num = 0;
                    foreach (Assembly assembly2 in hashtable2.Keys)
                    {
                        this._assemblies[num++] = assembly2.GetName(true);
                    }
                    this._objectState = objectState;
                    this._objects = null;
                }
            }

            internal ICollection Deserialize(IServiceProvider provider)
            {
                return this.Deserialize(provider, null, false, true, true);
            }

            internal ICollection Deserialize(IServiceProvider provider, IContainer container)
            {
                return this.Deserialize(provider, container, false, true, true);
            }

            private ICollection Deserialize(IServiceProvider provider, IContainer container, bool recycleInstances, bool validateRecycledTypes, bool applyDefaults)
            {
                PassThroughSerializationManager manager = new PassThroughSerializationManager(new LocalDesignerSerializationManager(this, new LocalServices(this, provider)));
                if (container != null)
                {
                    manager.Manager.Container = container;
                }
                DesignerSerializationManager service = provider.GetService(typeof(IDesignerSerializationManager)) as DesignerSerializationManager;
                if (service != null)
                {
                    foreach (IDesignerSerializationProvider provider2 in service.SerializationProviders)
                    {
                        ((IDesignerSerializationManager) manager.Manager).AddSerializationProvider(provider2);
                    }
                }
                manager.Manager.RecycleInstances = recycleInstances;
                manager.Manager.PreserveNames = recycleInstances;
                manager.Manager.ValidateRecycledTypes = validateRecycledTypes;
                ArrayList list = null;
                if (this._resourceStream != null)
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    this._resourceStream.Seek(0L, SeekOrigin.Begin);
                    Hashtable data = formatter.Deserialize(this._resourceStream) as Hashtable;
                    this._resources = new LocalResourceManager(data);
                }
                if (!recycleInstances)
                {
                    list = new ArrayList(this._objectNames.Count);
                }
                using (manager.Manager.CreateSession())
                {
                    if (this._shimObjectNames.Count > 0)
                    {
                        List<string> list2 = this._shimObjectNames;
                        IDesignerSerializationManager manager3 = manager;
                        if ((manager3 != null) && (container != null))
                        {
                            foreach (string str in list2)
                            {
                                object instance = container.Components[str];
                                if ((instance != null) && (manager3.GetInstance(str) == null))
                                {
                                    manager3.SetName(instance, str);
                                }
                            }
                        }
                    }
                    ComponentListCodeDomSerializer.Instance.Deserialize(manager, this._objectState, this._objectNames, applyDefaults);
                    if (!recycleInstances)
                    {
                        foreach (string str2 in this._objectNames)
                        {
                            object obj3 = ((IDesignerSerializationManager) manager.Manager).GetInstance(str2);
                            if (obj3 != null)
                            {
                                list.Add(obj3);
                            }
                        }
                    }
                    this._errors = manager.Manager.Errors;
                }
                return list;
            }

            internal void DeserializeTo(IServiceProvider provider, IContainer container, bool validateRecycledTypes, bool applyDefaults)
            {
                this.Deserialize(provider, container, true, validateRecycledTypes, applyDefaults);
            }

            private string GetObjectName(object value)
            {
                IComponent component = value as IComponent;
                if (component != null)
                {
                    ISite site = component.Site;
                    if (site != null)
                    {
                        INestedSite site2 = site as INestedSite;
                        if ((site2 != null) && !string.IsNullOrEmpty(site2.FullName))
                        {
                            return site2.FullName;
                        }
                        if (!string.IsNullOrEmpty(site.Name))
                        {
                            return site.Name;
                        }
                    }
                }
                string str = Guid.NewGuid().ToString().Replace("-", "_");
                return string.Format(CultureInfo.CurrentCulture, "object_{0}", new object[] { str });
            }

            internal static CodeDomComponentSerializationService.CodeDomSerializationStore Load(Stream stream)
            {
                BinaryFormatter formatter = new BinaryFormatter();
                return (CodeDomComponentSerializationService.CodeDomSerializationStore) formatter.Deserialize(stream);
            }

            public override void Save(Stream stream)
            {
                this.Close();
                new BinaryFormatter().Serialize(stream, this);
            }

            void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
            {
                Hashtable data = null;
                if (this._resources != null)
                {
                    data = this._resources.Data;
                }
                info.AddValue("State", this._objectState);
                info.AddValue("Names", this._objectNames);
                info.AddValue("Assemblies", this._assemblies);
                info.AddValue("Resources", data);
                info.AddValue("Shim", this._shimObjectNames);
            }

            [Conditional("DEBUG")]
            internal static void Trace(string message, params object[] args)
            {
            }

            private AssemblyName[] AssemblyNames
            {
                get
                {
                    return this._assemblies;
                }
            }

            public override ICollection Errors
            {
                get
                {
                    if (this._errors == null)
                    {
                        this._errors = new object[0];
                    }
                    object[] array = new object[this._errors.Count];
                    this._errors.CopyTo(array, 0);
                    return array;
                }
            }

            private LocalResourceManager Resources
            {
                get
                {
                    if (this._resources == null)
                    {
                        this._resources = new LocalResourceManager();
                    }
                    return this._resources;
                }
            }

            private class ComponentListCodeDomSerializer : CodeDomSerializer
            {
                private Dictionary<string, ArrayList> _expressions;
                private Hashtable _nameResolveGuard = new Hashtable();
                private Hashtable _objectState;
                private Hashtable _statementsTable;
                private bool applyDefaults = true;
                internal static CodeDomComponentSerializationService.CodeDomSerializationStore.ComponentListCodeDomSerializer Instance = new CodeDomComponentSerializationService.CodeDomSerializationStore.ComponentListCodeDomSerializer();

                public override object Deserialize(IDesignerSerializationManager manager, object state)
                {
                    throw new NotSupportedException();
                }

                internal void Deserialize(IDesignerSerializationManager manager, IDictionary objectState, IList objectNames, bool applyDefaults)
                {
                    CodeStatementCollection completeStatements = new CodeStatementCollection();
                    this._expressions = new Dictionary<string, ArrayList>();
                    this.applyDefaults = applyDefaults;
                    foreach (string str in objectNames)
                    {
                        object[] objArray = (object[]) objectState[str];
                        if (objArray != null)
                        {
                            if (objArray[0] != null)
                            {
                                this.PopulateCompleteStatements(objArray[0], str, completeStatements);
                            }
                            if (objArray[1] != null)
                            {
                                this.PopulateCompleteStatements(objArray[1], str, completeStatements);
                            }
                        }
                    }
                    CodeStatementCollection targetStatements = new CodeStatementCollection();
                    CodeMethodMap map = new CodeMethodMap(targetStatements, null);
                    map.Add(completeStatements);
                    map.Combine();
                    this._statementsTable = new Hashtable();
                    CodeDomSerializerBase.FillStatementTable(manager, this._statementsTable, targetStatements);
                    ArrayList list = new ArrayList(objectNames);
                    foreach (string str2 in this._statementsTable.Keys)
                    {
                        if (!list.Contains(str2))
                        {
                            list.Add(str2);
                        }
                    }
                    this._objectState = new Hashtable(objectState.Keys.Count);
                    foreach (DictionaryEntry entry in objectState)
                    {
                        this._objectState.Add(entry.Key, entry.Value);
                    }
                    ResolveNameEventHandler handler = new ResolveNameEventHandler(this.OnResolveName);
                    manager.ResolveName += handler;
                    try
                    {
                        foreach (string str3 in list)
                        {
                            this.ResolveName(manager, str3, true);
                        }
                    }
                    finally
                    {
                        this._objectState = null;
                        manager.ResolveName -= handler;
                    }
                }

                private void DeserializeDefaultProperties(IDesignerSerializationManager manager, string name, object state)
                {
                    if ((state != null) && this.applyDefaults)
                    {
                        object instance = manager.GetInstance(name);
                        if (instance != null)
                        {
                            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(instance);
                            string[] strArray = (string[]) state;
                            MemberRelationshipService service = manager.GetService(typeof(MemberRelationshipService)) as MemberRelationshipService;
                            foreach (string str in strArray)
                            {
                                PropertyDescriptor descriptor = properties[str];
                                if ((descriptor != null) && descriptor.CanResetValue(instance))
                                {
                                    if ((service != null) && (service[instance, descriptor] != MemberRelationship.Empty))
                                    {
                                        service[instance, descriptor] = MemberRelationship.Empty;
                                    }
                                    descriptor.ResetValue(instance);
                                }
                            }
                        }
                    }
                }

                private void DeserializeDesignTimeProperties(IDesignerSerializationManager manager, string name, object state)
                {
                    if (state != null)
                    {
                        object instance = manager.GetInstance(name);
                        if (instance != null)
                        {
                            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(instance);
                            foreach (DictionaryEntry entry in (IDictionary) state)
                            {
                                PropertyDescriptor descriptor = properties[(string) entry.Key];
                                if (descriptor != null)
                                {
                                    descriptor.SetValue(instance, entry.Value);
                                }
                            }
                        }
                    }
                }

                private void DeserializeEventResets(IDesignerSerializationManager manager, string name, object state)
                {
                    List<string> list = state as List<string>;
                    if (((list != null) && (manager != null)) && !string.IsNullOrEmpty(name))
                    {
                        IEventBindingService service = manager.GetService(typeof(IEventBindingService)) as IEventBindingService;
                        object instance = manager.GetInstance(name);
                        if ((instance != null) && (service != null))
                        {
                            PropertyDescriptorCollection eventProperties = service.GetEventProperties(TypeDescriptor.GetEvents(instance));
                            if (eventProperties != null)
                            {
                                foreach (string str in list)
                                {
                                    PropertyDescriptor descriptor = eventProperties[str];
                                    if (descriptor != null)
                                    {
                                        descriptor.SetValue(instance, null);
                                    }
                                }
                            }
                        }
                    }
                }

                private static void DeserializeModifier(IDesignerSerializationManager manager, string name, object state)
                {
                    object instance = manager.GetInstance(name);
                    if (instance != null)
                    {
                        MemberAttributes attributes = (MemberAttributes) state;
                        PropertyDescriptor descriptor = TypeDescriptor.GetProperties(instance)["Modifiers"];
                        if (descriptor != null)
                        {
                            descriptor.SetValue(instance, attributes);
                        }
                    }
                }

                private void OnResolveName(object sender, ResolveNameEventArgs e)
                {
                    if (!this._nameResolveGuard.ContainsKey(e.Name))
                    {
                        this._nameResolveGuard.Add(e.Name, true);
                        try
                        {
                            IDesignerSerializationManager manager = (IDesignerSerializationManager) sender;
                            if (this.ResolveName(manager, e.Name, false))
                            {
                                e.Value = manager.GetInstance(e.Name);
                            }
                        }
                        finally
                        {
                            this._nameResolveGuard.Remove(e.Name);
                        }
                    }
                }

                private void PopulateCompleteStatements(object data, string name, CodeStatementCollection completeStatements)
                {
                    CodeStatementCollection statements = data as CodeStatementCollection;
                    if (statements != null)
                    {
                        completeStatements.AddRange(statements);
                    }
                    else
                    {
                        CodeStatement statement = data as CodeStatement;
                        if (statement != null)
                        {
                            completeStatements.Add(statement);
                        }
                        else
                        {
                            CodeExpression expression = data as CodeExpression;
                            if (expression != null)
                            {
                                ArrayList list = null;
                                if (this._expressions.ContainsKey(name))
                                {
                                    list = this._expressions[name];
                                }
                                if (list == null)
                                {
                                    list = new ArrayList();
                                    this._expressions[name] = list;
                                }
                                list.Add(expression);
                            }
                        }
                    }
                }

                private bool ResolveName(IDesignerSerializationManager manager, string name, bool canInvokeManager)
                {
                    bool flag = false;
                    CodeDomSerializerBase.OrderedCodeStatementCollection codeObject = this._statementsTable[name] as CodeDomSerializerBase.OrderedCodeStatementCollection;
                    object[] objArray = (object[]) this._objectState[name];
                    if (name.IndexOf('.') > 0)
                    {
                        string outerComponent = null;
                        IComponent instance = this.ResolveNestedName(manager, name, ref outerComponent);
                        if ((instance != null) && (outerComponent != null))
                        {
                            manager.SetName(instance, name);
                            this.ResolveName(manager, outerComponent, canInvokeManager);
                        }
                    }
                    if (codeObject == null)
                    {
                        flag = this._statementsTable[name] != null;
                        if (!flag)
                        {
                            if (this._expressions.ContainsKey(name))
                            {
                                ArrayList list2 = this._expressions[name];
                                foreach (CodeExpression expression2 in list2)
                                {
                                    object obj3 = base.DeserializeExpression(manager, name, expression2);
                                    if (((obj3 != null) && !flag) && (canInvokeManager && (manager.GetInstance(name) == null)))
                                    {
                                        manager.SetName(obj3, name);
                                        flag = true;
                                    }
                                }
                            }
                            if (!flag && canInvokeManager)
                            {
                                flag = manager.GetInstance(name) != null;
                            }
                            if ((flag && (objArray != null)) && (objArray[2] != null))
                            {
                                this.DeserializeDefaultProperties(manager, name, objArray[2]);
                            }
                            if ((flag && (objArray != null)) && (objArray[3] != null))
                            {
                                this.DeserializeDesignTimeProperties(manager, name, objArray[3]);
                            }
                            if ((flag && (objArray != null)) && (objArray[4] != null))
                            {
                                this.DeserializeEventResets(manager, name, objArray[4]);
                            }
                            if ((flag && (objArray != null)) && (objArray[5] != null))
                            {
                                DeserializeModifier(manager, name, objArray[5]);
                            }
                        }
                        if (!flag && (flag || canInvokeManager))
                        {
                            manager.ReportError(new CodeDomSerializerException(System.Design.SR.GetString("CodeDomComponentSerializationServiceDeserializationError", new object[] { name }), manager));
                        }
                        return flag;
                    }
                    this._objectState[name] = null;
                    this._statementsTable[name] = null;
                    string typeName = null;
                    foreach (CodeStatement statement in codeObject)
                    {
                        CodeVariableDeclarationStatement statement2 = statement as CodeVariableDeclarationStatement;
                        if (statement2 != null)
                        {
                            typeName = statement2.Type.BaseType;
                            break;
                        }
                    }
                    if (typeName != null)
                    {
                        Type valueType = manager.GetType(typeName);
                        if (valueType == null)
                        {
                            manager.ReportError(new CodeDomSerializerException(System.Design.SR.GetString("SerializerTypeNotFound", new object[] { typeName }), manager));
                            goto Label_01DA;
                        }
                        if ((codeObject == null) || (codeObject.Count <= 0))
                        {
                            goto Label_01DA;
                        }
                        CodeDomSerializer serializer = base.GetSerializer(manager, valueType);
                        if (serializer == null)
                        {
                            manager.ReportError(new CodeDomSerializerException(System.Design.SR.GetString("SerializerNoSerializerForComponent", new object[] { valueType.FullName }), manager));
                            goto Label_01DA;
                        }
                        try
                        {
                            object obj2 = serializer.Deserialize(manager, codeObject);
                            flag = obj2 != null;
                            if (flag)
                            {
                                this._statementsTable[name] = obj2;
                            }
                            goto Label_01DA;
                        }
                        catch (Exception exception)
                        {
                            manager.ReportError(exception);
                            goto Label_01DA;
                        }
                    }
                    foreach (CodeStatement statement3 in codeObject)
                    {
                        base.DeserializeStatement(manager, statement3);
                    }
                    flag = true;
                Label_01DA:
                    if ((objArray != null) && (objArray[2] != null))
                    {
                        this.DeserializeDefaultProperties(manager, name, objArray[2]);
                    }
                    if ((objArray != null) && (objArray[3] != null))
                    {
                        this.DeserializeDesignTimeProperties(manager, name, objArray[3]);
                    }
                    if ((objArray != null) && (objArray[4] != null))
                    {
                        this.DeserializeEventResets(manager, name, objArray[4]);
                    }
                    if ((objArray != null) && (objArray[5] != null))
                    {
                        DeserializeModifier(manager, name, objArray[5]);
                    }
                    if (!this._expressions.ContainsKey(name))
                    {
                        return flag;
                    }
                    ArrayList list = this._expressions[name];
                    foreach (CodeExpression expression in list)
                    {
                        base.DeserializeExpression(manager, name, expression);
                    }
                    this._expressions.Remove(name);
                    return true;
                }

                private IComponent ResolveNestedName(IDesignerSerializationManager manager, string name, ref string outerComponent)
                {
                    IComponent instance = null;
                    if ((name != null) && (manager != null))
                    {
                        bool flag = true;
                        int index = name.IndexOf('.', 0);
                        outerComponent = name.Substring(0, index);
                        instance = manager.GetInstance(outerComponent) as IComponent;
                        int num2 = index;
                        int length = name.IndexOf('.', index + 1);
                        while (flag)
                        {
                            flag = length != -1;
                            string str = flag ? name.Substring(num2 + 1, length) : name.Substring(num2 + 1);
                            if ((instance != null) && (instance.Site != null))
                            {
                                INestedContainer service = instance.Site.GetService(typeof(INestedContainer)) as INestedContainer;
                                if ((service != null) && !string.IsNullOrEmpty(str))
                                {
                                    instance = service.Components[str];
                                    goto Label_00B7;
                                }
                            }
                            return null;
                        Label_00B7:
                            if (flag)
                            {
                                num2 = length;
                                length = name.IndexOf('.', length + 1);
                            }
                        }
                    }
                    return instance;
                }

                public override object Serialize(IDesignerSerializationManager manager, object state)
                {
                    throw new NotSupportedException();
                }

                internal void Serialize(IDesignerSerializationManager manager, IDictionary objectData, IDictionary objectState, IList shimObjectNames)
                {
                    IContainer container = manager.GetService(typeof(IContainer)) as IContainer;
                    if (container != null)
                    {
                        this.SetupVariableReferences(manager, container, objectData, shimObjectNames);
                    }
                    StatementContext context = new StatementContext();
                    context.StatementCollection.Populate(objectData.Keys);
                    manager.Context.Push(context);
                    try
                    {
                        foreach (CodeDomComponentSerializationService.CodeDomSerializationStore.ObjectData data in objectData.Values)
                        {
                            CodeDomSerializer serializer = (CodeDomSerializer) manager.GetSerializer(data.Value.GetType(), typeof(CodeDomSerializer));
                            object[] objArray = new object[6];
                            CodeStatementCollection statements = new CodeStatementCollection();
                            manager.Context.Push(statements);
                            if (serializer != null)
                            {
                                if (data.EntireObject)
                                {
                                    if (!base.IsSerialized(manager, data.Value))
                                    {
                                        if (data.Absolute)
                                        {
                                            objArray[0] = serializer.SerializeAbsolute(manager, data.Value);
                                        }
                                        else
                                        {
                                            objArray[0] = serializer.Serialize(manager, data.Value);
                                        }
                                        CodeStatementCollection statements2 = context.StatementCollection[data.Value];
                                        if ((statements2 != null) && (statements2.Count > 0))
                                        {
                                            objArray[1] = statements2;
                                        }
                                        if (statements.Count > 0)
                                        {
                                            CodeStatementCollection statements3 = objArray[0] as CodeStatementCollection;
                                            if (statements3 != null)
                                            {
                                                statements3.AddRange(statements);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        objArray[0] = context.StatementCollection[data.Value];
                                    }
                                }
                                else
                                {
                                    CodeStatementCollection statements4 = new CodeStatementCollection();
                                    foreach (CodeDomComponentSerializationService.CodeDomSerializationStore.MemberData data2 in data.Members)
                                    {
                                        if (data2.Member.Attributes.Contains(DesignOnlyAttribute.Yes))
                                        {
                                            PropertyDescriptor member = data2.Member as PropertyDescriptor;
                                            if ((member != null) && member.PropertyType.IsSerializable)
                                            {
                                                if (objArray[3] == null)
                                                {
                                                    objArray[3] = new Hashtable();
                                                }
                                                ((Hashtable) objArray[3])[member.Name] = member.GetValue(data.Value);
                                            }
                                        }
                                        else if (data2.Absolute)
                                        {
                                            statements4.AddRange(serializer.SerializeMemberAbsolute(manager, data.Value, data2.Member));
                                        }
                                        else
                                        {
                                            statements4.AddRange(serializer.SerializeMember(manager, data.Value, data2.Member));
                                        }
                                    }
                                    objArray[0] = statements4;
                                }
                            }
                            if (statements.Count > 0)
                            {
                                CodeStatementCollection statements5 = objArray[0] as CodeStatementCollection;
                                if (statements5 != null)
                                {
                                    statements5.AddRange(statements);
                                }
                            }
                            manager.Context.Pop();
                            ArrayList list = null;
                            List<string> list2 = null;
                            IEventBindingService service = manager.GetService(typeof(IEventBindingService)) as IEventBindingService;
                            if (data.EntireObject)
                            {
                                foreach (PropertyDescriptor descriptor2 in TypeDescriptor.GetProperties(data.Value))
                                {
                                    if ((!descriptor2.ShouldSerializeValue(data.Value) && !descriptor2.Attributes.Contains(DesignerSerializationVisibilityAttribute.Hidden)) && (descriptor2.Attributes.Contains(DesignerSerializationVisibilityAttribute.Content) || !descriptor2.IsReadOnly))
                                    {
                                        if (list == null)
                                        {
                                            list = new ArrayList(data.Members.Count);
                                        }
                                        list.Add(descriptor2.Name);
                                    }
                                }
                                if (service != null)
                                {
                                    foreach (PropertyDescriptor descriptor3 in service.GetEventProperties(TypeDescriptor.GetEvents(data.Value)))
                                    {
                                        if (((descriptor3 != null) && !descriptor3.IsReadOnly) && (descriptor3.GetValue(data.Value) == null))
                                        {
                                            if (list2 == null)
                                            {
                                                list2 = new List<string>();
                                            }
                                            list2.Add(descriptor3.Name);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                foreach (CodeDomComponentSerializationService.CodeDomSerializationStore.MemberData data3 in data.Members)
                                {
                                    PropertyDescriptor property = data3.Member as PropertyDescriptor;
                                    if ((property != null) && !property.ShouldSerializeValue(data.Value))
                                    {
                                        if ((service != null) && (service.GetEvent(property) != null))
                                        {
                                            if (list2 == null)
                                            {
                                                list2 = new List<string>();
                                            }
                                            list2.Add(property.Name);
                                        }
                                        else
                                        {
                                            if (list == null)
                                            {
                                                list = new ArrayList(data.Members.Count);
                                            }
                                            list.Add(property.Name);
                                        }
                                    }
                                }
                            }
                            PropertyDescriptor descriptor5 = TypeDescriptor.GetProperties(data.Value)["Modifiers"];
                            if (descriptor5 != null)
                            {
                                objArray[5] = descriptor5.GetValue(data.Value);
                            }
                            if (list != null)
                            {
                                objArray[2] = (string[]) list.ToArray(typeof(string));
                            }
                            if (list2 != null)
                            {
                                objArray[4] = list2;
                            }
                            if ((objArray[0] != null) || (objArray[2] != null))
                            {
                                objectState[data.Name] = objArray;
                            }
                        }
                    }
                    finally
                    {
                        manager.Context.Pop();
                    }
                }

                internal void SetupVariableReferences(IDesignerSerializationManager manager, IContainer container, IDictionary objectData, IList shimObjectNames)
                {
                    foreach (IComponent component in container.Components)
                    {
                        string componentName = TypeDescriptor.GetComponentName(component);
                        if ((componentName != null) && (componentName.Length > 0))
                        {
                            bool flag = true;
                            if (objectData.Contains(component) && ((CodeDomComponentSerializationService.CodeDomSerializationStore.ObjectData) objectData[component]).EntireObject)
                            {
                                flag = false;
                            }
                            if (flag)
                            {
                                CodeVariableReferenceExpression expression = new CodeVariableReferenceExpression(componentName);
                                base.SetExpression(manager, component, expression);
                                if (!shimObjectNames.Contains(componentName))
                                {
                                    shimObjectNames.Add(componentName);
                                }
                                if (component.Site != null)
                                {
                                    INestedContainer service = component.Site.GetService(typeof(INestedContainer)) as INestedContainer;
                                    if ((service != null) && (service.Components.Count > 0))
                                    {
                                        this.SetupVariableReferences(manager, service, objectData, shimObjectNames);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            private class LocalDesignerSerializationManager : DesignerSerializationManager
            {
                private CodeDomComponentSerializationService.CodeDomSerializationStore _store;
                private bool? _typeSvcAvailable;

                internal LocalDesignerSerializationManager(CodeDomComponentSerializationService.CodeDomSerializationStore store, IServiceProvider provider) : base(provider)
                {
                    this._typeSvcAvailable = null;
                    this._store = store;
                }

                protected override object CreateInstance(Type type, ICollection arguments, string name, bool addToContainer)
                {
                    if (typeof(ResourceManager).IsAssignableFrom(type))
                    {
                        return this._store.Resources;
                    }
                    return base.CreateInstance(type, arguments, name, addToContainer);
                }

                protected override Type GetType(string name)
                {
                    Type type = base.GetType(name);
                    if ((type == null) && !this.TypeResolutionAvailable.Value)
                    {
                        AssemblyName[] assemblyNames = this._store.AssemblyNames;
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

                private bool? TypeResolutionAvailable
                {
                    get
                    {
                        if (!this._typeSvcAvailable.HasValue)
                        {
                            this._typeSvcAvailable = new bool?(this.GetService(typeof(ITypeResolutionService)) != null);
                        }
                        return new bool?(this._typeSvcAvailable.Value);
                    }
                }
            }

            private class LocalResourceManager : ResourceManager, IResourceWriter, IResourceReader, IEnumerable, IDisposable
            {
                private Hashtable _hashtable;

                internal LocalResourceManager()
                {
                }

                internal LocalResourceManager(Hashtable data)
                {
                    this._hashtable = data;
                }

                public void AddResource(string name, object value)
                {
                    this.Data[name] = value;
                }

                public void AddResource(string name, string value)
                {
                    this.Data[name] = value;
                }

                public void AddResource(string name, byte[] value)
                {
                    this.Data[name] = value;
                }

                public void Close()
                {
                }

                public void Dispose()
                {
                    this.Data.Clear();
                }

                public void Generate()
                {
                }

                public IDictionaryEnumerator GetEnumerator()
                {
                    return this.Data.GetEnumerator();
                }

                public override object GetObject(string name)
                {
                    return this.Data[name];
                }

                public override string GetString(string name)
                {
                    return (this.Data[name] as string);
                }

                IEnumerator IEnumerable.GetEnumerator()
                {
                    return this.GetEnumerator();
                }

                internal Hashtable Data
                {
                    get
                    {
                        if (this._hashtable == null)
                        {
                            this._hashtable = new Hashtable();
                        }
                        return this._hashtable;
                    }
                }
            }

            private class LocalServices : IServiceProvider, IResourceService
            {
                private IServiceProvider _provider;
                private CodeDomComponentSerializationService.CodeDomSerializationStore _store;

                internal LocalServices(CodeDomComponentSerializationService.CodeDomSerializationStore store, IServiceProvider provider)
                {
                    this._store = store;
                    this._provider = provider;
                }

                IResourceReader IResourceService.GetResourceReader(CultureInfo info)
                {
                    return this._store.Resources;
                }

                IResourceWriter IResourceService.GetResourceWriter(CultureInfo info)
                {
                    return this._store.Resources;
                }

                object IServiceProvider.GetService(Type serviceType)
                {
                    if (serviceType == null)
                    {
                        throw new ArgumentNullException("serviceType");
                    }
                    if (serviceType == typeof(IResourceService))
                    {
                        return this;
                    }
                    if (this._provider != null)
                    {
                        return this._provider.GetService(serviceType);
                    }
                    return null;
                }
            }

            private class MemberData
            {
                internal bool Absolute;
                internal MemberDescriptor Member;

                internal MemberData(MemberDescriptor member, bool absolute)
                {
                    this.Member = member;
                    this.Absolute = absolute;
                }
            }

            private class ObjectData
            {
                private bool _absolute;
                private bool _entireObject;
                private ArrayList _members;
                internal string Name;
                internal object Value;

                internal bool Absolute
                {
                    get
                    {
                        return this._absolute;
                    }
                    set
                    {
                        this._absolute = value;
                    }
                }

                internal bool EntireObject
                {
                    get
                    {
                        return this._entireObject;
                    }
                    set
                    {
                        if (value && (this._members != null))
                        {
                            this._members.Clear();
                        }
                        this._entireObject = value;
                    }
                }

                internal IList Members
                {
                    get
                    {
                        if (this._members == null)
                        {
                            this._members = new ArrayList();
                        }
                        return this._members;
                    }
                }
            }

            private class PassThroughSerializationManager : IDesignerSerializationManager, IServiceProvider
            {
                private DesignerSerializationManager manager;
                private Hashtable resolved = new Hashtable();
                private ResolveNameEventHandler resolveNameEventHandler;

                event ResolveNameEventHandler IDesignerSerializationManager.ResolveName
                {
                    add
                    {
                        this.manager.ResolveName += value;
                        this.resolveNameEventHandler = (ResolveNameEventHandler) Delegate.Combine(this.resolveNameEventHandler, value);
                    }
                    remove
                    {
                        this.manager.ResolveName -= value;
                        this.resolveNameEventHandler = (ResolveNameEventHandler) Delegate.Remove(this.resolveNameEventHandler, value);
                    }
                }

                event EventHandler IDesignerSerializationManager.SerializationComplete
                {
                    add
                    {
                        this.manager.SerializationComplete += value;
                    }
                    remove
                    {
                        this.manager.SerializationComplete -= value;
                    }
                }

                public PassThroughSerializationManager(DesignerSerializationManager manager)
                {
                    this.manager = manager;
                }

                void IDesignerSerializationManager.AddSerializationProvider(IDesignerSerializationProvider provider)
                {
                    ((IDesignerSerializationManager) this.manager).AddSerializationProvider(provider);
                }

                object IDesignerSerializationManager.CreateInstance(Type type, ICollection arguments, string name, bool addToContainer)
                {
                    return ((IDesignerSerializationManager) this.manager).CreateInstance(type, arguments, name, addToContainer);
                }

                object IDesignerSerializationManager.GetInstance(string name)
                {
                    object instance = ((IDesignerSerializationManager) this.manager).GetInstance(name);
                    if ((((this.resolveNameEventHandler != null) && (instance != null)) && (!this.resolved.ContainsKey(name) && this.manager.PreserveNames)) && ((this.manager.Container != null) && (this.manager.Container.Components[name] != null)))
                    {
                        this.resolved[name] = true;
                        this.resolveNameEventHandler(this, new ResolveNameEventArgs(name));
                    }
                    return instance;
                }

                string IDesignerSerializationManager.GetName(object value)
                {
                    return ((IDesignerSerializationManager) this.manager).GetName(value);
                }

                object IDesignerSerializationManager.GetSerializer(Type objectType, Type serializerType)
                {
                    return ((IDesignerSerializationManager) this.manager).GetSerializer(objectType, serializerType);
                }

                Type IDesignerSerializationManager.GetType(string typeName)
                {
                    return ((IDesignerSerializationManager) this.manager).GetType(typeName);
                }

                void IDesignerSerializationManager.RemoveSerializationProvider(IDesignerSerializationProvider provider)
                {
                    ((IDesignerSerializationManager) this.manager).RemoveSerializationProvider(provider);
                }

                void IDesignerSerializationManager.ReportError(object errorInformation)
                {
                    ((IDesignerSerializationManager) this.manager).ReportError(errorInformation);
                }

                void IDesignerSerializationManager.SetName(object instance, string name)
                {
                    ((IDesignerSerializationManager) this.manager).SetName(instance, name);
                }

                object IServiceProvider.GetService(Type serviceType)
                {
                    return ((IServiceProvider) this.manager).GetService(serviceType);
                }

                public DesignerSerializationManager Manager
                {
                    get
                    {
                        return this.manager;
                    }
                }

                ContextStack IDesignerSerializationManager.Context
                {
                    get
                    {
                        return ((IDesignerSerializationManager) this.manager).Context;
                    }
                }

                PropertyDescriptorCollection IDesignerSerializationManager.Properties
                {
                    get
                    {
                        return ((IDesignerSerializationManager) this.manager).Properties;
                    }
                }
            }
        }
    }
}

