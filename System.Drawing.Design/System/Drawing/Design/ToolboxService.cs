namespace System.Drawing.Design
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.IO;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.Remoting.Lifetime;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Security.Permissions;
    using System.Windows.Forms;

    [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust"), PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust")]
    public abstract class ToolboxService : IToolboxService, IComponentDiscoveryService
    {
        private Hashtable _designerCreators;
        private IDesignerEventService _designerEventService;
        private static AppDomain _domain;
        private static DomainProxyObject _domainObject;
        private static ClientSponsor _domainObjectSponsor;
        private ArrayList _globalCreators;
        private ICollection _lastMergedCreators;
        private IDesignerHost _lastMergedHost;
        internal DesignerToolboxInfo _lastState;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected ToolboxService()
        {
        }

        protected virtual ToolboxItemContainer CreateItemContainer(IDataObject dataObject)
        {
            if (dataObject == null)
            {
                throw new ArgumentNullException("dataObject");
            }
            return new ToolboxItemContainer(dataObject);
        }

        protected virtual ToolboxItemContainer CreateItemContainer(ToolboxItem item, IDesignerHost link)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }
            if (link != null)
            {
                return null;
            }
            return new ToolboxItemContainer(item);
        }

        protected virtual void FilterChanged()
        {
        }

        private ICollection GetCreatorCollection(IDesignerHost host)
        {
            if (host == null)
            {
                return this._globalCreators;
            }
            if (host != this._lastMergedHost)
            {
                ICollection is2 = this._globalCreators;
                ICollection is3 = null;
                if (this._designerCreators != null)
                {
                    is3 = this._designerCreators[host] as ICollection;
                    if (is3 != null)
                    {
                        int count = is3.Count;
                        if (is2 != null)
                        {
                            count += is2.Count;
                        }
                        ToolboxItemCreator[] array = new ToolboxItemCreator[count];
                        is3.CopyTo(array, 0);
                        if (is2 != null)
                        {
                            is2.CopyTo(array, is3.Count);
                        }
                        is2 = array;
                    }
                }
                this._lastMergedCreators = is2;
                this._lastMergedHost = host;
            }
            return this._lastMergedCreators;
        }

        private static FilterSupport GetFilterSupport(ICollection itemFilter, ICollection targetFilter)
        {
            FilterSupport supported = FilterSupport.Supported;
            int num = 0;
            int num2 = 0;
            foreach (ToolboxItemFilterAttribute attribute in itemFilter)
            {
                if (supported == FilterSupport.NotSupported)
                {
                    break;
                }
                if (attribute.FilterType == ToolboxItemFilterType.Require)
                {
                    num++;
                    foreach (object obj2 in targetFilter)
                    {
                        ToolboxItemFilterAttribute attribute2 = obj2 as ToolboxItemFilterAttribute;
                        if ((attribute2 != null) && attribute.Match(attribute2))
                        {
                            num2++;
                            break;
                        }
                    }
                }
                else if (attribute.FilterType == ToolboxItemFilterType.Prevent)
                {
                    foreach (object obj3 in targetFilter)
                    {
                        ToolboxItemFilterAttribute attribute3 = obj3 as ToolboxItemFilterAttribute;
                        if ((attribute3 != null) && attribute.Match(attribute3))
                        {
                            supported = FilterSupport.NotSupported;
                            break;
                        }
                    }
                }
                else if ((supported != FilterSupport.Custom) && (attribute.FilterType == ToolboxItemFilterType.Custom))
                {
                    if (attribute.FilterString.Length == 0)
                    {
                        supported = FilterSupport.Custom;
                    }
                    else
                    {
                        foreach (ToolboxItemFilterAttribute attribute4 in targetFilter)
                        {
                            if (attribute.FilterString.Equals(attribute4.FilterString))
                            {
                                supported = FilterSupport.Custom;
                                break;
                            }
                        }
                    }
                }
            }
            if (((supported != FilterSupport.NotSupported) && (num > 0)) && (num2 == 0))
            {
                supported = FilterSupport.NotSupported;
            }
            if (supported != FilterSupport.NotSupported)
            {
                num = 0;
                num2 = 0;
                foreach (ToolboxItemFilterAttribute attribute5 in targetFilter)
                {
                    if (supported == FilterSupport.NotSupported)
                    {
                        break;
                    }
                    if (attribute5.FilterType == ToolboxItemFilterType.Require)
                    {
                        num++;
                        foreach (ToolboxItemFilterAttribute attribute6 in itemFilter)
                        {
                            if (attribute5.Match(attribute6))
                            {
                                num2++;
                                break;
                            }
                        }
                    }
                    else if (attribute5.FilterType == ToolboxItemFilterType.Prevent)
                    {
                        foreach (ToolboxItemFilterAttribute attribute7 in itemFilter)
                        {
                            if (attribute5.Match(attribute7))
                            {
                                supported = FilterSupport.NotSupported;
                                break;
                            }
                        }
                    }
                    else if ((supported != FilterSupport.Custom) && (attribute5.FilterType == ToolboxItemFilterType.Custom))
                    {
                        if (attribute5.FilterString.Length == 0)
                        {
                            supported = FilterSupport.Custom;
                        }
                        else
                        {
                            foreach (ToolboxItemFilterAttribute attribute8 in itemFilter)
                            {
                                if (attribute5.FilterString.Equals(attribute8.FilterString))
                                {
                                    supported = FilterSupport.Custom;
                                    break;
                                }
                            }
                        }
                    }
                }
                if (((supported != FilterSupport.NotSupported) && (num > 0)) && (num2 == 0))
                {
                    supported = FilterSupport.NotSupported;
                }
            }
            return supported;
        }

        protected abstract IList GetItemContainers();
        protected abstract IList GetItemContainers(string categoryName);
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static ToolboxItem GetToolboxItem(System.Type toolType)
        {
            return GetToolboxItem(toolType, false);
        }

        public static ToolboxItem GetToolboxItem(System.Type toolType, bool nonPublic)
        {
            ToolboxItem item = null;
            if (toolType == null)
            {
                throw new ArgumentNullException("toolType");
            }
            if (((nonPublic || toolType.IsPublic) || toolType.IsNestedPublic) && (typeof(IComponent).IsAssignableFrom(toolType) && !toolType.IsAbstract))
            {
                ToolboxItemAttribute attribute = (ToolboxItemAttribute) TypeDescriptor.GetAttributes(toolType)[typeof(ToolboxItemAttribute)];
                if (!attribute.IsDefaultAttribute())
                {
                    System.Type toolboxItemType = attribute.ToolboxItemType;
                    if (toolboxItemType != null)
                    {
                        ConstructorInfo constructor = toolboxItemType.GetConstructor(new System.Type[] { typeof(System.Type) });
                        if ((constructor != null) && (toolType != null))
                        {
                            return (ToolboxItem) constructor.Invoke(new object[] { toolType });
                        }
                        constructor = toolboxItemType.GetConstructor(new System.Type[0]);
                        if (constructor != null)
                        {
                            item = (ToolboxItem) constructor.Invoke(new object[0]);
                            item.Initialize(toolType);
                        }
                    }
                    return item;
                }
                if (!attribute.Equals(ToolboxItemAttribute.None) && !toolType.ContainsGenericParameters)
                {
                    item = new ToolboxItem(toolType);
                }
                return item;
            }
            if (typeof(ToolboxItem).IsAssignableFrom(toolType))
            {
                item = (ToolboxItem) Activator.CreateInstance(toolType, true);
            }
            return item;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static ICollection GetToolboxItems(AssemblyName an)
        {
            return GetToolboxItems(an, false);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static ICollection GetToolboxItems(Assembly a, string newCodeBase)
        {
            return GetToolboxItems(a, newCodeBase, false);
        }

        public static ICollection GetToolboxItems(AssemblyName an, bool throwOnError)
        {
            if (_domainObject == null)
            {
                _domain = AppDomain.CreateDomain("Assembly Enumeration Domain");
                _domainObject = (DomainProxyObject) _domain.CreateInstanceAndUnwrap(typeof(DomainProxyObject).Assembly.FullName, typeof(DomainProxyObject).FullName);
                _domainObjectSponsor = new ClientSponsor(new TimeSpan(0, 5, 0));
                _domainObjectSponsor.Register(_domainObject);
            }
            byte[] toolboxItems = _domainObject.GetToolboxItems(an, throwOnError);
            BinaryFormatter formatter = new BinaryFormatter();
            return (ICollection) formatter.Deserialize(new MemoryStream(toolboxItems));
        }

        public static ICollection GetToolboxItems(Assembly a, string newCodeBase, bool throwOnError)
        {
            AssemblyName name;
            if (a == null)
            {
                throw new ArgumentNullException("a");
            }
            ArrayList list = new ArrayList();
            if (a.GlobalAssemblyCache)
            {
                name = a.GetName();
                name.CodeBase = newCodeBase;
            }
            else
            {
                name = null;
            }
            try
            {
                foreach (System.Type type in a.GetTypes())
                {
                    if (typeof(IComponent).IsAssignableFrom(type))
                    {
                        ConstructorInfo constructor = type.GetConstructor(new System.Type[0]);
                        if (constructor == null)
                        {
                            constructor = type.GetConstructor(new System.Type[] { typeof(IContainer) });
                        }
                        if (constructor != null)
                        {
                            try
                            {
                                ToolboxItem toolboxItem = GetToolboxItem(type);
                                if (toolboxItem != null)
                                {
                                    if (name != null)
                                    {
                                        toolboxItem.AssemblyName = name;
                                    }
                                    list.Add(toolboxItem);
                                }
                            }
                            catch
                            {
                                if (throwOnError)
                                {
                                    throw;
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                if (throwOnError)
                {
                    throw;
                }
            }
            return list;
        }

        protected virtual bool IsItemContainer(IDataObject dataObject, IDesignerHost host)
        {
            if (dataObject == null)
            {
                throw new ArgumentNullException("dataObject");
            }
            if (ToolboxItemContainer.ContainsFormat(dataObject))
            {
                return true;
            }
            ICollection creatorCollection = this.GetCreatorCollection(host);
            if (creatorCollection != null)
            {
                foreach (ToolboxItemCreator creator in creatorCollection)
                {
                    if (dataObject.GetDataPresent(creator.Format))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        protected bool IsItemContainerSupported(ToolboxItemContainer container, IDesignerHost host)
        {
            if (container == null)
            {
                throw new ArgumentNullException("container");
            }
            if (host == null)
            {
                throw new ArgumentNullException("host");
            }
            ICollection creatorCollection = this.GetCreatorCollection(host);
            this._lastState = host.GetService(typeof(DesignerToolboxInfo)) as DesignerToolboxInfo;
            if (this._lastState == null)
            {
                this._lastState = new DesignerToolboxInfo(this, host);
                host.AddService(typeof(DesignerToolboxInfo), this._lastState);
            }
            switch (GetFilterSupport(container.GetFilter(creatorCollection), this._lastState.Filter))
            {
                case FilterSupport.NotSupported:
                    return false;

                case FilterSupport.Supported:
                    return true;

                case FilterSupport.Custom:
                    if (this._lastState.ToolboxUser == null)
                    {
                        break;
                    }
                    return this._lastState.ToolboxUser.GetToolSupported(container.GetToolboxItem(creatorCollection));
            }
            return false;
        }

        internal void OnDesignerInfoChanged(DesignerToolboxInfo state)
        {
            if (this._designerEventService == null)
            {
                this._designerEventService = state.DesignerHost.GetService(typeof(IDesignerEventService)) as IDesignerEventService;
            }
            if ((this._designerEventService != null) && (this._designerEventService.ActiveDesigner == state.DesignerHost))
            {
                this.FilterChanged();
            }
        }

        protected abstract void Refresh();
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected virtual void SelectedItemContainerUsed()
        {
            this.SelectedItemContainer = null;
        }

        protected virtual bool SetCursor()
        {
            if (this.SelectedItemContainer != null)
            {
                Cursor.Current = Cursors.Cross;
                return true;
            }
            return false;
        }

        ICollection IComponentDiscoveryService.GetComponentTypes(IDesignerHost designerHost, System.Type baseType)
        {
            Hashtable hashtable = new Hashtable();
            ToolboxItemCollection toolboxItems = ((IToolboxService) this).GetToolboxItems();
            if (toolboxItems != null)
            {
                System.Type type = typeof(IComponent);
                TypeDescriptionProviderService service = null;
                TypeDescriptionProvider provider = null;
                if (designerHost != null)
                {
                    service = designerHost.GetService(typeof(TypeDescriptionProviderService)) as TypeDescriptionProviderService;
                }
                foreach (ToolboxItem item in toolboxItems)
                {
                    System.Type c = item.GetType(designerHost);
                    if (((c != null) && type.IsAssignableFrom(c)) && ((baseType == null) || baseType.IsAssignableFrom(c)))
                    {
                        if (service != null)
                        {
                            provider = service.GetProvider(c);
                            if ((provider != null) && !provider.IsSupportedType(c))
                            {
                                continue;
                            }
                        }
                        hashtable[c] = c;
                    }
                }
            }
            return hashtable.Values;
        }

        void IToolboxService.AddCreator(ToolboxItemCreatorCallback creator, string format)
        {
            if (creator == null)
            {
                throw new ArgumentNullException("creator");
            }
            if (format == null)
            {
                throw new ArgumentNullException("format");
            }
            if (this._globalCreators == null)
            {
                this._globalCreators = new ArrayList();
            }
            this._globalCreators.Add(new ToolboxItemCreator(creator, format));
            this._lastMergedHost = null;
            this._lastMergedCreators = null;
        }

        void IToolboxService.AddCreator(ToolboxItemCreatorCallback creator, string format, IDesignerHost host)
        {
            if (creator == null)
            {
                throw new ArgumentNullException("creator");
            }
            if (format == null)
            {
                throw new ArgumentNullException("format");
            }
            if (host == null)
            {
                throw new ArgumentNullException("host");
            }
            if (this._designerCreators == null)
            {
                this._designerCreators = new Hashtable();
            }
            ArrayList list = this._designerCreators[host] as ArrayList;
            if (list == null)
            {
                list = new ArrayList(4);
                this._designerCreators[host] = list;
            }
            list.Add(new ToolboxItemCreator(creator, format));
            this._lastMergedHost = null;
            this._lastMergedCreators = null;
        }

        void IToolboxService.AddLinkedToolboxItem(ToolboxItem toolboxItem, IDesignerHost host)
        {
            if (toolboxItem == null)
            {
                throw new ArgumentNullException("toolboxItem");
            }
            if (host == null)
            {
                throw new ArgumentNullException("host");
            }
            ToolboxItemContainer container = this.CreateItemContainer(toolboxItem, host);
            if (container != null)
            {
                this.GetItemContainers(this.SelectedCategory).Add(container);
            }
        }

        void IToolboxService.AddLinkedToolboxItem(ToolboxItem toolboxItem, string category, IDesignerHost host)
        {
            if (toolboxItem == null)
            {
                throw new ArgumentNullException("toolboxItem");
            }
            if (category == null)
            {
                throw new ArgumentNullException("category");
            }
            if (host == null)
            {
                throw new ArgumentNullException("host");
            }
            ToolboxItemContainer container = this.CreateItemContainer(toolboxItem, host);
            if (container != null)
            {
                this.GetItemContainers(category).Add(container);
            }
        }

        void IToolboxService.AddToolboxItem(ToolboxItem toolboxItem)
        {
            if (toolboxItem == null)
            {
                throw new ArgumentNullException("toolboxItem");
            }
            ToolboxItemContainer container = this.CreateItemContainer(toolboxItem, null);
            if (container != null)
            {
                this.GetItemContainers(this.SelectedCategory).Add(container);
            }
        }

        void IToolboxService.AddToolboxItem(ToolboxItem toolboxItem, string category)
        {
            if (toolboxItem == null)
            {
                throw new ArgumentNullException("toolboxItem");
            }
            if (category == null)
            {
                throw new ArgumentNullException("category");
            }
            ToolboxItemContainer container = this.CreateItemContainer(toolboxItem, null);
            if (container != null)
            {
                this.GetItemContainers(category).Add(container);
            }
        }

        ToolboxItem IToolboxService.DeserializeToolboxItem(object serializedObject)
        {
            if (serializedObject == null)
            {
                throw new ArgumentNullException("serializedObject");
            }
            IDataObject dataObject = serializedObject as IDataObject;
            if (dataObject == null)
            {
                dataObject = new DataObject(serializedObject);
            }
            ToolboxItemContainer container = this.CreateItemContainer(dataObject);
            if (container != null)
            {
                return container.GetToolboxItem(this.GetCreatorCollection(null));
            }
            return null;
        }

        ToolboxItem IToolboxService.DeserializeToolboxItem(object serializedObject, IDesignerHost host)
        {
            if (serializedObject == null)
            {
                throw new ArgumentNullException("serializedObject");
            }
            if (host == null)
            {
                throw new ArgumentNullException("host");
            }
            IDataObject dataObject = serializedObject as IDataObject;
            if (dataObject == null)
            {
                dataObject = new DataObject(serializedObject);
            }
            ToolboxItemContainer container = this.CreateItemContainer(dataObject);
            if (container != null)
            {
                return container.GetToolboxItem(this.GetCreatorCollection(host));
            }
            return null;
        }

        ToolboxItem IToolboxService.GetSelectedToolboxItem()
        {
            ToolboxItemContainer selectedItemContainer = this.SelectedItemContainer;
            if (selectedItemContainer != null)
            {
                return selectedItemContainer.GetToolboxItem(this.GetCreatorCollection(null));
            }
            return null;
        }

        ToolboxItem IToolboxService.GetSelectedToolboxItem(IDesignerHost host)
        {
            if (host == null)
            {
                throw new ArgumentNullException("host");
            }
            ToolboxItemContainer selectedItemContainer = this.SelectedItemContainer;
            if (selectedItemContainer != null)
            {
                return selectedItemContainer.GetToolboxItem(this.GetCreatorCollection(host));
            }
            return null;
        }

        ToolboxItemCollection IToolboxService.GetToolboxItems()
        {
            IList itemContainers = this.GetItemContainers();
            ArrayList list2 = new ArrayList(itemContainers.Count);
            ICollection creatorCollection = this.GetCreatorCollection(null);
            foreach (ToolboxItemContainer container in itemContainers)
            {
                ToolboxItem toolboxItem = container.GetToolboxItem(creatorCollection);
                if (toolboxItem != null)
                {
                    list2.Add(toolboxItem);
                }
            }
            ToolboxItem[] array = new ToolboxItem[list2.Count];
            list2.CopyTo(array, 0);
            return new ToolboxItemCollection(array);
        }

        ToolboxItemCollection IToolboxService.GetToolboxItems(IDesignerHost host)
        {
            if (host == null)
            {
                throw new ArgumentNullException("host");
            }
            IList itemContainers = this.GetItemContainers();
            ArrayList list2 = new ArrayList(itemContainers.Count);
            ICollection creatorCollection = this.GetCreatorCollection(host);
            foreach (ToolboxItemContainer container in itemContainers)
            {
                ToolboxItem toolboxItem = container.GetToolboxItem(creatorCollection);
                if (toolboxItem != null)
                {
                    list2.Add(toolboxItem);
                }
            }
            ToolboxItem[] array = new ToolboxItem[list2.Count];
            list2.CopyTo(array, 0);
            return new ToolboxItemCollection(array);
        }

        ToolboxItemCollection IToolboxService.GetToolboxItems(string category)
        {
            if (category == null)
            {
                throw new ArgumentNullException("category");
            }
            IList itemContainers = this.GetItemContainers(category);
            ArrayList list2 = new ArrayList(itemContainers.Count);
            ICollection creatorCollection = this.GetCreatorCollection(null);
            foreach (ToolboxItemContainer container in itemContainers)
            {
                ToolboxItem toolboxItem = container.GetToolboxItem(creatorCollection);
                if (toolboxItem != null)
                {
                    list2.Add(toolboxItem);
                }
            }
            ToolboxItem[] array = new ToolboxItem[list2.Count];
            list2.CopyTo(array, 0);
            return new ToolboxItemCollection(array);
        }

        ToolboxItemCollection IToolboxService.GetToolboxItems(string category, IDesignerHost host)
        {
            if (category == null)
            {
                throw new ArgumentNullException("category");
            }
            if (host == null)
            {
                throw new ArgumentNullException("host");
            }
            IList itemContainers = this.GetItemContainers(category);
            ArrayList list2 = new ArrayList(itemContainers.Count);
            ICollection creatorCollection = this.GetCreatorCollection(host);
            foreach (ToolboxItemContainer container in itemContainers)
            {
                ToolboxItem toolboxItem = container.GetToolboxItem(creatorCollection);
                if (toolboxItem != null)
                {
                    list2.Add(toolboxItem);
                }
            }
            ToolboxItem[] array = new ToolboxItem[list2.Count];
            list2.CopyTo(array, 0);
            return new ToolboxItemCollection(array);
        }

        bool IToolboxService.IsSupported(object serializedObject, ICollection filterAttributes)
        {
            if (serializedObject == null)
            {
                throw new ArgumentNullException("serializedObject");
            }
            if (filterAttributes == null)
            {
                throw new ArgumentNullException("filterAttributes");
            }
            IDataObject dataObject = serializedObject as IDataObject;
            if (dataObject == null)
            {
                dataObject = new DataObject(serializedObject);
            }
            if (!this.IsItemContainer(dataObject, null))
            {
                return false;
            }
            return (GetFilterSupport(this.CreateItemContainer(dataObject).GetFilter(this.GetCreatorCollection(null)), filterAttributes) == FilterSupport.Supported);
        }

        bool IToolboxService.IsSupported(object serializedObject, IDesignerHost host)
        {
            if (serializedObject == null)
            {
                throw new ArgumentNullException("serializedObject");
            }
            if (host == null)
            {
                throw new ArgumentNullException("host");
            }
            IDataObject dataObject = serializedObject as IDataObject;
            if (dataObject == null)
            {
                dataObject = new DataObject(serializedObject);
            }
            if (!this.IsItemContainer(dataObject, host))
            {
                return false;
            }
            ToolboxItemContainer container = this.CreateItemContainer(dataObject);
            return this.IsItemContainerSupported(container, host);
        }

        bool IToolboxService.IsToolboxItem(object serializedObject)
        {
            if (serializedObject == null)
            {
                throw new ArgumentNullException("serializedObject");
            }
            IDataObject dataObject = serializedObject as IDataObject;
            if (dataObject == null)
            {
                dataObject = new DataObject(serializedObject);
            }
            return this.IsItemContainer(dataObject, null);
        }

        bool IToolboxService.IsToolboxItem(object serializedObject, IDesignerHost host)
        {
            if (serializedObject == null)
            {
                throw new ArgumentNullException("serializedObject");
            }
            if (host == null)
            {
                throw new ArgumentNullException("host");
            }
            IDataObject dataObject = serializedObject as IDataObject;
            if (dataObject == null)
            {
                dataObject = new DataObject(serializedObject);
            }
            return this.IsItemContainer(dataObject, host);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        void IToolboxService.Refresh()
        {
            this.Refresh();
        }

        void IToolboxService.RemoveCreator(string format)
        {
            if (format == null)
            {
                throw new ArgumentNullException("format");
            }
            if (this._globalCreators != null)
            {
                for (int i = 0; i < this._globalCreators.Count; i++)
                {
                    ToolboxItemCreator creator = this._globalCreators[i] as ToolboxItemCreator;
                    if (creator.Format.Equals(format))
                    {
                        this._globalCreators.RemoveAt(i);
                        this._lastMergedHost = null;
                        this._lastMergedCreators = null;
                        return;
                    }
                }
            }
        }

        void IToolboxService.RemoveCreator(string format, IDesignerHost host)
        {
            if (format == null)
            {
                throw new ArgumentNullException("format");
            }
            if (host == null)
            {
                throw new ArgumentNullException("host");
            }
            if (this._designerCreators != null)
            {
                ArrayList list = this._designerCreators[host] as ArrayList;
                if (list != null)
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        ToolboxItemCreator creator = list[i] as ToolboxItemCreator;
                        if (creator.Format.Equals(format))
                        {
                            list.RemoveAt(i);
                            this._lastMergedHost = null;
                            this._lastMergedCreators = null;
                            return;
                        }
                    }
                }
            }
        }

        void IToolboxService.RemoveToolboxItem(ToolboxItem toolboxItem)
        {
            if (toolboxItem == null)
            {
                throw new ArgumentNullException("toolboxItem");
            }
            this.GetItemContainers().Remove(this.CreateItemContainer(toolboxItem, null));
        }

        void IToolboxService.RemoveToolboxItem(ToolboxItem toolboxItem, string category)
        {
            if (toolboxItem == null)
            {
                throw new ArgumentNullException("toolboxItem");
            }
            if (category == null)
            {
                throw new ArgumentNullException("category");
            }
            this.GetItemContainers(category).Remove(this.CreateItemContainer(toolboxItem, null));
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        void IToolboxService.SelectedToolboxItemUsed()
        {
            this.SelectedItemContainerUsed();
        }

        object IToolboxService.SerializeToolboxItem(ToolboxItem toolboxItem)
        {
            if (toolboxItem == null)
            {
                throw new ArgumentNullException("toolboxItem");
            }
            return this.CreateItemContainer(toolboxItem, null).ToolboxData;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        bool IToolboxService.SetCursor()
        {
            return this.SetCursor();
        }

        void IToolboxService.SetSelectedToolboxItem(ToolboxItem toolboxItem)
        {
            if (toolboxItem != null)
            {
                this.SelectedItemContainer = this.CreateItemContainer(toolboxItem, null);
            }
            else
            {
                this.SelectedItemContainer = null;
            }
        }

        public static void UnloadToolboxItems()
        {
            if (_domain != null)
            {
                AppDomain domain = _domain;
                _domainObjectSponsor.Close();
                _domainObjectSponsor = null;
                _domainObject = null;
                _domain = null;
                AppDomain.Unload(domain);
            }
        }

        protected abstract CategoryNameCollection CategoryNames { get; }

        protected abstract string SelectedCategory { get; set; }

        protected abstract ToolboxItemContainer SelectedItemContainer { get; set; }

        CategoryNameCollection IToolboxService.CategoryNames
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.CategoryNames;
            }
        }

        string IToolboxService.SelectedCategory
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.SelectedCategory;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.SelectedCategory = value;
            }
        }

        private class DomainProxyObject : MarshalByRefObject
        {
            internal byte[] GetToolboxItems(AssemblyName an, bool throwOnError)
            {
                Assembly a = null;
                try
                {
                    a = Assembly.Load(an);
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
                if ((a == null) && (an.CodeBase != null))
                {
                    a = Assembly.LoadFrom(new Uri(an.CodeBase).LocalPath);
                }
                if (a == null)
                {
                    throw new ArgumentException(System.Drawing.Design.SR.GetString("ToolboxServiceAssemblyNotFound", new object[] { an.FullName }));
                }
                ICollection graph = null;
                try
                {
                    graph = ToolboxService.GetToolboxItems(a, null, throwOnError);
                }
                catch (Exception exception)
                {
                    ReflectionTypeLoadException exception2 = exception as ReflectionTypeLoadException;
                    if (exception2 != null)
                    {
                        throw new ReflectionTypeLoadException(null, exception2.LoaderExceptions, exception2.Message);
                    }
                    throw;
                }
                BinaryFormatter formatter = new BinaryFormatter();
                MemoryStream serializationStream = new MemoryStream();
                formatter.Serialize(serializationStream, graph);
                serializationStream.Close();
                return serializationStream.GetBuffer();
            }
        }

        private enum FilterSupport
        {
            NotSupported,
            Supported,
            Custom
        }
    }
}

