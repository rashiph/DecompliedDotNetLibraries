namespace Microsoft.VisualBasic.ApplicationServices
{
    using System;
    using System.ComponentModel;
    using System.Runtime;
    using System.Security.Permissions;
    using System.Security.Principal;
    using System.Threading;

    [HostProtection(SecurityAction.LinkDemand, Resources=HostProtectionResource.ExternalProcessMgmt)]
    public class User
    {
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public void InitializeWithWindowsUser()
        {
            Thread.CurrentPrincipal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
        }

        public bool IsInRole(BuiltInRole role)
        {
            ValidateBuiltInRoleEnumValue(role, "role");
            TypeConverter converter = TypeDescriptor.GetConverter(typeof(BuiltInRole));
            if (this.IsWindowsPrincipal())
            {
                WindowsBuiltInRole role2 = (WindowsBuiltInRole) converter.ConvertTo(role, typeof(WindowsBuiltInRole));
                return ((WindowsPrincipal) this.InternalPrincipal).IsInRole(role2);
            }
            return this.InternalPrincipal.IsInRole(converter.ConvertToString(role));
        }

        public bool IsInRole(string role)
        {
            return this.InternalPrincipal.IsInRole(role);
        }

        private bool IsWindowsPrincipal()
        {
            return (this.InternalPrincipal is WindowsPrincipal);
        }

        internal static void ValidateBuiltInRoleEnumValue(BuiltInRole testMe, string parameterName)
        {
            if (((((((((testMe != BuiltInRole.AccountOperator) && (testMe != BuiltInRole.Administrator)) && (testMe != BuiltInRole.BackupOperator)) && (testMe != BuiltInRole.Guest)) && (testMe != BuiltInRole.PowerUser)) && (testMe != BuiltInRole.PrintOperator)) && (testMe != BuiltInRole.Replicator)) && (testMe != BuiltInRole.SystemOperator)) && (testMe != BuiltInRole.User))
            {
                throw new InvalidEnumArgumentException(parameterName, (int) testMe, typeof(BuiltInRole));
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public IPrincipal CurrentPrincipal
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.InternalPrincipal;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.InternalPrincipal = value;
            }
        }

        protected virtual IPrincipal InternalPrincipal
        {
            get
            {
                return Thread.CurrentPrincipal;
            }
            set
            {
                Thread.CurrentPrincipal = value;
            }
        }

        public bool IsAuthenticated
        {
            get
            {
                return this.InternalPrincipal.Identity.IsAuthenticated;
            }
        }

        public string Name
        {
            get
            {
                return this.InternalPrincipal.Identity.Name;
            }
        }
    }
}

