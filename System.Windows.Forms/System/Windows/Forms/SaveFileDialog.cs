namespace System.Windows.Forms
{
    using System;
    using System.ComponentModel;
    using System.IO;
    using System.Security;
    using System.Security.Permissions;

    [System.Windows.Forms.SRDescription("DescriptionSaveFileDialog"), Designer("System.Windows.Forms.Design.SaveFileDialogDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
    public sealed class SaveFileDialog : FileDialog
    {
        internal override FileDialogNative.IFileDialog CreateVistaDialog()
        {
            return (FileDialogNative.NativeFileSaveDialog) new FileDialogNative.FileSaveDialogRCW();
        }

        internal override void EnsureFileDialogPermission()
        {
            System.Windows.Forms.IntSecurity.FileDialogSaveFile.Demand();
        }

        public Stream OpenFile()
        {
            System.Windows.Forms.IntSecurity.FileDialogSaveFile.Demand();
            string str = base.FileNamesInternal[0];
            if (string.IsNullOrEmpty(str))
            {
                throw new ArgumentNullException("FileName");
            }
            Stream stream = null;
            new FileIOPermission(FileIOPermissionAccess.AllAccess, System.Windows.Forms.IntSecurity.UnsafeGetFullPath(str)).Assert();
            try
            {
                stream = new FileStream(str, FileMode.Create, FileAccess.ReadWrite);
            }
            finally
            {
                CodeAccessPermission.RevertAssert();
            }
            return stream;
        }

        internal override string[] ProcessVistaFiles(FileDialogNative.IFileDialog dialog)
        {
            FileDialogNative.IShellItem item;
            FileDialogNative.IFileSaveDialog dialog1 = (FileDialogNative.IFileSaveDialog) dialog;
            dialog.GetResult(out item);
            return new string[] { FileDialog.GetFilePathFromShellItem(item) };
        }

        private bool PromptFileCreate(string fileName)
        {
            return base.MessageBoxWithFocusRestore(System.Windows.Forms.SR.GetString("FileDialogCreatePrompt", new object[] { fileName }), base.DialogCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
        }

        private bool PromptFileOverwrite(string fileName)
        {
            return base.MessageBoxWithFocusRestore(System.Windows.Forms.SR.GetString("FileDialogOverwritePrompt", new object[] { fileName }), base.DialogCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
        }

        internal override bool PromptUserIfAppropriate(string fileName)
        {
            if (!base.PromptUserIfAppropriate(fileName))
            {
                return false;
            }
            if ((((base.options & 2) != 0) && FileDialog.FileExists(fileName)) && (!base.UseVistaDialogInternal && !this.PromptFileOverwrite(fileName)))
            {
                return false;
            }
            if ((((base.options & 0x2000) != 0) && !FileDialog.FileExists(fileName)) && !this.PromptFileCreate(fileName))
            {
                return false;
            }
            return true;
        }

        public override void Reset()
        {
            base.Reset();
            base.SetOption(2, true);
        }

        internal override bool RunFileDialog(NativeMethods.OPENFILENAME_I ofn)
        {
            System.Windows.Forms.IntSecurity.FileDialogSaveFile.Demand();
            bool saveFileName = System.Windows.Forms.UnsafeNativeMethods.GetSaveFileName(ofn);
            if (!saveFileName && (SafeNativeMethods.CommDlgExtendedError() == 0x3002))
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("FileDialogInvalidFileName", new object[] { base.FileName }));
            }
            return saveFileName;
        }

        [System.Windows.Forms.SRCategory("CatBehavior"), DefaultValue(false), System.Windows.Forms.SRDescription("SaveFileDialogCreatePrompt")]
        public bool CreatePrompt
        {
            get
            {
                return base.GetOption(0x2000);
            }
            set
            {
                System.Windows.Forms.IntSecurity.FileDialogCustomization.Demand();
                base.SetOption(0x2000, value);
            }
        }

        [System.Windows.Forms.SRDescription("SaveFileDialogOverWritePrompt"), System.Windows.Forms.SRCategory("CatBehavior"), DefaultValue(true)]
        public bool OverwritePrompt
        {
            get
            {
                return base.GetOption(2);
            }
            set
            {
                System.Windows.Forms.IntSecurity.FileDialogCustomization.Demand();
                base.SetOption(2, value);
            }
        }
    }
}

