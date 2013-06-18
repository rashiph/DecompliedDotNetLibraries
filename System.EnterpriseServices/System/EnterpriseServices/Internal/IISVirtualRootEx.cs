namespace System.EnterpriseServices.Internal
{
    using System;
    using System.DirectoryServices;
    using System.EnterpriseServices;
    using System.Globalization;
    using System.IO;
    using System.Runtime.InteropServices;

    internal static class IISVirtualRootEx
    {
        private const uint MD_ACCESS_READ = 1;
        private const uint MD_ACCESS_SCRIPT = 0x200;
        private const uint MD_ACCESS_SSL = 8;
        private const uint MD_AUTH_ANONYMOUS = 1;
        private const uint MD_AUTH_NT = 4;
        private const uint MD_DIRBROW_LOADDEFAULT = 0x4000001e;
        private const uint MD_DIRBROW_NONE = 0;
        private const int POOLED = 2;

        internal static bool CheckIfExists(string rootWeb, string virtualDirectory)
        {
            DirectoryEntry entry = new DirectoryEntry(rootWeb + "/" + virtualDirectory);
            try
            {
                string name = entry.Name;
            }
            catch (Exception exception)
            {
                if ((exception is NullReferenceException) || (exception is SEHException))
                {
                    throw;
                }
                return false;
            }
            return true;
        }

        internal static void CreateOrModify(string rootWeb, string inPhysicalDirectory, string virtualDirectory, bool secureSockets, bool windowsAuth, bool anonymous, bool homePage)
        {
            string str = inPhysicalDirectory;
            while (str.EndsWith("/", StringComparison.Ordinal) || str.EndsWith(@"\", StringComparison.Ordinal))
            {
                str = str.Remove(str.Length - 1, 1);
            }
            bool flag = CheckIfExists(rootWeb, virtualDirectory);
            DirectoryEntry entry = new DirectoryEntry(rootWeb);
            DirectoryEntry entry2 = null;
            if (flag)
            {
                entry2 = entry.Children.Find(virtualDirectory, "IIsWebVirtualDir");
            }
            else
            {
                entry2 = entry.Children.Add(virtualDirectory, "IIsWebVirtualDir");
            }
            if (entry2 == null)
            {
                throw new ServicedComponentException(Resource.FormatString("Soap_VRootCreationFailed"));
            }
            entry2.CommitChanges();
            entry2.Properties["Path"][0] = str;
            if (secureSockets)
            {
                uint num = uint.Parse(entry2.Properties["AccessSSLFlags"][0].ToString(), CultureInfo.InvariantCulture) | 8;
                entry2.Properties["AccessSSLFlags"][0] = num;
            }
            uint num2 = uint.Parse(entry2.Properties["AuthFlags"][0].ToString(), CultureInfo.InvariantCulture);
            if (!flag && anonymous)
            {
                num2 |= 1;
            }
            if (windowsAuth)
            {
                num2 = 4;
            }
            entry2.Properties["AuthFlags"][0] = num2;
            entry2.Properties["EnableDefaultDoc"][0] = homePage;
            if ((secureSockets && windowsAuth) && !anonymous)
            {
                entry2.Properties["DirBrowseFlags"][0] = 0;
            }
            else if (!flag)
            {
                entry2.Properties["DirBrowseFlags"][0] = 0x4000001e;
            }
            entry2.Properties["AccessFlags"][0] = 0x201;
            entry2.Properties["AppFriendlyName"][0] = virtualDirectory;
            entry2.CommitChanges();
            object[] args = new object[] { 2 };
            entry2.Invoke("AppCreate2", args);
        }

        internal static void GetStatus(string RootWeb, string PhysicalPath, string VirtualDirectory, out bool bExists, out bool bSSL, out bool bWindowsAuth, out bool bAnonymous, out bool bHomePage, out bool bDiscoFile)
        {
            bSSL = false;
            bWindowsAuth = false;
            bAnonymous = false;
            bHomePage = false;
            bDiscoFile = false;
            bExists = CheckIfExists(RootWeb, VirtualDirectory);
            if (bExists)
            {
                DirectoryEntry entry = new DirectoryEntry(RootWeb);
                if (entry != null)
                {
                    DirectoryEntry entry2 = entry.Children.Find(VirtualDirectory, "IIsWebVirtualDir");
                    if (entry2 != null)
                    {
                        if ((uint.Parse(entry2.Properties["AccessSSLFlags"][0].ToString(), CultureInfo.InvariantCulture) & 8) > 0)
                        {
                            bSSL = true;
                        }
                        uint num2 = uint.Parse(entry2.Properties["AuthFlags"][0].ToString(), CultureInfo.InvariantCulture);
                        if ((num2 & 1) > 0)
                        {
                            bAnonymous = true;
                        }
                        if ((num2 & 4) > 0)
                        {
                            bWindowsAuth = true;
                        }
                        bHomePage = (bool) entry2.Properties["EnableDefaultDoc"][0];
                        if (File.Exists(PhysicalPath + @"\default.disco"))
                        {
                            bDiscoFile = true;
                        }
                    }
                }
            }
        }
    }
}

