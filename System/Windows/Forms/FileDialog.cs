namespace System.Windows.Forms
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Text;
    using System.Threading;
    using System.Windows.Forms.VisualStyles;

    [DefaultProperty("FileName"), DefaultEvent("FileOk")]
    public abstract class FileDialog : CommonDialog
    {
        private bool _autoUpgradeEnabled = true;
        private FileDialogCustomPlacesCollection _customPlaces = new FileDialogCustomPlacesCollection();
        private System.Windows.Forms.UnsafeNativeMethods.CharBuffer charBuffer;
        private string defaultExt;
        private IntPtr dialogHWnd;
        protected static readonly object EventFileOk = new object();
        private const int FILEBUFSIZE = 0x2000;
        private string[] fileNames;
        private string filter;
        private int filterIndex;
        private bool ignoreSecondFileOkNotification;
        private string initialDir;
        private int okNotificationCount;
        internal const int OPTION_ADDEXTENSION = -2147483648;
        internal int options;
        private bool securityCheckFileNames;
        private bool supportMultiDottedExtensions;
        private string title;

        [System.Windows.Forms.SRDescription("FDfileOkDescr")]
        public event CancelEventHandler FileOk
        {
            add
            {
                base.Events.AddHandler(EventFileOk, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventFileOk, value);
            }
        }

        internal FileDialog()
        {
            this.Reset();
        }

        internal abstract FileDialogNative.IFileDialog CreateVistaDialog();
        private bool DoFileOk(IntPtr lpOFN)
        {
            System.Windows.Forms.NativeMethods.OPENFILENAME_I openfilename_i = (System.Windows.Forms.NativeMethods.OPENFILENAME_I) System.Windows.Forms.UnsafeNativeMethods.PtrToStructure(lpOFN, typeof(System.Windows.Forms.NativeMethods.OPENFILENAME_I));
            int options = this.options;
            int filterIndex = this.filterIndex;
            string[] fileNames = this.fileNames;
            bool securityCheckFileNames = this.securityCheckFileNames;
            bool flag2 = false;
            try
            {
                this.options = (this.options & -2) | (openfilename_i.Flags & 1);
                this.filterIndex = openfilename_i.nFilterIndex;
                this.charBuffer.PutCoTaskMem(openfilename_i.lpstrFile);
                this.securityCheckFileNames = true;
                Thread.MemoryBarrier();
                if ((this.options & 0x200) == 0)
                {
                    this.fileNames = new string[] { this.charBuffer.GetString() };
                }
                else
                {
                    this.fileNames = this.GetMultiselectFiles(this.charBuffer);
                }
                if (!this.ProcessFileNames())
                {
                    return flag2;
                }
                CancelEventArgs e = new CancelEventArgs();
                if (NativeWindow.WndProcShouldBeDebuggable)
                {
                    this.OnFileOk(e);
                    return !e.Cancel;
                }
                try
                {
                    this.OnFileOk(e);
                    flag2 = !e.Cancel;
                }
                catch (Exception exception)
                {
                    Application.OnThreadException(exception);
                }
            }
            finally
            {
                if (!flag2)
                {
                    this.securityCheckFileNames = securityCheckFileNames;
                    Thread.MemoryBarrier();
                    this.fileNames = fileNames;
                    this.options = options;
                    this.filterIndex = filterIndex;
                }
            }
            return flag2;
        }

        internal abstract void EnsureFileDialogPermission();
        internal static bool FileExists(string fileName)
        {
            bool flag = false;
            try
            {
                new FileIOPermission(FileIOPermissionAccess.Read, System.Windows.Forms.IntSecurity.UnsafeGetFullPath(fileName)).Assert();
                try
                {
                    flag = File.Exists(fileName);
                }
                finally
                {
                    CodeAccessPermission.RevertAssert();
                }
            }
            catch (PathTooLongException)
            {
            }
            return flag;
        }

        internal static string GetFilePathFromShellItem(FileDialogNative.IShellItem item)
        {
            string str;
            item.GetDisplayName((FileDialogNative.SIGDN) (-2147319808), out str);
            return str;
        }

        private static FileDialogNative.COMDLG_FILTERSPEC[] GetFilterItems(string filter)
        {
            List<FileDialogNative.COMDLG_FILTERSPEC> list = new List<FileDialogNative.COMDLG_FILTERSPEC>();
            if (!string.IsNullOrEmpty(filter))
            {
                string[] strArray = filter.Split(new char[] { '|' });
                if ((strArray.Length % 2) == 0)
                {
                    for (int i = 1; i < strArray.Length; i += 2)
                    {
                        FileDialogNative.COMDLG_FILTERSPEC comdlg_filterspec;
                        comdlg_filterspec.pszSpec = strArray[i];
                        comdlg_filterspec.pszName = strArray[i - 1];
                        list.Add(comdlg_filterspec);
                    }
                }
            }
            return list.ToArray();
        }

        private string[] GetMultiselectFiles(System.Windows.Forms.UnsafeNativeMethods.CharBuffer charBuffer)
        {
            string str = charBuffer.GetString();
            string str2 = charBuffer.GetString();
            if (str2.Length == 0)
            {
                return new string[] { str };
            }
            if (str[str.Length - 1] != '\\')
            {
                str = str + @"\";
            }
            ArrayList list = new ArrayList();
            do
            {
                if ((str2[0] != '\\') && (((str2.Length <= 3) || (str2[1] != ':')) || (str2[2] != '\\')))
                {
                    str2 = str + str2;
                }
                list.Add(str2);
                str2 = charBuffer.GetString();
            }
            while (str2.Length > 0);
            string[] array = new string[list.Count];
            list.CopyTo(array, 0);
            return array;
        }

        internal bool GetOption(int option)
        {
            return ((this.options & option) != 0);
        }

        private FileDialogNative.FOS GetOptions()
        {
            FileDialogNative.FOS fos = ((FileDialogNative.FOS) this.options) & (FileDialogNative.FOS.FOS_ALLOWMULTISELECT | FileDialogNative.FOS.FOS_CREATEPROMPT | FileDialogNative.FOS.FOS_FILEMUSTEXIST | FileDialogNative.FOS.FOS_NOCHANGEDIR | FileDialogNative.FOS.FOS_NODEREFERENCELINKS | FileDialogNative.FOS.FOS_NOVALIDATE | FileDialogNative.FOS.FOS_OVERWRITEPROMPT | FileDialogNative.FOS.FOS_PATHMUSTEXIST);
            fos |= FileDialogNative.FOS.FOS_DEFAULTNOMINIMODE;
            return (fos | FileDialogNative.FOS.FOS_FORCEFILESYSTEM);
        }

        internal static FileDialogNative.IShellItem GetShellItemForPath(string path)
        {
            FileDialogNative.IShellItem ppsi = null;
            IntPtr zero = IntPtr.Zero;
            uint rgflnOut = 0;
            if ((0 > System.Windows.Forms.UnsafeNativeMethods.Shell32.SHILCreateFromPath(path, out zero, ref rgflnOut)) || (0 > System.Windows.Forms.UnsafeNativeMethods.Shell32.SHCreateShellItem(IntPtr.Zero, IntPtr.Zero, zero, out ppsi)))
            {
                throw new FileNotFoundException();
            }
            return ppsi;
        }

        private bool HandleVistaFileOk(FileDialogNative.IFileDialog dialog)
        {
            int options = this.options;
            int filterIndex = this.filterIndex;
            string[] fileNames = this.fileNames;
            bool securityCheckFileNames = this.securityCheckFileNames;
            bool flag2 = false;
            try
            {
                uint num3;
                this.securityCheckFileNames = true;
                Thread.MemoryBarrier();
                dialog.GetFileTypeIndex(out num3);
                this.filterIndex = (int) num3;
                this.fileNames = this.ProcessVistaFiles(dialog);
                if (!this.ProcessFileNames())
                {
                    return flag2;
                }
                CancelEventArgs e = new CancelEventArgs();
                if (NativeWindow.WndProcShouldBeDebuggable)
                {
                    this.OnFileOk(e);
                    return !e.Cancel;
                }
                try
                {
                    this.OnFileOk(e);
                    flag2 = !e.Cancel;
                }
                catch (Exception exception)
                {
                    Application.OnThreadException(exception);
                }
            }
            finally
            {
                if (!flag2)
                {
                    this.securityCheckFileNames = securityCheckFileNames;
                    Thread.MemoryBarrier();
                    this.fileNames = fileNames;
                    this.options = options;
                    this.filterIndex = filterIndex;
                }
                else if ((this.options & 4) != 0)
                {
                    this.options &= -2;
                }
            }
            return flag2;
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        protected override IntPtr HookProc(IntPtr hWnd, int msg, IntPtr wparam, IntPtr lparam)
        {
            if (msg == 0x4e)
            {
                this.dialogHWnd = System.Windows.Forms.UnsafeNativeMethods.GetParent(new HandleRef(null, hWnd));
                try
                {
                    System.Windows.Forms.UnsafeNativeMethods.OFNOTIFY structure = (System.Windows.Forms.UnsafeNativeMethods.OFNOTIFY) System.Windows.Forms.UnsafeNativeMethods.PtrToStructure(lparam, typeof(System.Windows.Forms.UnsafeNativeMethods.OFNOTIFY));
                    switch (structure.hdr_code)
                    {
                        case -606:
                            if (this.ignoreSecondFileOkNotification)
                            {
                                if (this.okNotificationCount != 0)
                                {
                                    break;
                                }
                                this.okNotificationCount = 1;
                            }
                            goto Label_0171;

                        case -604:
                            this.ignoreSecondFileOkNotification = true;
                            this.okNotificationCount = 0;
                            goto Label_01CF;

                        case -602:
                        {
                            System.Windows.Forms.NativeMethods.OPENFILENAME_I openfilename_i = (System.Windows.Forms.NativeMethods.OPENFILENAME_I) System.Windows.Forms.UnsafeNativeMethods.PtrToStructure(structure.lpOFN, typeof(System.Windows.Forms.NativeMethods.OPENFILENAME_I));
                            int num = (int) System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, this.dialogHWnd), 0x464, IntPtr.Zero, IntPtr.Zero);
                            if (num > openfilename_i.nMaxFile)
                            {
                                try
                                {
                                    int size = num + 0x800;
                                    System.Windows.Forms.UnsafeNativeMethods.CharBuffer buffer = System.Windows.Forms.UnsafeNativeMethods.CharBuffer.CreateBuffer(size);
                                    IntPtr ptr = buffer.AllocCoTaskMem();
                                    Marshal.FreeCoTaskMem(openfilename_i.lpstrFile);
                                    openfilename_i.lpstrFile = ptr;
                                    openfilename_i.nMaxFile = size;
                                    this.charBuffer = buffer;
                                    Marshal.StructureToPtr(openfilename_i, structure.lpOFN, true);
                                    Marshal.StructureToPtr(structure, lparam, true);
                                }
                                catch
                                {
                                }
                            }
                            this.ignoreSecondFileOkNotification = false;
                            goto Label_01CF;
                        }
                        case -601:
                            CommonDialog.MoveToScreenCenter(this.dialogHWnd);
                            goto Label_01CF;

                        default:
                            goto Label_01CF;
                    }
                    this.ignoreSecondFileOkNotification = false;
                    System.Windows.Forms.UnsafeNativeMethods.SetWindowLong(new HandleRef(null, hWnd), 0, new HandleRef(null, System.Windows.Forms.NativeMethods.InvalidIntPtr));
                    return System.Windows.Forms.NativeMethods.InvalidIntPtr;
                Label_0171:
                    if (!this.DoFileOk(structure.lpOFN))
                    {
                        System.Windows.Forms.UnsafeNativeMethods.SetWindowLong(new HandleRef(null, hWnd), 0, new HandleRef(null, System.Windows.Forms.NativeMethods.InvalidIntPtr));
                        return System.Windows.Forms.NativeMethods.InvalidIntPtr;
                    }
                }
                catch
                {
                    if (this.dialogHWnd != IntPtr.Zero)
                    {
                        System.Windows.Forms.UnsafeNativeMethods.EndDialog(new HandleRef(this, this.dialogHWnd), IntPtr.Zero);
                    }
                    throw;
                }
            }
        Label_01CF:
            return IntPtr.Zero;
        }

        private static string MakeFilterString(string s, bool dereferenceLinks)
        {
            if ((s == null) || (s.Length == 0))
            {
                if (dereferenceLinks && (Environment.OSVersion.Version.Major >= 5))
                {
                    s = " |*.*";
                }
                else if (s == null)
                {
                    return null;
                }
            }
            int length = s.Length;
            char[] destination = new char[length + 2];
            s.CopyTo(0, destination, 0, length);
            for (int i = 0; i < length; i++)
            {
                if (destination[i] == '|')
                {
                    destination[i] = '\0';
                }
            }
            destination[length + 1] = '\0';
            return new string(destination);
        }

        internal bool MessageBoxWithFocusRestore(string message, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            bool flag;
            IntPtr focus = System.Windows.Forms.UnsafeNativeMethods.GetFocus();
            try
            {
                flag = RTLAwareMessageBox.Show(null, message, caption, buttons, icon, MessageBoxDefaultButton.Button1, 0) == DialogResult.Yes;
            }
            finally
            {
                System.Windows.Forms.UnsafeNativeMethods.SetFocus(new HandleRef(null, focus));
            }
            return flag;
        }

        internal virtual void OnBeforeVistaDialog(FileDialogNative.IFileDialog dialog)
        {
            dialog.SetDefaultExtension(this.DefaultExt);
            dialog.SetFileName(this.FileName);
            if (!string.IsNullOrEmpty(this.InitialDirectory))
            {
                try
                {
                    FileDialogNative.IShellItem shellItemForPath = GetShellItemForPath(this.InitialDirectory);
                    dialog.SetDefaultFolder(shellItemForPath);
                    dialog.SetFolder(shellItemForPath);
                }
                catch (FileNotFoundException)
                {
                }
            }
            dialog.SetTitle(this.Title);
            dialog.SetOptions(this.GetOptions());
            this.SetFileTypes(dialog);
            this._customPlaces.Apply(dialog);
        }

        protected void OnFileOk(CancelEventArgs e)
        {
            CancelEventHandler handler = (CancelEventHandler) base.Events[EventFileOk];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private bool ProcessFileNames()
        {
            if ((this.options & 0x100) == 0)
            {
                string[] filterExtensions = this.FilterExtensions;
                for (int i = 0; i < this.fileNames.Length; i++)
                {
                    string path = this.fileNames[i];
                    if (((this.options & -2147483648) != 0) && !Path.HasExtension(path))
                    {
                        bool flag = (this.options & 0x1000) != 0;
                        for (int j = 0; j < filterExtensions.Length; j++)
                        {
                            string extension = Path.GetExtension(path);
                            string fileName = path.Substring(0, path.Length - extension.Length);
                            if (filterExtensions[j].IndexOfAny(new char[] { '*', '?' }) == -1)
                            {
                                fileName = fileName + "." + filterExtensions[j];
                            }
                            if (!flag || FileExists(fileName))
                            {
                                path = fileName;
                                break;
                            }
                        }
                        this.fileNames[i] = path;
                    }
                    if (!this.PromptUserIfAppropriate(path))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        internal abstract string[] ProcessVistaFiles(FileDialogNative.IFileDialog dialog);
        private void PromptFileNotFound(string fileName)
        {
            this.MessageBoxWithFocusRestore(System.Windows.Forms.SR.GetString("FileDialogFileNotFound", new object[] { fileName }), this.DialogCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        internal virtual bool PromptUserIfAppropriate(string fileName)
        {
            if (((this.options & 0x1000) != 0) && !FileExists(fileName))
            {
                this.PromptFileNotFound(fileName);
                return false;
            }
            return true;
        }

        public override void Reset()
        {
            this.options = -2147481596;
            this.title = null;
            this.initialDir = null;
            this.defaultExt = null;
            this.fileNames = null;
            this.filter = null;
            this.filterIndex = 1;
            this.supportMultiDottedExtensions = false;
            this._customPlaces.Clear();
        }

        protected override bool RunDialog(IntPtr hWndOwner)
        {
            if (Control.CheckForIllegalCrossThreadCalls && (Application.OleRequired() != ApartmentState.STA))
            {
                throw new ThreadStateException(System.Windows.Forms.SR.GetString("DebuggingExceptionOnly", new object[] { System.Windows.Forms.SR.GetString("ThreadMustBeSTA") }));
            }
            this.EnsureFileDialogPermission();
            if (this.UseVistaDialogInternal)
            {
                return this.RunDialogVista(hWndOwner);
            }
            return this.RunDialogOld(hWndOwner);
        }

        private bool RunDialogOld(IntPtr hWndOwner)
        {
            bool flag;
            System.Windows.Forms.NativeMethods.WndProc proc = new System.Windows.Forms.NativeMethods.WndProc(this.HookProc);
            System.Windows.Forms.NativeMethods.OPENFILENAME_I ofn = new System.Windows.Forms.NativeMethods.OPENFILENAME_I();
            try
            {
                this.charBuffer = System.Windows.Forms.UnsafeNativeMethods.CharBuffer.CreateBuffer(0x2000);
                if (this.fileNames != null)
                {
                    this.charBuffer.PutString(this.fileNames[0]);
                }
                ofn.lStructSize = Marshal.SizeOf(typeof(System.Windows.Forms.NativeMethods.OPENFILENAME_I));
                if ((Environment.OSVersion.Platform != PlatformID.Win32NT) || (Environment.OSVersion.Version.Major < 5))
                {
                    ofn.lStructSize = 0x4c;
                }
                ofn.hwndOwner = hWndOwner;
                ofn.hInstance = this.Instance;
                ofn.lpstrFilter = MakeFilterString(this.filter, this.DereferenceLinks);
                ofn.nFilterIndex = this.filterIndex;
                ofn.lpstrFile = this.charBuffer.AllocCoTaskMem();
                ofn.nMaxFile = 0x2000;
                ofn.lpstrInitialDir = this.initialDir;
                ofn.lpstrTitle = this.title;
                ofn.Flags = this.Options | 0x880020;
                ofn.lpfnHook = proc;
                ofn.FlagsEx = 0x1000000;
                if ((this.defaultExt != null) && this.AddExtension)
                {
                    ofn.lpstrDefExt = this.defaultExt;
                }
                flag = this.RunFileDialog(ofn);
            }
            finally
            {
                this.charBuffer = null;
                if (ofn.lpstrFile != IntPtr.Zero)
                {
                    Marshal.FreeCoTaskMem(ofn.lpstrFile);
                }
            }
            return flag;
        }

        [SecurityPermission(SecurityAction.Assert, Flags=SecurityPermissionFlag.UnmanagedCode)]
        private bool RunDialogVista(IntPtr hWndOwner)
        {
            uint num;
            bool flag;
            FileDialogNative.IFileDialog dialog = this.CreateVistaDialog();
            this.OnBeforeVistaDialog(dialog);
            VistaDialogEvents pfde = new VistaDialogEvents(this);
            dialog.Advise(pfde, out num);
            try
            {
                int num2 = dialog.Show(hWndOwner);
                flag = 0 == num2;
            }
            finally
            {
                dialog.Unadvise(num);
                GC.KeepAlive(pfde);
            }
            return flag;
        }

        internal abstract bool RunFileDialog(System.Windows.Forms.NativeMethods.OPENFILENAME_I ofn);
        private void SetFileTypes(FileDialogNative.IFileDialog dialog)
        {
            FileDialogNative.COMDLG_FILTERSPEC[] filterItems = this.FilterItems;
            dialog.SetFileTypes((uint) filterItems.Length, filterItems);
            if (filterItems.Length > 0)
            {
                dialog.SetFileTypeIndex((uint) this.filterIndex);
            }
        }

        internal void SetOption(int option, bool value)
        {
            if (value)
            {
                this.options |= option;
            }
            else
            {
                this.options &= ~option;
            }
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder(base.ToString() + ": Title: " + this.Title + ", FileName: ");
            try
            {
                builder.Append(this.FileName);
            }
            catch (Exception exception)
            {
                builder.Append("<");
                builder.Append(exception.GetType().FullName);
                builder.Append(">");
            }
            return builder.ToString();
        }

        [System.Windows.Forms.SRCategory("CatBehavior"), DefaultValue(true), System.Windows.Forms.SRDescription("FDaddExtensionDescr")]
        public bool AddExtension
        {
            get
            {
                return this.GetOption(-2147483648);
            }
            set
            {
                System.Windows.Forms.IntSecurity.FileDialogCustomization.Demand();
                this.SetOption(-2147483648, value);
            }
        }

        [DefaultValue(true)]
        public bool AutoUpgradeEnabled
        {
            get
            {
                return this._autoUpgradeEnabled;
            }
            set
            {
                this._autoUpgradeEnabled = value;
            }
        }

        [DefaultValue(false), System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("FDcheckFileExistsDescr")]
        public virtual bool CheckFileExists
        {
            get
            {
                return this.GetOption(0x1000);
            }
            set
            {
                System.Windows.Forms.IntSecurity.FileDialogCustomization.Demand();
                this.SetOption(0x1000, value);
            }
        }

        [System.Windows.Forms.SRCategory("CatBehavior"), DefaultValue(true), System.Windows.Forms.SRDescription("FDcheckPathExistsDescr")]
        public bool CheckPathExists
        {
            get
            {
                return this.GetOption(0x800);
            }
            set
            {
                System.Windows.Forms.IntSecurity.FileDialogCustomization.Demand();
                this.SetOption(0x800, value);
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public FileDialogCustomPlacesCollection CustomPlaces
        {
            get
            {
                return this._customPlaces;
            }
        }

        [System.Windows.Forms.SRCategory("CatBehavior"), DefaultValue(""), System.Windows.Forms.SRDescription("FDdefaultExtDescr")]
        public string DefaultExt
        {
            get
            {
                if (this.defaultExt != null)
                {
                    return this.defaultExt;
                }
                return "";
            }
            set
            {
                if (value != null)
                {
                    if (value.StartsWith("."))
                    {
                        value = value.Substring(1);
                    }
                    else if (value.Length == 0)
                    {
                        value = null;
                    }
                }
                this.defaultExt = value;
            }
        }

        [System.Windows.Forms.SRDescription("FDdereferenceLinksDescr"), DefaultValue(true), System.Windows.Forms.SRCategory("CatBehavior")]
        public bool DereferenceLinks
        {
            get
            {
                return !this.GetOption(0x100000);
            }
            set
            {
                System.Windows.Forms.IntSecurity.FileDialogCustomization.Demand();
                this.SetOption(0x100000, !value);
            }
        }

        internal string DialogCaption
        {
            get
            {
                StringBuilder lpString = new StringBuilder(SafeNativeMethods.GetWindowTextLength(new HandleRef(this, this.dialogHWnd)) + 1);
                System.Windows.Forms.UnsafeNativeMethods.GetWindowText(new HandleRef(this, this.dialogHWnd), lpString, lpString.Capacity);
                return lpString.ToString();
            }
        }

        [DefaultValue(""), System.Windows.Forms.SRDescription("FDfileNameDescr"), System.Windows.Forms.SRCategory("CatData")]
        public string FileName
        {
            get
            {
                if (this.fileNames == null)
                {
                    return "";
                }
                if (this.fileNames[0].Length <= 0)
                {
                    return "";
                }
                if (this.securityCheckFileNames)
                {
                    System.Windows.Forms.IntSecurity.DemandFileIO(FileIOPermissionAccess.AllAccess, this.fileNames[0]);
                }
                return this.fileNames[0];
            }
            set
            {
                System.Windows.Forms.IntSecurity.FileDialogCustomization.Demand();
                if (value == null)
                {
                    this.fileNames = null;
                }
                else
                {
                    this.fileNames = new string[] { value };
                }
                this.securityCheckFileNames = false;
            }
        }

        [System.Windows.Forms.SRDescription("FDFileNamesDescr"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public string[] FileNames
        {
            get
            {
                string[] fileNamesInternal = this.FileNamesInternal;
                if (this.securityCheckFileNames)
                {
                    foreach (string str in fileNamesInternal)
                    {
                        System.Windows.Forms.IntSecurity.DemandFileIO(FileIOPermissionAccess.AllAccess, str);
                    }
                }
                return fileNamesInternal;
            }
        }

        internal string[] FileNamesInternal
        {
            get
            {
                if (this.fileNames == null)
                {
                    return new string[0];
                }
                return (string[]) this.fileNames.Clone();
            }
        }

        [DefaultValue(""), Localizable(true), System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("FDfilterDescr")]
        public string Filter
        {
            get
            {
                if (this.filter != null)
                {
                    return this.filter;
                }
                return "";
            }
            set
            {
                if (value != this.filter)
                {
                    if ((value != null) && (value.Length > 0))
                    {
                        string[] strArray = value.Split(new char[] { '|' });
                        if ((strArray == null) || ((strArray.Length % 2) != 0))
                        {
                            throw new ArgumentException(System.Windows.Forms.SR.GetString("FileDialogInvalidFilter"));
                        }
                    }
                    else
                    {
                        value = null;
                    }
                    this.filter = value;
                }
            }
        }

        private string[] FilterExtensions
        {
            get
            {
                string filter = this.filter;
                ArrayList list = new ArrayList();
                if (this.defaultExt != null)
                {
                    list.Add(this.defaultExt);
                }
                if (filter != null)
                {
                    string[] strArray = filter.Split(new char[] { '|' });
                    if (((this.filterIndex * 2) - 1) >= strArray.Length)
                    {
                        throw new InvalidOperationException(System.Windows.Forms.SR.GetString("FileDialogInvalidFilterIndex"));
                    }
                    if (this.filterIndex > 0)
                    {
                        foreach (string str2 in strArray[(this.filterIndex * 2) - 1].Split(new char[] { ';' }))
                        {
                            int num = this.supportMultiDottedExtensions ? str2.IndexOf('.') : str2.LastIndexOf('.');
                            if (num >= 0)
                            {
                                list.Add(str2.Substring(num + 1, str2.Length - (num + 1)));
                            }
                        }
                    }
                }
                string[] array = new string[list.Count];
                list.CopyTo(array, 0);
                return array;
            }
        }

        [System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("FDfilterIndexDescr"), DefaultValue(1)]
        public int FilterIndex
        {
            get
            {
                return this.filterIndex;
            }
            set
            {
                this.filterIndex = value;
            }
        }

        private FileDialogNative.COMDLG_FILTERSPEC[] FilterItems
        {
            get
            {
                return GetFilterItems(this.filter);
            }
        }

        [System.Windows.Forms.SRDescription("FDinitialDirDescr"), System.Windows.Forms.SRCategory("CatData"), DefaultValue("")]
        public string InitialDirectory
        {
            get
            {
                if (this.initialDir != null)
                {
                    return this.initialDir;
                }
                return "";
            }
            set
            {
                System.Windows.Forms.IntSecurity.FileDialogCustomization.Demand();
                this.initialDir = value;
            }
        }

        protected virtual IntPtr Instance
        {
            [SecurityPermission(SecurityAction.InheritanceDemand, Flags=SecurityPermissionFlag.UnmanagedCode), SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
            get
            {
                return System.Windows.Forms.UnsafeNativeMethods.GetModuleHandle(null);
            }
        }

        protected int Options
        {
            get
            {
                return (this.options & 0x100b1d);
            }
        }

        [System.Windows.Forms.SRDescription("FDrestoreDirectoryDescr"), System.Windows.Forms.SRCategory("CatBehavior"), DefaultValue(false)]
        public bool RestoreDirectory
        {
            get
            {
                return this.GetOption(8);
            }
            set
            {
                System.Windows.Forms.IntSecurity.FileDialogCustomization.Demand();
                this.SetOption(8, value);
            }
        }

        internal virtual bool SettingsSupportVistaDialog
        {
            get
            {
                if (this.ShowHelp)
                {
                    return false;
                }
                if (Application.VisualStyleState != VisualStyleState.ClientAreaEnabled)
                {
                    return (Application.VisualStyleState == VisualStyleState.ClientAndNonClientAreasEnabled);
                }
                return true;
            }
        }

        [System.Windows.Forms.SRCategory("CatBehavior"), DefaultValue(false), System.Windows.Forms.SRDescription("FDshowHelpDescr")]
        public bool ShowHelp
        {
            get
            {
                return this.GetOption(0x10);
            }
            set
            {
                this.SetOption(0x10, value);
            }
        }

        [System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("FDsupportMultiDottedExtensionsDescr"), DefaultValue(false)]
        public bool SupportMultiDottedExtensions
        {
            get
            {
                return this.supportMultiDottedExtensions;
            }
            set
            {
                this.supportMultiDottedExtensions = value;
            }
        }

        [DefaultValue(""), System.Windows.Forms.SRCategory("CatAppearance"), Localizable(true), System.Windows.Forms.SRDescription("FDtitleDescr")]
        public string Title
        {
            get
            {
                if (this.title != null)
                {
                    return this.title;
                }
                return "";
            }
            set
            {
                System.Windows.Forms.IntSecurity.FileDialogCustomization.Demand();
                this.title = value;
            }
        }

        internal bool UseVistaDialogInternal
        {
            get
            {
                if ((System.Windows.Forms.UnsafeNativeMethods.IsVista && this._autoUpgradeEnabled) && this.SettingsSupportVistaDialog)
                {
                    new EnvironmentPermission(PermissionState.Unrestricted).Assert();
                    try
                    {
                        return (SystemInformation.BootMode == BootMode.Normal);
                    }
                    finally
                    {
                        CodeAccessPermission.RevertAssert();
                    }
                }
                return false;
            }
        }

        [System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("FDvalidateNamesDescr"), DefaultValue(true)]
        public bool ValidateNames
        {
            get
            {
                return !this.GetOption(0x100);
            }
            set
            {
                System.Windows.Forms.IntSecurity.FileDialogCustomization.Demand();
                this.SetOption(0x100, !value);
            }
        }

        private class VistaDialogEvents : FileDialogNative.IFileDialogEvents
        {
            private FileDialog _dialog;

            public VistaDialogEvents(FileDialog dialog)
            {
                this._dialog = dialog;
            }

            public int OnFileOk(FileDialogNative.IFileDialog pfd)
            {
                if (!this._dialog.HandleVistaFileOk(pfd))
                {
                    return 1;
                }
                return 0;
            }

            public void OnFolderChange(FileDialogNative.IFileDialog pfd)
            {
            }

            public int OnFolderChanging(FileDialogNative.IFileDialog pfd, FileDialogNative.IShellItem psiFolder)
            {
                return 0;
            }

            public void OnOverwrite(FileDialogNative.IFileDialog pfd, FileDialogNative.IShellItem psi, out FileDialogNative.FDE_OVERWRITE_RESPONSE pResponse)
            {
                pResponse = FileDialogNative.FDE_OVERWRITE_RESPONSE.FDEOR_DEFAULT;
            }

            public void OnSelectionChange(FileDialogNative.IFileDialog pfd)
            {
            }

            public void OnShareViolation(FileDialogNative.IFileDialog pfd, FileDialogNative.IShellItem psi, out FileDialogNative.FDE_SHAREVIOLATION_RESPONSE pResponse)
            {
                pResponse = FileDialogNative.FDE_SHAREVIOLATION_RESPONSE.FDESVR_DEFAULT;
            }

            public void OnTypeChange(FileDialogNative.IFileDialog pfd)
            {
            }
        }
    }
}

