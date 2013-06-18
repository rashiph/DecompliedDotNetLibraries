namespace System.Web.UI.Design
{
    using System;
    using System.ComponentModel.Design;
    using System.Web.UI;

    public abstract class DesignerAutoFormat
    {
        private string _name;
        private DesignerAutoFormatStyle _style;

        protected DesignerAutoFormat(string name)
        {
            if ((name == null) || (name.Length == 0))
            {
                throw new ArgumentNullException("name");
            }
            this._name = name;
        }

        public abstract void Apply(Control control);
        public virtual Control GetPreviewControl(Control runtimeControl)
        {
            IDesignerHost service = (IDesignerHost) runtimeControl.Site.GetService(typeof(IDesignerHost));
            ControlDesigner designer = service.GetDesigner(runtimeControl) as ControlDesigner;
            if (designer != null)
            {
                return designer.CreateClonedControl(service, true);
            }
            return null;
        }

        public override string ToString()
        {
            return this.Name;
        }

        public string Name
        {
            get
            {
                return this._name;
            }
        }

        public DesignerAutoFormatStyle Style
        {
            get
            {
                if (this._style == null)
                {
                    this._style = new DesignerAutoFormatStyle();
                }
                return this._style;
            }
        }
    }
}

