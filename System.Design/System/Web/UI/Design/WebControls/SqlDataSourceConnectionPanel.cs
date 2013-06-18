namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.Collections;
    using System.ComponentModel.Design.Data;
    using System.Design;
    using System.Web.UI.Design.Util;
    using System.Web.UI.WebControls;

    internal abstract class SqlDataSourceConnectionPanel : WizardPanel
    {
        private SqlDataSourceDesigner _sqlDataSourceDesigner;

        protected SqlDataSourceConnectionPanel(SqlDataSourceDesigner sqlDataSourceDesigner)
        {
            this._sqlDataSourceDesigner = sqlDataSourceDesigner;
        }

        protected bool CheckValidProvider()
        {
            DesignerDataConnection dataConnection = this.DataConnection;
            try
            {
                SqlDataSourceDesigner.GetDbProviderFactory(dataConnection.ProviderName);
                return true;
            }
            catch (Exception exception)
            {
                UIServiceHelper.ShowError(base.ServiceProvider, exception, System.Design.SR.GetString("SqlDataSourceConnectionPanel_ProviderNotFound", new object[] { dataConnection.ProviderName }));
                return false;
            }
        }

        internal static WizardPanel CreateCommandPanel(SqlDataSourceWizardForm wizard, DesignerDataConnection dataConnection, WizardPanel nextPanel)
        {
            IDataEnvironment service = null;
            IServiceProvider site = wizard.SqlDataSourceDesigner.Component.Site;
            if (site != null)
            {
                service = (IDataEnvironment) site.GetService(typeof(IDataEnvironment));
            }
            bool flag = false;
            if (service != null)
            {
                try
                {
                    IDesignerDataSchema connectionSchema = service.GetConnectionSchema(dataConnection);
                    if (connectionSchema != null)
                    {
                        flag = connectionSchema.SupportsSchemaClass(DesignerDataSchemaClass.Tables);
                        if (flag)
                        {
                            connectionSchema.GetSchemaItems(DesignerDataSchemaClass.Tables);
                        }
                        else
                        {
                            flag = connectionSchema.SupportsSchemaClass(DesignerDataSchemaClass.Views);
                            connectionSchema.GetSchemaItems(DesignerDataSchemaClass.Views);
                        }
                    }
                }
                catch (Exception exception)
                {
                    UIServiceHelper.ShowError(site, exception, System.Design.SR.GetString("SqlDataSourceConnectionPanel_CouldNotGetConnectionSchema"));
                    return null;
                }
            }
            if (nextPanel == null)
            {
                if (flag)
                {
                    return wizard.GetConfigureSelectPanel();
                }
                return CreateCustomCommandPanel(wizard, dataConnection);
            }
            if (flag)
            {
                if (nextPanel is SqlDataSourceConfigureSelectPanel)
                {
                    return nextPanel;
                }
                return wizard.GetConfigureSelectPanel();
            }
            if (nextPanel is SqlDataSourceCustomCommandPanel)
            {
                return nextPanel;
            }
            return CreateCustomCommandPanel(wizard, dataConnection);
        }

        private static WizardPanel CreateCustomCommandPanel(SqlDataSourceWizardForm wizard, DesignerDataConnection dataConnection)
        {
            SqlDataSource component = (SqlDataSource) wizard.SqlDataSourceDesigner.Component;
            ArrayList dest = new ArrayList();
            ArrayList list2 = new ArrayList();
            ArrayList list3 = new ArrayList();
            ArrayList list4 = new ArrayList();
            wizard.SqlDataSourceDesigner.CopyList(component.SelectParameters, dest);
            wizard.SqlDataSourceDesigner.CopyList(component.InsertParameters, list2);
            wizard.SqlDataSourceDesigner.CopyList(component.UpdateParameters, list3);
            wizard.SqlDataSourceDesigner.CopyList(component.DeleteParameters, list4);
            SqlDataSourceCustomCommandPanel customCommandPanel = wizard.GetCustomCommandPanel();
            customCommandPanel.SetQueries(dataConnection, new SqlDataSourceQuery(component.SelectCommand, component.SelectCommandType, dest), new SqlDataSourceQuery(component.InsertCommand, component.InsertCommandType, list2), new SqlDataSourceQuery(component.UpdateCommand, component.UpdateCommandType, list3), new SqlDataSourceQuery(component.DeleteCommand, component.DeleteCommandType, list4));
            return customCommandPanel;
        }

        public override bool OnNext()
        {
            if (!this.CheckValidProvider())
            {
                return false;
            }
            WizardPanel panel = CreateCommandPanel((SqlDataSourceWizardForm) base.ParentWizard, this.DataConnection, base.NextPanel);
            if (panel == null)
            {
                return false;
            }
            base.NextPanel = panel;
            return true;
        }

        public abstract DesignerDataConnection DataConnection { get; }
    }
}

