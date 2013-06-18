namespace System.Web.UI.Design
{
    using System;
    using System.ComponentModel.Design;
    using System.Design;
    using System.Security.Permissions;
    using System.Threading;

    [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    public class HierarchicalDataSourceDesigner : ControlDesigner, IHierarchicalDataSourceDesigner
    {
        private bool _raiseDataSourceChangedEvent;
        private bool _raiseSchemaRefreshedEvent;
        private int _suppressEventsCount;

        private event EventHandler _dataSourceChangedEvent;

        private event EventHandler _schemaRefreshedEvent;

        public event EventHandler DataSourceChanged
        {
            add
            {
                this._dataSourceChangedEvent += value;
            }
            remove
            {
                this._dataSourceChangedEvent -= value;
            }
        }

        public event EventHandler SchemaRefreshed
        {
            add
            {
                this._schemaRefreshedEvent += value;
            }
            remove
            {
                this._schemaRefreshedEvent -= value;
            }
        }

        public virtual void Configure()
        {
            throw new NotSupportedException();
        }

        public override string GetDesignTimeHtml()
        {
            return base.CreatePlaceHolderDesignTimeHtml();
        }

        public virtual DesignerHierarchicalDataSourceView GetView(string viewPath)
        {
            return null;
        }

        protected virtual void OnDataSourceChanged(EventArgs e)
        {
            if (this.SuppressingDataSourceEvents)
            {
                this._raiseDataSourceChangedEvent = true;
            }
            else
            {
                if (this._dataSourceChangedEvent != null)
                {
                    this._dataSourceChangedEvent(this, e);
                }
                this._raiseDataSourceChangedEvent = false;
            }
        }

        protected virtual void OnSchemaRefreshed(EventArgs e)
        {
            if (this.SuppressingDataSourceEvents)
            {
                this._raiseSchemaRefreshedEvent = true;
            }
            else
            {
                if (this._schemaRefreshedEvent != null)
                {
                    this._schemaRefreshedEvent(this, e);
                }
                this._raiseSchemaRefreshedEvent = false;
            }
        }

        public virtual void RefreshSchema(bool preferSilent)
        {
            throw new NotSupportedException();
        }

        public virtual void ResumeDataSourceEvents()
        {
            if (this._suppressEventsCount == 0)
            {
                throw new InvalidOperationException(System.Design.SR.GetString("DataSource_CannotResumeEvents"));
            }
            this._suppressEventsCount--;
            if (this._suppressEventsCount == 0)
            {
                if (this._raiseDataSourceChangedEvent)
                {
                    this.OnDataSourceChanged(EventArgs.Empty);
                }
                if (this._raiseSchemaRefreshedEvent)
                {
                    this.OnSchemaRefreshed(EventArgs.Empty);
                }
            }
        }

        public virtual void SuppressDataSourceEvents()
        {
            this._suppressEventsCount++;
        }

        public override DesignerActionListCollection ActionLists
        {
            get
            {
                DesignerActionListCollection lists = new DesignerActionListCollection();
                lists.AddRange(base.ActionLists);
                lists.Add(new HierarchicalDataSourceDesignerActionList(this));
                return lists;
            }
        }

        public virtual bool CanConfigure
        {
            get
            {
                return false;
            }
        }

        public virtual bool CanRefreshSchema
        {
            get
            {
                return false;
            }
        }

        protected bool SuppressingDataSourceEvents
        {
            get
            {
                return (this._suppressEventsCount > 0);
            }
        }

        private class HierarchicalDataSourceDesignerActionList : DesignerActionList
        {
            private HierarchicalDataSourceDesigner _parent;

            public HierarchicalDataSourceDesignerActionList(HierarchicalDataSourceDesigner parent) : base(parent.Component)
            {
                this._parent = parent;
            }

            public void Configure()
            {
                this._parent.Configure();
            }

            public override DesignerActionItemCollection GetSortedActionItems()
            {
                DesignerActionItemCollection items = new DesignerActionItemCollection();
                if (this._parent.CanConfigure)
                {
                    DesignerActionMethodItem item = new DesignerActionMethodItem(this, "Configure", System.Design.SR.GetString("DataSourceDesigner_ConfigureDataSourceVerb"), System.Design.SR.GetString("DataSourceDesigner_DataActionGroup"), System.Design.SR.GetString("DataSourceDesigner_ConfigureDataSourceVerbDesc"), true) {
                        AllowAssociate = true
                    };
                    items.Add(item);
                }
                if (this._parent.CanRefreshSchema)
                {
                    DesignerActionMethodItem item2 = new DesignerActionMethodItem(this, "RefreshSchema", System.Design.SR.GetString("DataSourceDesigner_RefreshSchemaVerb"), System.Design.SR.GetString("DataSourceDesigner_DataActionGroup"), System.Design.SR.GetString("DataSourceDesigner_RefreshSchemaVerbDesc"), false) {
                        AllowAssociate = true
                    };
                    items.Add(item2);
                }
                return items;
            }

            public void RefreshSchema()
            {
                this._parent.RefreshSchema(false);
            }

            public override bool AutoShow
            {
                get
                {
                    return true;
                }
                set
                {
                }
            }
        }
    }
}

