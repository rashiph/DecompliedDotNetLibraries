namespace System.EnterpriseServices.Internal
{
    using System;
    using System.DirectoryServices;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    [Guid("d8013ef1-730b-45e2-ba24-874b7242c425")]
    public class IISVirtualRoot : IComSoapIISVRoot
    {
        internal bool CheckIfExists(string RootWeb, string VirtualDirectory)
        {
            DirectoryEntry entry = new DirectoryEntry(RootWeb + "/" + VirtualDirectory);
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

        public void Create(string RootWeb, string inPhysicalDirectory, string VirtualDirectory, out string Error)
        {
            Error = "";
            try
            {
                new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Demand();
                string str = inPhysicalDirectory;
                while (str.EndsWith("/", StringComparison.Ordinal) || str.EndsWith(@"\", StringComparison.Ordinal))
                {
                    str = str.Remove(str.Length - 1, 1);
                }
                if (!this.CheckIfExists(RootWeb, VirtualDirectory))
                {
                    DirectoryEntry entry = new DirectoryEntry(RootWeb);
                    DirectoryEntry entry2 = entry.Children.Add(VirtualDirectory, "IIsWebVirtualDir");
                    entry2.CommitChanges();
                    entry2.Properties["Path"][0] = str;
                    entry2.Properties["AuthFlags"][0] = 5;
                    entry2.Properties["EnableDefaultDoc"][0] = true;
                    entry2.Properties["DirBrowseFlags"][0] = 0x4000003e;
                    entry2.Properties["AccessFlags"][0] = 0x201;
                    entry2.CommitChanges();
                    object[] args = new object[] { 2 };
                    entry2.Invoke("AppCreate2", args);
                    Error = "";
                }
            }
            catch (Exception exception)
            {
                if ((exception is NullReferenceException) || (exception is SEHException))
                {
                    throw;
                }
                Error = exception.ToString();
                ComSoapPublishError.Report(exception.ToString());
            }
        }

        public void Delete(string RootWeb, string PhysicalDirectory, string VirtualDirectory, out string Error)
        {
            Error = "";
            try
            {
                new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Demand();
                if (this.CheckIfExists(RootWeb, VirtualDirectory))
                {
                    DirectoryEntry entry = new DirectoryEntry(RootWeb);
                    new DirectoryEntry(RootWeb + "/" + VirtualDirectory).Invoke("AppDelete", null);
                    object[] args = new object[] { "IIsWebVirtualDir", VirtualDirectory };
                    entry.Invoke("Delete", args);
                    Directory.Delete(PhysicalDirectory, true);
                }
            }
            catch (Exception exception)
            {
                if ((exception is NullReferenceException) || (exception is SEHException))
                {
                    throw;
                }
                Error = exception.ToString();
                ComSoapPublishError.Report(exception.ToString());
            }
        }
    }
}

