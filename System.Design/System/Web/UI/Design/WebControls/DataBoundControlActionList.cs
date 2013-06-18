namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Design;
    using System.Web.UI.Design;

    internal class DataBoundControlActionList : DesignerActionList
    {
        private ControlDesigner _controlDesigner;
        private IDataSourceDesigner _dataSourceDesigner;

        public DataBoundControlActionList(ControlDesigner controlDesigner, IDataSourceDesigner dataSourceDesigner) : base(controlDesigner.Component)
        {
            this._controlDesigner = controlDesigner;
            this._dataSourceDesigner = dataSourceDesigner;
        }

        public override DesignerActionItemCollection GetSortedActionItems()
        {
            DesignerActionItemCollection items = new DesignerActionItemCollection();
            PropertyDescriptor descriptor = TypeDescriptor.GetProperties(this._controlDesigner.Component)["DataSourceID"];
            if ((descriptor != null) && descriptor.IsBrowsable)
            {
                items.Add(new DesignerActionPropertyItem("DataSourceID", System.Design.SR.GetString("BaseDataBoundControl_ConfigureDataVerb"), System.Design.SR.GetString("BaseDataBoundControl_DataActionGroup"), System.Design.SR.GetString("BaseDataBoundControl_ConfigureDataVerbDesc")));
            }
            ControlDesigner designer = this._dataSourceDesigner as ControlDesigner;
            if (designer != null)
            {
                ((DesignerActionPropertyItem) items[0]).RelatedComponent = designer.Component;
            }
            return items;
        }

        private bool SetDataSourceIDCallback(object context)
        {
            string str = (string) context;
            DataBoundControlDesigner designer = this._controlDesigner as DataBoundControlDesigner;
            if (designer != null)
            {
                TypeDescriptor.GetProperties(designer.Component)["DataSourceID"].SetValue(designer.Component, str);
            }
            else
            {
                BaseDataListDesigner designer2 = this._controlDesigner as BaseDataListDesigner;
                if (designer2 != null)
                {
                    TypeDescriptor.GetProperties(designer2.Component)["DataSourceID"].SetValue(designer2.Component, str);
                }
                else
                {
                    RepeaterDesigner designer3 = this._controlDesigner as RepeaterDesigner;
                    if (designer3 != null)
                    {
                        TypeDescriptor.GetProperties(designer3.Component)["DataSourceID"].SetValue(designer3.Component, str);
                    }
                }
            }
            return true;
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

        [TypeConverter(typeof(DataSourceIDConverter))]
        public string DataSourceID
        {
            get
            {
                string dataSourceID = null;
                DataBoundControlDesigner designer = this._controlDesigner as DataBoundControlDesigner;
                if (designer != null)
                {
                    dataSourceID = designer.DataSourceID;
                }
                else
                {
                    BaseDataListDesigner designer2 = this._controlDesigner as BaseDataListDesigner;
                    if (designer2 != null)
                    {
                        dataSourceID = designer2.DataSourceID;
                    }
                    else
                    {
                        RepeaterDesigner designer3 = this._controlDesigner as RepeaterDesigner;
                        if (designer3 != null)
                        {
                            dataSourceID = designer3.DataSourceID;
                        }
                    }
                }
                if (string.IsNullOrEmpty(dataSourceID))
                {
                    return System.Design.SR.GetString("DataSourceIDChromeConverter_NoDataSource");
                }
                return dataSourceID;
            }
            set
            {
                ControlDesigner.InvokeTransactedChange(this._controlDesigner.Component, new TransactedChangeCallback(this.SetDataSourceIDCallback), value, System.Design.SR.GetString("DataBoundControlActionList_SetDataSourceIDTransaction"));
            }
        }
    }
}

