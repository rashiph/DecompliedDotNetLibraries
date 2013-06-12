namespace System.Drawing.Design
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Security.Permissions;

    [PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust"), PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
    public class PaintValueEventArgs : EventArgs
    {
        private readonly Rectangle bounds;
        private readonly ITypeDescriptorContext context;
        private readonly System.Drawing.Graphics graphics;
        private readonly object valueToPaint;

        public PaintValueEventArgs(ITypeDescriptorContext context, object value, System.Drawing.Graphics graphics, Rectangle bounds)
        {
            this.context = context;
            this.valueToPaint = value;
            this.graphics = graphics;
            if (graphics == null)
            {
                throw new ArgumentNullException("graphics");
            }
            this.bounds = bounds;
        }

        public Rectangle Bounds
        {
            get
            {
                return this.bounds;
            }
        }

        public ITypeDescriptorContext Context
        {
            get
            {
                return this.context;
            }
        }

        public System.Drawing.Graphics Graphics
        {
            get
            {
                return this.graphics;
            }
        }

        public object Value
        {
            get
            {
                return this.valueToPaint;
            }
        }
    }
}

