namespace System.Drawing.Design
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Diagnostics;
    using System.Drawing;
    using System.IO;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    [Serializable, PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust"), PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
    public class ToolboxItem : ISerializable
    {
        private static object EventComponentsCreated = new object();
        private static object EventComponentsCreating = new object();
        private bool locked;
        private LockableDictionary properties;
        private static TraceSwitch ToolboxItemPersist = new TraceSwitch("ToolboxPersisting", "ToolboxItem: write data");

        public event ToolboxComponentsCreatedEventHandler ComponentsCreated;

        public event ToolboxComponentsCreatingEventHandler ComponentsCreating;

        public ToolboxItem()
        {
        }

        public ToolboxItem(Type toolType)
        {
            this.Initialize(toolType);
        }

        private ToolboxItem(SerializationInfo info, StreamingContext context)
        {
            this.Deserialize(info, context);
        }

        protected void CheckUnlocked()
        {
            if (this.Locked)
            {
                throw new InvalidOperationException(System.Drawing.SR.GetString("ToolboxItemLocked"));
            }
        }

        public IComponent[] CreateComponents()
        {
            return this.CreateComponents(null);
        }

        public IComponent[] CreateComponents(IDesignerHost host)
        {
            this.OnComponentsCreating(new ToolboxComponentsCreatingEventArgs(host));
            IComponent[] components = this.CreateComponentsCore(host, new Hashtable());
            if ((components != null) && (components.Length > 0))
            {
                this.OnComponentsCreated(new ToolboxComponentsCreatedEventArgs(components));
            }
            return components;
        }

        public IComponent[] CreateComponents(IDesignerHost host, IDictionary defaultValues)
        {
            this.OnComponentsCreating(new ToolboxComponentsCreatingEventArgs(host));
            IComponent[] components = this.CreateComponentsCore(host, defaultValues);
            if ((components != null) && (components.Length > 0))
            {
                this.OnComponentsCreated(new ToolboxComponentsCreatedEventArgs(components));
            }
            return components;
        }

        protected virtual IComponent[] CreateComponentsCore(IDesignerHost host)
        {
            ArrayList list = new ArrayList();
            Type componentClass = this.GetType(host, this.AssemblyName, this.TypeName, true);
            if (componentClass != null)
            {
                if (host != null)
                {
                    list.Add(host.CreateComponent(componentClass));
                }
                else if (typeof(IComponent).IsAssignableFrom(componentClass))
                {
                    list.Add(TypeDescriptor.CreateInstance(null, componentClass, null, null));
                }
            }
            IComponent[] array = new IComponent[list.Count];
            list.CopyTo(array, 0);
            return array;
        }

        protected virtual IComponent[] CreateComponentsCore(IDesignerHost host, IDictionary defaultValues)
        {
            IComponent[] componentArray = this.CreateComponentsCore(host);
            if (host != null)
            {
                for (int i = 0; i < componentArray.Length; i++)
                {
                    IComponentInitializer designer = host.GetDesigner(componentArray[i]) as IComponentInitializer;
                    if (designer != null)
                    {
                        bool flag = true;
                        try
                        {
                            designer.InitializeNewComponent(defaultValues);
                            flag = false;
                        }
                        finally
                        {
                            if (flag)
                            {
                                for (int j = 0; j < componentArray.Length; j++)
                                {
                                    host.DestroyComponent(componentArray[j]);
                                }
                            }
                        }
                    }
                }
            }
            return componentArray;
        }

        protected virtual void Deserialize(SerializationInfo info, StreamingContext context)
        {
            string[] strArray = null;
            SerializationInfoEnumerator enumerator = info.GetEnumerator();
            while (enumerator.MoveNext())
            {
                SerializationEntry current = enumerator.Current;
                if (current.Name.Equals("PropertyNames"))
                {
                    strArray = current.Value as string[];
                    break;
                }
            }
            if (strArray == null)
            {
                strArray = new string[] { "AssemblyName", "Bitmap", "DisplayName", "Filter", "IsTransient", "TypeName" };
            }
            SerializationInfoEnumerator enumerator2 = info.GetEnumerator();
            while (enumerator2.MoveNext())
            {
                SerializationEntry entry2 = enumerator2.Current;
                foreach (string str in strArray)
                {
                    if (str.Equals(entry2.Name))
                    {
                        this.Properties[entry2.Name] = entry2.Value;
                        continue;
                    }
                }
            }
            if (info.GetBoolean("Locked"))
            {
                this.Lock();
            }
        }

        public override bool Equals(object obj)
        {
            if (this != obj)
            {
                if (obj == null)
                {
                    return false;
                }
                if (!(obj.GetType() == base.GetType()))
                {
                    return false;
                }
                ToolboxItem item = (ToolboxItem) obj;
                if (this.TypeName != item.TypeName)
                {
                    if ((this.TypeName == null) || (item.TypeName == null))
                    {
                        return false;
                    }
                    if (!this.TypeName.Equals(item.TypeName))
                    {
                        return false;
                    }
                }
                if (this.AssemblyName != item.AssemblyName)
                {
                    if ((this.AssemblyName == null) || (item.AssemblyName == null))
                    {
                        return false;
                    }
                    if (!this.AssemblyName.FullName.Equals(item.AssemblyName.FullName))
                    {
                        return false;
                    }
                }
                if (this.DisplayName != item.DisplayName)
                {
                    if ((this.DisplayName == null) || (item.DisplayName == null))
                    {
                        return false;
                    }
                    if (!this.DisplayName.Equals(item.DisplayName))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        protected virtual object FilterPropertyValue(string propertyName, object value)
        {
            string str = propertyName;
            if (str != null)
            {
                if (!(str == "AssemblyName"))
                {
                    if ((str == "DisplayName") || (str == "TypeName"))
                    {
                        if (value == null)
                        {
                            value = string.Empty;
                        }
                        return value;
                    }
                    if (str == "Filter")
                    {
                        if (value == null)
                        {
                            value = new ToolboxItemFilterAttribute[0];
                        }
                        return value;
                    }
                    if ((str == "IsTransient") && (value == null))
                    {
                        value = false;
                    }
                    return value;
                }
                if (value != null)
                {
                    value = ((System.Reflection.AssemblyName) value).Clone();
                }
            }
            return value;
        }

        public override int GetHashCode()
        {
            int num = 0;
            if (this.TypeName != null)
            {
                num ^= this.TypeName.GetHashCode();
            }
            return (num ^ this.DisplayName.GetHashCode());
        }

        private System.Reflection.AssemblyName GetNonRetargetedAssemblyName(Type type, System.Reflection.AssemblyName policiedAssemblyName)
        {
            if ((type != null) && (policiedAssemblyName != null))
            {
                if (type.Assembly.FullName == policiedAssemblyName.FullName)
                {
                    return policiedAssemblyName;
                }
                foreach (System.Reflection.AssemblyName name in type.Assembly.GetReferencedAssemblies())
                {
                    if (name.FullName == policiedAssemblyName.FullName)
                    {
                        return name;
                    }
                }
                foreach (System.Reflection.AssemblyName name2 in type.Assembly.GetReferencedAssemblies())
                {
                    if (name2.Name == policiedAssemblyName.Name)
                    {
                        return name2;
                    }
                }
                foreach (System.Reflection.AssemblyName name3 in type.Assembly.GetReferencedAssemblies())
                {
                    Assembly assembly = null;
                    try
                    {
                        assembly = Assembly.Load(name3);
                        if ((assembly != null) && (assembly.FullName == policiedAssemblyName.FullName))
                        {
                            return name3;
                        }
                    }
                    catch
                    {
                    }
                }
            }
            return null;
        }

        public Type GetType(IDesignerHost host)
        {
            return this.GetType(host, this.AssemblyName, this.TypeName, false);
        }

        protected virtual Type GetType(IDesignerHost host, System.Reflection.AssemblyName assemblyName, string typeName, bool reference)
        {
            ITypeResolutionService service = null;
            Type type = null;
            if (typeName == null)
            {
                throw new ArgumentNullException("typeName");
            }
            if (host != null)
            {
                service = (ITypeResolutionService) host.GetService(typeof(ITypeResolutionService));
            }
            if (service != null)
            {
                if (reference)
                {
                    if (assemblyName != null)
                    {
                        service.ReferenceAssembly(assemblyName);
                        return service.GetType(typeName);
                    }
                    type = service.GetType(typeName);
                    if (type == null)
                    {
                        type = Type.GetType(typeName);
                    }
                    if (type != null)
                    {
                        service.ReferenceAssembly(type.Assembly.GetName());
                    }
                    return type;
                }
                if (assemblyName != null)
                {
                    Assembly assembly = service.GetAssembly(assemblyName);
                    if (assembly != null)
                    {
                        type = assembly.GetType(typeName);
                    }
                }
                if (type == null)
                {
                    type = service.GetType(typeName);
                }
                return type;
            }
            if (!string.IsNullOrEmpty(typeName))
            {
                if (assemblyName != null)
                {
                    Assembly assembly2 = null;
                    try
                    {
                        assembly2 = Assembly.Load(assemblyName);
                    }
                    catch (FileNotFoundException)
                    {
                    }
                    catch (BadImageFormatException)
                    {
                    }
                    catch (IOException)
                    {
                    }
                    if (((assembly2 == null) && (assemblyName.CodeBase != null)) && (assemblyName.CodeBase.Length > 0))
                    {
                        try
                        {
                            assembly2 = Assembly.LoadFrom(assemblyName.CodeBase);
                        }
                        catch (FileNotFoundException)
                        {
                        }
                        catch (BadImageFormatException)
                        {
                        }
                        catch (IOException)
                        {
                        }
                    }
                    if (assembly2 != null)
                    {
                        type = assembly2.GetType(typeName);
                    }
                }
                if (type == null)
                {
                    type = Type.GetType(typeName, false);
                }
            }
            return type;
        }

        public virtual void Initialize(Type type)
        {
            this.CheckUnlocked();
            if (type != null)
            {
                this.TypeName = type.FullName;
                System.Reflection.AssemblyName name = type.Assembly.GetName(true);
                if (type.Assembly.GlobalAssemblyCache)
                {
                    name.CodeBase = null;
                }
                Dictionary<string, System.Reflection.AssemblyName> dictionary = new Dictionary<string, System.Reflection.AssemblyName>();
                for (Type type2 = type; type2 != null; type2 = type2.BaseType)
                {
                    System.Reflection.AssemblyName policiedAssemblyName = type2.Assembly.GetName(true);
                    System.Reflection.AssemblyName nonRetargetedAssemblyName = this.GetNonRetargetedAssemblyName(type, policiedAssemblyName);
                    if ((nonRetargetedAssemblyName != null) && !dictionary.ContainsKey(nonRetargetedAssemblyName.FullName))
                    {
                        dictionary[nonRetargetedAssemblyName.FullName] = nonRetargetedAssemblyName;
                    }
                }
                System.Reflection.AssemblyName[] nameArray = new System.Reflection.AssemblyName[dictionary.Count];
                int num = 0;
                foreach (System.Reflection.AssemblyName name4 in dictionary.Values)
                {
                    nameArray[num++] = name4;
                }
                this.DependentAssemblies = nameArray;
                this.AssemblyName = name;
                this.DisplayName = type.Name;
                if (!type.Assembly.ReflectionOnly)
                {
                    object[] customAttributes = type.Assembly.GetCustomAttributes(typeof(AssemblyCompanyAttribute), true);
                    if ((customAttributes != null) && (customAttributes.Length > 0))
                    {
                        AssemblyCompanyAttribute attribute = customAttributes[0] as AssemblyCompanyAttribute;
                        if ((attribute != null) && (attribute.Company != null))
                        {
                            this.Company = attribute.Company;
                        }
                    }
                    DescriptionAttribute attribute2 = (DescriptionAttribute) TypeDescriptor.GetAttributes(type)[typeof(DescriptionAttribute)];
                    if (attribute2 != null)
                    {
                        this.Description = attribute2.Description;
                    }
                    ToolboxBitmapAttribute attribute3 = (ToolboxBitmapAttribute) TypeDescriptor.GetAttributes(type)[typeof(ToolboxBitmapAttribute)];
                    if (attribute3 != null)
                    {
                        System.Drawing.Bitmap image = attribute3.GetImage(type, false) as System.Drawing.Bitmap;
                        if ((image != null) && ((image.Width != 0x10) || (image.Height != 0x10)))
                        {
                            image = new System.Drawing.Bitmap(image, new Size(0x10, 0x10));
                        }
                        this.Bitmap = image;
                    }
                    bool flag = false;
                    ArrayList list = new ArrayList();
                    foreach (Attribute attribute4 in TypeDescriptor.GetAttributes(type))
                    {
                        ToolboxItemFilterAttribute attribute5 = attribute4 as ToolboxItemFilterAttribute;
                        if (attribute5 != null)
                        {
                            if (attribute5.FilterString.Equals(this.TypeName))
                            {
                                flag = true;
                            }
                            list.Add(attribute5);
                        }
                    }
                    if (!flag)
                    {
                        list.Add(new ToolboxItemFilterAttribute(this.TypeName));
                    }
                    this.Filter = (ToolboxItemFilterAttribute[]) list.ToArray(typeof(ToolboxItemFilterAttribute));
                }
            }
        }

        public virtual void Lock()
        {
            this.locked = true;
        }

        protected virtual void OnComponentsCreated(ToolboxComponentsCreatedEventArgs args)
        {
            if (this.componentsCreatedEvent != null)
            {
                this.componentsCreatedEvent(this, args);
            }
        }

        protected virtual void OnComponentsCreating(ToolboxComponentsCreatingEventArgs args)
        {
            if (this.componentsCreatingEvent != null)
            {
                this.componentsCreatingEvent(this, args);
            }
        }

        protected virtual void Serialize(SerializationInfo info, StreamingContext context)
        {
            bool traceVerbose = ToolboxItemPersist.TraceVerbose;
            info.AddValue("Locked", this.Locked);
            ArrayList list = new ArrayList(this.Properties.Count);
            foreach (DictionaryEntry entry in this.Properties)
            {
                list.Add(entry.Key);
                info.AddValue((string) entry.Key, entry.Value);
            }
            info.AddValue("PropertyNames", (string[]) list.ToArray(typeof(string)));
        }

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            System.Drawing.IntSecurity.UnmanagedCode.Demand();
            this.Serialize(info, context);
        }

        public override string ToString()
        {
            return this.DisplayName;
        }

        protected void ValidatePropertyType(string propertyName, object value, Type expectedType, bool allowNull)
        {
            if (value == null)
            {
                if (!allowNull)
                {
                    throw new ArgumentNullException("value");
                }
            }
            else if (!expectedType.IsInstanceOfType(value))
            {
                throw new ArgumentException(System.Drawing.SR.GetString("ToolboxItemInvalidPropertyType", new object[] { propertyName, expectedType.FullName }), "value");
            }
        }

        protected virtual object ValidatePropertyValue(string propertyName, object value)
        {
            switch (propertyName)
            {
                case "AssemblyName":
                    this.ValidatePropertyType(propertyName, value, typeof(System.Reflection.AssemblyName), true);
                    return value;

                case "Bitmap":
                    this.ValidatePropertyType(propertyName, value, typeof(System.Drawing.Bitmap), true);
                    return value;

                case "Company":
                case "Description":
                case "DisplayName":
                case "TypeName":
                    this.ValidatePropertyType(propertyName, value, typeof(string), true);
                    if (value == null)
                    {
                        value = string.Empty;
                    }
                    return value;

                case "Filter":
                {
                    this.ValidatePropertyType(propertyName, value, typeof(ICollection), true);
                    int num = 0;
                    ICollection is2 = (ICollection) value;
                    if (is2 != null)
                    {
                        foreach (object obj2 in is2)
                        {
                            if (obj2 is ToolboxItemFilterAttribute)
                            {
                                num++;
                            }
                        }
                    }
                    ToolboxItemFilterAttribute[] attributeArray = new ToolboxItemFilterAttribute[num];
                    if (is2 != null)
                    {
                        num = 0;
                        foreach (object obj3 in is2)
                        {
                            ToolboxItemFilterAttribute attribute = obj3 as ToolboxItemFilterAttribute;
                            if (attribute != null)
                            {
                                attributeArray[num++] = attribute;
                            }
                        }
                    }
                    value = attributeArray;
                    return value;
                }
                case "IsTransient":
                    this.ValidatePropertyType(propertyName, value, typeof(bool), false);
                    return value;
            }
            return value;
        }

        public System.Reflection.AssemblyName AssemblyName
        {
            get
            {
                return (System.Reflection.AssemblyName) this.Properties["AssemblyName"];
            }
            set
            {
                this.Properties["AssemblyName"] = value;
            }
        }

        public System.Drawing.Bitmap Bitmap
        {
            get
            {
                return (System.Drawing.Bitmap) this.Properties["Bitmap"];
            }
            set
            {
                this.Properties["Bitmap"] = value;
            }
        }

        public string Company
        {
            get
            {
                return (string) this.Properties["Company"];
            }
            set
            {
                this.Properties["Company"] = value;
            }
        }

        public virtual string ComponentType
        {
            get
            {
                return System.Drawing.SR.GetString("DotNET_ComponentType");
            }
        }

        public System.Reflection.AssemblyName[] DependentAssemblies
        {
            get
            {
                System.Reflection.AssemblyName[] nameArray = (System.Reflection.AssemblyName[]) this.Properties["DependentAssemblies"];
                if (nameArray != null)
                {
                    return (System.Reflection.AssemblyName[]) nameArray.Clone();
                }
                return null;
            }
            set
            {
                this.Properties["DependentAssemblies"] = value.Clone();
            }
        }

        public string Description
        {
            get
            {
                return (string) this.Properties["Description"];
            }
            set
            {
                this.Properties["Description"] = value;
            }
        }

        public string DisplayName
        {
            get
            {
                return (string) this.Properties["DisplayName"];
            }
            set
            {
                this.Properties["DisplayName"] = value;
            }
        }

        public ICollection Filter
        {
            get
            {
                return (ICollection) this.Properties["Filter"];
            }
            set
            {
                this.Properties["Filter"] = value;
            }
        }

        public bool IsTransient
        {
            get
            {
                return (bool) this.Properties["IsTransient"];
            }
            set
            {
                this.Properties["IsTransient"] = value;
            }
        }

        public virtual bool Locked
        {
            get
            {
                return this.locked;
            }
        }

        public IDictionary Properties
        {
            get
            {
                if (this.properties == null)
                {
                    this.properties = new LockableDictionary(this, 8);
                }
                return this.properties;
            }
        }

        public string TypeName
        {
            get
            {
                return (string) this.Properties["TypeName"];
            }
            set
            {
                this.Properties["TypeName"] = value;
            }
        }

        public virtual string Version
        {
            get
            {
                if (this.AssemblyName != null)
                {
                    return this.AssemblyName.Version.ToString();
                }
                return string.Empty;
            }
        }

        private class LockableDictionary : Hashtable
        {
            private ToolboxItem _item;

            internal LockableDictionary(ToolboxItem item, int capacity) : base(capacity)
            {
                this._item = item;
            }

            public override void Add(object key, object value)
            {
                string propertyName = this.GetPropertyName(key);
                value = this._item.ValidatePropertyValue(propertyName, value);
                this.CheckSerializable(value);
                this._item.CheckUnlocked();
                base.Add(propertyName, value);
            }

            private void CheckSerializable(object value)
            {
                if ((value != null) && !value.GetType().IsSerializable)
                {
                    throw new ArgumentException(System.Drawing.SR.GetString("ToolboxItemValueNotSerializable", new object[] { value.GetType().FullName }));
                }
            }

            public override void Clear()
            {
                this._item.CheckUnlocked();
                base.Clear();
            }

            private string GetPropertyName(object key)
            {
                if (key == null)
                {
                    throw new ArgumentNullException("key");
                }
                string str = key as string;
                if ((str == null) || (str.Length == 0))
                {
                    throw new ArgumentException(System.Drawing.SR.GetString("ToolboxItemInvalidKey"), "key");
                }
                return str;
            }

            public override void Remove(object key)
            {
                this._item.CheckUnlocked();
                base.Remove(key);
            }

            public override bool IsFixedSize
            {
                get
                {
                    return this._item.Locked;
                }
            }

            public override bool IsReadOnly
            {
                get
                {
                    return this._item.Locked;
                }
            }

            public override object this[object key]
            {
                get
                {
                    string propertyName = this.GetPropertyName(key);
                    object obj2 = base[propertyName];
                    return this._item.FilterPropertyValue(propertyName, obj2);
                }
                set
                {
                    string propertyName = this.GetPropertyName(key);
                    value = this._item.ValidatePropertyValue(propertyName, value);
                    this.CheckSerializable(value);
                    this._item.CheckUnlocked();
                    base[propertyName] = value;
                }
            }
        }
    }
}

