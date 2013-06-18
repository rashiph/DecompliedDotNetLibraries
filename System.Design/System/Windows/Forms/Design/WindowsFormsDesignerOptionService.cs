namespace System.Windows.Forms.Design
{
    using System;
    using System.ComponentModel.Design;

    public class WindowsFormsDesignerOptionService : DesignerOptionService
    {
        private DesignerOptions _options;

        protected override void PopulateOptionCollection(DesignerOptionService.DesignerOptionCollection options)
        {
            if (options.Parent == null)
            {
                DesignerOptions compatibilityOptions = this.CompatibilityOptions;
                if (compatibilityOptions != null)
                {
                    base.CreateOptionCollection(options, "DesignerOptions", compatibilityOptions);
                }
            }
        }

        public virtual DesignerOptions CompatibilityOptions
        {
            get
            {
                if (this._options == null)
                {
                    this._options = new DesignerOptions();
                }
                return this._options;
            }
        }
    }
}

