namespace System.Windows.Forms
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Drawing;
    using System.Internal;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.ComTypes;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Security;
    using System.Security.Permissions;
    using System.Text;

    [ClassInterface(ClassInterfaceType.None)]
    public class DataObject : System.Windows.Forms.IDataObject, System.Runtime.InteropServices.ComTypes.IDataObject
    {
        private static readonly TYMED[] ALLOWED_TYMEDS = new TYMED[] { TYMED.TYMED_HGLOBAL, TYMED.TYMED_ISTREAM, TYMED.TYMED_ENHMF, TYMED.TYMED_MFPICT, TYMED.TYMED_GDI };
        private static readonly string CF_DEPRECATED_FILENAME = "FileName";
        private static readonly string CF_DEPRECATED_FILENAMEW = "FileNameW";
        private const int DATA_S_SAMEFORMATETC = 0x40130;
        private const int DV_E_DVASPECT = -2147221397;
        private const int DV_E_FORMATETC = -2147221404;
        private const int DV_E_LINDEX = -2147221400;
        private const int DV_E_TYMED = -2147221399;
        private System.Windows.Forms.IDataObject innerData;
        private const int OLE_E_ADVISENOTSUPPORTED = -2147221501;
        private const int OLE_E_NOTRUNNING = -2147221499;
        private static readonly byte[] serializedObjectID = new Guid("FD9EA796-3B13-4370-A679-56106BB288FB").ToByteArray();

        public DataObject()
        {
            this.innerData = new DataStore();
        }

        public DataObject(object data)
        {
            if ((data is System.Windows.Forms.IDataObject) && !Marshal.IsComObject(data))
            {
                this.innerData = (System.Windows.Forms.IDataObject) data;
            }
            else if (data is System.Runtime.InteropServices.ComTypes.IDataObject)
            {
                this.innerData = new OleConverter((System.Runtime.InteropServices.ComTypes.IDataObject) data);
            }
            else
            {
                this.innerData = new DataStore();
                this.SetData(data);
            }
        }

        internal DataObject(System.Runtime.InteropServices.ComTypes.IDataObject data)
        {
            if (data is DataObject)
            {
                this.innerData = data as System.Windows.Forms.IDataObject;
            }
            else
            {
                this.innerData = new OleConverter(data);
            }
        }

        internal DataObject(System.Windows.Forms.IDataObject data)
        {
            this.innerData = data;
        }

        public DataObject(string format, object data) : this()
        {
            this.SetData(format, data);
        }

        public virtual bool ContainsAudio()
        {
            return this.GetDataPresent(DataFormats.WaveAudio, false);
        }

        public virtual bool ContainsFileDropList()
        {
            return this.GetDataPresent(DataFormats.FileDrop, true);
        }

        public virtual bool ContainsImage()
        {
            return this.GetDataPresent(DataFormats.Bitmap, true);
        }

        public virtual bool ContainsText()
        {
            return this.ContainsText(TextDataFormat.UnicodeText);
        }

        public virtual bool ContainsText(TextDataFormat format)
        {
            if (!System.Windows.Forms.ClientUtils.IsEnumValid(format, (int) format, 0, 4))
            {
                throw new InvalidEnumArgumentException("format", (int) format, typeof(TextDataFormat));
            }
            return this.GetDataPresent(ConvertToDataFormats(format), false);
        }

        private static string ConvertToDataFormats(TextDataFormat format)
        {
            switch (format)
            {
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

        public virtual Stream GetAudioStream()
        {
            return (this.GetData(DataFormats.WaveAudio, false) as Stream);
        }

        private IntPtr GetCompatibleBitmap(Bitmap bm)
        {
            IntPtr hbitmap = bm.GetHbitmap();
            IntPtr dC = System.Windows.Forms.UnsafeNativeMethods.GetDC(System.Windows.Forms.NativeMethods.NullHandleRef);
            IntPtr handle = System.Windows.Forms.UnsafeNativeMethods.CreateCompatibleDC(new HandleRef(null, dC));
            IntPtr ptr4 = System.Windows.Forms.SafeNativeMethods.SelectObject(new HandleRef(null, handle), new HandleRef(bm, hbitmap));
            IntPtr ptr5 = System.Windows.Forms.UnsafeNativeMethods.CreateCompatibleDC(new HandleRef(null, dC));
            IntPtr ptr6 = System.Windows.Forms.SafeNativeMethods.CreateCompatibleBitmap(new HandleRef(null, dC), bm.Size.Width, bm.Size.Height);
            IntPtr ptr7 = System.Windows.Forms.SafeNativeMethods.SelectObject(new HandleRef(null, ptr5), new HandleRef(null, ptr6));
            System.Windows.Forms.SafeNativeMethods.BitBlt(new HandleRef(null, ptr5), 0, 0, bm.Size.Width, bm.Size.Height, new HandleRef(null, handle), 0, 0, 0xcc0020);
            System.Windows.Forms.SafeNativeMethods.SelectObject(new HandleRef(null, handle), new HandleRef(null, ptr4));
            System.Windows.Forms.SafeNativeMethods.SelectObject(new HandleRef(null, ptr5), new HandleRef(null, ptr7));
            System.Windows.Forms.UnsafeNativeMethods.DeleteCompatibleDC(new HandleRef(null, handle));
            System.Windows.Forms.UnsafeNativeMethods.DeleteCompatibleDC(new HandleRef(null, ptr5));
            System.Windows.Forms.UnsafeNativeMethods.ReleaseDC(System.Windows.Forms.NativeMethods.NullHandleRef, new HandleRef(null, dC));
            System.Windows.Forms.SafeNativeMethods.DeleteObject(new HandleRef(bm, hbitmap));
            return ptr6;
        }

        public virtual object GetData(string format)
        {
            return this.GetData(format, true);
        }

        public virtual object GetData(System.Type format)
        {
            if (format == null)
            {
                return null;
            }
            return this.GetData(format.FullName);
        }

        public virtual object GetData(string format, bool autoConvert)
        {
            return this.innerData.GetData(format, autoConvert);
        }

        private void GetDataIntoOleStructs(ref FORMATETC formatetc, ref STGMEDIUM medium)
        {
            if (this.GetTymedUseable(formatetc.tymed) && this.GetTymedUseable(medium.tymed))
            {
                string name = DataFormats.GetFormat(formatetc.cfFormat).Name;
                if (!this.GetDataPresent(name))
                {
                    Marshal.ThrowExceptionForHR(-2147221404);
                }
                else
                {
                    object data = this.GetData(name);
                    if ((formatetc.tymed & TYMED.TYMED_HGLOBAL) == TYMED.TYMED_NULL)
                    {
                        if ((formatetc.tymed & TYMED.TYMED_GDI) == TYMED.TYMED_NULL)
                        {
                            Marshal.ThrowExceptionForHR(-2147221399);
                        }
                        else if (name.Equals(DataFormats.Bitmap) && (data is Bitmap))
                        {
                            Bitmap bm = (Bitmap) data;
                            if (bm != null)
                            {
                                medium.unionmember = this.GetCompatibleBitmap(bm);
                            }
                        }
                    }
                    else
                    {
                        int hr = this.SaveDataToHandle(data, name, ref medium);
                        if (System.Windows.Forms.NativeMethods.Failed(hr))
                        {
                            Marshal.ThrowExceptionForHR(hr);
                        }
                    }
                }
            }
            else
            {
                Marshal.ThrowExceptionForHR(-2147221399);
            }
        }

        public virtual bool GetDataPresent(string format)
        {
            return this.GetDataPresent(format, true);
        }

        public virtual bool GetDataPresent(System.Type format)
        {
            if (format == null)
            {
                return false;
            }
            return this.GetDataPresent(format.FullName);
        }

        public virtual bool GetDataPresent(string format, bool autoConvert)
        {
            return this.innerData.GetDataPresent(format, autoConvert);
        }

        private static string[] GetDistinctStrings(string[] formats)
        {
            ArrayList list = new ArrayList();
            for (int i = 0; i < formats.Length; i++)
            {
                string item = formats[i];
                if (!list.Contains(item))
                {
                    list.Add(item);
                }
            }
            string[] array = new string[list.Count];
            list.CopyTo(array, 0);
            return array;
        }

        public virtual StringCollection GetFileDropList()
        {
            StringCollection strings = new StringCollection();
            string[] data = this.GetData(DataFormats.FileDrop, true) as string[];
            if (data != null)
            {
                strings.AddRange(data);
            }
            return strings;
        }

        public virtual string[] GetFormats()
        {
            return this.GetFormats(true);
        }

        public virtual string[] GetFormats(bool autoConvert)
        {
            return this.innerData.GetFormats(autoConvert);
        }

        public virtual Image GetImage()
        {
            return (this.GetData(DataFormats.Bitmap, true) as Image);
        }

        private static string[] GetMappedFormats(string format)
        {
            if (format == null)
            {
                return null;
            }
            if ((format.Equals(DataFormats.Text) || format.Equals(DataFormats.UnicodeText)) || format.Equals(DataFormats.StringFormat))
            {
                return new string[] { DataFormats.StringFormat, DataFormats.UnicodeText, DataFormats.Text };
            }
            if ((format.Equals(DataFormats.FileDrop) || format.Equals(CF_DEPRECATED_FILENAME)) || format.Equals(CF_DEPRECATED_FILENAMEW))
            {
                return new string[] { DataFormats.FileDrop, CF_DEPRECATED_FILENAMEW, CF_DEPRECATED_FILENAME };
            }
            if (format.Equals(DataFormats.Bitmap) || format.Equals(typeof(Bitmap).FullName))
            {
                return new string[] { typeof(Bitmap).FullName, DataFormats.Bitmap };
            }
            return new string[] { format };
        }

        public virtual string GetText()
        {
            return this.GetText(TextDataFormat.UnicodeText);
        }

        public virtual string GetText(TextDataFormat format)
        {
            if (!System.Windows.Forms.ClientUtils.IsEnumValid(format, (int) format, 0, 4))
            {
                throw new InvalidEnumArgumentException("format", (int) format, typeof(TextDataFormat));
            }
            string data = this.GetData(ConvertToDataFormats(format), false) as string;
            if (data != null)
            {
                return data;
            }
            return string.Empty;
        }

        private bool GetTymedUseable(TYMED tymed)
        {
            for (int i = 0; i < ALLOWED_TYMEDS.Length; i++)
            {
                if ((tymed & ALLOWED_TYMEDS[i]) != TYMED.TYMED_NULL)
                {
                    return true;
                }
            }
            return false;
        }

        private int SaveDataToHandle(object data, string format, ref STGMEDIUM medium)
        {
            int num = -2147467259;
            if (data is Stream)
            {
                return this.SaveStreamToHandle(ref medium.unionmember, (Stream) data);
            }
            if ((format.Equals(DataFormats.Text) || format.Equals(DataFormats.Rtf)) || (format.Equals(DataFormats.Html) || format.Equals(DataFormats.OemText)))
            {
                return this.SaveStringToHandle(medium.unionmember, data.ToString(), false);
            }
            if (format.Equals(DataFormats.UnicodeText))
            {
                return this.SaveStringToHandle(medium.unionmember, data.ToString(), true);
            }
            if (format.Equals(DataFormats.FileDrop))
            {
                return this.SaveFileListToHandle(medium.unionmember, (string[]) data);
            }
            if (format.Equals(CF_DEPRECATED_FILENAME))
            {
                string[] strArray = (string[]) data;
                return this.SaveStringToHandle(medium.unionmember, strArray[0], false);
            }
            if (format.Equals(CF_DEPRECATED_FILENAMEW))
            {
                string[] strArray2 = (string[]) data;
                return this.SaveStringToHandle(medium.unionmember, strArray2[0], true);
            }
            if (format.Equals(DataFormats.Dib) && (data is Image))
            {
                return -2147221399;
            }
            if ((!format.Equals(DataFormats.Serializable) && !(data is ISerializable)) && ((data == null) || !data.GetType().IsSerializable))
            {
                return num;
            }
            return this.SaveObjectToHandle(ref medium.unionmember, data);
        }

        private int SaveFileListToHandle(IntPtr handle, string[] files)
        {
            if (files != null)
            {
                if (files.Length < 1)
                {
                    return 0;
                }
                if (handle == IntPtr.Zero)
                {
                    return -2147024809;
                }
                bool flag = Marshal.SystemDefaultCharSize != 1;
                IntPtr zero = IntPtr.Zero;
                int num = 20;
                int bytes = num;
                if (flag)
                {
                    for (int j = 0; j < files.Length; j++)
                    {
                        bytes += (files[j].Length + 1) * 2;
                    }
                    bytes += 2;
                }
                else
                {
                    for (int k = 0; k < files.Length; k++)
                    {
                        bytes += System.Windows.Forms.NativeMethods.Util.GetPInvokeStringLength(files[k]) + 1;
                    }
                    bytes++;
                }
                IntPtr ptr2 = System.Windows.Forms.UnsafeNativeMethods.GlobalReAlloc(new HandleRef(null, handle), bytes, 0x2002);
                if (ptr2 == IntPtr.Zero)
                {
                    return -2147024882;
                }
                IntPtr ptr3 = System.Windows.Forms.UnsafeNativeMethods.GlobalLock(new HandleRef(null, ptr2));
                if (ptr3 == IntPtr.Zero)
                {
                    return -2147024882;
                }
                zero = ptr3;
                int[] numArray2 = new int[5];
                numArray2[0] = num;
                int[] source = numArray2;
                if (flag)
                {
                    source[4] = -1;
                }
                Marshal.Copy(source, 0, zero, source.Length);
                zero = (IntPtr) (((long) zero) + num);
                for (int i = 0; i < files.Length; i++)
                {
                    if (flag)
                    {
                        System.Windows.Forms.UnsafeNativeMethods.CopyMemoryW(zero, files[i], files[i].Length * 2);
                        zero = (IntPtr) (((long) zero) + (files[i].Length * 2));
                        byte[] buffer = new byte[2];
                        Marshal.Copy(buffer, 0, zero, 2);
                        zero = (IntPtr) (((long) zero) + 2L);
                    }
                    else
                    {
                        int pInvokeStringLength = System.Windows.Forms.NativeMethods.Util.GetPInvokeStringLength(files[i]);
                        System.Windows.Forms.UnsafeNativeMethods.CopyMemoryA(zero, files[i], pInvokeStringLength);
                        zero = (IntPtr) (((long) zero) + pInvokeStringLength);
                        byte[] buffer2 = new byte[1];
                        Marshal.Copy(buffer2, 0, zero, 1);
                        zero = (IntPtr) (((long) zero) + 1L);
                    }
                }
                if (flag)
                {
                    char[] chArray = new char[1];
                    Marshal.Copy(chArray, 0, zero, 1);
                    zero = (IntPtr) (((long) zero) + 2L);
                }
                else
                {
                    byte[] buffer3 = new byte[1];
                    Marshal.Copy(buffer3, 0, zero, 1);
                    zero = (IntPtr) (((long) zero) + 1L);
                }
                System.Windows.Forms.UnsafeNativeMethods.GlobalUnlock(new HandleRef(null, ptr2));
            }
            return 0;
        }

        private int SaveObjectToHandle(ref IntPtr handle, object data)
        {
            Stream output = new MemoryStream();
            new BinaryWriter(output).Write(serializedObjectID);
            SaveObjectToHandleSerializer(output, data);
            return this.SaveStreamToHandle(ref handle, output);
        }

        [SecurityPermission(SecurityAction.Assert, Flags=SecurityPermissionFlag.SerializationFormatter)]
        private static void SaveObjectToHandleSerializer(Stream stream, object data)
        {
            new BinaryFormatter().Serialize(stream, data);
        }

        private int SaveStreamToHandle(ref IntPtr handle, Stream stream)
        {
            if (handle != IntPtr.Zero)
            {
                System.Windows.Forms.UnsafeNativeMethods.GlobalFree(new HandleRef(null, handle));
            }
            int length = (int) stream.Length;
            handle = System.Windows.Forms.UnsafeNativeMethods.GlobalAlloc(0x2002, length);
            if (handle == IntPtr.Zero)
            {
                return -2147024882;
            }
            IntPtr destination = System.Windows.Forms.UnsafeNativeMethods.GlobalLock(new HandleRef(null, handle));
            if (destination == IntPtr.Zero)
            {
                return -2147024882;
            }
            try
            {
                byte[] buffer = new byte[length];
                stream.Position = 0L;
                stream.Read(buffer, 0, length);
                Marshal.Copy(buffer, 0, destination, length);
            }
            finally
            {
                System.Windows.Forms.UnsafeNativeMethods.GlobalUnlock(new HandleRef(null, handle));
            }
            return 0;
        }

        private int SaveStringToHandle(IntPtr handle, string str, bool unicode)
        {
            if (handle == IntPtr.Zero)
            {
                return -2147024809;
            }
            IntPtr zero = IntPtr.Zero;
            if (unicode)
            {
                int bytes = (str.Length * 2) + 2;
                zero = System.Windows.Forms.UnsafeNativeMethods.GlobalReAlloc(new HandleRef(null, handle), bytes, 0x2042);
                if (zero == IntPtr.Zero)
                {
                    return -2147024882;
                }
                IntPtr pdst = System.Windows.Forms.UnsafeNativeMethods.GlobalLock(new HandleRef(null, zero));
                if (pdst == IntPtr.Zero)
                {
                    return -2147024882;
                }
                char[] psrc = str.ToCharArray(0, str.Length);
                System.Windows.Forms.UnsafeNativeMethods.CopyMemoryW(pdst, psrc, psrc.Length * 2);
            }
            else
            {
                int cb = System.Windows.Forms.UnsafeNativeMethods.WideCharToMultiByte(0, 0, str, str.Length, null, 0, IntPtr.Zero, IntPtr.Zero);
                byte[] pOutBytes = new byte[cb];
                System.Windows.Forms.UnsafeNativeMethods.WideCharToMultiByte(0, 0, str, str.Length, pOutBytes, pOutBytes.Length, IntPtr.Zero, IntPtr.Zero);
                zero = System.Windows.Forms.UnsafeNativeMethods.GlobalReAlloc(new HandleRef(null, handle), cb + 1, 0x2042);
                if (zero == IntPtr.Zero)
                {
                    return -2147024882;
                }
                IntPtr ptr3 = System.Windows.Forms.UnsafeNativeMethods.GlobalLock(new HandleRef(null, zero));
                if (ptr3 == IntPtr.Zero)
                {
                    return -2147024882;
                }
                System.Windows.Forms.UnsafeNativeMethods.CopyMemory(ptr3, pOutBytes, cb);
                byte[] source = new byte[1];
                Marshal.Copy(source, 0, (IntPtr) (((long) ptr3) + cb), 1);
            }
            if (zero != IntPtr.Zero)
            {
                System.Windows.Forms.UnsafeNativeMethods.GlobalUnlock(new HandleRef(null, zero));
            }
            return 0;
        }

        public virtual void SetAudio(byte[] audioBytes)
        {
            if (audioBytes == null)
            {
                throw new ArgumentNullException("audioBytes");
            }
            this.SetAudio(new MemoryStream(audioBytes));
        }

        public virtual void SetAudio(Stream audioStream)
        {
            if (audioStream == null)
            {
                throw new ArgumentNullException("audioStream");
            }
            this.SetData(DataFormats.WaveAudio, false, audioStream);
        }

        public virtual void SetData(object data)
        {
            this.innerData.SetData(data);
        }

        public virtual void SetData(string format, object data)
        {
            this.innerData.SetData(format, data);
        }

        public virtual void SetData(System.Type format, object data)
        {
            this.innerData.SetData(format, data);
        }

        public virtual void SetData(string format, bool autoConvert, object data)
        {
            this.innerData.SetData(format, autoConvert, data);
        }

        public virtual void SetFileDropList(StringCollection filePaths)
        {
            if (filePaths == null)
            {
                throw new ArgumentNullException("filePaths");
            }
            string[] array = new string[filePaths.Count];
            filePaths.CopyTo(array, 0);
            this.SetData(DataFormats.FileDrop, true, array);
        }

        public virtual void SetImage(Image image)
        {
            if (image == null)
            {
                throw new ArgumentNullException("image");
            }
            this.SetData(DataFormats.Bitmap, true, image);
        }

        public virtual void SetText(string textData)
        {
            this.SetText(textData, TextDataFormat.UnicodeText);
        }

        public virtual void SetText(string textData, TextDataFormat format)
        {
            if (string.IsNullOrEmpty(textData))
            {
                throw new ArgumentNullException("textData");
            }
            if (!System.Windows.Forms.ClientUtils.IsEnumValid(format, (int) format, 0, 4))
            {
                throw new InvalidEnumArgumentException("format", (int) format, typeof(TextDataFormat));
            }
            this.SetData(ConvertToDataFormats(format), false, textData);
        }

        [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        int System.Runtime.InteropServices.ComTypes.IDataObject.DAdvise(ref FORMATETC pFormatetc, ADVF advf, IAdviseSink pAdvSink, out int pdwConnection)
        {
            if (this.innerData is OleConverter)
            {
                return ((OleConverter) this.innerData).OleDataObject.DAdvise(ref pFormatetc, advf, pAdvSink, out pdwConnection);
            }
            pdwConnection = 0;
            return -2147467263;
        }

        [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        void System.Runtime.InteropServices.ComTypes.IDataObject.DUnadvise(int dwConnection)
        {
            if (this.innerData is OleConverter)
            {
                ((OleConverter) this.innerData).OleDataObject.DUnadvise(dwConnection);
            }
            else
            {
                Marshal.ThrowExceptionForHR(-2147467263);
            }
        }

        [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        int System.Runtime.InteropServices.ComTypes.IDataObject.EnumDAdvise(out IEnumSTATDATA enumAdvise)
        {
            if (this.innerData is OleConverter)
            {
                return ((OleConverter) this.innerData).OleDataObject.EnumDAdvise(out enumAdvise);
            }
            enumAdvise = null;
            return -2147221501;
        }

        [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        IEnumFORMATETC System.Runtime.InteropServices.ComTypes.IDataObject.EnumFormatEtc(DATADIR dwDirection)
        {
            if (this.innerData is OleConverter)
            {
                return ((OleConverter) this.innerData).OleDataObject.EnumFormatEtc(dwDirection);
            }
            if (dwDirection != DATADIR.DATADIR_GET)
            {
                throw new ExternalException(System.Windows.Forms.SR.GetString("ExternalException"), -2147467263);
            }
            return new FormatEnumerator(this);
        }

        [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        int System.Runtime.InteropServices.ComTypes.IDataObject.GetCanonicalFormatEtc(ref FORMATETC pformatetcIn, out FORMATETC pformatetcOut)
        {
            if (this.innerData is OleConverter)
            {
                return ((OleConverter) this.innerData).OleDataObject.GetCanonicalFormatEtc(ref pformatetcIn, out pformatetcOut);
            }
            pformatetcOut = new FORMATETC();
            return 0x40130;
        }

        [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        void System.Runtime.InteropServices.ComTypes.IDataObject.GetData(ref FORMATETC formatetc, out STGMEDIUM medium)
        {
            if (this.innerData is OleConverter)
            {
                ((OleConverter) this.innerData).OleDataObject.GetData(ref formatetc, out medium);
            }
            else
            {
                medium = new STGMEDIUM();
                if (this.GetTymedUseable(formatetc.tymed))
                {
                    if ((formatetc.tymed & TYMED.TYMED_HGLOBAL) != TYMED.TYMED_NULL)
                    {
                        medium.tymed = TYMED.TYMED_HGLOBAL;
                        medium.unionmember = System.Windows.Forms.UnsafeNativeMethods.GlobalAlloc(0x2042, 1);
                        if (medium.unionmember == IntPtr.Zero)
                        {
                            throw new OutOfMemoryException();
                        }
                        try
                        {
                            ((System.Runtime.InteropServices.ComTypes.IDataObject) this).GetDataHere(ref formatetc, ref medium);
                            return;
                        }
                        catch
                        {
                            System.Windows.Forms.UnsafeNativeMethods.GlobalFree(new HandleRef((STGMEDIUM) medium, medium.unionmember));
                            medium.unionmember = IntPtr.Zero;
                            throw;
                        }
                    }
                    medium.tymed = formatetc.tymed;
                    ((System.Runtime.InteropServices.ComTypes.IDataObject) this).GetDataHere(ref formatetc, ref medium);
                }
                else
                {
                    Marshal.ThrowExceptionForHR(-2147221399);
                }
            }
        }

        [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        void System.Runtime.InteropServices.ComTypes.IDataObject.GetDataHere(ref FORMATETC formatetc, ref STGMEDIUM medium)
        {
            if (this.innerData is OleConverter)
            {
                ((OleConverter) this.innerData).OleDataObject.GetDataHere(ref formatetc, ref medium);
            }
            else
            {
                this.GetDataIntoOleStructs(ref formatetc, ref medium);
            }
        }

        [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        int System.Runtime.InteropServices.ComTypes.IDataObject.QueryGetData(ref FORMATETC formatetc)
        {
            if (this.innerData is OleConverter)
            {
                return ((OleConverter) this.innerData).OleDataObject.QueryGetData(ref formatetc);
            }
            if (formatetc.dwAspect != DVASPECT.DVASPECT_CONTENT)
            {
                return -2147221397;
            }
            if (!this.GetTymedUseable(formatetc.tymed))
            {
                return -2147221399;
            }
            if (formatetc.cfFormat == 0)
            {
                return 1;
            }
            if (this.GetDataPresent(DataFormats.GetFormat(formatetc.cfFormat).Name))
            {
                return 0;
            }
            return -2147221404;
        }

        [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        void System.Runtime.InteropServices.ComTypes.IDataObject.SetData(ref FORMATETC pFormatetcIn, ref STGMEDIUM pmedium, bool fRelease)
        {
            if (!(this.innerData is OleConverter))
            {
                throw new NotImplementedException();
            }
            ((OleConverter) this.innerData).OleDataObject.SetData(ref pFormatetcIn, ref pmedium, fRelease);
        }

        internal bool RestrictedFormats { get; set; }

        private class DataStore : System.Windows.Forms.IDataObject
        {
            private Hashtable data = new Hashtable(System.Collections.Specialized.BackCompatibleStringComparer.Default);

            public virtual object GetData(string format)
            {
                return this.GetData(format, true);
            }

            public virtual object GetData(System.Type format)
            {
                return this.GetData(format.FullName);
            }

            public virtual object GetData(string format, bool autoConvert)
            {
                DataStoreEntry entry = (DataStoreEntry) this.data[format];
                object data = null;
                if (entry != null)
                {
                    data = entry.data;
                }
                object obj3 = data;
                if ((autoConvert && ((entry == null) || entry.autoConvert)) && ((data == null) || (data is MemoryStream)))
                {
                    string[] mappedFormats = DataObject.GetMappedFormats(format);
                    if (mappedFormats != null)
                    {
                        for (int i = 0; i < mappedFormats.Length; i++)
                        {
                            if (!format.Equals(mappedFormats[i]))
                            {
                                DataStoreEntry entry2 = (DataStoreEntry) this.data[mappedFormats[i]];
                                if (entry2 != null)
                                {
                                    data = entry2.data;
                                }
                                if ((data != null) && !(data is MemoryStream))
                                {
                                    obj3 = null;
                                    break;
                                }
                            }
                        }
                    }
                }
                if (obj3 != null)
                {
                    return obj3;
                }
                return data;
            }

            public virtual bool GetDataPresent(string format)
            {
                return this.GetDataPresent(format, true);
            }

            public virtual bool GetDataPresent(System.Type format)
            {
                return this.GetDataPresent(format.FullName);
            }

            public virtual bool GetDataPresent(string format, bool autoConvert)
            {
                if (!autoConvert)
                {
                    return this.data.ContainsKey(format);
                }
                string[] formats = this.GetFormats(autoConvert);
                for (int i = 0; i < formats.Length; i++)
                {
                    if (format.Equals(formats[i]))
                    {
                        return true;
                    }
                }
                return false;
            }

            public virtual string[] GetFormats()
            {
                return this.GetFormats(true);
            }

            public virtual string[] GetFormats(bool autoConvert)
            {
                string[] array = new string[this.data.Keys.Count];
                this.data.Keys.CopyTo(array, 0);
                if (!autoConvert)
                {
                    return array;
                }
                ArrayList list = new ArrayList();
                for (int i = 0; i < array.Length; i++)
                {
                    if (((DataStoreEntry) this.data[array[i]]).autoConvert)
                    {
                        string[] mappedFormats = DataObject.GetMappedFormats(array[i]);
                        for (int j = 0; j < mappedFormats.Length; j++)
                        {
                            list.Add(mappedFormats[j]);
                        }
                    }
                    else
                    {
                        list.Add(array[i]);
                    }
                }
                string[] strArray3 = new string[list.Count];
                list.CopyTo(strArray3, 0);
                return DataObject.GetDistinctStrings(strArray3);
            }

            public virtual void SetData(object data)
            {
                if ((data is ISerializable) && !this.data.ContainsKey(DataFormats.Serializable))
                {
                    this.SetData(DataFormats.Serializable, data);
                }
                this.SetData(data.GetType(), data);
            }

            public virtual void SetData(string format, object data)
            {
                this.SetData(format, true, data);
            }

            public virtual void SetData(System.Type format, object data)
            {
                this.SetData(format.FullName, data);
            }

            public virtual void SetData(string format, bool autoConvert, object data)
            {
                if ((data is Bitmap) && format.Equals(DataFormats.Dib))
                {
                    if (!autoConvert)
                    {
                        throw new NotSupportedException(System.Windows.Forms.SR.GetString("DataObjectDibNotSupported"));
                    }
                    format = DataFormats.Bitmap;
                }
                this.data[format] = new DataStoreEntry(data, autoConvert);
            }

            private class DataStoreEntry
            {
                public bool autoConvert;
                public object data;

                public DataStoreEntry(object data, bool autoConvert)
                {
                    this.data = data;
                    this.autoConvert = autoConvert;
                }
            }
        }

        private class FormatEnumerator : IEnumFORMATETC
        {
            internal int current;
            internal ArrayList formats;
            internal System.Windows.Forms.IDataObject parent;

            public FormatEnumerator(System.Windows.Forms.IDataObject parent) : this(parent, parent.GetFormats())
            {
            }

            public FormatEnumerator(System.Windows.Forms.IDataObject parent, FORMATETC[] formats)
            {
                this.formats = new ArrayList();
                this.formats.Clear();
                this.parent = parent;
                this.current = 0;
                if (formats != null)
                {
                    DataObject obj2 = parent as DataObject;
                    if (((obj2 != null) && obj2.RestrictedFormats) && !Clipboard.IsFormatValid(formats))
                    {
                        throw new SecurityException(System.Windows.Forms.SR.GetString("ClipboardSecurityException"));
                    }
                    for (int i = 0; i < formats.Length; i++)
                    {
                        FORMATETC formatetc = formats[i];
                        FORMATETC formatetc2 = new FORMATETC {
                            cfFormat = formatetc.cfFormat,
                            dwAspect = formatetc.dwAspect,
                            ptd = formatetc.ptd,
                            lindex = formatetc.lindex,
                            tymed = formatetc.tymed
                        };
                        this.formats.Add(formatetc2);
                    }
                }
            }

            public FormatEnumerator(System.Windows.Forms.IDataObject parent, string[] formats)
            {
                this.formats = new ArrayList();
                this.parent = parent;
                this.formats.Clear();
                string bitmap = DataFormats.Bitmap;
                string enhancedMetafile = DataFormats.EnhancedMetafile;
                string text = DataFormats.Text;
                string unicodeText = DataFormats.UnicodeText;
                string stringFormat = DataFormats.StringFormat;
                string str6 = DataFormats.StringFormat;
                if (formats != null)
                {
                    DataObject obj2 = parent as DataObject;
                    if (((obj2 != null) && obj2.RestrictedFormats) && !Clipboard.IsFormatValid(formats))
                    {
                        throw new SecurityException(System.Windows.Forms.SR.GetString("ClipboardSecurityException"));
                    }
                    for (int i = 0; i < formats.Length; i++)
                    {
                        string format = formats[i];
                        FORMATETC formatetc = new FORMATETC {
                            cfFormat = (short) DataFormats.GetFormat(format).Id,
                            dwAspect = DVASPECT.DVASPECT_CONTENT,
                            ptd = IntPtr.Zero,
                            lindex = -1
                        };
                        if (format.Equals(bitmap))
                        {
                            formatetc.tymed = TYMED.TYMED_GDI;
                        }
                        else if (format.Equals(enhancedMetafile))
                        {
                            formatetc.tymed = TYMED.TYMED_ENHMF;
                        }
                        else if (((format.Equals(text) || format.Equals(unicodeText)) || (format.Equals(stringFormat) || format.Equals(str6))) || ((format.Equals(DataFormats.Rtf) || format.Equals(DataFormats.CommaSeparatedValue)) || ((format.Equals(DataFormats.FileDrop) || format.Equals(DataObject.CF_DEPRECATED_FILENAME)) || format.Equals(DataObject.CF_DEPRECATED_FILENAMEW))))
                        {
                            formatetc.tymed = TYMED.TYMED_HGLOBAL;
                        }
                        else
                        {
                            formatetc.tymed = TYMED.TYMED_HGLOBAL;
                        }
                        if (formatetc.tymed != TYMED.TYMED_NULL)
                        {
                            this.formats.Add(formatetc);
                        }
                    }
                }
            }

            public void Clone(out IEnumFORMATETC ppenum)
            {
                FORMATETC[] array = new FORMATETC[this.formats.Count];
                this.formats.CopyTo(array, 0);
                ppenum = new DataObject.FormatEnumerator(this.parent, array);
            }

            public int Next(int celt, FORMATETC[] rgelt, int[] pceltFetched)
            {
                if ((this.current < this.formats.Count) && (celt > 0))
                {
                    FORMATETC formatetc = (FORMATETC) this.formats[this.current];
                    rgelt[0].cfFormat = formatetc.cfFormat;
                    rgelt[0].tymed = formatetc.tymed;
                    rgelt[0].dwAspect = DVASPECT.DVASPECT_CONTENT;
                    rgelt[0].ptd = IntPtr.Zero;
                    rgelt[0].lindex = -1;
                    if (pceltFetched != null)
                    {
                        pceltFetched[0] = 1;
                    }
                    this.current++;
                    return 0;
                }
                if (pceltFetched != null)
                {
                    pceltFetched[0] = 0;
                }
                return 1;
            }

            public int Reset()
            {
                this.current = 0;
                return 0;
            }

            public int Skip(int celt)
            {
                if ((this.current + celt) >= this.formats.Count)
                {
                    return 1;
                }
                this.current += celt;
                return 0;
            }
        }

        private class OleConverter : System.Windows.Forms.IDataObject
        {
            internal System.Runtime.InteropServices.ComTypes.IDataObject innerData;

            public OleConverter(System.Runtime.InteropServices.ComTypes.IDataObject data)
            {
                this.innerData = data;
            }

            public virtual object GetData(string format)
            {
                return this.GetData(format, true);
            }

            public virtual object GetData(System.Type format)
            {
                return this.GetData(format.FullName);
            }

            public virtual object GetData(string format, bool autoConvert)
            {
                object dataFromBoundOleDataObject = this.GetDataFromBoundOleDataObject(format);
                object obj3 = dataFromBoundOleDataObject;
                if (autoConvert && ((dataFromBoundOleDataObject == null) || (dataFromBoundOleDataObject is MemoryStream)))
                {
                    string[] mappedFormats = DataObject.GetMappedFormats(format);
                    if (mappedFormats != null)
                    {
                        for (int i = 0; i < mappedFormats.Length; i++)
                        {
                            if (!format.Equals(mappedFormats[i]))
                            {
                                dataFromBoundOleDataObject = this.GetDataFromBoundOleDataObject(mappedFormats[i]);
                                if ((dataFromBoundOleDataObject != null) && !(dataFromBoundOleDataObject is MemoryStream))
                                {
                                    obj3 = null;
                                    break;
                                }
                            }
                        }
                    }
                }
                if (obj3 != null)
                {
                    return obj3;
                }
                return dataFromBoundOleDataObject;
            }

            private object GetDataFromBoundOleDataObject(string format)
            {
                object dataFromOleOther = null;
                try
                {
                    dataFromOleOther = this.GetDataFromOleOther(format);
                    if (dataFromOleOther == null)
                    {
                        dataFromOleOther = this.GetDataFromOleHGLOBAL(format);
                    }
                    if (dataFromOleOther == null)
                    {
                        dataFromOleOther = this.GetDataFromOleIStream(format);
                    }
                }
                catch (Exception)
                {
                }
                return dataFromOleOther;
            }

            private object GetDataFromHGLOBLAL(string format, IntPtr hglobal)
            {
                object obj2 = null;
                if (hglobal != IntPtr.Zero)
                {
                    if ((format.Equals(DataFormats.Text) || format.Equals(DataFormats.Rtf)) || (format.Equals(DataFormats.Html) || format.Equals(DataFormats.OemText)))
                    {
                        obj2 = this.ReadStringFromHandle(hglobal, false);
                    }
                    else if (format.Equals(DataFormats.UnicodeText))
                    {
                        obj2 = this.ReadStringFromHandle(hglobal, true);
                    }
                    else if (format.Equals(DataFormats.FileDrop))
                    {
                        obj2 = this.ReadFileListFromHandle(hglobal);
                    }
                    else if (format.Equals(DataObject.CF_DEPRECATED_FILENAME))
                    {
                        obj2 = new string[] { this.ReadStringFromHandle(hglobal, false) };
                    }
                    else if (format.Equals(DataObject.CF_DEPRECATED_FILENAMEW))
                    {
                        obj2 = new string[] { this.ReadStringFromHandle(hglobal, true) };
                    }
                    else
                    {
                        obj2 = this.ReadObjectFromHandle(hglobal);
                    }
                    System.Windows.Forms.UnsafeNativeMethods.GlobalFree(new HandleRef(null, hglobal));
                }
                return obj2;
            }

            private object GetDataFromOleHGLOBAL(string format)
            {
                FORMATETC formatetc = new FORMATETC();
                STGMEDIUM medium = new STGMEDIUM();
                formatetc.cfFormat = (short) DataFormats.GetFormat(format).Id;
                formatetc.dwAspect = DVASPECT.DVASPECT_CONTENT;
                formatetc.lindex = -1;
                formatetc.tymed = TYMED.TYMED_HGLOBAL;
                medium.tymed = TYMED.TYMED_HGLOBAL;
                object dataFromHGLOBLAL = null;
                if (this.QueryGetData(ref formatetc) == 0)
                {
                    try
                    {
                        System.Windows.Forms.IntSecurity.UnmanagedCode.Assert();
                        try
                        {
                            this.innerData.GetData(ref formatetc, out medium);
                        }
                        finally
                        {
                            CodeAccessPermission.RevertAssert();
                        }
                        if (medium.unionmember != IntPtr.Zero)
                        {
                            dataFromHGLOBLAL = this.GetDataFromHGLOBLAL(format, medium.unionmember);
                        }
                    }
                    catch
                    {
                    }
                }
                return dataFromHGLOBLAL;
            }

            private object GetDataFromOleIStream(string format)
            {
                FORMATETC formatetc = new FORMATETC();
                STGMEDIUM medium = new STGMEDIUM();
                formatetc.cfFormat = (short) DataFormats.GetFormat(format).Id;
                formatetc.dwAspect = DVASPECT.DVASPECT_CONTENT;
                formatetc.lindex = -1;
                formatetc.tymed = TYMED.TYMED_ISTREAM;
                medium.tymed = TYMED.TYMED_ISTREAM;
                if (this.QueryGetData(ref formatetc) == 0)
                {
                    try
                    {
                        System.Windows.Forms.IntSecurity.UnmanagedCode.Assert();
                        try
                        {
                            this.innerData.GetData(ref formatetc, out medium);
                        }
                        finally
                        {
                            CodeAccessPermission.RevertAssert();
                        }
                    }
                    catch
                    {
                        return null;
                    }
                    if (medium.unionmember != IntPtr.Zero)
                    {
                        System.Windows.Forms.UnsafeNativeMethods.IStream objectForIUnknown = (System.Windows.Forms.UnsafeNativeMethods.IStream) Marshal.GetObjectForIUnknown(medium.unionmember);
                        Marshal.Release(medium.unionmember);
                        System.Windows.Forms.NativeMethods.STATSTG pStatstg = new System.Windows.Forms.NativeMethods.STATSTG();
                        objectForIUnknown.Stat(pStatstg, 0);
                        int cbSize = (int) pStatstg.cbSize;
                        IntPtr handle = System.Windows.Forms.UnsafeNativeMethods.GlobalAlloc(0x2042, cbSize);
                        IntPtr buf = System.Windows.Forms.UnsafeNativeMethods.GlobalLock(new HandleRef(this.innerData, handle));
                        objectForIUnknown.Read(buf, cbSize);
                        System.Windows.Forms.UnsafeNativeMethods.GlobalUnlock(new HandleRef(this.innerData, handle));
                        return this.GetDataFromHGLOBLAL(format, handle);
                    }
                }
                return null;
            }

            private object GetDataFromOleOther(string format)
            {
                FORMATETC formatetc = new FORMATETC();
                STGMEDIUM medium = new STGMEDIUM();
                TYMED tymed = TYMED.TYMED_NULL;
                if (format.Equals(DataFormats.Bitmap))
                {
                    tymed = TYMED.TYMED_GDI;
                }
                else if (format.Equals(DataFormats.EnhancedMetafile))
                {
                    tymed = TYMED.TYMED_ENHMF;
                }
                if (tymed == TYMED.TYMED_NULL)
                {
                    return null;
                }
                formatetc.cfFormat = (short) DataFormats.GetFormat(format).Id;
                formatetc.dwAspect = DVASPECT.DVASPECT_CONTENT;
                formatetc.lindex = -1;
                formatetc.tymed = tymed;
                medium.tymed = tymed;
                object obj2 = null;
                if (this.QueryGetData(ref formatetc) == 0)
                {
                    try
                    {
                        System.Windows.Forms.IntSecurity.UnmanagedCode.Assert();
                        try
                        {
                            this.innerData.GetData(ref formatetc, out medium);
                        }
                        finally
                        {
                            CodeAccessPermission.RevertAssert();
                        }
                    }
                    catch
                    {
                    }
                }
                if (!(medium.unionmember != IntPtr.Zero) || !format.Equals(DataFormats.Bitmap))
                {
                    return obj2;
                }
                System.Internal.HandleCollector.Add(medium.unionmember, System.Windows.Forms.NativeMethods.CommonHandles.GDI);
                Image image = null;
                System.Windows.Forms.IntSecurity.ObjectFromWin32Handle.Assert();
                try
                {
                    image = Image.FromHbitmap(medium.unionmember);
                }
                finally
                {
                    CodeAccessPermission.RevertAssert();
                }
                if (image != null)
                {
                    Image image2 = image;
                    image = (Image) image.Clone();
                    System.Windows.Forms.SafeNativeMethods.DeleteObject(new HandleRef(null, medium.unionmember));
                    image2.Dispose();
                }
                return image;
            }

            public virtual bool GetDataPresent(string format)
            {
                return this.GetDataPresent(format, true);
            }

            public virtual bool GetDataPresent(System.Type format)
            {
                return this.GetDataPresent(format.FullName);
            }

            public virtual bool GetDataPresent(string format, bool autoConvert)
            {
                System.Windows.Forms.IntSecurity.ClipboardRead.Demand();
                bool dataPresentInner = false;
                System.Windows.Forms.IntSecurity.UnmanagedCode.Assert();
                try
                {
                    dataPresentInner = this.GetDataPresentInner(format);
                }
                finally
                {
                    CodeAccessPermission.RevertAssert();
                }
                if (!dataPresentInner && autoConvert)
                {
                    string[] mappedFormats = DataObject.GetMappedFormats(format);
                    if (mappedFormats == null)
                    {
                        return dataPresentInner;
                    }
                    for (int i = 0; i < mappedFormats.Length; i++)
                    {
                        if (!format.Equals(mappedFormats[i]))
                        {
                            System.Windows.Forms.IntSecurity.UnmanagedCode.Assert();
                            try
                            {
                                dataPresentInner = this.GetDataPresentInner(mappedFormats[i]);
                            }
                            finally
                            {
                                CodeAccessPermission.RevertAssert();
                            }
                            if (dataPresentInner)
                            {
                                return dataPresentInner;
                            }
                        }
                    }
                }
                return dataPresentInner;
            }

            private bool GetDataPresentInner(string format)
            {
                FORMATETC formatetc = new FORMATETC {
                    cfFormat = (short) DataFormats.GetFormat(format).Id,
                    dwAspect = DVASPECT.DVASPECT_CONTENT,
                    lindex = -1
                };
                for (int i = 0; i < DataObject.ALLOWED_TYMEDS.Length; i++)
                {
                    formatetc.tymed |= DataObject.ALLOWED_TYMEDS[i];
                }
                return (this.QueryGetData(ref formatetc) == 0);
            }

            public virtual string[] GetFormats()
            {
                return this.GetFormats(true);
            }

            public virtual string[] GetFormats(bool autoConvert)
            {
                IEnumFORMATETC mformatetc = null;
                ArrayList list = new ArrayList();
                try
                {
                    mformatetc = this.innerData.EnumFormatEtc(DATADIR.DATADIR_GET);
                }
                catch
                {
                }
                if (mformatetc != null)
                {
                    mformatetc.Reset();
                    FORMATETC[] rgelt = new FORMATETC[] { new FORMATETC() };
                    int[] pceltFetched = new int[] { 1 };
                    while (pceltFetched[0] > 0)
                    {
                        pceltFetched[0] = 0;
                        try
                        {
                            mformatetc.Next(1, rgelt, pceltFetched);
                        }
                        catch
                        {
                        }
                        if (pceltFetched[0] > 0)
                        {
                            string name = DataFormats.GetFormat(rgelt[0].cfFormat).Name;
                            if (autoConvert)
                            {
                                string[] mappedFormats = DataObject.GetMappedFormats(name);
                                for (int i = 0; i < mappedFormats.Length; i++)
                                {
                                    list.Add(mappedFormats[i]);
                                }
                            }
                            else
                            {
                                list.Add(name);
                            }
                        }
                    }
                }
                string[] array = new string[list.Count];
                list.CopyTo(array, 0);
                return DataObject.GetDistinctStrings(array);
            }

            private int QueryGetData(ref FORMATETC formatetc)
            {
                int num;
                System.Windows.Forms.IntSecurity.UnmanagedCode.Assert();
                try
                {
                    num = this.QueryGetDataInner(ref formatetc);
                }
                finally
                {
                    CodeAccessPermission.RevertAssert();
                }
                return num;
            }

            private int QueryGetDataInner(ref FORMATETC formatetc)
            {
                return this.innerData.QueryGetData(ref formatetc);
            }

            private Stream ReadByteStreamFromHandle(IntPtr handle, out bool isSerializedObject)
            {
                Stream stream;
                IntPtr source = System.Windows.Forms.UnsafeNativeMethods.GlobalLock(new HandleRef(null, handle));
                if (source == IntPtr.Zero)
                {
                    throw new ExternalException(System.Windows.Forms.SR.GetString("ExternalException"), -2147024882);
                }
                try
                {
                    int length = System.Windows.Forms.UnsafeNativeMethods.GlobalSize(new HandleRef(null, handle));
                    byte[] destination = new byte[length];
                    Marshal.Copy(source, destination, 0, length);
                    int index = 0;
                    if (length > DataObject.serializedObjectID.Length)
                    {
                        isSerializedObject = true;
                        for (int i = 0; i < DataObject.serializedObjectID.Length; i++)
                        {
                            if (DataObject.serializedObjectID[i] != destination[i])
                            {
                                isSerializedObject = false;
                                break;
                            }
                        }
                        if (isSerializedObject)
                        {
                            index = DataObject.serializedObjectID.Length;
                        }
                    }
                    else
                    {
                        isSerializedObject = false;
                    }
                    stream = new MemoryStream(destination, index, destination.Length - index);
                }
                finally
                {
                    System.Windows.Forms.UnsafeNativeMethods.GlobalUnlock(new HandleRef(null, handle));
                }
                return stream;
            }

            private string[] ReadFileListFromHandle(IntPtr hdrop)
            {
                string[] strArray = null;
                StringBuilder lpszFile = new StringBuilder(260);
                int num = System.Windows.Forms.UnsafeNativeMethods.DragQueryFile(new HandleRef(null, hdrop), -1, null, 0);
                if (num > 0)
                {
                    strArray = new string[num];
                    for (int i = 0; i < num; i++)
                    {
                        int length = System.Windows.Forms.UnsafeNativeMethods.DragQueryFile(new HandleRef(null, hdrop), i, lpszFile, lpszFile.Capacity);
                        string path = lpszFile.ToString();
                        if (path.Length > length)
                        {
                            path = path.Substring(0, length);
                        }
                        string fullPath = Path.GetFullPath(path);
                        new FileIOPermission(FileIOPermissionAccess.PathDiscovery, fullPath).Demand();
                        strArray[i] = path;
                    }
                }
                return strArray;
            }

            private object ReadObjectFromHandle(IntPtr handle)
            {
                bool flag;
                Stream stream = this.ReadByteStreamFromHandle(handle, out flag);
                if (flag)
                {
                    return ReadObjectFromHandleDeserializer(stream);
                }
                return stream;
            }

            private static object ReadObjectFromHandleDeserializer(Stream stream)
            {
                BinaryFormatter formatter = new BinaryFormatter {
                    AssemblyFormat = FormatterAssemblyStyle.Simple
                };
                return formatter.Deserialize(stream);
            }

            private unsafe string ReadStringFromHandle(IntPtr handle, bool unicode)
            {
                string str = null;
                IntPtr ptr = System.Windows.Forms.UnsafeNativeMethods.GlobalLock(new HandleRef(null, handle));
                try
                {
                    if (unicode)
                    {
                        return new string((char*) ptr);
                    }
                    str = new string((sbyte*) ptr);
                }
                finally
                {
                    System.Windows.Forms.UnsafeNativeMethods.GlobalUnlock(new HandleRef(null, handle));
                }
                return str;
            }

            public virtual void SetData(object data)
            {
                if (data is ISerializable)
                {
                    this.SetData(DataFormats.Serializable, data);
                }
                else
                {
                    this.SetData(data.GetType(), data);
                }
            }

            public virtual void SetData(string format, object data)
            {
                this.SetData(format, true, data);
            }

            public virtual void SetData(System.Type format, object data)
            {
                this.SetData(format.FullName, data);
            }

            public virtual void SetData(string format, bool autoConvert, object data)
            {
            }

            public System.Runtime.InteropServices.ComTypes.IDataObject OleDataObject
            {
                get
                {
                    return this.innerData;
                }
            }
        }
    }
}

