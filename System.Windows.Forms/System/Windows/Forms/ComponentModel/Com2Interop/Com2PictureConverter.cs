namespace System.Windows.Forms.ComponentModel.Com2Interop
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;

    internal class Com2PictureConverter : Com2DataTypeToManagedDataTypeConverter
    {
        private object lastManaged;
        private IntPtr lastNativeHandle;
        private IntPtr lastPalette = IntPtr.Zero;
        private WeakReference pictureRef;
        private System.Type pictureType = typeof(Bitmap);

        public Com2PictureConverter(Com2PropertyDescriptor pd)
        {
            if ((pd.DISPID == -522) || (pd.Name.IndexOf("Icon") != -1))
            {
                this.pictureType = typeof(Icon);
            }
        }

        public override object ConvertManagedToNative(object managedValue, Com2PropertyDescriptor pd, ref bool cancelSet)
        {
            cancelSet = false;
            if (((this.lastManaged != null) && this.lastManaged.Equals(managedValue)) && ((this.pictureRef != null) && this.pictureRef.IsAlive))
            {
                return this.pictureRef.Target;
            }
            this.lastManaged = managedValue;
            if (managedValue != null)
            {
                Guid gUID = typeof(System.Windows.Forms.UnsafeNativeMethods.IPicture).GUID;
                System.Windows.Forms.NativeMethods.PICTDESC pictdesc = null;
                bool fOwn = false;
                if (this.lastManaged is Icon)
                {
                    pictdesc = System.Windows.Forms.NativeMethods.PICTDESC.CreateIconPICTDESC(((Icon) this.lastManaged).Handle);
                }
                else if (this.lastManaged is Bitmap)
                {
                    pictdesc = System.Windows.Forms.NativeMethods.PICTDESC.CreateBitmapPICTDESC(((Bitmap) this.lastManaged).GetHbitmap(), this.lastPalette);
                    fOwn = true;
                }
                System.Windows.Forms.UnsafeNativeMethods.IPicture target = System.Windows.Forms.UnsafeNativeMethods.OleCreatePictureIndirect(pictdesc, ref gUID, fOwn);
                this.lastNativeHandle = target.GetHandle();
                this.pictureRef = new WeakReference(target);
                return target;
            }
            this.lastManaged = null;
            this.lastNativeHandle = this.lastPalette = IntPtr.Zero;
            this.pictureRef = null;
            return null;
        }

        public override object ConvertNativeToManaged(object nativeValue, Com2PropertyDescriptor pd)
        {
            if (nativeValue == null)
            {
                return null;
            }
            System.Windows.Forms.UnsafeNativeMethods.IPicture target = (System.Windows.Forms.UnsafeNativeMethods.IPicture) nativeValue;
            IntPtr handle = target.GetHandle();
            if ((this.lastManaged == null) || (handle != this.lastNativeHandle))
            {
                this.lastNativeHandle = handle;
                if (!(handle != IntPtr.Zero))
                {
                    this.lastManaged = null;
                    this.pictureRef = null;
                }
                else
                {
                    switch (target.GetPictureType())
                    {
                        case 1:
                            this.pictureType = typeof(Bitmap);
                            this.lastManaged = Image.FromHbitmap(handle);
                            break;

                        case 3:
                            this.pictureType = typeof(Icon);
                            this.lastManaged = Icon.FromHandle(handle);
                            break;
                    }
                    this.pictureRef = new WeakReference(target);
                }
            }
            return this.lastManaged;
        }

        public override System.Type ManagedType
        {
            get
            {
                return this.pictureType;
            }
        }
    }
}

