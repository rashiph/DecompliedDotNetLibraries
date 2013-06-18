namespace System.Web.UI.WebControls
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Web.UI;

    public class ControlIDConverter : StringConverter
    {
        protected virtual bool FilterControl(Control control)
        {
            return true;
        }

        private string[] GetControls(IDesignerHost host, object instance)
        {
            IContainer container = host.Container;
            IComponent component = instance as IComponent;
            if ((component != null) && (component.Site != null))
            {
                container = component.Site.Container;
            }
            if (container == null)
            {
                return null;
            }
            ComponentCollection components = container.Components;
            ArrayList list = new ArrayList();
            foreach (IComponent component2 in components)
            {
                Control control = component2 as Control;
                if ((((control != null) && (control != instance)) && ((control != host.RootComponent) && (control.ID != null))) && ((control.ID.Length > 0) && this.FilterControl(control)))
                {
                    list.Add(control.ID);
                }
            }
            list.Sort(Comparer.Default);
            return (string[]) list.ToArray(typeof(string));
        }

        public override TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            if (context == null)
            {
                return null;
            }
            IDesignerHost service = (IDesignerHost) context.GetService(typeof(IDesignerHost));
            if (service == null)
            {
                return null;
            }
            string[] controls = this.GetControls(service, context.Instance);
            if (controls == null)
            {
                return null;
            }
            return new TypeConverter.StandardValuesCollection(controls);
        }

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return false;
        }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return (context != null);
        }
    }
}

