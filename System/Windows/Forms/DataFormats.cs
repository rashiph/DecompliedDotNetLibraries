namespace System.Windows.Forms
{
    using System;
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using System.Text;

    public class DataFormats
    {
        public static readonly string Bitmap = "Bitmap";
        public static readonly string CommaSeparatedValue = "Csv";
        public static readonly string Dib = "DeviceIndependentBitmap";
        public static readonly string Dif = "DataInterchangeFormat";
        public static readonly string EnhancedMetafile = "EnhancedMetafile";
        public static readonly string FileDrop = "FileDrop";
        private static int formatCount = 0;
        private static Format[] formatList;
        public static readonly string Html = "HTML Format";
        private static object internalSyncObject = new object();
        public static readonly string Locale = "Locale";
        public static readonly string MetafilePict = "MetaFilePict";
        public static readonly string OemText = "OEMText";
        public static readonly string Palette = "Palette";
        public static readonly string PenData = "PenData";
        public static readonly string Riff = "RiffAudio";
        public static readonly string Rtf = "Rich Text Format";
        public static readonly string Serializable = (Application.WindowsFormsVersion + "PersistentObject");
        public static readonly string StringFormat = typeof(string).FullName;
        public static readonly string SymbolicLink = "SymbolicLink";
        public static readonly string Text = "Text";
        public static readonly string Tiff = "TaggedImageFileFormat";
        public static readonly string UnicodeText = "UnicodeText";
        public static readonly string WaveAudio = "WaveAudio";

        private DataFormats()
        {
        }

        private static void EnsureFormatSpace(int size)
        {
            if ((formatList == null) || (formatList.Length <= (formatCount + size)))
            {
                int num = formatCount + 20;
                Format[] formatArray = new Format[num];
                for (int i = 0; i < formatCount; i++)
                {
                    formatArray[i] = formatList[i];
                }
                formatList = formatArray;
            }
        }

        private static void EnsurePredefined()
        {
            if (formatCount == 0)
            {
                formatList = new Format[] { new Format(UnicodeText, 13), new Format(Text, 1), new Format(Bitmap, 2), new Format(MetafilePict, 3), new Format(EnhancedMetafile, 14), new Format(Dif, 5), new Format(Tiff, 6), new Format(OemText, 7), new Format(Dib, 8), new Format(Palette, 9), new Format(PenData, 10), new Format(Riff, 11), new Format(WaveAudio, 12), new Format(SymbolicLink, 4), new Format(FileDrop, 15), new Format(Locale, 0x10) };
                formatCount = formatList.Length;
            }
        }

        public static Format GetFormat(int id)
        {
            return InternalGetFormat(null, id);
        }

        public static Format GetFormat(string format)
        {
            lock (internalSyncObject)
            {
                EnsurePredefined();
                for (int i = 0; i < formatCount; i++)
                {
                    if (formatList[i].Name.Equals(format))
                    {
                        return formatList[i];
                    }
                }
                for (int j = 0; j < formatCount; j++)
                {
                    if (string.Equals(formatList[j].Name, format, StringComparison.OrdinalIgnoreCase))
                    {
                        return formatList[j];
                    }
                }
                int id = SafeNativeMethods.RegisterClipboardFormat(format);
                if (id == 0)
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error(), System.Windows.Forms.SR.GetString("RegisterCFFailed"));
                }
                EnsureFormatSpace(1);
                formatList[formatCount] = new Format(format, id);
                return formatList[formatCount++];
            }
        }

        private static Format InternalGetFormat(string strName, int id)
        {
            lock (internalSyncObject)
            {
                EnsurePredefined();
                for (int i = 0; i < formatCount; i++)
                {
                    if (formatList[i].Id == id)
                    {
                        return formatList[i];
                    }
                }
                StringBuilder lpString = new StringBuilder(0x80);
                if (SafeNativeMethods.GetClipboardFormatName(id, lpString, lpString.Capacity) == 0)
                {
                    lpString.Length = 0;
                    if (strName == null)
                    {
                        lpString.Append("Format").Append(id);
                    }
                    else
                    {
                        lpString.Append(strName);
                    }
                }
                EnsureFormatSpace(1);
                formatList[formatCount] = new Format(lpString.ToString(), id);
                return formatList[formatCount++];
            }
        }

        public class Format
        {
            private readonly int id;
            private readonly string name;

            public Format(string name, int id)
            {
                this.name = name;
                this.id = id;
            }

            public int Id
            {
                get
                {
                    return this.id;
                }
            }

            public string Name
            {
                get
                {
                    return this.name;
                }
            }
        }
    }
}

