namespace System.Windows.Forms.Design
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Windows.Forms;

    internal class DataGridViewComboBoxColumnDesigner : DataGridViewColumnDesigner
    {
        private static BindingContext bc;

        protected override void PreFilterProperties(IDictionary properties)
        {
            base.PreFilterProperties(properties);
            PropertyDescriptor oldPropertyDescriptor = (PropertyDescriptor) properties["ValueMember"];
            if (oldPropertyDescriptor != null)
            {
                properties["ValueMember"] = TypeDescriptor.CreateProperty(typeof(DataGridViewComboBoxColumnDesigner), oldPropertyDescriptor, new Attribute[0]);
            }
            oldPropertyDescriptor = (PropertyDescriptor) properties["DisplayMember"];
            if (oldPropertyDescriptor != null)
            {
                properties["DisplayMember"] = TypeDescriptor.CreateProperty(typeof(DataGridViewComboBoxColumnDesigner), oldPropertyDescriptor, new Attribute[0]);
            }
        }

        private bool ShouldSerializeDisplayMember()
        {
            DataGridViewComboBoxColumn component = (DataGridViewComboBoxColumn) base.Component;
            return !string.IsNullOrEmpty(component.DisplayMember);
        }

        private bool ShouldSerializeValueMember()
        {
            DataGridViewComboBoxColumn component = (DataGridViewComboBoxColumn) base.Component;
            return !string.IsNullOrEmpty(component.ValueMember);
        }

        private static bool ValidDataMember(object dataSource, string dataMember)
        {
            if (!string.IsNullOrEmpty(dataMember))
            {
                BindingManagerBase base2;
                if (bc == null)
                {
                    bc = new BindingContext();
                }
                BindingMemberInfo info = new BindingMemberInfo(dataMember);
                PropertyDescriptorCollection itemProperties = null;
                try
                {
                    base2 = bc[dataSource, info.BindingPath];
                }
                catch (ArgumentException)
                {
                    return false;
                }
                if (base2 == null)
                {
                    return false;
                }
                itemProperties = base2.GetItemProperties();
                if (itemProperties == null)
                {
                    return false;
                }
                if (itemProperties[info.BindingField] == null)
                {
                    return false;
                }
            }
            return true;
        }

        private string DisplayMember
        {
            get
            {
                DataGridViewComboBoxColumn component = (DataGridViewComboBoxColumn) base.Component;
                return component.DisplayMember;
            }
            set
            {
                DataGridViewComboBoxColumn component = (DataGridViewComboBoxColumn) base.Component;
                if ((component.DataSource != null) && ValidDataMember(component.DataSource, value))
                {
                    component.DisplayMember = value;
                }
            }
        }

        private string ValueMember
        {
            get
            {
                DataGridViewComboBoxColumn component = (DataGridViewComboBoxColumn) base.Component;
                return component.ValueMember;
            }
            set
            {
                DataGridViewComboBoxColumn component = (DataGridViewComboBoxColumn) base.Component;
                if ((component.DataSource != null) && ValidDataMember(component.DataSource, value))
                {
                    component.ValueMember = value;
                }
            }
        }
    }
}

