namespace Microsoft.VisualBasic.MyServices
{
    using Microsoft.VisualBasic.FileIO;
    using System;
    using System.ComponentModel;
    using System.Security.Permissions;

    [EditorBrowsable(EditorBrowsableState.Never), HostProtection(SecurityAction.LinkDemand, Resources=HostProtectionResource.ExternalProcessMgmt)]
    public class SpecialDirectoriesProxy
    {
        internal SpecialDirectoriesProxy()
        {
        }

        public string AllUsersApplicationData
        {
            get
            {
                return SpecialDirectories.AllUsersApplicationData;
            }
        }

        public string CurrentUserApplicationData
        {
            get
            {
                return SpecialDirectories.CurrentUserApplicationData;
            }
        }

        public string Desktop
        {
            get
            {
                return SpecialDirectories.Desktop;
            }
        }

        public string MyDocuments
        {
            get
            {
                return SpecialDirectories.MyDocuments;
            }
        }

        public string MyMusic
        {
            get
            {
                return SpecialDirectories.MyMusic;
            }
        }

        public string MyPictures
        {
            get
            {
                return SpecialDirectories.MyPictures;
            }
        }

        public string ProgramFiles
        {
            get
            {
                return SpecialDirectories.ProgramFiles;
            }
        }

        public string Programs
        {
            get
            {
                return SpecialDirectories.Programs;
            }
        }

        public string Temp
        {
            get
            {
                return SpecialDirectories.Temp;
            }
        }
    }
}

