namespace System.Windows.Forms
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    [Serializable]
    public sealed class ImageListStreamer : ISerializable, IDisposable
    {
        private static readonly byte[] HEADER_MAGIC = new byte[] { 0x4d, 0x53, 70, 0x74 };
        private ImageList imageList;
        private static object internalSyncObject = new object();
        private ImageList.NativeImageList nativeImageList;

        internal ImageListStreamer(ImageList il)
        {
            this.imageList = il;
        }

        private ImageListStreamer(SerializationInfo info, StreamingContext context)
        {
            SerializationInfoEnumerator enumerator = info.GetEnumerator();
            if (enumerator != null)
            {
                while (enumerator.MoveNext())
                {
                    if (string.Equals(enumerator.Name, "Data", StringComparison.OrdinalIgnoreCase))
                    {
                        byte[] input = (byte[]) enumerator.Value;
                        if (input != null)
                        {
                            IntPtr userCookie = System.Windows.Forms.UnsafeNativeMethods.ThemingScope.Activate();
                            try
                            {
                                MemoryStream dataStream = new MemoryStream(this.Decompress(input));
                                lock (internalSyncObject)
                                {
                                    SafeNativeMethods.InitCommonControls();
                                    this.nativeImageList = new ImageList.NativeImageList(SafeNativeMethods.ImageList_Read(new System.Windows.Forms.UnsafeNativeMethods.ComStreamFromDataStream(dataStream)));
                                }
                            }
                            finally
                            {
                                System.Windows.Forms.UnsafeNativeMethods.ThemingScope.Deactivate(userCookie);
                            }
                            if (this.nativeImageList.Handle == IntPtr.Zero)
                            {
                                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("ImageListStreamerLoadFailed"));
                            }
                        }
                    }
                }
            }
        }

        private byte[] Compress(byte[] input)
        {
            int num = 0;
            int index = 0;
            int num3 = 0;
            while (index < input.Length)
            {
                byte num4 = input[index++];
                byte num5 = 1;
                while (((index < input.Length) && (input[index] == num4)) && (num5 < 0xff))
                {
                    num5 = (byte) (num5 + 1);
                    index++;
                }
                num += 2;
            }
            byte[] dst = new byte[num + HEADER_MAGIC.Length];
            Buffer.BlockCopy(HEADER_MAGIC, 0, dst, 0, HEADER_MAGIC.Length);
            int length = HEADER_MAGIC.Length;
            index = 0;
            while (index < input.Length)
            {
                byte num7 = input[index++];
                byte num8 = 1;
                while (((index < input.Length) && (input[index] == num7)) && (num8 < 0xff))
                {
                    num8 = (byte) (num8 + 1);
                    index++;
                }
                dst[length + num3++] = num8;
                dst[length + num3++] = num7;
            }
            return dst;
        }

        private byte[] Decompress(byte[] input)
        {
            int num = 0;
            int index = 0;
            int num3 = 0;
            if (input.Length < HEADER_MAGIC.Length)
            {
                return input;
            }
            for (index = 0; index < HEADER_MAGIC.Length; index++)
            {
                if (input[index] != HEADER_MAGIC[index])
                {
                    return input;
                }
            }
            for (index = HEADER_MAGIC.Length; index < input.Length; index += 2)
            {
                num += input[index];
            }
            byte[] buffer = new byte[num];
            index = HEADER_MAGIC.Length;
            while (index < input.Length)
            {
                byte num4 = input[index++];
                byte num5 = input[index++];
                int num6 = num3;
                int num7 = num3 + num4;
                while (num6 < num7)
                {
                    buffer[num6++] = num5;
                }
                num3 += num4;
            }
            return buffer;
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing && (this.nativeImageList != null))
            {
                this.nativeImageList.Dispose();
                this.nativeImageList = null;
            }
        }

        internal ImageList.NativeImageList GetNativeImageList()
        {
            return this.nativeImageList;
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.SerializationFormatter)]
        public void GetObjectData(SerializationInfo si, StreamingContext context)
        {
            MemoryStream stream = new MemoryStream();
            IntPtr zero = IntPtr.Zero;
            if (this.imageList != null)
            {
                zero = this.imageList.Handle;
            }
            else if (this.nativeImageList != null)
            {
                zero = this.nativeImageList.Handle;
            }
            if ((zero == IntPtr.Zero) || !this.WriteImageList(zero, stream))
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("ImageListStreamerSaveFailed"));
            }
            si.AddValue("Data", this.Compress(stream.ToArray()));
        }

        private bool WriteImageList(IntPtr imagelistHandle, Stream stream)
        {
            try
            {
                return (SafeNativeMethods.ImageList_WriteEx(new HandleRef(this, imagelistHandle), 1, new System.Windows.Forms.UnsafeNativeMethods.ComStreamFromDataStream(stream)) == 0);
            }
            catch (EntryPointNotFoundException)
            {
            }
            return SafeNativeMethods.ImageList_Write(new HandleRef(this, imagelistHandle), new System.Windows.Forms.UnsafeNativeMethods.ComStreamFromDataStream(stream));
        }
    }
}

