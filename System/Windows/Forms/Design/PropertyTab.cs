namespace System.Windows.Forms.Design
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Security.Permissions;

    [PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust")]
    public abstract class PropertyTab : IExtenderProvider
    {
        private System.Drawing.Bitmap bitmap;
        private bool checkedBmp;
        private object[] components;

        protected PropertyTab()
        {
        }

        public virtual bool CanExtend(object extendee)
        {
            return true;
        }

        public virtual void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && (this.bitmap != null))
            {
                this.bitmap.Dispose();
                this.bitmap = null;
            }
        }

        ~PropertyTab()
        {
            this.Dispose(false);
        }

        public virtual PropertyDescriptor GetDefaultProperty(object component)
        {
            return TypeDescriptor.GetDefaultProperty(component);
        }

        public virtual PropertyDescriptorCollection GetProperties(object component)
        {
            return this.GetProperties(component, null);
        }

        public abstract PropertyDescriptorCollection GetProperties(object component, Attribute[] attributes);
        public virtual PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object component, Attribute[] attributes)
        {
            return this.GetProperties(component, attributes);
        }

        public virtual System.Drawing.Bitmap Bitmap
        {
            get
            {
                if (!this.checkedBmp && (this.bitmap == null))
                {
                    string resource = base.GetType().Name + ".bmp";
                    try
                    {
                        this.bitmap = new System.Drawing.Bitmap(base.GetType(), resource);
                    }
                    catch (Exception)
                    {
                    }
                    this.checkedBmp = true;
                }
                return this.bitmap;
            }
        }

        public virtual object[] Components
        {
            get
            {
                return this.components;
            }
            set
            {
                this.components = value;
            }
        }

        public virtual string HelpKeyword
        {
            get
            {
                return this.TabName;
            }
        }

        public abstract string TabName { get; }
    }
}

