namespace System.DirectoryServices
{
    using System;
    using System.Net;
    using System.Security.Permissions;

    [DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
    public class SearchResult
    {
        private AuthenticationTypes parentAuthenticationType;
        private NetworkCredential parentCredentials;
        private ResultPropertyCollection properties = new ResultPropertyCollection();

        internal SearchResult(NetworkCredential parentCredentials, AuthenticationTypes parentAuthenticationType)
        {
            this.parentCredentials = parentCredentials;
            this.parentAuthenticationType = parentAuthenticationType;
        }

        [SecurityPermission(SecurityAction.Assert, Flags=SecurityPermissionFlag.UnmanagedCode), EnvironmentPermission(SecurityAction.Assert, Unrestricted=true)]
        public DirectoryEntry GetDirectoryEntry()
        {
            if (this.parentCredentials != null)
            {
                return new DirectoryEntry(this.Path, true, this.parentCredentials.UserName, this.parentCredentials.Password, this.parentAuthenticationType);
            }
            return new DirectoryEntry(this.Path, true, null, null, this.parentAuthenticationType);
        }

        public string Path
        {
            get
            {
                return (string) this.Properties["ADsPath"][0];
            }
        }

        public ResultPropertyCollection Properties
        {
            get
            {
                return this.properties;
            }
        }
    }
}

