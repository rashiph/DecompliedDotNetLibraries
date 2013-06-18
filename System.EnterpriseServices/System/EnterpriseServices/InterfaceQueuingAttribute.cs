namespace System.EnterpriseServices
{
    using System;
    using System.Collections;
    using System.EnterpriseServices.Admin;
    using System.Reflection;
    using System.Runtime.InteropServices;

    [ComVisible(false), AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class, Inherited=true, AllowMultiple=true)]
    public sealed class InterfaceQueuingAttribute : Attribute, IConfigurationAttribute
    {
        private bool _enabled;
        private string _interface;

        public InterfaceQueuingAttribute()
        {
            this._enabled = true;
        }

        public InterfaceQueuingAttribute(bool enabled)
        {
            this._enabled = enabled;
        }

        private bool ConfigureInterface(ICatalogObject obj)
        {
            bool flag = (bool) obj.GetValue("QueuingSupported");
            if (this._enabled && flag)
            {
                obj.SetValue("QueuingEnabled", this._enabled);
            }
            else
            {
                if (this._enabled)
                {
                    throw new RegistrationException(Resource.FormatString("Reg_QueueingNotSupported", (string) obj.Name()));
                }
                obj.SetValue("QueuingEnabled", this._enabled);
            }
            return true;
        }

        private void FindInterfaceByKey(string key, ICatalogCollection coll, Type comp, out ICatalogObject ifcObj, out Type ifcType)
        {
            ifcType = FindInterfaceByName(key, comp);
            if (ifcType == null)
            {
                throw new RegistrationException(Resource.FormatString("Reg_TypeFindError", key, comp.ToString()));
            }
            Guid guid = Marshal.GenerateGuidForType(ifcType);
            object[] aKeys = new object[] { "{" + guid + "}" };
            coll.PopulateByKey(aKeys);
            if (coll.Count() != 1)
            {
                throw new RegistrationException(Resource.FormatString("Reg_TypeFindError", key, comp.ToString()));
            }
            ifcObj = (ICatalogObject) coll.Item(0);
        }

        internal static Type FindInterfaceByName(string name, Type component)
        {
            Type type = ResolveTypeRelativeTo(name, component);
            if (type == null)
            {
                type = Type.GetType(name, false);
            }
            bool flag1 = type != null;
            return type;
        }

        internal static Type ResolveTypeRelativeTo(string typeName, Type serverType)
        {
            Type type2 = null;
            bool flag = false;
            bool flag2 = false;
            foreach (Type type in serverType.GetInterfaces())
            {
                string fullName = type.FullName;
                int indexB = fullName.Length - typeName.Length;
                if (((indexB >= 0) && (string.CompareOrdinal(typeName, 0, fullName, indexB, typeName.Length) == 0)) && ((indexB == 0) || ((indexB > 0) && (fullName[indexB - 1] == '.'))))
                {
                    if (type2 == null)
                    {
                        type2 = type;
                        flag = indexB == 0;
                    }
                    else
                    {
                        if (type2 != null)
                        {
                            flag2 = true;
                        }
                        if ((type2 != null) && flag)
                        {
                            if (indexB == 0)
                            {
                                throw new AmbiguousMatchException(Resource.FormatString("Reg_IfcAmbiguousMatch", typeName, type, type2));
                            }
                        }
                        else if (((type2 != null) && !flag) && (indexB == 0))
                        {
                            type2 = type;
                            flag = true;
                        }
                    }
                }
            }
            if (flag2 && !flag)
            {
                throw new AmbiguousMatchException(Resource.FormatString("Reg_IfcAmbiguousMatch", typeName, type, type2));
            }
            return type2;
        }

        private void StashModification(Hashtable cache, Type comp, Type ifc)
        {
            if (cache[comp] == null)
            {
                cache[comp] = new Hashtable();
            }
            ((Hashtable) cache[comp])[ifc] = true;
        }

        bool IConfigurationAttribute.AfterSaveChanges(Hashtable info)
        {
            if (this._interface != null)
            {
                ICatalogObject obj3;
                Type type2;
                ICatalogCollection catalogs = (ICatalogCollection) info["ComponentCollection"];
                ICatalogObject obj2 = (ICatalogObject) info["Component"];
                Type comp = (Type) info["ComponentType"];
                ICatalogCollection collection = (ICatalogCollection) catalogs.GetCollection("InterfacesForComponent", obj2.Key());
                this.FindInterfaceByKey(this._interface, collection, comp, out obj3, out type2);
                this.ConfigureInterface(obj3);
                collection.SaveChanges();
                this.StashModification(info, comp, type2);
            }
            return false;
        }

        bool IConfigurationAttribute.Apply(Hashtable info)
        {
            if (this._interface == null)
            {
                ICatalogObject obj2 = (ICatalogObject) info["Interface"];
                this.ConfigureInterface(obj2);
            }
            return true;
        }

        bool IConfigurationAttribute.IsValidTarget(string s)
        {
            if (this._interface == null)
            {
                return (s == "Interface");
            }
            return (s == "Component");
        }

        public bool Enabled
        {
            get
            {
                return this._enabled;
            }
            set
            {
                this._enabled = value;
            }
        }

        public string Interface
        {
            get
            {
                return this._interface;
            }
            set
            {
                this._interface = value;
            }
        }
    }
}

