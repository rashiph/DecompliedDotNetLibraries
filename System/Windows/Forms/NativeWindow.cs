namespace System.Windows.Forms
{
    using Microsoft.Win32;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Internal;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Runtime.Versioning;
    using System.Security;
    using System.Security.Permissions;
    using System.Text;

    [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode), SecurityPermission(SecurityAction.InheritanceDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    public class NativeWindow : MarshalByRefObject, IWin32Window
    {
        [ThreadStatic]
        private static bool anyHandleCreated;
        private static bool anyHandleCreatedInApp;
        private const int AssemblyIsDebuggable = 0x10;
        private static object createWindowSyncObject = new object();
        private const int DebuggerPresent = 2;
        private IntPtr defWindowProc;
        private static short globalID = 1;
        private IntPtr handle;
        private static int handleCount;
        private static HandleBucket[] hashBuckets;
        private static Dictionary<IntPtr, short> hashForHandleId;
        private static Dictionary<short, IntPtr> hashForIdHandle;
        private const float hashLoadFactor = 0.72f;
        private static int hashLoadSize;
        private const int InitializedFlags = 1;
        private static object internalSyncObject = new object();
        private const int LoadConfigSettings = 8;
        private NativeWindow nextWindow;
        private bool ownHandle;
        private NativeWindow previousWindow;
        private static readonly int[] primes = new int[] { 
            11, 0x11, 0x17, 0x1d, 0x25, 0x2f, 0x3b, 0x47, 0x59, 0x6b, 0x83, 0xa3, 0xc5, 0xef, 0x125, 0x161, 
            0x1af, 0x209, 0x277, 0x2f9, 0x397, 0x44f, 0x52f, 0x63d, 0x78b, 0x91d, 0xaf1, 0xd2b, 0xfd1, 0x12fd, 0x16cf, 0x1b65, 
            0x20e3, 0x2777, 0x2f6f, 0x38ff, 0x446f, 0x521f, 0x628d, 0x7655, 0x8e01, 0xaa6b, 0xcc89, 0xf583, 0x126a7, 0x1619b, 0x1a857, 0x1fd3b, 
            0x26315, 0x2dd67, 0x3701b, 0x42023, 0x4f361, 0x5f0ed, 0x72125, 0x88e31, 0xa443b, 0xc51eb, 0xec8c1, 0x11bdbf, 0x154a3f, 0x198c4f, 0x1ea867, 0x24ca19, 
            0x2c25c1, 0x34fa1b, 0x3f928f, 0x4c4987, 0x5b8b6f, 0x6dda89
         };
        private bool suppressedGC;
        private const int UseDebuggableWndProc = 4;
        private static IntPtr userDefWindowProc;
        [ThreadStatic]
        private static byte userSetProcFlags = 0;
        private static byte userSetProcFlagsForApp;
        private WeakReference weakThisPtr;
        private System.Windows.Forms.NativeMethods.WndProc windowProc;
        private IntPtr windowProcPtr;
        private static readonly TraceSwitch WndProcChoice;
        [ThreadStatic]
        private static byte wndProcFlags = 0;

        static NativeWindow()
        {
            EventHandler handler = new EventHandler(NativeWindow.OnShutdown);
            AppDomain.CurrentDomain.ProcessExit += handler;
            AppDomain.CurrentDomain.DomainUnload += handler;
            int num = primes[4];
            hashBuckets = new HandleBucket[num];
            hashLoadSize = (int) (0.72f * num);
            if (hashLoadSize >= num)
            {
                hashLoadSize = num - 1;
            }
            hashForIdHandle = new Dictionary<short, IntPtr>();
            hashForHandleId = new Dictionary<IntPtr, short>();
        }

        public NativeWindow()
        {
            this.weakThisPtr = new WeakReference(this);
        }

        internal static void AddWindowToIDTable(object wrapper, IntPtr handle)
        {
            hashForIdHandle[globalID] = handle;
            hashForHandleId[handle] = globalID;
            System.Windows.Forms.UnsafeNativeMethods.SetWindowLong(new HandleRef(wrapper, handle), -12, new HandleRef(wrapper, (IntPtr) globalID));
            globalID = (short) (globalID + 1);
        }

        private static void AddWindowToTable(IntPtr handle, NativeWindow window)
        {
            lock (internalSyncObject)
            {
                uint num;
                uint num2;
                if (handleCount >= hashLoadSize)
                {
                    ExpandTable();
                }
                anyHandleCreated = true;
                anyHandleCreatedInApp = true;
                uint num3 = InitHash(handle, hashBuckets.Length, out num, out num2);
                int num4 = 0;
                int index = -1;
                GCHandle handle2 = GCHandle.Alloc(window, GCHandleType.Weak);
                do
                {
                    int num6 = (int) (num % hashBuckets.Length);
                    if (((index == -1) && (hashBuckets[num6].handle == new IntPtr(-1))) && (hashBuckets[num6].hash_coll < 0))
                    {
                        index = num6;
                    }
                    if ((hashBuckets[num6].handle == IntPtr.Zero) || ((hashBuckets[num6].handle == new IntPtr(-1)) && ((hashBuckets[num6].hash_coll & 0x80000000L) == 0L)))
                    {
                        if (index != -1)
                        {
                            num6 = index;
                        }
                        hashBuckets[num6].window = handle2;
                        hashBuckets[num6].handle = handle;
                        hashBuckets[num6].hash_coll |= (int) num3;
                        handleCount++;
                        goto Label_0269;
                    }
                    if (((hashBuckets[num6].hash_coll & 0x7fffffff) == num3) && (handle == hashBuckets[num6].handle))
                    {
                        GCHandle handle3 = hashBuckets[num6].window;
                        if (handle3.IsAllocated)
                        {
                            window.previousWindow = (NativeWindow) handle3.Target;
                            window.previousWindow.nextWindow = window;
                            handle3.Free();
                        }
                        hashBuckets[num6].window = handle2;
                        goto Label_0269;
                    }
                    if (index == -1)
                    {
                        hashBuckets[num6].hash_coll |= -2147483648;
                    }
                    num += num2;
                }
                while (++num4 < hashBuckets.Length);
                if (index != -1)
                {
                    hashBuckets[index].window = handle2;
                    hashBuckets[index].handle = handle;
                    hashBuckets[index].hash_coll |= (int) num3;
                    handleCount++;
                }
            Label_0269:;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static int AdjustWndProcFlagsFromConfig(int wndProcFlags)
        {
            if (WindowsFormsSection.GetSection().JitDebugging)
            {
                wndProcFlags |= 4;
            }
            return wndProcFlags;
        }

        private static int AdjustWndProcFlagsFromMetadata(int wndProcFlags)
        {
            if ((wndProcFlags & 2) != 0)
            {
                Assembly entryAssembly = Assembly.GetEntryAssembly();
                if ((entryAssembly != null) && Attribute.IsDefined(entryAssembly, typeof(DebuggableAttribute)))
                {
                    Attribute[] customAttributes = Attribute.GetCustomAttributes(entryAssembly, typeof(DebuggableAttribute));
                    if (customAttributes.Length > 0)
                    {
                        DebuggableAttribute attribute = (DebuggableAttribute) customAttributes[0];
                        if (attribute.IsJITTrackingEnabled)
                        {
                            wndProcFlags |= 0x10;
                        }
                    }
                }
            }
            return wndProcFlags;
        }

        private static int AdjustWndProcFlagsFromRegistry(int wndProcFlags)
        {
            new RegistryPermission(PermissionState.Unrestricted).Assert();
            try
            {
                RegistryKey key = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\.NETFramework");
                if (key == null)
                {
                    return wndProcFlags;
                }
                try
                {
                    object obj2 = key.GetValue("DbgJITDebugLaunchSetting");
                    if (obj2 != null)
                    {
                        int num = 0;
                        try
                        {
                            num = (int) obj2;
                        }
                        catch (InvalidCastException)
                        {
                            num = 1;
                        }
                        if (num != 1)
                        {
                            wndProcFlags |= 2;
                            wndProcFlags |= 8;
                        }
                        return wndProcFlags;
                    }
                    if (key.GetValue("DbgManagedDebugger") != null)
                    {
                        wndProcFlags |= 2;
                        wndProcFlags |= 8;
                    }
                    return wndProcFlags;
                }
                finally
                {
                    key.Close();
                }
            }
            finally
            {
                CodeAccessPermission.RevertAssert();
            }
            return wndProcFlags;
        }

        public void AssignHandle(IntPtr handle)
        {
            this.AssignHandle(handle, true);
        }

        internal void AssignHandle(IntPtr handle, bool assignUniqueID)
        {
            lock (this)
            {
                this.CheckReleased();
                this.handle = handle;
                if (userDefWindowProc == IntPtr.Zero)
                {
                    string lpProcName = (Marshal.SystemDefaultCharSize == 1) ? "DefWindowProcA" : "DefWindowProcW";
                    userDefWindowProc = System.Windows.Forms.UnsafeNativeMethods.GetProcAddress(new HandleRef(null, System.Windows.Forms.UnsafeNativeMethods.GetModuleHandle("user32.dll")), lpProcName);
                    if (userDefWindowProc == IntPtr.Zero)
                    {
                        throw new Win32Exception();
                    }
                }
                this.defWindowProc = System.Windows.Forms.UnsafeNativeMethods.GetWindowLong(new HandleRef(this, handle), -4);
                if (WndProcShouldBeDebuggable)
                {
                    this.windowProc = new System.Windows.Forms.NativeMethods.WndProc(this.DebuggableCallback);
                }
                else
                {
                    this.windowProc = new System.Windows.Forms.NativeMethods.WndProc(this.Callback);
                }
                AddWindowToTable(handle, this);
                System.Windows.Forms.UnsafeNativeMethods.SetWindowLong(new HandleRef(this, handle), -4, this.windowProc);
                this.windowProcPtr = System.Windows.Forms.UnsafeNativeMethods.GetWindowLong(new HandleRef(this, handle), -4);
                if ((assignUniqueID && ((((int) ((long) System.Windows.Forms.UnsafeNativeMethods.GetWindowLong(new HandleRef(this, handle), -16))) & 0x40000000) != 0)) && (((int) ((long) System.Windows.Forms.UnsafeNativeMethods.GetWindowLong(new HandleRef(this, handle), -12))) == 0))
                {
                    System.Windows.Forms.UnsafeNativeMethods.SetWindowLong(new HandleRef(this, handle), -12, new HandleRef(this, handle));
                }
                if (this.suppressedGC)
                {
                    new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Assert();
                    try
                    {
                        GC.ReRegisterForFinalize(this);
                    }
                    finally
                    {
                        CodeAccessPermission.RevertAssert();
                    }
                    this.suppressedGC = false;
                }
                this.OnHandleChange();
            }
        }

        private IntPtr Callback(IntPtr hWnd, int msg, IntPtr wparam, IntPtr lparam)
        {
            Message m = Message.Create(hWnd, msg, wparam, lparam);
            try
            {
                if (this.weakThisPtr.IsAlive && (this.weakThisPtr.Target != null))
                {
                    this.WndProc(ref m);
                }
                else
                {
                    this.DefWndProc(ref m);
                }
            }
            catch (Exception exception)
            {
                this.OnThreadException(exception);
            }
            finally
            {
                if (msg == 130)
                {
                    this.ReleaseHandle(false);
                }
                if (msg == System.Windows.Forms.NativeMethods.WM_UIUNSUBCLASS)
                {
                    this.ReleaseHandle(true);
                }
            }
            return m.Result;
        }

        private void CheckReleased()
        {
            if (this.handle != IntPtr.Zero)
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("HandleAlreadyExists"));
            }
        }

        public virtual void CreateHandle(CreateParams cp)
        {
            System.Windows.Forms.IntSecurity.CreateAnyWindow.Demand();
            if (((cp.Style & 0x40000000) != 0x40000000) || (cp.Parent == IntPtr.Zero))
            {
                System.Windows.Forms.IntSecurity.TopLevelWindow.Demand();
            }
            lock (this)
            {
                this.CheckReleased();
                WindowClass class2 = WindowClass.Create(cp.ClassName, cp.ClassStyle);
                lock (createWindowSyncObject)
                {
                    if (this.handle == IntPtr.Zero)
                    {
                        class2.targetWindow = this;
                        IntPtr moduleHandle = System.Windows.Forms.UnsafeNativeMethods.GetModuleHandle(null);
                        IntPtr zero = IntPtr.Zero;
                        int error = 0;
                        try
                        {
                            if ((cp.Caption != null) && (cp.Caption.Length > 0x7fff))
                            {
                                cp.Caption = cp.Caption.Substring(0, 0x7fff);
                            }
                            zero = System.Windows.Forms.UnsafeNativeMethods.CreateWindowEx(cp.ExStyle, class2.windowClassName, cp.Caption, cp.Style, cp.X, cp.Y, cp.Width, cp.Height, new HandleRef(cp, cp.Parent), System.Windows.Forms.NativeMethods.NullHandleRef, new HandleRef(null, moduleHandle), cp.Param);
                            error = Marshal.GetLastWin32Error();
                        }
                        catch (NullReferenceException exception)
                        {
                            throw new OutOfMemoryException(System.Windows.Forms.SR.GetString("ErrorCreatingHandle"), exception);
                        }
                        class2.targetWindow = null;
                        if (zero == IntPtr.Zero)
                        {
                            throw new Win32Exception(error, System.Windows.Forms.SR.GetString("ErrorCreatingHandle"));
                        }
                        this.ownHandle = true;
                        System.Internal.HandleCollector.Add(zero, System.Windows.Forms.NativeMethods.CommonHandles.Window);
                    }
                }
            }
        }

        private IntPtr DebuggableCallback(IntPtr hWnd, int msg, IntPtr wparam, IntPtr lparam)
        {
            Message m = Message.Create(hWnd, msg, wparam, lparam);
            try
            {
                if (this.weakThisPtr.IsAlive && (this.weakThisPtr.Target != null))
                {
                    this.WndProc(ref m);
                }
                else
                {
                    this.DefWndProc(ref m);
                }
            }
            finally
            {
                if (msg == 130)
                {
                    this.ReleaseHandle(false);
                }
                if (msg == System.Windows.Forms.NativeMethods.WM_UIUNSUBCLASS)
                {
                    this.ReleaseHandle(true);
                }
            }
            return m.Result;
        }

        public void DefWndProc(ref Message m)
        {
            if (this.previousWindow == null)
            {
                if (this.defWindowProc == IntPtr.Zero)
                {
                    m.Result = System.Windows.Forms.UnsafeNativeMethods.DefWindowProc(m.HWnd, m.Msg, m.WParam, m.LParam);
                }
                else
                {
                    m.Result = System.Windows.Forms.UnsafeNativeMethods.CallWindowProc(this.defWindowProc, m.HWnd, m.Msg, m.WParam, m.LParam);
                }
            }
            else
            {
                m.Result = this.previousWindow.Callback(m.HWnd, m.Msg, m.WParam, m.LParam);
            }
        }

        public virtual void DestroyHandle()
        {
            lock (this)
            {
                if (this.handle != IntPtr.Zero)
                {
                    if (!System.Windows.Forms.UnsafeNativeMethods.DestroyWindow(new HandleRef(this, this.handle)))
                    {
                        this.UnSubclass();
                        System.Windows.Forms.UnsafeNativeMethods.PostMessage(new HandleRef(this, this.handle), 0x10, 0, 0);
                    }
                    this.handle = IntPtr.Zero;
                    this.ownHandle = false;
                }
                new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Assert();
                try
                {
                    GC.SuppressFinalize(this);
                }
                finally
                {
                    CodeAccessPermission.RevertAssert();
                }
                this.suppressedGC = true;
            }
        }

        private static void ExpandTable()
        {
            int length = hashBuckets.Length;
            int prime = GetPrime(1 + (length * 2));
            HandleBucket[] bucketArray = new HandleBucket[prime];
            for (int i = 0; i < length; i++)
            {
                HandleBucket bucket = hashBuckets[i];
                if ((bucket.handle != IntPtr.Zero) && (bucket.handle != new IntPtr(-1)))
                {
                    uint num4 = (uint) (bucket.hash_coll & 0x7fffffff);
                    uint num5 = (uint) (1 + (((num4 >> 5) + 1) % (bucketArray.Length - 1)));
                    while (true)
                    {
                        int index = (int) (num4 % bucketArray.Length);
                        if ((bucketArray[index].handle == IntPtr.Zero) || (bucketArray[index].handle == new IntPtr(-1)))
                        {
                            bucketArray[index].window = bucket.window;
                            bucketArray[index].handle = bucket.handle;
                            bucketArray[index].hash_coll |= bucket.hash_coll & 0x7fffffff;
                            break;
                        }
                        bucketArray[index].hash_coll |= -2147483648;
                        num4 += num5;
                    }
                }
            }
            hashBuckets = bucketArray;
            hashLoadSize = (int) (0.72f * prime);
            if (hashLoadSize >= prime)
            {
                hashLoadSize = prime - 1;
            }
        }

        ~NativeWindow()
        {
            this.ForceExitMessageLoop();
        }

        internal void ForceExitMessageLoop()
        {
            IntPtr handle;
            bool ownHandle;
            lock (this)
            {
                handle = this.handle;
                ownHandle = this.ownHandle;
            }
            if (this.handle != IntPtr.Zero)
            {
                if (System.Windows.Forms.UnsafeNativeMethods.IsWindow(new HandleRef(null, this.handle)))
                {
                    int num;
                    Application.ThreadContext context = Application.ThreadContext.FromId(System.Windows.Forms.SafeNativeMethods.GetWindowThreadProcessId(new HandleRef(null, this.handle), out num));
                    IntPtr ptr2 = (context == null) ? IntPtr.Zero : context.GetHandle();
                    if (ptr2 != IntPtr.Zero)
                    {
                        int lpdwExitCode = 0;
                        System.Windows.Forms.SafeNativeMethods.GetExitCodeThread(new HandleRef(null, ptr2), out lpdwExitCode);
                        if (!AppDomain.CurrentDomain.IsFinalizingForUnload() && (lpdwExitCode == 0x103))
                        {
                            IntPtr ptr3;
                            bool flag1 = System.Windows.Forms.UnsafeNativeMethods.SendMessageTimeout(new HandleRef(null, this.handle), System.Windows.Forms.NativeMethods.WM_UIUNSUBCLASS, IntPtr.Zero, IntPtr.Zero, 2, 100, out ptr3) == IntPtr.Zero;
                        }
                    }
                }
                if (this.handle != IntPtr.Zero)
                {
                    this.ReleaseHandle(true);
                }
            }
            if ((handle != IntPtr.Zero) && ownHandle)
            {
                System.Windows.Forms.UnsafeNativeMethods.PostMessage(new HandleRef(this, handle), 0x10, 0, 0);
            }
        }

        public static NativeWindow FromHandle(IntPtr handle)
        {
            if ((handle != IntPtr.Zero) && (handleCount > 0))
            {
                return GetWindowFromTable(handle);
            }
            return null;
        }

        internal IntPtr GetHandleFromID(short id)
        {
            IntPtr ptr;
            if ((hashForIdHandle != null) && hashForIdHandle.TryGetValue(id, out ptr))
            {
                return ptr;
            }
            return IntPtr.Zero;
        }

        private static int GetPrime(int minSize)
        {
            if (minSize < 0)
            {
                throw new OutOfMemoryException();
            }
            for (int i = 0; i < primes.Length; i++)
            {
                int num2 = primes[i];
                if (num2 >= minSize)
                {
                    return num2;
                }
            }
            for (int j = (minSize - 2) | 1; j < 0x7fffffff; j += 2)
            {
                bool flag = true;
                if ((j & 1) != 0)
                {
                    int num4 = (int) Math.Sqrt((double) j);
                    for (int k = 3; k < num4; k += 2)
                    {
                        if ((j % k) == 0)
                        {
                            flag = false;
                            break;
                        }
                    }
                    if (!flag)
                    {
                        continue;
                    }
                    return j;
                }
                if (j == 2)
                {
                    return j;
                }
            }
            return minSize;
        }

        private static NativeWindow GetWindowFromTable(IntPtr handle)
        {
            uint num;
            uint num2;
            HandleBucket bucket;
            HandleBucket[] hashBuckets = NativeWindow.hashBuckets;
            int num3 = 0;
            uint num4 = InitHash(handle, hashBuckets.Length, out num, out num2);
            do
            {
                int index = (int) (num % hashBuckets.Length);
                bucket = hashBuckets[index];
                if (bucket.handle == IntPtr.Zero)
                {
                    return null;
                }
                if ((((bucket.hash_coll & 0x7fffffff) == num4) && (handle == bucket.handle)) && bucket.window.IsAllocated)
                {
                    return (NativeWindow) bucket.window.Target;
                }
                num += num2;
            }
            while ((bucket.hash_coll < 0) && (++num3 < hashBuckets.Length));
            return null;
        }

        private static uint InitHash(IntPtr handle, int hashsize, out uint seed, out uint incr)
        {
            uint num = (uint) (handle.GetHashCode() & 0x7fffffff);
            seed = num;
            incr = 1 + ((uint) (((seed >> 5) + 1) % (hashsize - 1)));
            return num;
        }

        private static bool IsRootWindowInListWithChildren(NativeWindow window)
        {
            return ((window.PreviousWindow != null) && (window.nextWindow == null));
        }

        protected virtual void OnHandleChange()
        {
        }

        [PrePrepareMethod]
        private static void OnShutdown(object sender, EventArgs e)
        {
            if (handleCount > 0)
            {
                lock (internalSyncObject)
                {
                    for (int i = 0; i < hashBuckets.Length; i++)
                    {
                        HandleBucket wrapper = hashBuckets[i];
                        if ((wrapper.handle != IntPtr.Zero) && (wrapper.handle != new IntPtr(-1)))
                        {
                            HandleRef hWnd = new HandleRef(wrapper, wrapper.handle);
                            System.Windows.Forms.UnsafeNativeMethods.SetWindowLong(hWnd, -4, new HandleRef(null, userDefWindowProc));
                            System.Windows.Forms.UnsafeNativeMethods.SetClassLong(hWnd, -24, userDefWindowProc);
                            System.Windows.Forms.UnsafeNativeMethods.PostMessage(hWnd, 0x10, 0, 0);
                            if (wrapper.window.IsAllocated)
                            {
                                NativeWindow target = (NativeWindow) wrapper.window.Target;
                                if (target != null)
                                {
                                    target.handle = IntPtr.Zero;
                                }
                            }
                            wrapper.window.Free();
                        }
                        hashBuckets[i].handle = IntPtr.Zero;
                        hashBuckets[i].hash_coll = 0;
                    }
                    handleCount = 0;
                }
            }
            WindowClass.DisposeCache();
        }

        protected virtual void OnThreadException(Exception e)
        {
        }

        public virtual void ReleaseHandle()
        {
            this.ReleaseHandle(true);
        }

        private void ReleaseHandle(bool handleValid)
        {
            if (this.handle != IntPtr.Zero)
            {
                lock (this)
                {
                    if (this.handle != IntPtr.Zero)
                    {
                        if (handleValid)
                        {
                            this.UnSubclass();
                        }
                        RemoveWindowFromTable(this.handle, this);
                        if (this.ownHandle)
                        {
                            System.Internal.HandleCollector.Remove(this.handle, System.Windows.Forms.NativeMethods.CommonHandles.Window);
                            this.ownHandle = false;
                        }
                        this.handle = IntPtr.Zero;
                        if (this.weakThisPtr.IsAlive && (this.weakThisPtr.Target != null))
                        {
                            this.OnHandleChange();
                            new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Assert();
                            try
                            {
                                GC.SuppressFinalize(this);
                            }
                            finally
                            {
                                CodeAccessPermission.RevertAssert();
                            }
                            this.suppressedGC = true;
                        }
                    }
                }
            }
        }

        internal static void RemoveWindowFromIDTable(IntPtr handle)
        {
            short key = hashForHandleId[handle];
            hashForHandleId.Remove(handle);
            hashForIdHandle.Remove(key);
        }

        private static void RemoveWindowFromTable(IntPtr handle, NativeWindow window)
        {
            lock (internalSyncObject)
            {
                uint num;
                uint num2;
                int num5;
                uint num3 = InitHash(handle, hashBuckets.Length, out num, out num2);
                int num4 = 0;
                NativeWindow previousWindow = window.PreviousWindow;
                do
                {
                    num5 = (int) (num % hashBuckets.Length);
                    HandleBucket bucket = hashBuckets[num5];
                    if (((bucket.hash_coll & 0x7fffffff) == num3) && (handle == bucket.handle))
                    {
                        bool flag = window.nextWindow == null;
                        bool flag2 = IsRootWindowInListWithChildren(window);
                        if (window.previousWindow != null)
                        {
                            window.previousWindow.nextWindow = window.nextWindow;
                        }
                        if (window.nextWindow != null)
                        {
                            window.nextWindow.defWindowProc = window.defWindowProc;
                            window.nextWindow.previousWindow = window.previousWindow;
                        }
                        window.nextWindow = null;
                        window.previousWindow = null;
                        if (flag2)
                        {
                            if (hashBuckets[num5].window.IsAllocated)
                            {
                                hashBuckets[num5].window.Free();
                            }
                            hashBuckets[num5].window = GCHandle.Alloc(previousWindow, GCHandleType.Weak);
                        }
                        else if (flag)
                        {
                            hashBuckets[num5].hash_coll &= -2147483648;
                            if (hashBuckets[num5].hash_coll != 0)
                            {
                                hashBuckets[num5].handle = new IntPtr(-1);
                            }
                            else
                            {
                                hashBuckets[num5].handle = IntPtr.Zero;
                            }
                            if (hashBuckets[num5].window.IsAllocated)
                            {
                                hashBuckets[num5].window.Free();
                            }
                            handleCount--;
                        }
                        break;
                    }
                    num += num2;
                }
                while ((hashBuckets[num5].hash_coll < 0) && (++num4 < hashBuckets.Length));
            }
        }

        internal static void SetUnhandledExceptionModeInternal(UnhandledExceptionMode mode, bool threadScope)
        {
            if (!threadScope && anyHandleCreatedInApp)
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("ApplicationCannotChangeApplicationExceptionMode"));
            }
            if (threadScope && anyHandleCreated)
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("ApplicationCannotChangeThreadExceptionMode"));
            }
            switch (mode)
            {
                case UnhandledExceptionMode.Automatic:
                    if (!threadScope)
                    {
                        userSetProcFlagsForApp = 0;
                        return;
                    }
                    userSetProcFlags = 0;
                    return;

                case UnhandledExceptionMode.ThrowException:
                    if (!threadScope)
                    {
                        userSetProcFlagsForApp = 5;
                        return;
                    }
                    userSetProcFlags = 5;
                    return;

                case UnhandledExceptionMode.CatchException:
                    if (!threadScope)
                    {
                        userSetProcFlagsForApp = 1;
                        return;
                    }
                    userSetProcFlags = 1;
                    return;
            }
            throw new InvalidEnumArgumentException("mode", (int) mode, typeof(UnhandledExceptionMode));
        }

        private void UnSubclass()
        {
            bool flag = !this.weakThisPtr.IsAlive || (this.weakThisPtr.Target == null);
            HandleRef hWnd = new HandleRef(this, this.handle);
            IntPtr windowLong = System.Windows.Forms.UnsafeNativeMethods.GetWindowLong(new HandleRef(this, this.handle), -4);
            if (this.windowProcPtr == windowLong)
            {
                if (this.previousWindow == null)
                {
                    System.Windows.Forms.UnsafeNativeMethods.SetWindowLong(hWnd, -4, new HandleRef(this, this.defWindowProc));
                }
                else if (flag)
                {
                    System.Windows.Forms.UnsafeNativeMethods.SetWindowLong(hWnd, -4, new HandleRef(this, userDefWindowProc));
                }
                else
                {
                    System.Windows.Forms.UnsafeNativeMethods.SetWindowLong(hWnd, -4, this.previousWindow.windowProc);
                }
            }
            else if ((this.nextWindow == null) || (this.nextWindow.defWindowProc != this.windowProcPtr))
            {
                System.Windows.Forms.UnsafeNativeMethods.SetWindowLong(hWnd, -4, new HandleRef(this, userDefWindowProc));
            }
        }

        protected virtual void WndProc(ref Message m)
        {
            this.DefWndProc(ref m);
        }

        internal static bool AnyHandleCreated
        {
            get
            {
                return anyHandleCreated;
            }
        }

        public IntPtr Handle
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                return this.handle;
            }
        }

        internal NativeWindow PreviousWindow
        {
            get
            {
                return this.previousWindow;
            }
        }

        internal static IntPtr UserDefindowProc
        {
            get
            {
                return userDefWindowProc;
            }
        }

        private static int WndProcFlags
        {
            get
            {
                int wndProcFlags = NativeWindow.wndProcFlags;
                if (wndProcFlags == 0)
                {
                    if (userSetProcFlags != 0)
                    {
                        wndProcFlags = userSetProcFlags;
                    }
                    else if (userSetProcFlagsForApp != 0)
                    {
                        wndProcFlags = userSetProcFlagsForApp;
                    }
                    else if (!Application.CustomThreadExceptionHandlerAttached)
                    {
                        if (Debugger.IsAttached)
                        {
                            wndProcFlags |= 4;
                        }
                        else
                        {
                            wndProcFlags = AdjustWndProcFlagsFromRegistry(wndProcFlags);
                            if ((wndProcFlags & 2) != 0)
                            {
                                wndProcFlags = AdjustWndProcFlagsFromMetadata(wndProcFlags);
                                if ((wndProcFlags & 0x10) != 0)
                                {
                                    if ((wndProcFlags & 8) != 0)
                                    {
                                        wndProcFlags = AdjustWndProcFlagsFromConfig(wndProcFlags);
                                    }
                                    else
                                    {
                                        wndProcFlags |= 4;
                                    }
                                }
                            }
                        }
                    }
                    wndProcFlags |= 1;
                    NativeWindow.wndProcFlags = (byte) wndProcFlags;
                }
                return wndProcFlags;
            }
        }

        internal static bool WndProcShouldBeDebuggable
        {
            get
            {
                return ((WndProcFlags & 4) != 0);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct HandleBucket
        {
            public IntPtr handle;
            public GCHandle window;
            public int hash_coll;
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        private class WindowClass
        {
            internal static NativeWindow.WindowClass cache;
            internal string className;
            internal int classStyle;
            internal IntPtr defWindowProc;
            private static int domainQualifier = 0;
            internal int hashCode;
            internal NativeWindow.WindowClass next;
            internal bool registered;
            internal NativeWindow targetWindow;
            private static object wcInternalSyncObject = new object();
            internal string windowClassName;
            internal System.Windows.Forms.NativeMethods.WndProc windowProc;

            internal WindowClass(string className, int classStyle)
            {
                this.className = className;
                this.classStyle = classStyle;
                this.RegisterClass();
            }

            public IntPtr Callback(IntPtr hWnd, int msg, IntPtr wparam, IntPtr lparam)
            {
                System.Windows.Forms.UnsafeNativeMethods.SetWindowLong(new HandleRef(null, hWnd), -4, new HandleRef(this, this.defWindowProc));
                this.targetWindow.AssignHandle(hWnd);
                return this.targetWindow.Callback(hWnd, msg, wparam, lparam);
            }

            internal static NativeWindow.WindowClass Create(string className, int classStyle)
            {
                lock (wcInternalSyncObject)
                {
                    NativeWindow.WindowClass cache = NativeWindow.WindowClass.cache;
                    if (className != null)
                    {
                        goto Label_003F;
                    }
                    while ((cache != null) && ((cache.className != null) || (cache.classStyle != classStyle)))
                    {
                        cache = cache.next;
                    }
                    goto Label_0050;
                Label_0038:
                    cache = cache.next;
                Label_003F:
                    if ((cache != null) && !className.Equals(cache.className))
                    {
                        goto Label_0038;
                    }
                Label_0050:
                    if (cache == null)
                    {
                        cache = new NativeWindow.WindowClass(className, classStyle) {
                            next = NativeWindow.WindowClass.cache
                        };
                        NativeWindow.WindowClass.cache = cache;
                    }
                    else if (!cache.registered)
                    {
                        cache.RegisterClass();
                    }
                    return cache;
                }
            }

            internal static void DisposeCache()
            {
                lock (wcInternalSyncObject)
                {
                    for (NativeWindow.WindowClass class2 = cache; class2 != null; class2 = class2.next)
                    {
                        class2.UnregisterClass();
                    }
                }
            }

            private string GetFullClassName(string className)
            {
                StringBuilder builder = new StringBuilder(50);
                builder.Append(Application.WindowsFormsVersion);
                builder.Append('.');
                builder.Append(className);
                builder.Append(".app.");
                builder.Append(domainQualifier);
                builder.Append('.');
                string name = Convert.ToString(AppDomain.CurrentDomain.GetHashCode(), 0x10);
                builder.Append(VersioningHelper.MakeVersionSafeName(name, ResourceScope.Process, ResourceScope.AppDomain));
                return builder.ToString();
            }

            private void RegisterClass()
            {
                string className;
                System.Windows.Forms.NativeMethods.WNDCLASS_D wc = new System.Windows.Forms.NativeMethods.WNDCLASS_D();
                if (NativeWindow.userDefWindowProc == IntPtr.Zero)
                {
                    string lpProcName = (Marshal.SystemDefaultCharSize == 1) ? "DefWindowProcA" : "DefWindowProcW";
                    NativeWindow.userDefWindowProc = System.Windows.Forms.UnsafeNativeMethods.GetProcAddress(new HandleRef(null, System.Windows.Forms.UnsafeNativeMethods.GetModuleHandle("user32.dll")), lpProcName);
                    if (NativeWindow.userDefWindowProc == IntPtr.Zero)
                    {
                        throw new Win32Exception();
                    }
                }
                if (this.className == null)
                {
                    wc.hbrBackground = System.Windows.Forms.UnsafeNativeMethods.GetStockObject(5);
                    wc.style = this.classStyle;
                    this.defWindowProc = NativeWindow.userDefWindowProc;
                    className = "Window." + Convert.ToString(this.classStyle, 0x10);
                    this.hashCode = 0;
                }
                else
                {
                    System.Windows.Forms.NativeMethods.WNDCLASS_I wndclass_i = new System.Windows.Forms.NativeMethods.WNDCLASS_I();
                    bool flag = System.Windows.Forms.UnsafeNativeMethods.GetClassInfo(System.Windows.Forms.NativeMethods.NullHandleRef, this.className, wndclass_i);
                    int error = Marshal.GetLastWin32Error();
                    if (!flag)
                    {
                        throw new Win32Exception(error, System.Windows.Forms.SR.GetString("InvalidWndClsName"));
                    }
                    wc.style = wndclass_i.style;
                    wc.cbClsExtra = wndclass_i.cbClsExtra;
                    wc.cbWndExtra = wndclass_i.cbWndExtra;
                    wc.hIcon = wndclass_i.hIcon;
                    wc.hCursor = wndclass_i.hCursor;
                    wc.hbrBackground = wndclass_i.hbrBackground;
                    wc.lpszMenuName = Marshal.PtrToStringAuto(wndclass_i.lpszMenuName);
                    className = this.className;
                    this.defWindowProc = wndclass_i.lpfnWndProc;
                    this.hashCode = this.className.GetHashCode();
                }
                this.windowClassName = this.GetFullClassName(className);
                this.windowProc = new System.Windows.Forms.NativeMethods.WndProc(this.Callback);
                wc.lpfnWndProc = this.windowProc;
                wc.hInstance = System.Windows.Forms.UnsafeNativeMethods.GetModuleHandle(null);
                wc.lpszClassName = this.windowClassName;
                short num2 = System.Windows.Forms.UnsafeNativeMethods.RegisterClass(wc);
                if (num2 == 0)
                {
                    int num3 = Marshal.GetLastWin32Error();
                    if (num3 == 0x582)
                    {
                        System.Windows.Forms.NativeMethods.WNDCLASS_I wndclass_i2 = new System.Windows.Forms.NativeMethods.WNDCLASS_I();
                        if (System.Windows.Forms.UnsafeNativeMethods.GetClassInfo(new HandleRef(null, System.Windows.Forms.UnsafeNativeMethods.GetModuleHandle(null)), this.windowClassName, wndclass_i2) && (wndclass_i2.lpfnWndProc == NativeWindow.UserDefindowProc))
                        {
                            if (System.Windows.Forms.UnsafeNativeMethods.UnregisterClass(this.windowClassName, new HandleRef(null, System.Windows.Forms.UnsafeNativeMethods.GetModuleHandle(null))))
                            {
                                num2 = System.Windows.Forms.UnsafeNativeMethods.RegisterClass(wc);
                            }
                            else
                            {
                                do
                                {
                                    domainQualifier++;
                                    this.windowClassName = this.GetFullClassName(className);
                                    wc.lpszClassName = this.windowClassName;
                                    num2 = System.Windows.Forms.UnsafeNativeMethods.RegisterClass(wc);
                                }
                                while ((num2 == 0) && (Marshal.GetLastWin32Error() == 0x582));
                            }
                        }
                    }
                    if (num2 == 0)
                    {
                        this.windowProc = null;
                        throw new Win32Exception(num3);
                    }
                }
                this.registered = true;
            }

            private void UnregisterClass()
            {
                if (this.registered && System.Windows.Forms.UnsafeNativeMethods.UnregisterClass(this.windowClassName, new HandleRef(null, System.Windows.Forms.UnsafeNativeMethods.GetModuleHandle(null))))
                {
                    this.windowProc = null;
                    this.registered = false;
                }
            }
        }
    }
}

