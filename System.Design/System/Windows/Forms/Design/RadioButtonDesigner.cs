namespace System.Windows.Forms.Design
{
    using System;
    using System.Collections;
    using System.ComponentModel;

    internal class RadioButtonDesigner : ButtonBaseDesigner
    {
        public override void InitializeNewComponent(IDictionary defaultValues)
        {
            base.InitializeNewComponent(defaultValues);
            PropertyDescriptor descriptor = TypeDescriptor.GetProperties(base.Component)["TabStop"];
            if (((descriptor != null) && (descriptor.PropertyType == typeof(bool))) && (!descriptor.IsReadOnly && descriptor.IsBrowsable))
            {
                descriptor.SetValue(base.Component, true);
            }
        }
    }
}

