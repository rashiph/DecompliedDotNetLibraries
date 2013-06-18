namespace System.Workflow.Activities.Configuration
{
    using System;
    using System.Configuration;

    public sealed class ActiveDirectoryRoleFactoryConfiguration : ConfigurationSection
    {
        private const string _DirectReports = "DirectReports";
        private const string _DistinguishedName = "DistiguishedName";
        private const string _Group = "Group";
        private const string _Manager = "Manager";
        private const string _Member = "Member";
        private const string _RootPath = "RootPath";

        [ConfigurationProperty("DirectReports", DefaultValue="directReports")]
        public string DirectReports
        {
            get
            {
                return (string) base["DirectReports"];
            }
            set
            {
                base["DirectReports"] = value;
            }
        }

        [ConfigurationProperty("DistiguishedName", DefaultValue="distinguishedName")]
        public string DistinguishedName
        {
            get
            {
                return (string) base["DistiguishedName"];
            }
            set
            {
                base["DistiguishedName"] = value;
            }
        }

        [ConfigurationProperty("Group", DefaultValue="group")]
        public string Group
        {
            get
            {
                return (string) base["Group"];
            }
            set
            {
                base["Group"] = value;
            }
        }

        [ConfigurationProperty("Manager", DefaultValue="manager")]
        public string Manager
        {
            get
            {
                return (string) base["Manager"];
            }
            set
            {
                base["Manager"] = value;
            }
        }

        [ConfigurationProperty("Member", DefaultValue="member")]
        public string Member
        {
            get
            {
                return (string) base["Member"];
            }
            set
            {
                base["Member"] = value;
            }
        }

        [ConfigurationProperty("RootPath", DefaultValue="")]
        public string RootPath
        {
            get
            {
                return (string) base["RootPath"];
            }
            set
            {
                base["RootPath"] = value;
            }
        }
    }
}

