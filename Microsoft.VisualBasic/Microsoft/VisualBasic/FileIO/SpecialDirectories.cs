namespace Microsoft.VisualBasic.FileIO
{
    using Microsoft.VisualBasic.CompilerServices;
    using System;
    using System.IO;
    using System.Security.Permissions;
    using System.Windows.Forms;

    [HostProtection(SecurityAction.LinkDemand, Resources=HostProtectionResource.ExternalProcessMgmt)]
    public class SpecialDirectories
    {
        private static string GetDirectoryPath(string Directory, string DirectoryNameResID)
        {
            if (Directory == "")
            {
                throw ExceptionUtils.GetDirectoryNotFoundException("IO_SpecialDirectoryNotExist", new string[] { Utils.GetResourceString(DirectoryNameResID) });
            }
            return FileSystem.NormalizePath(Directory);
        }

        public static string AllUsersApplicationData
        {
            get
            {
                return GetDirectoryPath(Application.CommonAppDataPath, "IO_SpecialDirectory_AllUserAppData");
            }
        }

        public static string CurrentUserApplicationData
        {
            get
            {
                return GetDirectoryPath(Application.UserAppDataPath, "IO_SpecialDirectory_UserAppData");
            }
        }

        public static string Desktop
        {
            get
            {
                return GetDirectoryPath(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "IO_SpecialDirectory_Desktop");
            }
        }

        public static string MyDocuments
        {
            get
            {
                return GetDirectoryPath(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "IO_SpecialDirectory_MyDocuments");
            }
        }

        public static string MyMusic
        {
            get
            {
                return GetDirectoryPath(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic), "IO_SpecialDirectory_MyMusic");
            }
        }

        public static string MyPictures
        {
            get
            {
                return GetDirectoryPath(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "IO_SpecialDirectory_MyPictures");
            }
        }

        public static string ProgramFiles
        {
            get
            {
                return GetDirectoryPath(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "IO_SpecialDirectory_ProgramFiles");
            }
        }

        public static string Programs
        {
            get
            {
                return GetDirectoryPath(Environment.GetFolderPath(Environment.SpecialFolder.Programs), "IO_SpecialDirectory_Programs");
            }
        }

        public static string Temp
        {
            get
            {
                return GetDirectoryPath(Path.GetTempPath(), "IO_SpecialDirectory_Temp");
            }
        }
    }
}

