namespace Microsoft.JScript
{
    using Microsoft.JScript.Vsa;
    using System;
    using System.Collections;
    using System.Security.Permissions;

    internal class VsaScriptScope : VsaItem, IVsaScriptScope, IJSVsaItem, IDebugScriptScope
    {
        private bool isClosed;
        private bool isCompiled;
        private ArrayList items;
        private VsaScriptScope parent;
        private GlobalScope scope;

        internal VsaScriptScope(VsaEngine engine, string itemName, VsaScriptScope parent) : base(engine, itemName, (JSVsaItemType) 0x13, JSVsaItemFlag.None)
        {
            this.parent = parent;
            this.scope = null;
            this.items = new ArrayList(8);
            this.isCompiled = false;
            this.isClosed = false;
        }

        public virtual IJSVsaItem AddItem(string itemName, JSVsaItemType type)
        {
            VsaItem item = null;
            if (this.isClosed)
            {
                throw new JSVsaException(JSVsaError.EngineClosed);
            }
            if (this.GetItem(itemName) != null)
            {
                throw new JSVsaException(JSVsaError.ItemNameInUse);
            }
            switch (((int) type))
            {
                case 0x10:
                case 0x11:
                case 0x12:
                    item = new VsaHostObject(base.engine, itemName, type, this);
                    if ((type == ((JSVsaItemType) 0x11)) || (type == ((JSVsaItemType) 0x12)))
                    {
                        ((VsaHostObject) item).exposeMembers = true;
                    }
                    if ((type == ((JSVsaItemType) 0x10)) || (type == ((JSVsaItemType) 0x12)))
                    {
                        ((VsaHostObject) item).isVisible = true;
                    }
                    if (base.engine.IsRunning)
                    {
                        ((VsaHostObject) item).Compile();
                        ((VsaHostObject) item).Run();
                    }
                    break;

                case 0x13:
                    item = new VsaScriptScope(base.engine, itemName, this);
                    break;

                case 20:
                    item = new VsaScriptCode(base.engine, itemName, type, this);
                    break;

                case 0x15:
                    if (!base.engine.IsRunning)
                    {
                        throw new JSVsaException(JSVsaError.EngineNotRunning);
                    }
                    item = new VsaScriptCode(base.engine, itemName, type, this);
                    break;

                case 0x16:
                    if (!base.engine.IsRunning)
                    {
                        throw new JSVsaException(JSVsaError.EngineNotRunning);
                    }
                    item = new VsaScriptCode(base.engine, itemName, type, this);
                    break;
            }
            if (item == null)
            {
                throw new JSVsaException(JSVsaError.ItemTypeNotSupported);
            }
            this.items.Add(item);
            return item;
        }

        internal override void CheckForErrors()
        {
            if (this.items.Count != 0)
            {
                try
                {
                    base.engine.Globals.ScopeStack.Push((ScriptObject) this.GetObject());
                    foreach (object obj2 in this.items)
                    {
                        ((VsaItem) obj2).CheckForErrors();
                    }
                }
                finally
                {
                    base.engine.Globals.ScopeStack.Pop();
                }
            }
        }

        internal override void Close()
        {
            foreach (object obj2 in this.items)
            {
                ((VsaItem) obj2).Close();
            }
            this.items = null;
            this.parent = null;
            this.scope = null;
            this.isClosed = true;
        }

        internal override void Compile()
        {
            if ((this.items.Count != 0) && !this.isCompiled)
            {
                this.isCompiled = true;
                try
                {
                    base.engine.Globals.ScopeStack.Push((ScriptObject) this.GetObject());
                    try
                    {
                        foreach (object obj2 in this.items)
                        {
                            ((VsaItem) obj2).Compile();
                        }
                    }
                    finally
                    {
                        base.engine.Globals.ScopeStack.Pop();
                    }
                }
                catch
                {
                    this.isCompiled = false;
                    throw;
                }
            }
        }

        public virtual IJSVsaItem CreateDynamicItem(string itemName, JSVsaItemType type)
        {
            if (!base.engine.IsRunning)
            {
                throw new JSVsaException(JSVsaError.EngineNotRunning);
            }
            return this.AddItem(itemName, type);
        }

        public virtual IJSVsaItem GetItem(string itemName)
        {
            int num = 0;
            int count = this.items.Count;
            while (num < count)
            {
                VsaItem item = (VsaItem) this.items[num];
                if (((item.Name == null) && (itemName == null)) || ((item.Name != null) && item.Name.Equals(itemName)))
                {
                    return (IJSVsaItem) this.items[num];
                }
                num++;
            }
            return null;
        }

        public virtual IJSVsaItem GetItemAtIndex(int index)
        {
            if (index >= this.items.Count)
            {
                throw new JSVsaException(JSVsaError.ItemNotFound);
            }
            return (IJSVsaItem) this.items[index];
        }

        public virtual int GetItemCount()
        {
            return this.items.Count;
        }

        public virtual object GetObject()
        {
            if (this.scope == null)
            {
                if (this.parent != null)
                {
                    this.scope = new GlobalScope((GlobalScope) this.parent.GetObject(), base.engine, false);
                }
                else
                {
                    this.scope = new GlobalScope(null, base.engine);
                }
            }
            return this.scope;
        }

        public virtual void RemoveItem(IJSVsaItem item)
        {
            int num = 0;
            int count = this.items.Count;
            while (num < count)
            {
                VsaItem item2 = (VsaItem) this.items[num];
                if (item2 == item)
                {
                    item2.Remove();
                    this.items.Remove(num);
                    return;
                }
                num++;
            }
            throw new JSVsaException(JSVsaError.ItemNotFound);
        }

        public virtual void RemoveItem(string itemName)
        {
            int num = 0;
            int count = this.items.Count;
            while (num < count)
            {
                VsaItem item = (VsaItem) this.items[num];
                if (((item.Name == null) && (itemName == null)) || ((item.Name != null) && item.Name.Equals(itemName)))
                {
                    item.Remove();
                    this.items.Remove(num);
                    return;
                }
                num++;
            }
            throw new JSVsaException(JSVsaError.ItemNotFound);
        }

        public virtual void RemoveItemAtIndex(int index)
        {
            if (index >= this.items.Count)
            {
                throw new JSVsaException(JSVsaError.ItemNotFound);
            }
            ((VsaItem) this.items[index]).Remove();
            this.items.Remove(index);
        }

        internal void ReRun(GlobalScope scope)
        {
            foreach (object obj2 in this.items)
            {
                if (obj2 is VsaHostObject)
                {
                    ((VsaHostObject) obj2).ReRun(scope);
                }
            }
            if (this.parent != null)
            {
                this.parent.ReRun(scope);
            }
        }

        internal override void Reset()
        {
            foreach (object obj2 in this.items)
            {
                ((VsaItem) obj2).Reset();
            }
        }

        internal override void Run()
        {
            if (this.items.Count != 0)
            {
                try
                {
                    base.engine.Globals.ScopeStack.Push((ScriptObject) this.GetObject());
                    foreach (object obj2 in this.items)
                    {
                        ((VsaItem) obj2).Run();
                    }
                }
                finally
                {
                    base.engine.Globals.ScopeStack.Pop();
                }
            }
        }

        [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
        public virtual void SetThisValue(object thisValue)
        {
            if (this.scope != null)
            {
                this.scope.thisObject = thisValue;
            }
        }

        public IVsaScriptScope Parent
        {
            get
            {
                return this.parent;
            }
        }
    }
}

