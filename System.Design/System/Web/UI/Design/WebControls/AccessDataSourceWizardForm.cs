namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.ComponentModel.Design.Data;
    using System.Drawing;
    using System.Web.UI.WebControls;

    internal class AccessDataSourceWizardForm : SqlDataSourceWizardForm
    {
        public AccessDataSourceWizardForm(IServiceProvider serviceProvider, AccessDataSourceDesigner accessDataSourceDesigner, IDataEnvironment dataEnvironment) : base(serviceProvider, accessDataSourceDesigner, dataEnvironment)
        {
            base.Glyph = new Bitmap(typeof(AccessDataSourceWizardForm), "datasourcewizard.bmp");
        }

        protected override SqlDataSourceConnectionPanel CreateConnectionPanel()
        {
            AccessDataSourceDesigner sqlDataSourceDesigner = (AccessDataSourceDesigner) base.SqlDataSourceDesigner;
            return new AccessDataSourceConnectionChooserPanel(sqlDataSourceDesigner, (AccessDataSource) sqlDataSourceDesigner.Component);
        }

        protected override string HelpTopic
        {
            get
            {
                return "net.Asp.AccessDataSource.ConfigureDataSource";
            }
        }
    }
}

