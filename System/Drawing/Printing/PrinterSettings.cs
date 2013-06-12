namespace System.Drawing.Printing
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Drawing.Internal;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Text;

    [Serializable]
    public class PrinterSettings : ICloneable
    {
        private byte[] cachedDevmode;
        private TriState collate = TriState.Default;
        private short copies = -1;
        private PageSettings defaultPageSettings;
        private short devmodebytes;
        private string driverName = "";
        private System.Drawing.Printing.Duplex duplex = System.Drawing.Printing.Duplex.Default;
        private short extrabytes;
        private byte[] extrainfo;
        private int fromPage;
        private int maxPage = 0x270f;
        private int minPage;
        private string outputPort = "";
        private const int PADDING_IA64 = 4;
        private bool printDialogDisplayed;
        private string printerName;
        private System.Drawing.Printing.PrintRange printRange;
        private bool printToFile;
        private int toPage;

        public PrinterSettings()
        {
            this.defaultPageSettings = new PageSettings(this);
        }

        public object Clone()
        {
            PrinterSettings settings = (PrinterSettings) base.MemberwiseClone();
            settings.printDialogDisplayed = false;
            return settings;
        }

        internal DeviceContext CreateDeviceContext(PageSettings pageSettings)
        {
            IntPtr hdevmodeInternal = this.GetHdevmodeInternal();
            DeviceContext context = null;
            try
            {
                System.Drawing.IntSecurity.AllPrintingAndUnmanagedCode.Assert();
                try
                {
                    pageSettings.CopyToHdevmode(hdevmodeInternal);
                }
                finally
                {
                    CodeAccessPermission.RevertAssert();
                }
                context = this.CreateDeviceContext(hdevmodeInternal);
            }
            finally
            {
                SafeNativeMethods.GlobalFree(new HandleRef(null, hdevmodeInternal));
            }
            return context;
        }

        internal DeviceContext CreateDeviceContext(IntPtr hdevmode)
        {
            IntPtr handle = SafeNativeMethods.GlobalLock(new HandleRef(null, hdevmode));
            DeviceContext context = DeviceContext.CreateDC(this.DriverName, this.PrinterNameInternal, null, new HandleRef(null, handle));
            SafeNativeMethods.GlobalUnlock(new HandleRef(null, hdevmode));
            return context;
        }

        internal DeviceContext CreateInformationContext(PageSettings pageSettings)
        {
            DeviceContext context;
            IntPtr hdevmodeInternal = this.GetHdevmodeInternal();
            try
            {
                System.Drawing.IntSecurity.AllPrintingAndUnmanagedCode.Assert();
                try
                {
                    pageSettings.CopyToHdevmode(hdevmodeInternal);
                }
                finally
                {
                    CodeAccessPermission.RevertAssert();
                }
                context = this.CreateInformationContext(hdevmodeInternal);
            }
            finally
            {
                SafeNativeMethods.GlobalFree(new HandleRef(null, hdevmodeInternal));
            }
            return context;
        }

        internal DeviceContext CreateInformationContext(IntPtr hdevmode)
        {
            IntPtr handle = SafeNativeMethods.GlobalLock(new HandleRef(null, hdevmode));
            DeviceContext context = DeviceContext.CreateIC(this.DriverName, this.PrinterNameInternal, null, new HandleRef(null, handle));
            SafeNativeMethods.GlobalUnlock(new HandleRef(null, hdevmode));
            return context;
        }

        public Graphics CreateMeasurementGraphics()
        {
            return this.CreateMeasurementGraphics(this.DefaultPageSettings);
        }

        public Graphics CreateMeasurementGraphics(bool honorOriginAtMargins)
        {
            Graphics graphics = this.CreateMeasurementGraphics();
            if ((graphics != null) && honorOriginAtMargins)
            {
                System.Drawing.IntSecurity.AllPrintingAndUnmanagedCode.Assert();
                try
                {
                    graphics.TranslateTransform(-this.defaultPageSettings.HardMarginX, -this.defaultPageSettings.HardMarginY);
                }
                finally
                {
                    CodeAccessPermission.RevertAssert();
                }
                graphics.TranslateTransform((float) this.defaultPageSettings.Margins.Left, (float) this.defaultPageSettings.Margins.Top);
            }
            return graphics;
        }

        public Graphics CreateMeasurementGraphics(PageSettings pageSettings)
        {
            DeviceContext context = this.CreateDeviceContext(pageSettings);
            Graphics graphics = Graphics.FromHdcInternal(context.Hdc);
            graphics.PrintingHelper = context;
            return graphics;
        }

        public Graphics CreateMeasurementGraphics(PageSettings pageSettings, bool honorOriginAtMargins)
        {
            Graphics graphics = this.CreateMeasurementGraphics();
            if ((graphics != null) && honorOriginAtMargins)
            {
                System.Drawing.IntSecurity.AllPrintingAndUnmanagedCode.Assert();
                try
                {
                    graphics.TranslateTransform(-pageSettings.HardMarginX, -pageSettings.HardMarginY);
                }
                finally
                {
                    CodeAccessPermission.RevertAssert();
                }
                graphics.TranslateTransform((float) pageSettings.Margins.Left, (float) pageSettings.Margins.Top);
            }
            return graphics;
        }

        private static SafeNativeMethods.PRINTDLG CreatePRINTDLG()
        {
            return new SafeNativeMethods.PRINTDLG { 
                lStructSize = Marshal.SizeOf(typeof(SafeNativeMethods.PRINTDLG)), hwndOwner = IntPtr.Zero, hDevMode = IntPtr.Zero, hDevNames = IntPtr.Zero, Flags = 0, hwndOwner = IntPtr.Zero, hDC = IntPtr.Zero, nFromPage = 1, nToPage = 1, nMinPage = 0, nMaxPage = 0x270f, nCopies = 1, hInstance = IntPtr.Zero, lCustData = IntPtr.Zero, lpfnPrintHook = IntPtr.Zero, lpfnSetupHook = IntPtr.Zero, 
                lpPrintTemplateName = null, lpSetupTemplateName = null, hPrintTemplate = IntPtr.Zero, hSetupTemplate = IntPtr.Zero
             };
        }

        private static SafeNativeMethods.PRINTDLGX86 CreatePRINTDLGX86()
        {
            return new SafeNativeMethods.PRINTDLGX86 { 
                lStructSize = Marshal.SizeOf(typeof(SafeNativeMethods.PRINTDLGX86)), hwndOwner = IntPtr.Zero, hDevMode = IntPtr.Zero, hDevNames = IntPtr.Zero, Flags = 0, hwndOwner = IntPtr.Zero, hDC = IntPtr.Zero, nFromPage = 1, nToPage = 1, nMinPage = 0, nMaxPage = 0x270f, nCopies = 1, hInstance = IntPtr.Zero, lCustData = IntPtr.Zero, lpfnPrintHook = IntPtr.Zero, lpfnSetupHook = IntPtr.Zero, 
                lpPrintTemplateName = null, lpSetupTemplateName = null, hPrintTemplate = IntPtr.Zero, hSetupTemplate = IntPtr.Zero
             };
        }

        private int DeviceCapabilities(short capability, IntPtr pointerToBuffer, int defaultValue)
        {
            System.Drawing.IntSecurity.AllPrinting.Assert();
            string printerName = this.PrinterName;
            CodeAccessPermission.RevertAssert();
            System.Drawing.IntSecurity.UnmanagedCode.Assert();
            return FastDeviceCapabilities(capability, pointerToBuffer, defaultValue, printerName);
        }

        private static int FastDeviceCapabilities(short capability, IntPtr pointerToBuffer, int defaultValue, string printerName)
        {
            int num = SafeNativeMethods.DeviceCapabilities(printerName, GetOutputPort(), capability, pointerToBuffer, IntPtr.Zero);
            if (num == -1)
            {
                return defaultValue;
            }
            return num;
        }

        internal PaperSize[] Get_PaperSizes()
        {
            System.Drawing.IntSecurity.AllPrintingAndUnmanagedCode.Assert();
            string printerName = this.PrinterName;
            int num = FastDeviceCapabilities(0x10, IntPtr.Zero, -1, printerName);
            if (num == -1)
            {
                return new PaperSize[0];
            }
            int num2 = Marshal.SystemDefaultCharSize * 0x40;
            IntPtr pointerToBuffer = Marshal.AllocCoTaskMem(num2 * num);
            FastDeviceCapabilities(0x10, pointerToBuffer, -1, printerName);
            IntPtr ptr2 = Marshal.AllocCoTaskMem(2 * num);
            FastDeviceCapabilities(2, ptr2, -1, printerName);
            IntPtr ptr3 = Marshal.AllocCoTaskMem(8 * num);
            FastDeviceCapabilities(3, ptr3, -1, printerName);
            PaperSize[] sizeArray = new PaperSize[num];
            for (int i = 0; i < num; i++)
            {
                string name = Marshal.PtrToStringAuto((IntPtr) (((long) pointerToBuffer) + (num2 * i)), 0x40);
                int index = name.IndexOf('\0');
                if (index > -1)
                {
                    name = name.Substring(0, index);
                }
                short num5 = Marshal.ReadInt16((IntPtr) (((long) ptr2) + (i * 2)));
                int num6 = Marshal.ReadInt32((IntPtr) (((long) ptr3) + (i * 8)));
                int num7 = Marshal.ReadInt32((IntPtr) ((((long) ptr3) + (i * 8)) + 4L));
                sizeArray[i] = new PaperSize((PaperKind) num5, name, PrinterUnitConvert.Convert(num6, PrinterUnit.TenthsOfAMillimeter, PrinterUnit.Display), PrinterUnitConvert.Convert(num7, PrinterUnit.TenthsOfAMillimeter, PrinterUnit.Display));
            }
            Marshal.FreeCoTaskMem(pointerToBuffer);
            Marshal.FreeCoTaskMem(ptr2);
            Marshal.FreeCoTaskMem(ptr3);
            return sizeArray;
        }

        internal PaperSource[] Get_PaperSources()
        {
            System.Drawing.IntSecurity.AllPrintingAndUnmanagedCode.Assert();
            string printerName = this.PrinterName;
            int num = FastDeviceCapabilities(12, IntPtr.Zero, -1, printerName);
            if (num == -1)
            {
                return new PaperSource[0];
            }
            int num2 = Marshal.SystemDefaultCharSize * 0x18;
            IntPtr pointerToBuffer = Marshal.AllocCoTaskMem(num2 * num);
            FastDeviceCapabilities(12, pointerToBuffer, -1, printerName);
            IntPtr ptr2 = Marshal.AllocCoTaskMem(2 * num);
            FastDeviceCapabilities(6, ptr2, -1, printerName);
            PaperSource[] sourceArray = new PaperSource[num];
            for (int i = 0; i < num; i++)
            {
                string name = Marshal.PtrToStringAuto((IntPtr) (((long) pointerToBuffer) + (num2 * i)));
                short num4 = Marshal.ReadInt16((IntPtr) (((long) ptr2) + (2 * i)));
                sourceArray[i] = new PaperSource((PaperSourceKind) num4, name);
            }
            Marshal.FreeCoTaskMem(pointerToBuffer);
            Marshal.FreeCoTaskMem(ptr2);
            return sourceArray;
        }

        internal PrinterResolution[] Get_PrinterResolutions()
        {
            System.Drawing.IntSecurity.AllPrintingAndUnmanagedCode.Assert();
            string printerName = this.PrinterName;
            int num = FastDeviceCapabilities(13, IntPtr.Zero, -1, printerName);
            if (num == -1)
            {
                return new PrinterResolution[] { new PrinterResolution(PrinterResolutionKind.High, -4, -1), new PrinterResolution(PrinterResolutionKind.Medium, -3, -1), new PrinterResolution(PrinterResolutionKind.Low, -2, -1), new PrinterResolution(PrinterResolutionKind.Draft, -1, -1) };
            }
            PrinterResolution[] resolutionArray = new PrinterResolution[num + 4];
            resolutionArray[0] = new PrinterResolution(PrinterResolutionKind.High, -4, -1);
            resolutionArray[1] = new PrinterResolution(PrinterResolutionKind.Medium, -3, -1);
            resolutionArray[2] = new PrinterResolution(PrinterResolutionKind.Low, -2, -1);
            resolutionArray[3] = new PrinterResolution(PrinterResolutionKind.Draft, -1, -1);
            IntPtr pointerToBuffer = Marshal.AllocCoTaskMem(8 * num);
            FastDeviceCapabilities(13, pointerToBuffer, -1, printerName);
            for (int i = 0; i < num; i++)
            {
                int x = Marshal.ReadInt32((IntPtr) (((long) pointerToBuffer) + (i * 8)));
                int y = Marshal.ReadInt32((IntPtr) ((((long) pointerToBuffer) + (i * 8)) + 4L));
                resolutionArray[i + 4] = new PrinterResolution(PrinterResolutionKind.Custom, x, y);
            }
            Marshal.FreeCoTaskMem(pointerToBuffer);
            return resolutionArray;
        }

        private static string GetDefaultPrinterName()
        {
            System.Drawing.IntSecurity.UnmanagedCode.Assert();
            if (IntPtr.Size == 8)
            {
                SafeNativeMethods.PRINTDLG printdlg = CreatePRINTDLG();
                printdlg.Flags = 0x400;
                if (!SafeNativeMethods.PrintDlg(printdlg))
                {
                    return System.Drawing.SR.GetString("NoDefaultPrinter");
                }
                IntPtr handle = printdlg.hDevNames;
                IntPtr zero = SafeNativeMethods.GlobalLock(new HandleRef(printdlg, handle));
                if (zero == IntPtr.Zero)
                {
                    throw new Win32Exception();
                }
                string str = ReadOneDEVNAME(zero, 1);
                SafeNativeMethods.GlobalUnlock(new HandleRef(printdlg, handle));
                zero = IntPtr.Zero;
                SafeNativeMethods.GlobalFree(new HandleRef(printdlg, printdlg.hDevNames));
                SafeNativeMethods.GlobalFree(new HandleRef(printdlg, printdlg.hDevMode));
                return str;
            }
            SafeNativeMethods.PRINTDLGX86 lppd = CreatePRINTDLGX86();
            lppd.Flags = 0x400;
            if (!SafeNativeMethods.PrintDlg(lppd))
            {
                return System.Drawing.SR.GetString("NoDefaultPrinter");
            }
            IntPtr hDevNames = lppd.hDevNames;
            IntPtr pDevnames = SafeNativeMethods.GlobalLock(new HandleRef(lppd, hDevNames));
            if (pDevnames == IntPtr.Zero)
            {
                throw new Win32Exception();
            }
            string str2 = ReadOneDEVNAME(pDevnames, 1);
            SafeNativeMethods.GlobalUnlock(new HandleRef(lppd, hDevNames));
            pDevnames = IntPtr.Zero;
            SafeNativeMethods.GlobalFree(new HandleRef(lppd, lppd.hDevNames));
            SafeNativeMethods.GlobalFree(new HandleRef(lppd, lppd.hDevMode));
            return str2;
        }

        private int GetDeviceCaps(int capability, int defaultValue)
        {
            DeviceContext wrapper = this.CreateInformationContext(this.DefaultPageSettings);
            int deviceCaps = defaultValue;
            try
            {
                deviceCaps = UnsafeNativeMethods.GetDeviceCaps(new HandleRef(wrapper, wrapper.Hdc), capability);
            }
            catch (InvalidPrinterException)
            {
            }
            finally
            {
                wrapper.Dispose();
            }
            return deviceCaps;
        }

        public IntPtr GetHdevmode()
        {
            System.Drawing.IntSecurity.AllPrintingAndUnmanagedCode.Demand();
            IntPtr hdevmodeInternal = this.GetHdevmodeInternal();
            this.defaultPageSettings.CopyToHdevmode(hdevmodeInternal);
            return hdevmodeInternal;
        }

        public IntPtr GetHdevmode(PageSettings pageSettings)
        {
            System.Drawing.IntSecurity.AllPrintingAndUnmanagedCode.Demand();
            IntPtr hdevmodeInternal = this.GetHdevmodeInternal();
            pageSettings.CopyToHdevmode(hdevmodeInternal);
            return hdevmodeInternal;
        }

        internal IntPtr GetHdevmodeInternal()
        {
            return this.GetHdevmodeInternal(this.PrinterNameInternal);
        }

        private IntPtr GetHdevmodeInternal(string printer)
        {
            int num = SafeNativeMethods.DocumentProperties(System.Drawing.NativeMethods.NullHandleRef, System.Drawing.NativeMethods.NullHandleRef, printer, IntPtr.Zero, System.Drawing.NativeMethods.NullHandleRef, 0);
            if (num < 1)
            {
                throw new InvalidPrinterException(this);
            }
            IntPtr handle = SafeNativeMethods.GlobalAlloc(2, (uint) num);
            IntPtr destination = SafeNativeMethods.GlobalLock(new HandleRef(null, handle));
            if (this.cachedDevmode != null)
            {
                Marshal.Copy(this.cachedDevmode, 0, destination, this.devmodebytes);
            }
            else if (SafeNativeMethods.DocumentProperties(System.Drawing.NativeMethods.NullHandleRef, System.Drawing.NativeMethods.NullHandleRef, printer, destination, System.Drawing.NativeMethods.NullHandleRef, 2) < 0)
            {
                throw new Win32Exception();
            }
            SafeNativeMethods.DEVMODE structure = (SafeNativeMethods.DEVMODE) UnsafeNativeMethods.PtrToStructure(destination, typeof(SafeNativeMethods.DEVMODE));
            if ((this.extrainfo != null) && (this.extrabytes <= structure.dmDriverExtra))
            {
                IntPtr ptr3 = (IntPtr) (((long) destination) + structure.dmSize);
                Marshal.Copy(this.extrainfo, 0, ptr3, this.extrabytes);
            }
            if (((structure.dmFields & 0x100) == 0x100) && (this.copies != -1))
            {
                structure.dmCopies = this.copies;
            }
            if (((structure.dmFields & 0x1000) == 0x1000) && (this.duplex != System.Drawing.Printing.Duplex.Default))
            {
                structure.dmDuplex = (short) this.duplex;
            }
            if (((structure.dmFields & 0x8000) == 0x8000) && this.collate.IsNotDefault)
            {
                structure.dmCollate = ((bool) this.collate) ? ((short) 1) : ((short) 0);
            }
            Marshal.StructureToPtr(structure, destination, false);
            if (SafeNativeMethods.DocumentProperties(System.Drawing.NativeMethods.NullHandleRef, System.Drawing.NativeMethods.NullHandleRef, printer, destination, destination, 10) < 0)
            {
                SafeNativeMethods.GlobalFree(new HandleRef(null, handle));
                SafeNativeMethods.GlobalUnlock(new HandleRef(null, handle));
                return IntPtr.Zero;
            }
            SafeNativeMethods.GlobalUnlock(new HandleRef(null, handle));
            return handle;
        }

        public IntPtr GetHdevnames()
        {
            System.Drawing.IntSecurity.AllPrintingAndUnmanagedCode.Demand();
            string printerName = this.PrinterName;
            string driverName = this.DriverName;
            string outputPort = this.OutputPort;
            int num = ((4 + printerName.Length) + driverName.Length) + outputPort.Length;
            short val = (short) (8 / Marshal.SystemDefaultCharSize);
            uint dwBytes = (uint) (Marshal.SystemDefaultCharSize * (val + num));
            IntPtr handle = SafeNativeMethods.GlobalAlloc(0x42, dwBytes);
            IntPtr ptr = SafeNativeMethods.GlobalLock(new HandleRef(null, handle));
            Marshal.WriteInt16(ptr, val);
            val = (short) (val + this.WriteOneDEVNAME(driverName, ptr, val));
            Marshal.WriteInt16((IntPtr) (((long) ptr) + 2L), val);
            val = (short) (val + this.WriteOneDEVNAME(printerName, ptr, val));
            Marshal.WriteInt16((IntPtr) (((long) ptr) + 4L), val);
            val = (short) (val + this.WriteOneDEVNAME(outputPort, ptr, val));
            Marshal.WriteInt16((IntPtr) (((long) ptr) + 6L), val);
            SafeNativeMethods.GlobalUnlock(new HandleRef(null, handle));
            return handle;
        }

        internal short GetModeField(ModeField field, short defaultValue)
        {
            return this.GetModeField(field, defaultValue, IntPtr.Zero);
        }

        internal short GetModeField(ModeField field, short defaultValue, IntPtr modeHandle)
        {
            short dmOrientation;
            bool flag = false;
            try
            {
                if (modeHandle == IntPtr.Zero)
                {
                    try
                    {
                        modeHandle = this.GetHdevmodeInternal();
                        flag = true;
                    }
                    catch (InvalidPrinterException)
                    {
                        return defaultValue;
                    }
                }
                SafeNativeMethods.DEVMODE devmode = (SafeNativeMethods.DEVMODE) UnsafeNativeMethods.PtrToStructure(SafeNativeMethods.GlobalLock(new HandleRef(this, modeHandle)), typeof(SafeNativeMethods.DEVMODE));
                switch (field)
                {
                    case ModeField.Orientation:
                        dmOrientation = devmode.dmOrientation;
                        break;

                    case ModeField.PaperSize:
                        dmOrientation = devmode.dmPaperSize;
                        break;

                    case ModeField.PaperLength:
                        dmOrientation = devmode.dmPaperLength;
                        break;

                    case ModeField.PaperWidth:
                        dmOrientation = devmode.dmPaperWidth;
                        break;

                    case ModeField.Copies:
                        dmOrientation = devmode.dmCopies;
                        break;

                    case ModeField.DefaultSource:
                        dmOrientation = devmode.dmDefaultSource;
                        break;

                    case ModeField.PrintQuality:
                        dmOrientation = devmode.dmPrintQuality;
                        break;

                    case ModeField.Color:
                        dmOrientation = devmode.dmColor;
                        break;

                    case ModeField.Duplex:
                        dmOrientation = devmode.dmDuplex;
                        break;

                    case ModeField.YResolution:
                        dmOrientation = devmode.dmYResolution;
                        break;

                    case ModeField.TTOption:
                        dmOrientation = devmode.dmTTOption;
                        break;

                    case ModeField.Collate:
                        dmOrientation = devmode.dmCollate;
                        break;

                    default:
                        dmOrientation = defaultValue;
                        break;
                }
                SafeNativeMethods.GlobalUnlock(new HandleRef(this, modeHandle));
            }
            finally
            {
                if (flag)
                {
                    SafeNativeMethods.GlobalFree(new HandleRef(this, modeHandle));
                }
            }
            return dmOrientation;
        }

        private static string GetOutputPort()
        {
            System.Drawing.IntSecurity.UnmanagedCode.Assert();
            if (IntPtr.Size == 8)
            {
                SafeNativeMethods.PRINTDLG printdlg = CreatePRINTDLG();
                printdlg.Flags = 0x400;
                if (!SafeNativeMethods.PrintDlg(printdlg))
                {
                    return System.Drawing.SR.GetString("NoDefaultPrinter");
                }
                IntPtr handle = printdlg.hDevNames;
                IntPtr zero = SafeNativeMethods.GlobalLock(new HandleRef(printdlg, handle));
                if (zero == IntPtr.Zero)
                {
                    throw new Win32Exception();
                }
                string str = ReadOneDEVNAME(zero, 2);
                SafeNativeMethods.GlobalUnlock(new HandleRef(printdlg, handle));
                zero = IntPtr.Zero;
                SafeNativeMethods.GlobalFree(new HandleRef(printdlg, printdlg.hDevNames));
                SafeNativeMethods.GlobalFree(new HandleRef(printdlg, printdlg.hDevMode));
                return str;
            }
            SafeNativeMethods.PRINTDLGX86 lppd = CreatePRINTDLGX86();
            lppd.Flags = 0x400;
            if (!SafeNativeMethods.PrintDlg(lppd))
            {
                return System.Drawing.SR.GetString("NoDefaultPrinter");
            }
            IntPtr hDevNames = lppd.hDevNames;
            IntPtr pDevnames = SafeNativeMethods.GlobalLock(new HandleRef(lppd, hDevNames));
            if (pDevnames == IntPtr.Zero)
            {
                throw new Win32Exception();
            }
            string str2 = ReadOneDEVNAME(pDevnames, 2);
            SafeNativeMethods.GlobalUnlock(new HandleRef(lppd, hDevNames));
            pDevnames = IntPtr.Zero;
            SafeNativeMethods.GlobalFree(new HandleRef(lppd, lppd.hDevNames));
            SafeNativeMethods.GlobalFree(new HandleRef(lppd, lppd.hDevMode));
            return str2;
        }

        public bool IsDirectPrintingSupported(Image image)
        {
            bool flag = false;
            if (image.RawFormat.Equals(ImageFormat.Jpeg) || image.RawFormat.Equals(ImageFormat.Png))
            {
                MemoryStream stream = new MemoryStream();
                try
                {
                    image.Save(stream, image.RawFormat);
                    stream.Position = 0L;
                    using (BufferedStream stream2 = new BufferedStream(stream))
                    {
                        int length = (int) stream2.Length;
                        byte[] buffer = new byte[length];
                        stream2.Read(buffer, 0, length);
                        int inData = image.RawFormat.Equals(ImageFormat.Jpeg) ? 0x1017 : 0x1018;
                        int outData = 0;
                        DeviceContext wrapper = this.CreateInformationContext(this.DefaultPageSettings);
                        HandleRef hDC = new HandleRef(wrapper, wrapper.Hdc);
                        try
                        {
                            if (SafeNativeMethods.ExtEscape(hDC, 8, Marshal.SizeOf(typeof(int)), ref inData, 0, out outData) > 0)
                            {
                                flag = (SafeNativeMethods.ExtEscape(hDC, inData, length, buffer, Marshal.SizeOf(typeof(int)), out outData) > 0) && (outData == 1);
                            }
                        }
                        finally
                        {
                            wrapper.Dispose();
                        }
                        return flag;
                    }
                }
                finally
                {
                    stream.Close();
                }
            }
            return flag;
        }

        public bool IsDirectPrintingSupported(ImageFormat imageFormat)
        {
            bool flag = false;
            if (imageFormat.Equals(ImageFormat.Jpeg) || imageFormat.Equals(ImageFormat.Png))
            {
                int inData = imageFormat.Equals(ImageFormat.Jpeg) ? 0x1017 : 0x1018;
                int outData = 0;
                DeviceContext wrapper = this.CreateInformationContext(this.DefaultPageSettings);
                HandleRef hDC = new HandleRef(wrapper, wrapper.Hdc);
                try
                {
                    flag = SafeNativeMethods.ExtEscape(hDC, 8, Marshal.SizeOf(typeof(int)), ref inData, 0, out outData) > 0;
                }
                finally
                {
                    wrapper.Dispose();
                }
            }
            return flag;
        }

        private static string ReadOneDEVNAME(IntPtr pDevnames, int slot)
        {
            int num = Marshal.SystemDefaultCharSize * Marshal.ReadInt16((IntPtr) (((long) pDevnames) + (slot * 2)));
            return Marshal.PtrToStringAuto((IntPtr) (((long) pDevnames) + num));
        }

        public void SetHdevmode(IntPtr hdevmode)
        {
            System.Drawing.IntSecurity.AllPrintingAndUnmanagedCode.Demand();
            if (hdevmode == IntPtr.Zero)
            {
                throw new ArgumentException(System.Drawing.SR.GetString("InvalidPrinterHandle", new object[] { hdevmode }));
            }
            IntPtr lparam = SafeNativeMethods.GlobalLock(new HandleRef(null, hdevmode));
            SafeNativeMethods.DEVMODE devmode = (SafeNativeMethods.DEVMODE) UnsafeNativeMethods.PtrToStructure(lparam, typeof(SafeNativeMethods.DEVMODE));
            this.devmodebytes = devmode.dmSize;
            if (this.devmodebytes > 0)
            {
                this.cachedDevmode = new byte[this.devmodebytes];
                Marshal.Copy(lparam, this.cachedDevmode, 0, this.devmodebytes);
            }
            this.extrabytes = devmode.dmDriverExtra;
            if (this.extrabytes > 0)
            {
                this.extrainfo = new byte[this.extrabytes];
                Marshal.Copy((IntPtr) (((long) lparam) + devmode.dmSize), this.extrainfo, 0, this.extrabytes);
            }
            if ((devmode.dmFields & 0x100) == 0x100)
            {
                this.copies = devmode.dmCopies;
            }
            if ((devmode.dmFields & 0x1000) == 0x1000)
            {
                this.duplex = (System.Drawing.Printing.Duplex) devmode.dmDuplex;
            }
            if ((devmode.dmFields & 0x8000) == 0x8000)
            {
                this.collate = devmode.dmCollate == 1;
            }
            SafeNativeMethods.GlobalUnlock(new HandleRef(null, hdevmode));
        }

        public void SetHdevnames(IntPtr hdevnames)
        {
            System.Drawing.IntSecurity.AllPrintingAndUnmanagedCode.Demand();
            if (hdevnames == IntPtr.Zero)
            {
                throw new ArgumentException(System.Drawing.SR.GetString("InvalidPrinterHandle", new object[] { hdevnames }));
            }
            IntPtr pDevnames = SafeNativeMethods.GlobalLock(new HandleRef(null, hdevnames));
            this.driverName = ReadOneDEVNAME(pDevnames, 0);
            this.printerName = ReadOneDEVNAME(pDevnames, 1);
            this.outputPort = ReadOneDEVNAME(pDevnames, 2);
            this.PrintDialogDisplayed = true;
            SafeNativeMethods.GlobalUnlock(new HandleRef(null, hdevnames));
        }

        public override string ToString()
        {
            string str = System.Drawing.IntSecurity.HasPermission(System.Drawing.IntSecurity.AllPrinting) ? this.PrinterName : "<printer name unavailable>";
            return ("[PrinterSettings " + str + " Copies=" + this.Copies.ToString(CultureInfo.InvariantCulture) + " Collate=" + this.Collate.ToString(CultureInfo.InvariantCulture) + " Duplex=" + TypeDescriptor.GetConverter(typeof(System.Drawing.Printing.Duplex)).ConvertToString((int) this.Duplex) + " FromPage=" + this.FromPage.ToString(CultureInfo.InvariantCulture) + " LandscapeAngle=" + this.LandscapeAngle.ToString(CultureInfo.InvariantCulture) + " MaximumCopies=" + this.MaximumCopies.ToString(CultureInfo.InvariantCulture) + " OutputPort=" + this.OutputPort.ToString(CultureInfo.InvariantCulture) + " ToPage=" + this.ToPage.ToString(CultureInfo.InvariantCulture) + "]");
        }

        private short WriteOneDEVNAME(string str, IntPtr bufferStart, int index)
        {
            if (str == null)
            {
                str = "";
            }
            IntPtr destination = (IntPtr) (((long) bufferStart) + (index * Marshal.SystemDefaultCharSize));
            if (Marshal.SystemDefaultCharSize == 1)
            {
                byte[] bytes = Encoding.Default.GetBytes(str);
                Marshal.Copy(bytes, 0, destination, bytes.Length);
                Marshal.WriteByte((IntPtr) (((long) destination) + bytes.Length), 0);
            }
            else
            {
                char[] source = str.ToCharArray();
                Marshal.Copy(source, 0, destination, source.Length);
                Marshal.WriteInt16((IntPtr) (((long) destination) + (source.Length * 2)), (short) 0);
            }
            return (short) (str.Length + 1);
        }

        public bool CanDuplex
        {
            get
            {
                return (this.DeviceCapabilities(7, IntPtr.Zero, 0) == 1);
            }
        }

        public bool Collate
        {
            get
            {
                if (!this.collate.IsDefault)
                {
                    return (bool) this.collate;
                }
                return (this.GetModeField(ModeField.Collate, 0) == 1);
            }
            set
            {
                this.collate = value;
            }
        }

        public short Copies
        {
            get
            {
                if (this.copies != -1)
                {
                    return this.copies;
                }
                return this.GetModeField(ModeField.Copies, 1);
            }
            set
            {
                if (value < 0)
                {
                    object[] args = new object[] { "value", value.ToString(CultureInfo.CurrentCulture), 0.ToString(CultureInfo.CurrentCulture) };
                    throw new ArgumentException(System.Drawing.SR.GetString("InvalidLowBoundArgumentEx", args));
                }
                System.Drawing.IntSecurity.SafePrinting.Demand();
                this.copies = value;
            }
        }

        public PageSettings DefaultPageSettings
        {
            get
            {
                return this.defaultPageSettings;
            }
        }

        internal string DriverName
        {
            get
            {
                return this.driverName;
            }
        }

        public System.Drawing.Printing.Duplex Duplex
        {
            get
            {
                if (this.duplex != System.Drawing.Printing.Duplex.Default)
                {
                    return this.duplex;
                }
                return (System.Drawing.Printing.Duplex) this.GetModeField(ModeField.Duplex, 1);
            }
            set
            {
                if (!System.Drawing.ClientUtils.IsEnumValid(value, (int) value, -1, 3))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(System.Drawing.Printing.Duplex));
                }
                this.duplex = value;
            }
        }

        public int FromPage
        {
            get
            {
                return this.fromPage;
            }
            set
            {
                if (value < 0)
                {
                    object[] args = new object[] { "value", value.ToString(CultureInfo.CurrentCulture), 0.ToString(CultureInfo.CurrentCulture) };
                    throw new ArgumentException(System.Drawing.SR.GetString("InvalidLowBoundArgumentEx", args));
                }
                this.fromPage = value;
            }
        }

        public static StringCollection InstalledPrinters
        {
            get
            {
                int num4;
                int num5;
                string[] strArray;
                System.Drawing.IntSecurity.AllPrinting.Demand();
                if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                {
                    num4 = 4;
                    if (IntPtr.Size == 8)
                    {
                        num5 = ((IntPtr.Size * 2) + Marshal.SizeOf(typeof(int))) + 4;
                    }
                    else
                    {
                        num5 = (IntPtr.Size * 2) + Marshal.SizeOf(typeof(int));
                    }
                }
                else
                {
                    num4 = 5;
                    num5 = (IntPtr.Size * 2) + (Marshal.SizeOf(typeof(int)) * 3);
                }
                System.Drawing.IntSecurity.UnmanagedCode.Assert();
                try
                {
                    int num2;
                    int num3;
                    SafeNativeMethods.EnumPrinters(6, null, num4, IntPtr.Zero, 0, out num2, out num3);
                    IntPtr pPrinterEnum = Marshal.AllocCoTaskMem(num2);
                    int num = SafeNativeMethods.EnumPrinters(6, null, num4, pPrinterEnum, num2, out num2, out num3);
                    strArray = new string[num3];
                    if (num == 0)
                    {
                        Marshal.FreeCoTaskMem(pPrinterEnum);
                        throw new Win32Exception();
                    }
                    for (int i = 0; i < num3; i++)
                    {
                        IntPtr ptr = Marshal.ReadIntPtr((IntPtr) (((long) pPrinterEnum) + (i * num5)));
                        strArray[i] = Marshal.PtrToStringAuto(ptr);
                    }
                    Marshal.FreeCoTaskMem(pPrinterEnum);
                }
                finally
                {
                    CodeAccessPermission.RevertAssert();
                }
                return new StringCollection(strArray);
            }
        }

        public bool IsDefaultPrinter
        {
            get
            {
                if (this.printerName != null)
                {
                    return (this.printerName == GetDefaultPrinterName());
                }
                return true;
            }
        }

        public bool IsPlotter
        {
            get
            {
                return (this.GetDeviceCaps(2, 2) == 0);
            }
        }

        public bool IsValid
        {
            get
            {
                return (this.DeviceCapabilities(0x12, IntPtr.Zero, -1) != -1);
            }
        }

        public int LandscapeAngle
        {
            get
            {
                return this.DeviceCapabilities(0x11, IntPtr.Zero, 0);
            }
        }

        public int MaximumCopies
        {
            get
            {
                return this.DeviceCapabilities(0x12, IntPtr.Zero, 1);
            }
        }

        public int MaximumPage
        {
            get
            {
                return this.maxPage;
            }
            set
            {
                if (value < 0)
                {
                    object[] args = new object[] { "value", value.ToString(CultureInfo.CurrentCulture), 0.ToString(CultureInfo.CurrentCulture) };
                    throw new ArgumentException(System.Drawing.SR.GetString("InvalidLowBoundArgumentEx", args));
                }
                this.maxPage = value;
            }
        }

        public int MinimumPage
        {
            get
            {
                return this.minPage;
            }
            set
            {
                if (value < 0)
                {
                    object[] args = new object[] { "value", value.ToString(CultureInfo.CurrentCulture), 0.ToString(CultureInfo.CurrentCulture) };
                    throw new ArgumentException(System.Drawing.SR.GetString("InvalidLowBoundArgumentEx", args));
                }
                this.minPage = value;
            }
        }

        internal string OutputPort
        {
            get
            {
                return this.outputPort;
            }
            set
            {
                this.outputPort = value;
            }
        }

        public PaperSizeCollection PaperSizes
        {
            get
            {
                return new PaperSizeCollection(this.Get_PaperSizes());
            }
        }

        public PaperSourceCollection PaperSources
        {
            get
            {
                return new PaperSourceCollection(this.Get_PaperSources());
            }
        }

        internal bool PrintDialogDisplayed
        {
            get
            {
                return this.printDialogDisplayed;
            }
            set
            {
                System.Drawing.IntSecurity.AllPrinting.Demand();
                this.printDialogDisplayed = value;
            }
        }

        public string PrinterName
        {
            get
            {
                System.Drawing.IntSecurity.AllPrinting.Demand();
                return this.PrinterNameInternal;
            }
            set
            {
                System.Drawing.IntSecurity.AllPrinting.Demand();
                this.PrinterNameInternal = value;
            }
        }

        private string PrinterNameInternal
        {
            get
            {
                if (this.printerName == null)
                {
                    return GetDefaultPrinterName();
                }
                return this.printerName;
            }
            set
            {
                this.cachedDevmode = null;
                this.extrainfo = null;
                this.printerName = value;
            }
        }

        public PrinterResolutionCollection PrinterResolutions
        {
            get
            {
                return new PrinterResolutionCollection(this.Get_PrinterResolutions());
            }
        }

        public string PrintFileName
        {
            get
            {
                string outputPort = this.OutputPort;
                if (!string.IsNullOrEmpty(outputPort))
                {
                    System.Drawing.IntSecurity.DemandReadFileIO(outputPort);
                }
                return outputPort;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentNullException(value);
                }
                System.Drawing.IntSecurity.DemandWriteFileIO(value);
                this.OutputPort = value;
            }
        }

        public System.Drawing.Printing.PrintRange PrintRange
        {
            get
            {
                return this.printRange;
            }
            set
            {
                if (!Enum.IsDefined(typeof(System.Drawing.Printing.PrintRange), value))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(System.Drawing.Printing.PrintRange));
                }
                this.printRange = value;
            }
        }

        public bool PrintToFile
        {
            get
            {
                return this.printToFile;
            }
            set
            {
                this.printToFile = value;
            }
        }

        public bool SupportsColor
        {
            get
            {
                return (this.GetDeviceCaps(12, 1) > 1);
            }
        }

        public int ToPage
        {
            get
            {
                return this.toPage;
            }
            set
            {
                if (value < 0)
                {
                    object[] args = new object[] { "value", value.ToString(CultureInfo.CurrentCulture), 0.ToString(CultureInfo.CurrentCulture) };
                    throw new ArgumentException(System.Drawing.SR.GetString("InvalidLowBoundArgumentEx", args));
                }
                this.toPage = value;
            }
        }

        private class ArrayEnumerator : IEnumerator
        {
            private object[] array;
            private int endIndex;
            private int index;
            private object item;
            private int startIndex;

            public ArrayEnumerator(object[] array, int startIndex, int count)
            {
                this.array = array;
                this.startIndex = startIndex;
                this.endIndex = this.index + count;
                this.index = this.startIndex;
            }

            public bool MoveNext()
            {
                if (this.index >= this.endIndex)
                {
                    return false;
                }
                this.item = this.array[this.index++];
                return true;
            }

            public void Reset()
            {
                this.index = this.startIndex;
                this.item = null;
            }

            public object Current
            {
                get
                {
                    return this.item;
                }
            }
        }

        public class PaperSizeCollection : ICollection, IEnumerable
        {
            private PaperSize[] array;

            public PaperSizeCollection(PaperSize[] array)
            {
                this.array = array;
            }

            [EditorBrowsable(EditorBrowsableState.Never)]
            public int Add(PaperSize paperSize)
            {
                PaperSize[] array = new PaperSize[this.Count + 1];
                ((ICollection) this).CopyTo(array, 0);
                array[this.Count] = paperSize;
                this.array = array;
                return this.Count;
            }

            public void CopyTo(PaperSize[] paperSizes, int index)
            {
                Array.Copy(this.array, index, paperSizes, 0, this.array.Length);
            }

            public IEnumerator GetEnumerator()
            {
                return new PrinterSettings.ArrayEnumerator(this.array, 0, this.Count);
            }

            void ICollection.CopyTo(Array array, int index)
            {
                Array.Copy(this.array, index, array, 0, this.array.Length);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }

            public int Count
            {
                get
                {
                    return this.array.Length;
                }
            }

            public virtual PaperSize this[int index]
            {
                get
                {
                    return this.array[index];
                }
            }

            int ICollection.Count
            {
                get
                {
                    return this.Count;
                }
            }

            bool ICollection.IsSynchronized
            {
                get
                {
                    return false;
                }
            }

            object ICollection.SyncRoot
            {
                get
                {
                    return this;
                }
            }
        }

        public class PaperSourceCollection : ICollection, IEnumerable
        {
            private PaperSource[] array;

            public PaperSourceCollection(PaperSource[] array)
            {
                this.array = array;
            }

            [EditorBrowsable(EditorBrowsableState.Never)]
            public int Add(PaperSource paperSource)
            {
                PaperSource[] array = new PaperSource[this.Count + 1];
                ((ICollection) this).CopyTo(array, 0);
                array[this.Count] = paperSource;
                this.array = array;
                return this.Count;
            }

            public void CopyTo(PaperSource[] paperSources, int index)
            {
                Array.Copy(this.array, index, paperSources, 0, this.array.Length);
            }

            public IEnumerator GetEnumerator()
            {
                return new PrinterSettings.ArrayEnumerator(this.array, 0, this.Count);
            }

            void ICollection.CopyTo(Array array, int index)
            {
                Array.Copy(this.array, index, array, 0, this.array.Length);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }

            public int Count
            {
                get
                {
                    return this.array.Length;
                }
            }

            public virtual PaperSource this[int index]
            {
                get
                {
                    return this.array[index];
                }
            }

            int ICollection.Count
            {
                get
                {
                    return this.Count;
                }
            }

            bool ICollection.IsSynchronized
            {
                get
                {
                    return false;
                }
            }

            object ICollection.SyncRoot
            {
                get
                {
                    return this;
                }
            }
        }

        public class PrinterResolutionCollection : ICollection, IEnumerable
        {
            private PrinterResolution[] array;

            public PrinterResolutionCollection(PrinterResolution[] array)
            {
                this.array = array;
            }

            [EditorBrowsable(EditorBrowsableState.Never)]
            public int Add(PrinterResolution printerResolution)
            {
                PrinterResolution[] array = new PrinterResolution[this.Count + 1];
                ((ICollection) this).CopyTo(array, 0);
                array[this.Count] = printerResolution;
                this.array = array;
                return this.Count;
            }

            public void CopyTo(PrinterResolution[] printerResolutions, int index)
            {
                Array.Copy(this.array, index, printerResolutions, 0, this.array.Length);
            }

            public IEnumerator GetEnumerator()
            {
                return new PrinterSettings.ArrayEnumerator(this.array, 0, this.Count);
            }

            void ICollection.CopyTo(Array array, int index)
            {
                Array.Copy(this.array, index, array, 0, this.array.Length);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }

            public int Count
            {
                get
                {
                    return this.array.Length;
                }
            }

            public virtual PrinterResolution this[int index]
            {
                get
                {
                    return this.array[index];
                }
            }

            int ICollection.Count
            {
                get
                {
                    return this.Count;
                }
            }

            bool ICollection.IsSynchronized
            {
                get
                {
                    return false;
                }
            }

            object ICollection.SyncRoot
            {
                get
                {
                    return this;
                }
            }
        }

        public class StringCollection : ICollection, IEnumerable
        {
            private string[] array;

            public StringCollection(string[] array)
            {
                this.array = array;
            }

            [EditorBrowsable(EditorBrowsableState.Never)]
            public int Add(string value)
            {
                string[] array = new string[this.Count + 1];
                ((ICollection) this).CopyTo(array, 0);
                array[this.Count] = value;
                this.array = array;
                return this.Count;
            }

            public void CopyTo(string[] strings, int index)
            {
                Array.Copy(this.array, index, strings, 0, this.array.Length);
            }

            public IEnumerator GetEnumerator()
            {
                return new PrinterSettings.ArrayEnumerator(this.array, 0, this.Count);
            }

            void ICollection.CopyTo(Array array, int index)
            {
                Array.Copy(this.array, index, array, 0, this.array.Length);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }

            public int Count
            {
                get
                {
                    return this.array.Length;
                }
            }

            public virtual string this[int index]
            {
                get
                {
                    return this.array[index];
                }
            }

            int ICollection.Count
            {
                get
                {
                    return this.Count;
                }
            }

            bool ICollection.IsSynchronized
            {
                get
                {
                    return false;
                }
            }

            object ICollection.SyncRoot
            {
                get
                {
                    return this;
                }
            }
        }
    }
}

