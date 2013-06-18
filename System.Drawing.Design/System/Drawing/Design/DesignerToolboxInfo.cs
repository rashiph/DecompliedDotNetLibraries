namespace System.Drawing.Design
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;

    internal sealed class DesignerToolboxInfo : IDisposable
    {
        private Hashtable _attributeHash;
        private ArrayList _filter;
        private IDesigner _filterDesigner;
        private IDesignerHost _host;
        private ISelectionService _selectionService;
        private ToolboxService _toolboxService;
        private IToolboxUser _toolboxUser;

        internal DesignerToolboxInfo(ToolboxService toolboxService, IDesignerHost host)
        {
            this._toolboxService = toolboxService;
            this._host = host;
            this._selectionService = host.GetService(typeof(ISelectionService)) as ISelectionService;
            if (this._selectionService != null)
            {
                this._selectionService.SelectionChanged += new EventHandler(this.OnSelectionChanged);
            }
            if (this._host.RootComponent != null)
            {
                this._host.RootComponent.Disposed += new EventHandler(this.OnDesignerDisposed);
            }
            TypeDescriptor.Refreshed += new RefreshEventHandler(this.OnTypeDescriptorRefresh);
        }

        public AttributeCollection GetDesignerAttributes(IDesigner designer)
        {
            if (designer == null)
            {
                throw new ArgumentNullException("designer");
            }
            if (this._attributeHash == null)
            {
                this._attributeHash = new Hashtable();
            }
            else
            {
                this._attributeHash.Clear();
            }
            if (!(designer is ITreeDesigner))
            {
                IComponent rootComponent = this._host.RootComponent;
                if (rootComponent != null)
                {
                    this.RecurseDesignerTree(this._host.GetDesigner(rootComponent), this._attributeHash);
                }
            }
            this.RecurseDesignerTree(designer, this._attributeHash);
            Attribute[] array = new Attribute[this._attributeHash.Values.Count];
            this._attributeHash.Values.CopyTo(array, 0);
            return new AttributeCollection(array);
        }

        private void OnDesignerDisposed(object sender, EventArgs e)
        {
            if (this._toolboxService._lastState == this)
            {
                this._toolboxService._lastState = null;
            }
        }

        private void OnSelectionChanged(object sender, EventArgs e)
        {
            if (this.Update())
            {
                this._toolboxService.OnDesignerInfoChanged(this);
            }
        }

        private void OnTypeDescriptorRefresh(RefreshEventArgs r)
        {
            if (r.ComponentChanged == this._filterDesigner)
            {
                this._filter = null;
                this._filterDesigner = null;
            }
        }

        private void RecurseDesignerTree(IDesigner designer, Hashtable table)
        {
            ITreeDesigner designer2 = designer as ITreeDesigner;
            if (designer2 != null)
            {
                IDesigner parent = designer2.Parent;
                if (parent != null)
                {
                    this.RecurseDesignerTree(parent, table);
                }
            }
            foreach (Attribute attribute in TypeDescriptor.GetAttributes(designer))
            {
                table[attribute.TypeId] = attribute;
            }
        }

        void IDisposable.Dispose()
        {
            if (this._selectionService != null)
            {
                this._selectionService.SelectionChanged -= new EventHandler(this.OnSelectionChanged);
            }
            if (this._host.RootComponent != null)
            {
                this._host.RootComponent.Disposed -= new EventHandler(this.OnDesignerDisposed);
            }
            TypeDescriptor.Refreshed -= new RefreshEventHandler(this.OnTypeDescriptorRefresh);
        }

        private bool Update()
        {
            bool flag = false;
            IDesigner designer = null;
            IComponent primarySelection = this._selectionService.PrimarySelection as IComponent;
            if (primarySelection != null)
            {
                designer = this._host.GetDesigner(primarySelection);
            }
            if (designer == null)
            {
                primarySelection = this._host.RootComponent;
                if (primarySelection != null)
                {
                    designer = this._host.GetDesigner(primarySelection);
                }
            }
            if (designer != this._filterDesigner)
            {
                ArrayList list;
                if (designer != null)
                {
                    AttributeCollection designerAttributes = this.GetDesignerAttributes(designer);
                    list = new ArrayList(designerAttributes.Count);
                    foreach (Attribute attribute in designerAttributes)
                    {
                        if (attribute is ToolboxItemFilterAttribute)
                        {
                            list.Add(attribute);
                        }
                    }
                }
                else
                {
                    list = new ArrayList();
                }
                if (this._filter == null)
                {
                    flag = true;
                }
                else if (this._filter.Count != list.Count)
                {
                    flag = true;
                }
                else
                {
                    IEnumerator enumerator = this._filter.GetEnumerator();
                    IEnumerator enumerator2 = list.GetEnumerator();
                    while (enumerator2.MoveNext())
                    {
                        enumerator.MoveNext();
                        if (!enumerator2.Current.Equals(enumerator.Current))
                        {
                            flag = true;
                            break;
                        }
                        ToolboxItemFilterAttribute current = (ToolboxItemFilterAttribute) enumerator2.Current;
                        if (current.FilterType == ToolboxItemFilterType.Custom)
                        {
                            flag = true;
                            break;
                        }
                    }
                }
                this._filter = list;
                this._filterDesigner = designer;
                this._toolboxUser = this._filterDesigner as IToolboxUser;
                if (this._toolboxUser == null)
                {
                    IDesigner parent;
                    for (ITreeDesigner designer2 = this._filterDesigner as ITreeDesigner; (this._toolboxUser == null) && (designer2 != null); designer2 = parent as ITreeDesigner)
                    {
                        parent = designer2.Parent;
                        this._toolboxUser = parent as IToolboxUser;
                    }
                }
                if ((this._toolboxUser == null) && (this._host.RootComponent != null))
                {
                    this._toolboxUser = this._host.GetDesigner(this._host.RootComponent) as IToolboxUser;
                }
            }
            if (this._filter == null)
            {
                this._filter = new ArrayList();
            }
            return flag;
        }

        internal IDesignerHost DesignerHost
        {
            get
            {
                return this._host;
            }
        }

        internal ICollection Filter
        {
            get
            {
                if (this._filter == null)
                {
                    this.Update();
                }
                return this._filter;
            }
        }

        internal IToolboxUser ToolboxUser
        {
            get
            {
                if (this._toolboxUser == null)
                {
                    this.Update();
                }
                return this._toolboxUser;
            }
        }
    }
}

