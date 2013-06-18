namespace Microsoft.JScript
{
    using Microsoft.JScript.Vsa;
    using System;
    using System.Security.Permissions;

    public abstract class VsaItem : IJSVsaItem
    {
        internal string codebase;
        internal VsaEngine engine;
        protected JSVsaItemFlag flag;
        protected bool isDirty;
        protected string name;
        protected JSVsaItemType type;

        internal VsaItem(VsaEngine engine, string itemName, JSVsaItemType type, JSVsaItemFlag flag)
        {
            this.engine = engine;
            this.type = type;
            this.name = itemName;
            this.flag = flag;
            this.codebase = null;
            this.isDirty = true;
        }

        internal virtual void CheckForErrors()
        {
        }

        internal virtual void Close()
        {
            this.engine = null;
        }

        internal virtual void Compile()
        {
        }

        internal virtual Type GetCompiledType()
        {
            return null;
        }

        [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
        public virtual object GetOption(string name)
        {
            if (this.engine == null)
            {
                throw new JSVsaException(JSVsaError.EngineClosed);
            }
            if (string.Compare(name, "codebase", StringComparison.OrdinalIgnoreCase) != 0)
            {
                throw new JSVsaException(JSVsaError.OptionNotSupported);
            }
            return this.codebase;
        }

        internal virtual void Remove()
        {
            this.engine = null;
        }

        internal virtual void Reset()
        {
        }

        internal virtual void Run()
        {
        }

        [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
        public virtual void SetOption(string name, object value)
        {
            if (this.engine == null)
            {
                throw new JSVsaException(JSVsaError.EngineClosed);
            }
            if (string.Compare(name, "codebase", StringComparison.OrdinalIgnoreCase) != 0)
            {
                throw new JSVsaException(JSVsaError.OptionNotSupported);
            }
            this.codebase = (string) value;
            this.isDirty = true;
            this.engine.IsDirty = true;
        }

        public virtual bool IsDirty
        {
            [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
            get
            {
                if (this.engine == null)
                {
                    throw new JSVsaException(JSVsaError.EngineClosed);
                }
                return this.isDirty;
            }
            [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
            set
            {
                if (this.engine == null)
                {
                    throw new JSVsaException(JSVsaError.EngineClosed);
                }
                this.isDirty = value;
            }
        }

        public JSVsaItemType ItemType
        {
            [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
            get
            {
                if (this.engine == null)
                {
                    throw new JSVsaException(JSVsaError.EngineClosed);
                }
                return this.type;
            }
        }

        public virtual string Name
        {
            [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
            get
            {
                if (this.engine == null)
                {
                    throw new JSVsaException(JSVsaError.EngineClosed);
                }
                return this.name;
            }
            [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
            set
            {
                if (this.engine == null)
                {
                    throw new JSVsaException(JSVsaError.EngineClosed);
                }
                if (this.name != value)
                {
                    if (!this.engine.IsValidIdentifier(value))
                    {
                        throw new JSVsaException(JSVsaError.ItemNameInvalid);
                    }
                    foreach (IJSVsaItem item in this.engine.Items)
                    {
                        if (item.Name.Equals(value))
                        {
                            throw new JSVsaException(JSVsaError.ItemNameInUse);
                        }
                    }
                    this.name = value;
                    this.isDirty = true;
                    this.engine.IsDirty = true;
                }
            }
        }
    }
}

