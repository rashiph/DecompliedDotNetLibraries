namespace System.EnterpriseServices
{
    using System;
    using System.Collections;
    using System.EnterpriseServices.Admin;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;

    internal class ApplicationSpec
    {
        private string _appid;
        private System.Reflection.Assembly _asm;
        private Type[] _cfgtypes;
        private Type[] _events;
        private Type[] _normal;
        private RegistrationConfig _regConfig;

        internal ApplicationSpec(System.Reflection.Assembly asm, RegistrationConfig regConfig)
        {
            this._asm = asm;
            this._regConfig = regConfig;
            this.GenerateNames();
            this.ReadTypes();
        }

        private string FormatApplicationName(System.Reflection.Assembly asm)
        {
            object[] customAttributes = asm.GetCustomAttributes(typeof(ApplicationNameAttribute), true);
            if (customAttributes.Length > 0)
            {
                return ((ApplicationNameAttribute) customAttributes[0]).Value;
            }
            return asm.GetName().Name;
        }

        private void GenerateNames()
        {
            if ((this._regConfig.TypeLibrary == null) || (this._regConfig.TypeLibrary.Length == 0))
            {
                string directoryName = Path.GetDirectoryName(this.File);
                this._regConfig.TypeLibrary = Path.Combine(directoryName, this._asm.GetName().Name + ".tlb");
            }
            else
            {
                this._regConfig.TypeLibrary = Path.GetFullPath(this._regConfig.TypeLibrary);
            }
            if (((this.Name != null) && (this.Name.Length != 0)) && ('{' == this.Name[0]))
            {
                this._appid = "{" + new Guid(this.Name) + "}";
                this.Name = null;
            }
            if ((this.Name == null) || (this.Name.Length == 0))
            {
                this.Name = this.FormatApplicationName(this._asm);
            }
            object[] customAttributes = this._asm.GetCustomAttributes(typeof(ApplicationIDAttribute), true);
            if (customAttributes.Length > 0)
            {
                ApplicationIDAttribute attribute = (ApplicationIDAttribute) customAttributes[0];
                this._appid = "{" + new Guid(attribute.Value.ToString()).ToString() + "}";
            }
        }

        public bool Matches(ICatalogObject obj)
        {
            if (this.ID != null)
            {
                Guid guid = new Guid(this.ID);
                Guid guid2 = new Guid((string) obj.GetValue("ID"));
                if (guid == guid2)
                {
                    return true;
                }
            }
            else
            {
                string str = ((string) obj.GetValue("Name")).ToLower(CultureInfo.InvariantCulture);
                if (this.Name.ToLower(CultureInfo.InvariantCulture) == str)
                {
                    return true;
                }
            }
            return false;
        }

        private void ReadTypes()
        {
            ArrayList list = new ArrayList();
            ArrayList list2 = new ArrayList();
            foreach (Type type in new RegistrationServices().GetRegistrableTypesInAssembly(this._asm))
            {
                if (ServicedComponentInfo.IsTypeServicedComponent(type))
                {
                    object[] customAttributes = type.GetCustomAttributes(typeof(EventClassAttribute), true);
                    if ((customAttributes != null) && (customAttributes.Length > 0))
                    {
                        list.Add(type);
                    }
                    else
                    {
                        list2.Add(type);
                    }
                }
            }
            if (list.Count > 0)
            {
                this._events = new Type[list.Count];
                list.CopyTo(this._events);
            }
            else
            {
                this._events = null;
            }
            if (list2.Count > 0)
            {
                this._normal = new Type[list2.Count];
                list2.CopyTo(this._normal);
            }
            else
            {
                this._normal = null;
            }
            int num = ((this._normal != null) ? this._normal.Length : 0) + ((this._events != null) ? this._events.Length : 0);
            if (num > 0)
            {
                this._cfgtypes = new Type[num];
                if (this._events != null)
                {
                    this._events.CopyTo(this._cfgtypes, 0);
                }
                if (this._normal != null)
                {
                    this._normal.CopyTo(this._cfgtypes, (int) (num - this._normal.Length));
                }
            }
        }

        public override string ToString()
        {
            if (this.ID != null)
            {
                return ("id=" + this.ID);
            }
            return ("name=" + this.Name);
        }

        internal string AppRootDir
        {
            get
            {
                return this._regConfig.ApplicationRootDirectory;
            }
        }

        internal System.Reflection.Assembly Assembly
        {
            get
            {
                return this._asm;
            }
        }

        internal Type[] ConfigurableTypes
        {
            get
            {
                return this._cfgtypes;
            }
        }

        internal string DefinitiveName
        {
            get
            {
                if (this.ID != null)
                {
                    return this.ID;
                }
                return this.Name;
            }
        }

        internal Type[] EventTypes
        {
            get
            {
                return this._events;
            }
        }

        internal string File
        {
            get
            {
                return this._regConfig.AssemblyFile;
            }
        }

        internal string ID
        {
            get
            {
                return this._appid;
            }
        }

        internal string Name
        {
            get
            {
                return this._regConfig.Application;
            }
            set
            {
                this._regConfig.Application = value;
            }
        }

        internal Type[] NormalTypes
        {
            get
            {
                return this._normal;
            }
        }

        internal string Partition
        {
            get
            {
                return this._regConfig.Partition;
            }
            set
            {
                this._regConfig.Partition = value;
            }
        }

        internal string TypeLib
        {
            get
            {
                return this._regConfig.TypeLibrary;
            }
        }
    }
}

