namespace System.Drawing.Design
{
    using System;
    using System.Drawing;
    using System.Security.Permissions;

    [PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust"), PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
    public class PropertyValueUIItem
    {
        private PropertyValueUIItemInvokeHandler handler;
        private System.Drawing.Image itemImage;
        private string tooltip;

        public PropertyValueUIItem(System.Drawing.Image uiItemImage, PropertyValueUIItemInvokeHandler handler, string tooltip)
        {
            this.itemImage = uiItemImage;
            this.handler = handler;
            if (this.itemImage == null)
            {
                throw new ArgumentNullException("uiItemImage");
            }
            if (handler == null)
            {
                throw new ArgumentNullException("handler");
            }
            this.tooltip = tooltip;
        }

        public virtual void Reset()
        {
        }

        public virtual System.Drawing.Image Image
        {
            get
            {
                return this.itemImage;
            }
        }

        public virtual PropertyValueUIItemInvokeHandler InvokeHandler
        {
            get
            {
                return this.handler;
            }
        }

        public virtual string ToolTip
        {
            get
            {
                return this.tooltip;
            }
        }
    }
}

