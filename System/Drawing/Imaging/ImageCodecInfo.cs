namespace System.Drawing.Imaging
{
    using System;
    using System.Drawing;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    public sealed class ImageCodecInfo
    {
        private Guid clsid;
        private string codecName;
        private string dllName;
        private string filenameExtension;
        private ImageCodecFlags flags;
        private string formatDescription;
        private Guid formatID;
        private string mimeType;
        private byte[][] signatureMasks;
        private byte[][] signaturePatterns;
        private int version;

        internal ImageCodecInfo()
        {
        }

        private static ImageCodecInfo[] ConvertFromMemory(IntPtr memoryStart, int numCodecs)
        {
            ImageCodecInfo[] infoArray = new ImageCodecInfo[numCodecs];
            for (int i = 0; i < numCodecs; i++)
            {
                IntPtr lparam = (IntPtr) (((long) memoryStart) + (Marshal.SizeOf(typeof(ImageCodecInfoPrivate)) * i));
                ImageCodecInfoPrivate data = new ImageCodecInfoPrivate();
                System.Drawing.UnsafeNativeMethods.PtrToStructure(lparam, data);
                infoArray[i] = new ImageCodecInfo();
                infoArray[i].Clsid = data.Clsid;
                infoArray[i].FormatID = data.FormatID;
                infoArray[i].CodecName = Marshal.PtrToStringUni(data.CodecName);
                infoArray[i].DllName = Marshal.PtrToStringUni(data.DllName);
                infoArray[i].FormatDescription = Marshal.PtrToStringUni(data.FormatDescription);
                infoArray[i].FilenameExtension = Marshal.PtrToStringUni(data.FilenameExtension);
                infoArray[i].MimeType = Marshal.PtrToStringUni(data.MimeType);
                infoArray[i].Flags = (ImageCodecFlags) data.Flags;
                infoArray[i].Version = data.Version;
                infoArray[i].SignaturePatterns = new byte[data.SigCount][];
                infoArray[i].SignatureMasks = new byte[data.SigCount][];
                for (int j = 0; j < data.SigCount; j++)
                {
                    infoArray[i].SignaturePatterns[j] = new byte[data.SigSize];
                    infoArray[i].SignatureMasks[j] = new byte[data.SigSize];
                    Marshal.Copy((IntPtr) (((long) data.SigMask) + (j * data.SigSize)), infoArray[i].SignatureMasks[j], 0, data.SigSize);
                    Marshal.Copy((IntPtr) (((long) data.SigPattern) + (j * data.SigSize)), infoArray[i].SignaturePatterns[j], 0, data.SigSize);
                }
            }
            return infoArray;
        }

        public static ImageCodecInfo[] GetImageDecoders()
        {
            ImageCodecInfo[] infoArray;
            int num;
            int num2;
            int status = SafeNativeMethods.Gdip.GdipGetImageDecodersSize(out num, out num2);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            IntPtr decoders = Marshal.AllocHGlobal(num2);
            try
            {
                status = SafeNativeMethods.Gdip.GdipGetImageDecoders(num, num2, decoders);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
                infoArray = ConvertFromMemory(decoders, num);
            }
            finally
            {
                Marshal.FreeHGlobal(decoders);
            }
            return infoArray;
        }

        public static ImageCodecInfo[] GetImageEncoders()
        {
            ImageCodecInfo[] infoArray;
            int num;
            int num2;
            int status = SafeNativeMethods.Gdip.GdipGetImageEncodersSize(out num, out num2);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            IntPtr encoders = Marshal.AllocHGlobal(num2);
            try
            {
                status = SafeNativeMethods.Gdip.GdipGetImageEncoders(num, num2, encoders);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
                infoArray = ConvertFromMemory(encoders, num);
            }
            finally
            {
                Marshal.FreeHGlobal(encoders);
            }
            return infoArray;
        }

        public Guid Clsid
        {
            get
            {
                return this.clsid;
            }
            set
            {
                this.clsid = value;
            }
        }

        public string CodecName
        {
            get
            {
                return this.codecName;
            }
            set
            {
                this.codecName = value;
            }
        }

        public string DllName
        {
            get
            {
                if (this.dllName != null)
                {
                    new FileIOPermission(FileIOPermissionAccess.PathDiscovery, this.dllName).Demand();
                }
                return this.dllName;
            }
            set
            {
                if (value != null)
                {
                    new FileIOPermission(FileIOPermissionAccess.PathDiscovery, value).Demand();
                }
                this.dllName = value;
            }
        }

        public string FilenameExtension
        {
            get
            {
                return this.filenameExtension;
            }
            set
            {
                this.filenameExtension = value;
            }
        }

        public ImageCodecFlags Flags
        {
            get
            {
                return this.flags;
            }
            set
            {
                this.flags = value;
            }
        }

        public string FormatDescription
        {
            get
            {
                return this.formatDescription;
            }
            set
            {
                this.formatDescription = value;
            }
        }

        public Guid FormatID
        {
            get
            {
                return this.formatID;
            }
            set
            {
                this.formatID = value;
            }
        }

        public string MimeType
        {
            get
            {
                return this.mimeType;
            }
            set
            {
                this.mimeType = value;
            }
        }

        [CLSCompliant(false)]
        public byte[][] SignatureMasks
        {
            get
            {
                return this.signatureMasks;
            }
            set
            {
                this.signatureMasks = value;
            }
        }

        [CLSCompliant(false)]
        public byte[][] SignaturePatterns
        {
            get
            {
                return this.signaturePatterns;
            }
            set
            {
                this.signaturePatterns = value;
            }
        }

        public int Version
        {
            get
            {
                return this.version;
            }
            set
            {
                this.version = value;
            }
        }
    }
}

