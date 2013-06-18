namespace System.Windows.Forms.Design
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.ComponentModel.Design.Serialization;
    using System.Windows.Forms;

    internal class TableLayoutPanelCodeDomSerializer : CodeDomSerializer
    {
        private static readonly string LayoutSettingsPropName = "LayoutSettings";

        public override object Deserialize(IDesignerSerializationManager manager, object codeObject)
        {
            return this.GetBaseSerializer(manager).Deserialize(manager, codeObject);
        }

        private CodeDomSerializer GetBaseSerializer(IDesignerSerializationManager manager)
        {
            return (CodeDomSerializer) manager.GetSerializer(typeof(TableLayoutPanel).BaseType, typeof(CodeDomSerializer));
        }

        private bool IsLocalizable(IDesignerHost host)
        {
            if (host != null)
            {
                PropertyDescriptor descriptor = TypeDescriptor.GetProperties(host.RootComponent)["Localizable"];
                if ((descriptor != null) && (descriptor.PropertyType == typeof(bool)))
                {
                    return (bool) descriptor.GetValue(host.RootComponent);
                }
            }
            return false;
        }

        public override object Serialize(IDesignerSerializationManager manager, object value)
        {
            object obj2 = this.GetBaseSerializer(manager).Serialize(manager, value);
            TableLayoutPanel component = value as TableLayoutPanel;
            if (component != null)
            {
                InheritanceAttribute attribute = (InheritanceAttribute) TypeDescriptor.GetAttributes(component)[typeof(InheritanceAttribute)];
                if ((attribute == null) || (attribute.InheritanceLevel != InheritanceLevel.InheritedReadOnly))
                {
                    IDesignerHost service = (IDesignerHost) manager.GetService(typeof(IDesignerHost));
                    if (this.IsLocalizable(service))
                    {
                        PropertyDescriptor descriptor = TypeDescriptor.GetProperties(component)[LayoutSettingsPropName];
                        object obj3 = (descriptor != null) ? descriptor.GetValue(component) : null;
                        if (obj3 != null)
                        {
                            string resourceName = manager.GetName(component) + "." + LayoutSettingsPropName;
                            base.SerializeResourceInvariant(manager, resourceName, obj3);
                        }
                    }
                }
            }
            return obj2;
        }
    }
}

