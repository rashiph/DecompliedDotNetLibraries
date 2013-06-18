namespace System.Workflow.Activities
{
    using System;
    using System.DirectoryServices;
    using System.Runtime;
    using System.Security.Principal;
    using System.Workflow.Activities.Configuration;

    public static class ActiveDirectoryRoleFactory
    {
        private static ActiveDirectoryRoleFactoryConfiguration s_configuration = ((ActiveDirectoryRoleFactoryConfiguration) ConfigurationManager.GetSection(s_configurationSectionName));
        private static string s_configurationSectionName = "System.Workflow.Runtime.Hosting.ADRoleFactory";
        private static DirectoryGroupQuery s_directoryGroupQuery = new DirectoryGroupQuery();
        private static DirectoryEntry s_rootEntry;

        static ActiveDirectoryRoleFactory()
        {
            if (s_configuration == null)
            {
                s_configuration = new ActiveDirectoryRoleFactoryConfiguration();
            }
        }

        public static ActiveDirectoryRole CreateFromAlias(string alias)
        {
            if (alias == null)
            {
                throw new ArgumentNullException("alias");
            }
            ActiveDirectoryRole adRole = new ActiveDirectoryRole(GetRootEntry(), new DirectoryRootQuery("sAMAccountName", alias, DirectoryQueryOperation.Equal));
            adRole.Operations.Add(s_directoryGroupQuery);
            ValidateRole(adRole);
            return adRole;
        }

        public static ActiveDirectoryRole CreateFromEmailAddress(string emailAddress)
        {
            if (emailAddress == null)
            {
                throw new ArgumentNullException("emailAddress");
            }
            ActiveDirectoryRole adRole = new ActiveDirectoryRole(GetRootEntry(), new DirectoryRootQuery("mail", emailAddress, DirectoryQueryOperation.Equal));
            adRole.Operations.Add(s_directoryGroupQuery);
            ValidateRole(adRole);
            return adRole;
        }

        public static ActiveDirectoryRole CreateFromSecurityIdentifier(SecurityIdentifier sid)
        {
            if (sid == null)
            {
                throw new ArgumentNullException("sid");
            }
            ActiveDirectoryRole adRole = new ActiveDirectoryRole(GetRootEntry(), new DirectoryRootQuery("objectSID", sid.ToString(), DirectoryQueryOperation.Equal));
            adRole.Operations.Add(s_directoryGroupQuery);
            ValidateRole(adRole);
            return adRole;
        }

        private static DirectoryEntry GetRootEntry()
        {
            if (s_rootEntry == null)
            {
                if (((s_configuration == null) || (s_configuration.RootPath == null)) || (s_configuration.RootPath.Length == 0))
                {
                    s_rootEntry = new DirectoryEntry();
                }
                else
                {
                    s_rootEntry = new DirectoryEntry(s_configuration.RootPath);
                }
            }
            return s_rootEntry;
        }

        private static void ValidateRole(ActiveDirectoryRole adRole)
        {
            if (adRole.GetEntries().Count == 0)
            {
                throw new ArgumentException(SR.GetString("Error_NoMatchingActiveDirectoryEntry"));
            }
        }

        public static ActiveDirectoryRoleFactoryConfiguration Configuration
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return s_configuration;
            }
        }
    }
}

