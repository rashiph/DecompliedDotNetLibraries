namespace System.EnterpriseServices
{
    using System;
    using System.Collections;
    using System.EnterpriseServices.Admin;
    using System.Runtime.InteropServices;

    internal class ComponentConfigCallback : IConfigCallback
    {
        private Hashtable _cache;
        private ICatalogCollection _coll;
        private RegistrationDriver _driver;
        private InstallationFlags _installFlags;
        private ApplicationSpec _spec;

        public ComponentConfigCallback(ICatalogCollection coll, ApplicationSpec spec, Hashtable cache, RegistrationDriver driver, InstallationFlags installFlags)
        {
            this._spec = spec;
            this._coll = coll;
            this._cache = cache;
            this._driver = driver;
            this._installFlags = installFlags;
            RegistrationDriver.Populate(coll);
        }

        public bool AfterSaveChanges(object a, object key)
        {
            return this._driver.AfterSaveChanges((Type) key, (ICatalogObject) a, this._coll, "Component", this._cache);
        }

        public bool Configure(object a, object key)
        {
            return this._driver.ConfigureObject((Type) key, (ICatalogObject) a, this._coll, "Component", this._cache);
        }

        public void ConfigureDefaults(object a, object key)
        {
            ICatalogObject obj2 = (ICatalogObject) a;
            obj2.SetValue("AllowInprocSubscribers", true);
            obj2.SetValue("ComponentAccessChecksEnabled", false);
            obj2.SetValue("COMTIIntrinsics", false);
            obj2.SetValue("ConstructionEnabled", false);
            obj2.SetValue("EventTrackingEnabled", false);
            obj2.SetValue("FireInParallel", false);
            obj2.SetValue("IISIntrinsics", false);
            obj2.SetValue("JustInTimeActivation", false);
            obj2.SetValue("LoadBalancingSupported", false);
            obj2.SetValue("MustRunInClientContext", false);
            obj2.SetValue("ObjectPoolingEnabled", false);
            obj2.SetValue("Synchronization", SynchronizationOption.Disabled);
            obj2.SetValue("Transaction", TransactionOption.Disabled);
            obj2.SetValue("ComponentTransactionTimeoutEnabled", false);
            obj2.SetValue("TxIsolationLevel", TransactionIsolationLevel.Serializable);
        }

        public void ConfigureSubCollections(ICatalogCollection coll)
        {
            if ((this._installFlags & InstallationFlags.ConfigureComponentsOnly) == InstallationFlags.Default)
            {
                foreach (Type type in this._spec.ConfigurableTypes)
                {
                    ICatalogObject obj2 = (ICatalogObject) this.FindObject(coll, type);
                    ICatalogCollection collection = (ICatalogCollection) coll.GetCollection(CollectionName.Interfaces, obj2.Key());
                    this._cache["Component"] = obj2;
                    this._cache["ComponentType"] = type;
                    InterfaceConfigCallback cb = new InterfaceConfigCallback(collection, type, this._cache, this._driver);
                    this._driver.ConfigureCollection(collection, cb);
                    if ((this._cache["SecurityOnMethods"] != null) || ServicedComponentInfo.AreMethodsSecure(type))
                    {
                        this.FixupMethodSecurity(collection);
                        this._cache["SecurityOnMethods"] = null;
                    }
                }
            }
        }

        public object FindObject(ICatalogCollection coll, object key)
        {
            Guid guid = Marshal.GenerateGuidForType((Type) key);
            for (int i = 0; i < coll.Count(); i++)
            {
                ICatalogObject obj2 = (ICatalogObject) coll.Item(i);
                Guid guid2 = new Guid((string) obj2.Key());
                if (guid2 == guid)
                {
                    return obj2;
                }
            }
            throw new RegistrationException(Resource.FormatString("Reg_ComponentMissing", ((Type) key).FullName));
        }

        private void FixupMethodSecurity(ICatalogCollection coll)
        {
            this.FixupMethodSecurityForInterface(coll, typeof(IManagedObject));
            this.FixupMethodSecurityForInterface(coll, typeof(IServicedComponentInfo));
            this.FixupMethodSecurityForInterface(coll, typeof(IDisposable));
        }

        private void FixupMethodSecurityForInterface(ICatalogCollection coll, Type InterfaceType)
        {
            ICatalogObject obj2 = null;
            Guid guid = Marshal.GenerateGuidForType(InterfaceType);
            int num = coll.Count();
            for (int i = 0; i < num; i++)
            {
                ICatalogObject obj3 = (ICatalogObject) coll.Item(i);
                if (new Guid((string) obj3.Key()) == guid)
                {
                    obj2 = obj3;
                    break;
                }
            }
            if (obj2 != null)
            {
                SecurityRoleAttribute attribute = new SecurityRoleAttribute("Marshaler", false) {
                    Description = Resource.FormatString("Reg_MarshalerDesc")
                };
                IConfigurationAttribute attribute2 = attribute;
                this._cache["CurrentTarget"] = "Interface";
                this._cache["InterfaceCollection"] = coll;
                this._cache["Interface"] = obj2;
                this._cache["InterfaceType"] = InterfaceType;
                if (attribute2.Apply(this._cache))
                {
                    coll.SaveChanges();
                }
                if (attribute2.AfterSaveChanges(this._cache))
                {
                    coll.SaveChanges();
                }
            }
        }

        public IEnumerator GetEnumerator()
        {
            return this._spec.ConfigurableTypes.GetEnumerator();
        }
    }
}

