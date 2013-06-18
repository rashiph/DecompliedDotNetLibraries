namespace System.EnterpriseServices
{
    using System;
    using System.Collections;
    using System.EnterpriseServices.Admin;
    using System.EnterpriseServices.Thunk;
    using System.Runtime.InteropServices;

    [ComVisible(false), AttributeUsage(AttributeTargets.Interface | AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Assembly, Inherited=true, AllowMultiple=true)]
    public sealed class SecurityRoleAttribute : Attribute, IConfigurationAttribute
    {
        private string _description;
        private static string _everyone;
        private string _role;
        private bool _setEveryoneAccess;
        private static readonly string RoleCacheString = "RoleAttribute::ApplicationRoleCache";

        public SecurityRoleAttribute(string role) : this(role, false)
        {
        }

        public SecurityRoleAttribute(string role, bool everyone)
        {
            this._role = role;
            this._setEveryoneAccess = everyone;
            this._description = null;
        }

        private void AddRoleFor(string target, Hashtable cache)
        {
            ICatalogCollection catalogs = (ICatalogCollection) cache[target + "Collection"];
            ICatalogObject obj2 = (ICatalogObject) cache[target];
            ICatalogCollection collection = (ICatalogCollection) catalogs.GetCollection(CollectionName.RolesFor(target), obj2.Key());
            collection.Populate();
            if (this.Search(collection, "Name", this._role) == null)
            {
                ((ICatalogObject) collection.Add()).SetValue("Name", this._role);
                collection.SaveChanges();
                collection.Populate();
                for (int i = 0; i < collection.Count(); i++)
                {
                    ICatalogObject obj1 = (ICatalogObject) collection.Item(i);
                }
            }
        }

        private void EnsureRole(Hashtable cache)
        {
            ICatalogCollection catalogs = null;
            ICatalogObject obj2 = null;
            ICatalogCollection coll = null;
            Hashtable hashtable = (Hashtable) cache[RoleCacheString];
            if (hashtable == null)
            {
                hashtable = new Hashtable();
                cache[RoleCacheString] = hashtable;
            }
            if (hashtable[this._role] == null)
            {
                catalogs = (ICatalogCollection) cache["ApplicationCollection"];
                obj2 = (ICatalogObject) cache["Application"];
                coll = (ICatalogCollection) catalogs.GetCollection(CollectionName.Roles, obj2.Key());
                coll.Populate();
                if (this.Search(coll, "Name", this._role) == null)
                {
                    ICatalogObject obj3 = (ICatalogObject) coll.Add();
                    obj3.SetValue("Name", this._role);
                    if (this._description != null)
                    {
                        obj3.SetValue("Description", this._description);
                    }
                    coll.SaveChanges();
                    if (this._setEveryoneAccess)
                    {
                        ICatalogCollection collection = (ICatalogCollection) coll.GetCollection(CollectionName.UsersInRole, obj3.Key());
                        collection.Populate();
                        ((ICatalogObject) collection.Add()).SetValue("User", EveryoneAccount);
                        collection.SaveChanges();
                    }
                }
                hashtable[this._role] = true;
            }
        }

        private ICatalogObject Search(ICatalogCollection coll, string key, string value)
        {
            for (int i = 0; i < coll.Count(); i++)
            {
                ICatalogObject obj2 = (ICatalogObject) coll.Item(i);
                string str = (string) obj2.GetValue(key);
                if (str == value)
                {
                    return obj2;
                }
            }
            return null;
        }

        bool IConfigurationAttribute.AfterSaveChanges(Hashtable cache)
        {
            string str = (string) cache["CurrentTarget"];
            if (str == "Component")
            {
                this.AddRoleFor("Component", cache);
            }
            else if (str == "Method")
            {
                this.AddRoleFor("Method", cache);
            }
            else if (str == "Interface")
            {
                this.AddRoleFor("Interface", cache);
            }
            else
            {
                bool flag1 = str == "Application";
            }
            return true;
        }

        bool IConfigurationAttribute.Apply(Hashtable cache)
        {
            this.EnsureRole(cache);
            string str = (string) cache["CurrentTarget"];
            if (str == "Method")
            {
                cache["SecurityOnMethods"] = true;
            }
            return true;
        }

        bool IConfigurationAttribute.IsValidTarget(string s)
        {
            if ((!(s == "Component") && !(s == "Method")) && (!(s == "Application") && !(s == "Interface")))
            {
                return false;
            }
            return true;
        }

        public string Description
        {
            get
            {
                return this._description;
            }
            set
            {
                this._description = value;
            }
        }

        private static string EveryoneAccount
        {
            get
            {
                if (_everyone == null)
                {
                    _everyone = Security.GetEveryoneAccountName();
                }
                return _everyone;
            }
        }

        public string Role
        {
            get
            {
                return this._role;
            }
            set
            {
                this._role = value;
            }
        }

        public bool SetEveryoneAccess
        {
            get
            {
                return this._setEveryoneAccess;
            }
            set
            {
                this._setEveryoneAccess = value;
            }
        }
    }
}

