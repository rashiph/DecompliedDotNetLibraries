namespace System.Windows.Forms
{
    using System;
    using System.Diagnostics;
    using System.Drawing.Printing;
    using System.IO;
    using System.Security;
    using System.Security.Permissions;

    internal static class IntSecurity
    {
        private static CodeAccessPermission adjustCursorClip;
        private static CodeAccessPermission affectMachineState;
        private static CodeAccessPermission affectThreadBehavior;
        private static CodeAccessPermission allPrinting;
        private static PermissionSet allPrintingAndUnmanagedCode;
        private static CodeAccessPermission allWindows;
        private static CodeAccessPermission changeWindowRegionForTopLevel;
        private static CodeAccessPermission clipboardOwn;
        private static CodeAccessPermission clipboardRead;
        private static PermissionSet clipboardWrite;
        private static CodeAccessPermission controlFromHandleOrLocation;
        private static CodeAccessPermission createAnyWindow;
        private static CodeAccessPermission createGraphicsForControl;
        private static CodeAccessPermission defaultPrinting;
        private static CodeAccessPermission fileDialogCustomization;
        private static CodeAccessPermission fileDialogOpenFile;
        private static CodeAccessPermission fileDialogSaveFile;
        private static CodeAccessPermission getCapture;
        private static CodeAccessPermission getParent;
        private static CodeAccessPermission manipulateWndProcAndHandles;
        private static CodeAccessPermission modifyCursor;
        private static CodeAccessPermission modifyFocus;
        private static CodeAccessPermission objectFromWin32Handle;
        private static CodeAccessPermission safePrinting;
        private static CodeAccessPermission safeSubWindows;
        private static CodeAccessPermission safeTopLevelWindows;
        public static readonly TraceSwitch SecurityDemand = new TraceSwitch("SecurityDemand", "Trace when security demands occur.");
        private static CodeAccessPermission sendMessages;
        private static CodeAccessPermission sensitiveSystemInformation;
        private static CodeAccessPermission topLevelWindow;
        private static CodeAccessPermission transparentWindows;
        private static CodeAccessPermission unmanagedCode;
        private static CodeAccessPermission unrestrictedWindows;
        private static CodeAccessPermission windowAdornmentModification;

        internal static void DemandFileIO(FileIOPermissionAccess access, string fileName)
        {
            new FileIOPermission(access, UnsafeGetFullPath(fileName)).Demand();
        }

        internal static string UnsafeGetFullPath(string fileName)
        {
            string fullPath = fileName;
            new FileIOPermission(PermissionState.None) { AllFiles = FileIOPermissionAccess.PathDiscovery }.Assert();
            try
            {
                fullPath = Path.GetFullPath(fileName);
            }
            finally
            {
                CodeAccessPermission.RevertAssert();
            }
            return fullPath;
        }

        public static CodeAccessPermission AdjustCursorClip
        {
            get
            {
                if (adjustCursorClip == null)
                {
                    adjustCursorClip = AllWindows;
                }
                return adjustCursorClip;
            }
        }

        public static CodeAccessPermission AdjustCursorPosition
        {
            get
            {
                return AllWindows;
            }
        }

        public static CodeAccessPermission AffectMachineState
        {
            get
            {
                if (affectMachineState == null)
                {
                    affectMachineState = UnmanagedCode;
                }
                return affectMachineState;
            }
        }

        public static CodeAccessPermission AffectThreadBehavior
        {
            get
            {
                if (affectThreadBehavior == null)
                {
                    affectThreadBehavior = UnmanagedCode;
                }
                return affectThreadBehavior;
            }
        }

        public static CodeAccessPermission AllPrinting
        {
            get
            {
                if (allPrinting == null)
                {
                    allPrinting = new PrintingPermission(PrintingPermissionLevel.AllPrinting);
                }
                return allPrinting;
            }
        }

        public static PermissionSet AllPrintingAndUnmanagedCode
        {
            get
            {
                if (allPrintingAndUnmanagedCode == null)
                {
                    PermissionSet set = new PermissionSet(PermissionState.None);
                    set.SetPermission(UnmanagedCode);
                    set.SetPermission(AllPrinting);
                    allPrintingAndUnmanagedCode = set;
                }
                return allPrintingAndUnmanagedCode;
            }
        }

        public static CodeAccessPermission AllWindows
        {
            get
            {
                if (allWindows == null)
                {
                    allWindows = new UIPermission(UIPermissionWindow.AllWindows);
                }
                return allWindows;
            }
        }

        public static CodeAccessPermission ChangeWindowRegionForTopLevel
        {
            get
            {
                if (changeWindowRegionForTopLevel == null)
                {
                    changeWindowRegionForTopLevel = AllWindows;
                }
                return changeWindowRegionForTopLevel;
            }
        }

        public static CodeAccessPermission ClipboardOwn
        {
            get
            {
                if (clipboardOwn == null)
                {
                    clipboardOwn = new UIPermission(UIPermissionClipboard.OwnClipboard);
                }
                return clipboardOwn;
            }
        }

        public static CodeAccessPermission ClipboardRead
        {
            get
            {
                if (clipboardRead == null)
                {
                    clipboardRead = new UIPermission(UIPermissionClipboard.AllClipboard);
                }
                return clipboardRead;
            }
        }

        public static PermissionSet ClipboardWrite
        {
            get
            {
                if (clipboardWrite == null)
                {
                    clipboardWrite = new PermissionSet(PermissionState.None);
                    clipboardWrite.SetPermission(UnmanagedCode);
                    clipboardWrite.SetPermission(ClipboardOwn);
                }
                return clipboardWrite;
            }
        }

        public static CodeAccessPermission ControlFromHandleOrLocation
        {
            get
            {
                if (controlFromHandleOrLocation == null)
                {
                    controlFromHandleOrLocation = AllWindows;
                }
                return controlFromHandleOrLocation;
            }
        }

        public static CodeAccessPermission CreateAnyWindow
        {
            get
            {
                if (createAnyWindow == null)
                {
                    createAnyWindow = SafeSubWindows;
                }
                return createAnyWindow;
            }
        }

        public static CodeAccessPermission CreateGraphicsForControl
        {
            get
            {
                if (createGraphicsForControl == null)
                {
                    createGraphicsForControl = SafeSubWindows;
                }
                return createGraphicsForControl;
            }
        }

        public static CodeAccessPermission DefaultPrinting
        {
            get
            {
                if (defaultPrinting == null)
                {
                    defaultPrinting = new PrintingPermission(PrintingPermissionLevel.DefaultPrinting);
                }
                return defaultPrinting;
            }
        }

        public static CodeAccessPermission FileDialogCustomization
        {
            get
            {
                if (fileDialogCustomization == null)
                {
                    fileDialogCustomization = new FileIOPermission(PermissionState.Unrestricted);
                }
                return fileDialogCustomization;
            }
        }

        public static CodeAccessPermission FileDialogOpenFile
        {
            get
            {
                if (fileDialogOpenFile == null)
                {
                    fileDialogOpenFile = new FileDialogPermission(FileDialogPermissionAccess.Open);
                }
                return fileDialogOpenFile;
            }
        }

        public static CodeAccessPermission FileDialogSaveFile
        {
            get
            {
                if (fileDialogSaveFile == null)
                {
                    fileDialogSaveFile = new FileDialogPermission(FileDialogPermissionAccess.Save);
                }
                return fileDialogSaveFile;
            }
        }

        public static CodeAccessPermission GetCapture
        {
            get
            {
                if (getCapture == null)
                {
                    getCapture = AllWindows;
                }
                return getCapture;
            }
        }

        public static CodeAccessPermission GetParent
        {
            get
            {
                if (getParent == null)
                {
                    getParent = AllWindows;
                }
                return getParent;
            }
        }

        public static CodeAccessPermission ManipulateWndProcAndHandles
        {
            get
            {
                if (manipulateWndProcAndHandles == null)
                {
                    manipulateWndProcAndHandles = AllWindows;
                }
                return manipulateWndProcAndHandles;
            }
        }

        public static CodeAccessPermission ModifyCursor
        {
            get
            {
                if (modifyCursor == null)
                {
                    modifyCursor = SafeSubWindows;
                }
                return modifyCursor;
            }
        }

        public static CodeAccessPermission ModifyFocus
        {
            get
            {
                if (modifyFocus == null)
                {
                    modifyFocus = AllWindows;
                }
                return modifyFocus;
            }
        }

        public static CodeAccessPermission ObjectFromWin32Handle
        {
            get
            {
                if (objectFromWin32Handle == null)
                {
                    objectFromWin32Handle = UnmanagedCode;
                }
                return objectFromWin32Handle;
            }
        }

        public static CodeAccessPermission SafePrinting
        {
            get
            {
                if (safePrinting == null)
                {
                    safePrinting = new PrintingPermission(PrintingPermissionLevel.SafePrinting);
                }
                return safePrinting;
            }
        }

        public static CodeAccessPermission SafeSubWindows
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

        public static CodeAccessPermission SafeTopLevelWindows
        {
            get
            {
                if (safeTopLevelWindows == null)
                {
                    safeTopLevelWindows = new UIPermission(UIPermissionWindow.SafeTopLevelWindows);
                }
                return safeTopLevelWindows;
            }
        }

        public static CodeAccessPermission SendMessages
        {
            get
            {
                if (sendMessages == null)
                {
                    sendMessages = UnmanagedCode;
                }
                return sendMessages;
            }
        }

        public static CodeAccessPermission SensitiveSystemInformation
        {
            get
            {
                if (sensitiveSystemInformation == null)
                {
                    sensitiveSystemInformation = new EnvironmentPermission(PermissionState.Unrestricted);
                }
                return sensitiveSystemInformation;
            }
        }

        public static CodeAccessPermission TopLevelWindow
        {
            get
            {
                if (topLevelWindow == null)
                {
                    topLevelWindow = SafeTopLevelWindows;
                }
                return topLevelWindow;
            }
        }

        public static CodeAccessPermission TransparentWindows
        {
            get
            {
                if (transparentWindows == null)
                {
                    transparentWindows = AllWindows;
                }
                return transparentWindows;
            }
        }

        public static CodeAccessPermission UnmanagedCode
        {
            get
            {
                if (unmanagedCode == null)
                {
                    unmanagedCode = new SecurityPermission(SecurityPermissionFlag.UnmanagedCode);
                }
                return unmanagedCode;
            }
        }

        public static CodeAccessPermission UnrestrictedWindows
        {
            get
            {
                if (unrestrictedWindows == null)
                {
                    unrestrictedWindows = AllWindows;
                }
                return unrestrictedWindows;
            }
        }

        public static CodeAccessPermission WindowAdornmentModification
        {
            get
            {
                if (windowAdornmentModification == null)
                {
                    windowAdornmentModification = AllWindows;
                }
                return windowAdornmentModification;
            }
        }
    }
}

