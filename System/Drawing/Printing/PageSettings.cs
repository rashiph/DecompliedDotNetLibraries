namespace System.Drawing.Printing
{
    using System;
    using System.Drawing;
    using System.Drawing.Internal;
    using System.Runtime.InteropServices;

    [Serializable]
    public class PageSettings : ICloneable
    {
        private TriState color;
        private TriState landscape;
        private System.Drawing.Printing.Margins margins;
        private System.Drawing.Printing.PaperSize paperSize;
        private System.Drawing.Printing.PaperSource paperSource;
        private System.Drawing.Printing.PrinterResolution printerResolution;
        internal System.Drawing.Printing.PrinterSettings printerSettings;

        public PageSettings() : this(new System.Drawing.Printing.PrinterSettings())
        {
        }

        public PageSettings(System.Drawing.Printing.PrinterSettings printerSettings)
        {
            this.color = TriState.Default;
            this.landscape = TriState.Default;
            this.margins = new System.Drawing.Printing.Margins();
            this.printerSettings = printerSettings;
        }

        public object Clone()
        {
            PageSettings settings = (PageSettings) base.MemberwiseClone();
            settings.margins = (System.Drawing.Printing.Margins) this.margins.Clone();
            return settings;
        }

        public void CopyToHdevmode(IntPtr hdevmode)
        {
            IntSecurity.AllPrintingAndUnmanagedCode.Demand();
            IntPtr lparam = SafeNativeMethods.GlobalLock(new HandleRef(null, hdevmode));
            SafeNativeMethods.DEVMODE structure = (SafeNativeMethods.DEVMODE) UnsafeNativeMethods.PtrToStructure(lparam, typeof(SafeNativeMethods.DEVMODE));
            if (this.color.IsNotDefault && ((structure.dmFields & 0x800) == 0x800))
            {
                structure.dmColor = ((bool) this.color) ? ((short) 2) : ((short) 1);
            }
            if (this.landscape.IsNotDefault && ((structure.dmFields & 1) == 1))
            {
                structure.dmOrientation = ((bool) this.landscape) ? ((short) 2) : ((short) 1);
            }
            if (this.paperSize != null)
            {
                if ((structure.dmFields & 2) == 2)
                {
                    structure.dmPaperSize = (short) this.paperSize.RawKind;
                }
                bool flag = false;
                bool flag2 = false;
                if ((structure.dmFields & 4) == 4)
                {
                    structure.dmPaperLength = (short) PrinterUnitConvert.Convert(this.paperSize.Height, PrinterUnit.Display, PrinterUnit.TenthsOfAMillimeter);
                    flag2 = true;
                }
                if ((structure.dmFields & 8) == 8)
                {
                    structure.dmPaperWidth = (short) PrinterUnitConvert.Convert(this.paperSize.Width, PrinterUnit.Display, PrinterUnit.TenthsOfAMillimeter);
                    flag = true;
                }
                if (this.paperSize.Kind == PaperKind.Custom)
                {
                    if (!flag2)
                    {
                        structure.dmFields |= 4;
                        structure.dmPaperLength = (short) PrinterUnitConvert.Convert(this.paperSize.Height, PrinterUnit.Display, PrinterUnit.TenthsOfAMillimeter);
                    }
                    if (!flag)
                    {
                        structure.dmFields |= 8;
                        structure.dmPaperWidth = (short) PrinterUnitConvert.Convert(this.paperSize.Width, PrinterUnit.Display, PrinterUnit.TenthsOfAMillimeter);
                    }
                }
            }
            if ((this.paperSource != null) && ((structure.dmFields & 0x200) == 0x200))
            {
                structure.dmDefaultSource = (short) this.paperSource.RawKind;
            }
            if (this.printerResolution != null)
            {
                if (this.printerResolution.Kind == PrinterResolutionKind.Custom)
                {
                    if ((structure.dmFields & 0x400) == 0x400)
                    {
                        structure.dmPrintQuality = (short) this.printerResolution.X;
                    }
                    if ((structure.dmFields & 0x2000) == 0x2000)
                    {
                        structure.dmYResolution = (short) this.printerResolution.Y;
                    }
                }
                else if ((structure.dmFields & 0x400) == 0x400)
                {
                    structure.dmPrintQuality = (short) this.printerResolution.Kind;
                }
            }
            Marshal.StructureToPtr(structure, lparam, false);
            if ((structure.dmDriverExtra >= this.ExtraBytes) && (SafeNativeMethods.DocumentProperties(System.Drawing.NativeMethods.NullHandleRef, System.Drawing.NativeMethods.NullHandleRef, this.printerSettings.PrinterName, lparam, lparam, 10) < 0))
            {
                SafeNativeMethods.GlobalFree(new HandleRef(null, lparam));
            }
            SafeNativeMethods.GlobalUnlock(new HandleRef(null, hdevmode));
        }

        internal Rectangle GetBounds(IntPtr modeHandle)
        {
            System.Drawing.Printing.PaperSize paperSize = this.GetPaperSize(modeHandle);
            if (this.GetLandscape(modeHandle))
            {
                return new Rectangle(0, 0, paperSize.Height, paperSize.Width);
            }
            return new Rectangle(0, 0, paperSize.Width, paperSize.Height);
        }

        private bool GetLandscape(IntPtr modeHandle)
        {
            if (this.landscape.IsDefault)
            {
                return (this.printerSettings.GetModeField(ModeField.Orientation, 1, modeHandle) == 2);
            }
            return (bool) this.landscape;
        }

        private System.Drawing.Printing.PaperSize GetPaperSize(IntPtr modeHandle)
        {
            if (this.paperSize != null)
            {
                return this.paperSize;
            }
            bool flag = false;
            if (modeHandle == IntPtr.Zero)
            {
                modeHandle = this.printerSettings.GetHdevmode();
                flag = true;
            }
            SafeNativeMethods.DEVMODE mode = (SafeNativeMethods.DEVMODE) UnsafeNativeMethods.PtrToStructure(SafeNativeMethods.GlobalLock(new HandleRef(null, modeHandle)), typeof(SafeNativeMethods.DEVMODE));
            System.Drawing.Printing.PaperSize size = this.PaperSizeFromMode(mode);
            SafeNativeMethods.GlobalUnlock(new HandleRef(null, modeHandle));
            if (flag)
            {
                SafeNativeMethods.GlobalFree(new HandleRef(null, modeHandle));
            }
            return size;
        }

        private System.Drawing.Printing.PaperSize PaperSizeFromMode(SafeNativeMethods.DEVMODE mode)
        {
            System.Drawing.Printing.PaperSize[] sizeArray = this.printerSettings.Get_PaperSizes();
            if ((mode.dmFields & 2) == 2)
            {
                for (int i = 0; i < sizeArray.Length; i++)
                {
                    if (sizeArray[i].RawKind == mode.dmPaperSize)
                    {
                        return sizeArray[i];
                    }
                }
            }
            return new System.Drawing.Printing.PaperSize(PaperKind.Custom, "custom", PrinterUnitConvert.Convert((int) mode.dmPaperWidth, PrinterUnit.TenthsOfAMillimeter, PrinterUnit.Display), PrinterUnitConvert.Convert((int) mode.dmPaperLength, PrinterUnit.TenthsOfAMillimeter, PrinterUnit.Display));
        }

        private System.Drawing.Printing.PaperSource PaperSourceFromMode(SafeNativeMethods.DEVMODE mode)
        {
            System.Drawing.Printing.PaperSource[] sourceArray = this.printerSettings.Get_PaperSources();
            if ((mode.dmFields & 0x200) == 0x200)
            {
                for (int i = 0; i < sourceArray.Length; i++)
                {
                    if (((short) sourceArray[i].RawKind) == mode.dmDefaultSource)
                    {
                        return sourceArray[i];
                    }
                }
            }
            return new System.Drawing.Printing.PaperSource((PaperSourceKind) mode.dmDefaultSource, "unknown");
        }

        private System.Drawing.Printing.PrinterResolution PrinterResolutionFromMode(SafeNativeMethods.DEVMODE mode)
        {
            System.Drawing.Printing.PrinterResolution[] resolutionArray = this.printerSettings.Get_PrinterResolutions();
            for (int i = 0; i < resolutionArray.Length; i++)
            {
                if (((mode.dmPrintQuality >= 0) && ((mode.dmFields & 0x400) == 0x400)) && ((mode.dmFields & 0x2000) == 0x2000))
                {
                    if ((resolutionArray[i].X == mode.dmPrintQuality) && (resolutionArray[i].Y == mode.dmYResolution))
                    {
                        return resolutionArray[i];
                    }
                }
                else if (((mode.dmFields & 0x400) == 0x400) && (resolutionArray[i].Kind == ((PrinterResolutionKind) mode.dmPrintQuality)))
                {
                    return resolutionArray[i];
                }
            }
            return new System.Drawing.Printing.PrinterResolution(PrinterResolutionKind.Custom, mode.dmPrintQuality, mode.dmYResolution);
        }

        public void SetHdevmode(IntPtr hdevmode)
        {
            IntSecurity.AllPrintingAndUnmanagedCode.Demand();
            if (hdevmode == IntPtr.Zero)
            {
                throw new ArgumentException(System.Drawing.SR.GetString("InvalidPrinterHandle", new object[] { hdevmode }));
            }
            SafeNativeMethods.DEVMODE mode = (SafeNativeMethods.DEVMODE) UnsafeNativeMethods.PtrToStructure(SafeNativeMethods.GlobalLock(new HandleRef(null, hdevmode)), typeof(SafeNativeMethods.DEVMODE));
            if ((mode.dmFields & 0x800) == 0x800)
            {
                this.color = mode.dmColor == 2;
            }
            if ((mode.dmFields & 1) == 1)
            {
                this.landscape = mode.dmOrientation == 2;
            }
            this.paperSize = this.PaperSizeFromMode(mode);
            this.paperSource = this.PaperSourceFromMode(mode);
            this.printerResolution = this.PrinterResolutionFromMode(mode);
            SafeNativeMethods.GlobalUnlock(new HandleRef(null, hdevmode));
        }

        public override string ToString()
        {
            return ("[PageSettings: Color=" + this.Color.ToString() + ", Landscape=" + this.Landscape.ToString() + ", Margins=" + this.Margins.ToString() + ", PaperSize=" + this.PaperSize.ToString() + ", PaperSource=" + this.PaperSource.ToString() + ", PrinterResolution=" + this.PrinterResolution.ToString() + "]");
        }

        public Rectangle Bounds
        {
            get
            {
                IntSecurity.AllPrintingAndUnmanagedCode.Assert();
                IntPtr hdevmode = this.printerSettings.GetHdevmode();
                Rectangle bounds = this.GetBounds(hdevmode);
                SafeNativeMethods.GlobalFree(new HandleRef(this, hdevmode));
                return bounds;
            }
        }

        public bool Color
        {
            get
            {
                if (this.color.IsDefault)
                {
                    return (this.printerSettings.GetModeField(ModeField.Color, 1) == 2);
                }
                return (bool) this.color;
            }
            set
            {
                this.color = value;
            }
        }

        private short ExtraBytes
        {
            get
            {
                IntPtr hdevmodeInternal = this.printerSettings.GetHdevmodeInternal();
                SafeNativeMethods.DEVMODE devmode = (SafeNativeMethods.DEVMODE) UnsafeNativeMethods.PtrToStructure(SafeNativeMethods.GlobalLock(new HandleRef(this, hdevmodeInternal)), typeof(SafeNativeMethods.DEVMODE));
                short dmDriverExtra = devmode.dmDriverExtra;
                SafeNativeMethods.GlobalUnlock(new HandleRef(this, hdevmodeInternal));
                SafeNativeMethods.GlobalFree(new HandleRef(this, hdevmodeInternal));
                return dmDriverExtra;
            }
        }

        public float HardMarginX
        {
            get
            {
                IntSecurity.AllPrintingAndUnmanagedCode.Assert();
                using (DeviceContext context = this.printerSettings.CreateDeviceContext(this))
                {
                    int deviceCaps = UnsafeNativeMethods.GetDeviceCaps(new HandleRef(context, context.Hdc), 0x58);
                    return ((UnsafeNativeMethods.GetDeviceCaps(new HandleRef(context, context.Hdc), 0x70) * 100) / deviceCaps);
                }
            }
        }

        public float HardMarginY
        {
            get
            {
                using (DeviceContext context = this.printerSettings.CreateDeviceContext(this))
                {
                    int deviceCaps = UnsafeNativeMethods.GetDeviceCaps(new HandleRef(context, context.Hdc), 90);
                    return ((UnsafeNativeMethods.GetDeviceCaps(new HandleRef(context, context.Hdc), 0x71) * 100) / deviceCaps);
                }
            }
        }

        public bool Landscape
        {
            get
            {
                if (this.landscape.IsDefault)
                {
                    return (this.printerSettings.GetModeField(ModeField.Orientation, 1) == 2);
                }
                return (bool) this.landscape;
            }
            set
            {
                this.landscape = value;
            }
        }

        public System.Drawing.Printing.Margins Margins
        {
            get
            {
                return this.margins;
            }
            set
            {
                this.margins = value;
            }
        }

        public System.Drawing.Printing.PaperSize PaperSize
        {
            get
            {
                IntSecurity.AllPrintingAndUnmanagedCode.Assert();
                return this.GetPaperSize(IntPtr.Zero);
            }
            set
            {
                this.paperSize = value;
            }
        }

        public System.Drawing.Printing.PaperSource PaperSource
        {
            get
            {
                if (this.paperSource == null)
                {
                    IntSecurity.AllPrintingAndUnmanagedCode.Assert();
                    IntPtr hdevmode = this.printerSettings.GetHdevmode();
                    SafeNativeMethods.DEVMODE mode = (SafeNativeMethods.DEVMODE) UnsafeNativeMethods.PtrToStructure(SafeNativeMethods.GlobalLock(new HandleRef(this, hdevmode)), typeof(SafeNativeMethods.DEVMODE));
                    System.Drawing.Printing.PaperSource source = this.PaperSourceFromMode(mode);
                    SafeNativeMethods.GlobalUnlock(new HandleRef(this, hdevmode));
                    SafeNativeMethods.GlobalFree(new HandleRef(this, hdevmode));
                    return source;
                }
                return this.paperSource;
            }
            set
            {
                this.paperSource = value;
            }
        }

        public RectangleF PrintableArea
        {
            get
            {
                RectangleF ef = new RectangleF();
                DeviceContext wrapper = this.printerSettings.CreateInformationContext(this);
                HandleRef hDC = new HandleRef(wrapper, wrapper.Hdc);
                try
                {
                    int deviceCaps = UnsafeNativeMethods.GetDeviceCaps(hDC, 0x58);
                    int num2 = UnsafeNativeMethods.GetDeviceCaps(hDC, 90);
                    if (!this.Landscape)
                    {
                        ef.X = (UnsafeNativeMethods.GetDeviceCaps(hDC, 0x70) * 100f) / ((float) deviceCaps);
                        ef.Y = (UnsafeNativeMethods.GetDeviceCaps(hDC, 0x71) * 100f) / ((float) num2);
                        ef.Width = (UnsafeNativeMethods.GetDeviceCaps(hDC, 8) * 100f) / ((float) deviceCaps);
                        ef.Height = (UnsafeNativeMethods.GetDeviceCaps(hDC, 10) * 100f) / ((float) num2);
                        return ef;
                    }
                    ef.Y = (UnsafeNativeMethods.GetDeviceCaps(hDC, 0x70) * 100f) / ((float) deviceCaps);
                    ef.X = (UnsafeNativeMethods.GetDeviceCaps(hDC, 0x71) * 100f) / ((float) num2);
                    ef.Height = (UnsafeNativeMethods.GetDeviceCaps(hDC, 8) * 100f) / ((float) deviceCaps);
                    ef.Width = (UnsafeNativeMethods.GetDeviceCaps(hDC, 10) * 100f) / ((float) num2);
                }
                finally
                {
                    wrapper.Dispose();
                }
                return ef;
            }
        }

        public System.Drawing.Printing.PrinterResolution PrinterResolution
        {
            get
            {
                if (this.printerResolution == null)
                {
                    IntSecurity.AllPrintingAndUnmanagedCode.Assert();
                    IntPtr hdevmode = this.printerSettings.GetHdevmode();
                    SafeNativeMethods.DEVMODE mode = (SafeNativeMethods.DEVMODE) UnsafeNativeMethods.PtrToStructure(SafeNativeMethods.GlobalLock(new HandleRef(this, hdevmode)), typeof(SafeNativeMethods.DEVMODE));
                    System.Drawing.Printing.PrinterResolution resolution = this.PrinterResolutionFromMode(mode);
                    SafeNativeMethods.GlobalUnlock(new HandleRef(this, hdevmode));
                    SafeNativeMethods.GlobalFree(new HandleRef(this, hdevmode));
                    return resolution;
                }
                return this.printerResolution;
            }
            set
            {
                this.printerResolution = value;
            }
        }

        public System.Drawing.Printing.PrinterSettings PrinterSettings
        {
            get
            {
                return this.printerSettings;
            }
            set
            {
                if (value == null)
                {
                    value = new System.Drawing.Printing.PrinterSettings();
                }
                this.printerSettings = value;
            }
        }
    }
}

