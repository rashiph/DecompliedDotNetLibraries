namespace System.EnterpriseServices
{
    using System;
    using System.Collections;
    using System.EnterpriseServices.Admin;
    using System.Runtime.InteropServices;

    internal class InterfaceConfigCallback : IConfigCallback
    {
        private Hashtable _cache;
        private ICatalogCollection _coll;
        private RegistrationDriver _driver;
        private Type[] _ifcs;
        private Type _type;
        private static readonly Guid IID_IProcessInitializer = new Guid("1113f52d-dc7f-4943-aed6-88d04027e32a");

        public InterfaceConfigCallback(ICatalogCollection coll, Type t, Hashtable cache, RegistrationDriver driver)
        {
            this._type = t;
            this._coll = coll;
            this._cache = cache;
            this._driver = driver;
            this._ifcs = this.GetInteropInterfaces(this._type);
            foreach (Type type in this._ifcs)
            {
                if (Marshal.GenerateGuidForType(type) == IID_IProcessInitializer)
                {
                    try
                    {
                        ICatalogObject obj2 = cache["Component"] as ICatalogObject;
                        ICatalogCollection catalogs = cache["ComponentCollection"] as ICatalogCollection;
                        obj2.SetValue("InitializesServerApplication", 1);
                        catalogs.SaveChanges();
                    }
                    catch (Exception exception)
                    {
                        if ((exception is NullReferenceException) || (exception is SEHException))
                        {
                            throw;
                        }
                        throw new RegistrationException(Resource.FormatString("Reg_FailPIT", this._type), exception);
                    }
                }
            }
            RegistrationDriver.Populate(this._coll);
        }

        public bool AfterSaveChanges(object a, object key)
        {
            if (a == null)
            {
                return false;
            }
            return this._driver.AfterSaveChanges((Type) a, (ICatalogObject) key, this._coll, "Interface", this._cache);
        }

        public bool Configure(object a, object key)
        {
            if (a == null)
            {
                return false;
            }
            return this._driver.ConfigureObject((Type) a, (ICatalogObject) key, this._coll, "Interface", this._cache);
        }

        public void ConfigureDefaults(object a, object key)
        {
            bool flag = true;
            ICatalogObject obj2 = (ICatalogObject) key;
            if (this._cache[this._type] != null)
            {
                object obj3 = this._cache[this._type];
                if ((obj3 is Hashtable) && (((Hashtable) obj3)[a] != null))
                {
                    flag = false;
                }
            }
            if (flag)
            {
                obj2.SetValue("QueuingEnabled", false);
            }
        }

        public void ConfigureSubCollections(ICatalogCollection coll)
        {
            foreach (ICatalogObject obj2 in this)
            {
                Type t = (Type) this.FindObject(coll, obj2);
                if (t != null)
                {
                    ICatalogCollection collection = (ICatalogCollection) coll.GetCollection(CollectionName.Methods, obj2.Key());
                    this._driver.ConfigureCollection(collection, new MethodConfigCallback(collection, t, this._type, this._cache, this._driver));
                }
            }
        }

        private Type FindInterfaceByID(ICatalogObject ifcObj, Type t, Type[] interfaces)
        {
            Guid guid = new Guid((string) ifcObj.GetValue("IID"));
            foreach (Type type in interfaces)
            {
                if (Marshal.GenerateGuidForType(type) == guid)
                {
                    return type;
                }
            }
            return null;
        }

        private Type FindInterfaceByName(ICatalogObject ifcObj, Type t, Type[] interfaces)
        {
            string str = (string) ifcObj.GetValue("Name");
            foreach (Type type in interfaces)
            {
                if (type.IsInterface)
                {
                    if (type.Name == str)
                    {
                        return type;
                    }
                }
                else if (("_" + type.Name) == str)
                {
                    return type;
                }
            }
            return null;
        }

        public object FindObject(ICatalogCollection coll, object key)
        {
            ICatalogObject ifcObj = (ICatalogObject) key;
            Type type = null;
            type = this.FindInterfaceByID(ifcObj, this._type, this._ifcs);
            if (type == null)
            {
                type = this.FindInterfaceByName(ifcObj, this._type, this._ifcs);
            }
            return type;
        }

        public IEnumerator GetEnumerator()
        {
            IEnumerator pEnum = null;
            this._coll.GetEnumerator(out pEnum);
            return pEnum;
        }

        private Type[] GetInteropInterfaces(Type t)
        {
            Type baseType = t;
            ArrayList list = new ArrayList(t.GetInterfaces());
            while (baseType != null)
            {
                list.Add(baseType);
                baseType = baseType.BaseType;
            }
            list.Add(typeof(IManagedObject));
            Type[] array = new Type[list.Count];
            list.CopyTo(array);
            return array;
        }
    }
}

