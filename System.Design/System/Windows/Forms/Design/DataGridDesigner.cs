namespace System.Windows.Forms.Design
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Data;
    using System.Design;
    using System.Windows.Forms;

    internal class DataGridDesigner : ControlDesigner
    {
        private IComponentChangeService changeNotificationService;
        protected DesignerVerbCollection designerVerbs = new DesignerVerbCollection();

        private DataGridDesigner()
        {
            this.designerVerbs.Add(new DesignerVerb(System.Design.SR.GetString("DataGridAutoFormatString"), new EventHandler(this.OnAutoFormat)));
            base.AutoResizeHandles = true;
        }

        private void DataSource_ComponentRemoved(object sender, ComponentEventArgs e)
        {
            DataGrid component = (DataGrid) base.Component;
            if (e.Component == component.DataSource)
            {
                component.DataSource = null;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.changeNotificationService != null))
            {
                this.changeNotificationService.ComponentRemoved -= new ComponentEventHandler(this.DataSource_ComponentRemoved);
            }
            base.Dispose(disposing);
        }

        public override void Initialize(IComponent component)
        {
            base.Initialize(component);
            IDesignerHost service = (IDesignerHost) this.GetService(typeof(IDesignerHost));
            if (service != null)
            {
                this.changeNotificationService = (IComponentChangeService) service.GetService(typeof(IComponentChangeService));
                if (this.changeNotificationService != null)
                {
                    this.changeNotificationService.ComponentRemoved += new ComponentEventHandler(this.DataSource_ComponentRemoved);
                }
            }
        }

        private void OnAutoFormat(object sender, EventArgs e)
        {
            DataGrid component = base.Component as DataGrid;
            DataGridAutoFormatDialog dialog = new DataGridAutoFormatDialog(component);
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                DataRow selectedData = dialog.SelectedData;
                DesignerTransaction transaction = ((IDesignerHost) this.GetService(typeof(IDesignerHost))).CreateTransaction(System.Design.SR.GetString("DataGridAutoFormatUndoTitle", new object[] { base.Component.Site.Name }));
                try
                {
                    if (selectedData != null)
                    {
                        PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(typeof(DataGrid));
                        foreach (DataColumn column in selectedData.Table.Columns)
                        {
                            object obj3 = selectedData[column];
                            PropertyDescriptor descriptor = properties[column.ColumnName];
                            if (descriptor != null)
                            {
                                if (Convert.IsDBNull(obj3) || (obj3.ToString().Length == 0))
                                {
                                    descriptor.ResetValue(component);
                                }
                                else
                                {
                                    try
                                    {
                                        object obj4 = descriptor.Converter.ConvertFromString(obj3.ToString());
                                        descriptor.SetValue(component, obj4);
                                    }
                                    catch
                                    {
                                    }
                                }
                            }
                        }
                    }
                }
                finally
                {
                    transaction.Commit();
                }
                component.Invalidate();
            }
        }

        public override DesignerVerbCollection Verbs
        {
            get
            {
                return this.designerVerbs;
            }
        }
    }
}

