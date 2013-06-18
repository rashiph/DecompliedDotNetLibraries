namespace System.Windows.Forms.Design
{
    using System;
    using System.ComponentModel;
    using System.Design;
    using System.Drawing.Design;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;

    public class FolderNameEditor : UITypeEditor
    {
        private FolderBrowser folderBrowser;

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            if (this.folderBrowser == null)
            {
                this.folderBrowser = new FolderBrowser();
                this.InitializeDialog(this.folderBrowser);
            }
            if (this.folderBrowser.ShowDialog() != DialogResult.OK)
            {
                return value;
            }
            return this.folderBrowser.DirectoryPath;
        }

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }

        protected virtual void InitializeDialog(FolderBrowser folderBrowser)
        {
        }

        protected sealed class FolderBrowser : Component
        {
            private string descriptionText = string.Empty;
            private string directoryPath = string.Empty;
            private static readonly int MAX_PATH = 260;
            private System.Design.UnsafeNativeMethods.BrowseInfos privateOptions = System.Design.UnsafeNativeMethods.BrowseInfos.NewDialogStyle;
            private FolderNameEditor.FolderBrowserStyles publicOptions = FolderNameEditor.FolderBrowserStyles.RestrictToFilesystem;
            private FolderNameEditor.FolderBrowserFolder startLocation;

            private static System.Design.UnsafeNativeMethods.IMalloc GetSHMalloc()
            {
                System.Design.UnsafeNativeMethods.IMalloc[] ppMalloc = new System.Design.UnsafeNativeMethods.IMalloc[1];
                System.Design.UnsafeNativeMethods.Shell32.SHGetMalloc(ppMalloc);
                return ppMalloc[0];
            }

            public DialogResult ShowDialog()
            {
                return this.ShowDialog(null);
            }

            public DialogResult ShowDialog(IWin32Window owner)
            {
                IntPtr handle;
                IntPtr zero = IntPtr.Zero;
                if (owner != null)
                {
                    handle = owner.Handle;
                }
                else
                {
                    handle = System.Design.UnsafeNativeMethods.GetActiveWindow();
                }
                System.Design.UnsafeNativeMethods.Shell32.SHGetSpecialFolderLocation(handle, (int) this.startLocation, ref zero);
                if (zero == IntPtr.Zero)
                {
                    return DialogResult.Cancel;
                }
                int num = (int) (this.publicOptions | ((FolderNameEditor.FolderBrowserStyles) ((int) this.privateOptions)));
                if ((num & 0x40) != 0)
                {
                    Application.OleRequired();
                }
                IntPtr pidl = IntPtr.Zero;
                try
                {
                    System.Design.UnsafeNativeMethods.BROWSEINFO lpbi = new System.Design.UnsafeNativeMethods.BROWSEINFO();
                    IntPtr pszPath = Marshal.AllocHGlobal(MAX_PATH);
                    lpbi.pidlRoot = zero;
                    lpbi.hwndOwner = handle;
                    lpbi.pszDisplayName = pszPath;
                    lpbi.lpszTitle = this.descriptionText;
                    lpbi.ulFlags = num;
                    lpbi.lpfn = IntPtr.Zero;
                    lpbi.lParam = IntPtr.Zero;
                    lpbi.iImage = 0;
                    pidl = System.Design.UnsafeNativeMethods.Shell32.SHBrowseForFolder(lpbi);
                    if (pidl == IntPtr.Zero)
                    {
                        return DialogResult.Cancel;
                    }
                    System.Design.UnsafeNativeMethods.Shell32.SHGetPathFromIDList(pidl, pszPath);
                    this.directoryPath = Marshal.PtrToStringAuto(pszPath);
                    Marshal.FreeHGlobal(pszPath);
                }
                finally
                {
                    System.Design.UnsafeNativeMethods.IMalloc sHMalloc = GetSHMalloc();
                    sHMalloc.Free(zero);
                    if (pidl != IntPtr.Zero)
                    {
                        sHMalloc.Free(pidl);
                    }
                }
                return DialogResult.OK;
            }

            public string Description
            {
                get
                {
                    return this.descriptionText;
                }
                set
                {
                    this.descriptionText = (value == null) ? string.Empty : value;
                }
            }

            public string DirectoryPath
            {
                get
                {
                    return this.directoryPath;
                }
            }

            public FolderNameEditor.FolderBrowserFolder StartLocation
            {
                get
                {
                    return this.startLocation;
                }
                set
                {
                    this.startLocation = value;
                }
            }

            public FolderNameEditor.FolderBrowserStyles Style
            {
                get
                {
                    return this.publicOptions;
                }
                set
                {
                    this.publicOptions = value;
                }
            }
        }

        protected enum FolderBrowserFolder
        {
            Desktop = 0,
            Favorites = 6,
            MyComputer = 0x11,
            MyDocuments = 5,
            MyPictures = 0x27,
            NetAndDialUpConnections = 0x31,
            NetworkNeighborhood = 0x12,
            Printers = 4,
            Recent = 8,
            SendTo = 9,
            StartMenu = 11,
            Templates = 0x15
        }

        [Flags]
        protected enum FolderBrowserStyles
        {
            BrowseForComputer = 0x1000,
            BrowseForEverything = 0x4000,
            BrowseForPrinter = 0x2000,
            RestrictToDomain = 2,
            RestrictToFilesystem = 1,
            RestrictToSubfolders = 8,
            ShowTextBox = 0x10
        }
    }
}

