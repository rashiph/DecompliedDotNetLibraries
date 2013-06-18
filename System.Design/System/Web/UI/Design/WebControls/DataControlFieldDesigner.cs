namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.Web.UI;
    using System.Web.UI.Design;
    using System.Web.UI.Design.Util;
    using System.Web.UI.WebControls;

    public abstract class DataControlFieldDesigner
    {
        private System.Web.UI.Design.Util.DesignerForm _designerForm;

        protected DataControlFieldDesigner()
        {
        }

        public abstract DataControlField CreateField();
        public abstract DataControlField CreateField(IDataSourceFieldSchema fieldSchema);
        public abstract TemplateField CreateTemplateField(DataControlField dataControlField, DataBoundControl dataBoundControl);
        protected string GetNewDataSourceName(Type controlType, DataBoundControlMode mode)
        {
            DataControlFieldsEditor designerForm = this.DesignerForm as DataControlFieldsEditor;
            if (designerForm != null)
            {
                return designerForm.GetNewDataSourceName(controlType, mode);
            }
            return string.Empty;
        }

        public abstract string GetNodeText(DataControlField dataControlField);
        protected object GetService(Type serviceType)
        {
            if (this.ServiceProvider != null)
            {
                return this.ServiceProvider.GetService(serviceType);
            }
            return null;
        }

        protected ITemplate GetTemplate(DataBoundControl control, string templateContent)
        {
            return DataControlFieldHelper.GetTemplate(control, templateContent);
        }

        protected TemplateField GetTemplateField(DataControlField dataControlField, DataBoundControl dataBoundControl)
        {
            return DataControlFieldHelper.GetTemplateField(dataControlField, dataBoundControl);
        }

        public abstract bool IsEnabled(DataBoundControl parent);

        public abstract string DefaultNodeText { get; }

        internal System.Web.UI.Design.Util.DesignerForm DesignerForm
        {
            get
            {
                return this._designerForm;
            }
            set
            {
                this._designerForm = value;
            }
        }

        protected IServiceProvider ServiceProvider
        {
            get
            {
                return this._designerForm.ServiceProvider;
            }
        }

        public abstract bool UsesSchema { get; }
    }
}

