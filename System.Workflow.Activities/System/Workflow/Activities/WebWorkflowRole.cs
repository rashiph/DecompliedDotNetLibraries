namespace System.Workflow.Activities
{
    using System;
    using System.Collections.Generic;
    using System.Configuration.Provider;
    using System.Runtime;
    using System.Web.Security;

    [Serializable]
    public class WebWorkflowRole : WorkflowRole
    {
        private string m_roleName;
        private string m_roleProvider;

        public WebWorkflowRole(string roleName)
        {
            if (roleName == null)
            {
                throw new ArgumentNullException("roleName");
            }
            this.m_roleName = roleName;
            this.m_roleProvider = null;
        }

        public WebWorkflowRole(string roleName, string provider)
        {
            if (roleName == null)
            {
                throw new ArgumentNullException("roleName");
            }
            this.m_roleName = roleName;
            this.m_roleProvider = provider;
        }

        public override IList<string> GetIdentities()
        {
            List<string> list = new List<string>();
            System.Web.Security.RoleProvider roleProvider = this.GetRoleProvider();
            list.AddRange(roleProvider.GetUsersInRole(this.Name));
            return list;
        }

        private System.Web.Security.RoleProvider GetRoleProvider()
        {
            if (this.RoleProvider == null)
            {
                return Roles.Provider;
            }
            System.Web.Security.RoleProvider provider = Roles.Providers[this.RoleProvider];
            if (provider == null)
            {
                throw new ProviderException(SR.GetString("Error_RoleProviderNotAvailableOrEnabled", new object[] { this.RoleProvider }));
            }
            return provider;
        }

        public override bool IncludesIdentity(string identity)
        {
            return this.GetRoleProvider().IsUserInRole(identity, this.Name);
        }

        public override string Name
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.m_roleName;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                this.m_roleName = value;
            }
        }

        public string RoleProvider
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.m_roleProvider;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.m_roleProvider = value;
            }
        }
    }
}

