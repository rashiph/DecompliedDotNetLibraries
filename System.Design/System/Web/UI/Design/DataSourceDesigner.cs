namespace System.Web.UI.Design
{
    using System;
    using System.ComponentModel.Design;
    using System.Design;
    using System.Security.Permissions;
    using System.Threading;

    [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    public class DataSourceDesigner : ControlDesigner, IDataSourceDesigner
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

        public virtual DesignerDataSourceView GetView(string viewName)
        {
            return null;
        }

        public virtual string[] GetViewNames()
        {
            return new string[0];
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

        public static bool SchemasEquivalent(IDataSourceSchema schema1, IDataSourceSchema schema2)
        {
            if ((schema1 == null) ^ (schema2 == null))
            {
                return false;
            }
            if ((schema1 != null) || (schema2 != null))
            {
                IDataSourceViewSchema[] views = schema1.GetViews();
                IDataSourceViewSchema[] schemaArray2 = schema2.GetViews();
                if ((views == null) ^ (schemaArray2 == null))
                {
                    return false;
                }
                if ((views == null) && (schemaArray2 == null))
                {
                    return true;
                }
                int length = views.Length;
                int num2 = schemaArray2.Length;
                if (length != num2)
                {
                    return false;
                }
                foreach (IDataSourceViewSchema schema in views)
                {
                    bool flag = false;
                    string name = schema.Name;
                    foreach (IDataSourceViewSchema schema3 in schemaArray2)
                    {
                        if ((name == schema3.Name) && ViewSchemasEquivalent(schema, schema3))
                        {
                            flag = true;
                            break;
                        }
                    }
                    if (!flag)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public virtual void SuppressDataSourceEvents()
        {
            this._suppressEventsCount++;
        }

        public static bool ViewSchemasEquivalent(IDataSourceViewSchema viewSchema1, IDataSourceViewSchema viewSchema2)
        {
            if ((viewSchema1 == null) ^ (viewSchema2 == null))
            {
                return false;
            }
            if ((viewSchema1 != null) || (viewSchema2 != null))
            {
                IDataSourceFieldSchema[] fields = viewSchema1.GetFields();
                IDataSourceFieldSchema[] schemaArray2 = viewSchema2.GetFields();
                if ((fields == null) ^ (schemaArray2 == null))
                {
                    return false;
                }
                if ((fields == null) && (schemaArray2 == null))
                {
                    return true;
                }
                int length = fields.Length;
                int num2 = schemaArray2.Length;
                if (length != num2)
                {
                    return false;
                }
                foreach (IDataSourceFieldSchema schema in fields)
                {
                    bool flag = false;
                    string name = schema.Name;
                    Type dataType = schema.DataType;
                    foreach (IDataSourceFieldSchema schema2 in schemaArray2)
                    {
                        if ((name == schema2.Name) && (dataType == schema2.DataType))
                        {
                            flag = true;
                            break;
                        }
                    }
                    if (!flag)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public override DesignerActionListCollection ActionLists
        {
            get
            {
                DesignerActionListCollection lists = new DesignerActionListCollection();
                lists.AddRange(base.ActionLists);
                lists.Add(new DataSourceDesignerActionList(this));
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

        private class DataSourceDesignerActionList : DesignerActionList
        {
            private DataSourceDesigner _parent;

            public DataSourceDesignerActionList(DataSourceDesigner parent) : base(parent.Component)
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

