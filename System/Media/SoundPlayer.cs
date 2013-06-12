namespace System.Media
{
    using System;
    using System.ComponentModel;
    using System.IO;
    using System.Net;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Security.Permissions;
    using System.Threading;

    [Serializable, ToolboxItem(false), HostProtection(SecurityAction.LinkDemand, UI=true)]
    public class SoundPlayer : Component, ISerializable
    {
        private AsyncOperation asyncOperation;
        private const int blockSize = 0x400;
        private Thread copyThread;
        private int currentPos;
        private const int defaultLoadTimeout = 0x2710;
        private bool doesLoadAppearSynchronous;
        private static readonly object EventLoadCompleted = new object();
        private static readonly object EventSoundLocationChanged = new object();
        private static readonly object EventStreamChanged = new object();
        private bool isLoadCompleted;
        private Exception lastLoadException;
        private readonly SendOrPostCallback loadAsyncOperationCompleted;
        private int loadTimeout;
        private ManualResetEvent semaphore;
        private string soundLocation;
        private System.IO.Stream stream;
        private byte[] streamData;
        private object tag;
        private Uri uri;

        public event AsyncCompletedEventHandler LoadCompleted
        {
            add
            {
                base.Events.AddHandler(EventLoadCompleted, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventLoadCompleted, value);
            }
        }

        public event EventHandler SoundLocationChanged
        {
            add
            {
                base.Events.AddHandler(EventSoundLocationChanged, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventSoundLocationChanged, value);
            }
        }

        public event EventHandler StreamChanged
        {
            add
            {
                base.Events.AddHandler(EventStreamChanged, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventStreamChanged, value);
            }
        }

        public SoundPlayer()
        {
            this.soundLocation = string.Empty;
            this.loadTimeout = 0x2710;
            this.semaphore = new ManualResetEvent(true);
            this.loadAsyncOperationCompleted = new SendOrPostCallback(this.LoadAsyncOperationCompleted);
        }

        public SoundPlayer(System.IO.Stream stream) : this()
        {
            this.stream = stream;
        }

        public SoundPlayer(string soundLocation) : this()
        {
            if (soundLocation == null)
            {
                soundLocation = string.Empty;
            }
            this.SetupSoundLocation(soundLocation);
        }

        protected SoundPlayer(SerializationInfo serializationInfo, StreamingContext context)
        {
            this.soundLocation = string.Empty;
            this.loadTimeout = 0x2710;
            this.semaphore = new ManualResetEvent(true);
            SerializationInfoEnumerator enumerator = serializationInfo.GetEnumerator();
            while (enumerator.MoveNext())
            {
                SerializationEntry current = enumerator.Current;
                string name = current.Name;
                if (name != null)
                {
                    if (!(name == "SoundLocation"))
                    {
                        if (name == "Stream")
                        {
                            goto Label_0083;
                        }
                        if (name == "LoadTimeout")
                        {
                            goto Label_00B3;
                        }
                    }
                    else
                    {
                        this.SetupSoundLocation((string) current.Value);
                    }
                }
                continue;
            Label_0083:
                this.stream = (System.IO.Stream) current.Value;
                if (this.stream.CanSeek)
                {
                    this.stream.Seek(0L, SeekOrigin.Begin);
                }
                continue;
            Label_00B3:
                this.LoadTimeout = (int) current.Value;
            }
        }

        private static int BytesToInt(byte ch0, byte ch1, byte ch2, byte ch3)
        {
            return mmioFOURCC((char) ch3, (char) ch2, (char) ch1, (char) ch0);
        }

        private static short BytesToInt16(byte ch0, byte ch1)
        {
            int num = ch1;
            num |= ch0 << 8;
            return (short) num;
        }

        private void CleanupStreamData()
        {
            this.currentPos = 0;
            this.streamData = null;
            this.isLoadCompleted = false;
            this.lastLoadException = null;
            this.doesLoadAppearSynchronous = false;
            this.copyThread = null;
            this.semaphore.Set();
        }

        public void Load()
        {
            if ((this.uri != null) && this.uri.IsFile)
            {
                FileInfo info = new FileInfo(this.uri.LocalPath);
                if (!info.Exists)
                {
                    throw new FileNotFoundException(SR.GetString("SoundAPIFileDoesNotExist"), this.soundLocation);
                }
                this.isLoadCompleted = true;
                this.OnLoadCompleted(new AsyncCompletedEventArgs(null, false, null));
            }
            else
            {
                this.LoadSync();
            }
        }

        private void LoadAndPlay(int flags)
        {
            if (string.IsNullOrEmpty(this.soundLocation) && (this.stream == null))
            {
                SystemSounds.Beep.Play();
            }
            else if ((this.uri != null) && this.uri.IsFile)
            {
                string localPath = this.uri.LocalPath;
                new FileIOPermission(FileIOPermissionAccess.Read, localPath).Demand();
                this.isLoadCompleted = true;
                IntSecurity.SafeSubWindows.Demand();
                System.ComponentModel.IntSecurity.UnmanagedCode.Assert();
                try
                {
                    this.ValidateSoundFile(localPath);
                    UnsafeNativeMethods.PlaySound(localPath, IntPtr.Zero, 2 | flags);
                }
                finally
                {
                    CodeAccessPermission.RevertAssert();
                }
            }
            else
            {
                this.LoadSync();
                ValidateSoundData(this.streamData);
                IntSecurity.SafeSubWindows.Demand();
                System.ComponentModel.IntSecurity.UnmanagedCode.Assert();
                try
                {
                    UnsafeNativeMethods.PlaySound(this.streamData, IntPtr.Zero, 6 | flags);
                }
                finally
                {
                    CodeAccessPermission.RevertAssert();
                }
            }
        }

        public void LoadAsync()
        {
            if ((this.uri != null) && this.uri.IsFile)
            {
                this.isLoadCompleted = true;
                FileInfo info = new FileInfo(this.uri.LocalPath);
                if (!info.Exists)
                {
                    throw new FileNotFoundException(SR.GetString("SoundAPIFileDoesNotExist"), this.soundLocation);
                }
                this.OnLoadCompleted(new AsyncCompletedEventArgs(null, false, null));
            }
            else if ((this.copyThread == null) || (this.copyThread.ThreadState != ThreadState.Running))
            {
                this.isLoadCompleted = false;
                this.streamData = null;
                this.currentPos = 0;
                this.asyncOperation = AsyncOperationManager.CreateOperation(null);
                this.LoadStream(false);
            }
        }

        private void LoadAsyncOperationCompleted(object arg)
        {
            this.OnLoadCompleted((AsyncCompletedEventArgs) arg);
        }

        private void LoadStream(bool loadSync)
        {
            if (loadSync && this.stream.CanSeek)
            {
                int length = (int) this.stream.Length;
                this.currentPos = 0;
                this.streamData = new byte[length];
                this.stream.Read(this.streamData, 0, length);
                this.isLoadCompleted = true;
                this.OnLoadCompleted(new AsyncCompletedEventArgs(null, false, null));
            }
            else
            {
                this.semaphore.Reset();
                this.copyThread = new Thread(new ThreadStart(this.WorkerThread));
                this.copyThread.Start();
            }
        }

        private void LoadSync()
        {
            if (!this.semaphore.WaitOne(this.LoadTimeout, false))
            {
                if (this.copyThread != null)
                {
                    this.copyThread.Abort();
                }
                this.CleanupStreamData();
                throw new TimeoutException(SR.GetString("SoundAPILoadTimedOut"));
            }
            if (this.streamData == null)
            {
                if (((this.uri != null) && !this.uri.IsFile) && (this.stream == null))
                {
                    new WebPermission(NetworkAccess.Connect, this.uri.AbsolutePath).Demand();
                    WebRequest request = WebRequest.Create(this.uri);
                    request.Timeout = this.LoadTimeout;
                    this.stream = request.GetResponse().GetResponseStream();
                }
                if (this.stream.CanSeek)
                {
                    this.LoadStream(true);
                }
                else
                {
                    this.doesLoadAppearSynchronous = true;
                    this.LoadStream(false);
                    if (!this.semaphore.WaitOne(this.LoadTimeout, false))
                    {
                        if (this.copyThread != null)
                        {
                            this.copyThread.Abort();
                        }
                        this.CleanupStreamData();
                        throw new TimeoutException(SR.GetString("SoundAPILoadTimedOut"));
                    }
                    this.doesLoadAppearSynchronous = false;
                    if (this.lastLoadException != null)
                    {
                        throw this.lastLoadException;
                    }
                }
                this.copyThread = null;
            }
        }

        private static int mmioFOURCC(char ch0, char ch1, char ch2, char ch3)
        {
            int num = 0;
            num |= ch0;
            num |= ch1 << 8;
            num |= ch2 << 0x10;
            return (num | (ch3 << 0x18));
        }

        protected virtual void OnLoadCompleted(AsyncCompletedEventArgs e)
        {
            AsyncCompletedEventHandler handler = (AsyncCompletedEventHandler) base.Events[EventLoadCompleted];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnSoundLocationChanged(EventArgs e)
        {
            EventHandler handler = (EventHandler) base.Events[EventSoundLocationChanged];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnStreamChanged(EventArgs e)
        {
            EventHandler handler = (EventHandler) base.Events[EventStreamChanged];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public void Play()
        {
            this.LoadAndPlay(1);
        }

        public void PlayLooping()
        {
            this.LoadAndPlay(9);
        }

        public void PlaySync()
        {
            this.LoadAndPlay(0);
        }

        private static Uri ResolveUri(string partialUri)
        {
            Uri uri = null;
            try
            {
                uri = new Uri(partialUri);
            }
            catch (UriFormatException)
            {
            }
            if (uri == null)
            {
                try
                {
                    uri = new Uri(Path.GetFullPath(partialUri));
                }
                catch (UriFormatException)
                {
                }
            }
            return uri;
        }

        private void SetupSoundLocation(string soundLocation)
        {
            if (this.copyThread != null)
            {
                this.copyThread.Abort();
                this.CleanupStreamData();
            }
            this.uri = ResolveUri(soundLocation);
            this.soundLocation = soundLocation;
            this.stream = null;
            if (this.uri == null)
            {
                if (!string.IsNullOrEmpty(soundLocation))
                {
                    throw new UriFormatException(SR.GetString("SoundAPIBadSoundLocation"));
                }
            }
            else if (!this.uri.IsFile)
            {
                this.streamData = null;
                this.currentPos = 0;
                this.isLoadCompleted = false;
            }
        }

        private void SetupStream(System.IO.Stream stream)
        {
            if (this.copyThread != null)
            {
                this.copyThread.Abort();
                this.CleanupStreamData();
            }
            this.stream = stream;
            this.soundLocation = string.Empty;
            this.streamData = null;
            this.currentPos = 0;
            this.isLoadCompleted = false;
            if (stream != null)
            {
                this.uri = null;
            }
        }

        public void Stop()
        {
            IntSecurity.SafeSubWindows.Demand();
            UnsafeNativeMethods.PlaySound((byte[]) null, IntPtr.Zero, 0x40);
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.SerializationFormatter)]
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (!string.IsNullOrEmpty(this.soundLocation))
            {
                info.AddValue("SoundLocation", this.soundLocation);
            }
            if (this.stream != null)
            {
                info.AddValue("Stream", this.stream);
            }
            info.AddValue("LoadTimeout", this.loadTimeout);
        }

        private static void ValidateSoundData(byte[] data)
        {
            int index = 0;
            short num2 = -1;
            bool flag = false;
            if (data.Length < 12)
            {
                throw new InvalidOperationException(SR.GetString("SoundAPIInvalidWaveHeader"));
            }
            if (((data[0] != 0x52) || (data[1] != 0x49)) || ((data[2] != 70) || (data[3] != 70)))
            {
                throw new InvalidOperationException(SR.GetString("SoundAPIInvalidWaveHeader"));
            }
            if (((data[8] != 0x57) || (data[9] != 0x41)) || ((data[10] != 0x56) || (data[11] != 0x45)))
            {
                throw new InvalidOperationException(SR.GetString("SoundAPIInvalidWaveHeader"));
            }
            index = 12;
            int length = data.Length;
            while (!flag && (index < (length - 8)))
            {
                if (((data[index] == 0x66) && (data[index + 1] == 0x6d)) && ((data[index + 2] == 0x74) && (data[index + 3] == 0x20)))
                {
                    flag = true;
                    int num4 = BytesToInt(data[index + 7], data[index + 6], data[index + 5], data[index + 4]);
                    int num5 = 0x10;
                    if (num4 != num5)
                    {
                        int num6 = 0x12;
                        if (length < (((index + 8) + num6) - 1))
                        {
                            throw new InvalidOperationException(SR.GetString("SoundAPIInvalidWaveHeader"));
                        }
                        if ((BytesToInt16(data[((index + 8) + num6) - 1], data[((index + 8) + num6) - 2]) + num6) != num4)
                        {
                            throw new InvalidOperationException(SR.GetString("SoundAPIInvalidWaveHeader"));
                        }
                    }
                    if (length < (index + 9))
                    {
                        throw new InvalidOperationException(SR.GetString("SoundAPIInvalidWaveHeader"));
                    }
                    num2 = BytesToInt16(data[index + 9], data[index + 8]);
                    index += num4 + 8;
                }
                else
                {
                    index += 8 + BytesToInt(data[index + 7], data[index + 6], data[index + 5], data[index + 4]);
                }
            }
            if (!flag)
            {
                throw new InvalidOperationException(SR.GetString("SoundAPIInvalidWaveHeader"));
            }
            if (((num2 != 1) && (num2 != 2)) && (num2 != 3))
            {
                throw new InvalidOperationException(SR.GetString("SoundAPIFormatNotSupported"));
            }
        }

        private unsafe void ValidateSoundFile(string fileName)
        {
            NativeMethods.MMCKINFO lpck = new NativeMethods.MMCKINFO();
            NativeMethods.MMCKINFO mmckinfo2 = new NativeMethods.MMCKINFO();
            NativeMethods.WAVEFORMATEX structure = null;
            IntPtr hMIO = UnsafeNativeMethods.mmioOpen(fileName, IntPtr.Zero, 0x10000);
            if (hMIO == IntPtr.Zero)
            {
                throw new FileNotFoundException(SR.GetString("SoundAPIFileDoesNotExist"), this.soundLocation);
            }
            try
            {
                lpck.fccType = mmioFOURCC('W', 'A', 'V', 'E');
                if (UnsafeNativeMethods.mmioDescend(hMIO, lpck, null, 0x20) == 0)
                {
                    goto Label_0179;
                }
                throw new InvalidOperationException(SR.GetString("SoundAPIInvalidWaveFile", new object[] { this.soundLocation }));
            Label_008B:
                if ((mmckinfo2.dwDataOffset + mmckinfo2.cksize) > (lpck.dwDataOffset + lpck.cksize))
                {
                    throw new InvalidOperationException(SR.GetString("SoundAPIInvalidWaveHeader"));
                }
                if ((mmckinfo2.ckID == mmioFOURCC('f', 'm', 't', ' ')) && (structure == null))
                {
                    int cksize = mmckinfo2.cksize;
                    if (cksize < Marshal.SizeOf(typeof(NativeMethods.WAVEFORMATEX)))
                    {
                        cksize = Marshal.SizeOf(typeof(NativeMethods.WAVEFORMATEX));
                    }
                    structure = new NativeMethods.WAVEFORMATEX();
                    byte[] wf = new byte[cksize];
                    if (UnsafeNativeMethods.mmioRead(hMIO, wf, cksize) != cksize)
                    {
                        throw new InvalidOperationException(SR.GetString("SoundAPIReadError", new object[] { this.soundLocation }));
                    }
                    try
                    {
                        fixed (byte* numRef = wf)
                        {
                            Marshal.PtrToStructure((IntPtr) numRef, structure);
                        }
                    }
                    finally
                    {
                        numRef = null;
                    }
                }
                UnsafeNativeMethods.mmioAscend(hMIO, mmckinfo2, 0);
            Label_0179:
                if (UnsafeNativeMethods.mmioDescend(hMIO, mmckinfo2, lpck, 0) == 0)
                {
                    goto Label_008B;
                }
                if (structure == null)
                {
                    throw new InvalidOperationException(SR.GetString("SoundAPIInvalidWaveHeader"));
                }
                if (((structure.wFormatTag != 1) && (structure.wFormatTag != 2)) && (structure.wFormatTag != 3))
                {
                    throw new InvalidOperationException(SR.GetString("SoundAPIFormatNotSupported"));
                }
            }
            finally
            {
                if (hMIO != IntPtr.Zero)
                {
                    UnsafeNativeMethods.mmioClose(hMIO, 0);
                }
            }
        }

        private void WorkerThread()
        {
            try
            {
                if (((this.uri != null) && !this.uri.IsFile) && (this.stream == null))
                {
                    this.stream = WebRequest.Create(this.uri).GetResponse().GetResponseStream();
                }
                this.streamData = new byte[0x400];
                int num = this.stream.Read(this.streamData, this.currentPos, 0x400);
                for (int i = num; num > 0; i += num)
                {
                    this.currentPos += num;
                    if (this.streamData.Length < (this.currentPos + 0x400))
                    {
                        byte[] destinationArray = new byte[this.streamData.Length * 2];
                        Array.Copy(this.streamData, destinationArray, this.streamData.Length);
                        this.streamData = destinationArray;
                    }
                    num = this.stream.Read(this.streamData, this.currentPos, 0x400);
                }
                this.lastLoadException = null;
            }
            catch (Exception exception)
            {
                this.lastLoadException = exception;
            }
            if (!this.doesLoadAppearSynchronous)
            {
                this.asyncOperation.PostOperationCompleted(this.loadAsyncOperationCompleted, new AsyncCompletedEventArgs(this.lastLoadException, false, null));
            }
            this.isLoadCompleted = true;
            this.semaphore.Set();
        }

        public bool IsLoadCompleted
        {
            get
            {
                return this.isLoadCompleted;
            }
        }

        public int LoadTimeout
        {
            get
            {
                return this.loadTimeout;
            }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("LoadTimeout", value, SR.GetString("SoundAPILoadTimeout"));
                }
                this.loadTimeout = value;
            }
        }

        public string SoundLocation
        {
            get
            {
                if ((this.uri != null) && this.uri.IsFile)
                {
                    new FileIOPermission(PermissionState.None) { AllFiles = FileIOPermissionAccess.PathDiscovery }.Demand();
                }
                return this.soundLocation;
            }
            set
            {
                if (value == null)
                {
                    value = string.Empty;
                }
                if (!this.soundLocation.Equals(value))
                {
                    this.SetupSoundLocation(value);
                    this.OnSoundLocationChanged(EventArgs.Empty);
                }
            }
        }

        public System.IO.Stream Stream
        {
            get
            {
                if (this.uri != null)
                {
                    return null;
                }
                return this.stream;
            }
            set
            {
                if (this.stream != value)
                {
                    this.SetupStream(value);
                    this.OnStreamChanged(EventArgs.Empty);
                }
            }
        }

        public object Tag
        {
            get
            {
                return this.tag;
            }
            set
            {
                this.tag = value;
            }
        }

        private class IntSecurity
        {
            private static CodeAccessPermission safeSubWindows;

            private IntSecurity()
            {
            }

            internal static CodeAccessPermission SafeSubWindows
            {
                get
                {
                    if (safeSubWindows == null)
                    {
                        safeSubWindows = new UIPermission(UIPermissionWindow.SafeSubWindows);
                    }
                    return safeSubWindows;
                }
            }
        }

        private class NativeMethods
        {
            internal const int MMIO_ALLOCBUF = 0x10000;
            internal const int MMIO_FINDRIFF = 0x20;
            internal const int MMIO_READ = 0;
            internal const int SND_ASYNC = 1;
            internal const int SND_FILENAME = 0x20000;
            internal const int SND_LOOP = 8;
            internal const int SND_MEMORY = 4;
            internal const int SND_NODEFAULT = 2;
            internal const int SND_NOSTOP = 0x10;
            internal const int SND_PURGE = 0x40;
            internal const int SND_SYNC = 0;
            internal const int WAVE_FORMAT_ADPCM = 2;
            internal const int WAVE_FORMAT_IEEE_FLOAT = 3;
            internal const int WAVE_FORMAT_PCM = 1;

            private NativeMethods()
            {
            }

            [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
            internal class MMCKINFO
            {
                internal int ckID;
                internal int cksize;
                internal int fccType;
                internal int dwDataOffset;
                internal int dwFlags;
            }

            [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
            internal class WAVEFORMATEX
            {
                internal short wFormatTag;
                internal short nChannels;
                internal int nSamplesPerSec;
                internal int nAvgBytesPerSec;
                internal short nBlockAlign;
                internal short wBitsPerSample;
                internal short cbSize;
            }
        }

        private class UnsafeNativeMethods
        {
            private UnsafeNativeMethods()
            {
            }

            [DllImport("winmm.dll", CharSet=CharSet.Auto)]
            internal static extern int mmioAscend(IntPtr hMIO, SoundPlayer.NativeMethods.MMCKINFO lpck, int flags);
            [DllImport("winmm.dll", CharSet=CharSet.Auto)]
            internal static extern int mmioClose(IntPtr hMIO, int flags);
            [DllImport("winmm.dll", CharSet=CharSet.Auto)]
            internal static extern int mmioDescend(IntPtr hMIO, [MarshalAs(UnmanagedType.LPStruct)] SoundPlayer.NativeMethods.MMCKINFO lpck, [MarshalAs(UnmanagedType.LPStruct)] SoundPlayer.NativeMethods.MMCKINFO lcpkParent, int flags);
            [DllImport("winmm.dll", CharSet=CharSet.Auto)]
            internal static extern IntPtr mmioOpen(string fileName, IntPtr not_used, int flags);
            [DllImport("winmm.dll", CharSet=CharSet.Auto)]
            internal static extern int mmioRead(IntPtr hMIO, [MarshalAs(UnmanagedType.LPArray)] byte[] wf, int cch);
            [DllImport("winmm.dll", CharSet=CharSet.Auto)]
            internal static extern bool PlaySound([MarshalAs(UnmanagedType.LPWStr)] string soundName, IntPtr hmod, int soundFlags);
            [DllImport("winmm.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
            internal static extern bool PlaySound(byte[] soundName, IntPtr hmod, int soundFlags);
        }
    }
}

