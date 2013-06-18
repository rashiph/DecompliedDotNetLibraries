namespace System.Drawing
{
    using System;
    using System.Drawing.Printing;
    using System.IO;
    using System.Security;
    using System.Security.Permissions;

    internal static class IntSecurity
    {
        public static readonly PrintingPermission AllPrinting = new PrintingPermission(PrintingPermissionLevel.AllPrinting);
        private static PermissionSet allPrintingAndUnmanagedCode;
        private static readonly UIPermission AllWindows = new UIPermission(UIPermissionWindow.AllWindows);
        public static readonly PrintingPermission DefaultPrinting = new PrintingPermission(PrintingPermissionLevel.DefaultPrinting);
        public static readonly PrintingPermission NoPrinting = new PrintingPermission(PrintingPermissionLevel.NoPrinting);
        public static readonly CodeAccessPermission ObjectFromWin32Handle = UnmanagedCode;
        public static readonly PrintingPermission SafePrinting = new PrintingPermission(PrintingPermissionLevel.SafePrinting);
        private static readonly UIPermission SafeSubWindows = new UIPermission(UIPermissionWindow.SafeSubWindows);
        public static readonly CodeAccessPermission UnmanagedCode = new SecurityPermission(SecurityPermissionFlag.UnmanagedCode);
        public static readonly CodeAccessPermission Win32HandleManipulation = UnmanagedCode;

        internal static void DemandReadFileIO(string fileName)
        {
            string path = fileName;
            path = UnsafeGetFullPath(fileName);
            new FileIOPermission(FileIOPermissionAccess.Read, path).Demand();
        }

        internal static void DemandWriteFileIO(string fileName)
        {
            string path = fileName;
            path = UnsafeGetFullPath(fileName);
            new FileIOPermission(FileIOPermissionAccess.Write, path).Demand();
        }

        internal static bool HasPermission(PrintingPermission permission)
        {
            try
            {
                permission.Demand();
                return true;
            }
            catch (SecurityException)
            {
                return false;
            }
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
    }
}

