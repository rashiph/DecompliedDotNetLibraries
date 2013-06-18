namespace System.Web.UI.WebControls
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Web.UI;

    public class ControlPropertyNameConverter : StringConverter
    {
        private string[] GetPropertyNames(Control control)
        {
            ArrayList list = new ArrayList();
            foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(control.GetType()))
            {
                list.Add(descriptor.Name);
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
            ControlParameter instance = (ControlParameter) context.Instance;
            string controlID = instance.ControlID;
            if (string.IsNullOrEmpty(controlID))
            {
                return null;
            }
            IDesignerHost service = (IDesignerHost) context.GetService(typeof(IDesignerHost));
            if (service == null)
            {
                return null;
            }
            Control control = service.Container.Components[controlID] as Control;
            if (control == null)
            {
                return null;
            }
            return new TypeConverter.StandardValuesCollection(this.GetPropertyNames(control));
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

