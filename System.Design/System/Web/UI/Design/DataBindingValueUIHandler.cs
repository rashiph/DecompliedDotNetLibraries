namespace System.Web.UI.Design
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Design;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Security.Permissions;
    using System.Web.UI;

    [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    public class DataBindingValueUIHandler
    {
        private Bitmap dataBindingBitmap;
        private string dataBindingToolTip;

        public void OnGetUIValueItem(ITypeDescriptorContext context, PropertyDescriptor propDesc, ArrayList valueUIItemList)
        {
            Control instance = context.Instance as Control;
            if (instance != null)
            {
                IDataBindingsAccessor accessor = instance;
                if (accessor.HasDataBindings && (accessor.DataBindings[propDesc.Name] != null))
                {
                    valueUIItemList.Add(new DataBindingUIItem(this));
                }
            }
        }

        private void OnValueUIItemInvoke(ITypeDescriptorContext context, PropertyDescriptor propDesc, PropertyValueUIItem invokedItem)
        {
        }

        private Bitmap DataBindingBitmap
        {
            get
            {
                if (this.dataBindingBitmap == null)
                {
                    this.dataBindingBitmap = new Bitmap(typeof(DataBindingValueUIHandler), "DataBindingGlyph.bmp");
                    this.dataBindingBitmap.MakeTransparent();
                }
                return this.dataBindingBitmap;
            }
        }

        private string DataBindingToolTip
        {
            get
            {
                if (this.dataBindingToolTip == null)
                {
                    this.dataBindingToolTip = System.Design.SR.GetString("DataBindingGlyph_ToolTip");
                }
                return this.dataBindingToolTip;
            }
        }

        private class DataBindingUIItem : PropertyValueUIItem
        {
            public DataBindingUIItem(DataBindingValueUIHandler handler) : base(handler.DataBindingBitmap, new PropertyValueUIItemInvokeHandler(handler.OnValueUIItemInvoke), handler.DataBindingToolTip)
            {
            }
        }
    }
}

