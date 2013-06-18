namespace System.Windows.Forms
{
    using System;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Drawing;
    using System.Globalization;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.ComTypes;
    using System.Security;
    using System.Security.Permissions;
    using System.Threading;

    public sealed class Clipboard
    {
        private Clipboard()
        {
        }

        public static void Clear()
        {
            SetDataObject(new DataObject());
        }

        public static bool ContainsAudio()
        {
            System.Windows.Forms.IDataObject dataObject = GetDataObject();
            return ((dataObject != null) && dataObject.GetDataPresent(DataFormats.WaveAudio, false));
        }

        public static bool ContainsData(string format)
        {
            System.Windows.Forms.IDataObject dataObject = GetDataObject();
            return ((dataObject != null) && dataObject.GetDataPresent(format, false));
        }

        public static bool ContainsFileDropList()
        {
            System.Windows.Forms.IDataObject dataObject = GetDataObject();
            return ((dataObject != null) && dataObject.GetDataPresent(DataFormats.FileDrop, true));
        }

        public static bool ContainsImage()
        {
            System.Windows.Forms.IDataObject dataObject = GetDataObject();
            return ((dataObject != null) && dataObject.GetDataPresent(DataFormats.Bitmap, true));
        }

        public static bool ContainsText()
        {
            if ((Environment.OSVersion.Platform == PlatformID.Win32NT) && (Environment.OSVersion.Version.Major >= 5))
            {
                return ContainsText(TextDataFormat.UnicodeText);
            }
            return ContainsText(TextDataFormat.Text);
        }

        public static bool ContainsText(TextDataFormat format)
        {
            if (!System.Windows.Forms.ClientUtils.IsEnumValid(format, (int) format, 0, 4))
            {
                throw new InvalidEnumArgumentException("format", (int) format, typeof(TextDataFormat));
            }
            System.Windows.Forms.IDataObject dataObject = GetDataObject();
            return ((dataObject != null) && dataObject.GetDataPresent(ConvertToDataFormats(format), false));
        }

        private static string ConvertToDataFormats(TextDataFormat format)
        {
            switch (format)
            {
                case TextDataFormat.Text:
                    return DataFormats.Text;

                case TextDataFormat.UnicodeText:
                    return DataFormats.UnicodeText;

                case TextDataFormat.Rtf:
                    return DataFormats.Rtf;

                case TextDataFormat.Html:
                    return DataFormats.Html;

                case TextDataFormat.CommaSeparatedValue:
                    return DataFormats.CommaSeparatedValue;
            }
            return DataFormats.UnicodeText;
        }

        public static Stream GetAudioStream()
        {
            System.Windows.Forms.IDataObject dataObject = GetDataObject();
            if (dataObject != null)
            {
                return (dataObject.GetData(DataFormats.WaveAudio, false) as Stream);
            }
            return null;
        }

        public static object GetData(string format)
        {
            System.Windows.Forms.IDataObject dataObject = GetDataObject();
            if (dataObject != null)
            {
                return dataObject.GetData(format);
            }
            return null;
        }

        public static System.Windows.Forms.IDataObject GetDataObject()
        {
            System.Windows.Forms.IntSecurity.ClipboardRead.Demand();
            if (Application.OleRequired() == ApartmentState.STA)
            {
                return GetDataObject(10, 100);
            }
            if (Application.MessageLoop)
            {
                throw new ThreadStateException(System.Windows.Forms.SR.GetString("ThreadMustBeSTA"));
            }
            return null;
        }

        private static System.Windows.Forms.IDataObject GetDataObject(int retryTimes, int retryDelay)
        {
            System.Runtime.InteropServices.ComTypes.IDataObject data = null;
            int num;
            int num2 = retryTimes;
            do
            {
                num = System.Windows.Forms.UnsafeNativeMethods.OleGetClipboard(ref data);
                if (num != 0)
                {
                    if (num2 == 0)
                    {
                        ThrowIfFailed(num);
                    }
                    num2--;
                    Thread.Sleep(retryDelay);
                }
            }
            while (num != 0);
            if (data == null)
            {
                return null;
            }
            if ((data is System.Windows.Forms.IDataObject) && !Marshal.IsComObject(data))
            {
                return (System.Windows.Forms.IDataObject) data;
            }
            return new DataObject(data);
        }

        public static StringCollection GetFileDropList()
        {
            System.Windows.Forms.IDataObject dataObject = GetDataObject();
            StringCollection strings = new StringCollection();
            if (dataObject != null)
            {
                string[] data = dataObject.GetData(DataFormats.FileDrop, true) as string[];
                if (data != null)
                {
                    strings.AddRange(data);
                }
            }
            return strings;
        }

        public static Image GetImage()
        {
            System.Windows.Forms.IDataObject dataObject = GetDataObject();
            if (dataObject != null)
            {
                return (dataObject.GetData(DataFormats.Bitmap, true) as Image);
            }
            return null;
        }

        public static string GetText()
        {
            if ((Environment.OSVersion.Platform == PlatformID.Win32NT) && (Environment.OSVersion.Version.Major >= 5))
            {
                return GetText(TextDataFormat.UnicodeText);
            }
            return GetText(TextDataFormat.Text);
        }

        public static string GetText(TextDataFormat format)
        {
            if (!System.Windows.Forms.ClientUtils.IsEnumValid(format, (int) format, 0, 4))
            {
                throw new InvalidEnumArgumentException("format", (int) format, typeof(TextDataFormat));
            }
            System.Windows.Forms.IDataObject dataObject = GetDataObject();
            if (dataObject != null)
            {
                string data = dataObject.GetData(ConvertToDataFormats(format), false) as string;
                if (data != null)
                {
                    return data;
                }
            }
            return string.Empty;
        }

        private static bool IsFormatValid(DataObject data)
        {
            return IsFormatValid(data.GetFormats());
        }

        internal static bool IsFormatValid(FORMATETC[] formats)
        {
            if ((formats == null) || (formats.Length > 4))
            {
                return false;
            }
            for (int i = 0; i < formats.Length; i++)
            {
                short cfFormat = formats[i].cfFormat;
                if (((cfFormat != 1) && (cfFormat != 13)) && ((cfFormat != DataFormats.GetFormat("System.String").Id) && (cfFormat != DataFormats.GetFormat("Csv").Id)))
                {
                    return false;
                }
            }
            return true;
        }

        internal static bool IsFormatValid(string[] formats)
        {
            if ((formats == null) || (formats.Length > 4))
            {
                return false;
            }
            for (int i = 0; i < formats.Length; i++)
            {
                string str;
                if (((str = formats[i]) == null) || (((str != "Text") && (str != "UnicodeText")) && ((str != "System.String") && (str != "Csv"))))
                {
                    return false;
                }
            }
            return true;
        }

        public static void SetAudio(byte[] audioBytes)
        {
            if (audioBytes == null)
            {
                throw new ArgumentNullException("audioBytes");
            }
            SetAudio(new MemoryStream(audioBytes));
        }

        public static void SetAudio(Stream audioStream)
        {
            if (audioStream == null)
            {
                throw new ArgumentNullException("audioStream");
            }
            System.Windows.Forms.IDataObject data = new DataObject();
            data.SetData(DataFormats.WaveAudio, false, audioStream);
            SetDataObject(data, true);
        }

        public static void SetData(string format, object data)
        {
            System.Windows.Forms.IDataObject obj2 = new DataObject();
            obj2.SetData(format, data);
            SetDataObject(obj2, true);
        }

        public static void SetDataObject(object data)
        {
            SetDataObject(data, false);
        }

        public static void SetDataObject(object data, bool copy)
        {
            SetDataObject(data, copy, 10, 100);
        }

        [UIPermission(SecurityAction.Demand, Clipboard=UIPermissionClipboard.OwnClipboard)]
        public static void SetDataObject(object data, bool copy, int retryTimes, int retryDelay)
        {
            if (Application.OleRequired() != ApartmentState.STA)
            {
                throw new ThreadStateException(System.Windows.Forms.SR.GetString("ThreadMustBeSTA"));
            }
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }
            if (retryTimes < 0)
            {
                object[] args = new object[] { "retryTimes", retryTimes.ToString(CultureInfo.CurrentCulture), 0.ToString(CultureInfo.CurrentCulture) };
                throw new ArgumentOutOfRangeException("retryTimes", System.Windows.Forms.SR.GetString("InvalidLowBoundArgumentEx", args));
            }
            if (retryDelay < 0)
            {
                object[] objArray2 = new object[] { "retryDelay", retryDelay.ToString(CultureInfo.CurrentCulture), 0.ToString(CultureInfo.CurrentCulture) };
                throw new ArgumentOutOfRangeException("retryDelay", System.Windows.Forms.SR.GetString("InvalidLowBoundArgumentEx", objArray2));
            }
            DataObject obj2 = null;
            if (!(data is System.Runtime.InteropServices.ComTypes.IDataObject))
            {
                obj2 = new DataObject(data);
            }
            bool flag = false;
            try
            {
                System.Windows.Forms.IntSecurity.ClipboardRead.Demand();
            }
            catch (SecurityException)
            {
                flag = true;
            }
            if (flag)
            {
                if (obj2 == null)
                {
                    obj2 = data as DataObject;
                }
                if (!IsFormatValid(obj2))
                {
                    throw new SecurityException(System.Windows.Forms.SR.GetString("ClipboardSecurityException"));
                }
            }
            if (obj2 != null)
            {
                obj2.RestrictedFormats = flag;
            }
            int num2 = retryTimes;
            System.Windows.Forms.IntSecurity.UnmanagedCode.Assert();
            try
            {
                int num;
                do
                {
                    if (data is System.Runtime.InteropServices.ComTypes.IDataObject)
                    {
                        num = System.Windows.Forms.UnsafeNativeMethods.OleSetClipboard((System.Runtime.InteropServices.ComTypes.IDataObject) data);
                    }
                    else
                    {
                        num = System.Windows.Forms.UnsafeNativeMethods.OleSetClipboard(obj2);
                    }
                    if (num != 0)
                    {
                        if (num2 == 0)
                        {
                            ThrowIfFailed(num);
                        }
                        num2--;
                        Thread.Sleep(retryDelay);
                    }
                }
                while (num != 0);
                if (copy)
                {
                    num2 = retryTimes;
                    do
                    {
                        num = System.Windows.Forms.UnsafeNativeMethods.OleFlushClipboard();
                        if (num != 0)
                        {
                            if (num2 == 0)
                            {
                                ThrowIfFailed(num);
                            }
                            num2--;
                            Thread.Sleep(retryDelay);
                        }
                    }
                    while (num != 0);
                }
            }
            finally
            {
                CodeAccessPermission.RevertAssert();
            }
        }

        public static void SetFileDropList(StringCollection filePaths)
        {
            if (filePaths == null)
            {
                throw new ArgumentNullException("filePaths");
            }
            if (filePaths.Count == 0)
            {
                throw new ArgumentException(System.Windows.Forms.SR.GetString("CollectionEmptyException"));
            }
            foreach (string str in filePaths)
            {
                try
                {
                    Path.GetFullPath(str);
                }
                catch (Exception exception)
                {
                    if (System.Windows.Forms.ClientUtils.IsSecurityOrCriticalException(exception))
                    {
                        throw;
                    }
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("Clipboard_InvalidPath", new object[] { str, "filePaths" }), exception);
                }
            }
            if (filePaths.Count > 0)
            {
                System.Windows.Forms.IDataObject data = new DataObject();
                string[] array = new string[filePaths.Count];
                filePaths.CopyTo(array, 0);
                data.SetData(DataFormats.FileDrop, true, array);
                SetDataObject(data, true);
            }
        }

        public static void SetImage(Image image)
        {
            if (image == null)
            {
                throw new ArgumentNullException("image");
            }
            System.Windows.Forms.IDataObject data = new DataObject();
            data.SetData(DataFormats.Bitmap, true, image);
            SetDataObject(data, true);
        }

        public static void SetText(string text)
        {
            if ((Environment.OSVersion.Platform != PlatformID.Win32NT) || (Environment.OSVersion.Version.Major < 5))
            {
                SetText(text, TextDataFormat.Text);
            }
            else
            {
                SetText(text, TextDataFormat.UnicodeText);
            }
        }

        public static void SetText(string text, TextDataFormat format)
        {
            if (string.IsNullOrEmpty(text))
            {
                throw new ArgumentNullException("text");
            }
            if (!System.Windows.Forms.ClientUtils.IsEnumValid(format, (int) format, 0, 4))
            {
                throw new InvalidEnumArgumentException("format", (int) format, typeof(TextDataFormat));
            }
            System.Windows.Forms.IDataObject data = new DataObject();
            data.SetData(ConvertToDataFormats(format), false, text);
            SetDataObject(data, true);
        }

        private static void ThrowIfFailed(int hr)
        {
            if (hr != 0)
            {
                ExternalException exception = new ExternalException(System.Windows.Forms.SR.GetString("ClipboardOperationFailed"), hr);
                throw exception;
            }
        }
    }
}

