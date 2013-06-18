namespace System.ComponentModel.Design.Serialization
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Design;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Text;

    public class DesignerSerializationManager : IDesignerSerializationManager, IServiceProvider
    {
        private IContainer container;
        private ContextStack contextStack;
        private Hashtable defaultProviderTable;
        private ArrayList designerSerializationProviders;
        private ArrayList errorList;
        private Hashtable instancesByName;
        private Hashtable namesByInstance;
        private bool preserveNames;
        private PropertyDescriptorCollection properties;
        private object propertyProvider;
        private IServiceProvider provider;
        private bool recycleInstances;
        private ResolveNameEventHandler resolveNameEventHandler;
        private bool searchedTypeResolver;
        private EventHandler serializationCompleteEventHandler;
        private Hashtable serializers;
        private IDisposable session;
        private ITypeResolutionService typeResolver;
        private bool validateRecycledTypes;

        public event EventHandler SessionCreated;

        public event EventHandler SessionDisposed;

        event ResolveNameEventHandler IDesignerSerializationManager.ResolveName
        {
            add
            {
                this.CheckSession();
                this.resolveNameEventHandler = (ResolveNameEventHandler) Delegate.Combine(this.resolveNameEventHandler, value);
            }
            remove
            {
                this.resolveNameEventHandler = (ResolveNameEventHandler) Delegate.Remove(this.resolveNameEventHandler, value);
            }
        }

        event EventHandler IDesignerSerializationManager.SerializationComplete
        {
            add
            {
                this.CheckSession();
                this.serializationCompleteEventHandler = (EventHandler) Delegate.Combine(this.serializationCompleteEventHandler, value);
            }
            remove
            {
                this.serializationCompleteEventHandler = (EventHandler) Delegate.Remove(this.serializationCompleteEventHandler, value);
            }
        }

        public DesignerSerializationManager()
        {
            this.preserveNames = true;
            this.validateRecycledTypes = true;
        }

        public DesignerSerializationManager(IServiceProvider provider)
        {
            if (provider == null)
            {
                throw new ArgumentNullException("provider");
            }
            this.provider = provider;
            this.preserveNames = true;
            this.validateRecycledTypes = true;
        }

        private void CheckNoSession()
        {
            if (this.session != null)
            {
                throw new InvalidOperationException(System.Design.SR.GetString("SerializationManagerWithinSession"));
            }
        }

        private void CheckSession()
        {
            if (this.session == null)
            {
                throw new InvalidOperationException(System.Design.SR.GetString("SerializationManagerNoSession"));
            }
        }

        protected virtual object CreateInstance(Type type, ICollection arguments, string name, bool addToContainer)
        {
            object[] array = null;
            if ((arguments != null) && (arguments.Count > 0))
            {
                array = new object[arguments.Count];
                arguments.CopyTo(array, 0);
            }
            object obj2 = null;
            if (this.RecycleInstances && (name != null))
            {
                if (this.instancesByName != null)
                {
                    obj2 = this.instancesByName[name];
                }
                if (((obj2 == null) && addToContainer) && (this.Container != null))
                {
                    obj2 = this.Container.Components[name];
                }
                if (((obj2 != null) && this.ValidateRecycledTypes) && (obj2.GetType() != type))
                {
                    obj2 = null;
                }
            }
            if ((((obj2 == null) && addToContainer) && typeof(IComponent).IsAssignableFrom(type)) && (((array == null) || (array.Length == 0)) || ((array.Length == 1) && (array[0] == this.Container))))
            {
                IDesignerHost service = this.GetService(typeof(IDesignerHost)) as IDesignerHost;
                if ((service != null) && (service.Container == this.Container))
                {
                    bool flag = false;
                    if ((!this.PreserveNames && (name != null)) && (this.Container.Components[name] != null))
                    {
                        flag = true;
                    }
                    if ((name == null) || flag)
                    {
                        obj2 = service.CreateComponent(type);
                    }
                    else
                    {
                        obj2 = service.CreateComponent(type, name);
                    }
                }
            }
            if (obj2 == null)
            {
                try
                {
                    try
                    {
                        obj2 = TypeDescriptor.CreateInstance(this.provider, type, null, array);
                    }
                    catch (MissingMethodException exception)
                    {
                        Type[] typeArray = new Type[array.Length];
                        for (int i = 0; i < array.Length; i++)
                        {
                            if (array[i] != null)
                            {
                                typeArray[i] = array[i].GetType();
                            }
                        }
                        object[] args = new object[array.Length];
                        foreach (ConstructorInfo info in TypeDescriptor.GetReflectionType(type).GetConstructors(BindingFlags.CreateInstance | BindingFlags.Public | BindingFlags.Instance))
                        {
                            ParameterInfo[] parameters = info.GetParameters();
                            if ((parameters != null) && (parameters.Length == typeArray.Length))
                            {
                                bool flag2 = true;
                                for (int j = 0; j < typeArray.Length; j++)
                                {
                                    if ((typeArray[j] == null) || parameters[j].ParameterType.IsAssignableFrom(typeArray[j]))
                                    {
                                        args[j] = array[j];
                                    }
                                    else
                                    {
                                        if (array[j] is IConvertible)
                                        {
                                            try
                                            {
                                                args[j] = ((IConvertible) array[j]).ToType(parameters[j].ParameterType, null);
                                                goto Label_0217;
                                            }
                                            catch (InvalidCastException)
                                            {
                                            }
                                        }
                                        flag2 = false;
                                        break;
                                    Label_0217:;
                                    }
                                }
                                if (flag2)
                                {
                                    obj2 = TypeDescriptor.CreateInstance(this.provider, type, null, args);
                                    break;
                                }
                            }
                        }
                        if (obj2 == null)
                        {
                            throw exception;
                        }
                    }
                }
                catch (MissingMethodException)
                {
                    StringBuilder builder = new StringBuilder();
                    foreach (object obj3 in array)
                    {
                        if (builder.Length > 0)
                        {
                            builder.Append(", ");
                        }
                        if (obj3 != null)
                        {
                            builder.Append(obj3.GetType().Name);
                        }
                        else
                        {
                            builder.Append("null");
                        }
                    }
                    Exception exception2 = new SerializationException(System.Design.SR.GetString("SerializationManagerNoMatchingCtor", new object[] { type.FullName, builder.ToString() })) {
                        HelpLink = "SerializationManagerNoMatchingCtor"
                    };
                    throw exception2;
                }
                if ((!addToContainer || !(obj2 is IComponent)) || (this.Container == null))
                {
                    return obj2;
                }
                bool flag3 = false;
                if ((!this.PreserveNames && (name != null)) && (this.Container.Components[name] != null))
                {
                    flag3 = true;
                }
                if ((name == null) || flag3)
                {
                    this.Container.Add((IComponent) obj2);
                    return obj2;
                }
                this.Container.Add((IComponent) obj2, name);
            }
            return obj2;
        }

        public IDisposable CreateSession()
        {
            if (this.session != null)
            {
                throw new InvalidOperationException(System.Design.SR.GetString("SerializationManagerAreadyInSession"));
            }
            this.session = new SerializationSession(this);
            this.OnSessionCreated(EventArgs.Empty);
            return this.session;
        }

        public Type GetRuntimeType(string typeName)
        {
            if ((this.typeResolver == null) && !this.searchedTypeResolver)
            {
                this.typeResolver = this.GetService(typeof(ITypeResolutionService)) as ITypeResolutionService;
                this.searchedTypeResolver = true;
            }
            if (this.typeResolver == null)
            {
                return Type.GetType(typeName);
            }
            return this.typeResolver.GetType(typeName);
        }

        public object GetSerializer(Type objectType, Type serializerType)
        {
            if (serializerType == null)
            {
                throw new ArgumentNullException("serializerType");
            }
            object currentSerializer = null;
            if (objectType != null)
            {
                if (this.serializers != null)
                {
                    currentSerializer = this.serializers[objectType];
                    if ((currentSerializer != null) && !serializerType.IsAssignableFrom(currentSerializer.GetType()))
                    {
                        currentSerializer = null;
                    }
                }
                if (currentSerializer == null)
                {
                    foreach (Attribute attribute in TypeDescriptor.GetAttributes(objectType))
                    {
                        if (attribute is DesignerSerializerAttribute)
                        {
                            DesignerSerializerAttribute attribute2 = (DesignerSerializerAttribute) attribute;
                            string serializerBaseTypeName = attribute2.SerializerBaseTypeName;
                            if (((serializerBaseTypeName != null) && (this.GetRuntimeType(serializerBaseTypeName) == serializerType)) && ((attribute2.SerializerTypeName != null) && (attribute2.SerializerTypeName.Length > 0)))
                            {
                                Type runtimeType = this.GetRuntimeType(attribute2.SerializerTypeName);
                                if (runtimeType != null)
                                {
                                    currentSerializer = Activator.CreateInstance(runtimeType, BindingFlags.CreateInstance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, null, null);
                                    break;
                                }
                            }
                        }
                    }
                    if ((currentSerializer != null) && (this.session != null))
                    {
                        if (this.serializers == null)
                        {
                            this.serializers = new Hashtable();
                        }
                        this.serializers[objectType] = currentSerializer;
                    }
                }
            }
            if ((this.defaultProviderTable == null) || !this.defaultProviderTable.ContainsKey(serializerType))
            {
                Type c = null;
                DefaultSerializationProviderAttribute attribute3 = (DefaultSerializationProviderAttribute) TypeDescriptor.GetAttributes(serializerType)[typeof(DefaultSerializationProviderAttribute)];
                if (attribute3 != null)
                {
                    c = this.GetRuntimeType(attribute3.ProviderTypeName);
                    if ((c != null) && typeof(IDesignerSerializationProvider).IsAssignableFrom(c))
                    {
                        IDesignerSerializationProvider provider = (IDesignerSerializationProvider) Activator.CreateInstance(c, BindingFlags.CreateInstance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, null, null);
                        ((IDesignerSerializationManager) this).AddSerializationProvider(provider);
                    }
                }
                if (this.defaultProviderTable == null)
                {
                    this.defaultProviderTable = new Hashtable();
                }
                this.defaultProviderTable[serializerType] = c;
            }
            if (this.designerSerializationProviders != null)
            {
                bool flag = true;
                for (int i = 0; flag && (i < this.designerSerializationProviders.Count); i++)
                {
                    flag = false;
                    foreach (IDesignerSerializationProvider provider2 in this.designerSerializationProviders)
                    {
                        object obj3 = provider2.GetSerializer(this, currentSerializer, objectType, serializerType);
                        if (obj3 != null)
                        {
                            flag = currentSerializer != obj3;
                            currentSerializer = obj3;
                        }
                    }
                }
            }
            return currentSerializer;
        }

        protected virtual object GetService(Type serviceType)
        {
            if (serviceType == typeof(IContainer))
            {
                return this.Container;
            }
            if (this.provider != null)
            {
                return this.provider.GetService(serviceType);
            }
            return null;
        }

        protected virtual Type GetType(string typeName)
        {
            Type runtimeType = this.GetRuntimeType(typeName);
            if (runtimeType != null)
            {
                TypeDescriptionProviderService service = this.GetService(typeof(TypeDescriptionProviderService)) as TypeDescriptionProviderService;
                if ((service != null) && !service.GetProvider(runtimeType).IsSupportedType(runtimeType))
                {
                    runtimeType = null;
                }
            }
            return runtimeType;
        }

        protected virtual void OnResolveName(ResolveNameEventArgs e)
        {
            if (this.resolveNameEventHandler != null)
            {
                this.resolveNameEventHandler(this, e);
            }
        }

        protected virtual void OnSessionCreated(EventArgs e)
        {
            if (this.sessionCreatedEventHandler != null)
            {
                this.sessionCreatedEventHandler(this, e);
            }
        }

        protected virtual void OnSessionDisposed(EventArgs e)
        {
            try
            {
                try
                {
                    if (this.sessionDisposedEventHandler != null)
                    {
                        this.sessionDisposedEventHandler(this, e);
                    }
                }
                finally
                {
                    if (this.serializationCompleteEventHandler != null)
                    {
                        this.serializationCompleteEventHandler(this, EventArgs.Empty);
                    }
                }
            }
            finally
            {
                this.resolveNameEventHandler = null;
                this.serializationCompleteEventHandler = null;
                this.instancesByName = null;
                this.namesByInstance = null;
                this.serializers = null;
                this.contextStack = null;
                this.errorList = null;
                this.session = null;
            }
        }

        void IDesignerSerializationManager.AddSerializationProvider(IDesignerSerializationProvider provider)
        {
            if (this.designerSerializationProviders == null)
            {
                this.designerSerializationProviders = new ArrayList();
            }
            if (!this.designerSerializationProviders.Contains(provider))
            {
                this.designerSerializationProviders.Add(provider);
            }
        }

        object IDesignerSerializationManager.CreateInstance(Type type, ICollection arguments, string name, bool addToContainer)
        {
            this.CheckSession();
            if (((name != null) && (this.instancesByName != null)) && this.instancesByName.ContainsKey(name))
            {
                Exception exception = new SerializationException(System.Design.SR.GetString("SerializationManagerDuplicateComponentDecl", new object[] { name })) {
                    HelpLink = "SerializationManagerDuplicateComponentDecl"
                };
                throw exception;
            }
            object obj2 = this.CreateInstance(type, arguments, name, addToContainer);
            if ((name != null) && (!(obj2 is IComponent) || !this.RecycleInstances))
            {
                if (this.instancesByName == null)
                {
                    this.instancesByName = new Hashtable();
                    this.namesByInstance = new Hashtable(new ReferenceComparer());
                }
                this.instancesByName[name] = obj2;
                this.namesByInstance[obj2] = name;
            }
            return obj2;
        }

        object IDesignerSerializationManager.GetInstance(string name)
        {
            object obj2 = null;
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            this.CheckSession();
            if (this.instancesByName != null)
            {
                obj2 = this.instancesByName[name];
            }
            if (((obj2 == null) && this.PreserveNames) && (this.Container != null))
            {
                obj2 = this.Container.Components[name];
            }
            if (obj2 == null)
            {
                ResolveNameEventArgs e = new ResolveNameEventArgs(name);
                this.OnResolveName(e);
                obj2 = e.Value;
            }
            return obj2;
        }

        string IDesignerSerializationManager.GetName(object value)
        {
            string str = null;
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            this.CheckSession();
            if (this.namesByInstance != null)
            {
                str = (string) this.namesByInstance[value];
            }
            if ((str != null) || !(value is IComponent))
            {
                return str;
            }
            ISite site = ((IComponent) value).Site;
            if (site == null)
            {
                return str;
            }
            INestedSite site2 = site as INestedSite;
            if (site2 != null)
            {
                return site2.FullName;
            }
            return site.Name;
        }

        object IDesignerSerializationManager.GetSerializer(Type objectType, Type serializerType)
        {
            return this.GetSerializer(objectType, serializerType);
        }

        Type IDesignerSerializationManager.GetType(string typeName)
        {
            this.CheckSession();
            Type type = null;
            while (type == null)
            {
                type = this.GetType(typeName);
                if (((type == null) && (typeName != null)) && (typeName.Length > 0))
                {
                    int length = typeName.LastIndexOf('.');
                    if ((length == -1) || (length == (typeName.Length - 1)))
                    {
                        return type;
                    }
                    typeName = typeName.Substring(0, length) + "+" + typeName.Substring(length + 1, (typeName.Length - length) - 1);
                }
            }
            return type;
        }

        void IDesignerSerializationManager.RemoveSerializationProvider(IDesignerSerializationProvider provider)
        {
            if (this.designerSerializationProviders != null)
            {
                this.designerSerializationProviders.Remove(provider);
            }
        }

        void IDesignerSerializationManager.ReportError(object errorInformation)
        {
            this.CheckSession();
            if (errorInformation != null)
            {
                this.Errors.Add(errorInformation);
            }
        }

        void IDesignerSerializationManager.SetName(object instance, string name)
        {
            this.CheckSession();
            if (instance == null)
            {
                throw new ArgumentNullException("instance");
            }
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (this.instancesByName == null)
            {
                this.instancesByName = new Hashtable();
                this.namesByInstance = new Hashtable(new ReferenceComparer());
            }
            if (this.instancesByName[name] != null)
            {
                throw new ArgumentException(System.Design.SR.GetString("SerializationManagerNameInUse", new object[] { name }));
            }
            if (this.namesByInstance[instance] != null)
            {
                throw new ArgumentException(System.Design.SR.GetString("SerializationManagerObjectHasName", new object[] { name, (string) this.namesByInstance[instance] }));
            }
            this.instancesByName[name] = instance;
            this.namesByInstance[instance] = name;
        }

        object IServiceProvider.GetService(Type serviceType)
        {
            return this.GetService(serviceType);
        }

        private PropertyDescriptor WrapProperty(PropertyDescriptor property, object owner)
        {
            if (property == null)
            {
                throw new ArgumentNullException("property");
            }
            return new WrappedPropertyDescriptor(property, owner);
        }

        public IContainer Container
        {
            get
            {
                if (this.container == null)
                {
                    IDesignerHost service = this.GetService(typeof(IDesignerHost)) as IDesignerHost;
                    if (service != null)
                    {
                        this.container = service.Container;
                    }
                }
                return this.container;
            }
            set
            {
                this.CheckNoSession();
                this.container = value;
            }
        }

        public IList Errors
        {
            get
            {
                this.CheckSession();
                if (this.errorList == null)
                {
                    this.errorList = new ArrayList();
                }
                return this.errorList;
            }
        }

        public bool PreserveNames
        {
            get
            {
                return this.preserveNames;
            }
            set
            {
                this.CheckNoSession();
                this.preserveNames = value;
            }
        }

        public object PropertyProvider
        {
            get
            {
                return this.propertyProvider;
            }
            set
            {
                if (this.propertyProvider != value)
                {
                    this.propertyProvider = value;
                    this.properties = null;
                }
            }
        }

        public bool RecycleInstances
        {
            get
            {
                return this.recycleInstances;
            }
            set
            {
                this.CheckNoSession();
                this.recycleInstances = value;
            }
        }

        internal ArrayList SerializationProviders
        {
            get
            {
                if (this.designerSerializationProviders == null)
                {
                    return new ArrayList();
                }
                return (this.designerSerializationProviders.Clone() as ArrayList);
            }
        }

        ContextStack IDesignerSerializationManager.Context
        {
            get
            {
                if (this.contextStack == null)
                {
                    this.CheckSession();
                    this.contextStack = new ContextStack();
                }
                return this.contextStack;
            }
        }

        PropertyDescriptorCollection IDesignerSerializationManager.Properties
        {
            get
            {
                if (this.properties == null)
                {
                    PropertyDescriptor[] descriptorArray;
                    object propertyProvider = this.PropertyProvider;
                    if (propertyProvider == null)
                    {
                        descriptorArray = new PropertyDescriptor[0];
                    }
                    else
                    {
                        PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(propertyProvider);
                        descriptorArray = new PropertyDescriptor[properties.Count];
                        for (int i = 0; i < descriptorArray.Length; i++)
                        {
                            descriptorArray[i] = this.WrapProperty(properties[i], propertyProvider);
                        }
                    }
                    this.properties = new PropertyDescriptorCollection(descriptorArray);
                }
                return this.properties;
            }
        }

        public bool ValidateRecycledTypes
        {
            get
            {
                return this.validateRecycledTypes;
            }
            set
            {
                this.CheckNoSession();
                this.validateRecycledTypes = value;
            }
        }

        private sealed class ReferenceComparer : IEqualityComparer
        {
            bool IEqualityComparer.Equals(object x, object y)
            {
                return object.ReferenceEquals(x, y);
            }

            int IEqualityComparer.GetHashCode(object x)
            {
                if (x != null)
                {
                    return x.GetHashCode();
                }
                return 0;
            }
        }

        private sealed class SerializationSession : IDisposable
        {
            private DesignerSerializationManager serializationManager;

            internal SerializationSession(DesignerSerializationManager serializationManager)
            {
                this.serializationManager = serializationManager;
            }

            public void Dispose()
            {
                this.serializationManager.OnSessionDisposed(EventArgs.Empty);
            }
        }

        private sealed class WrappedPropertyDescriptor : PropertyDescriptor
        {
            private PropertyDescriptor property;
            private object target;

            internal WrappedPropertyDescriptor(PropertyDescriptor property, object target) : base(property.Name, null)
            {
                this.property = property;
                this.target = target;
            }

            public override bool CanResetValue(object component)
            {
                return this.property.CanResetValue(this.target);
            }

            public override object GetValue(object component)
            {
                return this.property.GetValue(this.target);
            }

            public override void ResetValue(object component)
            {
                this.property.ResetValue(this.target);
            }

            public override void SetValue(object component, object value)
            {
                this.property.SetValue(this.target, value);
            }

            public override bool ShouldSerializeValue(object component)
            {
                return this.property.ShouldSerializeValue(this.target);
            }

            public override AttributeCollection Attributes
            {
                get
                {
                    return this.property.Attributes;
                }
            }

            public override Type ComponentType
            {
                get
                {
                    return this.property.ComponentType;
                }
            }

            public override bool IsReadOnly
            {
                get
                {
                    return this.property.IsReadOnly;
                }
            }

            public override Type PropertyType
            {
                get
                {
                    return this.property.PropertyType;
                }
            }
        }
    }
}

