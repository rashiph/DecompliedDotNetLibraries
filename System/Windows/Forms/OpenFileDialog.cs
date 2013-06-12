namespace System.Windows.Forms
{
    using System;
    using System.ComponentModel;
    using System.IO;
    using System.Security;
    using System.Security.Permissions;

    [System.Windows.Forms.SRDescription("DescriptionOpenFileDialog")]
    public sealed class OpenFileDialog : FileDialog
    {
        internal override FileDialogNative.IFileDialog CreateVistaDialog()
        {
            return (FileDialogNative.NativeFileOpenDialog) new FileDialogNative.FileOpenDialogRCW();
        }

        internal override void EnsureFileDialogPermission()
        {
            System.Windows.Forms.IntSecurity.FileDialogOpenFile.Demand();
        }

        public Stream OpenFile()
        {
            System.Windows.Forms.IntSecurity.FileDialogOpenFile.Demand();
            string fileName = base.FileNamesInternal[0];
            if ((fileName == null) || (fileName.Length == 0))
            {
                throw new ArgumentNullException("FileName");
            }
            Stream stream = null;
            new FileIOPermission(FileIOPermissionAccess.Read, System.Windows.Forms.IntSecurity.UnsafeGetFullPath(fileName)).Assert();
            try
            {
                stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            }
            finally
            {
                CodeAccessPermission.RevertAssert();
            }
            return stream;
        }

        internal override string[] ProcessVistaFiles(FileDialogNative.IFileDialog dialog)
        {
            FileDialogNative.IShellItem item2;
            FileDialogNative.IFileOpenDialog dialog2 = (FileDialogNative.IFileOpenDialog) dialog;
            if (this.Multiselect)
            {
                FileDialogNative.IShellItemArray array;
                uint num;
                dialog2.GetResults(out array);
                array.GetCount(out num);
                string[] strArray = new string[num];
                for (uint i = 0; i < num; i++)
                {
                    FileDialogNative.IShellItem item;
                    array.GetItemAt(i, out item);
                    strArray[i] = FileDialog.GetFilePathFromShellItem(item);
                }
                return strArray;
            }
            dialog2.GetResult(out item2);
            return new string[] { FileDialog.GetFilePathFromShellItem(item2) };
        }

        private static string RemoveSensitivePathInformation(string fullPath)
        {
            return Path.GetFileName(fullPath);
        }

        public override void Reset()
        {
            base.Reset();
            base.SetOption(0x1000, true);
        }

        internal override bool RunFileDialog(NativeMethods.OPENFILENAME_I ofn)
        {
            System.Windows.Forms.IntSecurity.FileDialogOpenFile.Demand();
            bool openFileName = System.Windows.Forms.UnsafeNativeMethods.GetOpenFileName(ofn);
            if (!openFileName)
            {
                switch (SafeNativeMethods.CommDlgExtendedError())
                {
                    case 0x3001:
                        throw new InvalidOperationException(System.Windows.Forms.SR.GetString("FileDialogSubLassFailure"));

                    case 0x3002:
                        throw new InvalidOperationException(System.Windows.Forms.SR.GetString("FileDialogInvalidFileName", new object[] { base.FileName }));

                    case 0x3003:
                        throw new InvalidOperationException(System.Windows.Forms.SR.GetString("FileDialogBufferTooSmall"));
                }
            }
            return openFileName;
        }

        [DefaultValue(true), System.Windows.Forms.SRDescription("OFDcheckFileExistsDescr")]
        public override bool CheckFileExists
        {
            get
            {
                return base.CheckFileExists;
            }
            set
            {
                base.CheckFileExists = value;
            }
        }

        [System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("OFDmultiSelectDescr"), DefaultValue(false)]
        public bool Multiselect
        {
            get
            {
                return base.GetOption(0x200);
            }
            set
            {
                base.SetOption(0x200, value);
            }
        }

        [System.Windows.Forms.SRCategory("CatBehavior"), DefaultValue(false), System.Windows.Forms.SRDescription("OFDreadOnlyCheckedDescr")]
        public bool ReadOnlyChecked
        {
            get
            {
                return base.GetOption(1);
            }
            set
            {
                base.SetOption(1, value);
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string SafeFileName
        {
            get
            {
                new FileIOPermission(PermissionState.Unrestricted).Assert();
                string fileName = base.FileName;
                CodeAccessPermission.RevertAssert();
                if (string.IsNullOrEmpty(fileName))
                {
                    return "";
                }
                return RemoveSensitivePathInformation(fileName);
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string[] SafeFileNames
        {
            get
            {
                new FileIOPermission(PermissionState.Unrestricted).Assert();
                string[] fileNames = base.FileNames;
                CodeAccessPermission.RevertAssert();
                if ((fileNames == null) || (fileNames.Length == 0))
                {
                    return new string[0];
                }
                string[] strArray2 = new string[fileNames.Length];
                for (int i = 0; i < strArray2.Length; i++)
                {
                    strArray2[i] = RemoveSensitivePathInformation(fileNames[i]);
                }
                return strArray2;
            }
        }

        internal override bool SettingsSupportVistaDialog
        {
            get
            {
                return (base.SettingsSupportVistaDialog && !this.ShowReadOnly);
            }
        }

        [DefaultValue(false), System.Windows.Forms.SRDescription("OFDshowReadOnlyDescr"), System.Windows.Forms.SRCategory("CatBehavior")]
        public bool ShowReadOnly
        {
            get
            {
                return !base.GetOption(4);
            }
            set
            {
                base.SetOption(4, !value);
            }
        }
    }
}

